from __future__ import annotations

import json
import math
import sys
from dataclasses import dataclass, field
from pathlib import Path
from typing import Dict, Iterable, List, Sequence, Tuple

VENDOR_DIR = Path(__file__).resolve().parent / "vendor"
if VENDOR_DIR.exists():
    sys.path.insert(0, str(VENDOR_DIR))

from PIL import Image, ImageDraw, ImageFont  # type: ignore

Color = Tuple[int, int, int, int]
Point = Tuple[float, float]

CANVAS_SIZE = 256
ASSET_ROOT = Path("assets/images/instructions/controls/generated")
FONT_CANDIDATES = (
    Path("C:/Windows/Fonts/segoeuib.ttf"),
    Path("C:/Windows/Fonts/arialbd.ttf"),
    Path("C:/Windows/Fonts/trebucbd.ttf"),
)


@dataclass(frozen=True)
class ControlSpec:
    kind: str
    label: str = ""
    fill: Color = (44, 52, 64, 255)
    border: Color = (13, 17, 23, 255)
    text: Color = (245, 247, 250, 255)
    accent: Color | None = None
    symbol: str = ""
    members: Tuple[str, ...] = ()
    layout: str = ""


@dataclass(frozen=True)
class PlatformSpec:
    name: str
    display_name: str
    controls: Dict[str, ControlSpec] = field(default_factory=dict)


def rgba(hex_value: str, alpha: int = 255) -> Color:
    hex_value = hex_value.lstrip("#")
    return (
        int(hex_value[0:2], 16),
        int(hex_value[2:4], 16),
        int(hex_value[4:6], 16),
        alpha,
    )


def with_alpha(color: Color, alpha: int) -> Color:
    return (color[0], color[1], color[2], alpha)


def blend_colors(primary: Color, secondary: Color, primary_weight: float) -> Color:
    secondary_weight = 1.0 - primary_weight
    return (
        round(primary[0] * primary_weight + secondary[0] * secondary_weight),
        round(primary[1] * primary_weight + secondary[1] * secondary_weight),
        round(primary[2] * primary_weight + secondary[2] * secondary_weight),
        255,
    )


def font(size: int) -> ImageFont.FreeTypeFont | ImageFont.ImageFont:
    for candidate in FONT_CANDIDATES:
        if candidate.is_file():
            return ImageFont.truetype(str(candidate), size=size)
    return ImageFont.load_default()


class PngCanvas:
    def __init__(self, size: int = CANVAS_SIZE) -> None:
        self.size = size
        self.image = Image.new("RGBA", (size, size), (0, 0, 0, 0))
        self.draw = ImageDraw.Draw(self.image)

    def rounded_rect(
        self,
        bounds: Tuple[float, float, float, float],
        radius: float,
        fill: Color,
        outline: Color | None,
        width: int = 4,
    ) -> None:
        self.draw.rounded_rectangle(bounds, radius=radius, fill=fill, outline=outline, width=width)

    def circle(
        self,
        center: Point,
        radius: float,
        fill: Color,
        outline: Color,
        width: int = 4,
    ) -> None:
        x, y = center
        self.draw.ellipse((x - radius, y - radius, x + radius, y + radius), fill=fill, outline=outline, width=width)

    def polygon(self, points: Sequence[Point], fill: Color, outline: Color | None = None, width: int = 3) -> None:
        self.draw.polygon(points, fill=fill, outline=outline)
        if outline is not None and width > 1:
            closed = list(points) + [points[0]]
            self.draw.line(closed, fill=outline, width=width)

    def line(self, points: Sequence[Point], fill: Color, width: int = 3) -> None:
        self.draw.line(points, fill=fill, width=width, joint="curve")

    def text(
        self,
        center: Point,
        label: str,
        size: int,
        fill: Color,
        stroke_fill: Color | None = None,
        stroke_width: int = 0,
    ) -> None:
        self.draw.text(
            center,
            label,
            fill=fill,
            font=font(size),
            anchor="mm",
            stroke_fill=stroke_fill,
            stroke_width=stroke_width,
        )

    def save(self, path: Path) -> None:
        path.parent.mkdir(parents=True, exist_ok=True)
        self.image.save(path)


class SvgCanvas:
    def __init__(self, size: int = CANVAS_SIZE) -> None:
        self.size = size
        self.elements: List[str] = []

    def rounded_rect(
        self,
        bounds: Tuple[float, float, float, float],
        radius: float,
        fill: Color,
        outline: Color | None,
        width: int = 4,
    ) -> None:
        x1, y1, x2, y2 = bounds
        stroke_attr = f' stroke="{svg_color(outline)}" stroke-width="{width}"' if outline is not None else ""
        self.elements.append(
            f'<rect x="{x1:.1f}" y="{y1:.1f}" width="{x2 - x1:.1f}" height="{y2 - y1:.1f}" '
            f'rx="{radius:.1f}" fill="{svg_color(fill)}"{stroke_attr} />'
        )

    def circle(
        self,
        center: Point,
        radius: float,
        fill: Color,
        outline: Color,
        width: int = 4,
    ) -> None:
        self.elements.append(
            f'<circle cx="{center[0]:.1f}" cy="{center[1]:.1f}" r="{radius:.1f}" fill="{svg_color(fill)}" '
            f'stroke="{svg_color(outline)}" stroke-width="{width}" />'
        )

    def polygon(self, points: Sequence[Point], fill: Color, outline: Color | None = None, width: int = 3) -> None:
        point_text = " ".join(f"{x:.1f},{y:.1f}" for x, y in points)
        stroke_attr = f' stroke="{svg_color(outline)}" stroke-width="{width}"' if outline is not None else ""
        self.elements.append(f'<polygon points="{point_text}" fill="{svg_color(fill)}"{stroke_attr} />')

    def line(self, points: Sequence[Point], fill: Color, width: int = 3) -> None:
        point_text = " ".join(f"{x:.1f},{y:.1f}" for x, y in points)
        self.elements.append(
            f'<polyline points="{point_text}" fill="none" stroke="{svg_color(fill)}" '
            f'stroke-width="{width}" stroke-linecap="round" stroke-linejoin="round" />'
        )

    def text(
        self,
        center: Point,
        label: str,
        size: int,
        fill: Color,
        stroke_fill: Color | None = None,
        stroke_width: int = 0,
    ) -> None:
        stroke = f' stroke="{svg_color(stroke_fill)}" stroke-width="{stroke_width}" paint-order="stroke fill"' if stroke_fill else ""
        self.elements.append(
            f'<text x="{center[0]:.1f}" y="{center[1]:.1f}" font-family="Segoe UI, Arial, sans-serif" '
            f'font-weight="700" font-size="{size}" text-anchor="middle" dominant-baseline="middle" '
            f'fill="{svg_color(fill)}"{stroke}>{escape_xml(label)}</text>'
        )

    def save(self, path: Path) -> None:
        path.parent.mkdir(parents=True, exist_ok=True)
        payload = [
            '<?xml version="1.0" encoding="UTF-8"?>',
            f'<svg xmlns="http://www.w3.org/2000/svg" width="{self.size}" height="{self.size}" viewBox="0 0 {self.size} {self.size}">',
            *self.elements,
            "</svg>",
        ]
        path.write_text("\n".join(payload), encoding="utf-8")


