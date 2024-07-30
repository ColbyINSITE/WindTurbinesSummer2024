/*     Unity GIS Tech 202-2021      */

using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class Prefs
    {
        public int detailResolution = 2048;
        public int resolutionPerPatch = 16;
        public int baseMapResolution = 1024;
        public int heightmapResolution = 128;
        public Vector2Int m_terraincount;
        public Vector3 m_Size;
        public OptionEnabDisab UnderWater;
        public Vector3 Scale;
        public TerrainElevation terrainElevation = TerrainElevation.RealWorldElevation;
        public Vector2 TerrainMaxMinElevation = new Vector2(0, 0);

        public Prefs(int detailresolution, int resolutionperPatch, int basemapresolution, int heightmapresolution, Vector2Int Terraincount, Vector3 size, OptionEnabDisab m_UnderWater,Vector3 m_Scale)
        {
            detailResolution = detailresolution;
            resolutionPerPatch = resolutionperPatch;
            baseMapResolution = basemapresolution;
            heightmapResolution = heightmapresolution;
            m_terraincount = Terraincount;
            m_Size = size;
            UnderWater = m_UnderWater;
            Scale = m_Scale;
        }
    }
}