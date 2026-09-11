#!/usr/bin/env python3
"""Structural checks for the Asteroids gone rogue Week 1 Unity project."""

from __future__ import annotations

import hashlib
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
TITLE = "Asteroids gone rogue"
ERRORS: list[str] = []


def guid_for(relative: str) -> str:
    return hashlib.md5(f"asteroids-gone-rogue:{relative}".encode("utf-8")).hexdigest()


def err(message: str) -> None:
    ERRORS.append(message)


def require(path: Path, hint: str = "") -> None:
    if not path.exists():
        err(f"missing {path.relative_to(ROOT)}" + (f" ({hint})" if hint else ""))


def read(path: Path) -> str:
    return path.read_text(encoding="utf-8")


_CS_BLOCK_COMMENT = re.compile(r"/\*.*?\*/", re.DOTALL)
_CS_LINE_COMMENT = re.compile(r"//.*?$", re.MULTILINE)
_CS_STRING = re.compile(r'"(?:\\.|[^"\\])*"')
_SHADER_DECL = re.compile(r"\bShader\s+(\w+)\b")
_MATERIAL_CTOR = re.compile(r"\bnew\s+Material\s*\(")


def strip_cs_noise(source: str) -> str:
    """Drop comments and string literals so static C# scans stay structural."""
    cleaned = _CS_BLOCK_COMMENT.sub(" ", source)
    cleaned = _CS_LINE_COMMENT.sub(" ", cleaned)
    return _CS_STRING.sub('""', cleaned)


def _split_cs_statements(source: str) -> list[str]:
    statements: list[str] = []
    buf: list[str] = []
    depth = 0
    for ch in source:
        if ch in "([{":
            depth += 1
        elif ch in ")]}":
            depth = max(0, depth - 1)
        if ch == ";" and depth == 0:
            stmt = "".join(buf).strip()
            if stmt:
                statements.append(stmt)
            buf = []
            continue
        buf.append(ch)
    tail = "".join(buf).strip()
    if tail:
        statements.append(tail)
    return statements


def _paren_args(source: str, open_index: int) -> str | None:
    if open_index < 0 or open_index >= len(source) or source[open_index] != "(":
        return None
    depth = 0
    for i in range(open_index, len(source)):
        ch = source[i]
        if ch == "(":
            depth += 1
        elif ch == ")":
            depth -= 1
            if depth == 0:
                return source[open_index + 1 : i]
    return None


def _has_toplevel_ternary(expr: str) -> bool:
    """True when `?:` is the top-level operator (Unity 6000.6 CS1503 / target-typed ?:)."""
    depth = 0
    i = 0
    while i < len(expr):
        ch = expr[i]
        if ch in "([{":
            depth += 1
        elif ch in ")]}":
            depth = max(0, depth - 1)
        elif ch == "?" and depth == 0:
            nxt = expr[i + 1] if i + 1 < len(expr) else ""
            if nxt not in ".?":
                return True
        i += 1
    return False


def shader_conditional_violations(source: str) -> list[str]:
    """Flag ternary→Shader and target-typed `?:` passed to Material(Shader) (CS1503)."""
    hits: list[str] = []
    cleaned = strip_cs_noise(source)
    shader_names: set[str] = set()
    for stmt in _split_cs_statements(cleaned):
        for match in _SHADER_DECL.finditer(stmt):
            shader_names.add(match.group(1))

        decl = re.search(r"\bShader\s+(\w+)\s*=\s*(.*)$", stmt, re.DOTALL)
        if decl and _has_toplevel_ternary(decl.group(2)):
            hits.append(f"ternary assigned to Shader {decl.group(1)}")

        assign = re.search(r"^(\w+)\s*=\s*(.*)$", stmt, re.DOTALL)
        if assign and assign.group(1) in shader_names and _has_toplevel_ternary(assign.group(2)):
            label = f"ternary assigned to Shader {assign.group(1)}"
            if label not in hits:
                hits.append(label)

        search_from = 0
        while True:
            ctor = _MATERIAL_CTOR.search(stmt, search_from)
            if not ctor:
                break
            args = _paren_args(stmt, ctor.end() - 1)
            if args is not None and _has_toplevel_ternary(args):
                hits.append("target-typed ternary passed to new Material")
            search_from = ctor.end()
    return hits