def svg_color(color: Color) -> str:
    return f"rgba({color[0]},{color[1]},{color[2]},{color[3] / 255:.3f})"


def escape_xml(value: str) -> str:
    return value.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")


def circle_button(label: str, fill: Color, text: Color = rgba("#FFFFFF"), border: Color = rgba("#111827")) -> ControlSpec:
    return ControlSpec(kind="circle", label=label, fill=fill, border=border, text=text)


def n64_c_dir(direction: str, fill: Color, text: Color = rgba("#FFFFFF"), border: Color = rgba("#111827")) -> ControlSpec:
    return ControlSpec(kind="n64_c_dir", label=direction, fill=fill, border=border, text=text)


def symbol_button(symbol: str, accent: Color, fill: Color = rgba("#242A33"), border: Color = rgba("#0F141B")) -> ControlSpec:
    return ControlSpec(kind="symbol", fill=fill, border=border, text=accent, accent=accent, symbol=symbol)


def capsule_button(label: str, fill: Color, text: Color = rgba("#FFFFFF"), border: Color = rgba("#111827")) -> ControlSpec:
    return ControlSpec(kind="capsule", label=label, fill=fill, border=border, text=text)


def trigger_button(label: str, fill: Color, text: Color = rgba("#FFFFFF"), border: Color = rgba("#111827")) -> ControlSpec:
    return ControlSpec(kind="trigger", label=label, fill=fill, border=border, text=text)


def key_button(label: str) -> ControlSpec:
    return ControlSpec(kind="key", label=label, fill=rgba("#F4F6F8"), border=rgba("#535D6A"), text=rgba("#17212B"))


def dpad_dir(label: str, fill: Color, text: Color = rgba("#FFFFFF"), border: Color = rgba("#111827")) -> ControlSpec:
    return ControlSpec(kind="dpad_dir", label=label, fill=fill, border=border, text=text)


def stick_button(label: str, fill: Color, text: Color = rgba("#E7ECF2"), border: Color = rgba("#10151B")) -> ControlSpec:
    return ControlSpec(kind="stick", label=label, fill=fill, border=border, text=text)


def trackpad_button(label: str, fill: Color, accent: Color, text: Color = rgba("#F7FAFC"), border: Color = rgba("#10151B")) -> ControlSpec:
    return ControlSpec(kind="trackpad", label=label, fill=fill, border=border, text=text, accent=accent)


def touch_button(label: str, fill: Color, accent: Color, text: Color = rgba("#E9EEF5"), border: Color = rgba("#111827")) -> ControlSpec:
    return ControlSpec(kind="touch", label=label, fill=fill, border=border, text=text, accent=accent)


def cluster(*members: str, layout: str) -> ControlSpec:
    return ControlSpec(kind="cluster", members=tuple(members), layout=layout)


def shoulder_pair(*members: str) -> ControlSpec:
    return ControlSpec(kind="shoulder_pair", members=tuple(members))


def build_catalog() -> Dict[str, PlatformSpec]:
    return {
        "keyboard": build_keyboard_platform(),
        "xbox360": build_xbox360_platform(),
        "switch": build_switch_platform(),
        "gamecube": build_gamecube_platform(),
        "wii": build_wii_platform(),
        "ds": build_ds_platform(),
        "3ds": build_3ds_platform(),
        "psp": build_psp_platform(),
        "ps2": build_ps2_platform(),
        "psvita": build_psvita_platform(),
        "n64": build_n64_platform(),
        "dreamcast": build_dreamcast_platform(),
        "ps1": build_ps1_platform(),
        "ps3": build_ps3_platform(),
        "xbox": build_xbox_platform(),
        "steamdeck": build_steamdeck_platform(),
    }


def build_keyboard_platform() -> PlatformSpec:
    controls: Dict[str, ControlSpec] = {}
    for letter in "ABCDEFGHIJKLMNOPQRSTUVWXYZ":
        controls[f"key_{letter.lower()}"] = key_button(letter)
    for digit in "0123456789":
        controls[f"key_{digit}"] = key_button(digit)
    for index in range(1, 13):
        controls[f"f{index}"] = key_button(f"F{index}")

    common_keys = {
        "escape": "Esc",
        "tab": "Tab",
        "caps_lock": "Caps",
        "shift_left": "LShift",
        "shift_right": "RShift",
        "ctrl_left": "LCtrl",
        "ctrl_right": "RCtrl",
        "alt_left": "LAlt",
        "alt_right": "RAlt",
        "meta_left": "LWin",
        "meta_right": "RWin",
        "space": "Space",
        "enter": "Enter",
        "backspace": "Bksp",
        "delete": "Del",
        "insert": "Ins",
        "home": "Home",
        "end": "End",
        "page_up": "PgUp",
        "page_down": "PgDn",
        "up": "Up",
        "down": "Down",
        "left": "Left",
        "right": "Right",
        "print_screen": "PrtSc",
        "scroll_lock": "ScrLk",
        "pause": "Pause",
        "num_lock": "Num",
        "grave": "`",
        "minus": "-",
        "equals": "=",
        "left_bracket": "[",
        "right_bracket": "]",
        "backslash": "\\",
        "semicolon": ";",
        "quote": "'",
        "comma": ",",
        "period": ".",
        "slash": "/",
    }
    for name, label in common_keys.items():
        controls[name] = key_button(label)

    numpad_keys = {
        "numpad_0": "N0",
        "numpad_1": "N1",
        "numpad_2": "N2",
        "numpad_3": "N3",
        "numpad_4": "N4",
        "numpad_5": "N5",
        "numpad_6": "N6",
        "numpad_7": "N7",
        "numpad_8": "N8",
        "numpad_9": "N9",
        "numpad_plus": "N+",
        "numpad_minus": "N-",
        "numpad_multiply": "N*",
        "numpad_divide": "N/",
        "numpad_decimal": "N.",
        "numpad_enter": "NEnt",
    }
    for name, label in numpad_keys.items():
        controls[name] = key_button(label)

    controls["arrow_cluster"] = cluster("left", "up", "down", "right", layout="diamond")
    controls["wasd"] = cluster("key_a", "key_w", "key_s", "key_d", layout="diamond")
    return PlatformSpec(name="keyboard", display_name="Keyboard", controls=controls)


