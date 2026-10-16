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
        // Built-in UI layer. Capture composites canvases with this mask only.
        public const int UiLayer = 5;
        public const int UiCameraMask = 1 << UiLayer;

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

        public static string[] Evaluate(
            string world,
            Rules rules,
            Fact[] facts,
            CanvasFact[] canvases,
            string[] uiOverlaps,
            BodyFact[] bodies,
            CameraFact[] cameras)
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
                    if (pair != null && pair.Length > 0)
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

            return hits.ToArray();
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

            return true;
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

            return Glob("Ship*", "Ship_Body") && Glob("Mat_*", "Mat_Arena") && !Glob("Ship*", "Enemy_01");
        }

        static void NoteFact(string world, Rules rules, string[] rendererPatterns, string[] materialPatterns, Fact fact, System.Collections.Generic.List<string> hits)
        {
            if (!Shown(fact))
            {
                return;
            }

            string name = Bare(fact.Name);
            string material = Bare(fact.Material);
            int queue = EffectiveQueue(fact.Queue);
            if (!MatchAny(rendererPatterns, name) || !MatchAny(materialPatterns, material))
            {
                hits.Add("unknown " + name + " material " + material + " world " + world);
            }

            if (queue < TransparentMin && fact.Emission <= DeadEmissionMax && fact.Albedo <= DeadAlbedoMax && (fact.Unlit || DeadHint(rules, name) || DeadHint(rules, material)))
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
            rules.Materials = new string[] { "Mat_Ship_*", "Mat_Enemy", "Mat_Asteroid", "Mat_Env_*", "Mat_Arena_*" };
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
