using System;
using UnityEngine;
#pragma warning disable CS0067

// Unity 6000.6 editor surface used by Assets/Editor. Members are the real ones
// those scripts call. Runtime scripts are not given editor-only members.
namespace UnityEditor
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class MenuItemAttribute : Attribute
    {
        public MenuItemAttribute(string itemName) { }
        public MenuItemAttribute(string itemName, bool isValidateFunction) { }
        public MenuItemAttribute(string itemName, bool isValidateFunction, int priority) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InitializeOnLoadAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class InitializeOnEnterPlayModeAttribute : Attribute { }

    public enum EnterPlayModeOptions
    {
        None = 0,
        DisableDomainReload = 1,
        DisableSceneReload = 2,
    }

    public enum PlayModeStateChange
    {
        EnteredEditMode,
        ExitingEditMode,
        EnteredPlayMode,
        ExitingPlayMode,
    }

    public class AssetPostprocessor
    {
        public string assetPath { get { return ""; } }
        public AssetImporter assetImporter { get { return null; } }
    }

    public static class EditorApplication
    {
        public delegate void CallbackFunction();
        public static event CallbackFunction update;
        public static event Action quitting;
        public static event Action<PlayModeStateChange> playModeStateChanged;
        public static bool isPlaying { get; set; }
        public static bool isPlayingOrWillChangePlaymode { get; set; }
        public static bool isPaused { get; set; }
        public static bool isCompiling { get; set; }
        public static bool isUpdating { get; set; }
        public static double timeSinceStartup { get { return 0; } }
        public static void QueuePlayerLoopUpdate() { }
        public static void EnterPlaymode() { }
        public static void ExitPlaymode() { }
        public static void Exit(int returnValue) { }
        public static void Step() { }
    }

    public static class SessionState
    {
        public static bool GetBool(string key, bool defaultValue) { return defaultValue; }
        public static void SetBool(string key, bool value) { }
        public static int GetInt(string key, int defaultValue) { return defaultValue; }
        public static void SetInt(string key, int value) { }
        public static string GetString(string key, string defaultValue) { return defaultValue; }
        public static void SetString(string key, string value) { }
        public static void EraseBool(string key) { }
        public static void EraseInt(string key) { }
        public static void EraseString(string key) { }
    }

    public static class AssetDatabase
    {
        public static T LoadAssetAtPath<T>(string assetPath) where T : UnityEngine.Object { return null; }
        public static string[] FindAssets(string filter) { return new string[0]; }
        public static string GUIDToAssetPath(string guid) { return ""; }
        public static string AssetPathToGUID(string path) { return ""; }
        public static void ImportAsset(string path) { }
        public static void Refresh() { }
    }

    public static class EditorGUIUtility
    {
        public static void PingObject(UnityEngine.Object obj) { }
    }

    public static class Selection
    {
        public static UnityEngine.Object activeObject { get; set; }
        public static GameObject activeGameObject { get; set; }
    }

    public class EditorBuildSettingsScene
    {
        public bool enabled { get; set; }
        public string path { get; set; }
    }

    public static class EditorBuildSettings
    {
        public static EditorBuildSettingsScene[] scenes { get; set; }
    }

    public static class PlayerSettings
    {
        public static string productName { get; set; }
        public static string companyName { get; set; }
    }

    public enum TextureImporterType { Default, NormalMap, GUI, Sprite, Cursor, Cookie, Lightmap, SingleChannel }

    public enum SpriteImportMode { None, Single, Multiple, Polygon }

    public enum TextureImporterCompression { Uncompressed, Compressed, CompressedHQ, CompressedLQ }

    public enum TextureImporterNPOTScale { None, ToNearest, ToLarger, ToSmaller }

    public class AssetImporter : UnityEngine.Object
    {
    }

    public class TextureImporter : AssetImporter
    {
        public TextureImporterType textureType { get; set; }
        public SpriteImportMode spriteImportMode { get; set; }
        public bool mipmapEnabled { get; set; }
        public bool alphaIsTransparency { get; set; }
        public FilterMode filterMode { get; set; }
        public TextureImporterCompression textureCompression { get; set; }
        public TextureImporterNPOTScale npotScale { get; set; }
        public TextureWrapMode wrapMode { get; set; }
    }
}

namespace UnityEditor.SceneManagement
{
    public static class EditorSceneManager
    {
        public static void OpenScene(string scenePath) { }
    }
}

namespace UnityEngine.SceneManagement
{
    public struct Scene
    {
        public string name { get { return ""; } }
        public string path { get { return ""; } }
        public bool isLoaded { get { return true; } }
        public int rootCount { get { return 0; } }
        public bool IsValid() { return true; }
    }

    public static class SceneManager
    {
        public static Scene GetActiveScene() { return default; }
        public static void LoadScene(string sceneName) { }
        public static void LoadScene(int sceneBuildIndex) { }
    }
}

namespace UnityEditor
{
    public static class EditorSettings
    {
        public static bool enterPlayModeOptionsEnabled { get; set; }
        public static EnterPlayModeOptions enterPlayModeOptions { get; set; }
    }
}
