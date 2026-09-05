#!/usr/bin/env python3
"""Create Unity YAML (.meta, materials, prefabs, Play scene) with stable GUIDs."""

from __future__ import annotations

import hashlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def guid_for(relative: str) -> str:
    return hashlib.md5(f"asteroids-gone-rogue:{relative}".encode("utf-8")).hexdigest()


def write(path: Path, content: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content.rstrip() + "\n", encoding="utf-8")


def script_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
MonoImporter:
  externalObjects: {{}}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def default_meta(guid: str, importer: str = "DefaultImporter") -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
{importer}:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def folder_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def audio_meta(guid: str, streaming: bool) -> str:
    load_type = 2 if streaming else 0
    preload = 0 if streaming else 1
    return f"""fileFormatVersion: 2
guid: {guid}
AudioImporter:
  externalObjects: {{}}
  serializedVersion: 7
  defaultSettings:
    serializedVersion: 2
    loadType: {load_type}
    sampleRateSetting: 0
    sampleRateOverride: 44100
    compressionFormat: 1
    quality: 1
    conversionMode: 0
  platformSettingOverrides: {{}}
  forceToMono: 0
  normalize: 1
  preloadAudioData: {preload}
  loadInBackground: 0
  ambisonic: 0
  3D: 1
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def markdown_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
TextScriptImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def fbx_meta(guid: str) -> str:
    """Unity 2022.3 ModelImporter: scale 1, no colliders, no animation."""
    return f"""fileFormatVersion: 2
guid: {guid}
ModelImporter:
  serializedVersion: 213
  internalIDToNameTable: []
  externalObjects: {{}}
  materials:
    materialImportMode: 2
    materialName: 0
    materialSearch: 1
    materialLocation: 1
  animations:
    legacyGenerateAnimations: 4
    bakeSimulation: 0
    resampleCurves: 1
    optimizeGameObjects: 0
    motionNodeName: 
    rigImportErrors: 
    rigImportWarnings: 
    animationImportErrors: 
    animationImportWarnings: 
    animationRetargetingWarnings: 
    animationDoRetargetingWarnings: 0
    importAnimatedCustomProperties: 0
    importConstraints: 0
    animationCompression: 1
    animationRotationError: 0.5
    animationPositionError: 0.5
    animationScaleError: 0.5
    animationWrapMode: 0
    extraExposedTransformPaths: []
    extraUserProperties: []
    clipAnimations: []
    isReadable: 0
  meshes:
    lODScreenPercentages: []
    globalScale: 1
    meshCompression: 0
    addColliders: 0
    useSRGBMaterialColor: 1
    sortHierarchyByName: 1
    importVisibility: 1
    importBlendShapes: 1
    importCameras: 0
    importLights: 0
    fileIdsGeneration: 2
    swapUVChannels: 0
    generateSecondaryUV: 0
    useFileUnits: 1
    keepQuads: 0
    weldVertices: 1
    bakeAxisConversion: 0
    preserveHierarchy: 0
    skinWeightsMode: 0
    maxBonesPerVertex: 4
    minBoneWeight: 0.001
    optimizeBones: 1
    meshOptimizationFlags: -1
    indexFormat: 0
    secondaryUVAngleDistortion: 8
    secondaryUVAreaDistortion: 15.000001
    secondaryUVHardAngle: 88
    secondaryUVMarginMethod: 1
    secondaryUVMinLightmapResolution: 40
    secondaryUVMinObjectScale: 1
    secondaryUVPackMargin: 4
    useFileScale: 1
    strictVertexDataChecks: 0
  tangentSpace:
    normalSmoothAngle: 60
    normalImportMode: 0
    tangentImportMode: 3
    normalCalculationMode: 4
    legacyComputeAllNormalsFromSmoothingGroupsWhenMeshHasBlendShapes: 0
    blendShapeNormalImportMode: 1
    normalSmoothingSource: 0
  referencedClips: []
  importAnimation: 0
  humanDescription:
    serializedVersion: 3
    human: []
    skeleton: []
    armTwist: 0.5
    foreArmTwist: 0.5
    upperLegTwist: 0.5
    legTwist: 0.5
    armStretch: 0.05
    legStretch: 0.05
    feetSpacing: 0
    globalScale: 1
    rootMotionBoneName: 
    hasTranslationDoF: 0
    hasExtraRoot: 0
    skeletonHasParents: 1
  lastHumanDescriptionAvatarSource: {{instanceID: 0}}
  autoGenerateAvatarMappingIfUnspecified: 1
  animationType: 0
  humanoidOversampling: 1
  avatarSetup: 0
  addHumanoidExtraRootOnlyWhenUsingAvatar: 1
  importBlendShapeDeformPercent: 1
  remapMaterialsIfMaterialImportModeIsNone: 0
  additionalBone: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def material_yaml(name: str, color: tuple[float, float, float, float], metallic: float, gloss: float, emission=None) -> str:
    r, g, b, a = color
    er, eg, eb = emission if emission else (0.0, 0.0, 0.0)
    keywords = " []"
    if emission:
        keywords = "\n  - _EMISSION"
    return f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!21 &2100000
