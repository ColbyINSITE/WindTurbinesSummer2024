/*     Unity GIS Tech 2020-2021      */

#if GISTerrainLoaderPdal

using System;
using UnityEngine;
using pdal;
using System.IO;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderLASLoader
    {
        public static event ReaderEvents OnReadError;

        public GISTerrainLoaderFileData data;

        public bool LoadComplet;

        public string jsonPipeline;

        public string GeneratedFilePath;

        private PdalPipeline Jpipeline;
        public GISTerrainLoaderLASLoader()
        {
            data = new GISTerrainLoaderFileData();
        }

        public void LoadLasFile(string filePath)
        {

            try
            {
                CreateJsonPipeline(filePath);

                LoadLasData(filePath);

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
        private void CreateJsonPipeline(string filePath)
        {

            GeneratedFilePath = Path.ChangeExtension(filePath, ".tif");


            Jpipeline = new PdalPipeline();
            Jpipeline.SourceFilename = filePath;
            Jpipeline.GenFilename = GeneratedFilePath;
            Jpipeline.Gdaldriver = GdalDriver.GTiff;
            Jpipeline.OutputType = OutputType.mean;
            Jpipeline.Resolution = 0.5f;
            Jpipeline.DataType = DataType.m_float;
            Jpipeline.Nodata = -9999;
            Jpipeline.Type = "writers.gdal";
            Jpipeline.WindowSize = 6;

            Jpipeline.GenerateJson();

            Jpipeline.SaveToPdalJsonFile();
        }
        private void LoadLasData(string filePath)
        {
            try
            {

                string jsonFile = Jpipeline.LoadPdalJsonFile();

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

                    pdal.Pipeline pipeline = new pdal.Pipeline(Jpipeline.LoadPdalJsonFile());

                    long TotalPipelinepointCount = pipeline.Execute();

                    data = new GISTerrainLoaderFileData();

                    //Read Header  + MetaData 

                    LasHeader header = new LasHeader(filePath, pipeline);

                    header.GetLasData(data);

                    pipeline.Dispose();

                    if (TotalPipelinepointCount == 0)
                    {
                        Debug.LogError("File Not Valid or File is used by another application ... ");
                        OnReadError();
                    }

                    if (File.Exists(GeneratedFilePath))
                        LoadComplet = true;

                }


            }
            catch (Exception ex)
            {
                Debug.Log("Couldn't Load Terrain file: " + ex.Message + "  " + Environment.NewLine);

                OnReadError();
            };



        }
        public void LoadLazFile(TextureSource textureSource, string filePath)
        {
            OnReadError();
        }
    }
}

namespace GISTech.GISTerrainLoader
{
    public class PdalPipeline
    {
        public string SourceFilename { get; set; }
        public string GenFilename { get; set; }
        public GdalDriver Gdaldriver { get; set; }
        public OutputType OutputType { get; set; }
        public float Resolution { get; set; }
        public DataType DataType { get; set; }
        public long Nodata { get; set; }
        public string Type { get; set; }
        public long WindowSize { get; set; }

        private string JsonName = "LasToDEM";

