/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderGeographic
    {
        public static string DecimalToDegMinSec(DVector2 LatLon)
        {
            var Lon = DDtoDMS(LatLon.x, "longitude");
            var Lat = DDtoDMS(LatLon.y, "latitude");

            return Lat + "  " + Lon; 
        }
        private static string DDtoDMS(double coordinate, string type)
        {
            bool neg = coordinate < 0d;

            coordinate = Math.Abs(coordinate);

            double d = Math.Floor(coordinate);
            coordinate -= d;
            coordinate *= 60;
            double m = Math.Floor(coordinate);
            coordinate -= m;
            coordinate *= 60;
            double s = Math.Round(coordinate);

            char pad;
            char.TryParse("0", out pad);

            string dd = d.ToString();
            string mm = m.ToString().PadLeft(2, pad);
            string ss = s.ToString().PadLeft(2, pad);

            string dms = string.Format("{0}°{1}'{2}\"", dd, mm, ss);

            switch (type)
            {
                case "longitude":
                    dms += neg ? "W" : "E";
                    break;
                case "latitude":
                    dms += neg ? "S" : "N";
                    break;
            }

            return dms;
        }
    }
}
