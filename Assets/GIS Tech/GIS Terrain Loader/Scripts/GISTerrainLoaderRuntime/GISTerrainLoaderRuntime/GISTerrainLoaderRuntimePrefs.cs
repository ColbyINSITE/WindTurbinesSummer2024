/*     Unity GIS Tech 2020-2021      */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderRuntimePrefs : MonoSingleton<GISTerrainLoaderRuntimePrefs>
    {

#if UNITY_EDITOR

        public int lastTab = 0;
#endif
        public Projections Projection= Projections.Geographic_LatLon_Decimale;

        public TerrainElevation TerrainElevation = TerrainElevation.RealWorldElevation;
        public TiffElevationSource tiffElevationSource = TiffElevationSource.DEM;
        public int EPSGCode;

        public TerrainDimensionsMode terrainDimensionMode = TerrainDimensionsMode.AutoDetection;
        public bool TerrainHasDimensions;
        public OptionEnabDisab UnderWater = OptionEnabDisab.Disable;
        public FixOption TerrainFixOption = FixOption.Disable;
        public Vector2 TerrainMaxMinElevation = new Vector2(0, 0);

        public OptionEnabDisab TerrainLayerSet = OptionEnabDisab.Disable;
        public int TerrainLayer;

        public ReadingMode readingMode = ReadingMode.Full;
        public bool ShowCoordinates;
        public DVector2 SubRegionUpperLeftCoordiante;
        public DVector2 SubRegionDownRightCoordiante;

        public int heightmapResolution_index = 2;
        public int detailResolution_index = 5;
        public int resolutionPerPatch_index = 1;
        public int baseMapResolution_index = 5;
        public int[] heightmapResolutions = { 33, 65, 129, 257, 513, 1025, 2049, 4097 };
        public string[] heightmapResolutionsSrt = new string[] { "33", "65", "129", "257", "513", "1025", "2049", "4097" };


        public int[] availableHeights = { 32, 64, 129, 256, 512, 1024, 2048, 4096 };
        public string[] availableHeightSrt = new string[] { "32", "64", "128", "256", "512", "1024", "2048", "4097" };

        public int[] availableHeightsResolutionPrePec = { 4, 8, 16, 32 };
        public string[] availableHeightsResolutionPrePectSrt = new string[] { "4", "8", "16", "32" };
 

        public int detailResolution = 2048;
        public int resolutionPerPatch = 16;
        public int baseMapResolution = 1024;
        public int heightmapResolution = 128;
        public float PixelError;
        public int BaseMapDistance = 2000;
        public TerrainMaterialMode terrainMaterialMode = TerrainMaterialMode.Standard;
        public Material terrainMaterial = null;

        public float TerrainExaggeration;
        public Vector2Int terrainCount = Vector2Int.one;
        public Vector3 terrainScale = Vector3.one;
        public Vector2 TerrainDimensions;


        public TextureMode textureMode = TextureMode.WithTexture;
        public int textureHeight = 1024;
        public int textureWidth = 1024;
        public Color TerrainEmptyColor = Color.white;
        public bool UseTerrainEmptyColor = false;
        public TexturesLoadingMode textureloadingMode = TexturesLoadingMode.AutoDetection;
        public OptionEnabDisab UnderWaterShader = OptionEnabDisab.Disable;
        public ShaderType TerrainShaderType = ShaderType.ColorRamp;
        public OptionEnabDisab SaveShaderTextures = OptionEnabDisab.Disable;

        //Raw File Parameters
        public RawDepth Raw_Depth = RawDepth.Bit16;
        public RawByteOrder Raw_ByteOrder = RawByteOrder.Windows;


        [SerializeField]
        public GISTerrainLoaderTerrainLayer BaseTerrainLayers = new GISTerrainLoaderTerrainLayer();
        [SerializeField]
        public List<GISTerrainLoaderTerrainLayer> TerrainLayers = new List<GISTerrainLoaderTerrainLayer>();
        public float Slope=0.1f;
        public int MergeRaduis=1;
        public int MergingFactor=1;


        public bool UseTerrainSurfaceSmoother;
        public int TerrainSurfaceSmoothFactor = 4;
        public bool UseTerrainHeightSmoother;
        public float TerrainHeightSmoothFactor = 0.05f;


        public VectorType vectorType = VectorType.OpenStreetMap;

        public bool EnableTreeGeneration;
        public float TreeDistance = 4000f;
        public float BillBoardStartDistance = 300;
 

        public bool EnableGrassGeneration;

        public float GrassScaleFactor = 1.5f;
        public float DetailDistance = 400;

        [SerializeField]
        public List<GISTerrainLoaderSO_GeoPoint> GeoPointsPrefab = new List<GISTerrainLoaderSO_GeoPoint>();
        [SerializeField]
        public List<GISTerrainLoaderSO_Building> BuildingsPrefab = new List<GISTerrainLoaderSO_Building>();
        [SerializeField]
        public List<GISTerrainLoaderSO_Road> RoadsPrefab = new List<GISTerrainLoaderSO_Road>();
        [SerializeField]
        public List<GISTerrainLoaderSO_Tree> TreePrefabs = new List<GISTerrainLoaderSO_Tree>();
        [SerializeField]
        public List<GISTerrainLoaderSO_GrassObject> GrassPrefabs = new List<GISTerrainLoaderSO_GrassObject>();

        
        public bool EnableGeoPointGeneration;
        public bool EnableRoadGeneration;
        public bool EnableRoadName;
        public RoadGenerationType RoadType = RoadGenerationType.Line;


        public bool EnableBuildingGeneration;

        public bool EnableGeoLocationPointGeneration;
        public GameObject GeoPointPrefab;
        public GISTerrainLoaderSO_Road PathPrefab;


        public bool IsVectorGenerationEnabled(string fileExtension)
        {
            var isGeoFile = GISTerrainLoaderSupport.GeoFile.Contains(fileExtension);
 
            var val = false;

            if (isGeoFile && (EnableTreeGeneration || EnableGrassGeneration || EnableRoadGeneration || EnableBuildingGeneration))
                val = true;
            else
                val = false;

            return val;
        }

        public void ResetPrefs()
        {
            readingMode = ReadingMode.Full;
            ShowCoordinates = false;
            SubRegionDownRightCoordiante = new DVector2(0, 0);
            SubRegionUpperLeftCoordiante = new DVector2(0, 0);
            ////////////////////////////////////////////////////////////////////////////////

            TerrainElevation = TerrainElevation.RealWorldElevation;
            TerrainExaggeration =  0.27f;
            terrainDimensionMode = TerrainDimensionsMode.AutoDetection;
            TerrainDimensions = new Vector2(10, 10);
            terrainScale = new Vector3(1, 1, 1);
            UnderWater = OptionEnabDisab.Disable;

            ////////////////////////////////////////////////////////////////////////////////

            heightmapResolution_index = 2;
            heightmapResolution = heightmapResolutions[heightmapResolution_index];

            detailResolution_index = 4;
            detailResolution = availableHeights[detailResolution_index];

            resolutionPerPatch_index = 1;
            resolutionPerPatch = availableHeightsResolutionPrePec[resolutionPerPatch_index];

            baseMapResolution_index = 4;
            baseMapResolution = availableHeights[baseMapResolution_index];

            PixelError = 1;
            BaseMapDistance = 1000;

            terrainMaterialMode = TerrainMaterialMode.Standard;

            ////////////////////////////////////////////////////////////////////////////////

            textureMode = TextureMode.WithTexture;
            textureWidth = 1024;
            textureHeight = 1024;
            TerrainEmptyColor = Color.white;
            UseTerrainEmptyColor = false;

            ////////////////////////////////////////////////////////////////////////////////

            UseTerrainHeightSmoother = false;
            TerrainHeightSmoothFactor = 0.05f;
            UseTerrainSurfaceSmoother = false;
            TerrainSurfaceSmoothFactor = 4;

            ////////////////////////////////////////////////////////////////////////////////

            EnableTreeGeneration = false;
            TreeDistance = 4000;
            BillBoardStartDistance = 300;
            TreePrefabs = new List<GISTerrainLoaderSO_Tree>();

            EnableGrassGeneration = false;
            GrassScaleFactor = 2.5f;
            DetailDistance = 400;
            GrassPrefabs = new List<GISTerrainLoaderSO_GrassObject>();

            EnableRoadGeneration = false;
            RoadType = RoadGenerationType.Line;

        }
        public void LoadAllTreePrefabs()
        {
            var prefabs = Resources.LoadAll("Prefabs/Environment/Trees", typeof(GISTerrainLoaderSO_Tree));

            if (prefabs.Length > 0)
            {
                TreePrefabs.Clear();

                foreach (var prefab in prefabs)
                {
                    if (prefab != null)
                        TreePrefabs.Add(prefab as GISTerrainLoaderSO_Tree);
                }

            }
            else
                Debug.Log("Not tree prefabs detected in 'GIS Tech/GIS Terrain Loader/Resources/Prefabs/Environment/Trees'");
        }
        public void LoadAllGrassPrefabs()
        {
            var prefabs = Resources.LoadAll("Prefabs/Environment/Grass", typeof(GISTerrainLoaderSO_GrassObject));

            if (prefabs.Length > 0)
            {
                GrassPrefabs.Clear();

                foreach (var prefab in prefabs)
                {
                    if (prefab != null)
                        GrassPrefabs.Add(prefab as GISTerrainLoaderSO_GrassObject);
                }

            }
            else
                Debug.Log("Not tree prefabs detected in 'GIS Tech/GIS Terrain Loader/Resources/Prefabs/Environment/Grass'");
        }

    }
}