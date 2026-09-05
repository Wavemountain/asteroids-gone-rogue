#!/usr/bin/env python3
"""Structural checks for the Asteroids gone rogue Week 1 Unity project."""

from __future__ import annotations

import hashlib
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
    require(ROOT / "Assets/Resources/Audio/Music/OutThere.ogg")
    require(ROOT / "Assets/Resources/Audio/Music/spacelifeNo14.ogg")

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

    # Unity 6.6 API: obsolete FindObjectOfType / Rigidbody.velocity / drag must be gone.
    if "FindObjectOfType" in blob or "FindObjectsOfType" in blob:
        err("scripts still call obsolete FindObjectOfType / FindObjectsOfType")
    if "body.velocity" in blob or "_body.velocity" in blob:
        err("scripts still assign Rigidbody.velocity; use linearVelocity")
    if "body.drag" in blob or "body.angularDrag" in blob:
        err("scripts still use Rigidbody.drag / angularDrag; use linearDamping / angularDamping")

    manifest = read(ROOT / "Packages/manifest.json")
    if '"com.unity.ugui": "2.0.0"' not in manifest:
        err("Packages/manifest.json should pin com.unity.ugui 2.0.0 for Unity 6")
    if '"com.unity.inputsystem"' in manifest:
        err("do not add the Input System package (keeps first-open clean)")

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
