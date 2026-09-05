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
        self.last_resolved_wave = 0
        self.last_credits_awarded = 0
        self.last_run_score = 0

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
        self.last_resolved_wave = self.wave
        self.last_credits_awarded = credits
        self.score += bonus
        self.credits += credits
        self.last_run_score = self.score
        self.wave += 1
        self.phase = Phase.WAVE_CLEAR

    def fail(self, reason: str | None = None) -> None:
        assert self.phase == Phase.PLAYING
        self.fail_reason = reason or "Unknown cause"
        self.last_resolved_wave = self.wave
        self.last_credits_awarded = 0
        self.last_run_score = self.score
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
    assert s.last_resolved_wave == 1
    assert s.last_credits_awarded == 150
    assert s.last_run_score == 185
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
    assert "Hangar_LaunchSign" in factory
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
    assert "HangarMusicScale = 0.48f" in audio
    assert "ArenaMusicScale = 0.82f" in audio
    assert "HangarMusicPitch = 0.94f" in audio
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
    assert "Projectile_Bolt_Buffer_v2" in art
    assert "Projectile_EnemyBolt_Buffer" in art
    assert art.index('"Projectile_Bolt"') < art.index("Projectile_Bolt_Buffer_v2")
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


def test_juice_best_hud() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    session = (root / "Assets/Scripts/Core/GameSession.cs").read_text(encoding="utf-8")
    summary = (root / "Assets/Scripts/Core/RunSummary.cs").read_text(encoding="utf-8")
    juice = (root / "Assets/Scripts/Combat/CombatJuice.cs").read_text(encoding="utf-8")
    flash = (root / "Assets/Scripts/Combat/MeshHitFlash.cs").read_text(encoding="utf-8")
    camera = (root / "Assets/Scripts/Player/FollowCamera.cs").read_text(encoding="utf-8")
    health = (root / "Assets/Scripts/Player/ShipHealth.cs").read_text(encoding="utf-8")
    visuals = (root / "Assets/Scripts/Player/ShipVisuals.cs").read_text(encoding="utf-8")
    seeker = (root / "Assets/Scripts/Combat/EnemySeeker.cs").read_text(encoding="utf-8")
    asteroid = (root / "Assets/Scripts/Combat/Asteroid.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    art = (root / "Assets/Scripts/Content/ArtImport.cs").read_text(encoding="utf-8")
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")
    enemies = (root / "Assets/Scripts/Combat/EnemyKind.cs").read_text(encoding="utf-8")

    assert "LastResolvedWave" in session
    assert "LastCreditsAwarded" in session
    assert "RunSummaryCard" in ui
    assert "RefreshRunSummary" in ui
    assert "BestCardLine()" in ui.split("private string BuildHud")[1]
    assert "ContinueHint" in ui
    assert "World 2 at wave" in summary
    assert "ShowAfterWave1Hint" in summary
    assert "CreditsLine" in summary and "UpgradesLine" in summary
    assert "WAVE CLEAR" in summary and "SHIP LOST" in summary

    s = Session()
    s.begin()
    s.add_score(85)
    s.complete()
    assert s.last_resolved_wave == 1
    assert s.last_credits_awarded == 150
    assert RunSummary_show_after_wave1(s.last_resolved_wave, s.phase)
    assert not RunSummary_show_after_wave1(2, Phase.WAVE_CLEAR)
    assert "Score 185  ·  Wave 1  ·  World 1" == run_stats_line(185, 1, 1)
    assert "Credits 150  (+150)" == run_credits_line(150, 150)

    assert "PlayerDamaged" in juice
    assert "ThreatDamaged" in juice
    assert "AddShake" in camera
    assert "FlashHit" in ui
    assert "ScreenFlash" in ui
    assert "MaterialPropertyBlock" in flash
    assert "public static void Play" in flash
    assert "CombatJuice.PlayerDamaged(false)" in health
    assert "CombatJuice.PlayerDamaged(true)" in health
    assert "CombatJuice.ThreatDamaged(transform, false)" in seeker
    assert "CombatJuice.ThreatDamaged(transform, true)" in seeker
    assert "CombatJuice.ThreatDamaged(transform, true)" in asteroid
    assert "Fx_HitFlash" not in visuals and "DeathBurst" not in visuals

    assert "PierceNeedle" in factory
    assert "SpreadCore" in factory
    assert "new Vector3(0.48f, 0.48f, 2.05f)" in factory
    assert "new Vector3(1.65f, 1.65f, 0.48f)" in factory
    assert "TryEnemyVisual" in factory
    assert "Enemy_Scout_Buffer_v4" in art
    assert "Enemy_Gunner_Buffer_v4" in art
    assert 'PlaceHangarProp("Hangar_ShipComplete", "Ship_Complete"' in factory
    warm = art.split("PlayModeAssets")[1].split("};")[0]
    assert "Enemy_Scout" in warm and "Enemy_Gunner" in warm
    assert "Ship_Complete" in warm
    assert "Enemy_Scout_Buffer_v4" in enemies

    assert "DuckMusic" in audio
    assert "PlayAbortWhoosh" in audio
    assert "AbortDuckScale" in audio
    assert "HitPunchScale = 1.22f" in audio
    assert "HangarLayerScale" in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/impactMetal_000")' in audio
    punch = audio.split("public void PlayHit()")[1].split("public void")[0]
    assert "HitPunchScale" in punch
    abort_fn = audio.split("public void PlayAbortWhoosh()")[1].split("public void")[0]
    assert "DuckMusic" in abort_fn

    lfs_prefix = b"version https://git-lfs.github.com/spec/v1"
    for name in ("Enemy_Scout", "Enemy_Gunner", "Ship_Complete"):
        art_fbx = root / f"Assets/Art/Import/{name}.fbx"
        res_fbx = root / f"Assets/Resources/Art/Import/{name}.fbx"
        assert art_fbx.is_file() and art_fbx.stat().st_size > 1000
        assert res_fbx.is_file() and res_fbx.stat().st_size > 1000
        assert not res_fbx.read_bytes()[:64].startswith(lfs_prefix)
        assert art_fbx.read_bytes() == res_fbx.read_bytes()


def run_stats_line(score: int, wave: int, world: int) -> str:
    return f"Score {score}  ·  Wave {wave}  ·  World {world}"


def run_credits_line(credits: int, awarded: int) -> str:
    if awarded > 0:
        return f"Credits {credits}  (+{awarded})"
    return f"Credits {credits}"


def RunSummary_show_after_wave1(last_resolved_wave: int, phase: str) -> bool:
    return last_resolved_wave == 1 and phase == Phase.WAVE_CLEAR


def RunSummary_show_continue(last_resolved_wave: int, phase: str) -> bool:
    return phase == Phase.WAVE_CLEAR and 1 <= last_resolved_wave <= 3


def is_better_best_world(
    best_score: int, best_wave: int, best_world: int, score: int, wave: int, world: int
) -> bool:
    if score != best_score:
        return score > best_score
    if wave != best_wave:
        return wave > best_wave
    return world > best_world


def test_enemies_launch_034() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    best_src = (root / "Assets/Scripts/Core/LocalBest.cs").read_text(encoding="utf-8")
    summary = (root / "Assets/Scripts/Core/RunSummary.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    art = (root / "Assets/Scripts/Content/ArtImport.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    enemies = (root / "Assets/Scripts/Combat/EnemyKind.cs").read_text(encoding="utf-8")
    waves = (root / "Assets/Scripts/Core/WaveManager.cs").read_text(encoding="utf-8")
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")

    assert "PlayCompare" in best_src
    assert "int bestWorld" in best_src
    assert is_better_best(100, 3, 100, 4)
    assert not is_better_best(100, 3, 100, 3)
    assert is_better_best_world(100, 3, 1, 100, 3, 2)
    assert not is_better_best_world(100, 3, 2, 100, 3, 1)
    assert "PlayBestCompare" in ui
    hud = ui.split("private string BuildHud")[1].split("private static Font")[0]
    assert "PlayBestCompare()" in hud
    assert '_audioPanel.SetActive(!playing)' in ui

    assert "ShowContinueHint" in summary
    assert RunSummary_show_continue(1, Phase.WAVE_CLEAR)
    assert RunSummary_show_continue(2, Phase.WAVE_CLEAR)
    assert RunSummary_show_continue(3, Phase.WAVE_CLEAR)
    assert not RunSummary_show_continue(4, Phase.WAVE_CLEAR)
    assert not RunSummary_show_continue(2, Phase.FAILED)
    assert "Gunner at wave 4" in summary
    assert "before Gunner" in summary
    assert "RunSummary.ContinueHint(" in ui
    assert "RunSummary.ShowContinueHint(" in ui

    warm = art.split("PlayModeAssets")[1].split("};")[0]
    assert "Enemy_Bomber" in warm and "Enemy_Sniper" in warm
    assert "Hangar_LaunchSign" in warm
    assert "CandidateNames" in art
    assert "Enemy_Bomber_Buffer_v5" in art
    assert "Enemy_Sniper_Buffer_v5" in art
    assert art.index('"Enemy_Bomber"') < art.index("Enemy_Bomber_Buffer_v5")
    assert art.index('"Enemy_Sniper"') < art.index("Enemy_Sniper_Buffer_v5")
    assert "Enemy_Scout_Buffer_v4" not in factory
    assert "Enemy_Gunner_Buffer_v4" not in factory
    assert "Projectile_Bolt_Buffer_v2" not in factory
    assert "EnemyKind.Bomber" in waves and "EnemyKind.Sniper" in waves
    assert "EnemyCatalog.VisualName" in waves
    assert 'return "Enemy_Bomber"' in enemies
    assert 'return "Enemy_Sniper"' in enemies
    assert "Enemy_Bomber_Buffer_v5" in enemies
    assert "Enemy_Sniper_Buffer_v5" in enemies
    assert 'PlaceHangarProp("Hangar_LaunchSign"' in factory
    assert "Hangar_LaunchSign" in factory

    death = audio.split("public void PlayEnemyDeath()")[1].split("public void")[0]
    split = audio.split("public void PlayAsteroidSplit()")[1].split("public void")[0]
    assert "Play(_enemyDeath)" in death
    assert "EnemyDeathPunchScale" in death
    assert "_enemyDeathPunch" in death
    assert "Play(_asteroidSplit)" in split
    assert "_enemyDeathPunch" not in split
    assert 'Resources.Load<AudioClip>("Audio/Sfx/explosionCrunch_000")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/explosionCrunch_003")' in audio
    assert "DuckMusic" in audio and "AbortDuckScale" in audio
    assert "HitPunchScale = 1.22f" in audio
    punch = audio.split("public void PlayHit()")[1].split("public void")[0]
    assert "HitPunchScale" in punch
    abort_fn = audio.split("public void PlayAbortWhoosh()")[1].split("public void")[0]
    assert "DuckMusic" in abort_fn

    lfs_prefix = b"version https://git-lfs.github.com/spec/v1"
    for name in ("Enemy_Bomber", "Enemy_Sniper", "Hangar_LaunchSign"):
        art_fbx = root / f"Assets/Art/Import/{name}.fbx"
        res_fbx = root / f"Assets/Resources/Art/Import/{name}.fbx"
        assert art_fbx.is_file() and art_fbx.stat().st_size > 1000
        assert res_fbx.is_file() and res_fbx.stat().st_size > 1000
        assert not res_fbx.read_bytes()[:64].startswith(lfs_prefix)
        assert art_fbx.read_bytes() == res_fbx.read_bytes()


def continue_hint(last_resolved_wave: int, next_title: str | None) -> str:
    if last_resolved_wave == 3:
        return f"Buy {next_title} before Gunner" if next_title else "Push for a new best before Gunner"
    landmark = "Gunner at wave 4" if last_resolved_wave == 2 else "World 2 at wave 6"
    buy = f"Buy {next_title}" if next_title else "Push for a new best."
    return landmark + "  ·  " + buy


def wave_medal(last_resolved_wave: int, phase: str) -> str:
    if phase != Phase.WAVE_CLEAR or last_resolved_wave != 3:
        return ""
    return "★ Scout Wing  ·  World 2 at wave 6"


def test_scout_drone_polish_0341() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    summary = (root / "Assets/Scripts/Core/RunSummary.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    art = (root / "Assets/Scripts/Content/ArtImport.cs").read_text(encoding="utf-8")
    enemies = (root / "Assets/Scripts/Combat/EnemyKind.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")
    seeker = (root / "Assets/Scripts/Combat/EnemySeeker.cs").read_text(encoding="utf-8")

    assert 'Buy " + next.Title + " before Gunner' in summary
    assert continue_hint(3, "Shield Cell") == "Buy Shield Cell before Gunner"
    assert continue_hint(3, None) == "Push for a new best before Gunner"
    assert continue_hint(2, "Body Upgrade") == "Gunner at wave 4  ·  Buy Body Upgrade"
    assert "ShowWaveMedal" in summary
    assert "WaveMedal" in ui
    assert wave_medal(3, Phase.WAVE_CLEAR) == "★ Scout Wing  ·  World 2 at wave 6"
    assert wave_medal(2, Phase.WAVE_CLEAR) == ""
    assert wave_medal(3, Phase.FAILED) == ""
    assert "Scout Wing" in summary
    assert "RunSummary.ShowWaveMedal(" in ui
    assert "RunSummary.WaveMedal(" in ui

    assert "Enemy_Scout_Buffer_v5" in art
    assert "Enemy_Gunner_Buffer_v5" in art
    assert "Enemy_Drone_Buffer_v4" in art
    assert art.index('"Enemy_Scout"') < art.index("Enemy_Scout_Buffer_v5")
    assert art.index("Enemy_Scout_Buffer_v5") < art.index("Enemy_Scout_Buffer_v4")
    assert art.index('"Enemy_Gunner"') < art.index("Enemy_Gunner_Buffer_v5")
    assert art.index('"Enemy_Drone"') < art.index("Enemy_Drone_Buffer_v4")
    assert "Enemy_Scout_Buffer_v5" in enemies
    assert "Enemy_Gunner_Buffer_v5" in enemies
    assert "Enemy_Drone_Buffer_v4" in enemies
    assert "Enemy_Scout_Buffer_v5" not in factory
    assert "Enemy_Gunner_Buffer_v5" not in factory
    assert "Enemy_Drone_Buffer_v4" not in factory
    assert 'EnemyCatalog.VisualName' in (root / "Assets/Scripts/Core/WaveManager.cs").read_text(
        encoding="utf-8"
    )

    assert 'new Vector3(1.95f, 0f, -2.55f)' in factory
    assert "198f" in factory.split('PlaceHangarProp("Hangar_LaunchSign"')[1].split(";")[0]

    death_kind = audio.split("public void PlayEnemyDeath(EnemyKind kind)")[1].split("public ")[0]
    hit_kind = audio.split("public void PlayHit(EnemyKind kind)")[1].split("public ")[0]
    death = audio.split("public void PlayEnemyDeath()")[1].split("public void")[0]
    split = audio.split("public void PlayAsteroidSplit()")[1].split("public void")[0]
    punch = audio.split("public void PlayHit()")[1].split("public void")[0]
    abort_fn = audio.split("public void PlayAbortWhoosh()")[1].split("public void")[0]
    assert "UsesLightThreatSfx" in death_kind
    assert "UsesLightThreatSfx" in hit_kind
    assert "_enemyDeathLight" in death_kind
    assert "_hitLight" in hit_kind
    assert "Play(_enemyDeath)" in death
    assert "EnemyDeathPunchScale" in death
    assert "Play(_asteroidSplit)" in split
    assert "_enemyDeathPunch" not in split
    assert "HitPunchScale" in punch
    assert "DuckMusic" in abort_fn
    assert "PlayEnemyDeath(_kind)" in seeker
    assert "PlayHit(_kind)" in seeker
    assert 'Resources.Load<AudioClip>("Audio/Sfx/explosionCrunch_001")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/impactMetal_001")' in audio
    assert (root / "Assets/Resources/Audio/Sfx/explosionCrunch_001.ogg").is_file()
    assert (root / "Assets/Resources/Audio/Sfx/impactMetal_001.ogg").is_file()

    lfs_prefix = b"version https://git-lfs.github.com/spec/v1"
    for name in ("Enemy_Scout", "Enemy_Gunner", "Enemy_Drone"):
        art_fbx = root / f"Assets/Art/Import/{name}.fbx"
        res_fbx = root / f"Assets/Resources/Art/Import/{name}.fbx"
        assert art_fbx.is_file() and art_fbx.stat().st_size > 1000
        assert res_fbx.is_file() and res_fbx.stat().st_size > 1000
        assert not res_fbx.read_bytes()[:64].startswith(lfs_prefix)
        assert art_fbx.read_bytes() == res_fbx.read_bytes()


def medal_badge_row(mask: int) -> str:
    parts: list[str] = []
    if mask & 1:
        parts.append("★ Scout Wing")
    if mask & 2:
        parts.append("★ Deep Orbit")
    return "  ·  ".join(parts)


def try_award_mask(mask: int, bit: int) -> tuple[bool, int]:
    nxt = mask | bit
    return nxt != mask, nxt


def test_medals_swarm_035() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    persist = (root / "Assets/Scripts/Core/HangarPersist.cs").read_text(encoding="utf-8")
    medals = (root / "Assets/Scripts/Core/MedalCatalog.cs").read_text(encoding="utf-8")
    summary = (root / "Assets/Scripts/Core/RunSummary.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    manager = (root / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    art = (root / "Assets/Scripts/Content/ArtImport.cs").read_text(encoding="utf-8")
    enemies = (root / "Assets/Scripts/Combat/EnemyKind.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")
    sign = (root / "Assets/Scripts/Content/HangarSignPulse.cs").read_text(encoding="utf-8")

    assert "agr.hangar.medals" in persist
    assert "class HangarPersist" in persist
    assert "TryAward" in persist
    assert "BadgeRow" in persist
    assert "MedalId.ScoutWing" in medals
    assert "MedalId.DeepOrbit" in medals
    assert 'DeepOrbitTitle = "Deep Orbit"' in medals
    assert "TryForClearedWave" in medals
    assert "TryForWorldEntry" in medals
    assert "WorldEntryBeat" in medals
    assert medal_badge_row(0) == ""
    assert medal_badge_row(1) == "★ Scout Wing"
    assert medal_badge_row(2) == "★ Deep Orbit"
    assert medal_badge_row(3) == "★ Scout Wing  ·  ★ Deep Orbit"
    awarded, mask = try_award_mask(0, 1)
    assert awarded and mask == 1
    awarded, mask = try_award_mask(1, 1)
    assert not awarded and mask == 1
    awarded, mask = try_award_mask(1, 2)
    assert awarded and mask == 3
    assert "ShowWaveMedal" in summary
    assert wave_medal(3, Phase.WAVE_CLEAR) == "★ Scout Wing  ·  World 2 at wave 6"
    assert "BadgeRow" in ui
    assert "AnnounceMedalBeat" in ui
    assert "RefreshBadgeRow" in ui
    assert "TryAwardWaveMedal" in manager
    assert "TryAwardWorldMedal" in manager
    assert "HangarPersist.Load" in manager
    assert "AnnounceMedalBeat" in manager

    assert "Enemy_01_Buffer_v8" in art
    assert "Enemy_SwarmPod_Buffer_v6" in art
    assert "Enemy_Bomber_Buffer_v6" in art
    assert "Ship_Complete_Buffer_v4" in art
    assert art.index('"Enemy_01"') < art.index("Enemy_01_Buffer_v8")
    assert art.index('"Enemy_SwarmPod"') < art.index("Enemy_SwarmPod_Buffer_v6")
    assert art.index('"Enemy_Bomber"') < art.index("Enemy_Bomber_Buffer_v6")
    assert art.index("Enemy_Bomber_Buffer_v6") < art.index("Enemy_Bomber_Buffer_v5")
    assert art.index('"Ship_Complete"') < art.index("Ship_Complete_Buffer_v4")
    assert "Enemy_01_Buffer_v8" in enemies
    assert "Enemy_SwarmPod_Buffer_v6" in enemies
    assert "Enemy_Bomber_Buffer_v6" in enemies
    assert "Enemy_01_Buffer_v8" not in factory
    assert "DressMidMesh" in factory
    assert "DressLaunchSign" in factory
    assert "LaunchGoDecal" in factory
    assert 'go.text = "GO"' in factory
    assert "Mat_LaunchSign_Decal" in factory
    assert "HangarSignPulse" in factory
    assert "class HangarSignPulse" in sign

    spawn = audio.split("public void PlaySwarmPodSpawn()")[1].split("public void")[0]
    assert "_swarmPodSpawn" in spawn
    death_kind = audio.split("public void PlayEnemyDeath(EnemyKind kind)")[1].split("public ")[0]
    hit_kind = audio.split("public void PlayHit(EnemyKind kind)")[1].split("public ")[0]
    death = audio.split("public void PlayEnemyDeath()")[1].split("public void")[0]
    split = audio.split("public void PlayAsteroidSplit()")[1].split("public void")[0]
    abort_fn = audio.split("public void PlayAbortWhoosh()")[1].split("public void")[0]
    assert "UsesLightThreatSfx" in death_kind
    assert "UsesLightThreatSfx" in hit_kind
    assert "Play(_enemyDeath)" in death
    assert "EnemyDeathPunchScale" in death
    assert "Play(_asteroidSplit)" in split
    assert "_enemyDeathPunch" not in split
    assert "DuckMusic" in abort_fn
    assert 'Resources.Load<AudioClip>("Audio/Sfx/phaserUp5")' in audio
    assert (root / "Assets/Resources/Audio/Sfx/phaserUp5.ogg").is_file()
    assert (root / "Assets/Resources/Audio/Sfx/phaserUp5.ogg").stat().st_size > 1000

    lfs_prefix = b"version https://git-lfs.github.com/spec/v1"
    for name, min_size in (
        ("Ship_Complete", 150000),
        ("Enemy_SwarmPod", 200000),
        ("Enemy_Bomber", 160000),
        ("Enemy_01", 100000),
    ):
        art_fbx = root / f"Assets/Art/Import/{name}.fbx"
        res_fbx = root / f"Assets/Resources/Art/Import/{name}.fbx"
        assert art_fbx.is_file() and art_fbx.stat().st_size > min_size
        assert res_fbx.is_file() and res_fbx.stat().st_size > min_size
        assert not art_fbx.read_bytes()[:64].startswith(lfs_prefix)
        assert not res_fbx.read_bytes()[:64].startswith(lfs_prefix)
        assert art_fbx.read_bytes() == res_fbx.read_bytes()


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
    test_juice_best_hud()
    test_enemies_launch_034()
    test_scout_drone_polish_0341()
    test_medals_swarm_035()
    print("Week 1 logic tests passed (Hangar → Play → Clear/Fail + shop persist)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
