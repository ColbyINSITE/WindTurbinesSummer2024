/*     Unity GIS Tech 2020-2021      */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderSplatMapping
    {
        private static int MergeRaduis;
 

        public static void SetTerrainSpaltMap(GISTerrainLoaderTerrainLayer baselayer,List<GISTerrainLoaderTerrainLayer> terrainLayers,float slope,TerrainObject terrain,int m_MergeRaduis, int m_MergingFactor)
        {
            MergeRaduis = m_MergeRaduis;

            AddLayerToTerrain(baselayer, terrain);

            AddLayersToTerrain(terrainLayers, terrain);
 

            int alphamapRwidth = terrain.terrainData.alphamapResolution;

            float[,,] splatMapData = terrain.terrainData.GetAlphamaps(0, 0, alphamapRwidth, alphamapRwidth);

            float MaxHeight = 0.001f; 

            for (int y = 0; y < alphamapRwidth; y++)
            {
                for (int x = 0; x < alphamapRwidth; x++)
                {
                    if (terrain.terrainData.GetInterpolatedHeight((float)x / alphamapRwidth, (float)y / alphamapRwidth) > MaxHeight)
                    {
                        MaxHeight = terrain.terrainData.GetInterpolatedHeight((float)x / alphamapRwidth, (float)y / alphamapRwidth);
                    }
                }
            }

            for (int y = 0; y < alphamapRwidth; y++)
            {
                for (int x = 0; x < alphamapRwidth; x++)
                {

                    Vector3 InterpolatedN = terrain.terrainData.GetInterpolatedNormal((float)x / alphamapRwidth, (float)y / alphamapRwidth);
 
                    if (InterpolatedN.y < slope)
                    {
                        SetSplatValue(splatMapData, y, x, 1);
                    }
                    else
                    {
                        float HeightInterpolated = terrain.terrainData.GetInterpolatedHeight((float)x / alphamapRwidth, (float)y / alphamapRwidth);
                        float InterpolatedFactor = HeightInterpolated / MaxHeight;

                        for (int i = 0; i < terrainLayers.Count; i++)
                        {
                            if (InterpolatedFactor >= terrainLayers[i].X_Height && InterpolatedFactor <= terrainLayers[i].Y_Height)
                            {
                                SetSplatValue(splatMapData, y, x, i + 1);
                            }
                        }
                    }
                }
            }

            terrain.terrainData.SetAlphamaps(0, 0, splatMapData);

            for(int i=0;i<= m_MergingFactor;i++)
            {
                MergeTerrainLayers(terrain);
            }
           
        }
        private static void AddLayersToTerrain(List<GISTerrainLoaderTerrainLayer> terrainLayers, TerrainObject terrain)
        {
            foreach (var layer in terrainLayers)
            {

                var layerName = terrain.name + "_Splat_" + terrainLayers.IndexOf(layer) + ".terrainlayer";
 
                if (layer.Diffuse != null)
                {

#if UNITY_2018_1_OR_NEWER

                    TerrainLayer NewterrainLayer = new TerrainLayer();
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        string path = Path.Combine(terrain.container.GeneratedTerrainfolder, layerName);
                        AssetDatabase.CreateAsset(NewterrainLayer, path);
                    }
#endif
                

                TerrainLayer[] ExistingTerrainLayers = terrain.terrainData.terrainLayers;

                    List<TerrainLayer> NewLayers = new List<TerrainLayer>();

                    foreach(var l in ExistingTerrainLayers)
                    {
                        NewLayers.Add(l);
                    }




                    NewterrainLayer.diffuseTexture = layer.Diffuse;
                    if (layer.NormalMap != null)
                        NewterrainLayer.normalMapTexture = layer.NormalMap;

                    NewterrainLayer.tileSize = new Vector2(layer.TextureSize.x, layer.TextureSize.y);
                    NewterrainLayer.tileOffset = Vector2.zero;

                    NewLayers.Add(NewterrainLayer);
                    terrain.terrainData.terrainLayers = NewLayers.ToArray();
 
#else

                        SplatPrototype sp = new SplatPrototype
                    {
                        texture = layer.Diffuse,
                        normalMap = layer.NormalMap,
                        tileSize = new Vector2(layer.Texture_Size.x, layer.Texture_Size.y),
                        tileOffset = Vector2.zero
                    };
                terrain.terrainData.splatPrototypes = new[] { sp };

#endif

                }
                else
                    Debug.LogError("Diffuse map is " + terrainLayers.IndexOf(layer)  + " null ... ");
 
            }
        }
        private static void AddLayerToTerrain(GISTerrainLoaderTerrainLayer terrainLayer, TerrainObject terrain)
        {
            var layerName = terrain.name + "_Splat_" + terrain.terrainData.terrainLayers.Length + ".terrainlayer";
 
            if (terrainLayer.Diffuse != null)
            {

#if UNITY_2018_1_OR_NEWER

                TerrainLayer NewterrainLayer = new TerrainLayer();
#if UNITYEDITOR
                if (!Application.isPlaying)
                {
                    string path = Path.Combine(terrain.container.GeneratedTerrainfolder, layerName);
                    AssetDatabase.CreateAsset(NewterrainLayer, path);
                }
#endif


                TerrainLayer[] ExistingTerrainLayers = terrain.terrainData.terrainLayers;

                List<TerrainLayer> NewLayers = new List<TerrainLayer>();

                foreach (var l in ExistingTerrainLayers)
                {
                    NewLayers.Add(l);
                }
 
                NewterrainLayer.diffuseTexture = terrainLayer.Diffuse;
                if (terrainLayer.NormalMap != null)
                    NewterrainLayer.normalMapTexture = terrainLayer.NormalMap;

                NewterrainLayer.tileSize = new Vector2(terrainLayer.TextureSize.x, terrainLayer.TextureSize.y);
                NewterrainLayer.tileOffset = Vector2.zero;

                NewLayers.Add(NewterrainLayer);
                terrain.terrainData.terrainLayers = NewLayers.ToArray();
 
#else

                    SplatPrototype sp = new SplatPrototype
                    {
                        texture = layer.Diffuse,
                        normalMap = layer.NormalMap,
                        tileSize = new Vector2(layer.Texture_Size.x, layer.Texture_Size.y),
                        tileOffset = Vector2.zero
                    };
                terrain.terrainData.splatPrototypes = new[] { sp };

#endif

            }
            else
                Debug.LogError("Diffuse map " + (terrain.terrainData.terrainLayers.Length +1) + " is null ... ");
 
        } 
        private static void SetSplatValue(float[,,] splats, int y, int x, int splat)
        {
            for (int i = 0; i < splats.GetLength(2); i++)
            {
                if (i == splat)
                {
                    splats[y, x, i] = 1;
                }
                else
                {
                    splats[y, x, i] = 0;
                }
            }
        }
        private static void MergeTerrainLayers(TerrainObject terrain)
        {
            float[,,] splatMapData = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);

            MergeSplatMap(splatMapData);

            terrain.terrainData.SetAlphamaps(0, 0, splatMapData);
        }
        private static void MergeSplatMap(float[,,] splats)
        {

            for (int y = 0; y < splats.GetLength(0); y++)
            {
                for (int x = 0; x < splats.GetLength(1); x++)
                {
                    float[] Num = new float[splats.GetLength(2)];
                    float[] Num2 = new float[splats.GetLength(2)];
                    float[] Num4 = new float[splats.GetLength(2)];
                    float[] Num6 = new float[splats.GetLength(2)];
                    float[] Num8 = new float[splats.GetLength(2)];

                    for (int i = 0; i < Num.Length; i++)
                    {
                        Num[i] = splats[y, x, i];
                    }

                    for (int i = 0; i < Num2.Length; i++)
                    {
                        Num2[i] = splats[y, Mathf.Clamp(x + MergeRaduis, 0, splats.GetLength(1) - 1), i];
                    }

                    for (int i = 0; i < Num4.Length; i++)
                    {
                        Num4[i] = splats[y, Mathf.Clamp(x - MergeRaduis, 0, splats.GetLength(1) - 1), i];
                    }

                    for (int i = 0; i < Num6.Length; i++)
                    {
                        Num6[i] = splats[Mathf.Clamp(y - MergeRaduis, 0, splats.GetLength(0) - 1), x, i];
                    }

                    for (int i = 0; i < Num8.Length; i++)
                    {
                        Num8[i] = splats[Mathf.Clamp(y + MergeRaduis, 0, splats.GetLength(0) - 1), x, i];
                    }

                    for (int i = 0; i < Num.Length; i++)
                    {
                        Num[i] = (Num[i] + Num2[i] + Num4[i] + Num6[i] + Num8[i]) / 5;
                    }

                    for (int i = 0; i < Num.Length; i++)
                    {
                        splats[y, x, i] = Num[i];
                    }

                }
            }


        }

        public static void DistributingHeights(List<GISTerrainLoaderTerrainLayer> TerrainLayers)
        {
            float step = 1f / TerrainLayers.Count;

            for (int i = 0; i < TerrainLayers.Count; i++)
            {
                TerrainLayers[i].X_Height = i * step;
                TerrainLayers[i].Y_Height = (i + 1) * step;
            }
        }
    }
}
