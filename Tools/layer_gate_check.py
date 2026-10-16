"""Static half of the layer gate.

Scans shader files for ZTest Always and Overlay queues, checks the dome ring
is depth-tested, and checks Tools/layer_whitelist.json against CanvasOrder.
Injection cases prove each static failure class returns FAIL.
"""

from __future__ import annotations

import json
import re
from pathlib import Path

_SHADER_NAME = re.compile(r'Shader\s+"([^"]+)"')
_ZTEST_ALWAYS = re.compile(r"ZTest\s+Always\b")
_QUEUE_OVERLAY = re.compile(r'"Queue"\s*=\s*"Overlay"')
_RENDER_QUEUE = re.compile(r"renderQueue\s*=\s*(\d+)")
_CANVAS_CONST = re.compile(r"public const int (\w+) = (\d+);")

_REQUIRED_KEYS = (
    "renderers",
    "materials",
    "renderers.hangar",
    "renderers.1",
    "renderers.3",
    "renderers.4",
    "renderers.death",
    "renderers.waveclear",
    "materials.hangar",
    "materials.1",
    "materials.3",
    "materials.4",
    "materials.death",
    "materials.waveclear",
    "overlayExemptShaders",
    "deadNameHints",
    "canvasSorts",
)

_SELF_TOKENS = (
    "unknown",
    "dead",
    "ztest-always",
    "overlay-queue",
    "gameplay-queue",
    "dome-ztest",
    "canvas-sort",
    "ui-overlap",
    "contact-shadow",
    "canvas-layer",
    "ui-camera",
    "spawn-decor",
)

_CANVAS_SOURCES = (
    "Assets/Scripts/UI/GameUi.cs",
    "Assets/Scripts/UI/RebindOverlay.cs",
)


def shader_hits(shader_name: str, text: str, exempt: list[str]) -> list[str]:
    """Return FAIL reasons for one shader. Empty means the shader is allowed."""
    hits: list[str] = []
    if _exempt(shader_name, exempt):
        if "DomeRing" in shader_name and _ZTEST_ALWAYS.search(text):
            hits.append(f"dome-ztest {shader_name} ZTest Always")
        return hits
    if _ZTEST_ALWAYS.search(text):
        hits.append(f"ztest-always {shader_name}")
    if _QUEUE_OVERLAY.search(text):
        hits.append(f"overlay-queue {shader_name}")
    if "DomeRing" in shader_name:
        depth = "ZTest LEqual" in text or "ZTest [_ZTest]" in text
        if not depth or _ZTEST_ALWAYS.search(text):
            hits.append(f"dome-ztest {shader_name}")
    return hits


def queue_literal_hits(source: str, label: str) -> list[str]:
    hits: list[str] = []
    for raw in _RENDER_QUEUE.findall(source):
        if int(raw) >= 4000:
            hits.append(f"overlay-queue {label} renderQueue {raw}")
    return hits


