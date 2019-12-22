using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColourMap(Color[] map, int height, int width)
    {
        var texture = new Texture2D(width, height);
        //texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(map);
        texture.Apply();
        return texture;
    }

    public static Texture2D GenerateTextureFromNoiseMap(float[,] map)
    {
        var colorMap = new Color[map.GetLength(0) * map.GetLength(1)];
        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                colorMap[y * map.GetLength(0) + x] = Color.Lerp(Color.black, Color.white, map[x, y]);
            }
        }
        return TextureFromColourMap(colorMap, map.GetLength(0), map.GetLength(1));
    }
}
