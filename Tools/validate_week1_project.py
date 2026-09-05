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
    require(ROOT / "CREDITS.md")
    require(ROOT / "Assets/Resources/Audio/Sfx/laserSmall_000.ogg")
    require(ROOT / "Assets/Resources/Audio/Music/OutThere.ogg")
    require(ROOT / "Assets/Resources/Audio/Music/spacelifeNo14.ogg")

    version = read(ROOT / "ProjectSettings/ProjectVersion.txt")
    if "2022.3.21f1" not in version:
        err("ProjectVersion.txt should target Unity 2022.3.21f1 LTS")

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
    if "2022.3.21f1" not in readme:
        err("README must document the Unity version")

    scripts = list((ROOT / "Assets/Scripts").rglob("*.cs"))
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
        "class GameBootstrap",
        "class GameUi",
        "enum GamePhase",
    ]
    blob = "\n".join(read(p) for p in scripts)
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

    for prefab in (
        "Ship_Nose",
        "Ship_Body",
        "Ship_Engine",
        "Ship_Nose_Upgrade01",
        "Ship_Engine_Upgrade01",
        "Ship_Complete",
        "Asteroid_Large",
        "Asteroid_Small",
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

    if ERRORS:
        print("Week 1 validation FAILED:")
        for item in ERRORS:
            print(" -", item)
        return 1

    print("Week 1 Unity project structure OK")
    print(f" Title: {TITLE}")
    print(" Unity: 2022.3.21f1 LTS")
    print(f" Scripts: {len(scripts)}")
    print(" Scene: Assets/Scenes/Play.unity → GameBootstrap")
    return 0


if __name__ == "__main__":
    sys.exit(main())
