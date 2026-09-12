using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Named part slots that share origin (0,0,0) so FBX swaps stay aligned.
    /// </summary>
    public sealed class ShipVisuals : MonoBehaviour
    {
        public Transform BodySlot;
        public Transform NoseSlot;
        public Transform EngineSlot;
        public GameObject DefaultBody;
        public GameObject UpgradedBody;
        public GameObject DefaultNose;
        public GameObject UpgradedNose;
        public GameObject UpgradedNose02;
        public GameObject DefaultEngine;
        public GameObject UpgradedEngine;
        public GameObject UpgradedEngine02;
        public GameObject ShieldBubble;
        public GameObject HullPlates;
        public GameObject SpreadPods;
        public GameObject TwinBarrels;
        public GameObject PierceNeedle;
        public GameObject SeekerRail;
        public GameObject RicochetFacet;
        public GameObject OverchargerGlow;
        public GameObject AfterburnerGlow;
        public Material PreviewGhostMaterial;

        public const float BlinkIntervalSeconds = 0.09f;

        private float _blinkUntil;
        private float _nextToggle;
        private bool _blinkHidden;
        private Renderer[] _blinkRenderers;
        private readonly System.Collections.Generic.List<Renderer> _ghosted =
            new System.Collections.Generic.List<Renderer>();
        private readonly System.Collections.Generic.List<Material[]> _ghostRestore =
            new System.Collections.Generic.List<Material[]>();

        public bool IsBlinking
        {
            get { return _blinkUntil > 0f && Time.time < _blinkUntil; }
        }

        public void PlayHitBlink(float duration)
        {
            StopHitBlink();
            if (duration <= 0f)
            {
                return;
            }

            _blinkRenderers = GetComponentsInChildren<Renderer>(false);
            _blinkUntil = Time.time + duration;
            _nextToggle = Time.time;
            _blinkHidden = false;
            ToggleBlink();
        }

        public void StopHitBlink()
        {
            _blinkUntil = 0f;
            _nextToggle = 0f;
            _blinkHidden = false;
            SetBlinkRenderersVisible(true);
            _blinkRenderers = null;
        }

        public void ApplyLoadout(LoadoutState loadout)
        {
            ApplyLoadout(loadout, null);
        }

        public void ApplyLoadout(LoadoutState loadout, LoadoutState owned)
        {
            ClearGhost();
            bool bodyUpgrade = loadout != null && loadout.BodyUpgrade01;
            if (DefaultBody != null)
            {
                DefaultBody.SetActive(!bodyUpgrade);
            }

            if (UpgradedBody != null)
            {
                UpgradedBody.SetActive(bodyUpgrade);
            }

            int noseTier = 0;
            if (loadout != null && (loadout.NoseUpgrade03 || loadout.NoseUpgrade02))
            {
                noseTier = 2;
            }
            else if (loadout != null && loadout.NoseHardpoint)
            {
                noseTier = 1;
            }

            SetTier(DefaultNose, UpgradedNose, UpgradedNose02, noseTier);

            int engineTier = 0;
            if (loadout != null && (loadout.EngineUpgrade03 || loadout.EngineUpgrade02))
            {
                engineTier = 2;
            }
            else if (loadout != null && loadout.RapidFire)
            {
                engineTier = 1;
            }

            SetTier(DefaultEngine, UpgradedEngine, UpgradedEngine02, engineTier);
            SetCosmetic(HullPlates, loadout != null && loadout.BodyUpgrade02);
            SetCosmetic(SpreadPods, loadout != null && loadout.SpreadBolt);
            SetCosmetic(TwinBarrels, loadout != null && loadout.TwinGuns);
            SetCosmetic(PierceNeedle, loadout != null && loadout.Pierce);
            SetCosmetic(SeekerRail, loadout != null && loadout.Seeker);
            SetCosmetic(RicochetFacet, loadout != null && loadout.Ricochet);
            SetCosmetic(OverchargerGlow, loadout != null && loadout.Overcharger);
            SetCosmetic(AfterburnerGlow, loadout != null && loadout.Afterburner);
            bool shieldOn = loadout != null && (loadout.ShieldCharges > 0 || loadout.ShieldMatrix);
            SetShieldVisible(shieldOn);
            if (ShieldBubble != null && loadout != null && loadout.ShieldMatrix)
            {
                ShieldBubble.transform.localScale = new Vector3(2.75f, 2.75f, 2.75f);
            }
            else if (ShieldBubble != null)
            {
                ShieldBubble.transform.localScale = new Vector3(2.4f, 2.4f, 2.4f);
            }

            if (owned != null && loadout != null)
            {
                GhostIfNew(DefaultBody, owned.BodyUpgrade01 && !loadout.BodyUpgrade01);
                GhostIfNew(UpgradedBody, loadout.BodyUpgrade01 && !owned.BodyUpgrade01);
                GhostIfNew(DefaultNose, NoseTier(owned) == 0 && noseTier != 0);
                GhostIfNew(UpgradedNose, noseTier == 1 && NoseTier(owned) != 1);
                GhostIfNew(UpgradedNose02, noseTier == 2 && NoseTier(owned) != 2);
                GhostIfNew(DefaultEngine, EngineTier(owned) == 0 && engineTier != 0);
                GhostIfNew(UpgradedEngine, engineTier == 1 && EngineTier(owned) != 1);
                GhostIfNew(UpgradedEngine02, engineTier == 2 && EngineTier(owned) != 2);
                GhostIfNew(HullPlates, loadout.BodyUpgrade02 && !owned.BodyUpgrade02);
                GhostIfNew(SpreadPods, loadout.SpreadBolt && !owned.SpreadBolt);
                GhostIfNew(TwinBarrels, loadout.TwinGuns && !owned.TwinGuns);
                GhostIfNew(PierceNeedle, loadout.Pierce && !owned.Pierce);
                GhostIfNew(SeekerRail, loadout.Seeker && !owned.Seeker);
                GhostIfNew(RicochetFacet, loadout.Ricochet && !owned.Ricochet);
                GhostIfNew(OverchargerGlow, loadout.Overcharger && !owned.Overcharger);
                GhostIfNew(AfterburnerGlow, loadout.Afterburner && !owned.Afterburner);
                GhostIfNew(ShieldBubble, shieldOn && owned.ShieldCharges <= 0 && !owned.ShieldMatrix);
            }
        }

        private static int NoseTier(LoadoutState loadout)
        {
            if (loadout.NoseUpgrade03 || loadout.NoseUpgrade02)
            {
                return 2;
            }

            return loadout.NoseHardpoint ? 1 : 0;
        }

        private static int EngineTier(LoadoutState loadout)
        {
            if (loadout.EngineUpgrade03 || loadout.EngineUpgrade02)
            {
                return 2;
            }

            return loadout.RapidFire ? 1 : 0;
        }

        private static void SetCosmetic(GameObject go, bool on)
        {
            if (go != null)
            {
                go.SetActive(on);
            }
        }

        private void GhostIfNew(GameObject go, bool isNew)
        {
            if (!isNew || go == null || !go.activeInHierarchy || PreviewGhostMaterial == null)
            {
                return;
            }

            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                _ghosted.Add(renderer);
                _ghostRestore.Add(renderer.sharedMaterials);
                Material[] ghosted = new Material[renderer.sharedMaterials.Length];
                for (int m = 0; m < ghosted.Length; m++)
                {
                    ghosted[m] = PreviewGhostMaterial;
                }

                renderer.sharedMaterials = ghosted;
            }
        }

        private void ClearGhost()
        {
            for (int i = 0; i < _ghosted.Count; i++)
            {
                if (_ghosted[i] != null)
                {
                    _ghosted[i].sharedMaterials = _ghostRestore[i];
                }
            }

            _ghosted.Clear();
            _ghostRestore.Clear();
        }

        private static void SetTier(GameObject tier0, GameObject tier1, GameObject tier2, int tier)
        {
            if (tier0 != null)
            {
                tier0.SetActive(tier == 0);
            }

            if (tier1 != null)
            {
                tier1.SetActive(tier == 1);
            }

            if (tier2 != null)
            {
                tier2.SetActive(tier == 2);
            }
        }

        public void SetShieldVisible(bool visible)
        {
            if (ShieldBubble != null)
            {
                ShieldBubble.SetActive(visible);
            }
        }

        private void Update()
        {
            if (_blinkUntil <= 0f)
            {
                return;
            }

            if (Time.time >= _blinkUntil)
            {
                StopHitBlink();
                return;
            }

            if (Time.time >= _nextToggle)
            {
                ToggleBlink();
            }
        }

        private void ToggleBlink()
        {
            _blinkHidden = !_blinkHidden;
            SetBlinkRenderersVisible(!_blinkHidden);
            _nextToggle = Time.time + BlinkIntervalSeconds;
        }

        private void SetBlinkRenderersVisible(bool visible)
        {
            if (_blinkRenderers == null)
            {
                return;
            }

            for (int i = 0; i < _blinkRenderers.Length; i++)
            {
                if (_blinkRenderers[i] != null)
                {
                    _blinkRenderers[i].enabled = visible;
                }
            }
        }

        private void OnDisable()
        {
            StopHitBlink();
        }
    }
}
