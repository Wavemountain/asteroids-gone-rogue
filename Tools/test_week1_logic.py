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
        self.campaign_won = False
        self.wave_took_hit = False
        self.extra_life_streak = 0
        self.world_cleared = 0

    @property
    def can_start(self) -> bool:
        return self.phase in (Phase.HANGAR, Phase.WAVE_CLEAR, Phase.FAILED)

    def begin(self) -> None:
        assert self.can_start
        self.fail_reason = ""
        self.campaign_won = False
        self.wave_took_hit = False
        self.world_cleared = 0
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
        self.world_cleared = _world_index(self.wave) if _is_world_boundary(self.wave) else 0
        self.wave += 1
        self.phase = Phase.WAVE_CLEAR

    def complete_campaign(self, bonus: int = 100, credits: int = 150) -> None:
        assert self.phase == Phase.PLAYING
        self.last_resolved_wave = self.wave
        self.last_credits_awarded = credits
        self.score += bonus
        self.credits += credits
        self.last_run_score = self.score
        self.campaign_won = True
        self.phase = "CampaignClear"

    def note_extra_life(self) -> None:
        self.extra_life_streak += 1

    def note_life_lost(self) -> None:
        self.extra_life_streak = 0

    def fail(self, reason: str | None = None) -> None:
        assert self.phase == Phase.PLAYING
        self.fail_reason = reason or "Unknown cause"
        self.last_resolved_wave = self.wave
        self.last_credits_awarded = 0
        self.last_run_score = self.score
        self.world_cleared = 0
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
        if self.lives <= 0:
            return False
        self.note_life_lost()
        return True

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
    assert "Start = launch wave" in ui
    assert "Abort (Esc)" not in ui.split("HangarHintBody")[1].split("FirstWaveCoach")[0]
    assert "LT utility" in ui
    assert "LB cycle primary" in ui
    assert "RT fire" in ui
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
    assert "HitPunchScale = 0.55f" in audio
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
    assert '_settingsGear.gameObject.SetActive(!playing)' in ui

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
    assert "HitPunchScale = 0.55f" in audio
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
    assert "playing ? 12 : 14" in ui
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
    assert "PlayUiClick" in ui.split("private void ToggleSettingsMute()")[1].split("private void")[0]

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


def test_cs0136_local_shadow_gate() -> None:
    """Unity CS0136: a nested local conflicts with the same name anywhere in an enclosing block."""
    validator = _load_week1_validator()
    shadow = validator.local_shadow_violations

    # Inner x0 is declared first; the hull x0 comes later in the same method. Still CS0136.
    bad = """
        class Sample
        {
            void ShopButtonRect()
            {
                if (group == ShopGroup.Doctrine)
                {
                    float x0 = 0.04f;
                    min = new Vector2(x0, 0.08f);
                    max = new Vector2(x0 + 0.44f, 0.92f);
                    return;
                }

                float x0 = 0.02f;
            }
        }
        """
    hits = shadow(bad)
    assert hits, "nested x0 redeclared later in the method must fail the CS0136 gate"
    assert any("x0" in hit and "CS0136" in hit for hit in hits)

    good = """
        class Sample
        {
            void ShopButtonRect()
            {
                if (group == ShopGroup.Doctrine)
                {
                    float dx0 = 0.04f;
                    min = new Vector2(dx0, 0.08f);
                    max = new Vector2(dx0 + 0.44f, 0.92f);
                    return;
                }

                float x0 = 0.02f;
            }
        }
        """
    assert not shadow(good)

    siblings = """
        class Sample
        {
            void M()
            {
                if (a)
                {
                    int x = 1;
                }

                if (b)
                {
                    int x = 2;
                }
            }
        }
        """
    assert not shadow(siblings), "sibling blocks may reuse a local name"


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
    assert warm.count("\n            \"") == 57
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

    assert catalog.count("new ShopItem(") == 23
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
    cycle = (root / "Assets/Scripts/Core/WeaponSlots.cs").read_text(encoding="utf-8").split("PrimaryCycle")[1].split(";")[0]
    assert "FireMode.Bolt" in cycle and "FireMode.Spread" in cycle and "FireMode.Twin" in cycle
    assert "FireMode.Pierce" in cycle
    assert "FireMode.Seeker" not in cycle and "FireMode.Ricochet" not in cycle
    assert "CycleOrder = WeaponSlots.PrimaryCycle" in shooter

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
    assert "SeekerFireCooldown = BaseFireCooldown * SeekerCooldownMul" in loadout
    assert "SeekerFireCooldown" in shooter
    assert "mode == FireMode.Seeker" in shooter.split("public void TryFireUtility()")[1].split("else if (mode == FireMode.Ricochet)")[0]
    assert abs(0.38 * 2.4 - 0.912) < 1e-6
    assert 0.38 * 2.4 >= 0.38 * 2.2
    assert "OverchargerDamageBonus" in loadout
    assert "HasAltFire" in loadout and "HasAltFire" in ui and "HasAltFire" in shooter
    assert "hullIndex % 4" in ui
    assert "LB cycle primary" in ui
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
    assert warm.count("\n            \"") == 57
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
    assert "UiTheme.ApplyButton(_creditsContinue, true, false, false)" in continue_fn
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

    assert "UsFlag" not in ui and "SvFlag" not in ui
    assert "BuildUsFlag" not in ui and "BuildSwedishFlag" not in ui
    assert "LanguagePanel" not in ui
    assert "OnPickLanguage" in ui
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

    assert "SeekerFireCooldown = BaseFireCooldown * SeekerCooldownMul" in loadout
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
    assert warm.count("\n            \"") == 57
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
    assert "SeekerFireCooldown = BaseFireCooldown * SeekerCooldownMul" in loadout
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
    assert "FailedHealthMin = new Vector2(0.562f, 0.480f)" in ui
    assert "FailedHealthMax = new Vector2(0.986f, 0.596f)" in ui
    assert "UiTheme.Danger" in ui.split("private void ApplyFailChrome")[1].split("private void")[0]
    assert "UiFonts.Display()" in ui
    chrome = ui.split("private void ApplyFailChrome")[1].split("public void FlashHit")[0]
    assert "UiTheme.DangerHeader" in chrome
    assert "UiTheme.Primary" in chrome
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

    assert "SeekerFireCooldown = BaseFireCooldown * SeekerCooldownMul" in loadout
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
    assert "NormalWaveClearCredits = 165" in settings
    assert "HardWaveClearCredits = 140" in settings
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

    assert "SeekerFireCooldown = BaseFireCooldown * SeekerCooldownMul" in loadout
    assert "SeekerSpeedScale = 0.58f" in loadout
    assert "SeekerDamagePenalty = 1" in loadout
    assert "SeekerTurnDegrees = 165f" in projectile
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
    assert "0.44-juice-firstrun" in checklist
    assert "no 0.45" in checklist
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
    assert "new Vector3(0.2f, 4.55f, -10f)" in preview
    assert "new Vector3(0.2f, 3.7f, -11.5f)" not in preview
    assert "new Vector3(0f, 0.08f, 0f)" in preview
    assert "new Vector3(0f, 0.42f, 0f)" not in preview
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
    assert "0.014f, 0.080f" in ui and "0.55f, 0.888f" in ui
    assert "0.014f, 0.035f" not in ui and "0.55f, 0.725f" not in ui
    assert "0.562f, 0.080f" in ui and "0.986f, 0.596f" in ui
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
    assert "GamepadInput.UtilityHeld" in ship
    assert "GamepadInput.AimStick" in ship
    assert "GamepadInput.MoveStick" in ship
    assert "GamepadInput.PadMoveStick" in ship
    assert "SyncHangarPadSelection" in ui
    assert "Navigation.Mode.None" in ui
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

    assert "LanguageFlagScale" not in ui
    assert "LanguageFlagRect" not in ui
    assert "BuildUsFlag" not in ui and "BuildSwedishFlag" not in ui
    assert "0.635f, 0.905f" not in ui
    assert "DifficultyMin = new Vector2(0.748f, 0.905f)" in ui
    assert "SettingsGearMin = new Vector2(0.900f, 0.905f)" in ui
    assert "0.548f, 0.778f" not in ui
    assert "0.475f, 0.72f" not in ui
    assert "OnPickLanguage" in ui

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
    assert "UtilityHeld" in pad
    assert "m_Name: UtilityTrigger" in inputs
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
    assert "no 0.45" in checklist


def test_campaign_cap_and_session_best() -> None:
    s = Session()
    s.begin()
    s.wave = 5
    s.add_score(400)
    loadout_mark = "spread"
    s.complete()
    assert s.phase == "WaveClear"
    assert s.wave == 6
    assert s.campaign_won is False
    assert s.world_cleared == 1
    assert s.last_resolved_wave == 5
    assert s.score == 500
    assert s.credits == 150
    assert loadout_mark == "spread"
    s.note_extra_life()
    s.note_extra_life()
    assert s.extra_life_streak == 2
    s.note_life_lost()
    assert s.extra_life_streak == 0


