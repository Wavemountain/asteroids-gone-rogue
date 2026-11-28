using UnityEngine;
using UnityEngine.UI;

namespace AsteroidsGoneRogue
{
    public sealed partial class GameUi
    {
        private Text _settingsShakeIntensityLabel;
        private Text _settingsShakeIntensityValue;
        private Slider _settingsShakeIntensitySlider;
        private Text _settingsHitstopLabel;
        private Text _settingsHitstopValue;
        private Text _settingsFlashLabel;
        private Text _settingsFlashValue;
        private float _bossBarPulseUntil;

        public void PulseBossBar()
        {
            _bossBarPulseUntil = Time.unscaledTime + 0.16f;
        }

        private Color BossBarColor()
        {
            if (Time.unscaledTime >= _bossBarPulseUntil)
            {
                return UiTheme.Danger;
            }

            float remain = (_bossBarPulseUntil - Time.unscaledTime) / 0.16f;
            if (remain < 0f)
            {
                remain = 0f;
            }

            if (remain > 1f)
            {
                remain = 1f;
            }

            return Color.Lerp(UiTheme.Danger, Color.white, 0.35f * remain);
        }

        private void BuildSettingsShakeIntensityRow(int rowIndex, Font body, float y0, float y1)
        {
            Button row = CreateButton(
                "SettingsShakeIntensity",
                _settingsRows,
                body,
                new Vector2(SettingsMeasure.RowMinX, y0),
                new Vector2(SettingsMeasure.RowMaxX, y1));
            _settingsRowButtons[rowIndex] = row;
            UiTheme.ApplyButton(row, false, false, false);

            _settingsShakeIntensityLabel = row.GetComponentInChildren<Text>();
            _settingsShakeIntensityLabel.fontSize = UiTheme.BodyMin;
            _settingsShakeIntensityLabel.alignment = TextAnchor.MiddleLeft;
            _settingsShakeIntensityLabel.fontStyle = FontStyle.Bold;
            _settingsShakeIntensityLabel.color = UiTheme.Accent;
            _settingsShakeIntensityLabel.raycastTarget = false;
            Stretch(_settingsShakeIntensityLabel.rectTransform, new Vector2(0.04f, 0.08f), new Vector2(0.36f, 0.92f));

            EnsureSettings();
            float initial = _settings != null ? _settings.ShakeIntensity : FeelScale.IntensityDefault;
            _settingsShakeIntensitySlider = CreateSlider(
                "SettingsShakeIntensitySlider",
                row.transform,
                new Vector2(SettingsMeasure.SliderMinX, 0.18f),
                new Vector2(SettingsMeasure.SliderMaxX, 0.82f),
                initial,
                OnSettingsShakeIntensity);
            _settingsShakeIntensitySlider.minValue = FeelScale.IntensityMin;
            _settingsShakeIntensitySlider.maxValue = FeelScale.IntensityMax;
            _settingsShakeIntensitySlider.value = SettingsRows.ClampShakeIntensity(initial);

            _settingsShakeIntensityValue = CreateText(
                "SettingsShakeIntensityValue",
                row.transform,
                body,
                UiTheme.BodyMin,
                TextAnchor.MiddleRight,
                FontStyle.Bold);
            Stretch(_settingsShakeIntensityValue.rectTransform, new Vector2(0.84f, 0.12f), new Vector2(0.98f, 0.88f));
            _settingsShakeIntensityValue.color = UiTheme.Primary;
            _settingsShakeIntensityValue.raycastTarget = false;
            RefreshSettingsShakeIntensity();
        }

        private void BuildSettingsHitstopRow(int rowIndex, Font body, float y0, float y1)
        {
            Button row = CreateButton(
                "SettingsHitstop",
                _settingsRows,
                body,
                new Vector2(SettingsMeasure.RowMinX, y0),
                new Vector2(SettingsMeasure.RowMaxX, y1));
            _settingsRowButtons[rowIndex] = row;
            BindSettingsClick(row, rowIndex, ToggleSettingsHitstop);
            UiTheme.ApplyButton(row, false, false, false);

            _settingsHitstopLabel = row.GetComponentInChildren<Text>();
            _settingsHitstopLabel.fontSize = UiTheme.BodyMin;
            _settingsHitstopLabel.alignment = TextAnchor.MiddleLeft;
            _settingsHitstopLabel.fontStyle = FontStyle.Bold;
            _settingsHitstopLabel.color = UiTheme.Accent;
            _settingsHitstopLabel.raycastTarget = false;
            Stretch(_settingsHitstopLabel.rectTransform, new Vector2(0.04f, 0.08f), new Vector2(0.62f, 0.92f));

            _settingsHitstopValue = CreateText(
                "SettingsHitstopValue",
                row.transform,
                body,
                UiTheme.BodyMin,
                TextAnchor.MiddleRight,
                FontStyle.Bold);
            Stretch(_settingsHitstopValue.rectTransform, new Vector2(0.64f, 0.12f), new Vector2(0.96f, 0.88f));
            _settingsHitstopValue.color = UiTheme.Primary;
            _settingsHitstopValue.raycastTarget = false;
            RefreshSettingsHitstop();
        }

