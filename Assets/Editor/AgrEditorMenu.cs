using UnityEditor;
using UnityEngine;

namespace AsteroidsGoneRogue.EditorTools
{
    public static class AgrEditorMenu
    {
        private const string PlayScene = "Assets/Scenes/Play.unity";

        [MenuItem("Asteroids gone rogue/Open Play Scene")]
        public static void OpenPlayScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.Log("Stop Play Mode before opening the scene.");
                return;
            }

            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(PlayScene);
        }

        [MenuItem("Asteroids gone rogue/Ping Art Import Folder")]
        public static void PingImportFolder()
        {
            Object folder = AssetDatabase.LoadAssetAtPath<Object>("Assets/Art/Import");
            if (folder != null)
            {
                EditorGUIUtility.PingObject(folder);
                Selection.activeObject = folder;
            }
        }

        [MenuItem("Asteroids gone rogue/Validate Week 1 Setup")]
        public static void ValidateWeek1()
        {
            bool ok = true;
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            bool hasPlay = false;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled && scenes[i].path == PlayScene)
                {
                    hasPlay = true;
                    break;
                }
            }

            if (!hasPlay)
            {
                Debug.LogError("EditorBuildSettings is missing Assets/Scenes/Play.unity.");
                ok = false;
            }

            if (Object.FindObjectOfType<GameBootstrap>(true) == null)
            {
                Debug.LogWarning("GameBootstrap is not in the open scene. Open Play and press Play — the scene YAML includes it.");
            }

            if (PlayerSettings.productName != GameBootstrap.ProductTitle)
            {
                Debug.LogError("PlayerSettings productName must be exactly '" + GameBootstrap.ProductTitle + "'.");
                ok = false;
            }

            if (ok)
            {
                Debug.Log("Asteroids gone rogue Week 1 setup looks good. Press Play.");
            }
        }
    }
}
