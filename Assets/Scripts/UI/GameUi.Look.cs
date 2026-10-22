using UnityEngine;
using UnityEngine.UI;

namespace AsteroidsGoneRogue
{
    public sealed partial class GameUi
    {
        private Image _hitArc;
        private float _hitArcStart = -1f;
        private bool _hitArcShield;
        private float _hitArcAngle;
        private Text _settingsGrainLabel;
        private Text _settingsGrainValue;
        private float _hangarReturnAt = -1f;

        public void ShowHitVignette(Vector3 worldDirection, bool shield)
        {
            _hitArcShield = shield;
            _hitArcStart = Time.unscaledTime;
            _hitArcAngle = 0f;
            if (worldDirection.sqrMagnitude > 0.0001f)
            {
                _hitArcAngle = Mathf.Atan2(worldDirection.x, worldDirection.z) * Mathf.Rad2Deg;
            }

            ApplyHitArc();
        }

        public void BeginHangarReturn()
        {
            _hangarReturnAt = Time.unscaledTime;
        }

        private void EnsureHitArc()
        {
            if (_hitArc != null)
            {
                return;
            }

            GameObject arc = CreateFill(
                "HitArc",
                transform,
                new Color(LookPolish.HitRimR, LookPolish.HitRimG, LookPolish.HitRimB, 0f),
                Vector2.zero,
                Vector2.one);
            _hitArc = arc.GetComponent<Image>();
            if (_hitArc == null)
            {
                return;
            }

            _hitArc.sprite = MakeHitArcSprite();
            _hitArc.raycastTarget = false;
            _hitArc.type = Image.Type.Simple;
            RaiseCanvas(arc, CanvasOrder.Toast);
        }

        private void ApplyHitArc()
        {
            if (_hitArc == null)
            {
                return;
            }

            bool reduce = SettingsState.ReduceEffectsEnabled;
            float elapsed = _hitArcStart < 0f ? 99f : Time.unscaledTime - _hitArcStart;
            float alpha = LookPolish.HitVignetteAlpha(elapsed, reduce);
            Color tint = _hitArcShield
                ? new Color(LookPolish.SteelR, LookPolish.SteelG, LookPolish.SteelB, alpha)
                : new Color(LookPolish.HitRimR, LookPolish.HitRimG, LookPolish.HitRimB, alpha);
            _hitArc.color = tint;
            _hitArc.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -_hitArcAngle);
        }

