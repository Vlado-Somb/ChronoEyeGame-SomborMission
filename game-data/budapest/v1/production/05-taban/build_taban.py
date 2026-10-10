#!/usr/bin/env python3
"""CHRONO EYE Taban church: original procedural shell; all measurements provisional.
Metres, X width, +Y apse, Z up. NO third-party Cathedral.FBX geometry is copied.
For the detailed offline prototype see TabanChurch_build.py in the review package.
"""
from pathlib import Path
import math, json
import numpy as np
import trimesh

HERE=Path(__file__).resolve().parent
OUT=HERE/"generated"; OUT.mkdir(exist_ok=True)
S=trimesh.Scene(); parts=[]
COL={"wall":[194,181,156,255],"trim":[224,210,183,255],
"roof":[80,72,67,255],"metal":[82,105,99,255],
"dark":[37,42,46,255],"cross":[162,142,92,255]}
def add(name,m,color,evidence="inferred"):
    m.visual.vertex_colors=np.tile(COL[color],(len(m.vertices),1)).astype(np.uint8)
    S.add_geometry(m,geom_name=name,node_name=name)
    parts.append({"name":name,"evidence":evidence,"faces":len(m.faces)})
def box(n,c,s,mat,ev="inferred"):
    m=trimesh.creation.box(extents=s);m.apply_translation(c);add(n,m,mat,ev)
def cyl(n,c,r,h,mat,axis=(0,0,1),ev="inferred"):
    m=trimesh.creation.cylinder(radius=r,height=h,sections=24)
    if tuple(axis)!=(0,0,1):
        m.apply_transform(trimesh.geometry.align_vectors([0,0,1],axis))
    m.apply_translation(c);add(n,m,mat,ev)
def surf(n,vertices,faces,mat,ev="inferred"):
    add(n,trimesh.Trimesh(vertices=vertices,faces=faces,process=False),mat,ev)
def lathe(n,profile,mat,ev="photo-supported silhouette",cy=-14.1):
    v=[];f=[];N=32
    for r,z in profile:
        for i in range(N):
            a=2*math.pi*i/N;v.append([r*math.cos(a),cy+r*math.sin(a),z])
    for j in range(len(profile)-1):
        for i in range(N):
            a=j*N+i;b=j*N+(i+1)%N
            f.extend([[a,b,b+N],[a,b+N,a+N]])
    surf(n,v,f,mat,ev)
def window(n,c,norm,w,z0,z1,ev="inferred"):
    nx,ny=norm;tx,ty=-ny,nx;cx,cy=c
    # Polygon with arched crown; dark surface is placed just outside wall.
    poly=[(-w/2,z0),(w/2,z0),(w/2,z1)]
    for i in range(1,17):
        a=math.pi*i/16;poly.append((w/2*math.cos(a),z1+w/2*math.sin(a)))
    v=[[cx+tx*u+nx*.24,cy+ty*u+ny*.24,z] for u,z in poly]
    f=[[0,i,i+1] for i in range(1,len(v)-1)]
    surf(n,v,f,"dark",ev)
    # framing as narrow rods
    for i in range(len(v)):
        a=np.array(v[i]);b=np.array(v[(i+1)%len(v)]);d=b-a
        if np.linalg.norm(d)<1e-4:continue
        m=trimesh.creation.cylinder(radius=.07,height=np.linalg.norm(d),sections=8)
        m.apply_transform(trimesh.geometry.align_vectors([0,0,1],d))
        m.apply_translation((a+b)/2);add(n+"_trim_"+str(i),m,"trim",ev)

# Main nave, estimated 11.6m wide and 23.8m long.
NW=11.6;Y0=-11.4;Y1=12.4;R=NW/2
box("HYP_nave",(0,.5,5.85),(NW,23.8,11.7),"wall")
for side in (-1,1):
    for y in (-7.6,-2.2,3.2,8.6):
        window("DOC_side_%s_%s"%(side,str(y)),(side*5.89,y),(side,0),1.14,5.4,8.45,"photo-supported openings")
    for y in (-10.7,-5.3,.1,5.5,10.9):
        box("HYP_pilaster_%s_%s"%(side,str(y)),(side*5.96,y,6.15),(.36,.46,12.3),"trim")
    box("DOC_eaves_%s"%side,(side*5.98,.5,11.85),(.42,24.7,.45),"trim","photo-supported silhouette")
    e=NW/2+.72; a=Y0-.5;b=Y1+.15
    v=[[0,a,16.75],[0,b,16.75],[side*e,b,12.25],[side*e,a,12.25]]
    surf("DOC_nave_roof_%s"%side,v,[[0,1,2],[0,2,3]],"roof","photo-supported silhouette")

