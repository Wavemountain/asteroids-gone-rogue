#!/usr/bin/env python3
"""WCAG contrast + colour-blind check for Asteroids gone rogue worlds 1-7 (0.47 C-spec).
Reads WorldRules.PaletteMilli from the repo snapshot and the ContentFactory material values
(transcribed below, each with its source line). Linear colour space (m_ActiveColorSpace: 1).
Run: python3 contrast_worlds.py [path/to/WorldRules.cs]
"""
import re, sys, math, json
import numpy as np

WR = sys.argv[1] if len(sys.argv) > 1 else '/workspace/agr-0.47/_src/repo/Assets/Scripts/Core/WorldRules.cs'
src = open(WR).read()
body = src[src.index('PaletteMilli'):]
body = body[body.index('{'):body.index('};')]
nums = [int(n) for n in re.findall(r'\b\d+\b', body)]
assert len(nums) == 105, len(nums)
STRIDE = 15

def lin(c):
    c = np.asarray(c, float)
    return np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)
def enc(c):
    c = np.clip(np.asarray(c, float), 0, 1)
    return np.where(c <= 0.0031308, c * 12.92, 1.055 * c ** (1 / 2.4) - 0.055)
def hexs(c):
    return '#%02X%02X%02X' % tuple(int(round(float(v) * 255)) for v in np.clip(c, 0, 1))
def lum(c_srgb):
    l = lin(c_srgb); return float(0.2126 * l[0] + 0.7152 * l[1] + 0.0722 * l[2])
def ratio(a, b):
    la, lb = lum(a), lum(b)
    hi, lo = max(la, lb), min(la, lb)
    return (hi + 0.05) / (lo + 0.05)
def H(h):
    h = h.lstrip('#'); return np.array([int(h[i:i + 2], 16) / 255 for i in (0, 2, 4)])

# ---- tokens (UiTheme.cs)
TOK = dict(void='#070B12', surface='#0E1520', surface2='#141C28', primary='#D4A04A', secondary='#6AA8C8',
           accent='#C8CED6', danger='#B85A28', disabled='#3A4450', focus='#E8C878',
           shop_locked_text='#A8B2BC', shop_poor_text='#F0C8A8')

# ---- lighting (Play.unity / GameBootstrap / ArenaEnv / ContentFactory)
def quat_fwd(x, y, z, w):
    # rotate (0,0,1)
    return np.array([2 * (x * z + w * y), 2 * (y * z - w * x), 1 - 2 * (x * x + y * y)])
sun_fwd = quat_fwd(0.35355338, -0.35355338, 0.1464466, 0.8535534)       # Play.unity m_LocalRotation
sun_ndl = max(0.0, -sun_fwd[1])
SUN_I, SUN_COL = 1.15, lin([0.92, 0.95, 1.0])
AMB = lin([0.12, 0.14, 0.18])                                            # RenderSettings.ambientLight (combat)
rim_pitch = math.radians(16)                                             # ArenaRimLight Euler(16,214,0)
rim_ndl = math.sin(rim_pitch); RIM_I = 0.32
BG = np.array([0.02, 0.03, 0.05])                                         # camera.backgroundColor (sRGB)

def soften(c, chroma):
    y = 0.2126 * c[0] + 0.7152 * c[1] + 0.0722 * c[2]       # ArenaEnv.SoftenAmount (on sRGB values, like the code)
    return np.clip(y + (c - y) * chroma, 0, 1)

