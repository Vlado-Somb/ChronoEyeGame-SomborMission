"""Synthetic fixtures only; no claims about phone performance."""
import json, struct, tempfile, unittest, zipfile
from pathlib import Path
from analyze import analyze, heatmap

class SyntheticAnalysisTest(unittest.TestCase):
    def fixture(self,path,bad=False):
        with zipfile.ZipFile(path,'w') as z:
            z.writestr('manifest.json',json.dumps({'schemaVersion':1,'sessionId':'SYNTHETIC','synthetic':True}))
            z.writestr('series.csv','elapsedMs,fps\n1000,30\n2000,29\n')
            events=[{'type':'sample','elapsedMs':1000,'data':{'fps':30,'frameMeanMs':33.3,'frameMaxMs':40,'depthAgeMs':20,'detailed':True}}, {'type':'sample','elapsedMs':2000,'data':{'fps':29,'frameMeanMs':34.5,'frameMaxMs':55,'depthAgeMs':101,'detailed':False}}, {'type':'problem_mark','elapsedMs':1500,'data':{}}]
            z.writestr('events.jsonl','\n'.join(json.dumps(e) for e in events))
            z.writestr('depth-0001.json',json.dumps({'width':2,'height':2,'format':'uint16_le','unit':'mm','file':'depth-0001.u16','elapsedMs':1500,'fresh':True,'textureUvQuad':[0,1,1,1,0,0,1,0],'viewportWidth':2,'viewportHeight':2}))
            z.writestr('depth-0001.u16',struct.pack('<HHHH',0,1000,2500,5000)[:-1] if bad else struct.pack('<HHHH',0,1000,2500,5000))
    def test_private_zip_and_heatmap(self):
        with tempfile.TemporaryDirectory() as t:
            root=Path(t); self.fixture(root/'s.zip'); result=analyze(root/'s.zip',root/'out')
            self.assertTrue(result['synthetic']);self.assertEqual(result['meanFpsDetailed'],30);self.assertEqual(result['meanFpsBasic'],29)
            self.assertIn('SYNTHETIC TEST',(root/'out/report.html').read_text())
            self.assertTrue((root/'out/heatmap-000-screen.png').read_bytes().startswith(b'\x89PNG'))
            with zipfile.ZipFile(root/'s.zip') as z:
                stats=heatmap(z,json.loads(z.read('depth-0001.json')),root/'test.png');self.assertEqual(stats['validFraction'],.75)
    def test_truncated_depth_rejected(self):
        with tempfile.TemporaryDirectory() as t:
            root=Path(t);self.fixture(root/'s.zip',bad=True)
            with self.assertRaisesRegex(ValueError,'encoding'): analyze(root/'s.zip',root/'out')

if __name__=='__main__': unittest.main()
