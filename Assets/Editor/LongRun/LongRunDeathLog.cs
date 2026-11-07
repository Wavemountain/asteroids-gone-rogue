using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AsteroidsGoneRogue.EditorTools
{
    /// <summary>
    /// Fixed 2 s ring, only while the long-run harness is armed. Not in player builds.
    /// </summary>
    public sealed class LongRunDeathLog : MonoBehaviour
    {
        public const int Capacity = 100;
        public const int ThreatCap = 8;
        public const float WindowSeconds = 2f;

        struct Threat
        {
            public string Name;
            public float X;
            public float Z;
            public float Vx;
            public float Vz;
            public float Radius;
        }

        struct Sample
        {
            public float Time;
            public int Step;
            public float ChosenX;
            public float ChosenY;
            public float ChosenAimX;
            public float ChosenAimY;
            public int ChosenFire;
            public float UsedX;
            public float UsedY;
            public float UsedAimX;
            public float UsedAimY;
            public int UsedFire;
            public float Px;
            public float Pz;
            public float Vx;
            public float Vz;
            public float Radius;
            public int Threats;
            public Threat T0;
            public Threat T1;
            public Threat T2;
            public Threat T3;
            public Threat T4;
            public Threat T5;
            public Threat T6;
            public Threat T7;
        }

        static readonly Sample[] Ring = new Sample[Capacity];
        static readonly Threat[] Near = new Threat[ThreatCap];
        static int _count;
        static int _cursor;
        static int _step;
        static int _deaths;
        static int _lives = -1;
        static int _wave = -1;
        static float _chosenX;
        static float _chosenY;
        static float _chosenAimX;
        static float _chosenAimY;
        static int _chosenFire;

        public static void NoteChosen(float moveX, float moveY, float aimX, float aimY, bool fire)
        {
            _chosenX = moveX;
            _chosenY = moveY;
            _chosenAimX = aimX;
            _chosenAimY = aimY;
            _chosenFire = fire ? 1 : 0;
        }

        public static void Append(string line)
        {
            string dir = SessionState.GetString("LongRun.Out", string.Empty);
            if (dir.Length == 0)
            {
                return;
            }

            Directory.CreateDirectory(dir);
            File.AppendAllText(Path.Combine(dir, "damage.log"), line + "\n");
        }

        void FixedUpdate()
        {
            if (!LongRunTrace.Armed)
            {
                return;
            }

            GameManager game = GameManager.Live;
            GameSession session = game != null ? game.Session : null;
            ShipController ship = UnityEngine.Object.FindAnyObjectByType<ShipController>();
            Push(ship);
            if (session == null)
            {
                return;
            }

            if (session.Phase == GamePhase.Playing && session.WaveIndex != _wave)
            {
                _wave = session.WaveIndex;
                WriteWave(session, ship);
            }

            if (_lives >= 0 && session.Lives < _lives)
            {
                Dump(session, _lives, session.Lives);
            }

            _lives = session.Lives;
        }

        static void Push(ShipController ship)
        {
            Sample sample = new Sample();
            sample.Time = Time.realtimeSinceStartup;
            sample.Step = _step;
            _step += 1;
            sample.ChosenX = _chosenX;
            sample.ChosenY = _chosenY;
            sample.ChosenAimX = _chosenAimX;
            sample.ChosenAimY = _chosenAimY;
            sample.ChosenFire = _chosenFire;
            Vector2 usedMove = GamepadInput.MoveStick();
            Vector2 usedAim = GamepadInput.AimStick();
            sample.UsedX = usedMove.x;
            sample.UsedY = usedMove.y;
            sample.UsedAimX = usedAim.x;
            sample.UsedAimY = usedAim.y;
            sample.UsedFire = GamepadInput.FireHeld() ? 1 : 0;
            if (ship != null)
            {
                sample.Px = ship.transform.position.x;
                sample.Pz = ship.transform.position.z;
                Rigidbody body = ship.GetComponent<Rigidbody>();
                if (body != null)
                {
                    sample.Vx = body.linearVelocity.x;
                    sample.Vz = body.linearVelocity.z;
                }

                SphereCollider sphere = ship.GetComponent<SphereCollider>();
                if (sphere != null)
                {
                    sample.Radius = sphere.radius;
                }
            }

            sample.Threats = Collect(sample.Px, sample.Pz);
            if (sample.Threats > 0)
            {
                sample.T0 = Near[0];
            }

            if (sample.Threats > 1)
            {
                sample.T1 = Near[1];
            }

            if (sample.Threats > 2)
            {
                sample.T2 = Near[2];
            }

            if (sample.Threats > 3)
            {
                sample.T3 = Near[3];
            }

            if (sample.Threats > 4)
            {
                sample.T4 = Near[4];
            }

            if (sample.Threats > 5)
            {
                sample.T5 = Near[5];
            }

            if (sample.Threats > 6)
            {
                sample.T6 = Near[6];
            }

            if (sample.Threats > 7)
            {
                sample.T7 = Near[7];
            }

            Ring[_cursor] = sample;
            _cursor += 1;
            if (_cursor >= Capacity)
            {
                _cursor = 0;
            }

            if (_count < Capacity)
            {
                _count += 1;
            }
        }

        static int Collect(float px, float pz)
        {
            int kept = 0;
            Asteroid[] rocks = UnityEngine.Object.FindObjectsByType<Asteroid>();
            if (rocks != null)
            {
                for (int index = 0; index < rocks.Length; index++)
                {
                    Asteroid rock = rocks[index];
                    if (rock == null)
                    {
                        continue;
                    }

                    kept = Keep(px, pz, rock.name, rock.transform.position, rock.GetComponent<Rigidbody>(), rock.GetComponent<SphereCollider>(), kept);
                }
            }

            EnemySeeker[] enemies = UnityEngine.Object.FindObjectsByType<EnemySeeker>();
            if (enemies != null)
            {
                for (int index = 0; index < enemies.Length; index++)
                {
                    EnemySeeker enemy = enemies[index];
                    if (enemy == null)
                    {
                        continue;
                    }

                    kept = Keep(px, pz, enemy.name, enemy.transform.position, enemy.GetComponent<Rigidbody>(), enemy.GetComponent<SphereCollider>(), kept);
                }
            }

            return kept;
        }

        static int Keep(float px, float pz, string name, Vector3 pos, Rigidbody body, SphereCollider sphere, int kept)
        {
            Threat threat = new Threat();
            threat.Name = name;
            threat.X = pos.x;
            threat.Z = pos.z;
            if (body != null)
            {
                threat.Vx = body.linearVelocity.x;
                threat.Vz = body.linearVelocity.z;
            }

            threat.Radius = sphere != null ? sphere.radius : 0.5f;
            float dx = threat.X - px;
            float dz = threat.Z - pz;
            float dist = (dx * dx) + (dz * dz);
            if (kept < ThreatCap)
            {
                Near[kept] = threat;
                return kept + 1;
            }

            int farthest = 0;
            float far = -1f;
            for (int index = 0; index < ThreatCap; index++)
            {
                float ax = Near[index].X - px;
                float az = Near[index].Z - pz;
                float score = (ax * ax) + (az * az);
                if (score > far)
                {
                    far = score;
                    farthest = index;
                }
            }

            if (dist < far)
            {
                Near[farthest] = threat;
            }

            return ThreatCap;
        }

        static void Dump(GameSession session, int before, int after)
        {
            string dir = SessionState.GetString("LongRun.Out", string.Empty);
            if (dir.Length == 0)
            {
                return;
            }

            Directory.CreateDirectory(dir);
            _deaths += 1;
            string path = Path.Combine(dir, "death-" + _deaths.ToString("00", CultureInfo.InvariantCulture) + ".txt");
            StringBuilder builder = new StringBuilder();
            builder.Append("lives ");
            builder.Append(before.ToString(CultureInfo.InvariantCulture));
            builder.Append(" -> ");
            builder.Append(after.ToString(CultureInfo.InvariantCulture));
            builder.Append(" wave ");
            builder.Append(session.WaveIndex.ToString(CultureInfo.InvariantCulture));
            builder.Append(" steps ");
            builder.Append(_count.ToString(CultureInfo.InvariantCulture));
            builder.Append(" cap ");
            builder.Append(Capacity.ToString(CultureInfo.InvariantCulture));
            builder.Append(" window ");
            builder.Append(WindowSeconds.ToString("0.0", CultureInfo.InvariantCulture));
            builder.Append('\n');
            int start = _count < Capacity ? 0 : _cursor;
            for (int index = 0; index < _count; index++)
            {
                int slot = start + index;
                if (slot >= Capacity)
                {
                    slot -= Capacity;
                }

                WriteSample(builder, Ring[slot]);
            }

            File.WriteAllText(path, builder.ToString());
        }

        static void WriteSample(StringBuilder builder, Sample sample)
        {
            builder.Append("t=");
            builder.Append(sample.Time.ToString("0.000", CultureInfo.InvariantCulture));
            builder.Append(" step=");
            builder.Append(sample.Step.ToString(CultureInfo.InvariantCulture));
            builder.Append(" chosen=");
            builder.Append(sample.ChosenX.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(',');
            builder.Append(sample.ChosenY.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(" aim=");
            builder.Append(sample.ChosenAimX.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(',');
            builder.Append(sample.ChosenAimY.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(" fire=");
            builder.Append(sample.ChosenFire.ToString(CultureInfo.InvariantCulture));
            builder.Append(" used=");
            builder.Append(sample.UsedX.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(',');
            builder.Append(sample.UsedY.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(" uaim=");
            builder.Append(sample.UsedAimX.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(',');
            builder.Append(sample.UsedAimY.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(" ufire=");
            builder.Append(sample.UsedFire.ToString(CultureInfo.InvariantCulture));
            builder.Append(" p=");
            builder.Append(sample.Px.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(',');
            builder.Append(sample.Pz.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(" v=");
            builder.Append(sample.Vx.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(',');
            builder.Append(sample.Vz.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(" r=");
            builder.Append(sample.Radius.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(" threats=");
            builder.Append(sample.Threats.ToString(CultureInfo.InvariantCulture));
            AppendThreat(builder, sample.Threats, 0, sample.T0);
            AppendThreat(builder, sample.Threats, 1, sample.T1);
            AppendThreat(builder, sample.Threats, 2, sample.T2);
            AppendThreat(builder, sample.Threats, 3, sample.T3);
            AppendThreat(builder, sample.Threats, 4, sample.T4);
            AppendThreat(builder, sample.Threats, 5, sample.T5);
            AppendThreat(builder, sample.Threats, 6, sample.T6);
            AppendThreat(builder, sample.Threats, 7, sample.T7);
            builder.Append('\n');
        }

        static void AppendThreat(StringBuilder builder, int count, int index, Threat threat)
        {
            if (index >= count)
            {
                return;
            }

            builder.Append(" | ");
            builder.Append(threat.Name);
            builder.Append(' ');
            builder.Append(threat.X.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(',');
            builder.Append(threat.Z.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(" v ");
            builder.Append(threat.Vx.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(',');
            builder.Append(threat.Vz.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(" r ");
            builder.Append(threat.Radius.ToString("0.00", CultureInfo.InvariantCulture));
        }

        static void WriteWave(GameSession session, ShipController ship)
        {
            string dir = SessionState.GetString("LongRun.Out", string.Empty);
            if (dir.Length == 0)
            {
                return;
            }

            Directory.CreateDirectory(dir);
            int mask = 0;
            int mk2 = 0;
            int doctrine = 0;
            PlayerLoadout loadout = UnityEngine.Object.FindAnyObjectByType<PlayerLoadout>();
            if (loadout != null && loadout.State != null)
            {
                mask = RunSaveCodec.PackUpgrades(loadout.State);
                mk2 = loadout.State.Mk2Mask;
                doctrine = (int)loadout.State.Doctrine;
            }

            int hull = 0;
            int shield = 0;
            if (ship != null && ship.Health != null)
            {
                hull = ship.Health.Hull;
                shield = ship.Health.Shield;
            }

            UnityEngine.Random.State rng = UnityEngine.Random.state;
            StringBuilder builder = new StringBuilder();
            builder.Append("wave=");
            builder.Append(session.WaveIndex.ToString(CultureInfo.InvariantCulture));
            builder.Append(" world=");
            builder.Append(WorldCatalog.NumberForWave(session.WaveIndex).ToString(CultureInfo.InvariantCulture));
            builder.Append(" lives=");
            builder.Append(session.Lives.ToString(CultureInfo.InvariantCulture));
            builder.Append(" credits=");
            builder.Append(session.Credits.ToString(CultureInfo.InvariantCulture));
            builder.Append(" difficulty=");
            builder.Append(DifficultySettings.Current.ToString());
            builder.Append(" assist=");
            builder.Append(SettingsState.AssistEnabled ? "1" : "0");
            builder.Append(" reduce=");
            builder.Append(SettingsState.ReduceEffectsEnabled ? "1" : "0");
            builder.Append(" upgrades=");
            builder.Append(mask.ToString(CultureInfo.InvariantCulture));
            builder.Append(" mk2=");
            builder.Append(mk2.ToString(CultureInfo.InvariantCulture));
            builder.Append(" doctrine=");
            builder.Append(doctrine.ToString(CultureInfo.InvariantCulture));
            builder.Append(" hull=");
            builder.Append(hull.ToString(CultureInfo.InvariantCulture));
            builder.Append(" shield=");
            builder.Append(shield.ToString(CultureInfo.InvariantCulture));
            builder.Append(" dailySeed=");
            builder.Append(RunRng.ActiveDailySeed.ToString(CultureInfo.InvariantCulture));
            builder.Append(" randomState=");
            builder.Append(rng.GetHashCode().ToString(CultureInfo.InvariantCulture));
            builder.Append('\n');
            File.AppendAllText(Path.Combine(dir, "waves.txt"), builder.ToString());
        }
    }
}
