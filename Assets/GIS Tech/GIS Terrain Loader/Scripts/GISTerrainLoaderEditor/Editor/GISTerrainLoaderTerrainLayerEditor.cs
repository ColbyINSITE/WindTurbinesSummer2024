using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    [CustomPropertyDrawer(typeof(GISTerrainLoaderTerrainLayer))]
    public class GISTerrainLoaderTerrainLayerEditor : PropertyDrawer
    {
        SerializedProperty diffuseProperty;
        SerializedProperty normalMapProperty;
        SerializedProperty x_height;
        SerializedProperty y_height;
        SerializedProperty textureSizeProperty;
        SerializedProperty Show_height;


        private static GUIStyle s_TempStyle = new GUIStyle();

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            diffuseProperty = property.FindPropertyRelative("Diffuse");
            normalMapProperty = property.FindPropertyRelative("NormalMap");

            x_height = property.FindPropertyRelative("X_Height");
            y_height = property.FindPropertyRelative("Y_Height");
            Show_height = property.FindPropertyRelative("ShowHeight") ;

            textureSizeProperty = property.FindPropertyRelative("TextureSize");

            EditorGUILayout.BeginHorizontal();

            diffuseProperty.objectReferenceValue = TextureField("Diffuse", diffuseProperty);
            normalMapProperty.objectReferenceValue = TextureField("NormalMap", normalMapProperty);
            EditorGUILayout.BeginVertical();
 
            if(Show_height.boolValue)
            {
                GUILayout.Label(" Height ");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(" From ", GUILayout.MaxWidth(50));
                x_height.floatValue = EditorGUILayout.Slider(x_height.floatValue, 0.0f, 1, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(" To   ", GUILayout.MaxWidth(50));
                y_height.floatValue = EditorGUILayout.Slider(y_height.floatValue, 0.0f, 1, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Label(new GUIContent(" Size ", "    "), GUILayout.MaxWidth(200));
            textureSizeProperty.vector2Value = EditorGUILayout.Vector2Field("", textureSizeProperty.vector2Value);


            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();



        }
        private Texture2D TextureField(string name, SerializedProperty property)
        {
            GUILayout.BeginVertical();
            var style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.UpperCenter;
            style.fixedWidth = 70;
            style.fixedHeight = 20;
            GUILayout.Label(name, style);
            var result = (Texture2D)EditorGUILayout.ObjectField((Texture2D)property.objectReferenceValue, typeof(Texture2D), false, GUILayout.Width(100), GUILayout.Height(70));
            GUILayout.EndVertical();
            return result;
        }
    }
}