def test_steam_slice_044() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    session = (root / "Assets/Scripts/Core/GameSession.cs").read_text(encoding="utf-8")
    phase = (root / "Assets/Scripts/Core/GamePhase.cs").read_text(encoding="utf-8")
    cap = (root / "Assets/Scripts/Core/CampaignCap.cs").read_text(encoding="utf-8")
    ach = (root / "Assets/Scripts/Core/AchievementCatalog.cs").read_text(encoding="utf-8")
    persist = (root / "Assets/Scripts/Core/AchievementPersist.cs").read_text(encoding="utf-8")
    steam = (root / "Assets/Scripts/Core/SteamAchievements.cs").read_text(encoding="utf-8")
    padnav = (root / "Assets/Scripts/Core/HangarPadNav.cs").read_text(encoding="utf-8")
    pad = (root / "Assets/Scripts/Core/GamepadInput.cs").read_text(encoding="utf-8")
    manager = (root / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    loc = (root / "Assets/Scripts/Core/Loc.cs").read_text(encoding="utf-8")
    summary = (root / "Assets/Scripts/Core/RunSummary.cs").read_text(encoding="utf-8")
    best = (root / "Assets/Scripts/Core/LocalBest.cs").read_text(encoding="utf-8")
    health = (root / "Assets/Scripts/Player/ShipHealth.cs").read_text(encoding="utf-8")
    follow = (root / "Assets/Scripts/Player/FollowCamera.cs").read_text(encoding="utf-8")
    bootstrap = (root / "Assets/Scripts/Content/GameBootstrap.cs").read_text(encoding="utf-8")
    poses = (root / "Assets/Scripts/Content/StoreCapturePoses.cs").read_text(encoding="utf-8")
    director = (root / "Assets/Scripts/Content/StoreCaptureDirector.cs").read_text(encoding="utf-8")
    menu = (root / "Assets/Editor/StoreCaptureMenu.cs").read_text(encoding="utf-8")
    inputs = (root / "ProjectSettings/InputManager.asset").read_text(encoding="utf-8")
    manifest = (root / "Packages/manifest.json").read_text(encoding="utf-8")
    lock = (root / "Packages/packages-lock.json").read_text(encoding="utf-8")
    readme = (root / "README.md").read_text(encoding="utf-8")
    checklist = (root / "MERGE_CHECKLIST.md").read_text(encoding="utf-8")
    hub = (root / "Docs/HUB_SMOKE.md").read_text(encoding="utf-8")
    store = (root / "Docs/StoreCaptures/README.md").read_text(encoding="utf-8")

    assert "CampaignClear" in phase
    assert "FinalWave = 5" in cap
    assert "FinalWorld = 1" in cap
    assert "CompleteCampaign" in session
    assert "CampaignWon" in session
    assert "WaveTookHit" in session
    assert "ExtraLifeStreak" in session
    assert "MarkWaveHit" in session
    assert "GamePhase.CampaignClear" in session
    assert "CampaignCap.IsFinalWave" in ach
    assert "ShouldUnlockHardClear" in manager
    assert "CompleteCampaign" not in manager
    assert "_session.CompleteWave" in manager
    assert "TryUnlockAchievement" in manager
    assert "SessionBest" in manager
    assert "AchievementPersist" in manager
    assert "NotifyPlayerHit" in manager
    assert "NotifyPlayerHit" in health
    assert "AchievementId.FirstClear" in ach
    assert "AchievementId.NoHitWave" in ach
    assert "AchievementId.HardClear" in ach
    assert "AchievementId.ExtraLifeStreak" in ach
    assert 'AGR_FIRST_CLEAR' in ach
    assert 'AGR_NO_HIT_WAVE' in ach
    assert 'AGR_HARD_CLEAR' in ach
    assert 'AGR_EXTRALIFE_STREAK' in ach
    assert "ExtraLifeStreakNeed = 2" in ach
    assert 'PrefsKey = "agr.achievements.mask"' in ach
    assert "SteamUserStats" in steam
    assert "SetAchievement" in steam
    assert persist.count("SteamAchievements.Unlock") >= 1
    assert "AchievementCatalog.PrefsKey" in persist
    assert "LayoutSelfCheck" in padnav
    assert "Step(PrimarySlot, 0, 1) == ShopSlot(1)" in padnav
    assert "Step(PrimarySlot, 0, -1) == NormalSlot" in padnav
    assert "LangEnSlot" not in padnav and "SettingsSlot" in padnav and "GotItSlot" in padnav
    assert "Step(HardSlot, 1, 0) == SettingsSlot" in padnav
    assert "Step(SettingsSlot, -1, 0) == HardSlot" in padnav
    assert "PadDpadX" in pad and "PadDpadY" in pad
    assert "UiNavDpad" in pad
    assert "UiNavCombined" in pad
    assert "JoystickButton11" in pad
    assert "m_Name: PadDpadX" in inputs
    assert "m_Name: PadDpadY" in inputs
    assert "m_Name: DpadUp" in inputs
    assert "m_Name: DpadDown" in inputs
    assert "m_Name: DpadLeft" in inputs
    assert "m_Name: DpadRight" in inputs
    assert "axis: 6" in inputs and "axis: 7" in inputs
    assert "joystick button 11" in inputs
    assert "joystick button 13" in inputs
    dpad_x = inputs.split("m_Name: PadDpadX")[1].split("m_Name:")[0]
    assert "axis: 6" in dpad_x
    assert "not aliased onto move" in dpad_x.lower() or "not aliased onto move" in inputs
    hangar_h = inputs.split("m_Name: Horizontal")[1].split("m_Name:")[0]
    assert "PadDpad" not in hangar_h
    assert "UiNavCombined" in ui
    assert "HangarPadNav.Step" in ui
    assert "Navigation.Mode.None" in ui
    assert "AnnounceAchievement" in ui
    assert "SessionBest" in ui
    assert "DeathRetryLine" in ui and "DeathRetryLine" in best
    assert "RETRY" in summary
    assert "CampaignClear" in ui
    assert "New Run" in ui
    assert "SECTOR CLEAR" in cap
    assert "CampaignCap.SectorClearTitle" in summary
    assert "HangarWinHint" not in cap and "HangarWinHint" not in summary
    assert "run.next_sector" in summary
    assert "run.fail_retry" in loc
    assert "session.card" in loc and "ach.first" in loc
    assert "StoreCaptureDirector.Ensure" in bootstrap
    assert "HoldCapturePose" in follow
    assert "KeyCode.F12" in director
    assert "KeyCode.F9" in director
    assert "UnityEngine.ScreenCapture.CaptureScreenshot" in director
    assert "com.unity.modules.screencapture" in manifest
    assert "com.unity.modules.screencapture" in lock
    assert 'CapsuleHex = "#D4A04A"' in poses
    assert "01_hangar_shop_health_launchsign" in poses
    assert "02_play_void_astrofloor" in poses
    assert "03_combat_juice_bolt_spread" in poses
    assert "04_brute_swarm_beat" in poses
    assert "05_fail_or_win" in poses
    assert "capsule_ship_complete_34" in poses
    assert "Store Captures" in menu
    assert "#D4A04A" in store
    assert "Ship_Complete" in store
    assert "F12" in store and "F9" in hub
    assert "0.44-steam-slice" in checklist
    assert "0.44-juice-firstrun" in checklist
    assert "no 0.45" in checklist
    assert "future tag is `0.44`" in checklist
    assert "Docs/HUB_SMOKE.md" in readme
    assert "Docs/StoreCaptures" in readme
    assert "SECTOR CLEAR" in readme
    assert "com.rlabrecque.steamworks.net" not in manifest
    assert "com.unity.modules.vr" not in manifest
    assert "com.unity.modules.xr" not in lock
    assert (root / "Docs/HUB_SMOKE.md").is_file()
    assert (root / "Docs/StoreCaptures/README.md").is_file()
    for name in (
        "01_hangar_shop_health_launchsign",
        "02_play_void_astrofloor",
        "03_combat_juice_bolt_spread",
        "04_brute_swarm_beat",
        "05_fail_or_win",
        "capsule_ship_complete_34",
    ):
        assert (root / "Docs/StoreCaptures/placeholders" / f"{name}.txt").is_file()
    assert (root / "Docs/StoreCaptures/out/.gitkeep").is_file()
    assert "6000.6.0f1" in checklist
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")
    assert "ArenaMusicScale = 0.65f" in audio
    assert "ExtraLifePickupScale = 0.82f" in audio


def test_juice_firstrun_044() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")
    pickup = (root / "Assets/Scripts/Combat/Pickup.cs").read_text(encoding="utf-8")
    juice = (root / "Assets/Scripts/Combat/CombatJuice.cs").read_text(encoding="utf-8")
    burst = (root / "Assets/Scripts/Combat/JuiceBurst.cs").read_text(encoding="utf-8")
    waves = (root / "Assets/Scripts/Core/WaveManager.cs").read_text(encoding="utf-8")
    manager = (root / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    pad = (root / "Assets/Scripts/Core/GamepadInput.cs").read_text(encoding="utf-8")
    preview = (root / "Assets/Scripts/Hangar/HangarShipPreview.cs").read_text(encoding="utf-8")
    credits = (root / "CREDITS.md").read_text(encoding="utf-8")
    readme = (root / "README.md").read_text(encoding="utf-8")
    checklist = (root / "MERGE_CHECKLIST.md").read_text(encoding="utf-8")
    manifest = (root / "Packages/manifest.json").read_text(encoding="utf-8")
    lock = (root / "Packages/packages-lock.json").read_text(encoding="utf-8")

    assert "ExtraLifePickupScale = 0.82f" in audio
    assert "ExtraLifeAltScale = 0.78f" in audio
    assert "ExtraLifeMissScale = 0.48f" in audio
    assert "ExtraLifeDuckSeconds = 0.22f" in audio
    assert "ExtraLifeDuckScale = 0.5f" in audio
    assert "HitPunchScale = 0.55f" in audio
    assert "HitPitchJitter = 0.04f" in audio
    assert "LightKillScale = 0.9f" in audio
    assert "LightKillPitchJitter = 0.05f" in audio
    assert "ArenaMusicScale = 0.65f" in audio
    extra_fn = audio.split("public void PlayExtraLifePickup()")[1].split("public void")[0]
    assert "ExtraLifePickupScale" in extra_fn
    assert "DuckMusic(ExtraLifeDuckSeconds, ExtraLifeDuckScale)" in extra_fn
    assert "PlayHangarPurchase" not in extra_fn
    miss_fn = audio.split("public void PlayExtraLifeMiss()")[1].split("public void")[0]
    assert "ExtraLifeMissScale" in miss_fn
    assert "DuckMusic" not in miss_fn
    assert 'Resources.Load<AudioClip>("Audio/Sfx/powerUp7")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/threeTone2")' in audio
    punch = audio.split("public void PlayHit()")[1].split("public void")[0]
    assert "PlayPooledPitched(_hits" in punch
    assert "HitPitchJitter" in punch
    assert "HitPunchScale" in punch
    light = audio.split("public void PlayEnemyDeath(EnemyKind kind)")[1].split("public ")[0]
    assert "LightKillScale" in light
    assert "LightKillPitchJitter" in light
    bolt = audio.split("public void PlayShoot()")[1].split("public void")[0]
    spread = audio.split("public void PlayShootSpread()")[1].split("public void")[0]
    pierce = audio.split("public void PlayShootPierce()")[1].split("public void")[0]
    twin = audio.split("public void PlayShootTwin()")[1].split("public void")[0]
    seeker = audio.split("public void PlayShootSeeker()")[1].split("public void")[0]
    ricochet = audio.split("public void PlayShootRicochet()")[1].split("public void")[0]
    assert "BoltPitchJitter" in bolt
    assert "SpreadShotScale" in spread
    assert "PierceShotScale" in pierce
    assert "TwinLayerScale" in twin
    assert "SeekerShotScale" in seeker
    assert "RicochetShotScale" in ricochet

    assert "PlayExtraLifePickup" in pickup
    assert "PlayExtraLifeMiss" in pickup
    assert "PlayPickupMinor" in pickup
    assert "PlayHangarPurchase" not in pickup
    assert "DressMustPick" in pickup
    assert "HeartBeacon" in pickup
    assert "class JuiceBurst" in burst
    assert "HeartBloom" in burst
    assert "KillBloom" in juice and "HitSpark" in juice
    assert "ExtraLifeTaken" in juice
    assert "NotifySoftLockAbort" in waves and "NotifySoftLockAbort" in manager
    assert "SetSoftLockHint" in waves and "SetAbortUrgent" in ui
    assert "FirstWaveCoach" in ui
    assert "NavigateHangarPad" in ui
    assert "UiNavStick" in pad
    assert "interactable = !runOver" in ui and "_session.ShopOpen" in ui
    assert "ShowcaseScale = 1.25f" in preview
    assert "CameraFov = 40f" in preview
    assert "new Vector3(0.2f, 4.55f, -10f)" in preview
    assert "powerUp7" in credits and "threeTone2" in credits
    assert "powerUp7" in readme
    assert "0.44-juice-firstrun" in checklist
    assert "no 0.45" in checklist
    assert "future tag is `0.44`" in checklist
    power = root / "Assets/Resources/Audio/Sfx/powerUp7.ogg"
    alt = root / "Assets/Resources/Audio/Sfx/threeTone2.ogg"
    assert power.is_file() and power.stat().st_size > 1000 and power.read_bytes()[:4] == b"OggS"
    assert alt.is_file() and alt.stat().st_size > 1000 and alt.read_bytes()[:4] == b"OggS"
    assert "com.unity.modules.vr" not in manifest
    assert "com.unity.modules.xr" not in lock


def test_ui_theme_pad_menus_044() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    theme = (root / "Assets/Scripts/UI/UiTheme.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    padnav = (root / "Assets/Scripts/Core/HangarPadNav.cs").read_text(encoding="utf-8")
    pad = (root / "Assets/Scripts/Core/GamepadInput.cs").read_text(encoding="utf-8")
    fonts = (root / "Assets/Scripts/UI/UiFonts.cs").read_text(encoding="utf-8")
    manifest = (root / "Packages/manifest.json").read_text(encoding="utf-8")
    lock = (root / "Packages/packages-lock.json").read_text(encoding="utf-8")
    readme = (root / "README.md").read_text(encoding="utf-8")
    checklist = (root / "MERGE_CHECKLIST.md").read_text(encoding="utf-8")
    hub = (root / "Docs/HUB_SMOKE.md").read_text(encoding="utf-8")

    assert "class UiTheme" in theme
    assert 'VoidHex = "#070B12"' in theme
    assert 'SurfaceHex = "#0E1520"' in theme
    assert 'Surface2Hex = "#141C28"' in theme
    assert 'PrimaryHex = "#D4A04A"' in theme
    assert 'SecondaryHex = "#6AA8C8"' in theme
    assert 'AccentHex = "#C8CED6"' in theme
    assert 'DangerHex = "#B85A28"' in theme
    assert 'DisabledHex = "#3A4450"' in theme
    assert 'FocusHex = "#E8C878"' in theme
    assert "SurfaceAlpha = 0.72f" in theme
    assert "FocusFillAlpha = 0.08f" in theme
    assert "HoverBright = 0.10f" in theme
    assert "PressedDark = 0.12f" in theme
    assert "InactiveDesat = 0.40f" in theme
    assert "FocusRingPx = 2f" in theme
    assert "HeaderMin = 28" in theme and "HeaderMax = 46" in theme
    assert "BodyMin = 14" in theme and "BodyMax = 22" in theme
    assert "OwnedCheck" in theme
    assert "BuildPanel" in theme
    assert "MenuButtonColors" in theme
    assert "PaintShopPlate" in theme
    assert "PaintLanguageChip" in theme
    assert "SetPadFocus" in theme
    assert "Arial.ttf" not in theme
    assert "Arial.ttf" not in ui
    assert "Arial.ttf" not in fonts

    assert "UiTheme.BuildPanel" in ui
    assert 'BuildPanel(\n                "HudPlate"' in ui or 'UiTheme.BuildPanel(\n                "HudPlate"' in ui
    assert '"HealthRack"' in ui
    assert "UiTheme.HudPlate" in ui
    assert "UiTheme.DangerHeader" in ui
    assert "UiTheme.Primary" in ui.split("private void ApplyFailChrome")[1]
    chrome = ui.split("private void ApplyFailChrome")[1].split("public void FlashHit")[0]
    assert "if (win &&" not in chrome
    assert "ApplyButton(_primary, true, false, false)" in chrome
    assert "UiTheme.DangerTint" in ui
    assert "ApplyButton(_abortButton, false, true, false)" in ui
    assert "ApplyButton(_primary, true, false, false)" in ui
    assert "ApplyButton(button, false, false, true)" in ui
    flash = ui.split("if (Time.unscaledTime < _worldFlashUntil)")[1].split("private void PulseHangarLaunch")[0]
    assert "UiTheme.Secondary" in flash and "UiTheme.Primary" in flash
    assert "0.82f, 0.94f, 1f" not in flash
    assert "1f, 0.58f, 0.18f" not in flash
    toast = ui.split("private void PulseAchievementToast()")[1].split("private void")[0]
    assert "UiTheme.Primary" in toast and "UiTheme.Focus" in toast
    assert "1f, 0.92f, 0.62f" not in toast
    assert "BuildUsFlag" not in ui and "BuildSwedishFlag" not in ui
    assert "PaintLanguageChip" in ui or "PaintChip" in ui
    assert "InactiveDesat" in theme
    assert 'Loc.T("ui.owned", "OWNED") + UiTheme.OwnedCheck' in ui
    assert "ShopLockedForPhase" in ui
    assert "!runOver" in ui and "_session.ShopOpen" in ui
    assert "owned && weapon" in ui
    assert "!owned && !locked && !tooPoor" in ui
    assert "ShipPreviewFrame" in ui
    assert "LOADOUT" in ui
    assert "StepActivePad" not in ui
    assert "HangarPadNav.StepSelectable(fromSlot, dx, dy, padMask)" in ui
    assert "HangarPadNav.EasySlot" in ui
    assert "HangarPadNav.LangEnSlot" not in ui
    assert "HangarPadNav.HardSlot" in ui or "HangarPadNav.SettingsSlot" in ui
    assert "HangarPadNav.SettingsSlot" in ui
    assert "HangarPadNav.CreditsSlot" in ui
    assert "HangarPadNav.GotItSlot" in ui
    assert "SetPadFocus" in ui
    assert "Navigation.Mode.None" in ui
    assert "UiNavCombined" in ui and "UiNavCombined" in pad
    assert "ConfirmPressed" in pad
    assert "CancelPressed" in pad
    assert "PausePressed" in pad
    assert "JoystickButton1" in ui or "CancelPressed" in ui
    assert "LangEnSlot" not in padnav
    assert "Step(PrimarySlot, 0, -1) == NormalSlot" in padnav
    assert "DominantStepY(0f, -1f, Flick) == 1" in padnav
    assert "DominantStepY(0f, 1f, Flick) == -1" in padnav
    assert "LayoutSelfCheck" in padnav

    assert "#D4A04A" in checklist and "#6AA8C8" in checklist
    assert "UiTheme" in checklist and "UiTheme" in readme
    assert "no 0.45" in checklist
    assert "future tag is `0.44`" in checklist
    assert "6000.6.0f1" in checklist
    assert "LS / D-pad" in hub or "D-pad" in hub
    assert "difficulty" in hub.lower() and "mute" in hub.lower()
    assert "com.unity.modules.vr" not in manifest
    assert "com.unity.modules.xr" not in lock
    assert "com.unity.modules.screencapture" in manifest
    assert (root / "Assets/Scripts/UI/UiTheme.cs.meta").is_file()


def _overlap(a: tuple[float, float, float, float], b: tuple[float, float, float, float]) -> bool:
    ax0, ay0, ax1, ay1 = a
    bx0, by0, bx1, by1 = b
    return ax0 < bx1 and ax1 > bx0 and ay0 < by1 and ay1 > by0


def test_hangar_wave_clear_layout() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    padnav = (root / "Assets/Scripts/Core/HangarPadNav.cs").read_text(encoding="utf-8")
    hangar = (root / "Assets/Scripts/Hangar/HangarShop.cs").read_text(encoding="utf-8")
    manifest = (root / "Packages/manifest.json").read_text(encoding="utf-8")
    lock = (root / "Packages/packages-lock.json").read_text(encoding="utf-8")

    assert "HangarPanelMin = new Vector2(0.014f, 0.080f)" in ui
    assert "HangarPanelMax = new Vector2(0.55f, 0.888f)" in ui
    assert "ShipPreviewMin = new Vector2(0.562f, 0.080f)" in ui
    assert "ShipPreviewMax = new Vector2(0.986f, 0.596f)" in ui
    assert "ShopGridTop = 0.665f" in ui
    assert "ShopCellHeight = 0.094f" in ui
    assert "ShopCellGutter = 0.016f" in ui
    assert '"HullCol"' in ui and '"WeaponsCol"' in ui and '"DefenseCol"' in ui
    assert "hullIndex % 4" in ui
    assert "TryBuy" in hangar and "CanApply" in hangar

    top_bar = (0.012, 0.905, 0.988, 0.995)
    hangar_panel = (0.014, 0.080, 0.55, 0.888)
    preview = (0.562, 0.080, 0.986, 0.596)
    doctrine = (0.562, 0.608, 0.986, 0.898)
    hint = (0.14, 0.008, 0.86, 0.072)
    credits_btn = (0.014, 0.010, 0.128, 0.070)
    gear = (0.900, 0.905, 0.988, 0.995)
    diff = (0.748, 0.905, 0.888, 0.995)
    assert not _overlap(top_bar, hangar_panel)
    assert not _overlap(preview, doctrine)
    assert not _overlap(doctrine, top_bar)
    assert not _overlap(doctrine, hangar_panel)
    assert doctrine[1] > preview[3]
    assert doctrine[3] < top_bar[1]
    assert not _overlap(hint, hangar_panel)
    assert not _overlap(credits_btn, hangar_panel)
    assert not _overlap(gear, diff)
    assert not _overlap(gear, hangar_panel)
    assert not _overlap(diff, hangar_panel)
    assert gear[0] > diff[2]
    assert "AudioPanel" not in ui and "BuildAudioControls" not in ui
    assert abs(hangar_panel[1] - preview[1]) < 0.0001
    assert hangar_panel[3] < top_bar[1]
    assert hint[3] < hangar_panel[1]

    strip = (0.02, 0.82, 0.98, 0.995)
    cta = (0.03, 0.735, 0.97, 0.800)
    headers = (0.02, 0.675, 0.98, 0.728)
    status = (0.03, 0.016, 0.97, 0.088)
    assert not _overlap(strip, cta)
    assert not _overlap(cta, headers)
    assert cta[3] < strip[1]
    assert headers[3] < cta[1]
    assert status[3] < 0.665 - 5 * (0.094 + 0.016) + 0.016 + 0.02

    assert "new Vector2(0.03f, 0.735f)" in ui and "new Vector2(0.97f, 0.800f)" in ui
    assert "ClampOneLine" in ui
    assert '_statusBase = Loc.T("ui.hangar_controls"' not in ui
    assert "_hud.gameObject.SetActive(playing)" in ui
    assert "_credits.gameObject.SetActive(false)" in ui
    assert "y > 0f ? -1 : 1" in padnav
    assert "DominantStepY(0f, -1f, Flick) == 1" in padnav

    def dominant_y(x: float, y: float, flick: float) -> int:
        ax, ay = abs(x), abs(y)
        if ax < flick and ay < flick:
            return 0
        if ay > ax:
            return -1 if y > 0 else 1
        return 0

    assert dominant_y(0.0, -1.0, 0.55) == 1
    assert dominant_y(0.0, 1.0, 0.55) == -1
    assert dominant_y(1.0, 0.0, 0.55) == 0
    assert "com.unity.modules.vr" not in manifest
    assert "com.unity.modules.xr" not in lock
    assert "com.unity.inputsystem" not in manifest
    assert "com.unity.textmeshpro" not in manifest


def test_dual_fire_v1() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    loadout = (root / "Assets/Scripts/Core/LoadoutState.cs").read_text(encoding="utf-8")
    slots = (root / "Assets/Scripts/Core/WeaponSlots.cs").read_text(encoding="utf-8")
    shooter = (root / "Assets/Scripts/Player/ShipShooter.cs").read_text(encoding="utf-8")
    ship = (root / "Assets/Scripts/Player/ShipController.cs").read_text(encoding="utf-8")
    pad = (root / "Assets/Scripts/Core/GamepadInput.cs").read_text(encoding="utf-8")
    hangar = (root / "Assets/Scripts/Hangar/HangarShop.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    loc = (root / "Assets/Scripts/Core/Loc.cs").read_text(encoding="utf-8")
    summary = (root / "Assets/Scripts/Core/RunSummary.cs").read_text(encoding="utf-8")
    catalog = (root / "Assets/Scripts/Core/ShopCatalog.cs").read_text(encoding="utf-8")
    inputs = (root / "ProjectSettings/InputManager.asset").read_text(encoding="utf-8")
    readme = (root / "README.md").read_text(encoding="utf-8")
    checklist = (root / "MERGE_CHECKLIST.md").read_text(encoding="utf-8")
    manifest = (root / "Packages/manifest.json").read_text(encoding="utf-8")
    lock = (root / "Packages/packages-lock.json").read_text(encoding="utf-8")

    assert "enum WeaponSlot" in slots
    assert "SeekerCooldownMul = 2.4f" in slots
    assert "RicochetCooldownMul = 1.5f" in slots
    assert "SpreadCooldownMul = 1.35f" in slots
    assert "TwinCooldownMul = 1.1f" in slots
    assert abs(0.38 * 2.4 - 0.912) < 1e-6
    assert abs(0.38 * 1.5 - 0.57) < 1e-6
    assert "RicochetBounces = 2" in loadout
    assert "ProjectileDamage / 3" in loadout
    assert "HasUtility" in loadout and "PrimaryMode" in loadout
    assert "AutoEquipAfterPurchase" in loadout and "AutoEquipAfterPurchase" in hangar
    assert "TryEquip" in loadout and "TryEquip" in hangar and "TryEquip" in ui
    assert "public void TryFireUtility()" in shooter
    assert "RapidFireCooldown" in shooter.split("public void TryFire()")[1].split("public void TryFireUtility()")[0]
    util_fn = shooter.split("public void TryFireUtility()")[1].split("private void FireModeShot")[0]
    assert "LoadoutState.SeekerFireCooldown" in util_fn
    assert "LoadoutState.RicochetFireCooldown" in util_fn
    assert "loadout.FireCooldown" not in util_fn
    assert "AfterburnerCooldown" not in util_fn
    assert "RapidFireCooldown" not in util_fn
    assert "GamepadInput.UtilityHeld" in ship
    assert "GamepadInput.CyclePrevPressed" in ship
    assert "TriggerHeld(FireTrigger3)" in pad.split("public static bool UtilityHeld()")[1].split("public static bool CyclePressed()")[0]
    fire_held = pad.split("public static bool FireHeld()")[1].split("public static bool UtilityHeld()")[0]
    assert "TriggerHeld(FireTrigger3)" not in fire_held
    assert "TriggerHeld(FireTrigger)" in fire_held
    assert "TriggerHeld(FireTrigger6)" in fire_held
    assert "KeyCode.E" in pad
    assert "GetMouseButton(1)" in pad
    assert "GetMouseButtonDown(1)" not in pad
    assert "KeyCode.JoystickButton5" in pad
    assert "m_Name: UtilityTrigger" in inputs
    assert "LT utility · LB cycle primary · RT fire" in ui
    assert "LT utility · LB cykla primary · RT skjut" in loc
    assert "Buy Seeker → hold LT" in summary
    assert "PRIMARY {0}" in ui and "UTILITY {0}" in ui
    assert "ui.hud_empty" in loc and "tom" in loc
    assert "UtilityHud" in ui and "Radial360" in ui
    assert "Primary slot" in catalog and "Utility slot" in catalog
    assert "empty utility" in readme.lower() or "Empty slot" in readme
    assert "UtilityTrigger" in checklist
    assert "com.unity.inputsystem" not in manifest
    assert "com.unity.modules.vr" not in lock
    assert "com.unity.textmeshpro" not in manifest
    assert (root / "Assets/Scripts/Core/WeaponSlots.cs.meta").is_file()


def test_doctrine_rail_045() -> None:
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    rules = (root / "Assets/Scripts/Core/DoctrineRules.cs").read_text(encoding="utf-8")
    loadout = (root / "Assets/Scripts/Core/LoadoutState.cs").read_text(encoding="utf-8")
    slots = (root / "Assets/Scripts/Core/WeaponSlots.cs").read_text(encoding="utf-8")
    shooter = (root / "Assets/Scripts/Player/ShipShooter.cs").read_text(encoding="utf-8")
    ship = (root / "Assets/Scripts/Player/ShipController.cs").read_text(encoding="utf-8")
    projectile = (root / "Assets/Scripts/Player/Projectile.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    art = (root / "Assets/Scripts/Content/ArtImport.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    loc = (root / "Assets/Scripts/Core/Loc.cs").read_text(encoding="utf-8")
    catalog = (root / "Assets/Scripts/Core/ShopCatalog.cs").read_text(encoding="utf-8")
    poses = (root / "Assets/Scripts/Content/StoreCapturePoses.cs").read_text(encoding="utf-8")
    settings = (root / "Assets/Scripts/Core/DifficultySettings.cs").read_text(encoding="utf-8")
    hangar = (root / "Assets/Scripts/Hangar/HangarShop.cs").read_text(encoding="utf-8")
    manifest = (root / "Packages/manifest.json").read_text(encoding="utf-8")
    lock = (root / "Packages/packages-lock.json").read_text(encoding="utf-8")

    assert "UnlockWave = 2" in rules
    assert "BarrageGateCost = 55" in rules
    assert "LanceGateCost = 25" in rules
    assert "HunterGateCost = 40" in rules
    assert "OffPathMul = 1.35f" in rules
    assert "FlakFeedCooldownMul = 0.85f" in rules
    assert "FlakFeedHalfAngleBonus = 4f" in rules
    assert "StormPelletCount = 5" in rules
    assert "StormCooldownMul = 1.8f" in rules
    assert "StormSeconds = 1.5f" in rules
    assert "RailHoldSeconds = 0.55f" in rules
    assert "RailDamageMul = 3f" in rules
    assert "RailSpeedMul = 1.2f" in rules
    assert "RailCooldownMul = 1.6f" in rules
    assert "RailMissCancelCooldownMul = 0.5f" in rules
    assert "RailCost = 160" in rules
    assert "OverchargeTwinCooldownMul = 0.9f" in rules
    assert "OverchargePierceBonusTargets = 1" in rules
    assert "SeekerCadenceCooldownMul = 0.75f" in rules
    assert "SeekerCadenceTurnDegrees = 165f" in rules
    assert "TwinSeekCount = 2" in rules
    assert "TwinSeekDamageScale = 0.70f" in rules
    assert "TwinSeekCooldownMul = 1.2f" in rules
    assert "HangarUnlocked" in rules
    assert "PenalizedCost" in rules
    assert "SoftLocked" in loadout and "SetDoctrine" in loadout
    assert "EffectiveCost" in loadout and "EffectiveCost" in hangar
    assert "TryPickDoctrine" in hangar
    assert "FireMode.Rail" in slots
    assert "TickRailCharge" in shooter and "TickRailCharge" in ship
    assert "ArmRailMiss" in projectile and "NotifyRailMiss" in shooter
    assert "SeekerTurnDegrees = 165f" in projectile
    assert "Rail_Hardpoint" in factory and "Rail_Muzzle" in factory and "Rail_Seeker" in factory
    assert "Mat_Ship_Accent_Hot" in factory and "Mat_Ship_Glow" in factory
    assert "Mat_Ship_Hull" in factory and "Mat_Ship_Accent" in factory
    assert "localPosition = Vector3.zero" in factory
    assert "CreateRailChargeVfx" in factory and "RailChargeGlow" in factory
    assert "RailHardpoint" in factory
    warm = art.split("PlayModeAssets")[1].split("};")[0]
    assert "Rail_Hardpoint" in warm and "Rail_Muzzle" in warm and "Rail_Seeker" in warm
    assert "DoctrineCard" in ui and "UiTheme.BuildPanel" in ui
    assert "ui.hud_doctrine" in ui and "ui.doctrine.tip" in ui
    assert "ui.hint_rail" in ui and "ui.hint_dual" in ui
    assert "ui.doctrine.barrage" in loc and "ui.hud_doctrine" in loc
    assert "shop.title.Rail" in loc and "shop.title.FlakFeed" in loc
    assert "shop.title.Storm" in loc and "shop.title.OverchargeLance" in loc
    assert "shop.title.SeekerCadence" in loc and "shop.title.TwinSeek" in loc
    assert "ui.hint_rail" in loc and "ui.hint_dual" in loc
    assert "06_rail_charge" in poses
    assert "NormalWaveClearCredits = 165" in settings
    assert "HardWaveClearCredits = 140" in settings
    assert "SpreadCooldownMul = 1.35f" in slots
    assert "TwinCooldownMul = 1.1f" in slots
    assert 'UpgradeId.SpreadBolt' in catalog and "110," in catalog.split("UpgradeId.SpreadBolt")[1].split("new ShopItem")[0]
    assert "com.unity.modules.vr" not in manifest and "com.unity.modules.xr" not in lock
    lfs_prefix = b"version https://git-lfs.github.com/spec/v1"
    for name in ("Rail_Hardpoint", "Rail_Muzzle", "Rail_Seeker"):
        for folder in ("Assets/Art/Import", "Assets/Resources/Art/Import"):
            path = root / folder / f"{name}.fbx"
            assert path.is_file() and path.stat().st_size > 1000
            assert not path.read_bytes()[:64].startswith(lfs_prefix), f"{path} is an LFS pointer"
            assert path.read_bytes()[:21] == b"Kaydara FBX Binary  \x00"
    assert (root / "Docs/StoreCaptures/placeholders/06_rail_charge.txt").is_file()
    assert "Docs/StoreCaptures/out" in poses or "Docs/StoreCaptures/out" in (root / "Assets/Scripts/Content/StoreCapturePoses.cs").read_text(encoding="utf-8")
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")
    credits = (root / "CREDITS.md").read_text(encoding="utf-8")
    director = (root / "Assets/Scripts/Content/StoreCaptureDirector.cs").read_text(encoding="utf-8")
    menu = (root / "Assets/Editor/StoreCaptureMenu.cs").read_text(encoding="utf-8")
    summary = (root / "Assets/Scripts/Core/RunSummary.cs").read_text(encoding="utf-8")
    ach = (root / "Assets/Scripts/Core/AchievementCatalog.cs").read_text(encoding="utf-8")
    manager = (root / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    theme = (root / "Assets/Scripts/UI/UiTheme.cs").read_text(encoding="utf-8")
    assert "DoctrineBadge = 18" in theme
    assert "OffPathTint" in theme and "PaintOffPathCue" in theme
    assert "UiTheme.DoctrineBadge" in ui
    assert 'Loc.T("ui.off_path", "off-path")' in ui
    assert "ui.off_path" in loc and "av vägen" in loc
    assert "DoctrineRunLine" in summary and "run.doctrine_wave" in summary and "run.doctrine_wave" in loc
    assert "Lance run — wave {1}" in summary or "{0} run — wave {1}" in summary
    assert "PlayRailChargeRise" in audio and "PlayRailRelease" in audio and "PlayDoctrinePick" in audio
    assert "StopRailCharge" in audio and "TickRailHold" in audio
    assert "RailChargeScale = 0.6f" in audio and "RailChargePitch = 0.95f" in audio
    assert "RailHoldLoopScale = 0.2f" in audio
    assert "RailHoldPitchMin = 0.9f" in audio and "RailHoldPitchMax = 1.15f" in audio
    assert "RailHoldFadeSeconds = 0.08f" in audio
    assert "RailShotScale = 1.0f" in audio and "RailShotPitch = 0.92f" in audio
    assert "RailShotPitchJitter = 0.03f" in audio
    assert "RailThumpLayerScale = 0.5f" in audio
    assert "RailDuckSeconds = 0.18f" in audio and "RailDuckScale = 0.6f" in audio
    assert "DoctrinePickScale = 0.68f" in audio
    assert "DoctrinePickDuckSeconds = 0.3f" in audio and "DoctrinePickDuckScale = 0.4f" in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/phaserUp3")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/engineCircular_001")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/laserLarge_002")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/lowFrequency_explosion_001")' in audio
    assert 'Resources.Load<AudioClip>("Audio/Sfx/jingles_NES03")' in audio
    release = audio.split("public void PlayRailRelease()")[1].split("public void")[0]
    assert "laserLarge_000" not in release
    assert "DuckMusic(RailDuckSeconds, RailDuckScale)" in release
    assert "_railRise.Stop()" in audio
    pick = audio.split("public void PlayDoctrinePick()")[1].split("public void")[0]
    assert "confirmation_002" not in pick
    assert "DuckMusic(DoctrinePickDuckSeconds, DoctrinePickDuckScale)" in pick
    assert "PlayRailChargeRise" in shooter and "TickRailHold" in shooter and "StopRailCharge" in shooter
    assert "PlayDoctrinePick" in hangar and "PlayHangarPurchase" in hangar
    assert "phaserUp3" in credits and "engineCircular_001" in credits
    assert "laserLarge_002" in credits and "lowFrequency_explosion_001" in credits
    assert "jingles_NES03" in credits
    for name in (
        "phaserUp3",
        "engineCircular_001",
        "laserLarge_002",
        "lowFrequency_explosion_001",
        "jingles_NES03",
    ):
        ogg = root / "Assets/Resources/Audio/Sfx" / f"{name}.ogg"
        assert ogg.is_file() and ogg.stat().st_size > 1000
        head = ogg.read_bytes()[:64]
        assert head.startswith(b"OggS"), f"{ogg} is not an Ogg file"
        assert not head.startswith(lfs_prefix), f"{ogg} is an LFS pointer"
    muzzle = factory.split("public GameObject CreateRailChargeVfx")[1].split("public void ApplyLoadoutVisuals")[0]
    assert "_projectilePierce" in muzzle
    assert 'TryVisual("Rail_Muzzle", parent, _glow' not in muzzle
    assert "HasPose" in poses and "TryShot" in poses and "06_rail_charge" in poses
    assert "RailCharge" in menu and "CyclePose" in menu and "CyclePose" in director
    for shot in (
        "HangarShop",
        "PlayVoid",
        "CombatJuice",
        "BruteSwarm",
        "FailOrWin",
        "RailCharge",
    ):
        assert shot in poses.split("public static bool HasPose")[1].split("public static bool TryIndex")[0]
    assert "AchievementId.Storm" in ach
    assert "AchievementId.OverchargeLance" in ach
    assert "AchievementId.TwinSeek" in ach
    assert "AGR_STORM" in ach and "AGR_OVERCHARGE_LANCE" in ach and "AGR_TWIN_SEEK" in ach
    assert "ach.storm" in loc and "ach.overcharge" in loc and "ach.twinseek" in loc
    assert "TryUnlockCapstones" in manager and "ShouldUnlockCapstone" in ach
    assert "AnnounceAchievement" in ui
    assert "ShouldUnlockDoctrine" in ach and "ShouldUnlockRailCharge" in ach
    assert "NotifyDoctrinePicked" in manager and "AchievementId.Doctrine" in manager
    assert "NotifyRailCharged" in manager and "AchievementId.RailCharge" in manager
    assert "NotifyDoctrinePicked" in hangar
    release_branch = shooter.split("heldFor + 0.0001f >= DoctrineRules.RailHoldSeconds")[1].split("return;")[0]
    assert "NotifyRailCharged(heldFor)" in release_branch
    cancel = shooter.split("public void CancelCharge()")[1].split("public void ")[0]
    assert "NotifyRailCharged" not in cancel
    assert "ui.doctrine.path.barrage" in ui and "ui.doctrine.path.lance" in ui and "ui.doctrine.path.hunter" in ui
    assert "ui.doctrine.need" in ui and "ui.doctrine.need_either" in ui
    assert "ui.doctrine.hint_title" in ui and "ui.doctrine.hint_body" in ui
    assert "DoctrineHintKey" in ui and "DismissDoctrineIntro" in ui
    assert "Wide shots" in ui and "Breda skott" in loc
    assert "Needs {0}" in ui and "Behöver {0}" in loc
    assert "New Run to swap" in ui
    gate_need = ui.split("private static string DoctrineGateNeed")[1].split("private static string DoctrineLabel")[0]
    assert "ui.locked" not in gate_need
    assert "DoctrinePanelMin" in ui and "0.562f, 0.608f" in ui and "0.986f, 0.898f" in ui


def _cs_int(src: str, name: str) -> int:
    import re

    match = re.search(rf"{name}\s*=\s*(-?\d+)", src)
    assert match, name
    return int(match.group(1))


def _shop_cost(catalog: str, upgrade_id: str) -> int:
    block = catalog.split(f"UpgradeId.{upgrade_id}")[1].split("new ShopItem")[0]
    for line in block.splitlines():
        token = line.strip().rstrip(",")
        if token.isdigit():
            return int(token)
    raise AssertionError(f"missing cost for {upgrade_id}")


def test_doctrine_w1_affordability() -> None:
    """Regression: all three doctrines fit a Normal wave-1 clear, and none fit Hard.

    Credits are the flat wave-clear purse (kills do not pay credits). A doctrine
    costs its gate weapon plus the entry pick. Lance's cheaper legal gate is Twin.
    """
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    rules = (root / "Assets/Scripts/Core/DoctrineRules.cs").read_text(encoding="utf-8")
    settings = (root / "Assets/Scripts/Core/DifficultySettings.cs").read_text(encoding="utf-8")
    catalog = (root / "Assets/Scripts/Core/ShopCatalog.cs").read_text(encoding="utf-8")

    normal = _cs_int(settings, "NormalWaveClearCredits")
    hard = _cs_int(settings, "HardWaveClearCredits")
    assert normal == 165
    assert hard == 140
    assert "OffPathMul = 1.35f" in rules

    spread = _shop_cost(catalog, "SpreadBolt")
    seeker = _shop_cost(catalog, "Seeker")
    twin = _shop_cost(catalog, "TwinGuns")
    pierce = _shop_cost(catalog, "Pierce")
    barrage_gate = _cs_int(rules, "BarrageGateCost")
    lance_gate = _cs_int(rules, "LanceGateCost")
    hunter_gate = _cs_int(rules, "HunterGateCost")
    assert spread == 110 and seeker == 125 and twin == 140 and pierce == 155

    barrage = spread + barrage_gate
    hunter = seeker + hunter_gate
    lance_twin = twin + lance_gate
    lance_pierce = pierce + lance_gate
    # Minimal entry costs: each cheapest path lands on the Normal purse exactly.
    assert barrage == normal
    assert hunter == normal
    assert lance_twin == normal
    assert lance_pierce > normal
    # Hard wave-1 purse cannot buy any of the three doctrines.
    assert barrage > hard and hunter > hard and lance_twin > hard


def test_hotfix_045_rail_cancel_and_duck_merge() -> None:
    import re
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    ship = (root / "Assets/Scripts/Player/ShipController.cs").read_text(encoding="utf-8")
    shooter = (root / "Assets/Scripts/Player/ShipShooter.cs").read_text(encoding="utf-8")
    audio = (root / "Assets/Scripts/Content/AudioCues.cs").read_text(encoding="utf-8")

    set_fn = ship.split("public void SetInputEnabled(bool enabled)")[1].split("public void ResetForWave")[0]
    assert "if (!enabled)" in set_fn
    disabled = set_fn.split("if (!enabled)", 1)[1]
    assert "_shooter.CancelCharge()" in disabled

    cancel = shooter.split("public void CancelCharge()")[1].split("public void ")[0]
    assert "StopRailAudio()" in cancel
    assert "HideCharge()" in cancel
    assert "FireRail" not in cancel
    assert "_nextFireTime" not in cancel
    assert "RailMissCancelCooldownMul" not in cancel
    assert "_charging = false" in cancel

    on_disable = shooter.split("void OnDisable()")[1].split("void OnDestroy()")[0]
    assert "CancelCharge()" in on_disable
    assert "StopRailAudio()" in cancel
    on_destroy = shooter.split("void OnDestroy()")[1].split("}", 1)[0]
    assert "CancelCharge()" in on_destroy

    duck = audio.split("public void DuckMusic(")[1].split("public void ")[0]
    assert re.search(r"now\s*<\s*_duckUntil", duck)
    assert re.search(r"_duckTarget\s*=\s*Mathf\.Min\(_duckTarget,\s*target\)", duck)
    assert re.search(r"_duckUntil\s*=\s*Mathf\.Max\(_duckUntil,\s*now\s*\+\s*duration\)", duck)


def _map_anchors(px0, py0, px1, py1, cx0, cy0, cx1, cy1):
    width = px1 - px0
    height = py1 - py0
    return (
        px0 + cx0 * width,
        py0 + cy0 * height,
        px0 + cx1 * width,
        py0 + cy1 * height,
    )


def _pad_force_primary(selectable):
    mask = list(selectable)
    if mask:
        mask[0] = True
    return mask


def _pad_resolve_fallback(slot, selectable):
    if not selectable:
        return 0
    if 0 <= slot < len(selectable) and selectable[slot]:
        return slot
    return 0


def _pad_step_selectable(slot, dx, dy, selectable, step_fn):
    """Mirrors HangarPadNav.StepSelectable. Invalid focus snaps to slot 0 and does not step."""
    mask = _pad_force_primary(selectable)
    current_ok = 0 <= slot < len(mask) and mask[slot]
    origin = slot if current_ok else 0
    if not current_ok or (dx == 0 and dy == 0):
        return origin
    count = len(mask)
    candidate = step_fn(origin, dx, dy)
    guard = 0
    while guard < count:
        if 0 <= candidate < count and mask[candidate]:
            return candidate
        stepped = step_fn(candidate, dx, dy)
        if stepped == candidate:
            return 0
        candidate = stepped
        guard += 1
    return 0


def _card_is_action(shop_open, chosen, other_path, gate_met, too_poor):
    if not shop_open or chosen or other_path or not gate_met or too_poor:
        return False
    return True


def _primary_restarts(phase):
    return phase == "Failed"


def test_hangar_next_wave_always_selectable() -> None:
    """Next Wave stays on the pad list, wins invalid focus, and is not covered or restarted by mistake."""
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    padnav = (root / "Assets/Scripts/Core/HangarPadNav.cs").read_text(encoding="utf-8")
    rules = (root / "Assets/Scripts/Core/DoctrineRules.cs").read_text(encoding="utf-8")
    session = (root / "Assets/Scripts/Core/GameSession.cs").read_text(encoding="utf-8")
    manager = (root / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    loc = (root / "Assets/Scripts/Core/Loc.cs").read_text(encoding="utf-8")

    assert "NavIncludesPrimary" in padnav
    assert "ResolveFallback" in padnav and "StepSelectable" in padnav
    assert "ForcePrimarySelectable" in padnav
    assert "LockedShopFallsBackToPrimary" in padnav
    assert "NextWaveScreenClear" in padnav
    assert "DoctrinePadCell" in padnav
    assert "padY = deltaY <= shopPitch * 0.5f ? -1 : 5" in padnav
    assert "Accepts(slot, selectable)" in padnav
    assert "cy = 5 + (doctrine / 3)" not in padnav
    assert "HangarPadNav.StepSelectable" in ui
    assert "FocusPrimaryIfSelectionInvalid" in ui
    assert "SelectionNeedsPrimaryFallback" in ui
    assert "EnsurePrimaryClickable" in ui

    escape = ui.split("private void OnHangarEscape()")[1].split("private void")[0]
    assert "OnPrimary" not in escape
    assert "PrimarySlot" in escape
    back = ui.split("private void OnHangarBack()")[1].split("private void")[0]
    assert "OnPrimary" not in back
    assert "DismissHangarHints" in back
    start = ui.split("private void OnHangarStart()")[1].split("private void")[0]
    assert "OnPrimary()" in start
    update = ui.split("private void Update()")[1].split("private void PulseHangarLaunch")[0]
    assert "OnHangarEscape()" in update
    assert "OnHangarStart()" in update
    assert "OnHangarBack()" in update

    default_btn = ui.split("private Button DefaultHangarButton()")[1].split("private void")[0]
    assert "_gotItButton" not in default_btn
    slot_ok = ui.split("private bool SlotIsSelectable")[1].split("private void")[0]
    assert "GotItSlot" in slot_ok and "DoctrineHintSlot" in slot_ok

    pick = ui.split("private void OnPickDoctrine")[1].split("private void RefreshDoctrinePicks")[0]
    assert "CardIsAction" in pick
    assert "StartWave" not in pick
    assert "ResetRun" not in pick
    assert "ResetFullRun" not in pick
    paint = ui.split("private void PaintDoctrineButton")[1].split("private static void FitDoctrineLabel")[0]
    assert "raycastTarget = action" in paint
    assert "New Run to swap" in ui
    assert "CardIsAction" in rules

    start_wave = manager.split("public void StartWave()")[1].split("public void ContinueFromResults")[0]
    assert "PrimaryRestartsRun" in start_wave
    assert "Lives <= 0" not in start_wave
    assert "PrimaryRestartsRun" in session
    assert 'Loc.T("ui.next_wave", "Next Wave")' in loc or 'Loc.T("ui.next_wave", "Next Wave")' in (
        root / "Assets/Scripts/Core/RunSummary.cs"
    ).read_text(encoding="utf-8")
    summary = (root / "Assets/Scripts/Core/RunSummary.cs").read_text(encoding="utf-8")
    assert 'Loc.T("ui.new_run_reset", "New Run (reset)")' in summary
    assert 'Loc.T("ui.next_wave", "Next Wave")' in summary
    assert "B / Esc Next Wave" in ui
    assert "B / Esc nästa våg" in loc

    # Invalid / fully locked shop snaps to Next Wave and does not consume the stick step.
    calls = {"n": 0}

    def exploding_step(_slot, _dx, _dy):
        calls["n"] += 1
        return 99

    locked = [False] * 36
    assert _pad_resolve_fallback(12, locked) == 0
    assert _pad_resolve_fallback(-1, locked) == 0
    assert _pad_step_selectable(12, 0, -1, locked, exploding_step) == 0
    assert calls["n"] == 0
    assert 0 in range(36)

    def neighbor(_slot, _dx, _dy):
        return 4

    mixed = [False] * 36
    mixed[4] = True
    assert _pad_step_selectable(0, 0, 1, mixed, neighbor) == 4

    def stall(_slot, _dx, _dy):
        return 3

    assert _pad_step_selectable(0, 1, 0, [True, False, False, False], stall) == 0

    # B / Esc focuses Next Wave in one press from any row. Start launches that control.
    assert "FocusHangarSlot(HangarPadNav.PrimarySlot)" in escape
    assert "FocusHangarSlot(HangarPadNav.PrimarySlot)" in back

    assert _card_is_action(True, False, True, True, False) is False
    assert _card_is_action(True, False, False, True, False) is True
    assert _card_is_action(True, False, False, False, False) is False
    assert _card_is_action(True, True, False, True, False) is False
    assert _card_is_action(False, False, False, True, False) is False
    assert _primary_restarts("WaveClear") is False
    assert _primary_restarts("Hangar") is False
    assert _primary_restarts("Playing") is False
    assert _primary_restarts("Failed") is True
    assert _primary_restarts("CampaignClear") is False
    assert _primary_restarts("WaveClear") is False

    wave = _map_anchors(0.014, 0.080, 0.55, 0.888, 0.03, 0.735, 0.97, 0.800)
    strip = _map_anchors(0.014, 0.080, 0.55, 0.888, 0.02, 0.82, 0.98, 0.995)
    headers = _map_anchors(0.014, 0.080, 0.55, 0.888, 0.02, 0.675, 0.98, 0.728)
    blockers = (
        strip,
        headers,
        _map_anchors(0.014, 0.080, 0.55, 0.888, 0.02, 0.800, 0.98, 0.995),
        (0.562, 0.608, 0.986, 0.898),
        (0.562, 0.080, 0.986, 0.596),
        (0.012, 0.905, 0.988, 0.995),
    )
    for blocker in blockers:
        assert not _overlap(wave, blocker)
    assert wave[3] <= 0.730
    assert wave[1] >= 0.080

    for width, height in ((1280, 800), (1920, 1080), (1366, 768), (1440, 900), (2560, 1080), (3440, 1440)):
        wave_px = (wave[0] * width, wave[1] * height, wave[2] * width, wave[3] * height)
        for blocker in blockers:
            block_px = (blocker[0] * width, blocker[1] * height, blocker[2] * width, blocker[3] * height)
            assert not _overlap(wave_px, block_px), (width, height, block_px)


_PAD_ITEMS = (
    ("BodyUpgrade01", "Hull", 90),
    ("BodyUpgrade02", "Hull", 175),
    ("NoseHardpoint", "Hull", 120),
    ("NoseUpgrade02", "Hull", 150),
    ("NoseUpgrade03", "Hull", 200),
    ("RapidFire", "Hull", 100),
    ("EngineUpgrade02", "Hull", 140),
    ("EngineUpgrade03", "Hull", 190),
    ("Overcharger", "Hull", 230),
    ("Afterburner", "Hull", 230),
    ("SpreadBolt", "Weapons", 110),
    ("Pierce", "Weapons", 155),
    ("TwinGuns", "Weapons", 140),
    ("Seeker", "Weapons", 125),
    ("Ricochet", "Weapons", 170),
    ("ShieldCell", "Defense", 80),
    ("ShieldMatrix", "Defense", 185),
    ("Rail", "Doctrine", 160),
    ("FlakFeed", "Doctrine", 150),
    ("Storm", "Doctrine", 240),
    ("OverchargeLance", "Doctrine", 235),
    ("SeekerCadence", "Doctrine", 155),
    ("TwinSeek", "Doctrine", 225),
)
_PAD_N = len(_PAD_ITEMS)
_PAD_SHOP0 = 1
_PAD_CREDITS = _PAD_SHOP0 + _PAD_N
_PAD_EASY = _PAD_CREDITS + 1
_PAD_NORMAL = _PAD_CREDITS + 2
_PAD_HARD = _PAD_CREDITS + 3
_PAD_GOTIT = _PAD_CREDITS + 4
_PAD_BARRAGE = _PAD_GOTIT + 1
_PAD_LANCE = _PAD_GOTIT + 2
_PAD_HUNTER = _PAD_GOTIT + 3
_PAD_HINT = _PAD_HUNTER + 1
_PAD_SETTINGS = _PAD_HINT + 1
_PAD_SLOTS = _PAD_SETTINGS + 1
_PAD_WEAPONS = {"SpreadBolt", "Pierce", "TwinGuns", "Seeker", "Ricochet", "Rail"}
_PAD_PATH = {
    "Rail": "Lance",
    "FlakFeed": "Barrage",
    "Storm": "Barrage",
    "OverchargeLance": "Lance",
    "SeekerCadence": "Hunter",
    "TwinSeek": "Hunter",
    "SpreadBolt": "Barrage",
    "Pierce": "Lance",
    "TwinGuns": "Lance",
    "Seeker": "Hunter",
}
_PAD_DIRS = ((1, 0), (-1, 0), (0, 1), (0, -1))
# Hangar grid +Y walks down the shop, so up the screen is dy=-1.
_PAD_DIRS_UP_RIGHT = ((1, 0), (0, -1))
_PAD_DIRS_DOWN_LEFT = ((-1, 0), (0, 1))
_PAD_INDEX = {name: index for index, (name, _group, _cost) in enumerate(_PAD_ITEMS)}


def _pad_sign(value: int) -> int:
    if value > 0:
        return 1
    if value < 0:
        return -1
    return 0


def _pad_coord(slot: int, legacy: bool) -> tuple[int, int]:
    if slot <= 0:
        return 1, -1
    if slot == _PAD_CREDITS:
        return 0, 8
    if slot == _PAD_EASY:
        return 0, -3
    if slot == _PAD_NORMAL:
        return 1, -3
    if slot == _PAD_HARD:
        return 2, -3
    if slot == _PAD_GOTIT:
        return 0, -4
    if slot == _PAD_BARRAGE:
        return 6, -2
    if slot == _PAD_LANCE:
        return 7, -2
    if slot == _PAD_HUNTER:
        return 8, -2
    if slot == _PAD_HINT:
        return 8, -1
    if slot == _PAD_SETTINGS:
        return 3, -3
    hull = weapon = defense = doctrine = 0
    shop_index = slot - _PAD_SHOP0
    for index, (_name, group, _cost) in enumerate(_PAD_ITEMS):
        if group == "Weapons":
            cx, cy = 4, weapon
            weapon += 1
        elif group == "Defense":
            cx, cy = 5, defense
            defense += 1
        elif group == "Doctrine":
            if legacy:
                cx = 6 + (doctrine % 3)
                cy = 5 + (doctrine // 3)
            else:
                cx = 6 + (doctrine % 2)
                cy = -1
            doctrine += 1
        else:
            cx = hull % 4
            cy = hull // 4
            hull += 1
        if index == shop_index:
            return cx, cy
    return 1, -1


def _pad_coords(legacy: bool) -> list[tuple[int, int]]:
    return [_pad_coord(slot, legacy) for slot in range(_PAD_SLOTS)]


def _pad_accepts(slot: int, selectable) -> bool:
    if selectable is None:
        return True
    return 0 <= slot < len(selectable) and selectable[slot]


def _pad_find_at(x: int, y: int, coords, selectable) -> int:
    for slot, (sx, sy) in enumerate(coords):
        if _pad_accepts(slot, selectable) and sx == x and sy == y:
            return slot
    return -1


def _pad_find_along(frm: int, x: int, y: int, dx: int, dy: int, wrap: bool, coords, selectable) -> int:
    best = -1
    best_score = 10**18
    for slot, (sx, sy) in enumerate(coords):
        if slot == frm or not _pad_accepts(slot, selectable):
            continue
        delx = sx - x
        dely = sy - y
        if dx:
            dir_ok = _pad_sign(delx) == (-_pad_sign(dx) if wrap else _pad_sign(dx))
            y_dist = abs(dely)
            x_dist = abs(delx) if wrap else (delx if dx > 0 else -delx)
            score = y_dist * 20 + x_dist
        else:
            dir_ok = _pad_sign(dely) == (-_pad_sign(dy) if wrap else _pad_sign(dy))
            x_dist = abs(delx)
            y_dist = abs(dely) if wrap else (dely if dy > 0 else -dely)
            score = x_dist * 20 + y_dist
        if dir_ok and score < best_score:
            best_score = score
            best = slot
    return best


def _pad_step(slot: int, dx: int, dy: int, coords, selectable) -> int:
    if dx == 0 and dy == 0:
        return slot
    if dx and dy:
        if dx * dx >= dy * dy:
            dy = 0
        else:
            dx = 0
    x, y = coords[slot]
    exact = _pad_find_at(x + dx, y + dy, coords, selectable)
    if exact >= 0:
        return exact
    along = _pad_find_along(slot, x, y, dx, dy, False, coords, selectable)
    if along >= 0:
        return along
    wrap = _pad_find_along(slot, x, y, dx, dy, True, coords, selectable)
    return wrap if wrap >= 0 else slot


def _pad_step_live(slot: int, dx: int, dy: int, selectable, coords, masked: bool) -> int:
    """Live path mirrors StepSelectable. Invalid focus snaps to slot 0 and does not step."""
    mask = _pad_force_primary(selectable)
    current_ok = 0 <= slot < len(mask) and mask[slot]
    origin = slot if current_ok else 0
    if not current_ok or (dx == 0 and dy == 0):
        return origin
    count = len(mask)
    candidate = _pad_step(origin, dx, dy, coords, mask if masked else None)
    guard = 0
    while guard < count:
        if 0 <= candidate < count and mask[candidate]:
            return candidate
        stepped = _pad_step(candidate, dx, dy, coords, mask if masked else None)
        if stepped == candidate:
            return 0
        candidate = stepped
        guard += 1
    return 0


def _pad_penalized(cost: int, off_path: bool) -> int:
    if not off_path or cost <= 0:
        return cost
    return (cost * 135 + 50) // 100


def _pad_soft_locked(owned: frozenset[str]) -> bool:
    return "FlakFeed" in owned or "Rail" in owned or "SeekerCadence" in owned


def _pad_off_path(owned: frozenset[str], doctrine: str, upgrade: str) -> bool:
    if not _pad_soft_locked(owned) or doctrine == "None" or upgrade == "Ricochet":
        return False
    path = _PAD_PATH.get(upgrade)
    return path is not None and path != doctrine


def _pad_can_apply(owned: frozenset[str], shield: int, doctrine: str, upgrade: str) -> bool:
    if upgrade in owned and upgrade != "ShieldCell":
        return False
    if upgrade == "BodyUpgrade02":
        return "BodyUpgrade01" in owned
    if upgrade == "NoseUpgrade02":
        return "NoseHardpoint" in owned
    if upgrade == "NoseUpgrade03":
        return "NoseUpgrade02" in owned
    if upgrade == "EngineUpgrade02":
        return "RapidFire" in owned
    if upgrade == "EngineUpgrade03":
        return "EngineUpgrade02" in owned
    if upgrade == "Overcharger":
        return "NoseUpgrade03" in owned and "Afterburner" not in owned
    if upgrade == "Afterburner":
        return "EngineUpgrade03" in owned and "Overcharger" not in owned
    if upgrade == "ShieldMatrix":
        return shield >= 2 and "ShieldMatrix" not in owned
    if upgrade == "ShieldCell":
        cap = 3 if "ShieldMatrix" in owned else 2
        return shield < cap
    if upgrade == "FlakFeed":
        return doctrine == "Barrage" and "SpreadBolt" in owned
    if upgrade == "Storm":
        return doctrine == "Barrage" and "FlakFeed" in owned
    if upgrade == "Rail":
        return doctrine == "Lance"
    if upgrade == "OverchargeLance":
        return doctrine == "Lance" and "Rail" in owned and ("Pierce" in owned or "TwinGuns" in owned)
    if upgrade == "SeekerCadence":
        return doctrine == "Hunter" and "Seeker" in owned
    if upgrade == "TwinSeek":
        return doctrine == "Hunter" and "SeekerCadence" in owned
    return True


def _pad_shop_mask(owned: frozenset[str], shield: int, doctrine: str, credits: int) -> list[bool]:
    """Selectable hangar controls. Got it / doctrine intro stay out of the pad order."""
    mask = [False] * _PAD_SLOTS
    mask[0] = True
    for slot in (_PAD_CREDITS, _PAD_EASY, _PAD_NORMAL, _PAD_HARD, _PAD_SETTINGS):
        mask[slot] = True
    if doctrine == "None":
        gates = (
            ("SpreadBolt" in owned, 55, _PAD_BARRAGE),
            ("Pierce" in owned or "TwinGuns" in owned, 25, _PAD_LANCE),
            ("Seeker" in owned, 40, _PAD_HUNTER),
        )
        for gate_met, cost, slot in gates:
            if gate_met and credits >= cost:
                mask[slot] = True
    for index, (name, group, cost) in enumerate(_PAD_ITEMS):
        if group == "Doctrine" and (doctrine == "None" or _PAD_PATH.get(name) != doctrine):
            continue
        if name == "ShieldCell":
            cap = 3 if "ShieldMatrix" in owned else 2
            owned_flag = shield >= cap
            can = shield < cap
        elif name == "ShieldMatrix":
            owned_flag = "ShieldMatrix" in owned
            can = shield >= 2 and not owned_flag
        else:
            owned_flag = name in owned
            can = (not owned_flag) and _pad_can_apply(owned, shield, doctrine, name)
        price = _pad_penalized(cost, _pad_off_path(owned, doctrine, name))
        too_poor = (not owned_flag) and can and credits < price
        locked = (not owned_flag) and (not can)
        mask[_PAD_SHOP0 + index] = (owned_flag and name in _PAD_WEAPONS) or (
            (not owned_flag) and (not locked) and (not too_poor)
        )
    return mask


def _pad_reach(dirs, start: int, goal: int, mask: list[bool], step_fn) -> bool:
    seen = {start}
    queue = [start]
    while queue:
        current = queue.pop()
        if current == goal:
            return True
        for dx, dy in dirs:
            nxt = step_fn(current, dx, dy, mask)
            if nxt not in seen and 0 <= nxt < len(mask) and mask[nxt]:
                seen.add(nxt)
                queue.append(nxt)
    return goal in seen


def _pad_closures(mask: list[bool], step_fn) -> tuple[list[int], list[int], int]:
    nodes = [index for index, on in enumerate(mask) if on]
    adj = {node: [] for node in nodes}
    for node in nodes:
        for dx, dy in _PAD_DIRS:
            adj[node].append(step_fn(node, dx, dy, mask))
    seen = {0: 0}
    queue = [0]
    head = 0
    while head < len(queue):
        current = queue[head]
        head += 1
        for nxt in adj.get(current, ()):
            if nxt not in seen:
                seen[nxt] = seen[current] + 1
                queue.append(nxt)
    reverse = {node: [] for node in nodes}
    for node, outs in adj.items():
        for nxt in outs:
            if nxt in reverse:
                reverse[nxt].append(node)
    back = {0}
    queue = [0]
    while queue:
        current = queue.pop()
        for prev in reverse.get(current, ()):
            if prev not in back:
                back.add(prev)
                queue.append(prev)
    missing = [node for node in nodes if node not in seen]
    trapped = [node for node in nodes if node not in back]
    farthest = max(seen.values()) if seen else 0
    return missing, trapped, farthest


def _pad_states(doctrine: str, gate: tuple[str, ...], mid: str, bands: tuple[int, ...], mid_owned: bool):
    free = ("Pierce", "TwinGuns", "Seeker", "Ricochet")
    for bits in range(16):
        for shield, matrix in ((0, False), (1, False), (2, False), (2, True)):
            for nose in (0, 1, 2):
                for engine in (0, 1):
                    owned = set(gate)
                    for bit, name in enumerate(free):
                        if bits >> bit & 1:
                            owned.add(name)
                    if nose >= 1:
                        owned.add("NoseHardpoint")
                    if nose >= 2:
                        owned.add("NoseUpgrade02")
                    if engine >= 1:
                        owned.add("RapidFire")
                    if matrix:
                        owned.add("ShieldMatrix")
                    if mid_owned:
                        owned.add(mid)
                    charges = 2 if matrix else shield
                    for credits in bands:
                        yield frozenset(owned), charges, doctrine, credits


def _pad_audit(states, coords, masked: bool) -> tuple[int, int, int, int]:
    bad = 0
    dead = 0
    farthest = 0
    total = 0

    def step_fn(slot, dx, dy, mask, _coords=coords, _masked=masked):
        return _pad_step_live(slot, dx, dy, mask, _coords, _masked)

    for owned, shield, doctrine, credits in states:
        total += 1
        mask = _pad_shop_mask(owned, shield, doctrine, credits)
        assert mask[0]
        assert not mask[_PAD_GOTIT] and not mask[_PAD_HINT]
        if doctrine != "None":
            assert not mask[_PAD_BARRAGE] and not mask[_PAD_LANCE] and not mask[_PAD_HUNTER]
        missing, trapped, dist = _pad_closures(mask, step_fn)
        if missing:
            bad += 1
        if trapped:
            dead += 1
        if dist > farthest:
            farthest = dist
    return total, bad, dead, farthest


def _doctrine_strip_shares_next_wave_row() -> bool:
    wave = _map_anchors(0.014, 0.080, 0.55, 0.888, 0.03, 0.735, 0.97, 0.800)
    strip = _map_anchors(0.562, 0.608, 0.986, 0.898, 0.012, 0.190, 0.988, 0.397)
    left = _map_anchors(strip[0], strip[1], strip[2], strip[3], 0.04, 0.08, 0.48, 0.92)
    right = _map_anchors(strip[0], strip[1], strip[2], strip[3], 0.52, 0.08, 0.96, 0.92)
    wave_mid = (wave[1] + wave[3]) * 0.5
    left_mid = (left[1] + left[3]) * 0.5
    pitch = (0.094 + 0.016) * (0.888 - 0.080)
    return abs(left_mid - wave_mid) <= pitch * 0.5 and left[0] > wave[2] and right[0] > left[2]


def test_doctrine_rows_pad_reachable() -> None:
    """Every purchasable hangar row is on the D-pad graph, including Barrage mids.

    The 1536 Barrage states are spread-owned, flak-not-owned, the other four weapons,
    shield 0/1/2/matrix, nose tier 0–2, rapid-fire on/off, and purses 140/150/160/165.
    Legacy FindAlong ignored the mask and parked doctrine rows at y=5, so Flak Feed
    was unreachable whenever Ricochet was not a bridge (576 states).
    """
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    padnav = (root / "Assets/Scripts/Core/HangarPadNav.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    loc = (root / "Assets/Scripts/Core/Loc.cs").read_text(encoding="utf-8")
    assert "Step(origin, dx, dy, selectable)" in padnav
    assert "DoctrineShopStripSharesNextWaveRow" in padnav
    assert "0.190f" in padnav and "0.397f" in padnav
    assert _doctrine_strip_shares_next_wave_row()

    legacy_coords = _pad_coords(True)
    fixed_coords = _pad_coords(False)
    assert _pad_coord(_PAD_SHOP0 + _PAD_INDEX["FlakFeed"], True) == (7, 5)
    assert _pad_coord(_PAD_SHOP0 + _PAD_INDEX["FlakFeed"], False) == (7, -1)
    assert _pad_coord(_PAD_SHOP0 + _PAD_INDEX["Storm"], False) == (6, -1)

    pre_bands = (140, 150, 160, 165)
    cap_bands = (200, 240, 260, 400)
    spaces = (
        ("Barrage", ("SpreadBolt",), "FlakFeed", pre_bands, False),
        ("Barrage", ("SpreadBolt",), "FlakFeed", cap_bands, True),
        ("Lance", ("TwinGuns",), "Rail", pre_bands, False),
        ("Lance", ("TwinGuns",), "Rail", cap_bands, True),
        ("Hunter", ("Seeker",), "SeekerCadence", pre_bands, False),
        ("Hunter", ("Seeker",), "SeekerCadence", cap_bands, True),
    )
    before = {}
    after = {}
    for doctrine, gate, mid, bands, mid_owned in spaces:
        states = list(_pad_states(doctrine, gate, mid, bands, mid_owned))
        key = f"{doctrine} {'cap' if mid_owned else 'mid'}"
        before[key] = _pad_audit(states, legacy_coords, False)
        after[key] = _pad_audit(states, fixed_coords, True)
        print(f"pad {key}: legacy unreachable {before[key][1]}/{before[key][0]} dead {before[key][2]} far {before[key][3]}")
        print(f"pad {key}: fixed  unreachable {after[key][1]}/{after[key][0]} dead {after[key][2]} far {after[key][3]}")

    assert before["Barrage mid"][0] == 1536
    assert before["Barrage mid"][1] == 576
    assert before["Lance mid"][0] == 1536
    assert before["Hunter mid"][0] == 1536
    for key, (_total, bad, dead, farthest) in after.items():
        assert bad == 0, key
        assert dead == 0, key
        assert farthest <= 8, (key, farthest)
    for key, (total, _bad, dead, _far) in before.items():
        assert total == 1536
        assert dead == 0

    example = _pad_shop_mask(frozenset({"SpreadBolt"}), 0, "Barrage", 160)
    flak = _PAD_SHOP0 + _PAD_INDEX["FlakFeed"]
    storm = _PAD_SHOP0 + _PAD_INDEX["Storm"]
    assert example[flak] and not example[storm]

    def legacy_step(slot, dx, dy, mask):
        return _pad_step_live(slot, dx, dy, mask, legacy_coords, False)

    def fixed_step(slot, dx, dy, mask):
        return _pad_step_live(slot, dx, dy, mask, fixed_coords, True)

    legacy_missing, _legacy_trapped, _legacy_far = _pad_closures(example, legacy_step)
    fixed_missing, fixed_trapped, fixed_far = _pad_closures(example, fixed_step)
    assert flak in legacy_missing
    assert flak not in fixed_missing and not fixed_trapped
    assert fixed_step(0, 1, 0, example) == flak
    assert fixed_far <= 8

    # Capstone with Ricochet locked out of the purse still sits on the Next Wave row.
    storm_mask = _pad_shop_mask(frozenset({"SpreadBolt", "FlakFeed"}), 0, "Barrage", 240)
    assert storm_mask[storm]
    storm_missing, storm_trapped, _storm_far = _pad_closures(storm_mask, fixed_step)
    assert storm not in storm_missing and not storm_trapped
    assert fixed_step(0, 1, 0, storm_mask) == storm

    # Doctrine not chosen: live cards are reachable, passive cards and Got it are not.
    picking = _pad_shop_mask(frozenset({"SpreadBolt"}), 0, "None", 60)
    assert picking[_PAD_BARRAGE] and not picking[_PAD_LANCE] and not picking[_PAD_HUNTER]
    assert not picking[_PAD_GOTIT]
    pick_missing, pick_trapped, _pick_far = _pad_closures(picking, fixed_step)
    assert _PAD_BARRAGE not in pick_missing and not pick_trapped

    # Gear sits one step right of Hard. Up then right from Next Wave reaches it.
    assert _pad_coord(_PAD_SETTINGS, False) == (3, -3)
    assert _pad_coord(_PAD_HARD, False) == (2, -3)
    gear_mask = _pad_shop_mask(frozenset({"SpreadBolt"}), 0, "Barrage", 160)
    assert gear_mask[_PAD_SETTINGS] and gear_mask[0] and gear_mask[_PAD_HARD]
    assert fixed_step(_PAD_HARD, 1, 0, gear_mask) == _PAD_SETTINGS
    assert fixed_step(_PAD_SETTINGS, -1, 0, gear_mask) == _PAD_HARD
    gear_missing, gear_trapped, _gear_far = _pad_closures(gear_mask, fixed_step)
    assert _PAD_SETTINGS not in gear_missing and _PAD_SETTINGS not in gear_trapped
    assert 0 not in gear_missing
    up_right = _pad_reach(_PAD_DIRS_UP_RIGHT, 0, _PAD_SETTINGS, gear_mask, fixed_step)
    down_left = _pad_reach(_PAD_DIRS_DOWN_LEFT, _PAD_SETTINGS, 0, gear_mask, fixed_step)
    assert up_right, "gear must be reachable from Next Wave with up/right"
    assert down_left, "Next Wave must be reachable from the gear"

    en_hint = "LS move · LT utility · LB cycle · A confirm · B / Esc Next Wave · Start launch wave"
    sv_hint = "LS styr · LT utility · LB cykla · A bekräfta · B / Esc nästa våg · Start starta våg"
    assert "LS move · {0}" in ui
    assert "LT utility · LB cycle · A confirm · B / Esc Next Wave · Start launch wave" in ui
    assert "LS styr · {0}" in loc
    assert "LT utility · LB cykla · A bekräfta · B / Esc nästa våg · Start starta våg" in loc
    # Footer is one line in (0.14, 0.008)–(0.86, 0.072). At 18px, Kenney Future
    # Narrow is about 10px/char and the 1280px hint box is 922px, so stay <= 90.
    hint_box = (0.86 - 0.14) * 1280
    assert len(en_hint) <= 90 and len(sv_hint) <= 90
    assert len(en_hint) * 10 < hint_box
    assert len(sv_hint) * 10 < hint_box
    assert len(sv_hint) < 165


def test_hangar_footer_launch_and_hint_size() -> None:
    """Launch key on the hangar footer, corrected first-flight copy, 18px hint floor."""
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    loc = (root / "Assets/Scripts/Core/Loc.cs").read_text(encoding="utf-8")
    theme = (root / "Assets/Scripts/UI/UiTheme.cs").read_text(encoding="utf-8")

    keys = (
        "ui.hint_hangar",
        "ui.hangar_controls",
        "ui.hint_play",
        "ui.hangar_hint_body",
        "ui.first_wave_coach",
    )
    swedish = loc.split("private static readonly Dictionary")[1].split("};")[0]
    for key in keys:
        assert f'"{key}"' in swedish, key
        assert f'"{key}"' in ui, key

    en_hangar = "LS move · LT utility · LB cycle · A confirm · B / Esc Next Wave · Start launch wave"
    sv_hangar = "LS styr · LT utility · LB cykla · A bekräfta · B / Esc nästa våg · Start starta våg"
    assert "Start launch wave" in en_hangar and "Start launch wave" in ui
    assert "Start starta våg" in sv_hangar and "Start starta våg" in loc
    assert "LS move · {0}" in ui and "LS styr · {0}" in loc
    assert len(en_hangar) <= 90 and len(sv_hangar) <= 90
    hint_box = (0.86 - 0.14) * 1280
    assert len(en_hangar) * 10 < hint_box
    assert len(sv_hangar) * 10 < hint_box

    en_play = "WASD/LS move · Mouse/RS aim · LMB/RT fire · E/LT utility · Q/LB cycle · Esc / Start = back to hangar"
    sv_play = "WASD/LS styr · Mus/RS sikte · VMB/RT skjut · E/LT utility · Q/LB cykla · Esc/Start = tillbaka till hangaren"
    assert en_play in ui and sv_play in loc
    assert "Esc / Start abort" not in ui
    assert "Esc / Start avbryt" not in loc
    assert len(en_play) <= 125 and len(sv_play) <= 125

    card = ui.split("HangarHintBody")[1].split("FirstWaveCoach")[0]
    assert "Abort (Esc)" not in card
    assert "Start = launch wave" in card
    assert "B / Esc = focus Next Wave" in card
    assert "Avbryt (Esc)" not in loc
    assert "B / Esc = fokusera Nästa våg" in loc
    assert "Esc / Start returns to hangar" in ui
    assert "Esc / Start återvänder till hangaren" in loc
    assert "Abort (Start)" not in ui

    assert "HintMin = 18" in theme
    assert "BodyMin = 14" in theme
    assert "HintSize(int screenWidth)" in theme
    assert "screenWidth <= 1280" in theme
    assert "return HintMin" in theme
    assert "UiTheme.HintSize(Screen.width)" in ui
    assert "_hint.fontSize = footerSize" in ui
    assert "_firstFlightBody.fontSize = footerSize" in ui
    assert "ui.hint_footer" in ui
    assert "A Select · Start Launch wave · Select = Settings" in ui

    def hint_size(screen_width: int) -> int:
        return 18 if screen_width <= 1280 else 16

    assert hint_size(1280) >= 18
    assert hint_size(800) >= 18
    assert hint_size(1920) == 16


def _settings_default() -> dict:
    return {
        "screen_shake": True,
        "hint_mode": 2,
        "hint_size": 1,
        "confirm_in_play": True,
        "confirm_new_run": True,
        "pad_nav": 2,
    }


def _clamp_hint_size(step: int) -> int:
    if step < 0:
        return 0
    if step > 2:
        return 2
    return step


def _normalize_hint_mode(value: int) -> int:
    if value in (0, 1, 2, 3):
        return value
    return 2


def _hint_px(step: int) -> int:
    clamped = _clamp_hint_size(step)
    return (14, 18, 22)[clamped]


def _effective_hint_size(step: int, screen_width: int, narrow_floor: int) -> int:
    chosen = _hint_px(step)
    if screen_width <= 1280 and narrow_floor > chosen:
        return narrow_floor
    return chosen


def _shows_hangar_footer(mode: int) -> bool:
    return mode in (2, 3)


def _shows_play_hint(mode: int) -> bool:
    return mode == 3


def _shows_first_wave_coach(first_wave: bool, mode: int) -> bool:
    """Mirrors SettingsState.ShowsFirstWaveCoach. Wave 1 is on for every hint mode."""
    if not first_wave:
        return False
    return mode in (0, 1, 2, 3) or True


def _hint_size_options(width: int) -> tuple[int, ...]:
    """Mirrors SettingsState.HintSizeOptions. 14 is hidden at the 18px floor."""
    if width <= 1280:
        return (1, 2)
    return (0, 1, 2)


def _visible_hint_step(step: int, width: int) -> int:
    options = _hint_size_options(width)
    clamped = _clamp_hint_size(step)
    best = options[0]
    for option in options:
        if option >= clamped:
            return option
        best = option
    return best


def _step_hint_size_for_width(step: int, direction: int, width: int) -> int:
    options = _hint_size_options(width)
    visible = _visible_hint_step(step, width)
    index = options.index(visible)
    if direction > 0:
        index += 1
    elif direction < 0:
        index -= 1
    if index < 0:
        index = 0
    if index >= len(options):
        index = len(options) - 1
    return options[index]


def _step_hint_mode(current: int, direction: int) -> int:
    index = 0
    if current == 2:
        index = 1
    elif current == 3:
        index = 2
    if direction > 0:
        index += 1
    elif direction < 0:
        index -= 1
    if index < 0:
        index = 0
    if index > 2:
        index = 2
    return (0, 2, 3)[index]


def _step_hint_size(step: int, direction: int) -> int:
    nxt = _clamp_hint_size(step)
    if direction > 0:
        nxt += 1
    elif direction < 0:
        nxt -= 1
    return _clamp_hint_size(nxt)


def _normalize_pad_nav(value: int) -> int:
    if value in (0, 1, 2):
        return value
    return 2


def _settings_from_ints(version, shake, hint, size, in_play, new_run, pad) -> dict:
    state = _settings_default()
    if version not in (1, 2):
        return state
    state["screen_shake"] = shake != 0
    state["hint_mode"] = _normalize_hint_mode(hint)
    state["hint_size"] = _clamp_hint_size(size)
    state["confirm_in_play"] = in_play != 0
    # Version 1 stored confirm-off as the old default. Treat that bit as unset.
    state["confirm_new_run"] = True if version == 1 else new_run != 0
    state["pad_nav"] = _normalize_pad_nav(pad)
    return state


def _settings_capture(state: dict) -> tuple:
    return (
        2,
        1 if state["screen_shake"] else 0,
        _normalize_hint_mode(state["hint_mode"]),
        _clamp_hint_size(state["hint_size"]),
        1 if state["confirm_in_play"] else 0,
        1 if state["confirm_new_run"] else 0,
        _normalize_pad_nav(state["pad_nav"]),
    )


def _settings_route(flags: dict) -> str:
    """Mirrors SettingsInputRouter.Route. Settings wins, then credits, then play/hangar."""
    if flags.get("open"):
        if flags.get("escape") or flags.get("start") or flags.get("cancel") or flags.get("select") or flags.get("f1") or flags.get("scrim"):
            return "close"
        if flags.get("playing"):
            return "none"
        if flags.get("submit") or flags.get("gear"):
            return "activate"
        if flags.get("nav_y"):
            return "move"
        if flags.get("nav_x"):
            return "nudge"
        return "none"
    if flags.get("credits"):
        if flags.get("escape") or flags.get("cancel"):
            return "credits"
        return "none"
    if flags.get("playing") and (flags.get("escape") or flags.get("start")):
        return "abort"
    if not flags.get("playing") and not flags.get("credits"):
        if flags.get("f1") or flags.get("select") or flags.get("gear"):
            return "open"
        if flags.get("escape"):
            return "hangar_escape"
        if flags.get("start"):
            return "hangar_start"
        if flags.get("cancel"):
            return "hangar_back"
    return "none"


def _settings_blocks_pad(flags: dict) -> bool:
    return bool(flags.get("open") and not flags.get("playing"))


def _settings_roles() -> tuple[str, ...]:
    return (
        "value",
        "value",
        "value",
        "value",
        "value",
        "value",
        "value",
        "value",
        "value",
        "value",
        "section",
        "action",
    )


def _row_bands() -> list[tuple[float, float]]:
    order = _settings_roles()
    weights = {"value": 1.0, "section": 7.2, "action": 1.0}
    top, bottom, gap = 0.86, 0.05, 0.012
    weight_sum = sum(weights[kind] for kind in order)
    span = top - bottom - gap * (len(order) - 1)
    cursor = top
    bands = []
    for kind in order:
        height = span * (weights[kind] / weight_sum)
        bands.append((cursor - height, cursor))
        cursor = cursor - height - gap
    return bands


def _screen_step_y(x: float, y: float, flick: float) -> int:
    ax, ay = abs(x), abs(y)
    if ax < flick and ay < flick:
        return 0
    if ay > ax:
        return 1 if y > 0 else -1
    return 0


def _settings_move(index: int, delta: int) -> int:
    navigable = tuple(i for i, role in enumerate(_settings_roles()) if role != "section")
    if delta == 0:
        return index
    direction = 1 if delta > 0 else -1
    steps = abs(delta)
    current = index
    if current not in navigable:
        if direction > 0:
            higher = [item for item in navigable if item > current]
            current = higher[0] if higher else navigable[-1]
        else:
            lower = [item for item in navigable if item < current]
            current = lower[-1] if lower else navigable[0]
    for _step in range(steps):
        if direction > 0:
            higher = [item for item in navigable if item > current]
            nxt = higher[0] if higher else current
        else:
            lower = [item for item in navigable if item < current]
            nxt = lower[-1] if lower else current
        if nxt == current:
            break
        current = nxt
    return current


def SettingsState_shake(enabled: bool, amplitude: float) -> float:
    if not enabled or amplitude <= 0:
        return 0.0
    return amplitude


def _step_volume(current: float, direction: int) -> float:
    delta = 0.1 if direction > 0 else (-0.1 if direction < 0 else 0.0)
    nxt = min(1.0, max(0.0, current + delta))
    return round(nxt * 1000.0) / 1000.0


def _canvas_scale(width: float, height: float) -> float:
    import math

    log_w = math.log2(max(width, 1.0) / 1920.0)
    log_h = math.log2(max(height, 1.0) / 1080.0)
    return 2 ** (log_w + (log_h - log_w) * 0.5)


def _estimate_width(text: str, font_size: int) -> float:
    if not text or font_size <= 0:
        return 0.0
    return len(text) * 10.0 * font_size / 18.0


def _wrapped_line_count(text: str, box_width: float, font_size: int) -> int:
    if not text:
        return 0
    char_width = 10.0 * font_size / 18.0
    if char_width < 0.01:
        char_width = 0.01
    lines = 0
    for para in text.split("\n"):
        if para == "":
            lines += 1
            continue
        used = 0.0
        count = 1
        for word in para.split(" "):
            word_width = len(word) * char_width
            space = char_width if used > 0 else 0.0
            if used + space + word_width > box_width and used > 0:
                count += 1
                used = word_width
            else:
                used += space + word_width
        lines += count
    return lines


def test_settings_shell() -> None:
    """Settings shell: persisted defaults, gear clearance, and shortcut consumption."""
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    state = (root / "Assets/Scripts/Core/SettingsState.cs").read_text(encoding="utf-8")
    rows = (root / "Assets/Scripts/Core/SettingsRows.cs").read_text(encoding="utf-8")
    router = (root / "Assets/Scripts/Core/SettingsInputRouter.cs").read_text(encoding="utf-8")
    padnav = (root / "Assets/Scripts/Core/HangarPadNav.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    loc = (root / "Assets/Scripts/Core/Loc.cs").read_text(encoding="utf-8")
    inputs = (root / "ProjectSettings/InputManager.asset").read_text(encoding="utf-8")

    assert "class SettingsState" in state
    assert "CurrentVersion = 2" in state
    assert "DefaultHintSizeStep = 1" in state
    assert "MaxHintSizeStep = 2" in state
    assert "ScreenShake = true" in state
    assert "HintMode.HangarFooter" in state
    assert "ConfirmRestartInPlay = true" in state
    assert "ConfirmRestartNewRun = true" in state
    assert "PadNavSource.Both" in state
    assert "enum HintMode" in state and "Off = 0" in state and "SettingsOnly = 1" in state and "HangarFooter = 2" in state and "On = 3" in state
    assert "HintPxSmall = 14" in state and "HintPxMedium = 18" in state and "HintPxLarge = 22" in state
    assert "EffectiveHintSize" in state and "ShowsHangarFooter" in state and "ShowsPlayHint" in state
    assert "enum PadNavSource" in state and "DPad = 0" in state and "Analog = 1" in state and "Both = 2" in state
    assert "ClampHintSize" in state and "Normalize()" in state and "FromInts" in state and "Capture()" in state
    assert "PlayerPrefs.GetInt(VersionKey, 0)" in state and "PlayerPrefs.Save()" in state
    assert "agr.settings.version" in state
    assert "agr.ui.language" not in state
    assert "invert" not in state.lower()
    fresh = _settings_default()
    assert fresh["screen_shake"] is True
    assert fresh["hint_mode"] == 2
    assert fresh["hint_size"] == 1
    assert fresh["confirm_in_play"] is True
    assert fresh["confirm_new_run"] is True
    assert fresh["pad_nav"] == 2
    assert _clamp_hint_size(-4) == 0 and _clamp_hint_size(9) == 2 and _clamp_hint_size(1) == 1
    assert _normalize_hint_mode(99) == 2 and _normalize_hint_mode(0) == 0 and _normalize_hint_mode(3) == 3 and _normalize_hint_mode(1) == 1
    assert _hint_px(0) == 14 and _hint_px(1) == 18 and _hint_px(2) == 22 and _hint_px(9) == 22
    assert _effective_hint_size(0, 1280, 18) == 18
    assert _effective_hint_size(2, 1280, 18) == 22
    assert _effective_hint_size(0, 1920, 16) == 14
    assert _effective_hint_size(1, 3440, 16) == 18
    assert _shows_hangar_footer(2) and _shows_hangar_footer(3)
    assert not _shows_hangar_footer(0) and not _shows_hangar_footer(1)
    assert _shows_play_hint(3) and not _shows_play_hint(2) and not _shows_play_hint(0) and not _shows_play_hint(1)
    assert "ShowsFirstWaveCoach" in state
    for hint_mode in (0, 1, 2, 3):
        assert _shows_first_wave_coach(True, hint_mode)
        assert not _shows_first_wave_coach(False, hint_mode)
    assert "HintSizeOptions" in state and "VisibleHintStep" in state
    for narrow in (800, 1024, 1280):
        assert _hint_size_options(narrow) == (1, 2)
        assert 0 not in _hint_size_options(narrow)
    for wide in (1281, 1366, 1920, 2560, 3440):
        assert _hint_size_options(wide) == (0, 1, 2)
    assert _visible_hint_step(0, 1280) == 1 and _hint_px(_visible_hint_step(0, 1280)) == 18
    assert _visible_hint_step(0, 1920) == 0 and _hint_px(_visible_hint_step(0, 1920)) == 14
    assert _visible_hint_step(2, 1280) == 2
    for width in (1280, 1920, 3440):
        options = _hint_size_options(width)
        for step in (0, 1, 2):
            assert _step_hint_size_for_width(step, 1, width) in options
            assert _step_hint_size_for_width(step, -1, width) in options
            assert _step_hint_size_for_width(step, 0, width) in options
    assert _step_hint_size_for_width(0, 1, 1280) == 2
    assert _step_hint_size_for_width(2, -1, 1280) == 1
    assert _step_hint_size_for_width(0, -1, 1920) == 0
    assert _step_hint_size_for_width(0, 1, 1920) == 1
    assert _step_hint_mode(0, 1) == 2 and _step_hint_mode(1, 1) == 2
    assert _step_hint_mode(2, 1) == 3 and _step_hint_mode(3, 1) == 3
    assert _step_hint_mode(3, -1) == 2 and _step_hint_mode(2, -1) == 0
    assert _step_hint_size(1, 1) == 2 and _step_hint_size(2, 1) == 2 and _step_hint_size(0, -1) == 0
    assert _normalize_pad_nav(-1) == 2 and _normalize_pad_nav(1) == 1
    clamped = _settings_from_ints(1, 2, 40, 9, 0, 5, -3)
    assert clamped["screen_shake"] is True
    assert clamped["hint_mode"] == 2
    assert clamped["hint_size"] == 2
    assert clamped["confirm_in_play"] is False
    assert clamped["confirm_new_run"] is True
    assert clamped["pad_nav"] == 2
    assert _settings_from_ints(0, 0, 0, 0, 0, 1, 0) == fresh
    migrated = _settings_from_ints(1, 1, 2, 1, 1, 0, 2)
    assert migrated["confirm_new_run"] is True
    turned_off = _settings_from_ints(2, 1, 2, 1, 1, 0, 2)
    assert turned_off["confirm_new_run"] is False
    dirty = {
        "screen_shake": False,
        "hint_mode": 0,
        "hint_size": 2,
        "confirm_in_play": True,
        "confirm_new_run": True,
        "pad_nav": 0,
    }
    packed = _settings_capture(dirty)
    assert _settings_from_ints(*packed) == dirty
    over = dict(dirty)
    over["hint_size"] = 8
    over["hint_mode"] = 7
    over["pad_nav"] = 4
    round_trip = _settings_from_ints(*_settings_capture(over))
    assert round_trip["hint_size"] == 2
    assert round_trip["hint_mode"] == 2
    assert round_trip["pad_nav"] == 2
    assert round_trip["screen_shake"] is False
    on_mode = dict(dirty)
    on_mode["hint_mode"] = 3
    on_mode["hint_size"] = 0
    assert _settings_from_ints(*_settings_capture(on_mode))["hint_mode"] == 3
    assert _settings_from_ints(*_settings_capture(on_mode))["hint_size"] == 0

    order = rows.split("Order =")[1].split(";")[0]
    assert "SettingsRowId.Language" in order
    assert "SettingsRowId.Music" in order
    assert "SettingsRowId.Sfx" in order
    assert "SettingsRowId.Mute" in order
    assert "SettingsRowId.ScreenShake" in order
    assert "SettingsRowId.Controls" in order
    assert "SettingsRowId.Close" in order
    assert order.index("Language") < order.index("Music") < order.index("Sfx") < order.index("Mute")
    assert "SettingsRowId.HintMode" in order and "SettingsRowId.HintSize" in order
    assert order.index("Mute") < order.index("ScreenShake") < order.index("HintMode") < order.index("HintSize")
    assert "SettingsRowId.ConfirmAbort" in order and "SettingsRowId.ConfirmNewRun" in order
    assert order.index("HintSize") < order.index("ConfirmAbort") < order.index("ConfirmNewRun")
    assert "SettingsRowId.PadNav" in order
    assert order.index("ConfirmNewRun") < order.index("PadNav") < order.index("Controls") < order.index("Close")
    assert "IsNavigable" in rows and "IsValue" in rows and "RowBand" in rows and "Move(" in rows
    assert "IsSlider" in rows and "StepVolume" in rows and "VolumeStep = 0.1f" in rows
    assert "SliderMinX = 0.40f" in rows and "SliderMaxX = 0.96f" in rows
    assert "RowMinX = 0.06f" in rows and "RowMaxX = 0.94f" in rows
    assert "OldSliderMinUnits = 101f" in rows and "LadderFont = 12" in rows
    assert "ContentTop = 0.86f" in rows and "ContentBottom = 0.05f" in rows
    assert "RowGap = 0.012f" in rows and "SectionWeight = 7.2f" in rows
    bands = _row_bands()
    assert len(bands) == 12
    for y0, y1 in bands:
        assert 0.05 - 1e-6 <= y0 < y1 <= 0.86 + 1e-6
    for left, right in zip(bands, bands[1:]):
        assert left[0] > right[1]
        assert not _overlap((0.06, left[0], 0.94, left[1]), (0.06, right[0], 0.94, right[1]))
    assert _settings_move(0, 1) == 1
    assert _settings_move(4, 1) == 5
    assert _settings_move(6, 1) == 7
    assert _settings_move(8, 1) == 9
    assert _settings_move(9, 1) == 11
    assert _settings_move(9, -1) == 8
    assert _settings_move(11, -1) == 9
    assert _settings_move(10, 1) == 11
    assert _settings_move(10, -1) == 8
    assert _settings_move(0, 0) == 0
    assert SettingsState_shake(False, 0.4) == 0.0
    assert SettingsState_shake(False, 0.0) == 0.0
    assert SettingsState_shake(True, -0.2) == 0.0
    assert SettingsState_shake(True, 0.2) == 0.2
    assert "ShakeAmplitude(bool enabled, float amplitude)" in state
    assert "ScreenShakeEnabled" in state and "Publish(SettingsState state)" in state
    follow = (root / "Assets/Scripts/Player/FollowCamera.cs").read_text(encoding="utf-8")
    assert "SettingsState.ScreenShakeEnabled" in follow
    assert "SettingsState.ShakeAmplitude" in follow
    assert "_shake = 0f" in follow
    assert "PlayerPrefs" not in follow
    assert 'Loc.T("ui.settings.shake", "Screen shake")' in ui
    assert _step_volume(0.28, 1) == 0.38
    assert _step_volume(0.28, -1) == 0.18
    assert _step_volume(0.0, -1) == 0.0
    assert _step_volume(1.0, 1) == 1.0
    assert _step_volume(0.5, 0) == 0.5

    assert "class SettingsInputRouter" in router
    assert "BlocksHangarPad" in router
    assert "ScreenStepY" in router
    assert "return flags.Open && !flags.Playing;" in router
    assert _screen_step_y(0.0, 1.0, 0.55) == 1
    assert _screen_step_y(0.0, -1.0, 0.55) == -1
    assert _screen_step_y(1.0, 0.0, 0.55) == 0
    hangar = {"playing": False, "credits": False, "open": False}
    assert _settings_route({**hangar, "escape": True}) == "hangar_escape"
    assert _settings_route({**hangar, "start": True}) == "hangar_start"
    assert _settings_route({**hangar, "cancel": True}) == "hangar_back"
    assert _settings_route({**hangar, "f1": True}) == "open"
    assert _settings_route({**hangar, "select": True}) == "open"
    assert _settings_route({**hangar, "gear": True}) == "open"
    opened = {**hangar, "open": True}
    assert _settings_route({**opened, "escape": True}) == "close"
    assert _settings_route({**opened, "start": True}) == "close"
    assert _settings_route({**opened, "cancel": True}) == "close"
    assert _settings_route({**opened, "select": True}) == "close"
    assert _settings_route({**opened, "f1": True}) == "close"
    assert _settings_route({**opened, "scrim": True}) == "close"
    assert _settings_route({**opened, "submit": True}) == "activate"
    assert _settings_route({**opened, "nav_y": 1}) == "move"
    assert _settings_route({**opened, "nav_x": -1}) == "nudge"
    assert _settings_route({**opened, "escape": True, "start": True, "submit": True}) == "close"
    assert _settings_blocks_pad(opened) is True
    assert _settings_blocks_pad(hangar) is False
    assert _settings_route({**opened, "playing": True, "escape": True}) == "close"
    assert _settings_route({**opened, "playing": True, "start": True}) == "close"
    assert _settings_route({**opened, "playing": True, "submit": True}) == "none"
    assert _settings_route({"playing": True, "escape": True}) == "abort"
    assert _settings_route({"playing": True, "start": True}) == "abort"
    assert _settings_route({"playing": True, "f1": True}) == "none"
    assert _settings_route({"playing": True, "select": True}) == "none"
    assert _settings_blocks_pad({"open": True, "playing": True}) is False
    assert _settings_route({"credits": True, "cancel": True, "open": True}) == "close"
    assert _settings_route({"credits": True, "escape": True}) == "credits"

    assert "SettingsSlot" in padnav
    assert "Step(HardSlot, 1, 0) == SettingsSlot" in padnav
    assert "Step(SettingsSlot, -1, 0) == HardSlot" in padnav
    assert "LangSvSlot" not in padnav and "LangEnSlot" not in padnav
    assert "MuteSlot" not in padnav
    assert "x = 6" in padnav and "y = -3" in padnav
    assert "SettingsGear" in ui and "SettingsPanel" in ui and "SettingsScrim" in ui
    assert "AudioPanel" not in ui and "BuildAudioControls" not in ui
    assert "new Vector2(0.900f, 0.905f)" in ui
    assert "new Vector2(0.988f, 0.995f)" in ui
    assert "new Vector2(0.30f, 0.12f)" in ui
    assert "new Vector2(0.70f, 0.88f)" in ui
    assert "SetMusicVolume" in ui and "SetSfxVolume" in ui and "ToggleMute" in ui
    assert "SettingsMeasure.SliderMinX" in ui and "SettingsMeasure.SliderMaxX" in ui
    assert "VerticalWrapMode.Overflow" in ui
    assert "CompactCount" in ui
    assert "KeyCode.F1" in ui and "JoystickButton6" in ui
    assert "SettingsInputRouter.Route" in ui and "BlocksHangarPad" in ui
    assert "FocusHangarSlot(HangarPadNav.SettingsSlot)" in ui
    assert "FullControlHint" in ui
    assert "ShowsHangarFooter" in ui and "ShowsPlayHint" in ui and "EffectiveHintSize" in ui
    assert "ShowsFirstWaveCoach" in ui and "VisibleHintStep" in ui
    assert "StepHintSize(_settings.HintSizeStep, direction, Screen.width)" in ui
    assert "WaveIndex == 1" in ui.split("private void ApplyBottomHint")[1].split("private static void ClampOneLine")[0]
    assert "_firstRunCoachUntil" not in ui
    assert "\u2699" not in ui and "\u2699" not in loc
    assert "A Select · Start Launch wave · Select = Settings" in ui
    assert "A Välj · Start Starta våg · Select = Inställningar" in loc
    assert "new Vector2(0.07f, 0.14f)" in ui and "new Vector2(0.93f, 0.85f)" in ui
    assert 'Loc.T("ui.settings", "Settings")' in ui
    assert 'Loc.T("ui.settings.language", "Language")' in ui
    assert 'Loc.T("ui.settings.close", "Close")' in ui
    assert 'Loc.T("ui.settings.controls", "Controls")' in ui
    hint_fn = ui.split("private static string FullControlHint()")[1].split("private static int IndexOfSettingsRow")[0]
    assert "ui.hint_play" in hint_fn
    assert "ui.hint_hangar" in hint_fn
    assert "ui.hangar_controls" in hint_fn
    assert "UiTheme.HintSize(Screen.width)" in ui
    assert "raycastTarget = true" in ui.split("SettingsScrim")[1].split("SettingsPanel")[0]
    update = ui.split("private void Update()")[1].split("private void PulseHangarLaunch")[0]
    assert update.index("SettingsInputRouter.Route") < update.index("OnHangarEscape()")
    assert update.index("SettingsInputRouter.Route") < update.index("OnHangarStart()")
    assert update.index("SettingsInputRouter.Route") < update.index("OnHangarBack()")
    assert update.index("OnAbort()") < update.index("NavigateHangarPad()")
    assert "SettingsInputRouter.BlocksHangarPad(settingsFlags)" in update
    assert update.index("BlocksHangarPad") < update.index("NavigateHangarPad()")
    assert "NavigateHangarPad()" in update and "SyncHangarPadSelection()" in update
    assert "BuildSettingsRow" in ui and "SettingsRows.Order" in ui
    assert "Start launch wave" in ui
    swedish = loc.split("private static readonly Dictionary")[1].split("};")[0]
    for key in (
        "ui.settings",
        "ui.settings.language",
        "ui.settings.controls",
        "ui.settings.close",
        "ui.settings.en",
        "ui.settings.sv",
        "ui.settings.play",
        "ui.settings.hangar",
        "ui.settings.on",
        "ui.settings.off",
        "ui.settings.shake",
        "ui.settings.hint",
        "ui.settings.hint.hangar",
        "ui.settings.hint.panel",
        "ui.settings.hint_size",
        "ui.settings.hint.px",
        "ui.settings.confirm_abort",
        "ui.settings.confirm_new_run",
        "ui.confirm.abort_title",
        "ui.confirm.abort_body",
        "ui.confirm.new_run_title",
        "ui.confirm.new_run_body",
        "ui.confirm.yes",
        "ui.confirm.no",
        "ui.settings.pad_nav",
        "ui.settings.pad.dpad",
        "ui.settings.pad.analog",
        "ui.settings.pad.both",
        "ui.hint_footer",
    ):
        assert f'"{key}"' in swedish, key
        assert f'"{key}"' in ui, key
    assert '"ach.compact"' in swedish
    catalog = (root / "Assets/Scripts/Core/AchievementCatalog.cs").read_text(encoding="utf-8")
    assert '"ach.compact"' in catalog and "CompactCount" in catalog
    assert "Inställningar" in loc and "Språk" in loc and "Kontroller" in loc and "Stäng" in loc
    assert "Spel" in loc and '"ui.settings.on", "På"' in loc and '"ui.settings.off", "Av"' in loc
    credits_sv = swedish.split('"credits.body"')[1].split("},")[0]
    assert "Ljud" in credits_sv and "Typsnitt" in credits_sv and "Kenney Future" in credits_sv and "Speltest" in credits_sv
    assert "invert: 1" in inputs
    assert "m_Name: Vertical" in inputs

    gear = (0.900, 0.905, 0.988, 0.995)
    panel = (0.30, 0.12, 0.70, 0.88)
    diff = (0.748, 0.905, 0.888, 0.995)
    doctrine = (0.562, 0.608, 0.986, 0.898)
    wave = _map_anchors(0.014, 0.080, 0.55, 0.888, 0.03, 0.735, 0.97, 0.800)
    assert panel == (0.30, 0.12, 0.70, 0.88)
    # Gear stays top-right. The settings card is a centered modal under a scrim,
    # so it stays off the top bar while covering the hangar behind it.
    assert not _overlap(panel, gear)
    assert panel[3] < gear[1]
    resolutions = ((1280, 800), (1600, 900), (1920, 1080), (2560, 1440), (3440, 1440))
    for width, height in resolutions:
        def px(rect, _w=width, _h=height):
            return (rect[0] * _w, rect[1] * _h, rect[2] * _w, rect[3] * _h)

        gear_px = px(gear)
        panel_px = px(panel)
        for other in (wave, doctrine, diff):
            assert not _overlap(gear_px, px(other)), (width, height, other)
        assert not _overlap(panel_px, gear_px), (width, height)

        scale = _canvas_scale(width, height)
        canvas_w = width / scale
        slider_w = (0.96 - 0.40) * (0.94 - 0.06) * (0.70 - 0.30) * canvas_w
        assert slider_w >= 101.0, (width, height, slider_w)

        controls_index = _settings_roles().index("section")
        band_bottom, band_top = bands[controls_index]
        body_span = (band_top - 0.05) - band_bottom
        body_h = body_span * (0.88 - 0.12) * (height / scale)
        body_w = (0.92 - 0.08) * (0.70 - 0.30) * canvas_w
        play_en = "WASD/LS move · Mouse/RS aim · LMB/RT fire · E/LT utility · Q/LB cycle · Esc / Start = back to hangar"
        play_sv = "WASD/LS styr · Mus/RS sikte · VMB/RT skjut · E/LT utility · Q/LB cykla · Esc/Start = tillbaka till hangaren"
        hang_en = "LS move · LT utility · LB cycle · A confirm · B / Esc Next Wave · Start launch wave"
        hang_sv = "LS styr · LT utility · LB cykla · A bekräfta · B / Esc nästa våg · Start starta våg"
        for block in (
            "Play\n" + play_en + "\nHangar\n" + hang_en,
            "Spel\n" + play_sv + "\nHangar\n" + hang_sv,
        ):
            lines = _wrapped_line_count(block, body_w, 18)
            assert lines * 18 * 1.15 <= body_h, (width, height, lines, body_h)

        value_h = bands[0][1] - bands[0][0]
        value_px = value_h * (0.88 - 0.12) * (height / scale)
        assert value_px >= 14 * 1.15, (width, height, value_px)

        ladder_w = (0.478 - 0.355) * canvas_w
        for header in ("ACHIEVEMENTS", "PRESTATIONER"):
            assert _estimate_width(header, 12) <= ladder_w, (width, height, header, ladder_w)
        assert _estimate_width("★ 9/9", 12) <= ladder_w

        hint_w = (0.86 - 0.14) * canvas_w
        footer_en = "A Select · Start Launch wave · Select = Settings"
        footer_sv = "A Välj · Start Starta våg · Select = Inställningar"
        coach_en = "Shoot rocks  ·  Esc / Start returns to hangar"
        coach_sv = "Skjut stenar  ·  Esc / Start återvänder till hangaren"
        for line in (footer_en, footer_sv, coach_en, coach_sv):
            assert _estimate_width(line, 22) <= hint_w, (width, height, line, hint_w)
            assert _wrapped_line_count(line, hint_w, 22) == 1
        hint_h = (0.072 - 0.008) * (height / scale)
        for size in (14, 18, 22):
            for line in (play_en, play_sv, footer_en, footer_sv, coach_en, coach_sv):
                assert _estimate_width(line, size) <= hint_w, (width, height, size, line, hint_w)
                assert _wrapped_line_count(line, hint_w, size) == 1
                assert size * 1.15 <= hint_h, (width, height, size, hint_h)

        flight = _map_anchors(0.014, 0.080, 0.55, 0.888, 0.02, 0.800, 0.98, 0.995)
        body = _map_anchors(flight[0], flight[1], flight[2], flight[3], 0.07, 0.14, 0.93, 0.85)
        card_w = (body[2] - body[0]) * canvas_w
        card_h = (body[3] - body[1]) * (height / scale)
        card_en = (
            "LS / WASD fly  ·  RT / LMB shoot  ·  LT / E utility\n"
            "Start = launch wave  ·  B / Esc = focus Next Wave\n"
            "Clear a wave to earn credits and upgrades.\n"
            "Medal ladder (top-left): ★ Scout Wing at wave 3."
        )
        card_sv = (
            "LS / WASD fly  ·  RT / VMB skjut  ·  LT / E utility\n"
            "Start = starta våg  ·  B / Esc = fokusera Nästa våg\n"
            "Rensa en våg för kredit och uppgraderingar.\n"
            "Medaljstege (uppe till vänster): ★ Spejarvinge på våg 3."
        )
        for card in (card_en, card_sv):
            card_lines = _wrapped_line_count(card, card_w, 22)
            assert card_lines * 22 * 1.15 <= card_h, (width, height, card_lines, card_h)


def _confirm_route(flags: dict) -> str:
    """Mirrors ConfirmDialogRouter.Route. Open dialog consumes every button."""
    if flags.get("open"):
        if flags.get("escape") or flags.get("start") or flags.get("cancel") or flags.get("scrim"):
            return "no"
        if flags.get("submit"):
            return "yes" if flags.get("focus") == 1 else "no"
        if flags.get("focus_delta"):
            return "move"
        return "blocked"
    if flags.get("settings_open") or flags.get("credits_visible"):
        return "none"
    abort = flags.get("playing") and (flags.get("escape") or flags.get("start") or flags.get("abort_click"))
    if abort:
        return "open" if flags.get("confirm_in_play", True) else "yes"
    new_run = flags.get("restart_screen") and (flags.get("new_run_click") or flags.get("start"))
    if new_run:
        return "open" if flags.get("confirm_new_run") else "yes"
    return "none"


def _confirm_move_focus(focus: int, delta: int) -> int:
    if delta < 0:
        return 1
    if delta > 0:
        return 0
    return 1 if focus == 1 else 0


def _confirm_time_scale(abort_open: bool, wave_live: bool) -> float:
    return 0.0 if abort_open and wave_live else 1.0


def _confirm_clock(dialog_open: bool, wave_live: bool, silenced: bool) -> dict:
    dismiss = dialog_open and not wave_live
    still_open = dialog_open and not dismiss
    silence_now = still_open and wave_live
    restore = silenced and not silence_now and wave_live
    return {
        "scale": _confirm_time_scale(still_open, wave_live),
        "dismiss": dismiss,
        "silence": silence_now,
        "restore": restore,
        "silenced": silence_now if silence_now else False,
    }


def test_confirm_restart() -> None:
    """Abort and New Run confirms, default No, and a pause that cannot leak."""
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    router = (root / "Assets/Scripts/Core/ConfirmDialogRouter.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    state = (root / "Assets/Scripts/Core/SettingsState.cs").read_text(encoding="utf-8")

    assert "class ConfirmDialogRouter" in router
    assert "DefaultFocus()" in router and "return FocusNo;" in router
    assert "class ConfirmPause" in router
    assert "DismissAbort" in router and "SilenceShip" in router and "RestoreShipInput" in router
    assert "ConfirmDialogLayout" in router
    assert "YesMinX = 0.08f" in router and "NoMinX = 0.54f" in router
    assert _confirm_move_focus(0, 0) == 0
    assert _confirm_move_focus(0, -1) == 1
    assert _confirm_move_focus(1, 1) == 0
    assert _confirm_move_focus(1, 0) == 1

    closed = {"open": False, "playing": True, "confirm_in_play": True, "focus": 0}
    assert _confirm_route({**closed, "escape": True}) == "open"
    assert _confirm_route({**closed, "start": True}) == "open"
    assert _confirm_route({**closed, "abort_click": True}) == "open"
    assert _confirm_route({**closed, "confirm_in_play": False, "escape": True}) == "yes"
    assert _confirm_route({**closed, "confirm_in_play": False, "start": True}) == "yes"
    assert _confirm_route({**closed, "confirm_in_play": False, "abort_click": True}) == "yes"
    assert _confirm_route({"playing": True, "f1": True, "confirm_in_play": True}) == "none"
    hangar = {"playing": False, "restart_screen": False, "confirm_new_run": True}
    assert _confirm_route({**hangar, "escape": True}) == "none"
    assert _confirm_route({**hangar, "start": True}) == "none"
    assert _confirm_route({**hangar, "cancel": True}) == "none"
    restart = {"playing": False, "restart_screen": True, "confirm_new_run": False}
    assert _confirm_route({**restart, "new_run_click": True}) == "yes"
    assert _confirm_route({**restart, "start": True}) == "yes"
    assert _confirm_route({**restart, "escape": True}) == "none"
    assert _confirm_route({**restart, "cancel": True}) == "none"
    assert _confirm_route({**restart, "confirm_new_run": True, "new_run_click": True}) == "open"
    assert _confirm_route({**restart, "confirm_new_run": True, "start": True}) == "open"
    wave_clear = {"playing": False, "restart_screen": False, "confirm_new_run": True, "start": True}
    assert _confirm_route(wave_clear) == "none"

    opened = {"open": True, "focus": 0, "playing": True, "confirm_in_play": True}
    assert _confirm_route(opened) == "blocked"
    assert _confirm_route({**opened, "escape": True}) == "no"
    assert _confirm_route({**opened, "start": True}) == "no"
    assert _confirm_route({**opened, "cancel": True}) == "no"
    assert _confirm_route({**opened, "scrim": True}) == "no"
    assert _confirm_route({**opened, "submit": True, "focus": 0}) == "no"
    assert _confirm_route({**opened, "submit": True, "focus": 1}) == "yes"
    assert _confirm_route({**opened, "escape": True, "submit": True, "focus": 1}) == "no"
    assert _confirm_route({**opened, "start": True, "submit": True, "focus": 1}) == "no"
    assert _confirm_route({**opened, "abort_click": True}) == "blocked"
    assert _confirm_route({**opened, "new_run_click": True}) == "blocked"
    assert _confirm_route({**opened, "focus_delta": -1}) == "move"
    assert _confirm_route({**opened, "focus_delta": 1, "playing": False, "restart_screen": True}) == "move"
    assert _confirm_route({**opened, "playing": False, "restart_screen": True, "start": True}) == "no"

    assert _confirm_time_scale(True, True) == 0.0
    assert _confirm_time_scale(True, False) == 1.0
    assert _confirm_time_scale(False, True) == 1.0
    assert _confirm_time_scale(False, False) == 1.0
    held = _confirm_clock(True, True, False)
    assert held["scale"] == 0.0 and held["silence"] and not held["dismiss"]
    ended = _confirm_clock(True, False, True)
    assert ended["scale"] == 1.0 and ended["dismiss"] and not ended["restore"]
    released = _confirm_clock(False, True, True)
    assert released["scale"] == 1.0 and released["restore"] and not released["silence"]
    new_run_open = _confirm_clock(False, False, False)
    assert new_run_open["scale"] == 1.0 and not new_run_open["dismiss"]

    assert "ConfirmDialogRouter.Route" in ui
    assert "ConfirmPause.TimeScale" in ui
    assert "Time.timeScale = ConfirmPause.TimeScale" in ui
    assert "_settings.ConfirmRestartInPlay" in ui
    assert "_settings.ConfirmRestartNewRun" in ui
    assert "ConfirmDialogRouter.DefaultFocus()" in ui
    assert "_confirmOpenedFrame != Time.frameCount" in ui
    assert "ConfirmScrim" in ui and "ConfirmPanel" in ui
    assert "ConfirmYes" in ui and "ConfirmNo" in ui
    assert "ui.settings.confirm_abort" in ui and "Confirm abort (Esc/Start) during wave" in ui
    assert 'Loc.T("ui.settings.confirm_new_run", "Confirm New Run")' in ui
    assert 'Loc.T("ui.confirm.yes", "Yes")' in ui and 'Loc.T("ui.confirm.no", "No")' in ui
    assert "UiTheme.BuildPanel" in ui.split("BuildConfirmDialog")[1].split("private static void LockButtonNavigation")[0]
    update = ui.split("private void Update()")[1].split("private void PulseHangarLaunch")[0]
    assert update.index("ConfirmDialogRouter.Route") < update.index("SettingsInputRouter.Route")
    assert update.index("ConfirmDialogRouter.Route") < update.index("NavigateHangarPad()")
    read_confirm = ui.split("private ConfirmRequest ReadConfirmRequest()")[1].split("private void OnConfirmScrim")[0]
    assert "request.RestartScreen = false" in read_confirm
    assert "request.SettingsOpen = _settingsOpen" in read_confirm
    assert "request.CreditsVisible = _creditsVisible" in read_confirm
    route_body = router.split("public static ConfirmAction Route")[1].split("class ConfirmPause")[0]
    assert "request.SettingsOpen || request.CreditsVisible" in route_body
    assert route_body.index("SettingsOpen") < route_body.index("request.RestartScreen &&")
    covered = {"settings_open": True, "credits_visible": True, "restart_screen": True, "playing": True, "confirm_new_run": True}
    for button in ("start", "escape", "cancel", "submit"):
        flagged = {**covered, button: True}
        assert _confirm_route(flagged) == "none"
    phases = {
        "failed": {"playing": False, "restart_screen": True},
        "campaign_clear": {"playing": False, "restart_screen": False},
        "wave_clear": {"playing": False, "restart_screen": False},
        "hangar": {"playing": False, "restart_screen": False},
        "playing": {"playing": True, "restart_screen": False},
    }
    buttons = {
        "start": {"start": True},
        "esc": {"escape": True},
        "b": {"cancel": True},
        "a": {"submit": True},
    }
    for settings_open in (False, True):
        for credits_visible in (False, True):
            for phase_name, phase in phases.items():
                for _button_name, button in buttons.items():
                    for confirm_new_run in (False, True):
                        overlay = settings_open or credits_visible
                        request = {
                            "open": False,
                            "playing": phase["playing"] and not overlay,
                            "restart_screen": phase["restart_screen"] and not overlay,
                            "settings_open": settings_open,
                            "credits_visible": credits_visible,
                            "confirm_new_run": confirm_new_run,
                            "confirm_in_play": True,
                        }
                        request.update(button)
                        action = _confirm_route(request)
                        if overlay:
                            assert action == "none", (settings_open, credits_visible, phase_name, button, confirm_new_run, action)
                        settings_action = "none"
                        if action == "none":
                            settings_action = _settings_route({
                                "open": settings_open,
                                "playing": phase["playing"],
                                "credits": credits_visible,
                                "escape": button.get("escape", False),
                                "start": button.get("start", False),
                                "cancel": button.get("cancel", False),
                                "submit": button.get("submit", False),
                            })
                        started = action in ("yes", "open") or settings_action in ("hangar_start", "abort")
                        if overlay:
                            assert not started, (settings_open, credits_visible, phase_name, button, confirm_new_run, action, settings_action)
    bare_fail = {"playing": False, "restart_screen": True, "confirm_new_run": False, "start": True}
    assert _confirm_route(bare_fail) == "yes"
    assert _confirm_route({**bare_fail, "confirm_new_run": True}) == "open"
    assert _confirm_route({"playing": True, "escape": True, "confirm_in_play": True}) == "open"
    assert "if (!_confirmOpen && confirmAction == ConfirmAction.None)" in update
    apply = ui.split("private void ApplyConfirmRoute")[1].split("private static ConfirmKind KindForRequest")[0]
    assert "OnAbort()" in apply and "OnPrimary()" in apply
    assert "PrimaryRestartsRun" in ui
    primary = ui.split("private void OnPrimary()")[1].split("private void OnBuy")[0]
    assert "_settingsOpen || _creditsVisible" in primary
    clock = ui.split("private void ApplyConfirmClock()")[1].split("private void SetConfirmNavigationLock")[0]
    assert "SetInputPaused(true)" in clock and "SetInputPaused(false)" in clock
    assert "SetInputEnabled(false)" not in clock
    ship = (root / "Assets/Scripts/Player/ShipController.cs").read_text(encoding="utf-8")
    paused = ship.split("public void SetInputPaused")[1].split("private void Update")[0]
    assert "CancelCharge" not in paused and "linearVelocity" not in paused
    assert "SetInputEnabled(false)" in (root / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    assert "ConfirmRestartInPlay = true" in state
    assert "ConfirmRestartNewRun = true" in state
    label = "Confirm abort (Esc/Start) during wave"
    label_sv = "Bekräfta avbrott (Esc/Start) under våg"
    for width, height in ((1280, 800), (1600, 900), (1920, 1080), (2560, 1440), (3440, 1440)):
        scale = _canvas_scale(width, height)
        label_w = 0.72 * (0.94 - 0.06) * (0.70 - 0.30) * (width / scale)
        assert _estimate_width(label, 14) <= label_w, (width, label_w)
        assert _estimate_width(label_sv, 14) <= label_w, (width, label_w)
    yes = (0.08, 0.12, 0.46, 0.36)
    no = (0.54, 0.12, 0.92, 0.36)
    assert not _overlap(yes, no)
    assert yes[2] < no[0]
    panel = (0.32, 0.36, 0.68, 0.64)
    assert panel[2] - panel[0] < 0.5
    assert panel[3] < 0.905


def _pad_live(x: float, y: float, dead: float = 0.22) -> bool:
    return (x * x) + (y * y) >= dead * dead


def _pad_select(source: int, dpad: tuple[float, float], stick: tuple[float, float], dead: float = 0.22) -> tuple[float, float]:
    """Mirrors PadNavSourceRules.Select. 0 d-pad, 1 analog, 2 both."""
    dpad_live = _pad_live(dpad[0], dpad[1], dead)
    stick_live = _pad_live(stick[0], stick[1], dead)
    if source == 0:
        return dpad if dpad_live else (0.0, 0.0)
    if source == 1:
        return stick if stick_live else (0.0, 0.0)
    if dpad_live:
        return dpad
    return stick if stick_live else (0.0, 0.0)


def _pad_select_menu(
    source: int,
    dpad: tuple[float, float],
    stick: tuple[float, float],
    fallback: tuple[float, float],
    dead: float = 0.22,
) -> tuple[float, float]:
    """Mirrors the fallback Select. DPad ignores stick and Horizontal/Vertical."""
    picked = _pad_select(source, dpad, stick, dead)
    if source == 0:
        return picked
    if _pad_live(picked[0], picked[1], dead):
        return picked
    if _pad_live(fallback[0], fallback[1], dead):
        return fallback
    return (0.0, 0.0)


def _step_pad_nav(current: int, direction: int) -> int:
    index = 0
    if current == 1:
        index = 1
    elif current == 2:
        index = 2
    if direction > 0:
        index += 1
    elif direction < 0:
        index -= 1
    if index < 0:
        index = 0
    if index > 2:
        index = 2
    return index


def test_pad_nav_source() -> None:
    """Pad navigation filters hangar menus and never the settings panel or fly stick."""
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    state = (root / "Assets/Scripts/Core/SettingsState.cs").read_text(encoding="utf-8")
    pad = (root / "Assets/Scripts/Core/GamepadInput.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    ship = (root / "Assets/Scripts/Player/ShipController.cs").read_text(encoding="utf-8")
    rows = (root / "Assets/Scripts/Core/SettingsRows.cs").read_text(encoding="utf-8")

    assert "class PadNavSourceRules" in state and "Select(" in state
    assert "MenuPadNav" in state and "_menuPadNav = source.PadNavSource" in state
    assert "StepPadNav" in state
    dpad = (1.0, 0.0)
    stick = (0.0, -1.0)
    quiet = (0.0, 0.0)
    tiny = (0.1, 0.0)
    for source in (0, 1, 2):
        assert _pad_select(source, dpad, quiet) == (dpad if source != 1 else quiet)
        assert _pad_select(source, quiet, stick) == (stick if source != 0 else quiet)
        assert _pad_select(source, dpad, stick) == (stick if source == 1 else dpad)
        assert _pad_select(source, quiet, quiet) == quiet
        assert _pad_select(source, tiny, tiny) == quiet
    assert _pad_select(2, dpad, stick) == dpad
    assert _pad_select(0, quiet, stick) == quiet
    assert _pad_select(1, dpad, quiet) == quiet
    # DPad source with stick-only input returns zero, even when Horizontal/Vertical
    # carry the same left-stick axes. Analog with d-pad-only input returns zero.
    assert _pad_select_menu(0, quiet, stick, stick) == quiet
    assert _pad_select_menu(0, quiet, stick, (1.0, 0.0)) == quiet
    assert _pad_select_menu(1, dpad, quiet, quiet) == quiet
    assert _pad_select_menu(1, dpad, quiet, quiet) == (0.0, 0.0)
    assert _pad_select_menu(2, dpad, stick, stick) == dpad
    assert _pad_select_menu(2, quiet, stick, stick) == stick
    assert _pad_select_menu(2, quiet, quiet, (1.0, 0.0)) == (1.0, 0.0)
    assert _pad_select_menu(1, quiet, quiet, (0.0, 1.0)) == (0.0, 1.0)
    assert _step_pad_nav(2, -1) == 1 and _step_pad_nav(1, -1) == 0 and _step_pad_nav(0, -1) == 0
    assert _step_pad_nav(0, 1) == 1 and _step_pad_nav(1, 1) == 2 and _step_pad_nav(2, 1) == 2
    fresh = _settings_default()
    assert fresh["pad_nav"] == 2
    flipped = dict(fresh)
    flipped["pad_nav"] = 0
    assert _settings_from_ints(*_settings_capture(flipped))["pad_nav"] == 0
    flipped["pad_nav"] = 1
    assert _settings_from_ints(*_settings_capture(flipped))["pad_nav"] == 1

    combined = pad.split("public static Vector2 UiNavCombined(PadNavSource source)")[1].split("public static Vector2 MouseDelta")[0]
    assert "PadNavSourceRules.Select" in combined
    assert "PadMoveStick()" in combined
    assert "UiNavDpad()" in combined
    assert "Axis(MoveX)" in combined
    dpad_branch = combined.split("PadNavSource.DPad")[1].split("PadMoveStick()")[0]
    assert "Axis(MoveX)" not in dpad_branch
    assert "Axis(MoveY)" not in dpad_branch
    assert "PadMoveX" not in dpad_branch
    assert "Horizontal" not in dpad_branch
    assert "Vertical" not in dpad_branch
    assert "return UiNavCombined(SettingsState.MenuPadNav);" in pad
    assert "PlayerPrefs" not in pad
    settings_flags = ui.split("private SettingsInputFlags ReadSettingsFlags()")[1].split("private void OpenSettings()")[0]
    assert "UiNavCombined(PadNavSource.Both)" in settings_flags
    hangar = ui.split("private void NavigateHangarPad()")[1].split("private int SlotFromSelected")[0]
    assert "UiNavCombined()" in hangar
    assert "PadNavSource.Both" not in hangar
    assert "MoveStick()" in ship
    assert "PadNavSource" not in ship and "MenuPadNav" not in ship
    assert 'Loc.T("ui.settings.pad_nav", "Pad navigation")' in ui
    assert 'Loc.T("ui.settings.pad.dpad", "D-pad")' in ui
    assert 'Loc.T("ui.settings.pad.analog", "Analog")' in ui
    assert 'Loc.T("ui.settings.pad.both", "Both")' in ui
    assert "_settings.PadNavSource" in ui
    order = rows.split("Order =")[1].split(";")[0]
    assert order.index("PadNav") < order.index("Controls")
    assert "invert" not in state.lower()


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
    test_cs0136_local_shadow_gate()
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
    test_juice_firstrun_044()
    test_campaign_cap_and_session_best()
    test_steam_slice_044()
    test_ui_theme_pad_menus_044()
    test_hangar_wave_clear_layout()
    test_dual_fire_v1()
    test_doctrine_rail_045()
    test_doctrine_w1_affordability()
    test_hotfix_045_rail_cancel_and_duck_merge()
    test_hangar_next_wave_always_selectable()
    test_doctrine_rows_pad_reachable()
    test_hangar_footer_launch_and_hint_size()
    test_settings_shell()
    test_confirm_restart()
    test_pad_nav_source()
    test_world_continue_and_hangar_readability()
    print("Week 1 logic tests passed (Hangar → Play → Clear/Fail + shop persist)")
    return 0


def _is_world_boundary(wave: int) -> bool:
    if wave < 5:
        return False
    return wave % 5 == 0


def _world_index(wave: int) -> int:
    shown = 1 if wave < 1 else wave
    return ((shown - 1) // 5 % 7) + 1


def _layout_for_wave(wave: int) -> str:
    names = (
        "Open",
        "PylonRing",
        "SplitTrench",
        "MineBelt",
        "CrossGates",
        "DebrisIslands",
        "SpokeRing",
    )
    world = _world_index(wave)
    index = (world - 1) % 7
    return names[index]


def _arena_visual(wave: int) -> str:
    names = (
        "Arena_Blockout",
        "Arena_World2_Blockout",
        "Arena_World3_Blockout",
        "Arena_World4_Blockout",
        "Arena_World5_Blockout",
        "Arena_World6_Blockout",
    )
    index = (_world_index(wave) - 1) % len(names)
    return names[index]


def _scale_enemy_hp(hp: int, grade: str, world: int) -> int:
    if hp < 1:
        hp = 1
    if grade == "easy":
        graded = max(1, (hp * 4) // 5)
    elif grade == "hard":
        graded = max(hp + 1, (hp * 5) // 4)
    else:
        graded = hp
    steps = world - 1
    if steps < 0:
        steps = 0
    if steps > 6:
        steps = 6
    return max(1, graded * (100 + 15 * steps) // 100)


def _primary_label(phase: str, world_cleared: int, next_world: int) -> str:
    if phase == "Failed":
        return "New Run (reset)"
    if phase == "WaveClear" and world_cleared > 0:
        world = world_cleared + 1 if next_world < 1 else next_world
        return f"Continue to World {world}"
    if phase in ("WaveClear", "CampaignClear"):
        return "Next Wave"
    return "Start Wave"


def _hex_rgb(value: str) -> tuple[float, float, float]:
    text = value.lstrip("#")
    return tuple(int(text[i : i + 2], 16) / 255.0 for i in (0, 2, 4))


def _channel_lum(channel: float) -> float:
    if channel <= 0.04045:
        return channel / 12.92
    return ((channel + 0.055) / 1.055) ** 2.4


def _rel_lum(rgb: tuple[float, float, float]) -> float:
    r, g, b = rgb
    return 0.2126 * _channel_lum(r) + 0.7152 * _channel_lum(g) + 0.0722 * _channel_lum(b)


def _contrast(text_hex: str, plate_hex: str) -> float:
    lighter = max(_rel_lum(_hex_rgb(text_hex)), _rel_lum(_hex_rgb(plate_hex)))
    darker = min(_rel_lum(_hex_rgb(text_hex)), _rel_lum(_hex_rgb(plate_hex)))
    return (lighter + 0.05) / (darker + 0.05)


def _contains(outer, inner) -> bool:
    return outer[0] <= inner[0] and outer[1] <= inner[1] and outer[2] >= inner[2] and outer[3] >= inner[3]


def _has_run_progress(wave: int, score: int, credits: int, purchase: bool) -> bool:
    return wave > 1 or score > 0 or credits > 0 or purchase


def _should_confirm_new_run(setting_on: bool, has_progress: bool) -> bool:
    return setting_on and has_progress


def test_world_continue_and_hangar_readability() -> None:
    """Wave 5 continues the run. Hangar chrome does not overlap. Shop text stays readable."""
    from pathlib import Path

    root = Path(__file__).resolve().parents[1]
    session = (root / "Assets/Scripts/Core/GameSession.cs").read_text(encoding="utf-8")
    cap = (root / "Assets/Scripts/Core/CampaignCap.cs").read_text(encoding="utf-8")
    summary = (root / "Assets/Scripts/Core/RunSummary.cs").read_text(encoding="utf-8")
    manager = (root / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    diff = (root / "Assets/Scripts/Core/DifficultySettings.cs").read_text(encoding="utf-8")
    waves = (root / "Assets/Scripts/Core/WaveManager.cs").read_text(encoding="utf-8")
    layout = (root / "Assets/Scripts/Core/ArenaLayout.cs").read_text(encoding="utf-8")
    factory = (root / "Assets/Scripts/Content/ContentFactory.cs").read_text(encoding="utf-8")
    theme = (root / "Assets/Scripts/UI/UiTheme.cs").read_text(encoding="utf-8")
    ui = (root / "Assets/Scripts/UI/GameUi.cs").read_text(encoding="utf-8")
    ach = (root / "Assets/Scripts/Core/AchievementCatalog.cs").read_text(encoding="utf-8")

    assert "IsWorldBoundary" in cap
    assert "WorldCleared" in session
    assert "return phase == GamePhase.Failed" in session
    assert "CompleteCampaign" not in manager
    assert "ShouldUnlockHardClear" in ach and "IsFinalWave" in ach
    assert "WorldHpPercent = 15" in diff and "MaxWorldHpSteps = 6" in diff
    assert "Mathf.Clamp(waveIndex, 1, 10)" in waves
    assert "ArenaWorlds" in factory

    run = Session()
    run.credits = 40
    run.begin()
    run.wave = 5
    run.add_score(80)
    run.complete()
    assert run.phase == "WaveClear"
    assert run.wave == 6
    assert run.score == 180
    assert run.credits == 190
    assert run.world_cleared == 1
    assert _primary_restarts(run.phase) is False
    assert _primary_restarts("Hangar") is False
    assert _primary_restarts("Failed") is True
    assert _primary_label("WaveClear", 1, 2) == "Continue to World 2"
    assert _primary_label("WaveClear", 0, 2) == "Next Wave"
    assert _primary_label("Failed", 0, 1) == "New Run (reset)"
    assert _primary_label("Hangar", 0, 1) == "Start Wave"
    failed = Session()
    failed.begin()
    failed.wave = 4
    failed.add_score(20)
    failed.credits = 30
    failed.fail("hull")
    assert failed.phase == "Failed"
    assert failed.wave == 4
    assert failed.score == 20
    assert _primary_restarts(failed.phase) is True

    for wave in (5, 10, 15, 20, 25, 30, 35):
        assert _is_world_boundary(wave)
    for wave in (1, 4, 6, 9, 11, 14):
        assert not _is_world_boundary(wave)
    assert _world_index(1) == 1 and _world_index(5) == 1 and _world_index(6) == 2
    assert _world_index(10) == 2 and _world_index(11) == 3 and _world_index(35) == 7
    assert _world_index(36) == 1
    for wave in range(1, 41):
        world = _world_index(wave)
        assert 1 <= world <= 7
        assert _layout_for_wave(wave)
        assert _arena_visual(wave).startswith("Arena_")
    assert _arena_visual(1) == "Arena_Blockout"
    assert _arena_visual(31) == "Arena_Blockout"
    assert _layout_for_wave(31) == "SpokeRing"
    assert _scale_enemy_hp(10, "normal", 1) == 10
    assert _scale_enemy_hp(10, "normal", 2) == 11
    assert _scale_enemy_hp(10, "normal", 7) == 19
    assert _scale_enemy_hp(10, "normal", 8) == 19
    assert _scale_enemy_hp(10, "easy", 1) == 8
    assert "WavesPerLayout = 5" in layout and "LayoutCount = 7" in layout
    assert 'Loc.Tf("run.sector_world", "SECTOR CLEAR - World {0} complete"' in cap
    assert 'Loc.Tf("ui.continue_world", "Continue to World {0}"' in summary
    assert 'Loc.Tf("run.over_title", "RUN OVER - out of lives (wave {0})"' in summary
    assert "HangarWinHint" not in cap

    assert _should_confirm_new_run(True, True) is True
    assert _should_confirm_new_run(True, False) is False
    assert _should_confirm_new_run(False, True) is False
    assert _has_run_progress(1, 0, 0, False) is False
    assert _has_run_progress(2, 0, 0, False) is True
    assert _has_run_progress(1, 1, 0, False) is True
    assert _has_run_progress(1, 0, 1, False) is True
    assert _has_run_progress(1, 0, 0, True) is True
    assert "ShouldConfirmNewRun" in session and "HasRunProgress" in session

    accent, surface2 = "#C8CED6", "#141C28"
    assert _contrast(accent, surface2) >= 4.5
    assert _contrast("#6AA8C8", surface2) >= 4.5
    assert _contrast("#A8B2BC", "#3A4450") >= 4.5
    assert _contrast("#F0C8A8", "#3A4450") >= 4.5
    assert "ShopHullSize = 14" in theme and "ShopNameSize = 18" in theme
    assert "DoctrineCardSize = 16" in theme and "ShopLineSpacing = 1.1f" in theme
    assert "ContrastRatio" in theme

    hangar = (0.014, 0.080, 0.55, 0.888)
    resolutions = ((1280, 800), (1600, 900), (1920, 1080), (2560, 1440), (3440, 1440))
    hull_en = (
        "Body Upgrade",
        "Hull Plate 02",
        "Nose Hardpoint",
        "Nose Upgrade 02",
        "Nose Upgrade 03",
        "Rapid Fire",
        "Engine Upgrade 02",
        "Engine Upgrade 03",
        "Overcharger",
        "Afterburner",
    )
    hull_sv = (
        "Skrovbyte",
        "Skrovplatta 02",
        "Noshårdpunkt",
        "Nos 02",
        "Nos 03",
        "Snabbeld",
        "Motor 02",
        "Motor 03",
        "Överladdare",
        "Efterbrännare",
    )
    wide_en = ("Spread", "Pierce", "Twin Guns", "Seeker", "Ricochet", "Shield", "Matrix", "Tvillingkanoner")
    wide_sv = ("Spridbult", "Pierce", "Tvillingkanoner", "Sökare", "Rikoschett", "Sköldcell", "Sköldmatris")
    headlines = (
        "SECTOR CLEAR - World 1 complete",
        "SECTOR CLEAR - World 7 complete",
        "SEKTOR KLAR - Värld 1 klar",
        "RUN OVER - out of lives (wave 12)",
        "SLUT - inga liv kvar (våg 12)",
        "Continue to World 2",
        "Fortsätt till värld 2",
        "Next Wave",
        "Nästa våg",
        "New Run (reset)",
        "Ny runda (nollställ)",
        "Your ship, upgrades and credits reset on New Run.",
        "Skepp, uppgraderingar och kredit nollställs vid Ny runda.",
    )
    for width, height in resolutions:
        scale = _canvas_scale(width, height)
        canvas_w = width / scale
        hull_w = 0.110 * (hangar[2] - hangar[0]) * canvas_w
        weapon_w = (0.735 - 0.51) * (hangar[2] - hangar[0]) * canvas_w
        for name in hull_en + hull_sv:
            assert _wrapped_line_count(name, hull_w, 14) <= 2, (width, name)
            for word in name.split(" "):
                assert _estimate_width(word, 14) <= hull_w, (width, word, hull_w)
        for name in wide_en + wide_sv:
            assert _estimate_width(name, 18) <= weapon_w, (width, name, weapon_w)
        summary_box = _map_anchors(*hangar, 0.02, 0.82, 0.98, 0.995)
        title_box = _map_anchors(*summary_box, 0.03, 0.62, 0.70, 0.96)
        title_w = (title_box[2] - title_box[0]) * canvas_w
        primary = _map_anchors(*hangar, 0.03, 0.735, 0.97, 0.800)
        primary_w = (primary[2] - primary[0]) * canvas_w
        explain = _map_anchors(*summary_box, 0.03, 0.06, 0.97, 0.34)
        explain_w = (explain[2] - explain[0]) * canvas_w
        explain_h = (explain[3] - explain[1]) * (height / scale)
        for line in headlines:
            if line.startswith("SECTOR") or line.startswith("SEKTOR") or line.startswith("RUN") or line.startswith("SLUT"):
                assert _estimate_width(line, 28) <= title_w, (width, line, title_w)
            elif line.startswith("Continue") or line.startswith("Fortsätt") or line in ("Next Wave", "Nästa våg", "New Run (reset)", "Ny runda (nollställ)"):
                assert _estimate_width(line, 20) <= primary_w, (width, line, primary_w)
            else:
                lines = _wrapped_line_count(line, explain_w, 16)
                assert lines * 16 * 1.1 <= explain_h, (width, line, lines, explain_h)

        flight = summary_box
        preview = (0.562, 0.080, 0.986, 0.596)
        failed_preview = (0.562, 0.080, 0.986, 0.468)
        doctrine = (0.562, 0.608, 0.986, 0.898)
        gear = (0.900, 0.905, 0.988, 0.995)
        difficulty = (0.748, 0.905, 0.888, 0.995)
        toast = (0.500, 0.912, 0.735, 0.988)
        hint = (0.14, 0.008, 0.86, 0.072)
        credits = (0.014, 0.010, 0.128, 0.070)
        title = (0.012, 0.905, 0.205, 0.995)
        world = (0.205, 0.905, 0.355, 0.995)
        medals = (0.355, 0.950, 0.478, 0.995)
        ladder = (0.355, 0.905, 0.478, 0.950)
        health = (0.562, 0.480, 0.986, 0.596)
        primary_screen = primary
        shop_headers = (
            _map_anchors(*hangar, 0.02, 0.675, 0.49, 0.728),
            _map_anchors(*hangar, 0.51, 0.704, 0.735, 0.728),
            _map_anchors(*hangar, 0.755, 0.675, 0.98, 0.728),
        )
        cells = []
        row_step = 0.094 + 0.016
        for col in range(4):
            for row in range(3):
                if row == 2 and col > 1:
                    continue
                x0 = 0.02 + col * 0.1175
                top = 0.665 - row * row_step
                cells.append(_map_anchors(*hangar, x0, top - 0.094, x0 + 0.110, top))
        for index in range(5):
            top = 0.665 - index * row_step
            cells.append(_map_anchors(*hangar, 0.51, top - 0.094, 0.735, top))
        for index in range(2):
            top = 0.665 - index * row_step
            cells.append(_map_anchors(*hangar, 0.755, top - 0.094, 0.98, top))

        def visible(name_rects):
            pairs = list(name_rects.items())
            for i, (left_name, left) in enumerate(pairs):
                for right_name, right in pairs[i + 1 :]:
                    if _contains(left, right) or _contains(right, left):
                        continue
                    assert not _overlap(left, right), (width, height, left_name, right_name)

        shared = {
            "hangar": hangar,
            "primary": primary_screen,
            "preview": preview,
            "gear": gear,
            "difficulty": difficulty,
            "toast": toast,
            "hint": hint,
            "credits": credits,
            "title": title,
            "world": world,
            "medals": medals,
            "ladder": ladder,
        }
        for cell_index, cell in enumerate(cells):
            shared[f"cell{cell_index}"] = cell
        for header_index, header in enumerate(shop_headers):
            shared[f"header{header_index}"] = header
        first = dict(shared)
        first["flight"] = _map_anchors(*hangar, 0.02, 0.800, 0.98, 0.995)
        visible(first)
        with_doctrine = dict(shared)
        with_doctrine["summary"] = flight
        with_doctrine["doctrine"] = doctrine
        visible(with_doctrine)
        failed_screen = dict(shared)
        failed_screen["preview"] = failed_preview
        failed_screen["summary"] = flight
        failed_screen["health"] = health
        visible(failed_screen)
        boundary = dict(with_doctrine)
        visible(boundary)


if __name__ == "__main__":
    sys.exit(main())
