/*     Unity GIS Tech 2020-2021      */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderTriangulator
    {
        private List<Vector2> m_points = new List<Vector2>();

        public GISTerrainLoaderTriangulator(Vector2[] points)
        {
            m_points = new List<Vector2>(points);
        }

        public int[] Triangulate()
        {
            List<int> indices = new List<int>();

            int n = m_points.Count;
            if (n < 3)
                return indices.ToArray();

            int[] V = new int[n];
            if (Area() > 0)
            {
                for (int v = 0; v < n; v++)
                    V[v] = v;
            }
            else
            {
                for (int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }

            int nv = n;
            int count = 2 * nv;
            for (int m = 0, v = nv - 1; nv > 2;)
            {
                if ((count--) <= 0)
                    return indices.ToArray();

                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;

                if (Snip(u, v, w, nv, V))
                {
                    int a, b, c, s, t;
                    a = V[u];
                    b = V[v];
                    c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    m++;
                    for (s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            indices.Reverse();
            return indices.ToArray();
        }

        private float Area()
        {
            int n = m_points.Count;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = m_points[p];
                Vector2 qval = m_points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return (A * 0.5f);
        }

        private bool Snip(int u, int v, int w, int n, int[] V)
        {
            int p;
            Vector2 A = m_points[V[u]];
            Vector2 B = m_points[V[v]];
            Vector2 C = m_points[V[w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;
            for (p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                Vector2 P = m_points[V[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }

        private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = C.x - B.x; ay = C.y - B.y;
            bx = A.x - C.x; by = A.y - C.y;
            cx = B.x - A.x; cy = B.y - A.y;
            apx = P.x - A.x; apy = P.y - A.y;
            bpx = P.x - B.x; bpy = P.y - B.y;
            cpx = P.x - C.x; cpy = P.y - C.y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }

        public static IEnumerable<int> Triangulate(float[] points, int countVertices, List<int> indices)
        {
            indices.Clear();

            int n = countVertices;
            if (n < 3) return indices;

            int[] V = new int[n];
            if (TriangulateArea(points, countVertices) > 0)
            {
                for (int v = 0; v < n; v++) V[v] = v;
            }
            else
            {
                for (int v = 0; v < n; v++) V[v] = n - 1 - v;
            }

            int nv = n;
            int count = 2 * nv;

            for (int v = nv - 1; nv > 2;)
            {
                if (count-- <= 0) return indices;

                int u = v;
                if (nv <= u) u = 0;
                v = u + 1;
                if (nv <= v) v = 0;
                int w = v + 1;
                if (nv <= w) w = 0;

                if (TriangulateSnip(points, u, v, w, nv, V))
                {
                    int s, t;
                    indices.Add(V[u]);
                    indices.Add(V[v]);
                    indices.Add(V[w]);
                    for (s = v, t = v + 1; t < nv; s++, t++) V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            indices.Reverse();
            return indices;
        }
        public static float TriangulateArea(List<Vector2> points)
        {
            int n = points.Count;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = points[p];
                Vector2 qval = points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return A * 0.5f;
        }

        public static float TriangulateArea(float[] points, int countVertices)
        {
            int n = countVertices;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                float pvx = points[p * 2];
                float pvy = points[p * 2 + 1];
                float qvx = points[q * 2];
                float qvy = points[q * 2 + 1];

                A += pvx * qvy - qvx * pvy;
            }
            return A * 0.5f;
        }

        public static bool TriangulateInsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float bp = (C.x - B.x) * (P.y - B.y) - (C.y - B.y) * (P.x - B.x);
            float ap = (B.x - A.x) * (P.y - A.y) - (B.y - A.y) * (P.x - A.x);
            float cp = (A.x - C.x) * (P.y - C.y) - (A.y - C.y) * (P.x - C.x);
            return bp > 0.0f && cp > 0.0f && ap > 0.0f;
        }

        public static bool TriangulateInsideTriangle(float ax, float ay, float bx, float by, float cx, float cy, float px, float py)
        {
            float bp = (cx - bx) * (py - by) - (cy - by) * (px - bx);
            float ap = (bx - ax) * (py - ay) - (by - ay) * (px - ax);
            float cp = (ax - cx) * (py - cy) - (ay - cy) * (px - cx);
            return (bp >= 0.0f) && (cp >= 0.0f) && (ap >= 0.0f);
        }

        public static bool TriangulateSnip(List<Vector2> points, int u, int v, int w, int n, int[] V)
        {
            Vector2 A = points[V[u]];
            Vector2 B = points[V[v]];
            Vector2 C = points[V[w]];
            if (Mathf.Epsilon > (B.x - A.x) * (C.y - A.y) - (B.y - A.y) * (C.x - A.x)) return false;
            for (int p = 0; p < n; p++)
            {
                if (p == u || p == v || p == w) continue;
                if (TriangulateInsideTriangle(A, B, C, points[V[p]])) return false;
            }
            return true;
        }

        public static bool TriangulateSnip(float[] points, int u, int v, int w, int n, int[] V)
        {
            float ax = points[V[u] * 2];
            float ay = points[V[u] * 2 + 1];
            float bx = points[V[v] * 2];
            float by = points[V[v] * 2 + 1];
            float cx = points[V[w] * 2];
            float cy = points[V[w] * 2 + 1];

            if (Mathf.Epsilon > (bx - ax) * (cy - ay) - (by - ay) * (cx - ax)) return false;
            for (int p = 0; p < n; p++)
            {
                if (p == u || p == v || p == w) continue;
                if (TriangulateInsideTriangle(ax, ay, bx, by, cx, cy, points[V[p] * 2], points[V[p] * 2 + 1])) return false;
            }
            return true;
        }
    }
}