Material:
  serializedVersion: 8
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: {name}
  m_Shader: {{fileID: 46, guid: 0000000000000000f000000000000000, type: 0}}
  m_Parent: {{fileID: 0}}
  m_ModifiedSerializedProperties: 0
  m_ValidKeywords:{keywords}
  m_InvalidKeywords: []
  m_LightmapFlags: {2 if emission else 4}
  m_EnableInstancingVariants: 0
  m_DoubleSidedGI: 0
  m_CustomRenderQueue: -1
  stringTagMap: {{}}
  disabledShaderPasses: []
  m_LockedProperties: 
  m_SavedProperties:
    serializedVersion: 3
    m_TexEnvs:
    - _BumpMap:
        m_Texture: {{fileID: 0}}
        m_Scale: {{x: 1, y: 1}}
        m_Offset: {{x: 0, y: 0}}
    - _DetailAlbedoMap:
        m_Texture: {{fileID: 0}}
        m_Scale: {{x: 1, y: 1}}
        m_Offset: {{x: 0, y: 0}}
    - _DetailMask:
        m_Texture: {{fileID: 0}}
        m_Scale: {{x: 1, y: 1}}
        m_Offset: {{x: 0, y: 0}}
    - _DetailNormalMap:
        m_Texture: {{fileID: 0}}
        m_Scale: {{x: 1, y: 1}}
        m_Offset: {{x: 0, y: 0}}
    - _EmissionMap:
        m_Texture: {{fileID: 0}}
        m_Scale: {{x: 1, y: 1}}
        m_Offset: {{x: 0, y: 0}}
    - _MainTex:
        m_Texture: {{fileID: 0}}
        m_Scale: {{x: 1, y: 1}}
        m_Offset: {{x: 0, y: 0}}
    - _MetallicGlossMap:
        m_Texture: {{fileID: 0}}
        m_Scale: {{x: 1, y: 1}}
        m_Offset: {{x: 0, y: 0}}
    - _OcclusionMap:
        m_Texture: {{fileID: 0}}
        m_Scale: {{x: 1, y: 1}}
        m_Offset: {{x: 0, y: 0}}
    - _ParallaxMap:
        m_Texture: {{fileID: 0}}
        m_Scale: {{x: 1, y: 1}}
        m_Offset: {{x: 0, y: 0}}
    m_Ints: []
    m_Floats:
    - _BumpScale: 1
    - _Cutoff: 0.5
    - _DetailNormalMapScale: 1
    - _DstBlend: 0
    - _GlossMapScale: 1
    - _Glossiness: {gloss}
    - _GlossyReflections: 1
    - _Metallic: {metallic}
    - _Mode: 0
    - _OcclusionStrength: 1
    - _Parallax: 0.02
    - _SmoothnessTextureChannel: 0
    - _SpecularHighlights: 1
    - _SrcBlend: 1
    - _UVSec: 0
    - _ZWrite: 1
    m_Colors:
    - _Color: {{r: {r}, g: {g}, b: {b}, a: {a}}}
    - _EmissionColor: {{r: {er}, g: {eg}, b: {eb}, a: 1}}
  m_BuildTextureStacks: []
