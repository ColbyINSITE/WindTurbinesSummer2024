/*     Unity GIS Tech 2020-2021      */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderGeoPointGenerator
    {
        private static List<GISTerrainLoaderSO_GeoPoint> PointsPrefab;
        public static List<GISTerrainLoaderSO_GeoPoint> GetPointsPrefab()
        {
            var PointsPrefab = Resources.LoadAll("Prefabs/Environment/GeoPoints/", typeof(GISTerrainLoaderSO_GeoPoint));

            List<GISTerrainLoaderSO_GeoPoint> prefabs = new List<GISTerrainLoaderSO_GeoPoint>();

            foreach (var point in PointsPrefab)
            {
                var r = (GISTerrainLoaderSO_GeoPoint)(point as GISTerrainLoaderSO_GeoPoint);
                prefabs.Add(r);
            }
            return prefabs;
        }

        public static void GenerateGeoPoint(TerrainContainerObject container, GISTerrainLoaderGeoVectorData GeoData, List<GISTerrainLoaderSO_GeoPoint> m_PointsPrefab)
        {
            PointsPrefab = m_PointsPrefab;

            GameObject highways = null;
            if (container.transform.Find("GeoPoints") == null)
            {
                highways = new GameObject();
                highways.name = "GeoPoints";
                highways.transform.parent = container.transform;
            }
            else
                highways = container.transform.Find("GeoPoints").gameObject;


            for (int i = 0; i < GeoData.GeoPoints.Count; i++)
            {
                var P = GeoData.GeoPoints[i];
                var prefab = GetPointPrefab(P.Tag);
                var point = P.GeoPoint;

                if (prefab != null)
                {
                    if(prefab.Prefab)
                    {
                        var GeoPoint = GameObject.Instantiate(prefab.Prefab, highways.transform);

                        if (GeoPoint)
                        {
                            GeoPoint.transform.position = GeoRefConversion.LatLonToUnityWorldSpace(point, container);
                            GeoPoint.name = "GeoPoint_" + P.Name;
                        }

                        var GISTerrainLoaderGeopoint = GeoPoint.GetComponent<GISTerrainLoaderGeoPoint>();
                        if (GISTerrainLoaderGeopoint)
                            GISTerrainLoaderGeopoint.SetName(P.Name);

                    }

                }
            }

        }
        private static GISTerrainLoaderSO_GeoPoint GetPointPrefab(string pointtype)
        {
            GISTerrainLoaderSO_GeoPoint point = null;
            foreach (var prefab in PointsPrefab)
            {
                if (prefab != null)
                {
                    if (prefab.GeoPointType == pointtype)
                        point = prefab;

                }
            }
            return point;
        }
        #region GPX

        public static void GenerateGeoPoint(GISTerrainLoaderGPXFileData m_GPXFileData, TerrainContainerObject container, GameObject GeoPointPrefab)
        {
            GameObject highways = null;

            if (container.transform.Find("GeoPoints") == null)
            {
                highways = new GameObject();
                highways.name = "GeoPoints";
                highways.transform.parent = container.transform;
            }
            else
                highways = container.transform.Find("GeoPoints").gameObject;


            for (int i = 0; i < m_GPXFileData.WayPoints.Count; i++)
            {
                var point = m_GPXFileData.WayPoints[i];

                if (GeoPointPrefab != null)
                {
                    var GeoPoint = GameObject.Instantiate(GeoPointPrefab, highways.transform).GetComponent<GISTerrainLoaderGeoPoint>();

                    if (GeoPoint)
                    {
                        GeoPoint.transform.position = GeoRefConversion.LatLonToUnityWorldSpace(new DVector2(point.Longitude, point.Latitude), container);
                        GeoPoint.name = "GeoPoint_" + point.Name;
                        GeoPoint.SetName(point.Name);
                    }
                }

            }

        }

        #endregion
    }
}

 