        public string GenerateJson()
        {
            string Pdalpipeline =
                "{"
                    + "\n"
                    + "\"" + "pipeline" + "\"" + ":["
                    + "\n"
                    + "\"" + SourceFilename.Replace(@"\", "/") + "\"" + ","
                    + "\n"
                 + "{"
                 + "\n"
                       + "\"filename\"" + ":" + "\"" + GenFilename.Replace(@"\", "/") + "\"" + ","

                    + "\n"

                        + "\"gdaldriver\"" + ":" + "\"" + Gdaldriver + "\"" + ","

                    + "\n"

                         + "\"output_type\"" + ":" + "\"" + OutputType + "\"" + ","

                    + "\n"

                         + "\"resolution\"" + ":" + "\"" + Resolution.ToString().Replace(',', '.') + "\"" + ","

                    + "\n"
                         + "\"data_type\"" + ":" + "\"" + DataType.ToString().Split('_')[1] + "\"" + ","

                    + "\n"

                         + "\"nodata\"" + ": " + Nodata + ","

                    + "\n"

                         + "\"type\"" + ": " + "\"" + Type + "\"" + ","

                    + "\n"

                         + "\"window_size\"" + ":" + WindowSize

                      + "\n"

                      + "}"

                      + "\n"

                      + "]"

                      + "\n"

                     + "}";

            return Pdalpipeline;

        }

        public void SaveToPdalJsonFile()
        {
            //Save Json
            var Jsonpath = Path.Combine(Application.persistentDataPath, JsonName);
            File.WriteAllText(Jsonpath, GenerateJson());
        }
        public string LoadPdalJsonFile()
        {
            //Load Json
            var Jsonpath = Path.Combine(Application.persistentDataPath, JsonName);
            string jsonFile = File.ReadAllText(Jsonpath);

            return jsonFile;
        }

    }
    public enum OutputType
    {
        max = 0,
        min,
        mean,
        idw,
        count,
        stdev

    }
    public enum GdalDriver
    {
        GTiff = 0
    }
    public enum DataType
    {
        m_float = 0,
        m_int16 = 1
    }
}

namespace GISTech.GISTerrainLoader
{
    public class LasHeader
    {
#region LASHeader
        public string _fileSignature = string.Empty;
        public ushort _fileSourceId;
        public ushort _globalEncoding;
        public uint _guiDdata1;
        public ushort _guiDdata2;
        public ushort _guiDdata3;
        public readonly byte[] _guiDdata4 = new byte[8];
        public byte _versionMajor;
        public byte _versionMinor;
        public string _systemIdentifier = string.Empty;
        public string _generatingSoftware = string.Empty;
        public ushort _fileCreationDayOfYear;
        public ushort _fileCreationYear;
        public ushort _headerSize;
        public uint _offsetToPointData;
        public uint _numberOfVariableLengthRecords;
        public byte _pointDataFormatId;
        public ushort _pointDataRecordLength;
        public uint _numberOfPointRecords;
        public readonly uint[] _numberOfPointsByReturn = new uint[5];
        public double _scaleFactorX;
        public double _scaleFactorY;
        public double _scaleFactorZ;
        public double _offsetX;
        public double _offsetY;
        public double _offsetZ;
        public double _maxX;
        public double _minX;
        public double _maxY;
        public double _minY;
        public double _maxZ;
        public double _minZ;
#endregion

        public string Projection;
        public int ESPGCode;
        public int Zone;
        public string Datum;
        public string Ellps;
        public string Unite;

        public DVector2 TL_LatLon;
        public DVector2 DR_LatLon;