"""


MESH = {
    "cube": "{fileID: 10202, guid: 0000000000000000e000000000000000, type: 0}",
    "sphere": "{fileID: 10207, guid: 0000000000000000e000000000000000, type: 0}",
    "capsule": "{fileID: 10208, guid: 0000000000000000e000000000000000, type: 0}",
    "cylinder": "{fileID: 10206, guid: 0000000000000000e000000000000000, type: 0}",
}


def primitive_prefab(name: str, mesh_key: str, mat_guid: str, scale: tuple[float, float, float]) -> str:
    sx, sy, sz = scale
    return f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &100000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 100001}}
  - component: {{fileID: 100002}}
  - component: {{fileID: 100003}}
  m_Layer: 0
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &100001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 100000}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: {sx}, y: {sy}, z: {sz}}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!33 &100002
MeshFilter:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 100000}}
  m_Mesh: {MESH[mesh_key]}
--- !u!23 &100003
MeshRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 100000}}
  m_Enabled: 1
  m_CastShadows: 1
  m_ReceiveShadows: 1
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 2
  m_RayTraceProcedural: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {{fileID: 2100000, guid: {mat_guid}, type: 2}}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {{fileID: 0}}
  m_ProbeAnchor: {{fileID: 0}}
  m_LightProbeVolumeOverride: {{fileID: 0}}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 3
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {{fileID: 0}}
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 0
  m_AdditionalVertexStreams: {{fileID: 0}}
"""


def play_scene(bootstrap_guid: str) -> str:
    # Eventsystem built-in script GUIDs (com.unity.ugui / UnityEngine.UI)
    event_system = "76c392e42b5098c458856cdf6ecaaaa1"
    standalone = "4f231eb8fc47f54ca11b152d6d181d1e"
    return f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!29 &1
OcclusionCullingSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_OcclusionBakeSettings:
    smallestOccluder: 5
    smallestHole: 0.25
    backfaceThreshold: 100
  m_SceneGUID: 00000000000000000000000000000000
  m_OcclusionCullingData: {{fileID: 0}}
--- !u!104 &2
RenderSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 9
  m_Fog: 0
  m_FogColor: {{r: 0.5, g: 0.5, b: 0.5, a: 1}}
  m_FogMode: 3
  m_FogDensity: 0.01
  m_LinearFogStart: 0
  m_LinearFogEnd: 300
  m_AmbientSkyColor: {{r: 0.12, g: 0.14, b: 0.18, a: 1}}
  m_AmbientEquatorColor: {{r: 0.11, g: 0.12, b: 0.15, a: 1}}
  m_AmbientGroundColor: {{r: 0.05, g: 0.05, b: 0.06, a: 1}}
  m_AmbientIntensity: 1
  m_AmbientMode: 3
  m_SubtractiveShadowColor: {{r: 0.42, g: 0.48, b: 0.63, a: 1}}
  m_SkyboxMaterial: {{fileID: 0}}
  m_HaloStrength: 0.5
  m_FlareStrength: 1
  m_FlareFadeSpeed: 3
  m_HaloTexture: {{fileID: 0}}
  m_SpotCookie: {{fileID: 10001, guid: 0000000000000000e000000000000000, type: 0}}
  m_DefaultReflectionMode: 0
  m_DefaultReflectionResolution: 128
  m_ReflectionBounces: 1
  m_ReflectionIntensity: 1
  m_CustomReflection: {{fileID: 0}}
  m_Sun: {{fileID: 200002}}
  m_UseRadianceAmbientProbe: 0