        private void BuildSettingsFlashRow(int rowIndex, Font body, float y0, float y1)
        {
            Button row = CreateButton(
                "SettingsFlash",
                _settingsRows,
                body,
                new Vector2(SettingsMeasure.RowMinX, y0),
                new Vector2(SettingsMeasure.RowMaxX, y1));
            _settingsRowButtons[rowIndex] = row;
            BindSettingsClick(row, rowIndex, ActivateSettingsFlash);
            UiTheme.ApplyButton(row, false, false, false);

            _settingsFlashLabel = row.GetComponentInChildren<Text>();
            _settingsFlashLabel.fontSize = UiTheme.BodyMin;
            _settingsFlashLabel.alignment = TextAnchor.MiddleLeft;
            _settingsFlashLabel.fontStyle = FontStyle.Bold;
            _settingsFlashLabel.color = UiTheme.Accent;
            _settingsFlashLabel.raycastTarget = false;
            Stretch(_settingsFlashLabel.rectTransform, new Vector2(0.04f, 0.08f), new Vector2(0.62f, 0.92f));

            _settingsFlashValue = CreateText(
                "SettingsFlashValue",
                row.transform,
                body,
                UiTheme.BodyMin,
                TextAnchor.MiddleRight,
                FontStyle.Bold);
            Stretch(_settingsFlashValue.rectTransform, new Vector2(0.64f, 0.12f), new Vector2(0.96f, 0.88f));
            _settingsFlashValue.color = UiTheme.Primary;
            _settingsFlashValue.raycastTarget = false;
            RefreshSettingsFlash();
        }

        private void OnSettingsShakeIntensity(float value)
        {
            EnsureSettings();
            _settings.ShakeIntensity = SettingsRows.ClampShakeIntensity(value);
            _settings.Save();
            RefreshSettingsShakeIntensity();
        }

        private void StepSettingsShakeIntensity(int direction)
        {
            EnsureSettings();
            _settings.ShakeIntensity = SettingsRows.StepShakeIntensity(_settings.ShakeIntensity, direction);
            _settings.Save();
            if (_settingsShakeIntensitySlider != null)
            {
                _settingsShakeIntensitySlider.value = _settings.ShakeIntensity;
            }

            RefreshSettingsShakeIntensity();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private void ToggleSettingsHitstop()
        {
            EnsureSettings();
            _settings.Hitstop = !_settings.Hitstop;
            _settings.Save();
            RefreshSettingsHitstop();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private void StepSettingsFlash(int direction)
        {
            EnsureSettings();
            _settings.FlashLevel = SettingsRows.StepFlashLevel(_settings.FlashLevel, direction);
            _settings.Save();
            RefreshSettingsFlash();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private void ActivateSettingsFlash()
        {
            EnsureSettings();
            int next = _settings.FlashLevel + 1;
            if (next > FeelScale.FlashOff)
            {
                next = FeelScale.FlashFull;
            }

            _settings.FlashLevel = next;
            _settings.Save();
            RefreshSettingsFlash();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private void RefreshSettingsFeel()
        {
            RefreshSettingsShakeIntensity();
            RefreshSettingsHitstop();
            RefreshSettingsFlash();
        }

        private void RefreshSettingsShakeIntensity()
        {
            if (_settingsShakeIntensityLabel != null)
            {
                _settingsShakeIntensityLabel.text = Loc.T("ui.settings.shake_intensity", "Shake intensity");
            }

            if (_settingsShakeIntensityValue == null)
            {
                return;
            }

            float scale = _settings != null ? _settings.ShakeIntensity : FeelScale.IntensityDefault;
            int percent = (int)System.Math.Round(scale * 100.0);
            _settingsShakeIntensityValue.text = percent.ToString() + "%";
        }

        private void RefreshSettingsHitstop()
        {
            if (_settingsHitstopLabel != null)
            {
                _settingsHitstopLabel.text = Loc.T("ui.settings.hitstop", "Hitstop");
            }

            if (_settingsHitstopValue == null)
            {
                return;
            }

            bool enabled = _settings == null || _settings.Hitstop;
            _settingsHitstopValue.text = enabled
                ? Loc.T("ui.settings.on", "On")
                : Loc.T("ui.settings.off", "Off");
        }

        private void RefreshSettingsFlash()
        {
            if (_settingsFlashLabel != null)
            {
                _settingsFlashLabel.text = Loc.T("ui.settings.flash", "Flash");
            }

            if (_settingsFlashValue == null)
            {
                return;
            }

            int level = _settings != null ? _settings.FlashLevel : FeelScale.FlashFull;
            string key = "ui.settings.flash.full";
            string fallback = "Full";
            if (level == FeelScale.FlashHalf)
            {
                key = "ui.settings.flash.half";
                fallback = "Half";
            }
            else if (level == FeelScale.FlashOff)
            {
                key = "ui.settings.flash.off";
                fallback = "Off";
            }

            _settingsFlashValue.text = Loc.T(key, fallback);
        }
    }
}
