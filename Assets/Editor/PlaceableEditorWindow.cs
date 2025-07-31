using System;
using ScriptableObjectS;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class PlaceableEditorWindow: EditorWindow
    {
        [MenuItem("Window/Placeable Editor")]
        public static void ShowWindow()
        {
            GetWindow<PlaceableEditorWindow>("Placeable Objects");
        }

        private void OnGUI()
        {
            GUILayout.Label("Testing");

            if (GUILayout.Button("Test"))
            {
                
            }
            
            string[] guids = AssetDatabase.FindAssets("t:PlaceableSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PlaceableSO so = AssetDatabase.LoadAssetAtPath<PlaceableSO>(path);
                
                GUILayout.Label($"ScriptableObject: {so.name}");
            }
        }
    }
}