LFS_POINTER_PREFIX = b"version https://git-lfs.github.com/spec/v1"


def is_lfs_pointer(path: Path) -> bool:
    """True when the working-tree file is Git LFS pointer text, not an FBX binary."""
    if not path.is_file():
        return False
    if path.stat().st_size < 1000:
        return True
    head = path.read_bytes()[:64]
    return head.startswith(LFS_POINTER_PREFIX)


def main() -> int:
    require(ROOT / "Packages/manifest.json")
    require(ROOT / "ProjectSettings/ProjectVersion.txt")
    require(ROOT / "ProjectSettings/ProjectSettings.asset")
    require(ROOT / "ProjectSettings/EditorBuildSettings.asset")
    require(ROOT / "ProjectSettings/TagManager.asset")
    require(ROOT / "Assets/Scenes/Play.unity")
    require(ROOT / "Assets/Scripts/Content/GameBootstrap.cs")
    require(ROOT / "Assets/Art/Import/IMPORT.md")
    require(ROOT / "Assets/Art/Import/MANIFEST.md")
    require(ROOT / "README.md")
    require(ROOT / "MERGE_CHECKLIST.md")
    checklist = read(ROOT / "MERGE_CHECKLIST.md")
    if "Do not merge until Wagge" not in checklist and "until Wagge says yes" not in checklist:
        err("MERGE_CHECKLIST.md must say not to merge until Wagge says yes")
    if "6000.6.0f1" not in checklist:
        err("MERGE_CHECKLIST.md must name Unity 6000.6.0f1")
    require(ROOT / "CREDITS.md")
    require(ROOT / "Assets/Resources/Audio/Sfx/laserSmall_000.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/click_002.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/minimize_005.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/laserRetro_000.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/laserLarge_000.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/impactMetal_003.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/impactMetal_000.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/impactMetal_001.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/explosionCrunch_001.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/phaserUp5.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/lowThreeTone.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/phaseJump1.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/slime_000.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/impactMetal_002.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/laserSmall_002.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/laserSmall_004.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/zap1.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/spaceTrash1.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/spaceTrash3.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/forceField_001.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/lowFrequency_explosion_000.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/laserRetro_002.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/jingles_PIZZA16.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/jingles_NES07.ogg")
    require(ROOT / "Assets/Resources/Audio/Music/OutThere.ogg")
    require(ROOT / "Assets/Resources/Audio/Music/spacelifeNo14.ogg")
    require(ROOT / "Assets/Resources/Audio/Music/SpaceCadet.ogg")
    require(ROOT / "Assets/Resources/Fonts/KenneyFuture.ttf")
    require(ROOT / "Assets/Resources/Fonts/KenneyFutureNarrow.ttf")
    require(ROOT / "Assets/Resources/Fonts/Kenney_Fonts_License.txt")

    version = read(ROOT / "ProjectSettings/ProjectVersion.txt")
    if "6000.6.0f1" not in version:
        err("ProjectVersion.txt should target Unity 6000.6.0f1")

    settings = read(ROOT / "ProjectSettings/ProjectSettings.asset")
    if f"productName: {TITLE}" not in settings:
        err("PlayerSettings productName must be exactly 'Asteroids gone rogue'")
    if "activeInputHandler: 0" not in settings:
        err("activeInputHandler should be 0 (old Input Manager)")

    build = read(ROOT / "ProjectSettings/EditorBuildSettings.asset")
    if "Assets/Scenes/Play.unity" not in build:
        err("EditorBuildSettings must list Assets/Scenes/Play.unity")
    if "3a8c0e1b5d7246f0a2c9d4e6b1f70835" not in build:
        err("EditorBuildSettings scene guid mismatch")

    tags = read(ROOT / "ProjectSettings/TagManager.asset")
    for tag in ("Player", "Enemy", "Asteroid", "Projectile"):
        if f"- {tag}" not in tags:
            err(f"TagManager missing {tag}")

    readme = read(ROOT / "README.md")
    if not readme.startswith(f"# {TITLE}"):
        err("README title must be Asteroids gone rogue")
    if "6000.6.0f1" not in readme:
        err("README must document Unity 6000.6.0f1")

    scripts = list((ROOT / "Assets/Scripts").rglob("*.cs"))
    editor_scripts = list((ROOT / "Assets/Editor").rglob("*.cs"))
    if len(scripts) < 20:
        err(f"expected a full script set, found {len(scripts)}")

    required_types = [
        "class GameSession",
        "class GameManager",
        "class WaveManager",
        "class ShipController",
        "class Asteroid",
        "class EnemySeeker",
        "class HangarShop",
        "class ContentFactory",
        "class ArtImport",
        "class GameBootstrap",
        "class GameUi",
        "class UiFonts",
        "class HangarPersist",
        "class MedalCatalog",
        "class ArenaLayout",
        "class ArenaHazard",
        "class MonsterPresence",
        "class TelegraphRing",
        "class Loc",
        "enum GamePhase",
    ]
    blob = "\n".join(read(p) for p in scripts + editor_scripts)
    for token in required_types:
        if token not in blob:
            err(f"missing C# {token}")

    for phase in ("Hangar", "Playing", "WaveClear", "Failed"):
        if phase not in blob:
            err(f"state machine missing {phase}")

    bootstrap_guid = guid_for("Assets/Scripts/Content/GameBootstrap.cs")
    meta = ROOT / "Assets/Scripts/Content/GameBootstrap.cs.meta"
    if meta.exists():
        if f"guid: {bootstrap_guid}" not in read(meta):
            err("GameBootstrap.cs.meta guid does not match generator convention")
    else:
        err("GameBootstrap.cs.meta missing")

    scene = read(ROOT / "Assets/Scenes/Play.unity")
    if bootstrap_guid not in scene:
        err("Play.unity does not reference GameBootstrap script guid")
    if "GameSystems" not in scene:
        err("Play.unity missing GameSystems object")
    if "Main Camera" not in scene:
        err("Play.unity missing Main Camera")
    if "EventSystem" not in scene:
        err("Play.unity missing EventSystem")
    if "76c392e42b5098c458856cdf6ecaaaa1" not in scene:
        err("Play.unity EventSystem must reference ugui EventSystem script guid")
    if "4f231c4fb786f3946a6b90b886c48677" not in scene:
        err("Play.unity must persist StandaloneInputModule (ugui package guid) so Hub open has an Input Module")
    if "4f231eb8fc47f54ca11b152d6d181d1e" in scene:
        err("Play.unity still uses the old UnityEngine.UI.dll StandaloneInputModule guid (missing script after Library wipe)")
    if "m_HorizontalAxis: Horizontal" not in scene or "m_VerticalAxis: Vertical" not in scene:
        err("Play.unity StandaloneInputModule must bind Horizontal / Vertical Input Manager axes")
    if "m_SubmitButton: Submit" not in scene or "m_CancelButton: Cancel" not in scene:
        err("Play.unity StandaloneInputModule must bind Submit / Cancel Input Manager buttons")
    if "InputSystemUIInputModule" in scene:
        err("Play.unity must stay on StandaloneInputModule (no Input System UI module)")
    bootstrap_src = read(ROOT / "Assets/Scripts/Content/GameBootstrap.cs")
    if "EnsureEventSystem" not in bootstrap_src:
        err("GameBootstrap must EnsureEventSystem")
    if "GetComponent<StandaloneInputModule>" not in bootstrap_src:
        err("GameBootstrap must repair a scene EventSystem that is missing StandaloneInputModule")
    if "AddComponent<StandaloneInputModule>" not in bootstrap_src:
        err("GameBootstrap must AddComponent StandaloneInputModule")
    if "horizontalAxis" not in bootstrap_src:
        err("GameBootstrap must wire StandaloneInputModule Input Manager axes")
    if "forceModuleActive" in bootstrap_src or "forceModuleActive" in blob:
        err("scripts must not set obsolete StandaloneInputModule.forceModuleActive (CS0619)")
    if "m_ForceModuleActive: 1" in scene:
        err("Play.unity must not force StandaloneInputModule (m_ForceModuleActive: 1)")

    for mat in (
        "Mat_Ship_Hull",
        "Mat_Ship_Accent",
        "Mat_Ship_Glass",
        "Mat_Ship_Glow",
        "Mat_Asteroid",
        "Mat_Enemy",
        "Mat_Arena",
    ):
        require(ROOT / f"Assets/Art/Materials/{mat}.mat")

    factory = read(ROOT / "Assets/Scripts/Content/ContentFactory.cs")
    if "ArtImport.TryInstantiate" not in factory:
        err("ContentFactory should instantiate Import FBX through ArtImport")
    if "Ship_Nose_Upgrade01" not in factory or "Ship_Engine_Upgrade01" not in factory:
        err("ContentFactory should bind hangar upgrade FBX slots")
    if "Ship_Body_Upgrade01" not in factory:
        err("ContentFactory should bind Ship_Body_Upgrade01 for the shop swap")
    if "RosterForWave" not in read(ROOT / "Assets/Scripts/Core/WaveManager.cs"):
        err("WaveManager should expose a Scout/Gunner/Drone ladder")

    art_import = read(ROOT / "Assets/Scripts/Content/ArtImport.cs")
    if "Resources.Load" not in art_import:
        err("ArtImport must Resources.Load Play Mode FBX so Press Play needs no Inspector wiring")
    if "AssetDatabase.LoadAssetAtPath" not in art_import:
        err("ArtImport should also load Assets/Art/Import via AssetDatabase in the Editor")

    play_fbx = (
        "Ship_Nose",
        "Ship_Body",
        "Ship_Body_Upgrade01",
        "Ship_Engine",
        "Ship_Nose_Upgrade01",
        "Ship_Engine_Upgrade01",
        "Enemy_01",
        "Enemy_Scout",
        "Enemy_Gunner",
        "Enemy_Drone",
        "Enemy_Bomber",
        "Enemy_Sniper",
        "Monster_Brute",
        "Monster_Swarm",
        "Arena_Hazard_Spike",
        "Asteroid_Large",
        "Asteroid_Small",
        "Asteroid_VariantB_Large",
        "Asteroid_VariantB_Small",
        "Arena_Blockout",
        "Hangar_Crate",
        "Hangar_Terminal",
        "Hangar_LightPillar",
        "Hangar_Workbench",
        "Hangar_FuelCell",
        "Hangar_ShopKiosk",
        "Hangar_Console",
        "Hangar_PowerBox",
        "Hangar_FireExtinguisher",
        "Hangar_Locker",
        "Hangar_LaunchSign",
        "Ship_Complete",
        "Projectile_Bolt",
        "Projectile_EnemyBolt",
        "Pickup_Score",
        "Pickup_Shield",
    )
    for name in play_fbx:
        path = ROOT / f"Assets/Art/Import/{name}.fbx"
        require(path, "Play Mode mesh")
        if path.exists() and is_lfs_pointer(path):
            err(f"{path.relative_to(ROOT)} looks like an LFS pointer, not an FBX")
        meta = ROOT / f"Assets/Art/Import/{name}.fbx.meta"
        require(meta, "ModelImporter settings")
        if meta.exists():
            text = read(meta)
            if "addColliders: 0" not in text:
                err(f"{name}.fbx.meta should disable generated colliders")
            if "globalScale: 1" not in text:
                err(f"{name}.fbx.meta should import at scale 1")

        resources = ROOT / f"Assets/Resources/Art/Import/{name}.fbx"
        require(resources, "Resources.Load Play Mode mesh")
        if resources.exists() and is_lfs_pointer(resources):
            err(f"{resources.relative_to(ROOT)} looks like an LFS pointer, not an FBX")
        require(ROOT / f"Assets/Resources/Art/Import/{name}.fbx.meta", "Resources ModelImporter")

    for font_name in ("KenneyFuture.ttf", "KenneyFutureNarrow.ttf"):
        font_path = ROOT / "Assets/Resources/Fonts" / font_name
        require(font_path, "bundled HUD font")
        if font_path.exists() and is_lfs_pointer(font_path):
            err(f"{font_path.relative_to(ROOT)} looks like an LFS pointer, not a font")

    resources_import = ROOT / "Assets/Resources/Art/Import"
    for fbx in sorted(resources_import.glob("*.fbx")):
        if is_lfs_pointer(fbx):
            err(f"{fbx.relative_to(ROOT)} is a Git LFS pointer text file, not an FBX binary")

    for prefab in (
        "Ship_Nose",
        "Ship_Body",
        "Ship_Engine",
        "Ship_Nose_Upgrade01",
        "Ship_Engine_Upgrade01",
        "Ship_Complete",
        "Asteroid_Large",
        "Asteroid_Small",
        "Asteroid_VariantB_Large",
        "Asteroid_VariantB_Small",
        "Enemy_01",
        "Arena_Blockout",
        "Hangar_Crate",
        "Hangar_Terminal",
        "Hangar_LightPillar",
    ):
        require(ROOT / f"Assets/Art/Prefabs/{prefab}.prefab")

    # No script should use the new Input System package API.
    if "UnityEngine.InputSystem" in blob:
        err("scripts should stay on the old Input Manager for a clean first open")

    # Unity 6.6 API: Arial builtin is gone; bundled Kenney fonts with LegacyRuntime fallback.
    if "Arial.ttf" in blob or '"Arial"' in blob:
        err("scripts still load builtin/OS Arial; use bundled font or LegacyRuntime.ttf")
    if 'GetBuiltinResource<Font>("LegacyRuntime.ttf")' not in blob and "LegacyBuiltin" not in blob:
        err("scripts should keep LegacyRuntime.ttf as the Unity 6.6 font fallback")
    fonts = read(ROOT / "Assets/Scripts/UI/UiFonts.cs")
    if "Fonts/KenneyFuture" not in fonts or "Fonts/KenneyFutureNarrow" not in fonts:
        err("UiFonts should load bundled Kenney Future / Future Narrow")
    if 'GetBuiltinResource<Font>(LegacyBuiltin)' not in fonts and 'GetBuiltinResource<Font>("LegacyRuntime.ttf")' not in fonts:
        err("UiFonts should fall back to builtin LegacyRuntime.ttf")
    for path in scripts + editor_scripts:
        for hit in shader_conditional_violations(read(path)):
            err(f"{path.relative_to(ROOT)}: {hit} (Unity 6000.6 CS1503)")

    if "FindObjectOfType" in blob or "FindObjectsOfType" in blob:
        err("scripts still call obsolete FindObjectOfType / FindObjectsOfType")
    if "GetInstanceID" in blob:
        err("scripts still call obsolete GetInstanceID; use GetEntityId")
    if "FindFirstObjectByType" in blob:
        err("scripts still call FindFirstObjectByType; use FindAnyObjectByType")
    if "FindObjectsSortMode" in blob:
        err("scripts still pass FindObjectsSortMode to FindObjectsByType")
    if "body.velocity" in blob or "_body.velocity" in blob:
        err("scripts still assign Rigidbody.velocity; use linearVelocity")
    if "body.drag" in blob or "body.angularDrag" in blob:
        err("scripts still use Rigidbody.drag / angularDrag; use linearDamping / angularDamping")

    manifest = read(ROOT / "Packages/manifest.json")
    lock = read(ROOT / "Packages/packages-lock.json")
    if '"com.unity.ugui": "2.0.0"' not in manifest:
        err("Packages/manifest.json should pin com.unity.ugui 2.0.0 for Unity 6")
    if '"com.unity.inputsystem"' in manifest:
        err("do not add the Input System package (keeps first-open clean)")
    if "com.unity.textmeshpro" in manifest or "com.unity.textmeshpro" in lock:
        err("do not add TextMeshPro (bundled Kenney Font + ugui Text stays Hub-safe)")
    for blocked in (
        "com.unity.modules.vr",
        "com.unity.modules.xr",
        "com.unity.modules.cloth",
        "com.unity.modules.terrain",
        "com.unity.modules.vehicles",
        "com.unity.modules.androidjni",
        "com.unity.modules.accessibility",
        "com.unity.modules.umbra",
        "com.unity.modules.unityanalytics",
        "com.unity.modules.tilemap",
        "com.unity.modules.wind",
    ):
        if f'"{blocked}"' in manifest:
            err(f"Packages/manifest.json must not list {blocked} (Hub Continue / unused builtin)")
        if f'"{blocked}"' in lock:
            err(f"Packages/packages-lock.json must not list {blocked}")

    if ERRORS:
        print("Week 1 validation FAILED:")
        for item in ERRORS:
            print(" -", item)
        return 1

    print("Week 1 Unity project structure OK")
    print(f" Title: {TITLE}")
    print(" Unity: 6000.6.0f1")
    print(f" Scripts: {len(scripts)}")
    print(" Scene: Assets/Scenes/Play.unity → GameBootstrap")
    return 0


if __name__ == "__main__":
    sys.exit(main())
