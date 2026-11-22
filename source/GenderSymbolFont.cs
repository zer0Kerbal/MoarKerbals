using System;
using TMPro;
using UnityEngine;
using MoarKerbals.Core;

namespace MoarKerbals
{
    internal static class GenderSymbolFont
    {
        private static TMP_FontAsset font;
        private static bool failed;

        internal static void Ensure()
        {
            if (failed) return;
            try
            {
                var fallbacks = TMP_Settings.fallbackFontAssets;
                if (fallbacks == null) return;
                if (font == null) font = Create();
                if (!fallbacks.Contains(font)) fallbacks.Add(font);
            }
            catch (Exception error)
            {
                failed = true;
                Logging.DLogWarning("Gender symbol font unavailable: " + error);
            }
        }

        private static TMP_FontAsset Create()
        {
            const int size = 128, height = 80;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "MoarKerbals gender symbols SDF", filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontUnloadUnusedAsset
            };
            var pixels = new Color32[size * size];
            for (int y = size - height; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    double distance = GenderLabels.Distance(x >= 64, x % 64 + 0.5, y - (size - height) + 0.5);
                    byte alpha = (byte)Math.Round(Math.Max(0, Math.Min(1, 0.5 - distance / 12)) * 255);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            var asset = ScriptableObject.CreateInstance<TMP_FontAsset>();
            asset.name = "MoarKerbalsGenderSymbols";
            asset.fontAssetType = TMP_FontAsset.FontAssetTypes.SDF;
            asset.hashCode = TMP_TextUtilities.GetSimpleHashCode(asset.name);
            asset.hideFlags = HideFlags.DontUnloadUnusedAsset;
            asset.atlas = texture;
            asset.AddFaceInfo(new FaceInfo { Name = asset.name, PointSize = 64, Scale = 1, CharacterCount = 2,
                LineHeight = 80, Ascender = 72, CapHeight = 64, Descender = -8, Padding = 6,
                AtlasWidth = size, AtlasHeight = size });
            asset.AddGlyphInfo(new[] { Glyph(0x2640, 0), Glyph(0x2642, 64) });
            asset.material = new Material(TMP_Settings.defaultFontAsset.material)
            {
                name = "MoarKerbals gender symbols material", hideFlags = HideFlags.DontUnloadUnusedAsset
            };
            asset.material.mainTexture = texture;
            asset.materialHashCode = TMP_TextUtilities.GetSimpleHashCode(asset.material.name);
            asset.material.SetFloat("_TextureWidth", size);
            asset.material.SetFloat("_TextureHeight", size);
            asset.material.SetFloat("_GradientScale", 7);
            // This TextMeshPro version reads the kerning table even for a font with no pairs.
            asset.AddKerningInfo(new KerningTable());
            asset.ReadFontDefinition();
            return asset;
        }

        private static TMP_Glyph Glyph(int codepoint, int x) => new TMP_Glyph {
            id = codepoint, x = x, y = 0, width = 64, height = 80,
            xOffset = 0, yOffset = 72, xAdvance = 64, scale = 1
        };
    }
}
