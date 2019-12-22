using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ImageCreator : MonoBehaviour
{

    public enum ImageType
    {
        PNG,
        JPG,
        EXR,
        TGA,
    }

    public Renderer textureRender;
    public int Width;
    public int Height;
    public int Seed;
    public int Size = 64;
    public Vector2 Offset;

    [SerializeField, Range(1, 5)]
    int _fractalLevel = 1;

    public ImageType ExportImageType;


    public void SaveToImage()
    {
        var texture = (Texture2D)textureRender.sharedMaterial.mainTexture;
        if (texture == null || textureRender == null)
        {
            Debug.LogError("Texture from textureRender not found!");
            return;
        }
        byte[] bytes = new byte[1];
        var fileType = "";
        switch (ExportImageType)
        {
            case ImageType.PNG:
                bytes = texture.EncodeToPNG();
                fileType = "png";
                break;
            case ImageType.JPG:
                bytes = texture.EncodeToJPG();
                fileType = "jpg";
                break;
            case ImageType.EXR:
                bytes = texture.EncodeToEXR();
                fileType = "exr";
                break;
            case ImageType.TGA:
                bytes = texture.EncodeToTGA();
                fileType = "tga";
                break;
        }

        var path = EditorUtility.SaveFilePanel(
           "Save texture as " + fileType,
           "",
           texture.name + "." + fileType,
           fileType);

        if (path.Length != 0)
        {
            if (bytes.Length > 1)
            {
                File.WriteAllBytes(path, bytes);
            }
        }
    }

    public void CreateRandomNoiseTexture()
    {
        this.Width = Random.Range(32, 513);
        this.Height = this.Width;
        this.Size = Random.Range(1, 20);
        this.Offset = new Vector2(Random.Range(0, 200f), Random.Range(0, 200f));
        this.Seed = Random.Range(-10000, 10000);
        this._fractalLevel = Random.Range(1, 6);
        Draw2DNosieTextureBaseOnProperty();
    }

    public void Draw2DNosieTextureBaseOnProperty()
    {
        var noiseMap = NoiseMap.CreateNoiseMap(Width, Height, Size, Seed, Offset, _fractalLevel);
        var texture = TextureGenerator.GenerateTextureFromNoiseMap(noiseMap);
        textureRender.sharedMaterial.mainTexture = texture;
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
        Draw2DNosieTextureBaseOnProperty();
    }
}
