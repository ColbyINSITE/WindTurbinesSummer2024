/*     Unity GIS Tech 2020-2021      */
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderSupport
    {
        public static string[] SupportedDEMFiles = new string[] { ".flt",".bin",".tif", ".bil", ".hgt", ".asc", ".las", ".ter", ".png"
        ,".raw"};
        public static string[] GeoFile = new string[] { ".flt", ".bin", ".tif", ".bil", ".hgt", ".asc", ".las" };

        public static bool IsValidTerrainFile(string filepath)
        {
            var ext = Path.GetExtension(filepath);

            bool valid = false;

            if (GISTerrainLoaderSupport.SupportedDEMFiles.Contains(ext))
            {
                valid = true;
            }

            if (!valid)
                Debug.LogError("DEM File not supprted, try another one ");

            return valid;
        }
        public static bool IsGeoFile(string ext)
        {
            bool valid = false;

            if (GISTerrainLoaderSupport.GeoFile.Contains(ext))
            {
                valid = true;
            }
            return valid;
        }
    }
}
