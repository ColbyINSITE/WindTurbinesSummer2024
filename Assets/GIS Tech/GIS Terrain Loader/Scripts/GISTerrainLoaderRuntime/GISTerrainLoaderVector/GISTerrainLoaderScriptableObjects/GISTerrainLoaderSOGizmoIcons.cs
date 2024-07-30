/*     Unity GIS Tech 2020-2021      */
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderSOGizmoIcons
    {
        [DidReloadScripts]
        static GISTerrainLoaderSOGizmoIcons()
        {
            EditorApplication.projectWindowItemOnGUI = ItemOnGUI;
        }

        static void ItemOnGUI(string guid, Rect rect)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            GISTerrainLoaderSO_GeoPoint obj_GeoPoint = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GISTerrainLoaderSO_GeoPoint)) as GISTerrainLoaderSO_GeoPoint;
            GISTerrainLoaderSO_Building obj_building = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GISTerrainLoaderSO_Building)) as GISTerrainLoaderSO_Building;
            GISTerrainLoaderSO_Road  obj_road = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GISTerrainLoaderSO_Road)) as GISTerrainLoaderSO_Road;
            GISTerrainLoaderSO_Grass obj_grass = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GISTerrainLoaderSO_Grass)) as GISTerrainLoaderSO_Grass;
            GISTerrainLoaderSO_GrassObject obj_GrassObject = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GISTerrainLoaderSO_GrassObject)) as GISTerrainLoaderSO_GrassObject;
            GISTerrainLoaderSO_Tree obj_tree = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GISTerrainLoaderSO_Tree)) as GISTerrainLoaderSO_Tree;

            rect.width = rect.height;

            if (obj_GeoPoint != null)
                GUI.DrawTexture(rect, (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/GIS Tech/GIS Terrain Loader/Gizmos/Icon_GeoPoint.png", typeof(Texture2D)));
            if (obj_building != null)
                GUI.DrawTexture(rect, (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/GIS Tech/GIS Terrain Loader/Gizmos/Icon_Building.png", typeof(Texture2D)));
            if (obj_road != null)
                GUI.DrawTexture(rect, (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/GIS Tech/GIS Terrain Loader/Gizmos/Icon_Road.png", typeof(Texture2D)));
            if (obj_grass != null)
                GUI.DrawTexture(rect, (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/GIS Tech/GIS Terrain Loader/Gizmos/Icon_GrassModel.png", typeof(Texture2D)));
            if (obj_GrassObject != null)
                GUI.DrawTexture(rect, (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/GIS Tech/GIS Terrain Loader/Gizmos/Icon_Grass.png", typeof(Texture2D)));
            if (obj_tree != null)
                GUI.DrawTexture(rect, (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/GIS Tech/GIS Terrain Loader/Gizmos/Icon_Tree.png", typeof(Texture2D)));

        }
    }
}
#endif