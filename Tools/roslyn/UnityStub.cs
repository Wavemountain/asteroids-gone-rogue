using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Stand-in for Unity 6000.6 editor assemblies. Members match the real API the
// game scripts call. Enums only list real members (HorizontalWrapMode is
// Wrap/Overflow; VerticalWrapMode is Truncate/Overflow).
namespace UnityEngine
{
    public enum FindObjectsInactive { Exclude, Include }

    public enum HideFlags
    {
        None = 0,
        HideInHierarchy = 1,
        HideInInspector = 2,
        DontSaveInEditor = 4,
        NotEditable = 8,
        DontSaveInBuild = 16,
        DontUnloadUnusedAsset = 32,
        DontSave = 52,
        HideAndDontSave = 61,
    }

    public struct EntityId : IEquatable<EntityId>
    {
        public bool Equals(EntityId other) { return true; }
        public override bool Equals(object obj) { return obj is EntityId; }
        public override int GetHashCode() { return 0; }
    }

    public class Object
    {
        public string name { get; set; }
        public HideFlags hideFlags { get; set; }
        public static void Destroy(Object obj) { }
        public static void Destroy(Object obj, float delay) { }
        public static void DestroyImmediate(Object obj) { }
        public static T FindAnyObjectByType<T>() where T : Object { return null; }
        public static T FindAnyObjectByType<T>(FindObjectsInactive findObjectsInactive) where T : Object { return null; }
        public static Object FindAnyObjectByType(Type type, FindObjectsInactive findObjectsInactive) { return null; }
        public static T[] FindObjectsByType<T>() where T : Object { return new T[0]; }
        public static T Instantiate<T>(T original) where T : Object { return null; }
        public static T Instantiate<T>(T original, Transform parent, bool worldPositionStays) where T : Object { return null; }
        public EntityId GetEntityId() { return default; }
        public static implicit operator bool(Object obj) { return !ReferenceEquals(obj, null); }
        public static bool operator ==(Object a, Object b) { return ReferenceEquals(a, b); }
        public static bool operator !=(Object a, Object b) { return !ReferenceEquals(a, b); }
        public override bool Equals(object other) { return ReferenceEquals(this, other); }
        public override int GetHashCode() { return base.GetHashCode(); }
    }

    public class Component : Object
    {
        public GameObject gameObject { get; set; }
        public Transform transform { get; set; }
        public string tag { get; set; }
        public Component GetComponent(Type type) { return null; }
        public T GetComponent<T>() { return default; }
        public T GetComponentInChildren<T>() { return default; }
        public T GetComponentInChildren<T>(bool includeInactive) { return default; }
        public T GetComponentInParent<T>() { return default; }
        public T[] GetComponentsInChildren<T>() { return new T[0]; }
        public T[] GetComponentsInChildren<T>(bool includeInactive) { return new T[0]; }
        public bool CompareTag(string tag) { return false; }
        public Coroutine StartCoroutine(IEnumerator routine) { return null; }
    }

    public class Behaviour : Component
    {
        public bool enabled { get; set; }
        public bool isActiveAndEnabled { get { return enabled; } }
    }

    public class MonoBehaviour : Behaviour
    {
    }

    public class GameObject : Object
    {
        public GameObject() { }
        public GameObject(string name) { this.name = name; }
        public GameObject(string name, params Type[] components) { this.name = name; }
        public Transform transform { get; set; }
        public int layer { get; set; }
        public string tag { get; set; }
        public bool activeSelf { get { return true; } }
        public bool activeInHierarchy { get { return true; } }
        public void SetActive(bool value) { }
        public T GetComponent<T>() { return default; }
        public T GetComponentInChildren<T>() { return default; }
        public T[] GetComponentsInChildren<T>() { return new T[0]; }
        public T[] GetComponentsInChildren<T>(bool includeInactive) { return new T[0]; }
        public Component AddComponent(Type componentType) { return null; }
        public T AddComponent<T>() where T : Component { return null; }
        public static GameObject CreatePrimitive(PrimitiveType type) { return null; }
        public static GameObject FindGameObjectWithTag(string tag) { return null; }
        public static GameObject[] FindGameObjectsWithTag(string tag) { return new GameObject[0]; }
    }

