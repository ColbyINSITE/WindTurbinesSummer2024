/*     Unity GIS Tech 2020-2021      */

using System;
using UnityEngine;
using BitMiracle.LibTiff.Classic;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderTIFFLoader
    {
        public static event ReaderEvents OnReadError;

        public static event TerrainProgression OnProgress;

        public GISTerrainLoaderFileData data;

        public bool LoadComplet;

        private GTLGeographicCoordinateSystem CoordinateReferenceSystem;
        private GISTerrainLoaderTIFFMetadataReader TiffMetadata;

        private List<float> FixedList;
        public Byte[] WebData = null;
        private static int EPSG =0;
        private TiffElevationSource tiffElevationSource;
        public GISTerrainLoaderTIFFLoader()
        {
            data = new GISTerrainLoaderFileData();
            FixedList = new List<float>();
        }
        public void LoadTiff(string filePath, TerrainDimensionsMode terrainDimensionMode, GISTerrainLoaderFileData LasData = null, FixOption fixOption = FixOption.Disable,int m_EPSG= 0,TiffElevationSource m_tiffElevationSource = TiffElevationSource.DEM)
        {
            if(m_EPSG!=0)
            EPSG = m_EPSG;

            tiffElevationSource = m_tiffElevationSource;

            LoadComplet = false;

            if (fixOption == FixOption.ManualFix)
            {
                data.MinElevation = data.TerrainMaxMinElevation.x;
                data.MaxElevation = data.TerrainMaxMinElevation.y;
            }

            try
            {

#if UNITY_WEBGL

                GISTerrainLoaderTiffStreamForBytes byteStream = new GISTerrainLoaderTiffStreamForBytes(WebData);

                using (Tiff tiff = Tiff.ClientOpen("bytes", "r", null, byteStream))
                {
                    if (tiff == null)
                    {
                        Debug.Log("Could not open DEM file");

                        if (OnReadError != null)
                            OnReadError();
                    }

                    ParseTiff(tiff, fixOption, terrainDimensionMode, LasData);
                }
#else

                using (Tiff tiff = Tiff.Open(filePath, "r"))
                {
                    ParseTiff(tiff, fixOption, terrainDimensionMode, LasData);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.Log("Couldn't Load Terrain file: " + ex.Message + "  " + Environment.NewLine);

                if (OnReadError != null)
                    OnReadError();
            };

        }
        public void LoadTiff(DVector2 SubRegionUpperLeftCoordiante, DVector2 SubRegionDownRightCoordiante, string filePath, TerrainDimensionsMode terrainDimensionMode, GISTerrainLoaderFileData LasData = null, FixOption fixOption = FixOption.Disable)
        {
            try
            {
                LoadComplet = false;

                using (Tiff tiff = Tiff.Open(filePath, "r"))
                {

                    int width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();

                    int lenght = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

                    int BITSPERSAMPLE = tiff.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();

                    data.mapSize_row_y = lenght;
                    data.mapSize_col_x = width;

                    data.floatheightData = new float[width, lenght];

                    int s = -1;



                    switch (BITSPERSAMPLE)
                    {
                        case 16:

                            var scanline = new byte[tiff.ScanlineSize()];

                            for (int row = 0; row < lenght; row++)
                            {
                                tiff.ReadScanline(scanline, row);

                                for (int col = 0; col < width; col++)
                                {
                                    var el = (short)((scanline[col * 2 + 1] << 8) + scanline[col * 2]);

                                    var el1 = Convert.ToSingle(el);

                                    if (fixOption == FixOption.ManualFix)
                                    {
                                        if (el < data.TerrainMaxMinElevation.x)
                                            el = (short)data.TerrainMaxMinElevation.x;

                                        if (el > data.TerrainMaxMinElevation.y)
                                            el = (short)data.TerrainMaxMinElevation.y;

                                    }
                                    else
                                    {
                                        if (el < data.MinElevation)
                                            data.MinElevation = el;
                                        if (el > data.MaxElevation)
                                            data.MaxElevation = el;
                                    }

                                    data.floatheightData[col, data.mapSize_row_y - row - 1] = el1;
                                    FixedList.Add(el1);
                                }

                                var prog = (row * 100 / lenght);

                                if (s != prog && prog <= 99)
                                {
                                    if (OnProgress != null)
                                        OnProgress("Loading File ", prog);

                                    s = prog;
                                }

                            }

                            if (fixOption == FixOption.AutoFix)
                                FixTerrainData();

                            break;

                        case 32:

                            scanline = new byte[tiff.ScanlineSize()];

                            float[] scanline32Bit = new float[tiff.ScanlineSize() / 2];

                            for (int i = 0; i < lenght; i++)
                            {
                                for (int j = 0; j < width; j++)
                                {
                                    tiff.ReadScanline(scanline, 0, i, 0);

                                    Buffer.BlockCopy(scanline, 0, scanline32Bit, 0, scanline.Length);

                                    float el = scanline32Bit[j];

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
                                    data.floatheightData[j, data.mapSize_row_y - i - 1] = el;
                                    FixedList.Add(el);
                                }
                            }

                            if (fixOption == FixOption.AutoFix)
                                FixTerrainData();

                            break;
                    }

                    if (terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
                    {
                        if (LasData == null)
                        {

                            FieldValue[] modelPixelScaleTag = tiff.GetField((TiffTag)33550);
                            FieldValue[] modelTiepointTag = tiff.GetField((TiffTag)33922);

                            byte[] modelPixelScale = modelPixelScaleTag[1].GetBytes();
                            double pixelSizeX = BitConverter.ToDouble(modelPixelScale, 0);
                            double pixelSizeY = BitConverter.ToDouble(modelPixelScale, 8) * -1;

                            byte[] modelTransformation = modelTiepointTag[1].GetBytes();
                            double DownRightLon = BitConverter.ToDouble(modelTransformation, 24);
                            double DownRightLat = BitConverter.ToDouble(modelTransformation, 32);


                            double startLat = DownRightLat + (pixelSizeY / 2.0);
                            double startLon = DownRightLon + (pixelSizeX / 2.0);

                            double currentLat = startLat;
                            double currentLon = startLon;

                            data.DownLeftPoint = new DVector2(DownRightLon, startLat + (pixelSizeY * lenght));
                            data.TopLeftPoint = new DVector2(DownRightLon, DownRightLat);
                            data.DownRightPoint = new DVector2(startLon + (pixelSizeX * width), startLat + (pixelSizeY * lenght));
                            data.TopRightPoint = new DVector2(data.DownRightPoint.x, data.TopLeftPoint.y);

                            // Read Projection

                            data.DownLeftPoint = GeoRefConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.DownLeftPoint, EPSG);
                            data.TopLeftPoint = GeoRefConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.TopLeftPoint, EPSG);
                            data.DownRightPoint = GeoRefConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.DownRightPoint, EPSG);
                            data.TopRightPoint = GeoRefConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.TopRightPoint, EPSG);

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

                        }
                        else
                    if (LasData.AlreadyLoaded)
                        {
                            data.TopLeftPoint = LasData.TopLeftPoint;
                            data.DownRightPoint = LasData.DownRightPoint;

                            data.DownLeftPoint = LasData.DownLeftPoint;
                            data.TopRightPoint = LasData.TopRightPoint;

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
                                    OnReadError();
                            }
                        }
                    }


                    LoadComplet = true;

                }
            }
            catch (Exception ex)
            {
                Debug.Log("Couldn't Load Terrain file: " + ex.Message + "  " + Environment.NewLine);

                if (OnReadError != null)
                {
                    OnReadError();
                }
            };



        }
        private void ParseTiff(Tiff tiff, FixOption fixOption, TerrainDimensionsMode terrainDimensionMode, GISTerrainLoaderFileData LasData)
        {
            if (terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
            {
                TiffMetadata = new GISTerrainLoaderTIFFMetadataReader(tiff);
                CoordinateReferenceSystem = TiffMetadata.CoordinateReferenceSystem;

                if (TiffMetadata.ProjectionSystem == ProjectionSystem.Undefined && EPSG==0)
                {
                    Debug.LogError("Error While Loading File : Undefined Projection System, reproject your file to one of supported Projection or Set Dimention Mode to manual");

                    if (OnReadError != null)
                        OnReadError();
                    return;
                }

            }

            int width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int lenght = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

            int BITSPERSAMPLE = tiff.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();

            int samplesPerPixel = tiff.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToInt();

            data.mapSize_row_y = lenght;
            data.mapSize_col_x = width;

            data.floatheightData = new float[width, lenght];

            int counter = -1;

            int[] raster = new int[lenght * width];

            switch (tiffElevationSource)
            {
                case TiffElevationSource.GrayScale:
                    if (!tiff.ReadRGBAImage(width, lenght, raster))
                    {
                        Debug.LogError("Could not read Tiff image ...");

                        if (OnReadError != null)
                            OnReadError();

                        return;
                    }

                    for (int row = 0; row < lenght; ++row)
                        for (int col = 0; col < width; ++col)
                        {
                            int offset = (lenght - row - 1) * width + col;
                            Color color = new Color();
                            color.r = Tiff.GetR(raster[offset]);
                            color.g = Tiff.GetG(raster[offset]);
                            color.b = Tiff.GetB(raster[offset]);

                            var el = color.grayscale;

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

                            data.floatheightData[col, lenght - row - 1] = el;
                            FixedList.Add(el);

                        }
                    if (fixOption == FixOption.AutoFix)
                        FixTerrainData();
                    break;
                case TiffElevationSource.DEM:

                    switch (BITSPERSAMPLE)
                    {
                        case 16:

                            if (tiff.GetField(TiffTag.TILEWIDTH) != null)
                            {
                                if (tiff.GetField(TiffTag.TILEWIDTH).Length > 0)
                                {

                                    //Get the tile size
                                    int tileWidth = tiff.GetField(TiffTag.TILEWIDTH)[0].ToInt();
                                    int tileHeight = tiff.GetField(TiffTag.TILELENGTH)[0].ToInt();
                                    int tileSize = tiff.TileSize();

                                    //Pixel depth
                                    int depth = tileSize / (tileWidth * tileHeight);

                                    byte[] buffer = new byte[tileSize];

                                    for (int y = 0; y < lenght; y += tileHeight)
                                    {
                                        for (int x = 0; x < width; x += tileWidth)
                                        {
                                            //Read the value and store to the buffer
                                            tiff.ReadTile(buffer, 0, x, y, 0, 0);

                                            for (int i = 0; i < tileWidth; i++)
                                            {
                                                for (int j = 0; j < tileHeight; j++)
                                                {
                                                    int startIndex = (i + tileWidth * j) * depth;
                                                    if (startIndex >= buffer.Length)
                                                        continue;

                                                    int pixelX = x + i;
                                                    int pixelY = y + j;
                                                    if (pixelX >= width || pixelY >= lenght)
                                                        continue;

                                                    var el = BitConverter.ToInt16(buffer, startIndex);

                                                    if (fixOption == FixOption.ManualFix)
                                                    {
                                                        if (el < data.TerrainMaxMinElevation.x)
                                                            el = (short)data.TerrainMaxMinElevation.x;

                                                        if (el > data.TerrainMaxMinElevation.y)
                                                            el = (short)data.TerrainMaxMinElevation.y;

                                                    }
                                                    else
                                                    {
                                                        if (el < data.MinElevation)
                                                            data.MinElevation = el;
                                                        if (el > data.MaxElevation)
                                                            data.MaxElevation = el;
                                                    }

                                                    data.floatheightData[pixelX, data.mapSize_row_y - pixelY - 1] = el;
                                                    FixedList.Add(el);

                                                }


                                            }
                                        }

                                        var prog = (y * 100 / lenght);


                                        if (counter != prog && prog <= 99)
                                        {

                                            if (OnProgress != null)
                                                OnProgress("Loading File ", prog);

                                            counter = prog;
                                        }


                                    }
                                    if (fixOption == FixOption.AutoFix)
                                        FixTerrainData();

                                }

                            }
                            else
                            {
                                byte[] scanline16 = new byte[tiff.ScanlineSize()];

                                for (int row = 0; row < lenght; row++)
                                {
                                    tiff.ReadScanline(scanline16, row);

                                    for (int col = 0; col < width; col++)
                                    {
                                        var el = (short)((scanline16[col * 2 + 1] << 8) + scanline16[col * 2]);

                                        var el1 = Convert.ToSingle(el);

                                        if (fixOption == FixOption.ManualFix)
                                        {
                                            if (el1 < data.TerrainMaxMinElevation.x)
                                                el1 = data.TerrainMaxMinElevation.x;

                                            if (el1 > data.TerrainMaxMinElevation.y)
                                                el1 = data.TerrainMaxMinElevation.y;

                                        }
                                        else
                                        {
                                            if (el1 < data.MinElevation)
                                                data.MinElevation = el;
                                            if (el1 > data.MaxElevation)
                                                data.MaxElevation = el;
                                        }

                                        data.floatheightData[col, data.mapSize_row_y - row - 1] = el1;
                                        FixedList.Add(el1);
                                    }


                                    var prog = (row * 100 / lenght);


                                    if (counter != prog && prog <= 99)
                                    {

                                        if (OnProgress != null)
                                            OnProgress("Loading File ", prog);

                                        counter = prog;
                                    }

                                    if (fixOption == FixOption.AutoFix)
                                        FixTerrainData();
                                }
                            }

                            break;

                        case 32:

                            if (tiff.GetField(TiffTag.TILEWIDTH) != null)
                            {
                                if (tiff.GetField(TiffTag.TILEWIDTH).Length > 0)
                                {
                                    //Get the tile size
                                    int tileWidth = tiff.GetField(TiffTag.TILEWIDTH)[0].ToInt();
                                    int tileHeight = tiff.GetField(TiffTag.TILELENGTH)[0].ToInt();
                                    int tileSize = tiff.TileSize();

                                    //Pixel depth
                                    int depth = tileSize / (tileWidth * tileHeight);

                                    byte[] buffer = new byte[tileSize];

                                    for (int y = 0; y < lenght; y += tileHeight)
                                    {
                                        for (int x = 0; x < width; x += tileWidth)
                                        {
                                            //Read the value and store to the buffer
                                            tiff.ReadTile(buffer, 0, x, y, 0, 0);

                                            for (int i = 0; i < tileWidth; i++)
                                            {
                                                for (int j = 0; j < tileHeight; j++)
                                                {
                                                    int startIndex = (i + tileWidth * j) * depth;
                                                    if (startIndex >= buffer.Length)
                                                        continue;

                                                    int pixelX = x + i;
                                                    int pixelY = y + j;
                                                    if (pixelX >= width || pixelY >= lenght)
                                                        continue;

                                                    float el = BitConverter.ToSingle(buffer, startIndex); ;

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

                                                    data.floatheightData[pixelX, data.mapSize_row_y - pixelY - 1] = el;
                                                    FixedList.Add(el);
                                                }

                                            }

                                        }

                                        var prog = (y * 100 / lenght);

                                        if (counter != prog && prog <= 99)
                                        {
                                            if (OnProgress != null)
                                                OnProgress("Loading File ", prog);

                                            counter = prog;
                                        }

                                    }


                                    if (fixOption == FixOption.AutoFix)
                                        FixTerrainData();
                                }

                            }
                            else
                            {

                                var scanline32 = new byte[tiff.ScanlineSize()];

                                float[] scanline32Bit = new float[tiff.ScanlineSize() / 2];


                                for (int i = 0; i < lenght; i++)
                                {
                                    for (int j = 0; j < width; j++)
                                    {
                                        tiff.ReadScanline(scanline32, 0, i, 0);

                                        Buffer.BlockCopy(scanline32, 0, scanline32Bit, 0, scanline32.Length);

                                        float el = scanline32Bit[j];

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

                                        data.floatheightData[j, data.mapSize_row_y - i - 1] = el;
                                        FixedList.Add(el);
                                    }

                                    var prog = (i * 100 / lenght);

                                    if (counter != prog && prog <= 99)
                                    {
                                        if (OnProgress != null)
                                            OnProgress("Loading File ", prog);

                                        counter = prog;
                                    }

                                }

                                if (fixOption == FixOption.AutoFix)
                                    FixTerrainData();
                            }

                            break;

                        case 64:

                            if (tiff.GetField(TiffTag.TILEWIDTH) != null)
                            {
                                if (tiff.GetField(TiffTag.TILEWIDTH).Length > 0)
                                {
                                    int tileWidth = tiff.GetField(TiffTag.TILEWIDTH)[0].ToInt();
                                    int tileHeight = tiff.GetField(TiffTag.TILELENGTH)[0].ToInt();
                                    int tileSize = tiff.TileSize();

                                    int depth = tileSize / (tileWidth * tileHeight);

                                    byte[] buffer = new byte[tileSize];

                                    for (int y = 0; y < lenght; y += tileHeight)
                                    {
                                        for (int x = 0; x < width; x += tileWidth)
                                        {
                                            tiff.ReadTile(buffer, 0, x, y, 0, 0);

                                            for (int i = 0; i < tileWidth; i++)
                                            {
                                                for (int j = 0; j < tileHeight; j++)
                                                {
                                                    int startIndex = (i + tileWidth * j) * depth;
                                                    if (startIndex >= buffer.Length)
                                                        continue;

                                                    int pixelX = x + i;
                                                    int pixelY = y + j;
                                                    if (pixelX >= width || pixelY >= lenght)
                                                        continue;

                                                    float el = (float)(BitConverter.ToDouble(buffer, startIndex));

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

                                                    data.floatheightData[pixelX, data.mapSize_row_y - pixelY - 1] = el;
                                                    FixedList.Add(el);
                                                }

                                            }

                                        }

                                        var prog = (y * 100 / lenght);

                                        if (counter != prog && prog <= 99)
                                        {
                                            if (OnProgress != null)
                                                OnProgress("Loading File ", prog);

                                            counter = prog;
                                        }

                                    }


                                    if (fixOption == FixOption.AutoFix)
                                        FixTerrainData();
                                }

                            }
                            else
                            {
                                var scanline64 = new byte[tiff.ScanlineSize()];

                                double[] scanline64Bit = new double[tiff.ScanlineSize()];

                                for (int i = 0; i < lenght; i++)
                                {
                                    for (int j = 0; j < width; j++)
                                    {
                                        tiff.ReadScanline(scanline64, 0, i, 0);

                                        Buffer.BlockCopy(scanline64, 0, scanline64Bit, 0, scanline64.Length);

                                        float el = (float)scanline64Bit[j];

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

                                        data.floatheightData[j, data.mapSize_row_y - i - 1] = el;
                                        FixedList.Add(el);
                                    }

                                    var prog = (i * 100 / lenght);

                                    if (counter != prog && prog <= 99)
                                    {
                                        if (OnProgress != null)
                                            OnProgress("Loading File ", prog);

                                        counter = prog;
                                    }

                                }

                                if (fixOption == FixOption.AutoFix)
                                    FixTerrainData();
                            }

                            break;

                    }
                    break;

                case TiffElevationSource.BandsData:

                    //Use this var to define in which table elevation data are stored

                    int ElevationBandIndex = 7;

                    GISTerrainLoaderTiffMultiBands TiffData = null;

                    switch (BITSPERSAMPLE)
                    {
                        case 8:

                            TiffData = new GISTerrainLoaderTiffMultiBands(samplesPerPixel, width, lenght);
                            TiffData.TiffMetadata = TiffMetadata;
                            TiffData.CoordinateReferenceSystem = CoordinateReferenceSystem;

                            byte[] scanline8 = new byte[tiff.ScanlineSize()];
                            float[] scanlinBit = new float[tiff.ScanlineSize()];

                            int BandCounter = 0;
                            int C_BandCounter = 0;
                            int R_BandCounter = 0;

                            for (int row = 0; row < lenght; row++)
                            {
                                tiff.ReadScanline(scanline8, row);

                                for (int col = 0; col < width; col++)
                                {
                                    var el = (short)((scanline8[col]) + scanline8[col]);

                                    var el1 = Convert.ToSingle(el) / 2;

                                    if (BandCounter >= samplesPerPixel)
                                    {
                                        BandCounter = 0;
                                        C_BandCounter++;

                                        if (C_BandCounter > width - 1)
                                        {
                                            C_BandCounter = 0;
                                            R_BandCounter++;
                                        }

                                    }

                                    TiffData.BandsData[BandCounter][R_BandCounter, C_BandCounter] = el1;

                                    BandCounter++;
                                }


                            }

                            break;
                        case 16:
                            break;
                        case 32:

                            TiffData = new GISTerrainLoaderTiffMultiBands(samplesPerPixel, width, lenght);
                            TiffData.TiffMetadata = TiffMetadata;
                            TiffData.CoordinateReferenceSystem = CoordinateReferenceSystem;

                            var scanlin = new byte[tiff.ScanlineSize()];
                            scanlinBit = new float[tiff.ScanlineSize()];
                            BandCounter = 0;
                            C_BandCounter = 0;
                            R_BandCounter = 0;

                            for (int i = 0; i < lenght; i++)
                            {
                                tiff.ReadScanline(scanlin, 0, i, 0);

                                for (int j = 0; j < width * samplesPerPixel; j++)
                                {
                                    Buffer.BlockCopy(scanlin, 0, scanlinBit, 0, scanlin.Length);

                                    float el = Convert.ToSingle(scanlinBit[j]);

                                    if (BandCounter >= samplesPerPixel)
                                    {
                                        BandCounter = 0;
                                        C_BandCounter++;

                                        if (C_BandCounter > width - 1)
                                        {
                                            C_BandCounter = 0;
                                            R_BandCounter++;
                                        }

                                    }

                                    TiffData.BandsData[BandCounter][R_BandCounter, C_BandCounter] = el;

                                    BandCounter++;
                                }
                            }
                            break;

                    }

                    if(TiffData!=null)
                    {
                        if(TiffData.BandsData !=null)
                        {
                            if(ElevationBandIndex< TiffData.BandsData.Count)
                            {
                                for (int i = 0; i < lenght; i++)
                                {
                                    for (int j = 0; j < width; j++)
                                    {
                                        float el = TiffData.BandsData[ElevationBandIndex][i,j ];

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

                                        data.floatheightData[j, data.mapSize_row_y - i - 1] = el;
                                        FixedList.Add(el);
                                    }

                                    var prog = (i * 100 / lenght);

                                    if (counter != prog && prog <= 99)
                                    {
                                        if (OnProgress != null)
                                            OnProgress("Loading File ", prog);

                                        counter = prog;
                                    }

                                }

                                if (fixOption == FixOption.AutoFix)
                                    FixTerrainData();
                            }

                        }
                    }



                    break;


            }


            if (terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
            {
                if (LasData == null)
                {
                    try
                    {
                        FieldValue[] modelPixelScaleTag = tiff.GetField((TiffTag)33550);
                        FieldValue[] modelTiepointTag = tiff.GetField((TiffTag)33922);

                        byte[] modelPixelScale = modelPixelScaleTag[1].GetBytes();

                        double pixelSizeX = BitConverter.ToDouble(modelPixelScale, 0);
                        double pixelSizeY = BitConverter.ToDouble(modelPixelScale, 8) * -1;

                        byte[] modelTransformation = modelTiepointTag[1].GetBytes();

                        double originLon = BitConverter.ToDouble(modelTransformation, 24);
                        double originLat = BitConverter.ToDouble(modelTransformation, 32);


                        double startLat = originLat + (pixelSizeY / 2.0);
                        double startLon = originLon + (pixelSizeX / 2.0);

                        double currentLat = startLat;
                        double currentLon = startLon;

                        data.DownLeftPoint = new DVector2(originLon, startLat + (pixelSizeY * lenght));
                        data.TopLeftPoint = new DVector2(originLon, originLat);
                        data.DownRightPoint = new DVector2(startLon + (pixelSizeX * width), startLat + (pixelSizeY * lenght));
                        data.TopRightPoint = new DVector2(data.DownRightPoint.x, data.TopLeftPoint.y);

                        // Read Projection
                        if (EPSG == 0) EPSG = CoordinateReferenceSystem.EPSG_Code;
                        data.DownLeftPoint = GeoRefConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.DownLeftPoint, EPSG);
                        data.TopRightPoint = GeoRefConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.TopRightPoint, EPSG);
                        data.TopLeftPoint = GeoRefConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.TopLeftPoint, EPSG);
                        data.DownRightPoint = GeoRefConversion.ConvertTOLatLon(CoordinateReferenceSystem, data.DownRightPoint, EPSG);

                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Error While Loading File : Some of file parameters not set correctly " + ex);

                        if (OnReadError != null)
                            OnReadError();
                    }

                }
                else
            if (LasData.AlreadyLoaded)
                {

                    data.TopLeftPoint = LasData.TopLeftPoint;
                    data.DownRightPoint = LasData.DownRightPoint;

                    data.DownLeftPoint = LasData.DownLeftPoint;
                    data.TopRightPoint = LasData.TopRightPoint;

                }

                data.Terrain_Dimension.x = GeoRefConversion.Getdistance(data.DownLeftPoint, data.DownRightPoint, 'X');
                data.Terrain_Dimension.y = GeoRefConversion.Getdistance(data.DownLeftPoint, data.TopLeftPoint, 'Y');

                Debug.Log("Zone Bounds : Top-Left - " + data.TopLeftPoint + " Down-Right - " + data.DownRightPoint);
                Debug.Log("Terrain Dimensions : " + Math.Round(data.Terrain_Dimension.x, 2) + " X " + Math.Round(data.Terrain_Dimension.y, 2) + " Km ");

            }

            LoadComplet = true;

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

                    SubZone[Step_X, Step_Y] = el;

                }

            }

            data.mapSize_col_x = submapSize_col_x;
            data.mapSize_row_y = submapSize_row_y;

            return SubZone;

        }
        public bool ReadProjectionFile(string path, GTLGeographicCoordinateSystem CoordinateReferencesystem)
        {
            string prjFile = path.Replace(Path.GetExtension(path), ".prj");

            if (File.Exists(prjFile))
            {
                CoordinateReferencesystem = GISTerrainLoaderProjectionReader.ReadProjectionFile(path);
                return true;
            }
            else
                return false;
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

        public static GISTerrainLoaderTiffMultiBands LoadTiffBands(string filePath)
        {
            GISTerrainLoaderTiffMultiBands TiffData = null;

            using (Tiff tiff = Tiff.Open(filePath, "r"))
            {
                int Col = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                int Row = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

                var samplesPerPixel = tiff.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToInt();
                int BITSPERSAMPLE = tiff.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();

                TiffData = new GISTerrainLoaderTiffMultiBands(samplesPerPixel, Col,Row);

                TiffData.TiffMetadata = new GISTerrainLoaderTIFFMetadataReader(tiff);
                TiffData.CoordinateReferenceSystem = TiffData.TiffMetadata.CoordinateReferenceSystem;

                if (TiffData.TiffMetadata.GeographicTypeGeoKey == GeographicCoordinateSystem.Undefined)
                {
                    Debug.LogError("Error While Loading File : Undefined Projection System, reproject your file to one of supported Projection or Set Dimention Mode to manual");

                    if (OnReadError != null)
                        OnReadError();
                }

                switch (BITSPERSAMPLE)
                {
                    case 8:

                        byte[] scanline8 = new byte[tiff.ScanlineSize()];
                        float[] scanlinBit = new float[tiff.ScanlineSize()];

                        int BandCounter = 0;
                        int C_BandCounter = 0;
                        int R_BandCounter = 0;
                        for (int row = 0; row < Row; row++)
                        {
                            tiff.ReadScanline(scanline8, row);

                            for (int col = 0; col < Col; col++)
                            {
                                var el = (short)((scanline8[col]) + scanline8[col]);

                                var el1 = Convert.ToSingle(el)/2;

                                if (BandCounter >= samplesPerPixel)
                                {
                                    BandCounter = 0;
                                    C_BandCounter++;

                                    if (C_BandCounter > Col - 1)
                                    {
                                        C_BandCounter = 0;
                                        R_BandCounter++;
                                    }

                                }

                                TiffData.BandsData[BandCounter][R_BandCounter, C_BandCounter] = el1;

                                BandCounter++;
                            }


                        }

                        break;
                    case 16:
                        break;
                    case 32:
                        var scanlin = new byte[tiff.ScanlineSize()];
                        scanlinBit = new float[tiff.ScanlineSize()];
                         BandCounter = 0;
                         C_BandCounter = 0;
                         R_BandCounter = 0;

                        for (int i = 0; i < Row; i++)
                        {
                            tiff.ReadScanline(scanlin, 0, i, 0);

                            for (int j = 0; j < Col * samplesPerPixel; j++)
                            {
                                Buffer.BlockCopy(scanlin, 0, scanlinBit, 0, scanlin.Length);

                                float el = Convert.ToSingle(scanlinBit[j]);

                                if (BandCounter >= samplesPerPixel)
                                {
                                    BandCounter = 0;
                                    C_BandCounter++;

                                    if (C_BandCounter > Col - 1)
                                    {
                                        C_BandCounter = 0;
                                        R_BandCounter++;
                                    }

                                }

                                TiffData.BandsData[BandCounter][R_BandCounter, C_BandCounter] = el;

                                BandCounter++;
                            }
                        }
                        break;

                }



                FieldValue[] modelPixelScaleTag = tiff.GetField((TiffTag)33550);
                FieldValue[] modelTiepointTag = tiff.GetField((TiffTag)33922);

                byte[] modelPixelScale = modelPixelScaleTag[1].GetBytes();

                double pixelSizeX = BitConverter.ToDouble(modelPixelScale, 0);
                double pixelSizeY = BitConverter.ToDouble(modelPixelScale, 8) * -1;

                byte[] modelTransformation = modelTiepointTag[1].GetBytes();

                double originLon = BitConverter.ToDouble(modelTransformation, 24);
                double originLat = BitConverter.ToDouble(modelTransformation, 32);


                double startLat = originLat + (pixelSizeY / 2.0);
                double startLon = originLon + (pixelSizeX / 2.0);

                double currentLat = startLat;
                double currentLon = startLon;

                TiffData.data.DownLeftPoint = new DVector2(originLon, startLat + (pixelSizeY * Row));
                TiffData.data.TopLeftPoint = new DVector2(originLon, originLat);
                TiffData.data.DownRightPoint = new DVector2(startLon + (pixelSizeX * Col), startLat + (pixelSizeY * Row));
                TiffData.data.TopRightPoint = new DVector2(TiffData.data.DownRightPoint.x, TiffData.data.TopLeftPoint.y);

                // Read Projection
                if (EPSG == 0) EPSG = TiffData.CoordinateReferenceSystem.EPSG_Code;
                TiffData.data.DownLeftPoint = GeoRefConversion.ConvertTOLatLon(TiffData.CoordinateReferenceSystem, TiffData.data.DownLeftPoint, EPSG);
                TiffData.data.TopRightPoint = GeoRefConversion.ConvertTOLatLon(TiffData.CoordinateReferenceSystem, TiffData.data.TopRightPoint, EPSG);
                TiffData.data.TopLeftPoint = GeoRefConversion.ConvertTOLatLon(TiffData.CoordinateReferenceSystem, TiffData.data.TopLeftPoint, EPSG);
                TiffData.data.DownRightPoint = GeoRefConversion.ConvertTOLatLon(TiffData.CoordinateReferenceSystem, TiffData.data.DownRightPoint, EPSG);

            }

            return TiffData;
        }
        private static Color getSample(int x, int y, int[] raster, int width, int height)
        {
            int offset = (height - y - 1) * width + x;
            int red = Tiff.GetR(raster[offset]);
            int green = Tiff.GetG(raster[offset]);
            int blue = Tiff.GetB(raster[offset]);
            return new Color(red, green, blue);
        }
    }
    public class GISTerrainLoaderTiffMultiBands
    {
        public GISTerrainLoaderFileData data;
        public int BandsNumber = 0;
        public List<float[,]> BandsData = null;
        public GTLGeographicCoordinateSystem CoordinateReferenceSystem;
        public GISTerrainLoaderTIFFMetadataReader TiffMetadata;
        public int samplesPerPixel = 0;


        public GISTerrainLoaderTiffMultiBands(int m_BandsNumber, int Col,int Row)
        {
            data = new GISTerrainLoaderFileData();

            BandsData = new List<float[,]>();

            BandsNumber = m_BandsNumber;

            for (int b = 0; b < BandsNumber; b++)
            {
                var list = new float[Row, Col];
                BandsData.Add(list);
            }
            data.mapSize_col_x = Col;
            data.mapSize_row_y = Row;

        }
        public float GetValue(int BandID,int row, int col )
        {
            return  BandsData[BandID][row, col];
        }
        public float GetValue(int BandsNumber, DVector2 LatLon)
        {
            float value = 0;

            var rang_x = Math.Abs(Math.Abs(data.DownRightPoint.x) - Math.Abs(data.TopLeftPoint.x));
            var rang_y = Math.Abs(Math.Abs(data.TopLeftPoint.y) - Math.Abs(data.DownRightPoint.y));

            var rang_px = Math.Abs(Math.Abs(LatLon.x) - Math.Abs(data.TopLeftPoint.x));
            var rang_py = Math.Abs(Math.Abs(data.TopLeftPoint.y) - Math.Abs(LatLon.y));

            int localLat = (int)(rang_px * data.mapSize_col_x / rang_x);
            int localLon = (int)(rang_py * data.mapSize_row_y / rang_y);

            if (localLat > data.mapSize_col_x - 1) localLat = data.mapSize_col_x - 1;
            if (localLon > data.mapSize_row_y - 1) localLon = data.mapSize_row_y - 1;

            value = BandsData[BandsNumber][localLon,localLat];

            return value;
        }
        public float [] GetValues(int BandsNumber, DVector2 LatLon)
        {
            float[] data = new float[BandsNumber];

            for(int i=0;i<BandsNumber;i++)
                data[i] = GetValue(i, LatLon);

            return data;
        }

    }
    class GISTerrainLoaderTiffStreamForBytes : TiffStream
    {
        private byte[] m_bytes;
        private int m_position;

        public GISTerrainLoaderTiffStreamForBytes(byte[] bytes)
        {
            m_bytes = bytes;
            m_position = 0;
        }

        public override int Read(object clientData, byte[] buffer, int offset, int count)
        {
            if ((m_position + count) > m_bytes.Length)
                return -1;

            Buffer.BlockCopy(m_bytes, m_position, buffer, offset, count);
            m_position += count;
            return count;
        }

        public override void Write(object clientData, byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException("This stream is read-only");
        }

        public override long Seek(object clientData, long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset > m_bytes.Length)
                        return -1;

                    m_position = (int)offset;
                    return m_position;

                case SeekOrigin.Current:
                    if ((offset + m_position) > m_bytes.Length)
                        return -1;

                    m_position += (int)offset;
                    return m_position;

                case SeekOrigin.End:
                    if ((m_bytes.Length - offset) < 0)
                        return -1;

                    m_position = (int)(m_bytes.Length - offset);
                    return m_position;
            }

            return -1;
        }

        public override void Close(object clientData)
        {
            // nothing to do
        }

        public override long Size(object clientData)
        {
            return m_bytes.Length;
        }
    }
}