def world(n):
    r = nums[(n - 1) * STRIDE:(n) * STRIDE]
    m = [v / 1000 for v in r]
    star, neb, grid, floor = np.array(m[0:3]), np.array(m[3:6]), np.array(m[6:9]), np.array(m[9:12])
    br, purple, chroma = m[12], int(r[13] > 0), m[14]
    S = lambda c: soften(np.clip(c * br, 0, 1), chroma)
    floor_alb = np.clip(floor * br, 0, 1)                                 # ArenaEnv.TintFloor (not softened)
    light = AMB + SUN_I * SUN_COL * sun_ndl + RIM_I * rim_ndl * lin(floor_alb)
    floor_lit = enc(np.clip(lin(floor_alb) * light, 0, 1))
    nebula = S(neb); nebula_a = 0.34
    sky = BG * (1 - nebula_a) + nebula * nebula_a
    sky_inner = np.clip(S(neb) + np.array([0.12, 0, 0]), 0, 1)
    sky = sky * (1 - 0.18) + sky_inner * 0.18
    star_c = S(star)
    return dict(n=n, star=star_c, nebula=nebula, grid=S(grid), floor_alb=floor_alb, floor_lit=floor_lit,
                sky=sky, bright=br, chroma=chroma, purple=purple, light=light, rim_col=floor_alb, glow_col=star_c)
W = [world(n) for n in range(1, 8)]
NAMES = ['Launch Belt', 'Deep Orbit', 'Far Drift', 'Mine Fields', 'Cross Gates', 'Debris Islands', 'Spoke Ring']
RULES = ['more pickups', 'faster asteroids', 'faster enemy fire', 'denser debris', 'fewer pickups', 'dim visibility', 'heavy enemy fire']

# ---- gameplay objects: (albedo, emission_rgb_incl_intensity, kind) -- ContentFactory.cs
def em(c, k=1.0): return np.array(c, float) * k
OBJ = {
 # projectiles (ContentFactory L96-102)
 'Player bolt (primary)':   ('crit', [0.831, 0.627, 0.29], em([0.831, 0.627, 0.29], 1.6)),
 'Player spread':           ('crit', [0.788, 0.537, 0.227], em([0.788, 0.537, 0.227], 1.5)),
 'Player pierce (steel)':   ('crit', [0.373, 0.627, 0.722], em([0.373, 0.627, 0.722], 1.7)),
 'Player seeker':           ('crit', [0.4, 0.58, 0.68], em([0.373, 0.627, 0.722], 1.4)),
 'Player ricochet':         ('crit', [0.78, 0.6, 0.32], em([0.831, 0.627, 0.29], 1.4)),
 'Enemy bolt':              ('crit', [0.722, 0.353, 0.157], em([0.722, 0.353, 0.157], 1.2)),
 # enemies: albedo Mat_Enemy (L93), emission via DressEnemyEmission property block (L1523-1557)
 'Enemy Mid01 (red)':       ('crit', [0.5, 0.3, 0.32], em([0.82, 0.1, 0.12])),
 'Enemy Scout/Gunner/Drone/SwarmPod (mat default)': ('crit', [0.5, 0.3, 0.32], em([0.42, 0.08, 0.1], 0.42)),
 'Enemy Sniper (cyan)':     ('crit', [0.5, 0.3, 0.32], em([0.18, 0.72, 1.0])),
 'Enemy Bomber (orange)':   ('crit', [0.5, 0.3, 0.32], em([1.0, 0.42, 0.08], 1.45)),
 'Monster Brute / Boss (aura, pulse avg)': ('crit', [0.58, 0.34, 0.36], em([1.0, 0.32, 0.1], 0.78)),
 'Monster Brute / Boss (pulse min)':       ('crit', [0.58, 0.34, 0.36], em([1.0, 0.32, 0.1], 0.56)),
 'Monster Swarm (aura)':    ('crit', [0.28, 0.44, 0.46], em([0.12, 0.88, 1.0], 0.78)),
 'Monster Swarmling (aura)': ('crit', [0.36, 0.46, 0.4], em([0.28, 1.0, 0.48], 0.78)),
 'Elite mark (MarkElite)':  ('crit', [1.0, 0.78, 0.28], em([1.0, 0.55, 0.12], 1.6)),
 # pickups
 'Pickup ExtraLife heart':  ('crit', [0.92, 0.16, 0.28], em([1.0, 0.22, 0.38])),
 'Pickup generic (Mat_Ship_Accent)': ('crit', [1.0, 0.55, 0.14], em([1.0, 0.4, 0.05], 1.4)),
 'Pickup Shield (Mat_Shield, a=.22)': ('crit', [0.25, 0.85, 1.0], em([0.2, 0.7, 1.0], 0.6)),
 # hazard + player
 'Spike (damaging, emission pulse avg)': ('crit', [0.722, 0.353, 0.157], em([1.0, 0.28, 0.05], 2.2 * 0.72)),
 'Player hull (steel)':     ('info', [0.45, 0.52, 0.58], em([0, 0, 0])),
 'Player accent (amber)':   ('info', [1.0, 0.55, 0.14], em([1.0, 0.4, 0.05], 1.4)),
 'Asteroid':                ('info', [0.38, 0.32, 0.28], em([0, 0, 0])),
}
TELE = {  # telegraph ring: emission = colour * (1.2 + alpha) per TelegraphRing.cs, alpha 0.55 -> 0
 'Ring: Swarm drop (cyan)':    [0.12, 0.88, 1.0],
 'Ring: Boss aimed (yellow)':  [1.0, 0.82, 0.2],
 'Ring: Boss radial (orange)': [1.0, 0.45, 0.12],
 'Ring: ExtraLife (red)':      [1.0, 0.18, 0.32],
}
def apparent(alb, emi, shade=0.5, light=None):
    L = AMB + shade * SUN_I * SUN_COL * sun_ndl
    lit = lin(np.array(alb)) * L
    return enc(np.clip(lit + lin(np.clip(emi, 0, 50)), 0, 1))