def build_xbox360_platform() -> PlatformSpec:
    controls = build_xbox_family(display_name="Xbox 360", include_black_white=False)
    controls["guide"] = circle_button("X", rgba("#A7B2BF"), rgba("#23313F"))
    return PlatformSpec(name="xbox360", display_name="Xbox 360", controls=controls)


def build_xbox_platform() -> PlatformSpec:
    controls = build_xbox_family(display_name="Xbox", include_black_white=True)
    controls["black"] = capsule_button("Black", rgba("#1F2933"))
    controls["white"] = capsule_button("White", rgba("#EEF2F7"), rgba("#1B2430"))
    return PlatformSpec(name="xbox", display_name="Xbox Original", controls=controls)


def build_xbox_family(display_name: str, include_black_white: bool) -> Dict[str, ControlSpec]:
    controls: Dict[str, ControlSpec] = {
        "a": circle_button("A", rgba("#41B95D"), rgba("#0D2914")),
        "b": circle_button("B", rgba("#D94A48")),
        "x": circle_button("X", rgba("#2C7BE5")),
        "y": circle_button("Y", rgba("#F2C94C"), rgba("#48390A")),
        "lb": capsule_button("LB", rgba("#404A57")),
        "rb": capsule_button("RB", rgba("#404A57")),
        "lt": trigger_button("LT", rgba("#525F6F")),
        "rt": trigger_button("RT", rgba("#525F6F")),
        "start": capsule_button("Start", rgba("#2F3945")),
        "back": capsule_button("Back", rgba("#2F3945")),
        "left_stick": stick_button("LS", rgba("#2A3038")),
        "right_stick": stick_button("RS", rgba("#2A3038")),
        "left_stick_click": stick_button("L3", rgba("#2A3038")),
        "right_stick_click": stick_button("R3", rgba("#2A3038")),
        "dpad_up": dpad_dir("UP", rgba("#2E3642")),
        "dpad_down": dpad_dir("DN", rgba("#2E3642")),
        "dpad_left": dpad_dir("LT", rgba("#2E3642")),
        "dpad_right": dpad_dir("RT", rgba("#2E3642")),
        "dpad": ControlSpec(kind="dpad", fill=rgba("#2E3642"), border=rgba("#111827"), text=rgba("#F8FBFF")),
        "face_cluster": cluster("x", "y", "a", "b", layout="diamond"),
        "shoulders": shoulder_pair("lb", "rb"),
    }
    if include_black_white:
        controls["left_trigger"] = trigger_button("L", rgba("#525F6F"))
        controls["right_trigger"] = trigger_button("R", rgba("#525F6F"))
    return controls


def build_switch_platform() -> PlatformSpec:
    dark = rgba("#1E252E")
    controls: Dict[str, ControlSpec] = {
        "a": circle_button("A", dark),
        "b": circle_button("B", dark),
        "x": circle_button("X", dark),
        "y": circle_button("Y", dark),
        "l": capsule_button("L", rgba("#1E252E")),
        "r": capsule_button("R", rgba("#1E252E")),
        "zl": trigger_button("ZL", rgba("#0F7FA6")),
        "zr": trigger_button("ZR", rgba("#C43E39")),
        "plus": capsule_button("+", rgba("#1E252E")),
        "minus": capsule_button("-", rgba("#1E252E")),
        "home": circle_button("Home", rgba("#1E252E"), rgba("#FFFFFF")),
        "capture": circle_button("Cap", rgba("#1E252E"), rgba("#FFFFFF")),
        "sl": capsule_button("SL", rgba("#2A313C")),
        "sr": capsule_button("SR", rgba("#2A313C")),
        "left_stick": stick_button("LS", rgba("#222A34")),
        "right_stick": stick_button("RS", rgba("#222A34")),
        "left_stick_click": stick_button("L3", rgba("#222A34")),
        "right_stick_click": stick_button("R3", rgba("#222A34")),
        "dpad_up": dpad_dir("UP", dark),
        "dpad_down": dpad_dir("DN", dark),
        "dpad_left": dpad_dir("LT", dark),
        "dpad_right": dpad_dir("RT", dark),
        "dpad": ControlSpec(kind="dpad", fill=dark, border=rgba("#111827"), text=rgba("#FFFFFF")),
        "face_cluster": cluster("y", "x", "b", "a", layout="diamond"),
        "shoulders": shoulder_pair("l", "r"),
    }
    return PlatformSpec(name="switch", display_name="Nintendo Switch", controls=controls)


def build_gamecube_platform() -> PlatformSpec:
    controls: Dict[str, ControlSpec] = {
        "a": circle_button("A", rgba("#47B360")),
        "b": circle_button("B", rgba("#D55C52")),
        "x": circle_button("X", rgba("#C5CCD6"), rgba("#223041"), rgba("#223041")),
        "y": circle_button("Y", rgba("#D8DDE5"), rgba("#223041"), rgba("#223041")),
        "z": capsule_button("Z", rgba("#7B4AC7")),
        "l": trigger_button("L", rgba("#B9C3D1"), rgba("#1D2936")),
        "r": trigger_button("R", rgba("#B9C3D1"), rgba("#1D2936")),
        "start": capsule_button("Start", rgba("#8A8F98")),
        "control_stick": stick_button("C", rgba("#D0DBE8"), rgba("#203040")),
        "c_stick": stick_button("C", rgba("#F2C84B"), rgba("#5B4A12")),
        "dpad_up": dpad_dir("UP", rgba("#C8D0DA"), rgba("#243140")),
        "dpad_down": dpad_dir("DN", rgba("#C8D0DA"), rgba("#243140")),
        "dpad_left": dpad_dir("LT", rgba("#C8D0DA"), rgba("#243140")),
        "dpad_right": dpad_dir("RT", rgba("#C8D0DA"), rgba("#243140")),
        "dpad": ControlSpec(kind="dpad", fill=rgba("#C8D0DA"), border=rgba("#243140"), text=rgba("#243140")),
        "face_cluster": cluster("b", "x", "a", "y", layout="gamecube"),
        "shoulders": shoulder_pair("l", "r"),
    }
    return PlatformSpec(name="gamecube", display_name="GameCube", controls=controls)


