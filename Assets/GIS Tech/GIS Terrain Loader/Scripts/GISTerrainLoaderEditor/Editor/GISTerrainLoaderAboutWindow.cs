/*     Unity GIS Tech 2020-2021      */

using System;
using UnityEditor;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderAboutWindow : EditorWindow
    {
        [MenuItem("Tools/GIS Tech/GIS Terrain Loader/About")]

        public static void Init()
        {
            GISTerrainLoaderAboutWindow window = GetWindow<GISTerrainLoaderAboutWindow>(true, "About", true);
            window.minSize = new Vector2(200, 150);
            window.maxSize = new Vector2(200, 150);
        }

        public void OnGUI()
        {
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.alignment = TextAnchor.MiddleCenter;

            GUIStyle textStyle = new GUIStyle(EditorStyles.label);
            textStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.Label("   GIS Terrain Loader   ", titleStyle);
            GUILayout.Label("Version : " + EditorTerrainGenerator.version, textStyle);
            GUILayout.Label("Unity GIS Tech ", textStyle);
            GUILayout.Label("2019-" + DateTime.Now.Year, textStyle);
        }
 
    }
}