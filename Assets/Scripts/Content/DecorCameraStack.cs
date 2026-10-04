using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Draws arena decor before the ship. The decor camera fills the color
    /// buffer; the play camera then clears depth only, so ships, enemies,
    /// shots and pickups always composite in front of buildings.
    /// </summary>
    [DefaultExecutionOrder(20000)]
    public sealed class DecorCameraStack : MonoBehaviour
    {
        public const int DecorLayer = 9;
        public const string DecorLayerName = "ArenaDecor";
        public const string CameraName = "DecorCamera";

        private Camera _play;

        public static int LayerIndex()
        {
            int named = LayerMask.NameToLayer(DecorLayerName);
            if (named >= 0)
            {
                return named;
            }

            return DecorLayer;
        }

        public static void ApplyLayer(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            int layer = LayerIndex();
            Transform[] nodes = root.GetComponentsInChildren<Transform>(true);
            for (int nodeIndex = 0; nodeIndex < nodes.Length; nodeIndex++)
            {
                if (nodes[nodeIndex] != null)
                {
                    nodes[nodeIndex].gameObject.layer = layer;
                }
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                if (renderer == null)
                {
                    continue;
                }

                renderer.sortingOrder = -2;
                Material material = renderer.sharedMaterial;
                if (material != null && material.renderQueue < 2500 && material.renderQueue != DecorDepthRules.DecorQueue)
                {
                    material.renderQueue = DecorDepthRules.DecorQueue;
                }
            }
        }

        public static void Attach(Camera gameplay)
        {
            if (gameplay == null)
            {
                return;
            }

            int decor = LayerIndex();
            gameplay.cullingMask &= ~(1 << decor);
            gameplay.clearFlags = CameraClearFlags.Depth;
            gameplay.depth = 0f;

            Transform existing = gameplay.transform.Find(CameraName);
            GameObject cameraObject = existing != null ? existing.gameObject : new GameObject(CameraName);
            cameraObject.transform.SetParent(gameplay.transform, false);
            cameraObject.transform.localPosition = Vector3.zero;
            cameraObject.transform.localRotation = Quaternion.identity;
            Camera decorCamera = cameraObject.GetComponent<Camera>();
            if (decorCamera == null)
            {
                decorCamera = cameraObject.AddComponent<Camera>();
            }

            decorCamera.clearFlags = CameraClearFlags.SolidColor;
            decorCamera.backgroundColor = gameplay.backgroundColor;
            decorCamera.cullingMask = 1 << decor;
            decorCamera.depth = -1f;
            decorCamera.fieldOfView = gameplay.fieldOfView;
            decorCamera.nearClipPlane = gameplay.nearClipPlane;
            decorCamera.farClipPlane = gameplay.farClipPlane;
            decorCamera.allowHDR = gameplay.allowHDR;
            decorCamera.allowMSAA = gameplay.allowMSAA;
            DecorCameraStack stack = cameraObject.GetComponent<DecorCameraStack>();
            if (stack == null)
            {
                stack = cameraObject.AddComponent<DecorCameraStack>();
            }

            stack._play = gameplay;
        }

        private void LateUpdate()
        {
            Camera decorCamera = GetComponent<Camera>();
            if (decorCamera == null || _play == null)
            {
                return;
            }

            decorCamera.fieldOfView = _play.fieldOfView;
            decorCamera.nearClipPlane = _play.nearClipPlane;
            decorCamera.farClipPlane = _play.farClipPlane;
            decorCamera.rect = _play.rect;
            decorCamera.backgroundColor = _play.backgroundColor;
            decorCamera.enabled = _play.enabled;
        }
    }
}
