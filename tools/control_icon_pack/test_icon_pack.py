import shutil
import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))

from icon_pack import build_catalog, generate_pack
from PIL import Image


def expected_n64_triangle_color(fill: tuple[int, int, int, int], text: tuple[int, int, int, int]) -> tuple[int, int, int, int]:
    return (
        round(fill[0] * 0.72 + text[0] * 0.28),
        round(fill[1] * 0.72 + text[1] * 0.28),
        round(fill[2] * 0.72 + text[2] * 0.28),
        255,
    )


class ControlIconPackTests(unittest.TestCase):
    def test_catalog_includes_all_requested_platforms(self) -> None:
        catalog = build_catalog()

        expected_platforms = {
            "keyboard",
            "xbox360",
            "switch",
            "gamecube",
            "wii",
            "ds",
            "3ds",
            "psp",
            "ps2",
            "psvita",
            "n64",
            "dreamcast",
            "ps1",
            "ps3",
            "xbox",
            "steamdeck",
        }

        self.assertEqual(expected_platforms, set(catalog.keys()))
        self.assertIn("a", catalog["xbox360"].controls)
        self.assertIn("dpad", catalog["switch"].controls)
        self.assertIn("touch", catalog["ds"].controls)
        self.assertIn("trackpad_left", catalog["steamdeck"].controls)
        self.assertIn("key_a", catalog["keyboard"].controls)
        self.assertIn("numpad_0", catalog["keyboard"].controls)

    def test_generate_pack_writes_svg_png_and_manifest(self) -> None:
        output_root = Path(tempfile.mkdtemp(prefix="control-icon-pack-"))
        try:
            manifest_path = generate_pack(output_root)

            self.assertTrue(manifest_path.is_file())
            self.assertTrue((output_root / "keyboard" / "key_a.svg").is_file())
            self.assertTrue((output_root / "keyboard" / "key_a.png").is_file())
            self.assertTrue((output_root / "xbox360" / "a.svg").is_file())
            self.assertTrue((output_root / "xbox360" / "a.png").is_file())
            self.assertTrue((output_root / "switch" / "dpad.svg").is_file())
            self.assertTrue((output_root / "steamdeck" / "trackpad_left.svg").is_file())
        finally:
            shutil.rmtree(output_root)

    def test_3ds_dpad_composite_keeps_arrows_without_inner_pill_borders(self) -> None:
        output_root = Path(tempfile.mkdtemp(prefix="control-icon-pack-"))
        try:
            generate_pack(output_root)

            catalog = build_catalog()
            fill = catalog["3ds"].controls["dpad"].fill
            text = catalog["3ds"].controls["dpad"].text
            dpad_image = Image.open(output_root / "3ds" / "dpad.png")

            for sample_point in ((100, 128), (156, 128), (128, 100), (128, 156)):
                self.assertEqual(fill, dpad_image.getpixel(sample_point))

            for sample_point in ((182, 128), (74, 128), (128, 74), (128, 182)):
                self.assertEqual(text, dpad_image.getpixel(sample_point))
        finally:
            shutil.rmtree(output_root)

    def test_3ds_face_cluster_has_center_gap_between_buttons(self) -> None:
        output_root = Path(tempfile.mkdtemp(prefix="control-icon-pack-"))
        try:
            generate_pack(output_root)

            face_cluster_image = Image.open(output_root / "3ds" / "face_cluster.png")

            for sample_point in ((116, 116), (140, 116), (116, 140), (140, 140)):
                self.assertEqual((0, 0, 0, 0), face_cluster_image.getpixel(sample_point))
        finally:
            shutil.rmtree(output_root)

    def test_3ds_circle_pad_uses_pad_label(self) -> None:
        catalog = build_catalog()

        self.assertEqual("Pad", catalog["3ds"].controls["circle_pad"].label)

    def test_n64_face_cluster_is_an_ab_pair(self) -> None:
        output_root = Path(tempfile.mkdtemp(prefix="control-icon-pack-"))
        try:
            catalog = build_catalog()
            self.assertEqual(("b", "a"), catalog["n64"].controls["face_cluster"].members)
            self.assertEqual("pair_diagonal", catalog["n64"].controls["face_cluster"].layout)

            generate_pack(output_root)
            face_cluster_image = Image.open(output_root / "n64" / "face_cluster.png")

            for sample_point in ((88, 128), (168, 128), (128, 88), (128, 168)):
                self.assertEqual((0, 0, 0, 0), face_cluster_image.getpixel(sample_point))

            for sample_point in ((88, 88), (168, 168)):
                self.assertNotEqual((0, 0, 0, 0), face_cluster_image.getpixel(sample_point))
        finally:
            shutil.rmtree(output_root)

    def test_n64_c_cluster_uses_directional_triangles_with_center_c(self) -> None:
        output_root = Path(tempfile.mkdtemp(prefix="control-icon-pack-"))
        try:
            catalog = build_catalog()
            self.assertEqual("n64_c_cluster", catalog["n64"].controls["c_cluster"].layout)

            generate_pack(output_root)
            c_cluster_image = Image.open(output_root / "n64" / "c_cluster.png")
            fill = catalog["n64"].controls["c_up"].fill
            text = catalog["n64"].controls["c_up"].text
            triangle = expected_n64_triangle_color(fill, text)

            for sample_point in ((128, 88), (114, 76), (142, 76), (88, 128), (76, 114), (76, 142), (168, 128), (180, 114), (180, 142), (128, 168), (114, 180), (142, 180)):
                self.assertEqual(fill, c_cluster_image.getpixel(sample_point))

            for sample_point in ((128, 64), (64, 128), (192, 128), (128, 192)):
                self.assertEqual(triangle, c_cluster_image.getpixel(sample_point))

            for sample_point in ((128, 70), (70, 128), (186, 128), (128, 186)):
                self.assertEqual(triangle, c_cluster_image.getpixel(sample_point))

            self.assertNotEqual((0, 0, 0, 0), c_cluster_image.getpixel((120, 128)))
        finally:
            shutil.rmtree(output_root)

    def test_n64_dpad_left_uses_full_dpad_shape_with_single_arrow(self) -> None:
        output_root = Path(tempfile.mkdtemp(prefix="control-icon-pack-"))
        try:
            catalog = build_catalog()
            fill = catalog["n64"].controls["dpad"].fill
            text = catalog["n64"].controls["dpad"].text

            generate_pack(output_root)
            dpad_left_image = Image.open(output_root / "n64" / "dpad_left.png")

            for sample_point in ((128, 76), (180, 128), (128, 180), (128, 128)):
                self.assertEqual(fill, dpad_left_image.getpixel(sample_point))

            self.assertEqual(text, dpad_left_image.getpixel((74, 128)))
        finally:
            shutil.rmtree(output_root)

    def test_n64_c_left_uses_full_c_cluster_with_single_triangle(self) -> None:
        output_root = Path(tempfile.mkdtemp(prefix="control-icon-pack-"))
        try:
            catalog = build_catalog()
            fill = catalog["n64"].controls["c_up"].fill
            text = catalog["n64"].controls["c_up"].text
            triangle = expected_n64_triangle_color(fill, text)

            generate_pack(output_root)
            c_left_image = Image.open(output_root / "n64" / "c_left.png")

            for sample_point in ((128, 88), (114, 76), (142, 76), (128, 168), (114, 180), (142, 180), (168, 128), (180, 114), (180, 142)):
                self.assertEqual(fill, c_left_image.getpixel(sample_point))

            for sample_point in ((64, 128), (70, 128)):
                self.assertEqual(triangle, c_left_image.getpixel(sample_point))

            self.assertEqual(fill, c_left_image.getpixel((128, 64)))
            self.assertEqual(fill, c_left_image.getpixel((192, 128)))
            self.assertEqual(fill, c_left_image.getpixel((128, 192)))
            self.assertNotEqual((0, 0, 0, 0), c_left_image.getpixel((120, 128)))
        finally:
            shutil.rmtree(output_root)

    def test_n64_start_uses_all_caps_label(self) -> None:
        catalog = build_catalog()

        self.assertEqual("START", catalog["n64"].controls["start"].label)

    def test_n64_control_stick_has_no_word_label(self) -> None:
        catalog = build_catalog()

        self.assertEqual("", catalog["n64"].controls["control_stick"].label)


if __name__ == "__main__":
    unittest.main()
