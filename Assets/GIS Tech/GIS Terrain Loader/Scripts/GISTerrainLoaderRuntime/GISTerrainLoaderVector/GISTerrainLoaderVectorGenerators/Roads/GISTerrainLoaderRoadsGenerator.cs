/*     Unity GIS Tech 2020-2021      */

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderRoadsGenerator
    {
 
        private static GISTerrainLoaderShapeFileLoader ShapeLoader;
        private static RoadGenerationType roadType;
        private static bool EnableRoadName;


        private static bool IsRuntime = false;

        private static HashSet<string> alreadyCreated;
        private static HashSet<string> alreadyAdded;

        private static List<GISTerrainLoaderSO_Road> RoadsPrefab;

        public static void GenerateTerrainRoades(TerrainContainerObject container, GISTerrainLoaderGeoVectorData GeoData, RoadGenerationType m_roadType, bool m_EnableRoadName, List<GISTerrainLoaderSO_Road> m_RoadsPrefab, bool m_Runtime = false)
        {
            IsRuntime = m_Runtime;
            RoadsPrefab = m_RoadsPrefab;
            roadType = m_roadType;
            EnableRoadName = m_EnableRoadName;


            if (m_roadType == RoadGenerationType.EasyRoad3D)
            {
#if EASYROADS || EASYROADS3D
            GISTerrainLoaderEasyRoadGenerator.DestroyRoads();
#endif
            }
            else
            {
                GenerateRoades(container, GeoData);
            }

            if (m_roadType == RoadGenerationType.EasyRoad3D)
            {
#if EASYROADS || EASYROADS3D
            if (roadType == RoadGenerationType.EasyRoad3D)
                GISTerrainLoaderEasyRoadGenerator.Finilize();
#endif
            }
            GeoRefConversion.terrain = null;
        }
        private static void GenerateRoades(TerrainContainerObject Container, GISTerrainLoaderGeoVectorData GeoData)
        {
            if (GeoData.GeoRoads.Count != 0)
            {

                GameObject highways = null;

                if (Container.transform.Find("highways") == null)
                {
                    highways = new GameObject();
                    highways.name = "highways";
                    highways.transform.parent = Container.transform;
                }
                else
                    highways = Container.transform.Find("highways").gameObject;

                foreach (var Road in GeoData.GeoRoads)
                    CreateHighway(Road, Container, highways.transform);
            }
        }
        public static GameObject CreateHighway(GISTerrainLoaderLinesGeoData Road, TerrainContainerObject container, Transform parent)
        {
            TerrainData tdata = container.terrains[0, 0].terrainData;
            int detailResolution = tdata.detailResolution;

            Vector3[] linePoints = new Vector3[Road.GeoPoints.Count];

            for (int i = 0; i < Road.GeoPoints.Count; i++)
            {
                var latlon = new DVector2(Road.GeoPoints[i].x, Road.GeoPoints[i].y);

                linePoints[i] = GeoRefConversion.LatLonToUnityWorldSpace(latlon, container);
             }

            var roadtype = Road.Tag;

            var defaultRoad = RoadsPrefab[0];

            foreach (var so_road in RoadsPrefab)
            {
                if (so_road.name == roadtype)
                {
                    defaultRoad = so_road;
                }
            }

            var road = new GISTerrainLoaderRoad(defaultRoad, container);
            road.Points = linePoints;

            GameObject m_road = null;

            switch (roadType)
            {
                case RoadGenerationType.Line:

                    m_road = GISTerrainLoaderLineGenerator.CreateLine(road);
                    m_road.transform.parent = parent;
                    m_road.name = roadtype;
                    break;
                case RoadGenerationType.EasyRoad3D:

                    if (EasyRoadBaseModelExist())
                    {

#if EASYROADS || EASYROADS3D
                        m_road = GISTerrainLoaderEasyRoadGenerator.CreateRoad(road, container, IsRuntime);
#endif
                    }
                    else
                    {
                        Debug.Log("EasyRoad asset not exist or main resources prefab not added to the current project.. ");
                        Debug.Log("Please Import EasyRoad and add 'EASYROADS' or 'EASYROADS3D' to the scripting Define Symbols from Player Settings plane .");
                    }

                    break;
            }

            if (m_road != null)
            {
                m_road.name = Road.Name + "_" + Road.Tag;

                if (EnableRoadName)
                    CreateRoadNameLabel(linePoints, m_road.name, m_road.transform, container);

            }

            return m_road;
        }
















        #region GPX
        public static void GenerateTerrainRoades(GISTerrainLoaderGPXFileData m_GPXFileData, TerrainContainerObject container, RoadGenerationType m_roadType, bool m_EnableRoadName, List<GISTerrainLoaderSO_Road> m_RoadsPrefab,GISTerrainLoaderSO_Road TrackPrefab, bool m_Runtime = false)
        {
            IsRuntime = m_Runtime;
            RoadsPrefab = m_RoadsPrefab;
            roadType = m_roadType;
            EnableRoadName = m_EnableRoadName;

            GameObject highways = null;

            if (container.transform.Find("highways") == null)
            {
                highways = new GameObject();
                highways.name = "highways";
                highways.transform.parent = container.transform;
            }
            else
                highways = container.transform.Find("highways").gameObject;

            GISTerrainLoaderSO_Road defaultRoad = null;

            if (TrackPrefab == null)
            {
                defaultRoad = RoadsPrefab[0];
                var roadtype = "track";

                foreach (var so_road in RoadsPrefab)
                {
                    if (so_road.name == roadtype)
                    {
                        defaultRoad = so_road;
                    }

                }
            }
            else
                defaultRoad = TrackPrefab;

          var TLPMercator_X = container.TLPointMercator.x;
            var TLPMercator_Y = container.TLPointMercator.y;

            var DRPMercator_X = container.DRPointMercator.x;
            var DRPMercator_Y = container.DRPointMercator.y;

            TerrainData tdata = container.terrains[0, 0].terrainData;

            int detailResolution = tdata.detailResolution;


            for (int i = 0; i < m_GPXFileData.Paths.Count; i++)
            {
                var path = m_GPXFileData.Paths[i];
  
                var points = path.WayPoints;  

                float pxmin = float.MaxValue, pxmax = float.MinValue, pymin = float.MaxValue, pymax = float.MinValue;

                List<Vector3> Points = new List<Vector3>();

                for (int p = 0; p < points.Count; p++)
                {

                    var pll = points[p];

                    Vector3 wspostion = GeoRefConversion.LatLonToUnityWorldSpace(new DVector2(pll.x, pll.y), container);

                    if (wspostion.x < pxmin) pxmin = wspostion.x;
                    if (wspostion.x > pxmax) pxmax = wspostion.x;
                    if (wspostion.z < pymin) pymin = wspostion.z;
                    if (wspostion.z > pymax) pymax = wspostion.z;

                    Points.Add(wspostion);

                }
                if (Points.Count < 2) continue;

                var road = new GISTerrainLoaderRoad(defaultRoad, container);
                road.Points = Points.ToArray();

                GameObject m_road = null;

                switch (roadType)
                {
                    case RoadGenerationType.Line:

                        m_road = GISTerrainLoaderLineGenerator.CreateLine(road);
                        m_road.transform.parent = highways.transform;
                        m_road.name = "Track" + "_" + (i + 1);


                        break;
                    case RoadGenerationType.EasyRoad3D:

                        if (EasyRoadBaseModelExist())
                        {
#if EASYROADS || EASYROADS3D
                                m_road = GISTerrainLoaderEasyRoadGenerator.CreateRoad(road, container, IsRuntime);
                                m_road.name = shapeObj.Tag + "_" + (i + 1);
                                data.Generated = true;
#endif
                        }
                        else
                        {
                            Debug.Log("EasyRoad asset not exist or main resources prefab not added to the current project.. ");
                            Debug.Log("Please Import EasyRoad and add 'EASYROADS' or 'EASYROADS3D' to the scripting Define Symbols from Player Settings plane .");
                        }

                        break;
                }
            }

#if EASYROADS || EASYROADS3D
                    if (roadType == RoadGenerationType.EasyRoad3D)
                        GISTerrainLoaderEasyRoadGenerator.Finilize();
#endif


        }
        #endregion
        private static Vector3[] FindMaxDistance(Vector3[] linePoints)
        {
            Vector3[] result = new Vector3[2];
            float max = 0;
            for (int i = 1; i < linePoints.Length; i++)
            {
                float dis = Vector3.Distance(linePoints[i - 1], linePoints[i]);
                if (dis > max)
                {
                    max = dis;
                    result[0] = linePoints[i - 1];
                    result[1] = linePoints[i];
                }

            }

            return result;
        }
        private static void CreateRoadNameLabel(Vector3[] linePoints, string roadName, Transform parent, TerrainContainerObject container)
        {
            if (linePoints.Length > 1)
            {
                int b = linePoints.Length / 2;
                int a = b - 1;
                Vector3 pointA = new Vector3(linePoints[a].x, linePoints[a].y, linePoints[a].z);
                Vector3 pointB = new Vector3(linePoints[b].x, linePoints[b].y, linePoints[b].z);

                if (Vector3.Distance(pointA, pointB) > roadName.Length * RoadLableConstants.roadNameStringSizeMultipler)
                {
                    CreateRoadNameLabel(pointA, pointB, roadName, parent, container);
                }
                else
                {
                    Vector3[] maxDis = FindMaxDistance(linePoints);

                    pointA = maxDis[0];
                    pointB = maxDis[1];

                    if (Vector3.Distance(pointA, pointB) > roadName.Length * RoadLableConstants.roadNameStringSizeMultipler)
                    {
                        CreateRoadNameLabel(pointA, pointB, roadName, parent, container);
                    }
                }
            }
        }
        private static void CreateRoadNameLabel(Vector3 pointA, Vector3 pointB, string roadName, Transform parent, TerrainContainerObject container)
        {
            GameObject text = new GameObject();
            text.transform.parent = parent.transform;
            text.name = "Road name";
            TextMesh textMesh = text.AddComponent<TextMesh>();
            textMesh.text = roadName;
            textMesh.transform.Rotate(90, 90, 0);
            textMesh.fontSize = 100;
            textMesh.characterSize = RoadLableConstants.roadNameLabelSize * container.LableScaleOverage();
            textMesh.color = RoadLableConstants.roadNameLabelColor;

            if (pointA.z < pointB.z)
            {
                var elevation = GeoRefConversion.GetHeight(text.transform.position) + 0.7f;
                text.transform.position = new Vector3(pointB.x, elevation, pointB.z);
                text.transform.LookAt(pointA);
                text.transform.Rotate(90, text.transform.rotation.y - 90.0f, 0);
            }
            else if (pointA.z > pointB.z)
            {
                var elevation = GeoRefConversion.GetHeight(text.transform.position) + 0.7f;
                text.transform.position = new Vector3(pointA.x, elevation, pointA.z);
                text.transform.LookAt(pointB);
                text.transform.Rotate(90, text.transform.rotation.y - 90.0f, 0);
            }
            else
            {
                if (pointA.x < pointB.x)
                {
                    var elevation = GeoRefConversion.GetHeight(text.transform.position) + 0.7f;
                    text.transform.position = new Vector3(pointA.x, elevation, pointA.z);
                    text.transform.LookAt(pointB);
                    text.transform.Rotate(90, text.transform.rotation.y - 90.0f, 0);
                }
                else
                {
                    var elevation = GeoRefConversion.GetHeight(text.transform.position) + 0.7f;
                    text.transform.position = new Vector3(pointB.x, elevation, pointB.z);
                    text.transform.LookAt(pointA);
                    text.transform.Rotate(90, text.transform.rotation.y - 90.0f, 0);
                }
            }
        }
        private static bool EasyRoadBaseModelExist()
        {
            bool exist = false;

            var ERNet_01 = Resources.Load("ERRoadNetwork") as GameObject;
            var ERNet_02 = Resources.Load("ER Road Network") as GameObject;


            if (ERNet_01 == null)
                exist = true;

            if (ERNet_02 == null)
                exist = true;

            return exist;
        }
        public static List<GISTerrainLoaderSO_Road> GetRoadsPrefab(RoadGenerationType roadType)
        {
            var roadsPrefab = Resources.LoadAll("Prefabs/Environment/Roads/", typeof(GISTerrainLoaderSO_Road));

            List<GISTerrainLoaderSO_Road> prefabs = new List<GISTerrainLoaderSO_Road>();

            foreach (var road in roadsPrefab)
            {
                var r = (GISTerrainLoaderSO_Road)(road as GISTerrainLoaderSO_Road);
                Material mat = null;

                if (r.MaterialType == MaterialSet.Auto)
                {

                    if (roadType == RoadGenerationType.EasyRoad3D)
                        mat = Resources.Load("Environment/Roads/Materials/ForEasyRoad3D/" + road.name, typeof(Material)) as Material;
                    if (roadType == RoadGenerationType.Line)
                    {
                        mat = Resources.Load("Environment/Roads/Materials/StandardLineRender/" + road.name, typeof(Material)) as Material;
                        if (mat) mat.SetColor("_Color", r.RoadColor);
                    }



                }

                if (r.Roadmaterial == null)
                    mat = Resources.Load("Environment/Roads/Materials/Standard", typeof(Material)) as Material;
                else
                    mat = r.Roadmaterial;


                r.Roadmaterial = mat;
                prefabs.Add(r);

            }

            return prefabs;
        }
    }
}