--- !u!157 &3
LightmapSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 12
  m_GIWorkflowMode: 1
  m_GISettings:
    serializedVersion: 2
    m_BounceScale: 1
    m_IndirectOutputScale: 1
    m_AlbedoBoost: 1
    m_EnvironmentLightingMode: 0
    m_EnableBakedLightmaps: 0
    m_EnableRealtimeLightmaps: 0
  m_LightmapEditorSettings:
    serializedVersion: 12
    m_Resolution: 2
    m_BakeResolution: 40
    m_AtlasSize: 1024
    m_AO: 0
    m_AOMaxDistance: 1
    m_CompAOExponent: 1
    m_CompAOExponentDirect: 0
    m_ExtractAmbientOcclusion: 0
    m_Padding: 2
    m_LightmapParameters: {{fileID: 0}}
    m_LightmapsBakeMode: 1
    m_TextureCompression: 1
    m_FinalGather: 0
    m_FinalGatherFiltering: 1
    m_FinalGatherRayCount: 256
    m_ReflectionCompression: 2
    m_MixedBakeMode: 2
    m_BakeBackend: 1
    m_PVRSampling: 1
    m_PVRDirectSampleCount: 32
    m_PVRSampleCount: 512
    m_PVRBounces: 2
    m_PVREnvironmentSampleCount: 256
    m_PVREnvironmentReferencePointCount: 2048
    m_PVRFilteringMode: 1
    m_PVRDenoiserTypeDirect: 1
    m_PVRDenoiserTypeIndirect: 1
    m_PVRDenoiserTypeAO: 1
    m_PVRFilterTypeDirect: 0
    m_PVRFilterTypeIndirect: 0
    m_PVRFilterTypeAO: 0
    m_PVREnvironmentMIS: 1
    m_PVRCulling: 1
    m_PVRFilteringGaussRadiusDirect: 1
    m_PVRFilteringGaussRadiusIndirect: 5
    m_PVRFilteringGaussRadiusAO: 2
    m_PVRFilteringAtrousPositionSigmaDirect: 0.5
    m_PVRFilteringAtrousPositionSigmaIndirect: 2
    m_PVRFilteringAtrousPositionSigmaAO: 1
    m_ExportTrainingData: 0
    m_TrainingDataDestination: TrainingData
    m_LightProbeSampleCountMultiplier: 4
  m_LightingDataAsset: {{fileID: 0}}
  m_LightingSettings: {{fileID: 0}}
--- !u!196 &4
NavMeshSettings:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_BuildSettings:
    serializedVersion: 3
    agentTypeID: 0
    agentRadius: 0.5
    agentHeight: 2
    agentSlope: 45
    agentClimb: 0.4
    ledgeDropHeight: 0
    maxJumpAcrossDistance: 0
    minRegionArea: 2
    manualCellSize: 0
    cellSize: 0.16666667
    manualTileSize: 0
    tileSize: 256
    buildHeightMesh: 0
    maxJobWorkers: 0
    preserveTilesOutsideBounds: 0
    debug:
      m_Flags: 0
  m_NavMeshData: {{fileID: 0}}
--- !u!1 &100000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 100001}}
  - component: {{fileID: 100002}}
  - component: {{fileID: 100003}}
  m_Layer: 0
  m_Name: Main Camera
  m_TagString: MainCamera
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &100001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 100000}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0.46251225, y: 0, z: 0, w: 0.8866119}}
  m_LocalPosition: {{x: 0, y: 26, z: -16}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 55.1, y: 0, z: 0}}
--- !u!20 &100002
Camera:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 100000}}
  m_Enabled: 1
  serializedVersion: 2
  m_ClearFlags: 2
  m_BackGroundColor: {{r: 0.02, g: 0.03, b: 0.05, a: 0}}
  m_projectionMatrixMode: 1
  m_GateFitMode: 2
  m_FOVAxisMode: 0
  m_Iso: 200
  m_ShutterSpeed: 0.005
  m_Aperture: 16
  m_FocusDistance: 10
  m_FocalLength: 50
  m_BladeCount: 5
  m_Curvature: {{x: 2, y: 11}}
  m_BarrelClipping: 0.25
  m_Anamorphism: 0
  m_SensorSize: {{x: 36, y: 24}}
  m_LensShift: {{x: 0, y: 0}}
  m_NormalizedViewPortRect:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1
    height: 1
  near clip plane: 0.1
  far clip plane: 200
  field of view: 50
  orthographic: 0
  orthographic size: 5
  m_Depth: -1
  m_CullingMask:
    serializedVersion: 2
    m_Bits: 4294967295
  m_RenderingPath: -1
  m_TargetTexture: {{fileID: 0}}
  m_TargetDisplay: 0
  m_TargetEye: 3
  m_HDR: 1
  m_AllowMSAA: 1
  m_AllowDynamicResolution: 0
  m_ForceIntoRT: 0
  m_OcclusionCulling: 1
  m_StereoConvergence: 10
  m_StereoSeparation: 0.022
