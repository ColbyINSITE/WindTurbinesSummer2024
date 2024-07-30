/*     Unity GIS Tech 2020-2021      */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
#if UNITY_EDITOR

    public class GISTerrainLoaderSO
    {
        [MenuItem("Tools/GIS Tech/GIS Terrain Loader/Add Runtime GTL to Scene", false, 2)]
        public static void AddRuntimeGTLToScene()
        {
            if(!GameObject.FindObjectOfType<RuntimeTerrainGenerator>())
            {
                var GISTech = new GameObject("GIS Tech");
                var RuntimeGTL = new GameObject("Runtime GIS Terrain Loader");
                RuntimeGTL.transform.parent = GISTech.transform;
                RuntimeGTL.gameObject.AddComponent<RuntimeTerrainGenerator>();
                RuntimeGTL.gameObject.AddComponent<GISTerrainLoaderRuntimePrefs>();
            }else
            {
                Debug.LogError("Runtime GIS Terrain Loader already exists in your scene");
            }

        }

        [MenuItem("Tools/GIS Tech/GIS Terrain Loader/Create Vector Prefab/GeoPoint")]
        public static void CreateGeoPointSO()
        {
            GISTerrainLoaderSO_GeoPoint asset = ScriptableObject.CreateInstance<GISTerrainLoaderSO_GeoPoint>();

            AssetDatabase.CreateAsset(asset, "Assets/GIS Tech/GIS Terrain Loader/Resources/Prefabs/Environment/GeoPoints/NewPoint.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = asset;
        }
        [MenuItem("Tools/GIS Tech/GIS Terrain Loader/Create Vector Prefab/Road")]
        public static void CreateRoadSO()
        {
            GISTerrainLoaderSO_Road asset = ScriptableObject.CreateInstance<GISTerrainLoaderSO_Road>();

            AssetDatabase.CreateAsset(asset, "Assets/GIS Tech/GIS Terrain Loader/Resources/Prefabs/Environment/Roads/NewRoad.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
 

            Selection.activeObject = asset;
        }
        [MenuItem("Tools/GIS Tech/GIS Terrain Loader/Create Vector Prefab/Tree")]
        public static void CreateTreeSO()
        {
            GISTerrainLoaderSO_Tree asset = ScriptableObject.CreateInstance<GISTerrainLoaderSO_Tree>();

            AssetDatabase.CreateAsset(asset, "Assets/GIS Tech/GIS Terrain Loader/Resources/Prefabs/Environment/Trees/NewTree.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = asset;
        }
        [MenuItem("Tools/GIS Tech/GIS Terrain Loader/Create Vector Prefab/Building")]
        public static void CreateBuildingSO()
        {
            GISTerrainLoaderSO_Building asset = ScriptableObject.CreateInstance<GISTerrainLoaderSO_Building>();

            AssetDatabase.CreateAsset(asset, "Assets/GIS Tech/GIS Terrain Loader/Resources/Prefabs/Environment/Buildings/NewBuilding.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = asset;
        }
        [MenuItem("Tools/GIS Tech/GIS Terrain Loader/Create Vector Prefab//Grass/Grass Model")]
        public static void CreateGrassSO_Model()
        {
            GISTerrainLoaderSO_Grass asset = ScriptableObject.CreateInstance<GISTerrainLoaderSO_Grass>();

            AssetDatabase.CreateAsset(asset, "Assets/GIS Tech/GIS Terrain Loader/Resources/Prefabs/Environment/Grass/Models/NewGrassModel.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = asset;
        }
        [MenuItem("Tools/GIS Tech/GIS Terrain Loader/Create Vector Prefab//Grass/Grass Prefab")]
        public static void CreateGrassSO_Prefab()
        {
            GISTerrainLoaderSO_GrassObject asset = ScriptableObject.CreateInstance<GISTerrainLoaderSO_GrassObject>();

            AssetDatabase.CreateAsset(asset, "Assets/GIS Tech/GIS Terrain Loader/Resources/Prefabs/Environment/Grass/NewGrass.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = asset;
        }
        [MenuItem("Tools/GIS Tech/GIS Terrain Loader/Create Vector Prefab/Attribute Ref")]
        public static void CreateGrassAttribute_Prefab()
        {
            GISTerrainLoaderAttributes_SO asset = ScriptableObject.CreateInstance<GISTerrainLoaderAttributes_SO>();

            AssetDatabase.CreateAsset(asset, "Assets/GIS Tech/GIS Terrain Loader/Resources/VectorAttributes/NewAttribute.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = asset;
        }
    }

#endif

}