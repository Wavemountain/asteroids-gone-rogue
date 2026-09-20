using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Legacy Input Manager Xbox / pad map. Keyboard and mouse stay live (hot-plug).
    /// Left stick flies. RT fires (axes read separately so they cannot cancel).
    /// LB cycles. Right stick aims; otherwise face the left-stick fly vector.
    /// </summary>
    public static class GamepadInput
    {
        public const float StickDead = 0.22f;
        public const float TriggerFire = 0.45f;
        public const string MoveX = "Horizontal";
        public const string MoveY = "Vertical";
        public const string PadMoveX = "PadMoveX";
        public const string PadMoveY = "PadMoveY";
        public const string AimX = "AimX";
        public const string AimY = "AimY";
        public const string FireTrigger = "FireTrigger";
        public const string FireTrigger3 = "FireTrigger3";
        public const string FireTrigger6 = "FireTrigger6";
        public const string FirePad = "FirePad";
        public const string CycleFire = "CycleFire";
        public const string Pause = "Pause";

        public static Vector2 MoveStick()
        {
            return Deadzone(new Vector2(Axis(MoveX), Axis(MoveY)));
        }

        public static Vector2 PadMoveStick()
        {
            return Deadzone(new Vector2(Axis(PadMoveX), Axis(PadMoveY)));
        }

        public static Vector2 AimStick()
        {
            return Deadzone(new Vector2(Axis(AimX), Axis(AimY)));
        }

        public static bool FireHeld()
        {
            if (Input.GetButton("Fire1") || Input.GetKey(KeyCode.Space))
            {
                return true;
            }

            if (Input.GetButton(FirePad))
            {
                return true;
            }

            return TriggerHeld(FireTrigger) || TriggerHeld(FireTrigger3) || TriggerHeld(FireTrigger6);
        }

        public static bool CyclePressed()
        {
            return Input.GetKeyDown(KeyCode.Q)
                || Input.GetMouseButtonDown(1)
                || Input.GetKeyDown(KeyCode.JoystickButton4)
                || ButtonDown(CycleFire);
        }

        public static bool PausePressed()
        {
            return Input.GetKeyDown(KeyCode.Escape) || ButtonDown(Pause);
        }

        public static bool ConfirmPressed()
        {
            return Input.GetButtonDown("Submit") || ButtonDown(FirePad);
        }

        public static bool CancelPressed()
        {
            return Input.GetButtonDown("Cancel") || Input.GetKeyDown(KeyCode.Escape);
        }

        public static Vector2 UiNavStick()
        {
            Vector2 stick = PadMoveStick();
            if (stick.sqrMagnitude >= StickDead * StickDead)
            {
                return stick;
            }

            return Deadzone(new Vector2(Axis(MoveX), Axis(MoveY)));
        }

        public static Vector2 MouseDelta()
        {
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }

        private static bool TriggerHeld(string name)
        {
            return Axis(name) >= TriggerFire;
        }

        private static float Axis(string name)
        {
            try
            {
                return Input.GetAxisRaw(name);
            }
            catch (System.ArgumentException)
            {
                return 0f;
            }
        }

        private static bool ButtonDown(string name)
        {
            try
            {
                return Input.GetButtonDown(name);
            }
            catch (System.ArgumentException)
            {
                return false;
            }
        }

        private static Vector2 Deadzone(Vector2 stick)
        {
            if (stick.sqrMagnitude < StickDead * StickDead)
            {
                return Vector2.zero;
            }

            if (stick.sqrMagnitude > 1f)
            {
                stick.Normalize();
            }

            return stick;
        }
    }
}
