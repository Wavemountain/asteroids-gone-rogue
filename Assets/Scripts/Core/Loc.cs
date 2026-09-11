using System.Collections.Generic;
using UnityEngine;

namespace AsteroidsGoneRogue
{
    public enum GameLanguage
    {
        English,
        Swedish
    }

    /// <summary>
    /// Hangar EN/SV copy. English fallbacks stay at call sites so Week 1 tests
    /// still lock the source strings; Swedish lives here.
    /// </summary>
    public static class Loc
    {
        public const string PrefsKey = "agr.ui.language";
        public const string EnglishCode = "en";
        public const string SwedishCode = "sv";

        private static readonly Dictionary<string, string> Swedish = new Dictionary<string, string>
        {
            { "ui.start_wave", "Starta våg" },
            { "ui.next_wave", "Nästa våg" },
            { "ui.retry_wave", "Försök igen" },
            { "ui.abort", "Avbryt → Hangar" },
            { "ui.credits", "Medverkande" },
            { "ui.continue", "Fortsätt" },
            { "ui.mute", "Tyst" },
            { "ui.unmute", "Ljud på" },
            { "ui.sfx", "SFX" },
            { "ui.music", "Musik" },
            { "ui.lang", "SPRÅK" },
            { "ui.first_flight", "Första flygningen" },
            { "ui.got_it", "Uppfattat" },
            { "ui.owned", "KÖPT" },
            { "ui.locked", "LÅST" },
            { "ui.need_cr", "behöver {0} kr" },
            { "ui.cost_cr", "{0} kr" },
            { "ui.credits_line", "Kredit: {0}" },
            { "ui.hint_play", "WASD styr  ·  Mus sikte  ·  VMB / Mellanslag skjut  ·  Q / HMB eldläge  ·  Esc avbryt" },
            { "ui.hint_hangar", "WASD styr  ·  Mus sikte  ·  VMB / Mellanslag skjut  ·  {0}" },
            { "ui.hangar_controls", "Avbryt (Esc)  ·  Q / HMB eldlägen (upptäck Spread / Pierce när du äger dem)" },
            { "ui.hangar_hint_body", "WASD styr · mus sikte\nVMB / Mellanslag skjut\nAvbryt (Esc) lämnar vågen\n"
                + "Q / HMB eldlägen\n(upptäck Spread / Pierce när du äger dem)\n\n"
                + "Rensa en våg för kredit och uppgraderingar.\n"
                + "Medaljstege (uppe till vänster): ★ Spejarvinge på våg 3.\n\n"
                + "Shoppen köper uppgraderingar för den krediten.\nStarta våg för att flyga." },
            { "ui.hangar_clear_wave", "Hangar  ·  Rensa en våg för kredit och uppgraderingar." },
            { "ui.hangar_wave_line", "Hangar  ·  Våg {0}  ·  Värld {1} layout: {2}" },
            { "ui.medals", "MEDALJER" },
            { "ui.world_badge", "VÄRLD {0}  ·  {1}" },
            { "ui.layout_swap", "LAYOUTBYTE\nVÄRLD {0}  ONLINE" },
            { "ui.hud_wave_score", "Våg {0}   ·   Poäng {1}" },
            { "ui.health", "HÄLSA" },
            { "ui.hull_label", "SKROV" },
            { "ui.shield_label", "SKÖLD" },
            { "ui.hud_hull", "Skrov {0}   ·   Sköld {1}" },
            { "ui.hud_remaining", "   ·   Kvar {0}" },
            { "ui.hud_fire", "\nEld {0}" },
            { "ui.new_best", "NYTT REKORD" },
            { "ui.new_best_dot", "  ·  NYTT REKORD" },
            { "shop.header.hull", "SKROV / NOS / MOTOR" },
            { "shop.header.weapons", "VAPEN" },
            { "shop.header.defense", "FÖRSVAR" },
            { "shop.title.BodyUpgrade01", "Skrovbyte" },
            { "shop.desc.BodyUpgrade01", "Byter skrovet till Ship_Body_Upgrade01 och ger +1 skrovträff." },
            { "shop.title.BodyUpgrade02", "Skrovplatta 02" },
            { "shop.desc.BodyUpgrade02", "Kräver Skrovbyte. Extra skrovplatta (5 träffar). Återanvänder Upgrade01-mesh." },
            { "shop.title.NoseHardpoint", "Noshårdpunkt" },
            { "shop.desc.NoseHardpoint", "Byter nossleckan mot snabbare, hårdare skott." },
            { "shop.title.NoseUpgrade02", "Nos 02" },
            { "shop.desc.NoseUpgrade02", "Kräver Noshårdpunkt. Byter till Ship_Nose_Upgrade02 (3 skada)." },
            { "shop.title.NoseUpgrade03", "Nos 03" },
            { "shop.desc.NoseUpgrade03", "Kräver Nos 02. 4-skada skott. Återanvänder Nos 02-mesh." },
            { "shop.title.RapidFire", "Snabbeld" },
            { "shop.desc.RapidFire", "Halverar nästan kanonens cooldown och byter motorsleckan." },
            { "shop.title.EngineUpgrade02", "Motor 02" },
            { "shop.desc.EngineUpgrade02", "Kräver Snabbeld. Byter till Ship_Engine_Upgrade02 (snabbare kanon)." },
            { "shop.title.EngineUpgrade03", "Motor 03" },
            { "shop.desc.EngineUpgrade03", "Kräver Motor 02. Snabbare kanon. Återanvänder Motor 02-mesh." },
            { "shop.title.Overcharger", "Överladdare" },
            { "shop.desc.Overcharger", "Nosgren. +1 skada, lite långsammare kanon. Låser Efterbrännare." },
            { "shop.title.Afterburner", "Efterbrännare" },
            { "shop.desc.Afterburner", "Motorgren. Snabbaste kanonen. Låser Överladdare." },
            { "shop.title.SpreadBolt", "Spridbult" },
            { "shop.desc.SpreadBolt", "Eldläge: 3 bärnstenshagel. Q / HMB för att byta. Skilt från cyan pierce." },
            { "shop.title.Pierce", "Pierce" },
            { "shop.desc.Pierce", "Eldläge: bulten går genom mål. Q / HMB för att byta." },
            { "shop.title.TwinGuns", "Tvillingkanoner" },
            { "shop.desc.TwinGuns", "Eldläge: två parallella fullskada-bultar. Inte en spridsol." },
            { "shop.title.Seeker", "Sökare" },
            { "shop.desc.Seeker", "Eldläge: magenta-robot som styr mot närmaste hot. Långsammare takt än bult." },
            { "shop.title.Ricochet", "Rikoschett" },
            { "shop.desc.Ricochet", "Eldläge: limebult som studsar på arenakanten (inte pierce)." },
            { "shop.title.ShieldCell", "Sköldcell" },
            { "shop.desc.ShieldCell", "En synlig sköldträff före skrovskada (max 2, eller 3 med Matris)." },
            { "shop.title.ShieldMatrix", "Sköldmatris" },
            { "shop.desc.ShieldMatrix", "Kräver två Sköldceller. Höjer sköldtaket till 3." },
            { "run.ship_lost", "SKEPP FÖRLORAT" },
            { "run.ship_lost_reason", "SKEPP FÖRLORAT  ·  {0}" },
            { "run.wave_clear", "VÅG KLAR" },
            { "run.run", "RUNDA" },
            { "run.stats", "Poäng {0}  ·  Våg {1}  ·  Värld {2}" },
            { "run.credits_plus", "Kredit {0}  (+{1})" },
            { "run.credits", "Kredit {0}" },
            { "run.upgrades_none", "Uppgraderingar —" },
            { "run.upgrades", "Uppgraderingar  {0}" },
            { "run.fail_keep", "Ditt skrov. Kredit och uppgraderingar stannar — Försök igen." },
            { "run.tease_brute", "Se upp  ·  Våg 5 Brute — sidosteg för stormningen" },
            { "run.tease_swarm", "Se upp  ·  Våg 6 Svärm — slå sönder boet" },
            { "run.tease_both", "Se upp  ·  Brute stormar  ·  Svärmbon släpper svärmungar" },
            { "run.buy_gunner", "Köp {0} före Skytt" },
            { "run.push_gunner", "Jaga ett nytt rekord före Skytt" },
            { "run.buy", "Köp {0}" },
            { "run.push_best", "Jaga ett nytt rekord." },
            { "run.deep_orbit_now", "Värld 2  ·  ★ {0}" },
            { "run.deep_orbit_at", "★ {0} på våg {1}" },
            { "run.far_drift_clear", "Rensa våg 10  ·  ★ {0}" },
            { "run.far_drift_at", "★ {0} på våg {1}" },
            { "run.world2_at", "Värld 2 på våg {0}" },
            { "run.world3_at", "Värld 3 på våg {0}" },
            { "run.next_medal", "Nästa  ·  ★ {0} på våg {1}" },
            { "run.star_at", "★ {0} på våg {1}" },
            { "run.next_world3", "Nästa  ·  Värld 3 på våg {0}" },
            { "run.gunner_wave4", "Skytt på våg 4" },
            { "run.before_gunner", "före Skytt" },
            { "run.world3_online", "Värld 3 online  ·  {0}" },
            { "fail.asteroid", "Asteroidkollision" },
            { "fail.enemy", "Fiendekontakt" },
            { "fail.enemy_kind", "Fiendekontakt ({0})" },
            { "fail.hazard", "Arenafara" },
            { "fail.unknown", "Okänd orsak" },
            { "fail.fault", "{0} — det var du. Lasten stannar på Försök igen." },
            { "medal.scout", "Spejarvinge" },
            { "medal.deep", "Djup omloppsbana" },
            { "medal.far", "Fjärrdrift" },
            { "medal.world3", "Ny sektor" },
            { "layout.open", "Öppen" },
            { "layout.pylon", "Pylonring" },
            { "layout.trench", "Delad grav" },
            { "layout.mines", "Minbälte" },
            { "layout.cross", "Korsportar" },
            { "layout.islands", "Skrotöar" },
            { "layout.spokes", "Ekring" },
            { "badge.open", "ÖPPEN" },
            { "badge.pylons", "PYLONER" },
            { "badge.trench", "GRAV" },
            { "badge.mines", "MINOR" },
            { "badge.cross", "KORS" },
            { "badge.islands", "ÖAR" },
            { "badge.spokes", "EKRAR" },
            { "best.empty", "Bäst —" },
            { "best.card", "Bäst {0}  ·  Våg {1}  ·  Värld {2}" },
            { "best.slash", " / Bäst {0}" },
            { "credits.body", "Ljud\nKenney.nl + yd\n\n"
                + "Typsnitt\nKenney Future\n\n"
                + "Team\nSpelPM / GameBot / BlenderBot / AtmosBot / Speltest" },
            { "up.Body", "Skrov" },
            { "up.Hull02", "Skrov 02" },
            { "up.Nose", "Nos" },
            { "up.Nose02", "Nos 02" },
            { "up.Nose03", "Nos 03" },
            { "up.Rapid", "Snabbeld" },
            { "up.Engine02", "Motor 02" },
            { "up.Engine03", "Motor 03" },
            { "up.Overcharger", "Överladdare" },
            { "up.Afterburner", "Efterbrännare" },
            { "up.Spread", "Spread" },
            { "up.Pierce", "Pierce" },
            { "up.Twin", "Tvilling" },
            { "up.Seeker", "Sökare" },
            { "up.Ricochet", "Rikoschett" },
            { "up.Shield", "Sköld x{0}" },
            { "up.Matrix", "Matris" },
            { "mode.Bolt", "Bult" },
            { "mode.Spread", "Spread" },
            { "mode.Twin", "Tvilling" },
            { "mode.Pierce", "Pierce" },
            { "mode.Seeker", "Sökare" },
            { "mode.Ricochet", "Rikoschett" },
            { "enemy.Mid01", "Mitt" },
            { "enemy.Scout", "Spejare" },
            { "enemy.Gunner", "Skytt" },
            { "enemy.Drone", "Drönare" },
            { "enemy.Bomber", "Bombare" },
            { "enemy.Sniper", "Prickskytt" },
            { "enemy.SwarmPod", "Svärmbalja" },
            { "enemy.Brute", "Brute" },
            { "enemy.Swarm", "Svärm" },
            { "enemy.Swarmling", "Svärmunge" }
        };

