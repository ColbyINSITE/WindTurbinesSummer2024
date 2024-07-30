/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderOSMFileLoader : GISTerrainLoaderGeoDataHolder
    {
        private static object osmDataLock;
        public GISTerrainLoaderOSMData osmData;

        public GISTerrainLoaderOSMFiltredData FiltredPointsData;
        public GISTerrainLoaderOSMFiltredData FiltredGrassData;
        public GISTerrainLoaderOSMFiltredData FiltredRoadsData;
        public GISTerrainLoaderOSMFiltredData FiltredBuildingsData;
        public GISTerrainLoaderOSMFiltredData FiltredTreesData;


        private static GISTerrainLoaderAttributes_SO Attribute_Roads;

        public GISTerrainLoaderOSMFileLoader(string FilePath, TerrainContainerObject container)
        {

            var parser = new GISTerrainLoaderOSMParser();
            osmData = parser.ParseFromFile(FilePath);
            osmData.FillNodes(container);
            FilterData();
        }

        private void FilterData()
        {
            FiltredPointsData = new GISTerrainLoaderOSMFiltredData();

            FiltredRoadsData = new GISTerrainLoaderOSMFiltredData();
            FiltredTreesData = new GISTerrainLoaderOSMFiltredData();

            FiltredBuildingsData = new GISTerrainLoaderOSMFiltredData();
            FiltredGrassData = new GISTerrainLoaderOSMFiltredData();

            var Attribute_Points= Resources.Load("VectorAttributes/Attribute_Points") as GISTerrainLoaderAttributes_SO;
            if (!Attribute_Points)
            {
                Attribute_Points = new GISTerrainLoaderAttributes_SO();
                Attribute_Points.Attributes = new List<string>();
                Debug.LogError("Roads Attribute File Not found .. Restore 'Attribute_Roads' ScriptableObject ");
            }

            var Attribute_Roads = Resources.Load("VectorAttributes/Attribute_Roads") as GISTerrainLoaderAttributes_SO;
            if (!Attribute_Roads)
            {
                Attribute_Roads = new GISTerrainLoaderAttributes_SO();
                Attribute_Roads.Attributes = new List<string>();
                Debug.LogError("Roads Attribute File Not found .. Restore 'Attribute_Roads' ScriptableObject ");
            }

            var Attribute_Trees = Resources.Load("VectorAttributes/Attribute_Trees") as GISTerrainLoaderAttributes_SO;
            if (!Attribute_Trees)
            {
                Attribute_Trees = new GISTerrainLoaderAttributes_SO();
                Attribute_Trees.Attributes = new List<string>();
                Debug.LogError("Trees Attribute File Not found .. Restore 'Attribute_Trees' ScriptableObject ");
            }

            var Attribute_Grass = Resources.Load("VectorAttributes/Attribute_Grass") as GISTerrainLoaderAttributes_SO;
            if (!Attribute_Grass)
            {
                Attribute_Grass = new GISTerrainLoaderAttributes_SO();
                Attribute_Grass.Attributes = new List<string>();
                Debug.LogError("Grass Attribute File Not found .. Restore 'Attribute_Grass' ScriptableObject ");
            }
            var Attribute_Buildings = Resources.Load("VectorAttributes/Attribute_Buildings") as GISTerrainLoaderAttributes_SO;
            if (!Attribute_Buildings)
            {
                Attribute_Buildings = new GISTerrainLoaderAttributes_SO();
                Attribute_Buildings.Attributes = new List<string>();
                Debug.LogError("Buildings Attribute File Not found .. Restore 'Attribute_Buildings' ScriptableObject ");
            }


            foreach (var wayDic in osmData.Ways)
            {
                long wayID = long.Parse(wayDic.Id);

                var WayDicAttributes = wayDic.Tags;

                foreach (var Value in WayDicAttributes)
                {

                    //Roads
                    if (Attribute_Roads.Attributes.Contains(Value.Attribute))
                    {
                        wayDic.MainTag.Attribute = Value.Attribute;
                        wayDic.MainTag.Value = Value.Value;

                        if (!FiltredRoadsData.Ways.ContainsKey(wayID))
                        {
                            FiltredRoadsData.Ways.Add(wayID, wayDic);
                            break;
                        }

                    }

                    //Trees
                    if (Attribute_Trees.Attributes.Contains(Value.Attribute))
                    {
                        if (!FiltredTreesData.Ways.ContainsKey(wayID))
                        {
                            wayDic.MainTag.Attribute = Value.Attribute;
                            wayDic.MainTag.Value = Value.Value;
                            FiltredTreesData.Ways.Add(wayID, wayDic);
                        }

                    }

                    //Grass
                    if (Attribute_Grass.Attributes.Contains(Value.Attribute))
                    {
                        wayDic.MainTag.Attribute = Value.Attribute;
                        wayDic.MainTag.Value = Value.Value;

                        if (!FiltredGrassData.Ways.ContainsKey(wayID))
                        {
                            FiltredGrassData.Ways.Add(wayID, wayDic);
                            break;
                        }

                    }
                    //Buildings
                    if (Attribute_Buildings.Attributes.Contains(Value.Attribute))
                    {
                        wayDic.MainTag.Attribute = Value.Attribute;
                        wayDic.MainTag.Value = Value.Value;

                        if (!FiltredBuildingsData.Ways.ContainsKey(wayID))
                        {
                            FiltredBuildingsData.Ways.Add(wayID, wayDic);
                            break;
                        }

                    }


                }
 
            }


            foreach (var node in osmData.Nodes)
            {
                long wayID = node.Key;

                var nodeAttributes = node.Value.Tags;

                foreach (var Value in nodeAttributes)
                {                        
                    //Points
                    if (Attribute_Points.Attributes.Contains(Value.Attribute))
                    {
                        node.Value.MainTag.Attribute = Value.Attribute;
                        node.Value.MainTag.Value = Value.Value;

                        if (!FiltredPointsData.Nodes.ContainsKey(wayID))
                        {
                            FiltredPointsData.Nodes.Add(wayID, node.Value);
                            break;
                        }
                    }

                    //Trees
                    if (Attribute_Trees.Attributes.Contains(Value.Attribute))
                    {
                        if (!FiltredTreesData.Nodes.ContainsKey(wayID))
                        {
                            node.Value.MainTag.Attribute = Value.Attribute;
                            node.Value.MainTag.Value = Value.Value;
                            FiltredTreesData.Nodes.Add(wayID, node.Value);
                        }
                    }

                    //Grass
                    if (Attribute_Grass.Attributes.Contains(Value.Attribute))
                    {
                        if (!FiltredGrassData.Nodes.ContainsKey(wayID))
                        {
                            node.Value.MainTag.Attribute = Value.Attribute;
                            node.Value.MainTag.Value = Value.Value;
                            FiltredGrassData.Nodes.Add(wayID, node.Value);
                        }
                    }

                }

            }

        }
        public override void GetGeoVectorRoadsData(GISTerrainLoaderGeoVectorData GeoDataContainer)
        {
            if (osmData.Ways.Count != 0)
            {
                foreach (var wayDic in FiltredRoadsData.Ways)
                {
                    GISTerrainLoaderLinesGeoData RoadGeoData = new GISTerrainLoaderLinesGeoData();

                    RoadGeoData.Tag = wayDic.Value.MainTag.Value;

                    for (int i = 0; i < wayDic.Value.Nodes.Count; i++)
                    {
                        var latlon = new DVector2(wayDic.Value.Nodes[i].Lon, wayDic.Value.Nodes[i].Lat);

                        RoadGeoData.GeoPoints.Add(latlon);
                    }

                    foreach (var attribute in wayDic.Value.Tags)
                    {
                        if (attribute.Attribute == "name")
                            RoadGeoData.Name = attribute.Value;
                    }


                    GeoDataContainer.GeoRoads.Add(RoadGeoData);

                }
            }

        }
        public override void GetGeoVectorTreesData(GISTerrainLoaderGeoVectorData GeoDataContainer)
        {
            if (osmData.Ways.Count != 0)
            {
                foreach (var wayDic in FiltredTreesData.Ways)
                {
                    GISTerrainLoaderPolygonGeoData TreeGeoData = new GISTerrainLoaderPolygonGeoData();

                    TreeGeoData.Tag = wayDic.Value.MainTag.Value;

                    for (int i = 0; i < wayDic.Value.Nodes.Count; i++)
                    {
                        var latlon = new DVector2(wayDic.Value.Nodes[i].Lon, wayDic.Value.Nodes[i].Lat);

                        TreeGeoData.GeoPoints.Add(latlon);
                    }

                    foreach (var attribute in wayDic.Value.Tags)
                    {
                        if (attribute.Attribute == "name")
                            TreeGeoData.Name = attribute.Value;
                    }


                    GeoDataContainer.GeoTrees.Add(TreeGeoData);

                }
            }
        }
        public override void GetGeoVectorBuildingData(GISTerrainLoaderGeoVectorData GeoDataContainer)
        {
            if (osmData.Ways.Count != 0)
            {
                foreach (var wayDic in FiltredBuildingsData.Ways)
                {
                    GISTerrainLoaderPolygonGeoData BuildingGeoData = new GISTerrainLoaderPolygonGeoData();

                    BuildingGeoData.Tag = wayDic.Value.MainTag.Value;

                    for (int i = 0; i < wayDic.Value.Nodes.Count; i++)
                    {
                        var latlon = new DVector2(wayDic.Value.Nodes[i].Lon, wayDic.Value.Nodes[i].Lat);

                        BuildingGeoData.GeoPoints.Add(latlon);
                    }

                    foreach (var attribute in wayDic.Value.Tags)
                    {
                        if (attribute.Attribute == "name")
                            BuildingGeoData.Name = attribute.Value;
 
                        if (attribute.Attribute == "building:levels")
                            BuildingGeoData.Levels = int.Parse(attribute.Value, CultureInfo.InvariantCulture); 

                        if (attribute.Attribute == "building:min_level")
                            BuildingGeoData.MinLevel = int.Parse(attribute.Value, CultureInfo.InvariantCulture); 

                        if (attribute.Attribute == "building:height")
                            BuildingGeoData.Height = float.Parse(attribute.Value.Replace(" ", "").Replace("m", ""), CultureInfo.InvariantCulture); ;

                        if (attribute.Attribute == "building:min_height")
                            BuildingGeoData.MinHeight = float.Parse(attribute.Value, CultureInfo.InvariantCulture); 


                    }

                    GeoDataContainer.GeoBuilding.Add(BuildingGeoData);

                }
            }
        }
        public override void GetGeoVectorGrassData(GISTerrainLoaderGeoVectorData GeoDataContainer)
        {
            if (osmData.Ways.Count != 0)
            {
                foreach (var wayDic in FiltredGrassData.Ways)
                {
                    GISTerrainLoaderPolygonGeoData GrassGeoData = new GISTerrainLoaderPolygonGeoData();

                    GrassGeoData.Tag = wayDic.Value.MainTag.Value;

                    for (int i = 0; i < wayDic.Value.Nodes.Count; i++)
                    {
                        var latlon = new DVector2(wayDic.Value.Nodes[i].Lon, wayDic.Value.Nodes[i].Lat);

                        GrassGeoData.GeoPoints.Add(latlon);
                    }

                    foreach (var attribute in wayDic.Value.Tags)
                    {
                        if (attribute.Attribute == "name")
                            GrassGeoData.Name = attribute.Value;
                    }


                    GeoDataContainer.GeoGrass.Add(GrassGeoData);

                }
            }

        }
        public override void GetGeoVectorPointsData(GISTerrainLoaderGeoVectorData GeoDataContainer)
        {

            if (osmData.Nodes.Count != 0)
            {
                foreach (var node in FiltredPointsData.Nodes)
                {
                    GISTerrainLoaderPointGeoData PointGeoData = new GISTerrainLoaderPointGeoData();
                    PointGeoData.Tag = node.Value.MainTag.Value;
                    PointGeoData.GeoPoint = new DVector2(node.Value.Lon, node.Value.Lat);

                    foreach (var attribute in node.Value.Tags)
                    {
                        if (attribute.Attribute == "name")
                            PointGeoData.Name = attribute.Value;
                    }
     
                    GeoDataContainer.GeoPoints.Add(PointGeoData);

                }
            }
        }
    }
}

