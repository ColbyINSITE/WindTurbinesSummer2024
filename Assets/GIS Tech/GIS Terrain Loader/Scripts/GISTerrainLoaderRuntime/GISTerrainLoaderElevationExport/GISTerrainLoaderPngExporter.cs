/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderPngExporter 
    {
        private TerrainContainerObject container;
        private string path;
        private ExportAs exportAs;
 
        public GISTerrainLoaderPngExporter(string m_path,TerrainContainerObject m_container,ExportAs m_exportAs)
        {
            path = m_path;
            container = m_container;
            exportAs = m_exportAs;
        }

        public void ExportToPng()
        {
 
            int heightmapResolution = -1;

            int cx = container != null ? container.terrainCount.x : 1;
            int cy = container != null ? container.terrainCount.y : 1;

            foreach (var terrain in container.terrains)
            {
                if (heightmapResolution == -1) heightmapResolution = terrain.terrainData.heightmapResolution;
                else if (heightmapResolution != terrain.terrainData.heightmapResolution)
                {
                    Debug.LogError("Error Terrains have different heightmap resolution.");
                    return;
                }
            }

            Texture2D MainTex = new Texture2D(heightmapResolution * container.terrainCount.x, heightmapResolution * container.terrainCount.y, TextureFormat.ARGB32, false);

            for (int y = 0; y < cy; y++)
            {
                for (int x = 0; x < cx; x++)
                {
                    var tdata = container.terrains[x, y].terrainData;

                    float[,] rawHeights = tdata.GetHeights(0, 0, heightmapResolution, heightmapResolution);

                    for (int dy = 0; dy < heightmapResolution; dy++)
                    {
                        for (int dx = 0; dx < heightmapResolution; dx++)
                        {
                            var color = new Vector4(rawHeights[dy, dx], rawHeights[dy, dx], rawHeights[dy, dx], 1);
                            MainTex.SetPixel(dx + (heightmapResolution * x), dy + (heightmapResolution * y), color);
 
                        }

                    }
 
                }
            }

            MainTex.Apply();

            byte[] TextData = null;

            if (exportAs== ExportAs.Png)
            {
                TextData = MainTex.EncodeToPNG();
            }else
            if(exportAs == ExportAs.jpg)
            {
                TextData = MainTex.EncodeToJPG();
            }


            if (MainTex != null)
            {
                File.WriteAllBytes(path, TextData);
                Debug.Log("Heightmap Exported : Saved as " + exportAs + " in " + path);
            }
            else
            {
                Debug.LogError("Failed to export heightmap");
            }
        }
    }
}