def tele_app(col, a):
    col = np.array(col); return enc(np.clip(lin(np.clip(col * (1.2 + a), 0, 50)), 0, 1))

rows = []; flags = []
out = []
P = out.append
P('### A. World palette (reused from `WorldRules.PaletteMilli`, loop 1; hex = what ArenaEnv.Retint/TintFloor produce)\n')
P('| W | Name | floor albedo | floor lit* | sky (nebula over bg)** | star | nebula | brightness | chroma | purple neb | rule |')
P('|---|---|---|---|---|---|---|---|---|---|---|')
for w in W:
    n = w['n']
    P(f"| {n} | {NAMES[n-1]} | `{hexs(w['floor_alb'])}` | `{hexs(w['floor_lit'])}` | `{hexs(w['sky'])}` | `{hexs(w['star'])}` | `{hexs(w['nebula'])}` | {w['bright']:.2f} | {w['chroma']:.2f} | {w['purple']} | {RULES[n-1]} |")
P('')
P(f"\\* lit = albedo x (ambient `(0.12,0.14,0.18)` + sun 1.15 x `(0.92,0.95,1)` x N.L {sun_ndl:.3f} + rim 0.32 x N.L {rim_ndl:.3f} x floor tint), linear space, then sRGB. "
  f"\\*\\* sky = camera bg `#050813` under nebula a=0.34 + inner a=0.18 (uniform-cover upper bound, real texture is patchier/darker). Fog: **off** (`m_Fog: 0`), no skybox, `clearFlags=SolidColor`. Ambient: Flat `#1F242E`-ish `(0.12,0.14,0.18)` in combat, `(0.20,0.17,0.13)` in hangar, same for all 7 worlds.\n")

# B: contrast per item per world
P('### B. Contrast ratios (WCAG, higher = easier) object vs LIT FLOOR, typical lighting (shade factor 0.5)\n')
P('Cell = ratio. `!` = below 3:1 (gameplay-critical). Shade-side minimum (emission+ambient only) is in section C.\n')
hdr = '| Item | ' + ' | '.join(f'W{w["n"]}' for w in W) + ' | min | apparent colour (W1) |'
P(hdr); P('|---|' + '---|' * 9)
worst = {}
for name, (k, alb, emi) in OBJ.items():
    app = apparent(alb, emi); cells = []; mn = 99
    for w in W:
        r = ratio(app, w['floor_lit']); mn = min(mn, r)
        cells.append(f"{r:.1f}" + ('!' if (r < 3 and k == 'crit') else ''))
    worst[name] = mn
    P(f"| {name} | " + ' | '.join(cells) + f" | **{mn:.1f}** | `{hexs(app)}` |")
