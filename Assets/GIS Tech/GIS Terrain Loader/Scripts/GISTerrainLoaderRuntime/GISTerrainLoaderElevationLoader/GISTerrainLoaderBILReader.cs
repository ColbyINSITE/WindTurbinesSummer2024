/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
 
    public class GISTerrainLoaderBILReader
    {
        public static event ReaderEvents OnReadError;

        public static event TerrainProgression OnProgress;

        public GISTerrainLoaderFileData data;

        public bool LoadComplet;

        private GTLGeographicCoordinateSystem CoordinateReferenceSystem;
        private List<float> FixedList;
        public GISTerrainLoaderBILReader()
        {
            data = new GISTerrainLoaderFileData();
            FixedList = new List<float>();
        }

        public void LoadFloatGrid(string filepath, TerrainDimensionsMode terrainDimensionMode, FixOption fixOption = FixOption.Disable)
        {
            LoadComplet = false;

            var hdrpath = Path.ChangeExtension(filepath, ".hdr");

            if (!File.Exists(filepath))
            {
                Debug.LogError("Please select a Band interleaved by line (BIL) file.");

                if (OnReadError != null)
                {
                    OnReadError();
                }

                return;
            }

            if (File.Exists(hdrpath))
            {
 
                CoordinateReferenceSystem = new GTLGeographicCoordinateSystem("GCS_WGS_1984");

                StreamReader hdrReader = new StreamReader(hdrpath);

                string hdrTemp = null;

                hdrTemp = hdrReader.ReadLine();

                while (hdrTemp != null)
                {
                    int spaceStart = hdrTemp.IndexOf(" ");
                    int spaceEnd = hdrTemp.LastIndexOf(" ");

                    hdrTemp = hdrTemp.Remove(spaceStart, spaceEnd - spaceStart);

                    string[] lineTemp = hdrTemp.Split(" "[0]);

                    switch (lineTemp[0])
                    {
                        case "NROWS":
                            data.mapSize_row_y = Int32.Parse(lineTemp[1]);
                            break;
                        case "NCOLS":
                            data.mapSize_col_x = Int32.Parse(lineTemp[1]);
                            break;
                        case "ULXMAP":
                            data.TopLeftPoint.x = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                            break;
                        case "ULYMAP":
                            data.TopLeftPoint.y = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                            break;
                        case "XDIM":
                            data.dim.x = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                            break;
                        case "YDIM":
                            data.dim.y = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                            break;
                    }
                    hdrTemp = hdrReader.ReadLine();
                }

                ReadProjection(filepath);

                if (data.cellsize ==0)
                {
                    data.DownRightPoint.x = data.TopLeftPoint.x + (data.dim.x * data.mapSize_col_x);
                    data.DownRightPoint.y = data.TopLeftPoint.y - (data.dim.y * data.mapSize_row_y);
                }else
                {
                    data.DownRightPoint.x = data.TopLeftPoint.x + (data.cellsize * data.mapSize_col_x);
                    data.DownRightPoint.y = data.TopLeftPoint.y - (data.cellsize * data.mapSize_row_y);
                }

                data.TopLeftPoint = GeoRefConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.TopLeftPoint);
                data.DownRightPoint = GeoRefConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.DownRightPoint);

                data.TopRightPoint.x = data.DownRightPoint.x;
                data.TopRightPoint.y = data.TopLeftPoint.y;

                data.DownLeftPoint.x = data.TopLeftPoint.x;
                data.DownLeftPoint.y = data.DownRightPoint.y;

                data.Terrain_Dimension.x = GeoRefConversion.Getdistance(data.DownLeftPoint, data.DownRightPoint, 'X');
                data.Terrain_Dimension.y = GeoRefConversion.Getdistance(data.DownLeftPoint, data.TopLeftPoint, 'Y');

                Debug.Log("Zone Bounds : Top-Left - " + data.TopLeftPoint + " Down-Right - " + data.DownRightPoint);
                Debug.Log("Terrain Dimensions : " + Math.Round(data.Terrain_Dimension.x, 2) + " X " + Math.Round(data.Terrain_Dimension.y, 2) + " Km ");
            }
            else
            {
                Debug.LogError("The header (HDR) file is missing.");

                if (OnReadError != null)
                {
                    OnReadError();
                }
                return;
            }

            if (File.Exists(filepath))
            {
                var bytes = File.ReadAllBytes(filepath);

                data.floatheightData = new float[data.mapSize_col_x, data.mapSize_row_y];

                    try
                {
                    for (int i = 0; i < data.mapSize_row_y; i++)
                    {


                        for (int j = 0; j < data.mapSize_col_x; j++)
                        {

                         
                            var el = BitConverter.ToUInt16(bytes, i * data.mapSize_col_x * 2 + j * 2);

                            if (fixOption == FixOption.ManualFix)
                            {
                                if (el < data.TerrainMaxMinElevation.x)
                                   el =  (ushort)data.TerrainMaxMinElevation.x;

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
                            var elS = Convert.ToSingle(el);
                            data.floatheightData[j, data.mapSize_row_y - i - 1] = elS;
                            FixedList.Add(elS);

                        }
                        if (OnProgress != null)
                        {
                            OnProgress("Loading File ", i * 100 / (data.mapSize_row_y));
                        }
                    }

                    if (fixOption == FixOption.AutoFix)
                        FixTerrainData();

                    LoadComplet = true;
                }
                catch (Exception ex)
                {
                     Debug.LogError(ex.Message + Environment.NewLine);
                    if (OnReadError != null)
                    {
                        OnReadError();
                    }

                };

            }
            else
            {
                Debug.Log("File not found!");
                return;
            }

        }
        public void LoadFloatGrid(DVector2 SubRegionUpperLeftCoordiante, DVector2 SubRegionDownRightCoordiante, string filepath, TerrainDimensionsMode terrainDimensionMode, FixOption fixOption = FixOption.Disable)
        {
            LoadComplet = false;

            var hdrpath = Path.ChangeExtension(filepath, ".hdr");

            if (!File.Exists(filepath))
            {
                Debug.LogError("Please select a Band interleaved by line (BIL) file.");

                if (OnReadError != null)
                {
                    OnReadError();
                }

                return;
            }

            if (File.Exists(hdrpath))
            {
                StreamReader hdrReader = new StreamReader(hdrpath);

                string hdrTemp = null;

                hdrTemp = hdrReader.ReadLine();

                while (hdrTemp != null)
                {
                    int spaceStart = hdrTemp.IndexOf(" ");
                    int spaceEnd = hdrTemp.LastIndexOf(" ");

                    hdrTemp = hdrTemp.Remove(spaceStart, spaceEnd - spaceStart);

                    string[] lineTemp = hdrTemp.Split(" "[0]);

                    switch (lineTemp[0])
                    {
                        case "NROWS":
                            data.mapSize_row_y = Int32.Parse(lineTemp[1]);
                            break;
                        case "NCOLS":
                            data.mapSize_col_x = Int32.Parse(lineTemp[1]);
                            break;
                        case "ULXMAP":
                            data.TopLeftPoint.x = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                            break;
                        case "ULYMAP":
                            data.TopLeftPoint.y = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                            break;
                        case "XDIM":
                            data.dim.x = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                            break;
                        case "YDIM":
                            data.dim.y = GISTerrainLoaderExtensions.ConvertToDouble(lineTemp[1]);
                            break;
                    }
                    hdrTemp = hdrReader.ReadLine();
                }
                if (terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
                {
                    ReadProjection(filepath);

                    if (data.cellsize == 0)
                    {
                        data.DownRightPoint.x = data.TopLeftPoint.x + (data.dim.x * data.mapSize_col_x);
                        data.DownRightPoint.y = data.TopLeftPoint.y - (data.dim.y * data.mapSize_row_y);
                    }
                    else
                    {
                        data.DownRightPoint.x = data.TopLeftPoint.x + (data.cellsize * data.mapSize_col_x);
                        data.DownRightPoint.y = data.TopLeftPoint.y - (data.cellsize * data.mapSize_row_y);
                    }

                    data.TopLeftPoint = GeoRefConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.TopLeftPoint);
                    data.DownRightPoint = GeoRefConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.DownRightPoint);

                    data.TopRightPoint.x = data.DownRightPoint.x;
                    data.TopRightPoint.y = data.TopLeftPoint.y;

                    data.DownLeftPoint.x = data.TopLeftPoint.x;
                    data.DownLeftPoint.y = data.DownRightPoint.y;
                }
                    


 
            }
            else
            {
                Debug.LogError("The header (HDR) file is missing.");

                if (OnReadError != null)
                {
                    OnReadError();
                }
                return;
            }
            if (File.Exists(filepath))
            {
                var bytes = File.ReadAllBytes(filepath);
                data.floatheightData = new float[data.mapSize_col_x, data.mapSize_row_y];

                for (int i = 0; i < data.mapSize_row_y; i++)
                {
                    for (int j = 0; j < data.mapSize_col_x; j++)
                    {
                        var el = BitConverter.ToUInt16(bytes, i * data.mapSize_col_x * 2 + j * 2);

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

                        var elm = Convert.ToSingle(el);
                        data.floatheightData[j, i] = elm;
                        FixedList.Add(elm);
                        if (OnProgress != null)
                        {
                            OnProgress("Loading File ", i * j * 100 / (data.mapSize_row_y * data.mapSize_col_x));
                        }
                        

                    }
                }

                if (fixOption == FixOption.AutoFix)
                    FixTerrainData();

                if (GISTerrainLoaderExtensions.IsSubRegionIncluded(data.TopLeftPoint,data.DownRightPoint, SubRegionUpperLeftCoordiante, SubRegionDownRightCoordiante))
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
            else
            {
                Debug.Log("File not found!");
                return;
            }

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

            for (int x = (int)StartLocation.x; x < (int)EndLocation.x-1; x++)
            {
                for (int y = (int)StartLocation.y; y < (int)EndLocation.y-1; y++)
                {
                    int Step_X = x-1 - ((int)StartLocation.x - 1);
                    int Step_Y = y-1 - ((int)StartLocation.y - 1);

                    var el = data.floatheightData[x, y];

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

                    SubZone[ Step_X,submapSize_row_y - Step_Y - 1 ] = el;

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