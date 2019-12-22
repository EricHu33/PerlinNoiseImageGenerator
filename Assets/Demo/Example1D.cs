using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example1D : MonoBehaviour
{
    public Renderer textureRender;
    public int Width;
    public int Height;
    public int seed;
    public int size = 64;

    [SerializeField, Range(1, 5)]
    int _fractalLevel = 1;

    public void Draw1DNosieTexture()
    {
        var scale = 1.0f / size;
        var heightMap = new float[Width, Height];
        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                var noiseValue = Perlin.Noise(j * scale + Time.time);
                heightMap[j, i] = noiseValue;
            }
        }
        var texture = TextureGenerator.GenerateTextureFromNoiseMap(heightMap);
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
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
        Draw1DNosieTexture();
    }
}
