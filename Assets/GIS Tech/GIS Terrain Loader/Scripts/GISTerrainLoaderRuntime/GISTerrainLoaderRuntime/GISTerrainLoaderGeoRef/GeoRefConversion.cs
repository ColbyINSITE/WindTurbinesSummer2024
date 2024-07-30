/*     Unity GIS Tech 2020-2021      */

using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace GISTech.GISTerrainLoader
{
    public class GeoRefConversion
    {
        #region Useful API
        /// <summary>
        /// Get Real World Elevation [m] of a gameoject (in unity world space)
        /// </summary>
        /// <param name="container"Generated Terrain Container></param>
        /// <param name="ObjectPosition" GameObject Position in Unity World Space></param>
        /// <returns></returns>
        public static float GetRealWorldElevation(TerrainContainerObject container, Vector3 ObjectPosition, RealWorldElevation elevationMode = RealWorldElevation.Altitude, float Scale = 1)
        {
            float elevation = 0;
            var LatLonPos = GeoRefConversion.UnityWorldSpaceToLatLog(ObjectPosition, container);

            if (container.IncludePoint(LatLonPos))
            {
                switch (elevationMode)
                {
                    case RealWorldElevation.Elevation:
                        elevation = container.data.GetElevation(LatLonPos);
                        break;
                    case RealWorldElevation.Altitude:
                        var RWElevation = container.data.GetElevation(LatLonPos);
                        var RWSelevation = GeoRefConversion.GetRealWorldHeight(container, ObjectPosition) * Scale;
                        elevation = RWElevation + RWSelevation / 10 * container.scale.y;
                        break;
                    case RealWorldElevation.Height:
                        elevation = GeoRefConversion.GetRealWorldHeight(container, ObjectPosition) * Scale;
                        break;


                }
            }

            return elevation;
        }

        /// <summary>
        /// Get Real World Elevation [m] of a gameoject (by real world coordinates)
        /// </summary>
        /// <param name="container"Generated Terrain Container></param>
        /// <param name="ObjectPosition" GameObject Position in Unity World Space></param>
        /// <returns></returns>
        public static float GetRealWorldElevation(TerrainContainerObject container, DVector2 LatLonPos, RealWorldElevation elevationMode = RealWorldElevation.Altitude, float Scale = 1)
        {
            float elevation = 0;

            var ObjectPosition = LatLonToUnityWorldSpace(LatLonPos, container, false);

            if (container.IncludePoint(LatLonPos))
            {
                switch (elevationMode)
                {
                    case RealWorldElevation.Elevation:
                        elevation = container.data.GetElevation(LatLonPos);
                        break;
                    case RealWorldElevation.Altitude:
                        var RWElevation = container.data.GetElevation(LatLonPos);
                        var RWSelevation = GeoRefConversion.GetRealWorldHeight(container, ObjectPosition) * Scale;
                        elevation = RWElevation + RWSelevation / 10 * container.scale.y;

                        break;
                    case RealWorldElevation.Height:
                        elevation = GeoRefConversion.GetRealWorldHeight(container, ObjectPosition) * Scale;
                        break;


                }
            }

            return elevation;
        }

        /// <summary>
        /// Get Real World Elevation [m] of a gameoject (by real world coordinates)
        /// </summary>
        /// <param name="container"Generated Terrain Container></param>
        /// <param name="ObjectPosition" GameObject Position in Unity World Space></param>
        /// <returns></returns>
        public static float GetRealWorldElevation(TerrainContainerObject container, DVector2 LatLonPos)
        {
            float elevation = 0;
            elevation = container.data.GetElevation(LatLonPos);
            return elevation;
        }

        private static float GetRealWorldHeight(TerrainContainerObject container, Vector3 SpacePosition)
        {
            var PostionOnTerrain = GeoRefConversion.GetHeight(SpacePosition);

            var Diff = SpacePosition.y - PostionOnTerrain;

            var elevation = (Diff / container.scale.y) * 10;

            return elevation;


        }

        /// <summary>
        /// Convert Unity World space (X,Y,Z) coordinates to (Lat, Lon) coordinates
        /// </summary>
        /// <returns>
        /// Returns DVector2 containing Latitude and Longitude
        /// </returns>
        /// <param name='position'>
        /// (X,Y,Z) Position Parameter
        /// </param>
        public static DVector2 UnityWorldSpaceToLatLog(Vector3 position, TerrainContainerObject container)
        {
            var m_Origin = new DVector2(container.TopLeftLatLong.x, container.DownRightLatLong.y);
            GeoRefConversion.SetLocalOrigin(m_Origin);
            FindMetersPerLat(container.DownRightLatLong.y);
            DVector2 geoLocation = new DVector2(0, 0);
            geoLocation.y = (_LatOrigin + (position.z / container.scale.z) / metersPerLat);
            geoLocation.x = (_LonOrigin + (position.x / container.scale.x) / metersPerLon);
            return geoLocation;
        }

        /// <summary>
        /// Convert (Lat, Lon) coordinates to Unity World space (X,Y,Z) coordinates
        /// </summary>
        /// <returns>
        /// Returns a Vector3 containing (X, Y, Z)
        /// </returns>
        /// <param name='latlon'>
        /// (Lat, Lon) as Vector2
        /// </param>
        public static Vector3 LatLonToUnityWorldSpace(DVector2 latlon, TerrainContainerObject container, bool GetElevation = true)
        {

            if (container)
            {
                float elevation = 0;

                Vector3 UnityWorldSpacePosition = Vector3.zero;

                var TLPMercator_X = container.TLPointMercator.x;
                var TLPMercator_Y = container.TLPointMercator.y;

                var DRPMercator_X = container.DRPointMercator.x;
                var DRPMercator_Y = container.DRPointMercator.y;

                var NodeP_Merc = LatLongToMercat(latlon.x, latlon.y);

                double Offest_x = (NodeP_Merc.x - TLPMercator_X) / (DRPMercator_X - TLPMercator_X);

                double Offest_y = 1 - (NodeP_Merc.y - TLPMercator_Y) / (DRPMercator_Y - TLPMercator_Y);

                if (GetElevation)
                {
                    Vector3 HightWSPos = new Vector3((float)(container.transform.position.x + container.ContainerSize.x * Offest_x), 50000, (float)(container.ContainerSize.z * Offest_y));
                    elevation = GeoRefConversion.GetHeight(HightWSPos) + 0.7f;
                    UnityWorldSpacePosition = new Vector3((float)(container.transform.position.x + container.ContainerSize.x * Offest_x), elevation, (float)(container.ContainerSize.z * Offest_y));

                }
                else
                {
                    UnityWorldSpacePosition = new Vector3((float)(container.transform.position.x + container.ContainerSize.x * Offest_x), 0, (float)(container.ContainerSize.z * Offest_y));
                }


                return UnityWorldSpacePosition;

            }
            else
            {
                Debug.LogError("No Terrain Existing");

                return Vector3.zero;
            }

        }




        /// <summary>
        /// Set GameObject position by converting real world coordinates to unity space position (Inputs Lat/Lon position + Elevation [m])
        /// </summary>
        /// <param name="container">Generated Terrain Container</param>
        /// <param name="LatLonPos"> Real World Position</param>
        /// <param name="RWElevationValue"> Elevation in m </param>
        /// <param name="elevationMode"> Check the documentation.. </param>
        /// <returns></returns>
        public static Vector3 SetRealWorldPosition(TerrainContainerObject container, DVector2 LatLonPos, float RWElevationValue, SetElevationMode elevationMode = SetElevationMode.RelativeToSeaLevel, float Scale = 1)
        {
            Vector3 SpacePositon = Vector3.zero;

            var SpacePos = LatLonToUnityWorldSpace(LatLonPos, container);

            if (container.IncludePoint(LatLonPos))
            {
                switch (elevationMode)
                {
                    case SetElevationMode.OnTheGround:
                        var RWElevation = container.data.GetElevation(LatLonPos);
                        SpacePositon = SpacePos;
                        break;
                    case SetElevationMode.RelativeToSeaLevel:
                        RWElevation = container.data.GetElevation(LatLonPos);
                        var RWE_Diff = RWElevationValue - RWElevation;
                        var Space_Y = SpacePos.y + (RWE_Diff * container.scale.y * Scale);
                        SpacePositon = new Vector3(SpacePos.x, Space_Y, SpacePos.z);
                        break;
                    case SetElevationMode.RelativeToTheGround:
                        Space_Y = SpacePos.y + (RWElevationValue * container.scale.y * Scale);
                        SpacePositon = new Vector3(SpacePos.x, Space_Y, SpacePos.z);
                        break;

                }
            }

            return SpacePositon;
        }


        #endregion

        #region GTLAPI


        /// <summary>
        /// Convert (Lat, Lon) coordinates to Unity World space (X,Y,Z) coordinates
        /// by Using TerrainContainerObject and return with which terrain the point intersect
        /// </summary>
        /// <returns>
        /// Returns a Vector3 containing (X, Y, Z)
        /// </returns>
        /// <param name='latlon'>
        /// (Lat, Lon) as Vector2
        /// </param>
        public static Vector3 LatLonToUWS(DVector2 latlon, TerrainContainerObject container, ref TerrainObject terrain)
        {
            if (container)
            {
                var TLPMercator_X = container.TLPointMercator.x;
                var TLPMercator_Y = container.TLPointMercator.y;

                var DRPMercator_X = container.DRPointMercator.x;
                var DRPMercator_Y = container.DRPointMercator.y;

                var NodeP_Merc = LatLongToMercat(latlon.x, latlon.y);

                double Offest_x = (NodeP_Merc.x - TLPMercator_X) / (DRPMercator_X - TLPMercator_X);

                double Offest_y = 1 - (NodeP_Merc.y - TLPMercator_Y) / (DRPMercator_Y - TLPMercator_Y);

                Vector3 HightWSPos = new Vector3((float)(container.transform.position.x + container.ContainerSize.x * Offest_x), 50000, (float)(container.ContainerSize.z * Offest_y));

                var elevation = GeoRefConversion.GetHeight(HightWSPos, ref terrain) + 0.7f;

                return new Vector3((float)(container.transform.position.x + container.ContainerSize.x * Offest_x), elevation, (float)(container.ContainerSize.z * Offest_y));

            }
            else
            {
                Debug.LogError("No Terrain Existing");

                return Vector3.zero;
            }

        }

        /// <summary>
        /// Change the relative origin offset (Lat, Lon), the Default is (0,0), 
        /// used to bring a local area to (0,0,0) in UCS coordinate system
        /// </summary>
        /// <param name='localOrigin'>
        /// Referance point.
        /// </param>
        public static void SetLocalOrigin(DVector2 origine)
        {
            Origin.x = origine.x;

            Origin.y = origine.y;
        }
        private static DVector2 Origin = new DVector2(0, 0);
        private static double _LatOrigin { get { return Origin.y; } }
        private static double _LonOrigin { get { return Origin.x; } }

        private static float metersPerLat;
        private static float metersPerLon;

        private static void FindMetersPerLat(double lat)
        {
            // Compute lengths of degrees
            // Set up "Constants"
            float m1 = 111132.92f;
            float m2 = -559.82f;
            float m3 = 1.175f;
            float m4 = -0.0023f;

            float p1 = 111412.84f;
            float p2 = -93.5f;
            float p3 = 0.118f;

            lat = lat * Mathf.Deg2Rad;

            // Calculate the length of a degree of latitude and longitude in meters
            metersPerLat = m1 + (m2 * Mathf.Cos(2 * (float)lat)) + (m3 * Mathf.Cos(4 * (float)lat)) + (m4 * Mathf.Cos(6 * (float)lat));

            metersPerLon = (p1 * Mathf.Cos((float)lat)) + (p2 * Mathf.Cos(3 * (float)lat)) + (p3 * Mathf.Cos(5 * (float)lat));
        }

        /// <summary>
        /// Calculate the distance between two Lat/Log Points.
        /// </summary>
        /// <param name="lon1"></param>
        /// <param name="lat1"></param>
        /// <param name="lon2"></param>
        /// <param name="lat2"></param>
        /// <returns></returns>
        public static double Getdistance(double lat1, double lon1, double lat2, double lon2, char unit = 'K')
        {
            if ((lat1 == lat2) && (lon1 == lon2))
            {
                return 0;
            }
            else
            {
                var radlat1 = Math.PI * lat1 / 180;
                var radlat2 = Math.PI * lat2 / 180;
                var theta = lon1 - lon2;
                var radtheta = Math.PI * theta / 180;

                var dist = Math.Sin(radlat1) * Math.Sin(radlat2) + Math.Cos(radlat1) * Math.Cos(radlat2) * Math.Cos(radtheta);
                if (dist > 1)
                {
                    dist = 1;
                }
                //if (dist > 0) dist = dist * -1;
                dist = Math.Acos(dist);
                dist = dist * 180 / Math.PI;
                dist = dist * 60 * 1.1515;
                if (unit == 'K') { dist = dist * 1.609344; }
                if (unit == 'N') { dist = dist * 0.8684; }

                return dist;
            }
        }
        public static double Getdistance(DVector2 P1, DVector2 P2, char ax, char unit = 'K')
        {
            double distance = 0;

            if (ax == 'X')
            {
                if (P1.x < 0 && P2.x > 0)
                {
                    var p0 = new DVector2(0, 0);
                    var p4 = new DVector2(P2.x, 0);

                    var d1 = CalDistance(new DVector2(P1.x, 0), new DVector2(0, 0));
                    var d2 = CalDistance(new DVector2(0, 0), p4);

                    distance = d1 + d2;

                }
                else
                    distance = CalDistance(P1, P2);
            }

            if (ax == 'Y')
            {
                if (P1.y < 0 && P2.y > 0)
                {


                    var p0 = new DVector2(0, 0);
                    var p4 = new DVector2(0, P2.y);

                    var d1 = CalDistance(new DVector2(0, P1.y), new DVector2(0, 0));
                    var d2 = CalDistance(new DVector2(0, 0), p4);

                    distance = d1 + d2;

                }
                else
                    distance = CalDistance(P1, P2);
            }
            return distance;

        }
        public static double CalDistance(DVector2 P1, DVector2 P2, char unit = 'K')
        {
            if ((P1.y == P2.y) && (P1.x == P2.x))
            {
                return 0;
            }
            else
            {
                var radlat1 = Math.PI * P1.y / 180;
                var radlat2 = Math.PI * P2.y / 180;
                var theta = P2.x - P1.x;
                var radtheta = Math.PI * theta / 180;
                var dist = Math.Sin(radlat1) * Math.Sin(radlat2) + Math.Cos(radlat1) * Math.Cos(radlat2) * Math.Cos(radtheta);

                if (dist > 1)
                {
                    dist = 1;
                }
                //if (dist > 0) dist = dist * -1;
                dist = Math.Acos(dist);
                dist = dist * 180 / Math.PI;
                dist = dist * 60 * 1.1515;
                if (unit == 'K') { dist = dist * 1.609344; }
                if (unit == 'N') { dist = dist * 0.8684; }

                return dist;
            }
        }

        public const double DEG2RAD = Math.PI / 180;
        public static DVector2 LatLongToMercat(double x, double y)
        {
            double sy = Math.Sin(y * DEG2RAD);
            var mx = (x + 180) / 360;
            var my = 0.5 - Math.Log((1 + sy) / (1 - sy)) / (Math.PI * 4);

            return new DVector2(mx, my);
        }
        public static Vector3 MercatCoordsToWorld(double mx, float y, double mz, TerrainContainerObject container)
        {
            var sx = (mx - container.TLPointMercator.x) / (container.DRPointMercator.x - container.TLPointMercator.x) * container.ContainerSize.x;
            var sz = (1 - (mz - container.TLPointMercator.y) / (container.DRPointMercator.y - container.TLPointMercator.y)) * container.ContainerSize.z;
            return new Vector3((float)sx, y * container.scale.y, (float)sz);
        }


        //Get Terrain Height

        private static Terrain m_terrain;

        public static Terrain terrain
        {
            get { return m_terrain; }
            set
            {
                if (m_terrain != value)
                {
                    m_terrain = value;

                }
            }
        }
        private static TerrainObject m_terrainO;

        public static TerrainObject terrainO
        {
            get { return m_terrainO; }
            set
            {
                if (m_terrainO != value)
                {
                    m_terrainO = value;

                }
            }
        }
        public static float GetHeight(Vector3 WSposition)
        {
            float height = 0;

            terrain = GetTerrain(WSposition);

            if (terrain != null)
            {
                TerrainData t = terrain.terrainData;
                height = terrain.SampleHeight(WSposition)
                + terrain.GetPosition().y;
            }


            return height;
        }
        public static float GetHeight(Vector3 WSposition, ref TerrainObject terrainO)
        {
            float height = 0;

            terrainO = GetTerrainObject(WSposition);

            if (terrainO != null)
            {
                TerrainData t = terrainO.terrain.terrainData;
                height = terrainO.terrain.SampleHeight(WSposition)
                + terrainO.terrain.GetPosition().y;
            }


            return height;
        }
        public static Terrain GetTerrain(Vector3 WSposition)
        {
            var downDirection = Vector3.down;

            RaycastHit hitInfo;

            var ray = new Ray(WSposition, (downDirection));

            if (Physics.Raycast(ray, out hitInfo, 100000))
            {
                if (terrain == null)
                {
                    var t = hitInfo.collider.transform.gameObject.GetComponent<Terrain>();

                    if (t)
                        terrain = t;
                }

                if (terrain != null)
                {
                    if (!string.Equals(hitInfo.collider.transform.name, terrain.name))
                    {
                        if (hitInfo.collider.transform.gameObject.GetComponent<Terrain>())
                            terrain = hitInfo.collider.transform.gameObject.GetComponent<Terrain>();
                    }
                }

            }

            return terrain;
        }
        public static TerrainObject GetTerrainObject(Vector3 WSposition)
        {
            var downDirection = Vector3.down;

            RaycastHit hitInfo;

            var ray = new Ray(WSposition, (downDirection));

            if (Physics.Raycast(ray, out hitInfo, 100000))
            {
                if (terrainO == null)
                {
                    var t = hitInfo.collider.transform.gameObject.GetComponent<TerrainObject>();

                    if (t)
                        terrainO = t;
                }

                if (terrainO != null)
                {
                    if (!string.Equals(hitInfo.collider.transform.name, terrainO.name))
                    {
                        if (hitInfo.collider.transform.gameObject.GetComponent<TerrainObject>())
                            terrainO = hitInfo.collider.transform.gameObject.GetComponent<TerrainObject>();
                    }
                }

            }

            return terrainO;
        }
        //Get Raster Projection

        /// <summary>
        /// Used for ShapeFile
        /// </summary>
        /// <param name="projReader"></param>
        /// <param name="point"></param>
        /// <param name="Szone"></param>
        /// <returns></returns>
        public static DVector2 ConvertTOLatLon(GTLGeographicCoordinateSystem projReader, DVector2 point, int epsg = 0)
        {
            DVector2 LatLon = new DVector2(0, 0);

            switch (projReader.GEOGCSProjection)
                {

                case "Undefined":
#if DotSpatial
                    if (epsg != 0)
                            LatLon = GISTerrainLoaderNAD.ToLatLon(new DVector2(point.x, point.y), epsg);
                        else
                        Debug.LogError("EPSG Not Defiened ..");
#endif
                    break;
                    // Geographic(lat/lon) coordinates
                    case "GCS_WGS_1984":
                        //if (projReader.Datum.Name == "WGS84")
                        LatLon = point;
                        break;
                    // UTM Projection
                    case "GCS_North_American_1983":
                        var utmTL = new DVector2(point.x, point.y);
                        var Fullzone = projReader.Name.Split('_')[4];
                        var ZoneNum = Regex.Match(Fullzone, @"\d+").Value;
                        var ZoneL = Regex.Replace(Fullzone, @"[\d-]", string.Empty);
                        var coor = ZoneNum + " " + ZoneL + " " + utmTL.x + " " + utmTL.y;
                        GISTerrainLoaderUTM cc = new GISTerrainLoaderUTM();
                        var str1 = cc.UTMToLatLon(coor);
                        LatLon = new DVector2(str1[1], str1[0]);
                        break;
                    case "UTM":
                        utmTL = new DVector2(point.x, point.y);
                        ZoneNum = projReader.UTMData.ZoneNum.ToString();
                        ZoneL = projReader.UTMData.ZoneLet;
                        coor = ZoneNum + " " + ZoneL + " " + utmTL.x + " " + utmTL.y;
                        cc = new GISTerrainLoaderUTM();
                        str1 = cc.UTMToLatLon(coor);
                        LatLon = new DVector2(str1[1], str1[0]);
                    break;
                    // Lumbert
                    case "GCS_RESEAU_GEODESIQUE_FRANCAIS_1993":
                        var Lmb_TL = new DVector2(point.x, point.y);
                        DVector3 LatLog_TL = GISTerrainLoaderLambert.convertToWGS84Deg(Lmb_TL.x, Lmb_TL.y, projReader.LambertData.Lambertzone);
                        LatLon = new DVector2(LatLog_TL.x, LatLog_TL.y);
                        break;
                    // Mercator
                    case "merc":
                        var merc = new DVector2(point.x, point.y);
                        LatLon = merc;
                        break;
                    case "NAD83":
#if DotSpatial
                    if (projReader.UTMData != null)
                    {
                        utmTL = new DVector2(point.x, point.y);
                        ZoneNum = projReader.UTMData.ZoneNum.ToString();
                        ZoneL = projReader.UTMData.ZoneLet;
                        coor = ZoneNum + " " + ZoneL + " " + utmTL.x + " " + utmTL.y;
                        LatLon = GISTerrainLoaderNAD.Nad83ToLatLon(utmTL);
                    }
                    else
                    {
                        if (epsg == 0)
                            LatLon = GISTerrainLoaderNAD.Nad83ToLatLon(new DVector2(point.x, point.y));
                        else
                            LatLon = GISTerrainLoaderNAD.ToLatLon(new DVector2(point.x, point.y), epsg);
                    }
#else
                        //Debug.LogError("File Projected in NAD, Please add DotSpatial Lib (See Projection Section)");
#endif


                        break;
                }
          

            return LatLon;
        }
        //Get Raster Projection

        /// <summary>
        /// Convert  to (Lat, Lon) coordinates 
        /// </summary>
        /// <returns>
        /// </returns>
        /// <param name='latlon'>
        /// (Lat, Lon) as Vector2
        /// </param>
        public static DVector2 ConvertTOLatLon(GISTerrainLoaderProjectionReader projReader, DVector2 point, string Szone = "S")
        {
            DVector2 LatLon = new DVector2(0, 0);

            switch (projReader.Projection)
            {
                // Case of Geographic(lat/lon) coordinates
                case "longlat":

                    LatLon = point;
                    //if (projReader.Datum == "WGS84")
                    //{
                    //    Debug.Log(projReader.Projection + "  " + point);
                    //    LatLon = point;
                    //}
                    break;
                // Case of UTM Projection
                case "utm":
                    if (projReader.Datum == "NAD83")
                    {
                        var utmTL = new DVector2(point.x, point.y);
                        LatLon = GISTerrainLoaderNAD.Nad83ToLatLon(utmTL);
                    }
                    else
                    {

                        var utmTL = new DVector2(point.x, point.y);
                        var coor = projReader.Zone.ToString() + " " + Szone + " " + utmTL.x + " " + utmTL.y;
                        GISTerrainLoaderUTM cc = new GISTerrainLoaderUTM();
                        var str1 = cc.UTMToLatLon(coor);
                        LatLon = new DVector2(str1[1], str1[0]);
                    }
                    break;

                // Lumbert
                case "lcc":
                    var Lmb_TL = new DVector2(point.x, point.y);
                    DVector3 LatLog_TL = GISTerrainLoaderLambert.convertToWGS84Deg(Lmb_TL.x, Lmb_TL.y, LambertZone.Lambert93);
                    LatLon = new DVector2(LatLog_TL.x, LatLog_TL.y);
                    break;

                // Lumbert
                case "merc":
                    var merc = new DVector2(point.x, point.y);
                    LatLon = merc;

                    break;
            }

            return LatLon;
        }

        /// <summary>
        /// Convert Lat/Lon to Different Projection
        /// </summary>
        /// <param name="projReader"></param>
        /// <param name="point"></param>
        /// <param name="Szone"></param>
        /// <returns></returns>
        public static string ConvertLatLonTO(DVector2 LatLon, Projections proj)
        {
            string pos = " ";

            switch (proj)
            {
                case GISTerrainLoader.Projections.Geographic_LatLon_Decimale:
                    pos = LatLon.ToString();
                    break;
                case GISTerrainLoader.Projections.Geographic_LatLon_DegMinSec:
                    pos = GISTerrainLoaderGeographic.DecimalToDegMinSec(LatLon);
                    break;
                case GISTerrainLoader.Projections.UTM:
                    GISTerrainLoaderUTM utm = new GISTerrainLoaderUTM();
                    pos = utm.LatLonToUTM(LatLon);
                    break;

                case GISTerrainLoader.Projections.UTM_MGRUTM:
                    //UTM Zone 19 + Latitude Band T + MGRS column + DMGRS row J + MGRS Easting 38588 + MGRS Northing 97366
                    GISTerrainLoaderUTM MGRUTM = new GISTerrainLoaderUTM();
                    pos = MGRUTM.LatLonToMGRUTM(LatLon);
                    break;

                case GISTerrainLoader.Projections.Lambert:
                    DVector3 Lambert = GISTerrainLoaderLambert.LatLonToLambert(LatLon, LambertZone.Lambert93);
                    var p = new DVector2(Lambert.x, Lambert.y);
                    pos = p.ToString();
                    break;
                case GISTerrainLoader.Projections.UTM_Nad83:
                    pos = GISTerrainLoaderNAD.ToUTM_Nad83(LatLon);
                    break;
            }

            return pos;


        }
        #endregion


    }
}