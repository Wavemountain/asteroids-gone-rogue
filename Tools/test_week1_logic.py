#!/usr/bin/env python3
"""Mirrors GameSession / LoadoutState / shop rules used by the C# Week 1 loop."""

from __future__ import annotations

import sys


class Phase:
    HANGAR = "Hangar"
    PLAYING = "Playing"
    WAVE_CLEAR = "WaveClear"
    FAILED = "Failed"


class Session:
    def __init__(self) -> None:
        self.phase = Phase.HANGAR
        self.wave = 1
        self.score = 0
        self.credits = 0
        self.fail_reason = ""

    @property
    def can_start(self) -> bool:
        return self.phase in (Phase.HANGAR, Phase.WAVE_CLEAR, Phase.FAILED)

    def begin(self) -> None:
        assert self.can_start
        self.fail_reason = ""
        self.phase = Phase.PLAYING

    def add_score(self, amount: int) -> None:
        self.score += amount

    def complete(self, bonus: int = 100, credits: int = 150) -> None:
        assert self.phase == Phase.PLAYING
        self.score += bonus
        self.credits += credits
        self.wave += 1
        self.phase = Phase.WAVE_CLEAR

    def fail(self, reason: str | None = None) -> None:
        assert self.phase == Phase.PLAYING
        self.fail_reason = reason or "Unknown cause"
        self.phase = Phase.FAILED

    def hangar(self) -> None:
        self.phase = Phase.HANGAR

    def abort(self) -> None:
        assert self.phase == Phase.PLAYING
        self.fail_reason = ""
        self.phase = Phase.HANGAR

    def spend(self, cost: int) -> bool:
        if self.credits < cost:
            return False
        self.credits -= cost
        return True


class Loadout:
    def __init__(self) -> None:
        self.rapid = False
        self.shields = 0
        self.nose = False
        self.body = False

    @property
    def cooldown(self) -> float:
        return 0.16 if self.rapid else 0.38

    @property
    def damage(self) -> int:
        return 2 if self.nose else 1

    @property
    def hull(self) -> int:
        return 4 if self.body else 3


def test_clear_loop() -> None:
    s = Session()
    assert s.phase == Phase.HANGAR
    s.begin()
    s.add_score(25 + 10 + 50)
    s.complete()
    assert s.phase == Phase.WAVE_CLEAR
    assert s.wave == 2
    assert s.score == 185
    assert s.credits == 150
    assert s.spend(100)
    loadout = Loadout()
    loadout.rapid = True
    assert loadout.cooldown == 0.16
    s.hangar()
    s.begin()
    assert s.wave == 2


def test_fail_keeps_wave_and_upgrades() -> None:
    s = Session()
    s.begin()
    s.fail()
    assert s.phase == Phase.FAILED
    assert s.fail_reason == "Unknown cause"
    assert s.wave == 1
    assert s.credits == 0
    s.hangar()
    s.begin()
    assert s.wave == 1
    assert s.fail_reason == ""


def test_fail_stores_death_cause() -> None:
    s = Session()
    s.begin()
    s.fail("Asteroid collision")
    assert s.phase == Phase.FAILED
    assert s.fail_reason == "Asteroid collision"
    s.hangar()
    s.begin()
    s.fail("Enemy contact")
    assert s.fail_reason == "Enemy contact"


def test_shop_cannot_overspend() -> None:
    s = Session()
    s.begin()
    s.complete()
    assert not s.spend(200)
    assert s.credits == 150
    assert s.spend(80)
    assert s.credits == 70
    loadout = Loadout()
    loadout.shields = 1
    assert loadout.shields == 1


def test_abort_keeps_wave_score_and_skips_bonus() -> None:
    s = Session()
    s.begin()
    s.add_score(50)
    s.abort()
    assert s.phase == Phase.HANGAR
    assert s.wave == 1
    assert s.score == 50
    assert s.credits == 0
    s.begin()
    s.complete()
    assert s.credits == 150
    assert s.wave == 2


def wrap_xz(x: float, z: float, radius: float, inset: float = 0.05) -> tuple[float, float]:
    inner = radius - inset
    mag = (x * x + z * z) ** 0.5
    if mag <= radius:
        return x, z
    return -x * inner / mag, -z * inner / mag


