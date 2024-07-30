/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;


namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderExtensions
    {
        public static string EditorCheckForOSMFile(string TerrainFilePath, string TerrainFileName, out bool exist)
        {
            exist = false;
            string osmfile = "";

            DirectoryInfo di = new DirectoryInfo(TerrainFilePath);

            var VectorFolderPath = TerrainFileName + "_VectorData";

            for (int i = 0; i <= 5; i++)
            {
                di = di.Parent;

                VectorFolderPath = di.Name + "/" + VectorFolderPath;

                //If Directory GIS Terrains Exist
                if (di.Name == "GIS Terrains")
                {
                    var MainfolderPath = Path.GetDirectoryName(TerrainFilePath);
                    var VectorDataFolder = Path.Combine(MainfolderPath, TerrainFileName + "_VectorData");

                    osmfile = VectorDataFolder + "/" + TerrainFileName + ".osm";

                    if (File.Exists(osmfile))
                    {
                        exist = true;
                    }
                    else
                        Debug.LogError("Osm File Not Found : Please put your terrain in GIS Terrain Loader/Recources/GIS Terrains/TerrainFileName_VectorData/TerrainFileName.osm  " + osmfile);

                    break;
                }


                if (i == 5)
                {
                    exist = false;
                    Debug.LogError("Vector folder not found! : Please put your terrain in GIS Terrain Loader/Recources/GIS Terrains/");
                }

            }
            return osmfile;
        }
        public static string[] GetOSMFiles(string terrainPath)
        {
            var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
            var VectorFolder = Path.Combine(Path.GetDirectoryName(terrainPath), TerrainFilename + "_VectorData");

            string[] tiles = null;

            if (Directory.Exists(VectorFolder))
            {
                var supportedExtensions = new HashSet<string> { ".osm" };
                tiles = Directory.GetFiles(VectorFolder, "*.*", SearchOption.AllDirectories).Where(f => supportedExtensions.Contains(new FileInfo(f).Extension, StringComparer.OrdinalIgnoreCase)).ToArray();

            }
            else
                Debug.LogError("VectorData directory not exist");

            return tiles;
        }
        public static string RuntimeCheckForOSMFile(string TerrainFilePath, string TerrainFileName, out bool exist)
        {
            exist = false;
            string osmfile = "";

            DirectoryInfo di = new DirectoryInfo(TerrainFilePath);



            var MainfolderPath = Path.GetDirectoryName(TerrainFilePath);

            var VectorFolderPath = MainfolderPath + "/" + TerrainFileName + "_VectorData";


            if (Directory.Exists(VectorFolderPath))
            {
                osmfile = VectorFolderPath + "/" + TerrainFileName + ".osm";

                if (File.Exists(osmfile))
                {
                    exist = true;
                }
                else
                    Debug.LogError("Osm File Not Found : Please put your terrain in Path../TerrainFolder/TerrainFileName_VectorData/TerrainFileName.osm  " + osmfile);

            }
            else
            {
                exist = false;
                Debug.LogError("OSM Vector folder not found!");
            }

            return osmfile;
        }
        public static double ConvertToDouble(string s)
        {
            char systemSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];
            double result = 0;
            try
            {
                if (s != null)
                    if (!s.Contains(","))
                        result = double.Parse(s, CultureInfo.InvariantCulture);
                    else
                        result = Convert.ToDouble(s.Replace(".", systemSeparator.ToString()).Replace(",", systemSeparator.ToString()));
            }
            catch (Exception e)
            {
                try
                {
                    result = Convert.ToDouble(s);
                }
                catch
                {
                    try
                    {
                        result = Convert.ToDouble(s.Replace(",", ";").Replace(".", ",").Replace(";", "."));
                    }
                    catch
                    {
                        throw new Exception("Wrong string-to-double format  :" + e.Message);
                    }
                }
            }
            return result;
        }

        public static bool IsPointInPolygon(Vector3[] poly, float x, float y)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if ((poly[i].z <= y && y < poly[j].z ||
                     poly[j].z <= y && y < poly[i].z) &&
                    x < (poly[j].x - poly[i].x) * (y - poly[i].z) / (poly[j].z - poly[i].z) + poly[i].x)
                {
                    c = !c;
                }
            }
            return c;
        }
        public static bool IsPointInPolygon(List<DVector3> poly, double x, double y)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                if ((poly[i].z <= y && y < poly[j].z ||
                     poly[j].z <= y && y < poly[i].z) &&
                    x < (poly[j].x - poly[i].x) * (y - poly[i].z) / (poly[j].z - poly[i].z) + poly[i].x)
                {
                    c = !c;
                }
            }
            return c;
        }
        public static Rect GetRectFromPoints(List<Vector3> points)
        {
            return new Rect
            {
                x = points.Min(p => p.x),
                y = points.Min(p => p.z),
                xMax = points.Max(p => p.x),
                yMax = points.Max(p => p.z)
            };
        }
        public static Rect GetRectFromPoints(List<DVector3> points)
        {
            return new Rect
            {
                x = points.Min(p => (float)p.x),
                y = points.Min(p => (float)p.z),
                xMax = points.Max(p => (float)p.x),
                yMax = points.Max(p => (float)p.z)
            };
        }

        public static bool IsSubRegionIncluded(DVector2 FileUpperLeftCoordiante, DVector2 FileDownRightCoordiante, DVector2 SubRegionUpperLeftCoordiante, DVector2 SubRegionDownRightCoordiante)
        {
            bool Included = true;

            if (SubRegionUpperLeftCoordiante.x >= SubRegionDownRightCoordiante.x)
            {
                Debug.LogError("Down-Right Longitude must be greater than Top-Left Longitude");
                Included = false;
            }
            if (SubRegionUpperLeftCoordiante.y <= SubRegionDownRightCoordiante.y)
            {
                Debug.LogError("Top-Left Latitude must be greater than Bottom-Right Latitude");
                Included = false;
            }
            //-------
            if (SubRegionUpperLeftCoordiante.x < FileUpperLeftCoordiante.x)
            {
                Debug.LogError("Sub region Top-Left Longitude must be greater or equal than file Top-Left Longitude");
                Included = false;
            }

            if (SubRegionUpperLeftCoordiante.y > FileUpperLeftCoordiante.y)
            {
                Debug.LogError("Sub region Top-Left Latitude must be smaller or equal than file Top-Left Latitude");
                Included = false;
            }
            //-------
            if (SubRegionDownRightCoordiante.x > FileDownRightCoordiante.x)
            {
                Debug.LogError("Sub region Top-Left Longitude must be smaller or equal than file Top-Left Longitude");
                Included = false;
            }

            if (SubRegionDownRightCoordiante.y < FileDownRightCoordiante.y)
            {
                Debug.LogError("Sub region Down-Right Latitude must be greater or equal than file Top-Left Latitude");
                Included = false;
            }

            return Included;
        }
        public static Vector3 GetLocalLocation(GISTerrainLoaderFileData data, DVector2 point)
        {
            var rang_x = Math.Abs(Math.Abs(data.DownRightPoint.x) - Math.Abs(data.TopLeftPoint.x));
            var rang_y = Math.Abs(Math.Abs(data.TopLeftPoint.y) - Math.Abs(data.DownRightPoint.y));

            var rang_px = Math.Abs(Math.Abs(point.x) - Math.Abs(data.TopLeftPoint.x));
            var rang_py = Math.Abs(Math.Abs(data.TopLeftPoint.y) - Math.Abs(point.y));

            int localLat = (int)(rang_px * data.mapSize_col_x / rang_x);
            int localLon = (int)(rang_py * data.mapSize_row_y / rang_y);

            if (localLat > data.mapSize_col_x - 1) localLat = data.mapSize_col_x - 1;
            if (localLon > data.mapSize_row_y - 1) localLon = data.mapSize_row_y - 1;
            var elevation = data.floatheightData[localLat, localLon];

            return new Vector3(localLat, localLon, elevation);
        }

        public static short ToBigEndian(short value) => System.Net.IPAddress.HostToNetworkOrder(value);
        public static int ToBigEndian(int value) => System.Net.IPAddress.HostToNetworkOrder(value);
        public static long ToBigEndian(long value) => System.Net.IPAddress.HostToNetworkOrder(value);
        public static short FromBigEndian(short value) => System.Net.IPAddress.NetworkToHostOrder(value);
        public static int FromBigEndian(int value) => System.Net.IPAddress.NetworkToHostOrder(value);
        public static long FromBigEndian(long value) => System.Net.IPAddress.NetworkToHostOrder(value);
        public static long GetArraySize(Array arr)
        {
            return arr.LongLength * Marshal.SizeOf(arr.GetType().GetElementType());
        }



        public static Color HexToColor(string hex)
        {
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color32(r, g, b, 255);
        }


        public static bool OnlyHexInString(string test)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(test, @"\A\b[0-9a-fA-F]+\b\Z");
        }

        public static GameObject CreateMesh(TerrainContainerObject container, List<Vector3> points)
        {
            var buildingCorners = new List<Vector3>();

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 pointA = new Vector3(points[i].x, 0, points[i].z);
                buildingCorners.Add(pointA);
            }

            GameObject result = new GameObject();

            result.AddComponent<MeshRenderer>();
            MeshFilter mf = result.AddComponent<MeshFilter>();

            if (buildingCorners.Count > 2)
            {
                Mesh mesh = CreateMesh(buildingCorners);
                mf.mesh = mesh;
            }

            return result;
        }
        public static Mesh CreateMesh(List<Vector3> verts)
        {
            var tris = new GISTerrainLoaderTriangulator(verts.Select(x => new Vector2(x.x, x.z)).ToArray());
            var mesh = new Mesh();

            var vertices = verts.Select(x => new Vector3(x.x, x.y, x.z)).ToList();
            var indices = tris.Triangulate().ToList();

            var n = vertices.Count;
            for (int index = 0; index < n; index++)
            {
                var v = vertices[index];
                vertices.Add(new Vector3(v.x, v.y, v.z));
            }

            for (int i = 0; i < n - 1; i++)
            {
                indices.Add(i);
                indices.Add(i + n);
                indices.Add(i + n + 1);
                indices.Add(i);
                indices.Add(i + n + 1);
                indices.Add(i + 1);
            }

            indices.Add(n - 1);
            indices.Add(n);
            indices.Add(0);

            indices.Add(n - 1);
            indices.Add(n + n - 1);
            indices.Add(n);



            mesh.vertices = vertices.ToArray();
            mesh.triangles = indices.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        #region MathExt
        public static float SlopeCal(float Z_Pos_x, float Z_Pos_y)
        {
            float Z_Pos = Z_Pos_x * Z_Pos_x + Z_Pos_y * Z_Pos_y;
            float Sq_Pos = SqrtCal(Z_Pos);

            return Mathf.Atan(Sq_Pos) * (180.0f / (float)Math.PI) / 90.0f;
        }
        public static float SqrtCal(float Value)
        {
            if (Value <= 0.0f) return 0.0f;
            return (float)Math.Sqrt(Value);
        }
        public static float SignOrZero(float v)
        {
            if (v == 0) return 0;
            return Math.Sign(v);
        }
        #endregion




    }

    public static class TransformExtensions
    {
        /// <summary>
        /// Updates the local eulerAngles to a new vector3, if a value is omitted then the old value will be used.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public static void SetLocalEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var vector = new Vector3();
            if (x != null) { vector.x = x.Value; } else { vector.x = transform.localEulerAngles.x; }
            if (y != null) { vector.y = y.Value; } else { vector.y = transform.localEulerAngles.y; }
            if (z != null) { vector.z = z.Value; } else { vector.z = transform.localEulerAngles.z; }
            transform.localEulerAngles = vector;
        }

        /// <summary>
        /// Updates the position to a new vector3, if a value is omitted then the old value will be used.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public static void SetPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var vector = new Vector3();
            if (x != null) { vector.x = x.Value; } else { vector.x = transform.position.x; }
            if (y != null) { vector.y = y.Value; } else { vector.y = transform.position.y; }
            if (z != null) { vector.z = z.Value; } else { vector.z = transform.position.z; }
            transform.position = vector;
        }

        public static void DestroyChildren(this Transform t)
        {
            bool isPlaying = Application.isPlaying;

            while (t.childCount != 0)
            {
                Transform child = t.GetChild(0);

                if (isPlaying)
                {
                    child.parent = null;
                    UnityEngine.Object.Destroy(child.gameObject);
                }
                else UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }
    }

    [Serializable]
    public class DVector2
    {
        public static DVector2 Zero = new DVector2(0, 0);

        public double x;

        public double y;

        public double z;

        private static System.Random _random = new System.Random();

        public DVector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public DVector2(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public void Reset()
        {
            x = 0;
            y = 0;
        }

        public void Normalize()
        {
            double length = Length();

            x /= length;
            y /= length;
        }

        public DVector2 Normalized()
        {
            return Clone() / Length();
        }

        public void Negate()
        {
            x = -x;
            y = -y;
        }

        public DVector2 Clone()
        {
            return new DVector2(x, y);
        }

        public static DVector2 operator +(DVector2 a, DVector2 b)
        {
            return new DVector2(a.x + b.x, a.y + b.y);
        }

        public static DVector2 operator -(DVector2 a, DVector2 b)
        {
            return new DVector2(a.x - b.x, a.y - b.y);
        }

        public static DVector2 operator *(DVector2 a, double b)
        {
            return new DVector2(a.x * b, a.y * b);
        }

        public static DVector2 operator /(DVector2 a, DVector2 b)
        {
            return new DVector2(a.x / b.x, a.y / b.y);
        }

        public static DVector2 operator /(DVector2 a, double b)
        {
            return new DVector2(a.x / b, a.y / b);
        }

        public void Accumulate(DVector2 other)
        {
            x += other.x;
            y += other.y;
        }

        public DVector2 Divide(float scalar)
        {
            return new DVector2(x / scalar, y / scalar);
        }

        public DVector2 Divide(double scalar)
        {
            return new DVector2(x / scalar, y / scalar);
        }

        public double Dot(DVector2 v)
        {
            return x * v.x + y * v.y;
        }

        public double Cross(DVector2 v)
        {
            return x * v.y - y * v.x;
        }

        public double Length()
        {
            return Math.Sqrt(x * x + y * y);
        }

        public double LengthSquared()
        {
            return x * x + y * y;
        }

        public double Angle()
        {
            return Math.Atan2(y, x);
        }

        public static DVector2 Lerp(DVector2 from, DVector2 to, double t)
        {
            return new DVector2(from.x + t * (to.x - from.x),
                               from.y + t * (to.y - from.y));
        }

        public static DVector2 FromAngle(double angle)
        {
            return new DVector2(Math.Cos(angle), Math.Sin(angle));
        }

        public static double Distance(DVector2 v1, DVector2 v2)
        {
            return (v2 - v1).Length();
        }

        public static DVector2 RandomUnitVector()
        {
            double angle = _random.NextDouble() * Math.PI * 2;

            return new DVector2(Math.Cos(angle), Math.Sin(angle));
        }

        public override string ToString()
        {
            return "{" + Math.Round(x, 5) + "," + Math.Round(y, 5) + "}";
        }
        public Vector2 ToVector2()
        {
            return new Vector2((float)this.x, (float)this.y);
        }
    }
    [Serializable]
    public class DVector3
    {
        public double x;
        public double y;
        public double z;

        private const double radianTodegree = 180.0 / Math.PI;

        public DVector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public double? Measure
        {
            get { return m_Measure; }
            set { m_Measure = value; }
        }
        protected double? m_Measure = null;



        public void translate(double x, double y, double z)
        {

            this.x += x;
            this.y += y;
            this.z += z;
        }
        private void Scale(double scale)
        {
            this.x *= scale;
            this.y *= scale;
            this.z *= scale;
        }

        public void toDegree()
        {
            Scale(radianTodegree);
        }
        public string GetString()
        {
            return this.x + " " + this.y + " " + this.z;

        }
        public DVector2 ToDVector2()
        {
            return new DVector2(this.x, this.y);
        }

    }

    public static class GISTerrainLoaderEnumExtension
    {
        public static string GetDescription(this Enum e)
        {
            var attribute =
                e.GetType()
                    .GetTypeInfo()
                    .GetMember(e.ToString())
                    .FirstOrDefault(member => member.MemberType == MemberTypes.Field)
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .SingleOrDefault()
                    as DescriptionAttribute;

            return attribute?.Description ?? e.ToString();
        }
        public static IEnumerable<Enum> GetEnumValues(this Enum e)
        {
            // Can't use type constraints on value types, so have to do check like this
            if (typeof(Enum).BaseType != typeof(Enum))
            {
                throw new ArgumentException("T must be of type System.Enum");
            }

            return Enum.GetValues(typeof(Enum)).Cast<Enum>();
        }
        public static bool Contains(this Enum e, string enu)
        {
            bool contains = false;

            var values = GetEnumValues(e);

            foreach (var val in values)
            {
                if (val.ToString() == enu)
                    contains = true;
            }

            return contains;
        }
    }
}