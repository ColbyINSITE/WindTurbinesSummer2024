/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderRawExporter 
    {
        private RawDepth depth = RawDepth.Bit16;
        private RawByteOrder order = RawByteOrder.Windows;
        private TerrainContainerObject container;
        private string path;


        public GISTerrainLoaderRawExporter(string m_path,RawDepth m_depth, RawByteOrder m_order, TerrainContainerObject m_container)
        {
            path = m_path;
            depth = m_depth;
            order = m_order;
            container = m_container;

        }


        public void ExportToRaw()
        {
            int m_Depth = 8;

            if (depth == RawDepth.Bit16)
                m_Depth = 16;

            int heightmapResolution = -1;
 
            int cx = container != null ? container.terrainCount.x : 1;
            int cy = container != null ? container.terrainCount.y : 1;
 
            foreach (var terrain in container.terrains)
            {
                if (heightmapResolution == -1) heightmapResolution = terrain.terrainData.heightmapResolution;
                else if (heightmapResolution != terrain.terrainData.heightmapResolution)
                {
                    Debug.LogError("Error Terrains have different heightmap resolution");
                    return;
                }
            }

            FileStream stream = new FileStream(path, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(stream);

            int textureWidth = cx * heightmapResolution;
            int coof = m_Depth == 8 ? 1 : 2;

            for (int y = 0; y < cy; y++)
            {
                for (int x = 0; x < cx; x++)
                {
                    float[,] heightmap = container.terrains[x,y].terrainData.GetHeights(0, 0, heightmapResolution, heightmapResolution);

                    for (int dy = 0; dy < heightmapResolution; dy++)
                    {

                        int row = cy * heightmapResolution - (y * heightmapResolution + dy) - 1;
                        int seek = (row * textureWidth + x * heightmapResolution) * coof;

                        stream.Seek(seek, SeekOrigin.Begin);

                        for (int dx = 0; dx < heightmapResolution; dx++)
                        {
                            if (m_Depth == 8) writer.Write((byte)Mathf.RoundToInt(heightmap[dy, dx] * 255));
                            else
                            {
                                short v = (short)Mathf.RoundToInt(heightmap[dy, dx] * 65536);
                                if (order == RawByteOrder.Windows) writer.Write(v);
                                else
                                {
                                    writer.Write((byte)(v / 256));
                                    writer.Write((byte)(v % 256));
                                }
                            }
                        }
                    }
                }
            }

            stream.Close();
        }
    }
}