--- !u!81 &100003
AudioListener:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 100000}}
  m_Enabled: 1
--- !u!1 &200000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 200001}}
  - component: {{fileID: 200002}}
  m_Layer: 0
  m_Name: Directional Light
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &200001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 200000}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0.35355338, y: -0.35355338, z: 0.1464466, w: 0.8535534}}
  m_LocalPosition: {{x: 0, y: 12, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 42, y: -35, z: 0}}
--- !u!108 &200002
Light:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 200000}}
  m_Enabled: 1
  serializedVersion: 10
  m_Type: 1
  m_Shape: 0
  m_Color: {{r: 0.92, g: 0.95, b: 1, a: 1}}
  m_Intensity: 1.15
  m_Range: 10
  m_SpotAngle: 30
  m_InnerSpotAngle: 21.80208
  m_CookieSize: 10
  m_Shadows:
    m_Type: 2
    m_Resolution: -1
    m_CustomResolution: -1
    m_Strength: 0.6
    m_Bias: 0.05
    m_NormalBias: 0.4
    m_NearPlane: 0.2
    m_CullingMatrixOverride:
      e00: 1
      e01: 0
      e02: 0
      e03: 0
      e10: 0
      e11: 1
      e12: 0
      e13: 0
      e20: 0
      e21: 0
      e22: 1
      e23: 0
      e30: 0
      e31: 0
      e32: 0
      e33: 1
    m_UseCullingMatrixOverride: 0
  m_Cookie: {{fileID: 0}}
  m_DrawHalo: 0
  m_Flare: {{fileID: 0}}
  m_RenderMode: 0
  m_CullingMask:
    serializedVersion: 2
    m_Bits: 4294967295
  m_RenderingLayerMask: 1
  m_Lightmapping: 4
  m_LightShadowCasterMode: 0
  m_AreaSize: {{x: 1, y: 1}}
  m_BounceIntensity: 1
  m_ColorTemperature: 6570
  m_UseColorTemperature: 0
  m_BoundingSphereOverride: {{x: 0, y: 0, z: 0, w: 0}}
  m_UseBoundingSphereOverride: 0
  m_UseViewFrustumForShadowCasterCull: 1
  m_ShadowRadius: 0
  m_ShadowAngle: 0
--- !u!1 &300000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 300001}}
  - component: {{fileID: 300002}}
  m_Layer: 0
  m_Name: GameSystems
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &300001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 300000}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!114 &300002
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 300000}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {bootstrap_guid}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
--- !u!1 &400000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 400001}}
  - component: {{fileID: 400002}}
  - component: {{fileID: 400003}}
  m_Layer: 0
  m_Name: EventSystem
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &400001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 400000}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!114 &400002
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 400000}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {event_system}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_FirstSelected: {{fileID: 0}}
  m_sendNavigationEvents: 1
  m_DragThreshold: 10
--- !u!114 &400003
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 400000}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {standalone}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_SendPointerHoverToParent: 1
  m_HorizontalAxis: Horizontal
  m_VerticalAxis: Vertical
  m_SubmitButton: Submit
  m_CancelButton: Cancel
  m_InputActionsPerSecond: 10
  m_RepeatDelay: 0.5
  m_ForceModuleActive: 0
"""


def extra_project_settings() -> None:
    write(
        ROOT / "ProjectSettings/TimeManager.asset",
        """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!5 &1
TimeManager:
  m_ObjectHideFlags: 0
  Fixed Timestep: 0.02
  Maximum Allowed Timestep: 0.1
  m_TimeScale: 1
  Maximum Particle Timestep: 0.03
""",
    )
    write(
        ROOT / "ProjectSettings/DynamicsManager.asset",
        """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!55 &1