        int SIZE = 227;
        public LasHeader(string filepath, pdal.Pipeline pipeline)
        {
            string _fileSignature = string.Empty;

            byte[] rawBytes;

            using (var binaryReader = new BinaryReader(File.Open(filepath, FileMode.Open)))
            {
                rawBytes = binaryReader.ReadBytes(SIZE);
            }

            var position = 0;
            for (var i = 0; i < 4; i++)
                _fileSignature += (char)rawBytes[position + i];
            position += 4;


            _fileSourceId = BitConverter.ToUInt16(rawBytes, position);
            position += sizeof(ushort);

            _globalEncoding = BitConverter.ToUInt16(rawBytes, position);
            position += sizeof(ushort);

            _guiDdata1 = BitConverter.ToUInt32(rawBytes, position);
            position += sizeof(uint);

            _guiDdata2 = BitConverter.ToUInt16(rawBytes, position);
            position += sizeof(ushort);

            _guiDdata3 = BitConverter.ToUInt16(rawBytes, position);
            position += sizeof(ushort);

            for (var i = 0; i < _guiDdata4.Length; i++)
                _guiDdata4[i] = rawBytes[position + i];
            position += _guiDdata4.Length;

            _versionMajor = rawBytes[position];
            position += 1;

            _versionMinor = rawBytes[position];
            position += 1;

            for (var i = 0; i < 32; i++)
            {
                if (rawBytes[position + i] == 0) break;
                _systemIdentifier += (char)rawBytes[position + i];
            }
            position += 32;

            for (var i = 0; i < 32; i++)
            {
                if (rawBytes[position + i] == 0) break;
                _generatingSoftware += (char)rawBytes[position + i];
            }
            position += 32;

            _fileCreationDayOfYear = BitConverter.ToUInt16(rawBytes, position);
            position += sizeof(ushort);

            _fileCreationYear = BitConverter.ToUInt16(rawBytes, position);
            position += sizeof(ushort);

            _headerSize = BitConverter.ToUInt16(rawBytes, position);
            position += sizeof(ushort);

            _offsetToPointData = BitConverter.ToUInt32(rawBytes, position);
            position += sizeof(uint);

            _numberOfVariableLengthRecords = BitConverter.ToUInt32(rawBytes, position);
            position += sizeof(uint);

            _pointDataFormatId = rawBytes[position];
            position += 1;

            _pointDataRecordLength = BitConverter.ToUInt16(rawBytes, position);
            position += sizeof(ushort);

            _numberOfPointRecords = BitConverter.ToUInt32(rawBytes, position);
            position += sizeof(uint);

            for (var i = 0; i < _numberOfPointsByReturn.Length; i++)
                _numberOfPointsByReturn[i] = BitConverter.ToUInt32(rawBytes, position + i * 4);
            position += _numberOfPointsByReturn.Length * sizeof(uint);

            _scaleFactorX = BitConverter.ToDouble(rawBytes, position);
            position += sizeof(double);

            _scaleFactorY = BitConverter.ToDouble(rawBytes, position);
            position += sizeof(double);

            _scaleFactorZ = BitConverter.ToDouble(rawBytes, position);
            position += sizeof(double);

            _offsetX = BitConverter.ToDouble(rawBytes, position);
            position += sizeof(double);

            _offsetY = BitConverter.ToDouble(rawBytes, position);
            position += sizeof(double);

            _offsetZ = BitConverter.ToDouble(rawBytes, position);
            position += sizeof(double);

            _maxX = BitConverter.ToDouble(rawBytes, position);
            position += sizeof(double);

            _minX = BitConverter.ToDouble(rawBytes, position);
            position += sizeof(double);

            _maxY = BitConverter.ToDouble(rawBytes, position);
            position += sizeof(double);

            _minY = BitConverter.ToDouble(rawBytes, position);
            position += sizeof(double);

            _maxZ = BitConverter.ToDouble(rawBytes, position);
            position += sizeof(double);

            _minZ = BitConverter.ToDouble(rawBytes, position);



            // Read Projection
            GISTerrainLoaderProjectionReader projReader = new GISTerrainLoaderProjectionReader(filepath, PdalRader.LAS);

            Projection = projReader.Projection;
            Zone = projReader.Zone;
            Ellps = projReader.Ellps;
            Unite = projReader.Unite;

            Datum = projReader.Datum;
            ESPGCode = projReader.ESPGCode;


            TL_LatLon = new DVector2(_minX, _maxY);
            DR_LatLon=new DVector2(_maxX, _minY);


            TL_LatLon = GeoRefConversion.ConvertTOLatLon(projReader, TL_LatLon);
            DR_LatLon = GeoRefConversion.ConvertTOLatLon(projReader, DR_LatLon);



        }
        public void GetLasData(GISTerrainLoaderFileData data)
        {
            data.TopLeftPoint = new DVector2(TL_LatLon.x,TL_LatLon.y);
            data.DownRightPoint = new DVector2(DR_LatLon.x,DR_LatLon.y);

            data.DownLeftPoint = new DVector2(data.TopLeftPoint.x, data.DownRightPoint.y);
            data.TopRightPoint = new DVector2(data.DownRightPoint.x, data.TopLeftPoint.y);

            var p1 = new DVector2(data.TopRightPoint.x, data.DownLeftPoint.y);
            var p2 = new DVector2(data.DownLeftPoint.x, data.TopRightPoint.y);

            data.Terrain_Dimension.x = GeoRefConversion.Getdistance(data.DownLeftPoint.y, data.DownLeftPoint.x, p1.y, p1.x) * 10;
            data.Terrain_Dimension.y = GeoRefConversion.Getdistance(data.DownLeftPoint.y, data.DownLeftPoint.x, p2.y, p2.x) * 10;

            data.AlreadyLoaded = true;

        }

    }

}
#endif