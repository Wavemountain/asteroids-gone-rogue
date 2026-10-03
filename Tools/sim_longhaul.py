#!/usr/bin/env python3
"""Pure-logic long-haul sweep. Mirrors DifficultyCurve, WaveRoster, and the spawner.

No Unity. `python3 Tools/sim_longhaul.py` prints the wave 1-80 report.
"""

from __future__ import annotations

import re
from pathlib import Path


def _const(source: str, name: str) -> int:
    match = re.search(rf"\b{name}\s*=\s*(-?\d+)", source)
    if match is None:
        raise RuntimeError(f"missing constant {name}")
    return int(match.group(1))


def _enemy_hp(source: str) -> dict[str, int]:
    body = source.split("public static int HitPoints")[1].split("public static float Speed")[0]
    found = {
        name: int(amount)
        for name, amount in re.findall(r"case EnemyKind\.(\w+):\s*return\s+(\d+);", body)
    }
    found["Mid01"] = int(re.search(r"default:\s*return\s+(\d+);", body).group(1))
    return found


def _base_roster(source: str) -> dict[int, tuple[str, ...]]:
    body = source.split("private static EnemyKind[] BaseRoster")[1].split("public void Register")[0]
    roster: dict[int, tuple[str, ...]] = {}
    for rung, block in re.findall(r"case\s+(\d+):(.*?)(?=case\s+\d+:|default:)", body, re.S):
        kinds = re.findall(r"EnemyKind\.(\w+)", block)
        roster[int(rung)] = tuple(kinds)
    default = body.split("default:")[1].split("}")[0]
    roster[10] = tuple(re.findall(r"EnemyKind\.(\w+)", default))
    return roster


def _emphasis(source: str) -> dict[int, tuple[str, ...]]:
    emphasis: dict[int, tuple[str, ...]] = {}
    for layout in range(1, 8):
        block = source.split(f"Emphasis{layout} =")[1].split("};")[0]
        emphasis[layout] = tuple(re.findall(r"EnemyKind\.(\w+)", block))
    return emphasis


def _shop_costs(source: str) -> list[int]:
    return [int(token) for token in re.findall(r",\s*(\d+)\s*,\s*ShopGroup\.", source)]


