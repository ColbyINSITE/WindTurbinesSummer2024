/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderShapeFileLoader : GISTerrainLoaderGeoDataHolder
    {
        public List<GISTerrainLoaderShapeFileData> TotalShapes;
        public List<GISTerrainLoaderShapeFileData> FiltredGrassData;
        public List<GISTerrainLoaderShapeFileData> FiltredRoadsData;
        public List<GISTerrainLoaderShapeFileData> FiltredBuildingsData;
        public List<GISTerrainLoaderShapeFileData> FiltredTreesData;

        public static int DBFColCount = 0;
        public static int DBFRowCount = 0;
        public static DbfFile DBFBase;
        public GISTerrainLoaderShapeFileLoader(GISTerrainLoaderShpFile ShapeFile)
        {
            TotalShapes = new List<GISTerrainLoaderShapeFileData>();
            FiltredGrassData = new List<GISTerrainLoaderShapeFileData>();
            FiltredRoadsData = new List<GISTerrainLoaderShapeFileData>();
            FiltredBuildingsData = new List<GISTerrainLoaderShapeFileData>();
            FiltredTreesData = new List<GISTerrainLoaderShapeFileData>();

            string dbfpath = Path.ChangeExtension(ShapeFile.FilePath, ".dbf");
            ShapeFile.ReadProjection(ShapeFile.FilePath);
            DbfFile dbfDataBase = LoadDBFBase(dbfpath, ShapeFile);

            FilterData();

        }


        public DbfFile LoadDBFBase(string dbfPath, GISTerrainLoaderShpFile shapeFile)
        {
            if (File.Exists(dbfPath))
            {
                try
                {
                    DBFBase = new DbfFile(System.Text.Encoding.ASCII);
                    DBFBase.Open(dbfPath, FileMode.Open);

                    DBFColCount = (int)DBFBase.Header.ColumnCount;
                    DBFRowCount = (int)DBFBase.Header.RecordCount;

                    for (int r = 0; r < DBFRowCount; r++)
                    {
                        var database = new List<GISTerrainLoaderShpDataBase>();

                        for (var c = 0; c < DBFColCount; c++)
                        {
                            string tag = "";
                            DBFBase.ReadValue(r, c, out tag);
                            database.Add(new GISTerrainLoaderShpDataBase(DBFBase.Header._fields[c].Name, tag));
                            shapeFile.RecordSet[r].DataBase = database;
  
                        }

                    }

                    for (int i = 0; i < shapeFile.RecordSet.Count; i++)
                    {
                        var shape = shapeFile.RecordSet[i];
                        
                        GISTerrainLoaderShapeFileData shapeData = new GISTerrainLoaderShapeFileData(shapeFile.ShpType, shape,shapeFile.CoordinateReferenceSystem);

                        TotalShapes.Add(shapeData);
                    }

                    DBFBase.Close();

                  return DBFBase;
                }
                catch (Exception e)
                {
                    Debug.Log("Could not read DataBase .. " + e);
                    DBFBase.Close();
                    return null;

                }
            }
            else
            {
                Debug.LogError("DBF Database not exist");
                return null;
            }


        }
        private HashSet<string> RoadAlreadyAdded = new HashSet<string>();
        private HashSet<string> TreeAlreadyAdded = new HashSet<string>();
        private HashSet<string> GrassAlreadyAdded = new HashSet<string>();
        private HashSet<string> BuildingAlreadyAdded = new HashSet<string>();

        private GISTerrainLoaderAttributes_SO Attribute_Points;
        private GISTerrainLoaderAttributes_SO Attribute_Roads;
        private GISTerrainLoaderAttributes_SO Attribute_Trees;
        private GISTerrainLoaderAttributes_SO Attribute_Grass;
        private GISTerrainLoaderAttributes_SO Attribute_Buildings;
 
        private void FilterData()
        {
            Attribute_Points = Resources.Load("VectorAttributes/Attribute_Points") as GISTerrainLoaderAttributes_SO;
            if (!Attribute_Points)
            {
                Attribute_Points = new GISTerrainLoaderAttributes_SO();
                Attribute_Points.Attributes = new List<string>();
                Debug.LogError("Roads Attribute File Not found .. Restore 'Attribute_Roads' ScriptableObject ");
            }

            Attribute_Roads = Resources.Load("VectorAttributes/Attribute_Roads") as GISTerrainLoaderAttributes_SO;
            if (!Attribute_Roads)
            {
                Attribute_Roads = new GISTerrainLoaderAttributes_SO();
                Attribute_Roads.Attributes = new List<string>();
                Debug.LogError("Roads Attribute File Not found .. Restore 'Attribute_Roads' ScriptableObject ");
            }

            Attribute_Trees = Resources.Load("VectorAttributes/Attribute_Trees") as GISTerrainLoaderAttributes_SO;
            if (!Attribute_Trees)
            {
                Attribute_Trees = new GISTerrainLoaderAttributes_SO();
                Attribute_Trees.Attributes = new List<string>();
                Debug.LogError("Trees Attribute File Not found .. Restore 'Attribute_Trees' ScriptableObject ");
            }

            Attribute_Grass = Resources.Load("VectorAttributes/Attribute_Grass") as GISTerrainLoaderAttributes_SO;
            if (!Attribute_Grass)
            {
                Attribute_Grass = new GISTerrainLoaderAttributes_SO();
                Attribute_Grass.Attributes = new List<string>();
                Debug.LogError("Grass Attribute File Not found .. Restore 'Attribute_Grass' ScriptableObject ");
            }
            Attribute_Buildings = Resources.Load("VectorAttributes/Attribute_Buildings") as GISTerrainLoaderAttributes_SO;
            if (!Attribute_Buildings)
            {
                Attribute_Buildings = new GISTerrainLoaderAttributes_SO();
                Attribute_Buildings.Attributes = new List<string>();
                Debug.LogError("Buildings Attribute File Not found .. Restore 'Attribute_Buildings' ScriptableObject ");
            }
            


        }
        public override void GetGeoVectorPointsData(GISTerrainLoaderGeoVectorData GeoDataContainer)
        {
            for (int i = 0; i < TotalShapes.Count; i++)
            {
                var shape = TotalShapes[i];

                IEnumerable<string> PointsIntersection = null;
                var ShapeAttributes = shape.DataBase.Keys.ToList();
                PointsIntersection = Attribute_Points.Attributes.Intersect<string>(ShapeAttributes);

                if (PointsIntersection != null)
                {
                    var PointID = shape.Id;

                    foreach (var attribute in PointsIntersection)
                    {
                        var Value = "";
                        if (shape.DataBase.TryGetValue(attribute, out Value))
                        {

                            if (!string.IsNullOrEmpty(Value) && !RoadAlreadyAdded.Contains(PointID))
                            {
                                FiltredRoadsData.Add(shape);
                                RoadAlreadyAdded.Add(PointID);

                                GISTerrainLoaderPointGeoData PointGeoData = new GISTerrainLoaderPointGeoData();
                                PointGeoData.ID = PointID;
                                PointGeoData.Tag = Value;
                                var points = GISTerrainLoaderShapeFactory.GetTypePoint(shape.ShapeType, shape.ShapeRecord.Contents).ToList();

                                if(points.Count==1)
                                {
                                    var pll = points[0];
                                    var LatLon = GeoRefConversion.ConvertTOLatLon(shape.CoordinateReferenceSystem, new DVector2(pll.X, pll.Y));
                                    PointGeoData.GeoPoint = LatLon;

                                    var Name = "";
                                    if (shape.DataBase.TryGetValue("name", out Name))
                                    {
                                        PointGeoData.Name = Name;
                                    }

                                    GeoDataContainer.GeoPoints.Add(PointGeoData);
                                }

                            }

                        }
                    }

                }
            }
        }
        public override void GetGeoVectorBuildingData(GISTerrainLoaderGeoVectorData GeoDataContainer)
        {
            for (int i = 0; i < TotalShapes.Count; i++)
            {
                var shape = TotalShapes[i];

                IEnumerable<string> BuildingIntersection = null;
                var ShapeAttributes = shape.DataBase.Keys.ToList();
                BuildingIntersection = Attribute_Buildings.Attributes.Intersect<string>(ShapeAttributes);

                if (BuildingIntersection != null)
                {
                    var PolyID = shape.Id;

                    foreach (var attribute in BuildingIntersection)
                    {
                        var Value = "";
                        if (shape.DataBase.TryGetValue(attribute, out Value))
                        {
                            if (!string.IsNullOrEmpty(Value) && !BuildingAlreadyAdded.Contains(PolyID))
                            {
                                FiltredBuildingsData.Add(shape);
                                BuildingAlreadyAdded.Add(PolyID);

                                GISTerrainLoaderPolygonGeoData BuildingsGeoData = new GISTerrainLoaderPolygonGeoData();
                                BuildingsGeoData.ID = PolyID;
                                BuildingsGeoData.Tag = Value;
                                var points = GISTerrainLoaderShapeFactory.GetTypePoint(shape.ShapeType, shape.ShapeRecord.Contents).ToList();

                                for (int p = 0; p < points.Count; p++)
                                {
                                    var pll = points[p]; 
                                    var LatLon = GeoRefConversion.ConvertTOLatLon(shape.CoordinateReferenceSystem, new DVector2(pll.X, pll.Y));
                                    BuildingsGeoData.GeoPoints.Add(LatLon);
                     
                                }

                                var Name = "";
                                if (shape.DataBase.TryGetValue("name", out Name))
                                    BuildingsGeoData.Name = Name;

                                BuildingsGeoData.CoordinateReferenceSystem = shape.CoordinateReferenceSystem;

                                GeoDataContainer.GeoBuilding.Add(BuildingsGeoData);

                                var Levels = "";
                                if (shape.DataBase.TryGetValue("building:levels",out Levels))
                                    BuildingsGeoData.Levels = int.Parse(Levels, CultureInfo.InvariantCulture);
 
                                var MinLevel = "";
                                if (shape.DataBase.TryGetValue("building:min_level", out MinLevel))
                                    BuildingsGeoData.MinLevel = int.Parse(MinLevel, CultureInfo.InvariantCulture);

                                var Height = "";
                                if (shape.DataBase.TryGetValue("building:height", out Height))
                                    BuildingsGeoData.Height = float.Parse(Height.Replace(" ", "").Replace("m", ""), CultureInfo.InvariantCulture);

                                var MinHeight = "";
                                if (shape.DataBase.TryGetValue("building:min_height",out MinHeight))
                                    BuildingsGeoData.MinHeight = float.Parse(MinHeight, CultureInfo.InvariantCulture);
 
                            }

                        }
                    }

                }
            }
        }

        public override void GetGeoVectorRoadsData(GISTerrainLoaderGeoVectorData GeoDataContainer)
        {
            for (int i = 0; i < TotalShapes.Count; i++)
            {
                var shape = TotalShapes[i];

                IEnumerable<string> RoadIntersection = null;
                var ShapeAttributes = shape.DataBase.Keys.ToList();
                RoadIntersection = Attribute_Roads.Attributes.Intersect<string>(ShapeAttributes);

                if (RoadIntersection != null)
                {
                    var RoadID = shape.Id;

                    foreach (var attribute in RoadIntersection)
                    {
                        var Value = "";
                        if (shape.DataBase.TryGetValue(attribute, out Value))
                        {

                            if (!string.IsNullOrEmpty(Value) && !RoadAlreadyAdded.Contains(RoadID))
                            {
                                FiltredRoadsData.Add(shape);
                                RoadAlreadyAdded.Add(RoadID);

                                GISTerrainLoaderLinesGeoData RoadGeoData = new GISTerrainLoaderLinesGeoData();
                                RoadGeoData.ID = RoadID;
                                RoadGeoData.Tag = Value;
                                var points = GISTerrainLoaderShapeFactory.GetTypePoint(shape.ShapeType, shape.ShapeRecord.Contents).ToList();

                                for (int p = 0; p < points.Count; p++)
                                {
                                    var pll = points[p];
                                    var LatLon = GeoRefConversion.ConvertTOLatLon(shape.CoordinateReferenceSystem, new DVector2(pll.X, pll.Y));
                                    RoadGeoData.GeoPoints.Add(LatLon);
                                }

                                var Name = "";
                                if (shape.DataBase.TryGetValue("name", out Name))
                                {
                                    RoadGeoData.Name = Name;
                                }

                                GeoDataContainer.GeoRoads.Add(RoadGeoData);
                            }

                        }
                    }

                }
            }
        }

        public override void GetGeoVectorTreesData(GISTerrainLoaderGeoVectorData GeoDataContainer)
        {
            for (int i = 0; i < TotalShapes.Count; i++)
            {
                var shape = TotalShapes[i];

                IEnumerable<string> TreesIntersection = null;
                var ShapeAttributes = shape.DataBase.Keys.ToList();
                TreesIntersection = Attribute_Trees.Attributes.Intersect<string>(ShapeAttributes);

                if (TreesIntersection != null)
                {
                    var PolyID = shape.Id;

                    foreach (var attribute in TreesIntersection)
                    {
                        var Value = "";
                        if (shape.DataBase.TryGetValue(attribute, out Value))
                        {

                            if (!string.IsNullOrEmpty(Value) && !TreeAlreadyAdded.Contains(PolyID))
                            {
                                FiltredTreesData.Add(shape);
                                TreeAlreadyAdded.Add(PolyID);

                                GISTerrainLoaderPolygonGeoData TreeGeoData = new GISTerrainLoaderPolygonGeoData();
                                TreeGeoData.ID = PolyID;
                                TreeGeoData.Tag = Value;
                                var points = GISTerrainLoaderShapeFactory.GetTypePoint(shape.ShapeType, shape.ShapeRecord.Contents).ToList();

                                for (int p = 0; p < points.Count; p++)
                                {
                                    var pll = points[p];
                                    var LatLon = GeoRefConversion.ConvertTOLatLon(shape.CoordinateReferenceSystem, new DVector2(pll.X, pll.Y));
                                    TreeGeoData.GeoPoints.Add(LatLon);
                                }

                                var Name = "";
                                if (shape.DataBase.TryGetValue("name", out Name))
                                {
                                    TreeGeoData.Name = Name;
                                }

                                TreeGeoData.CoordinateReferenceSystem = shape.CoordinateReferenceSystem;
                                GeoDataContainer.GeoTrees.Add(TreeGeoData);
                           
                            }

                        }
                    }

                }
            }
        }

        public override void GetGeoVectorGrassData(GISTerrainLoaderGeoVectorData GeoDataContainer)
        {
            for (int i = 0; i < TotalShapes.Count; i++)
            {
                var shape = TotalShapes[i];

                IEnumerable<string> GrassIntersection = null;
                var ShapeAttributes = shape.DataBase.Keys.ToList();
                GrassIntersection = Attribute_Grass.Attributes.Intersect<string>(ShapeAttributes);

                if (GrassIntersection != null)
                {
                    var PolyID = shape.Id;

                    foreach (var attribute in GrassIntersection)
                    {
                        var Value = "";
                        if (shape.DataBase.TryGetValue(attribute, out Value))
                        {
                            if (!string.IsNullOrEmpty(Value) && !TreeAlreadyAdded.Contains(PolyID))
                            {
                                FiltredGrassData.Add(shape);
                                GrassAlreadyAdded.Add(PolyID);

                                GISTerrainLoaderPolygonGeoData GrassGeoData = new GISTerrainLoaderPolygonGeoData();
                                GrassGeoData.ID = PolyID;
                                GrassGeoData.Tag = Value;
                                var points = GISTerrainLoaderShapeFactory.GetTypePoint(shape.ShapeType, shape.ShapeRecord.Contents).ToList();

                                for (int p = 0; p < points.Count; p++)
                                {
                                    var pll = points[p];
                                    var LatLon = GeoRefConversion.ConvertTOLatLon(shape.CoordinateReferenceSystem, new DVector2(pll.X, pll.Y));
                                    GrassGeoData.GeoPoints.Add(LatLon);
                                }

                                var Name = "";
                                if (shape.DataBase.TryGetValue("name", out Name))
                                    GrassGeoData.Name = Name;

                                GrassGeoData.CoordinateReferenceSystem = shape.CoordinateReferenceSystem;
                                GeoDataContainer.GeoGrass.Add(GrassGeoData);

                            }

                        }
                    }

                }
            }

        }

        public GISTerrainLoaderGeoVectorData GetGeoShapeFileData()
        {
            GISTerrainLoaderGeoVectorData GeoDataContainer = new GISTerrainLoaderGeoVectorData();

            for (int i = 0; i < TotalShapes.Count; i++)
            {

                var shape = TotalShapes[i];

                if (shape.CoordinateReferenceSystem == null)
                {
                    Debug.LogError("Shapefile Coordinate Reference System not recognized, Check if .Prj file exist .. ! ");
                }

                switch (shape.ShapeType)
                {
                    case ShapeType.Point:

                        GISTerrainLoaderPointGeoData PointGeoData = new GISTerrainLoaderPointGeoData();
                        PointGeoData.ID = shape.Id;
                        PointGeoData.DataBase = shape.DataBase;

                        var points = GISTerrainLoaderShapeFactory.GetTypePoint(shape.ShapeType, shape.ShapeRecord.Contents).ToList();

                        if (points.Count == 1)
                        {
                            var pll = points[0];
                            var LatLon = GeoRefConversion.ConvertTOLatLon(shape.CoordinateReferenceSystem, new DVector2(pll.X, pll.Y));
                            PointGeoData.GeoPoint = LatLon;
                        }

                        GeoDataContainer.GeoPoints.Add(PointGeoData);
                        break;
                    case ShapeType.PointZ:

                        PointGeoData = new GISTerrainLoaderPointGeoData();
                        PointGeoData.ID = shape.Id;
                        PointGeoData.DataBase = shape.DataBase;

                        points = GISTerrainLoaderShapeFactory.GetTypePoint(shape.ShapeType, shape.ShapeRecord.Contents).ToList();
                        var Elevations = GISTerrainLoaderShapeFactory.GetElevation(shape.ShapeType, shape.ShapeRecord.Contents).ToList();

                        if (points.Count == 1)
                        {
                            var pll = points[0];
                            var LatLon = GeoRefConversion.ConvertTOLatLon(shape.CoordinateReferenceSystem, new DVector2(pll.X, pll.Y));
                            LatLon.z = Elevations[0];
                            PointGeoData.GeoPoint = LatLon;

                        }

                        GeoDataContainer.GeoPoints.Add(PointGeoData);
                        break;
                    case ShapeType.PolyLine:

                        GISTerrainLoaderLinesGeoData LineGeoData = new GISTerrainLoaderLinesGeoData();
                        LineGeoData.ID = shape.Id;
                        LineGeoData.DataBase = shape.DataBase;

                        points = GISTerrainLoaderShapeFactory.GetTypePoint(shape.ShapeType, shape.ShapeRecord.Contents).ToList();

                        for (int p = 0; p < points.Count; p++)
                        {
                            var pll = points[p];
                            var LatLon = GeoRefConversion.ConvertTOLatLon(shape.CoordinateReferenceSystem, new DVector2(pll.X, pll.Y));
                            LineGeoData.GeoPoints.Add(LatLon);
                        }

                        GeoDataContainer.GeoLines.Add(LineGeoData);

                        break;
                    case ShapeType.PolyLineZ:

                        LineGeoData = new GISTerrainLoaderLinesGeoData();
                        LineGeoData.ID = shape.Id;
                        LineGeoData.DataBase = shape.DataBase;

                        points = GISTerrainLoaderShapeFactory.GetTypePoint(shape.ShapeType, shape.ShapeRecord.Contents).ToList();
                        Elevations = GISTerrainLoaderShapeFactory.GetElevation(shape.ShapeType, shape.ShapeRecord.Contents).ToList();

                        for (int p = 0; p < points.Count; p++)
                        {
                            var pll = points[p];
                            var LatLon = GeoRefConversion.ConvertTOLatLon(shape.CoordinateReferenceSystem, new DVector2(pll.X, pll.Y));
                            LatLon.z = Elevations[p];
                            LineGeoData.GeoPoints.Add(LatLon);
                        }

                        GeoDataContainer.GeoLines.Add(LineGeoData);

                        break;

                    case ShapeType.Polygon:
                        GISTerrainLoaderPolygonGeoData PolyGeoData = new GISTerrainLoaderPolygonGeoData();
                        PolyGeoData.ID = shape.Id;
                        PolyGeoData.DataBase = shape.DataBase;

                        points = GISTerrainLoaderShapeFactory.GetTypePoint(shape.ShapeType, shape.ShapeRecord.Contents).ToList();

                        for (int p = 0; p < points.Count; p++)
                        {
                            var pll = points[p];
                            var LatLon = GeoRefConversion.ConvertTOLatLon(shape.CoordinateReferenceSystem, new DVector2(pll.X, pll.Y));
                            PolyGeoData.GeoPoints.Add(LatLon);
                        }

                        GeoDataContainer.GeoPolygons.Add(PolyGeoData);
                        break;
                    case ShapeType.PolygonZ:
                        PolyGeoData = new GISTerrainLoaderPolygonGeoData();
                        PolyGeoData.ID = shape.Id;
                        PolyGeoData.DataBase = shape.DataBase;

                        points = GISTerrainLoaderShapeFactory.GetTypePoint(shape.ShapeType, shape.ShapeRecord.Contents).ToList();
                        Elevations = GISTerrainLoaderShapeFactory.GetElevation(shape.ShapeType, shape.ShapeRecord.Contents).ToList();

                        for (int p = 0; p < points.Count; p++)
                        {
                            var pll = points[p];
                            var LatLon = GeoRefConversion.ConvertTOLatLon(shape.CoordinateReferenceSystem, new DVector2(pll.X, pll.Y));
                            LatLon.z = Elevations[p];
                            PolyGeoData.GeoPoints.Add(LatLon);
                        }


                        GeoDataContainer.GeoPolygons.Add(PolyGeoData);
                        break;
                }

            }
            return GeoDataContainer;
        }
    }
}