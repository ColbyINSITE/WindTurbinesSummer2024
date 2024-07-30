/*     Unity GIS Tech 2020-2021      */
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderTerrainSmoother
    {
        private static float[,] heightmap;
        private static float[,] heights;
        [Range(0.2f, 1f)]
        private static float TerrainHeightSmoothFactor = 0.9f;
        [Range(0, 5)]
        private static int TerrainSurfaceSmoothFactor = 4;
        public static void SmoothTerrainHeights(List<TerrainObject> terrains, float m_TerrainHeightSmoothFactor)
        {
            TerrainHeightSmoothFactor = m_TerrainHeightSmoothFactor;

            foreach (var terrain in terrains)
            {
                heightmap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);

                float[,] smoothData = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
                float numRows = terrain.terrainData.heightmapResolution;
                float numCols = terrain.terrainData.heightmapResolution;
                // Copy heightValues into smoothData using average to smooth
                for (int i = 0; i < numRows; i++)
                {
                    for (int j = 0; j < numCols; j++)
                    {
                        smoothData[i, j] = Average(terrain, i, j);
                    }
                }

                // Copy smoothData back to heightValues
                terrain.terrainData.SetHeights(0, 0, smoothData);

            }

        }
        private static float Average(TerrainObject terrain, int row, int col)
        {
            float avg = 0.0f;
            float total = 0.0f;

            for (int i = row - 1; i < row + 1; i++)
            {
                for (int j = col - 1; j < col + 1; j++)
                {
                    if (InBounds(terrain, i, j))
                    {
                        avg += heightmap[i, j] * (TerrainHeightSmoothFactor);
                        total += 1.0f;
                    }
                }
            }

            return avg / total;
        }
        private static bool InBounds(TerrainObject terrain, int row, int col)
        {
            float numRows = terrain.terrainData.heightmapResolution - 1;
            float numCols = terrain.terrainData.heightmapResolution - 1;

            return ((row >= 0 && row < numRows) &&
                    (col >= 0 && col < numCols));
        }

        public static void SmoothTerrainSurface(List<TerrainObject> terrains, int m_TerrainSurfaceSmoothFactor)
        {
            TerrainSurfaceSmoothFactor = m_TerrainSurfaceSmoothFactor;

            foreach (var terrain in terrains)
            {
                float numRows = terrain.terrainData.heightmapResolution;

                smooth(terrain, new Vector2(numRows, numRows));

               
            }

        }

        public enum Neighbourhood { Moore = 0, VonNeumann = 1 }
        private static Neighbourhood neighbourhood = Neighbourhood.Moore;
        private static void smooth(TerrainObject terrain, Vector2 arraySize)
        {

            float[,] heightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);

            int Tw = (int)arraySize.x;
            int Th = (int)arraySize.y;
            int xNeighbours;
            int yNeighbours;
            int xShift;
            int yShift;
            int xIndex;
            int yIndex;
            int Tx;
            int Ty;
            // Start iterations...
            for (int iter = 0; iter < TerrainSurfaceSmoothFactor; iter++)
            {
                for (Ty = 0; Ty < Th; Ty++)
                {
                    // y...
                    if (Ty == 0)
                    {
                        yNeighbours = 2;
                        yShift = 0;
                        yIndex = 0;
                    }
                    else if (Ty == Th - 1)
                    {
                        yNeighbours = 2;
                        yShift = -1;
                        yIndex = 1;
                    }
                    else
                    {
                        yNeighbours = 3;
                        yShift = -1;
                        yIndex = 1;
                    }
                    for (Tx = 0; Tx < Tw; Tx++)
                    {
                        // x...
                        if (Tx == 0)
                        {
                            xNeighbours = 2;
                            xShift = 0;
                            xIndex = 0;
                        }
                        else if (Tx == Tw - 1)
                        {
                            xNeighbours = 2;
                            xShift = -1;
                            xIndex = 1;
                        }
                        else
                        {
                            xNeighbours = 3;
                            xShift = -1;
                            xIndex = 1;
                        }
                        int Ny;
                        int Nx;
                        float hCumulative = 0.0f;
                        int nNeighbours = 0;
                        for (Ny = 0; Ny < yNeighbours; Ny++)
                        {
                            for (Nx = 0; Nx < xNeighbours; Nx++)
                            {
                                if (neighbourhood == Neighbourhood.Moore || (neighbourhood == Neighbourhood.VonNeumann && (Nx == xIndex || Ny == yIndex)))
                                {
                                    float heightAtPoint = heightMap[Tx + Nx + xShift, Ty + Ny + yShift]; // Get height at point
                                    hCumulative += heightAtPoint;
                                    nNeighbours++;
                                }
                            }
                        }
                        float hAverage = hCumulative / nNeighbours;
                        heightMap[Tx + xIndex + xShift, Ty + yIndex + yShift] = hAverage;
                    }
                }
                // Show progress...
                float percentComplete = (iter + 1) / TerrainSurfaceSmoothFactor;

            }

            terrain.terrainData.SetHeights(0, 0, heightMap);

        }

    }
}