for name, col in TELE.items():
    cells = []; mn = 99
    for w in W:
        app = tele_app(col, 0.0); r = ratio(app, w['floor_lit']); mn = min(mn, r); cells.append(f"{r:.1f}")
    P(f"| {name} (alpha 0 end state) | " + ' | '.join(cells) + f" | **{mn:.1f}** | `{hexs(tele_app(col,0))}` |")
P('')
P('### C. Same, object vs SKY (edge of arena, nebula behind), and shade-side minimum vs floor\n')
P('| Item | ' + ' | '.join(f'W{w["n"]} sky' for w in W) + ' | min sky | min floor, shade-side |')
P('|---|' + '---|' * 9)
for name, (k, alb, emi) in OBJ.items():
    app = apparent(alb, emi); app0 = apparent(alb, emi, shade=0.0)
    cells = []; mn = 99
    for w in W:
        r = ratio(app, w['sky']); mn = min(mn, r); cells.append(f"{r:.1f}" + ('!' if (r < 3 and k == 'crit') else ''))
    mn0 = min(ratio(app0, w['floor_lit']) for w in W)
    P(f"| {name} | " + ' | '.join(cells) + f" | **{mn:.1f}** | {mn0:.1f}{'!' if (mn0<3 and k=='crit') else ''} |")
P('')

# D: HUD
P('### D. HUD tokens: text on HudPlate (Surface @ 0.72) composited over each world\'s lit floor / sky, and on solid panels\n')
P('| Token | hex | on Void | on Surface | on Surface2 | min over W1-7 plate-over-floor | min plate-over-sky | AA text (4.5) |')
P('|---|---|---|---|---|---|---|---|')
surf = H(TOK['surface'])
def plate_over(bg):  # compose in sRGB like Unity UI does (gamma blend)
    return surf * 0.72 + bg * 0.28
for t in ['primary', 'secondary', 'accent', 'danger', 'focus', 'disabled', 'shop_locked_text', 'shop_poor_text']:
    c = H(TOK[t]); f = min(ratio(c, plate_over(w['floor_lit'])) for w in W); s = min(ratio(c, plate_over(w['sky'])) for w in W)
    rv, rs, rs2 = ratio(c, H(TOK['void'])), ratio(c, H(TOK['surface'])), ratio(c, H(TOK['surface2']))
    ok = 'pass' if min(rs, rs2, f, s) >= 4.5 else ('large only' if min(rs, rs2, f, s) >= 3 else 'FAIL')
    P(f"| {t} | `{TOK[t]}` | {rv:.1f} | {rs:.1f} | {rs2:.1f} | {f:.1f} | {s:.1f} | {ok} |")
P('')
P('Disabled `#3A4450` is by design a de-emphasised state (WCAG exempts inactive controls); never use it for live information (hull/shield/cooldown).\n')

# E: colour-blind
M = {'protan': np.array([[0.152286, 1.052583, -0.204868], [0.114503, 0.786281, 0.099216], [-0.003882, -0.048116, 1.051998]]),
     'deutan': np.array([[0.367322, 0.860646, -0.227968], [0.280085, 0.672501, 0.047413], [-0.011820, 0.042940, 0.968881]]),
     'tritan': np.array([[1.255528, -0.076749, -0.178779], [-0.078411, 0.930809, 0.147602], [0.004733, 0.691367, 0.303900]])}
def sim(c, kind): return enc(np.clip(M[kind] @ lin(c), 0, 1))
def lab(c):
    l = lin(c); X = 0.4124 * l[0] + 0.3576 * l[1] + 0.1805 * l[2]; Y = 0.2126 * l[0] + 0.7152 * l[1] + 0.0722 * l[2]; Z = 0.0193 * l[0] + 0.1192 * l[1] + 0.9505 * l[2]
    f = lambda t: t ** (1 / 3) if t > 0.008856 else 7.787 * t + 16 / 116
    fx, fy, fz = f(X / 0.95047), f(Y), f(Z / 1.08883)
    return np.array([116 * fy - 16, 500 * (fx - fy), 200 * (fy - fz)])
