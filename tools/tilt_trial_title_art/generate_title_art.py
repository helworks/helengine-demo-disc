"""Generates the deterministic PNG sprite set used by the Tilt Trial title screen."""

from pathlib import Path
import sys

VENDOR_DIR = Path(__file__).resolve().parents[1] / "control_icon_pack" / "vendor"
sys.path.insert(0, str(VENDOR_DIR))

from PIL import Image, ImageDraw, ImageFont


OUTPUT_ROOT = Path(__file__).resolve().parents[2] / "assets" / "images" / "ui" / "tilt_trial" / "title"
FONT_PATH = Path("C:/Windows/Fonts/trebucbd.ttf")


def require_font(size: int) -> ImageFont.FreeTypeFont:
    """Returns the approved system font used to bake reliable title-screen lettering."""
    if not FONT_PATH.is_file():
        raise FileNotFoundError(f"Required title-art font is unavailable: {FONT_PATH}")

    return ImageFont.truetype(str(FONT_PATH), size)


def draw_centered_text(draw: ImageDraw.ImageDraw, bounds: tuple[int, int, int, int], label: str, font: ImageFont.FreeTypeFont, fill: tuple[int, int, int, int], stroke_fill: tuple[int, int, int, int], stroke_width: int) -> None:
    """Draws one label centered within the supplied bounds with a readable game-show outline."""
    center_x = (bounds[0] + bounds[2]) // 2
    center_y = (bounds[1] + bounds[3]) // 2
    draw.text((center_x, center_y), label, font=font, fill=fill, anchor="mm", stroke_fill=stroke_fill, stroke_width=stroke_width)


def generate_background() -> None:
    """Creates the opaque title backdrop containing the fixed Tilt Trial wordmark and arena decoration."""
    image = Image.new("RGBA", (1280, 720), (17, 18, 56, 255))
    draw = ImageDraw.Draw(image)
    draw.polygon([(0, 0), (310, 0), (0, 260)], fill=(12, 151, 183, 255))
    draw.polygon([(1280, 0), (970, 0), (1280, 285)], fill=(104, 46, 168, 255))
    draw.ellipse((72, 424, 246, 598), outline=(92, 239, 222, 255), width=8)
    draw.ellipse((104, 456, 214, 566), outline=(255, 205, 44, 255), width=6)
    draw.ellipse((1052, 450, 1164, 562), fill=(255, 205, 44, 255), outline=(255, 243, 151, 255), width=4)
    draw.rounded_rectangle((938, 316, 1186, 368), radius=18, fill=(45, 36, 103, 255), outline=(92, 239, 222, 255), width=4)
    draw_centered_text(draw, (220, 150, 1060, 300), "TILT TRIAL", require_font(92), (255, 205, 44, 255), (69, 24, 105, 255), 8)
    image.save(OUTPUT_ROOT / "background.png")


def generate_button(filename: str, size: tuple[int, int], label: str, primary: bool, selected: bool) -> None:
    """Creates one baked label-and-chrome button texture for a title-screen action state."""
    fill = (255, 205, 44, 255) if primary else (45, 36, 103, 255)
    border = (255, 53, 177, 255) if selected else ((255, 243, 151, 255) if primary else (92, 239, 222, 255))
    label_fill = (30, 22, 63, 255) if primary else (247, 248, 252, 255)
    image = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    draw.rounded_rectangle((2, 2, size[0] - 3, size[1] - 3), radius=16, fill=fill, outline=border, width=4)
    font_size = 34 if primary else 20 if label == "OPTIONS" else 15
    draw_centered_text(draw, (0, 0, size[0], size[1]), label, require_font(font_size), label_fill, (0, 0, 0, 0), 0)
    image.save(OUTPUT_ROOT / filename)


def generate_assets() -> None:
    """Writes every title-screen texture consumed by the authored scene generator."""
    OUTPUT_ROOT.mkdir(parents=True, exist_ok=True)
    generate_background()
    generate_button("button_primary.png", (520, 72), "PLAY", True, False)
    generate_button("button_primary_selected.png", (520, 72), "PLAY", True, True)
    generate_button("button_secondary_options.png", (250, 52), "OPTIONS", False, False)
    generate_button("button_secondary_options_selected.png", (250, 52), "OPTIONS", False, True)
    generate_button("button_secondary_demo_disc.png", (250, 52), "BACK TO DEMO DISC", False, False)
    generate_button("button_secondary_demo_disc_selected.png", (250, 52), "BACK TO DEMO DISC", False, True)


if __name__ == "__main__":
    generate_assets()