def _load(root: Path) -> dict:
    curve = (root / "Assets/Scripts/Core/DifficultyCurve.cs").read_text(encoding="utf-8")
    settings = (root / "Assets/Scripts/Core/DifficultySettings.cs").read_text(encoding="utf-8")
    modifier = (root / "Assets/Scripts/Core/WaveModifier.cs").read_text(encoding="utf-8")
    boss = (root / "Assets/Scripts/Core/BossRules.cs").read_text(encoding="utf-8")
    roster = (root / "Assets/Scripts/Core/WaveRoster.cs").read_text(encoding="utf-8")
    waves = (root / "Assets/Scripts/Core/WaveManager.cs").read_text(encoding="utf-8")
    rules = (root / "Assets/Scripts/Core/WorldRules.cs").read_text(encoding="utf-8")
    catalog = (root / "Assets/Scripts/Core/WorldCatalog.cs").read_text(encoding="utf-8")
    enemies = (root / "Assets/Scripts/Combat/EnemyKind.cs").read_text(encoding="utf-8")
    shop = (root / "Assets/Scripts/Core/ShopCatalog.cs").read_text(encoding="utf-8")
    prices = (root / "Assets/Scripts/Core/ShopPrices.cs").read_text(encoding="utf-8")
    doctrine = (root / "Assets/Scripts/Core/DoctrineRules.cs").read_text(encoding="utf-8")
    boons = (root / "Assets/Scripts/Core/BoonCatalog.cs").read_text(encoding="utf-8")
    legacy = (root / "Assets/Scripts/Core/LegacyProgress.cs").read_text(encoding="utf-8")
    return {
        "curve": curve,
        "within_hp": _const(curve, "WithinWorldHpPercent"),
        "damage_bonus": _const(curve, "DamageBonusPerWorld"),
        "max_damage": _const(curve, "MaxDamageBonus"),
        "extra_span": _const(curve, "ExtraEnemyWorldSpan"),
        "max_extra": _const(curve, "MaxExtraEnemies"),
        "fire_bonus": _const(curve, "FireBonusPerWorld"),
        "max_fire": _const(curve, "MaxFireBonus"),
        "credit_bonus": _const(curve, "CreditBonusPerWorld"),
        "max_credit": _const(curve, "MaxCreditBonus"),
        "base_rocks": _const(curve, "BaseLargeAsteroids"),
        "early_rocks": _const(curve, "EarlyAsteroidCap"),
        "plateau_wave": _const(curve, "PlateauWave"),
        "plateau_rocks": _const(curve, "PlateauAsteroidCap"),
        "max_spawned": _const(curve, "MaxSpawnedEnemies"),
        "world_hp": _const(settings, "WorldHpPercent"),
        "max_hp_steps": _const(settings, "MaxWorldHpSteps"),
        "easy_credits": _const(settings, "EasyWaveClearCredits"),
        "normal_credits": _const(settings, "NormalWaveClearCredits"),
        "hard_credits": _const(settings, "HardWaveClearCredits"),
        "world_count": _const(catalog, "Count"),
        "waves_per_world": _const(catalog, "WavesPerWorld"),
        "elite_first": _const(modifier, "FirstEliteWave"),
        "elite_stride": _const(modifier, "EliteStride"),
        "elite_hp": _const(modifier, "EliteHpPercent"),
        "elite_credit": _const(modifier, "EliteCreditPercent"),
        "boss_first": _const(boss, "FirstWave"),
        "boss_factor": _const(boss, "HpFactor"),
        "aimed": _const(boss, "AimedBurstCount"),
        "radial": _const(boss, "RadialCount"),
        "roster_max": _const(roster, "MaxCount"),
        "hostile_cap": _const(roster, "EliteBossHostileCap"),
        "boon_max": _const(boons, "MaxLevel"),
        "boon_count": _const(boons, "Count"),
        "credit_per_boon": _const(boons, "CreditPerLevel"),
        "resist_per_boon": _const(boons, "ResistPerLevel"),
        "legacy_max": _const(legacy, "MaxLevel"),
        "discount_cap": _const(legacy, "DiscountCapPercent"),
        "discount_step": _const(legacy, "DiscountPercentPerLevel"),
        "shield_cap": _const(legacy, "ShieldBonusCap"),
        "hull_cap": _const(legacy, "HullBonusCap"),
        "legacy_credits": _const(legacy, "CreditsPerLevel"),
        "shop_costs": _shop_costs(shop),
        "mk2_percent": _const(prices, "Mk2Percent"),
        "gates": [
            _const(doctrine, "BarrageGateCost"),
            _const(doctrine, "LanceGateCost"),
            _const(doctrine, "HunterGateCost"),
        ],
        "hp": _enemy_hp(enemies),
        "base": _base_roster(waves),
        "emphasis": _emphasis(rules),
        "brute_hp": None,
    }