def build_wii_platform() -> PlatformSpec:
    controls: Dict[str, ControlSpec] = {
        "a": circle_button("A", rgba("#F8FBFF"), rgba("#223042"), rgba("#223042")),
        "b": trigger_button("B", rgba("#E4EBF3"), rgba("#1F2D3D"), rgba("#1F2D3D")),
        "1": circle_button("1", rgba("#F8FBFF"), rgba("#223042"), rgba("#223042")),
        "2": circle_button("2", rgba("#F8FBFF"), rgba("#223042"), rgba("#223042")),
        "plus": capsule_button("+", rgba("#F5F8FC"), rgba("#1F2D3D"), rgba("#1F2D3D")),
        "minus": capsule_button("-", rgba("#F5F8FC"), rgba("#1F2D3D"), rgba("#1F2D3D")),
        "home": circle_button("Home", rgba("#F5F8FC"), rgba("#1F2D3D"), rgba("#1F2D3D")),
        "c": capsule_button("C", rgba("#BFE8FF"), rgba("#17455F"), rgba("#17455F")),
        "z": capsule_button("Z", rgba("#BFE8FF"), rgba("#17455F"), rgba("#17455F")),
        "stick": stick_button("N", rgba("#DAE4EF"), rgba("#203040")),
        "dpad_up": dpad_dir("UP", rgba("#F1F5FA"), rgba("#203040"), rgba("#203040")),
        "dpad_down": dpad_dir("DN", rgba("#F1F5FA"), rgba("#203040"), rgba("#203040")),
        "dpad_left": dpad_dir("LT", rgba("#F1F5FA"), rgba("#203040"), rgba("#203040")),
        "dpad_right": dpad_dir("RT", rgba("#F1F5FA"), rgba("#203040"), rgba("#203040")),
        "dpad": ControlSpec(kind="dpad", fill=rgba("#F1F5FA"), border=rgba("#203040"), text=rgba("#203040")),
        "face_cluster": cluster("1", "a", "2", "b", layout="diamond"),
        "shoulders": shoulder_pair("c", "z"),
    }
    return PlatformSpec(name="wii", display_name="Wii", controls=controls)


def build_ds_platform() -> PlatformSpec:
    controls: Dict[str, ControlSpec] = {
        "a": circle_button("A", rgba("#EDF1F6"), rgba("#1B2838"), rgba("#1B2838")),
        "b": circle_button("B", rgba("#EDF1F6"), rgba("#1B2838"), rgba("#1B2838")),
        "x": circle_button("X", rgba("#EDF1F6"), rgba("#1B2838"), rgba("#1B2838")),
        "y": circle_button("Y", rgba("#EDF1F6"), rgba("#1B2838"), rgba("#1B2838")),
        "l": capsule_button("L", rgba("#F4F7FB"), rgba("#1B2838"), rgba("#1B2838")),
        "r": capsule_button("R", rgba("#F4F7FB"), rgba("#1B2838"), rgba("#1B2838")),
        "start": capsule_button("Start", rgba("#E3E9F0"), rgba("#1B2838"), rgba("#1B2838")),
        "select": capsule_button("Select", rgba("#E3E9F0"), rgba("#1B2838"), rgba("#1B2838")),
        "touch": touch_button("Touch", rgba("#EEF3F8"), rgba("#52B4E8"), rgba("#1B2838"), rgba("#1B2838")),
        "dpad_up": dpad_dir("UP", rgba("#E8EEF5"), rgba("#223042"), rgba("#223042")),
        "dpad_down": dpad_dir("DN", rgba("#E8EEF5"), rgba("#223042"), rgba("#223042")),
        "dpad_left": dpad_dir("LT", rgba("#E8EEF5"), rgba("#223042"), rgba("#223042")),
        "dpad_right": dpad_dir("RT", rgba("#E8EEF5"), rgba("#223042"), rgba("#223042")),
        "dpad": ControlSpec(kind="dpad", fill=rgba("#E8EEF5"), border=rgba("#223042"), text=rgba("#223042")),
        "face_cluster": cluster("y", "x", "b", "a", layout="diamond"),
        "shoulders": shoulder_pair("l", "r"),
    }
    return PlatformSpec(name="ds", display_name="Nintendo DS", controls=controls)


def build_3ds_platform() -> PlatformSpec:
    controls = dict(build_ds_platform().controls)
    controls.update(
        {
            "zl": trigger_button("ZL", rgba("#DFE7EF"), rgba("#223042"), rgba("#223042")),
            "zr": trigger_button("ZR", rgba("#DFE7EF"), rgba("#223042"), rgba("#223042")),
            "home": circle_button("Home", rgba("#EDF1F6"), rgba("#223042"), rgba("#223042")),
            "circle_pad": stick_button("Pad", rgba("#D7E7EF"), rgba("#223042")),
            "c_stick": stick_button("C", rgba("#FFC766"), rgba("#5D4521")),
        }
    )
    return PlatformSpec(name="3ds", display_name="Nintendo 3DS", controls=controls)


def build_psp_platform() -> PlatformSpec:
    controls = build_playstation_controls(include_second_shoulder=False, include_second_trigger=False, include_sticks=False)
    controls.update(
        {
            "l": capsule_button("L", rgba("#BCC6D4"), rgba("#213041"), rgba("#213041")),
            "r": capsule_button("R", rgba("#BCC6D4"), rgba("#213041"), rgba("#213041")),
            "start": capsule_button("Start", rgba("#303745")),
            "select": capsule_button("Select", rgba("#303745")),
            "analog": stick_button("AN", rgba("#1E242D")),
            "home": circle_button("Home", rgba("#1F2430")),
        }
    )
    return PlatformSpec(name="psp", display_name="PSP", controls=controls)


