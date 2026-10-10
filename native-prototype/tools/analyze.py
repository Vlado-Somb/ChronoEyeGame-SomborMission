#!/usr/bin/env python3
"""Offline private diagnostic ZIP analyzer. No network, no extraction, stdlib only."""
import argparse, csv, html, io, json, math, statistics, struct, zlib, zipfile
from pathlib import Path

MAX_MEMBER = 70 * 1024 * 1024
COLORS = ['#50b9ff', '#ffb74d', '#89d185', '#e879b9']

def read(z, name):
    info = z.getinfo(name)
    if info.file_size > MAX_MEMBER:
        raise ValueError('Member exceeds diagnostic size limit')
    return z.read(name)

def heat_color(mm):
    if not mm: return (166, 0, 166)
    t = min(mm / 5000, 1)
    return tuple(round(255 * max(0, min(1, 1.5 - abs(4*t - k)))) for k in (3,2,1))

def png(width, height, rgb):
    def chunk(kind, data):
        return struct.pack('>I', len(data)) + kind + data + struct.pack('>I', zlib.crc32(kind+data)&0xffffffff)
    raw = b''.join(b'\0'+rgb[y*width*3:(y+1)*width*3] for y in range(height))
    return b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('>IIBBBBB',width,height,8,2,0,0,0))+chunk(b'IDAT',zlib.compress(raw))+chunk(b'IEND',b'')

