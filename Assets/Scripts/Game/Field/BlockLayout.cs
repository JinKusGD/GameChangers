using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class BlockLayout
{
    public const int Size = 48;

    private const byte MinOpaqueAlpha = 128;

    private static readonly Color32[] PixelToBlockKeyColors =
    {
        new Color32(0x8C, 0x85, 0x80, 255),
        new Color32(0xCC, 0x7A, 0x47, 255),
        new Color32(0xF2, 0xCC, 0x40, 255),
        new Color32(0x73, 0xCC, 0xF2, 255),
        new Color32(0xB3, 0x73, 0xE6, 255),
        new Color32(0x66, 0xD9, 0x73, 255),
        new Color32(0xF2, 0x73, 0x8C, 255),
        new Color32(0x4D, 0x66, 0xD9, 255),
        new Color32(0x26, 0x26, 0x26, 255),
    };

    private readonly int[] _blockKeys = new int[Size * Size];

    public string Id { get; }

    private BlockLayout(string id)
    {
        Id = id;
    }

    public static BlockLayout Create(Texture2D texture, List<string> errors)
    {
        if (!IsValidTexture(texture, errors)) { return null; }

        BlockLayout layout = new BlockLayout(texture.name);
        Color32[] pixels = texture.GetPixels32();

        PixelParseResult result = ParsePixels(pixels, layout._blockKeys);

        if (!IsValidLayout(result, errors)) { return null; }

        return layout;
    }

    public int GetBlockKey(int column, int row)
    {
        int cellIndex = row * Size + column;

        return _blockKeys[cellIndex];
    }

    public bool UsesBlockKey(int blockKey)
    {
        return Array.IndexOf(_blockKeys, blockKey) >= 0;
    }

    private static bool IsValidTexture(Texture2D texture, List<string> errors)
    {
        if (texture.width != Size || texture.height != Size)
        {
            errors.Add($"크기 {texture.width}x{texture.height} (반드시 {Size}x{Size})");
            return false;
        }

        if (!texture.isReadable)
        {
            errors.Add("Read/Write가 꺼져 있음");
            return false;
        }

        if (GraphicsFormatUtility.IsCompressedFormat(texture.format))
        {
            errors.Add($"텍스처가 압축됨 ({texture.format}). Import 설정에서 Compression을 None으로 바꿔야 함");
            return false;
        }

        return true;
    }

    private static PixelParseResult ParsePixels(Color32[] pixels, int[] blockKeys)
    {
        PixelParseResult result = new PixelParseResult();

        for (int i = 0; i < pixels.Length; i++)
        {
            Color32 pixel = pixels[i];

            if (pixel.a < MinOpaqueAlpha) { continue; }

            int blockKey = FindBlockKey(pixel);

            if (blockKey == BlockKey.Empty)
            {
                result.AddUnknown(i, pixel);
                continue;
            }

            blockKeys[i] = blockKey;
            result.BlockCount++;
        }

        return result;
    }

    private static bool IsValidLayout(PixelParseResult result, List<string> errors)
    {
        bool isValid = true;

        if (result.UnknownCount > 0)
        {
            errors.Add($"팔레트에 없는 색 픽셀 {result.UnknownCount}개 (처음 위치 {DescribePixel(result.FirstUnknownIndex, result.FirstUnknownColor)})");
            isValid = false;
        }

        if (result.BlockCount == 0)
        {
            errors.Add("블록이 하나도 없음");
            isValid = false;
        }

        return isValid;
    }

    private static string DescribePixel(int pixelIndex, Color32 color)
    {
        int x = pixelIndex % Size;
        int y = Size - 1 - pixelIndex / Size;
        string hex = ColorUtility.ToHtmlStringRGB(color);

        return $"x {x}, y {y}, #{hex}";
    }

    private static int FindBlockKey(Color32 pixel)
    {
        for (int i = 0; i < PixelToBlockKeyColors.Length; i++)
        {
            Color32 keyColor = PixelToBlockKeyColors[i];

            if (pixel.r == keyColor.r && pixel.g == keyColor.g && pixel.b == keyColor.b)
            {
                return i + 1;
            }
        }

        return BlockKey.Empty;
    }

    private struct PixelParseResult
    {
        public int BlockCount;
        public int UnknownCount;
        public int FirstUnknownIndex;
        public Color32 FirstUnknownColor;

        public void AddUnknown(int pixelIndex, Color32 color)
        {
            if (UnknownCount == 0)
            {
                FirstUnknownIndex = pixelIndex;
                FirstUnknownColor = color;
            }

            UnknownCount++;
        }
    }
}