        private static Sprite MakeHitArcSprite()
        {
            const int Size = 64;
            Texture2D tex = new Texture2D(Size, Size, TextureFormat.ARGB32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Vector2 center = new Vector2(0.5f, 0.5f);
            float halfArc = LookPolish.HitArcDegrees * 0.5f;
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float nx = (x + 0.5f) / Size;
                    float ny = (y + 0.5f) / Size;
                    float radial = Vector2.Distance(new Vector2(nx, ny), center) * 2f;
                    float degrees = Mathf.Atan2(ny - 0.5f, nx - 0.5f) * Mathf.Rad2Deg;
                    float delta = Mathf.DeltaAngle(90f, degrees);
                    float alpha = 0f;
                    if (radial > 0.62f && Mathf.Abs(delta) <= halfArc)
                    {
                        alpha = Mathf.Clamp01((radial - 0.62f) / 0.38f);
                    }

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            tex.name = "HitArc";
            return Sprite.Create(tex, new Rect(0f, 0f, Size, Size), new Vector2(0.5f, 0.5f), 64f);
        }

        private float HangarReturnAlpha()
        {
            if (_hangarReturnAt < 0f)
            {
                return 0f;
            }

            return LookPolish.HangarVeilAlpha(Time.unscaledTime - _hangarReturnAt);
        }

        private bool HangarCreditsHeld()
        {
            if (_hangarReturnAt < 0f)
            {
                return false;
            }

            return LookPolish.HangarStatusHeld(Time.unscaledTime - _hangarReturnAt);
        }

        private void BuildSettingsGrainRow(int rowIndex, Font body, float y0, float y1)
        {
            Button row = CreateButton(
                "SettingsGrain",
                _settingsRows,
                body,
                new Vector2(SettingsMeasure.RowMinX, y0),
                new Vector2(SettingsMeasure.RowMaxX, y1));
            _settingsRowButtons[rowIndex] = row;
            BindSettingsClick(row, rowIndex, ToggleFilmGrain);
            UiTheme.ApplyButton(row, false, false, false);

            _settingsGrainLabel = row.GetComponentInChildren<Text>();
            _settingsGrainLabel.fontSize = UiTheme.BodyMin;
            _settingsGrainLabel.alignment = TextAnchor.MiddleLeft;
            _settingsGrainLabel.fontStyle = FontStyle.Bold;
            _settingsGrainLabel.color = UiTheme.Accent;
            _settingsGrainLabel.raycastTarget = false;
            Stretch(_settingsGrainLabel.rectTransform, new Vector2(0.04f, 0.08f), new Vector2(0.62f, 0.92f));

            _settingsGrainValue = CreateText(
                "SettingsGrainValue",
                row.transform,
                body,
                UiTheme.BodyMin,
                TextAnchor.MiddleRight,
                FontStyle.Bold);
            Stretch(_settingsGrainValue.rectTransform, new Vector2(0.64f, 0.12f), new Vector2(0.96f, 0.88f));
            _settingsGrainValue.color = UiTheme.Primary;
            _settingsGrainValue.raycastTarget = false;
        }

        private void ToggleFilmGrain()
        {
            EnsureSettings();
            _settings.FilmGrain = !_settings.FilmGrain;
            _settings.Save();
            RefreshSettingsGrain();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private void RefreshSettingsGrain()
        {
            if (_settingsGrainLabel != null)
            {
                _settingsGrainLabel.text = Loc.T("ui.settings.grain", "Film grain");
            }

            if (_settingsGrainValue == null)
            {
                return;
            }

            bool enabled = _settings != null && _settings.FilmGrain;
            _settingsGrainValue.text = enabled
                ? Loc.T("ui.settings.on", "On")
                : Loc.T("ui.settings.off", "Off");
        }

        private Text _settingsTurnLabel;
        private Text _settingsTurnValue;
        private Slider _settingsTurnSlider;

        private void BuildSettingsTurnRow(int rowIndex, Font body, float y0, float y1)
        {
            Button row = CreateButton(
                "SettingsTurn",
                _settingsRows,
                body,
                new Vector2(SettingsMeasure.RowMinX, y0),
                new Vector2(SettingsMeasure.RowMaxX, y1));
            _settingsRowButtons[rowIndex] = row;
            UiTheme.ApplyButton(row, false, false, false);

            _settingsTurnLabel = row.GetComponentInChildren<Text>();
            _settingsTurnLabel.fontSize = UiTheme.BodyMin;
            _settingsTurnLabel.alignment = TextAnchor.MiddleLeft;
            _settingsTurnLabel.fontStyle = FontStyle.Bold;
            _settingsTurnLabel.color = UiTheme.Accent;
            _settingsTurnLabel.raycastTarget = false;
            Stretch(_settingsTurnLabel.rectTransform, new Vector2(0.04f, 0.08f), new Vector2(0.36f, 0.92f));

            EnsureSettings();
            float initial = _settings != null ? _settings.TurnSpeed : 1f;
            _settingsTurnSlider = CreateSlider(
                "SettingsTurnSlider",
                row.transform,
                new Vector2(SettingsMeasure.SliderMinX, 0.18f),
                new Vector2(SettingsMeasure.SliderMaxX, 0.82f),
                0.5f,
                OnSettingsTurn);
            _settingsTurnSlider.minValue = SettingsRows.TurnMin;
            _settingsTurnSlider.maxValue = SettingsRows.TurnMax;
            _settingsTurnSlider.value = SettingsRows.ClampTurn(initial);

            _settingsTurnValue = CreateText(
                "SettingsTurnValue",
                row.transform,
                body,
                UiTheme.BodyMin,
                TextAnchor.MiddleRight,
                FontStyle.Bold);
            Stretch(_settingsTurnValue.rectTransform, new Vector2(0.84f, 0.12f), new Vector2(0.98f, 0.88f));
            _settingsTurnValue.color = UiTheme.Primary;
            _settingsTurnValue.raycastTarget = false;
            RefreshSettingsTurn();
        }

        private void OnSettingsTurn(float value)
        {
            EnsureSettings();
            _settings.TurnSpeed = SettingsRows.ClampTurn(value);
            _settings.Save();
            RefreshSettingsTurn();
        }

        private void StepSettingsTurn(int direction)
        {
            EnsureSettings();
            _settings.TurnSpeed = SettingsRows.StepTurn(_settings.TurnSpeed, direction);
            _settings.Save();
            if (_settingsTurnSlider != null)
            {
                _settingsTurnSlider.value = _settings.TurnSpeed;
            }

            RefreshSettingsTurn();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayUiClick();
            }
        }

        private void RefreshSettingsTurn()
        {
            if (_settingsTurnLabel != null)
            {
                _settingsTurnLabel.text = Loc.T("ui.settings.turn", "Turn speed");
            }

            if (_settingsTurnValue == null)
            {
                return;
            }

            float scale = _settings != null ? _settings.TurnSpeed : 1f;
            int percent = (int)System.Math.Round(scale * 100.0);
            _settingsTurnValue.text = percent.ToString() + "%";
        }
    }
}
