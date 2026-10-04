using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Reads legacy Input once a frame and publishes <see cref="InputSchemeDetector"/>.
    /// Axis names stay on <see cref="GamepadInput"/>. InputManager axes are not edited.
    /// </summary>
    public static class InputSchemeDriver
    {
        public static InputScheme Current = InputScheme.Keyboard;
        public static bool Changed;

        public static void Poll(InputSchemePreference preference)
        {
            SchemeSample sample = ReadSample();
            InputScheme next = InputSchemeDetector.Decide(Current, preference, sample);
            Changed = next != Current;
            Current = next;
        }

        public static SchemeSample ReadSample()
        {
            SchemeSample sample = new SchemeSample();
            bool padDown = false;
            bool padHeld = false;
            for (int button = 0; button < 20; button++)
            {
                KeyCode code = (KeyCode)((int)KeyCode.JoystickButton0 + button);
                if (Input.GetKeyDown(code))
                {
                    padDown = true;
                }

                if (Input.GetKey(code))
                {
                    padHeld = true;
                }
            }

            sample.PadButton = padDown || padHeld;
            sample.KeyDown = Input.anyKeyDown && !padDown;
            float mouseX = ReadAxis("Mouse X");
            float mouseY = ReadAxis("Mouse Y");
            if (mouseX < 0f)
            {
                mouseX = -mouseX;
            }

            if (mouseY < 0f)
            {
                mouseY = -mouseY;
            }

            bool mouseButton = Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2);
            sample.MouseActive = mouseButton || (mouseX + mouseY) > 0.001f;
            sample.AxisAbs = MaxAxis();
            sample.JoystickNames = Input.GetJoystickNames();
            sample.JoystickName = FirstName(sample.JoystickNames);
            string deckEnv = System.Environment.GetEnvironmentVariable("SteamDeck");
            sample.SteamDeck = deckEnv == "1";
            return sample;
        }

        private static float MaxAxis()
        {
            float peak = 0f;
            peak = Wider(peak, GamepadInput.PadMoveX);
            peak = Wider(peak, GamepadInput.PadMoveY);
            peak = Wider(peak, GamepadInput.AimX);
            peak = Wider(peak, GamepadInput.AimY);
            peak = Wider(peak, GamepadInput.FireTrigger);
            peak = Wider(peak, GamepadInput.FireTrigger3);
            peak = Wider(peak, GamepadInput.FireTrigger6);
            peak = Wider(peak, GamepadInput.UtilityTrigger);
            peak = Wider(peak, GamepadInput.MoveX);
            peak = Wider(peak, GamepadInput.MoveY);
            peak = Wider(peak, GamepadInput.PadDpadX);
            peak = Wider(peak, GamepadInput.PadDpadY);
            return peak;
        }

        private static float Wider(float peak, string axisName)
        {
            float value = ReadAxis(axisName);
            if (value < 0f)
            {
                value = -value;
            }

            return value > peak ? value : peak;
        }

        private static float ReadAxis(string axisName)
        {
            try
            {
                return Input.GetAxisRaw(axisName);
            }
            catch (System.ArgumentException)
            {
                return 0f;
            }
        }

        private static string FirstName(string[] names)
        {
            if (names == null)
            {
                return string.Empty;
            }

            int count = names.Length;
            for (int index = 0; index < count; index++)
            {
                string name = names[index];
                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }
            }

            return string.Empty;
        }
    }
}