def heatmap(z, metadata, target):
    w,h=metadata['width'],metadata['height']
    if not isinstance(w,int) or not isinstance(h,int) or w<=0 or h<=0 or w*h>4_000_000:
        raise ValueError('Invalid depth dimensions')
    raw=read(z,metadata['file'])
    if len(raw)!=w*h*2 or metadata['format']!='uint16_le' or metadata['unit']!='mm':
        raise ValueError('Unsupported depth encoding or inconsistent size')
    values=[v[0] for v in struct.iter_unpack('<H',raw)]
    rgb=bytearray()
    for i,v in enumerate(values):
        rgb.extend(heat_color(v) if v else ((166,0,166) if (i%w//6+i//w//6)%2 else (18,18,18)))
    target.write_bytes(png(w,h,rgb))
    # Apply exported NDC->texture UV affine transform to reconstruct screen orientation.
    uv=metadata.get('textureUvQuad')
    if uv and len(uv)==8:
        dw=360; dh=max(1,min(1000,round(dw*metadata['viewportHeight']/metadata['viewportWidth'])))
        screen=bytearray()
        for y in range(dh):
            ty=1-(y+.5)/dh
            for x in range(dw):
                tx=(x+.5)/dw
                u=uv[0]+tx*(uv[2]-uv[0])+ty*(uv[4]-uv[0])
                v=uv[1]+tx*(uv[3]-uv[1])+ty*(uv[5]-uv[1])
                if 0<=u<1 and 0<=v<1:
                    ix,iy=int(u*w),int(v*h); mm=values[iy*w+ix]
                    color=heat_color(mm) if mm else ((166,0,166) if (x//12+y//12)%2 else (18,18,18))
                else: color=(18,18,18)
                screen.extend(color)
        target.with_name(target.stem+'-screen.png').write_bytes(png(dw,dh,screen))
    return {'validFraction':sum(v>0 for v in values)/len(values),'minMm':min((v for v in values if v),default=None),'maxMm':max(values)}

def plot(title, series, marks):
    values=[(x,y) for _,points in series for x,y in points if y is not None and math.isfinite(y)]
    if not values: return '<p>'+html.escape(title)+': no data</p>'
    xmax=max(max(x for x,y in values),1); ymax=max(max(y for x,y in values),1)
    content=f'<h2>{html.escape(title)}</h2><svg viewBox="0 0 920 230" role="img" aria-label="{html.escape(title)}"><path d="M60 10V195H905" fill="none" stroke="#667"/>'
    for fraction in (0,.5,1):
        y=195-fraction*175
        content+=f'<text x="4" y="{y}" fill="#abb">{ymax*fraction:.1f}</text><path d="M60 {y}H905" stroke="#263246"/>'
    for t in marks:
        x=60+t/xmax*840
        content+=f'<path d="M{x:.1f} 12V195" stroke="#ff5070" stroke-dasharray="3 4"/>'
    for i,(name,points) in enumerate(series):
        pts=' '.join(f'{60+x/xmax*840:.1f},{195-y/ymax*175:.1f}' for x,y in points if y is not None and math.isfinite(y))
        content+=f'<polyline points="{pts}" fill="none" stroke="{COLORS[i%4]}" stroke-width="2"/><text x="{65+i*210}" y="222" fill="{COLORS[i%4]}">{html.escape(name)}</text>'
    return content+f'<text x="790" y="208" fill="#abb">{xmax:.0f} elapsed s</text></svg>'

def analyze(source, output, selected=None):
    output.mkdir(parents=True,exist_ok=True)
    with zipfile.ZipFile(source) as z:
        if sum(i.file_size for i in z.infolist())>100*1024*1024: raise ValueError('Archive exceeds size limit')
        manifest=json.loads(read(z,'manifest.json'))
        if manifest.get('schemaVersion')!=1: raise ValueError('Unsupported schemaVersion')
        events=[]; malformed=0
        for line in read(z,'events.jsonl').decode().splitlines():
            try: events.append(json.loads(line))
            except json.JSONDecodeError: malformed+=1
        rows=list(csv.DictReader(io.StringIO(read(z,'series.csv').decode())))
        marks=[e['elapsedMs']/1000 for e in events if e.get('type')=='problem_mark']
        samples=[e for e in events if e.get('type')=='sample']
        system=[e for e in events if e.get('type')=='system']
        def points(records,key):
            return [(e['elapsedMs']/1000,e['data'].get(key)) for e in records]
        full=[e['data']['fps'] for e in samples if e['data'].get('detailed')]
        basic=[e['data']['fps'] for e in samples if not e['data'].get('detailed')]
        summary={'sessionId':manifest['sessionId'],'synthetic':manifest.get('synthetic',False),
          'samples':len(samples),'csvRows':len(rows),'problemMarks':len(marks),'malformedLines':malformed,
          'meanFpsDetailed':statistics.mean(full) if full else None,'meanFpsBasic':statistics.mean(basic) if basic else None,
          'comparisonCaveat':'Descriptive only: compare matched scenes and lighting; no causal logging-overhead conclusion from unmatched intervals.'}
        header='SYNTHETIC TEST — NOT PHONE DATA' if summary['synthetic'] else 'ChronoEye private session'
        page=f'<!doctype html><meta charset="utf-8"><title>AR diagnostics</title><style>body{{background:#111c2c;color:#e7eef7;font:16px system-ui;max-width:1050px;margin:40px auto;padding:20px}}svg{{width:100%;background:#18263a}}h2{{font-size:19px}}img{{max-width:45%;image-rendering:pixelated}}pre{{white-space:pre-wrap}}a{{color:#64c7ff}}</style><h1>{header}</h1><p>{html.escape(manifest["sessionId"])}</p><p>Red dashed lines: problem marks. All x axes: monotonic elapsed seconds. Missing values are omitted; lines connect available samples.</p>'
        for title, series in [('FPS',[('FPS',points(samples,'fps'))]),('Frame time (ms)',[('mean',points(samples,'frameMeanMs')),('maximum',points(samples,'frameMaxMs'))]),('Depth age (ms)',[('age',points(samples,'depthAgeMs'))]),('Process memory (KiB)',[('PSS',points(system,'pssKiB')),('Java heap',points(system,'javaHeapKiB'))]),('Process CPU (% of one core)',[('100% = one full core',points(system,'processCpuOneCorePercent'))])]:
            page+=plot(title,series,marks)
        files=[n for n in z.namelist() if n.startswith('depth-') and n.endswith('.json')]
        if selected:
            if selected not in files: raise ValueError('Requested snapshot not found')
            files=[selected]
        else: files=files[:12]
        for i,name in enumerate(files):
            metadata=json.loads(read(z,name));target=output/f'heatmap-{i:03d}.png'; stats=heatmap(z,metadata,target)
            page+=f'<h2>Depth @ {metadata.get("captureElapsedMs",metadata["elapsedMs"])/1000:.3f}s · {html.escape(metadata.get("reason",""))}</h2><p>Fresh: {metadata.get("fresh")} · Blue 0 m / green 2.5 m / red ≥5 m. Checkerboard: no data. Left: native depth; right: reconstructed screen.</p><img src="{target.name}">'
            if target.with_name(target.stem+'-screen.png').exists(): page+=f'<img src="{target.stem}-screen.png">'
            page+='<pre>'+html.escape(json.dumps(stats,indent=2))+'</pre>'
        page+='<h2>Summary</h2><pre>'+html.escape(json.dumps(summary,indent=2))+'</pre><p>Stable pair distances do not exclude common drift. CPU temperature and GPU load are unavailable. No data are uploaded.</p>'
        (output/'report.html').write_text(page,encoding='utf-8');(output/'summary.json').write_text(json.dumps(summary,indent=2),encoding='utf-8')
        return summary

if __name__=='__main__':
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('zip',type=Path);p.add_argument('--out',type=Path,default=Path('analysis'));p.add_argument('--snapshot',help='Exact depth-*.json member; default first 12')
    a=p.parse_args(); print(json.dumps(analyze(a.zip,a.out,a.snapshot),indent=2));print(a.out/'report.html')
