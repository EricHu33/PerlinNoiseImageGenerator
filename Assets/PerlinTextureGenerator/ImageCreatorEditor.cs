using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ImageCreator))]
public class ImageCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ImageCreator imageCreator = (ImageCreator)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Randomize"))
        {
            imageCreator.CreateRandomNoiseTexture();
        }
        if (GUILayout.Button("Save To Image"))
        {
            imageCreator.SaveToImage();
        }
    }
}