def build_ps2_platform() -> PlatformSpec:
    controls = build_playstation_controls(include_second_shoulder=True, include_second_trigger=True, include_sticks=True)
    controls.update(
        {
            "start": capsule_button("Start", rgba("#313A45")),
            "select": capsule_button("Select", rgba("#313A45")),
            "analog": capsule_button("Analog", rgba("#313A45")),
        }
    )
    return PlatformSpec(name="ps2", display_name="PlayStation 2", controls=controls)


def build_ps1_platform() -> PlatformSpec:
    controls = build_playstation_controls(include_second_shoulder=True, include_second_trigger=True, include_sticks=True)
    controls.update(
        {
            "start": capsule_button("Start", rgba("#313A45")),
            "select": capsule_button("Select", rgba("#313A45")),
            "analog": capsule_button("Analog", rgba("#313A45")),
        }
    )
    return PlatformSpec(name="ps1", display_name="PlayStation", controls=controls)


def build_ps3_platform() -> PlatformSpec:
    controls = build_playstation_controls(include_second_shoulder=True, include_second_trigger=True, include_sticks=True)
    controls.update(
        {
            "start": capsule_button("Start", rgba("#313A45")),
            "select": capsule_button("Select", rgba("#313A45")),
            "ps": circle_button("PS", rgba("#202734")),
        }
    )
    return PlatformSpec(name="ps3", display_name="PlayStation 3", controls=controls)


def build_psvita_platform() -> PlatformSpec:
    controls = build_playstation_controls(include_second_shoulder=False, include_second_trigger=False, include_sticks=True)
    controls.update(
        {
            "l": capsule_button("L", rgba("#BAC4D2"), rgba("#223042"), rgba("#223042")),
            "r": capsule_button("R", rgba("#BAC4D2"), rgba("#223042"), rgba("#223042")),
            "start": capsule_button("Start", rgba("#313A45")),
            "select": capsule_button("Select", rgba("#313A45")),
            "ps": circle_button("PS", rgba("#202734")),
            "touch_front": touch_button("Front", rgba("#1D2631"), rgba("#57C7FF")),
            "touch_rear": touch_button("Rear", rgba("#1D2631"), rgba("#F2C14E")),
        }
    )
    return PlatformSpec(name="psvita", display_name="PS Vita", controls=controls)


def build_playstation_controls(include_second_shoulder: bool, include_second_trigger: bool, include_sticks: bool) -> Dict[str, ControlSpec]:
    controls: Dict[str, ControlSpec] = {
        "triangle": symbol_button("triangle", rgba("#65D07A")),
        "circle": symbol_button("circle", rgba("#F05B5B")),
        "cross": symbol_button("cross", rgba("#5EA8FF")),
        "square": symbol_button("square", rgba("#F29AF1")),
        "dpad_up": dpad_dir("UP", rgba("#2D3440")),
        "dpad_down": dpad_dir("DN", rgba("#2D3440")),
        "dpad_left": dpad_dir("LT", rgba("#2D3440")),
        "dpad_right": dpad_dir("RT", rgba("#2D3440")),
        "dpad": ControlSpec(kind="dpad", fill=rgba("#2D3440"), border=rgba("#0F141B"), text=rgba("#F3F7FD")),
        "face_cluster": cluster("square", "triangle", "cross", "circle", layout="diamond"),
    }
    controls["l1"] = capsule_button("L1", rgba("#424A58"))
    controls["r1"] = capsule_button("R1", rgba("#424A58"))
    if include_second_shoulder:
        controls["l2"] = trigger_button("L2", rgba("#5B6472"))
        controls["r2"] = trigger_button("R2", rgba("#5B6472"))
    if include_sticks:
        controls["left_stick"] = stick_button("LS", rgba("#1F2530"))
        controls["right_stick"] = stick_button("RS", rgba("#1F2530"))
        controls["left_stick_click"] = stick_button("L3", rgba("#1F2530"))
        controls["right_stick_click"] = stick_button("R3", rgba("#1F2530"))
    controls["shoulders"] = shoulder_pair("l1", "r1")
    if include_second_trigger:
        controls["triggers"] = shoulder_pair("l2", "r2")
    return controls


def build_n64_platform() -> PlatformSpec:
    controls: Dict[str, ControlSpec] = {
        "a": circle_button("A", rgba("#2D7FE8")),
        "b": circle_button("B", rgba("#53BD63")),
        "c_up": n64_c_dir("c_up", rgba("#F0C447"), rgba("#5B4910")),
        "c_down": n64_c_dir("c_down", rgba("#F0C447"), rgba("#5B4910")),
        "c_left": n64_c_dir("c_left", rgba("#F0C447"), rgba("#5B4910")),
        "c_right": n64_c_dir("c_right", rgba("#F0C447"), rgba("#5B4910")),
        "z": trigger_button("Z", rgba("#346BB6")),
        "l": capsule_button("L", rgba("#E5EDF6"), rgba("#223042"), rgba("#223042")),
        "r": capsule_button("R", rgba("#E5EDF6"), rgba("#223042"), rgba("#223042")),
        "start": circle_button("START", rgba("#D24E4B")),
        "control_stick": stick_button("", rgba("#C6D0DC"), rgba("#223042")),
        "dpad_up": dpad_dir("UP", rgba("#D9E2EC"), rgba("#223042"), rgba("#223042")),
        "dpad_down": dpad_dir("DN", rgba("#D9E2EC"), rgba("#223042"), rgba("#223042")),
        "dpad_left": dpad_dir("LT", rgba("#D9E2EC"), rgba("#223042"), rgba("#223042")),
        "dpad_right": dpad_dir("RT", rgba("#D9E2EC"), rgba("#223042"), rgba("#223042")),
        "dpad": ControlSpec(kind="dpad", fill=rgba("#D9E2EC"), border=rgba("#223042"), text=rgba("#223042")),
        "face_cluster": cluster("b", "a", layout="pair_diagonal"),
        "c_cluster": cluster("c_left", "c_up", "c_down", "c_right", layout="n64_c_cluster"),
        "shoulders": shoulder_pair("l", "r"),
    }
    return PlatformSpec(name="n64", display_name="Nintendo 64", controls=controls)


