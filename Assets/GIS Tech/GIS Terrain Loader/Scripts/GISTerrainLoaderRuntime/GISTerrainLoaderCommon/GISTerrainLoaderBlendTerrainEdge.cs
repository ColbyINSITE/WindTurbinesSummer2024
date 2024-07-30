/*     Unity GIS Tech 2020-2021      */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderBlendTerrainEdge
    {
        public static event TerrainProgression OnProgress;

        private static float Smoothlevel = 20;
        private static int StitcheLength = 20;
        private static float StitcheLevel = 7.0f;
        private static bool errorLength = false;
        private static int m_stitchelenght;
        private static Vector2 firstPosition;
        private static TerrainObject[] _terrains;
        private static Dictionary<int[], TerrainObject> _terrainDict = null;
        enum TerrainSide
        {
            Left,
            Right,
            Top,
            Bottom
        }
        public enum StitchMethod
        {
            AveragePower
        }
        // Update is called once per frame
        void Update()
        {
            if (errorLength)
            {
                Debug.LogError("terrain is smaller than stitch range!");
                errorLength = false;
            }
        }
        /// <summary>
        /// The selected method.
        /// </summary>
        private static StitchMethod m_stitcheMethode;
        private static StitchMethod stitcheMethod
        {
            get { return m_stitcheMethode; }
            set
            {
                if (m_stitcheMethode != value)
                {
                    m_stitcheMethode = value;

                    OnStitcheMethodeChanged(stitcheMethod);
                }
            }
        }
        private static void OnStitcheMethodeChanged(StitchMethod stitcheMethod)
        {
            switch (stitcheMethod)
            {
                case StitchMethod.AveragePower:
                    break;
            }
        }


        public static void StitchTerrain(List<TerrainObject> newterrains, float m_Smoothlevel, int m_StitcheLength)
        {

            m_stitchelenght = m_StitcheLength;
            Smoothlevel = m_Smoothlevel;

            m_stitchelenght = StitcheLength;

            errorLength = false;
            if (_terrainDict == null)
                _terrainDict = new Dictionary<int[], TerrainObject>(new IntArrayComparer());
            else
            {
                _terrainDict.Clear();
            }

            _terrains = newterrains.ToArray();

            foreach (var item in newterrains)
            {

                if (item.terrainData.heightmapResolution < StitcheLength)
                {
                    errorLength = true;
                    return;
                }
            }

            if (_terrains.Length > 0)
            {

                firstPosition = new Vector2(_terrains[0].transform.position.x, _terrains[0].transform.position.z);

                int sizeX = (int)_terrains[0].terrainData.size.x;
                int sizeZ = (int)_terrains[0].terrainData.size.z;

                foreach (var m_terrain in _terrains)
                {

                    int[] posTer = new int[] {
                        (int)(Mathf.RoundToInt ((m_terrain.terrain.transform.position.x - firstPosition.x) / sizeX)),
                        (int)(Mathf.RoundToInt ((m_terrain.transform.position.z - firstPosition.y) / sizeZ))
                    };
                    _terrainDict.Add(posTer, m_terrain);
                }
                //Checks neighbours and stitches them

                for (int i = 0; i < _terrainDict.Count; i++)
                {
                    var item = _terrainDict.ElementAt(i);
                    int[] posTer = item.Key;

                    TerrainObject top = null;
                    TerrainObject left = null;
                    TerrainObject right = null;
                    TerrainObject bottom = null;

                    Terrain t_top = null;
                    Terrain t_left = null;
                    Terrain t_right = null;
                    Terrain t_bottom = null;

                    _terrainDict.TryGetValue(new int[] { posTer[0], posTer[1] + 1 }, out top);
                    _terrainDict.TryGetValue(new int[] { posTer[0] - 1, posTer[1] }, out left);
                    _terrainDict.TryGetValue(new int[] { posTer[0] + 1, posTer[1] }, out right);
                    _terrainDict.TryGetValue(new int[] { posTer[0], posTer[1] - 1 }, out bottom);


                    if (top) t_top = top.terrain;
                    if (left) t_left = left.terrain;
                    if (right) t_right = right.terrain;
                    if (bottom) t_bottom = bottom.terrain;


                    item.Value.terrain.SetNeighbors(t_left, t_top, t_right, t_bottom);

                    item.Value.terrain.Flush();

                    if (stitcheMethod == StitchMethod.AveragePower || StitcheLength == 0)
                    {

                        if (right != null)
                        {

                            StitchTerrains(item.Value.terrain, right.terrain, TerrainSide.Right);

                        }

                        if (top != null)
                        {
                            StitchTerrains(item.Value.terrain, top.terrain, TerrainSide.Top);

                        }


                    }

                    float prog = ((i) * 100 / (_terrainDict.Count));
 
                    if (OnProgress != null)
                    {
                        OnProgress("Reparing Terrains", prog);
                    }
                    
                }
                //Repairs corners
                for (int i = 0; i < _terrainDict.Count; i++)
                {
                    var item = _terrainDict.ElementAt(i);
                    int[] posTer = item.Key;
                    TerrainObject top = null;
                    TerrainObject left = null;
                    TerrainObject right = null;
                    TerrainObject bottom = null;
                    _terrainDict.TryGetValue(new int[] {
                        posTer [0],
                        posTer [1] + 1
                    }, out top);
                    _terrainDict.TryGetValue(new int[] {
                        posTer [0] - 1,
                        posTer [1]
                    }, out left);
                    _terrainDict.TryGetValue(new int[] {
                        posTer [0] + 1,
                        posTer [1]
                    }, out right);
                    _terrainDict.TryGetValue(new int[] {
                        posTer [0],
                        posTer [1] - 1
                    }, out bottom);


                    StitcheLength = 0;

                    if (right != null)
                    {
                        StitchTerrains(item.Value.terrain, right.terrain, TerrainSide.Right);
                    }

                    if (top != null)
                    {
                        StitchTerrains(item.Value.terrain, top.terrain, TerrainSide.Top);
                    }

                    StitcheLength = m_stitchelenght;

                    if (right != null && bottom != null)
                    {
                        TerrainObject rightBottom = null;
                        _terrainDict.TryGetValue(new int[] {
                            posTer [0] + 1,
                            posTer [1] - 1
                        }, out rightBottom);
                        if (rightBottom != null)
                            StitchTerrainsRepair(item.Value.terrain, right.terrain, bottom.terrain, rightBottom.terrain);
                    }

                }
            }
        }
        private static void StitchTerrains(Terrain terrain, Terrain second, TerrainSide side, bool smooth = true)
        {
            TerrainData terrainData = terrain.terrainData;
            TerrainData secondData = second.terrainData;



            float[,] heights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
            float[,] secondHeights = secondData.GetHeights(0, 0, secondData.heightmapResolution, secondData.heightmapResolution);



            if (side == TerrainSide.Right)
            {
                int y = heights.GetLength(0) - 1;
                int x = 0;

                int y2 = 0;

                for (x = 0; x < heights.GetLength(1); x++)
                {

                    heights[x, y] = average(heights[x, y], secondHeights[x, y2]);

                    if (smooth)
                        heights[x, y] += Mathf.Abs(heights[x, y - 1] - secondHeights[x, y2 + 1]) / Smoothlevel;

                    secondHeights[x, y2] = heights[x, y];

                    for (int i = 1; i < StitcheLength; i++)
                    {

                        heights[x, y - i] = (average(heights[x, y - i], heights[x, y - i + 1]) + Mathf.Abs(heights[x, y - i] - heights[x, y - i + 1]) / Smoothlevel) * (StitcheLength - i) / StitcheLength + heights[x, y - i] * i / StitcheLength;
                        secondHeights[x, y2 + i] = (average(secondHeights[x, y2 + i], secondHeights[x, y2 + i - 1]) + Mathf.Abs(secondHeights[x, y2 + i] - secondHeights[x, y2 + i - 1]) / Smoothlevel) * (StitcheLength - i) / StitcheLength + secondHeights[x, y2 + i] * i / StitcheLength;

                    }

                }
            }
            else
            {
                if (side == TerrainSide.Top)
                {

                    int y = 0;
                    int x = heights.GetLength(0) - 1;

                    int x2 = 0;

                    for (y = 0; y < heights.GetLength(1); y++)
                    {

                        heights[x, y] = average(heights[x, y], secondHeights[x2, y]);

                        if (smooth)
                            heights[x, y] += Mathf.Abs(heights[x - 1, y] - secondHeights[x2 + 1, y]) / Smoothlevel;


                        secondHeights[x2, y] = heights[x, y];

                        for (int i = 1; i < StitcheLength; i++)
                        {

                            heights[x - i, y] = (average(heights[x - i, y], heights[x - i + 1, y]) + Mathf.Abs(heights[x - i, y] - heights[x - i + 1, y]) / Smoothlevel) * (StitcheLength - i) / StitcheLength + heights[x - i, y] * i / StitcheLength;
                            secondHeights[x2 + i, y] = (average(secondHeights[x2 + i, y], secondHeights[x2 + i - 1, y]) + Mathf.Abs(secondHeights[x2 + i, y] - secondHeights[x2 + i - 1, y]) / Smoothlevel) * (StitcheLength - i) / StitcheLength + secondHeights[x2 + i, y] * i / StitcheLength;

                        }

                    }
                }
            }


            terrainData.SetHeights(0, 0, heights);
            terrain.terrainData = terrainData;

            secondData.SetHeights(0, 0, secondHeights);
            second.terrainData = secondData;

            terrain.Flush();
            second.Flush();

        }
        /// <summary>
        /// Average the specified first and second value.
        /// </summary>
        /// <param name="first">First.</param>
        /// <param name="second">Second.</param>
        private static float average(float first, float second)
        {

            return Mathf.Pow((Mathf.Pow(first, StitcheLevel) + Mathf.Pow(second, StitcheLevel)) / 2.0f, 1 / StitcheLevel);
        }
        /// <summary>
        /// Stitchs the terrains corners.
        /// </summary>
        /// <param name="terrain11">Terrain11.</param>
        /// <param name="terrain21">Terrain21.</param>
        /// <param name="terrain12">Terrain12.</param>
        /// <param name="terrain22">Terrain22.</param>
        private static void StitchTerrainsRepair(Terrain terrain11, Terrain terrain21, Terrain terrain12, Terrain terrain22)
        {
            int size = terrain11.terrainData.heightmapResolution - 1;
            int size0 = 0;
            List<float> heights = new List<float>();


            heights.Add(terrain11.terrainData.GetHeights(size, size0, 1, 1)[0, 0]);
            heights.Add(terrain21.terrainData.GetHeights(size0, size0, 1, 1)[0, 0]);
            heights.Add(terrain12.terrainData.GetHeights(size, size, 1, 1)[0, 0]);
            heights.Add(terrain22.terrainData.GetHeights(size0, size, 1, 1)[0, 0]);


            float[,] height = new float[1, 1];
            height[0, 0] = heights.Max();

            terrain11.terrainData.SetHeights(size, size0, height);
            terrain21.terrainData.SetHeights(size0, size0, height);
            terrain12.terrainData.SetHeights(size, size, height);
            terrain22.terrainData.SetHeights(size0, size, height);

            terrain11.Flush();
            terrain12.Flush();
            terrain21.Flush();
            terrain22.Flush();


        }

    }
    /// <summary>
    /// Int array comparer.
    /// </summary>
    public class IntArrayComparer : IEqualityComparer<int[]>
    {
        /// <summary>
        /// Equals the specified x and y.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public bool Equals(int[] x, int[] y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        /// <param name="obj">Object.</param>
        public int GetHashCode(int[] obj)
        {
            int result = 17;
            for (int i = 0; i < obj.Length; i++)
            {
                unchecked
                {
                    result = result * 23 + obj[i];
                }
            }
            return result;
        }
    }

}