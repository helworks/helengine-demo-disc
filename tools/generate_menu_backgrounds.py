#!/usr/bin/env python3
"""Generate the shared and PS1-sized variants of the DemoDisc menu background."""

from __future__ import annotations

import json
import struct
import zlib
from pathlib import Path


PALETTE = [
    (24, 16, 40),       # authored camera clear
    (120, 104, 144),    # scanline
    (72, 48, 100),      # grid
    (96, 76, 120),      # scanline over grid
]
VARIANTS = (
    ("main_menu_background.png", 512, 512, "RGB", "c6b0ea0d4e534180af4313cce7d93f12"),
    ("main_menu_background_ps1.png", 240, 240, "INDEXED4", "9ef42b9c5a7d4862a9d1336e1f9c72a4"),
)


def chunk(kind: bytes, payload: bytes) -> bytes:
    return struct.pack(">I", len(payload)) + kind + payload + struct.pack(">I", zlib.crc32(kind + payload) & 0xFFFFFFFF)


def color_index(x: int, y: int, *, ps1: bool) -> int:
    scanline_period = 3 if ps1 else 12
    grid_x_period = 12 if ps1 else 48
    grid_y_period = 16 if ps1 else 48
    scanline = y % scanline_period == 1 if ps1 else y % scanline_period == 4
    grid = x % grid_x_period == 0 or y % grid_y_period == 0
    return 3 if scanline and grid else 1 if scanline else 2 if grid else 0


def make_png(width: int, height: int, indexed4: bool) -> bytes:
    rows = bytearray()
    for y in range(height):
        rows.append(0)
        if indexed4:
            for x in range(0, width, 2):
                rows.append((color_index(x, y, ps1=True) << 4) | color_index(x + 1, y, ps1=True))
        else:
            for x in range(width):
                rows.extend(PALETTE[color_index(x, y, ps1=False)])

    if indexed4:
        header = struct.pack(">IIBBBBB", width, height, 4, 3, 0, 0, 0)
        palette = bytes(channel for color in PALETTE for channel in color)
        body = chunk(b"IHDR", header) + chunk(b"PLTE", palette)
    else:
        header = struct.pack(">IIBBBBB", width, height, 8, 2, 0, 0, 0)
        body = chunk(b"IHDR", header)
    return b"\x89PNG\r\n\x1a\n" + body + chunk(b"IDAT", zlib.compress(bytes(rows), level=9)) + chunk(b"IEND", b"")


def main() -> None:
    asset_root = Path(__file__).resolve().parents[1] / "assets" / "textures" / "menu"
    asset_root.mkdir(parents=True, exist_ok=True)
    for filename, width, height, _kind, asset_id in VARIANTS:
        indexed4 = filename.endswith("_ps1.png")
        source = asset_root / filename
        source.write_bytes(make_png(width, height, indexed4))
        metadata = {"version": 1, "assetId": asset_id, "formerAssetIds": []}
        source.with_suffix(source.suffix + ".hmeta").write_text(json.dumps(metadata, indent=2) + "\n", encoding="utf-8")
        description = "indexed 4-bit" if indexed4 else "RGB"
        print(f"{source}: {width}x{height}, {description}")


if __name__ == "__main__":
    main()