def build_dreamcast_platform() -> PlatformSpec:
    controls: Dict[str, ControlSpec] = {
        "a": circle_button("A", rgba("#D84F47")),
        "b": circle_button("B", rgba("#2C80E8")),
        "x": circle_button("X", rgba("#F2C84C"), rgba("#5D4610")),
        "y": circle_button("Y", rgba("#5BC26B")),
        "l": trigger_button("L", rgba("#BEC7D3"), rgba("#223041"), rgba("#223041")),
        "r": trigger_button("R", rgba("#BEC7D3"), rgba("#223041"), rgba("#223041")),
        "start": capsule_button("Start", rgba("#29303A")),
        "analog": stick_button("Stick", rgba("#D1D8E0"), rgba("#223041")),
        "dpad_up": dpad_dir("UP", rgba("#D8E1EB"), rgba("#223041"), rgba("#223041")),
        "dpad_down": dpad_dir("DN", rgba("#D8E1EB"), rgba("#223041"), rgba("#223041")),
        "dpad_left": dpad_dir("LT", rgba("#D8E1EB"), rgba("#223041"), rgba("#223041")),
        "dpad_right": dpad_dir("RT", rgba("#D8E1EB"), rgba("#223041"), rgba("#223041")),
        "dpad": ControlSpec(kind="dpad", fill=rgba("#D8E1EB"), border=rgba("#223041"), text=rgba("#223041")),
        "face_cluster": cluster("x", "y", "a", "b", layout="diamond"),
        "shoulders": shoulder_pair("l", "r"),
    }
    return PlatformSpec(name="dreamcast", display_name="Dreamcast", controls=controls)


def build_steamdeck_platform() -> PlatformSpec:
    dark = rgba("#1C222A")
    controls: Dict[str, ControlSpec] = {
        "a": circle_button("A", dark),
        "b": circle_button("B", dark),
        "x": circle_button("X", dark),
        "y": circle_button("Y", dark),
        "l1": capsule_button("L1", rgba("#2B323E")),
        "r1": capsule_button("R1", rgba("#2B323E")),
        "l2": trigger_button("L2", rgba("#36404D")),
        "r2": trigger_button("R2", rgba("#36404D")),
        "l4": capsule_button("L4", rgba("#2B323E")),
        "l5": capsule_button("L5", rgba("#2B323E")),
        "r4": capsule_button("R4", rgba("#2B323E")),
        "r5": capsule_button("R5", rgba("#2B323E")),
        "menu": capsule_button("Menu", rgba("#2B323E")),
        "view": capsule_button("View", rgba("#2B323E")),
        "steam": circle_button("Steam", dark),
        "quick_access": circle_button("...", dark),
        "left_stick": stick_button("LS", rgba("#252C35")),
        "right_stick": stick_button("RS", rgba("#252C35")),
        "left_stick_click": stick_button("L3", rgba("#252C35")),
        "right_stick_click": stick_button("R3", rgba("#252C35")),
        "trackpad_left": trackpad_button("LT", dark, rgba("#50D2FF")),
        "trackpad_right": trackpad_button("RT", dark, rgba("#FFAE57")),
        "dpad_up": dpad_dir("UP", dark),
        "dpad_down": dpad_dir("DN", dark),
        "dpad_left": dpad_dir("LT", dark),
        "dpad_right": dpad_dir("RT", dark),
        "dpad": ControlSpec(kind="dpad", fill=dark, border=rgba("#111827"), text=rgba("#FFFFFF")),
        "face_cluster": cluster("x", "y", "a", "b", layout="diamond"),
        "shoulders": shoulder_pair("l1", "r1"),
        "back_grips": shoulder_pair("l4", "r4"),
    }
    return PlatformSpec(name="steamdeck", display_name="Steam Deck", controls=controls)


def generate_pack(output_root: Path | None = None) -> Path:
    target_root = output_root or ASSET_ROOT
    catalog = build_catalog()
    manifest = {
        "canvas_size": CANVAS_SIZE,
        "platforms": {},
    }

    for platform_name, platform in catalog.items():
        platform_root = target_root / platform_name
        platform_root.mkdir(parents=True, exist_ok=True)
        for control_name, spec in platform.controls.items():
            svg_canvas = SvgCanvas()
            png_canvas = PngCanvas()
            render_control(svg_canvas, platform, spec)
            render_control(png_canvas, platform, spec)
            svg_canvas.save(platform_root / f"{control_name}.svg")
            png_canvas.save(platform_root / f"{control_name}.png")
        manifest["platforms"][platform_name] = {
            "display_name": platform.display_name,
            "count": len(platform.controls),
            "controls": sorted(platform.controls.keys()),
        }

    manifest_path = target_root / "manifest.json"
    manifest_path.write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    (target_root / "README.md").write_text(build_readme(catalog), encoding="utf-8")
    return manifest_path


def build_readme(catalog: Dict[str, PlatformSpec]) -> str:
    lines = [
        "# Helengine Control Icon Pack",
        "",
        "Generated SVG masters and PNG exports for platform-authentic control prompts.",
        "",
        "## Layout",
        "",
        "- One folder per platform",
        "- One SVG and one PNG per control name",
        "- `manifest.json` contains the exported coverage",
        "",
        "## Platforms",
        "",
    ]
    for platform in catalog.values():
        lines.append(f"- `{platform.name}`: {len(platform.controls)} controls")
    lines.append("")
    return "\n".join(lines)


def render_control(canvas: PngCanvas | SvgCanvas, platform: PlatformSpec, spec: ControlSpec) -> None:
    if spec.kind == "key":
        render_key(canvas, spec)
    elif spec.kind == "circle":
        render_circle(canvas, spec)
    elif spec.kind == "capsule":
        render_capsule(canvas, spec, trigger=False)
    elif spec.kind == "trigger":
        render_capsule(canvas, spec, trigger=True)
    elif spec.kind == "dpad_dir":
        render_dpad_direction(canvas, spec)
    elif spec.kind == "dpad":
        render_dpad(canvas, spec)
    elif spec.kind == "stick":
        render_stick(canvas, spec)
    elif spec.kind == "trackpad":
        render_trackpad(canvas, spec)
    elif spec.kind == "touch":
        render_touch(canvas, spec)
    elif spec.kind == "symbol":
        render_symbol_button(canvas, spec)
    elif spec.kind == "n64_c_dir":
        render_n64_c_direction(canvas, spec)
    elif spec.kind == "cluster":
        render_cluster(canvas, platform, spec)
    elif spec.kind == "shoulder_pair":
        render_shoulder_pair(canvas, platform, spec)
    else:
        raise ValueError(f"Unsupported control kind: {spec.kind}")


