using UnityEngine;

public static class NoiseMap
{
    public static float[,] CreateNoiseMap(int width, int height, float size, int seed, Vector2 offset, int fract)
    {
        var scale = 1.0f / size;
        var noiseMap = new float[width, height];
        var maxNoiseHeight = float.MinValue;
        var minNoiseHeight = float.MaxValue;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                var noiseValue = Perlin.Fbm(seed + (j + offset.x) * scale, seed + (i + offset.y) * scale, fract);
                if (noiseValue < minNoiseHeight)
                {
                    minNoiseHeight = noiseValue;
                }
                else if (noiseValue > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseValue;
                }
                noiseMap[j, i] = (noiseValue);
            }
        }
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                noiseMap[j, i] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[j, i]);
            }
        }
        return noiseMap;
    }
}