    public class Transform : Component
    {
        public Vector3 position { get; set; }
        public Vector3 localPosition { get; set; }
        public Vector3 localScale { get; set; }
        public Quaternion rotation { get; set; }
        public Quaternion localRotation { get; set; }
        public Vector3 forward { get; set; }
        public Vector3 right { get; set; }
        public Vector3 up { get; set; }
        public Vector3 lossyScale { get { return localScale; } }
        public Transform parent { get; set; }
        public int childCount { get { return 0; } }
        public Transform Find(string n) { return null; }
        public Transform GetChild(int index) { return null; }
        public void LookAt(Vector3 worldPosition) { }
        public void LookAt(Transform target) { }
        public void Rotate(float xAngle, float yAngle, float zAngle, Space relativeTo) { }
        public void SetParent(Transform parent) { }
        public void SetParent(Transform parent, bool worldPositionStays) { }
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation) { }
        public void SetAsFirstSibling() { }
        public void SetAsLastSibling() { }
        public void SetSiblingIndex(int index) { }
        public bool IsChildOf(Transform parent) { return false; }
        public Vector3 InverseTransformPoint(Vector3 position) { return position; }
        public Vector3 TransformPoint(Vector3 position) { return position; }
    }

    public class RectTransform : Transform
    {
        public Vector2 anchorMin { get; set; }
        public Vector2 anchorMax { get; set; }
        public Vector2 anchoredPosition { get; set; }
        public Vector2 offsetMin { get; set; }
        public Vector2 offsetMax { get; set; }
        public Vector2 pivot { get; set; }
        public Vector2 sizeDelta { get; set; }
        public Rect rect { get { return new Rect(0f, 0f, 0f, 0f); } }
        public void GetWorldCorners(Vector3[] fourCornersArray) { }
    }

    public struct Vector2
    {
        public float x;
        public float y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero { get { return new Vector2(0f, 0f); } }
        public static Vector2 one { get { return new Vector2(1f, 1f); } }
        public static Vector2 up { get { return new Vector2(0f, 1f); } }
        public static Vector2 right { get { return new Vector2(1f, 0f); } }
        public float sqrMagnitude { get { return (x * x) + (y * y); } }
        public float magnitude { get { return (float)Math.Sqrt(sqrMagnitude); } }
        public void Normalize()
        {
            float mag = (float)Math.Sqrt(sqrMagnitude);
            if (mag > 0.0001f)
            {
                x /= mag;
                y /= mag;
            }
        }
        public static float Distance(Vector2 a, Vector2 b) { return (a - b).magnitude; }
        public static Vector2 operator +(Vector2 a, Vector2 b) { return new Vector2(a.x + b.x, a.y + b.y); }
        public static Vector2 operator -(Vector2 a, Vector2 b) { return new Vector2(a.x - b.x, a.y - b.y); }
        public static Vector2 operator *(Vector2 v, float s) { return new Vector2(v.x * s, v.y * s); }
        public static Vector2 operator *(float s, Vector2 v) { return new Vector2(v.x * s, v.y * s); }
        public static Vector2 operator /(Vector2 v, float s) { return new Vector2(v.x / s, v.y / s); }
        public static Vector2 operator -(Vector2 v) { return new Vector2(-v.x, -v.y); }
        public static implicit operator Vector2(Vector3 v) { return new Vector2(v.x, v.y); }
        public static implicit operator Vector3(Vector2 v) { return new Vector3(v.x, v.y, 0f); }
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero { get { return new Vector3(0f, 0f, 0f); } }
        public static Vector3 one { get { return new Vector3(1f, 1f, 1f); } }
        public static Vector3 up { get { return new Vector3(0f, 1f, 0f); } }
        public static Vector3 down { get { return new Vector3(0f, -1f, 0f); } }
        public static Vector3 forward { get { return new Vector3(0f, 0f, 1f); } }
        public static Vector3 right { get { return new Vector3(1f, 0f, 0f); } }
        public static Vector3 left { get { return new Vector3(-1f, 0f, 0f); } }
        public float sqrMagnitude { get { return (x * x) + (y * y) + (z * z); } }
        public float magnitude { get { return (float)Math.Sqrt(sqrMagnitude); } }
        public Vector3 normalized { get { return magnitude > 0.0001f ? this / magnitude : zero; } }
        public void Normalize()
        {
            float mag = magnitude;
            if (mag > 0.0001f)
            {
                x /= mag;
                y /= mag;
                z /= mag;
            }
        }
        public static Vector3 operator *(Vector3 v, float s) { return new Vector3(v.x * s, v.y * s, v.z * s); }
        public static Vector3 operator *(float s, Vector3 v) { return new Vector3(v.x * s, v.y * s, v.z * s); }
        public static Vector3 operator +(Vector3 a, Vector3 b) { return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z); }
        public static Vector3 operator -(Vector3 a, Vector3 b) { return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z); }
        public static Vector3 operator -(Vector3 v) { return new Vector3(-v.x, -v.y, -v.z); }
        public static Vector3 operator /(Vector3 v, float s) { return new Vector3(v.x / s, v.y / s, v.z / s); }
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) { return new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t); }
        public static float Dot(Vector3 a, Vector3 b) { return (a.x * b.x) + (a.y * b.y) + (a.z * b.z); }
        public static Vector3 Cross(Vector3 a, Vector3 b) { return new Vector3((a.y * b.z) - (a.z * b.y), (a.z * b.x) - (a.x * b.z), (a.x * b.y) - (a.y * b.x)); }
        public static Vector3 Reflect(Vector3 inDirection, Vector3 inNormal) { return inDirection - (2f * Dot(inNormal, inDirection) * inNormal); }
    }

    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public Color(float r, float g, float b) { this.r = r; this.g = g; this.b = b; this.a = 1f; }
        public static Color black { get { return new Color(0f, 0f, 0f, 1f); } }
        public static Color white { get { return new Color(1f, 1f, 1f, 1f); } }
        public static Color clear { get { return new Color(0f, 0f, 0f, 0f); } }
        public static Color cyan { get { return new Color(0f, 1f, 1f, 1f); } }
        public static Color magenta { get { return new Color(1f, 0f, 1f, 1f); } }
        public float grayscale { get { return (0.299f * r) + (0.587f * g) + (0.114f * b); } }
        public float maxColorComponent { get { return Math.Max(r, Math.Max(g, b)); } }
        public static Color operator *(Color c, float s) { return new Color(c.r * s, c.g * s, c.b * s, c.a); }
        public static Color operator *(float s, Color c) { return new Color(c.r * s, c.g * s, c.b * s, c.a); }
        public static Color Lerp(Color a, Color b, float t) { return new Color(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, a.a + (b.a - a.a) * t); }
    }

    public struct Color32
    {
        public byte r, g, b, a;
        public Color32(byte r, byte g, byte b, byte a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static implicit operator Color(Color32 c) { return new Color(c.r / 255f, c.g / 255f, c.b / 255f, c.a / 255f); }
    }

    public struct Rect
    {
        public float x, y, width, height;
        public Rect(float x, float y, float w, float h) { this.x = x; this.y = y; this.width = w; this.height = h; }
    }

    public class Camera : Behaviour
    {
        public static Camera main { get { return null; } }
        public static Camera[] allCameras { get { return new Camera[0]; } }
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
        public RenderTexture targetTexture { get; set; }
        public StereoTargetEyeMask stereoTargetEye { get; set; }
        public bool orthographic { get; set; }
        public bool forceIntoRenderTexture { get; set; }
        public void Render() { }
        public Ray ScreenPointToRay(Vector3 pos) { return new Ray(pos, Vector3.forward); }
        public Vector3 WorldToViewportPoint(Vector3 position) { return Vector3.zero; }
    }

    public enum CameraClearFlags { Skybox, SolidColor, Depth, Nothing }

    public class AudioListener : Behaviour
    {
        public static float volume { get; set; }
        public static bool pause { get; set; }
    }

    public static class Mathf
    {
        public const float PI = 3.14159274f;
        public static float Sqrt(float v) { return (float)Math.Sqrt(v); }
        public static float Clamp(float v, float a, float b) { return v < a ? a : (v > b ? b : v); }
        public static int Clamp(int v, int a, int b) { return v < a ? a : (v > b ? b : v); }
        public static float Max(float a, float b) { return a > b ? a : b; }
        public static int Max(int a, int b) { return a > b ? a : b; }
        public static float Min(float a, float b) { return a < b ? a : b; }
        public static int Min(int a, int b) { return a < b ? a : b; }
        public static float Cos(float v) { return (float)Math.Cos(v); }
        public static float Sin(float v) { return (float)Math.Sin(v); }
        public static float Round(float v) { return (float)Math.Round(v); }
        public static float Log(float v, float b) { return (float)Math.Log(v, b); }
        public static float Pow(float v, float p) { return (float)Math.Pow(v, p); }
        public static float Abs(float v) { return v < 0f ? -v : v; }
        public static int Abs(int v) { return v < 0 ? -v : v; }
        public static float Lerp(float a, float b, float t) { return a + (b - a) * t; }
        public static float Clamp01(float v) { return Clamp(v, 0f, 1f); }
        public static float Exp(float v) { return (float)Math.Exp(v); }
        public static float PingPong(float t, float length) { return 0f; }
        public static int RoundToInt(float v) { return (int)Math.Round(v); }
        public static int FloorToInt(float v) { return (int)Math.Floor(v); }
        public static int CeilToInt(float v) { return (int)Math.Ceiling(v); }
        public static float Tan(float v) { return (float)Math.Tan(v); }
        public const float Deg2Rad = 0.0174532924f;
        public const float Rad2Deg = 57.29578f;
    }

    public static class Random
    {
        public static float value { get { return 0f; } }
        public static int Range(int min, int max) { return min; }
        public static float Range(float min, float max) { return min; }
        public static void InitState(int seed) { }
        public static Vector3 insideUnitSphere { get { return Vector3.zero; } }
    }

    public static class PlayerPrefs
    {
        public static int GetInt(string key, int fallback) { return fallback; }
        public static void SetInt(string key, int value) { }
        public static string GetString(string key, string fallback) { return fallback; }
        public static void SetString(string key, string value) { }
        public static void Save() { }
        public static bool HasKey(string key) { return false; }
        public static int GetInt(string key) { return 0; }
        public static float GetFloat(string key) { return 0f; }
        public static float GetFloat(string key, float fallback) { return fallback; }
        public static void SetFloat(string key, float value) { }
        public static void DeleteKey(string key) { }
    }

    public static class JsonUtility
    {
        public static T FromJson<T>(string json) where T : class { return null; }
        public static string ToJson(object value) { return ""; }
    }

    public static class Application
    {
        public static string persistentDataPath { get { return "/tmp"; } }
        public static string dataPath { get { return "/tmp/Assets"; } }
        public static int targetFrameRate { get; set; }
        public static bool isPlaying { get { return false; } }
        public static bool isBatchMode { get { return false; } }
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
        public static Rect safeArea { get { return new Rect(0f, 0f, width, height); } }
        public static Resolution currentResolution { get { return new Resolution { width = 1920, height = 1080 }; } }
        public static Resolution[] resolutions { get { return new Resolution[0]; } }
        public static void SetResolution(int width, int height, FullScreenMode mode) { }
    }

    public enum ColorSpace { Gamma, Linear }

    public static class QualitySettings
    {
        public static int vSyncCount { get; set; }
        public static int pixelLightCount { get; set; }
        public static ColorSpace activeColorSpace { get { return ColorSpace.Linear; } }
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
        public static Vector3 mousePosition { get { return Vector3.zero; } }
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
        JoystickButton6 = 336,
        A = 97,
        D = 100,
        S = 115,
        W = 119,
        F1 = 282,
        F9 = 290,
        F12 = 293,
        Return = 13,
        KeypadEnter = 271,
        LeftArrow = 276,
        RightArrow = 275,
    }

    public static class Time
    {
        public static int frameCount { get { return 0; } }
        public static float deltaTime { get { return 0.016f; } }
        public static float unscaledDeltaTime { get { return 0.016f; } }
        public static float unscaledTime { get { return 0f; } }
        public static float time { get { return 0f; } }
        public static float timeScale { get; set; }
        public static float fixedDeltaTime { get { return 0.02f; } }
        public static float captureDeltaTime { get; set; }
    }

    public static class Debug
    {
        public static void Log(object message) { }
        public static void LogWarning(object message) { }
        public static void LogError(object message) { }
    }

    public class Renderer : Component
    {
        public bool enabled { get; set; }
        public bool isVisible { get { return enabled; } }
        public Bounds bounds { get; set; }
        public Bounds localBounds { get; set; }
        public Material material { get; set; }
        public Material sharedMaterial { get; set; }
        public Material[] sharedMaterials { get; set; }
        public bool receiveShadows { get; set; }
        public Rendering.ShadowCastingMode shadowCastingMode { get; set; }
        public int sortingOrder { get; set; }
        public void GetPropertyBlock(MaterialPropertyBlock block) { }
        public void GetPropertyBlock(MaterialPropertyBlock block, int materialIndex) { }
        public void SetPropertyBlock(MaterialPropertyBlock block) { }
        public void SetPropertyBlock(MaterialPropertyBlock block, int materialIndex) { }
    }

    public class MaterialPropertyBlock
    {
        public void Clear() { }
        public void SetColor(int id, Color color) { }
        public void SetColor(string name, Color color) { }
    }

    public class Shader : Object
    {
        public static int PropertyToID(string name) { return 0; }
        public static Shader Find(string name) { return null; }
        public bool isSupported { get { return true; } }
    }

    public class Material : Object
    {
        public Shader shader { get; set; }
        public Material(Shader shader) { this.shader = shader; }
        public Material(Material source) { }
        public Color color { get; set; }
        public Texture mainTexture { get; set; }
        public Vector2 mainTextureOffset { get; set; }
        public Vector2 mainTextureScale { get; set; }
        public int renderQueue { get; set; }
        public void SetColor(string name, Color color) { }
        public void SetColor(int nameID, Color color) { }
        public Color GetColor(string name) { return Color.white; }
        public Color GetColor(int nameID) { return Color.white; }
        public void SetFloat(string name, float value) { }
        public void SetFloat(int nameID, float value) { }
        public float GetFloat(string name) { return 0f; }
        public float GetFloat(int nameID) { return 0f; }
        public void SetInt(string name, int value) { }
        public void SetInt(int nameID, int value) { }
        public void SetTexture(string name, Texture value) { }
        public void SetTexture(int nameID, Texture value) { }
        public bool HasProperty(string name) { return false; }
        public bool HasProperty(int nameID) { return false; }
        public void EnableKeyword(string keyword) { }
        public void DisableKeyword(string keyword) { }
    }

    public class Rigidbody : Component
    {
        public Vector3 linearVelocity { get; set; }
        public Vector3 angularVelocity { get; set; }
        public float linearDamping { get; set; }
        public float angularDamping { get; set; }
        public float mass { get; set; }
        public bool useGravity { get; set; }
        public bool isKinematic { get; set; }
        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }
        public CollisionDetectionMode collisionDetectionMode { get; set; }
        public RigidbodyConstraints constraints { get; set; }
        public RigidbodyInterpolation interpolation { get; set; }
        public void AddForce(Vector3 force) { }
        public void AddForce(Vector3 force, ForceMode mode) { }
    }

    public struct Quaternion
    {
        public static Quaternion identity { get { return new Quaternion(); } }
        public static Quaternion Euler(float x, float y, float z) { return identity; }
        public static Quaternion Euler(Vector3 euler) { return identity; }
        public static Quaternion LookRotation(Vector3 forward) { return identity; }
        public static Quaternion LookRotation(Vector3 forward, Vector3 upwards) { return identity; }
        public static Quaternion AngleAxis(float angle, Vector3 axis) { return identity; }
        public static Quaternion RotateTowards(Quaternion from, Quaternion to, float maxDegreesDelta) { return to; }
        public static Vector3 operator *(Quaternion rotation, Vector3 point) { return point; }
        public static Quaternion operator *(Quaternion a, Quaternion b) { return a; }
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

    public enum LightType { Spot, Point, Directional, Area }

    public enum LightShadows { None, Hard, Soft }

    public class Light : Behaviour
    {
        public LightType type { get; set; }
        public LightShadows shadows { get; set; }
        public Color color { get; set; }
        public float intensity { get; set; }
        public float range { get; set; }
        public float spotAngle { get; set; }
        public float innerSpotAngle { get; set; }
        public int cullingMask { get; set; }
        public float shadowStrength { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RequireComponentAttribute : Attribute
    {
        public RequireComponentAttribute(System.Type type) { }
        public RequireComponentAttribute(System.Type a, System.Type b) { }
        public RequireComponentAttribute(System.Type a, System.Type b, System.Type c) { }
    }

    public enum PrimitiveType { Sphere, Capsule, Cylinder, Cube, Plane, Quad }

    public enum TextureFormat { RGB24, RGBA32, ARGB32, RGBAHalf, RGBAFloat }

    public enum FilterMode { Point, Bilinear, Trilinear }

    public enum TextureWrapMode { Repeat, Clamp, Mirror, MirrorOnce }

    public enum TextAnchor
    {
        UpperLeft, UpperCenter, UpperRight,
        MiddleLeft, MiddleCenter, MiddleRight,
        LowerLeft, LowerCenter, LowerRight,
    }

    public enum FontStyle { Normal, Bold, Italic, BoldAndItalic }

    public enum TextAlignment { Left, Center, Right }

    public enum HorizontalWrapMode { Wrap, Overflow }

    public enum VerticalWrapMode { Truncate, Overflow }

    public enum Space { World, Self }

    public enum ForceMode { Force, Acceleration, Impulse, VelocityChange }

    public enum CollisionDetectionMode { Discrete, Continuous, ContinuousDynamic, ContinuousSpeculative }

    public enum RigidbodyInterpolation { None, Interpolate, Extrapolate }

    public enum RigidbodyConstraints
    {
        None = 0,
        FreezePositionX = 2,
        FreezePositionY = 4,
        FreezePositionZ = 8,
        FreezeRotationX = 16,
        FreezeRotationY = 32,
        FreezeRotationZ = 64,
        FreezePosition = 14,
        FreezeRotation = 112,
        FreezeAll = 126,
    }

    public enum StereoTargetEyeMask { None, Left, Right, Both }

    public enum RenderMode { ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace }

    public class Collider : Component
    {
        public bool enabled { get; set; }
        public bool isTrigger { get; set; }
        public Bounds bounds { get { return new Bounds(Vector3.zero, Vector3.zero); } }
    }

    public class BoxCollider : Collider
    {
        public Vector3 center { get; set; }
        public Vector3 size { get; set; }
    }

    public class SphereCollider : Collider
    {
        public Vector3 center { get; set; }
        public float radius { get; set; }
    }

    public class CapsuleCollider : Collider
    {
        public Vector3 center { get; set; }
        public float radius { get; set; }
        public float height { get; set; }
        public int direction { get; set; }
    }

    public class Collision
    {
        public Collider collider { get { return null; } }
    }

    public struct Bounds
    {
        public Vector3 center;
        public Vector3 size;
        public Bounds(Vector3 center, Vector3 size) { this.center = center; this.size = size; }
        public Vector3 extents { get { return size * 0.5f; } set { size = value * 2f; } }
        public Vector3 min { get { return center - extents; } set { } }
        public Vector3 max { get { return center + extents; } set { } }
        public void Encapsulate(Vector3 point) { }
        public void Encapsulate(Bounds bounds) { }
    }

    public class Font : Object
    {
        public static Font CreateDynamicFontFromOSFont(string fontname, int size) { return null; }
        public void RequestCharactersInTexture(string characters, int size, FontStyle style) { }
    }

    public class Texture : Object
    {
        public FilterMode filterMode { get; set; }
        public TextureWrapMode wrapMode { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Texture2D : Texture
    {
        public Texture2D(int width, int height) { }
        public Texture2D(int width, int height, TextureFormat format, bool mipChain) { }
        public void SetPixel(int x, int y, Color color) { }
        public void Apply() { }
        public void Apply(bool updateMipmaps, bool makeNoLongerReadable) { }
        public void ReadPixels(Rect source, int destX, int destY) { }
        public Color GetPixel(int x, int y) { return Color.black; }
        public byte[] EncodeToPNG() { return new byte[0]; }
    }

    public class Sprite : Object
    {
        public static Sprite Create(Texture2D texture, Rect rect, Vector2 pivot, float pixelsPerUnit) { return null; }
    }

    public class GraphicsTexture { }

    public class RenderTexture : Texture
    {
        public int width { get; set; }
        public int height { get; set; }
        public int antiAliasing { get; set; }
        public bool useMipMap { get; set; }
        public RenderTexture(int width, int height, int depth) { }
        public RenderTexture(int width, int height, int depth, RenderTextureFormat format) { }
        public static RenderTexture active { get; set; }
        public static RenderTexture GetTemporary(int width, int height, int depthBuffer, RenderTextureFormat format) { return null; }
        public static void ReleaseTemporary(RenderTexture temp) { }
        public void Create() { }
        public void Release() { }
    }

    // Unity 6000.6 Graphics.Blit. dest is RenderTexture or GraphicsTexture, so a
    // bare null is CS0121. Material+pass and the (Texture, RenderTexture, ...)
    // family are both present, matching the editor overloads.
    public static class Graphics
    {
        public static void Blit(Texture source, Material mat, int pass = -1) { }
        public static void Blit(Texture source, Material mat, int pass, int destDepthSlice) { }
        public static void Blit(Texture source, RenderTexture dest) { }
        public static void Blit(Texture source, RenderTexture dest, Material mat, int pass = -1) { }
        public static void Blit(Texture source, RenderTexture dest, Vector2 scale, Vector2 offset) { }
        public static void Blit(Texture source, RenderTexture dest, int sourceDepthSlice, int destDepthSlice) { }
        public static void Blit(Texture source, RenderTexture dest, Vector2 scale, Vector2 offset, int sourceDepthSlice, int destDepthSlice) { }
        public static void Blit(Texture source, GraphicsTexture dest) { }
        public static void Blit(Texture source, GraphicsTexture dest, Material mat, int pass = -1) { }
        public static void Blit(Texture source, GraphicsTexture dest, Vector2 scale, Vector2 offset) { }
        public static void Blit(Texture source, GraphicsTexture dest, int sourceDepthSlice, int destDepthSlice) { }
        public static void Blit(Texture source, GraphicsTexture dest, Vector2 scale, Vector2 offset, int sourceDepthSlice, int destDepthSlice) { }
    }

    public enum RenderTextureFormat
    {
        ARGB32,
        ARGBHalf,
        ARGBFloat,
        DefaultHDR,
    }

    public static class SystemInfo
    {
        public static bool SupportsRenderTextureFormat(RenderTextureFormat format)
        {
            return format == RenderTextureFormat.ARGB32;
        }

        public static int graphicsMemorySize { get { return 2048; } }
        public static string graphicsDeviceName { get { return ""; } }
        public static string deviceModel { get { return ""; } }
    }
    public class MeshRenderer : Renderer { }

    public class Mesh : Object
    {
        public Bounds bounds { get { return new Bounds(Vector3.zero, Vector3.zero); } }
        public bool isReadable { get { return true; } }
        public Vector3[] vertices { get; set; }
        public Vector2[] uv { get; set; }
        public int[] triangles { get; set; }
        public int subMeshCount { get { return 1; } }
        public int[] GetTriangles(int submesh) { return triangles; }
        public void SetTriangles(int[] triangles, int submesh) { this.triangles = triangles; }
        public void RecalculateBounds() { }
    }

    public class CanvasRenderer : Component
    {
        public void SetColor(Color color) { }
    }
    public class AudioClip : Object
    {
        public float length { get; set; }
        public int frequency { get; set; }
    }

    public class AudioSource : Behaviour
    {
        public AudioClip clip { get; set; }
        public bool loop { get; set; }
        public float pitch { get; set; }
        public int priority { get; set; }
        public int timeSamples { get; set; }
        public bool playOnAwake { get; set; }
        public float spatialBlend { get; set; }
        public float dopplerLevel { get; set; }
        public bool isPlaying { get; set; }
        public float volume { get; set; }
        public void Play() { }
        public void Stop() { }
        public void PlayOneShot(AudioClip clip, float volumeScale) { }
        public void Pause() { }
        public void UnPause() { }
    }

    public class AudioLowPassFilter : Behaviour
    {
        public float cutoffFrequency { get; set; }
        public float lowpassResonanceQ { get; set; }
    }
    public class Canvas : Behaviour
    {
        public bool overrideSorting { get; set; }
        public bool pixelPerfect { get; set; }
        public RenderMode renderMode { get; set; }
        public int sortingOrder { get; set; }
        public Camera worldCamera { get; set; }
        public float planeDistance { get; set; }
        public float scaleFactor { get; set; }
        public static void ForceUpdateCanvases() { }
        public static Material GetDefaultCanvasMaterial() { return null; }
    }

    public class CanvasGroup : Behaviour
    {
        public float alpha { get; set; }
        public bool blocksRaycasts { get; set; }
        public bool interactable { get; set; }
    }

    public class TextMesh : Component
    {
        public string text { get; set; }
        public Font font { get; set; }
        public int fontSize { get; set; }
        public FontStyle fontStyle { get; set; }
        public TextAnchor anchor { get; set; }
        public TextAlignment alignment { get; set; }
        public float characterSize { get; set; }
        public Color color { get; set; }
    }

    public class TextGenerator
    {
        public float GetPreferredWidth(string str, TextGenerationSettings settings) { return 0f; }
    }

    public class TextGenerationSettings
    {
        public HorizontalWrapMode horizontalOverflow { get; set; }
        public VerticalWrapMode verticalOverflow { get; set; }
        public float scaleFactor { get; set; }
    }

    public class Animator : Behaviour { }

    public struct Ray
    {
        public Vector3 origin;
        public Vector3 direction;
        public Ray(Vector3 origin, Vector3 direction) { this.origin = origin; this.direction = direction; }
        public Vector3 GetPoint(float distance) { return origin + (direction * distance); }
    }

    public struct Plane
    {
        public Plane(Vector3 inNormal, Vector3 inPoint) { }
        public bool Raycast(Ray ray, out float enter) { enter = 0f; return false; }
    }

    public class MeshFilter : Component
    {
        public Mesh sharedMesh { get; set; }
    }

    public class LineRenderer : Renderer
    {
        public int positionCount { get; set; }
        public float widthMultiplier { get; set; }
        public bool useWorldSpace { get; set; }
        public bool loop { get; set; }
        public Color startColor { get; set; }
        public Color endColor { get; set; }
        public void SetPosition(int index, Vector3 position) { }
    }

    public class TrailRenderer : Renderer
    {
        public float time { get; set; }
        public float startWidth { get; set; }
        public float endWidth { get; set; }
        public float minVertexDistance { get; set; }
        public Color startColor { get; set; }
        public Color endColor { get; set; }
    }

    public class YieldInstruction { }

    public class Coroutine : YieldInstruction { }

    public sealed class WaitForSeconds : YieldInstruction
    {
        public WaitForSeconds(float seconds) { }
    }

    public static class Resources
    {
        public static T Load<T>(string path) where T : Object { return null; }
        public static T GetBuiltinResource<T>(string path) where T : Object { return null; }
    }

    public enum QueryTriggerInteraction { UseGlobal, Ignore, Collide }

    public struct RaycastHit
    {
        public Vector3 point;
        public Vector3 normal;
        public Collider collider;
        public float distance;
    }

    public static class Physics
    {
        public static Vector3 gravity { get; set; }
        public static int DefaultRaycastLayers { get { return ~0; } }
        public static Collider[] OverlapSphere(Vector3 position, float radius) { return new Collider[0]; }
        public static RaycastHit[] RaycastAll(Vector3 origin, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            return new RaycastHit[0];
        }
    }

    public static class Gizmos
    {
        public static Color color { get; set; }
        public static void DrawWireSphere(Vector3 center, float radius) { }
    }

    public static class GL
    {
        public static void Clear(bool clearDepth, bool clearColor, Color backgroundColor) { }
    }

    public static class ColorUtility
    {
        public static bool TryParseHtmlString(string htmlString, out Color color) { color = Color.black; return false; }
    }

    public static class LayerMask
    {
        public static int NameToLayer(string layerName) { return 0; }
        public static string LayerToName(int layer) { return ""; }
    }

    public static class RectTransformUtility
    {
        public static bool RectangleContainsScreenPoint(RectTransform rect, Vector2 screenPoint, Camera cam) { return false; }
    }

    public static class RenderSettings
    {
        public static Rendering.AmbientMode ambientMode { get; set; }
        public static Color ambientLight { get; set; }
    }

    public static class ScreenCapture
    {
        public static void CaptureScreenshot(string filename) { }
    }

    public enum ParticleSystemSimulationSpace { Local, World }

    public enum ParticleSystemShapeType { Sphere, Hemisphere, Cone, Box }

    public class ParticleSystem : Component
    {
        public struct MainModule
        {
            public int maxParticles { get; set; }
            public float duration { get; set; }
            public bool playOnAwake { get; set; }
            public bool loop { get; set; }
            public float startLifetime { get; set; }
            public float startSpeed { get; set; }
            public ParticleSystem.MinMaxCurve startSize { get; set; }
            public Color startColor { get; set; }
            public ParticleSystemSimulationSpace simulationSpace { get; set; }
        }

        public struct EmissionModule
        {
            public bool enabled { get; set; }
            public ParticleSystem.MinMaxCurve rateOverTime { get; set; }
        }

        public struct ShapeModule
        {
            public bool enabled { get; set; }
            public float radius { get; set; }
            public ParticleSystemShapeType shapeType { get; set; }
        }

        public struct EmitParams
        {
            public Vector3 position { get; set; }
            public Vector3 velocity { get; set; }
            public float startLifetime { get; set; }
            public float startSize { get; set; }
            public Color startColor { get; set; }
        }

        public struct Particle
        {
            public Vector3 position;
            public Color32 startColor;
        }

        public struct MinMaxCurve
        {
            public MinMaxCurve(float constant) { }
            public MinMaxCurve(float min, float max) { }
            public static implicit operator MinMaxCurve(float v) { return new MinMaxCurve(v); }
        }

        public struct VelocityOverLifetimeModule
        {
            public bool enabled { get; set; }
            public ParticleSystemSimulationSpace space { get; set; }
            public ParticleSystem.MinMaxCurve x { get; set; }
            public ParticleSystem.MinMaxCurve y { get; set; }
            public ParticleSystem.MinMaxCurve z { get; set; }
        }

        public MainModule main { get { return new MainModule(); } }
        public EmissionModule emission { get { return new EmissionModule(); } }
        public ShapeModule shape { get { return new ShapeModule(); } }
        public VelocityOverLifetimeModule velocityOverLifetime { get { return new VelocityOverLifetimeModule(); } }
        public void Emit(EmitParams emitParams, int count) { }
        public void Clear() { }
        public void Stop() { }
        public int GetParticles(Particle[] particles) { return 0; }
        public void SetParticles(Particle[] particles, int size) { }
    }

    public class ParticleSystemRenderer : Renderer
    {
    }
}

namespace UnityEngine.Rendering
{
    public enum ShadowCastingMode { Off, On, TwoSided, ShadowsOnly }
    public enum AmbientMode { Skybox, Trilight, Flat, Custom }
    public enum BlendMode
    {
        Zero,
        One,
        DstColor,
        SrcColor,
        OneMinusDstColor,
        SrcAlpha,
        OneMinusSrcColor,
        DstAlpha,
        OneMinusDstAlpha,
        SrcAlphaSaturate,
        OneMinusSrcAlpha,
    }
}

namespace UnityEngine.Events
{
    public delegate void UnityAction();
    public delegate void UnityAction<T>(T arg);

    public class UnityEvent
    {
        public void AddListener(UnityAction call) { }
        public void RemoveListener(UnityAction call) { }
        public void Invoke() { }
    }

    public class UnityEvent<T>
    {
        public void AddListener(UnityAction<T> call) { }
        public void RemoveListener(UnityAction<T> call) { }
        public void Invoke(T arg) { }
    }
}

namespace UnityEngine.EventSystems
{
    public class EventSystem : UnityEngine.MonoBehaviour
    {
        public static EventSystem current { get; set; }
        public GameObject currentSelectedGameObject { get { return null; } }
        public bool sendNavigationEvents { get; set; }
        public void SetSelectedGameObject(GameObject selected) { }
    }

    public class BaseInputModule : UnityEngine.MonoBehaviour { }

    public class StandaloneInputModule : BaseInputModule
    {
        public string horizontalAxis { get; set; }
        public string verticalAxis { get; set; }
        public string submitButton { get; set; }
        public string cancelButton { get; set; }
    }

    public enum EventTriggerType { PointerEnter, PointerExit, PointerDown, PointerUp, PointerClick, Drag, BeginDrag, EndDrag, Submit, Cancel }
    public class BaseEventData { }
    public class PointerEventData : BaseEventData { }
    public class EventTrigger : UnityEngine.MonoBehaviour
    {
        public class TriggerEvent : UnityEvent<BaseEventData> { }
        public class Entry
        {
            public EventTriggerType eventID;
            public TriggerEvent callback = new TriggerEvent();
        }
        public System.Collections.Generic.List<Entry> triggers = new System.Collections.Generic.List<Entry>();
    }
}

namespace UnityEngine.UI
{
    public struct Navigation
    {
        public enum Mode { None, Horizontal, Vertical, Automatic, Explicit }
        public Mode mode;
    }

    public struct ColorBlock
    {
        public Color normalColor;
        public Color highlightedColor;
        public Color pressedColor;
        public Color selectedColor;
        public Color disabledColor;
        public float colorMultiplier;
        public float fadeDuration;
        public static ColorBlock defaultColorBlock { get { return new ColorBlock(); } }
    }

    public class Graphic : MonoBehaviour
    {
        public bool raycastTarget { get; set; }
        public Color color { get; set; }
        public Material material { get; set; }
        public RectTransform rectTransform { get { return null; } }
        public Canvas canvas { get { return null; } }
        public CanvasRenderer canvasRenderer { get { return null; } }
    }

    public class MaskableGraphic : Graphic { }

    public class Image : MaskableGraphic
    {
        public enum Type { Simple, Sliced, Tiled, Filled }
        public enum FillMethod { Horizontal, Vertical, Radial90, Radial180, Radial360 }
        public enum OriginHorizontal { Left, Right }
        public enum OriginVertical { Bottom, Top }
        public enum Origin90 { BottomLeft, TopLeft, TopRight, BottomRight }
        public enum Origin180 { Bottom, Left, Top, Right }
        public enum Origin360 { Bottom, Right, Top, Left }
        public Sprite sprite { get; set; }
        public Type type { get; set; }
        public bool preserveAspect { get; set; }
        public FillMethod fillMethod { get; set; }
        public int fillOrigin { get; set; }
        public float fillAmount { get; set; }
        public bool fillClockwise { get; set; }
    }

    public class RawImage : MaskableGraphic
    {
        public Texture texture { get; set; }
        public Rect uvRect { get; set; }
    }

    public class Text : MaskableGraphic
    {
        public string text { get; set; }
        public Font font { get; set; }
        public int fontSize { get; set; }
        public FontStyle fontStyle { get; set; }
        public TextAnchor alignment { get; set; }
        public bool alignByGeometry { get; set; }
        public bool supportRichText { get; set; }
        public HorizontalWrapMode horizontalOverflow { get; set; }
        public VerticalWrapMode verticalOverflow { get; set; }
        public float lineSpacing { get; set; }
        public bool resizeTextForBestFit { get; set; }
        public int resizeTextMinSize { get; set; }
        public int resizeTextMaxSize { get; set; }
        public float preferredWidth { get { return 0f; } }
        public float preferredHeight { get { return 0f; } }
        public TextGenerationSettings GetGenerationSettings(Vector2 extents) { return new TextGenerationSettings(); }
    }

    public class Selectable : MonoBehaviour
    {
        public enum Transition { None, ColorTint, SpriteSwap, Animation }
        public bool interactable { get; set; }
        public Transition transition { get; set; }
        public Graphic targetGraphic { get; set; }
        public Navigation navigation { get; set; }
        public ColorBlock colors { get; set; }
        public bool IsInteractable() { return interactable; }
    }

    public class Button : Selectable
    {
        public class ButtonClickedEvent : UnityEvent { }
        public ButtonClickedEvent onClick { get { return new ButtonClickedEvent(); } }
    }

    public class Slider : Selectable
    {
        public class SliderEvent : UnityEvent<float> { }
        public SliderEvent onValueChanged { get { return new SliderEvent(); } }
        public RectTransform fillRect { get; set; }
        public RectTransform handleRect { get; set; }
        public float minValue { get; set; }
        public float maxValue { get; set; }
        public float value { get; set; }
        public void SetValueWithoutNotify(float input) { }
    }

    public class Scrollbar : Selectable
    {
        public enum Direction { LeftToRight, RightToLeft, BottomToTop, TopToBottom }
        public class ScrollEvent : UnityEvent<float> { }
        public ScrollEvent onValueChanged { get { return new ScrollEvent(); } }
        public Direction direction { get; set; }
        public float size { get; set; }
        public float value { get; set; }
        public RectTransform handleRect { get; set; }
    }

    public class Shadow : MonoBehaviour
    {
        public Color effectColor { get; set; }
        public Vector2 effectDistance { get; set; }
        public bool useGraphicAlpha { get; set; }
    }

    public class Outline : Shadow { }

    public class CanvasScaler : MonoBehaviour
    {
        public enum ScaleMode { ConstantPixelSize, ScaleWithScreenSize, ConstantPhysicalSize }
        public ScaleMode uiScaleMode { get; set; }
        public Vector2 referenceResolution { get; set; }
        public float matchWidthOrHeight { get; set; }
    }

    public class GraphicRaycaster : MonoBehaviour { }

    public class RectMask2D : MonoBehaviour { }
}