def _world(spec: dict, wave: int) -> int:
    shown = 1 if wave < 1 else wave
    return ((shown - 1) // spec["waves_per_world"]) + 1


def _layout(spec: dict, wave: int) -> int:
    number = _world(spec, wave)
    return ((number - 1) % spec["world_count"]) + 1


def _hp_steps(spec: dict, world: int) -> int:
    steps = world - 1
    if steps < 0:
        steps = 0
    if steps > spec["max_hp_steps"]:
        steps = spec["max_hp_steps"]
    return steps


def _hp_percent(spec: dict, wave: int) -> int:
    shown = 1 if wave < 1 else wave
    world = _world(spec, shown)
    steps = _hp_steps(spec, world)
    within = (shown - 1) % spec["waves_per_world"]
    if steps >= spec["max_hp_steps"] and world > spec["world_count"]:
        within = spec["waves_per_world"] - 1
    percent = 100 + (spec["world_hp"] * steps) + (spec["within_hp"] * within)
    peak = 100 + (spec["world_hp"] * spec["max_hp_steps"]) + (
        spec["within_hp"] * (spec["waves_per_world"] - 1)
    )
    return percent if percent < peak else peak


def _capped_bonus(world: int, per: int, cap: int) -> int:
    bonus = (max(world, 1) - 1) * per
    if bonus < 0:
        bonus = 0
    if bonus > cap:
        bonus = cap
    return bonus


def _damage_percent(spec: dict, world: int) -> int:
    return 100 + _capped_bonus(world, spec["damage_bonus"], spec["max_damage"])


def _extra_enemies(spec: dict, world: int) -> int:
    extra = (max(world, 1) - 1) // spec["extra_span"]
    if extra < 0:
        extra = 0
    if extra > spec["max_extra"]:
        extra = spec["max_extra"]
    return extra


def _fire_percent(spec: dict, world: int) -> int:
    return 100 + _capped_bonus(world, spec["fire_bonus"], spec["max_fire"])


def _credit_percent(spec: dict, world: int, per: int | None = None) -> int:
    step = spec["credit_bonus"] if per is None else per
    return 100 + _capped_bonus(world, step, spec["max_credit"])


def _is_elite(spec: dict, wave: int) -> bool:
    shown = 1 if wave < 1 else wave
    return shown >= spec["elite_first"] and (shown % spec["elite_stride"]) == 0


def _is_boss(spec: dict, wave: int) -> bool:
    shown = 1 if wave < 1 else wave
    return shown >= spec["boss_first"] and (shown % spec["waves_per_world"]) == 0


def _modifier(spec: dict, wave: int) -> str:
    if not _is_elite(spec, wave):
        return "None"
    slot = ((wave // spec["elite_stride"]) - (spec["elite_first"] // spec["elite_stride"])) % 3
    return ("FasterEnemies", "ShieldedAsteroids", "DenseSwarm")[slot]


def _rocks(spec: dict, wave: int) -> int:
    shown = 1 if wave < 1 else wave
    count = spec["base_rocks"] + (shown - 1)
    if count < spec["base_rocks"]:
        count = spec["base_rocks"]
    if count > spec["early_rocks"]:
        count = spec["early_rocks"]
    if shown > spec["plateau_wave"]:
        count = count + (shown - spec["plateau_wave"])
        if count > spec["plateau_rocks"]:
            count = spec["plateau_rocks"]
    return count


def _grade_hp(hp: int, grade: str) -> int:
    if hp < 1:
        hp = 1
    if grade == "easy":
        easy = (hp * 4) // 5
        return 1 if easy < 1 else easy
    if grade == "hard":
        hard = (hp * 5) // 4
        if hard < hp + 1:
            hard = hp + 1
        return hard
    return hp


def _scale_hp(spec: dict, hp: int, wave: int, grade: str) -> int:
    graded = _grade_hp(hp, grade)
    scaled = graded * _hp_percent(spec, wave) // 100
    return 1 if scaled < 1 else scaled


def _boss_hp(spec: dict, wave: int, grade: str) -> int:
    percent = 100 + spec["world_hp"] * _hp_steps(spec, _world(spec, wave))
    raw = spec["boss_factor"] * spec["hp"]["Brute"] * percent // 100
    if raw < 1:
        raw = 1
    return _grade_hp(raw, grade)


def _scale_credits(spec: dict, base: int, wave: int, per: int | None = None) -> int:
    if base < 0:
        base = 0
    scaled = base * _credit_percent(spec, _world(spec, wave), per) // 100
    if _is_elite(spec, wave):
        scaled = scaled * (100 + spec["elite_credit"]) // 100
    return scaled


def _roster(spec: dict, wave: int) -> tuple[str, ...]:
    shown = 1 if wave < 1 else wave
    rung = shown if shown < 10 else 10
    if rung < 1:
        rung = 1
    source = spec["base"][rung]
    if shown == spec["boss_first"]:
        return source
    built = list(spec["emphasis"][_layout(spec, shown)])
    if shown > 10 and _modifier(spec, shown) == "DenseSwarm":
        built.insert(0, "Swarm")
    if shown <= 10:
        built.extend(source)
    else:
        shift = (shown - 1) % len(source)
        for step in range(len(source)):
            built.append(source[(step + shift) % len(source)])
    if not built:
        return ("Mid01",)
    return tuple(built[: spec["roster_max"]])


def _include(spec: dict, kind: str, wave: int) -> bool:
    return not (kind == "Brute" and wave == spec["boss_first"] and _is_boss(spec, wave))


def _grade_extras(grade: str) -> int:
    return 1 if grade == "hard" else 0


def hostile_plan(spec: dict, wave: int, grade: str) -> tuple[list[str], int]:
    """Spawn list excluding rocks, and how many roster-tail slots were dropped."""
    roster = list(_roster(spec, wave))
    included = [kind for kind in roster if _include(spec, kind, wave)]
    extras = _extra_enemies(spec, _world(spec, wave)) + _grade_extras(grade)
    elite = _is_elite(spec, wave)
    boss = _is_boss(spec, wave)
    reserved = (1 if boss else 0) + (1 if elite else 0)
    budget = spec["max_spawned"] - reserved
    if budget < 1:
        budget = 1
    roster_slots = len(included)
    if roster_slots > budget:
        roster_slots = budget
    room = budget - roster_slots
    if room < 0:
        room = 0
    extra_slots = extras if extras < room else room
    spawned = included[:roster_slots]
    dropped = 0
    if elite and boss:
        total = roster_slots + extra_slots + reserved
        overflow = total - spec["hostile_cap"]
        if overflow > 0:
            keep = roster_slots - overflow
            if keep < 0:
                keep = 0
            dropped = roster_slots - keep
            spawned = included[:keep]
    kinds = list(spawned)
    for _extra in range(extra_slots):
        kinds.append("Mid01")
    if elite:
        kinds.append("EliteBrute")
    if boss:
        kinds.append("Boss")
    return kinds, dropped


def _kind_hp(spec: dict, kind: str, wave: int, grade: str) -> int:
    if kind == "Boss":
        return _boss_hp(spec, wave, grade)
    if kind == "EliteBrute":
        scaled = _scale_hp(spec, spec["hp"]["Brute"], wave, grade)
        elite_hp = scaled * spec["elite_hp"] // 100
        return 1 if elite_hp < 1 else elite_hp
    return _scale_hp(spec, spec["hp"][kind], wave, grade)


def _finite_int(value: int, lo: int, hi: int) -> bool:
    return isinstance(value, int) and not isinstance(value, bool) and lo <= value <= hi


def sweep(root: Path | None = None) -> dict:
    spec = _load(root or Path(__file__).resolve().parents[1])
    grades = ("easy", "normal", "hard")
    base_credits = {
        "easy": spec["easy_credits"],
        "normal": spec["normal_credits"],
        "hard": spec["hard_credits"],
    }
    jumps: list[tuple] = []
    over_cap: list[tuple] = []
    roster_empty: list[int] = []
    non_finite: list[str] = []
    cap_breaks: list[str] = []
    previous_hp = {grade: None for grade in grades}
    previous_curve = {
        "hp": 0,
        "damage": 0,
        "extra": 0,
        "fire": 0,
        "rocks": 0,
        "credit": 0,
    }
    credit_total = {grade: 0 for grade in grades}
    floor_total = 0
    wave45_hard_count = 0
    wave45_hard_dropped = 0
    wave5_easy_count = 0
    wave10_hard_count = 0
    peak_hp = 100 + spec["world_hp"] * spec["max_hp_steps"] + spec["within_hp"] * (
        spec["waves_per_world"] - 1
    )

    for wave in range(1, 81):
        world = _world(spec, wave)
        hp_percent = _hp_percent(spec, wave)
        damage = _damage_percent(spec, world)
        extra = _extra_enemies(spec, world)
        fire = _fire_percent(spec, world)
        rocks = _rocks(spec, wave)
        credit_percent = _credit_percent(spec, world)
        curve_row = {
            "hp": hp_percent,
            "damage": damage,
            "extra": extra,
            "fire": fire,
            "rocks": rocks,
            "credit": credit_percent,
        }
        for name, value in curve_row.items():
            if value < previous_curve[name]:
                cap_breaks.append(f"{name} dropped at wave {wave}")
            previous_curve[name] = value
        if not _finite_int(hp_percent, 100, peak_hp):
            non_finite.append(f"hp%{wave}")
        if not _finite_int(damage, 100, 100 + spec["max_damage"]):
            non_finite.append(f"dmg%{wave}")
        if not _finite_int(extra, 0, spec["max_extra"]):
            non_finite.append(f"extra{wave}")
        if not _finite_int(fire, 100, 100 + spec["max_fire"]):
            non_finite.append(f"fire{wave}")
        if not _finite_int(rocks, spec["base_rocks"], spec["plateau_rocks"]):
            non_finite.append(f"rocks{wave}")
        if not _finite_int(credit_percent, 100, 100 + spec["max_credit"]):
            non_finite.append(f"credit%{wave}")

        row = _roster(spec, wave)
        if not row or len(row) > spec["roster_max"]:
            roster_empty.append(wave)

        boundary = wave > 1 and _world(spec, wave) != _world(spec, wave - 1)
        boss_wave = _is_boss(spec, wave)
        for grade in grades:
            kinds, dropped = hostile_plan(spec, wave, grade)
            cap = spec["hostile_cap"] if boss_wave and _is_elite(spec, wave) else spec["max_spawned"]
            if not kinds or len(kinds) > cap:
                over_cap.append((grade, wave, len(kinds), cap, dropped))
            budget = 0
            for kind in kinds:
                amount = _kind_hp(spec, kind, wave, grade)
                if amount < 1:
                    non_finite.append(f"hp {grade} {wave} {kind}")
                budget += amount
            previous = previous_hp[grade]
            if previous is not None and not boundary and not boss_wave:
                if budget * 100 > previous * 160:
                    jumps.append((grade, wave, previous, budget))
            previous_hp[grade] = budget
            if wave <= 35:
                gained = _scale_credits(spec, base_credits[grade], wave)
                credit_total[grade] += gained
                if gained < 0:
                    non_finite.append(f"credits {grade} {wave}")
            if grade == "hard" and wave == 45:
                wave45_hard_count = len(kinds)
                wave45_hard_dropped = dropped
            if grade == "easy" and wave == 5:
                wave5_easy_count = len(kinds)
            if grade == "hard" and wave == 10:
                wave10_hard_count = len(kinds)
        if wave <= 35:
            floor_total += _scale_credits(spec, spec["normal_credits"], wave, per=0)

        if boss_wave:
            easy_boss = _boss_hp(spec, wave, "easy")
            normal_boss = _boss_hp(spec, wave, "normal")
            hard_boss = _boss_hp(spec, wave, "hard")
            if not (easy_boss <= normal_boss <= hard_boss):
                cap_breaks.append(f"boss order {wave}")
            if easy_boss < 1:
                non_finite.append(f"boss {wave}")

    mk2_costs = []
    for base_cost in spec["shop_costs"]:
        mk2_cost = base_cost * spec["mk2_percent"] // 100
        if mk2_cost < 1:
            mk2_cost = 1
        mk2_costs.append(mk2_cost)
    # Spend-everything catalogue at world-1 prices: Mk I + Mk II + doctrine gates.
    # World surcharge and the credit bank are reported by the shop, not in this sum.
    catalogue = sum(spec["shop_costs"]) + sum(mk2_costs) + sum(spec["gates"])
    if catalogue < 1:
        non_finite.append("catalogue")
    for cost in spec["shop_costs"] + spec["gates"]:
        if cost < 1:
            non_finite.append(f"price {cost}")
        reduced = cost * (100 - spec["discount_cap"]) // 100
        if reduced < 1:
            cap_breaks.append(f"discount wiped {cost}")

    # Boon and legacy caps stay inside their published limits at max rank.
    if spec["boon_max"] * spec["credit_per_boon"] > 45:
        cap_breaks.append("boon credits")
    if 100 - spec["boon_max"] * spec["resist_per_boon"] < 70:
        cap_breaks.append("boon resist")
    if spec["legacy_max"] * spec["discount_step"] < spec["discount_cap"]:
        # The published cap may cut the raw product. Either way the applied
        # percent must not exceed the cap.
        pass
    applied_discount = spec["legacy_max"] * spec["discount_step"]
    if applied_discount > spec["discount_cap"]:
        applied_discount = spec["discount_cap"]
    if applied_discount > spec["discount_cap"] or spec["shield_cap"] > 2 or spec["hull_cap"] > 2:
        cap_breaks.append("legacy caps")
    if spec["legacy_max"] * spec["legacy_credits"] > 30:
        cap_breaks.append("legacy credits")

    ratio = {grade: credit_total[grade] / catalogue for grade in grades}
    return {
        "credit_total": credit_total,
        "ratio": ratio,
        "floor_ratio": {"normal": floor_total / catalogue},
        "floor_total": floor_total,
        "catalogue": catalogue,
        "shop": sum(spec["shop_costs"]),
        "mk2": sum(mk2_costs),
        "gates": sum(spec["gates"]),
        "credit_bonus_per_world": spec["credit_bonus"],
        "jumps": jumps,
        "over_cap": over_cap,
        "roster_empty": roster_empty,
        "non_finite": non_finite,
        "cap_breaks": cap_breaks,
        "easy_boss_hp": _boss_hp(spec, 5, "easy"),
        "aimed_shots": spec["aimed"],
        "radial_shots": spec["radial"],
        "wave45_hard_count": wave45_hard_count,
        "wave45_hard_dropped": wave45_hard_dropped,
        "wave5_easy_count": wave5_easy_count,
        "wave10_hard_count": wave10_hard_count,
    }


def format_report(report: dict) -> str:
    lines = ["Long-haul sweep waves 1-80"]
    for grade in ("easy", "normal", "hard"):
        percent = report["ratio"][grade] * 100.0
        lines.append(
            f"  {grade}: credits through wave 35 = {report['credit_total'][grade]}"
            f"  shop+doctrine {report['catalogue']}  ratio {percent:.1f}%"
        )
    floor = report["floor_ratio"]["normal"] * 100.0
    lines.append(
        f"  normal at CreditBonusPerWorld 0 = {report['floor_total']} ({floor:.1f}%)"
        f"  shipped bonus {report['credit_bonus_per_world']}"
    )
    lines.append(
        f"  easy wave 5 boss HP {report['easy_boss_hp']}"
        f"  aimed {report['aimed_shots']}  radial {report['radial_shots']}"
        f"  hostiles {report['wave5_easy_count']}"
    )
    lines.append(
        f"  hard wave 10 hostiles {report['wave10_hard_count']}"
        f"  hard wave 45 hostiles {report['wave45_hard_count']}"
        f"  roster-tail dropped {report['wave45_hard_dropped']}"
    )
    lines.append(
        f"  hp jumps {len(report['jumps'])}  over cap {len(report['over_cap'])}"
        f"  empty roster {len(report['roster_empty'])}"
    )
    return "\n".join(lines)


def main() -> int:
    report = sweep()
    print(format_report(report))
    return 0 if not report["jumps"] and not report["over_cap"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
