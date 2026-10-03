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
            { "ui.new_run_reset", "Ny runda (nollställ)" },
            { "ui.one_more_try", "En gång till" },
            { "ui.skip_tutorial", "Hoppa intro" },
            { "ui.first_start", "Starta" },
            { "ui.first_diff_title", "Välj svårighet" },
            { "ui.diff.easy_line", "Färre stenar, mjukare." },
            { "ui.diff.normal_line", "Den tänkta striden." },
            { "ui.diff.hard_line", "Fler stenar, hårdare träffar." },
            { "tut.move.key", "Flyg med WASD. Undvik stenar." },
            { "tut.move.pad", "Flyg med spaken. Undvik stenar." },
            { "tut.fire.key", "Skjut med mus eller mellanslag." },
            { "tut.fire.pad", "Skjut med RT." },
            { "tut.pickup", "Ta sköldplocket." },
            { "tut.finish", "Rensa vågen." },
            { "tut.skip_hint", "Hoppa intro  ·  Esc / Start" },
            { "run.first_upgrade", "Lägg kredit på din första uppgradering" },
            { "ui.continue_world", "Fortsätt till värld {0}" },
            { "ui.run_over", "Rundan är slut" },
            { "ui.abort", "Avbryt > Hangar" },
            { "ui.credits", "Medverkande" },
            { "ui.continue", "Fortsätt" },
            { "ui.mute", "Tyst" },
            { "ui.unmute", "Ljud på" },
            { "ui.sfx", "SFX" },
            { "ui.music", "Musik" },
            { "ui.settings", "Inställningar" },
            { "ui.settings.language", "Språk" },
            { "ui.settings.controls", "Kontroller" },
            { "ui.settings.close", "Stäng" },
            { "ui.settings.en", "EN" },
            { "ui.settings.sv", "SV" },
            { "ui.settings.play", "Spel" },
            { "ui.settings.hangar", "Hangar" },
            { "ui.settings.on", "På" },
            { "ui.settings.off", "Av" },
            { "ui.settings.shake", "Skärmskak" },
            { "ui.settings.assist", "Assistläge: extra sköld, mindre fiendeskada" },
            { "ui.hud.assist", "Assist" },
            { "ui.settings.hint", "Tipsrad" },
            { "ui.settings.hint.hangar", "Bara hangaren" },
            { "ui.settings.hint.panel", "Bara inställningar" },
            { "ui.settings.hint_size", "Tipsstorlek" },
            { "ui.settings.hint.px", "{0}" },
            { "ui.settings.confirm_abort", "Bekräfta avbrott (Esc/Start) under våg" },
            { "ui.settings.confirm_new_run", "Bekräfta ny runda" },
            { "ui.confirm.abort_title", "Avbryt vågen?" },
            { "ui.confirm.abort_body", "Tillbaka till hangaren?" },
            { "ui.confirm.new_run_title", "Ny runda?" },
            { "ui.confirm.new_run_body", "Börja om från våg 1? Våg {0}, poäng {1} och kredit {2} försvinner." },
            { "ui.confirm.yes", "Ja" },
            { "ui.confirm.no", "Nej" },
            { "ui.settings.pad_nav", "Padnavigering" },
            { "ui.settings.pad.dpad", "D-pad" },
            { "ui.settings.pad.analog", "Analog" },
            { "ui.settings.pad.both", "Båda" },
            { "ui.hint_footer", "A Välj · Start Starta våg · Select = Inställningar" },
            { "ach.compact", "\u2022 {0}/{1}" },
            { "ui.difficulty", "SVÅRIGHET" },
            { "ui.diff.easy", "Lätt" },
            { "ui.diff.normal", "Normal" },
            { "ui.diff.hard", "Svår" },
            { "ui.lives", "LIV" },
            { "ui.hud_lives", "Liv {0} / {1}" },
            { "ui.life_lost", "LIV FÖRLORAT  ·  {0} kvar" },
            { "ui.first_flight", "Första flygningen" },
            { "ui.got_it", "Uppfattat" },
            { "ui.owned", "KÖPT" },
            { "ui.locked", "LÅST" },
            { "ui.need_cr", "behöver {0} kr" },
            { "ui.cost_cr", "{0} kr" },
            { "ui.credits_line", "Kredit: {0}" },
            { "ui.hint_play", "WASD/LS styr · Mus/RS sikte · VMB/RT skjut · E/LT verktyg · Q/LB cykla · Esc/Start = tillbaka till hangaren" },
            { "ui.hint_hangar", "LS styr · {0}" },
            { "ui.hangar_controls", "LT verktyg · LB cykla · A bekräfta · B / Esc nästa våg · Start starta våg" },
            { "ui.hangar_hint_body", "LS / WASD flyg  ·  RT / VMB skjut\nStart = starta våg  ·  B / Esc = fokusera Nästa våg" },
            { "ui.first_wave_coach", "Skjut stenar  ·  Esc / Start återvänder till hangaren" },
            { "ui.hangar_clear_wave", "Hangar  ·  Rensa en våg för kredit och uppgraderingar." },
            { "ui.hangar_wave_line", "Hangar  ·  Våg {0}  ·  Värld {1} layout: {2}" },
            { "ui.medals", "MEDALJER" },
            { "ui.world_badge", "VÄRLD {0}  ·  {1}" },
            { "ui.layout_swap", "LAYOUTBYTE\nVÄRLD {0}  ONLINE" },
            { "ui.hud_wave_score", "Våg {0}   ·   Poäng {1}" },
            { "ui.health", "HÄLSA" },
            { "ui.hull_label", "SKROV" },
            { "ui.shield_label", "SKÖLD" },
            { "ui.ship_preview", "UTRUSTNING" },
            { "ui.hud_hull", "Skrov {0}   ·   Sköld {1}" },
            { "ui.hud_remaining", "   ·   Kvar {0}" },
            { "ui.hud_fire", "\nEld {0}" },
            { "ui.hud_primary", "\nPRIMÄR {0}" },
            { "ui.hud_utility", "\nVERKTYG {0}" },
            { "ui.hud_dash", "—" },
            { "ui.hud_empty", "tom" },
            { "ui.slot_primary", "PRIMÄR" },
            { "ui.slot_utility", "VERKTYG" },
            { "ui.loadout_slots", "PRIMÄR {0}  ·  VERKTYG {1}" },
            { "ui.new_best", "NYTT REKORD" },
            { "ui.new_best_dot", "  ·  NYTT REKORD" },
            { "session.empty", "Session —" },
            { "session.card", "Session {0}  ·  Våg {1}" },
            { "session.score", "Session {0}" },
            { "session.death", "Denna runda {0}  ·  {1}" },
            { "session.slash", " / Sess {0}" },
            { "ach.first", "Första rensning" },
            { "ach.nohit", "Orörd våg" },
            { "ach.hard", "Svår rensning" },
            { "ach.streak", "Extrliv-svit" },
            { "ach.unlock", "PRESTATION  ·  {0}" },
            { "ach.header", "PRESTATIONER" },
            { "run.sector_clear", "SEKTOR RENAD  ·  VÄRLD {0}" },
            { "run.fail_retry", "En gång till behåller rundan." },
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
            { "shop.desc.SpreadBolt", "PRIMÄR: 3 bärnstenshagel. LB / Q cyklar. Skilt från cyan pierce." },
            { "shop.title.Pierce", "Pierce" },
            { "shop.desc.Pierce", "PRIMÄR: bulten går genom mål. LB / Q cyklar." },
            { "shop.title.TwinGuns", "Tvillingkanoner" },
            { "shop.desc.TwinGuns", "PRIMÄR: två parallella fullskada-bultar. Inte en spridsol." },
            { "shop.title.Seeker", "Sökare" },
            { "shop.desc.Seeker", "VERKTYG: magenta-robot. Håll LT / E. Egen cooldown. Svagare styrning, långsammare takt, lägre skada." },
            { "shop.title.Ricochet", "Rikoschett" },
            { "shop.desc.Ricochet", "VERKTYG: limebult, 2 studsar på arenakanten. Håll LT / E. Egen cooldown." },
            { "shop.title.ShieldCell", "Sköldcell" },
            { "shop.desc.ShieldCell", "En synlig sköldträff före skrovskada (max 2, eller 3 med Matris)." },
            { "shop.title.ShieldMatrix", "Sköldmatris" },
            { "shop.desc.ShieldMatrix", "Kräver två Sköldceller. Höjer sköldtaket till 3." },
            { "shop.title.Rail", "Räl" },
            { "shop.desc.Rail", "Lans-mitt. PRIMÄR: håll RT 0.55s, släpp. Skada ×3, fart ×1.2, ingen pierce. CD ×1.6. Miss eller avbryt ger halv cooldown." },
            { "shop.title.FlakFeed", "Flakmatning" },
            { "shop.desc.FlakFeed", "Salva-mitt. Spread-cooldown ×0.85 och halvvinkel +4°. Mjuklåser andra vägar." },
            { "shop.title.Storm", "Storm" },
            { "shop.desc.Storm", "Salva-topp. Spread skjuter 5 hagel. Cooldown ×1.8, minst 1.5s." },
            { "shop.title.OverchargeLance", "Överladdad lans" },
            { "shop.desc.OverchargeLance", "Lans-topp. Pierce träffar +1 mål. Tvilling-cooldown ×0.9." },
            { "shop.title.SeekerCadence", "Sökartakt" },
            { "shop.desc.SeekerCadence", "Jägare-mitt. Verktygs-cooldown ×0.75. Sökare svänger 165. Mjuklåser andra vägar." },
            { "shop.title.TwinSeek", "Tvilling-sök" },
            { "shop.desc.TwinSeek", "Jägare-topp. Håll LT för 2 sökare på 70% skada. Verktygs-cooldown ×1.2." },
            { "ui.doctrine.tip", "Välj en doktrin." },
            { "ui.doctrine.barrage", "Salva" },
            { "ui.doctrine.lance", "Lans" },
            { "ui.doctrine.hunter", "Jägare" },
            { "ui.doctrine.none", "—" },
            { "ui.doctrine.pick", "Välj" },
            { "ui.doctrine.swap", "Ny runda byter" },
            { "ui.doctrine.path.barrage", "Breda skott\nSprid>Flak>Storm" },
            { "ui.doctrine.path.lance", "Tung linje\nTvilling>Räl>Lans" },
            { "ui.doctrine.path.hunter", "Målsök\nSök>Takt>Tvilling" },
            { "ui.doctrine.need", "Behöver {0}" },
            { "ui.doctrine.need_either", "Behöver {0}/{1}" },
            { "ui.doctrine.hint_title", "Doktriner" },
            { "ui.doctrine.hint_body", "Doktriner är öppna. Välj Salva, Lans eller Jägare." },
            { "ui.off_path", "av vägen" },
            { "ui.hud_doctrine", "DOKTRIN  ·  {0}" },
            { "ui.hint_rail", "Håll RT 0.55s, släpp — Räl. Miss eller avbryt ger halv cooldown." },
            { "ui.hint_dual", "LT verktyg · LB cykla primär · RT skjut" },
            { "ach.doctrine", "Doktrin" },
            { "ach.rail", "Rälladdning" },
            { "ach.storm", "Storm" },
            { "ach.overcharge", "Överladdad lans" },
            { "ach.twinseek", "Tvilling-sök" },
            { "ach.deep", "Djup omloppsbana" },
            { "ach.far", "Fjärrdrift" },
            { "up.Rail", "Räl" },
            { "up.FlakFeed", "Flakmatning" },
            { "up.Storm", "Storm" },
            { "up.Overcharge", "Överladdad lans" },
            { "up.Cadence", "Sökartakt" },
            { "up.TwinSeek", "Tvilling-sök" },
            { "mode.Rail", "Räl" },
            { "run.ship_lost", "SKEPP FÖRLORAT" },
            { "run.ship_lost_reason", "SKEPP FÖRLORAT  ·  {0}" },
            { "run.wave_clear", "VÅG KLAR" },
            { "run.run", "RUNDA" },
            { "run.stats", "Poäng {0}  ·  Våg {1}  ·  Värld {2}" },
            { "run.doctrine_wave", "{0}-runda — våg {1}" },
            { "run.credits_plus", "Kredit {0}  (+{1})" },
            { "run.credits", "Kredit {0}" },
            { "run.upgrades_none", "Uppgraderingar —" },
            { "run.upgrades", "Uppgraderingar  {0}" },
            { "run.upgrades_more", "+{0} fler" },
            { "run.fail_keep", "Ditt skrov. Runnen är över — börja från hangaren." },
            { "run.tease_brute", "Se upp  ·  Våg 5 Brute — sidosteg för stormningen" },
            { "run.tease_swarm", "Se upp  ·  Våg 6 Svärm — slå sönder boet" },
            { "run.tease_both", "Se upp  ·  Brute stormar  ·  Svärmbon släpper svärmungar" },
            { "run.buy_gunner", "Köp {0} före Skytt" },
            { "run.push_gunner", "Jaga ett nytt rekord före Skytt" },
            { "run.buy_seeker_lt", "Köp Sökare > håll LT" },
            { "run.hold_lt", "Håll LT för verktyg" },
            { "run.buy", "Köp {0}" },
            { "run.push_best", "Jaga ett nytt rekord." },
            { "run.deep_orbit_now", "Värld 2  ·  \u2022 {0}" },
            { "run.deep_orbit_at", "\u2022 {0} på våg {1}" },
            { "run.far_drift_clear", "Rensa våg 10  ·  \u2022 {0}" },
            { "run.far_drift_at", "\u2022 {0} på våg {1}" },
            { "run.world2_at", "Värld 2 på våg {0}" },
            { "run.scout_chip", "{0}  ·  våg {1}" },
            { "run.next_medal", "Nästa  ·  \u2022 {0} på våg {1}" },
            { "run.star_at", "\u2022 {0} på våg {1}" },
            { "run.next_world3", "Nästa  ·  Värld 3 på våg {0}" },
            { "run.next_sector", "Nästa  ·  Värld 2 efter våg {0}" },
            { "run.over_title", "SLUT - våg {0}" },
            { "run.over_explain", "En gång till behåller skeppet. Ny runda nollställer." },
            { "run.sector_world", "SEKTOR KLAR - Värld {0}" },
            { "run.world_cleared", "Klar: {0}" },
            { "run.reached", "Nådde värld {0}, våg {1}" },
            { "world.launch", "Utskjutningsbältet" },
            { "world.deep", "Djup omloppsbana" },
            { "world.far", "Fjärrdrift" },
            { "world.mines", "Minfält" },
            { "world.cross", "Korsportar" },
            { "world.islands", "Skrotöar" },
            { "world.spokes", "Ekring" },
            { "world.banner", "Värld {0} - {1}" },
            { "world.loop", "Varv {0}" },
            { "world.next", "Nästa: Värld {0} - {1}" },
            { "world.tip.1", "Öppen bana. Lär dig kanten." },
            { "world.tip.2", "Pyloner biter. Håll fart." },
            { "world.tip.3", "Håll dig ur graven." },
            { "world.tip.4", "Minor i bältet. Väv igenom." },
            { "world.tip.5", "Korseld. Glid genom portarna." },
            { "world.tip.6", "Öar blockar skott. Använd gliporna." },
            { "world.tip.7", "Ekrar skär ringen. Håll dig från linjerna." },
            { "run.gunner_wave4", "Skytt på våg 4" },
            { "run.before_gunner", "före Skytt" },
            { "run.world3_online", "Värld 3 online  ·  {0}" },
            { "fail.asteroid", "Asteroidkollision" },
            { "fail.enemy", "Fiendekontakt" },
            { "fail.enemy_kind", "Fiendekontakt ({0})" },
            { "fail.hazard", "Arenafara" },
            { "fail.unknown", "Okänd orsak" },
            { "fail.bolt", "Fiendebult" },
            { "fail.boss_bolt", "Bossväktarens bult" },
            { "fail.killed", "Dödad av: {0} (våg {1}, värld {2}) — skrov {3}" },
            { "fail.last_hits", "Senaste träffar: {0}" },
            { "fail.src.asteroid", "Asteroid" },
            { "fail.src.spike", "Spik" },
            { "fail.src.boss_bolt", "Bossväktarens bult" },
            { "fail.src.bolt", "{0}-bult" },
            { "fail.src.charge", "{0}-rusning" },
            { "fail.src.elite", "{0}  ·  {1}" },
            { "fail.fault", "{0} — det var du. En gång till, eller Ny runda separat." },
            { "fail.almost_one", "En kvar. Nästan!" },
            { "fail.almost_n", "Nästan — {0} kvar." },
            { "fail.left_n", "{0} kvar." },
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
            { "credits.body", "Ljud\nKenney.nl + yd\n"
                + "SFX 0.47 Kenney CC0: sköld, pansar, plock, tells, agr_ricochet, svärmdöd.\n\n"
                + "Musik\nJuhani Junkala, Kenney, MintoDog, HydroGene (CC0)\n\n"
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
            { "enemy.Swarmling", "Svärmunge" },
            { "ui.continue_run", "Fortsätt V{0} våg {1}" },
            { "ui.new_run", "Ny runda" },
            { "ui.legacy.line", "Arv {0}  ·  Bäst {1}  ·  Våg {2}  ·  Värld {3}" },
            { "ui.legacy.line_empty", "Arv {0}  ·  Bäst —" },
            { "ui.legacy.credits", "+{0} kr" },
            { "ui.legacy.discount", "Första -{0}%" },
            { "ui.legacy.shield", "Sköld +{0}" },
            { "ui.legacy.hull", "Skrov +{0}" },
            { "ui.legacy.shield_dur", "  ·  {0}% tid" },
            { "ui.legacy.hull_regen", "  ·  +{0}% lagning" },
            { "ui.legacy.max", "MAX" },
            { "ui.mk2", "Mk II" },
            { "shop.hull_repair", "Skrovreparation" },
            { "shop.extra_life", "Extraliv" },
            { "shop.shield_refill", "Sköldpåfyllning" },
            { "shop.bank", "Bankera kredit" },
            { "wave.elite.banner", "ELITVÅG - {0}" },
            { "wave.mod.faster", "Snabbare fiender" },
            { "wave.mod.shielded", "Sköldade asteroider" },
            { "wave.mod.dense", "Tät svärm" },
            { "ui.boss", "VÄRLDSVÄKTARE" },
            { "world.rule", "Världsregel: {0}" },
            { "world.rule.1", "Extra plock" },
            { "world.rule.2", "Snabbare asteroider" },
            { "world.rule.3", "Snabbare fiendeeld" },
            { "world.rule.4", "Tätare skrot" },
            { "world.rule.5", "Färre plock" },
            { "world.rule.6", "Dämpad sikt" },
            { "world.rule.7", "Tung fiendeeld" },
            { "boon.fire", "Eldhastighet" },
            { "boon.fire.fx", "+8% eldhastighet" },
            { "boon.fire.tag", "Eld" },
            { "boon.move", "Fart" },
            { "boon.move.fx", "+10% fart" },
            { "boon.move.tag", "Fart" },
            { "boon.hull", "Maxskrov" },
            { "boon.hull.fx", "+1 maxskrov" },
            { "boon.hull.tag", "Skro" },
            { "boon.credits", "Vågkredit" },
            { "boon.credits.fx", "+15% vågkredit" },
            { "boon.credits.tag", "Kred" },
            { "boon.rail", "Rälsladdning" },
            { "boon.rail.fx", "+15% räls" },
            { "boon.rail.tag", "Rals" },
            { "boon.shard", "Splitter" },
            { "boon.shard.fx", "+1 extra splitter" },
            { "boon.shard.tag", "Splt" },
            { "boon.resist", "Mindre skada" },
            { "boon.resist.fx", "10% chans att ignorera en träff" },
            { "boon.resist.tag", "Skad" },
            { "boon.shield", "Vågsköld" },
            { "boon.shield.fx", "+1 sköld per våg" },
            { "boon.shield.tag", "Skld" },
            { "boon.utility", "Verktygstid" },
            { "boon.utility.fx", "-10% verktyg" },
            { "boon.utility.tag", "Verk" },
            { "boon.stack", "{0} x{1}" },
            { "boon.level", "Nv {0}" },
            { "boon.row", "Bonusar  {0}" },
            { "boon.pick", "Välj 1 bonus" },
            { "boon.pad", "A välj  ·  D-pad eller spak" }
        };

        /// <summary>
        /// English strings for table keys that have no Loc.T call site.
        /// Dynamic shop, enemy, and mode keys take their English from the call-site fallback.
        /// </summary>
        private static readonly Dictionary<string, string> English = new Dictionary<string, string>
        {
            { "ui.retry_wave", "Retry Wave" },
            { "ui.credits_line", "Credits: {0}" },
            { "ui.hud_fire", "\nFire {0}" },
            { "ui.slot_primary", "PRIMARY" },
            { "ui.unmute", "Unmute" },
            { "ui.locked", "LOCKED" },
            { "ui.mk2", "Mk II" },
            { "ui.off_path", "off-path" },
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

        public static bool HasEnglish(string key)
        {
            return English.ContainsKey(key);
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