def injection_errors() -> list[str]:
    """Each injected static failure must FAIL. The exempt post pass must not."""
    errors: list[str] = []
    exempt = ["AGR/Post", "Hidden/*"]
    if not shader_hits("AGR/EvilZ", "ZTest Always\n", exempt):
        errors.append("self-test ztest-always did not FAIL")
    if not shader_hits("AGR/Evil", 'Tags { "Queue"="Overlay" }\n', exempt):
        errors.append("self-test overlay-queue did not FAIL")
    if shader_hits("AGR/Post", "Cull Off ZWrite Off ZTest Always\n", exempt):
        errors.append("self-test exempt post was flagged")
    if not shader_hits("AGR/DomeRing", "ZTest Always\n", exempt):
        errors.append("self-test dome-ztest did not FAIL")
    if shader_hits("AGR/DomeRing", "ZTest [_ZTest]\n", exempt):
        errors.append("self-test depth-tested dome was flagged")
    if not queue_literal_hits("material.renderQueue = 4000;", "Inject.cs"):
        errors.append("self-test gameplay overlay literal did not FAIL")
    if queue_literal_hits("material.renderQueue = 3000;", "Inject.cs"):
        errors.append("self-test transparent queue was flagged")
    bad_camera = "uiCam.cullingMask = mask;\nmask |= 1 << layer;\n"
    if not ui_camera_source_hits(bad_camera):
        errors.append("self-test ui-camera did not FAIL")
    good_camera = (
        "uiCam.cullingMask = LayerGate.UiCameraMask;\n"
        "uiObject.transform.position = new Vector3(0f, -10000f, 0f);\n"
    )
    if ui_camera_source_hits(good_camera):
        errors.append("self-test UI-only overlay camera was flagged")
    if not canvas_file_hits("canvasObject.AddComponent<Canvas>();\n", "Inject.cs"):
        errors.append("self-test canvas-layer did not FAIL")
    if canvas_file_hits("UseUiLayer(canvasObject);\ncanvasObject.AddComponent<Canvas>();\n", "Inject.cs"):
        errors.append("self-test UI canvas was flagged")
    if not spawn_clear_hits("", "", ""):
        errors.append("self-test spawn-decor did not FAIL")
    if spawn_clear_hits(
        "SpawnClear.ClearVisual(floorVisual);\n",
        "CollectSpawnIslands()\n",
        "LayerGate.IsSpawnClutter(minX, maxX, minZ, maxZ);\nLayerGate.IsRingCorner(minX, maxX, minZ, maxZ, ring);\n",
    ):
        errors.append("self-test spawn carve was flagged")
    return errors


def ui_camera_source_hits(source: str) -> list[str]:
    """FAIL when a capture overlay camera can see a non-UI layer or sits in the arena."""
    hits: list[str] = []
    if "cullingMask = LayerGate.UiCameraMask" not in source:
        hits.append("ui-camera overlay mask is not UI-only")
    if "mask |= 1 << layer" in source or "mask = (1 << 0)" in source:
        hits.append("ui-camera overlay mask includes a non-UI layer")
    if "new Vector3(0f, -10000f, 0f)" not in source:
        hits.append("ui-camera overlay camera is not parked away from the arena")
    return hits


def spawn_clear_hits(factory: str, capture: str, carve: str) -> list[str]:
    """FAIL when spawn leftovers are not carved, or capture does not measure them."""
    hits: list[str] = []
    if "SpawnClear.ClearVisual(" not in factory:
        hits.append("spawn-decor arena visual is not cleared around spawn")
    if "CollectSpawnIslands()" not in capture:
        hits.append("spawn-decor capture does not measure decor near spawn")
    if "IsSpawnClutter(" not in carve or "IsRingCorner(" not in carve:
        hits.append("spawn-decor carve does not use the layer-gate island rules")
    return hits


def canvas_file_hits(source: str, label: str) -> list[str]:
    """FAIL when a file adds a Canvas and never places it on the UI layer."""
    if "AddComponent<Canvas>" in source and "UseUiLayer(" not in source:
        return [f"canvas-layer {label} Canvas is not on the UI layer"]
    return []


