/*     Unity GIS Tech 2020-2021      */

using System;
using System.IO;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    [Serializable]
    public class GISTerrainLoaderFileData
    {
        public bool AlreadyLoaded =false;

        [SerializeField]
        public float[,] floatheightData;

        public float MaxElevation = -999999;
        public float MinElevation = 900000;

        public int mapSize_row_y;
        public int mapSize_col_x;

        public Vector2 Tiles = new Vector2(0, 0);

        public DVector2 DownLeftPoint = new DVector2(0, 0);
        public DVector2 TopRightPoint = new DVector2(0, 0);

        public DVector2 TopLeftPoint = new DVector2(0, 0);
        public DVector2 DownRightPoint = new DVector2(0, 0);

        public DVector2 dim = new DVector2(0, 0);

        public double cellsize = 0;

        public DVector2 Terrain_Dimension = new DVector2(0, 0);

        public Vector2 TerrainMaxMinElevation = new Vector2(0, 0);

        public GISTerrainLoaderFileData()
        {

            MaxElevation = -5000;
            MinElevation = 5000;

            mapSize_row_y = 0;
            mapSize_col_x = 0;

            Vector2 Tiles = new Vector2(0, 0);

            DVector2 Origin = new DVector2(0, 0);
            DVector2 TopRightPoint = new DVector2(0, 0);

            DVector2 TopLeftPoint = new DVector2(0, 0);
            DVector2 DownRightPoint = new DVector2(0, 0);

            DVector2 dim = new DVector2(0, 0);

            DVector2 Terrain_Dimension = new DVector2(0, 0);

            floatheightData = new float[mapSize_col_x, mapSize_row_y];
        }
        public float GetElevation(float fpx, float fpy)
        {
            int px = (int)fpx;
            int py = (int)fpy;

            float Rx = fpx - px;
            float Ry = fpy - py;

            return GetAverageElevation(Rx, Ry, px, py);
        }
        public float GetAverageElevation(float Rx, float Ry, int px, int py)
        {

            float C_25 = 0.25f;
            float C_12 = 12.0f;
            float C_36 = 36.0f;

            var Rsx_1 = Rx - 1;
            var Rsx_2 = Rx - 2;
            var RsxP_1 = Rx + 1;

            var Rsy_1 = Ry - 1;
            var Rsy_2 = Ry - 2;
            var RsyP_1 = Ry + 1;

            var PsxP_1 = px + 1;
            var PsyP_1 = py + 1;

            var PxyM = Rx * Ry;

            var Psx_1 = px - 1;
            var Psy_1 = py - 1;

            var PsxP_2 = px + 2;
            var PsyP_2 = py + 2;

            float el = Rsx_1 * Rsx_2 * RsxP_1 * Rsy_1 * Rsy_2 * RsyP_1 * C_25 * ReadValue(px, py);

            el -= Rx * RsxP_1 * Rsx_2 * Rsy_1 * Rsy_2 * RsyP_1 * C_25 * ReadValue(PsxP_1, py);
            el -= Ry * Rsx_1 * Rsx_2 * RsxP_1 * RsyP_1 * Rsy_2 * C_25 * ReadValue(px, PsyP_1);
            el += PxyM * RsxP_1 * Rsx_2 * RsyP_1 * Rsy_2 * C_25 * ReadValue(PsxP_1, PsyP_1);
            el -= Rx * Rsx_1 * Rsx_2 * Rsy_1 * Rsy_2 * RsyP_1 / C_12 * ReadValue(Psx_1, py);
            el -= Ry * Rsx_1 * Rsx_2 * RsxP_1 * Rsy_1 * Rsy_2 / C_12 * ReadValue(px, Psy_1);
            el += PxyM * Rsx_1 * Rsx_2 * RsyP_1 * Rsy_2 / C_12 * ReadValue(Psx_1, PsyP_1);
            el += PxyM * RsxP_1 * Rsx_2 * Rsy_1 * Rsy_2 / C_12 * ReadValue(PsxP_1, Psy_1);
            el += Rx * Rsx_1 * RsxP_1 * Rsy_1 * Rsy_2 * RsyP_1 / C_12 * ReadValue(PsxP_2, py);
            el += Ry * Rsx_1 * Rsx_2 * RsxP_1 * Rsy_1 * RsyP_1 / C_12 * ReadValue(px, PsyP_2);
            el += PxyM * Rsx_1 * Rsx_2 * Rsy_1 * Rsy_2 / C_36 * ReadValue(Psx_1, Psy_1);
            el -= PxyM * Rsx_1 * RsxP_1 * RsyP_1 * Rsy_2 / C_12 * ReadValue(PsxP_2, PsyP_1);
            el -= PxyM * RsxP_1 * Rsx_2 * Rsy_1 * RsyP_1 / C_12 * ReadValue(PsxP_1, PsyP_2);
            el -= PxyM * Rsx_1 * RsxP_1 * Rsy_1 * Rsy_2 / C_36 * ReadValue(PsxP_2, Psy_1);
            el -= PxyM * Rsx_1 * Rsx_2 * Rsy_1 * RsyP_1 / C_36 * ReadValue(Psx_1, PsyP_2);
            el += PxyM * Rsx_1 * RsxP_1 * Rsy_1 * RsyP_1 / C_36 * ReadValue(PsxP_2, PsyP_2);

            return el;
        }
        public float ReadValue(int PX, int PY)
        {
            try
            {
                PX = Mathf.Clamp(PX, 0, mapSize_col_x - 1);
                PY = Mathf.Clamp(PY, 0, mapSize_row_y - 1);
                var el = floatheightData[PX, PY];
                return el;
            }
            catch (Exception e)
            {
                var es = e;
                return 0;
            }
        }

        public void Store()
        {
            GISTerrainLoaderHeightmapSerializer.Serialize(this);
        }

        public float GetElevation(DVector2 LatLon)
        {
            float value = 0;

            var rang_x = Math.Abs(Math.Abs(DownRightPoint.x) - Math.Abs(TopLeftPoint.x));
            var rang_y = Math.Abs(Math.Abs(TopLeftPoint.y) - Math.Abs(DownRightPoint.y));

            var rang_px = Math.Abs(Math.Abs(LatLon.x) - Math.Abs(TopLeftPoint.x));
            var rang_py = Math.Abs(Math.Abs(TopLeftPoint.y) - Math.Abs(LatLon.y));

            int localLat = (int)(rang_px * mapSize_col_x / rang_x);
            int localLon = (int)(rang_py * mapSize_row_y / rang_y);

            value = floatheightData[localLat, mapSize_row_y- localLon-1];

            return value;
        }
    }
}