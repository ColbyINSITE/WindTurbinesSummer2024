using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public abstract class GISTerrainLoaderGeoDataHolder
    {
        public abstract void GetGeoVectorPointsData(GISTerrainLoaderGeoVectorData GeoDataContainer);
        public abstract void GetGeoVectorRoadsData(GISTerrainLoaderGeoVectorData GeoDataContainer);
        public abstract void GetGeoVectorTreesData(GISTerrainLoaderGeoVectorData GeoDataContainer);
        public abstract void GetGeoVectorGrassData(GISTerrainLoaderGeoVectorData GeoDataContainer);
        public abstract void GetGeoVectorBuildingData(GISTerrainLoaderGeoVectorData GeoDataContainer);

    }

    public class GISTerrainLoaderGeoVectorData
    {
        public List<GISTerrainLoaderPointGeoData> GeoPoints;
        public List<GISTerrainLoaderLinesGeoData> GeoRoads;
        public List<GISTerrainLoaderPolygonGeoData> GeoTrees;
        public List<GISTerrainLoaderPolygonGeoData> GeoGrass;
        public List<GISTerrainLoaderPolygonGeoData> GeoBuilding;

        public List<GISTerrainLoaderPolygonGeoData> GeoPolygons;
        public List<GISTerrainLoaderLinesGeoData> GeoLines;

        public GISTerrainLoaderGeoVectorData()
        {
            GeoPoints = new List<GISTerrainLoaderPointGeoData>();
            GeoRoads = new List<GISTerrainLoaderLinesGeoData>();
            GeoTrees = new List<GISTerrainLoaderPolygonGeoData>();
            GeoGrass = new List<GISTerrainLoaderPolygonGeoData>();
            GeoBuilding = new List<GISTerrainLoaderPolygonGeoData>();

            GeoPolygons = new List<GISTerrainLoaderPolygonGeoData>();
            GeoLines = new List<GISTerrainLoaderLinesGeoData>();




        }

    }

    #region Point
    public class GISTerrainLoaderPointGeoData
    {
        public string ID;
        public string Name;
        public string Tag;
        public DVector2 GeoPoint;
        public float Elevation;

        public Dictionary<string, string> DataBase;

        public GISTerrainLoaderPointGeoData()
        {
            ID = "";
            Name = "";
            Tag = "";
            GeoPoint = new DVector2 (0,0);

            DataBase = new Dictionary<string, string>();
        }
 
    }
    #endregion
    #region Line
    public class GISTerrainLoaderLinesGeoData
    {
        public string ID;
        public string Name;
        public string Tag;
        public List<DVector2> GeoPoints;

        //DataBase 
        public Dictionary<string, string> DataBase;
        public GISTerrainLoaderLinesGeoData()
        {
            ID = "";
            Name = "";
            Tag = "";
            GeoPoints = new List<DVector2>();

            DataBase = new Dictionary<string, string>();

        }

        public GISTerrainLoaderLinesGeoData(string m_name, string m_tag, List<DVector2> m_GeoPoints)
        {
            Name = m_name;
            Tag = m_tag;
            GeoPoints = m_GeoPoints;
        }

    }
    #endregion
    #region Polygon
    public class GISTerrainLoaderPolygonGeoData
    {
        public string ID;
        public string Name;
        public string Tag;
        public List<DVector2> GeoPoints;
        public GTLGeographicCoordinateSystem CoordinateReferenceSystem = null;

        //Building Data
        public float Height;
        public float MinHeight;
        public int Levels;
        public int MinLevel;

        //DataBase 

        public Dictionary<string, string> DataBase;

        public GISTerrainLoaderPolygonGeoData()
        {
            ID = "";
            Name = "";
            Tag = "";
            GeoPoints = new List<DVector2>();

            Levels = 0;
            MinLevel = 0;
            Height = 0;
            MinHeight = 0;

            DataBase = new Dictionary<string, string>();

        }
    }
    #endregion

}