def dE(a, b): return float(np.linalg.norm(lab(a) - lab(b)))
def A(name): return apparent(OBJ[name][1], OBJ[name][2])
PAIRS = [
 ('Player bolt vs Enemy bolt', A('Player bolt (primary)'), A('Enemy bolt')),
 ('Player spread vs Enemy bolt', A('Player spread'), A('Enemy bolt')),
 ('Player pierce vs Enemy bolt', A('Player pierce (steel)'), A('Enemy bolt')),
 ('Brute aura vs Swarmling aura (orange vs green)', A('Monster Brute / Boss (aura, pulse avg)'), A('Monster Swarmling (aura)')),
 ('Brute aura vs Swarm aura (orange vs cyan)', A('Monster Brute / Boss (aura, pulse avg)'), A('Monster Swarm (aura)')),
 ('Mid01 red vs Sniper cyan', A('Enemy Mid01 (red)'), A('Enemy Sniper (cyan)')),
 ('ExtraLife heart vs generic pickup', A('Pickup ExtraLife heart'), A('Pickup generic (Mat_Ship_Accent)')),
 ('ExtraLife heart vs Shield pickup', A('Pickup ExtraLife heart'), A('Pickup Shield (Mat_Shield, a=.22)')),
 ('Ring boss aimed (yellow) vs radial (orange)', tele_app(TELE['Ring: Boss aimed (yellow)'], 0.2), tele_app(TELE['Ring: Boss radial (orange)'], 0.2)),
 ('Ring ExtraLife (red) vs Swarm drop (cyan)', tele_app(TELE['Ring: ExtraLife (red)'], 0.2), tele_app(TELE['Ring: Swarm drop (cyan)'], 0.2)),
 ('Token Primary vs Danger (hull vs low hull)', H(TOK['primary']), H(TOK['danger'])),
 ('Token Primary vs Secondary (hull vs shield bar)', H(TOK['primary']), H(TOK['secondary'])),
]
P('### E. Colour-blind check (Machado 2009, severity 1.0, linear RGB). dE = CIE76 between the two apparent colours; luminance ratio is CVD-invariant enough to rely on\n')
P('| Pair | normal dE | protan dE | deutan dE | tritan dE | luminance ratio | verdict |')
P('|---|---|---|---|---|---|---|')
for nm, a, b in PAIRS:
    d0 = dE(a, b); dp = dE(sim(a, 'protan'), sim(b, 'protan')); dd = dE(sim(a, 'deutan'), sim(b, 'deutan')); dt = dE(sim(a, 'tritan'), sim(b, 'tritan'))
    lr = ratio(a, b); mn = min(dp, dd, dt)
    v = 'OK' if (mn >= 25 or lr >= 1.8) else ('weak - add shape/size cue' if mn >= 12 else 'FAIL - needs shape + luminance cue')
    P(f"| {nm} | {d0:.0f} | {dp:.0f} | {dd:.0f} | {dt:.0f} | {lr:.2f} | {v} |")
P('')

# ---- F: proposed fixes, re-measured
def world_p(n, bright):
    global nums
    saved = nums[(n - 1) * STRIDE + 12]
    nums[(n - 1) * STRIDE + 12] = int(round(bright * 1000))
    w = world(n)
    nums[(n - 1) * STRIDE + 12] = saved
    return w
WP = [world_p(3, 0.88) if n == 3 else world_p(4, 0.80) if n == 4 else world(n) for n in range(1, 8)]
def crit_min(alb, emi, shade=0.5, worlds=W, objs_floor=True):
    app = apparent(alb, emi, shade)
    return [ratio(app, w['floor_lit']) for w in worlds]
