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
    if not enemy_part_hits([], "unused"):
        errors.append("self-test enemy part whitelist did not FAIL")
    good_parts = (
        'if (parts[i].name == "Trim_Beam")\n'
        "parts[i].gameObject.SetActive(false);\n"
        "_enemyBody.renderQueue = 2000;\n"
        "_enemyTrim.renderQueue = 2000;\n"
        "PinEnemyLayer(root);\n"
        'MakeShipRim(\n"Mat_Enemy_V4_Body",\n'
    )
    if enemy_part_hits(list(_V4_BARE), good_parts):
        errors.append("self-test enemy parts were flagged")
    if not spawn_clear_hits("", "", ""):
        errors.append("self-test spawn-decor did not FAIL")
    if spawn_clear_hits(
        "SpawnClear.ClearVisual(floorVisual);\n",
        "CollectSpawnIslands()\n",
        "LayerGate.IsSpawnClutter(minX, maxX, minZ, maxZ);\nLayerGate.IsRingCorner(minX, maxX, minZ, maxZ, ring);\n",
    ):
        errors.append("self-test spawn carve was flagged")
    if not v4_binding_source_hits("nope", ["Head"]):
        errors.append("self-test v4 material binding did not FAIL")
    if v4_binding_source_hits('hits.Add("v4-mat " + name);\nV4MaterialHit(name, material);\n"Head"\n', ["Head"]):
        errors.append("self-test v4 material binding was flagged")
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


_NAME_BYTES = set(b"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_")
_V4_BARE = ("Crossbeam", "Hatch", "Head", "Lance", "Hub", "Ring")
_V4_MAT = re.compile(r"Mat_Enemy_V4_[A-Za-z0-9_]+")


def _glob(pattern: str, text: str) -> bool:
    """Same rules as LayerGate.Glob. A lone star matches any span."""
    if pattern is None or text is None:
        return False
    parts = pattern.split("*")
    index = 0
    for i, part in enumerate(parts):
        if not part:
            continue
        found = text.find(part, index)
        if found < 0:
            return False
        if i == 0 and pattern and pattern[0] != "*" and found != 0:
            return False
        index = found + len(part)
    if not pattern or pattern[-1] != "*":
        last = parts[-1] if parts else ""
        if last and not text.endswith(last):
            return False
    return True


def _match_any(patterns, text: str) -> bool:
    if not isinstance(patterns, list):
        return False
    return any(_glob(pattern, text) for pattern in patterns if isinstance(pattern, str))


def v4_model_names(root: Path) -> list[str]:
    """GameObject names on every Enemy_*_v4.fbx Model node."""
    folder = root / "Assets" / "Resources" / "Art" / "Import"
    names: set[str] = set()
    if not folder.is_dir():
        return []
    marker = b"\x01Model"
    for path in sorted(folder.glob("Enemy_*_v4.fbx")):
        data = path.read_bytes()
        start = 0
        while True:
            index = data.find(marker, start)
            if index < 0:
                break
            if index >= 1 and data[index - 1] == 0:
                end = index - 1
                cursor = end - 1
                while cursor >= 0 and data[cursor] in _NAME_BYTES:
                    cursor -= 1
                raw = data[cursor + 1 : end]
                try:
                    name = raw.decode("ascii")
                except UnicodeDecodeError:
                    name = ""
                if len(name) >= 2 and name[0].isupper() and name.replace("_", "").isalnum():
                    names.add(name)
            start = index + 1
    return sorted(names)


def v4_part_hits(renderers, materials, factory: str, root: Path) -> list[str]:
    """FAIL when any v4 FBX part or Mat_Enemy_V4_* material is off the whitelist."""
    hits: list[str] = []
    parts = v4_model_names(root)
    if len(parts) < 20:
        hits.append("v4 enemy FBX part list is empty")
    for token in parts:
        if not _match_any(renderers, token):
            hits.append(f"unknown {token} missing from layer whitelist")
    for token in sorted(set(_V4_MAT.findall(factory or ""))):
        if not _match_any(materials, token):
            hits.append(f"unknown {token} missing from layer whitelist")
    return hits


def v4_binding_source_hits(gate_source: str, names) -> list[str]:
    """FAIL unless every v4 part must be drawn with a Mat_Enemy_V4_* material."""
    hits: list[str] = []
    text = gate_source or ""
    if "v4-mat " not in text or "V4MaterialHit(" not in text:
        hits.append("v4 parts are not required to use Mat_Enemy_V4_")
    for token in names or []:
        if f'"{token}"' not in text:
            hits.append(f"v4 part {token} is not bound to Mat_Enemy_V4_")
    return hits


def enemy_part_hits(renderers, factory: str) -> list[str]:
    """FAIL when a bare v4 part is off the whitelist, or Trim_Beam stays live."""
    hits: list[str] = []
    names = renderers if isinstance(renderers, list) else []
    for token in _V4_BARE:
        if token not in names:
            hits.append(f"unknown {token} missing from layer whitelist")
    text = factory or ""
    if 'name == "Trim_Beam"' not in text or "SetActive(false)" not in text:
        hits.append("dead Trim_Beam is still in the world")
    if "_enemyBody.renderQueue = 2000;" not in text or "_enemyTrim.renderQueue = 2000;" not in text:
        hits.append("enemy v4 body/trim renderQueue is not geometry")
    if "PinEnemyLayer(" not in text:
        hits.append("enemy parts are not pinned to the gameplay layer")
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
    factory_path = root / "Assets/Scripts/Content/ContentFactory.cs"
    factory_text = factory_path.read_text(encoding="utf-8") if factory_path.is_file() else ""
    if not factory_path.is_file():
        errors.append("missing ContentFactory.cs for enemy part gate")
    errors.extend(enemy_part_hits(payload.get("renderers"), factory_text))
    errors.extend(v4_part_hits(payload.get("renderers"), payload.get("materials"), factory_text, root))
    gate_path = root / "Assets/Scripts/Core/LayerGate.cs"
    gate_text = gate_path.read_text(encoding="utf-8") if gate_path.is_file() else ""
    errors.extend(v4_binding_source_hits(gate_text, v4_model_names(root)))
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


def main() -> int:
    errors = check(Path(__file__).resolve().parents[1])
    if errors:
        for item in errors:
            print(item)
        print("FAIL " + str(len(errors)))
        return 1
    print("PASS")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