PhysicsManager:
  m_ObjectHideFlags: 0
  serializedVersion: 13
  m_Gravity: {x: 0, y: 0, z: 0}
  m_DefaultMaterial: {fileID: 0}
  m_BounceThreshold: 2
  m_DefaultMaxDepenetrationVelocity: 10
  m_SleepThreshold: 0.005
  m_DefaultContactOffset: 0.01
  m_DefaultSolverIterations: 6
  m_DefaultSolverVelocityIterations: 1
  m_QueriesHitBackfaces: 0
  m_QueriesHitTriggers: 1
  m_EnableAdaptiveForce: 0
  m_ClothInterCollisionDistance: 0.1
  m_ClothInterCollisionStiffness: 0.2
  m_ContactsGeneration: 1
  m_LayerCollisionMatrix: ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff
  m_AutoSimulation: 1
  m_AutoSyncTransforms: 0
  m_ReuseCollisionCallbacks: 1
  m_ClothInterCollisionSettingsToggle: 0
  m_ClothGravity: {x: 0, y: -9.81, z: 0}
  m_ContactPairsMode: 0
  m_BroadphaseType: 0
  m_WorldBounds:
    m_Center: {x: 0, y: 0, z: 0}
    m_Extent: {x: 250, y: 250, z: 250}
  m_WorldSubdivisions: 8
  m_FrictionType: 0
  m_EnableEnhancedDeterminism: 0
  m_EnableUnifiedHeightmaps: 1
  m_ImprovedPatchFriction: 0
  m_SolverType: 0
  m_DefaultMaxAngularSpeed: 50
""",
    )
    write(
        ROOT / "ProjectSettings/AudioManager.asset",
        """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!11 &1
AudioManager:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Volume: 1
  Rolloff Scale: 1
  Doppler Factor: 1
  Default Speaker Mode: 2
  m_SampleRate: 0
  m_DSPBufferSize: 1024
  m_VirtualVoiceCount: 512
  m_RealVoiceCount: 32
  m_EnableOutputSuspension: 1
  m_SpatializerPlugin: 
  m_AmbisonicDecoderPlugin: 
  m_DisableAudio: 0
  m_VirtualizeEffects: 1
  m_RequestedDSPBufferSize: 0
