using UnityEngine;
using UnityEngine.UI;

namespace AsteroidsGoneRogue
{
    public sealed partial class GameUi
    {
        private Image _bossTick50;
        private Image _bossTick20;
        private float _bossPhaseBlinkUntil;

        public void AnnounceBossPhase(int phase)
        {
            _bossPhaseBlinkUntil = Time.unscaledTime + 0.35f;
            if (_world == null)
            {
                return;
            }

            _eliteBanner = phase >= 3
                ? Loc.T("ui.boss.phase3", "PHASE 3")
                : Loc.T("ui.boss.phase2", "PHASE 2");
            _medalBeat = string.Empty;
            _worldFlashUntil = Time.unscaledTime + 1.2f;
            _world.horizontalOverflow = HorizontalWrapMode.Wrap;
            _world.verticalOverflow = VerticalWrapMode.Overflow;
            Stretch(_world.rectTransform, new Vector2(0.34f, 0.74f), new Vector2(0.66f, 0.90f));
            _world.text = _eliteBanner;
        }

        private void RefreshBossPhaseMarks()
        {
            EnsureBossTicks();
            if (_bossFill == null)
            {
                return;
            }

            if (Time.unscaledTime < _bossPhaseBlinkUntil)
            {
                _bossFill.color = Color.Lerp(UiTheme.Danger, Color.white, 0.45f);
            }
        }

        private void EnsureBossTicks()
        {
            if (_bossRoot == null || _bossTick50 != null)
            {
                return;
            }

            _bossTick50 = CreateBossTick("BossTick50", 0.50f);
            _bossTick20 = CreateBossTick("BossTick20", 0.20f);
        }

        private Image CreateBossTick(string name, float hpFraction)
        {
            GameObject tick = new GameObject(name);
            tick.transform.SetParent(_bossRoot.transform, false);
            Image image = tick.AddComponent<Image>();
            image.color = new Color(1f, 0.96f, 0.9f, 0.9f);
            image.raycastTarget = false;
            RectTransform rect = image.rectTransform;
            float x = 0.08f + (0.84f * hpFraction);
            rect.anchorMin = new Vector2(x, 0.18f);
            rect.anchorMax = new Vector2(x + 0.008f, 0.82f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return image;
        }
    }
}
