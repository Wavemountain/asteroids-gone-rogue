using System;

namespace UnityEngine
{
    public class Object
    {
        public static void Destroy(Object obj) { }
        public static T FindAnyObjectByType<T>() where T : Object { return null; }
        public static T[] FindObjectsByType<T>() where T : Object { return new T[0]; }
        public static implicit operator bool(Object obj) { return obj != null; }
    }

    public class Component : Object
    {
        public GameObject gameObject { get; set; }
        public Transform transform { get; set; }
        public T GetComponent<T>() where T : Component { return null; }
        public T AddComponent<T>() where T : Component, new() { return new T(); }
    }

    public class Behaviour : Component
    {
        public bool enabled { get; set; }
    }

    public class MonoBehaviour : Behaviour
    {
        public string name { get; set; }
    }

    public class GameObject : Object
    {
        public string name { get; set; }
        public Transform transform { get; set; }
        public T GetComponent<T>() where T : Component { return null; }
        public T AddComponent<T>() where T : Component, new() { return new T(); }
    }

    public class Transform : Component
    {
        public Vector3 position { get; set; }
        public Vector3 localScale { get; set; }
        public Transform parent { get; set; }
    }

    public struct Vector2
    {
        public float x;
        public float y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero { get { return new Vector2(0f, 0f); } }
        public static Vector2 one { get { return new Vector2(1f, 1f); } }
        public float sqrMagnitude { get { return (x * x) + (y * y); } }
        public void Normalize()
        {
            float mag = (float)Math.Sqrt(sqrMagnitude);
            if (mag > 0.0001f)
            {
                x /= mag;
                y /= mag;
            }
        }
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero { get { return new Vector3(0f, 0f, 0f); } }
        public static Vector3 one { get { return new Vector3(1f, 1f, 1f); } }
        public static Vector3 operator *(Vector3 v, float s) { return new Vector3(v.x * s, v.y * s, v.z * s); }
        public static Vector3 operator +(Vector3 a, Vector3 b) { return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z); }
    }

    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static Color black { get { return new Color(0f, 0f, 0f, 1f); } }
        public static Color operator *(Color c, float s) { return new Color(c.r * s, c.g * s, c.b * s, c.a); }
    }

    public struct Color32
    {
        public byte r, g, b, a;
        public Color32(byte r, byte g, byte b, byte a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static implicit operator Color(Color32 c) { return new Color(c.r / 255f, c.g / 255f, c.b / 255f, c.a / 255f); }
    }

    public struct Rect
    {
        public Rect(float x, float y, float w, float h) { }
    }

    public class Camera : Behaviour
    {
        public CameraClearFlags clearFlags { get; set; }
        public Color backgroundColor { get; set; }
        public int cullingMask { get; set; }
        public float depth { get; set; }
        public Rect rect { get; set; }
        public float fieldOfView { get; set; }
        public float nearClipPlane { get; set; }
        public float farClipPlane { get; set; }
        public bool allowHDR { get; set; }
        public bool allowMSAA { get; set; }
    }

    public enum CameraClearFlags { Skybox, SolidColor, Depth, Nothing }

    public class AudioListener : Behaviour { }

    public static class Mathf
    {
        public const float PI = 3.14159274f;
        public static float Sqrt(float v) { return (float)Math.Sqrt(v); }
        public static float Clamp(float v, float a, float b) { return v < a ? a : (v > b ? b : v); }
        public static int Clamp(int v, int a, int b) { return v < a ? a : (v > b ? b : v); }
        public static float Max(float a, float b) { return a > b ? a : b; }
        public static int Max(int a, int b) { return a > b ? a : b; }
        public static float Min(float a, float b) { return a < b ? a : b; }
        public static float Cos(float v) { return (float)Math.Cos(v); }
        public static float Sin(float v) { return (float)Math.Sin(v); }
        public static float Round(float v) { return (float)Math.Round(v); }
        public static float Log(float v, float b) { return (float)Math.Log(v, b); }
        public static float Pow(float v, float p) { return (float)Math.Pow(v, p); }
        public static float Abs(float v) { return v < 0f ? -v : v; }
        public static int Abs(int v) { return v < 0 ? -v : v; }
        public static float Lerp(float a, float b, float t) { return a + (b - a) * t; }
    }

    public static class Random
    {
        public static float value { get { return 0f; } }
        public static int Range(int min, int max) { return min; }
        public static float Range(float min, float max) { return min; }
    }

    public static class PlayerPrefs
    {
        public static int GetInt(string key, int fallback) { return fallback; }
        public static void SetInt(string key, int value) { }
        public static string GetString(string key, string fallback) { return fallback; }
        public static void SetString(string key, string value) { }
        public static void Save() { }
        public static bool HasKey(string key) { return false; }
    }

    public static class JsonUtility
    {
        public static T FromJson<T>(string json) where T : class { return null; }
        public static string ToJson(object value) { return ""; }
    }

    public static class Application
    {
        public static string persistentDataPath { get { return "/tmp"; } }
        public static int targetFrameRate { get; set; }
    }

    public struct Resolution
    {
        public int width;
        public int height;
    }

    public enum FullScreenMode { ExclusiveFullScreen, FullScreenWindow, MaximizedWindow, Windowed }

    public static class Screen
    {
        public static int width { get { return 1920; } }
        public static int height { get { return 1080; } }
        public static Resolution currentResolution { get { return new Resolution { width = 1920, height = 1080 }; } }
        public static Resolution[] resolutions { get { return new Resolution[0]; } }
        public static void SetResolution(int width, int height, FullScreenMode mode) { }
    }

    public static class QualitySettings
    {
        public static int vSyncCount { get; set; }
    }

    public static class Input
    {
        public static bool anyKeyDown { get { return false; } }
        public static Vector2 mouseScrollDelta { get { return Vector2.zero; } }
        public static bool GetKey(KeyCode code) { return false; }
        public static bool GetKeyDown(KeyCode code) { return false; }
        public static bool GetButton(string name) { return false; }
        public static bool GetButtonDown(string name) { return false; }
        public static bool GetMouseButton(int button) { return false; }
        public static string[] GetJoystickNames() { return new string[0]; }
        public static float GetAxisRaw(string name) { return 0f; }
        public static float GetAxis(string name) { return 0f; }
        public static bool GetMouseButtonDown(int button) { return false; }
    }

    public enum KeyCode
    {
        None = 0,
        Escape = 27,
        Space = 32,
        Q = 113,
        E = 101,
        JoystickButton0 = 330,
        JoystickButton4 = 334,
        JoystickButton5 = 335,
        JoystickButton11 = 341,
        JoystickButton12 = 342,
        JoystickButton13 = 343,
        JoystickButton14 = 344,
    }

    public static class Time
    {
        public static int frameCount { get { return 0; } }
        public static float deltaTime { get { return 0.016f; } }
        public static float unscaledDeltaTime { get { return 0.016f; } }
        public static float unscaledTime { get { return 0f; } }
    }

    public static class Debug
    {
        public static void Log(object message) { }
        public static void LogWarning(object message) { }
        public static void LogError(object message) { }
    }

    public class Renderer : Component
    {
        public void GetPropertyBlock(MaterialPropertyBlock block) { }
        public void SetPropertyBlock(MaterialPropertyBlock block) { }
    }

    public class MaterialPropertyBlock
    {
        public void SetColor(int id, Color color) { }
    }

    public class Shader : Object
    {
        public static int PropertyToID(string name) { return 0; }
    }

    public class Material : Object { }

    public class Rigidbody : Component
    {
        public Vector3 linearVelocity { get; set; }
    }

    public struct Quaternion
    {
        public static Quaternion identity { get { return new Quaternion(); } }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SerializeField : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class HideInInspector : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DefaultExecutionOrder : Attribute
    {
        public DefaultExecutionOrder(int order) { }
    }

    public class Light : Behaviour { }
}