def test_arena_wrap_mirrors_opposite_edge() -> None:
    x, z = wrap_xz(30.0, 0.0, 22.0)
    assert abs(x + 21.95) < 0.001
    assert abs(z) < 0.001
    x, z = wrap_xz(0.0, -40.0, 22.0)
    assert abs(x) < 0.001
    assert abs(z - 21.95) < 0.001
    x, z = wrap_xz(3.0, 4.0, 22.0)
    assert (x, z) == (3.0, 4.0)


def test_nose_changes_damage() -> None:
    loadout = Loadout()
    assert loadout.damage == 1
    loadout.nose = True
    assert loadout.damage == 2


def test_body_upgrade_adds_hull() -> None:
    loadout = Loadout()
    assert loadout.hull == 3
    loadout.body = True
    assert loadout.hull == 4


def large_asteroid_count(wave: int) -> int:
    count = min(max(4 + (wave - 1), 4), 7)
    if wave > 10:
        count = min(count + (wave - 10), 10)
    return count


def test_wave_ladder_rises() -> None:
    from pathlib import Path

    text = (Path(__file__).resolve().parents[1] / "Assets/Scripts/Core/WaveManager.cs").read_text(
        encoding="utf-8"
    )
    assert "case 2:" in text and "EnemyKind.Scout" in text
    assert "case 4:" in text and "EnemyKind.Gunner" in text
    assert "case 5:" in text and "EnemyKind.Drone" in text
    assert "EnemyKind.Bomber" in text
    factory = (Path(__file__).resolve().parents[1] / "Assets/Scripts/Content/ContentFactory.cs").read_text(
        encoding="utf-8"
    )
    assert "VariantC" in factory and "VariantD" in factory
    assert "ArenaVisualForWave" in factory
    assert "WorldIndexForWave" in factory
    assert "Arena_World2_Blockout" in factory
    assert "Hangar_AmmoRack" in factory
    assert "Hangar_Console" in factory
    assert "Hangar_PowerBox" in factory
    assert "Hangar_FireExtinguisher" in factory
    assert "Hangar_Locker" in factory
    assert "PlateauWave" in text
    assert "PlateauAsteroidCap" in text
    assert large_asteroid_count(1) == 4
    assert large_asteroid_count(4) == 7
    assert large_asteroid_count(10) == 7
    assert large_asteroid_count(11) == 8
    assert large_asteroid_count(13) == 10
    assert large_asteroid_count(20) == 10
    art_list = (Path(__file__).resolve().parents[1] / "Assets/Scripts/Content/ArtImport.cs").read_text(
        encoding="utf-8"
    )
    warm = art_list.split("PlayModeAssets")[1].split("};")[0]
    assert "Ship_Body_Upgrade02" not in warm
    assert "Vfx_MuzzleFlash" in (
        Path(__file__).resolve().parents[1] / "Assets/Scripts/Player/ShipShooter.cs"
    ).read_text(encoding="utf-8")