        private static bool _loaded;
        private static GameLanguage _language = GameLanguage.English;

        public static GameLanguage Language
        {
            get
            {
                EnsureLoaded();
                return _language;
            }
        }

        public static void EnsureLoaded()
        {
            if (_loaded)
            {
                return;
            }

            ApplyCode(PlayerPrefs.GetString(PrefsKey, EnglishCode), false);
            _loaded = true;
        }

        public static void SetLanguage(GameLanguage language)
        {
            _language = language;
            _loaded = true;
            PlayerPrefs.SetString(PrefsKey, language == GameLanguage.Swedish ? SwedishCode : EnglishCode);
            PlayerPrefs.Save();
        }

        public static bool IsSwedish
        {
            get { return Language == GameLanguage.Swedish; }
        }

        public static string T(string key, string english)
        {
            EnsureLoaded();
            string swedish;
            if (_language == GameLanguage.Swedish && Swedish.TryGetValue(key, out swedish))
            {
                return swedish;
            }

            return english;
        }

        public static string Tf(string key, string englishFormat, params object[] args)
        {
            return string.Format(T(key, englishFormat), args);
        }

        public static string Code
        {
            get { return Language == GameLanguage.Swedish ? SwedishCode : EnglishCode; }
        }

        public static bool HasSwedish(string key)
        {
            return Swedish.ContainsKey(key);
        }

        public static string FireModeName(FireMode mode)
        {
            return T("mode." + mode, mode.ToString());
        }

        private static void ApplyCode(string code, bool save)
        {
            _language = code == SwedishCode ? GameLanguage.Swedish : GameLanguage.English;
            if (save)
            {
                PlayerPrefs.SetString(PrefsKey, Code);
                PlayerPrefs.Save();
            }
        }
    }
}
