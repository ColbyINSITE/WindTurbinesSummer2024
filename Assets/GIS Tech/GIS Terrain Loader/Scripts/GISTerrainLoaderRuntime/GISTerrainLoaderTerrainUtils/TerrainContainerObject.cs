/*     Unity GIS Tech 2020-2021      */
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace GISTech.GISTerrainLoader
{
    public class TerrainContainerObject : MonoBehaviour
    {

#if UNITY_EDITOR
        public int lastTab = 0;
#endif
        public GISTerrainLoaderFileData data ;
        public float[] datas;

        public DVector2 TopLeftLatLong;
        public DVector2 DownRightLatLong;
        public Vector3 scale;
        public Vector3 ContainerSize;
        public Vector3 SubTerrainSize;
        public Vector2Int terrainCount;
        public Vector2 Dimensions;
        public Vector2 MinMaxElevation;
 

        [HideInInspector]
        public DVector2 TLPointMercator;
        [HideInInspector]
        public DVector2 DRPointMercator;
        [HideInInspector]
        public Bounds GlobalTerrainBounds;

        [HideInInspector]
        public string GeneratedTerrainfolder;


        private TerrainObject[,] _terrains;


        public TerrainObject[,] terrains
        {
            get
            {
                if (_terrains == null)
                {
                    _terrains = new TerrainObject[terrainCount.x, terrainCount.y];
                    TerrainObject[] items = GetComponentsInChildren<TerrainObject>();
                    foreach (TerrainObject item in items) _terrains[item.Number.x, item.Number.y] = item;
                }
                return _terrains;
            }
            set
            {
                _terrains = value;
            }
        }
        public bool IncludePoint(double Lon,double Lat)
        {
            bool Include = false;

            var MinLat = DownRightLatLong.y;
            var MinLon = TopLeftLatLong.x;
            var MaxLat = TopLeftLatLong.y;
            var MaxLon = DownRightLatLong.x;

            if (Lon > MinLon && Lon < MaxLon && Lat > MinLat && Lat < MaxLat)
                Include = true;

            return Include;
        }
        public bool IncludePoint(DVector2 LatLon)
        {
            bool Include = false;

            var MinLat = DownRightLatLong.y;
            var MinLon = TopLeftLatLong.x;
            var MaxLat = TopLeftLatLong.y;
            var MaxLon = DownRightLatLong.x;

            if (LatLon.x > MinLon && LatLon.x < MaxLon && LatLon.y > MinLat && LatLon.y < MaxLat)
                Include = true;

            return Include;
        }
        public float RouteScaleOverage()
        {

            float value = 1;

#if UNITY_EDITOR
            if (Application.isPlaying || EditorApplication.isPlaying)
                value = ((scale.x + scale.y + scale.z) / 3) / 6;

            if (Application.isEditor && !EditorApplication.isPlaying)
                value = ((scale.x + scale.y + scale.z) / 3) * 1.3f;

#else           
            value = ((scale.x + scale.y + scale.z) / 3) / 6;
#endif

            return value;
        }

        public float LableScaleOverage()
        {
            float value = 1;

#if UNITY_EDITOR
            if (Application.isPlaying || EditorApplication.isPlaying)
                value = ((scale.x + scale.y + scale.z) / 3) * 2.5f;

            if (Application.isEditor && !EditorApplication.isPlaying)
                value = ((scale.x + scale.y + scale.z) / 3)*10;
#else           
            value = ((scale.x + scale.y + scale.z) / 3)  * 2.5f;
#endif

            return value;
        }

        #region Export


        #endregion

        #region TotatlTerrains Prefs
        private int[] availableHeights = { 32, 64, 129, 256, 512, 1024, 2048, 4096 };
        private int[] availableHeightsResolutionPrePec = { 4, 8, 16, 32 };

        public float m_PixelErro;
        public float PixelErro
        {
            get { return m_PixelErro; }
            set
            {
                if (m_PixelErro != value)
                {
                    m_PixelErro = value;
                    OnPixelErroValueChanged(value);

                }
            }
        }

        public float m_BaseMapDistance = 1000;
        public float BaseMapDistance
        {
            get { return m_BaseMapDistance; }
            set
            {
                if (m_BaseMapDistance != value)
                {
                    m_BaseMapDistance = value;
                    OnBaseMapDistanceValueChanged(value);

                }
            }
        }

        public float m_DetailDistance = 100;
        public float DetailDistance
        {
            get { return m_DetailDistance; }
            set
            {
                if (m_DetailDistance != value)
                {
                    m_DetailDistance = value;
                    OnDetailDistanceValueChanged(value);

                }
            }
        }

        public float m_DetailDensity = 100;
        public float DetailDensity
        {
            get { return m_DetailDensity; }
            set
            {
                if (m_DetailDensity != value)
                {
                    m_DetailDensity = value;
                    OnDetailDensityValueChanged(value);

                }
            }
        }

        public float m_TreeDistance = 4000;
        public float TreeDistance
        {
            get { return m_TreeDistance; }
            set
            {
                if (m_TreeDistance != value)
                {
                    m_TreeDistance = value;
                    OnTreeDistanceValueChanged(value);

                }
            }
        }

        public float m_BillBoardStartDistance = 500;
        public float BillBoardStartDistance
        {
            get { return m_BillBoardStartDistance; }
            set
            {
                if (m_BillBoardStartDistance != value)
                {
                    m_BillBoardStartDistance = value;
                    OnBillBoardStartDistanceValueChanged(value);

                }
            }
        }

        private float m_FadeLength = 10;
        public float FadeLength
        {
            get { return m_FadeLength; }
            set
            {
                if (m_FadeLength != value)
                {
                    m_FadeLength = value;
                    OnFadeLengthValueChanged(value);

                }
            }
        }

        private int m_DetailResolution_index = 5;
        public int DetailResolution_index
        {
            get { return m_DetailResolution_index; }
            set
            {
                if (m_DetailResolution_index != value)
                {
                    m_DetailResolution_index = value;
                    OnDetailResolutionValueChanged(value);

                }
            }
        }
        private int m_ResolutionPerPatch_index = 1;
        public int  ResolutionPerPatch_index
        {
            get { return m_ResolutionPerPatch_index; }
            set
            {
                if (m_ResolutionPerPatch_index != value)
                {
                    m_ResolutionPerPatch_index = value;
                    OnResolutionPerPatchValueChanged(value);

                }
            }
        }

        private int m_BaseMapResolution_index = 5;
        public int BaseMapResolution_index
        {
            get { return m_BaseMapResolution_index; }
            set
            {
                if (m_BaseMapResolution_index != value)
                {
                    m_BaseMapResolution_index = value;
                    OnBaseMapResolutionValueChanged(value);

                }
            }
        }
 
        public void OnPixelErroValueChanged(float value)
        {
            foreach(var t in terrains)
                t.terrain.heightmapPixelError = value;
        }
        public void OnBaseMapDistanceValueChanged(float value)
        {
            foreach (var t in terrains)
                t.terrain.basemapDistance = value;
        }
        public void OnDetailDistanceValueChanged(float value)
        {
            foreach (var t in terrains)
                t.terrain.detailObjectDistance = value;
        }
        public void OnDetailDensityValueChanged(float value)
        {
            foreach (var t in terrains)
                t.terrain.detailObjectDensity = value;
        }
        
        public void OnTreeDistanceValueChanged(float value)
        {
            foreach (var t in terrains)
                t.terrain.treeDistance = value;
        }
        public void OnBillBoardStartDistanceValueChanged(float value)
        {
            foreach (var t in terrains)
                t.terrain.treeBillboardDistance = value;
        }
        public void OnFadeLengthValueChanged(float value)
        {
            foreach (var t in terrains)
                t.terrain.treeCrossFadeLength= value;
        }

        public void OnDetailResolutionValueChanged(int value)
        {
            var detailResolution = availableHeights[value];
            foreach (var t in terrains)
            {
                var resolutionPerPatch = t.terrain.terrainData.detailResolutionPerPatch;
                t.terrain.terrainData.SetDetailResolution(detailResolution, resolutionPerPatch);
            }
        }
        public void OnResolutionPerPatchValueChanged(int value)
        {
            var resolutionPerPatch = availableHeightsResolutionPrePec[value];

            foreach (var t in terrains)
            {
                var detailResolution = t.terrain.terrainData.detailResolution;
                t.terrain.terrainData.SetDetailResolution(detailResolution, resolutionPerPatch);
            }

        }
        public void OnBaseMapResolutionValueChanged(int value)
        {
            var baseMapResolution = availableHeights[value];

            foreach (var t in terrains)
                t.terrain.terrainData.baseMapResolution = baseMapResolution;
        }


        #endregion
        
        public void GetStoredHeightmap(string heightmapName= "File_Data")
        {
            var heightmap = (TextAsset)Resources.Load(("HeightmapData/"+ heightmapName), typeof(TextAsset));

            if (data != null && heightmap)
                data.floatheightData = GISTerrainLoaderHeightmapSerializer.DeserializeHeightMap(heightmap.bytes, new Vector2(data.mapSize_col_x, data.mapSize_row_y));

        }

    }
}