def check(root: Path) -> list[str]:
    errors = list(injection_errors())
    whitelist_path = root / "Tools" / "layer_whitelist.json"
    if not whitelist_path.is_file():
        errors.append("missing Tools/layer_whitelist.json")
        return errors
    try:
        payload = json.loads(whitelist_path.read_text(encoding="utf-8"))
    except json.JSONDecodeError as exc:
        errors.append(f"Tools/layer_whitelist.json is not JSON ({exc})")
        return errors
    if not isinstance(payload, dict):
        errors.append("Tools/layer_whitelist.json must be an object")
        return errors
    for key in _REQUIRED_KEYS:
        if key not in payload:
            errors.append(f"layer whitelist missing {key}")
    exempt = payload.get("overlayExemptShaders")
    if not isinstance(exempt, list) or "AGR/Post" not in exempt:
        errors.append("layer whitelist must exempt AGR/Post")
        exempt_names = ["AGR/Post"]
    else:
        exempt_names = [item for item in exempt if isinstance(item, str)]
    hints = payload.get("deadNameHints")
    if not isinstance(hints, list) or "Beam" not in hints:
        errors.append("layer whitelist deadNameHints must include Beam")
    materials = payload.get("materials")
    if not isinstance(materials, list) or "Mat_*" not in materials:
        errors.append("layer whitelist materials must include Mat_*")
    sorts = payload.get("canvasSorts")
    order_path = root / "Assets/Scripts/Core/CanvasOrder.cs"
    if order_path.is_file():
        values = {name: int(value) for name, value in _CANVAS_CONST.findall(order_path.read_text(encoding="utf-8"))}
        expected = sorted(values.values())
        got = sorted(sorts) if isinstance(sorts, list) else []
        if got != expected:
            errors.append(f"layer whitelist canvasSorts {got} must match CanvasOrder {expected}")
    else:
        errors.append("missing CanvasOrder.cs for the layer gate")
    gate = root / "Assets/Scripts/Core/LayerGate.cs"
    if not gate.is_file():
        errors.append("missing LayerGate.cs")
    else:
        source = gate.read_text(encoding="utf-8")
        if "public static bool SelfCheck()" not in source:
            errors.append("LayerGate.SelfCheck missing")
        for token in _SELF_TOKENS:
            if f'"{token}"' not in source and f"\"{token} " not in source and token not in source:
                errors.append(f"LayerGate self-test missing {token}")
    for path in sorted((root / "Assets").rglob("*.shader")):
        text = path.read_text(encoding="utf-8", errors="replace")
        match = _SHADER_NAME.search(text)
        shader_name = match.group(1) if match else path.stem
        for hit in shader_hits(shader_name, text, exempt_names):
            errors.append(f"{path.relative_to(root)}: {hit}")
    dome = root / "Assets/Resources/Shaders/AgrDomeRing.shader"
    if dome.is_file():
        dome_text = dome.read_text(encoding="utf-8")
        if "ZTest [_ZTest]" not in dome_text or "_ZTest" not in dome_text:
            errors.append("AgrDomeRing.shader must expose _ZTest and use ZTest [_ZTest]")
    else:
        errors.append("missing AgrDomeRing.shader")
    for folder in ("Assets/Scripts", "Assets/Editor"):
        base = root / folder
        if not base.is_dir():
            continue
        for path in sorted(base.rglob("*.cs")):
            for hit in queue_literal_hits(path.read_text(encoding="utf-8", errors="replace"), str(path.relative_to(root))):
                errors.append(hit)
    capture = root / "Assets/Editor/AgrCapture.cs"
    if not capture.is_file():
        errors.append("missing AgrCapture.cs")
    else:
        for hit in ui_camera_source_hits(capture.read_text(encoding="utf-8")):
            errors.append(hit)
    for rel in _CANVAS_SOURCES:
        path = root / rel
        if not path.is_file():
            errors.append(f"canvas-layer missing {rel}")
            continue
        for hit in canvas_file_hits(path.read_text(encoding="utf-8"), rel):
            errors.append(hit)
    ui = root / "Assets/Scripts/UI/GameUi.cs"
    if ui.is_file() and "layer = LayerGate.UiLayer" not in ui.read_text(encoding="utf-8"):
        errors.append("canvas-layer GameUi.UseUiLayer must assign LayerGate.UiLayer")
    factory = root / "Assets/Scripts/Content/ContentFactory.cs"
    carve = root / "Assets/Scripts/Content/SpawnClear.cs"
    factory_text = factory.read_text(encoding="utf-8") if factory.is_file() else ""
    capture_text = capture.read_text(encoding="utf-8") if capture.is_file() else ""
    carve_text = carve.read_text(encoding="utf-8") if carve.is_file() else ""
    if not carve.is_file():
        errors.append("spawn-decor missing Assets/Scripts/Content/SpawnClear.cs")
    errors.extend(spawn_clear_hits(factory_text, capture_text, carve_text))
    return errors


def _exempt(shader_name: str, exempt: list[str]) -> bool:
    for pattern in exempt:
        if pattern.endswith("*"):
            if shader_name.startswith(pattern[:-1]):
                return True
        elif shader_name == pattern:
            return True
    return False
