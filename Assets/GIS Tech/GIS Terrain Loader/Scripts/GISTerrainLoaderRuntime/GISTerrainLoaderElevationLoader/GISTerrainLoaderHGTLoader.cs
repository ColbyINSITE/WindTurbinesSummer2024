/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{

    public class GISTerrainLoaderHGTLoader 
    {
        public static event ReaderEvents OnReadError;

        public static event TerrainProgression OnProgress;

        public GISTerrainLoaderFileData data;

        public bool LoadComplet;
        private List<float> FixedList;
        public GISTerrainLoaderHGTLoader()
        {
            data = new GISTerrainLoaderFileData();
            FixedList = new List<float>();
        }
        public void LoadFloatGrid(string filepath, FixOption fixOption = FixOption.Disable)
        {
            LoadComplet = false;
            string filename = Path.GetFileNameWithoutExtension(filepath).ToLower();
            string[] fileCoordinate = filename.Split(new[] { 'e', 'w' });
            if (fileCoordinate.Length != 2)
                throw new ArgumentException("Invalid filename.", filepath);

            fileCoordinate[0] = fileCoordinate[0].TrimStart(new[] { 'n', 's' });
            var Latitude = int.Parse(fileCoordinate[0]);
            data.DownLeftPoint.y = Latitude;
            if (filename.Contains("s"))
                data.DownLeftPoint.y *= -1;

            var Longitude = int.Parse(fileCoordinate[1]);
            data.DownLeftPoint.x = Longitude;
            if (filename.Contains("w"))
                data.DownLeftPoint.x *= -1;

            var HgtData = File.ReadAllBytes(filepath);

            switch (HgtData.Length)
            {
                case 1201 * 1201 * 2:
                    data.mapSize_col_x = data.mapSize_row_y = 1201;
                    break;
                case 3601 * 3601 * 2:
                    data.mapSize_col_x = data.mapSize_row_y = 3601;
                    break;
                default:
                    throw new ArgumentException("Invalid file size.", filepath);
            }

            data.TopRightPoint.x = data.DownLeftPoint.x + 1;
            data.TopRightPoint.y = data.DownLeftPoint.y + 1;

            data.TopLeftPoint = new DVector2(data.DownLeftPoint.x, data.TopRightPoint.y);
            data.DownRightPoint = new DVector2(data.TopRightPoint.x, data.DownLeftPoint.y);

            data.Terrain_Dimension.x = GeoRefConversion.Getdistance(data.DownLeftPoint, data.DownRightPoint, 'X');
            data.Terrain_Dimension.y = GeoRefConversion.Getdistance(data.DownLeftPoint, data.TopLeftPoint, 'Y');

            Debug.Log("Zone Bounds : Top-Left - " + data.TopLeftPoint + " Down-Right - " + data.DownRightPoint);
            Debug.Log("Terrain Dimensions : " + Math.Round(data.Terrain_Dimension.x, 2) + " X " + Math.Round(data.Terrain_Dimension.y, 2) + " Km ");


            data.floatheightData = new float[data.mapSize_col_x, data.mapSize_row_y];
            short[,] heightMap = new short[data.mapSize_col_x + 1, data.mapSize_row_y + 1];

            FileStream fs = File.OpenRead(filepath);

            const int size = 1000000;

            int c = 0;

            do
            {
                byte[] buffer = new byte[size];
                int count = fs.Read(buffer, 0, size);

                for (int i = 0; i < count; i += 2)
                {
                    var buf = buffer[i] * 256 + buffer[i + 1];

                    short value = (short)(buf);

                    heightMap[c % data.mapSize_col_x, c / data.mapSize_row_y] = value;

                    float el = value;

                    var x = c % data.mapSize_col_x;
                    var y = c / data.mapSize_row_y;

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

                    data.floatheightData[x, data.mapSize_row_y - y - 1] = el;
                    FixedList.Add(el);
                    c++;
                }

            }
            while (fs.Position != fs.Length);

            fs.Close();

            if (fixOption == FixOption.AutoFix)
                FixTerrainData();


            GC.Collect();

            LoadComplet = true;

            if (!File.Exists(filepath))
            {
                if (OnReadError != null)
                {
                    OnReadError();
                }

                return;
            }

        }
        public void LoadFloatGrid(DVector2 SubRegionUpperLeftCoordiante, DVector2 SubRegionDownRightCoordiante, string filepath, FixOption fixOption = FixOption.Disable)
        {
            LoadComplet = false;
            string filename = Path.GetFileNameWithoutExtension(filepath).ToLower();
            string[] fileCoordinate = filename.Split(new[] { 'e', 'w' });
            if (fileCoordinate.Length != 2)
                throw new ArgumentException("Invalid filename.", filepath);

            fileCoordinate[0] = fileCoordinate[0].TrimStart(new[] { 'n', 's' });
            var Latitude = int.Parse(fileCoordinate[0]);
            data.DownLeftPoint.y = Latitude;
            if (filename.Contains("s"))
                data.DownLeftPoint.y *= -1;

            var Longitude = int.Parse(fileCoordinate[1]);
            data.DownLeftPoint.x = Longitude;
            if (filename.Contains("w"))
                data.DownLeftPoint.x *= -1;

            var HgtData = File.ReadAllBytes(filepath);

            switch (HgtData.Length)
            {
                case 1201 * 1201 * 2:
                    data.mapSize_col_x = data.mapSize_row_y = 1201;
                    break;
                case 3601 * 3601 * 2:
                    data.mapSize_col_x = data.mapSize_row_y = 3601;
                    break;
                default:
                    throw new ArgumentException("Invalid file size.", filepath);
            }

            data.TopRightPoint.x = data.DownLeftPoint.x + 1;
            data.TopRightPoint.y = data.DownLeftPoint.y + 1;

            data.TopLeftPoint = new DVector2(data.DownLeftPoint.x, data.TopRightPoint.y);
            data.DownRightPoint = new DVector2(data.TopRightPoint.x, data.DownLeftPoint.y);
 
            data.floatheightData = new float[data.mapSize_col_x, data.mapSize_row_y];
            short[,] heightMap = new short[data.mapSize_col_x + 1, data.mapSize_row_y + 1];

            FileStream fs = File.OpenRead(filepath);

            const int size = 1000000;

            int c = 0;

            do
            {
                byte[] buffer = new byte[size];
                int count = fs.Read(buffer, 0, size);

                for (int i = 0; i < count; i += 2)
                {
                    var buf = buffer[i] * 256 + buffer[i + 1];

                    short value = (short)(buf);

                    heightMap[c % data.mapSize_col_x, c / data.mapSize_row_y] = value;

                    float el = value;

                    var x = c % data.mapSize_col_x;
                    var y = c / data.mapSize_row_y;

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

                    data.floatheightData[x, data.mapSize_row_y - y - 1] = el;
                    FixedList.Add(el);

                    c++;
                }

            }
            while (fs.Position != fs.Length);

            fs.Close();
            GC.Collect();

            if (fixOption == FixOption.AutoFix)
                FixTerrainData();

            if (GISTerrainLoaderExtensions.IsSubRegionIncluded(data.TopLeftPoint, data.DownRightPoint, SubRegionUpperLeftCoordiante, SubRegionDownRightCoordiante))
            {
                var points = SubZone(data, SubRegionUpperLeftCoordiante, SubRegionDownRightCoordiante);

                data.floatheightData = points;

                data.TopLeftPoint = SubRegionUpperLeftCoordiante;
                data.DownRightPoint = SubRegionDownRightCoordiante;
                data.DownLeftPoint = new DVector2(data.TopLeftPoint.x, data.DownRightPoint.y);
                data.TopRightPoint = new DVector2(data.DownRightPoint.x, data.TopLeftPoint.y);

                data.Terrain_Dimension.x = GeoRefConversion.Getdistance(data.DownLeftPoint, data.DownRightPoint, 'X');
                data.Terrain_Dimension.y = GeoRefConversion.Getdistance(data.DownLeftPoint, data.TopLeftPoint, 'Y');

                Debug.Log("Zone Bounds : Top-Left - " + data.TopLeftPoint + " Down-Right - " + data.DownRightPoint);
                Debug.Log("Terrain Dimensions : " + Math.Round(data.Terrain_Dimension.x, 2) + " X " + Math.Round(data.Terrain_Dimension.y, 2) + " Km ");

            }
            else
            {
                if (OnReadError != null)
                {
                    OnReadError();
                }
            }

            LoadComplet = true;

            if (!File.Exists(filepath))
            {
                if (OnReadError != null)
                {
                    OnReadError();
                }

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

            for (int x = (int)StartLocation.x; x < (int)EndLocation.x - 1; x++)
            {
                for (int y = (int)StartLocation.y; y < (int)EndLocation.y -1; y++)
                {
                    int Step_X = x - 1 - ((int)StartLocation.x - 1);
                    int Step_Y = y - 1 - ((int)StartLocation.y - 1);
 
                  var el = data.floatheightData[x, data.mapSize_row_y - y - 1];

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
            Debug.Log(data.MaxElevation + "  " + data.MinElevation);
            data.mapSize_col_x = submapSize_col_x;
            data.mapSize_row_y = submapSize_row_y;

            return SubZone;

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