/*     Unity GIS Tech 2020-2021      */

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderTreeGenerator
    {
        private static List<PrototypeTagIndex> Treeprototypes;
        private static Dictionary<string,int> Treeprototypes_Ind= new Dictionary<string, int>();


        private static HashSet<string> alreadyCreated;
        private static HashSet<string> alreadyAdded;

        private static TerrainContainerObject container;
        private static float TreeDistance;
        private static float BillBoardStartDistance;
        private static List<GISTerrainLoaderSO_Tree> treesPrefabs = new List<GISTerrainLoaderSO_Tree>();

        public static void GenerateTrees(TerrainContainerObject container, GISTerrainLoaderGeoVectorData GeoData)
        {
             alreadyAdded = new HashSet<string>();

            var treesPrefabs_str = new List<string>();
            foreach (var p in treesPrefabs)
            {
                if(p!=null)
                treesPrefabs_str.Add(p.m_treeType);
            }

            if (GeoData.GeoTrees.Count == 0)
                return;

            List<Vector3> Points = new List<Vector3>();

            alreadyCreated = new HashSet<string>();

            var TLPMercator_X = container.TLPointMercator.x;
            var TLPMercator_Y = container.TLPointMercator.y;

            var DRPMercator_X = container.DRPointMercator.x;
            var DRPMercator_Y = container.DRPointMercator.y;


            GISTerrainLoaderSO_Tree tree_SO = null;


            string treetype = "forest";

            for (int i = 0; i < GeoData.GeoTrees.Count; i++)
            {
                var Poly = GeoData.GeoTrees[i];

                treetype = Poly.Tag;

                tree_SO = GetTreePrefab(treetype);

                if (tree_SO != null)
                {
                    var points = Poly.GeoPoints;

                    var spacePoints = new Vector3[points.Count];

                    for (int p = 0; p < points.Count; p++)
                    {
                        var pll = points[p];

                        if (Poly.CoordinateReferenceSystem == null)
                        {
                            Poly.CoordinateReferenceSystem = new GTLGeographicCoordinateSystem();
                            Poly.CoordinateReferenceSystem.GEOGCSProjection = "GCS_WGS_1984";
                        }


                        var LatLon = GeoRefConversion.ConvertTOLatLon(Poly.CoordinateReferenceSystem, new DVector2(pll.x, pll.y));

                        Vector3 WSPos = new Vector3();

                        var sp_Merc = GeoRefConversion.LatLongToMercat(LatLon.x, LatLon.y);

                        double rx = (sp_Merc.x - TLPMercator_X) / (DRPMercator_X - TLPMercator_X);
                        double ry = 1 - (sp_Merc.y - TLPMercator_Y) / (DRPMercator_Y - TLPMercator_Y);

                        WSPos.x = (float)(container.transform.position.x + container.ContainerSize.x * rx);
                        WSPos.z = (float)(container.transform.position.z + container.ContainerSize.z * ry);

                        spacePoints[p] = WSPos;
                    }

                    float m_TreeDensity = 100 - tree_SO.TreeDensity;

                    Rect rect = GISTerrainLoaderExtensions.GetRectFromPoints(spacePoints.ToList());
                    int lx = Mathf.RoundToInt(rect.width / m_TreeDensity);
                    int ly = Mathf.RoundToInt(rect.height / m_TreeDensity);

                    if (lx > 0 && ly > 0)
                    {
                        GenerateTerrainsTrees(tree_SO, container, lx, ly, rect, spacePoints.ToList());
                    }
                    continue;

                }

            }
        }
        private static void CreateTree(GISTerrainLoaderSO_Tree tree,TerrainContainerObject TerrainContainer, Vector3 pos)
        {

            float TreeScaleFactor = tree.TreeScaleFactor * TerrainContainer.scale.x;
            float RandomScaleFactor = tree.TreeRandomScaleFactor * TerrainContainer.scale.x;
 

            var m_prototypeIndex = GetTreePrototype(tree.m_treeType, Treeprototypes);
 
            for (int x = 0; x < TerrainContainer.terrainCount.x; x++)
            {
                for (int y = 0; y < TerrainContainer.terrainCount.y; y++)
                {

                    TerrainObject item = TerrainContainer.terrains[x, y];
                    Terrain terrain = item.terrain;
                    terrain.treeBillboardDistance = BillBoardStartDistance;
                    terrain.treeDistance = TreeDistance;
                    TerrainData tData = terrain.terrainData;
                    Vector3 terPos = terrain.transform.position;
                    Vector3 localPos = pos - terPos;
                    float heightmapWidth = (tData.heightmapResolution - 1) * tData.heightmapScale.x;
                    float heightmapHeight = (tData.heightmapResolution - 1) * tData.heightmapScale.z;

                    if (localPos.x > 0 && localPos.z > 0 && localPos.x < heightmapWidth && localPos.z < heightmapHeight)
                    {
                        terrain.AddTreeInstance(new TreeInstance
                        {
                            color = Color.white,
                            heightScale = TreeScaleFactor + UnityEngine.Random.Range(-RandomScaleFactor, RandomScaleFactor),
                            lightmapColor = Color.white,
                            position = new Vector3(localPos.x / heightmapWidth, 0, localPos.z / heightmapHeight),
                            prototypeIndex = UnityEngine.Random.Range(m_prototypeIndex.x, m_prototypeIndex.y),
                            widthScale = TreeScaleFactor + UnityEngine.Random.Range(-RandomScaleFactor, RandomScaleFactor)
                        });
                        break;
                    }
                }
            }
        }
        private static void GenerateTerrainsTrees(GISTerrainLoaderSO_Tree tree, TerrainContainerObject m_container, int factorX, int factorY, Rect rect, List<Vector3> points)
        {
            float TreeScaleFactor = tree.TreeScaleFactor * m_container.scale.x;
            float TreeRandomScaleFactor = tree.TreeRandomScaleFactor * m_container.scale.x;
            var m_TreeDensity = 100 - tree.TreeDensity;
            float treeDensity= m_TreeDensity * m_container.scale.x;
 
            Bounds bounds = m_container.GlobalTerrainBounds;

            Vector3 Bmin = bounds.min;
            Vector3 Bmax = bounds.max;

            float TreeValue = 10 / treeDensity;

            float rectx = (rect.xMax - rect.xMin) / factorX;
            float recty = (rect.yMax - rect.yMin) / factorY;

            int counter = 0;

            Vector3[] ps = points.ToArray();

            int Max_S_x = Mathf.Max(Mathf.FloorToInt((Bmin.x - rect.xMin) / rectx + 1), 0);
            int Min_E_x = Mathf.Min(Mathf.FloorToInt((Bmax.x - rect.xMin) / rectx), factorX);

            int Max_S_y = Mathf.Max(Mathf.FloorToInt((Bmin.z - rect.yMin) / recty + 1), 0);
            int Min_E_y = Mathf.Min(Mathf.FloorToInt((Bmax.z - rect.yMin) / recty), factorY);

            for (int x = Max_S_x; x < Min_E_x; x++)
            {

                float rx = x * rectx + rect.xMin;

                for (int y = Max_S_y; y < Min_E_y; y++)
                {
                    float ry = y * recty + rect.yMin;

                    float px = rx + UnityEngine.Random.Range(-TreeValue, TreeValue);
                    float pz = ry + UnityEngine.Random.Range(-TreeValue, TreeValue);

                    if (GISTerrainLoaderExtensions.IsPointInPolygon(ps, px, pz))
                    {
                        CreateTree(tree,m_container, new Vector3(px, 0, pz));
                        counter++;
                    }
                }
            }

        }
        public static void AddTreePrefabsToTerrains(TerrainContainerObject m_container, List<GISTerrainLoaderSO_Tree> m_treesPrefabs, float m_TreeDistance, float m_BillBoardStartDistance)
        {
            TreeDistance = m_TreeDistance;
            BillBoardStartDistance = m_BillBoardStartDistance;
            treesPrefabs = m_treesPrefabs;
            container = m_container;

            int c = 0;
            List<object> objects = new List<object>();
            List<string> objects_type = new List<string>();
            Treeprototypes_Ind = new Dictionary<string, int>();

            foreach ( var prefab in m_treesPrefabs)
            {
                if(prefab!=null)
                {
                    foreach(var t in prefab.TreePrefab)
                    {
                        if (t != null)
                        {
                            objects.Add(t);
                            objects_type.Add(prefab.m_treeType);
                            c++;
                        }
                    }
                    if(!Treeprototypes_Ind.ContainsKey(prefab.m_treeType))
                    Treeprototypes_Ind.Add(prefab.m_treeType, c);
                     c = 0;
                }
            }

            TreePrototype[] prototypes = new TreePrototype[objects.Count];

            Treeprototypes = new List<PrototypeTagIndex>();

            for (int i = 0; i < prototypes.Length; i++)
            {
                prototypes[i] = new TreePrototype
                {
                    prefab = (GameObject)objects[i] as GameObject
                };

                Treeprototypes.Add(new PrototypeTagIndex(prototypes[i], objects_type[i]));

            }

            foreach (var item in container.terrains)
            {
                item.terrainData.treePrototypes = prototypes;
                item.terrainData.treeInstances = new TreeInstance[0];
            }
        }
        public static List<Vector3> GetGlobalPointsFromWay(GISTerrainLoaderOSMWay way, Dictionary<long, GISTerrainLoaderOSMNode> _nodes)
        {
            List<Vector3> points = new List<Vector3>();

            if (way.Nodes.Count == 0) return points;

            foreach (var node in way.Nodes)
            {
                if (node != null)
                    points.Add(new Vector3((float)node.Lon, 0, (float)node.Lat));
            }
            return points;
        }
        private static Vector2Int GetTreePrototype(string treetype, List<PrototypeTagIndex> Treeprototypes)
        {
            Vector2Int Index = new Vector2Int(0, 0);

            var l = Treeprototypes_Ind.ToList();
            int t_value = 0;

            foreach (var tree in l)
            {
                if (tree.Key == treetype)
                {
                    Index = new Vector2Int(t_value, (t_value + tree.Value));
                }
                t_value += tree.Value;
            }
            return Index;
        }
        private static GISTerrainLoaderSO_Tree GetTreePrefab(string treetype)
        {
            GISTerrainLoaderSO_Tree tree = null;
            foreach (var prefab in treesPrefabs)
            {
                if (prefab != null)
                {
                    if (prefab.m_treeType == treetype)
                        tree= prefab;

                }
            }
            return tree;
        }
    }

    public class PrototypeTagIndex
    {
        public TreePrototype protoType;
        public string treeType;

        public PrototypeTagIndex(TreePrototype m_protoType, string m_treeType)
        {
            protoType = m_protoType;
            treeType = m_treeType;
        }
        
    }
}