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
        self.lives = 3
        self.max_lives = 5

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

    def lose_life(self) -> bool:
        if self.phase != Phase.PLAYING or self.lives <= 0:
            return False
        self.lives -= 1
        return self.lives > 0

    def gain_life(self) -> bool:
        if self.lives >= self.max_lives:
            return False
        self.lives += 1
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
    count = min(max(5 + (wave - 1), 5), 7)
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
    assert large_asteroid_count(1) == 5
    assert large_asteroid_count(2) == 6
    assert large_asteroid_count(3) == 7
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
    assert "ArenaMusicScale = 0.65f" in audio
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
    hud = ui.split("private string BuildHud")[1].split("private static void AddReadability")[0]
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
    if mask & 4:
        parts.append("★ Far Drift")
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
    assert medal_badge_row(7) == "★ Scout Wing  ·  ★ Deep Orbit  ·  ★ Far Drift"
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


def test_scout_gunner_medals_036() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    medals = (root / "Assets/Scripts/Core/MedalCatalog.cs").read_text(encoding="utf-8")
    persist = (root / "Assets/Scripts/Core/HangarPersist.cs").read_text(encoding="utf-8")
    summary = (root / "Assets/Scripts/Core/RunSummary.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    manager = (root / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    art = (root / "Assets/Scripts/Content/ArtImport.cs").read_text(encoding="utf-8")
    enemies = (root / "Assets/Scripts/Combat/EnemyKind.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")

    assert "MedalId.FarDrift" in medals
    assert 'FarDriftTitle = "Far Drift"' in medals
    assert "FarDriftClearsAtWave = 10" in medals
    assert "World3EntryWorld = 3" in medals
    assert "BadgeCapacity = 3" in medals
    assert "MedalId.FarDrift" in persist or "Far Drift" in persist
    assert medal_badge_row(4) == "★ Far Drift"
    assert medal_badge_row(7) == "★ Scout Wing  ·  ★ Deep Orbit  ·  ★ Far Drift"
    awarded, mask = try_award_mask(3, 4)
    assert awarded and mask == 7
    awarded, mask = try_award_mask(7, 4)
    assert not awarded and mask == 7

    assert "DeepOrbitTeaser" in summary
    assert "World3StartsAtWave" in summary
    assert "lastResolvedWave <= 9" in summary
    assert "DeepOrbitTitle" in summary
    assert wave_medal(3, Phase.WAVE_CLEAR) == "★ Scout Wing  ·  World 2 at wave 6"
    assert "ShowWaveMedal" in summary
    assert "World 3 at wave" in summary
    assert "MedalId.FarDrift" in summary
    assert "World 3" in summary
    assert "RunSummary.ShowWaveMedal(" in ui
    assert "RunSummary.WaveMedal(" in ui
    assert "0.72f" in ui
    assert "AnnounceMedalBeat" in manager
    assert "TryAwardWaveMedal" in manager
    assert "TryAwardWorldMedal" in manager

    assert "Enemy_Scout_Buffer_v6" in art
    assert "Enemy_Gunner_Buffer_v6" in art
    assert "Enemy_Drone_Buffer_v5" in art
    assert art.index('"Enemy_Scout"') < art.index("Enemy_Scout_Buffer_v6")
    assert art.index("Enemy_Scout_Buffer_v6") < art.index("Enemy_Scout_Buffer_v5")
    assert art.index('"Enemy_Gunner"') < art.index("Enemy_Gunner_Buffer_v6")
    assert art.index("Enemy_Gunner_Buffer_v6") < art.index("Enemy_Gunner_Buffer_v5")
    assert art.index('"Enemy_Drone"') < art.index("Enemy_Drone_Buffer_v5")
    assert art.index("Enemy_Drone_Buffer_v5") < art.index("Enemy_Drone_Buffer_v4")
    assert "Enemy_Scout_Buffer_v6" in enemies
    assert "Enemy_Gunner_Buffer_v6" in enemies
    assert "Enemy_Drone_Buffer_v5" in enemies
    assert "Enemy_Scout_Buffer_v6" not in factory
    assert "Enemy_Gunner_Buffer_v6" not in factory
    assert "Enemy_Drone_Buffer_v5" not in factory
    assert 'EnemyCatalog.VisualName' in (root / "Assets/Scripts/Core/WaveManager.cs").read_text(
        encoding="utf-8"
    )

    assert "LaunchGoMeshDecal" in factory
    assert "DressLaunchGoMeshDecal" in factory
    assert 'go.text = "GO"' in factory
    assert "Mat_LaunchSign_Decal" in factory

    spawn = audio.split("public void PlaySwarmPodSpawn()")[1].split("public void")[0]
    assert "_swarmPodSpawn" in spawn
    assert "SwarmPodSpawnScale" in spawn
    assert "DuckMusic" in spawn
    assert "SwarmPodSpawnGapSeconds" in spawn
    assert 'Resources.Load<AudioClip>("Audio/Sfx/phaserUp5")' in audio
    death_kind = audio.split("public void PlayEnemyDeath(EnemyKind kind)")[1].split("public ")[0]
    hit_kind = audio.split("public void PlayHit(EnemyKind kind)")[1].split("public ")[0]
    assert "UsesLightThreatSfx" in death_kind
    assert "UsesLightThreatSfx" in hit_kind

    lfs_prefix = b"version https://git-lfs.github.com/spec/v1"
    for name, min_size in (
        ("Enemy_Scout", 140000),
        ("Enemy_Gunner", 170000),
        ("Enemy_Drone", 200000),
    ):
        art_fbx = root / f"Assets/Art/Import/{name}.fbx"
        res_fbx = root / f"Assets/Resources/Art/Import/{name}.fbx"
        assert art_fbx.is_file() and art_fbx.stat().st_size > min_size
        assert res_fbx.is_file() and res_fbx.stat().st_size > min_size
        assert not art_fbx.read_bytes()[:64].startswith(lfs_prefix)
        assert not res_fbx.read_bytes()[:64].startswith(lfs_prefix)
        assert art_fbx.read_bytes() == res_fbx.read_bytes()


def medal_ladder_line(mask: int) -> str:
    parts: list[str] = []
    for bit, title in ((1, "Scout Wing"), (2, "Deep Orbit"), (4, "Far Drift")):
        parts.append(("★ " if mask & bit else "○ ") + title)
    return "  ·  ".join(parts)


def far_drift_teaser(last_resolved_wave: int) -> str:
    if last_resolved_wave == 9:
        return "Clear wave 10  ·  ★ Far Drift"
    return "★ Far Drift at wave 10"


def test_sniper_fardrift_037() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    medals = (root / "Assets/Scripts/Core/MedalCatalog.cs").read_text(encoding="utf-8")
    persist = (root / "Assets/Scripts/Core/HangarPersist.cs").read_text(encoding="utf-8")
    summary = (root / "Assets/Scripts/Core/RunSummary.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    manager = (root / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    art = (root / "Assets/Scripts/Content/ArtImport.cs").read_text(encoding="utf-8")
    enemies = (root / "Assets/Scripts/Combat/EnemyKind.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")
    waves = (root / "Assets/Scripts/Core/WaveManager.cs").read_text(encoding="utf-8")

    world_entry = medals.split("public static bool TryForWorldEntry")[1].split("public static")[0]
    assert "MedalId.DeepOrbit" in world_entry
    assert "MedalId.FarDrift" not in world_entry
    assert "LadderLine" in medals
    assert 'LockedLine' in medals
    assert medal_ladder_line(0) == "○ Scout Wing  ·  ○ Deep Orbit  ·  ○ Far Drift"
    assert medal_ladder_line(1) == "★ Scout Wing  ·  ○ Deep Orbit  ·  ○ Far Drift"
    assert medal_ladder_line(7) == "★ Scout Wing  ·  ★ Deep Orbit  ·  ★ Far Drift"
    assert "LadderLine" in persist
    assert persist.count("LadderLine") >= 1

    assert "FarDriftTeaser" in summary
    assert "lastResolvedWave <= 9" in summary
    assert far_drift_teaser(8) == "★ Far Drift at wave 10"
    assert far_drift_teaser(9) == "Clear wave 10  ·  ★ Far Drift"
    assert "World 3 at wave" in summary
    assert 'AwardLine(MedalId.FarDrift) + "  ·  World 3"' not in summary
    assert "ScoutWingTitle" in summary
    assert "RunSummary.ShowContinueHint(" in ui
    assert "persist.LadderLine()" in ui
    assert "playing ? 14 : 16" in ui
    complete = manager.split("private void CompleteWave()")[1].split("private bool TryAwardWorldMedal")[0]
    assert "PlayFarDriftAward" in complete
    assert "PlayWaveClear" in complete
    assert "MedalId.FarDrift" in complete

    assert "Enemy_Scout_Buffer_v7" in art
    assert "Enemy_Gunner_Buffer_v7" in art
    assert "Enemy_Drone_Buffer_v6" in art
    assert "Enemy_Sniper_Buffer_v8" in art
    assert art.index('"Enemy_Scout"') < art.index("Enemy_Scout_Buffer_v7")
    assert art.index("Enemy_Scout_Buffer_v7") < art.index("Enemy_Scout_Buffer_v6")
    assert art.index('"Enemy_Gunner"') < art.index("Enemy_Gunner_Buffer_v7")
    assert art.index("Enemy_Gunner_Buffer_v7") < art.index("Enemy_Gunner_Buffer_v6")
    assert art.index('"Enemy_Drone"') < art.index("Enemy_Drone_Buffer_v6")
    assert art.index("Enemy_Drone_Buffer_v6") < art.index("Enemy_Drone_Buffer_v5")
    assert art.index('"Enemy_Sniper"') < art.index("Enemy_Sniper_Buffer_v8")
    assert art.index("Enemy_Sniper_Buffer_v8") < art.index("Enemy_Sniper_Buffer_v5")
    assert "Enemy_Scout_Buffer_v7" in enemies
    assert "Enemy_Gunner_Buffer_v7" in enemies
    assert "Enemy_Drone_Buffer_v6" in enemies
    assert "Enemy_Sniper_Buffer_v8" in enemies
    require_mesh = enemies.split("public static bool RequiresImportedMesh")[1].split("public static")[0]
    assert "EnemyKind.Sniper" not in require_mesh
    assert "EnemyKind.Bomber" in require_mesh
    assert "EnemyKind.SwarmPod" in require_mesh
    assert "Enemy_Scout_Buffer_v7" not in factory
    assert "Enemy_Sniper_Buffer_v8" not in factory
    assert "DressSniperMesh" in factory
    assert 'ContainsIgnoreCase(name, "Sniper")' in factory
    assert 'ContainsIgnoreCase(name, "Scout")' in factory
    assert 'EnemyCatalog.VisualName' in waves
    assert "EnemyKind.Sniper" in waves

    assert "PlayFarDriftAward" in audio
    assert "FarDriftAwardScale = 0.94f" in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/jingles_PIZZA16")' in audio
    assert "SwarmPodSpawnScale = 0.86f" in audio
    assert "SwarmPodDuckSeconds = 0.36f" in audio
    assert "SwarmPodDuckScale = 0.38f" in audio
    assert "SwarmPodSpawnGapSeconds = 0.62f" in audio
    spawn = audio.split("public void PlaySwarmPodSpawn()")[1].split("public void")[0]
    assert "SwarmPodSpawnScale" in spawn
    assert "DuckMusic" in spawn
    assert "SwarmPodSpawnGapSeconds" in spawn
    far = audio.split("public void PlayFarDriftAward()")[1].split("public void")[0]
    assert "_farDriftAward" in far
    assert "FarDriftAwardScale" in far

    lfs_prefix = b"version https://git-lfs.github.com/spec/v1"
    for name, min_size in (
        ("Enemy_Scout", 140000),
        ("Enemy_Gunner", 170000),
        ("Enemy_Drone", 200000),
        ("Enemy_Sniper", 140000),
    ):
        art_fbx = root / f"Assets/Art/Import/{name}.fbx"
        res_fbx = root / f"Assets/Resources/Art/Import/{name}.fbx"
        assert art_fbx.is_file() and art_fbx.stat().st_size > min_size
        assert res_fbx.is_file() and res_fbx.stat().st_size > min_size
        assert not art_fbx.read_bytes()[:64].startswith(lfs_prefix)
        assert not res_fbx.read_bytes()[:64].startswith(lfs_prefix)
        assert art_fbx.read_bytes() == res_fbx.read_bytes()

    jingle = root / "Assets/Resources/Audio/Sfx/jingles_PIZZA16.ogg"
    assert jingle.is_file() and jingle.stat().st_size > 1000
    assert jingle.read_bytes()[:4] == b"OggS"


def test_ui_fonts_039() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    fonts = (root / "Assets/Scripts/UI/UiFonts.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    waves = (root / "Assets/Scripts/Core/WaveManager.cs").read_text(encoding="utf-8")
    camera = (root / "Assets/Scripts/Player/FollowCamera.cs").read_text(encoding="utf-8")
    bootstrap = (root / "Assets/Scripts/Content/GameBootstrap.cs").read_text(encoding="utf-8")
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")
    manifest = (root / "Packages/manifest.json").read_text(encoding="utf-8")
    lock = (root / "Packages/packages-lock.json").read_text(encoding="utf-8")
    readme = (root / "README.md").read_text(encoding="utf-8")
    checklist = (root / "MERGE_CHECKLIST.md").read_text(encoding="utf-8")
    credits = (root / "CREDITS.md").read_text(encoding="utf-8")

    assert "class UiFonts" in fonts
    assert 'DisplayResource = "Fonts/KenneyFuture"' in fonts
    assert 'BodyResource = "Fonts/KenneyFutureNarrow"' in fonts
    assert 'LegacyBuiltin = "LegacyRuntime.ttf"' in fonts
    assert "Arial.ttf" not in fonts
    assert "Arial.ttf" not in ui
    assert "Arial.ttf" not in factory
    assert "UiFonts.Display()" in ui
    assert "UiFonts.Body()" in ui
    assert "UiFonts.Display()" in factory
    assert "HudPlate" in ui
    assert "AddReadability" in ui
    assert "PlayUiClick" in ui.split("private void OnMute()")[1].split("private void")[0]

    display = root / "Assets/Resources/Fonts/KenneyFuture.ttf"
    body = root / "Assets/Resources/Fonts/KenneyFutureNarrow.ttf"
    license_txt = root / "Assets/Resources/Fonts/Kenney_Fonts_License.txt"
    assert display.is_file() and display.stat().st_size > 10000
    assert body.is_file() and body.stat().st_size > 10000
    assert license_txt.is_file()
    assert display.read_bytes()[0:4] in (b"\x00\x01\x00\x00", b"OTTO", b"true")
    assert body.read_bytes()[0:4] in (b"\x00\x01\x00\x00", b"OTTO", b"true")

    assert "ArenaRadius = 30f" in waves
    assert "ArenaDesignRadius = 22f" in waves
    assert "ScaledRing" in waves
    assert "WaveManager.ArenaRadius / WaveManager.ArenaDesignRadius" in factory
    assert "new Vector3(0f, 35f, -22f)" in camera
    assert "fieldOfView = 54f" in bootstrap
    assert "farClipPlane = 280f" in bootstrap

    click = audio.split("public void PlayUiClick()")[1].split("public void")[0]
    assert "0.88f" in click
    buy = audio.split("public void PlayHangarPurchase()")[1].split("public void")[0]
    assert "Play(_purchase" in buy

    assert "com.unity.modules.vr" not in manifest
    assert "com.unity.modules.xr" not in manifest
    assert "com.unity.textmeshpro" not in manifest
    assert "com.unity.modules.vr" not in lock
    assert "com.unity.modules.xr" not in lock
    assert "com.unity.textmeshpro" not in lock
    assert "0.39-ui-fonts" in checklist
    assert "no 0.41" in checklist
    assert "Hub-open smoke" in checklist
    assert "Kenney Future" in readme
    assert "Kenney Future" in credits
    assert "CC0" in credits
    assert "GetEntityId" in readme or "GetEntityId" in checklist


def world_entry_beat(world: int) -> str:
    if world == 2:
        return "★ Deep Orbit"
    if world == 3:
        return "New sector"
    return ""


def next_medal_hook(next_wave: int) -> str:
    if next_wave <= 3:
        return "Next  ·  ★ Scout Wing at wave 3"
    if next_wave <= 6:
        return "Next  ·  ★ Deep Orbit at wave 6"
    if next_wave <= 10:
        return "Next  ·  ★ Far Drift at wave 10"
    if next_wave <= 11:
        return "Next  ·  World 3 at wave 11"
    return ""


def test_steam_world3_038() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    medals = (root / "Assets/Scripts/Core/MedalCatalog.cs").read_text(encoding="utf-8")
    persist = (root / "Assets/Scripts/Core/HangarPersist.cs").read_text(encoding="utf-8")
    summary = (root / "Assets/Scripts/Core/RunSummary.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    manager = (root / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")
    art = (root / "Assets/Scripts/Content/ArtImport.cs").read_text(encoding="utf-8")
    manifest = (root / "Packages/manifest.json").read_text(encoding="utf-8")
    readme = (root / "README.md").read_text(encoding="utf-8")
    checklist = (root / "MERGE_CHECKLIST.md").read_text(encoding="utf-8")

    world_entry = medals.split("public static bool TryForWorldEntry")[1].split("public static")[0]
    assert "MedalId.DeepOrbit" in world_entry
    assert "MedalId.FarDrift" not in world_entry
    assert "World3EntryWorld" not in world_entry
    assert 'World3EntryTitle = "New sector"' in medals
    assert "World3HangarLine" in medals
    assert "World3FlashSeconds = 2.15f" in medals
    assert world_entry_beat(2) == "★ Deep Orbit"
    assert world_entry_beat(3) == "New sector"
    assert "★" not in world_entry_beat(3)
    assert world_entry_beat(1) == ""
    assert world_entry_beat(4) == ""
    beat_fn = medals.split("public static string WorldEntryBeat")[1].split("public static")[0]
    assert "World3EntryWorld" in beat_fn
    assert "World3EntryTitle" in beat_fn
    assert "AwardLine(medal)" in beat_fn
    hangar_fn = medals.split("public static string World3HangarLine")[1].split("public static")[0]
    assert "World 3 online" in hangar_fn
    assert "World3EntryTitle" in hangar_fn
    assert "★" not in hangar_fn

    assert "NextMedalHook" in summary
    assert next_medal_hook(1) == "Next  ·  ★ Scout Wing at wave 3"
    assert next_medal_hook(3) == "Next  ·  ★ Scout Wing at wave 3"
    assert next_medal_hook(4) == "Next  ·  ★ Deep Orbit at wave 6"
    assert next_medal_hook(10) == "Next  ·  ★ Far Drift at wave 10"
    assert next_medal_hook(11) == "Next  ·  World 3 at wave 11"
    assert next_medal_hook(12) == ""
    assert "IsWorld3EntryLine" in summary
    assert "World3StartsAtWave" in summary
    assert "World3HangarLine" in summary
    assert 'AwardLine(MedalId.FarDrift) + "  ·  World 3"' not in summary

    assert "Medal ladder (top-left)" in ui
    assert "Scout Wing at wave 3" in ui
    assert "Clear a wave to earn credits and upgrades." in ui
    assert 'MedalLadderPrefix = "MEDALS"' in ui
    assert "NextMedalHook" in ui
    assert "IsWorld3EntryLine" in ui
    assert "World3EntryWorld" in ui
    start = manager.split("public void StartWave()")[1].split("public void ContinueFromResults")[0]
    assert "TryAwardWorldMedal(world)" in start
    assert "WorldEntryBeat(world)" in start
    assert "WorldEntryFlashSeconds(world)" in start
    assert "worldMedal &&" not in start
    assert persist.count("LadderLine") >= 1

    assert "PlayWorldChange(WorldIndexForWave(waveIndex))" in factory
    assert "World3ChangeScale = 1.12f" in audio
    assert "World3DuckSeconds = 0.42f" in audio
    assert "World3DuckScale = 0.4f" in audio
    world_change = audio.split("public void PlayWorldChange(int world)")[1].split("public void")[0]
    assert "World3EntryWorld" in world_change
    assert "World3ChangeScale" in world_change
    assert "DuckMusic" in world_change
    assert "jingles_PIZZA16" not in world_change

    assert "Enemy_Scout_Buffer_v8" not in art
    assert "Enemy_Gunner_Buffer_v8" not in art
    assert "Enemy_Drone_Buffer_v7" not in art
    assert "Enemy_Sniper_Buffer_v9" not in art
    assert "Enemy_Scout_Buffer_v7" in art
    assert "Enemy_Sniper_Buffer_v8" in art
    assert "com.unity.modules.vr" not in manifest
    assert "com.unity.modules.xr" not in manifest
    assert "Hub-open smoke" in readme
    assert "Hub-open smoke" in checklist
    assert "New sector" in readme
    assert "without a medal" in readme.lower() or "no medal" in readme.lower()

    projectile = (root / "Assets/Scripts/Player/Projectile.cs").read_text(encoding="utf-8")
    seeker = (root / "Assets/Scripts/Combat/EnemySeeker.cs").read_text(encoding="utf-8")
    pickup = (root / "Assets/Scripts/Combat/Pickup.cs").read_text(encoding="utf-8")
    bootstrap = (root / "Assets/Scripts/Content/GameBootstrap.cs").read_text(encoding="utf-8")
    fonts = (root / "Assets/Scripts/UI/UiFonts.cs").read_text(encoding="utf-8")
    assert "GetInstanceID" not in projectile
    assert "GetEntityId()" in projectile
    assert "HashSet<EntityId>" in projectile
    assert "FindFirstObjectByType" not in seeker
    assert "FindFirstObjectByType" not in pickup
    assert "FindFirstObjectByType" not in bootstrap
    assert "FindAnyObjectByType<ContentFactory>" in seeker
    assert "FindAnyObjectByType<GameManager>" in pickup
    assert "FindAnyObjectByType<EventSystem>" in bootstrap
    assert "GetComponent<StandaloneInputModule>" in bootstrap
    assert "AddComponent<StandaloneInputModule>" in bootstrap
    assert "FindObjectsSortMode" not in bootstrap
    assert "FindObjectsByType<Light>()" in bootstrap
    assert "Arial.ttf" not in factory
    assert "Arial.ttf" not in ui
    assert "Arial.ttf" not in fonts
    assert 'GetBuiltinResource<Font>(LegacyBuiltin)' in fonts or 'GetBuiltinResource<Font>("LegacyRuntime.ttf")' in fonts
    assert "Fonts/KenneyFuture" in fonts


def arena_layout_for_wave(wave: int) -> int:
    wave = max(1, wave)
    return ((wave - 1) // 5 % 7) + 1


def _load_week1_validator():
    import importlib.util
    from pathlib import Path

    path = Path(__file__).resolve().parent / "validate_week1_project.py"
    spec = importlib.util.spec_from_file_location("validate_week1_project", path)
    module = importlib.util.module_from_spec(spec)
    assert spec.loader is not None
    spec.loader.exec_module(module)
    return module


def test_event_system_persist() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    scene = (root / "Assets/Scenes/Play.unity").read_text(encoding="utf-8")
    bootstrap = (root / "Assets/Scripts/Content/GameBootstrap.cs").read_text(encoding="utf-8")
    settings = (root / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    inputs = (root / "ProjectSettings/InputManager.asset").read_text(encoding="utf-8")
    manifest = (root / "Packages/manifest.json").read_text(encoding="utf-8")
    lock = (root / "Packages/packages-lock.json").read_text(encoding="utf-8")
    generator = (root / "Tools/generate_unity_assets.py").read_text(encoding="utf-8")

    assert "m_Name: EventSystem" in scene
    assert "76c392e42b5098c458856cdf6ecaaaa1" in scene
    assert "4f231c4fb786f3946a6b90b886c48677" in scene
    assert "4f231eb8fc47f54ca11b152d6d181d1e" not in scene
    assert "m_HorizontalAxis: Horizontal" in scene
    assert "m_VerticalAxis: Vertical" in scene
    assert "m_SubmitButton: Submit" in scene
    assert "m_CancelButton: Cancel" in scene
    assert "m_ForceModuleActive: 1" not in scene
    assert "forceModuleActive" not in scene
    assert "forceModuleActive" not in bootstrap
    assert "InputSystemUIInputModule" not in scene
    assert "UnityEngine.EventSystems.StandaloneInputModule" in scene

    ensure = bootstrap.split("private static void EnsureEventSystem()")[1].split("private static void EnsureLight")[0]
    assert "FindAnyObjectByType<EventSystem>" in ensure
    assert "GetComponent<StandaloneInputModule>" in ensure
    assert "AddComponent<StandaloneInputModule>" in ensure
    assert "horizontalAxis = \"Horizontal\"" in ensure
    assert "verticalAxis = \"Vertical\"" in ensure
    assert "submitButton = \"Submit\"" in ensure
    assert "cancelButton = \"Cancel\"" in ensure
    assert "forceModuleActive" not in ensure
    assert "if (FindAnyObjectByType<EventSystem>() != null)\n            {\n                return;" not in ensure

    assert "activeInputHandler: 0" in settings
    assert "m_Name: Horizontal" in inputs
    assert "m_Name: Vertical" in inputs
    assert "m_Name: Submit" in inputs
    assert "m_Name: Cancel" in inputs
    assert "m_Name: AimX" in inputs
    assert "m_Name: AimY" in inputs
    assert "m_Name: FireTrigger" in inputs
    assert "m_Name: FirePad" in inputs
    assert "m_Name: Pause" in inputs
    assert "4f231c4fb786f3946a6b90b886c48677" in generator
    assert "com.unity.inputsystem" not in manifest
    assert "com.unity.modules.vr" not in manifest
    assert "com.unity.modules.xr" not in manifest
    assert "com.unity.modules.vr" not in lock
    assert "com.unity.modules.xr" not in lock


def test_shader_cs1503_gate() -> None:
    validator = _load_week1_validator()
    violations = validator.shader_conditional_violations

    bad_assign = 'Shader shader = ok ? Shader.Find("Standard") : Shader.Find("Unlit/Color");'
    assert violations(bad_assign), "ternary→Shader must fail the CS1503 gate"

    bad_reassign = """
        Shader shader = Shader.Find("Standard");
        shader = lit ? Shader.Find("Standard") : Shader.Find("Unlit/Color");
    """
    assert violations(bad_reassign), "reassigned Shader ternary must fail the CS1503 gate"

    bad_material = "_mat = new Material(shader != null ? shader : renderer.sharedMaterial);"
    assert violations(bad_material), "target-typed Material ?: must fail the CS1503 gate"

    good_find = 'Shader shader = Shader.Find(useUnlit ? "Unlit/Color" : "Standard");'
    assert not violations(good_find)

    good_if = """
        Shader shader = Shader.Find("Standard");
        if (shader != null)
        {
            _mat = new Material(shader);
        }
        else
        {
            _mat = new Material(renderer.sharedMaterial);
        }
    """
    assert not violations(good_if)

    good_factory = 'Shader shader = Shader.Find("Standard"); Material material = new Material(shader);'
    assert not violations(good_factory)

    comment = '// Shader shader = ok ? Shader.Find("Standard") : Shader.Find("Unlit/Color");'
    assert not violations(comment)

    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    for rel in (
        "Assets/Scripts/Combat/TelegraphRing.cs",
        "Assets/Scripts/Combat/MonsterPresence.cs",
    ):
        source = (root / rel).read_text(encoding="utf-8")
        assert not violations(source), f"{rel} still has a Shader / Material ternary (CS1503)"
        assert "new Material(shader);" in source
        assert "new Material(" in source and "? shader :" not in source


def test_monsters_arenas_040() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    enemies = (root / "Assets/Scripts/Combat/EnemyKind.cs").read_text(encoding="utf-8")
    seeker = (root / "Assets/Scripts/Combat/EnemySeeker.cs").read_text(encoding="utf-8")
    waves = (root / "Assets/Scripts/Core/WaveManager.cs").read_text(encoding="utf-8")
    layout = (root / "Assets/Scripts/Core/ArenaLayout.cs").read_text(encoding="utf-8")
    hazard = (root / "Assets/Scripts/Combat/ArenaHazard.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    art = (root / "Assets/Scripts/Content/ArtImport.cs").read_text(encoding="utf-8")
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    cause = (root / "Assets/Scripts/Core/DamageCause.cs").read_text(encoding="utf-8")
    manifest = (root / "Packages/manifest.json").read_text(encoding="utf-8")
    lock = (root / "Packages/packages-lock.json").read_text(encoding="utf-8")
    readme = (root / "README.md").read_text(encoding="utf-8")
    checklist = (root / "MERGE_CHECKLIST.md").read_text(encoding="utf-8")
    credits = (root / "CREDITS.md").read_text(encoding="utf-8")

    assert "Brute," in enemies or "Brute\n" in enemies
    assert "Swarm," in enemies or "Swarm\n" in enemies
    assert "Swarmling" in enemies
    assert 'return "Monster_Brute"' in enemies
    assert 'return "Monster_Swarm"' in enemies
    assert 'return "Monster_Swarmling"' in enemies
    assert "case EnemyKind.Brute:\n                    return 10;" in enemies
    assert "case EnemyKind.Swarm:\n                    return 6;" in enemies
    assert "BruteChargeRange" in enemies
    assert "NestSpawnSeconds = 3.5f" in enemies
    assert "IsMonster" in enemies
    require_mesh = enemies.split("RequiresImportedMesh")[1].split("public static bool IsMonster")[0]
    assert "EnemyKind.Bomber" in require_mesh
    assert "EnemyKind.SwarmPod" in require_mesh
    assert "EnemyKind.Brute" not in require_mesh
    assert "EnemyKind.Swarmling" not in require_mesh
    assert "kind == EnemyKind.Swarm\n" not in require_mesh and "kind == EnemyKind.Swarm;" not in require_mesh

    assert "TuneBrute" in seeker
    assert "TrySpawnMinion" in seeker
    assert "EnemyKind.Swarmling" in seeker
    assert "FindAnyObjectByType<ContentFactory>" in seeker
    assert "GetInstanceID" not in seeker
    assert "SetCharging" in seeker
    assert "MonsterPresence" in seeker
    presence = (root / "Assets/Scripts/Combat/MonsterPresence.cs").read_text(encoding="utf-8")
    assert "class MonsterPresence" in presence
    assert "SetCharging" in presence
    assert "AuraColor" in presence
    assert "PlaySpawnRing" in presence
    assert "PulseNest" in presence
    assert "TelegraphRing" in presence
    telegraph = (root / "Assets/Scripts/Combat/TelegraphRing.cs").read_text(encoding="utf-8")
    assert "class TelegraphRing" in telegraph
    assert "SpawnTelegraphRing" in factory
    assert "TelegraphNest" in seeker
    assert "PulseNest" in seeker

    roster = waves.split("public static EnemyKind[] RosterForWave")[1].split("public void Register")[0]
    assert "EnemyKind.Brute" in roster
    assert "EnemyKind.Swarm" in roster
    assert "EnemyKind.Brute" not in roster.split("case 4:")[1].split("case 5:")[0]
    assert "EnemyKind.Brute" in roster.split("case 5:")[1].split("case 6:")[0]
    assert "EnemyKind.Swarm" in roster.split("case 6:")[1].split("case 7:")[0]
    assert "EnemyKind.Swarm" not in roster.split("case 7:")[1].split("case 8:")[0]
    assert "EnemyKind.Brute" in roster.split("case 8:")[1].split("case 9:")[0]
    assert "EnemyKind.Swarm" in roster.split("case 8:")[1].split("case 9:")[0]
    assert "EnemyKind.SwarmPod, EnemyKind.Swarm" in roster.split("case 9:")[1].split("default:")[0]

    assert "enum ArenaLayoutId" in layout
    assert "PylonRing" in layout
    assert "SplitTrench" in layout
    assert "MineBelt" in layout
    assert "CrossGates" in layout
    assert "DebrisIslands" in layout
    assert "SpokeRing" in layout
    assert "WavesPerLayout = 5" in layout
    assert "LayoutCount = 7" in layout
    assert arena_layout_for_wave(1) == 1
    assert arena_layout_for_wave(6) == 2
    assert arena_layout_for_wave(11) == 3
    assert arena_layout_for_wave(16) == 4
    assert arena_layout_for_wave(21) == 5
    assert arena_layout_for_wave(26) == 6
    assert arena_layout_for_wave(31) == 7
    assert arena_layout_for_wave(36) == 1
    assert "return ArenaLayout.WorldIndexForWave(waveIndex)" in factory
    assert "BuildArenaLayout" in factory
    assert "PlaceHazardSpike" in factory
    assert 'TryVisual("Arena_Hazard_Spike"' in factory
    assert "BuildMonsterPlaceholder" in factory
    assert "DressBruteMesh" in factory
    assert "DressSwarmMesh" in factory
    assert "DressMonsterPresence" in factory
    assert "DressHazardSpike" in factory
    assert "LayoutRail" in factory
    assert "Mat_Arena_Wash_Cyan" in factory
    assert "MakeTransparent(\"Mat_Arena_Wash_Cyan\"" in factory or "Mat_Arena_Wash_Cyan" in factory
    assert "DamageCause.HazardContact" in hazard
    assert "Arena hazard" in cause
    assert "PulseVisual" in hazard
    assert "DressPulse" in hazard

    warm = art.split("PlayModeAssets")[1].split("};")[0]
    assert warm.count("\n            \"") == 54
    assert "Monster_Brute" in warm
    assert "Monster_Swarm" in warm
    assert "Arena_Hazard_Spike" in warm
    assert '"Monster_Swarmling"' in art
    assert art.index('"Monster_Swarmling"') < art.index('"Monster_Swarm"') or "Monster_Swarmling" in art

    spawn = audio.split("public void PlayMonsterSpawn(EnemyKind kind)")[1].split("public void")[0]
    assert "BruteSpawnScale" in spawn
    assert "_bruteSpawn" in spawn
    assert "_swarmSpawn" in spawn
    assert "_worldChange" not in spawn
    assert "_uiClick" not in spawn
    assert "_purchase" not in spawn
    hit_kind = audio.split("public void PlayHit(EnemyKind kind)")[1].split("public void")[0]
    assert "_bruteHits" in hit_kind
    assert "_swarmHits" in hit_kind
    assert "PlayPooled" in hit_kind
    assert "PlayPooledPitched" in hit_kind
    assert "SwarmHitPitchJitter" in hit_kind
    assert "_uiClick" not in hit_kind
    assert "_purchase" not in hit_kind
    swarm_hit = hit_kind.split("EnemyKind.Swarm")[1]
    assert "PlayPooledPitched(_swarmHits" in swarm_hit
    assert "SwarmHitPitchJitter" in swarm_hit
    death_kind = audio.split("public void PlayEnemyDeath(EnemyKind kind)")[1].split("public static bool")[0]
    assert "_bruteDeath" in death_kind
    assert "_swarmDeaths" in death_kind
    brute_death = death_kind.split("EnemyKind.Brute")[1].split("EnemyKind.Swarm")[0]
    assert "_bruteDeath" in brute_death
    assert "_bruteDeathLayer" in brute_death
    assert "BruteDeathLayerScale" in brute_death
    assert "_enemyDeathPunch" not in brute_death
    assert "PlayPooled(_swarmDeaths" in death_kind.split("EnemyKind.Swarm")[1]
    assert "_bruteDeath" not in death_kind.split("EnemyKind.Swarm")[1]
    assert "_uiClick" not in death_kind
    monster_loads = audio.split("AtmosBot 0.40 list")[1].split("_arenaLoop")[0]
    assert 'Resources.Load<AudioClip>("Audio/Sfx/lowThreeTone")' in monster_loads
    assert "impactMetal_000" in monster_loads and "impactMetal_002" in monster_loads
    assert 'Resources.Load<AudioClip>("Audio/Sfx/explosionCrunch_003")' in monster_loads
    assert 'Resources.Load<AudioClip>("Audio/Sfx/lowFrequency_explosion_000")' in monster_loads
    assert 'Resources.Load<AudioClip>("Audio/Sfx/phaseJump1")' in monster_loads
    assert 'Resources.Load<AudioClip>("Audio/Sfx/slime_000")' in monster_loads
    assert "laserSmall_000" in monster_loads and "laserSmall_004" in monster_loads
    assert "zap1" in monster_loads and "spaceTrash1" in monster_loads and "spaceTrash3" in monster_loads
    assert 'Resources.Load<AudioClip>("Audio/Sfx/forceField_001")' in monster_loads
    assert "laserRetro_000" in monster_loads and "laserRetro_002" in monster_loads
    assert "click_002" not in monster_loads
    assert "confirmation_002" not in monster_loads
    assert "PlayHazardActivate" in audio
    assert "PlayHazardHit" in audio
    assert "PlayHazardActivate" in factory
    assert "PlayHazardHit" in hazard
    hazard_act = audio.split("public void PlayHazardActivate()")[1].split("public void")[0]
    assert "HazardActivateScale" in hazard_act
    assert "DuckMusic" in hazard_act
    assert "HazardActivateDuckSeconds" in hazard_act
    assert "HazardActivateScale = 0.94f" in audio
    assert "SwarmHitPitchJitter = 0.06f" in audio
    assert "BruteDeathLayerScale = 1.12f" in audio
    assert "UsesMonsterThreatSfx" in audio
    light = audio.split("public static bool UsesLightThreatSfx")[1].split("public static bool UsesMonsterThreatSfx")[0]
    assert "EnemyKind.Swarmling" in light
    assert "EnemyKind.SwarmPod" in light

    assert "ArenaLayout.Badge" in ui
    assert "Pylon ring" in layout
    assert "_flashedLayout" in ui
    assert "WORLD " in ui
    assert "LAYOUT SWAP" in ui
    assert "layout:" in ui
    assert "PingPong(Time.unscaledTime * 3.2f" in ui
    assert "PingPong(Time.unscaledTime * 7f" not in ui
    summary = (root / "Assets/Scripts/Core/RunSummary.cs").read_text(encoding="utf-8")
    assert "MonsterTeaser" in summary
    assert "Wave 5 Brute" in summary
    assert "Wave 6 Swarm" in summary
    assert "DamagingSpikeDamage = 2" in hazard
    assert "damaging ? ArenaHazard.DamagingSpikeDamage" in factory
    assert "NestSpawnSeconds = 3.5f" in enemies
    assert "case EnemyKind.Brute:\n                    return 10;" in enemies
    assert "forceModuleActive" not in (root / "Assets/Scripts/Content/GameBootstrap.cs").read_text(encoding="utf-8")
    assert "ShowFailContinue" in summary
    assert "FailContinueHint" in summary
    assert "Your hull" in summary
    assert "start from the hangar" in summary
    assert "MonsterTeaser" in ui
    assert "FailContinueHint" in ui
    assert "PlayerFaultLine" in cause
    assert "PlayerFaultLine" in ui
    assert "Spoke ring" in layout
    assert "ArenaLayoutId.SpokeRing" in factory

    lfs_prefix = b"version https://git-lfs.github.com/spec/v1"
    for name, min_size in (
        ("Monster_Brute", 80000),
        ("Monster_Swarm", 120000),
        ("Arena_Hazard_Spike", 50000),
    ):
        art_fbx = root / f"Assets/Art/Import/{name}.fbx"
        res_fbx = root / f"Assets/Resources/Art/Import/{name}.fbx"
        assert art_fbx.is_file() and art_fbx.stat().st_size > min_size
        assert res_fbx.is_file() and res_fbx.stat().st_size > min_size
        assert not art_fbx.read_bytes()[:64].startswith(lfs_prefix)
        assert art_fbx.read_bytes()[:8] == b"Kaydara "
        assert not res_fbx.read_bytes()[:64].startswith(lfs_prefix)
        assert art_fbx.read_bytes() == res_fbx.read_bytes()

    for clip in (
        "lowThreeTone.ogg",
        "impactMetal_002.ogg",
        "phaseJump1.ogg",
        "slime_000.ogg",
        "laserSmall_002.ogg",
        "laserSmall_004.ogg",
        "zap1.ogg",
        "spaceTrash1.ogg",
        "spaceTrash2.ogg",
        "spaceTrash3.ogg",
        "forceField_001.ogg",
        "lowFrequency_explosion_000.ogg",
        "laserRetro_001.ogg",
        "laserRetro_002.ogg",
    ):
        path = root / "Assets/Resources/Audio/Sfx" / clip
        assert path.is_file() and path.stat().st_size > 1000
        assert path.read_bytes()[:4] == b"OggS"

    assert "com.unity.modules.vr" not in manifest
    assert "com.unity.modules.xr" not in manifest
    assert "com.unity.modules.vr" not in lock
    assert "com.unity.modules.xr" not in lock
    assert "0.40-monsters-arenas" in checklist
    assert "no 0.41" in checklist
    assert "Hub-open smoke" in checklist
    assert "Monster_Brute" in readme
    assert "Monster_Swarm" in readme
    assert "Pylon ring" in readme or "pylon ring" in readme.lower()
    assert "GetEntityId" in readme or "GetEntityId" in checklist
    assert "Arial.ttf" not in factory
    assert "Arial.ttf" not in ui
    assert "CC0" in credits
    assert "lowThreeTone" in credits
    assert "phaseJump1" in credits
    assert "spaceTrash1" in credits
    assert "forceField_001" in credits
    assert "AtmosBot" in credits
    assert "retro-modern" in credits.lower()
    assert "AAA" in readme and "big-studio" in readme.lower()
    assert "look bible" in readme.lower()
    assert "retro-modern chip" in readme.lower() or "retro-modern chip" in credits.lower()
    assert "prototype-placeholder" in readme.lower()
    assert "look bible" in checklist.lower()
    assert "MonsterPresence" in checklist
    assert "AAA mix polish" in credits
    assert "AAA mix polish" in audio
    assert "lowFrequency_explosion_000" in credits
    assert "SwarmHitPitchJitter" in audio


def test_weapons_upgrades_040b() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    catalog = (root / "Assets/Scripts/Core/ShopCatalog.cs").read_text(encoding="utf-8")
    loadout = (root / "Assets/Scripts/Core/LoadoutState.cs").read_text(encoding="utf-8")
    ids = (root / "Assets/Scripts/Core/UpgradeId.cs").read_text(encoding="utf-8")
    fire = (root / "Assets/Scripts/Player/FireMode.cs").read_text(encoding="utf-8")
    shooter = (root / "Assets/Scripts/Player/ShipShooter.cs").read_text(encoding="utf-8")
    projectile = (root / "Assets/Scripts/Player/Projectile.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    summary = (root / "Assets/Scripts/Core/RunSummary.cs").read_text(encoding="utf-8")
    visuals = (root / "Assets/Scripts/Player/ShipVisuals.cs").read_text(encoding="utf-8")
    health = (root / "Assets/Scripts/Player/ShipHealth.cs").read_text(encoding="utf-8")
    art = (root / "Assets/Scripts/Content/ArtImport.cs").read_text(encoding="utf-8")
    readme = (root / "README.md").read_text(encoding="utf-8")
    credits = (root / "CREDITS.md").read_text(encoding="utf-8")
    checklist = (root / "MERGE_CHECKLIST.md").read_text(encoding="utf-8")

    assert catalog.count("new ShopItem(") == 17
    assert catalog.index("ShopGroup.Hull") < catalog.index("ShopGroup.Weapons")
    assert catalog.index("ShopGroup.Weapons") < catalog.index("ShopGroup.Defense")
    for token in (
        "TwinGuns",
        "Seeker",
        "Ricochet",
        "BodyUpgrade02",
        "NoseUpgrade03",
        "EngineUpgrade03",
        "Overcharger",
        "Afterburner",
        "ShieldMatrix",
        "SpreadBolt",
        "Pierce",
        "ShieldCell",
        '"Body Upgrade"',
        '"Shield Cell"',
        '"Spread Bolt"',
        '"Pierce"',
    ):
        assert token in catalog

    assert "TwinGuns" in ids and "Seeker" in ids and "Ricochet" in ids
    assert "Overcharger" in ids and "Afterburner" in ids and "ShieldMatrix" in ids
    assert "Twin," in fire
    assert "Seeker," in fire and "Ricochet" in fire

    assert "SpreadPelletCount = 3" in shooter
    assert "FireTwin" in shooter
    assert "SpawnStyledShot" in shooter and "SpawnStyledShot" in factory
    assert "SpawnProjectile(Vector3 origin, Vector3 direction, float speed, int damage, bool pierce)" in factory
    assert "FireMode.Twin" in shooter and "FireMode.Seeker" in shooter and "FireMode.Ricochet" in shooter
    assert "TwinOffsetMeters" in loadout and "TwinOffsetMeters" in shooter
    cycle = shooter.split("CycleOrder")[1].split(";")[0]
    assert "FireMode.Bolt" in cycle and "FireMode.Spread" in cycle and "FireMode.Twin" in cycle
    assert "FireMode.Pierce" in cycle and "FireMode.Seeker" in cycle and "FireMode.Ricochet" in cycle

    assert "SeekerCore" in factory and "RicochetFacet" in factory and "TwinCore" in factory
    assert "PierceNeedle" in factory and "SpreadCore" in factory
    assert "Mat_Projectile_Seeker" in factory and "Mat_Projectile_Ricochet" in factory
    assert "SeekerTurnDegrees" in projectile
    assert "RicochetBounces" in loadout and "RicochetBounces" in factory
    assert "bool seeker" in projectile and "int ricochetBounces" in projectile
    assert "BounceOnRim" in projectile
    assert "FindGameObjectsWithTag" in projectile
    assert "FindObjectsSortMode" not in projectile
    assert "GetInstanceID" not in projectile
    assert "GetEntityId()" in projectile
    assert "PlayShootSeeker" in audio and "PlayShootSeeker" in shooter
    assert "PlayShootTwin" in audio and "PlayShootTwin" in shooter
    assert "PlayShootRicochet" in audio and "PlayShootRicochet" in shooter
    seeker_fn = audio.split("public void PlayShootSeeker()")[1].split("public void")[0]
    assert "_shootSeeker" in seeker_fn and "_shootPierce" in seeker_fn
    assert "_worldChange" not in seeker_fn and "maximize_008" not in seeker_fn
    assert 'Resources.Load<AudioClip>("Audio/Sfx/phaserUp5")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/twoTone1")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/zap1")' in audio
    for clip in ("phaserUp5.ogg", "twoTone1.ogg", "zap1.ogg"):
        path = root / "Assets/Resources/Audio/Sfx" / clip
        assert path.is_file() and path.stat().st_size > 1000
        assert path.read_bytes()[:4] == b"OggS"

    can = loadout.split("public bool CanApply")[1].split("public void Apply")[0]
    assert "NoseUpgrade03 && !Overcharger && !Afterburner" in can
    assert "EngineUpgrade03 && !Afterburner && !Overcharger" in can
    assert "ShieldCharges >= MaxShieldCharges && !ShieldMatrix" in can
    assert "BodyUpgrade01 && !BodyUpgrade02" in can
    assert "NoseUpgrade02 && !NoseUpgrade03" in can
    assert "EngineUpgrade02 && !EngineUpgrade03" in can
    assert "MatrixMaxShieldCharges = 3" in loadout
    assert "MaxShieldCharges = 2" in loadout
    assert "CurrentMaxShield" in loadout and "CurrentMaxShield" in health
    assert "NoseUpgrade03Damage = 4" in loadout
    assert "AfterburnerCooldown = 0.075f" in loadout
    assert "SeekerFireCooldown = 0.85f" in loadout
    assert "SeekerFireCooldown" in shooter
    assert "mode == FireMode.Seeker" in shooter.split("public void TryFire()")[1].split("if (mode == FireMode.Spread)")[0]
    assert 0.85 >= 0.38 * 2.2
    assert "OverchargerDamageBonus" in loadout
    assert "HasAltFire" in loadout and "HasAltFire" in ui and "HasAltFire" in shooter
    assert "hullIndex % 4" in ui
    assert "discover Spread / Pierce when owned" in ui
    assert '"Body Upgrade"' in catalog
    shield_cell = catalog.split("UpgradeId.ShieldCell")[1].split("new ShopItem")[0]
    assert "80," in shield_cell
    costs = [
        int(line.strip().rstrip(","))
        for line in catalog.split("public static readonly ShopItem[] Items")[1].split("};")[0].splitlines()
        if line.strip().rstrip(",").isdigit()
    ]
    assert min(costs) == 80
    assert "Hull 02" in summary and "Nose 03" in summary and "Engine 03" in summary
    assert "Overcharger" in summary and "Afterburner" in summary
    assert "Twin" in summary and "Seeker" in summary and "Ricochet" in summary
    assert "NoseUpgrade03 || loadout.NoseUpgrade02" in visuals
    assert "EngineUpgrade03 || loadout.EngineUpgrade02" in visuals
    warm = art.split("PlayModeAssets")[1].split("};")[0]
    assert "Ship_Body_Upgrade02" not in warm
    assert "Twin Guns" in readme and "Seeker" in readme and "Ricochet" in readme
    assert "Overcharger" in readme and "Afterburner" in readme and "Shield Matrix" in readme
    assert "twoTone1" in credits and "phaserUp5" in credits and "zap1" in credits
    assert "Twin Guns" in checklist and "Shield Matrix" in checklist
    assert "AAA" in readme and "look bible" in readme.lower()


def test_art_parity_040c() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    art = (root / "Assets/Scripts/Content/ArtImport.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    enemies = (root / "Assets/Scripts/Combat/EnemyKind.cs").read_text(encoding="utf-8")
    readme = (root / "README.md").read_text(encoding="utf-8")
    checklist = (root / "MERGE_CHECKLIST.md").read_text(encoding="utf-8")
    manifest = (root / "Packages/manifest.json").read_text(encoding="utf-8")
    lock = (root / "Packages/packages-lock.json").read_text(encoding="utf-8")

    bomber_names = art.split('case "Enemy_Bomber":')[1].split("case ")[0]
    assert '"Enemy_Bomber"' in bomber_names
    assert "Enemy_Bomber_Buffer_v8" in bomber_names
    assert bomber_names.index("Enemy_Bomber_Buffer_v8") < bomber_names.index("Enemy_Bomber_Buffer_v6")
    assert bomber_names.index("Enemy_Bomber_Buffer_v6") < bomber_names.index("Enemy_Bomber_Buffer_v5")
    ship_names = art.split('case "Ship_Complete":')[1].split("case ")[0]
    assert '"Ship_Complete"' in ship_names
    assert "Ship_Complete_Buffer_v5" in ship_names
    assert ship_names.index("Ship_Complete_Buffer_v5") < ship_names.index("Ship_Complete_Buffer_v4")

    assert "Enemy_Bomber_Buffer_v8" in enemies
    assert "Enemy_Bomber_Buffer_v6" in enemies
    assert 'return "Enemy_Bomber"' in enemies
    assert 'PlaceHangarProp("Hangar_ShipComplete", "Ship_Complete"' in factory
    assert "DressBomberMesh" in factory
    assert "DressShipComplete" in factory
    assert "Enemy_Bomber_Buffer_v8" not in factory
    assert "Ship_Complete_Buffer_v5" not in factory
    assert "EnemyCatalog.VisualName" in (root / "Assets/Scripts/Core/WaveManager.cs").read_text(
        encoding="utf-8"
    )
    require_mesh = enemies.split("public static bool RequiresImportedMesh")[1].split("public static")[0]
    assert "EnemyKind.Bomber" in require_mesh
    warm = art.split("PlayModeAssets")[1].split("};")[0]
    assert warm.count("\n            \"") == 54
    assert "Ship_Complete" in warm and "Enemy_Bomber" in warm
    assert "Ship_Body_Upgrade02" not in warm

    lfs_prefix = b"version https://git-lfs.github.com/spec/v1"
    for folder in (
        root / "Assets/Art/Import",
        root / "Assets/Resources/Art/Import",
    ):
        for fbx in sorted(folder.glob("*.fbx")):
            data = fbx.read_bytes()
            assert not data[:64].startswith(lfs_prefix), f"{fbx} is an LFS pointer"
            assert data[:8] == b"Kaydara ", f"{fbx} is not an FBX binary"

    for name, min_size in (("Ship_Complete", 220000), ("Enemy_Bomber", 270000)):
        art_fbx = root / f"Assets/Art/Import/{name}.fbx"
        res_fbx = root / f"Assets/Resources/Art/Import/{name}.fbx"
        assert art_fbx.is_file() and art_fbx.stat().st_size > min_size
        assert res_fbx.is_file() and res_fbx.stat().st_size > min_size
        assert art_fbx.read_bytes() == res_fbx.read_bytes()

    assert "Ship_Complete` v5" in readme or "Ship_Complete v5" in readme or "parked `Ship_Complete` v5" in readme
    assert "Bomber v8" in readme
    assert "Ship_Complete v5" in checklist or "Ship_Complete** v5" in checklist
    assert "Bomber v8" in checklist
    assert "com.unity.modules.vr" not in manifest
    assert "com.unity.modules.xr" not in manifest
    assert "com.unity.modules.vr" not in lock
    assert "com.unity.modules.xr" not in lock
    assert "AAA" in readme and "look bible" in readme.lower()


def test_end_credits_040d() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    credits_cs = (root / "Assets/Scripts/Core/EndCredits.cs").read_text(encoding="utf-8")
    summary = (root / "Assets/Scripts/Core/RunSummary.cs").read_text(encoding="utf-8")
    readme = (root / "README.md").read_text(encoding="utf-8")
    credits_md = (root / "CREDITS.md").read_text(encoding="utf-8")
    checklist = (root / "MERGE_CHECKLIST.md").read_text(encoding="utf-8")
    manifest = (root / "Packages/manifest.json").read_text(encoding="utf-8")
    lock = (root / "Packages/packages-lock.json").read_text(encoding="utf-8")

    assert "CreditsLoopScale = 0.55f" in audio
    assert "CreditsOpenDuckSeconds = 0.35f" in audio
    assert "CreditsOpenDuckScale = 0.4f" in audio
    assert "PlayCreditsOpen" in audio and "PlayCreditsLoop" in audio
    assert "StopCreditsMusic" in audio and "PlayCreditsClose" in audio
    open_fn = audio.split("public void PlayCreditsOpen()")[1].split("public void")[0]
    assert "_creditsOpen" in open_fn
    assert "DuckMusic(CreditsOpenDuckSeconds, CreditsOpenDuckScale)" in open_fn
    assert "maximize_008" not in open_fn and "_worldChange" not in open_fn
    loop_fn = audio.split("public void PlayCreditsLoop()")[1].split("public void")[0]
    assert "CreditsLoopScale" in loop_fn
    stop_fn = audio.split("public void StopCreditsMusic()")[1].split("public void")[0]
    assert "SyncMusicToPhase(GamePhase.Hangar)" in stop_fn
    wave_fn = audio.split("public void PlayWaveClear()")[1].split("public void")[0]
    assert "Play(_waveClear, WaveClearScale)" in wave_fn
    assert "WaveClearScale = 0.88f" in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/jingles_HIT07")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Music/GameOver")' in audio
    assert "PlayCredits" not in wave_fn
    assert 'Resources.Load<AudioClip>("Audio/Music/OutThere")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Music/spacelifeNo14")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Music/SpaceCadet")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/jingles_NES07")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/jingles_NES12")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/jingles_PIZZA16")' in audio
    close_fn = audio.split("public void PlayCreditsClose()")[1].split("public void")[0]
    assert "_creditsClose" in close_fn and "_farDriftAward" in close_fn

    assert "class EndCredits" in credits_cs
    assert 'return "Asteroids gone rogue"' in credits_cs.split("public static string Title()")[1].split("public static string Body()")[0]
    body_fn = credits_cs.split("public static string Body()")[1]
    assert "Asteroids gone rogue" not in body_fn
    assert "Kenney.nl + yd" in credits_cs
    assert "Kenney Future" in credits_cs
    assert "SpelPM / GameBot / BlenderBot / AtmosBot / Speltest" in credits_cs
    assert "TitleSize = 32" in credits_cs
    assert "BodySize = 17" in credits_cs
    assert "#FFD16F" in credits_cs and "#B8E8FF" in credits_cs
    assert "new UnityEngine.Color32(255, 209, 111, 255)" in credits_cs
    assert "new UnityEngine.Color32(184, 232, 255, 255)" in credits_cs
    assert "ShowEndCredits" in ui and "HideEndCredits" in ui
    show_fn = ui.split("private void ShowEndCredits()")[1].split("private void HideEndCredits")[0]
    assert "PlayCreditsOpen" in show_fn and "PlayCreditsLoop" in show_fn
    assert "PlayUiClick" not in show_fn
    assert "StopCreditsMusic" in ui
    assert "EndCredits" in ui
    assert "RectMask2D" in ui
    continue_fn = ui.split("private void BuildEndCredits")[1].split("private void ShowEndCredits")[0]
    assert "Continue" in continue_fn
    assert "new Color(1f, 0.82f, 0.44f, 0.98f)" in continue_fn
    assert "new Color(0.42f, 0.26f, 0.08f, 0.98f)" not in continue_fn
    assert "OpenCredits" in ui
    assert "SHIP LOST" in summary

    for rel, min_size in (
        ("Assets/Resources/Audio/Music/SpaceCadet.ogg", 200000),
        ("Assets/Resources/Audio/Sfx/jingles_NES07.ogg", 8000),
        ("Assets/Resources/Audio/Sfx/jingles_NES12.ogg", 8000),
        ("Assets/Resources/Audio/Music/OutThere.ogg", 1000),
        ("Assets/Resources/Audio/Music/spacelifeNo14.ogg", 1000),
    ):
        path = root / rel
        assert path.is_file() and path.stat().st_size > min_size
        assert path.read_bytes()[:4] == b"OggS"

    assert "SpaceCadet" in readme and "jingles_NES07" in readme
    assert "SpaceCadet" in credits_md and "Kenney.nl" in credits_md
    assert "SpaceCadet" in checklist and "EndCredits" in checklist or "Credits" in checklist
    assert "com.unity.modules.vr" not in manifest
    assert "com.unity.modules.xr" not in manifest
    assert "com.unity.modules.vr" not in lock
    assert "com.unity.modules.xr" not in lock
    assert "AAA" in readme and "look bible" in readme.lower()


def test_localization_040() -> None:
    from pathlib import Path
    import re

    root = Path(__file__).resolve().parents[1]
    loc = (root / "Assets/Scripts/Core/Loc.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    catalog = (root / "Assets/Scripts/Core/ShopCatalog.cs").read_text(encoding="utf-8")
    summary = (root / "Assets/Scripts/Core/RunSummary.cs").read_text(encoding="utf-8")
    cause = (root / "Assets/Scripts/Core/DamageCause.cs").read_text(encoding="utf-8")
    medals = (root / "Assets/Scripts/Core/MedalCatalog.cs").read_text(encoding="utf-8")
    layout = (root / "Assets/Scripts/Core/ArenaLayout.cs").read_text(encoding="utf-8")
    credits_cs = (root / "Assets/Scripts/Core/EndCredits.cs").read_text(encoding="utf-8")
    best = (root / "Assets/Scripts/Core/LocalBest.cs").read_text(encoding="utf-8")
    health = (root / "Assets/Scripts/Player/ShipHealth.cs").read_text(encoding="utf-8")
    manager = (root / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    session = (root / "Assets/Scripts/Core/GameSession.cs").read_text(encoding="utf-8")
    loadout = (root / "Assets/Scripts/Core/LoadoutState.cs").read_text(encoding="utf-8")
    shooter = (root / "Assets/Scripts/Player/ShipShooter.cs").read_text(encoding="utf-8")
    bootstrap = (root / "Assets/Scripts/Content/GameBootstrap.cs").read_text(encoding="utf-8")
    readme = (root / "README.md").read_text(encoding="utf-8")
    checklist = (root / "MERGE_CHECKLIST.md").read_text(encoding="utf-8")
    manifest = (root / "Packages/manifest.json").read_text(encoding="utf-8")
    lock = (root / "Packages/packages-lock.json").read_text(encoding="utf-8")

    assert 'PrefsKey = "agr.ui.language"' in loc
    assert 'EnglishCode = "en"' in loc and 'SwedishCode = "sv"' in loc
    assert "SetLanguage" in loc
    assert "PlayerPrefs.SetString(PrefsKey" in loc
    assert "PlayerPrefs.GetString(PrefsKey" in loc
    assert "enum GameLanguage" in loc
    assert "Starta våg" in loc
    assert "Medverkande" in loc
    assert "SKEPP FÖRLORAT" in loc
    assert "Första flygningen" in loc
    assert "LAYOUTBYTE" in loc
    assert "Spejarvinge" in loc

    assert "UsFlag" in ui and "SvFlag" in ui
    assert "BuildUsFlag" in ui and "BuildSwedishFlag" in ui
    assert "OnPickLanguage" in ui
    assert "LanguagePanel" in ui
    assert "Loc.SetLanguage" in ui
    assert "ApplyLocalizedStaticLabels" in ui

    assert "Loc.T(" in ui and "Loc.T(" in summary and "Loc.T(" in cause
    assert "Loc.T(" in catalog and "Loc.T(" in medals and "Loc.T(" in layout
    assert "Loc.T(" in credits_cs and "Loc.T(" in best
    assert "HasStructuredFail" in session
    assert "NotifyPlayerDestroyed(DamageCause cause, EnemyKind kind)" in manager
    assert "NotifyPlayerDestroyed(cause, enemyKind)" in health
    assert "FailReasonText" in ui

    keys = set(re.findall(r'Loc\.Tf?\(\s*"([^"]+)"', "\n".join((loc, ui, catalog, summary, cause, medals, layout, credits_cs, best))))
    swedish = set(re.findall(r'\{\s*"([^"]+)"\s*,', loc.split("private static readonly Dictionary")[1].split("};")[0]))
    required = {
        "ui.start_wave", "ui.next_wave", "ui.retry_wave", "ui.abort", "ui.credits",
        "ui.health", "ui.hull_label", "ui.shield_label",
        "ui.hangar_hint_body", "ui.layout_swap", "ui.world_badge", "ui.credits_line",
        "shop.header.hull", "shop.title.Seeker", "run.wave_clear", "run.ship_lost",
        "fail.asteroid", "fail.hazard", "fail.unknown", "fail.almost_one", "fail.almost_n",
        "fail.left_n", "medal.scout", "layout.pylon",
        "credits.body", "best.card",
        "ui.difficulty", "ui.diff.easy", "ui.diff.normal", "ui.diff.hard",
        "ui.lives", "ui.hud_lives", "ui.life_lost",
        "ui.ship_preview",
    }
    missing_required = required - swedish
    assert not missing_required, missing_required
    missing_used = keys - swedish - {"shop.title.", "shop.desc.", "enemy.", "mode."}
    dynamic_ok = {k for k in missing_used if k.startswith("shop.title.") or k.startswith("shop.desc.") or k.startswith("enemy.") or k.startswith("mode.")}
    missing_used -= dynamic_ok
    assert not missing_used, missing_used

    assert "SeekerFireCooldown = 0.85f" in loadout
    assert "SeekerFireCooldown" in shooter
    assert "forceModuleActive" not in bootstrap
    assert "com.unity.modules.vr" not in manifest
    assert "com.unity.modules.xr" not in manifest
    assert "com.unity.modules.vr" not in lock
    assert "com.unity.modules.xr" not in lock
    assert "agr.ui.language" in readme
    assert "agr.ui.language" in checklist
    assert "0.41" not in loc and "Release" not in loc


def test_astro_env_040() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    env = (root / "Assets/Scripts/Content/ArenaEnv.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    art = (root / "Assets/Scripts/Content/ArtImport.cs").read_text(encoding="utf-8")
    credits = (root / "CREDITS.md").read_text(encoding="utf-8")
    readme = (root / "README.md").read_text(encoding="utf-8")
    checklist = (root / "MERGE_CHECKLIST.md").read_text(encoding="utf-8")
    loc = (root / "Assets/Scripts/Core/Loc.cs").read_text(encoding="utf-8")
    loadout = (root / "Assets/Scripts/Core/LoadoutState.cs").read_text(encoding="utf-8")
    bootstrap = (root / "Assets/Scripts/Content/GameBootstrap.cs").read_text(encoding="utf-8")
    manifest = (root / "Packages/manifest.json").read_text(encoding="utf-8")
    lock = (root / "Packages/packages-lock.json").read_text(encoding="utf-8")

    assert "class ArenaEnv" in env
    assert "NebulaRadiusScale = 3.8f" in env
    assert "NebulaInnerOpacity = 0.18f" in env
    assert "BeltRadiusScale = 1.58f" in env
    assert "Starfield_A" in env and "Starfield_B" in env
    assert "Nebula_Blue" in env
    assert "CreateBelt" in env and "AstroGrid" in env
    assert "NebulaInner" in env
    assert "DustRing" not in env
    assert "CreateDustRing" not in env
    assert "BeltOuterRadiusScale = 1.68f" in env
    assert "BeltSpinDegrees = 6f" in env
    assert "BeltCount = 22" in env
    assert "StarFarTint = 0.45f" in env
    assert "StarNearTint = 1.05f" in env
    assert "StarFarTiling = 3.6f" in env
    assert "GridFadeRadiusScale = 0.7f" in env
    assert "GridAlpha = 0.045f" in env
    assert "NebulaOpacity = 0.34f" in env
    assert "RetintChroma = 0.55f" in env
    assert "0.78f, 0.88f, 1f" in env
    assert "0.85f, 0.28f, 0.55f" not in env
    assert "0.35f, 0.9f, 0.28f" not in env
    assert 'Shader.Find("Particles/Additive")' in env
    assert "ArenaRimLight" in env
    assert "ArenaUnderGlow" in env
    assert "LightType.Point" in env
    assert "DarkBeltMaterial" in env
    assert "-0.5f, 3.2f" in env
    assert "0.7f, 1.4f" in env
    assert "mainTextureScale" in env
    assert "Retint" in env
    assert "ArenaEnv.Ensure" in factory
    assert 'child.name == "ArenaEnv"' in factory
    assert 'TryVisual("Arena_RockIsland_A"' in factory
    assert "SinkPlaySurface" in factory
    assert "IsAstroPlayFloor" in factory
    assert "ArenaPlaySurfaceY = -0.08f" in factory
    assert "AstroFloorVisualScale = 0.88f" in factory
    assert "DressArenaFloorRenderers" in factory
    assert "DressAstroFloorColliders" in factory
    dress = factory.split("private static void DressAstroFloorColliders")[1].split("private bool TryVisual")[0]
    assert "AddComponent<BoxCollider>" in dress
    assert "box.isTrigger = true" in dress
    assert "AstroFloor_v2" in factory
    assert "Arena_AstroFloor_v2" in factory
    assert 'MakeMaterial("Mat_AstroRim"' in factory
    assert "AstroRim" in factory
    assert "box.center = new Vector3(0.64f, 1.55f, 0.15f)" in factory
    shooter = (root / "Assets/Scripts/Player/ShipController.cs").read_text(encoding="utf-8")
    assert "PlayHeight = 0.4f" in shooter
    assert "new Vector3(0f, PlayHeight, 0f)" in shooter
    assert "pos.y = PlayHeight" in shooter
    seeker = (root / "Assets/Scripts/Combat/EnemySeeker.cs").read_text(encoding="utf-8")
    assert "_body.linearVelocity = dir * speed" in seeker
    assert "FreezePositionY" in factory
    assert "collider.direction = 2" in factory
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    assert "HealthRack" in ui
    assert "RefreshHealthBar" in ui
    assert "Image.Type.Filled" in ui
    assert "fill.fillAmount = 1f" in ui
    assert "_hullFill.fillAmount" in ui
    assert "_shieldFill.fillAmount" in ui
    assert "BarFillSprite" in ui
    assert "HealthBarFill" in ui
    assert "_shieldBarRow.SetActive(true)" in ui
    assert 'Loc.T("ui.health", "HEALTH")' in ui
    assert "ArenaLip" in env
    assert "PlayVignette" in ui
    assert "* 0.35f" in ui
    assert "0.039f, 0.063f, 0.086f" in factory
    assert "0.357f, 0.561f, 0.659f" in factory
    assert "TrailScale = 0.75f" in factory
    assert "0.722f, 0.353f, 0.157f" in factory
    assert "UiAmber" in ui
    assert "UiHull" in ui
    assert "UiShield" in ui
    assert "1f, 0.96f, 0.92f" in ui
    flash = (root / "Assets/Scripts/Combat/MeshHitFlash.cs").read_text(encoding="utf-8")
    assert "1f, 0.96f, 0.92f" in flash
    assert "fieldOfView = 54f" in bootstrap
    assert "public int MaxHull" in (root / "Assets/Scripts/Player/ShipHealth.cs").read_text(encoding="utf-8")
    assert '"Arena_AstroFloor"' in art
    blockout = art.split('case "Arena_Blockout":')[1].split("case ")[0]
    assert "AstroFloor_v2" in blockout
    assert "Arena_AstroFloor_v2" in blockout
    assert "Arena_AstroFloor" in blockout
    warm = art.split("PlayModeAssets")[1].split("};")[0]
    assert warm.count("\n            \"") == 54
    assert "Arena_AstroFloor" not in warm
    assert "AstroFloor_v2" not in warm
    assert "Arena_AstroFloor_v2" not in warm
    assert "Arena_RockIsland_A" not in warm

    lfs_prefix = b"version https://git-lfs.github.com/spec/v1"
    for name, min_size in (("Arena_AstroFloor", 80000), ("Arena_AstroFloor_v2", 80000), ("Arena_RockIsland_A", 40000)):
        art_fbx = root / f"Assets/Art/Import/{name}.fbx"
        res_fbx = root / f"Assets/Resources/Art/Import/{name}.fbx"
        assert art_fbx.is_file() and art_fbx.stat().st_size > min_size
        assert res_fbx.is_file() and res_fbx.stat().st_size > min_size
        assert art_fbx.read_bytes() == res_fbx.read_bytes()
        assert not art_fbx.read_bytes()[:64].startswith(lfs_prefix)
        assert art_fbx.read_bytes()[:8] == b"Kaydara "

    for name in ("Starfield_A.png", "Starfield_B.png", "Nebula_Blue.png", "Nebula_Purple.png"):
        path = root / "Assets/Resources/Art/Env" / name
        assert path.is_file() and path.stat().st_size > 1000
        assert path.read_bytes()[:8] == b"\x89PNG\r\n\x1a\n"

    assert "Screaming Brain Studios" in credits
    assert "Seamless Space Backgrounds" in credits
    assert "ArenaEnv" in readme and "Arena_AstroFloor" in readme
    assert "ArenaEnv" in checklist and "Arena_AstroFloor" in checklist
    assert 'PrefsKey = "agr.ui.language"' in loc
    assert "SeekerFireCooldown = 0.85f" in loadout
    assert "forceModuleActive" not in bootstrap
    assert "com.unity.modules.vr" not in manifest
    assert "com.unity.modules.xr" not in lock


def test_fair_death_042() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    session = (root / "Assets/Scripts/Core/GameSession.cs").read_text(encoding="utf-8")
    manager = (root / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    summary = (root / "Assets/Scripts/Core/RunSummary.cs").read_text(encoding="utf-8")
    cause = (root / "Assets/Scripts/Core/DamageCause.cs").read_text(encoding="utf-8")
    loc = (root / "Assets/Scripts/Core/Loc.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")
    health = (root / "Assets/Scripts/Player/ShipHealth.cs").read_text(encoding="utf-8")
    loadout = (root / "Assets/Scripts/Core/LoadoutState.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    seeker = (root / "Assets/Scripts/Combat/EnemySeeker.cs").read_text(encoding="utf-8")
    shooter = (root / "Assets/Scripts/Player/ShipShooter.cs").read_text(encoding="utf-8")
    bootstrap = (root / "Assets/Scripts/Content/GameBootstrap.cs").read_text(encoding="utf-8")
    readme = (root / "README.md").read_text(encoding="utf-8")
    checklist = (root / "MERGE_CHECKLIST.md").read_text(encoding="utf-8")
    credits = (root / "CREDITS.md").read_text(encoding="utf-8")
    manifest = (root / "Packages/manifest.json").read_text(encoding="utf-8")
    lock = (root / "Packages/packages-lock.json").read_text(encoding="utf-8")

    assert "FailRemainingThreats" in session
    assert "FailWave(string reason, int remainingThreats)" in session
    assert "FailWave(DamageCause cause, EnemyKind kind, int remainingThreats)" in session
    assert 'FailWave(string reason)' in session

    string_fail = manager.split("public void NotifyPlayerDestroyed(string cause)")[1].split("public void")[0]
    kind_fail = manager.split("public void NotifyPlayerDestroyed(DamageCause cause, EnemyKind kind)")[1].split("private bool BeginPlayerDeath")[0]
    for block in (string_fail, kind_fail):
        assert "BeginPlayerDeath" in block or "TryRespawnAfterLifeLoss" in block
        assert "TryRespawnAfterLifeLoss" in block
        assert "FailRun" in block
        assert "PlayWaveFail" not in block
        assert "PlayWaveClear" not in block
        assert "PlayUiClick" not in block
        assert "DespawnAll" not in block

    fail_run = manager.split("private void FailRun")[1].split("private void")[0]
    assert "RemainingThreats" in fail_run
    assert "DespawnAll" in fail_run
    assert fail_run.index("RemainingThreats") < fail_run.index("DespawnAll")
    assert "PlayWaveFail" in fail_run
    assert "PlayWaveClear" not in fail_run
    assert "PlayUiClick" not in fail_run

    start = manager.split("public void StartWave()")[1].split("public void")[0]
    assert "_loadout.State" in start
    assert "ResetForWave(_loadout.State)" in start

    assert "AlmostHadItMax = 3" in summary
    assert "AlmostHadIt" in summary
    assert "One left. Almost had it." in summary
    assert "Almost had it — {0} left." in summary
    assert "{0} left." in summary
    assert "Your hull. Run over — start from the hangar." in summary
    assert "FailContinueHint(string failReason, int waveIndex, int remainingThreats)" in summary
    assert "that was you. Run over — start from the hangar." in cause
    assert "PlayerFaultLine" in cause
    assert "PlayerFaultLine" in ui
    assert "FailContinueHint" in ui
    assert "FailRemainingThreats" in ui
    assert "ApplyFailChrome" in ui
    assert "LayoutHealthRack" in ui
    assert "GamePhase.Failed" in ui.split("private void RefreshHealthBar()")[1].split("private void")[0]
    assert "0.008f, 0.33f" in ui
    assert "0.178f, 0.62f" in ui
    assert "UiAmber" in ui.split("private void ApplyFailChrome")[1].split("private void")[0]
    assert "UiFonts.Display()" in ui
    chrome = ui.split("private void ApplyFailChrome")[1].split("public void FlashHit")[0]
    assert "0.10f, 0.09f, 0.08f" in chrome
    assert "0.35f, 0.9f, 0.28f" not in chrome
    assert "0.85f, 0.28f, 0.55f" not in chrome

    on_primary = ui.split("private void OnPrimary()")[1].split("private void")[0]
    assert "PlayRetry" in on_primary
    assert "GamePhase.Failed" in on_primary
    assert on_primary.index("PlayRetry") < on_primary.index("PlayUiClick")

    assert "public void PlayWaveFail()" in audio
    assert "public void PlayRetry()" in audio
    fail_fn = audio.split("public void PlayWaveFail()")[1].split("public void")[0]
    retry_fn = audio.split("public void PlayRetry()")[1].split("public void")[0]
    assert "phaserDown3" not in fail_fn
    assert "_fail" in fail_fn and "_failLayer" in fail_fn
    assert "DuckMusic" in fail_fn
    assert "FailDuckSeconds" in fail_fn
    assert "_waveClear" not in fail_fn
    assert "_uiClick" not in fail_fn
    assert "_purchase" not in fail_fn
    assert "explosionCrunch" not in fail_fn
    assert "jingles_NES" not in fail_fn
    assert "_retry" in retry_fn
    assert "DuckMusic" not in retry_fn
    assert "_purchase" not in retry_fn
    assert "_uiClick" not in retry_fn
    assert 'Resources.Load<AudioClip>("Audio/Sfx/phaserDown3")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/lowDown")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/twoTone1")' in audio
    assert "FailScale = 0.78f" in audio
    assert "FailLayerScale = 0.4f" in audio
    assert "FailDuckSeconds = 0.55f" in audio
    assert "FailDuckScale = 0.3f" in audio
    assert "RetryScale = 0.75f" in audio

    for clip, min_size in (
        ("phaserDown3.ogg", 8000),
        ("lowDown.ogg", 4000),
        ("twoTone1.ogg", 4000),
        ("jingles_HIT07.ogg", 4000),
        ("jingles_HIT04.ogg", 4000),
        ("jingles_HIT12.ogg", 4000),
    ):
        path = root / "Assets/Resources/Audio/Sfx" / clip
        assert path.is_file() and path.stat().st_size > min_size

    game_over = root / "Assets/Resources/Audio/Music/GameOver.ogg"
    assert game_over.is_file() and game_over.stat().st_size > 20000
    assert game_over.read_bytes()[:4] == b"OggS"

    assert "En kvar. Nästan!" in loc
    assert "Nästan — {0} kvar." in loc
    assert "{0} kvar." in loc
    assert "0.41" not in loc
    assert "Release" not in loc

    assert "NotifyPlayerDestroyed(cause, enemyKind)" in health
    assert "CombatJuice.PlayerDamaged(true)" in health
    juice = (root / "Assets/Scripts/Combat/CombatJuice.cs").read_text(encoding="utf-8")
    lethal = juice.split("public static void PlayerDamaged(bool lethal)")[1].split("public static void")[0]
    assert "if (lethal)" in lethal
    assert "return;" in lethal

    assert "SeekerFireCooldown = 0.85f" in loadout
    assert "SeekerFireCooldown" in shooter
    assert "AstroFloor_v2" in factory
    assert "Arena_AstroFloor_v2" in factory
    assert "box.isTrigger = true" in factory
    assert "_body.linearVelocity = dir * speed" in seeker
    assert "Image.Type.Filled" in ui
    assert "_hullFill.fillAmount" in ui
    assert "forceModuleActive" not in bootstrap
    assert "com.unity.modules.vr" not in manifest
    assert "com.unity.modules.xr" not in manifest
    assert "com.unity.modules.vr" not in lock
    assert "com.unity.modules.xr" not in lock
    assert "0.43-difficulty-economy" in checklist
    assert "Hub-open smoke" in checklist
    assert "phaserDown3" in readme and "twoTone1" in readme
    assert "Almost had it" in readme
    assert "phaserDown3" in credits and "twoTone1" in credits
    assert "PlayWaveFail" in audio


def test_difficulty_economy_043() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    settings = (root / "Assets/Scripts/Core/DifficultySettings.cs").read_text(encoding="utf-8")
    session = (root / "Assets/Scripts/Core/GameSession.cs").read_text(encoding="utf-8")
    manager = (root / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    waves = (root / "Assets/Scripts/Core/WaveManager.cs").read_text(encoding="utf-8")
    loadout = (root / "Assets/Scripts/Core/LoadoutState.cs").read_text(encoding="utf-8")
    catalog = (root / "Assets/Scripts/Core/ShopCatalog.cs").read_text(encoding="utf-8")
    projectile = (root / "Assets/Scripts/Player/Projectile.cs").read_text(encoding="utf-8")
    shooter = (root / "Assets/Scripts/Player/ShipShooter.cs").read_text(encoding="utf-8")
    health = (root / "Assets/Scripts/Player/ShipHealth.cs").read_text(encoding="utf-8")
    pickup = (root / "Assets/Scripts/Combat/Pickup.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    loc = (root / "Assets/Scripts/Core/Loc.cs").read_text(encoding="utf-8")
    bootstrap = (root / "Assets/Scripts/Content/GameBootstrap.cs").read_text(encoding="utf-8")
    readme = (root / "README.md").read_text(encoding="utf-8")
    credits = (root / "CREDITS.md").read_text(encoding="utf-8")
    checklist = (root / "MERGE_CHECKLIST.md").read_text(encoding="utf-8")
    manifest = (root / "Packages/manifest.json").read_text(encoding="utf-8")
    lock = (root / "Packages/packages-lock.json").read_text(encoding="utf-8")

    assert 'PrefsKey = "agr.difficulty"' in settings
    assert "enum DifficultyGrade" in settings
    assert "StartLivesCount = 3" in settings
    assert "EasyStartLives = 3" in settings
    assert "NormalStartLives = 3" in settings
    assert "HardStartLives = 3" in settings
    assert "MaxLives = 5" in settings
    assert "return StartLivesCount" in settings
    assert "EasyExtraLifeChance = 0.10f" in settings
    assert "NormalExtraLifeChance = 0.045f" in settings
    assert "HardExtraLifeChance = 0.02f" in settings
    assert "EasyWaveClearCredits = 185" in settings
    assert "NormalWaveClearCredits = 150" in settings
    assert "HardWaveClearCredits = 110" in settings
    assert "EasyPlayerHullBonus = 1" in settings
    assert "ScaleEnemyHp" in settings
    assert "ScaleIncomingDamage" in settings
    assert "PlayerPrefs.SetString(PrefsKey" in settings

    assert "TryLoseLife" in session and "TryGainLife" in session
    assert "ResetLives" in session
    assert "ResetRun" in session
    assert "ResetFullRun" in manager
    assert "TryRespawnAfterLifeLoss" in manager
    assert "FailRun" in manager
    assert "ResetFullRun()" in manager.split("public void StartWave()")[1].split("if (_hangarPreview")[0]
    assert "HeartLobeL" in factory and "HeartPoint" in factory
    assert "DressExtraLifeHeart" in factory
    heart_fn = factory.split("private static void DressExtraLifeHeart")[1].split("public void")[0]
    assert "HeartLobeL" in heart_fn and "HeartLobeR" in heart_fn
    assert "PlusH" not in heart_fn
    assert 'CreatePrimitive(PrimitiveType.Sphere, "Mesh"' not in heart_fn
    assert "DespawnAll" not in manager.split("private bool TryRespawnAfterLifeLoss()")[1].split("private void")[0]
    assert "GrantRespawnIFrames" in health and "GrantRespawnIFrames" in manager
    assert "AnnounceLifeLost" in ui and "AnnounceLifeLost" in manager
    assert "MaybeDropExtraLife" in factory
    assert "Pickup_ExtraLife" in factory
    assert "ExtraLife" in pickup
    assert "ExtraLifeTimeoutSeconds" in settings and "ExtraLifeTimeoutSeconds" in factory
    assert "EnemyKind.Swarmling" in factory.split("public void MaybeDropExtraLife")[1].split("public ")[0]
    assert "ExtraAsteroids" in waves and "ExtraEnemyCount" in waves
    assert "DifficultySettings.WaveClearCredits" in manager

    assert "SeekerFireCooldown = 0.85f" in loadout
    assert "SeekerSpeedScale = 0.58f" in loadout
    assert "SeekerDamagePenalty = 1" in loadout
    assert "SeekerTurnDegrees = 140f" in projectile
    assert "SeekerSpeedScale" in shooter and "SeekerDamagePenalty" in shooter

    def item_cost(upgrade_id: str) -> int:
        block = catalog.split(f"UpgradeId.{upgrade_id}")[1].split("new ShopItem")[0]
        for line in block.splitlines():
            token = line.strip().rstrip(",")
            if token.isdigit():
                return int(token)
        raise AssertionError(f"missing cost for {upgrade_id}")

    costs = {
        name: item_cost(name)
        for name in (
            "BodyUpgrade01",
            "BodyUpgrade02",
            "NoseHardpoint",
            "NoseUpgrade02",
            "NoseUpgrade03",
            "RapidFire",
            "EngineUpgrade02",
            "EngineUpgrade03",
            "Overcharger",
            "Afterburner",
            "SpreadBolt",
            "Pierce",
            "TwinGuns",
            "Seeker",
            "Ricochet",
            "ShieldCell",
            "ShieldMatrix",
        )
    }
    assert costs["ShieldCell"] == 80
    assert costs["RapidFire"] == 100
    assert costs["SpreadBolt"] == 110
    assert costs["Seeker"] == 125
    assert costs["TwinGuns"] == 140
    assert costs["Pierce"] == 155
    assert costs["Ricochet"] == 170
    assert costs["BodyUpgrade02"] == 175
    assert costs["ShieldMatrix"] == 185
    assert costs["EngineUpgrade03"] == 190
    assert costs["NoseUpgrade03"] == 200
    assert costs["Overcharger"] == 230
    assert costs["Afterburner"] == 230
    assert min(costs.values()) == 80

    assert "ArenaMusicScale = 0.65f" in audio
    assert "HighWaveMusicWave = 8" in audio
    assert 'Resources.Load<AudioClip>("Audio/Music/MissionPlausible")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Music/TimeDriving")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Music/OutThere")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Music/spacelifeNo14")' in audio
    assert "HangarMusicScale = 0.48f" in audio
    assert "CreditsLoopScale = 0.55f" in audio
    assert "BoltPitchJitter = 0.03f" in audio
    assert "SpreadShotScale = 1.05f" in audio
    assert "PierceShotScale = 1.12f" in audio
    assert "TwinLayerScale = 0.45f" in audio
    assert "SeekerShotScale = 0.72f" in audio
    assert "RicochetShotScale = 0.88f" in audio
    assert "RetryScale = 0.75f" in audio
    seeker_fn = audio.split("public void PlayShootSeeker()")[1].split("public void")[0]
    assert "SeekerShotScale" in seeker_fn
    assert "jingles_" not in seeker_fn
    twin_fn = audio.split("public void PlayShootTwin()")[1].split("public void")[0]
    assert "TwinLayerScale" in twin_fn
    retry_fn = audio.split("public void PlayRetry()")[1].split("public void")[0]
    assert "RetryScale" in retry_fn
    assert "TwinLayerScale" not in retry_fn

    assert "DifficultyPanel" in ui
    assert "OnPickDifficulty" in ui
    assert "LivesPips" in ui
    assert "LivesHud" in ui
    assert "BuildDifficultyPicker" in ui
    assert "Svår" in loc and "Easy" in loc
    assert "LIV FÖRLORAT" in loc
    assert "DifficultySettings.EnsureLoaded" in bootstrap
    assert "ResetLives(DifficultySettings.StartLives)" in bootstrap

    for rel, min_size in (
        ("Assets/Resources/Audio/Music/MissionPlausible.ogg", 80000),
        ("Assets/Resources/Audio/Music/TimeDriving.ogg", 60000),
    ):
        path = root / rel
        assert path.is_file() and path.stat().st_size > min_size
        assert path.read_bytes()[:4] == b"OggS"

    assert "0.43-difficulty-economy" in checklist
    assert "no 0.44" in checklist
    preview = (root / "Assets/Scripts/Hangar/HangarShipPreview.cs").read_text(encoding="utf-8")
    pad = (root / "Assets/Scripts/Core/GamepadInput.cs").read_text(encoding="utf-8")
    follow = (root / "Assets/Scripts/Player/FollowCamera.cs").read_text(encoding="utf-8")
    visuals = (root / "Assets/Scripts/Player/ShipVisuals.cs").read_text(encoding="utf-8")
    ship = (root / "Assets/Scripts/Player/ShipController.cs").read_text(encoding="utf-8")
    inputs = (root / "ProjectSettings/InputManager.asset").read_text(encoding="utf-8")
    assert "IdleSpinDegrees = 18f" in preview
    assert "PreviewX = 6.35f" in preview
    assert "StudioZ = -140f" in preview
    assert "ShowcaseScale = 1.25f" in preview
    assert "ShowcaseScale = 2.25f" not in preview
    assert "CameraFov = 40f" in preview
    assert "CameraFov = 32f" not in preview
    assert "new Vector3(0.2f, 3.7f, -11.5f)" in preview
    assert "new Vector3(0.2f, 2.05f, -5.7f)" not in preview
    assert "farClipPlane = 24f" in preview
    assert "PlayScale = 1f" in preview
    assert "ApplyPreviewScale" in preview
    assert "PreviewLayer = 8" in preview
    assert "BindViewport" in preview
    assert "RenderTexture" in preview
    assert "HangarPreviewCamera" in preview
    assert "IsStudioNode" in preview
    assert "NotifyVisualsChanged" in preview
    assert "stereoTargetEye" in preview
    assert "Camera.allCameras" in preview
    assert "HideDefaultRenderers" in preview
    assert "renderer.enabled = false" in preview
    assert "GL.Clear" in preview
    assert "[DefaultExecutionOrder(10000)]" in preview
    assert "ShipPreviewFrame" in ui
    assert "PreviewViewport" in ui
    assert "PreviewOuterBezel" in ui
    assert "BuildShipPreviewFrame" in ui
    assert "EnsureShipPreviewFrame" in ui
    assert "EnsureHangarPreview" in ui
    assert "ForcePreviewChrome" in ui
    assert "ShipPreviewCanvasName" in ui
    assert "GraphicRaycaster" in ui
    assert "HangarPanelMin" in ui and "ShipPreviewMin" in ui
    assert "0.014f, 0.035f" in ui and "0.55f, 0.725f" in ui
    assert "0.562f, 0.080f" in ui and "0.986f, 0.708f" in ui
    assert "0.575f, 0.725f" not in ui
    assert "0.590f, 0.085f" not in ui
    assert "0.658f, 0.725f" not in ui
    assert "0.672f, 0.09f" not in ui
    assert "overrideSorting" in ui
    assert "ShipPreviewSortOrder = 80" in ui
    assert "typeof(RectTransform)" in ui
    assert "RawImage" in ui and "BindViewport(_previewViewport)" in ui
    assert "ui.EnsureHangarPreview(ship)" in bootstrap
    assert "EnsureHangarPreview(_ship)" in manager
    assert "ui.ship_preview" in loc
    tags = (root / "ProjectSettings/TagManager.asset").read_text(encoding="utf-8")
    assert "HangarPreview" in tags
    assert "HangarShipPreview" in factory
    assert "PreviewGhostMaterial" in visuals
    assert "WithPreview" in loadout
    assert "PreviewUpgrade" in manager and "PreviewUpgrade" in ui
    assert "ClearUpgradePreview" in manager
    assert "NotifyVisualsChanged" in manager
    layer = preview.split("private void ApplyPreviewLayer")[1].split("private void ApplyWorldCull")[0]
    assert "GetComponent<Light>()" not in layer
    assert "IsStudioNode(node)" in layer
    assert "SetHangarFraming" in follow and "SetHangarFraming" in manager
    assert "UnityEngine.Object.FindAnyObjectByType<FollowCamera>" in manager
    assert manager.count("UnityEngine.Object.FindAnyObjectByType<FollowCamera>") == 2
    assert "class GamepadInput" in pad
    assert "AimX" in pad and "AimY" in pad and "FireTrigger" in pad and "FirePad" in pad
    assert "PadMoveX" in pad and "PadMoveY" in pad
    assert "FireTrigger3" in pad and "FireTrigger6" in pad
    assert "PausePressed" in pad
    assert "ConfirmPressed" in pad and "CancelPressed" in pad
    assert "GamepadInput.FireHeld" in ship
    assert "GamepadInput.AimStick" in ship
    assert "GamepadInput.MoveStick" in ship
    assert "GamepadInput.PadMoveStick" in ship
    assert "SyncHangarPadSelection" in ui
    assert "Navigation.Mode.Automatic" in ui
    assert "GamepadInput.CancelPressed" in ui
    assert "m_Name: AimX" in inputs
    assert "m_Name: AimY" in inputs
    assert "m_Name: FireTrigger" in inputs
    assert "m_Name: FireTrigger3" in inputs
    assert "m_Name: FireTrigger6" in inputs
    assert "m_Name: FirePad" in inputs
    assert "m_Name: Pause" in inputs
    assert "m_Name: PadMoveX" in inputs
    assert "m_Name: PadMoveY" in inputs
    assert "joystick button 0" in inputs
    assert "joystick button 4" in inputs
    assert "joystick button 7" in inputs
    assert "com.unity.inputsystem" not in manifest
    assert "MissionPlausible" in credits and "TimeDriving" in credits
    assert "phaserUp5" in credits and "zap1" in credits
    assert "agr.difficulty" in readme
    assert "MissionPlausible" in readme
    assert "com.unity.modules.vr" not in manifest
    assert "com.unity.modules.xr" not in manifest
    assert "com.unity.modules.vr" not in lock
    assert "com.unity.modules.xr" not in lock

    assert DifficultySettings_scale_hp(10, "easy") == 8
    assert DifficultySettings_scale_hp(10, "normal") == 10
    assert DifficultySettings_scale_hp(10, "hard") == 12
    assert DifficultySettings_scale_hp(3, "easy") == 2
    assert DifficultySettings_scale_hp(4, "hard") == 5
    s = Session()
    s.begin()
    assert s.lose_life()
    assert s.lives == 2
    assert s.lose_life()
    assert s.lives == 1
    assert not s.lose_life()
    assert s.lives == 0


def DifficultySettings_scale_hp(hp: int, grade: str) -> int:
    if hp < 1:
        hp = 1
    if grade == "easy":
        return max(1, (hp * 4) // 5)
    if grade == "hard":
        return max(hp + 1, (hp * 5) // 4)
    return hp


def test_hotfix_042_flags_colliders() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    enemies = (root / "Assets/Scripts/Combat/EnemyKind.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    manifest = (root / "Packages/manifest.json").read_text(encoding="utf-8")
    lock = (root / "Packages/packages-lock.json").read_text(encoding="utf-8")

    assert "LanguageFlagScale = 0.48f" in ui
    assert "LanguageFlagRect" in ui
    assert "0.548f, 0.778f" in ui
    assert "0.475f, 0.72f" not in ui
    assert "raycastTarget = true" in ui.split("private static Image CreateFlagButton")[1].split("private static void")[0]
    assert "0.55f, 0.12f, 0.16f, 0.78f" in ui
    assert "0.08f, 0.36f, 0.52f, 0.78f" in ui

    assert "ColliderCenter" in enemies
    assert "ColliderRadialKeep" not in enemies
    assert "ColliderLengthKeep" not in enemies
    assert "new Vector3(0f, 0.42f, 1.35f)" in enemies
    assert "new Vector3(0f, 0.22f, 0.13f)" in enemies
    assert "new Vector3(0f, 0.55f, 0.34f)" in enemies
    radius = enemies.split("public static float ColliderRadius")[1].split("public static float ColliderHeight")[0]
    height = enemies.split("public static float ColliderHeight")[1].split("public static Vector3 ColliderCenter")[0]
    assert "return 0.48f;" in radius.split("case EnemyKind.Scout:")[1].split("case ")[0]
    assert "return 0.54f;" in radius.split("case EnemyKind.Gunner:")[1].split("case ")[0]
    assert "return 0.46f;" in radius.split("case EnemyKind.Drone:")[1].split("case ")[0]
    assert "return 0.82f;" in radius.split("case EnemyKind.Bomber:")[1].split("case ")[0]
    assert "return 0.44f;" in radius.split("case EnemyKind.Sniper:")[1].split("case ")[0]
    assert "return 0.98f;" in radius.split("case EnemyKind.Brute:")[1].split("case ")[0]
    assert "return 0.62f;" in radius.split("case EnemyKind.Swarm:")[1].split("case ")[0]
    assert "return 3.3f;" in height.split("case EnemyKind.Scout:")[1].split("case ")[0]
    assert "return 1.25f;" in height.split("case EnemyKind.Brute:")[1].split("case ")[0]
    assert "return 1.45f;" in height.split("case EnemyKind.Swarm:")[1].split("case ")[0]
    assert "return 5.3f;" in height.split("case EnemyKind.Sniper:")[1].split("case ")[0]
    assert "return 3.8f;" not in height

    assert "collider.center = EnemyCatalog.ColliderCenter(kind)" in factory
    assert "FitEnemyCollider" in factory
    assert "FitAsteroidCollider" in factory
    assert "TryEnemyMeshBounds" in factory
    assert "TryRendererLocalBounds" in factory
    assert "sharedMesh.bounds" in factory
    fit = factory.split("private static void FitEnemyCollider")[1].split("private static void FitAsteroidCollider")[0]
    assert "ColliderRadialKeep" not in fit
    assert "ColliderLengthKeep" not in fit
    assert "Mathf.Max(size.x, size.y)" in fit
    assert "Mathf.Max(radius * 2f, size.z)" in fit
    rock = factory.split("private static void FitAsteroidCollider")[1].split("private static bool TryEnemyMeshBounds")[0]
    assert "local.extents" in rock
    assert "collider.radius = radius" in rock
    assert "Mathf.Max(local.extents.x" in rock
    create = factory.split("public EnemySeeker CreateEnemy(Vector3 position, Transform player, WaveManager waves, string visualName)")[1].split("public GameObject CreatePickup")[0]
    assert create.index("FitEnemyCollider") < create.index("DressMonsterPresence")
    asteroid_fn = factory.split("private Asteroid CreateAsteroid")[1].split("public GameObject CreatePickup")[0]
    assert asteroid_fn.index("TryVisual") < asteroid_fn.index("FitAsteroidCollider")
    assert "com.unity.modules.vr" not in manifest
    assert "com.unity.modules.xr" not in lock


def clamp_play_y(y: float, play_y: float = 0.4, max_y: float = 3.0) -> float:
    if y != y or y == float("inf") or y == float("-inf") or y < play_y or y > max_y:
        return play_y
    return play_y


def is_out_of_play_y(y: float, floor_y: float = -0.08, max_y: float = 3.0) -> bool:
    if y != y or y == float("inf") or y == float("-inf"):
        return True
    return y < floor_y or y > max_y


def test_asteroid_play_plane_and_turn() -> None:
    from pathlib import Path

    assert clamp_play_y(-4.0) == 0.4
    assert clamp_play_y(-0.09) == 0.4
    assert clamp_play_y(0.0) == 0.4
    assert clamp_play_y(0.4) == 0.4
    assert clamp_play_y(8.0) == 0.4
    assert clamp_play_y(float("nan")) == 0.4
    assert is_out_of_play_y(-0.09)
    assert is_out_of_play_y(-2.0)
    assert is_out_of_play_y(3.01)
    assert not is_out_of_play_y(0.0)
    assert not is_out_of_play_y(0.4)
    ox, oz = wrap_xz(40.0, 0.0, 30.0)
    assert abs(ox + 29.95) < 0.001 and abs(oz) < 0.001

    root = Path(__file__).resolve().parents[1]
    wrap = (root / "Assets/Scripts/Core/ArenaWrap.cs").read_text(encoding="utf-8")
    asteroid = (root / "Assets/Scripts/Combat/Asteroid.cs").read_text(encoding="utf-8")
    waves = (root / "Assets/Scripts/Core/WaveManager.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    ship = (root / "Assets/Scripts/Player/ShipController.cs").read_text(encoding="utf-8")
    manifest = (root / "Packages/manifest.json").read_text(encoding="utf-8")
    lock = (root / "Packages/packages-lock.json").read_text(encoding="utf-8")

    assert "PlayY = 0.4f" in wrap
    assert "FloorY = -0.08f" in wrap
    assert "MaxPlayY = 3f" in wrap
    assert "IsOutOfPlayY" in wrap
    assert "IsOutOfPlay(" in wrap
    assert "public static float ClampY" in wrap
    assert "y < PlayY" in wrap
    assert "y > MaxPlayY" in wrap
    assert "y < FloorY" in wrap
    assert "PlayHeight = 0.4f" in ship
    assert "TurnDegreesPerSecond = 480f" in ship
    assert "TurnDegreesPerSecond = 540f" not in ship
    assert "KeepInPlay" in asteroid
    assert "ClampToPlayPlane" in asteroid
    assert "ArenaWrap.ClampY" in asteroid
    assert "ArenaWrap.PlayY" in asteroid
    assert "new Vector3(ox, 0f, oz)" not in asteroid
    assert "private void LateUpdate()" in asteroid
    assert "IsOutOfPlayY(pos.y)" in waves
    assert "asteroid.KeepInPlay()" in waves
    assert "new Vector3(ox, ArenaWrap.PlayY, oz)" in waves
    assert "new Vector3(ox, 0f, oz)" not in waves
    create = factory.split("private Asteroid CreateAsteroid")[1].split("public GameObject CreatePickup")[0]
    assert "ArenaWrap.PlayY" in create
    assert "CenterAsteroidOnPlayOrigin" in create
    assert create.index("FitAsteroidCollider") < create.index("CenterAsteroidOnPlayOrigin")
    assert "collider.center = Vector3.zero" in factory
    assert "StripImportedFloorColliders" in factory
    assert "existing[i].enabled = false" in factory
    dress = factory.split("private static void DressAstroFloorColliders")[1].split("private bool TryVisual")[0]
    assert "StripImportedFloorColliders(visual)" in dress
    assert "box.isTrigger = true" in dress
    assert "com.unity.modules.vr" not in manifest
    assert "com.unity.modules.xr" not in lock


def _input_axis_blocks(text: str) -> list[dict[str, str]]:
    blocks: list[dict[str, str]] = []
    current: dict[str, str] | None = None
    for line in text.splitlines():
        if line.startswith("  - serializedVersion:"):
            if current is not None:
                blocks.append(current)
            current = {}
            continue
        if current is None or ":" not in line:
            continue
        key, value = line.strip().split(":", 1)
        current[key.strip()] = value.strip()
    if current:
        blocks.append(current)
    return blocks


def test_hotfix_043_gamepad() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    pad = (root / "Assets/Scripts/Core/GamepadInput.cs").read_text(encoding="utf-8")
    ship = (root / "Assets/Scripts/Player/ShipController.cs").read_text(encoding="utf-8")
    inputs = (root / "ProjectSettings/InputManager.asset").read_text(encoding="utf-8")
    manifest = (root / "Packages/manifest.json").read_text(encoding="utf-8")
    lock = (root / "Packages/packages-lock.json").read_text(encoding="utf-8")
    settings = (root / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    readme = (root / "README.md").read_text(encoding="utf-8")
    checklist = (root / "MERGE_CHECKLIST.md").read_text(encoding="utf-8")

    axes = _input_axis_blocks(inputs)
    by_name: dict[str, list[dict[str, str]]] = {}
    for axis in axes:
        by_name.setdefault(axis.get("m_Name", ""), []).append(axis)

    joy_move_x = [a for a in by_name["Horizontal"] if a.get("type") == "2"]
    joy_move_y = [a for a in by_name["Vertical"] if a.get("type") == "2"]
    assert len(joy_move_x) == 1 and joy_move_x[0]["axis"] == "0"
    assert len(joy_move_y) == 1 and joy_move_y[0]["axis"] == "1" and joy_move_y[0]["invert"] == "1"
    assert by_name["PadMoveX"][0]["type"] == "2" and by_name["PadMoveX"][0]["axis"] == "0"
    assert by_name["PadMoveY"][0]["type"] == "2" and by_name["PadMoveY"][0]["axis"] == "1"
    assert by_name["PadMoveY"][0]["invert"] == "1"

    fire = by_name["FireTrigger"][0]
    assert fire["type"] == "2" and fire["axis"] == "9"
    assert by_name["FireTrigger3"][0]["type"] == "2" and by_name["FireTrigger3"][0]["axis"] == "2"
    assert by_name["FireTrigger6"][0]["type"] == "2" and by_name["FireTrigger6"][0]["axis"] == "5"
    assert by_name["AimX"][0]["type"] == "2" and by_name["AimX"][0]["axis"] == "3"
    assert by_name["AimY"][0]["type"] == "2" and by_name["AimY"][0]["axis"] == "4"
    assert by_name["AimY"][0]["invert"] == "1"
    assert by_name["CycleFire"][0]["positiveButton"] == "joystick button 4"
    assert by_name["FirePad"][0]["positiveButton"] == "joystick button 0"
    assert by_name["Pause"][0]["positiveButton"] == "joystick button 7"

    assert "TriggerHeld(FireTrigger)" in pad
    assert "TriggerHeld(FireTrigger3)" in pad
    assert "TriggerHeld(FireTrigger6)" in pad
    assert "KeyCode.JoystickButton4" in pad
    assert "PadMoveStick" in pad
    aim = ship.split("private void Aim()")[1].split("private void AimDirection")[0]
    assert "GamepadInput.AimStick" in aim
    assert "GamepadInput.PadMoveStick" in aim
    assert "activeInputHandler: 0" in settings
    assert "com.unity.inputsystem" not in manifest
    assert "com.unity.modules.vr" not in manifest
    assert "com.unity.modules.xr" not in lock
    assert "left stick" in readme.lower()
    assert "joystick button 4" in readme
    assert "FireTrigger" in checklist and "PadMoveX" in checklist
    assert "no 0.44" in checklist


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
    test_scout_gunner_medals_036()
    test_sniper_fardrift_037()
    test_steam_world3_038()
    test_ui_fonts_039()
    test_event_system_persist()
    test_shader_cs1503_gate()
    test_monsters_arenas_040()
    test_weapons_upgrades_040b()
    test_art_parity_040c()
    test_end_credits_040d()
    test_localization_040()
    test_astro_env_040()
    test_fair_death_042()
    test_hotfix_042_flags_colliders()
    test_difficulty_economy_043()
    test_hotfix_043_gamepad()
    test_asteroid_play_plane_and_turn()
    print("Week 1 logic tests passed (Hangar → Play → Clear/Fail + shop persist)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
