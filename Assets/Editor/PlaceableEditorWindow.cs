using System;
using System.Collections.Generic;
using ScriptableObjectS;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class PlaceableEditorWindow: EditorWindow
    {
        private Dictionary<string, bool> foldouts = new Dictionary<string, bool>();
        private string soFilter = String.Empty;
        
        [MenuItem("Window/Placeable Editor")]
        public static void ShowWindow()
        {
            GetWindow<PlaceableEditorWindow>("Placeable Objects");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Placeable Objects");
            EditorGUILayout.Space();
            soFilter = EditorGUILayout.TextField("Filter: ", soFilter);
            EditorGUILayout.Space();
            
            string[] guids = AssetDatabase.FindAssets("t:PlaceableSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PlaceableSO so = AssetDatabase.LoadAssetAtPath<PlaceableSO>(path);
                
                if(soFilter.Length > 0 && !so.name.Contains(soFilter, StringComparison.InvariantCultureIgnoreCase))
                    continue;
                
                foldouts.TryAdd(so.name, false);
                
                foldouts[so.name] = EditorGUILayout.Foldout(foldouts[so.name], so.name);

                if (foldouts[so.name])
                {
                    GUILayout.Label($"ScriptableObject: {so.name}");
                    
                    // Create a SerializedObject for the ScriptableObject
                    SerializedObject serializedObject = new SerializedObject(so);
                    SerializedProperty property = serializedObject.GetIterator();
            
                    // Iterate through all serialized properties
                    property.NextVisible(true); // Skip the "m_Script" property
                    while (property.NextVisible(false))
                    {
                        // Display each property
                        EditorGUILayout.PropertyField(property, true);
                    }
            
                    serializedObject.ApplyModifiedProperties();

                    
                    if (GUILayout.Button("I am happy to be here!"))
                    {
                
                    }
                }
            }
        }
    }
}