P('### F. Proposed readability fixes and re-measured contrast (typical shading, min over the listed worlds)\n')
P('Floor proposal: `Brightness` W3 1.00 -> 0.88, W4 0.94 -> 0.80 (PaletteMilli index 12 of rows 3 and 4 only). Hue, chroma and all other worlds unchanged. '
  'Lit floor becomes W3 `%s`, W4 `%s` (was `%s` / `%s`).\n' % (hexs(WP[2]['floor_lit']), hexs(WP[3]['floor_lit']), hexs(W[2]['floor_lit']), hexs(W[3]['floor_lit'])))
P('| Item | change | before min | after min (W1-7, with floor change) | after at W5 (darkest) | after worst world |')
P('|---|---|---|---|---|---|')
def row(name, change, alb0, emi0, alb1, emi1):
    b = min(crit_min(alb0, emi0)); r1 = crit_min(alb1, emi1, worlds=WP)
    wi = int(np.argmin(r1)) + 1
    P(f"| {name} | {change} | {b:.1f} | {min(r1):.1f} | {r1[4]:.1f} | W{wi} |")
row('Floor only, no object change: Enemy Mid01 red', 'W3/W4 brightness only', OBJ['Enemy Mid01 (red)'][1], OBJ['Enemy Mid01 (red)'][2], OBJ['Enemy Mid01 (red)'][1], OBJ['Enemy Mid01 (red)'][2])
row('Floor only: Brute/Boss pulse avg', 'W3/W4 brightness only', OBJ['Monster Brute / Boss (aura, pulse avg)'][1], OBJ['Monster Brute / Boss (aura, pulse avg)'][2], OBJ['Monster Brute / Boss (aura, pulse avg)'][1], OBJ['Monster Brute / Boss (aura, pulse avg)'][2])
row('Floor only: Brute/Boss pulse min', 'W3/W4 brightness only', OBJ['Monster Brute / Boss (pulse min)'][1], OBJ['Monster Brute / Boss (pulse min)'][2], OBJ['Monster Brute / Boss (pulse min)'][1], OBJ['Monster Brute / Boss (pulse min)'][2])
DG = np.array([0.722, 0.353, 0.157])    # UiTheme.Danger
# generic enemy body: Mat_Enemy emission (0.42,0.08,0.1)*0.42 -> Danger * k
for k in (0.5, 0.7, 0.9):
    row('Scout/Gunner/Drone/SwarmPod body', f'Mat_Enemy emission = Danger x {k}', OBJ['Enemy Scout/Gunner/Drone/SwarmPod (mat default)'][1], OBJ['Enemy Scout/Gunner/Drone/SwarmPod (mat default)'][2], [0.5, 0.3, 0.32], DG * k)
row('Mid01 body', 'emission (0.82,0.1,0.12) x1.35', OBJ['Enemy Mid01 (red)'][1], OBJ['Enemy Mid01 (red)'][2], [0.5, 0.3, 0.32], em([0.82, 0.1, 0.12], 1.35))
row('Brute/Boss pulse min', 'pulse = 1.0 + 0.2 sin (idle), 0.9 + 0.35 sin (charge): min 0.8 / 0.55 -> use base 1.0, amp .2 / .3: min 0.8 / 0.7', OBJ['Monster Brute / Boss (pulse min)'][1], OBJ['Monster Brute / Boss (pulse min)'][2], [0.58, 0.34, 0.36], em([1.0, 0.32, 0.1], 0.8))
row('Brute/Boss pulse min (charging)', 'min pulse 0.7', OBJ['Monster Brute / Boss (pulse min)'][1], OBJ['Monster Brute / Boss (pulse min)'][2], [0.58, 0.34, 0.36], em([1.0, 0.32, 0.1], 0.7))
SEC = np.array([0.416, 0.659, 0.784])   # UiTheme.Secondary
for k in (0.15, 0.25, 0.35):
    row('Player hull (steel)', f'+ rim emission Secondary x {k}', OBJ['Player hull (steel)'][1], OBJ['Player hull (steel)'][2], [0.45, 0.52, 0.58], SEC * k)
