#version 300 es
#extension GL_OES_EGL_image_external_essl3 : require
precision highp float;
uniform samplerExternalOES u_CameraColorTexture;
uniform sampler2D u_CameraDepthTexture;
uniform int u_Mode;
uniform bool u_DepthValid;
in vec2 v_CameraTexCoord;
layout(location=0) out vec4 o_FragColor;
void main() {
 vec3 camera=texture(u_CameraColorTexture,v_CameraTexCoord).rgb;
 if(u_Mode==0) { o_FragColor=vec4(camera,1.0); return; }
 vec2 encoded=texture(u_CameraDepthTexture,v_CameraTexCoord).rg;
 float mm=dot(encoded,vec2(255.0,65280.0));
 // Blue 0 m -> cyan 1.25 -> green 2.5 -> yellow 3.75 -> red >=5 m.
 float t=clamp(mm/5000.0,0.0,1.0);
 vec3 heat=clamp(vec3(1.5-abs(4.0*t-3.0),1.5-abs(4.0*t-2.0),1.5-abs(4.0*t-1.0)),0.0,1.0);
 if(mm<0.5 || !u_DepthValid) {
   float check=mod(floor(gl_FragCoord.x/12.0)+floor(gl_FragCoord.y/12.0),2.0);
   heat=mix(vec3(0.07),vec3(0.65,0.0,0.65),check);
 }
 o_FragColor=vec4(u_Mode==2?mix(camera,heat,0.55):heat,1.0);
}