""",
    )


def main() -> None:
    extra_project_settings()

    folders = [
        "Assets",
        "Assets/Art",
        "Assets/Art/Import",
        "Assets/Art/Materials",
        "Assets/Art/Prefabs",
        "Assets/Scenes",
        "Assets/Scripts",
        "Assets/Scripts/Core",
        "Assets/Scripts/Content",
        "Assets/Scripts/Player",
        "Assets/Scripts/Combat",
        "Assets/Scripts/Hangar",
        "Assets/Scripts/UI",
        "Assets/Editor",
        "Assets/Audio",
        "Assets/Resources",
        "Assets/Resources/Art",
        "Assets/Resources/Art/Import",
        "Assets/Resources/Audio",
        "Assets/Resources/Audio/Sfx",
        "Assets/Resources/Audio/Music",
    ]
    for folder in folders:
        write(ROOT / f"{folder}.meta", folder_meta(guid_for(folder + "/")))

    for cs in ROOT.joinpath("Assets").rglob("*.cs"):
        rel = cs.relative_to(ROOT).as_posix()
        write(cs.with_suffix(".cs.meta"), script_meta(guid_for(rel)))

    for md in ROOT.joinpath("Assets").rglob("*.md"):
        rel = md.relative_to(ROOT).as_posix()
        write(md.with_suffix(".md.meta"), markdown_meta(guid_for(rel)))

    for txt in ROOT.joinpath("Assets").rglob("*.txt"):
        rel = txt.relative_to(ROOT).as_posix()
        write(txt.with_suffix(".txt.meta"), markdown_meta(guid_for(rel)))

    for ogg in ROOT.joinpath("Assets").rglob("*.ogg"):
        rel = ogg.relative_to(ROOT).as_posix()
        write(ogg.with_suffix(".ogg.meta"), audio_meta(guid_for(rel), streaming="Music" in rel))

    for fbx in ROOT.joinpath("Assets").rglob("*.fbx"):
        rel = fbx.relative_to(ROOT).as_posix()
        write(Path(str(fbx) + ".meta"), fbx_meta(guid_for(rel)))

    materials = {
        "Mat_Ship_Hull": ((0.45, 0.52, 0.58, 1.0), 0.45, 0.35, None),
        "Mat_Ship_Accent": ((1.0, 0.55, 0.14, 1.0), 0.2, 0.55, (1.0, 0.4, 0.05)),
        "Mat_Ship_Glass": ((0.35, 0.7, 0.95, 0.28), 0.0, 0.7, (0.2, 0.5, 0.8)),
        "Mat_Ship_Glow": ((1.0, 0.55, 0.15, 1.0), 0.0, 0.15, (1.0, 0.45, 0.05)),
        "Mat_Asteroid": ((0.38, 0.32, 0.28, 1.0), 0.05, 0.18, None),
        "Mat_Enemy": ((0.72, 0.16, 0.18, 1.0), 0.25, 0.4, (0.6, 0.05, 0.08)),
        "Mat_Arena": ((0.07, 0.11, 0.14, 1.0), 0.1, 0.12, None),
        "Mat_Projectile": ((1.0, 0.85, 0.25, 1.0), 0.0, 0.2, (1.0, 0.7, 0.1)),
    }
    mat_guids = {}
    for name, (color, metal, gloss, emission) in materials.items():
        rel = f"Assets/Art/Materials/{name}.mat"
        guid = guid_for(rel)
        mat_guids[name] = guid
        write(ROOT / rel, material_yaml(name, color, metal, gloss, emission))
        write(ROOT / f"{rel}.meta", default_meta(guid, "NativeFormatImporter"))

    prefabs = [
        ("Ship_Body", "cube", "Mat_Ship_Hull", (1.1, 0.6, 1.5)),
        ("Ship_Nose", "cube", "Mat_Ship_Accent", (0.45, 0.35, 0.7)),
        ("Ship_Engine", "cube", "Mat_Ship_Hull", (0.8, 0.45, 0.55)),
        ("Ship_Nose_Upgrade01", "cube", "Mat_Ship_Accent", (0.7, 0.25, 1.05)),
        ("Ship_Engine_Upgrade01", "cube", "Mat_Ship_Glow", (0.95, 0.5, 0.7)),
        ("Ship_Complete", "capsule", "Mat_Ship_Hull", (1.0, 1.0, 1.0)),
        ("Asteroid_Large", "sphere", "Mat_Asteroid", (4.8, 4.8, 4.8)),
        ("Asteroid_Small", "sphere", "Mat_Asteroid", (1.8, 1.8, 1.8)),
        ("Asteroid_VariantB_Large", "cube", "Mat_Asteroid", (4.8, 3.4, 4.8)),
        ("Asteroid_VariantB_Small", "cube", "Mat_Asteroid", (1.8, 1.25, 1.8)),
        ("Enemy_01", "capsule", "Mat_Enemy", (0.7, 0.7, 0.7)),
        ("Arena_Blockout", "cylinder", "Mat_Arena", (44.0, 0.04, 44.0)),
        ("Hangar_Crate", "cube", "Mat_Arena", (1.3, 1.1, 1.3)),
        ("Hangar_Terminal", "cube", "Mat_Ship_Hull", (1.1, 1.8, 0.55)),
        ("Hangar_LightPillar", "cylinder", "Mat_Ship_Glow", (0.55, 2.4, 0.55)),
        ("Projectile", "sphere", "Mat_Projectile", (0.22, 0.22, 0.55)),
    ]
    for name, mesh, mat, scale in prefabs:
        rel = f"Assets/Art/Prefabs/{name}.prefab"
        guid = guid_for(rel)
        write(ROOT / rel, primitive_prefab(name, mesh, mat_guids[mat], scale))
        write(ROOT / f"{rel}.meta", default_meta(guid, "PrefabImporter"))

    scene_rel = "Assets/Scenes/Play.unity"
    # Keep in sync with EditorBuildSettings.asset
    scene_guid = "3a8c0e1b5d7246f0a2c9d4e6b1f70835"
    bootstrap_guid = guid_for("Assets/Scripts/Content/GameBootstrap.cs")
    write(ROOT / scene_rel, play_scene(bootstrap_guid))
    write(
        ROOT / f"{scene_rel}.meta",
        f"""fileFormatVersion: 2
guid: {scene_guid}
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
""",
    )

    print("bootstrap", bootstrap_guid)
    print("scene", scene_guid)
    print("generated unity yaml")


if __name__ == "__main__":
    main()