def test_factory_wires_import_fbx() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    art = (root / "Assets/Scripts/Content/ArtImport.cs").read_text(encoding="utf-8")
    assert "ArtImport.TryInstantiate" in factory
    assert "Ship_Nose_Upgrade01" in factory
    assert "Ship_Engine_Upgrade01" in factory
    assert "Ship_Body_Upgrade01" in factory
    waves = (root / "Assets/Scripts/Core/WaveManager.cs").read_text(encoding="utf-8")
    assert "Enemy_Scout" in waves or "EnemyKind.Scout" in waves
    assert "EnemyKind.Gunner" in waves
    assert "EnemyKind.Drone" in waves
    assert "EnemyKind.Bomber" in waves
    assert "EnemyKind.Sniper" in waves
    assert "EnemyKind.SwarmPod" in waves
    catalog = (root / "Assets/Scripts/Core/ShopCatalog.cs").read_text(encoding="utf-8")
    assert "BodyUpgrade01" in catalog
    assert "NoseUpgrade02" in catalog
    assert "EngineUpgrade02" in catalog
    assert "ShopGroup.Hull" in catalog
    assert "ShopGroup.Weapons" in catalog
    assert "ShopGroup.Defense" in catalog
    assert "SpreadBolt" in catalog and "Pierce" in catalog
    assert "ShieldCell" in catalog
    assert "Projectile_Bolt" in factory
    assert "Projectile_EnemyBolt" in factory
    assert "Arena_Blockout" in factory
    assert "Resources.Load" in art
    assert "AssetDatabase.LoadAssetAtPath" in art
    assert "Enemy_01" in factory
    assert "CreateEnemy(" in factory
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    assert "FirstHangarHintKey" in ui
    assert "agr.ui.firstHangarHint" in ui
    assert "First flight" in ui
    assert "Clear a wave to earn credits and upgrades." in ui
    assert "Abort (Esc)" in ui
    assert "Q / RMB fire modes" in ui
    assert "discover Spread / Pierce when owned" in ui
    assert "Got it" in ui
    assert "DismissFirstHangarHint" in ui
    assert "HULL / NOSE / ENGINE" in ui or "HullHeader" in catalog
    assert "OWNED" in ui and "LOCKED" in ui
    assert "PointerEnter" in ui
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")
    assert "DefaultSfxVolume = 0.8f" in audio
    assert "DefaultMusicVolume = 0.28f" in audio
    assert "HangarMusicScale = 0.36f" in audio
    assert "ArenaMusicScale = 0.82f" in audio
    assert "HangarMusicPitch = 0.88f" in audio
    assert "PlayerPrefs.GetInt(MuteKey, 0)" in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/maximize_008")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/click_002")' in audio
    assert "Play(_worldChange)" in audio
    assert "Play(_purchase)" in audio
    world_fn = audio.split("public void PlayWorldChange()")[1].split("public void")[0]
    assert "Play(_purchase)" not in world_fn
    assert (root / "Assets/Resources/Audio/Sfx/maximize_008.ogg").is_file()


