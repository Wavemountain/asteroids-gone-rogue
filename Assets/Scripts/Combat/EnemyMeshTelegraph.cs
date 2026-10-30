using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Ramps the v4 Telegraph slot. Albedo stays warm white #F2ECE6.
    /// Reduce changes intensity through EffectScale and never the colour.
    /// </summary>
    public sealed class EnemyMeshTelegraph : MonoBehaviour
    {
        static readonly Color Warm = new Color(LookPolish.EnemyCoreR, LookPolish.EnemyCoreG, LookPolish.EnemyCoreB, 1f);
        Renderer[] _slots = System.Array.Empty<Renderer>();
        MaterialPropertyBlock _block;

        public void Collect(Transform root)
        {
            if (root == null)
            {
                _slots = System.Array.Empty<Renderer>();
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            int count = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (HasTelegraph(renderers[i]))
                {
                    count++;
                }
            }

            _slots = new Renderer[count];
            int write = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (!HasTelegraph(renderers[i]))
                {
                    continue;
                }

                _slots[write] = renderers[i];
                write++;
            }
        }

        void LateUpdate()
        {
            if (_slots == null || _slots.Length == 0)
            {
                return;
            }

            EnemySeeker seeker = GetComponent<EnemySeeker>();
            float ramp = seeker != null ? seeker.TelegraphRamp() : 0f;
            bool reduce = SettingsState.ReduceEffectsEnabled;
            float scale = ramp * EffectScale.MeshEmission(reduce);
            if (scale < 0f)
            {
                scale = 0f;
            }

            Color emit = new Color(Warm.r * scale, Warm.g * scale, Warm.b * scale, 1f);
            if (_block == null)
            {
                _block = new MaterialPropertyBlock();
            }

            for (int i = 0; i < _slots.Length; i++)
            {
                Renderer renderer = _slots[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(_block);
                _block.SetColor("_EmissionColor", emit);
                renderer.SetPropertyBlock(_block);
            }
        }

        static bool HasTelegraph(Renderer renderer)
        {
            if (renderer == null)
            {
                return false;
            }

            Material[] slots = renderer.sharedMaterials;
            if (slots == null)
            {
                return false;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                Material mat = slots[i];
                if (mat != null && mat.name.IndexOf("Telegraph", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
