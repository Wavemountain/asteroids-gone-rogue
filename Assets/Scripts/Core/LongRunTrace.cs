#if UNITY_EDITOR
namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Null unless the editor long-run harness arms it. Player builds omit the type.
    /// </summary>
    public static class LongRunTrace
    {
        public static bool Armed;
        public static System.Action<string> Write;

        public static void Damage(
            bool applied,
            DamageCause cause,
            EnemyKind kind,
            bool elite,
            string modifier,
            int hullBefore,
            int shieldBefore,
            int hullAfter,
            int shieldAfter,
            bool invulnerable)
        {
            if (!Armed || Write == null)
            {
                return;
            }

            Write(
                "damage t=" + UnityEngine.Time.time.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                + " frame=" + UnityEngine.Time.frameCount.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + " step=" + EditorTools.LongRunDeathLog.Step.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + " wave=" + EditorTools.LongRunDeathLog.Wave.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + " applied=" + applied
                + " cause=" + cause
                + " kind=" + kind
                + " elite=" + elite
                + " modifier=" + (modifier ?? string.Empty)
                + " hull=" + hullBefore + "->" + hullAfter
                + " shield=" + shieldBefore + "->" + shieldAfter
                + " invulnerable=" + invulnerable);
        }

        public static void Life(string source, int before, int after)
        {
            if (!Armed || Write == null)
            {
                return;
            }

            Write("life source=" + source + " before=" + before + " after=" + after);
        }
    }
}
#endif
