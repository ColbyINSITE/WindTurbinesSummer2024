/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections.Generic;
using UnityEngine;


namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderGrassGenerator
    {
        private static List<DetailPrototype> DetailPrototypes;
        private static List<GISTerrainLoaderSO_GrassObject> GrassPrefabs;
        private static TerrainContainerObject container;

        private static float DetailDistance;
        private static float GrassScaleFactor;

        private static IndexedDetails [,] Totaldetails;
        private static HashSet<string> alreadyCreated;
        private static HashSet<string> alreadyAdded;

        private static Dictionary<string, int> Grassprototypes_Ind = new Dictionary<string, int>();
        private static int totalGrassCount = 0;
        private static int detailResolution;

        private static TerrainObject terrainObj;

        public static void GenerateGrass(TerrainContainerObject container, GISTerrainLoaderGeoVectorData GeoData)
        {
            var GrassPrefabs_str = new List<string>();

            foreach (var p in GrassPrefabs)
            {
                if(p)
                GrassPrefabs_str.Add(p.grassType);
            }

            if (GeoData.GeoGrass.Count == 0)
                return;
 

            alreadyCreated = new HashSet<string>();

            var TLPMercator_X = container.TLPointMercator.x;
            var TLPMercator_Y = container.TLPointMercator.y;

            var DRPMercator_X = container.DRPointMercator.x;
            var DRPMercator_Y = container.DRPointMercator.y;

            GISTerrainLoaderSO_GrassObject grass_SO = null;


            string grasstype = "forest";

            for (int i = 0; i < GeoData.GeoGrass.Count; i++)
            {
                var Poly = GeoData.GeoGrass[i];

                grasstype = Poly.Tag;

                grass_SO = GetGrassPrefab(grasstype);
 
                if (grass_SO != null)
                {
                    var Geopoints = Poly.GeoPoints;

                    var spacePoints = new Vector3[Geopoints.Count];

                    DVector3 TL_point = new DVector3(0, 0, 0);
                    DVector3 DR_point = new DVector3(0, 0, 0);

                     List<DVector3> points = GetGlobalPointsFromWay(Geopoints, ref TL_point, ref DR_point);

                    var dis_x = Math.Abs(DR_point.x - TL_point.x);
                    var dis_y = Math.Abs(TL_point.y - DR_point.y);

                    double Step = 0.00001 * (100 - grass_SO.GrassDensity);
                    if (Step == 0)
                        Step = 0.00001;
                    List<DVector3> Newpoints = new List<DVector3>();

                    for (var lon = TL_point.x; lon <= DR_point.x; lon += Step)
                    {
                        for (var lat = DR_point.z; lat <= TL_point.z; lat += Step)
                        {
                            if (GISTerrainLoaderExtensions.IsPointInPolygon(points, lon, lat))
                            {
                                DVector3 p = new DVector3(lon, 0, lat);
                                Newpoints.Add(p);
                            }
                        }
                    }

                    foreach (var p in Newpoints)
                    {

                        var space = GeoRefConversion.LatLonToUWS(new DVector2(p.x, p.z), container, ref terrainObj);

                        if (terrainObj != null)
                        {
                            SetGrass(grass_SO, terrainObj.Number, space, 2f);
                        }
                    }

                }
            }

            foreach (var det in Totaldetails)
                det.SetTerraindetails();
        }

        private static void SetGrass(GISTerrainLoaderSO_GrassObject grass_SO, Vector2Int t_index, Vector3 position, float radius)
        {

            int Prefab_index = UnityEngine.Random.Range(0, grass_SO.GrassPrefab.Count);
            var grassModel = grass_SO.GrassPrefab[Prefab_index];
            int m_prototypeIndex = GetGrassPrototypeIndex(grassModel);

            var det = Totaldetails[t_index.x,t_index.y];

            var map = det.details[m_prototypeIndex];

            int TerrainDetailMapSize = det.terrain.terrainData.detailResolution;

            float PrPxSize = TerrainDetailMapSize / det.terrain.terrainData.size.x;

            Vector3 TexturePoint3D = position - det.terrain.transform.position;
            TexturePoint3D = TexturePoint3D * PrPxSize;

            float[] xymaxmin = new float[4];
            xymaxmin[0] = TexturePoint3D.z + radius;
            xymaxmin[1] = TexturePoint3D.z - radius;
            xymaxmin[2] = TexturePoint3D.x + radius;
            xymaxmin[3] = TexturePoint3D.x - radius;
 
            for (int y = 0; y < det.terrain.terrainData.detailHeight; y++)
            {
                if (xymaxmin[2] > y && xymaxmin[3] < y)
                {
                    for (int x = 0; x < det.terrain.terrainData.detailWidth; x++)
                    {
                        if (xymaxmin[0] > x && xymaxmin[1] < x)
                            map[x, y] = 1;
                    }
                }
            }

        }
        public static void AddDetailsLayersToTerrains(TerrainContainerObject m_container, List<GISTerrainLoaderSO_GrassObject> m_GrassPrefabs, float m_DetailDistance,float m_GrassScaleFactor)
        {
            GrassPrefabs = m_GrassPrefabs;
            container = m_container;
            DetailDistance = m_DetailDistance;
            GrassScaleFactor = m_GrassScaleFactor* container.scale.x;

            int c = 0;
            List<GISTerrainLoaderSO_Grass> objects = new List<GISTerrainLoaderSO_Grass>();
            List<string> objects_type = new List<string>();
            Grassprototypes_Ind = new Dictionary<string, int>();
            totalGrassCount = 0;
 
            foreach (var element in m_GrassPrefabs)
            {
                if (element != null)
                {
                    foreach (var prefab in element.GrassPrefab)
                    {
                        if (prefab != null)
                        {
                            objects.Add(prefab);
                            objects_type.Add(element.grassType);
                            c++;
                        }

                    }
                    if (!Grassprototypes_Ind.ContainsKey(element.grassType))
                        Grassprototypes_Ind.Add(element.grassType, c);
                    c = 0;

                }

            }

            DetailPrototypes = new List<DetailPrototype>(objects.Count);

            for (int i = 0; i < objects.Count; i++)
            {
                var prefab = objects[i];
                DetailPrototypes.Add((CopyDetailPrototype(m_container, prefab)));

            }

            foreach (var SO_prefab in GrassPrefabs)
            {
                if (SO_prefab != null)
                {
                    foreach (var prefab in SO_prefab.GrassPrefab)
                    {
                        totalGrassCount++;
                    }
                }
                else
                    Debug.LogError("Grass Prefab is null ");

            }

            TerrainData tdata = container.terrains[0, 0].terrainData;
            detailResolution = tdata.detailResolution;
            Totaldetails = new IndexedDetails [container.terrainCount.x, container.terrainCount.y];

            foreach (var t in container.terrains)
            {
                var IndexedDetails = new IndexedDetails(t.terrain, new Vector2Int(detailResolution, detailResolution), totalGrassCount);
                Totaldetails[t.Number.x,t.Number.y]=IndexedDetails;

            }

            foreach (var terrain in container.terrains)
            {
                terrain.terrainData.detailPrototypes = DetailPrototypes.ToArray();
                terrain.terrain.detailObjectDistance = DetailDistance;
            }



        }
        private static DetailPrototype CopyDetailPrototype(TerrainContainerObject m_container, GISTerrainLoaderSO_Grass Source_item)
        {
            var detailPrototype = new DetailPrototype();

            detailPrototype.renderMode = DetailRenderMode.GrassBillboard;
            detailPrototype.prototypeTexture = Source_item.DetailTexture;
            detailPrototype.minWidth = Source_item.MinWidth;
            detailPrototype.maxWidth = Source_item.MaxWidth * GrassScaleFactor * m_container.scale.x;
            detailPrototype.minHeight = Source_item.MinHeight;
            detailPrototype.maxHeight = Source_item.MaxHeight * GrassScaleFactor * m_container.scale.x;
            detailPrototype.noiseSpread = Source_item.Noise;
            detailPrototype.healthyColor = Source_item.HealthyColor;
            detailPrototype.dryColor = Source_item.DryColor;


            if (Source_item.BillBoard)
                detailPrototype.renderMode = DetailRenderMode.GrassBillboard;
            else detailPrototype.renderMode = DetailRenderMode.Grass;

            return detailPrototype;
        }
        private static int GetGrassPrototypeIndex(GISTerrainLoaderSO_GrassObject SO_Grass)
        {
            int Index = 0;

            for(int i=0;i< SO_Grass.GrassPrefab.Count;i++)
            {
                var prototype = SO_Grass.GrassPrefab[i];

                for (int j = 0; j< DetailPrototypes.Count;j++)
                {
                    var Details = DetailPrototypes[j];

                    if (prototype.DetailTexture.name == Details.prototypeTexture.name)
                    {
                        Index = DetailPrototypes.IndexOf(Details);
                        continue;
                    }
                        
                    
                }
            }
 
            return Index;
        }
        private static int GetGrassPrototypeIndex(GISTerrainLoaderSO_Grass SO_Grass)
        {
            int Index = 0;

            for (int j = 0; j < DetailPrototypes.Count; j++)
            {
                var Details = DetailPrototypes[j];

                if (SO_Grass.DetailTexture.name == Details.prototypeTexture.name)
                {
                    Index = DetailPrototypes.IndexOf(Details);
                    continue;
                }


            }
 
            return Index;
        }
        private static GISTerrainLoaderSO_GrassObject GetGrassPrefab(string grassType)
        {
            GISTerrainLoaderSO_GrassObject grass = null;
            foreach (var prefab in GrassPrefabs)
            {
                if (prefab != null)
                {
                    if (prefab.grassType == grassType)
                        grass = prefab;

                }
            }
            return grass;
        }
        public static List<DVector3> GetGlobalPointsFromWay(List<DVector2> GeoPoints, ref DVector3 TL_Point, ref DVector3 DR_Point)
        {
             TL_Point = new DVector3(180, 0, -90);
             DR_Point = new DVector3(-180, 0, 90);

            List<DVector3> points = new List<DVector3>();

            if (GeoPoints.Count == 0) return points;

            foreach (var p in GeoPoints)
            {
                if (p != null)
                {
                    var ps = new DVector3(p.x,0, p.y);

                    if (ps.x < TL_Point.x)
                        TL_Point.x = ps.x;
                    if (ps.z > TL_Point.z)
                        TL_Point.z=ps.z ;

                    if (ps.x > DR_Point.x)
                        DR_Point.x = ps.x;
                    if (ps.z < DR_Point.z)
                        DR_Point.z = ps.z;

                    points.Add(ps);
                }
                    
            }

            return points;
        }
        public static List<DVector3> GetGlobalPointsFromWay(GISTerrainLoaderShapeFileData shape, ref DVector3 TL_Point, ref DVector3 DR_Point)
        {
            var Shapepoints = GISTerrainLoaderShapeFactory.GetTypePoint(shape.ShapeType, shape.ShapeRecord.Contents);

            TL_Point = new DVector3(180, 0, -90);
            DR_Point = new DVector3(-180, 0, 90);

            List<DVector3> points = new List<DVector3>();

            if (Shapepoints.Length == 0) return points;

 
            foreach (var node in Shapepoints)
            {
                if (node != null)
                {
                    var LatLon = GeoRefConversion.ConvertTOLatLon(shape.CoordinateReferenceSystem, new DVector2(node.X, node.Y));

                    var p = new DVector3(node.X, 0, node.Y);

                    if (p.x < TL_Point.x)
                        TL_Point.x = p.x;
                    if (p.z > TL_Point.z)
                        TL_Point.z = p.z;

                    if (p.x > DR_Point.x)
                        DR_Point.x = p.x;
                    if (p.z < DR_Point.z)
                        DR_Point.z = p.z;

                    points.Add(p);
                }

            }

            return points;
        }

    }
    public class IndexedDetails
    {
        public List<int[,]> details = new List<int[,]>();
        public Terrain terrain;
        public IndexedDetails(Terrain m_terrain, Vector2Int dim,int totalGrassCount)
        {
            terrain = m_terrain;

            for (int i = 0; i < totalGrassCount; i++)
            {
                details.Add(new int[dim.x, dim.y]);
            }
        }

        public void SetTerraindetails()
        {
            for (var x = 0; x < details.Count; x++)
            {
                terrain.terrainData.SetDetailLayer(0, 0, x, details[x]);
            }
           
        }
    }

}