"""Optional host GLSL compile check using Mesa surfaceless EGL ES3; not a phone test."""
import ctypes as c
from pathlib import Path
E=c.CDLL('libEGL.so.1'); I=c.c_int; P=c.c_void_p
E.eglGetProcAddress.argtypes=[c.c_char_p];E.eglGetProcAddress.restype=P
get=c.CFUNCTYPE(P,c.c_uint,P,c.POINTER(I))(E.eglGetProcAddress(b'eglGetPlatformDisplayEXT'))
d=get(0x31DD,None,None)
for name,args,restype in [('eglInitialize',[P,c.POINTER(I),c.POINTER(I)],c.c_uint),('eglBindAPI',[c.c_uint],c.c_uint),('eglChooseConfig',[P,c.POINTER(I),c.POINTER(P),I,c.POINTER(I)],c.c_uint),('eglCreateContext',[P,P,P,c.POINTER(I)],P),('eglMakeCurrent',[P,P,P,P],c.c_uint)]:
 f=getattr(E,name);f.argtypes=args;f.restype=restype
major=I();minor=I();assert E.eglInitialize(d,c.byref(major),c.byref(minor));assert E.eglBindAPI(0x30A0)
attrs=(I*7)(0x3040,0x40,0x3033,1,0x3024,8,0x3038);config=P();n=I();assert E.eglChooseConfig(d,attrs,c.byref(config),1,c.byref(n)) and n.value
context=E.eglCreateContext(d,config,None,(I*3)(0x3098,3,0x3038));assert context;assert E.eglMakeCurrent(d,None,None,context)
def gl(name,rest,args):return c.CFUNCTYPE(rest,*args)(E.eglGetProcAddress(name.encode()))
create=gl('glCreateShader',c.c_uint,[c.c_uint]);source=gl('glShaderSource',None,[c.c_uint,I,c.POINTER(c.c_char_p),c.POINTER(I)]);compile=gl('glCompileShader',None,[c.c_uint]);status=gl('glGetShaderiv',None,[c.c_uint,c.c_uint,c.POINTER(I)]);info=gl('glGetShaderInfoLog',None,[c.c_uint,I,c.POINTER(I),c.c_char_p]);
root=Path(__file__).resolve().parents[1]/'app/src/main/assets/shaders'
for file in ['diagnostic_background.frag','occlusion.frag','environmental_hdr.frag']:
 text=(root/file).read_text(); variants=[0,1] if file=='occlusion.frag' else [None]
 for variant in variants:
  t=text if variant is None else text.replace('#version 300 es',f'#version 300 es\n#define USE_OCCLUSION {variant}')
  # Environmental shader requires upstream SH defines supplied by renderer.
  if file=='environmental_hdr.frag': t=t.replace('#version 300 es','#version 300 es\n#define NUMBER_OF_MIPMAP_LEVELS 5')
  s=create(0x8B30);encoded=c.c_char_p(t.encode());source(s,1,c.byref(encoded),None);compile(s);ok=I();status(s,0x8B81,c.byref(ok));buf=c.create_string_buffer(8192);info(s,8192,None,buf)
  print(file,variant,'PASS' if ok.value else 'FAIL',buf.value.decode());assert ok.value
