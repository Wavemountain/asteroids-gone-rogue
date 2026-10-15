namespace AsteroidsGoneRogue
{
    /// <summary>
    /// 0.48 VFX counts and caps (Docs/0.48/03_vfx_language.md).
    /// Mesh pools and particle emits read this. The daily roll is not read here.
    /// Enemy ordinals match <see cref="EnemyKind"/> declaration order.
    /// </summary>
    public static class VfxBudget
    {
        public const int Small = 0;
        public const int Mid = 1;
        public const int Large = 2;

        public const int MaxParticles = 600;
        public const int ChunkParticles = 280;
        public const int SmokeParticles = 180;
        public const int SparkParticles = 140;

        public const int DebrisCap = 24;
        public const int ShockCap = 8;
        public const int MuzzleCap = 12;

        public const int ChunkSmall = 6;
        public const int ChunkMid = 12;
        public const int ChunkLarge = 24;
        public const int SmokeSmall = 2;
        public const int SmokeMid = 4;
        public const int SmokeLarge = 8;

        public const float SmokeSecondsSmall = 0.6f;
        public const float SmokeSecondsMid = 0.9f;
        public const float SmokeSecondsLarge = 1.4f;
        public const float SmokeTimeMul = 0.5f;
        public const float MuzzleAlphaMul = 0.6f;
        public const float MuzzleMaxSeconds = 0.06f;
        public const int ReduceSparkCap = 2;

        public const float HotR = 255f / 255f;
        public const float HotG = 242f / 255f;
        public const float HotB = 214f / 255f;
        public const float EmberR = 224f / 255f;
        public const float EmberG = 122f / 255f;
        public const float EmberB = 58f / 255f;
        public const float SmokeR = 42f / 255f;
        public const float SmokeG = 47f / 255f;
        public const float SmokeB = 56f / 255f;
        public const float SmokeAlpha = 0.55f;

        public const int Billboard = 1;
        public const string RingMaterial = "Mat_FX_Ring";
        public const string Debris01 = "FX_Debris_01";
        public const string Debris02 = "FX_Debris_02";
        public const string Debris03 = "FX_Debris_03";
        public const string Debris04 = "FX_Debris_04";
        public const string ShockMesh = "FX_ShockRing";
        public const string MuzzleMesh = "FX_MuzzleCard";

        public static int ScaledCount(int full, bool reduce)
        {
            int count = full;
            if (count < 0)
            {
                count = 0;
            }

            if (!reduce || count == 0)
            {
                return count;
            }

            double scaled = count * (double)EffectScale.ParticleCountMul;
            int raised = (int)System.Math.Ceiling(scaled);
            if (raised < 1)
            {
                raised = 1;
            }

            return raised;
        }

        public static float ScaledSize(float full, bool reduce)
        {
            if (full < 0f)
            {
                full = 0f;
            }

            if (!reduce)
            {
                return full;
            }

            return full * EffectScale.ParticleSizeMul;
        }

        public static int Chunks(int size, bool reduce)
        {
            int full = ChunkMid;
            if (size == Small)
            {
                full = ChunkSmall;
            }
            else if (size == Large)
            {
                full = ChunkLarge;
            }

            return ScaledCount(full, reduce);
        }

        public static int Smokes(int size, bool reduce)
        {
            int full = SmokeMid;
            if (size == Small)
            {
                full = SmokeSmall;
            }
            else if (size == Large)
            {
                full = SmokeLarge;
            }

            return ScaledCount(full, reduce);
        }

        public static float SmokeSeconds(int size, bool reduce)
        {
            float seconds = SmokeSecondsMid;
            if (size == Small)
            {
                seconds = SmokeSecondsSmall;
            }
            else if (size == Large)
            {
                seconds = SmokeSecondsLarge;
            }

            if (reduce)
            {
                return seconds * SmokeTimeMul;
            }

            return seconds;
        }

        public static float ShockScale(float t, float start, float end)
        {
            if (t < 0f)
            {
                t = 0f;
            }

            if (t > 1f)
            {
                t = 1f;
            }

            return start + ((end - start) * t);
        }

        public static int SizeForEnemy(int kind, int boss)
        {
            if (boss != 0)
            {
                return Large;
            }

            if (kind == 7)
            {
                return Large;
            }

            if (kind == 1 || kind == 6 || kind == 8 || kind == 9)
            {
                return Small;
            }

            return Mid;
        }

        public static float MuzzleSeconds(int mode)
        {
            if (mode == 1 || mode == 2)
            {
                return 0.05f;
            }

            if (mode == 4 || mode == 6)
            {
                return 0.06f;
            }

            return 0.04f;
        }

        public static float MuzzleScale(int mode)
        {
            if (mode == 6)
            {
                return 0.6f;
            }

            if (mode == 2)
            {
                return 0.45f;
            }

            if (mode == 1)
            {
                return 0.35f;
            }

            if (mode == 4)
            {
                return 0.30f;
            }

            if (mode == 3)
            {
                return 0.22f;
            }

            return 0.25f;
        }

        public static int MuzzleCards(int mode)
        {
            if (mode == 3)
            {
                return 2;
            }

            return 1;
        }

        public static int MuzzleSparks(int mode, bool reduce)
        {
            int full = 0;
            if (mode == 2 || mode == 5)
            {
                full = 2;
            }
            else if (mode == 6)
            {
                full = 6;
            }

            int count = ScaledCount(full, reduce);
            if (reduce && count > ReduceSparkCap)
            {
                count = ReduceSparkCap;
            }

            return count;
        }

        public static string DebrisName(int index)
        {
            int slot = index % 4;
            if (slot < 0)
            {
                slot += 4;
            }

            if (slot == 1)
            {
                return Debris02;
            }

            if (slot == 2)
            {
                return Debris03;
            }

            if (slot == 3)
            {
                return Debris04;
            }

            return Debris01;
        }

        public static bool SelfCheck()
        {
            if (Bad(ChunkParticles + SmokeParticles + SparkParticles != MaxParticles))
            {
                return false;
            }

            if (Bad(DebrisCap < ChunkLarge || ShockCap < 1 || MuzzleCap < 2))
            {
                return false;
            }

            if (ScaledCount(6, false) != 6 || ScaledCount(6, true) != 3 || ScaledCount(1, true) != 1)
            {
                return false;
            }

            if (Chunks(Small, false) != 6 || Chunks(Large, false) != 24 || Chunks(Large, true) != 10)
            {
                return false;
            }

            if (Smokes(Mid, false) != 4 || !Near(SmokeSeconds(Large, true), 0.7f))
            {
                return false;
            }

            if (ShockScale(0f, 0.4f, 2.4f) != 0.4f || ShockScale(1f, 0.4f, 2.4f) != 2.4f)
            {
                return false;
            }

            if (SizeForEnemy(8, 0) != Small || SizeForEnemy(1, 0) != Small || SizeForEnemy(0, 0) != Mid)
            {
                return false;
            }

            if (SizeForEnemy(7, 0) != Large || SizeForEnemy(0, 1) != Large)
            {
                return false;
            }

            if (MuzzleSeconds(0) > MuzzleMaxSeconds || MuzzleSeconds(6) > MuzzleMaxSeconds)
            {
                return false;
            }

            if (MuzzleCards(3) != 2 || MuzzleSparks(6, false) != 6 || MuzzleSparks(6, true) != 2)
            {
                return false;
            }

            if (MuzzleSparks(2, true) > ReduceSparkCap || Billboard != 1)
            {
                return false;
            }

            if (DebrisName(0) != Debris01 || DebrisName(3) != Debris04 || DebrisName(4) != Debris01)
            {
                return false;
            }

            if (Bad(RingMaterial != "Mat_FX_Ring" || ShockMesh != "FX_ShockRing" || MuzzleMesh != "FX_MuzzleCard"))
            {
                return false;
            }

            if (Bad(HotG >= HotR || EmberR <= EmberB || SmokeAlpha <= 0f || SmokeAlpha >= 1f))
            {
                return false;
            }

            if (ScaledSize(1f, true) != EffectScale.ParticleSizeMul)
            {
                return false;
            }

            return true;
        }

        private static bool Bad(bool failed)
        {
            return failed;
        }

        private static bool Near(float a, float b)
        {
            float d = a - b;
            if (d < 0f)
            {
                d = -d;
            }

            return d < 0.002f;
        }
    }
}
