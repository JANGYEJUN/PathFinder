using System;
using UnityEditor;
using UnityEngine;
using Yejun.Tool;

namespace YejunEditor.Tool
{
    [CustomEditor(typeof(Terrain2D), true)]
    [CanEditMultipleObjects]
    public class Terrain2DEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var terrain = target as Terrain2D;

            int width = serializedObject.FindProperty("WIDTH").intValue;
            int height = serializedObject.FindProperty("HEIGHT").intValue;

            if (GUILayout.Button("Build Terrain"))
            {
                SpriteRenderer terrainSprite = terrain.GetComponent<SpriteRenderer>();
                terrainSprite.size = new Vector2(width, height);

                Vector3 pos = new Vector3(width, height) * 0.5f;
                terrain.transform.position = pos;
                Camera.main.transform.position = pos;
                Camera.main.orthographicSize = Math.Max(width, height) * 0.66f;
            }
        }
    }
}