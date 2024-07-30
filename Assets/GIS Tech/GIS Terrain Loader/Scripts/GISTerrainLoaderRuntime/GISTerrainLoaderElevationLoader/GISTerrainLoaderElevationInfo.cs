/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderElevationInfo
    {
 
        public GISTerrainLoaderFileData data;

        private TerrainData tdata;

        public float[,] tdataHeightmap;

        private int lastX;
 
        private float UnderWateroffest = 0;
        public async Task GenerateHeightMap(Prefs prefs, TerrainObject item)
        {

            float MaxElevation = data.MaxElevation;
            float MinElevation = data.MinElevation;
            float elevationRange =  data.MaxElevation - data.MinElevation;

            if (prefs.UnderWater == OptionEnabDisab.Enable)
            {
                UnderWateroffest = Math.Abs(MinElevation);
                MinElevation = data.MinElevation + UnderWateroffest;
                MaxElevation = data.MaxElevation + UnderWateroffest;
                elevationRange = MaxElevation - MinElevation;

            }
            else
            {
                if (data.MinElevation < 0)
                {
                    MinElevation = data.MinElevation;
                    elevationRange = data.MaxElevation - data.MinElevation;
                }


            }

            tdata = item.terrain.terrainData;

            if (tdataHeightmap == null)
                tdataHeightmap = new float[tdata.heightmapResolution, tdata.heightmapResolution];
            if (tdata == null)
            {
                tdata = item.terrain.terrainData;
                tdata.baseMapResolution = prefs.baseMapResolution;
                tdata.SetDetailResolution(prefs.detailResolution, prefs.resolutionPerPatch);
                tdata.size = item.size;

                if (tdataHeightmap == null)
                    tdataHeightmap = new float[tdata.heightmapResolution, tdata.heightmapResolution];
            }
 
            float thx = tdata.heightmapResolution - 1;
            float thy = tdata.heightmapResolution - 1;


            var y_Terrain_Col_num = (data.mapSize_row_y / prefs.m_terraincount.y);
            var x_Terrain_row_num = (data.mapSize_col_x / prefs.m_terraincount.x);

            int tw = tdata.heightmapResolution;
            int th = tdata.heightmapResolution;

            for (int x = lastX; x < tw; x++)
            {
                for (int y = 0; y < th; y++)
                {
                    var x_from = item.Number.x * x_Terrain_row_num;
                    var x_To = (item.Number.x * x_Terrain_row_num + x_Terrain_row_num);

                    var y_from = (item.Number.y * y_Terrain_Col_num);
                    var y_To = (item.Number.y * y_Terrain_Col_num + y_Terrain_Col_num);

                    float fpx = Mathf.Lerp(x_from, x_To, x / thx);
                    float fpy = Mathf.Lerp(y_from, y_To, y / thy);

                    int px = Mathf.FloorToInt(fpx);
                    int py = Mathf.FloorToInt(fpy);

                    if (y == tdata.heightmapResolution - 1)
                    {
                        if (y_To >= data.mapSize_row_y - 1)
                        {
                            y_To = data.mapSize_row_y - 1;
                        }

                        py = y_To;
                    }

                    if (x == tdata.heightmapResolution - 1)
                    {
                        if (px >= data.mapSize_col_x - 1)
                        {
                            px = data.mapSize_col_x - 1;
                        }
                        px = x_To;
                    }

                    if (px > data.floatheightData.GetLength(0) - 1)
                        px = data.floatheightData.GetLength(0) - 1;
                    if (py > data.floatheightData.GetLength(1) - 1)
                        py = data.floatheightData.GetLength(1) - 1;

                    var Rel = data.GetElevation(fpx, fpy);

                    if (prefs.UnderWater == OptionEnabDisab.Disable && Rel < 0)
                        Rel = 0;

                    if (prefs.UnderWater == OptionEnabDisab.Enable)
                        Rel =  Rel + UnderWateroffest;

                    var el = (((Rel - MinElevation)) / elevationRange);

                        tdataHeightmap[y, x] = el;
                }
                lastX = x;
            }

            lastX = 0;
            tdata.SetHeights(0, 0, tdataHeightmap);

            tdata = null;
 
            await Task.Delay(TimeSpan.FromSeconds(0.01));
 

        }
        public void RuntimeGenerateHeightMap(GISTerrainLoaderRuntimePrefs prefs, TerrainObject item)
        {

            float elevationRange = data.MaxElevation - data.MinElevation;
            float MaxElevation = data.MaxElevation;
            float MinElevation = data.MinElevation;

            if (prefs.UnderWater == OptionEnabDisab.Enable)
            {
                UnderWateroffest = Math.Abs(MinElevation);
                MinElevation = data.MinElevation + UnderWateroffest;
                MaxElevation = data.MaxElevation + UnderWateroffest;
                elevationRange = MaxElevation - MinElevation;

            }
            else
            {
                if (data.MinElevation < 0)
                {
                    MinElevation = data.MinElevation;
                    elevationRange = data.MaxElevation - data.MinElevation;
                }
            }

            tdata = item.terrain.terrainData;      

            if (tdataHeightmap == null)
                tdataHeightmap = new float[tdata.heightmapResolution, tdata.heightmapResolution];

            if (tdata == null)
            {
                tdata = item.terrain.terrainData;
                tdata.baseMapResolution = prefs.baseMapResolution;
                tdata.SetDetailResolution(prefs.detailResolution, prefs.resolutionPerPatch);
                tdata.size = item.size;

                if (tdataHeightmap == null)
                    tdataHeightmap = new float[tdata.heightmapResolution, tdata.heightmapResolution];
            }

           

            float thx = tdata.heightmapResolution - 1;
            float thy = tdata.heightmapResolution - 1;


            var y_Terrain_Col_num = (data.mapSize_row_y / prefs.terrainCount.y);
            var x_Terrain_row_num = (data.mapSize_col_x / prefs.terrainCount.x);

            int tw = tdata.heightmapResolution;
            int th = tdata.heightmapResolution;

            for (int x = lastX; x < tw; x++)
            {
                for (int y = 0; y < th; y++)
                {

                    var x_from = item.Number.x * x_Terrain_row_num;
                    var x_To = (item.Number.x * x_Terrain_row_num + x_Terrain_row_num);

                    var y_from = (item.Number.y * y_Terrain_Col_num);
                    var y_To = (item.Number.y * y_Terrain_Col_num + y_Terrain_Col_num);

                    float fpx = Mathf.Lerp(x_from, x_To, x / thx);
                    float fpy = Mathf.Lerp(y_from, y_To, y / thy);

                    int px = Mathf.FloorToInt(fpx);
                    int py = Mathf.FloorToInt(fpy);

                    if (y == tdata.heightmapResolution - 1)
                    {
                        if (y_To >= data.mapSize_row_y - 1)
                        {
                            y_To = data.mapSize_row_y - 1;
                        }

                        py = y_To;
                    }

                    if (x == tdata.heightmapResolution - 1)
                    {
                        if (px >= data.mapSize_col_x - 1)
                        {
                            px = data.mapSize_col_x - 1;
                        }
                        px = x_To;
                    }

                    if (px > data.floatheightData.GetLength(0) - 1)
                        px = data.floatheightData.GetLength(0) - 1;
                    if (py > data.floatheightData.GetLength(1) - 1)
                        py = data.floatheightData.GetLength(1) - 1;

                    var Rel = data.GetElevation(fpx, fpy);

                    if (prefs.UnderWater == OptionEnabDisab.Disable && Rel < 0)
                        Rel = 0;

                    if (prefs.UnderWater == OptionEnabDisab.Enable)
                        Rel = Rel + UnderWateroffest;

                    var el = (((Rel - MinElevation)) / elevationRange);

                    tdataHeightmap[y, x] = el;
                }
                lastX = x;
            }

            lastX = 0;
            tdata.SetHeights(0, 0, tdataHeightmap);

            tdata = null;

            item.ElevationState = ElevationState.Loaded;


        }
        public void GetData(GISTerrainLoaderFileData m_data)
        {
            data = new GISTerrainLoaderFileData();

            data.AlreadyLoaded = m_data.AlreadyLoaded;

            data.MaxElevation = m_data.MaxElevation;
            data.MinElevation = m_data.MinElevation;

            data.mapSize_row_y = m_data.mapSize_row_y;
            data.mapSize_col_x = m_data.mapSize_col_x;

            data.Tiles = m_data.Tiles;


            data.DownLeftPoint = m_data.DownLeftPoint;
            data.TopRightPoint = m_data.TopRightPoint;

            data.TopLeftPoint = m_data.TopLeftPoint;
            data.DownRightPoint = m_data.DownRightPoint;

            data.Terrain_Dimension = m_data.Terrain_Dimension;

            data.dim = m_data.dim;
            data.cellsize = m_data.cellsize;

            data.floatheightData = m_data.floatheightData;

        }


    }
}