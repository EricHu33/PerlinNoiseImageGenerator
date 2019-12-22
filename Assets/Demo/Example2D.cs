using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example2D : MonoBehaviour
{
    public Renderer textureRender;
    public int Width;
    public int Height;
    public int seed;
    public int size = 64;

    [SerializeField, Range(1, 5)]
    int _fractalLevel = 1;

    public void Draw2DNosieTexture()
    {
        var noiseMap = NoiseMap.CreateNoiseMap(Width, Height, size, seed, new Vector2(0, 0), _fractalLevel);
        var texture = TextureGenerator.GenerateTextureFromNoiseMap(noiseMap);
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
        Draw2DNosieTexture();
    }
}
