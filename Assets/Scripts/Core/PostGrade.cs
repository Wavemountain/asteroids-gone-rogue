namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Built-in post values from Docs/0.48/01_post_stack.md. Unity-free.
    /// Grade and the Neutral tonemap are one step: a row never tonemaps alone.
    /// World 8 and later use the same fold as <see cref="WorldCatalog.LayoutNumber"/>.
    /// Hangar is world 0. The daily roll is not read here.
    /// </summary>
    public struct PostGradeRow
    {
        public int World;
        public float Bloom;
        public float Diffusion;
        public float Exposure;
        public float Temperature;
        public float Tint;
        public float Saturation;
        public float Contrast;
        public float LiftR;
        public float LiftG;
        public float LiftB;
        public float LiftW;
        public float GammaR;
        public float GammaG;
        public float GammaB;
        public float GammaW;
        public float GainR;
        public float GainG;
        public float GainB;
        public float GainW;
        public float Vignette;
        public float VignetteSmooth;
        public float Grain;
        public float Threshold;
        public float SoftKnee;
        public float ClampMax;
        public int Iterations;
        public float Scatter;
        public int Reduce;
        public int Tonemap;
    }

    public struct PostBlendState
    {
        public int FromWorld;
        public int ToWorld;
        public float Elapsed;
        public int Snap;
        public float DeathElapsed;
    }

    public static class PostGrade
    {
        public const int Hangar = 0;
        public const int GradeWithTonemap = 1;
        public const float BlendSeconds = 1.2f;
        public const float DeathVignette = 0.45f;
        public const float DeathVignetteSeconds = 0.40f;
        public const float DeathVignetteSmooth = 0.82f;
        public const float BloomThresholdFull = 1.10f;
        public const float SoftKneeFull = 0.35f;
        public const float ClampFull = 8f;
        public const float ScatterFull = 0.65f;
        public const float ScatterReduced = 0.5f;
        public const int IterationsFull = 5;
        public const int IterationsReduced = 4;
        public const int DeckMemoryMb = 1024;
        public const float NeutralWhite = 5.3f;

        public const float HangarBloom = 0.55f;
        public const float HangarDiffusion = 6f;
        public const float HangarExposure = 0.08f;
        public const float HangarTemperature = 4f;
        public const float HangarTint = 2f;
        public const float HangarSaturation = -8f;
        public const float HangarContrast = 6f;
        public const float HangarVignette = 0.32f;
        public const float HangarVignetteSmooth = 0.40f;
        public const float HangarGrain = 0.08f;

        public const float VignetteVoidR = 7f / 255f;
        public const float VignetteVoidG = 11f / 255f;
        public const float VignetteVoidB = 18f / 255f;
        public const int BloomChainSlots = 6;
        public const float MidBodyFloor = 2.5f;
        public const float AsteroidBodyFloor = 1.5f;

        public static int Fold(int world)
        {
            if (world < 1)
            {
                return Hangar;
            }

            return WorldCatalog.LayoutNumber(world);
        }

        public static PostGradeRow Row(int world, bool reduce)
        {
            int layout = Fold(world);
            PostGradeRow row = layout == Hangar ? HangarRow() : WorldRow(layout);
            row.Reduce = reduce ? 1 : 0;
            row.Tonemap = GradeWithTonemap;
            row.Threshold = BloomThresholdFull;
            row.SoftKnee = SoftKneeFull;
            row.ClampMax = ClampFull;
            row.Scatter = ScatterFull;
            row.Iterations = IterationsFull;
            if (!reduce)
            {
                return row;
            }

            row.Bloom *= EffectScale.PostBloomMul;
            row.Threshold = BloomThresholdFull + EffectScale.PostThresholdAdd;
            row.Vignette *= EffectScale.PostVignetteMul;
            row.Grain *= EffectScale.PostGrainMul;
            row.Scatter = ScatterReduced;
            row.Iterations = IterationsReduced;
            return row;
        }

        public static int Iterations(bool reduce, int graphicsMemoryMb)
        {
            if (reduce)
            {
                return IterationsReduced;
            }

            if (graphicsMemoryMb > 0 && graphicsMemoryMb <= DeckMemoryMb)
            {
                return IterationsReduced;
            }

            return IterationsFull;
        }

        public static PostBlendState SnapTo(int world)
        {
            PostBlendState state = new PostBlendState();
            state.FromWorld = Fold(world);
            state.ToWorld = state.FromWorld;
            state.Elapsed = BlendSeconds;
            state.Snap = 1;
            state.DeathElapsed = -1f;
            return state;
        }

        public static PostBlendState Retarget(PostBlendState state, int world)
        {
            int next = Fold(world);
            if (next == state.ToWorld)
            {
                return state;
            }

            float fromWeight;
            float toWeight;
            Weights(state.Elapsed, out fromWeight, out toWeight);
            int dominant = state.FromWorld;
            if (toWeight >= fromWeight)
            {
                dominant = state.ToWorld;
            }

            PostBlendState blended = new PostBlendState();
            blended.FromWorld = dominant;
            blended.ToWorld = next;
            blended.Elapsed = 0f;
            blended.Snap = 0;
            blended.DeathElapsed = state.DeathElapsed;
            return blended;
        }

        public static PostBlendState BeginDeath(PostBlendState state)
        {
            if (state.DeathElapsed < 0f)
            {
                state.DeathElapsed = 0f;
            }

            return state;
        }

        public static PostBlendState AdvanceClock(PostBlendState state, float unscaledDt)
        {
            if (unscaledDt < 0f)
            {
                unscaledDt = 0f;
            }

            if (state.DeathElapsed >= 0f)
            {
                state.DeathElapsed += unscaledDt;
            }

            if (state.Snap != 0 && state.FromWorld == state.ToWorld)
            {
                return state;
            }

            state.Elapsed += unscaledDt;
            if (state.Elapsed >= BlendSeconds)
            {
                state.Elapsed = BlendSeconds;
                state.FromWorld = state.ToWorld;
                state.Snap = 1;
            }
            else
            {
                state.Snap = 0;
            }

            return state;
        }

        public static void Weights(float elapsed, out float fromWeight, out float toWeight)
        {
            float span = BlendSeconds;
            if (span < 0.0001f)
            {
                span = 0.0001f;
            }

            float t = elapsed / span;
            if (t < 0f)
            {
                t = 0f;
            }

            if (t > 1f)
            {
                t = 1f;
            }

            float shaped = t * t * (3f - (2f * t));
            fromWeight = 1f - shaped;
            toWeight = shaped;
        }

        public static PostGradeRow Lerp(PostGradeRow from, PostGradeRow to, float toWeight)
        {
            float t = toWeight;
            if (t < 0f)
            {
                t = 0f;
            }

            if (t > 1f)
            {
                t = 1f;
            }

            PostGradeRow row = to;
            row.Bloom = Mix(from.Bloom, to.Bloom, t);
            row.Diffusion = Mix(from.Diffusion, to.Diffusion, t);
            row.Exposure = Mix(from.Exposure, to.Exposure, t);
            row.Temperature = Mix(from.Temperature, to.Temperature, t);
            row.Tint = Mix(from.Tint, to.Tint, t);
            row.Saturation = Mix(from.Saturation, to.Saturation, t);
            row.Contrast = Mix(from.Contrast, to.Contrast, t);
            row.LiftR = Mix(from.LiftR, to.LiftR, t);
            row.LiftG = Mix(from.LiftG, to.LiftG, t);
            row.LiftB = Mix(from.LiftB, to.LiftB, t);
            row.LiftW = Mix(from.LiftW, to.LiftW, t);
            row.GammaR = Mix(from.GammaR, to.GammaR, t);
            row.GammaG = Mix(from.GammaG, to.GammaG, t);
            row.GammaB = Mix(from.GammaB, to.GammaB, t);
            row.GammaW = Mix(from.GammaW, to.GammaW, t);
            row.GainR = Mix(from.GainR, to.GainR, t);
            row.GainG = Mix(from.GainG, to.GainG, t);
            row.GainB = Mix(from.GainB, to.GainB, t);
            row.GainW = Mix(from.GainW, to.GainW, t);
            row.Vignette = Mix(from.Vignette, to.Vignette, t);
            row.VignetteSmooth = Mix(from.VignetteSmooth, to.VignetteSmooth, t);
            row.Grain = Mix(from.Grain, to.Grain, t);
            row.Threshold = Mix(from.Threshold, to.Threshold, t);
            row.Scatter = Mix(from.Scatter, to.Scatter, t);
            row.Tonemap = GradeWithTonemap;
            if (t < 0.5f)
            {
                row.Iterations = from.Iterations;
                row.World = from.World;
            }

            return row;
        }

        public static void Sample(PostBlendState state, bool reduce, int graphicsMemoryMb, out PostGradeRow row, out float vignette)
        {
            PostGradeRow from = Row(state.FromWorld, reduce);
            PostGradeRow to = Row(state.ToWorld, reduce);
            float fromWeight;
            float toWeight;
            Weights(state.Elapsed, out fromWeight, out toWeight);
            row = Lerp(from, to, toWeight);
            row.Iterations = Iterations(reduce, graphicsMemoryMb);
            float deathU = -1f;
            if (state.DeathElapsed >= 0f)
            {
                float span = DeathVignetteSeconds;
                if (span < 0.0001f)
                {
                    span = 0.0001f;
                }

                deathU = state.DeathElapsed / span;
            }

            vignette = VignetteNow(row.Vignette, deathU, reduce);
            row.Vignette = vignette;
            row.VignetteSmooth = SmoothNow(row.VignetteSmooth, deathU);
        }

        /// <summary>
        /// Ease-out so the death still, pinned at half the slow-mo window, is already dark.
        /// Smoothstep reached only ~0.26 of the ramp at that same moment.
        /// </summary>
        public static float DeathEase(float deathU)
        {
            if (deathU < 0f)
            {
                return 0f;
            }

            float u = deathU;
            if (u > 1f)
            {
                u = 1f;
            }

            float remain = 1f - u;
            return 1f - (remain * remain);
        }

        public static float VignetteNow(float graded, float deathU, bool reduce)
        {
            float shaped = DeathEase(deathU);
            if (shaped <= 0f)
            {
                return graded;
            }

            float target = DeathVignette;
            if (reduce)
            {
                target *= EffectScale.PostVignetteMul;
            }

            return graded + ((target - graded) * shaped);
        }

        public static float SmoothNow(float graded, float deathU)
        {
            float shaped = DeathEase(deathU);
            if (shaped <= 0f)
            {
                return graded;
            }

            return graded + ((DeathVignetteSmooth - graded) * shaped);
        }

        public static void WhiteBalance(float temperature, float tint, out float red, out float green, out float blue)
        {
            float t1 = temperature / 60f;
            float t2 = tint / 60f;
            float x = 0.31271f - (t1 * (t1 < 0f ? 0.1f : 0.05f));
            float y = (2.87f * x) - (3f * x * x) - 0.27509507f + (t2 * 0.05f);
            if (y < 0.0001f)
            {
                y = 0.0001f;
            }

            float cieY = 1f;
            float cieX = cieY * x / y;
            float cieZ = cieY * (1f - x - y) / y;
            float l = (0.7328f * cieX) + (0.4296f * cieY) - (0.1624f * cieZ);
            float m = (-0.7036f * cieX) + (1.6975f * cieY) + (0.0061f * cieZ);
            float s = (0.0030f * cieX) + (0.0136f * cieY) + (0.9834f * cieZ);
            red = l != 0f ? 0.949237f / l : 1f;
            green = m != 0f ? 1.03542f / m : 1f;
            blue = s != 0f ? 1.08728f / s : 1f;
        }

        public static void LiftGain(PostGradeRow row, out float liftR, out float liftG, out float liftB, out float gainR, out float gainG, out float gainB)
        {
            liftR = row.LiftW + (row.LiftR - 1f);
            liftG = row.LiftW + (row.LiftG - 1f);
            liftB = row.LiftW + (row.LiftB - 1f);
            gainR = row.GainR + row.GainW;
            gainG = row.GainG + row.GainW;
            gainB = row.GainB + row.GainW;
        }

        public static float Neutral(float x)
        {
            float white = Hable(NeutralWhite);
            if (white == 0f)
            {
                return 0f;
            }

            return Hable(x) / white;
        }

        public static float GradeLuma(float linear, float ev, float sat, float con, float liftW, float gainW)
        {
            float l = linear * (float)System.Math.Pow(2.0, ev);
            float y = l;
            l = y + ((l - y) * (1f + (sat / 100f)));
            float safe = l;
            if (safe < 0.00001f)
            {
                safe = 0.00001f;
            }

            float lg = (float)System.Math.Log(safe / 0.18, 2.0) * (1f + (con / 100f));
            l = 0.18f * (float)System.Math.Pow(2.0, lg);
            l = Neutral(l);
            l = (l * (1f + gainW)) + (liftW * (1f - l));
            return l;
        }

        public static bool TargetsNeedRebuild(int haveWidth, int haveHeight, int haveFormat, int haveSlots, int width, int height, int format)
        {
            if (haveSlots != BloomChainSlots || haveWidth < 2 || haveHeight < 2)
            {
                return true;
            }

            if (haveWidth != width || haveHeight != height || haveFormat != format)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Steam Deck only when the picture is 1280×800 and the device string
        /// (or a low-memory AMD APU name) agrees. Anything else, including a
        /// null name, stays on the normal iteration count.
        /// </summary>
        public static bool LooksLikeDeck(int width, int height, string deviceName, string deviceModel, int graphicsMemoryMb)
        {
            if (width != 1280 || height != 800)
            {
                return false;
            }

            if (HasDeckToken(deviceName) || HasDeckToken(deviceModel))
            {
                return true;
            }

            if (graphicsMemoryMb > 0 && graphicsMemoryMb <= DeckMemoryMb && HasAmdApu(deviceName, deviceModel))
            {
                return true;
            }

            return false;
        }

        public static float SrgbChannelToLinear(float channel)
        {
            if (channel <= 0f)
            {
                return 0f;
            }

            if (channel <= 0.04045f)
            {
                return channel / 12.92f;
            }

            return (float)System.Math.Pow((channel + 0.055) / 1.055, 2.4);
        }

        public static void VignetteRgb(bool linear, out float red, out float green, out float blue)
        {
            red = VignetteVoidR;
            green = VignetteVoidG;
            blue = VignetteVoidB;
            if (!linear)
            {
                return;
            }

            red = SrgbChannelToLinear(red);
            green = SrgbChannelToLinear(green);
            blue = SrgbChannelToLinear(blue);
        }

        public static bool BodyFloorsHold()
        {
            int world = 1;
            float worstMid = 99f;
            float worstRock = 99f;
            while (world <= 7)
            {
                PostGradeRow row = Row(world, false);
                float midR;
                float midG;
                float midB;
                float rockR;
                float rockG;
                float rockB;
                float floorR;
                float floorG;
                float floorB;
                BodySample(world, out midR, out midG, out midB, out rockR, out rockG, out rockB, out floorR, out floorG, out floorB);
                float gradedMid = Contrast(
                    GradeDisplayed(midR, midG, midB, row),
                    GradeDisplayed(floorR, floorG, floorB, row));
                float gradedRock = Contrast(
                    GradeDisplayed(rockR, rockG, rockB, row),
                    GradeDisplayed(floorR, floorG, floorB, row));
                if (gradedMid < worstMid)
                {
                    worstMid = gradedMid;
                }

                if (gradedRock < worstRock)
                {
                    worstRock = gradedRock;
                }

                world++;
            }

            return worstMid >= MidBodyFloor && worstRock >= AsteroidBodyFloor;
        }

        public static bool SelfCheck()
        {
            if (EffectScale.PostBloomMul != 0.5f || EffectScale.PostVignetteMul != 0.5f || EffectScale.PostGrainMul != 0f)
            {
                return false;
            }

            if (EffectScale.PostThresholdAdd != 0.15f)
            {
                return false;
            }

            if (GradeWithTonemap != 1)
            {
                return false;
            }

            PostGradeRow full = Row(1, false);
            PostGradeRow half = Row(1, true);
            if (!Near(full.Bloom, 0.60f) || !Near(half.Bloom, 0.30f))
            {
                return false;
            }

            if (!Near(full.Threshold, 1.10f) || !Near(half.Threshold, 1.25f))
            {
                return false;
            }

            if (!Near(full.Vignette, 0.28f) || !Near(half.Vignette, 0.14f))
            {
                return false;
            }

            if (full.Grain == 0f || half.Grain != 0f)
            {
                return false;
            }

            if (full.Saturation != half.Saturation || full.Contrast != half.Contrast || full.Exposure != half.Exposure)
            {
                return false;
            }

            if (full.Tonemap != 1 || half.Tonemap != 1)
            {
                return false;
            }

            if (!Near(Row(2, false).Bloom, 0.65f) || !Near(Row(3, false).Bloom, 0.60f) || !Near(Row(4, false).Bloom, 0.60f))
            {
                return false;
            }

            if (!Near(Row(5, false).Bloom, 0.65f) || !Near(Row(6, false).Bloom, 0.60f) || !Near(Row(7, false).Bloom, 0.65f))
            {
                return false;
            }

            if (!Near(Row(3, false).Saturation, -20f) || !Near(Row(4, false).Contrast, 10f) || !Near(Row(4, false).Temperature, -6f))
            {
                return false;
            }

            if (!Near(Row(6, false).Vignette, 0.24f) || !Near(Row(5, false).Exposure, 0.20f))
            {
                return false;
            }

            PostGradeRow hangar = Row(Hangar, false);
            PostGradeRow hangarHalf = Row(Hangar, true);
            if (!Near(hangar.Bloom, HangarBloom) || !Near(hangar.Saturation, HangarSaturation) || !Near(hangar.Contrast, HangarContrast))
            {
                return false;
            }

            if (!Near(hangar.Temperature, HangarTemperature) || !Near(hangar.Vignette, HangarVignette) || !Near(hangar.Grain, HangarGrain))
            {
                return false;
            }

            if (!Near(hangarHalf.Bloom, HangarBloom * 0.5f) || hangarHalf.Grain != 0f || hangarHalf.Saturation != hangar.Saturation)
            {
                return false;
            }

            if (Fold(8) != 1 || !Near(Row(8, false).Bloom, full.Bloom) || Row(8, false).Saturation != full.Saturation)
            {
                return false;
            }

            if (Iterations(false, 2048) != 5 || Iterations(false, 1024) != 4 || Iterations(true, 4096) != 4)
            {
                return false;
            }

            if (!Near(Neutral(NeutralWhite), 1f) || !Near(Neutral(0.18f), 0.14134f))
            {
                return false;
            }

            if (!Near(GradeLuma(0.4f, 0.10f, -12f, 8f, -0.010f, 0.02f), 0.31895f))
            {
                return false;
            }

            float balR;
            float balG;
            float balB;
            WhiteBalance(-4f, 0f, out balR, out balG, out balB);
            if (balB < 1.04f || balR > 0.995f)
            {
                return false;
            }

            PostBlendState state = SnapTo(1);
            state = Retarget(state, 2);
            state = AdvanceClock(state, 0.9f);
            float fromW;
            float toW;
            Weights(state.Elapsed, out fromW, out toW);
            if (toW < fromW)
            {
                return false;
            }

            state = Retarget(state, 3);
            if (state.FromWorld != 2 || state.ToWorld != 3 || state.Elapsed != 0f)
            {
                return false;
            }

            PostGradeRow deathRow;
            float deathVignette;
            PostBlendState dying = BeginDeath(SnapTo(1));
            dying = AdvanceClock(dying, DeathVignetteSeconds);
            Sample(dying, false, 2048, out deathRow, out deathVignette);
            if (!Near(deathVignette, DeathVignette) || deathRow.Tonemap != 1)
            {
                return false;
            }

            PostGradeRow reducedDeath;
            float reducedVignette;
            Sample(dying, true, 2048, out reducedDeath, out reducedVignette);
            if (!Near(reducedVignette, DeathVignette * EffectScale.PostVignetteMul))
            {
                return false;
            }

            if (reducedDeath.Exposure != deathRow.Exposure)
            {
                return false;
            }

            if (!TargetsNeedRebuild(0, 0, 0, 0, 1280, 800, 2))
            {
                return false;
            }

            if (TargetsNeedRebuild(1280, 800, 2, BloomChainSlots, 1280, 800, 2))
            {
                return false;
            }

            if (!TargetsNeedRebuild(1280, 800, 2, BloomChainSlots, 1920, 1080, 2))
            {
                return false;
            }

            if (!TargetsNeedRebuild(1280, 800, 2, BloomChainSlots, 1280, 800, 3))
            {
                return false;
            }

            if (LooksLikeDeck(1920, 1080, "Steam Deck", "Jupiter", 1024))
            {
                return false;
            }

            if (!LooksLikeDeck(1280, 800, "AMD Custom GPU 0405", "Jupiter", 1024))
            {
                return false;
            }

            if (!LooksLikeDeck(1280, 800, "Steam Deck", null, 0))
            {
                return false;
            }

            if (LooksLikeDeck(1280, 800, "NVIDIA GeForce", "PC", 8192))
            {
                return false;
            }

            if (LooksLikeDeck(1280, 800, null, null, 0) || LooksLikeDeck(1280, 800, "", "", 512))
            {
                return false;
            }

            float gammaR;
            float gammaG;
            float gammaB;
            float linearR;
            float linearG;
            float linearB;
            VignetteRgb(false, out gammaR, out gammaG, out gammaB);
            VignetteRgb(true, out linearR, out linearG, out linearB);
            if (gammaR != VignetteVoidR || linearR >= gammaR || linearG >= gammaG || linearB >= gammaB)
            {
                return false;
            }

            if (!BodyFloorsHold())
            {
                return false;
            }

            if (DeathVignetteSeconds > 0.41f || DeathVignetteSeconds < 0.39f)
            {
                return false;
            }

            PostGradeRow midRow;
            float midVignette;
            PostBlendState midDeath = BeginDeath(SnapTo(1));
            midDeath = AdvanceClock(midDeath, DeathVignetteSeconds * 0.5f);
            Sample(midDeath, false, 2048, out midRow, out midVignette);
            if (midVignette < 0.39f || midRow.VignetteSmooth < 0.68f)
            {
                return false;
            }

            return true;
        }

        private static bool HasDeckToken(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (value.IndexOf("steam deck", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (value.IndexOf("steamdeck", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (value.IndexOf("jupiter", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (value.IndexOf("0405", System.StringComparison.OrdinalIgnoreCase) >= 0
                && value.IndexOf("amd", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }

        private static bool HasAmdApu(string deviceName, string deviceModel)
        {
            return HasAmd(deviceName) || HasAmd(deviceModel);
        }

        private static bool HasAmd(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (value.IndexOf("amd", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (value.IndexOf("radv", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }

        private static void BodySample(
            int world,
            out float midR,
            out float midG,
            out float midB,
            out float rockR,
            out float rockG,
            out float rockB,
            out float floorR,
            out float floorG,
            out float floorB)
        {
            // Lit-floor and body sRGB at the W3/W4 brightness fix (760/680) plus the watch boost.
            if (world == 3)
            {
                midR = 1f;
                midG = 0.27545f;
                midB = 0.32153f;
                rockR = 0.41088f;
                rockG = 0.36633f;
                rockB = 0.34776f;
                floorR = 0.01043f;
                floorG = 0.19531f;
                floorB = 0.15188f;
                return;
            }

            if (world == 4)
            {
                midR = 1f;
                midG = 0.27545f;
                midB = 0.32153f;
                rockR = 0.41088f;
                rockG = 0.36633f;
                rockB = 0.34776f;
                floorR = 0.23720f;
                floorG = 0.12576f;
                floorB = 0.01143f;
                return;
            }

            midR = 1f;
            midG = 0.23240f;
            midB = 0.26876f;
            rockR = 0.35706f;
            rockG = 0.32119f;
            rockB = 0.30727f;
            if (world == 2)
            {
                floorR = 0.13932f;
                floorG = 0.02550f;
                floorB = 0.26910f;
                return;
            }

            if (world == 5)
            {
                floorR = 0.04265f;
                floorG = 0.05890f;
                floorB = 0.16159f;
                return;
            }

            if (world == 6)
            {
                floorR = 0.15220f;
                floorG = 0.02446f;
                floorB = 0.00925f;
                return;
            }

            if (world == 7)
            {
                floorR = 0.28612f;
                floorG = 0.02669f;
                floorB = 0.18065f;
                return;
            }

            floorR = 0.02747f;
            floorG = 0.11726f;
            floorB = 0.38963f;
        }

        private static float Contrast(float leftLuma, float rightLuma)
        {
            float hi = leftLuma;
            float lo = rightLuma;
            if (rightLuma > leftLuma)
            {
                hi = rightLuma;
                lo = leftLuma;
            }

            return (hi + 0.05f) / (lo + 0.05f);
        }

        private static float GradeDisplayed(float red, float green, float blue, PostGradeRow row)
        {
            float r = SrgbChannelToLinear(red);
            float g = SrgbChannelToLinear(green);
            float b = SrgbChannelToLinear(blue);
            float ev = (float)System.Math.Pow(2.0, row.Exposure);
            r *= ev;
            g *= ev;
            b *= ev;
            float y = (0.2126f * r) + (0.7152f * g) + (0.0722f * b);
            float sat = 1f + (row.Saturation / 100f);
            r = ToneChannel(y + ((r - y) * sat), row);
            g = ToneChannel(y + ((g - y) * sat), row);
            b = ToneChannel(y + ((b - y) * sat), row);
            return (0.2126f * r) + (0.7152f * g) + (0.0722f * b);
        }

        private static float ToneChannel(float linear, PostGradeRow row)
        {
            float safe = linear;
            if (safe < 0.00001f)
            {
                safe = 0.00001f;
            }

            float lg = (float)System.Math.Log(safe / 0.18, 2.0) * (1f + (row.Contrast / 100f));
            float v = 0.18f * (float)System.Math.Pow(2.0, lg);
            v = Neutral(v);
            v = (v * (1f + row.GainW)) + (row.LiftW * (1f - v));
            if (v < 0f)
            {
                return 0f;
            }

            return v;
        }

        private static float Hable(float v)
        {
            const float a = 0.2f;
            const float b = 0.29f;
            const float c = 0.24f;
            const float d = 0.272f;
            const float e = 0.02f;
            const float f = 0.3f;
            float num = (v * ((a * v) + (c * b))) + (d * e);
            float den = (v * ((a * v) + b)) + (d * f);
            if (den == 0f)
            {
                return 0f;
            }

            return (num / den) - (e / f);
        }

        private static float Mix(float a, float b, float t)
        {
            return a + ((b - a) * t);
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

        private static PostGradeRow HangarRow()
        {
            PostGradeRow row = new PostGradeRow();
            row.World = Hangar;
            row.Bloom = HangarBloom;
            row.Diffusion = HangarDiffusion;
            row.Exposure = HangarExposure;
            row.Temperature = HangarTemperature;
            row.Tint = HangarTint;
            row.Saturation = HangarSaturation;
            row.Contrast = HangarContrast;
            row.LiftR = 1.01f;
            row.LiftG = 0.99f;
            row.LiftB = 0.97f;
            row.LiftW = -0.005f;
            row.GammaR = 1f;
            row.GammaG = 1f;
            row.GammaB = 1f;
            row.GammaW = 0f;
            row.GainR = 1.03f;
            row.GainG = 1.01f;
            row.GainB = 0.98f;
            row.GainW = 0.01f;
            row.Vignette = HangarVignette;
            row.VignetteSmooth = HangarVignetteSmooth;
            row.Grain = HangarGrain;
            return row;
        }

        private static PostGradeRow WorldRow(int layout)
        {
            PostGradeRow row = new PostGradeRow();
            row.World = layout;
            row.GammaR = 1f;
            row.GammaG = 1f;
            row.GammaB = 1f;
            row.GammaW = 0f;
            switch (layout)
            {
                case 2:
                    row.Bloom = 0.65f;
                    row.Diffusion = 6f;
                    row.Exposure = 0.15f;
                    row.Temperature = -2f;
                    row.Tint = 3f;
                    row.Saturation = -18f;
                    row.Contrast = 8f;
                    row.LiftR = 1.00f;
                    row.LiftG = 0.98f;
                    row.LiftB = 1.03f;
                    row.LiftW = -0.010f;
                    row.GainR = 1.03f;
                    row.GainG = 1.00f;
                    row.GainB = 0.96f;
                    row.GainW = 0.02f;
                    row.Vignette = 0.30f;
                    row.VignetteSmooth = 0.40f;
                    row.Grain = 0.12f;
                    break;
                case 3:
                    row.Bloom = 0.60f;
                    row.Diffusion = 5.5f;
                    row.Exposure = 0.05f;
                    row.Temperature = 0f;
                    row.Tint = -2f;
                    row.Saturation = -20f;
                    row.Contrast = 10f;
                    row.LiftR = 0.98f;
                    row.LiftG = 1.01f;
                    row.LiftB = 1.02f;
                    row.LiftW = -0.015f;
                    row.GainR = 1.02f;
                    row.GainG = 1.00f;
                    row.GainB = 0.97f;
                    row.GainW = 0f;
                    row.Vignette = 0.26f;
                    row.VignetteSmooth = 0.38f;
                    row.Grain = 0.10f;
                    break;
                case 4:
                    row.Bloom = 0.60f;
                    row.Diffusion = 5.5f;
                    row.Exposure = 0f;
                    row.Temperature = -6f;
                    row.Tint = 0f;
                    row.Saturation = -25f;
                    row.Contrast = 10f;
                    row.LiftR = 0.97f;
                    row.LiftG = 1.00f;
                    row.LiftB = 1.04f;
                    row.LiftW = -0.015f;
                    row.GainR = 1.00f;
                    row.GainG = 1.00f;
                    row.GainB = 1.00f;
                    row.GainW = 0f;
                    row.Vignette = 0.26f;
                    row.VignetteSmooth = 0.38f;
                    row.Grain = 0.10f;
                    break;
                case 5:
                    row.Bloom = 0.65f;
                    row.Diffusion = 6f;
                    row.Exposure = 0.20f;
                    row.Temperature = -3f;
                    row.Tint = 0f;
                    row.Saturation = -10f;
                    row.Contrast = 6f;
                    row.LiftR = 0.98f;
                    row.LiftG = 1.00f;
                    row.LiftB = 1.03f;
                    row.LiftW = -0.005f;
                    row.GainR = 1.03f;
                    row.GainG = 1.01f;
                    row.GainB = 0.97f;
                    row.GainW = 0.03f;
                    row.Vignette = 0.30f;
                    row.VignetteSmooth = 0.42f;
                    row.Grain = 0.14f;
                    break;
                case 6:
                    row.Bloom = 0.60f;
                    row.Diffusion = 6f;
                    row.Exposure = 0.15f;
                    row.Temperature = -8f;
                    row.Tint = 0f;
                    row.Saturation = -25f;
                    row.Contrast = 8f;
                    row.LiftR = 0.97f;
                    row.LiftG = 1.00f;
                    row.LiftB = 1.04f;
                    row.LiftW = -0.010f;
                    row.GainR = 1.02f;
                    row.GainG = 1.00f;
                    row.GainB = 0.98f;
                    row.GainW = 0.02f;
                    row.Vignette = 0.24f;
                    row.VignetteSmooth = 0.38f;
                    row.Grain = 0.12f;
                    break;
                case 7:
                    row.Bloom = 0.65f;
                    row.Diffusion = 6f;
                    row.Exposure = 0.10f;
                    row.Temperature = -2f;
                    row.Tint = -3f;
                    row.Saturation = -20f;
                    row.Contrast = 8f;
                    row.LiftR = 0.99f;
                    row.LiftG = 1.00f;
                    row.LiftB = 1.03f;
                    row.LiftW = -0.010f;
                    row.GainR = 1.03f;
                    row.GainG = 1.00f;
                    row.GainB = 0.97f;
                    row.GainW = 0.02f;
                    row.Vignette = 0.30f;
                    row.VignetteSmooth = 0.40f;
                    row.Grain = 0.12f;
                    break;
                default:
                    row.World = 1;
                    row.Bloom = 0.60f;
                    row.Diffusion = 6f;
                    row.Exposure = 0.10f;
                    row.Temperature = -4f;
                    row.Tint = 0f;
                    row.Saturation = -12f;
                    row.Contrast = 8f;
                    row.LiftR = 0.98f;
                    row.LiftG = 1.00f;
                    row.LiftB = 1.03f;
                    row.LiftW = -0.010f;
                    row.GainR = 1.02f;
                    row.GainG = 1.00f;
                    row.GainB = 0.97f;
                    row.GainW = 0.02f;
                    row.Vignette = 0.28f;
                    row.VignetteSmooth = 0.40f;
                    row.Grain = 0.12f;
                    break;
            }

            return row;
        }
    }
}
