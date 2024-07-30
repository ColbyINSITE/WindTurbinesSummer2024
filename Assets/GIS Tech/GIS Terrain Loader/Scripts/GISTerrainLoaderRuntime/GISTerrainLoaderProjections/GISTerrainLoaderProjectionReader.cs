/*     Unity GIS Tech 2020-2021      */

using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

#if GISTerrainLoaderPdal
using pdal;
#endif

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderProjectionReader
    {
#if GISTerrainLoaderPdal
        public static event ReaderEvents OnReadError;

        private string JsonName = "RasterProjReader";
#endif


        private string SourceFile;

        public string Projection;
        public int ESPGCode;
        public int Zone;
        public string Datum;
        public string Ellps;
        public string Unite;
        public string ZoneLettre;

        public DVector2 TP_LatLon;
        public DVector2 DR_LatLon;

#if GISTerrainLoaderPdal
         public GISTerrainLoaderProjectionReader(string RasterPath, PdalRader type)
        {
            SourceFile = RasterPath;
            GenerateJson(type);
            LoadJsonData();
        }
        public GISTerrainLoaderProjectionReader()
        {
        }
        public void GenerateJson(PdalRader type)
        {
            string Pdalpipeline = "";
            if (type == PdalRader.Raster)
            {
                Pdalpipeline =
                   "{"
                       + "\n"
                       + "\"" + "pipeline" + "\"" + ":["
                       + "\n"
                    + "{"

                    + "\n"
                          + "\"filename\"" + ":" + "\"" + SourceFile.Replace(@"\", "/") + "\"" + ","

                          + "\n"

                          + "\"type\"" + ":" + "\"" + "readers.gdal" + "\""


                         + "\n"

                         + "}"

                         + "\n"

                         + "]"

                         + "\n"

                        + "}";
            }
            else
                if (type == PdalRader.LAS)
            {
                Pdalpipeline =
                   "{"
                       + "\n"
                       + "\"" + "pipeline" + "\"" + ":["
                       + "\n"
                    + "{"

                    + "\n"
                          + "\"filename\"" + ":" + "\"" + SourceFile.Replace(@"\", "/") + "\"" + ","

                          + "\n"

                          + "\"type\"" + ":" + "\"" + "readers.las" + "\""


                         + "\n"

                         + "}"

                         + "\n"

                         + "]"

                         + "\n"

                        + "}";
            }

            //Save Json
            var Jsonpath = Path.Combine(Application.persistentDataPath, JsonName);
            File.WriteAllText(Jsonpath, Pdalpipeline);

        }
        private void LoadJsonData()
        {
            try
            {
                //Load Json
                var Jsonpath = Path.Combine(Application.persistentDataPath, JsonName);
                string jsonFile = File.ReadAllText(Jsonpath);

                if (!string.IsNullOrEmpty(jsonFile))
                {
                    pdal.Config config = new pdal.Config();

                    if (Application.isPlaying && !Application.isEditor)
                    {
                        var PluginsPath = Application.dataPath + "/Plugins";
                        config.GdalData = PluginsPath + "/Lidar/gdal/Data";
                        config.Proj4Data = PluginsPath + "/Lidar/proj4/Data";
                    }
                    else
                    {
                        config.GdalData = Path.Combine(Application.dataPath, "GIS Tech/GIS Terrain Loader/Plugins/Lidar/gdal/Data");
                        config.Proj4Data = Path.Combine(Application.dataPath, "GIS Tech/GIS Terrain Loader/Plugins/Lidar/proj4/Data");

                    }

                    pdal.Pipeline pipeline = new pdal.Pipeline(jsonFile);

                    long TotalPipelinepointCount = pipeline.Execute();

                    //Read Header  + MetaData 

                    ReadLasMetaData(pipeline);

                    pipeline.Dispose();

                    if (TotalPipelinepointCount == 0)
                    {
                        Debug.LogError("File Not Valid ... ");

                        OnReadError();
                    }

                }


            }
            catch (Exception ex)
            {
                Debug.Log("Couldn't Load Terrain file: " + ex.Message + "  " + Environment.NewLine);

                OnReadError();
            };



        }
        private void ReadLasMetaData(pdal.Pipeline pipeline)
        {
            PointViewIterator views = pipeline.Views;
            PointView view = views != null ? views.Next : null;

            while (view != null)
            {
                var viewId = "View " + view.Id;
                var tproj4 = "\tproj4: " + view.Proj4;
                var tWKT = "\tWKT: " + view.Wkt;

                var ProjData = view.Proj4.Split('+');

                foreach (var line in ProjData)
                {
                    var linedata = line.Split('=');
                    if (linedata.Count() > 1)
                    {
                        var key = linedata[0];
                        var value = linedata[1].Trim();

                        if (key == "proj")
                            Projection = value.Trim();
                        if (key == "datum")
                            Datum = value;
                        if (key == "ellps")
                            Ellps = value;
                        if (key == "zone")
                            Zone = int.Parse(value);
                        if (key == "units")
                            Unite = value;


                    }

                }
                view.Dispose();
                view = views.Next;

            }

            if (views != null)
            {
                views.Dispose();
            }
        }
#endif

        /// <summary>
        /// Read *.Prj file
        /// </summary>
        /// <param name="prjFile">Full path for the prj file</param>
        public static GTLGeographicCoordinateSystem ReadProjectionFile(string PathFile)
        {
            GTLGeographicCoordinateSystem CoordinateReferenceSystem = null;

            string prjFile = PathFile.Replace(Path.GetExtension(PathFile), ".prj");

            if (string.IsNullOrEmpty(prjFile))
                throw new ArgumentNullException("prjFile");

            // Prj file is optional so ignore if it doesn't exist
            if (!File.Exists(prjFile))
            {
                return null;
            }
            GTLGeographicCoordinateSystem gcs = new GTLGeographicCoordinateSystem();
            GTLProjection prj = new GTLProjection();

            System.IO.TextReader tr = new StreamReader(prjFile);
            string prjContent = tr.ReadLine();

            Regex PROJCS = new Regex("(?:PROJCS\\[\")(?<PRJName>.*)(?:\",GEOGCS\\[\")(?<CRSName>.*" +
      ")(?:\",DATUM\\[\")(?<DatumName>.*)(?:\",SPHEROID\\[\")(?<Sph" +
      "eroidName>.*)(?:\",)(?<InverseFlatteningRatio>.*)(?:,)(?<Ax" +
      "is>.*)(?:\\]\\])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);


            Regex GEOGCS = new Regex("(?:GEOGCS\\[\")(?<CRSName>.*" +
      ")(?:\",DATUM\\[\")(?<DatumName>.*)(?:\",SPHEROID\\[\")(?<Sph" +
      "eroidName>.*)(?:\",)(?<InverseFlatteningRatio>.*)(?:,)(?<Ax" +
      "is>.*)(?:\\]\\])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

            if (PROJCS.IsMatch(prjContent))
            {
                Match m = PROJCS.Match(prjContent);
                gcs.Name = m.Groups["PRJName"].Value;
                gcs.GEOGCSProjection = m.Groups["CRSName"].Value;
                GTLRefDatum rd = new GTLRefDatum();
                rd.Name = m.Groups["DatumName"].Value;
                gcs.Datum = rd;
                String[] prjParamsArray = m.Groups["Parameters"].Value.Split(new Char[] { ',' });
                List<GTLProjectionParameter> prjParams = new List<GTLProjectionParameter>();
                for (int i = 0; i < prjParamsArray.Length - 1; i++)
                {
                    prjParams.Add(new GTLProjectionParameter(prjParamsArray[i].Replace("PARAMETER[", "").Replace("\"", ""), double.Parse(prjParamsArray[i + 1].Replace("]", ""), CultureInfo.InvariantCulture)));
                    i++;
                }
                prj.Parameters = prjParams;

                gcs.projectionData = prj;
            }
            else if (GEOGCS.IsMatch(prjContent))
            {
                Match m = GEOGCS.Match(prjContent);
                gcs.GEOGCSProjection = m.Groups["CRSName"].Value;
                GTLRefDatum rd = new GTLRefDatum();
                rd.Name = m.Groups["DatumName"].Value;
                gcs.Datum = rd;
                List<GTLProjectionParameter> prjParams = new List<GTLProjectionParameter>();
                prj.Parameters = prjParams;
                gcs.projectionData = prj;
            }

            tr.Close();

                

            CoordinateReferenceSystem = gcs;

            if (CoordinateReferenceSystem.Name.Contains("UTM"))
            {
                UTMCase(CoordinateReferenceSystem);
                CoordinateReferenceSystem.GEOGCSProjection = "UTM";
            }

            return CoordinateReferenceSystem;
        }
        private static void UTMCase(GTLGeographicCoordinateSystem CoordinateReferenceSystem)
        {
            CoordinateReferenceSystem.UTMData = new GTLUTMdata();

            var Sub = CoordinateReferenceSystem.Name.Split('_');

            if (Sub.Length > 0)
            {
                var full = Sub[4];
                var zoneLet = Regex.Replace(full, @"[^A-Z]+", string.Empty);
                CoordinateReferenceSystem.UTMData.ZoneLet = zoneLet;
                var zonenum = Regex.Match(full, @"\d+").Value;
                CoordinateReferenceSystem.UTMData.ZoneNum = int.Parse(zonenum);
                return;
            }
            CoordinateReferenceSystem.GEOGCSProjection = "UTM";
        }

        public static GTLGeographicCoordinateSystem SetCustomProjection(FileProjection fileProjection, int epsg=0)
        {
            GTLGeographicCoordinateSystem CoordinateReferenceSystem = new GTLGeographicCoordinateSystem();

            CoordinateReferenceSystem.GEOGCSProjection = GetProjName(fileProjection);
            if (epsg != 0)
                CoordinateReferenceSystem.EPSG_Code = epsg;

            if (CoordinateReferenceSystem.Name.Contains("UTM"))
            {
                UTMCase(CoordinateReferenceSystem);
                CoordinateReferenceSystem.GEOGCSProjection = "UTM";
            }

            return CoordinateReferenceSystem;
        }

        private static string GetProjName(FileProjection proj)
        {
            string pr = "";
            switch (proj)
            {
                case FileProjection.Undefined:
                    pr= "Undefined";
                    break;
                case FileProjection.Geographic_lat_lon:
                    pr = "GCS_WGS_1984";
                    break;
                case FileProjection.UTM:
                    pr = "UTM";
                    break;
                case FileProjection.Mercator:
                    pr = "GCS_RESEAU_GEODESIQUE_FRANCAIS_1993";
                    break;
                case FileProjection.NAD83:
                    pr = "NAD83";
                    break;

            }
            return pr;
        }
    }

    
    public enum FileProjection
    {
        Undefined = 0,
        CustomWithEPSG,
        Geographic_lat_lon,
        UTM,
        Lumbert,
        Mercator,
        NAD83
    }
}