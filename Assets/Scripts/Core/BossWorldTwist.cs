namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Per-world boss pattern, telegraph times, and the fire-rate caps.
    /// Twists change angles and telegraph length. They do not add bolts
    /// except Spoke Ring phase 3, which removes one. Unity-free.
    /// </summary>
    public static class BossWorldTwist
    {
        public const int StepAimed = 0;
        public const int StepRadial = 1;
        public const int StepFan = 2;
        public const int StepRadialRot = 3;
        public const int StepGap = 4;
        public const int StepRadial2 = 5;
        public const int StepAimed2 = 6;

        public const int TotalCapPermille = 1300;
        public const int AimedCapPermille = 1350;
        public const float Phase3SpeedMul = 1.10f;
        public const float RotateRadians = 0.39269908169f;
        public const float TrenchHalfWidth = 8.2f;
        public const float MineRadius = 18.5f;
        public const int PylonCount = 6;
        public const int SpokeCount = 4;

        public static int NormalizeLayout(int layout)
        {
            int world = layout;
            if (world < 1)
            {
                world = 1;
            }

            int index = (world - 1) % 7;
            if (index < 0)
            {
                index += 7;
            }

            return index + 1;
        }

        public static int EffectivePhase(int phase, int grade)
        {
            int safe = phase;
            if (safe < 0)
            {
                safe = 0;
            }

            if (safe > 2)
            {
                safe = 2;
            }

            if (safe == 2 && grade <= 0)
            {
                return 1;
            }

            return safe;
        }

        public static int AimedWindupMs(int layout)
        {
            if (NormalizeLayout(layout) == 6)
            {
                return 500;
            }

            return 400;
        }

        public static int RadialTelegraphMs(int layout)
        {
            if (NormalizeLayout(layout) == 6)
            {
                return 1000;
            }

            return 800;
        }

        public static int Phase3RadialTelegraphMs(int layout)
        {
            if (NormalizeLayout(layout) == 6)
            {
                return RadialTelegraphMs(layout);
            }

            return 600;
        }

        public static float AimedWindupSeconds(int layout)
        {
            return AimedWindupMs(layout) / 1000f;
        }

        public static float RadialTelegraphSeconds(int layout)
        {
            return RadialTelegraphMs(layout) / 1000f;
        }

        public static int PatternLength(int phase, int grade)
        {
            int effective = EffectivePhase(phase, grade);
            if (effective <= 0)
            {
                return 2;
            }

            if (effective == 1)
            {
                return 3;
            }

            return 4;
        }

        public static int PatternStep(int phase, int grade, int index)
        {
            int length = PatternLength(phase, grade);
            int slot = index % length;
            if (slot < 0)
            {
                slot += length;
            }

            int effective = EffectivePhase(phase, grade);
            if (effective <= 0)
            {
                return slot == 0 ? StepAimed : StepRadial;
            }

            if (effective == 1)
            {
                if (slot == 0)
                {
                    return StepFan;
                }

                if (slot == 1)
                {
                    return StepRadialRot;
                }

                return StepGap;
            }

            if (slot == 0)
            {
                return StepRadial2;
            }

            if (slot == 1)
            {
                return StepAimed2;
            }

            if (slot == 2)
            {
                return StepGap;
            }

            return StepAimed;
        }

        public static int GapCount(int layout, int phase, int grade)
        {
            if (EffectivePhase(phase, grade) >= 2 && NormalizeLayout(layout) == 7)
            {
                return 2;
            }

            return 1;
        }

        public static int GapBolts(int layout, int phase, int grade)
        {
            return 8 - GapCount(layout, phase, grade);
        }

        public static int StepBolts(int layout, int phase, int grade, int step)
        {
            if (step == StepAimed)
            {
                return 3;
            }

            if (step == StepRadial || step == StepRadialRot)
            {
                return 8;
            }

            if (step == StepFan)
            {
                return 5;
            }

            if (step == StepGap)
            {
                return GapBolts(layout, phase, grade);
            }

            if (step == StepRadial2)
            {
                return 16;
            }

            if (step == StepAimed2)
            {
                return 6;
            }

            return 0;
        }

        public static int StepAimedBolts(int step)
        {
            if (step == StepAimed || step == StepFan)
            {
                return 3;
            }

            if (step == StepAimed2)
            {
                return 6;
            }

            return 0;
        }

        public static int GapMs(int layout, int phase, int grade, int step)
        {
            int effective = EffectivePhase(phase, grade);
            if (step == StepAimed || step == StepFan || step == StepAimed2)
            {
                return 1700;
            }

            if (effective >= 2)
            {
                return 2850;
            }

            return 2600;
        }

        public static int LeadMs(int layout, int step)
        {
            if (step == StepAimed || step == StepFan || step == StepAimed2)
            {
                return AimedWindupMs(layout);
            }

            if (step == StepRadial2)
            {
                return Phase3RadialTelegraphMs(layout);
            }

            return RadialTelegraphMs(layout);
        }

        public static int StepMs(int layout, int phase, int grade, int step)
        {
            int wind = AimedWindupMs(layout);
            int aimedGap = GapMs(layout, phase, grade, StepAimed);
            int radialTele = RadialTelegraphMs(layout);
            int radialGap = GapMs(layout, phase, grade, step);
            if (step == StepAimed || step == StepFan)
            {
                return wind + aimedGap;
            }

            if (step == StepRadial || step == StepRadialRot || step == StepGap)
            {
                return radialTele + radialGap;
            }

            if (step == StepRadial2)
            {
                return Phase3RadialTelegraphMs(layout) + 400 + radialGap;
            }

            if (step == StepAimed2)
            {
                return wind + 350 + wind + aimedGap;
            }

            return 0;
        }

        public static void Cycle(int layout, int phase, int grade, out int bolts, out int aimed, out int ms)
        {
            bolts = 0;
            aimed = 0;
            ms = 0;
            int length = PatternLength(phase, grade);
            for (int index = 0; index < length; index++)
            {
                int step = PatternStep(phase, grade, index);
                bolts += StepBolts(layout, phase, grade, step);
                aimed += StepAimedBolts(step);
                ms += StepMs(layout, phase, grade, step);
            }
        }

        public static bool WithinCap(int layout, int phase, int grade)
        {
            int baseBolts;
            int baseAimed;
            int baseMs;
            int bolts;
            int aimed;
            int ms;
            Cycle(layout, 0, grade, out baseBolts, out baseAimed, out baseMs);
            Cycle(layout, phase, grade, out bolts, out aimed, out ms);
            if (baseMs <= 0 || ms <= 0 || baseBolts <= 0)
            {
                return false;
            }

            if ((long)bolts * baseMs * 1000 > (long)baseBolts * ms * TotalCapPermille)
            {
                return false;
            }

            if (baseAimed <= 0)
            {
                return aimed == 0;
            }

            return (long)aimed * baseMs * 1000 <= (long)baseAimed * ms * AimedCapPermille;
        }

        public static bool PhaseTelegraphsFair()
        {
            for (int layout = 1; layout <= 7; layout++)
            {
                int wind = AimedWindupMs(layout);
                if (wind < 300 || wind > 500)
                {
                    return false;
                }

                if (RadialTelegraphMs(layout) < 600)
                {
                    return false;
                }

                if (Phase3RadialTelegraphMs(layout) < 600)
                {
                    return false;
                }
            }

            return true;
        }

        public static float AlignRadians(int layout)
        {
            int world = NormalizeLayout(layout);
            if (world == 2 || world == 7)
            {
                return 0f;
            }

            return 0f;
        }

        public static int GapSpokeIndex(int layout)
        {
            if (NormalizeLayout(layout) == 2)
            {
                return 1;
            }

            return 0;
        }

        public static bool ArmsGapMine(int layout, int phase)
        {
            return NormalizeLayout(layout) == 4 && phase >= 1;
        }

        public static bool ShowsTrenchLaser(int layout)
        {
            return NormalizeLayout(layout) == 3;
        }

        public static bool FarDriftInTrench(float playerX)
        {
            float x = playerX;
            if (x < 0f)
            {
                x = -x;
            }

            return x < TrenchHalfWidth;
        }

        public static float CorridorDegrees(int slot)
        {
            int index = slot % 4;
            if (index < 0)
            {
                index += 4;
            }

            return 45f + (90f * index);
        }

        public static bool SelfCheck()
        {
            if (!PhaseTelegraphsFair())
            {
                return false;
            }

            if (PatternStep(2, 1, 0) != StepRadial2
                || PatternStep(2, 1, 1) != StepAimed2
                || PatternStep(2, 1, 2) != StepGap
                || PatternStep(2, 1, 3) != StepAimed)
            {
                return false;
            }

            if (PatternLength(2, 0) != PatternLength(1, 0))
            {
                return false;
            }

            for (int slot = 0; slot < PatternLength(1, 1); slot++)
            {
                if (PatternStep(2, 0, slot) != PatternStep(1, 1, slot))
                {
                    return false;
                }
            }

            int bolts;
            int aimed;
            int ms;
            Cycle(1, 0, 1, out bolts, out aimed, out ms);
            if (bolts != 11 || aimed != 3 || ms != 5500)
            {
                return false;
            }

            Cycle(1, 1, 1, out bolts, out aimed, out ms);
            if (bolts != 20 || aimed != 3 || ms != 8900)
            {
                return false;
            }

            Cycle(1, 2, 1, out bolts, out aimed, out ms);
            if (bolts != 32 || aimed != 9 || ms != 12450)
            {
                return false;
            }

            Cycle(7, 2, 2, out bolts, out aimed, out ms);
            if (bolts != 31 || aimed != 9 || ms != 12450)
            {
                return false;
            }

            for (int layout = 1; layout <= 7; layout++)
            {
                for (int grade = 0; grade <= 2; grade++)
                {
                    for (int phase = 0; phase <= 2; phase++)
                    {
                        if (!WithinCap(layout, phase, grade))
                        {
                            return false;
                        }
                    }

                    int easyBolts;
                    int easyAimed;
                    int easyMs;
                    int phase2Bolts;
                    int phase2Aimed;
                    int phase2Ms;
                    Cycle(layout, 2, 0, out easyBolts, out easyAimed, out easyMs);
                    Cycle(layout, 1, 0, out phase2Bolts, out phase2Aimed, out phase2Ms);
                    if (easyBolts != phase2Bolts || easyAimed != phase2Aimed || easyMs != phase2Ms)
                    {
                        return false;
                    }
                }

                int worldBolts;
                int ignoredAimed;
                int worldMs;
                Cycle(layout, 2, 1, out worldBolts, out ignoredAimed, out worldMs);
                Cycle(1, 2, 1, out bolts, out aimed, out ms);
                if (layout != 7 && layout != 6 && worldBolts != bolts)
                {
                    return false;
                }

                if (layout == 6 && worldMs <= ms)
                {
                    return false;
                }
            }

            if (GapCount(7, 2, 1) != 2 || GapCount(7, 1, 1) != 1 || GapCount(1, 2, 1) != 1)
            {
                return false;
            }

            if (!ArmsGapMine(4, 1) || ArmsGapMine(4, 0) || ArmsGapMine(1, 2))
            {
                return false;
            }

            return Phase3SpeedMul > 1.09f && Phase3SpeedMul < 1.11f && RotateRadians > 0.39f && RotateRadians < 0.40f;
        }
    }
}
