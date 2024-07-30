/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderBuilding : MonoBehaviour
    {
        private static GameObject BuildingParent;
        private static TerrainContainerObject container;
        public static List<int> roofIndices;
        protected static Shader defaultShader;
        protected static List<Vector3> vertices;
        protected static List<Vector2> uvs;
        protected static List<int> wallTriangles;
        protected static List<int> roofTriangles;
        protected static List<Vector3> roofVertices;
        public static float perimeter;
        private static class GISTerrainLoaderBuildingsConstants
        {
            public static float wallHeight = 10f;
            public const float wallWidth = 0.0001f;
            public const int firstIndexRandomRoof = 0;
            public const int endIndexRandomRoof = 4;

            public const int firstIndexRandomShop = 0;
            public const int endIndexRandomShop = 0;

            public const int firstIndexRandomBuilding = 0;
            public const int endIndexRandomBuilding = 3;

        }
        public static void CreateBuilding(TerrainContainerObject m_container, GISTerrainLoaderPolygonGeoData buildingData, Transform BuildingParent, GISTerrainLoaderSO_Building m_buildingsPrefab,GISTerrainLoaderSO_Building Default_buildingsPrefab)
        {
            GISTerrainLoaderSO_Building DefaultPrefab;

            container = m_container;

            var points = GetPolyPoints (buildingData.GeoPoints) ;

            if (points.Count < 3) return;
            if (points[0] == points[points.Count - 1]) points.RemoveAt(points.Count - 1);
            if (points.Count < 3) return;

            var buildingtype = buildingData.Tag;

            DefaultPrefab = m_buildingsPrefab;

            for (int i = 0; i < points.Count; i++)
            {

                int prev = i - 1;
                if (prev < 0) prev = points.Count - 1;

                int next = i + 1;
                if (next >= points.Count) next = 0;

                float a1 = Angle2D(points[prev], points[i]);
                float a2 = Angle2D(points[i], points[next]);

                if (Mathf.Abs(a1 - a2) < 5)
                {
                    points.RemoveAt(i);
                    i--;
                }
            }
            if (points.Count < 3) return;

            Vector3 centerPoint;

            Vector4 cp = new Vector4(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 point = points[i];
                if (point.x < cp.x) cp.x = point.x;
                if (point.z < cp.y) cp.y = point.z;
                if (point.x > cp.z) cp.z = point.x;
                if (point.z > cp.w) cp.w = point.z;
            }

            centerPoint = new Vector3((cp.z + cp.x) / 2, 0, (cp.y + cp.w) / 2);

            for (int i = 0; i < points.Count; i++) points[i] -= centerPoint;

            float height = 0;
            float roofHeight = 0;

            if (defaultShader == null) defaultShader = Shader.Find("Unlit/Transparent");

            GameObject houseGO = CreateGameObject(container, buildingData.ID.ToString());
            MeshRenderer renderer = houseGO.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = houseGO.AddComponent<MeshFilter>();

            houseGO.transform.localPosition = centerPoint;
            houseGO.transform.localRotation = Quaternion.Euler(Vector3.zero);
            houseGO.transform.localScale = Vector3.one;

            Vector2 scale = Vector2.one;

            if (DefaultPrefab.wall == null) DefaultPrefab.wall = new Material(defaultShader);
            if (DefaultPrefab.roof == null) DefaultPrefab.roof = new Material(defaultShader);

            scale = DefaultPrefab.WallTextureTiling;

            BuildingRoofType roofType = DefaultPrefab.roofType;

            DefaultPrefab.mesh = new Mesh { name = buildingData.ID.ToString() };

            meshFilter.sharedMesh = DefaultPrefab.mesh;

            renderer.sharedMaterials = new[]
            {
            DefaultPrefab.wall,
            DefaultPrefab.roof
        };
            renderer.sharedMaterials[0].mainTextureScale = DefaultPrefab.WallTextureTiling;
            renderer.sharedMaterials[1].mainTextureScale = DefaultPrefab.RoofTextureTiling;

            Vector2 centerCoords = Vector2.zero;
            float minCX = float.MaxValue, minCY = float.MaxValue, maxCX = float.MinValue, maxCY = float.MinValue;

            foreach (var nodez in buildingData.GeoPoints)
            {
                var nodeCoords = new Vector2((float)nodez.y, (float)nodez.x);
                centerCoords += nodeCoords;
                if (nodeCoords.x < minCX) minCX = nodeCoords.x;
                if (nodeCoords.y < minCY) minCY = nodeCoords.y;
                if (nodeCoords.x > maxCX) maxCX = nodeCoords.x;
                if (nodeCoords.y > maxCY) maxCY = nodeCoords.y;
            }

            int wallVerticesCount = (points.Count + 1) * 2;
            int roofVerticesCount = points.Count;
            int verticesCount = wallVerticesCount + roofVerticesCount;
            int countTriangles = wallVerticesCount * 3;

            if (vertices == null) vertices = new List<Vector3>(verticesCount);
            else vertices.Clear();

            if (uvs == null) uvs = new List<Vector2>(verticesCount);
            else uvs.Clear();

            if (wallTriangles == null) wallTriangles = new List<int>(countTriangles);
            else wallTriangles.Clear();

            if (roofTriangles == null) roofTriangles = new List<int>();
            else roofTriangles.Clear();


            float levels = 0;

            float minHeight = 0;

            houseGO.name = "<building_" + buildingtype + ">";

            houseGO.transform.parent = BuildingParent;


            if (buildingData.Levels != 0)
            {
                var h = GISTerrainLoaderBuildingsConstants.wallHeight * container.scale.y;
                if (GISTerrainLoaderBuildingsConstants.wallHeight > 5) h = GISTerrainLoaderBuildingsConstants.wallHeight * container.scale.y / 2;

                levels += buildingData.Levels;
                height = levels * GISTerrainLoaderBuildingsConstants.wallHeight * container.scale.y;
            }

            if (buildingData.MinLevel != 0)
            {
                float minLevel = buildingData.MinLevel;
                levels -= minLevel;
                minHeight = GISTerrainLoaderBuildingsConstants.wallHeight * minLevel;
            }

            if (buildingData.Height != 0)
                height = buildingData.Height;


            if (buildingData.MinHeight != 0)
                minHeight = buildingData.MinHeight;

            if (levels == 0)
                levels = 1;
            if (height == 0)
                height = DefaultPrefab.height;

            CreateHouseWall(points, height, DefaultPrefab.wall, scale);
            CreateHouseRoof(points, height, roofHeight, roofType);

            DefaultPrefab.mesh.vertices = vertices.ToArray();
            DefaultPrefab.mesh.uv = uvs.ToArray();
            DefaultPrefab.mesh.subMeshCount = 2;
            DefaultPrefab.mesh.SetTriangles(wallTriangles.ToArray(), 0);
            DefaultPrefab.mesh.SetTriangles(roofTriangles.ToArray(), 1);

            DefaultPrefab.mesh.RecalculateBounds();
            DefaultPrefab.mesh.RecalculateNormals();
            houseGO.gameObject.SetActive(true);

        }
        public static List<Vector3> GetPolyPoints(List<DVector2> buildingpoints)
        {
            List<Vector3> points = new List<Vector3>(0);

            for (int i = 1; i < buildingpoints.Count; i++)
            {
                 var PointA_LatLon = new DVector2(buildingpoints[i].x, buildingpoints[i].y);
                var pointA = GeoRefConversion.LatLonToUnityWorldSpace(PointA_LatLon, container);

                points.Add(pointA);

            }
            return points;

        }
        private static void CreateHouseRoofDome(float height, List<Vector3> vertices, List<int> triangles)
        {
            Vector3 roofTopPoint = Vector3.zero;
            roofTopPoint = vertices.Aggregate(roofTopPoint, (current, point) => current + point) / vertices.Count;
            roofTopPoint.y += height;
            int vIndex = vertices.Count;

            for (int i = 0; i < vertices.Count; i++)
            {
                int p1 = i;
                int p2 = i + 1;
                if (p2 >= vertices.Count) p2 -= vertices.Count;

                triangles.AddRange(new[] { p1, p2, vIndex });
            }

            vertices.Add(roofTopPoint);
        }
        private static void CreateHouseRoof(List<Vector3> points, float baseHeight, float roofHeight, BuildingRoofType roofType)
        {
            float[] roofPoints = new float[points.Count * 2];

            if (roofVertices == null) roofVertices = new List<Vector3>(points.Count);
            else roofVertices.Clear();

            try
            {
                int countVertices = CreateHouseRoofVerticles(points, roofVertices, roofPoints, baseHeight);
                CreateHouseRoofTriangles(countVertices, roofVertices, roofType, roofPoints, baseHeight, roofHeight, ref roofTriangles);

                if (roofTriangles.Count == 0)
                {
                    return;
                }

                Vector3 side1 = roofVertices[roofTriangles[1]] - roofVertices[roofTriangles[0]];
                Vector3 side2 = roofVertices[roofTriangles[2]] - roofVertices[roofTriangles[0]];
                Vector3 perp = Vector3.Cross(side1, side2);

                bool reversed = perp.y < 0;
                if (reversed) roofTriangles.Reverse();

                float minX = float.MaxValue;
                float minZ = float.MaxValue;
                float maxX = float.MinValue;
                float maxZ = float.MinValue;

                for (int i = 0; i < roofVertices.Count; i++)
                {
                    Vector3 v = roofVertices[i];
                    if (v.x < minX) minX = v.x;
                    if (v.z < minZ) minZ = v.z;
                    if (v.x > maxX) maxX = v.x;
                    if (v.z > maxZ) maxZ = v.z;
                }

                float offX = maxX - minX;
                float offZ = maxZ - minZ;

                for (int i = 0; i < roofVertices.Count; i++)
                {
                    Vector3 v = roofVertices[i];
                    uvs.Add(new Vector2((v.x - minX) / offX, (v.z - minZ) / offZ));
                }

                int triangleOffset = vertices.Count;
                for (int i = 0; i < roofTriangles.Count; i++)
                    roofTriangles[i] += triangleOffset;

                vertices.AddRange(roofVertices);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private static void CreateHouseRoofTriangles(int countVertices, List<Vector3> vertices, BuildingRoofType roofType, float[] roofPoints, float baseHeight, float roofHeight, ref List<int> triangles)
        {
            if (roofType == BuildingRoofType.flat)
            {
                if (roofIndices == null) roofIndices = new List<int>(60);
                triangles.AddRange(GISTerrainLoaderTriangulator.Triangulate(roofPoints, countVertices, roofIndices));
            }
            else if (roofType == BuildingRoofType.dome) CreateHouseRoofDome(baseHeight + roofHeight, vertices, triangles);
        }
        private static int CreateHouseRoofVerticles(List<Vector3> baseVertices, List<Vector3> verticles, float[] roofPoints, float baseHeight)
        {
            float topPoint = baseHeight;
            int countVertices = 0;

            for (int i = 0; i < baseVertices.Count; i++)
            {
                Vector3 p = baseVertices[i];
                float px = p.x;
                float py = p.y + topPoint;
                float pz = p.z;

                bool hasVerticle = false;

                for (int j = 0; j < countVertices * 2; j += 2)
                {
                    if (Math.Abs(roofPoints[j] - px) < float.Epsilon && Math.Abs(roofPoints[j + 1] - pz) < float.Epsilon)
                    {
                        hasVerticle = true;
                        break;
                    }
                }

                if (!hasVerticle)
                {
                    int cv2 = countVertices * 2;

                    roofPoints[cv2] = px;
                    roofPoints[cv2 + 1] = pz;
                    verticles.Add(new Vector3(px, py, pz));

                    countVertices++;
                }
            }

            return countVertices;
        }
        private static void CreateHouseWall(List<Vector3> baseVertices, float baseHeight, Material material, Vector2 materialScale)
        {
            CreateHouseWallMesh(baseVertices, baseHeight, false);

            Vector2 scale = material.mainTextureScale;
            scale.x *= perimeter / 100 * materialScale.x;
            scale.y *= baseHeight / 30 * materialScale.y;
            material.mainTextureScale = scale;
        }
        private static void CreateHouseWallMesh(List<Vector3> baseVertices, float baseHeight, bool inverted)
        {
            bool reversed = CreateHouseWallVerticles(baseHeight, baseVertices, vertices, uvs);
            if (inverted) reversed = !reversed;
            CreateHouseWallTriangles(vertices, reversed);
        }
        private static void CreateHouseWallTriangles(List<Vector3> vertices, bool reversed)
        {
            int countVertices = vertices.Count;
            for (int i = 0; i < countVertices / 4; i++)
            {
                int p1 = i * 4;
                int p2 = p1 + 2;
                int p3 = p2 + 1;
                int p4 = p1 + 1;

                if (p2 >= countVertices) p2 -= countVertices;
                if (p3 >= countVertices) p3 -= countVertices;

                if (reversed)
                {
                    wallTriangles.Add(p1);
                    wallTriangles.Add(p4);
                    wallTriangles.Add(p3);
                    wallTriangles.Add(p1);
                    wallTriangles.Add(p3);
                    wallTriangles.Add(p2);
                }
                else
                {
                    wallTriangles.Add(p2);
                    wallTriangles.Add(p3);
                    wallTriangles.Add(p1);
                    wallTriangles.Add(p3);
                    wallTriangles.Add(p4);
                    wallTriangles.Add(p1);
                }
            }
        }
        private static bool CreateHouseWallVerticles(float baseHeight, List<Vector3> baseVertices, List<Vector3> vertices, List<Vector2> uvs)
        {
            float topPoint = baseHeight;

            int baseVerticesCount = baseVertices.Count;
            Vector3 pp = Vector3.zero;
            Vector3 ptv = Vector3.zero;

            for (int i = 0; i <= baseVerticesCount; i++)
            {
                int j = i;
                if (j >= baseVerticesCount) j -= baseVerticesCount;

                Vector3 p = baseVertices[j];
                Vector3 tv = new Vector3(p.x, p.y+topPoint, p.z);

                if (i > 0)
                {
                    vertices.Add(pp);
                    vertices.Add(ptv);

                    vertices.Add(p);
                    vertices.Add(tv);
                }

                pp = p;
                ptv = tv;
            }

            float currentDistance = 0;
            int countVertices = vertices.Count;
            int fourthVerticesCount = countVertices / 4;
            perimeter = 0;

            for (int i = 0; i < fourthVerticesCount; i++)
            {
                int i1 = i * 4;
                int i2 = i * 4 + 2;

                float magnitude = (vertices[i1] - vertices[i2]).magnitude;
                perimeter += magnitude;
            }

            float prevDistance = 0;

            for (int i = 0; i < fourthVerticesCount; i++)
            {
                int i1 = i * 4;
                int i2 = i * 4 + 2;

                float magnitude = (vertices[i1] - vertices[i2]).magnitude;

                float prevU = prevDistance / perimeter;

                currentDistance += magnitude;
                prevDistance = currentDistance;

                float curU = currentDistance / perimeter;
                uvs.Add(new Vector2(prevU, 0));
                uvs.Add(new Vector2(prevU, 1));
                uvs.Add(new Vector2(curU, 0));
                uvs.Add(new Vector2(curU, 1));
            }

            int southIndex = -1;
            float southZ = float.MaxValue;

            for (int i = 0; i < baseVerticesCount; i++)
            {
                if (baseVertices[i].z < southZ)
                {
                    southZ = baseVertices[i].z;
                    southIndex = i;
                }
            }

            int prevIndex = southIndex - 1;
            if (prevIndex < 0) prevIndex = baseVerticesCount - 1;

            int nextIndex = southIndex + 1;
            if (nextIndex >= baseVerticesCount) nextIndex = 0;

            float angle1 = Angle2D(baseVertices[southIndex], baseVertices[nextIndex]);
            float angle2 = Angle2D(baseVertices[southIndex], baseVertices[prevIndex]);

            return angle1 < angle2;
        }
        public static float Angle2D(Vector3 point1, Vector3 point2)
        {
            return Mathf.Atan2(point2.z - point1.z, point2.x - point1.x) * Mathf.Rad2Deg;
        }
        public static GameObject CreateGameObject(TerrainContainerObject container, string id)
        {
            GameObject buildingGameObject = new GameObject(id);
            buildingGameObject.SetActive(false);

            buildingGameObject.transform.parent = container.transform;
            buildingGameObject.layer = container.gameObject.layer;
            return buildingGameObject;
        }
    }
  
}
