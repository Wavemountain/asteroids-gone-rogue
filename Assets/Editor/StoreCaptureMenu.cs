using AsteroidsGoneRogue;
using UnityEditor;
using UnityEngine;

namespace AsteroidsGoneRogue.EditorTools
{
    /// <summary>
    /// Atmos store-pass: scripted cam poses when Play Mode is up. F12 still
    /// dumps PNGs from StoreCaptureDirector. Capsule is a 3/4 on Ship_Complete.
    /// </summary>
    public static class StoreCaptureMenu
    {
        [MenuItem("Asteroids gone rogue/Store Captures/Pose Hangar shop + HEALTH + LaunchSign")]
        public static void PoseHangar()
        {
            Apply(StoreCapturePoses.HangarShop);
        }

        [MenuItem("Asteroids gone rogue/Store Captures/Pose Play void AstroFloor")]
        public static void PosePlayVoid()
        {
            Apply(StoreCapturePoses.PlayVoid);
        }

        [MenuItem("Asteroids gone rogue/Store Captures/Pose Combat juice Bolt/Spread")]
        public static void PoseCombat()
        {
            Apply(StoreCapturePoses.CombatJuice);
        }

        [MenuItem("Asteroids gone rogue/Store Captures/Pose Brute/Swarm beat")]
        public static void PoseBrute()
        {
            Apply(StoreCapturePoses.BruteSwarm);
        }

        [MenuItem("Asteroids gone rogue/Store Captures/Pose Fail SHIP LOST or win HIT07")]
        public static void PoseFailOrWin()
        {
            Apply(StoreCapturePoses.FailOrWin);
        }

        [MenuItem("Asteroids gone rogue/Store Captures/Pose Rail charge glow")]
        public static void PoseRailCharge()
        {
            Apply(StoreCapturePoses.RailCharge);
        }

        [MenuItem("Asteroids gone rogue/Store Captures/Pose Capsule 3/4 Ship_Complete")]
        public static void PoseCapsule()
        {
            Apply(StoreCapturePoses.Capsule);
        }

        [MenuItem("Asteroids gone rogue/Store Captures/F12 Capture current pose")]
        public static void CaptureNow()
        {
            StoreCaptureDirector director = StoreCaptureDirector.Ensure();
            director.CaptureCurrent();
        }

        [MenuItem("Asteroids gone rogue/Store Captures/Release pose hold")]
        public static void Release()
        {
            if (StoreCaptureDirector.Instance != null)
            {
                StoreCaptureDirector.Instance.ReleasePose();
            }
        }

        private static void Apply(string id)
        {
            if (!Application.isPlaying)
            {
                Debug.Log("Enter Play Mode, then pose. Paths: " + StoreCapturePoses.OutFolder
                    + "  ·  Capsule amber " + StoreCapturePoses.CapsuleHex + " anti-epa.");
            }

            StoreCaptureDirector director = StoreCaptureDirector.Ensure();
            director.ApplyPose(id);
            Selection.activeGameObject = Camera.main != null ? Camera.main.gameObject : null;
        }
    }
}
