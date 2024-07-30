/*     Unity GIS Tech 2020-2021      */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderASCILoader
    {
        public static event ReaderEvents OnReadError;

        public static event TerrainProgression OnProgress;

        public GISTerrainLoaderFileData data;

        public bool LoadComplet;

        private float cellsize;
        private float nodata_value;
        private string line;
        private int counter;
        int c = 0;

        private GTLGeographicCoordinateSystem CoordinateReferenceSystem;
        private List<float> FixedList;
        public GISTerrainLoaderASCILoader()
        {
            data = new GISTerrainLoaderFileData();
            FixedList = new List<float>();
        }

        public void LoadASCIGrid(string filepath, TerrainDimensionsMode terrainDimensionMode, FixOption fixOption = FixOption.Disable)
        {
            if (File.Exists(filepath))
            {
                try
                {
                    LoadComplet = false;

                    CoordinateReferenceSystem = new GTLGeographicCoordinateSystem("GCS_WGS_1984");

                    ReadASCIHead(filepath);

                    data.floatheightData = new float[data.mapSize_col_x, data.mapSize_row_y];

                    ReadASCIData(filepath, fixOption);

                    if (terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
                    {
                        double startLat = data.DownLeftPoint.y + (cellsize / 2.0);
                        double startLon = data.DownLeftPoint.x + (cellsize / 2.0);

                        double currentLat = startLat;
                        double currentLon = startLon;


                        data.TopLeftPoint = new DVector2(data.DownLeftPoint.x, data.DownLeftPoint.y + (cellsize * data.mapSize_row_y));
                        data.DownRightPoint = new DVector2(data.DownLeftPoint.x + (cellsize * data.mapSize_col_x), data.DownLeftPoint.y);
                        data.TopRightPoint = new DVector2(data.DownRightPoint.x, data.TopLeftPoint.y + (cellsize * data.mapSize_row_y));

                        data.DownLeftPoint = GeoRefConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.DownLeftPoint);
                        data.TopRightPoint = GeoRefConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.TopRightPoint);
                        data.TopLeftPoint = GeoRefConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.TopLeftPoint);
                        data.DownRightPoint = GeoRefConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.DownRightPoint);


                        data.Terrain_Dimension.x = GeoRefConversion.Getdistance(data.DownLeftPoint, data.DownRightPoint, 'X');
                        data.Terrain_Dimension.y = GeoRefConversion.Getdistance(data.DownLeftPoint, data.TopLeftPoint, 'Y');

                        Debug.Log("Zone Bounds : Top-Left - " + data.TopLeftPoint + " Down-Right - " + data.DownRightPoint);
                        Debug.Log("Terrain Dimensions : " + Math.Round(data.Terrain_Dimension.x, 2) + " X " + Math.Round(data.Terrain_Dimension.y, 2) + " Km ");

                    }

                    LoadComplet = true;

                }
                catch (Exception e)
                {
                    Debug.LogError("Error occured while reading ASC file!");

                    Debug.Log(e.ToString());

                    if (OnReadError != null)
                    {
                        OnReadError();
                    }
                    return;
                }


            }


        }
        public void LoadASCIGrid(DVector2 SubRegionUpperLeftCoordiante, DVector2 SubRegionDownRightCoordiante, string filepath, TerrainDimensionsMode terrainDimensionMode, FixOption fixOption = FixOption.Disable)
        {
            if (File.Exists(filepath))
            {
                try
                {
                    ReadASCIHead(filepath);

                    if (terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
                    {

                        data.TopRightPoint.x = data.DownLeftPoint.x + (cellsize * data.mapSize_col_x);
                        data.TopRightPoint.y = data.DownLeftPoint.y + (cellsize * data.mapSize_row_y);

                        data.DownLeftPoint = GeoRefConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.DownLeftPoint);
                        data.TopRightPoint = GeoRefConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.TopRightPoint);

                        data.TopLeftPoint = new DVector2(data.DownLeftPoint.x, data.TopRightPoint.y);
                        data.DownRightPoint = new DVector2(data.TopRightPoint.x, data.DownLeftPoint.y);
                    }

                    data.floatheightData = new float[data.mapSize_col_x, data.mapSize_row_y];

                    float el = 0;
                    StreamReader file = new StreamReader(filepath);

                    while ((line = file.ReadLine()) != null)
                    {
                        if (c < 6)
                        {
                            c++;
                        }
                        else
                        if (c >= 6)
                        {
                            var replacedLine = line.Replace('.', ',');

                            var floatLineList = replacedLine.Split(' ');

                            if (floatLineList.Length >= data.mapSize_row_y - 1)
                            {
                                for (int i = 0; i < floatLineList.Length; i++)
                                {
                                    if (!string.IsNullOrEmpty(floatLineList[i]))
                                        el = float.Parse(floatLineList[i]);

                                    if (el == -99999 || el == -9999)
                                        el = 0;

                                    if (fixOption == FixOption.ManualFix)
                                    {
                                        if (el < data.TerrainMaxMinElevation.x)
                                            el = data.TerrainMaxMinElevation.x;

                                        if (el > data.TerrainMaxMinElevation.y)
                                            el = data.TerrainMaxMinElevation.y;

                                    }
                                    else
                                    {
                                        if (el < data.MinElevation)
                                            data.MinElevation = el;
                                        if (el > data.MaxElevation)
                                            data.MaxElevation = el;
                                    }

                                    if (i < data.mapSize_col_x)
                                    {
                                        data.floatheightData[i, data.mapSize_row_y - (c - 6) - 1] = el;
                                        FixedList.Add(el);
                                    }
                                        
                                }
                            }
                            c++;
                        }

                    }

                    file.Close();


                    if (fixOption == FixOption.AutoFix)
                        FixTerrainData();

                        if (GISTerrainLoaderExtensions.IsSubRegionIncluded(data.TopLeftPoint, data.DownRightPoint, SubRegionUpperLeftCoordiante, SubRegionDownRightCoordiante))
                    {
                        var points = SubZone(data, SubRegionUpperLeftCoordiante, SubRegionDownRightCoordiante);

                        data.floatheightData = points;

                        if (terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
                        {
                            data.TopLeftPoint = SubRegionUpperLeftCoordiante;
                            data.DownRightPoint = SubRegionDownRightCoordiante;
                            data.DownLeftPoint = new DVector2(data.TopLeftPoint.x, data.DownRightPoint.y);
                            data.TopRightPoint = new DVector2(data.DownRightPoint.x, data.TopLeftPoint.y);


                            data.Terrain_Dimension.x = GeoRefConversion.Getdistance(data.DownLeftPoint, data.DownRightPoint, 'X');
                            data.Terrain_Dimension.y = GeoRefConversion.Getdistance(data.DownLeftPoint, data.TopLeftPoint, 'Y');

                            Debug.Log("Zone Bounds : Top-Left - " + data.TopLeftPoint + " Down-Right - " + data.DownRightPoint);
                            Debug.Log("Terrain Dimensions : " + Math.Round(data.Terrain_Dimension.x, 2) + " X " + Math.Round(data.Terrain_Dimension.y, 2) + " Km ");

                        }

                    }
                    else
                    {
                        if (OnReadError != null)
                        {
                            OnReadError();
                        }
                    }

                    LoadComplet = true;

                }
                catch (Exception e)
                {
                    Debug.LogError("Error occured while reading ASC file!");

                    Debug.Log(e.ToString());

                    if (OnReadError != null)
                    {
                        OnReadError();
                    }
                    return;
                }
            }
        }
        private void ReadASCIHead(string filepath)
        {
            StreamReader file = new StreamReader(filepath);

            while ((line = file.ReadLine()) != null)
            {
                if (counter < 6)
                {
                    string[] lineTemp = line.Split(' ');

                    switch (lineTemp[0])
                    {
                        case "ncols":
                            data.mapSize_col_x = int.Parse(lineTemp[lineTemp.Length - 1]);
                            break;
                        case "nrows":
                            data.mapSize_row_y = int.Parse(lineTemp[lineTemp.Length - 1]);
                            break;
                        case "xllcorner":
                            data.DownLeftPoint.x = float.Parse(lineTemp[lineTemp.Length - 1].Replace('.', ','));
                            break;
                        case "yllcorner":
                            data.DownLeftPoint.y = float.Parse(lineTemp[lineTemp.Length - 1].Replace('.', ','));
                            break;
                        case "cellsize":
                            cellsize = float.Parse(lineTemp[lineTemp.Length - 1].Replace('.', ','));
                            break;
                    }


                    counter++;
                }

            }

            file.Close();

            ReadProjection(filepath);

        }
        private void ReadASCIData(string filepath, FixOption fixOption)
        {
            float el = 200;

            StreamReader file = new StreamReader(filepath);

            while ((line = file.ReadLine()) != null)
            {
                if (c < 6)
                {
                    c++;
                }
                else
                if (c >= 6)
                {
                    var replacedLine = line.Replace('.', ',');

                    var floatLineList = replacedLine.Split(' ');

                    if (floatLineList.Length >= data.mapSize_row_y - 1)
                    {
                        for (int i = 0; i < floatLineList.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(floatLineList[i]))
                                el = float.Parse(floatLineList[i]);

                            if (el == -99999 || el == -9999)
                                el = 0;

                            if (fixOption == FixOption.ManualFix)
                            {
                                if (el < data.TerrainMaxMinElevation.x)
                                    el = (ushort)data.TerrainMaxMinElevation.x;

                                if (el > data.TerrainMaxMinElevation.y)
                                    el = (ushort)data.TerrainMaxMinElevation.y;

                            }
                            else
                            {
                                if (el < data.MinElevation)
                                    data.MinElevation = el;
                                if (el > data.MaxElevation)
                                    data.MaxElevation = el;
                            }

                            if (i < data.mapSize_col_x)
                            {
                                data.floatheightData[i, data.mapSize_row_y - (c - 6) - 1] = el;
                                FixedList.Add(el);
                            }
                               

                           

                            if (OnProgress != null)
                                OnProgress("Loading File ", i * c * 100 / (data.mapSize_row_y * data.mapSize_col_x));


                        }
                    }
                    c++;
                }

            }

            file.Close();


            if (fixOption == FixOption.AutoFix)
                FixTerrainData();
        }
        private float[,] SubZone(GISTerrainLoaderFileData data, DVector2 SubTopLeft, DVector2 SubDownRight)
        {
            var rang_x = Math.Abs(Math.Abs(data.DownRightPoint.x) - Math.Abs(data.TopLeftPoint.x));
            var rang_y = Math.Abs(Math.Abs(data.TopLeftPoint.y) - Math.Abs(data.DownRightPoint.y));

            var Subrang_x = Math.Abs(Math.Abs(SubDownRight.x) - Math.Abs(SubTopLeft.x));
            var Subrang_y = Math.Abs(Math.Abs(SubTopLeft.y) - Math.Abs(SubDownRight.y));

            int submapSize_col_x = (int)(Subrang_x * data.mapSize_col_x / rang_x);
            int submapSize_row_y = (int)(Subrang_y * data.mapSize_row_y / rang_y);

            var StartLocation = GISTerrainLoaderExtensions.GetLocalLocation(data, SubTopLeft);
            var EndLocation = GISTerrainLoaderExtensions.GetLocalLocation(data, SubDownRight);

            float[,] SubZone = new float[submapSize_col_x, submapSize_row_y];

            for (int x = (int)StartLocation.x; x < (int)EndLocation.x - 1; x++)
            {
                for (int y = (int)StartLocation.y; y < (int)EndLocation.y - 1; y++)
                {
                    int Step_X = x - 1 - ((int)StartLocation.x - 1);
                    int Step_Y = y - 1 - ((int)StartLocation.y - 1);

                    var el = data.floatheightData[x, data.mapSize_row_y - (y) - 1];

                    if (el > -9900)
                    {
                        if (el < data.MinElevation)
                            data.MinElevation = el;
                        if (el > data.MaxElevation)
                            data.MaxElevation = el;
                    }

                    if (OnProgress != null)
                    {
                        OnProgress("Loading File ", Step_X * Step_Y * 100 / (submapSize_col_x * submapSize_row_y));
                    }

                    SubZone[Step_X, submapSize_row_y - Step_Y - 1] = el;

                }

            }
            data.mapSize_col_x = submapSize_col_x;
            data.mapSize_row_y = submapSize_row_y;

            return SubZone;

        }
        public void ReadProjection(string path)
        {
            string prjFile = path.Replace(Path.GetExtension(path), ".prj");

            if (File.Exists(prjFile))
                CoordinateReferenceSystem = GISTerrainLoaderProjectionReader.ReadProjectionFile(path);
        }
        private void FixTerrainData()
        {
            var orderdDown = FixedList.OrderBy(x => x).ToList();
            for (int i = 0; i < orderdDown.Count; i++)
            {
                var el = orderdDown[i];
                if (el > -9999)
                {
                    data.MinElevation = el;
                    break;
                }
            }

            for (int i = 0; i < data.floatheightData.GetLength(0); i++)
            {
                for (int j = 0; j < data.floatheightData.GetLength(1); j++)
                {
                    var el = data.floatheightData[i, j];

                    if (el == -9999)
                    {
                        data.floatheightData[i, j] = (data.MinElevation + ((data.MaxElevation - data.MinElevation) / 2));

                    }

                }
            }
        }
    }
}