def render_key(canvas: PngCanvas | SvgCanvas, spec: ControlSpec) -> None:
    label_size = 44 if len(spec.label) <= 2 else 34 if len(spec.label) <= 5 else 26
    width = 168 if len(spec.label) <= 2 else 186 if len(spec.label) <= 5 else 212
    bounds = centered_bounds(width, 116)
    canvas.rounded_rect(bounds, radius=24, fill=spec.fill, outline=spec.border, width=5)
    inset = inset_bounds(bounds, 8, 8)
    canvas.rounded_rect(inset, radius=18, fill=with_alpha((255, 255, 255, 255), 60), outline=with_alpha(spec.border, 70), width=2)
    canvas.text((128, 132), spec.label, label_size, spec.text)


def render_circle(canvas: PngCanvas | SvgCanvas, spec: ControlSpec) -> None:
    canvas.circle((128, 128), 74, spec.fill, spec.border, width=5)
    font_size = 44 if len(spec.label) <= 2 else 24 if len(spec.label) <= 5 else 18
    stroke = spec.border if sum(spec.text[:3]) > 540 else None
    canvas.text((128, 128), spec.label, font_size, spec.text, stroke_fill=stroke, stroke_width=2 if stroke else 0)


def render_capsule(canvas: PngCanvas | SvgCanvas, spec: ControlSpec, trigger: bool) -> None:
    bounds = centered_bounds(192, 108 if trigger else 92)
    canvas.rounded_rect(bounds, radius=36, fill=spec.fill, outline=spec.border, width=5)
    if trigger:
        notch = [(76, 74), (180, 74), (170, 100), (86, 100)]
        canvas.polygon(notch, with_alpha(spec.fill, 220), spec.border, width=3)
    label_size = 38 if len(spec.label) <= 3 else 28 if len(spec.label) <= 6 else 22
    canvas.text((128, 126), spec.label, label_size, spec.text)


def render_dpad_direction(canvas: PngCanvas | SvgCanvas, spec: ControlSpec) -> None:
    render_dpad_body(canvas, spec)
    arrow_centers = {
        "UP": (128, 74),
        "DN": (128, 182),
        "LT": (74, 128),
        "RT": (182, 128),
    }
    canvas.polygon(direction_arrow(spec.label, center=arrow_centers[spec.label], size=22), spec.text, None)


def render_dpad(canvas: PngCanvas | SvgCanvas, spec: ControlSpec) -> None:
    render_dpad_body(canvas, spec)
    text = spec.text
    for label, center in (("UP", (128, 74)), ("DN", (128, 182)), ("LT", (74, 128)), ("RT", (182, 128))):
        canvas.polygon(direction_arrow(label, center=center, size=22), text, None)


def render_dpad_body(canvas: PngCanvas | SvgCanvas, spec: ControlSpec) -> None:
    fill = spec.fill
    border = spec.border
    border_stem_v = centered_bounds(64, 184)
    border_stem_h = centered_bounds(184, 64)
    fill_stem_v = inset_bounds(border_stem_v, 5, 5)
    fill_stem_h = inset_bounds(border_stem_h, 5, 5)
    canvas.rounded_rect(border_stem_v, radius=24, fill=border, outline=None, width=0)
    canvas.rounded_rect(border_stem_h, radius=24, fill=border, outline=None, width=0)
    canvas.rounded_rect(fill_stem_v, radius=19, fill=fill, outline=None, width=0)
    canvas.rounded_rect(fill_stem_h, radius=19, fill=fill, outline=None, width=0)


def render_stick(canvas: PngCanvas | SvgCanvas, spec: ControlSpec) -> None:
    canvas.circle((128, 128), 74, spec.fill, spec.border, width=5)
    canvas.circle((128, 128), 38, with_alpha(spec.border, 180), with_alpha(spec.text, 120), width=4)
    canvas.line([(128, 54), (128, 32)], spec.text, width=5)
    canvas.polygon([(128, 20), (118, 36), (138, 36)], spec.text)
    label_size = 28 if len(spec.label) <= 3 else 22
    canvas.text((128, 194), spec.label, label_size, spec.text)


def render_trackpad(canvas: PngCanvas | SvgCanvas, spec: ControlSpec) -> None:
    bounds = centered_bounds(170, 170)
    canvas.rounded_rect(bounds, radius=32, fill=spec.fill, outline=spec.border, width=5)
    accent = spec.accent or spec.text
    for offset in (-36, -12, 12, 36):
        canvas.line([(92 + offset, 72), (92 + offset, 184)], with_alpha(accent, 130), width=2)
        canvas.line([(72, 92 + offset), (184, 92 + offset)], with_alpha(accent, 130), width=2)
    canvas.text((128, 210), spec.label, 22, spec.text)


def render_touch(canvas: PngCanvas | SvgCanvas, spec: ControlSpec) -> None:
    bounds = centered_bounds(170, 124)
    canvas.rounded_rect(bounds, radius=18, fill=spec.fill, outline=spec.border, width=5)
    accent = spec.accent or spec.text
    canvas.circle((110, 128), 18, accent, spec.border, width=3)
    canvas.line([(118, 116), (160, 76)], accent, width=8)
    canvas.line([(154, 82), (178, 66)], accent, width=8)
    canvas.text((128, 204), spec.label, 22, spec.text)


def render_symbol_button(canvas: PngCanvas | SvgCanvas, spec: ControlSpec) -> None:
    canvas.circle((128, 128), 74, spec.fill, spec.border, width=5)
    draw_symbol(canvas, spec.symbol, (128, 128), 70, spec.accent or spec.text)


def render_cluster(canvas: PngCanvas | SvgCanvas, platform: PlatformSpec, spec: ControlSpec) -> None:
    if spec.layout == "diamond":
        positions = ((76, 128), (128, 76), (128, 180), (180, 128))
    elif spec.layout == "gamecube":
        positions = ((62, 140), (128, 70), (136, 150), (210, 128))
    elif spec.layout == "pair_horizontal":
        positions = ((88, 128), (168, 128))
    elif spec.layout == "pair_diagonal":
        positions = ((88, 88), (168, 168))
    elif spec.layout == "n64_c_cluster":
        render_n64_c_cluster(canvas, platform, spec)
        return
    else:
        raise ValueError(f"Unsupported cluster layout: {spec.layout}")
    for member_name, center in zip(spec.members, positions):
        member_spec = platform.controls[member_name]
        render_mini_control(canvas, member_spec, center)


def render_shoulder_pair(canvas: PngCanvas | SvgCanvas, platform: PlatformSpec, spec: ControlSpec) -> None:
    centers = ((84, 128), (172, 128))
    for member_name, center in zip(spec.members[:2], centers):
        member_spec = platform.controls[member_name]
        render_mini_control(canvas, member_spec, center, small_wide=True)