for k in (0.10, 0.16):
    row('Asteroid', f'albedo x1.4 + emission Secondary x {k}', OBJ['Asteroid'][1], OBJ['Asteroid'][2], np.array([0.38, 0.32, 0.28]) * 1.4, SEC * k)
P('')


def solve_k(alb, col, target, worlds=WP, shade=0.5):
    for k in np.arange(0.0, 1.6, 0.01):
        if min(ratio(apparent(alb, np.array(col) * k, shade), w['floor_lit']) for w in worlds) >= target:
            return round(float(k), 2)
    return None
P('Emission needed (Secondary `#6AA8C8` steel, lit floor with W3/W4 fix, typical shading) for neutral hazards that cannot rely on an accent colour:\n')
P('| Object | albedo | k for 2:1 | k for 3:1 |')
P('|---|---|---|---|')
for nm, alb in [('Asteroid (as is)', [0.38, 0.32, 0.28]), ('Asteroid albedo x1.5', list(np.array([0.38, 0.32, 0.28]) * 1.5)), ('Player hull', [0.45, 0.52, 0.58])]:
    P(f"| {nm} | `{hexs(np.array(alb))}` | {solve_k(alb, SEC, 2.0)} | {solve_k(alb, SEC, 3.0)} |")
P('')


P('Supplementary: CIELAB lightness difference dL* (object minus lit floor) for the low-ratio items. WCAG ratio is pessimistic for dark-on-dark; dL* >= 15 reads clearly, 8-15 is visible but weak, < 8 is camouflage.\n')
P('| Item | W1 | W2 | W3 | W4 | W5 | W6 | W7 |')
P('|---|---|---|---|---|---|---|---|')
for nm in ['Asteroid', 'Player hull (steel)', 'Enemy Scout/Gunner/Drone/SwarmPod (mat default)', 'Enemy Mid01 (red)', 'Monster Brute / Boss (pulse min)']:
    app = apparent(OBJ[nm][1], OBJ[nm][2])
    P(f"| {nm} | " + ' | '.join(f"{lab(app)[0]-lab(w['floor_lit'])[0]:.0f}" for w in W) + ' |')
P('')


P('dL* after proposed fixes (floor W3/W4 brightness change in all rows; object change as named):\n')
P('| Item / change | W1 | W2 | W3 | W4 | W5 | W6 | W7 | WCAG min |')
P('|---|---|---|---|---|---|---|---|---|')
FIX = [('Asteroid, floor fix only', [0.38, 0.32, 0.28], np.zeros(3)),
       ('Asteroid, floor fix + albedo x1.5', list(np.array([0.38, 0.32, 0.28]) * 1.5), np.zeros(3)),
       ('Asteroid, floor fix + albedo x1.5 + Secondary emission x0.15', list(np.array([0.38, 0.32, 0.28]) * 1.5), SEC * 0.15),
       ('Asteroid, floor fix + albedo x1.5 + Secondary emission x0.30', list(np.array([0.38, 0.32, 0.28]) * 1.5), SEC * 0.30),
       ('Generic enemy body, floor fix + emission Danger x0.9', [0.5, 0.3, 0.32], DG * 0.9)]
for nm, alb, emi in FIX:
    app = apparent(alb, emi)
    P(f"| {nm} | " + ' | '.join(f"{lab(app)[0]-lab(w['floor_lit'])[0]:.0f}" for w in WP) + f" | {min(ratio(app, w['floor_lit']) for w in WP):.1f} |")
P('')

open('/workspace/agr-0.47/tools/contrast_tables.generated.md', 'w').write('\n'.join(out))
json.dump({k: round(v, 2) for k, v in worst.items()}, open('/workspace/agr-0.47/tools/contrast_min.json', 'w'), indent=1)
print('\n'.join(out))
print('sun fwd', sun_fwd, 'ndl', sun_ndl)
