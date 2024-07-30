/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderShapeReader
    {
        public static List<GISTerrainLoaderShpFile> LoadShapes(string Terrainpath)
        {
            List<GISTerrainLoaderShpFile> shapesFile = new List<GISTerrainLoaderShpFile>();
 
            var shapes = GetShapeFile(Terrainpath);

            if(shapes.Length>0)
            {
                foreach (var shp in shapes)
                {
                    var shpfile = LoadFiles(shp) as GISTerrainLoaderShpFile;
                    shpfile.ReadProjection(shp);
                    shapesFile.Add(shpfile);
                }
                return shapesFile;
            }
            else return null;
        }

       public static GISTerrainLoaderIFile LoadFiles(string path)
        {
            try
            {
                GISTerrainLoaderIFile file;
                string fileExt = Path.GetExtension(path);
                file = GISTerrainLoaderFileFactory.CreateInstance(path);
                file.Load();
                return file;
            }
            catch (Exception e)
            {
                if (path.Length == 0)
                {
                    Debug.Log("Path is empty.");
                    return null;
                }
                Debug.Log(e);
                return null;
            }
        }
        private static string[] GetShapeFile(string terrainPath)
        {
             var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
            var VectorFolder = Path.Combine(Path.GetDirectoryName(terrainPath), TerrainFilename + "_VectorData");
            string[] tiles = null;

            if (Directory.Exists(VectorFolder))
            {
                var supportedExtensions = new HashSet<string> { ".shp" };
                tiles = Directory.GetFiles(VectorFolder, "*.*", SearchOption.AllDirectories).Where(f => supportedExtensions.Contains(new FileInfo(f).Extension, StringComparer.OrdinalIgnoreCase)).ToArray();
            }
            else
                Debug.LogError("VectorData directory not exist");

            return tiles;
        }

    }

    #region ShapeFile
    public class GISTerrainLoaderShpFile : GISTerrainLoaderIShpFile
    {
        private bool disposed;
        private FileStream fs;
        private BinaryReader br;

        public string FilePath { get; set; }
        public int FileCode { get; set; }
        public int FileLength { get; set; }
        public int FileVersion { get; set; }
        public ShapeType ShpType { get; set; }
        public GISTerrainLoaderRangeXY TotalXYRange { get; set; }
        public GISTerrainLoaderRange ZRange { get; set; }
        public GISTerrainLoaderRange MRange { get; set; }
        public int ContentLength { get; set; }
        public List<GISTerrainLoaderShpRecord> RecordSet { get; set; }

        public GTLGeographicCoordinateSystem CoordinateReferenceSystem = null;

        public GISTerrainLoaderShpFile()
        {
          
        }
        public GISTerrainLoaderShpFile(string path)
        {

            FilePath = path;
            fs = File.OpenRead(path);
            br = new BinaryReader(fs);

        }
        public void ReadProjection(string path)
        {
            CoordinateReferenceSystem = GISTerrainLoaderProjectionReader.ReadProjectionFile(path);
         }

        ~GISTerrainLoaderShpFile()  
        {
            Dispose(false);
        }

        public void Load()
        {

            FileCode = GISTerrainLoaderExtensions.FromBigEndian(br.ReadInt32());
            br.BaseStream.Seek(20, SeekOrigin.Current);
            FileLength = GISTerrainLoaderExtensions.FromBigEndian(br.ReadInt32()) * 2;
            FileVersion = br.ReadInt32();
            ShpType = (ShapeType)br.ReadInt32();
 
            TotalXYRange = new GISTerrainLoaderRangeXY();
            ZRange = new GISTerrainLoaderRange();
            MRange = new GISTerrainLoaderRange();
            TotalXYRange.Load(ref br);
            ZRange.Load(ref br);
            MRange.Load(ref br);

            ContentLength = FileLength - 100;

            long curPoint = 0;

            RecordSet = new List<GISTerrainLoaderShpRecord>();

            while (curPoint < ContentLength)
            {
              
                GISTerrainLoaderShpRecord record = new GISTerrainLoaderShpRecord(ShpType);
                record.Load(ref br);
                long size = record.GetLength();
                RecordSet.Add(record);

                curPoint += record.GetLength();

            }
            br.Close();
        }

        public GISTerrainLoaderIRecord GetData(int index)
        {
            return RecordSet.ElementAt(index);
        }

        public GISTerrainLoaderIRecord GetData(ShapeType type, int offset, int length)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            GISTerrainLoaderIRecord record = new GISTerrainLoaderShpRecord(type);
            record.Load(ref br);
            return record;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                br.Dispose();
                fs.Dispose();
            }
            disposed = true;
        }
    }
    public interface GISTerrainLoaderIShpFile : GISTerrainLoaderIFile
    {
        int FileCode { get; set; }
        int FileLength { get; set; }
        int FileVersion { get; set; }
        ShapeType ShpType { get; set; }
        GISTerrainLoaderRangeXY TotalXYRange { get; set; }
        GISTerrainLoaderRange ZRange { get; set; }
        GISTerrainLoaderRange MRange { get; set; }
        int ContentLength { get; set; }
    }
    public class GISTerrainLoaderShapeFactory
    {
        public static readonly IDictionary<ShapeType, Func<GISTerrainLoaderIElement>> Creators =
            new Dictionary<ShapeType, Func<GISTerrainLoaderIElement>>()
            {
                { ShapeType.Point, () => new GISTerrainLoaderShapePoint() },
                { ShapeType.PolyLine, () => new PolyLine() },
                { ShapeType.Polygon, () => new Polygon() },
                { ShapeType.MultiPoint, () => new MultiPoint() },
                { ShapeType.PointM, () => new PointM() },
                { ShapeType.PolyLineM, () => new PolyLineM() },
                { ShapeType.PolygonM, () => new PolygonM() },
                { ShapeType.MultiPointM, () => new MultiPointM() },
                { ShapeType.PointZ, () => new PointZ() },
                { ShapeType.PolyLineZ, () => new PolyLineZ() },
                { ShapeType.PolygonZ, () => new PolygonZ() },
                { ShapeType.MultiPointZ, () => new MultiPointZ() },
                { ShapeType.MultiPatch, () => new MultiPatch() }
            };
       

        public static GISTerrainLoaderShapePoint[] GetTypePoint(ShapeType type, GISTerrainLoaderIElement Contents)
        {
            GISTerrainLoaderShapePoint[] points = null;
            switch (type)
            {
                case ShapeType.Point:
                    var p = (GISTerrainLoaderShapePoint)Contents;
                    var pp = new GISTerrainLoaderShapePoint(); pp.X = p.X; pp.Y = p.Y;
                    points = new GISTerrainLoaderShapePoint[1] { pp };
                    break;
                case ShapeType.PolyLine:
                    var PolyLine = (PolyLine)Contents;
                    points = PolyLine.Points;
                    break;
                case ShapeType.Polygon:
                    var Polygon = (Polygon)Contents as Polygon;
                    points = Polygon.Points;
                    break;
                case ShapeType.MultiPoint:
                    var MultiPoint = (MultiPoint)Contents as MultiPoint;
                    points = MultiPoint.Points;
                    break;
                case ShapeType.PolyLineZ:
                    var PolyLineZ = (PolyLineZ)Contents as PolyLineZ;
                    points = PolyLineZ.Points;
                    break;
                case ShapeType.PolygonZ:
                    var PolygonZ = (PolygonZ)Contents as PolygonZ;
                    points = PolygonZ.Points;
                    break;
            }
            return points;
        }
        public static double[] GetElevation(ShapeType type, GISTerrainLoaderIElement Contents)
        {
            double[] Elevation = null;
            switch (type)
            {
                case ShapeType.Point:
                    var p = (GISTerrainLoaderShapePoint)Contents;
                    var pp = new GISTerrainLoaderShapePoint(); pp.X = p.X; pp.Y = p.Y;
                    break;
                case ShapeType.PolyLine:
                    var PolyLine = (PolyLine)Contents;
                    break;
                case ShapeType.Polygon:
                    var Polygon = (Polygon)Contents as Polygon;
                    break;
                case ShapeType.MultiPoint:
                    var MultiPoint = (MultiPoint)Contents as MultiPoint;
                    break;
                case ShapeType.PolyLineZ:
                    var PolyLineZ = (PolyLineZ)Contents as PolyLineZ;
                    Elevation = PolyLineZ.ZValues;
                    break;
                case ShapeType.PolygonZ:
                    var PolygonZ = (PolygonZ)Contents as PolygonZ;
                    Elevation = PolygonZ.ZValues;
                    break;
            }
            return Elevation;
        }


        public static GISTerrainLoaderIElement CreateInstance(ShapeType shapeType)
        {
            return Creators[shapeType]();
        }
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1), Serializable]
    public class GISTerrainLoaderShpRecord : GISTerrainLoaderIRecord 
    {
        public GISTerrainLoaderShpRecordHeader Header { get; set; }
        public GISTerrainLoaderIElement Contents { get; set; }
        public string Tag { get; set; }
        public List<GISTerrainLoaderShpDataBase> DataBase { get; set; }
        public Dictionary<Enum, string> Tags
        {
            get;
            set;
        }
        public GISTerrainLoaderShpRecord(ShapeType type)
        {
            Header = new GISTerrainLoaderShpRecordHeader();
            Contents = GISTerrainLoaderShapeFactory.CreateInstance(type);
            Tags = new Dictionary<Enum, string>();
        }

        public void Load(ref BinaryReader br)
        {
            Header.Load(ref br);
            Contents.Load(ref br);
        }

        public long GetLength()
        {
            return Header.GetLength() + Contents.GetLength();
        }
 
    }
    public class GISTerrainLoaderShpDataBase
    {
        public string Col_Name;
        public string Row_Value;
        public bool Generated = false;
        public GISTerrainLoaderShpDataBase(string m_Col_Name, string m_Row_Value)
        {
               Col_Name = m_Col_Name;
               Row_Value = m_Row_Value;
        }

    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1), Serializable]
    public class GISTerrainLoaderShpRecordHeader : GISTerrainLoaderIRecord
    {
        public int RecordNumber { get; set; }
        public int ContentLength { get; set; }
        public ShapeType Type { get; set; }

        public void Load(ref BinaryReader br)
        {
            RecordNumber = GISTerrainLoaderExtensions.FromBigEndian(br.ReadInt32());
            ContentLength = GISTerrainLoaderExtensions.FromBigEndian(br.ReadInt32()) * 2;
            Type = (ShapeType)br.ReadInt32();
        }

        public long GetLength()
        {
            return Marshal.SizeOf(this);
        }
    }
    #endregion
    public interface GISTerrainLoaderIFile
    {
        void Load();
        GISTerrainLoaderIRecord GetData(int index);
    }
    public class GISTerrainLoaderRangeXY : GISTerrainLoaderIElement
    {
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }

        public double Width { get { return MaxX - MinX; } }
        public double Height { get { return MaxY - MinY; } }

        public void Load(ref BinaryReader br)
        {
            MinX = br.ReadDouble();
            MaxX = br.ReadDouble();
            MinY = br.ReadDouble();
            MaxY = br.ReadDouble();
        }

        public long GetLength()
        {
            return sizeof(double) * 4;
        }
    }
    public class GISTerrainLoaderRange : GISTerrainLoaderIElement
    {
        public double Min { get; set; }
        public double Max { get; set; }

        public void Load(ref BinaryReader br)
        {
            Min = br.ReadDouble();
            Max = br.ReadDouble();
        }

        public long GetLength()
        {
            return sizeof(double) * 2;
        }
    }
    public class GISTerrainLoaderFileFactory
    {
        public static readonly IDictionary<string, Func<string, GISTerrainLoaderIFile>> Creators =
            new Dictionary<string, Func<string, GISTerrainLoaderIFile>>()
            {
                { ".shp", (path) => new GISTerrainLoaderShpFile(path) }              
            };

        public static GISTerrainLoaderIFile CreateInstance(string path)
        {
            return Creators[Path.GetExtension(path)](path);
        }
    }
    public interface GISTerrainLoaderIRecord
    {
        void Load(ref BinaryReader br);
        long GetLength();
    }
    public interface GISTerrainLoaderIElement
    {
        void Load(ref BinaryReader br);
        long GetLength();
    }

    #region Types
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class GISTerrainLoaderShapePoint : GISTerrainLoaderIElement
    {
        public double X { get; set; }
        public double Y { get; set; }
 
 
        public void Load(ref BinaryReader br)
        {
            X = br.ReadDouble();
            Y = br.ReadDouble();
        }

        public long GetLength()
        {
            return sizeof(double) * 2;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MultiPoint : GISTerrainLoaderIElement
    {
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumPoints { get; set; }
        public GISTerrainLoaderShapePoint[] Points { get; set; }

        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();
            XYRange.Load(ref br);
            NumPoints = br.ReadInt32();

            Points = new GISTerrainLoaderShapePoint[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new GISTerrainLoaderShapePoint();
                Points[i].Load(ref br);
            }
        }

        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += GISTerrainLoaderExtensions.GetArraySize(Points);
            return size;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PolyLine : GISTerrainLoaderIElement 
    {
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumParts { get; set; }
        public int NumPoints { get; set; }
        public int[] Parts { get; set; }
        public GISTerrainLoaderShapePoint[] Points { get; set; }

        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();
            XYRange.Load(ref br);
            NumParts = br.ReadInt32();
            NumPoints = br.ReadInt32();

            Parts = new int[NumParts];
            for (int i = 0; i < NumParts; i++)
            {
                Parts[i] = br.ReadInt32();
            }

            Points = new GISTerrainLoaderShapePoint[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new GISTerrainLoaderShapePoint();
                Points[i].Load(ref br);
            }
        }

        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += sizeof(int);
            size += GISTerrainLoaderExtensions.GetArraySize(Parts);
            size += GISTerrainLoaderExtensions.GetArraySize(Points);
            return size;
        }
 
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Polygon : GISTerrainLoaderIElement 
    {
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumParts { get; set; }
        public int NumPoints { get; set; }
        public int[] Parts { get; set; }
        public GISTerrainLoaderShapePoint[] Points { get; set; }

        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();
            XYRange.Load(ref br);
            NumParts = br.ReadInt32();
            NumPoints = br.ReadInt32();

            Parts = new int[NumParts];
            for (int i = 0; i < NumParts; i++)
            {
                Parts[i] = br.ReadInt32();
            }

            Points = new GISTerrainLoaderShapePoint[NumPoints];

            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new GISTerrainLoaderShapePoint();
                Points[i].Load(ref br);
            }
        }

        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += sizeof(int);
            size += GISTerrainLoaderExtensions.GetArraySize(Parts);
            size += GISTerrainLoaderExtensions.GetArraySize(Points);
            return size;
        }
 
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PointM : GISTerrainLoaderIElement
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double M { get; set; }

        public void Load(ref BinaryReader br)
        {
            X = br.ReadDouble();
            Y = br.ReadDouble();
            M = br.ReadDouble();
        }

        public long GetLength()
        {
            long size = 0;
            size += sizeof(double) * 3;
            return size;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MultiPointM : GISTerrainLoaderIElement
    {
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumPoints { get; set; }
        public GISTerrainLoaderShapePoint[] Points { get; set; }
        public GISTerrainLoaderRange MRange { get; set; }
        public double[] MValues { get; set; }

        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();
            XYRange.Load(ref br);
            NumPoints = br.ReadInt32();

            Points = new GISTerrainLoaderShapePoint[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new GISTerrainLoaderShapePoint();
                Points[i].Load(ref br);
            }

            MRange = new GISTerrainLoaderRange();
            MRange.Load(ref br);

            MValues = new double[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                MValues[i] = br.ReadDouble();
            }
        }

        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += GISTerrainLoaderExtensions.GetArraySize(Points);
            size += MRange.GetLength();
            size += GISTerrainLoaderExtensions.GetArraySize(MValues);
            return size;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PolyLineM : GISTerrainLoaderIElement 
    {
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumParts { get; set; }
        public int NumPoints { get; set; }
        public int[] Parts { get; set; }
        public GISTerrainLoaderShapePoint[] Points { get; set; }
        public GISTerrainLoaderRange MRange { get; set; }
        public double[] MValues { get; set; }

        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();
            XYRange.Load(ref br);
            NumParts = br.ReadInt32();
            NumPoints = br.ReadInt32();

            Parts = new int[NumParts];
            for (int i = 0; i < NumParts; i++)
            {
                Parts[i] = br.ReadInt32();
            }

            Points = new GISTerrainLoaderShapePoint[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new GISTerrainLoaderShapePoint();
                Points[i].Load(ref br);
            }

            MRange = new GISTerrainLoaderRange();
            MRange.Load(ref br);

            MValues = new double[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                MValues[i] = br.ReadDouble();
            }
        }

        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += sizeof(int);
            size += GISTerrainLoaderExtensions.GetArraySize(Parts);
            size += GISTerrainLoaderExtensions.GetArraySize(Points);
            size += MRange.GetLength();
            size += GISTerrainLoaderExtensions.GetArraySize(MValues);
            return size;
        }
 
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PolygonM : GISTerrainLoaderIElement 
    {
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumParts { get; set; }
        public int NumPoints { get; set; }
        public int[] Parts { get; set; }
        public GISTerrainLoaderShapePoint[] Points { get; set; }
        public GISTerrainLoaderRange MRange { get; set; }
        public double[] MValues { get; set; }

        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();
            XYRange.Load(ref br);
            NumParts = br.ReadInt32();
            NumPoints = br.ReadInt32();
            Parts = new int[NumParts];
            Points = new GISTerrainLoaderShapePoint[NumPoints];
            for (int i = 0; i < NumParts; i++)
            {
                Parts[i] = br.ReadInt32();
            }
            for (int i = 0; i < NumPoints; i++)
            {
                Points[i].Load(ref br);
            }
            MRange.Load(ref br);
            for (int i = 0; i < NumPoints; i++)
            {
                MValues[i] = br.ReadDouble();
            }
        }

        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += sizeof(int);
            size += GISTerrainLoaderExtensions.GetArraySize(Parts);
            size += GISTerrainLoaderExtensions.GetArraySize(Points);
            size += MRange.GetLength();
            size += GISTerrainLoaderExtensions.GetArraySize(MValues);
            return size;
        }
 
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PointZ : GISTerrainLoaderIElement
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double M { get; set; }

        public void Load(ref BinaryReader br)
        {
            X = br.ReadDouble();
            Y = br.ReadDouble();
            Z = br.ReadDouble();
            M = br.ReadDouble();
        }

        public long GetLength()
        {
            return sizeof(double) * 4;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MultiPointZ : GISTerrainLoaderIElement
    {
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumPoints { get; set; }
        public GISTerrainLoaderShapePoint[] Points { get; set; }
        public GISTerrainLoaderRange ZRange { get; set; }
        public double[] ZValues { get; set; }
        public GISTerrainLoaderRange MRange { get; set; }
        public double[] MValues { get; set; }

        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();
            XYRange.Load(ref br);
            NumPoints = br.ReadInt32();

            Points = new GISTerrainLoaderShapePoint[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new GISTerrainLoaderShapePoint();
                Points[i].Load(ref br);
            }

            ZRange = new GISTerrainLoaderRange();
            ZRange.Load(ref br);

            ZValues = new double[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                ZValues[i] = br.ReadDouble();
            }

            MRange = new GISTerrainLoaderRange();
            MRange.Load(ref br);

            MValues = new double[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                MValues[i] = br.ReadDouble();
            }
        }

        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += GISTerrainLoaderExtensions.GetArraySize(Points);
            size += ZRange.GetLength();
            size += GISTerrainLoaderExtensions.GetArraySize(ZValues);
            size += MRange.GetLength();
            size += GISTerrainLoaderExtensions.GetArraySize(MValues);
            return size;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PolyLineZ : GISTerrainLoaderIElement 
    {
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumParts { get; set; }
        public int NumPoints { get; set; }
        public int[] Parts { get; set; }
        public GISTerrainLoaderShapePoint[] Points { get; set; }
        public GISTerrainLoaderRange ZRange { get; set; }
        public double[] ZValues { get; set; }
        public GISTerrainLoaderRange MRange { get; set; }
        public double[] MValues { get; set; }

        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();
            XYRange.Load(ref br);
            NumParts = br.ReadInt32();
            NumPoints = br.ReadInt32();

            Parts = new int[NumParts];
            for (int i = 0; i < NumParts; i++)
            {
                Parts[i] = br.ReadInt32();
            }

            Points = new GISTerrainLoaderShapePoint[NumPoints];

            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new GISTerrainLoaderShapePoint();
                Points[i].Load(ref br);
            }

            ZRange = new GISTerrainLoaderRange();

            ZRange.Load(ref br);

            ZValues = new double[NumPoints];

            for (int i = 0; i < NumPoints; i++)
            {
                ZValues[i] = br.ReadDouble();
            }

        }

        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += sizeof(int);
            size += GISTerrainLoaderExtensions.GetArraySize(Parts);
            size += GISTerrainLoaderExtensions.GetArraySize(Points);
            size += ZRange.GetLength();
            size += GISTerrainLoaderExtensions.GetArraySize(ZValues);
            return size;
        }
 
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PolygonZ : GISTerrainLoaderIElement 
    {
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumParts { get; set; }
        public int NumPoints { get; set; }
        public int[] Parts { get; set; }
        public GISTerrainLoaderShapePoint[] Points { get; set; }
        public GISTerrainLoaderRange ZRange { get; set; }
        public double[] ZValues { get; set; }
        public GISTerrainLoaderRange MRange { get; set; }
        public double[] MValues { get; set; }

        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();
            XYRange.Load(ref br);
            NumParts = br.ReadInt32();
            NumPoints = br.ReadInt32();

            Parts = new int[NumParts];
            for (int i = 0; i < NumParts; i++)
            {
                Parts[i] = br.ReadInt32();
            }

            Points = new GISTerrainLoaderShapePoint[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new GISTerrainLoaderShapePoint();
                Points[i].Load(ref br);
            }

            ZRange = new GISTerrainLoaderRange();
            ZRange.Load(ref br);

            ZValues = new double[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                ZValues[i] = br.ReadDouble();

            }

        }

        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += sizeof(int);
            size += GISTerrainLoaderExtensions.GetArraySize(Parts);
            size += GISTerrainLoaderExtensions.GetArraySize(Points);
            size += ZRange.GetLength();
            size += GISTerrainLoaderExtensions.GetArraySize(ZValues);
            return size;
        }
 
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MultiPatch : GISTerrainLoaderIElement 
    {
        public GISTerrainLoaderRangeXY XYRange { get; set; }
        public int NumParts { get; set; }
        public int NumPoints { get; set; }
        public int[] Parts { get; set; }
        public int[] PartsTypes { get; set; }
        public GISTerrainLoaderShapePoint[] Points { get; set; }
        public GISTerrainLoaderRange ZRange { get; set; }
        public double[] ZValues { get; set; }
        public GISTerrainLoaderRange MRange { get; set; }
        public double[] MValues { get; set; }

        public void Load(ref BinaryReader br)
        {
            XYRange = new GISTerrainLoaderRangeXY();
            XYRange.Load(ref br);
            NumParts = br.ReadInt32();
            NumPoints = br.ReadInt32();

            Parts = new int[NumParts];
            for (int i = 0; i < NumParts; i++)
            {
                Parts[i] = br.ReadInt32();
            }

            PartsTypes = new int[NumParts];
            for (int i = 0; i < NumParts; i++)
            {
                PartsTypes[i] = br.ReadInt32();
            }

            Points = new GISTerrainLoaderShapePoint[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                Points[i] = new GISTerrainLoaderShapePoint();
                Points[i].Load(ref br);
            }

            ZRange = new GISTerrainLoaderRange();
            ZRange.Load(ref br);

            ZValues = new double[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                ZValues[i] = br.ReadDouble();
            }

            MRange = new GISTerrainLoaderRange();
            MRange.Load(ref br);

            MValues = new double[NumPoints];
            for (int i = 0; i < NumPoints; i++)
            {
                MValues[i] = br.ReadDouble();
            }
        }

        public long GetLength()
        {
            long size = 0;
            size += XYRange.GetLength();
            size += sizeof(int);
            size += sizeof(int);
            size += GISTerrainLoaderExtensions.GetArraySize(Parts);
            size += GISTerrainLoaderExtensions.GetArraySize(PartsTypes);
            size += GISTerrainLoaderExtensions.GetArraySize(Points);
            size += ZRange.GetLength();
            size += GISTerrainLoaderExtensions.GetArraySize(ZValues);
            size += MRange.GetLength();
            size += GISTerrainLoaderExtensions.GetArraySize(MValues);
            return size;
        }
 
    }




    public enum ShapeType : int
    {
        Null = 0,
        Point = 1,
        PolyLine = 3,
        Polygon = 5,
        MultiPoint = 8,
        PointZ = 11,
        PolyLineZ = 13,
        PolygonZ = 15,
        MultiPointZ = 18,
        PointM = 21,
        PolyLineM = 23,
        PolygonM = 25,
        MultiPointM = 28,
        MultiPatch = 31
    }
    #endregion





}