def render_n64_c_cluster(canvas: PngCanvas | SvgCanvas, platform: PlatformSpec, spec: ControlSpec) -> None:
    render_n64_c_cluster_body(canvas, platform.controls["c_up"])
    for member_name in spec.members:
        member_spec = platform.controls[member_name]
        canvas.polygon(n64_c_cluster_triangles()[member_name], n64_c_cluster_triangle_color(member_spec.fill, member_spec.text))


def render_n64_c_direction(canvas: PngCanvas | SvgCanvas, spec: ControlSpec) -> None:
    render_n64_c_cluster_body(canvas, spec)
    canvas.polygon(n64_c_cluster_triangles()[spec.label], n64_c_cluster_triangle_color(spec.fill, spec.text))


def render_n64_c_cluster_body(canvas: PngCanvas | SvgCanvas, spec: ControlSpec) -> None:
    for center in n64_c_cluster_positions().values():
        canvas.circle(center, 36, spec.fill, spec.border, width=4)
    canvas.text((128, 128), "C", 28, spec.text)


def n64_c_cluster_positions() -> Dict[str, Point]:
    return {
        "c_left": (76, 128),
        "c_up": (128, 76),
        "c_down": (128, 180),
        "c_right": (180, 128),
    }


def n64_c_cluster_triangles() -> Dict[str, List[Point]]:
    return {
        "c_left": [(54, 128), (82, 116), (82, 140)],
        "c_up": [(128, 54), (116, 82), (140, 82)],
        "c_down": [(128, 202), (116, 174), (140, 174)],
        "c_right": [(202, 128), (174, 116), (174, 140)],
    }


def n64_c_cluster_triangle_color(fill: Color, text: Color) -> Color:
    return blend_colors(fill, text, 0.72)


def render_mini_control(
    canvas: PngCanvas | SvgCanvas,
    spec: ControlSpec,
    center: Point,
    small_wide: bool = False,
) -> None:
    if spec.kind in {"circle", "symbol", "stick"}:
        radius = 32 if small_wide else 36
        canvas.circle(center, radius, spec.fill, spec.border, width=4)
        if spec.kind == "symbol":
            draw_symbol(canvas, spec.symbol, center, 34, spec.accent or spec.text)
        elif spec.kind == "stick":
            inner = 18
            canvas.circle(center, inner, with_alpha(spec.border, 180), with_alpha(spec.text, 120), width=3)
            canvas.text((center[0], center[1] + 48), spec.label, 14, spec.text)
        else:
            size = 20 if len(spec.label) <= 3 else 14
            canvas.text(center, spec.label, size, spec.text)
    else:
        width = 76 if small_wide else 66
        height = 44 if small_wide else 54
        bounds = (center[0] - width / 2, center[1] - height / 2, center[0] + width / 2, center[1] + height / 2)
        canvas.rounded_rect(bounds, radius=18, fill=spec.fill, outline=spec.border, width=4)
        size = 16 if len(spec.label) <= 3 else 12
        canvas.text(center, spec.label, size, spec.text)


def draw_symbol(
    canvas: PngCanvas | SvgCanvas,
    symbol: str,
    center: Point,
    size: float,
    color: Color,
) -> None:
    cx, cy = center
    if symbol == "triangle":
        points = [(cx, cy - size * 0.32), (cx - size * 0.3, cy + size * 0.22), (cx + size * 0.3, cy + size * 0.22)]
        canvas.polygon(points, with_alpha(color, 0), color, width=6)
    elif symbol == "circle":
        canvas.circle(center, size * 0.28, with_alpha(color, 0), color, width=6)
    elif symbol == "square":
        bounds = (cx - size * 0.26, cy - size * 0.26, cx + size * 0.26, cy + size * 0.26)
        canvas.rounded_rect(bounds, radius=4, fill=with_alpha(color, 0), outline=color, width=6)
    elif symbol == "cross":
        arm = size * 0.24
        canvas.line([(cx - arm, cy - arm), (cx + arm, cy + arm)], color, width=7)
        canvas.line([(cx - arm, cy + arm), (cx + arm, cy - arm)], color, width=7)
    else:
        raise ValueError(f"Unsupported symbol: {symbol}")


def centered_bounds(width: float, height: float) -> Tuple[float, float, float, float]:
    return ((CANVAS_SIZE - width) / 2, (CANVAS_SIZE - height) / 2, (CANVAS_SIZE + width) / 2, (CANVAS_SIZE + height) / 2)


def inset_bounds(bounds: Tuple[float, float, float, float], x_pad: float, y_pad: float) -> Tuple[float, float, float, float]:
    return (bounds[0] + x_pad, bounds[1] + y_pad, bounds[2] - x_pad, bounds[3] - y_pad)


def direction_arrow(direction: str, center: Point, size: float) -> List[Point]:
    cx, cy = center
    if direction == "UP":
        return [(cx, cy - size), (cx - size * 0.7, cy + size * 0.2), (cx - size * 0.2, cy + size * 0.2), (cx - size * 0.2, cy + size), (cx + size * 0.2, cy + size), (cx + size * 0.2, cy + size * 0.2), (cx + size * 0.7, cy + size * 0.2)]
    if direction == "DN":
        return [(cx, cy + size), (cx - size * 0.7, cy - size * 0.2), (cx - size * 0.2, cy - size * 0.2), (cx - size * 0.2, cy - size), (cx + size * 0.2, cy - size), (cx + size * 0.2, cy - size * 0.2), (cx + size * 0.7, cy - size * 0.2)]
    if direction == "LT":
        return [(cx - size, cy), (cx + size * 0.2, cy - size * 0.7), (cx + size * 0.2, cy - size * 0.2), (cx + size, cy - size * 0.2), (cx + size, cy + size * 0.2), (cx + size * 0.2, cy + size * 0.2), (cx + size * 0.2, cy + size * 0.7)]
    return [(cx + size, cy), (cx - size * 0.2, cy - size * 0.7), (cx - size * 0.2, cy - size * 0.2), (cx - size, cy - size * 0.2), (cx - size, cy + size * 0.2), (cx - size * 0.2, cy + size * 0.2), (cx - size * 0.2, cy + size * 0.7)]


def main() -> None:
    generate_pack()


if __name__ == "__main__":
    main()
