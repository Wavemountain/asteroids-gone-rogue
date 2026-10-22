namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Layer gate. Unity-free so a capture run and the self-test share one
    /// decision. A hit is a FAIL string. An empty result is a pass. The
    /// capture logs each hit and still advances to the next scene.
    /// </summary>
    public static class LayerGate
    {
        public const int GeometryMin = 2000;
        public const int GeometryMax = 2449;
        public const int AlphaTestMin = 2450;
        public const int AlphaTestMax = 2499;
        public const int TransparentMin = 2500;
        public const int TransparentMax = 3999;
        public const int OverlayMin = 4000;
        public const int ZTestLess = 2;
        public const int ZTestLEqual = 4;
        public const int ZTestAlways = 8;
        public const float VisibleAlpha = 0.04f;
        public const float DeadEmissionMax = 0.45f;
        public const float DeadAlbedoMax = 0.45f;
        // Darkening shader. A black unlit blend is the shadow, not a dead plate.
        public const string BlobShadowShader = "AGR/BlobShadow";
        // Built-in UI layer. Capture composites canvases with this mask only.
        public const int UiLayer = 5;
        public const int UiCameraMask = 1 << UiLayer;
        // Spawn leftovers sit inside the arena mesh. An island is clutter when
        // its XZ box comes within this of the origin and does not reach past
        // the outer radius (the floor and the W4 lane walls do).
        public const float SpawnClearRadius = 4f;
        public const float SpawnClutterRadius = 8f;
        // W4 corner blocks straddle the lip: inner edge this far inside the
        // arena, outer corner this far outside, and both spans in range.
        public const float RingBlockInner = 3f;
        public const float RingBlockOuter = 4f;
        public const float RingBlockSpanMin = 3f;
        public const float RingBlockSpanMax = 12f;
        // Lip inner edge is ArenaRadius 30 minus the 0.22 m half-width.
        // A spawn disc whose XZ box reaches this paints the ring.
        public const float SpawnDiscRingInner = 29.5f;

        public sealed class Rules
        {
            public string[] Renderers;
            public string[] Materials;
            public string[] HangarRenderers;
            public string[] World1Renderers;
            public string[] World3Renderers;
            public string[] World4Renderers;
            public string[] DeathRenderers;
            public string[] WaveClearRenderers;
            public string[] HangarMaterials;
            public string[] World1Materials;
            public string[] World3Materials;
            public string[] World4Materials;
            public string[] DeathMaterials;
            public string[] WaveClearMaterials;
            public string[] OverlayExemptShaders;
            public string[] DeadNameHints;
            public string[] DeadExempt;
            public int[] CanvasSorts;
        }

        public sealed class Fact
        {
            public string Name;
            public string Material;
            public string Shader;
            public string Role;
            public int Queue;
            public int ZTest;
            public float Alpha;
            public float Emission;
            public float Albedo;
            public bool Unlit;
            public bool Active;
            public bool ContactBlob;
            public bool OnFloor;
        }

        public sealed class CanvasFact
        {
            public string Name;
            public int Sort;
            public int Layer;
            public bool Enabled;
        }

        public sealed class CameraFact
        {
            public string Name;
            public int Mask;
            public bool Enabled;
        }

        public sealed class BodyFact
        {
            public string Name;
            public string Kind;
            public bool Active;
            public bool HasContact;
        }

        public sealed class IslandFact
        {
            public string Name;
            public float MinX;
            public float MaxX;
            public float MinZ;
            public float MaxZ;
        }

        public static string[] Evaluate(
            string world,
            Rules rules,
            Fact[] facts,
            CanvasFact[] canvases,
            string[] uiOverlaps,
            BodyFact[] bodies)
        {
            return Evaluate(world, rules, facts, canvases, uiOverlaps, bodies, null);
        }

        /// <summary>
        /// Parent plate behind a single control. HintScrim is drawn as the
        /// hint's background, so the pair is not two controls competing.
        /// ShipPreviewFrame is not listed: the hangar hint ends at
        /// GameUi.HangarHintMaxY (0.064) and the frame starts at 0.080.
        /// </summary>
        public static bool IsUiBackgroundPair(string pair)
        {
            if (pair == null)
            {
                return false;
            }

            return pair == "Hint x HintScrim" || pair == "HintScrim x Hint";
        }

        public static string[] Evaluate(
            string world,
            Rules rules,
            Fact[] facts,
            CanvasFact[] canvases,
            string[] uiOverlaps,
            BodyFact[] bodies,
            CameraFact[] cameras)
        {
            return Evaluate(world, rules, facts, canvases, uiOverlaps, bodies, cameras, null);
        }

        public static string[] Evaluate(
            string world,
            Rules rules,
            Fact[] facts,
            CanvasFact[] canvases,
            string[] uiOverlaps,
            BodyFact[] bodies,
            CameraFact[] cameras,
            IslandFact[] islands)
        {
            System.Collections.Generic.List<string> hits = new System.Collections.Generic.List<string>();
            if (rules == null)
            {
                hits.Add("whitelist missing");
                return hits.ToArray();
            }

            string scene = world == null ? string.Empty : world;
            string[] rendererPatterns = RendererPatterns(rules, scene);
            string[] materialPatterns = MaterialPatterns(rules, scene);
            if (facts != null)
            {
                for (int i = 0; i < facts.Length; i++)
                {
                    NoteFact(scene, rules, rendererPatterns, materialPatterns, facts[i], hits);
                }
            }

            if (canvases != null)
            {
                for (int i = 0; i < canvases.Length; i++)
                {
                    NoteCanvas(rules, canvases[i], hits);
                }
            }

            if (uiOverlaps != null)
            {
                for (int i = 0; i < uiOverlaps.Length; i++)
                {
                    string pair = uiOverlaps[i];
                    if (pair != null && pair.Length > 0 && !IsUiBackgroundPair(pair))
                    {
                        hits.Add("ui-overlap " + pair);
                    }
                }
            }

            if (bodies != null)
            {
                for (int i = 0; i < bodies.Length; i++)
                {
                    NoteBody(bodies[i], hits);
                }
            }

            if (cameras != null)
            {
                for (int i = 0; i < cameras.Length; i++)
                {
                    NoteUiCamera(cameras[i], hits);
                }
            }

            if (islands != null)
            {
                for (int i = 0; i < islands.Length; i++)
                {
                    NoteIsland(islands[i], hits);
                }
            }

            return hits.ToArray();
        }

        public static bool IsSpawnClutter(float minX, float maxX, float minZ, float maxZ)
        {
            float near = ClosestRadius(minX, maxX, minZ, maxZ);
            float far = FarthestRadius(minX, maxX, minZ, maxZ);
            return near <= SpawnClearRadius && far <= SpawnClutterRadius;
        }

        public static bool IsRingCorner(float minX, float maxX, float minZ, float maxZ, float arenaRadius)
        {
            float spanX = maxX - minX;
            float spanZ = maxZ - minZ;
            if (spanX < RingBlockSpanMin || spanX > RingBlockSpanMax)
            {
                return false;
            }

            if (spanZ < RingBlockSpanMin || spanZ > RingBlockSpanMax)
            {
                return false;
            }

            if (minX < 0f && maxX > 0f)
            {
                return false;
            }

            if (minZ < 0f && maxZ > 0f)
            {
                return false;
            }

            float near = ClosestRadius(minX, maxX, minZ, maxZ);
            float far = FarthestRadius(minX, maxX, minZ, maxZ);
            return near <= arenaRadius - RingBlockInner && far >= arenaRadius + RingBlockOuter;
        }

        public static bool IsRingCorner(float minX, float maxX, float minZ, float maxZ, ArenaShape shape)
        {
            if (shape == null)
            {
                return false;
            }

            float cx = (minX + maxX) * 0.5f;
            float cz = (minZ + maxZ) * 0.5f;
            float angle = (float)System.Math.Atan2(cz, cx);
            return IsRingCorner(minX, maxX, minZ, maxZ, shape.RadiusAt(angle));
        }

        public static float ClosestRadius(float minX, float maxX, float minZ, float maxZ)
        {
            float x = 0f;
            if (minX > 0f)
            {
                x = minX;
            }
            else if (maxX < 0f)
            {
                x = maxX;
            }

            float z = 0f;
            if (minZ > 0f)
            {
                z = minZ;
            }
            else if (maxZ < 0f)
            {
                z = maxZ;
            }

            return (float)System.Math.Sqrt((x * x) + (z * z));
        }

        public static float FarthestRadius(float minX, float maxX, float minZ, float maxZ)
        {
            float far = 0f;
            far = Farther(far, minX, minZ);
            far = Farther(far, minX, maxZ);
            far = Farther(far, maxX, minZ);
            far = Farther(far, maxX, maxZ);
            return far;
        }

        static float Farther(float far, float x, float z)
        {
            float radius = (float)System.Math.Sqrt((x * x) + (z * z));
            if (radius > far)
            {
                return radius;
            }

            return far;
        }

        public static bool TryParse(string json, out Rules rules, out string error)
        {
            rules = new Rules();
            error = string.Empty;
            if (json == null || json.Length == 0)
            {
                error = "whitelist missing";
                rules = null;
                return false;
            }

            if (!ReadStrings(json, "renderers", out rules.Renderers, out error))
            {
                rules = null;
                return false;
            }

            if (!ReadStrings(json, "materials", out rules.Materials, out error))
            {
                rules = null;
                return false;
            }

            if (!ReadStrings(json, "renderers.hangar", out rules.HangarRenderers, out error)
                || !ReadStrings(json, "renderers.1", out rules.World1Renderers, out error)
                || !ReadStrings(json, "renderers.3", out rules.World3Renderers, out error)
                || !ReadStrings(json, "renderers.4", out rules.World4Renderers, out error)
                || !ReadStrings(json, "renderers.death", out rules.DeathRenderers, out error)
                || !ReadStrings(json, "renderers.waveclear", out rules.WaveClearRenderers, out error)
                || !ReadStrings(json, "materials.hangar", out rules.HangarMaterials, out error)
                || !ReadStrings(json, "materials.1", out rules.World1Materials, out error)
                || !ReadStrings(json, "materials.3", out rules.World3Materials, out error)
                || !ReadStrings(json, "materials.4", out rules.World4Materials, out error)
                || !ReadStrings(json, "materials.death", out rules.DeathMaterials, out error)
                || !ReadStrings(json, "materials.waveclear", out rules.WaveClearMaterials, out error)
                || !ReadStrings(json, "overlayExemptShaders", out rules.OverlayExemptShaders, out error)
                || !ReadStrings(json, "deadNameHints", out rules.DeadNameHints, out error)
                || !ReadInts(json, "canvasSorts", out rules.CanvasSorts, out error))
            {
                rules = null;
                return false;
            }

            rules.DeadExempt = new string[0];
            if (json.IndexOf("\"deadExempt\"", System.StringComparison.Ordinal) >= 0
                && !ReadStrings(json, "deadExempt", out rules.DeadExempt, out error))
            {
                rules = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Hull particles are not ship children. A death shot that still has
        /// smoke, chunk, or spark billboards alive fails. Other worlds may
        /// keep them. The chunk ember is the few-pixel speck at the wreck.
        /// </summary>
        public static string DeathPuffHit(string world, string name, int alive, bool enabled)
        {
            if (world != "death" || !enabled || alive <= 0)
            {
                return null;
            }

            if (name == "Smoke" || name == "Chunks" || name == "Sparks")
            {
                return "death-remnant " + name;
            }

            return null;
        }

        /// <summary>
        /// The spawn disc must stay inside the arena lip. Farthest XZ corner
        /// at or past the inner rim overlaps the ring.
        /// </summary>
        public static string SpawnDiscRingHit(float minX, float maxX, float minZ, float maxZ)
        {
            return SpawnDiscRingHit(minX, maxX, minZ, maxZ, SpawnDiscRingInner);
        }

        public static string SpawnDiscRingHit(float minX, float maxX, float minZ, float maxZ, float ringInner)
        {
            if (FarthestRadius(minX, maxX, minZ, maxZ) >= ringInner)
            {
                return "spawn-disc ring";
            }

            return null;
        }

        public static bool Glob(string pattern, string text)
        {
            if (pattern == null || text == null)
            {
                return false;
            }

            string[] parts = pattern.Split('*');
            int index = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                if (part.Length == 0)
                {
                    continue;
                }

                int found = text.IndexOf(part, index, System.StringComparison.Ordinal);
                if (found < 0)
                {
                    return false;
                }

                if (i == 0 && pattern.Length > 0 && pattern[0] != '*' && found != 0)
                {
                    return false;
                }

                index = found + part.Length;
            }

            if (pattern.Length == 0 || pattern[pattern.Length - 1] != '*')
            {
                string last = parts.Length == 0 ? string.Empty : parts[parts.Length - 1];
                if (last.Length > 0 && !text.EndsWith(last, System.StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        public static int EffectiveQueue(int queue)
        {
            if (queue < 0)
            {
                return GeometryMin;
            }

            return queue;
        }

        /// <summary>
        /// A hint is soft when its edge score is under 0.9 of the wave-1 crop.
        /// The 048u smear sat near 0.35 of a hard stem. Wave clear has to match wave 1.
        /// The score is the inverse 10–90% width of the sharp glyph quintile, so
        /// a short crisp row matches a long one and the scrim behind the ink does not.
        /// </summary>
        public const float GlyphSharpRatio = 0.9f;

        /// <summary>
        /// Self-check floor for a crisp 048x crop against its own mode's wave-1
        /// segment. The runtime gate stays at <see cref="GlyphSharpRatio"/>.
        /// </summary>
        public const float GlyphSharpMargin = 0.95f;

        /// <summary>
        /// A box-blurred copy of a crisp crop must land under this fraction of
        /// the sharp reference. That is below the runtime fail line, with room.
        /// </summary>
        public const float GlyphBlurCeiling = 0.5f;

        /// <summary>
        /// Largest allowed gap between the two capture modes' ratios
        /// (reduce off and reduce on) for one segment.
        /// </summary>
        public const float GlyphModeGap = 0.05f;

        /// <summary>
        /// Luma is 0–255. A crop whose range is under this has no glyph ink.
        /// </summary>
        public const float GlyphFlatRange = 10f;

        /// <summary>
        /// <see cref="GlyphLuma"/> writes this scale. A reference under the
        /// floor is the old 0–1 noise reading, not a measured glyph.
        /// </summary>
        public const float GlyphLumaScale = 255f;

        public const float GlyphReferenceFloor = 1f;

        public const string GlyphSharpError = "hint-soft error";

        /// <summary>
        /// Rec. 709 luma on the 0–255 scale the sharpness gate compares.
        /// </summary>
        public static float GlyphLuma(float r, float g, float b)
        {
            return ((0.2126f * r) + (0.7152f * g) + (0.0722f * b)) * GlyphLumaScale;
        }

        // A step smaller than this is flat, not the side of a glyph.
        const float RiseFlatStep = 1.5f;

        // A reversal this large ends the edge. Smaller dips stay inside the rise.
        const float RiseReverse = 2.5f;

        // Ignore weak ramps. A real stem covers this fraction of the crop contrast.
        const float RiseSpanFraction = 0.45f;

        const float RiseMinWidth = 0.05f;

        const int RiseMinSamples = 4;

        // A rise whose bright end stays below this fraction of the crop range
        // is backing or scrim, not glyph ink. The stem reaches the ink.
        const float RiseInkFraction = 0.55f;

        // Sharp quintile of the ink rises. The median is pulled by the scrim
        // tail on the same edge. Rank 20 is the glyph stem. A smeared glyph
        // has no short rises, so this rank widens.
        const int RiseGlyphRank = 20;

        // A one-pixel step is 0.8 wide, so a hard edge lands near 80. The ratio
        // against wave 1 is what the gate compares; the scale only clears the floor.
        const float RiseScale = 64f;

        /// <summary>
        /// Sharpness of a luma crop, independent of how much text it holds and
        /// of the plate behind it. Whole-crop Laplacian variance grows with the
        /// edge count, and the median rise width grows with the scrim ramp
        /// under the ink. This keeps rises whose bright end reaches the glyph
        /// and scores the inverse 10–90% width at the sharp quintile. Empty
        /// pixels and extra letters do not change it. A hard stem scores high.
        /// A bilinear-smeared block scores low.
        /// </summary>
        public static float LaplacianVariance(float[] luma, int width, int height)
        {
            if (luma == null || width < 3 || height < 3 || luma.Length < width * height)
            {
                return 0f;
            }

            int count = width * height;
            float min = luma[0];
            float max = luma[0];
            for (int i = 1; i < count; i++)
            {
                float sample = luma[i];
                if (sample < min)
                {
                    min = sample;
                }

                if (sample > max)
                {
                    max = sample;
                }
            }

            float contrast = max - min;
            if (contrast < GlyphFlatRange || float.IsNaN(contrast))
            {
                return 0f;
            }

            float minSpan = GlyphFlatRange;
            float scaled = RiseSpanFraction * contrast;
            if (scaled > minSpan)
            {
                minSpan = scaled;
            }

            float inkFloor = min + (RiseInkFraction * contrast);
            System.Collections.Generic.List<float> rises = new System.Collections.Generic.List<float>();
            for (int y = 0; y < height; y++)
            {
                CollectRises(luma, y * width, 1, width, minSpan, inkFloor, rises);
            }

            for (int x = 0; x < width; x++)
            {
                CollectRises(luma, x, width, height, minSpan, inkFloor, rises);
            }

            if (rises.Count < RiseMinSamples)
            {
                return 0f;
            }

            float widthRank = GlyphRiseWidth(rises);
            if (widthRank <= 0f || float.IsNaN(widthRank))
            {
                return 0f;
            }

            return RiseScale / widthRank;
        }

        /// <summary>
        /// 10–90% widths of monotonic edges along one row or column.
        /// A rise whose bright end is below <paramref name="inkFloor"/> is backing, not ink.
        /// </summary>
        static void CollectRises(float[] data, int start, int stride, int count, float minSpan, float inkFloor, System.Collections.Generic.List<float> rises)
        {
            int x = 0;
            while (x < count - 1)
            {
                float delta = data[start + ((x + 1) * stride)] - data[start + (x * stride)];
                if (delta < RiseFlatStep && delta > -RiseFlatStep)
                {
                    x++;
                    continue;
                }

                float sign = delta > 0f ? 1f : -1f;
                int origin = x;
                int end = x + 1;
                while (end < count - 1)
                {
                    float step = data[start + ((end + 1) * stride)] - data[start + (end * stride)];
                    if (step * sign < -RiseReverse)
                    {
                        break;
                    }

                    end++;
                    float traveled = data[start + (end * stride)] - data[start + (origin * stride)];
                    if (traveled < 0f)
                    {
                        traveled = -traveled;
                    }

                    if (traveled >= minSpan && step < RiseFlatStep && step > -RiseFlatStep)
                    {
                        break;
                    }
                }

                float head = data[start + (origin * stride)];
                float tail = data[start + (end * stride)];
                float span = tail - head;
                if (span < 0f)
                {
                    span = -span;
                }

                if (span < minSpan)
                {
                    x = end > x ? end : x + 1;
                    continue;
                }

                float low = head < tail ? head : tail;
                float high = head > tail ? head : tail;
                if (high < inkFloor)
                {
                    x = end + 1;
                    continue;
                }
                float level10 = low + (0.10f * (high - low));
                float level90 = low + (0.90f * (high - low));
                bool rising = tail >= head;
                float pos10 = -1f;
                float pos90 = -1f;
                int steps = end - origin;
                for (int i = 1; i <= steps; i++)
                {
                    int leftIndex = rising ? origin + (i - 1) : end - (i - 1);
                    int rightIndex = rising ? origin + i : end - i;
                    float left = data[start + (leftIndex * stride)];
                    float right = data[start + (rightIndex * stride)];
                    if (left > right)
                    {
                        continue;
                    }

                    if (pos10 < 0f && right >= level10)
                    {
                        float den = right - left;
                        float frac = den > 1e-6f ? (level10 - left) / den : 1f;
                        pos10 = (i - 1) + frac;
                    }

                    if (right >= level90)
                    {
                        float den = right - left;
                        float frac = den > 1e-6f ? (level90 - left) / den : 1f;
                        pos90 = (i - 1) + frac;
                        break;
                    }
                }

                if (pos10 >= 0f && pos90 >= pos10)
                {
                    float width = pos90 - pos10;
                    if (width < RiseMinWidth)
                    {
                        width = RiseMinWidth;
                    }

                    rises.Add(width);
                }

                x = end + 1;
            }
        }

        /// <summary>
        /// Width at the sharp quintile. The wider scrim ramps stay above this rank.
        /// </summary>
        static float GlyphRiseWidth(System.Collections.Generic.List<float> values)
        {
            values.Sort();
            int last = values.Count - 1;
            int index = (last * RiseGlyphRank) / 100;
            if (index < 0)
            {
                index = 0;
            }

            if (index > last)
            {
                index = last;
            }

            return values[index];
        }

        /// <summary>
        /// False when the crop is flat, too small, or has no edge energy.
        /// The capture reports that as a gate error, not as a soft hint.
        /// </summary>
        public static bool TryGlyphVariance(float[] luma, int width, int height, out float variance)
        {
            variance = -1f;
            if (luma == null || width < 3 || height < 3 || luma.Length < width * height)
            {
                return false;
            }

            int count = width * height;
            float min = luma[0];
            float max = luma[0];
            for (int i = 1; i < count; i++)
            {
                float sample = luma[i];
                if (sample < min)
                {
                    min = sample;
                }

                if (sample > max)
                {
                    max = sample;
                }
            }

            if (max - min < GlyphFlatRange)
            {
                return false;
            }

            float measured = LaplacianVariance(luma, width, height);
            if (measured <= 0f || float.IsNaN(measured))
            {
                return false;
            }

            variance = measured;
            return true;
        }

        /// <summary>
        /// FAIL when this hint crop is smeared next to the wave-1 reference.
        /// A zero or sub-floor reference, or a negative variance (flat crop),
        /// is an error: the gate did not see glyphs, so it must not pass or fail.
        /// </summary>
        public static string GlyphSharpnessHit(float variance, float reference)
        {
            if (reference < GlyphReferenceFloor || variance < 0f || float.IsNaN(variance) || float.IsNaN(reference))
            {
                return GlyphSharpError;
            }

            if (variance < reference * GlyphSharpRatio)
            {
                return "hint-soft "
                    + variance.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)
                    + " ref "
                    + reference.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
            }

            return null;
        }

        public static bool SelfCheck()
        {
            Rules rules = TestRules();
            if (!Expect("unknown", Evaluate("3", rules, new Fact[] { FactOf("OlivePlate", "Default-Material", "decor", 2000, ZTestLEqual, 1f, 0f, 0.3f, true) }, null, null, null)))
            {
                return false;
            }

            if (!Expect("dead", Evaluate("4", rules, new Fact[] { FactOf("Arena_Beam", "Mat_Arena_W3_Beam", "decor", 2000, ZTestLEqual, 1f, 0.2f, 0.33f, false) }, null, null, null)))
            {
                return false;
            }

            // Unlit black beam plate. Not a blob shadow, so the dead rule still fires.
            Fact deadBeam = FactOf("Arena_Beam", "Mat_Arena_W3_Beam", "decor", "Unlit/Color", 2000, ZTestLEqual, 1f, 0.1f, 0.12f, true);
            if (!Expect("dead", Evaluate("4", rules, new Fact[] { deadBeam }, null, null, null)))
            {
                return false;
            }

            // A real blob (component or Mat_ContactBlob, on a floor, alpha
            // above the visible floor) is a shadow at 2450 and at 2990.
            Fact blob = BlobFact("ContactBlob", "Mat_ContactBlob", BlobShadowShader, 2450, 0.9f, true);
            string[] blobHits = Evaluate("1", rules, new Fact[] { blob }, null, null, null);
            if (blobHits.Length != 0 || Expect("dead", blobHits))
            {
                return false;
            }

            Fact blobQueue = BlobFact("ContactBlob", "Mat_ContactBlob", BlobShadowShader, 2990, 0.9f, true);
            if (Evaluate("1", rules, new Fact[] { blobQueue }, null, null, null).Length != 0)
            {
                return false;
            }

            Fact materialOnly = BlobFact("Arena_Disc", "Mat_ContactBlob", BlobShadowShader, 2450, 0.9f, false);
            if (Evaluate("1", rules, new Fact[] { materialOnly }, null, null, null).Length != 0)
            {
                return false;
            }

            // The object name is not a waiver. Off the floor, Mat_ContactBlob
            // is still a dead plate. The shader name is not a waiver either.
            Fact namedPlate = FactOf("ContactBlob", "Mat_ContactBlob", "decor", "Unlit/Color", 2450, ZTestLEqual, 0.9f, 0f, 0f, true);
            if (!Expect("dead", Evaluate("1", rules, new Fact[] { namedPlate }, null, null, null)))
            {
                return false;
            }

            Fact offFloor = BlobFact("ContactBlob", "Mat_ContactBlob", BlobShadowShader, 2450, 0.9f, true);
            offFloor.OnFloor = false;
            if (!Expect("dead", Evaluate("1", rules, new Fact[] { offFloor }, null, null, null)))
            {
                return false;
            }

            Fact shaderOnly = FactOf("Arena_Plate", "Mat_Arena_Plate", "decor", BlobShadowShader, 2450, ZTestLEqual, 0.9f, 0f, 0f, true);
            shaderOnly.OnFloor = true;
            if (!Expect("dead", Evaluate("1", rules, new Fact[] { shaderOnly }, null, null, null)))
            {
                return false;
            }

            Fact invisibleBlob = BlobFact("ContactBlob", "Mat_ContactBlob", BlobShadowShader, 2450, 0f, true);
            if (!Expect("shadow-alpha", Evaluate("1", rules, new Fact[] { invisibleBlob }, null, null, null)))
            {
                return false;
            }

            if (Evaluate("1", rules, new Fact[] { FactOf("Asteroid_M_C", "Mat_Env_Belt", "asteroid", 2000, ZTestLEqual, 1f, 0.1f, 0.2f, true) }, null, null, null).Length != 0)
            {
                return false;
            }

            if (DeathPuffHit("death", "Smoke", 8, true) != "death-remnant Smoke")
            {
                return false;
            }

            if (DeathPuffHit("1", "Smoke", 8, true) != null || DeathPuffHit("death", "Smoke", 0, true) != null)
            {
                return false;
            }

            if (DeathPuffHit("death", "Chunks", 3, true) != "death-remnant Chunks")
            {
                return false;
            }

            if (DeathPuffHit("death", "Sparks", 2, true) != "death-remnant Sparks")
            {
                return false;
            }

            if (DeathPuffHit("death", "Sparks", 2, false) != null || DeathPuffHit("1", "Chunks", 3, true) != null)
            {
                return false;
            }

            if (SpawnDiscRingHit(-3.6f, 3.6f, -3.6f, 3.6f) != null)
            {
                return false;
            }

            if (SpawnDiscRingHit(22f, 38f, -8f, 8f) != "spawn-disc ring")
            {
                return false;
            }

            if (!Expect("ztest-always", Evaluate("1", rules, new Fact[] { FactOf("Ship_Body", "Mat_Ship_Hull", "ship", "Hidden/Evil", 2000, ZTestAlways, 1f, 1f, 0.6f, false) }, null, null, null)))
            {
                return false;
            }

            if (!Expect("overlay-queue", Evaluate("1", rules, new Fact[] { FactOf("Enemy_01", "Mat_Enemy", "enemy", 4000, ZTestLEqual, 1f, 1f, 0.6f, false) }, null, null, null)))
            {
                return false;
            }

            if (!Expect("gameplay-queue", Evaluate("1", rules, new Fact[] { FactOf("Asteroid_Large", "Mat_Asteroid", "asteroid", 1000, ZTestLEqual, 1f, 0.5f, 0.5f, false) }, null, null, null)))
            {
                return false;
            }

            if (!Expect("dome-ztest", Evaluate("3", rules, new Fact[] { FactOf("ArenaLip", "Mat_Env_Lip", "dome", 3000, ZTestAlways, 0.3f, 0.2f, 0.5f, false) }, null, null, null)))
            {
                return false;
            }

            CanvasFact badSort = new CanvasFact();
            badSort.Name = "Canvas";
            badSort.Sort = 999;
            badSort.Layer = UiLayer;
            badSort.Enabled = true;
            if (!Expect("canvas-sort", Evaluate("hangar", rules, null, new CanvasFact[] { badSort }, null, null)))
            {
                return false;
            }

            if (!Expect("ui-overlap", Evaluate("hangar", rules, null, null, new string[] { "WorldBadge x BadgeRow" }, null)))
            {
                return false;
            }

            if (Expect("ui-overlap", Evaluate("hangar", rules, null, null, new string[] { "Hint x HintScrim" }, null)))
            {
                return false;
            }

            if (Expect("ui-overlap", Evaluate("hangar", rules, null, null, new string[] { "HintScrim x Hint" }, null)))
            {
                return false;
            }

            BodyFact bare = new BodyFact();
            bare.Name = "Ship";
            bare.Kind = "ship";
            bare.Active = true;
            bare.HasContact = false;
            if (!Expect("contact-shadow", Evaluate("1", rules, null, null, null, new BodyFact[] { bare })))
            {
                return false;
            }

            Fact hidden = FactOf("Arena_Beam", "Mat_Arena_W3_Beam", "decor", 3000, ZTestLEqual, 0f, 0f, 0f, true);
            string[] hiddenHits = Evaluate("4", rules, new Fact[] { hidden }, null, null, null);
            if (Expect("dead", hiddenHits) || Expect("unknown", hiddenHits))
            {
                return false;
            }

            Fact mine = FactOf("Mine_A", "Mat_Mine", "decor", 2000, ZTestLEqual, 1f, 0.8f, 0.6f, false);
            if (!Expect("unknown", Evaluate("3", rules, new Fact[] { mine }, null, null, null)))
            {
                return false;
            }

            if (Evaluate("4", rules, new Fact[] { mine }, null, null, null).Length != 0)
            {
                return false;
            }

            if (Evaluate("1", rules, CleanFacts(), CleanCanvas(), new string[0], CleanBodies(), CleanCameras()).Length != 0)
            {
                return false;
            }

            CanvasFact offUi = new CanvasFact();
            offUi.Name = "Canvas";
            offUi.Sort = 0;
            offUi.Layer = 0;
            offUi.Enabled = true;
            if (!Expect("canvas-layer", Evaluate("1", rules, null, new CanvasFact[] { offUi }, null, null)))
            {
                return false;
            }

            CameraFact wide = new CameraFact();
            wide.Name = "AgrCaptureUiCamera";
            wide.Mask = (1 << 0) | UiCameraMask;
            wide.Enabled = true;
            if (!Expect("ui-camera", Evaluate("1", rules, null, null, null, null, new CameraFact[] { wide })))
            {
                return false;
            }

            float arenaScale = 30f / 22f;
            IslandFact plate = new IslandFact();
            plate.Name = "Arena_World3_Blockout";
            plate.MinX = -2f * arenaScale;
            plate.MaxX = 2f * arenaScale;
            plate.MinZ = -3f * arenaScale;
            plate.MaxZ = 3f * arenaScale;
            if (!Expect("spawn-decor", Evaluate("3", rules, CleanFacts(), CleanCanvas(), new string[0], CleanBodies(), CleanCameras(), new IslandFact[] { plate })))
            {
                return false;
            }

            if (!IsSpawnClutter(-1.94f * arenaScale, 1.94f * arenaScale, -4.44f * arenaScale, -1.56f * arenaScale))
            {
                return false;
            }

            if (IsSpawnClutter(-18f * arenaScale, 18f * arenaScale, -3f * arenaScale, 3f * arenaScale))
            {
                return false;
            }

            if (IsSpawnClutter(-25f * arenaScale, 25f * arenaScale, -25f * arenaScale, 25f * arenaScale))
            {
                return false;
            }

            if (IsSpawnClutter(-8.88f * arenaScale, -6.12f * arenaScale, -6.43f * arenaScale, -2.23f * arenaScale))
            {
                return false;
            }

            IslandFact floor = new IslandFact();
            floor.Name = "Arena_Blockout";
            floor.MinX = -25f * arenaScale;
            floor.MaxX = 25f * arenaScale;
            floor.MinZ = -25f * arenaScale;
            floor.MaxZ = 25f * arenaScale;
            if (Evaluate("1", rules, CleanFacts(), CleanCanvas(), new string[0], CleanBodies(), CleanCameras(), new IslandFact[] { floor }).Length != 0)
            {
                return false;
            }

            if (!IsRingCorner(13.5f * arenaScale, 18.5f * arenaScale, 13.5f * arenaScale, 18.5f * arenaScale, 30f))
            {
                return false;
            }

            if (IsRingCorner(18.82f * arenaScale, 20.02f * arenaScale, 9.73f * arenaScale, 10.93f * arenaScale, 30f))
            {
                return false;
            }

            if (IsRingCorner(-18.39f * arenaScale, -15.15f * arenaScale, 11.44f * arenaScale, 15.30f * arenaScale, 30f))
            {
                return false;
            }

            Rules parsed;
            string error;
            if (!TryParse(TestJson(), out parsed, out error))
            {
                return false;
            }

            if (parsed.CanvasSorts == null || parsed.CanvasSorts.Length != 7 || parsed.CanvasSorts[0] != 0)
            {
                return false;
            }

            if (!GlyphSharpnessSelfCheck())
            {
                return false;
            }

            if (!ArenaShapeCatalog.SelfCheck())
            {
                return false;
            }

            return Glob("Ship*", "Ship_Body") && Glob("Mat_*", "Mat_Arena") && !Glob("Ship*", "Enemy_01");
        }

        static bool GlyphSharpnessSelfCheck()
        {
            const int width = 40;
            const int height = 20;
            float[] sharp = new float[width * height];
            for (int i = 0; i < sharp.Length; i++)
            {
                sharp[i] = 12f;
            }

            for (int y = 2; y < height - 2; y++)
            {
                for (int x = 8; x < 12; x++)
                {
                    sharp[(y * width) + x] = 200f;
                }

                for (int x = 20; x < 24; x++)
                {
                    sharp[(y * width) + x] = 200f;
                }
            }

            for (int y = (height / 2) - 1; y <= height / 2; y++)
            {
                for (int x = 8; x < 24; x++)
                {
                    sharp[(y * width) + x] = 200f;
                }
            }

            float[] smear = BoxBlur(BoxBlur(sharp, width, height), width, height);
            float sharpVar = LaplacianVariance(sharp, width, height);
            float smearVar = LaplacianVariance(smear, width, height);
            if (sharpVar <= 0f || smearVar < 0f || smearVar >= sharpVar * GlyphSharpRatio)
            {
                return false;
            }

            if (GlyphSharpnessHit(sharpVar, sharpVar) != null)
            {
                return false;
            }

            string hit = GlyphSharpnessHit(smearVar, sharpVar);
            if (hit == null || hit.IndexOf("hint-soft", System.StringComparison.Ordinal) < 0 || hit.IndexOf("error", System.StringComparison.Ordinal) >= 0)
            {
                return false;
            }

            string missing = GlyphSharpnessHit(smearVar, 0f);
            if (missing == null || missing.IndexOf("error", System.StringComparison.Ordinal) < 0)
            {
                return false;
            }

            string flatHit = GlyphSharpnessHit(-1f, sharpVar);
            if (flatHit == null || flatHit.IndexOf("error", System.StringComparison.Ordinal) < 0 || flatHit.IndexOf(" ref ", System.StringComparison.Ordinal) >= 0)
            {
                return false;
            }

            return RealCropSelfCheck();
        }

        // One file per hint segment, left to right, from the crisp 048x captures.
        // Mode 0 is reduce off. Mode 1 is reduce on. The capture gate scores
        // each segment on its own and keeps the worst.
        static readonly string[] GlyphSceneNames = new string[]
        {
            "w1", "w3", "w4", "death", "hangar", "waveclear"
        };

        static readonly int[] GlyphSceneSegments = new int[]
        {
            2, 6, 6, 2, 3, 1
        };

        /// <summary>
        /// Every hint segment from the crisp 048x captures, both modes.
        /// Each segment passes against that mode's wave-1 best with margin.
        /// A box blur of that same segment fails. A flat crop errors.
        /// The test runner copies Tools/fixtures/hint next to the executable.
        /// </summary>
        static bool RealCropSelfCheck()
        {
            int flatW;
            int flatH;
            float[] flat = LoadHintCrop("hint_flat.bmp", out flatW, out flatH);
            if (flat == null || flatW != 48 || flatH != 24)
            {
                return false;
            }

            float flatVar;
            if (TryGlyphVariance(flat, flatW, flatH, out flatVar))
            {
                return false;
            }

            if (GlyphSceneNames.Length != GlyphSceneSegments.Length || GlyphSceneNames[0] != "w1")
            {
                return false;
            }

            int scenes = GlyphSceneNames.Length;
            float[][][] sharp = new float[2][][];
            float[][][] blurred = new float[2][][];
            for (int mode = 0; mode < 2; mode++)
            {
                sharp[mode] = new float[scenes][];
                blurred[mode] = new float[scenes][];
                for (int scene = 0; scene < scenes; scene++)
                {
                    int count = GlyphSceneSegments[scene];
                    if (count < 1)
                    {
                        return false;
                    }

                    sharp[mode][scene] = new float[count];
                    blurred[mode][scene] = new float[count];
                    for (int index = 0; index < count; index++)
                    {
                        string name = GlyphCropName(GlyphSceneNames[scene], mode, index);
                        int width;
                        int height;
                        float[] crop = LoadHintCrop(name, out width, out height);
                        if (crop == null || width < 3 || height < 3)
                        {
                            return false;
                        }

                        float variance;
                        if (!TryGlyphVariance(crop, width, height, out variance) || variance <= GlyphReferenceFloor)
                        {
                            System.Console.WriteLine("hint crisp low " + name);
                            return false;
                        }

                        float[] smear = BoxBlur(BoxBlur(crop, width, height), width, height);
                        float smearVar;
                        if (!TryGlyphVariance(smear, width, height, out smearVar))
                        {
                            System.Console.WriteLine("hint blur flat " + name);
                            return false;
                        }

                        sharp[mode][scene][index] = variance;
                        blurred[mode][scene][index] = smearVar;
                    }
                }
            }

            float[] reference = new float[2];
            for (int mode = 0; mode < 2; mode++)
            {
                reference[mode] = sharp[mode][0][0];
                int w1Count = GlyphSceneSegments[0];
                for (int index = 1; index < w1Count; index++)
                {
                    if (sharp[mode][0][index] > reference[mode])
                    {
                        reference[mode] = sharp[mode][0][index];
                    }
                }

                if (reference[mode] <= GlyphReferenceFloor)
                {
                    return false;
                }

                string flatHit = GlyphSharpnessHit(-1f, reference[mode]);
                if (flatHit == null || flatHit.IndexOf("error", System.StringComparison.Ordinal) < 0 || flatHit.IndexOf(" ref ", System.StringComparison.Ordinal) >= 0)
                {
                    return false;
                }

                for (int scene = 0; scene < scenes; scene++)
                {
                    int count = GlyphSceneSegments[scene];
                    for (int index = 0; index < count; index++)
                    {
                        string name = GlyphCropName(GlyphSceneNames[scene], mode, index);
                        float variance = sharp[mode][scene][index];
                        if (variance < reference[mode] * GlyphSharpMargin)
                        {
                            System.Console.WriteLine(
                                "hint margin " + name
                                + " " + variance.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)
                                + " ref " + reference[mode].ToString("0.0", System.Globalization.CultureInfo.InvariantCulture));
                            return false;
                        }

                        if (GlyphSharpnessHit(variance, reference[mode]) != null)
                        {
                            return false;
                        }

                        float smearVar = blurred[mode][scene][index];
                        if (smearVar >= reference[mode] * GlyphBlurCeiling || smearVar >= variance * GlyphSharpRatio)
                        {
                            System.Console.WriteLine(
                                "hint blur high " + name
                                + " " + smearVar.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture));
                            return false;
                        }

                        string soft = GlyphSharpnessHit(smearVar, reference[mode]);
                        if (soft == null || soft.IndexOf("hint-soft", System.StringComparison.Ordinal) < 0 || soft.IndexOf("error", System.StringComparison.Ordinal) >= 0)
                        {
                            return false;
                        }
                    }
                }
            }

            for (int scene = 0; scene < scenes; scene++)
            {
                int count = GlyphSceneSegments[scene];
                for (int index = 0; index < count; index++)
                {
                    float ratio0 = sharp[0][scene][index] / reference[0];
                    float ratio1 = sharp[1][scene][index] / reference[1];
                    float gap = ratio0 - ratio1;
                    if (gap < 0f)
                    {
                        gap = -gap;
                    }

                    if (gap > GlyphModeGap)
                    {
                        System.Console.WriteLine(
                            "hint mode gap " + GlyphSceneNames[scene]
                            + " " + index.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        return false;
                    }
                }
            }

            return true;
        }

        static string GlyphCropName(string scene, int mode, int index)
        {
            return "hint_"
                + scene
                + "_r"
                + mode.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + "_"
                + index.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + ".bmp";
        }

        static float[] LoadHintCrop(string name, out int width, out int height)
        {
            width = 0;
            height = 0;
            string path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), name);
            if (!System.IO.File.Exists(path))
            {
                System.Console.WriteLine("hint crop missing " + path);
                return null;
            }

            byte[] file;
            try
            {
                file = System.IO.File.ReadAllBytes(path);
            }
            catch (System.Exception)
            {
                return null;
            }

            if (file == null || file.Length < 54 || file[0] != (byte)'B' || file[1] != (byte)'M')
            {
                return null;
            }

            int offset = System.BitConverter.ToInt32(file, 10);
            int cropW = System.BitConverter.ToInt32(file, 18);
            int cropH = System.BitConverter.ToInt32(file, 22);
            short bpp = System.BitConverter.ToInt16(file, 28);
            int compression = System.BitConverter.ToInt32(file, 30);
            if (bpp != 24 || compression != 0 || cropW < 3 || cropH < 3 || offset < 54)
            {
                return null;
            }

            int stride = ((cropW * 3) + 3) & ~3;
            if (offset + (stride * cropH) > file.Length)
            {
                return null;
            }

            float[] luma = new float[cropW * cropH];
            for (int y = 0; y < cropH; y++)
            {
                int row = offset + ((cropH - 1 - y) * stride);
                for (int x = 0; x < cropW; x++)
                {
                    int pixel = row + (x * 3);
                    float b = file[pixel] / 255f;
                    float g = file[pixel + 1] / 255f;
                    float r = file[pixel + 2] / 255f;
                    luma[(y * cropW) + x] = GlyphLuma(r, g, b);
                }
            }

            width = cropW;
            height = cropH;
            return luma;
        }

        static float[] BoxBlur(float[] source, int width, int height)
        {
            float[] blurred = new float[source.Length];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float sum = 0f;
                    int count = 0;
                    for (int oy = -2; oy <= 2; oy++)
                    {
                        int sampleY = y + oy;
                        if (sampleY < 0 || sampleY >= height)
                        {
                            continue;
                        }

                        for (int ox = -2; ox <= 2; ox++)
                        {
                            int sampleX = x + ox;
                            if (sampleX < 0 || sampleX >= width)
                            {
                                continue;
                            }

                            sum += source[(sampleY * width) + sampleX];
                            count++;
                        }
                    }

                    blurred[(y * width) + x] = count > 0 ? sum / count : source[(y * width) + x];
                }
            }

            return blurred;
        }

        static void NoteFact(string world, Rules rules, string[] rendererPatterns, string[] materialPatterns, Fact fact, System.Collections.Generic.List<string> hits)
        {
            if (fact == null || !fact.Active)
            {
                return;
            }

            bool shadow = IsRealBlob(fact);
            string name = Bare(fact.Name);
            string material = Bare(fact.Material);
            // Only a real contact shadow skips the dead plate. The blob shader
            // is not enough: the renderer needs a ContactBlob or Mat_ContactBlob,
            // it has to sit on a floor plate, and alpha has to clear the floor.
            if (shadow && fact.Alpha <= VisibleAlpha)
            {
                hits.Add("shadow-alpha " + name + " material " + material);
            }

            if (!Shown(fact))
            {
                return;
            }

            int queue = EffectiveQueue(fact.Queue);
            if (!MatchAny(rendererPatterns, name) || !MatchAny(materialPatterns, material))
            {
                hits.Add("unknown " + name + " material " + material + " world " + world);
            }

            bool deadExempt = MatchAny(rules.DeadExempt, name) || MatchAny(rules.DeadExempt, material);
            if (!shadow && !deadExempt && queue < TransparentMin && fact.Emission <= DeadEmissionMax && fact.Albedo <= DeadAlbedoMax && (fact.Unlit || DeadHint(rules, name) || DeadHint(rules, material)))
            {
                hits.Add("dead " + name + " material " + material);
            }

            string role = fact.Role == null ? string.Empty : fact.Role;
            bool exempt = role == "post" || role == "hud" || MatchAny(rules.OverlayExemptShaders, fact.Shader);
            if (!exempt && fact.ZTest == ZTestAlways)
            {
                hits.Add("ztest-always " + name + " shader " + fact.Shader);
            }

            if (!exempt && queue >= OverlayMin)
            {
                hits.Add("overlay-queue " + name + " queue " + queue.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            if (Gameplay(role) && (queue < GeometryMin || queue > TransparentMax))
            {
                hits.Add("gameplay-queue " + name + " queue " + queue.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            if (role == "dome" && fact.ZTest != ZTestLEqual && fact.ZTest != ZTestLess)
            {
                hits.Add("dome-ztest " + name + " ztest " + fact.ZTest.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        static void NoteCanvas(Rules rules, CanvasFact canvas, System.Collections.Generic.List<string> hits)
        {
            if (canvas == null || !canvas.Enabled)
            {
                return;
            }

            if (canvas.Layer != UiLayer)
            {
                string canvasName = canvas.Name == null ? "canvas" : canvas.Name;
                hits.Add("canvas-layer " + canvasName + " layer " + canvas.Layer.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            int[] sorts = rules.CanvasSorts;
            if (sorts == null)
            {
                hits.Add("canvas-sort " + canvas.Name + " order " + canvas.Sort.ToString(System.Globalization.CultureInfo.InvariantCulture));
                return;
            }

            for (int i = 0; i < sorts.Length; i++)
            {
                if (sorts[i] == canvas.Sort)
                {
                    return;
                }
            }

            hits.Add("canvas-sort " + canvas.Name + " order " + canvas.Sort.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        static void NoteBody(BodyFact body, System.Collections.Generic.List<string> hits)
        {
            if (body == null || !body.Active || body.HasContact)
            {
                return;
            }

            string kind = body.Kind == null ? "body" : body.Kind;
            hits.Add("contact-shadow " + kind + " " + body.Name);
        }

        static void NoteUiCamera(CameraFact camera, System.Collections.Generic.List<string> hits)
        {
            if (camera == null || !camera.Enabled)
            {
                return;
            }

            int outside = camera.Mask & ~UiCameraMask;
            if (outside == 0)
            {
                return;
            }

            string name = camera.Name == null ? "camera" : camera.Name;
            hits.Add("ui-camera " + name + " mask " + camera.Mask.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        static void NoteIsland(IslandFact island, System.Collections.Generic.List<string> hits)
        {
            if (island == null)
            {
                return;
            }

            if (!IsSpawnClutter(island.MinX, island.MaxX, island.MinZ, island.MaxZ))
            {
                return;
            }

            string name = island.Name == null ? "decor" : island.Name;
            hits.Add("spawn-decor " + name);
        }

        public static bool IsBlobShadow(string shader)
        {
            if (shader == null || shader.Length == 0)
            {
                return false;
            }

            return shader.IndexOf(BlobShadowShader, System.StringComparison.Ordinal) >= 0;
        }

        /// <summary>
        /// ContactBlob component or Mat_ContactBlob, on a floor plate.
        /// Alpha is checked by the caller. The shader name is not a class.
        /// </summary>
        static bool IsRealBlob(Fact fact)
        {
            if (fact == null || !fact.OnFloor)
            {
                return false;
            }

            if (fact.ContactBlob)
            {
                return true;
            }

            return Bare(fact.Material) == DecorDepthRules.BlobMaterial;
        }

        static bool Shown(Fact fact)
        {
            if (fact == null || !fact.Active)
            {
                return false;
            }

            int queue = EffectiveQueue(fact.Queue);
            if (queue < TransparentMin)
            {
                return true;
            }

            return fact.Alpha > VisibleAlpha;
        }

        static bool Gameplay(string role)
        {
            return role == "ship" || role == "enemy" || role == "asteroid" || role == "pickup" || role == "pillar";
        }

        static bool DeadHint(Rules rules, string name)
        {
            if (name == null || rules == null || rules.DeadNameHints == null)
            {
                return false;
            }

            for (int i = 0; i < rules.DeadNameHints.Length; i++)
            {
                string hint = rules.DeadNameHints[i];
                if (hint != null && hint.Length > 0 && name.IndexOf(hint, System.StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        static bool MatchAny(string[] patterns, string text)
        {
            if (patterns == null || text == null)
            {
                return false;
            }

            for (int i = 0; i < patterns.Length; i++)
            {
                if (Glob(patterns[i], text))
                {
                    return true;
                }
            }

            return false;
        }

        static string Bare(string name)
        {
            if (name == null)
            {
                return string.Empty;
            }

            const string Suffix = " (Instance)";
            if (name.EndsWith(Suffix, System.StringComparison.Ordinal))
            {
                return name.Substring(0, name.Length - Suffix.Length);
            }

            return name;
        }

        static string[] RendererPatterns(Rules rules, string world)
        {
            System.Collections.Generic.List<string> into = new System.Collections.Generic.List<string>();
            AddPatterns(into, rules.Renderers);
            AddPatterns(into, WorldRenderers(rules, world));
            if (world == "death" || world == "waveclear")
            {
                AddPatterns(into, rules.World1Renderers);
            }

            return into.ToArray();
        }

        static string[] MaterialPatterns(Rules rules, string world)
        {
            System.Collections.Generic.List<string> into = new System.Collections.Generic.List<string>();
            AddPatterns(into, rules.Materials);
            AddPatterns(into, WorldMaterials(rules, world));
            if (world == "death" || world == "waveclear")
            {
                AddPatterns(into, rules.World1Materials);
            }

            return into.ToArray();
        }

        static string[] WorldRenderers(Rules rules, string world)
        {
            if (world == "hangar")
            {
                return rules.HangarRenderers;
            }

            if (world == "1")
            {
                return rules.World1Renderers;
            }

            if (world == "3")
            {
                return rules.World3Renderers;
            }

            if (world == "4")
            {
                return rules.World4Renderers;
            }

            if (world == "death")
            {
                return rules.DeathRenderers;
            }

            if (world == "waveclear")
            {
                return rules.WaveClearRenderers;
            }

            return null;
        }

        static string[] WorldMaterials(Rules rules, string world)
        {
            if (world == "hangar")
            {
                return rules.HangarMaterials;
            }

            if (world == "1")
            {
                return rules.World1Materials;
            }

            if (world == "3")
            {
                return rules.World3Materials;
            }

            if (world == "4")
            {
                return rules.World4Materials;
            }

            if (world == "death")
            {
                return rules.DeathMaterials;
            }

            if (world == "waveclear")
            {
                return rules.WaveClearMaterials;
            }

            return null;
        }

        static void AddPatterns(System.Collections.Generic.List<string> into, string[] extra)
        {
            if (extra == null)
            {
                return;
            }

            for (int i = 0; i < extra.Length; i++)
            {
                if (extra[i] != null && extra[i].Length > 0)
                {
                    into.Add(extra[i]);
                }
            }
        }

        static bool Expect(string token, string[] reasons)
        {
            if (reasons == null || token == null)
            {
                return false;
            }

            for (int i = 0; i < reasons.Length; i++)
            {
                string reason = reasons[i];
                if (reason != null && reason.IndexOf(token, System.StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        static Fact BlobFact(string name, string material, string shader, int queue, float alpha, bool contact)
        {
            Fact fact = FactOf(name, material, "decor", shader, queue, ZTestLEqual, alpha, 0f, 0f, true);
            fact.ContactBlob = contact;
            fact.OnFloor = true;
            return fact;
        }

        static Fact FactOf(string name, string material, string role, int queue, int ztest, float alpha, float emission, float albedo, bool unlit)
        {
            return FactOf(name, material, role, string.Empty, queue, ztest, alpha, emission, albedo, unlit);
        }

        static Fact FactOf(string name, string material, string role, string shader, int queue, int ztest, float alpha, float emission, float albedo, bool unlit)
        {
            Fact fact = new Fact();
            fact.Name = name;
            fact.Material = material;
            fact.Shader = shader;
            fact.Role = role;
            fact.Queue = queue;
            fact.ZTest = ztest;
            fact.Alpha = alpha;
            fact.Emission = emission;
            fact.Albedo = albedo;
            fact.Unlit = unlit;
            fact.Active = true;
            return fact;
        }

        static Rules TestRules()
        {
            Rules rules = new Rules();
            rules.Renderers = new string[] { "Ship*", "Enemy*", "Asteroid*", "Arena*", "ContactBlob", "AstroGrid" };
            rules.Materials = new string[] { "Mat_Ship_*", "Mat_Enemy", "Mat_Asteroid", "Mat_Env_*", "Mat_Arena_*", "Mat_ContactBlob" };
            rules.HangarRenderers = new string[] { "Desk" };
            rules.World1Renderers = new string[0];
            rules.World3Renderers = new string[0];
            rules.World4Renderers = new string[] { "Mine*" };
            rules.DeathRenderers = new string[0];
            rules.WaveClearRenderers = new string[0];
            rules.HangarMaterials = new string[0];
            rules.World1Materials = new string[0];
            rules.World3Materials = new string[0];
            rules.World4Materials = new string[] { "Mat_Mine" };
            rules.DeathMaterials = new string[0];
            rules.WaveClearMaterials = new string[0];
            rules.OverlayExemptShaders = new string[] { "AGR/Post" };
            rules.DeadNameHints = new string[] { "Beam", "W3_Beam", "W4_Glow" };
            rules.DeadExempt = new string[] { "Asteroid*", "FX*", "Smoke", "Chunks", "Mat_Env_Belt", "Mat_FX_*" };
            rules.CanvasSorts = new int[] { 0, 80, 120, 150, 160, 200, 250 };
            return rules;
        }

        static Fact[] CleanFacts()
        {
            return new Fact[]
            {
                FactOf("Ship_Body", "Mat_Ship_Hull", "ship", 2000, ZTestLEqual, 1f, 1f, 0.6f, false),
                FactOf("ContactBlob", "Mat_Env_Blob", "decor", 3000, ZTestLEqual, 0.35f, 0f, 0f, true),
                FactOf("ArenaLip", "Mat_Env_Lip", "dome", 3000, ZTestLEqual, 0.3f, 0.2f, 0.5f, false),
                FactOf("AstroGrid", "Mat_Env_Grid", "decor", 2900, ZTestLEqual, 0.24f, 0f, 0.5f, true),
            };
        }

        static CanvasFact[] CleanCanvas()
        {
            CanvasFact canvas = new CanvasFact();
            canvas.Name = "Canvas";
            canvas.Sort = 0;
            canvas.Layer = UiLayer;
            canvas.Enabled = true;
            return new CanvasFact[] { canvas };
        }

        static CameraFact[] CleanCameras()
        {
            CameraFact camera = new CameraFact();
            camera.Name = "AgrCaptureUiCamera";
            camera.Mask = UiCameraMask;
            camera.Enabled = true;
            return new CameraFact[] { camera };
        }

        static BodyFact[] CleanBodies()
        {
            BodyFact ship = new BodyFact();
            ship.Name = "Ship";
            ship.Kind = "ship";
            ship.Active = true;
            ship.HasContact = true;
            BodyFact enemy = new BodyFact();
            enemy.Name = "Enemy_01";
            enemy.Kind = "enemy";
            enemy.Active = true;
            enemy.HasContact = true;
            BodyFact rock = new BodyFact();
            rock.Name = "Asteroid_Large";
            rock.Kind = "asteroid";
            rock.Active = true;
            rock.HasContact = true;
            return new BodyFact[] { ship, enemy, rock };
        }

        static string TestJson()
        {
            return "{"
                + "\"renderers\":[\"Ship*\"],"
                + "\"materials\":[\"Mat_*\"],"
                + "\"renderers.hangar\":[],"
                + "\"renderers.1\":[],"
                + "\"renderers.3\":[],"
                + "\"renderers.4\":[\"Mine*\"],"
                + "\"renderers.death\":[],"
                + "\"renderers.waveclear\":[],"
                + "\"materials.hangar\":[],"
                + "\"materials.1\":[],"
                + "\"materials.3\":[],"
                + "\"materials.4\":[],"
                + "\"materials.death\":[],"
                + "\"materials.waveclear\":[],"
                + "\"overlayExemptShaders\":[\"AGR/Post\"],"
                + "\"deadNameHints\":[\"Beam\"],"
                + "\"canvasSorts\":[0,80,120,150,160,200,250]"
                + "}";
        }

        static bool ReadStrings(string json, string key, out string[] values, out string error)
        {
            values = null;
            error = string.Empty;
            int body;
            if (!FindValue(json, key, out body, out error))
            {
                return false;
            }

            System.Collections.Generic.List<string> found = new System.Collections.Generic.List<string>();
            int end;
            if (!ReadArray(json, body, found, null, out end, out error))
            {
                return false;
            }

            values = found.ToArray();
            return true;
        }

        static bool ReadInts(string json, string key, out int[] values, out string error)
        {
            values = null;
            error = string.Empty;
            int body;
            if (!FindValue(json, key, out body, out error))
            {
                return false;
            }

            System.Collections.Generic.List<int> found = new System.Collections.Generic.List<int>();
            int end;
            if (!ReadArray(json, body, null, found, out end, out error))
            {
                return false;
            }

            values = found.ToArray();
            return true;
        }

        static bool FindValue(string json, string key, out int body, out string error)
        {
            body = -1;
            error = string.Empty;
            string token = "\"" + key + "\"";
            int at = 0;
            while (at < json.Length)
            {
                int hit = json.IndexOf(token, at, System.StringComparison.Ordinal);
                if (hit < 0)
                {
                    error = "whitelist missing " + key;
                    return false;
                }

                int look = hit + token.Length;
                while (look < json.Length && White(json[look]))
                {
                    look++;
                }

                if (look < json.Length && json[look] == ':')
                {
                    body = look + 1;
                    return true;
                }

                at = hit + token.Length;
            }

            error = "whitelist missing " + key;
            return false;
        }

        static bool ReadArray(string json, int body, System.Collections.Generic.List<string> texts, System.Collections.Generic.List<int> numbers, out int end, out string error)
        {
            end = body;
            error = string.Empty;
            int look = body;
            while (look < json.Length && White(json[look]))
            {
                look++;
            }

            if (look >= json.Length || json[look] != '[')
            {
                error = "whitelist array";
                return false;
            }

            look++;
            while (look < json.Length)
            {
                while (look < json.Length && (White(json[look]) || json[look] == ','))
                {
                    look++;
                }

                if (look < json.Length && json[look] == ']')
                {
                    end = look + 1;
                    return true;
                }

                if (texts != null)
                {
                    if (look >= json.Length || json[look] != '"')
                    {
                        error = "whitelist string";
                        return false;
                    }

                    look++;
                    int start = look;
                    while (look < json.Length && json[look] != '"')
                    {
                        look++;
                    }

                    if (look >= json.Length)
                    {
                        error = "whitelist string";
                        return false;
                    }

                    texts.Add(json.Substring(start, look - start));
                    look++;
                }
                else
                {
                    int start = look;
                    while (look < json.Length && (json[look] == '-' || (json[look] >= '0' && json[look] <= '9')))
                    {
                        look++;
                    }

                    if (start == look)
                    {
                        error = "whitelist int";
                        return false;
                    }

                    int number;
                    if (!int.TryParse(json.Substring(start, look - start), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out number))
                    {
                        error = "whitelist int";
                        return false;
                    }

                    numbers.Add(number);
                }
            }

            error = "whitelist array";
            return false;
        }

        static bool White(char value)
        {
            return value == ' ' || value == '\n' || value == '\r' || value == '\t';
        }
    }
}