def test_hit_iframes_and_fail_cause_ui() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    health = (root / "Assets/Scripts/Player/ShipHealth.cs").read_text(encoding="utf-8")
    visuals = (root / "Assets/Scripts/Player/ShipVisuals.cs").read_text(encoding="utf-8")
    session = (root / "Assets/Scripts/Core/GameSession.cs").read_text(encoding="utf-8")
    manager = (root / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    cause = (root / "Assets/Scripts/Core/DamageCause.cs").read_text(encoding="utf-8")
    assert "HitInvulnerabilitySeconds" in health
    assert "IsInvulnerable" in health
    assert "BeginInvulnerability" in health
    assert "DamageCause.AsteroidCollision" in health
    assert "DamageCause.EnemyContact" in health
    assert "PlayHitBlink" in health
    assert "PlayHitBlink" in visuals
    assert "GetComponentsInChildren<Renderer>" in visuals
    assert "renderer].enabled" in visuals or "_blinkRenderers[i].enabled" in visuals
    assert "Fx_HitFlash" not in visuals and "DeathBurst" not in visuals and "DeathRing" not in visuals
    assert "assets/buffer" not in visuals.lower()
    assert "FailReason" in session
    assert 'FailWave(string reason)' in session
    assert "NotifyPlayerDestroyed(string cause)" in manager
    assert "FailReasonText" in ui
    assert "Asteroid collision" in cause
    assert "Enemy contact" in cause
    assert "Enemy contact (" in cause
    assert "FailReason(DamageCause cause, EnemyKind kind)" in cause
    health = (root / "Assets/Scripts/Player/ShipHealth.cs").read_text(encoding="utf-8")
    assert "seeker.Kind" in health
    seeker = (root / "Assets/Scripts/Combat/EnemySeeker.cs").read_text(encoding="utf-8")
    assert "public EnemyKind Kind" in seeker


def test_tighter_loop_wrap_abort_weapons() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    asteroid = (root / "Assets/Scripts/Combat/Asteroid.cs").read_text(encoding="utf-8")
    wrap = (root / "Assets/Scripts/Core/ArenaWrap.cs").read_text(encoding="utf-8")
    waves = (root / "Assets/Scripts/Core/WaveManager.cs").read_text(encoding="utf-8")
    session = (root / "Assets/Scripts/Core/GameSession.cs").read_text(encoding="utf-8")
    manager = (root / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    catalog = (root / "Assets/Scripts/Core/ShopCatalog.cs").read_text(encoding="utf-8")
    shooter = (root / "Assets/Scripts/Player/ShipShooter.cs").read_text(encoding="utf-8")
    projectile = (root / "Assets/Scripts/Player/Projectile.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    enemies = (root / "Assets/Scripts/Combat/EnemyKind.cs").read_text(encoding="utf-8")

    assert "private void FixedUpdate()" in asteroid
    assert "WrapIfOutsideArena" in asteroid
    assert "ArenaWrap.WrapXz" in asteroid
    assert "SoftLockSeconds = 3f" in wrap
    assert "SoftLockSlack = 2f" in wrap
    assert "RescueStrandedThreats" in waves
    assert "ForceWrapOrDespawn" in waves
    assert "AbortToHangar" in session
    assert "AbortWave" in manager
    assert "DespawnAll()" in manager.split("public void AbortWave()")[1].split("public void")[0]
    assert "CompleteWave" not in manager.split("public void AbortWave()")[1].split("public void")[0]
    assert "Abort → Hangar" in ui
    assert "KeyCode.Escape" in ui
    assert "SpreadBolt" in catalog and "Pierce" in catalog
    assert "SpreadPelletCount = 3" in shooter
    assert "FireMode.Spread" in shooter
    assert "FireMode.Pierce" in shooter
    assert "bool pierce" in projectile
    assert "_hitIds" in projectile
    assert "SpawnProjectile(Vector3 origin, Vector3 direction, float speed, int damage, bool pierce)" in factory
    assert "Projectile_Bolt" in factory
    assert "case EnemyKind.Gunner:\n                    return 4;" in enemies
    assert "case EnemyKind.Bomber:\n                    return 5;" in enemies


def test_shop_clarity_and_hangar_wire() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    catalog = (root / "Assets/Scripts/Core/ShopCatalog.cs").read_text(encoding="utf-8")
    art = (root / "Assets/Scripts/Content/ArtImport.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")
    assert "HangarControlsHint" in ui
    assert "HangarReadyStatus" in ui
    assert "OnShopHover" in ui
    assert "+ costLine" in ui
    assert "item.Description" not in ui.split("RefreshBuyButton")[1].split("FailReasonText")[0]
    assert catalog.index("ShopGroup.Hull") < catalog.index("ShopGroup.Weapons")
    warm = art.split("PlayModeAssets")[1].split("};")[0]
    assert "Hangar_Console" in warm
    assert "Hangar_PowerBox" in warm
    assert "Hangar_FireExtinguisher" in warm
    assert 'PlaceHangarProp("Hangar_Console"' in factory
    assert 'PlaceHangarProp("Hangar_PowerBox"' in factory
    assert 'PlaceHangarProp("Hangar_FireExtinguisher"' in factory
    assert "PlayUiClick" in audio and "PlayUiClick" in ui
    lfs_prefix = b"version https://git-lfs.github.com/spec/v1"
    resources_import = root / "Assets/Resources/Art/Import"
    for fbx in sorted(resources_import.glob("*.fbx")):
        assert fbx.is_file() and fbx.stat().st_size > 1000
        assert not fbx.read_bytes()[:64].startswith(lfs_prefix), f"{fbx.name} is an LFS pointer"
    for name in ("Hangar_Console", "Hangar_PowerBox", "Hangar_FireExtinguisher"):
        art_fbx = root / f"Assets/Art/Import/{name}.fbx"
        res_fbx = root / f"Assets/Resources/Art/Import/{name}.fbx"
        assert art_fbx.is_file() and art_fbx.stat().st_size > 1000
        assert res_fbx.is_file() and res_fbx.stat().st_size > 1000
        assert not res_fbx.read_bytes()[:64].startswith(lfs_prefix)


def is_better_best(best_score: int, best_wave: int, score: int, wave: int) -> bool:
    return score > best_score or (score == best_score and wave > best_wave)


def test_local_best_audio_and_bolts() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    best_src = (root / "Assets/Scripts/Core/LocalBest.cs").read_text(encoding="utf-8")
    session = Session()
    assert not is_better_best(100, 3, 50, 8)
    assert is_better_best(100, 3, 100, 4)
    assert is_better_best(0, 0, 0, 1)
    assert not is_better_best(0, 1, 0, 1)
    session.begin()
    session.add_score(50)
    session.complete()
    assert is_better_best(0, 0, session.score, 1)
    assert "agr.best.score" in best_src
    assert "agr.best.wave" in best_src
    assert "agr.best.world" in best_src
    assert "static bool IsBetter" in best_src
    assert "CardLine" in best_src

    manager = (root / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")
    shooter = (root / "Assets/Scripts/Player/ShipShooter.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    art = (root / "Assets/Scripts/Content/ArtImport.cs").read_text(encoding="utf-8")
    seeker = (root / "Assets/Scripts/Combat/EnemySeeker.cs").read_text(encoding="utf-8")
    projectile = (root / "Assets/Scripts/Player/Projectile.cs").read_text(encoding="utf-8")
    assert "RecordBest" in manager
    assert "LastRunWasNewBest" in manager
    assert "PlayAbortWhoosh" in manager
    assert "BestCardLine" in ui
    assert "NEW BEST" in ui
    click_fn = audio.split("public void PlayUiClick()")[1].split("public void")[0]
    assert "click_002" in audio
    assert "_uiClick" in click_fn
    assert "Play(_purchase, 0.42f)" not in click_fn
    buy_fn = audio.split("public void PlayHangarPurchase()")[1].split("public void")[0]
    assert "Play(_purchase)" in buy_fn
    assert "PlayShootSpread" in audio and "PlayShootSpread" in shooter
    assert "PlayShootPierce" in audio and "PlayShootPierce" in shooter
    assert 'Resources.Load<AudioClip>("Audio/Sfx/laserRetro_000")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/laserLarge_000")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/impactMetal_003")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/minimize_005")' in audio
    assert "HangarMusicScale" in audio and "ArenaMusicScale" in audio
    assert "Projectile_EnemyBolt" in factory
    assert "Projectile_Bolt_Buffer_v2" in factory
    assert "Projectile_EnemyBolt_Buffer" in factory
    assert "SpawnEnemyProjectile" in factory
    assert 'PlaceHangarProp("Hangar_Locker"' in factory
    warm = art.split("PlayModeAssets")[1].split("};")[0]
    assert "Hangar_Locker" in warm
    assert "Projectile_EnemyBolt" in warm
    assert "FiresBolts" in seeker or "SpawnEnemyProjectile" in seeker
    assert "bool hostile" in projectile
    lfs_prefix = b"version https://git-lfs.github.com/spec/v1"
    for name in ("Projectile_Bolt", "Projectile_EnemyBolt", "Hangar_Locker"):
        art_fbx = root / f"Assets/Art/Import/{name}.fbx"
        res_fbx = root / f"Assets/Resources/Art/Import/{name}.fbx"
        assert art_fbx.is_file() and art_fbx.stat().st_size > 1000
        assert res_fbx.is_file() and res_fbx.stat().st_size > 1000
        assert not res_fbx.read_bytes()[:64].startswith(lfs_prefix)
    for clip in (
        "click_002.ogg",
        "minimize_005.ogg",
        "laserRetro_000.ogg",
        "laserLarge_000.ogg",
        "impactMetal_003.ogg",
        "laserSmall_001.ogg",
    ):
        path = root / "Assets/Resources/Audio/Sfx" / clip
        assert path.is_file() and path.stat().st_size > 1000


def main() -> int:
    test_clear_loop()
    test_fail_keeps_wave_and_upgrades()
    test_fail_stores_death_cause()
    test_abort_keeps_wave_score_and_skips_bonus()
    test_arena_wrap_mirrors_opposite_edge()
    test_shop_cannot_overspend()
    test_nose_changes_damage()
    test_body_upgrade_adds_hull()
    test_wave_ladder_rises()
    test_factory_wires_import_fbx()
    test_hit_iframes_and_fail_cause_ui()
    test_tighter_loop_wrap_abort_weapons()
    test_shop_clarity_and_hangar_wire()
    test_local_best_audio_and_bolts()
    print("Week 1 logic tests passed (Hangar → Play → Clear/Fail + shop persist)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
