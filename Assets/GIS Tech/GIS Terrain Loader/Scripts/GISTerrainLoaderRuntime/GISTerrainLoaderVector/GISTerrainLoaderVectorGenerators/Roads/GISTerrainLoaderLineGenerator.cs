/*     Unity GIS Tech 2020-2021      */

using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderLineGenerator
    {
        public static LineRenderer RLine(Vector3[] linePoints)
        {
            GameObject result = new GameObject();

            result.transform.Rotate(new Vector3(90, 0, 0));

            LineRenderer lineRender = result.AddComponent<LineRenderer>();
            lineRender.positionCount = linePoints.Length;
            lineRender.SetPositions(linePoints);

            return lineRender;

        }





        public static GameObject CreateLine(GISTerrainLoaderRoad m_road )
        {
            LineRenderer lineRender = RLine(m_road.Points);

            lineRender.alignment = LineAlignment.TransformZ;

            lineRender.material = m_road.material;


            lineRender.startWidth = m_road.width;
            lineRender.endWidth = m_road.width;

            lineRender.startColor = m_road.color;
            lineRender.endColor = m_road.color;

            return lineRender.gameObject;

        }
    }
}
