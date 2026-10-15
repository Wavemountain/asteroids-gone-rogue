#!/usr/bin/env python3
"""0.48 post-stack readability check. Approximates PPv2 ColorGrading (HDR, Neutral) on the
0.47 contrast_worlds.py colours: postExposure (EV), saturation, contrast (log, pivot 0.18),
Lift/Gamma/Gain (w channel only). Reports WCAG ratio object vs lit floor before/after.
Approximation only; final truth is an in-Editor capture."""
import numpy as np
def lin(c): c=np.asarray(c,float); return np.where(c<=0.04045,c/12.92,((c+0.055)/1.055)**2.4)
def enc(c): c=np.clip(c,0,1); return np.where(c<=0.0031308,c*12.92,1.055*c**(1/2.4)-0.055)
def hx(h): return np.array([int(h[i:i+2],16)/255 for i in (1,3,5)])
def L(c): l=lin(c); return 0.2126*l[0]+0.7152*l[1]+0.0722*l[2]
def ratio(a,b): x,y=sorted([L(a),L(b)]); return (y+0.05)/(x+0.05)
def neutral(x):  # Hable-ish neutral approx, white ~ 5.3
    a,b,c,d,e,f=0.2,0.29,0.24,0.272,0.02,0.3
    h=lambda v:((v*(a*v+c*b)+d*e)/(v*(a*v+b)+d*f))-e/f
    return h(x)/h(5.3)
def grade(srgb,ev,sat,con,lift,gain,vig=0.0):
    l=lin(srgb)*2**ev
    y=0.2126*l[0]+0.7152*l[1]+0.0722*l[2]
    l=y+(l-y)*(1+sat/100)
    lg=np.log2(np.maximum(l,1e-5)/0.18)*(1+con/100); l=0.18*2**lg
    l=neutral(l); l=l*(1+gain)+lift*(1-l)
    return enc(l*(1-vig))
FLOOR={1:'#071E63',2:'#240745',3:'#044233',4:'#552D04',5:'#0B0F29',6:'#270602',7:'#49072E'}
OBJ={'enemy bolt':'#F27837','Mid01':'#DC373F','player bolt':'#FFFF7F','pierce':'#AAFFFF'}
# ev, sat, contrast, lift(w), gain(w)  -- must match 01_post_stack.md table
G={1:(0.10,-12,8,-0.010,0.02),2:(0.15,-18,8,-0.010,0.02),3:(0.05,-20,10,-0.015,0.0),
   4:(0.00,-25,10,-0.015,0.0),5:(0.20,-10,6,-0.005,0.03),6:(0.15,-25,8,-0.010,0.02),7:(0.10,-20,8,-0.010,0.02)}
print('| object | '+' | '.join(f'W{w} pre/post' for w in range(1,8))+' | worst delta |')
print('|'+'---|'*9)
for n,h in OBJ.items():
    cells=[];worst=9
    for w in range(1,8):
        g=G[w]; pre=ratio(hx(h),hx(FLOOR[w]))
        # untoned baseline also goes through tonemap so compare like for like
        base=ratio(grade(hx(h),0,0,0,0,0),grade(hx(FLOOR[w]),0,0,0,0,0))
        post=ratio(grade(hx(h),*g),grade(hx(FLOOR[w]),*g))
        cells.append(f'{base:.1f}/{post:.1f}'); worst=min(worst,post-base)
    print(f'| {n} | '+' | '.join(cells)+f' | {worst:+.2f} |')
