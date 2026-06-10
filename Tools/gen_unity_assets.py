#!/usr/bin/env python3
"""
Deterministic Unity asset/.meta generator for the Workout Drop project.

Why this exists
---------------
Unity references assets by GUID (stored in each asset's .meta). This script gives every
asset under Assets/WorkoutDrop a STABLE GUID derived from its project-relative path
(md5(path)), then emits:
  * .meta files for every folder and source file (scripts, uxml, uss, png, ttf, asmdef),
  * the four ScriptableObject .asset files (AppConfig + data SOs) with their data,
  * the Main.unity scene wiring one AppBootstrap -> AppConfig reference,
  * EditorBuildSettings with the scene registered.

Because the GUID is a pure function of the path, every cross-reference resolves without a
running editor. Unity re-imports source assets on first open but never changes an existing
.meta GUID, so the links hold. Run from the project root:  python3 Tools/gen_unity_assets.py
"""

import hashlib
import os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# Unity main-object fileIDs (stable engine constants).
FID_SCRIPT = 11500000            # MonoScript (.cs)
FID_SO = 11400000                # ScriptableObject (.asset main object)
FID_FONT = 12800000              # Font (.ttf)
FID_SPRITE = 21300000            # Sprite sub-asset of a Single-mode texture
FID_VTA = 9197481963319205126    # VisualTreeAsset (.uxml)
FID_USS = 7433441132597879392    # StyleSheet (.uss)


def guid(relpath):
    return hashlib.md5(relpath.replace("\\", "/").encode("utf-8")).hexdigest()


def rel(*parts):
    return "/".join(("Assets", "WorkoutDrop") + parts)


def write(path, content):
    full = os.path.join(ROOT, path)
    os.makedirs(os.path.dirname(full), exist_ok=True)
    with open(full, "w", newline="\n", encoding="utf-8") as f:
        f.write(content)


def write_meta(asset_relpath, body):
    write(asset_relpath + ".meta", body)


# ---------------------------------------------------------------------------
# .meta bodies per asset type
# ---------------------------------------------------------------------------

def folder_meta(g):
    return (f"fileFormatVersion: 2\nguid: {g}\nfolderAsset: yes\n"
            "DefaultImporter:\n  externalObjects: {}\n  userData: \n"
            "  assetBundleName: \n  assetBundleVariant: \n")


def cs_meta(g):
    return (f"fileFormatVersion: 2\nguid: {g}\nMonoImporter:\n"
            "  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n"
            "  executionOrder: 0\n  icon: {instanceID: 0}\n  userData: \n"
            "  assetBundleName: \n  assetBundleVariant: \n")


def asmdef_meta(g):
    return (f"fileFormatVersion: 2\nguid: {g}\nAssemblyDefinitionImporter:\n"
            "  externalObjects: {}\n  userData: \n  assetBundleName: \n"
            "  assetBundleVariant: \n")


def uxml_meta(g):
    return (f"fileFormatVersion: 2\nguid: {g}\nScriptedImporter:\n"
            "  internalIDToNameTable: []\n  externalObjects: {}\n  serializedVersion: 2\n"
            "  userData: \n  assetBundleName: \n  assetBundleVariant: \n"
            "  script: {fileID: 13804, guid: 0000000000000000e000000000000000, type: 0}\n")


def uss_meta(g):
    return (f"fileFormatVersion: 2\nguid: {g}\nScriptedImporter:\n"
            "  internalIDToNameTable: []\n  externalObjects: {}\n  serializedVersion: 2\n"
            "  userData: \n  assetBundleName: \n  assetBundleVariant: \n"
            "  script: {fileID: 12385, guid: 0000000000000000e000000000000000, type: 0}\n")


def ttf_meta(g):
    return (f"fileFormatVersion: 2\nguid: {g}\nTrueTypeFontImporter:\n"
            "  internalIDToNameTable: []\n  externalObjects: {}\n  serializedVersion: 4\n"
            "  fontSize: 16\n  forceTextureCase: -2\n  characterSpacing: 0\n"
            "  characterPadding: 1\n  includeFontData: 1\n  fontNames:\n  - Space Mono\n"
            "  fallbackFontReferences: []\n  customCharacters: \n  fontRenderingMode: 0\n"
            "  ascentCalculationMode: 1\n  useLegacyBoundsCalculation: 0\n"
            "  shouldRoundAdvanceValue: 1\n  userData: \n  assetBundleName: \n"
            "  assetBundleVariant: \n")