# Curved apse, semicircular exterior and half-conical roof.
v=[];f=[];N=28
for i in range(N+1):
    a=math.pi*i/N;x=R*math.cos(a);y=Y1+R*math.sin(a)
    v.extend([[x,y,0],[x,y,11.7]])
for i in range(N):
    k=2*i;f.extend([[k,k+2,k+3],[k,k+3,k+1]])
surf("DOC_curved_apse",v,f,"wall","photo-supported massing")
for a in (math.pi*.26,math.pi*.5,math.pi*.74):
    nx,ny=math.cos(a),math.sin(a)
    window("HYP_apse_window_"+str(round(a*100)),(nx*5.9,Y1+ny*5.9),(nx,ny),1.03,5.8,8.5)
v=[[0,Y1,16.45]]
for i in range(N+1):
    a=math.pi*i/N;v.append([(R+.7)*math.cos(a),Y1+(R+.7)*math.sin(a),12.25])
surf("HYP_apse_roof",v,[[0,i+1,i+2] for i in range(N)],"roof")

# West tower: photos support the single tall Baroque silhouette, not these exact dimensions.
TY=-14.1
box("DOC_tower_lower",(0,TY,7.6),(6.8,6.8,15.2),"wall","photo-supported massing")
box("DOC_tower_cornice",(0,TY,15.1),(7.65,7.65,.6),"trim","photo-supported silhouette")
box("DOC_belfry",(0,TY,20.4),(6.15,6.15,10),"wall","photo-supported massing")
box("DOC_belfry_cornice",(0,TY,25.5),(7.2,7.2,.58),"trim","photo-supported silhouette")
for sx in (-1,1):
    for sy in (-1,1):
        box("HYP_belfry_pilaster_%s_%s"%(sx,sy),(sx*3.12,TY+sy*3.12,19.9),(.5,.5,10.3),"trim")
window("HYP_west_door",(0,TY-3.45),(0,-1),2.05,.45,3.4)
for n,c,d in [("west",(0,TY-3.13),(0,-1)),("east",(0,TY+3.13),(0,1)),
              ("north",(3.13,TY),(1,0)),("south",(-3.13,TY),(-1,0))]:
    window("DOC_belfry_"+n,c,d,1.85,18.2,21.8,"photo-supported openings")
    cx,cy=c;nx,ny=d
    cyl("HYP_oculus_"+n,(cx+nx*.32,cy+ny*.32,24.2),.82,.08,"dark",(nx,ny,0))
lathe("DOC_shoulder",[(3.65,25.9),(3.0,26.8),(2.1,29.0)],"metal")
lathe("DOC_onion",[(1.75,29),(2.18,29.25),(2.55,29.7),(2.62,30.2),
                 (2.28,30.7),(1.58,31.1),(1,31.75),(.84,32.3)],"metal")
box("DOC_lantern",(0,TY,33.6),(1.55,1.55,2.65),"wall","photo-supported silhouette")
cyl("DOC_lantern_cap",(0,TY,35.05),1.18,.35,"metal")
lathe("DOC_spire",[(.86,35.2),(.65,36.5),(.45,37.6),(.25,38.6),(.11,39.4)],"metal")
cyl("DOC_cross_orb",(0,TY,39.55),.32,.65,"cross")
box("DOC_cross_vertical",(0,TY,40.35),(.16,.16,1.45),"cross","photo-supported silhouette")
box("DOC_cross_horizontal",(0,TY,40.45),(1.03,.16,.16),"cross","photo-supported silhouette")

glb=OUT/"TabanChurch_reconstruction_v01.glb"
glb.write_bytes(S.export(file_type="glb"))
qa={"assetId":"BP_MODEL_CHURCH","status":"prototype_unverified",
"geometryOrigin":"original procedural, does not contain Cathedral.FBX",
"photographs":["34576.jpg","34577.jpg","34578.jpg","34579.jpg","34580.jpg"],
"estimatedUnits":"metres, Z up, +Y apse; actual cardinal orientation unverified",
"triangles":sum(len(g.faces) for g in S.geometry.values()),
"bounds":S.bounds.tolist(),"publicArReleaseBlocked":True}
(OUT/"TabanChurch_QA.json").write_text(json.dumps(qa,indent=2))
print("TABAN_BUILD_OK",qa)
