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

    @property
    def can_start(self) -> bool:
        return self.phase in (Phase.HANGAR, Phase.WAVE_CLEAR, Phase.FAILED)

    def begin(self) -> None:
        assert self.can_start
        self.phase = Phase.PLAYING

    def add_score(self, amount: int) -> None:
        self.score += amount

    def complete(self, bonus: int = 100, credits: int = 150) -> None:
        assert self.phase == Phase.PLAYING
        self.score += bonus
        self.credits += credits
        self.wave += 1
        self.phase = Phase.WAVE_CLEAR

    def fail(self) -> None:
        assert self.phase == Phase.PLAYING
        self.phase = Phase.FAILED

    def hangar(self) -> None:
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

    @property
    def cooldown(self) -> float:
        return 0.16 if self.rapid else 0.38

    @property
    def damage(self) -> int:
        return 2 if self.nose else 1


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
    assert s.wave == 1
    assert s.credits == 0
    s.hangar()
    s.begin()
    assert s.wave == 1


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


def test_nose_changes_damage() -> None:
    loadout = Loadout()
    assert loadout.damage == 1
    loadout.nose = True
    assert loadout.damage == 2


def main() -> int:
    test_clear_loop()
    test_fail_keeps_wave_and_upgrades()
    test_shop_cannot_overspend()
    test_nose_changes_damage()
    print("Week 1 logic tests passed (Hangar → Play → Clear/Fail + shop persist)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
