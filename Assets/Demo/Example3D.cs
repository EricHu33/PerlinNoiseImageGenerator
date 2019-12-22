using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example3D : MonoBehaviour
{
    public Renderer textureRender;
    public int Width;
    public int Height;
    public int seed;
    public int size = 64;

    [SerializeField, Range(1, 5)]
    int _fractalLevel = 1;

    public void Draw3DNosieTexture()
    {
        var scale = 1.0f / size;
        var heightMap = new float[Width, Height];
        var maxNoiseHeight = float.MinValue;
        var minNoiseHeight = float.MaxValue;
        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                var noiseValue = Perlin.Fbm((seed + j * scale), seed + i * scale, Time.time, _fractalLevel);
                if (noiseValue < minNoiseHeight)
                {
                    minNoiseHeight = noiseValue;
                }
                else if (noiseValue > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseValue;
                }
                heightMap[j, i] = (noiseValue);
            }
        }
        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                heightMap[j, i] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, heightMap[j, i]);
            }
        }
        var texture = TextureGenerator.GenerateTextureFromNoiseMap(heightMap);
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void Update()
    {
        Draw3DNosieTexture();
    }

    public void OnValidate()
    {
        if (Width < 1)
        {
            Width = 1;
        }
        if (Height < 1)
        {
            Height = 1;
        }
        Draw3DNosieTexture();
    }
}
