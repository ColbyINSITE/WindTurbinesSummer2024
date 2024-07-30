/*     Unity GIS Tech 2020-2021      */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if DotSpatial
using DotSpatial.Projections;
#endif

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderNAD  
    {
        public static string ToUTM_Nad83(DVector2 LatLon)
        {
#if DotSpatial
            ProjectionInfo source = ProjectionInfo.FromEpsgCode(4326);
            ProjectionInfo To = ProjectionInfo.FromEpsgCode(26915);

            double[] xy = new double[2] { LatLon.x, LatLon.y };
            double[] z = new double[] { 1 };

            Reproject.ReprojectPoints(xy, z, source, To, 0, 1);


            var Easting = xy[0];
            var Northing = xy[1];

            return (Easting).ToString() + "  -  " + (Northing).ToString(); 
#else
            return (LatLon.x).ToString() + "  -  " + (LatLon.y).ToString();
#endif
        }
        public static DVector2 Nad83ToLatLon(DVector2 Nad,bool useoffest =false)
        {
#if DotSpatial
            ProjectionInfo source = ProjectionInfo.FromEpsgCode(26915);

            ProjectionInfo To = ProjectionInfo.FromEpsgCode(4326);

            var Error_offest = new Vector3(0f, 0f);
            if (useoffest)
             Error_offest = new Vector3(-2.1f, +1.65f);

            double[] xy = new double[2] { (Nad.x+ Error_offest.x), (Nad.y+ Error_offest.y) };
            double[] z = new double[] { 1 };


            Reproject.ReprojectPoints(xy, z, source, To, 0, 1);


            var Lat_Easting = xy[0];
            var Lon_Northing = xy[1];

            return new DVector2(Lat_Easting, Lon_Northing);
#else
            return new DVector2(Nad.x, Nad.y);
#endif
        }

        /// <summary>
        /// Convert Coordinates to Lat Lon using DotSpatial Lib and EPSG Code
        /// </summary>
        /// <param name="Nad"></param>
        /// <param name="Epsg"></param>
        /// <param name="useoffest"></param>
        /// <returns></returns>
        public static DVector2 ToLatLon(DVector2 Nad, int Epsg, bool useoffest = false)
        {
#if DotSpatial
            ProjectionInfo source = ProjectionInfo.FromEpsgCode(Epsg);

            ProjectionInfo To = ProjectionInfo.FromEpsgCode(4326);

            var Error_offest = new Vector3(0f, 0f);
            if (useoffest)
                Error_offest = new Vector3(-2.1f, +1.65f);

            double[] xy = new double[2] { (Nad.x + Error_offest.x), (Nad.y + Error_offest.y) };
            double[] z = new double[] { 1 };


            Reproject.ReprojectPoints(xy, z, source, To, 0, 1);
       

            var Lat_Easting = xy[0];
            var Lon_Northing = xy[1];

            return new DVector2(Lat_Easting, Lon_Northing);
#else
            return new DVector2(Nad.x, Nad.y);
#endif
        }
    }
}