def png_meta(g):
    return f"""fileFormatVersion: 2
guid: {g}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 12
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 3
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: 5e97eb03825dee720800000000000000
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
  spritePackingTag:
  pSDRemoveMatte: 0
  pSDShowRemoveMatteOption: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""


def asset_meta(g):
    return (f"fileFormatVersion: 2\nguid: {g}\nNativeFormatImporter:\n"
            "  externalObjects: {}\n  mainObjectFileID: 11400000\n  userData: \n"
            "  assetBundleName: \n  assetBundleVariant: \n")


def scene_meta(g):
    return (f"fileFormatVersion: 2\nguid: {g}\nDefaultImporter:\n"
            "  externalObjects: {}\n  userData: \n  assetBundleName: \n"
            "  assetBundleVariant: \n")


# ---------------------------------------------------------------------------
# Reference helpers
# ---------------------------------------------------------------------------

def ref(fid, g, typ):
    return f"{{fileID: {fid}, guid: {g}, type: {typ}}}"

def script_ref(cs_relpath):
    return ref(FID_SCRIPT, guid(cs_relpath), 3)

def so_ref(asset_relpath):
    return ref(FID_SO, guid(asset_relpath), 2)

def vta_ref(uxml_relpath):
    return ref(FID_VTA, guid(uxml_relpath), 3)

def uss_ref(uss_relpath):
    return ref(FID_USS, guid(uss_relpath), 3)

def font_ref(ttf_relpath):
    return ref(FID_FONT, guid(ttf_relpath), 3)

def sprite_ref(png_relpath):
    return ref(FID_SPRITE, guid(png_relpath), 3)


# ---------------------------------------------------------------------------
# Generate folder + source-file metas
# ---------------------------------------------------------------------------

EXT_META = {
    ".cs": cs_meta,
    ".asmdef": asmdef_meta,
    ".uxml": uxml_meta,
    ".uss": uss_meta,
    ".ttf": ttf_meta,
    ".png": png_meta,
}

def gen_source_metas():
    base = os.path.join(ROOT, "Assets", "WorkoutDrop")
    for dirpath, dirnames, filenames in os.walk(base):
        # folder meta (skip the Assets root itself which has no meta)
        rel_dir = os.path.relpath(dirpath, ROOT).replace("\\", "/")
        write_meta(rel_dir, folder_meta(guid(rel_dir)))
        for fn in filenames:
            if fn.endswith(".meta"):
                continue
            ext = os.path.splitext(fn)[1].lower()
            maker = EXT_META.get(ext)
            if not maker:
                continue
            rp = (rel_dir + "/" + fn)
            write_meta(rp, maker(guid(rp)))


# ---------------------------------------------------------------------------
# Data ScriptableObject .asset files
# ---------------------------------------------------------------------------

SEED = [
    ("ex-stretch-hamstring", "Hamstring Stretch", 0, 1, 0, 60),
    ("ex-stretch-shoulder", "Shoulder Stretch", 0, 1, 0, 45),
    ("ex-stretch-childpose", "Child's Pose", 0, 1, 0, 60),
    ("ex-stretch-pigeon", "Pigeon Pose", 0, 1, 0, 60),
    ("ex-stretch-cobra", "Cobra Stretch", 0, 1, 0, 45),
    ("ex-cardio-jumprope", "Jump Rope", 1, 1, 0, 180),
    ("ex-cardio-jumpingjacks", "Jumping Jacks", 1, 50, 0, 0),
    ("ex-cardio-highknees", "High Knees", 1, 1, 0, 60),
    ("ex-cardio-runinplace", "Run In Place", 1, 1, 0, 300),
    ("ex-cardio-mountainclimb", "Mountain Climbers", 1, 30, 0, 0),
    ("ex-str-pushup", "Push-Ups", 2, 12, 0, 0),
    ("ex-str-squat", "Bodyweight Squat", 2, 15, 0, 0),
    ("ex-str-deadlift", "Deadlift", 2, 8, 60, 0),
    ("ex-str-bench", "Bench Press", 2, 8, 40, 0),
    ("ex-str-overhead", "Overhead Press", 2, 8, 25, 0),
    ("ex-str-row", "Bent-Over Row", 2, 10, 30, 0),
    ("ex-str-plank", "Plank", 2, 1, 0, 60),
    ("ex-beast-burpee", "Burpees", 3, 15, 0, 0),
    ("ex-beast-kbswing", "Kettlebell Swing", 3, 20, 16, 0),
    ("ex-beast-thruster", "Thrusters", 3, 12, 20, 0),
]

PROGRAMS = [
    (0, "Recovery", "Light stretching", 20, 1, 3),
    (1, "Cardio", "Run & jump", 30, 1, 3),
    (2, "Strength", "Lift heavy", 40, 3, 4),
    (3, "Beast Mode", "Max HIIT + bonus", 45, 4, 5),
]

CELL_PROGRAMS = [3, 2, 1, 0, 1, 2, 3]
WEIGHTS_EASY = [2, 6, 14, 26, 14, 6, 2]
WEIGHTS_BEAST = [18, 16, 10, 6, 10, 16, 18]


def so_header(script_relpath, name):
    return ("%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!114 &11400000\n"
            "MonoBehaviour:\n  m_ObjectHideFlags: 0\n"
            "  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n"
            "  m_GameObject: {fileID: 0}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n"
            f"  m_Script: {script_ref(script_relpath)}\n  m_Name: {name}\n"
            "  m_EditorClassIdentifier: \n")


def gen_exercise_db():
    ap = rel("Data", "ExerciseDatabase.asset")
    cs = "Assets/WorkoutDrop/Scripts/Data/ExerciseDatabase.cs"
    body = so_header(cs, "ExerciseDatabase")
    body += "  seedExercises:\n"
    for (eid, nm, cat, reps, wt, dur) in SEED:
        body += (f"  - id: {eid}\n    name: {yaml_str(nm)}\n    category: {cat}\n"
                 f"    defaultReps: {reps}\n    defaultWeight: {wt}\n    defaultDurationSec: {dur}\n")
    write(ap, body)
    write_meta(ap, asset_meta(guid(ap)))


def gen_plinko():
    ap = rel("Data", "PlinkoConfig.asset")
    cs = "Assets/WorkoutDrop/Scripts/Data/PlinkoConfig.cs"
    body = so_header(cs, "PlinkoConfig")
    body += "  cellPrograms:\n" + "".join(f"  - {v}\n" for v in CELL_PROGRAMS)
    body += "  cellWeightsEasy:\n" + "".join(f"  - {v}\n" for v in WEIGHTS_EASY)
    body += "  cellWeightsBeast:\n" + "".join(f"  - {v}\n" for v in WEIGHTS_BEAST)
    body += "  plinkoRows: 12\n"
    write(ap, body)
    write_meta(ap, asset_meta(guid(ap)))


def gen_program():
    ap = rel("Data", "ProgramConfig.asset")
    cs = "Assets/WorkoutDrop/Scripts/Data/ProgramConfig.cs"
    body = so_header(cs, "ProgramConfig")
    body += "  programs:\n"
    for (pt, label, tagline, dur, spe, ne) in PROGRAMS:
        body += (f"  - programType: {pt}\n    label: {yaml_str(label)}\n"
                 f"    tagline: {yaml_str(tagline)}\n    durationMin: {dur}\n"
                 f"    setsPerExercise: {spe}\n    numExercises: {ne}\n")
    write(ap, body)
    write_meta(ap, asset_meta(guid(ap)))


def gen_app_config():
    ap = rel("Data", "AppConfig.asset")
    cs = "Assets/WorkoutDrop/Scripts/Data/AppConfig.cs"
    body = so_header(cs, "AppConfig")
    body += f"  exercises: {so_ref(rel('Data', 'ExerciseDatabase.asset'))}\n"
    body += f"  plinko: {so_ref(rel('Data', 'PlinkoConfig.asset'))}\n"
    body += f"  programs: {so_ref(rel('Data', 'ProgramConfig.asset'))}\n"
    body += f"  globalStyle: {uss_ref('Assets/WorkoutDrop/UI/Styles/app.uss')}\n"
    body += f"  bodyFont: {font_ref('Assets/WorkoutDrop/Art/Fonts/SpaceMono-Regular.ttf')}\n"
    screens = [
        ("welcomeScreen", "welcome"), ("homeScreen", "home"), ("dropScreen", "drop"),
        ("workoutScreen", "workout"), ("summaryScreen", "summary"),
        ("progressScreen", "progress"), ("settingsScreen", "settings"),
        ("exercisesScreen", "exercises"),
    ]
    for field, fn in screens:
        body += f"  {field}: {vta_ref('Assets/WorkoutDrop/UI/Screens/' + fn + '.uxml')}\n"
    sprites = [
        ("kettlebell", "kettlebell"), ("kettlebellFire", "kettlebell_fire"),
        ("badgeFullDrop", "badge_full_drop"), ("badgeBeastMode", "badge_beast_mode"),
        ("badgeIronWeek", "badge_iron_week"), ("badgePr", "badge_pr"),
    ]
    for field, fn in sprites:
        body += f"  {field}: {sprite_ref('Assets/WorkoutDrop/Art/Sprites/' + fn + '.png')}\n"
    write(ap, body)
    write_meta(ap, asset_meta(guid(ap)))


def yaml_str(s):
    # Quote strings that contain characters YAML would misread (apostrophes, etc.).
    if any(c in s for c in ":#'\"") or s.strip() != s:
        return "'" + s.replace("'", "''") + "'"
    return s


# ---------------------------------------------------------------------------
# Scene
# ---------------------------------------------------------------------------

def gen_scene():
    scene_rp = rel("Scenes", "Main.unity")
    app_config_rp = rel("Data", "AppConfig.asset")
    bootstrap_cs = "Assets/WorkoutDrop/Scripts/UI/AppBootstrap.cs"

    go = 535000001
    tr = 535000002
    mb = 535000003

    scene = f"""%YAML 1.1
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
  serializedVersion: 10
  m_Fog: 0
  m_FogColor: {{r: 0.5, g: 0.5, b: 0.5, a: 1}}
  m_FogMode: 3
  m_FogDensity: 0.01
  m_LinearFogStart: 0
  m_LinearFogEnd: 300
  m_AmbientSkyColor: {{r: 0.212, g: 0.227, b: 0.259, a: 1}}
  m_AmbientEquatorColor: {{r: 0.114, g: 0.125, b: 0.133, a: 1}}
  m_AmbientGroundColor: {{r: 0.047, g: 0.043, b: 0.035, a: 1}}
  m_AmbientIntensity: 1
  m_AmbientMode: 0
  m_SubtractiveShadowColor: {{r: 0.42, g: 0.478, b: 0.627, a: 1}}
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
  m_Sun: {{fileID: 0}}
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
    m_EnableBakedLightmaps: 1
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
--- !u!1 &{go}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {tr}}}
  - component: {{fileID: {mb}}}
  m_Layer: 0
  m_Name: App
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &{tr}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!114 &{mb}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {script_ref(bootstrap_cs)}
  m_Name:
  m_EditorClassIdentifier:
  _config: {so_ref(app_config_rp)}
"""
    write(scene_rp, scene)
    write_meta(scene_rp, scene_meta(guid(scene_rp)))
    return guid(scene_rp)


def gen_build_settings(scene_guid):
    path = "ProjectSettings/EditorBuildSettings.asset"
    content = ("%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!1045 &1\n"
               "EditorBuildSettings:\n  m_ObjectHideFlags: 0\n  serializedVersion: 2\n"
               "  m_Scenes:\n  - enabled: 1\n    path: Assets/WorkoutDrop/Scenes/Main.unity\n"
               f"    guid: {scene_guid}\n  m_configObjects: {{}}\n"
               "  m_UseUCBPForAssetBundles: 0\n")
    write(path, content)


def main():
    gen_source_metas()
    gen_exercise_db()
    gen_plinko()
    gen_program()
    gen_app_config()
    sg = gen_scene()
    gen_build_settings(sg)
    print("Generated metas, data assets, scene and build settings.")
    print("Scene GUID:", sg)
    print("AppConfig GUID:", guid(rel("Data", "AppConfig.asset")))


if __name__ == "__main__":
    main()
