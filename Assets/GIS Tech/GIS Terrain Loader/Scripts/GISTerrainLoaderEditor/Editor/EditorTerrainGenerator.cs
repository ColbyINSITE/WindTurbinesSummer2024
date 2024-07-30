/*     Unity GIS Tech 2020-2021      */
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GISTech.GISTerrainLoader
{
    public class EditorTerrainGenerator : EditorWindow
    {
        public const string version = "2.8";
        #region Variables
        #region TerrainGenerator
        private static GeneratorState State = GeneratorState.idle;
        private static ReadingMode readingMode;
        private static ProjectionMode projectionMode = ProjectionMode.Auto;
        private static TiffElevationSource tiffElevationSource = TiffElevationSource.DEM;
        private static int EPSGCode;

        #if DotSpatial
        private static bool ShowProjectionMode=false;
        # endif

        private static bool ShowTiffElevationSourceMode;
        public DVector2 SubRegionUpperLeftCoordiante;
        public DVector2 SubRegionDownRightCoordiante;


        private bool ShowCoordinates;


        private int CurrentTerrainIndex = 0;
        private float ElevationScaleValue = 1112.0f;
        private float ScaleFactor = 1000;
        public TerrainObject[,] terrains = new TerrainObject[0,0];
        private TerrainContainerObject GeneratedContainer;

        private string LoadedFileExtension = "";
 
        public static float s_progress = 0f;
        public static string s_phase = "";

        //Terrain Prefs //
        private bool ShowSubRegion = false;
        private bool ShowMainTerrainFile = true;
        private bool ShowSetTerrainPref = true;
        private bool ShowTerrainPref = true;
        private bool ShowOSMVectorData = true;
        private bool ShowSmoothingOpr = true;
        private bool ShowTexturePref = true;

        private Texture2D m_terrain;
        private Texture2D m_downloaExamples;
        private Texture2D m_helpIcon;
        private Texture2D m_resetPrefs;
        private Texture2D m_aboutIcon;

        private Vector2 scrollPos = Vector2.zero;

        private GISTerrainLoaderElevationInfo ElevationInfo;

#endregion
#region TerrainPrefs
        private bool TerrainHasDimensions;
        private DVector2 TerrainDimensions = new DVector2(0, 0);
        private Vector2 m_terrainDimensions;
        private UnityEngine.Object m_TerrainFile;
        public UnityEngine.Object TerrainFile
        {
            get { return m_TerrainFile; }
            set
            {
                if (m_TerrainFile != value)
                {
                    m_TerrainFile = value;
                    OnTerrainFileChanged(TerrainFile);
                }
            }
        }

        public string TerrainFileName;
        public string TerrainFilePath;
        public float TerrainExaggeration;

        public Vector2Int terrainCount = Vector2Int.one;
        public Vector3 terrainScale = Vector3.one;


        public OptionEnabDisab RemovePrvTerrain;

        public int heightmapResolution = 1025;
        public int detailResolution = 2048;
        public int resolutionPerPatch = 16;
        public int baseMapResolution = 513;

        public int heightmapResolution_index = 2;
        public int detailResolution_index = 5;
        public int resolutionPerPatch_index = 1;
        public int baseMapResolution_index = 5;

        private int[] heightmapResolutions = { 33, 65, 129, 257, 513, 1025, 2049, 4097 };
        public string[] heightmapResolutionsSrt = new string[] { "33", "65", "129", "257", "513", "1025", "2049", "4097" };


        private int[] availableHeights = { 32, 64, 129, 256, 512, 1024, 2048, 4096 };
        public string[] availableHeightSrt = new string[] { "32", "64", "128", "256", "512", "1024", "2048", "4096" };

        private int[] availableHeightsResolutionPrePec = {8, 16, 32 };
        public string[] availableHeightsResolutionPrePectSrt = new string[] {"8", "16", "32" };

        public float PixelErro = 1;
        public float BaseMapDistance = 1000;

        private TerrainMaterialMode terrainMaterialMode = TerrainMaterialMode.Standard;
        private Material terrainMaterial = null;


        //Raw File Parameters
        private bool ShowRawParameters = false;
        public RawDepth Raw_Depth = RawDepth.Bit16;
        public RawByteOrder Raw_ByteOrder = RawByteOrder.Windows;


#endregion
#region TerrainTextures

        public TerrainElevation terrainElevation = TerrainElevation.RealWorldElevation;
        public TextureMode textureMode = TextureMode.WithTexture;
        public TexturesLoadingMode textureloadingMode = TexturesLoadingMode.AutoDetection;
        public TerrainDimensionsMode terrainDimensionMode = TerrainDimensionsMode.AutoDetection;
        public OptionEnabDisab UnderWater = OptionEnabDisab.Disable;
        public FixOption TerrainFixOption = FixOption.Disable;
        public ShaderType TerrainShaderType = ShaderType.ColorRamp;
        public OptionEnabDisab UnderWaterShader = OptionEnabDisab.Disable;
        public OptionEnabDisab SaveShaderTextures = OptionEnabDisab.Disable;
        public OptionEnabDisab TerrainLayerSet = OptionEnabDisab.Disable;




        public int TerrainLayer;

        public Vector2 TerrainMaxMinElevation = new Vector2(0, 0);

        public int textureHeight = 1024;
        public int textureWidth = 1024;
        public OptionEnabDisab UseTerrainEmptyColor = OptionEnabDisab.Disable;
        public Color TerrainEmptyColor = Color.white;

        [SerializeField]
        public GISTerrainLoaderTerrainLayer BaseTerrainLayers = new GISTerrainLoaderTerrainLayer();
        [SerializeField]
        public List<GISTerrainLoaderTerrainLayer> TerrainLayers = new List<GISTerrainLoaderTerrainLayer>();
        public float Slope;
        public int MergeRaduis;
        public int MergingFactor;
#endregion
#region TerrainSmoothing
        public OptionEnabDisab UseTerrainHeightSmoother;
        public OptionEnabDisab UseTerrainSurfaceSmoother;

        private static float TerrainHeightSmoothFactor = 0.05f;

        private static int TerrainSurfaceSmoothFactor = 4;
#endregion
#region TerrainVector
        public VectorType vectorType = VectorType.OpenStreetMap;
 
        [SerializeField]
        List<GISTerrainLoaderSO_Tree> TreePrefabs = new List<GISTerrainLoaderSO_Tree>();
        private float TreeDistance = 4000f;
        private float BillBoardStartDistance = 300;
 
        private float DetailDistance;
        private float GrassScaleFactor;

        [SerializeField]
        List<GISTerrainLoaderSO_GrassObject> GrassPrefabs = new List<GISTerrainLoaderSO_GrassObject>();

        public OptionEnabDisab EnableGeoPointGeneration = OptionEnabDisab.Disable;
        public OptionEnabDisab EnableRoadGeneration = OptionEnabDisab.Disable;
        public OptionEnabDisab EnableTreeGeneration = OptionEnabDisab.Disable;
        public OptionEnabDisab EnableBuildingGeneration = OptionEnabDisab.Disable;
        public OptionEnabDisab EnableGeoLocationPointGeneration = OptionEnabDisab.Disable;
        public OptionEnabDisab EnableGrassGeneration = OptionEnabDisab.Disable;
        
        public RoadGenerationType RoadType = RoadGenerationType.Line;
        private bool EnableRoadName;
        private List<GISTerrainLoaderSO_Road> RoadsPrefab = new List<GISTerrainLoaderSO_Road>();
        private List<GISTerrainLoaderSO_Building> BuildingsPrefab = new List<GISTerrainLoaderSO_Building>();
        private List<GISTerrainLoaderSO_GeoPoint> GeoPointsPrefab = new List<GISTerrainLoaderSO_GeoPoint>();
 
        private GameObject GeoPointPrefab;
        private GISTerrainLoaderSO_Road PathPrefab;
#endregion
#endregion
        public static EditorTerrainGenerator window;
 
        [MenuItem("Tools/GIS Tech/GIS Terrain Loader/Editor GTL", false, 1)]
        static void Init()
        {
            Application.runInBackground = true;
            window = EditorWindow.GetWindow<EditorTerrainGenerator>(false, "GIS Terrain Loader");

            window.ShowUtility();
            window.minSize = new Vector2(400, 500);
            window.Show();
        }
        void OnInspectorUpdate() { Repaint(); }
        void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            OnMainToolbarGUI();

            OnTerrainFileGUI();

            OnTerrainDimensionScaleGUI();

            OnTerrainPreferencesGUI();

            OnTexturePrefrencesGUI();

            OnTerrainSmoothignOperationGUI();

            OnTerrainVectorGenerationGUI();

            GeneratingBtn();

            EditorGUILayout.EndVertical();
        }

#region GUIElements
        private void OnMainToolbarGUI()
        {
            GUIStyle buttonStyle = new GUIStyle(EditorStyles.toolbarButton);

            GUILayout.BeginHorizontal();

            GUILayout.Label("", buttonStyle);

            if (GUILayout.Button(new GUIContent(" GIS Data Downloader ", m_downloaExamples, "Link to 'GIS Data Downloader', use this asset to download real world data from online servers "), buttonStyle, GUILayout.ExpandWidth(true)))
            {
                System.Diagnostics.Process.Start("https://assetstore.unity.com/packages/tools/integration/gis-data-downloader-199112?fbclid=IwAR2dVOdf-vIJp3fO8QGz6Gcepo4_cL0rp144cSUAwGXfFdLr8LFE7QqzcUA#content");

            }

            if (GUILayout.Button(new GUIContent(" Download Examples", m_downloaExamples, "Link to download 'GIS Terrain Loader Data Examples', it contains all GIS Data that can be loaded by GTL "), buttonStyle, GUILayout.ExpandWidth(true)))
            {
                System.Diagnostics.Process.Start("https://assetstore.unity.com/packages/tools/integration/gis-terrain-loader-data-exemples-152552");

            }

            if (GUILayout.Button(new GUIContent(" Help", m_helpIcon, "Ask a question in forum page "), buttonStyle, GUILayout.ExpandWidth(true)))
            {
                System.Diagnostics.Process.Start("https://forum.unity.com/threads/released-gis-terrain-loader.726206/");

            }

            if (GUILayout.Button(new GUIContent(" Reset", m_resetPrefs, " Reset all Prefs to default "), buttonStyle, GUILayout.ExpandWidth(true)))
            {
                ResetPrefs();

            }
            if (GUILayout.Button(new GUIContent(" About", m_aboutIcon, "About GIS Terrain Loader "), buttonStyle, GUILayout.ExpandWidth(true)))
            {
                GISTerrainLoaderAboutWindow.Init();

            }

            GUILayout.EndHorizontal();

        }
        private void OnTerrainFileGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.BeginVertical(GUI.skin.button);
            ShowMainTerrainFile = EditorGUILayout.Foldout(ShowMainTerrainFile, " DEM Terrain File ");
            EditorGUILayout.EndVertical();

            if (ShowMainTerrainFile)
            {

                GUILayout.BeginHorizontal();

                GUILayout.Label(new GUIContent(" Terrain File ", m_terrain, " After importing DEM file into 'Resources/GIS Terrains' Folder, drag and drop it into 'Terrain File Field' "), GUILayout.MaxWidth(200));
                TerrainFile = EditorGUILayout.ObjectField(TerrainFile, typeof(UnityEngine.Object) , true,GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                EditorGUILayout.HelpBox("Drag and drop DEM File here, Edit terrain parameters and click on Generate terrain", MessageType.Info);
 
                if (ShowSubRegion)
                {

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent(" File Reading Mode ", "Default is full heightmap mode, used to read whole hightmap file; sub region mode used to import a sub region of the file instead, note that coordinates of sub regions is needed; this option available only for GeoRefenced files (Tiff,HGT,BIL,ASC,FLT) "), GUILayout.MaxWidth(200));
                    readingMode = (ReadingMode)EditorGUILayout.EnumPopup("", readingMode);
                    GUILayout.EndHorizontal();


                    if (readingMode == ReadingMode.SubRegion)
                    {
                        CoordinatesBarGUI();
                    }
#if DotSpatial
                    if (ShowProjectionMode)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent(" Projection Mode ", " Use this option to customize the projection of tiff file by setting EPSG code, note that DotSpatial lib is required"), GUILayout.MaxWidth(200));
                        projectionMode = (ProjectionMode)EditorGUILayout.EnumPopup("", projectionMode);
                        GUILayout.EndHorizontal();

                        if (projectionMode == ProjectionMode.Custom)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(new GUIContent("  EPSG Code ", " Set the Projection Code"), GUILayout.MaxWidth(200));
                            EPSGCode = EditorGUILayout.IntField("", EPSGCode);
                            GUILayout.EndHorizontal();
                        }
                        else
                            EPSGCode = 0;


                    }
#endif

                    //DEM's Particularity
                    //Tiff
                    if (ShowTiffElevationSourceMode)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent(" Tiff Elevation Source ", " If you are using Tiff based on grayscale use this option to load heightmap data from grayscale color"), GUILayout.MaxWidth(200));
                        tiffElevationSource = (TiffElevationSource)EditorGUILayout.EnumPopup("", tiffElevationSource);
                        GUILayout.EndHorizontal();
                    }
                }


            }


            //DEM's Particularity
            //Raw
            if(ShowRawParameters)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(" Depth ", " Depth of raw file 8/16 bit"), GUILayout.MaxWidth(200));
                Raw_Depth = (RawDepth)EditorGUILayout.EnumPopup("", Raw_Depth);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(" Byts Order ", " Raw Byts Order Mac/Windows"), GUILayout.MaxWidth(200));
                Raw_ByteOrder = (RawByteOrder)EditorGUILayout.EnumPopup("", Raw_ByteOrder);
                GUILayout.EndHorizontal();
            }
 
            EditorGUILayout.EndVertical();
        }
        private void CoordinatesBarGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.BeginVertical(GUI.skin.button);
            ShowCoordinates = EditorGUILayout.Foldout(ShowCoordinates, "Sub Region Coordinates");
            EditorGUILayout.EndVertical();


            if (ShowCoordinates)
            {
                EditorGUILayout.HelpBox(" Set Sub Region Heightmap coordinates ", MessageType.Info);

                GUILayout.Label("Upper-Left : ", GUILayout.ExpandWidth(false));

                GUILayout.BeginHorizontal();

                GUILayout.Label("Latitude : ", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
                GUI.SetNextControlName("UpperLeftCoordianteLat");
                SubRegionUpperLeftCoordiante.y = EditorGUILayout.DoubleField(SubRegionUpperLeftCoordiante.y, GUILayout.ExpandWidth(true));

                GUILayout.Label("Longitude : ", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
                GUI.SetNextControlName("UpperLeftCoordianteLon");
                SubRegionUpperLeftCoordiante.x = EditorGUILayout.DoubleField(SubRegionUpperLeftCoordiante.x, GUILayout.ExpandWidth(true));

                GUILayout.EndHorizontal();


                GUILayout.Label("Down-Right : ", GUILayout.ExpandWidth(false));

                GUILayout.BeginHorizontal();

                GUILayout.Label("Latitude : ", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
                GUI.SetNextControlName("DownRightCoordianteLat");
                SubRegionDownRightCoordiante.y = EditorGUILayout.DoubleField(SubRegionDownRightCoordiante.y, GUILayout.ExpandWidth(true));

                GUILayout.Label("Longitude : ", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
                GUI.SetNextControlName("DownRightCoordianteLon");
                SubRegionDownRightCoordiante.x = EditorGUILayout.DoubleField(SubRegionDownRightCoordiante.x, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                GUILayout.Label("", GUILayout.ExpandWidth(false));

            }

            EditorGUILayout.EndVertical();
        }
        private void OnTerrainDimensionScaleGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            GUI.backgroundColor = Color.green;
            EditorGUILayout.BeginVertical(GUI.skin.button);
            ShowSetTerrainPref = EditorGUILayout.Foldout(ShowSetTerrainPref, " Elevation Mode, Scale, Dimensions ");
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;

            if (ShowSetTerrainPref)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(" Terrain Elevation Mode ", "Generate Terrain By Loading Real Elevation Data or By using 'Exaggeration' value to set manualy terrain elevation factor"), GUILayout.MaxWidth(200));
                terrainElevation = (TerrainElevation)EditorGUILayout.EnumPopup("", terrainElevation);
                GUILayout.EndHorizontal();

                if (terrainElevation == TerrainElevation.ExaggerationTerrain)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent(" Exaggeration value ", "Vertical exaggeration can be used to emphasize subtle changes in a surface. This can be useful in creating visualizations of terrain where the horizontal extent of the surface is significantly greater than the amount of vertical change in the surface. A fractional vertical exaggeration can be used to flatten surfaces or features that have extreme vertical variation"), GUILayout.MaxWidth(200));
                    TerrainExaggeration = EditorGUILayout.Slider(TerrainExaggeration, 0, 1);
                    GUILayout.EndHorizontal();
                }
                


                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(" Terrain Dimensions Mode ", "This Option let us to load real terrain Width/Length for almost of supported types - We can set it to manual to make terrain small or large as we want by setting new W/L values in 'KM' "), GUILayout.MaxWidth(200));
                terrainDimensionMode = (TerrainDimensionsMode)EditorGUILayout.EnumPopup("", terrainDimensionMode);
                GUILayout.EndHorizontal();



                if (terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
                {

                    if (!TerrainHasDimensions)
                    {
                        GUILayout.BeginHorizontal();

                        GUILayout.Label(new GUIContent(" Set Terrain Dimensions [Km] ", "This appear when DEM file not loaded yet or has no real dimensions so we have to set Manualy terrain width/lenght in KM"), GUILayout.MaxWidth(200));

                        GUILayout.Label(" Width ");
                        TerrainDimensions.x = EditorGUILayout.DoubleField(TerrainDimensions.x, GUILayout.ExpandWidth(true));

                        GUILayout.Label(" Lenght ");
                        TerrainDimensions.y = EditorGUILayout.DoubleField(TerrainDimensions.y, GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        if (TerrainDimensions.x == 0 || TerrainDimensions.y == 0)
                            EditorGUILayout.HelpBox("Can not Detect Terrain bounds,You have to set terrain dimensions in Km", MessageType.Warning);
                    }
                }
                else
                if (terrainDimensionMode == TerrainDimensionsMode.Manual)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label(new GUIContent(" Set Terrain Dimensions [Km] ", "Set Manually terrain width/length in KM"), GUILayout.MaxWidth(200));

                    GUILayout.Label(" Width ");
                    TerrainDimensions.x = EditorGUILayout.DoubleField(TerrainDimensions.x, GUILayout.ExpandWidth(true));

                    GUILayout.Label(" Lenght ");
                    TerrainDimensions.y = EditorGUILayout.DoubleField(TerrainDimensions.y, GUILayout.ExpandWidth(true));

                    GUILayout.EndHorizontal();


                    if (TerrainDimensions.x == 0 || TerrainDimensions.y == 0)
                        EditorGUILayout.HelpBox("Can not Detect Terrain bounds,You have to set terrain dimensions in Km", MessageType.Warning);
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(" UnderWater ", "Enable This Option to load negative values from DEM files "), GUILayout.MaxWidth(200));
                UnderWater = (OptionEnabDisab)EditorGUILayout.EnumPopup("", UnderWater);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(" Fix Terrain ", " (Only for Real World Data) Use this option to fix terrain Min/Max detecion to used in avoid extrem elevation values in order to generate terrain without any deformation Manually OR Automa "), GUILayout.MaxWidth(200));
                TerrainFixOption = (FixOption)EditorGUILayout.EnumPopup("", TerrainFixOption);
                GUILayout.EndHorizontal();

                if (TerrainFixOption == FixOption.ManualFix)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label(new GUIContent(" Elevation [m] ", "Set Manually terrain Max and Min Elevation in [m]"), GUILayout.MaxWidth(200));
                    GUILayout.Label("  Min ");
                    TerrainMaxMinElevation.x = EditorGUILayout.FloatField(TerrainMaxMinElevation.x, GUILayout.ExpandWidth(true));

                    GUILayout.Label("  Max ");
                    TerrainMaxMinElevation.y = EditorGUILayout.FloatField(TerrainMaxMinElevation.y, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                    if (TerrainDimensions.x == 0 || TerrainDimensions.y == 0)
                        EditorGUILayout.HelpBox("Can not Detect Terrain bounds,You have to set terrain dimensions in Km", MessageType.Warning);
 
                }

                GUILayout.BeginHorizontal();

                GUILayout.Label(new GUIContent(" Terrain Scale ", " Specifies the terrain scale factor in three directions (if terrain is large with 1 value you can set small float value like 0.5f - 0.1f - 0.01f"), GUILayout.MaxWidth(200));
                terrainScale = EditorGUILayout.Vector3Field("", terrainScale);
                GUILayout.EndHorizontal();

                if (terrainScale.x == 0 || terrainScale.y == 0 || terrainScale.z == 0)
                    EditorGUILayout.HelpBox("Check your Terrain Scale (Terrain Scale is null !)", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }
        private void OnTerrainPreferencesGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUI.backgroundColor = Color.green;
            EditorGUILayout.BeginVertical(GUI.skin.button);
            ShowTerrainPref = EditorGUILayout.Foldout(ShowTerrainPref, " Terrain Preferences ");
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;

            if (ShowTerrainPref)
            {

                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(" Heightmap Resolution ", "The pixel resolution of the Terrain’s heightmap"), GUILayout.MaxWidth(200));
                heightmapResolution_index = EditorGUILayout.Popup(heightmapResolution_index, heightmapResolutionsSrt, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(" Detail Resolution ", "The number of cells available for placing details onto the Terrain tile used to controls grass and detail meshes. Lower you set this number performance will be better"), GUILayout.MaxWidth(200));
                detailResolution_index = EditorGUILayout.Popup(detailResolution_index, availableHeightSrt, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(" Resolution Per Patch ", "The number of cells in a single patch (mesh), recommended value is 16 for very large detail object distance "), GUILayout.MaxWidth(200));
                resolutionPerPatch_index = EditorGUILayout.Popup(resolutionPerPatch_index, availableHeightsResolutionPrePectSrt, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(" Base Map Resolution ", "Resolution of the composite texture used on the terrain when viewed from a distance greater than the Basemap Distance"), GUILayout.MaxWidth(200));
                baseMapResolution_index = EditorGUILayout.Popup(baseMapResolution_index, availableHeightSrt, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(" Pixel Error ", " The accuracy of the mapping between Terrain maps (such as heightmaps and Textures) and generated Terrain. Higher values indicate lower accuracy, but with lower rendering overhead. "), GUILayout.MaxWidth(200));
                PixelErro = EditorGUILayout.Slider(PixelErro,1,200, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(" BaseMap Distance ", " The maximum distance at which Unity displays Terrain Textures at full resolution. Beyond this distance, the system uses a lower resolution composite image for efficiency "), GUILayout.MaxWidth(200));
                BaseMapDistance = EditorGUILayout.Slider(BaseMapDistance, 1, 20000, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(" Terrain Material Mode ", "This option used to cutomize terrain material ex : in case of using HDRP "), GUILayout.MaxWidth(200));
                terrainMaterialMode = (TerrainMaterialMode)EditorGUILayout.EnumPopup("", terrainMaterialMode);
                GUILayout.EndHorizontal();
 
                if (terrainMaterialMode == TerrainMaterialMode.Custom)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("  Terrain Material ", "Materail that will be used in the generated terrains "), GUILayout.MaxWidth(200));
                    terrainMaterial = (Material)EditorGUILayout.ObjectField(terrainMaterial, typeof(UnityEngine.Material),true, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();
 
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(" Set Terrain Layer ", "This option cutomize terrain Layer "), GUILayout.MaxWidth(200));
                TerrainLayerSet = (OptionEnabDisab)EditorGUILayout.EnumPopup("", TerrainLayerSet);
                GUILayout.EndHorizontal();

                if(TerrainLayerSet == OptionEnabDisab.Enable)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent(" Terrain Layer ", " Set Terrain Layer"), GUILayout.MaxWidth(220));
                    TerrainLayer = EditorGUILayout.IntField(TerrainLayer, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                }
            }

            EditorGUILayout.EndVertical();
        }
        private void OnTexturePrefrencesGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            GUI.backgroundColor = Color.green;
            EditorGUILayout.BeginVertical(GUI.skin.button);
            ShowTexturePref = EditorGUILayout.Foldout(ShowTexturePref, " Terrain Textures  ");
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;

            if (ShowTexturePref)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(" Texturing Mode ", "Generate Terrain with or without textures (Specifies the count terrains is needed when selecting 'Without' because Texture folder will not readed "), GUILayout.MaxWidth(200));
                textureMode = (TextureMode)EditorGUILayout.EnumPopup("", textureMode, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();


                switch (textureMode)
                {
                    case TextureMode.WithTexture:

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("  Textures Loading Mode ", " The Creation of terrain tiles is based on the number of texture tiles existing in the terrain texture folder, setting this parameter to Auto means that GTL will load and generate terrains by loading directly textures from texture folder /// if it set to Manually, GTL will make some operations of merging and spliting existing textures to make them simulair to terrain tiles count ' Attention' : this operation may consume memory when textures are larges '"), GUILayout.MaxWidth(200));
                        textureloadingMode = (TexturesLoadingMode)EditorGUILayout.EnumPopup("", textureloadingMode, GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        if (textureloadingMode == TexturesLoadingMode.Manual)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(new GUIContent("   Count Tiles ", " Specifie the number of terrain tiles , ' Attention '  Count Tiles set is different than the number of terrain textures tiles (Located in the Terrain texture folder), some operations (Spliting/mergins) textures will excuted so becarful when textures are large"), GUILayout.MaxWidth(200));
                            terrainCount = EditorGUILayout.Vector2IntField("", terrainCount);
                            GUILayout.EndHorizontal();

                            EditorGUILayout.HelpBox("' Attention Memory ' When terrain Count Tiles is different than the number of terrain textures tiles(Located in the Terrain texture folder), some operations(Spliting / Mergins) textures will excuted so becarful for large textures ", MessageType.Warning);

                        }

                        break;

                    case TextureMode.Splatmapping:

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("  Count Tiles ", " Number of Terrain Tiles in X-Y, This Option is avalaible Only when Texturing mode set to 'Without' or 'Splatmapping' "), GUILayout.MaxWidth(200));
                        terrainCount = EditorGUILayout.Vector2IntField("", terrainCount);
                        GUILayout.EndHorizontal();

                        if (GUILayout.Button(new GUIContent(" Distributing Values", m_resetPrefs, "Set All Splatmapping values to default and distributing slopes values "), new GUIStyle(EditorStyles.toolbarButton), GUILayout.ExpandWidth(true)))
                        {
                            Slope = 0.1f;
                            MergeRaduis = 1;
                            MergingFactor = 1;

                            float step = 1f / TerrainLayers.Count;

                            for (int i = 0; i < TerrainLayers.Count; i++)
                            {
                                TerrainLayers[i].X_Height = i * step;
                                TerrainLayers[i].Y_Height = (i + 1) * step;
                            }

                        }

                        GUILayout.BeginVertical();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("  Slope ", " Used to normalized the slope in Y dir, The default value = 0"), GUILayout.MaxWidth(200));
                        Slope = EditorGUILayout.Slider(Slope, 0.0f, 1, GUILayout.MaxWidth(200), GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("  Merging Raduis ", " Used to precise the raduis of merging between layers, 0 value means that no merging operation will apply  "), GUILayout.MaxWidth(200));
                        MergeRaduis = EditorGUILayout.IntSlider(MergeRaduis, 0, 500, GUILayout.MaxWidth(200), GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("  Merging Factor ", " Used to precise how many times the merging will applyed on the terrain, the default is 1 "), GUILayout.MaxWidth(200));
                        MergingFactor = EditorGUILayout.IntSlider(MergingFactor, 1, 5, GUILayout.MaxWidth(200), GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();


                        GUILayout.BeginVertical();
                        GUILayout.Label(" ");
                        GUILayout.Label(new GUIContent("  Base Terrain Map ", " this will be the first splatmap for slope = 0"), GUILayout.MaxWidth(200));
                        BaseTerrainLayers.ShowHeight = false;
                        ScriptableObject target = this;
                        SerializedObject BaseLayerso = new SerializedObject(target);
                        SerializedProperty BaseLayerProperty = BaseLayerso.FindProperty("BaseTerrainLayers");
                        EditorGUILayout.PropertyField(BaseLayerProperty, true);
                        BaseLayerso.ApplyModifiedProperties();

                        GUILayout.EndVertical();



                        GUILayout.BeginVertical();
                        GUILayout.Label(" ");

                        target = this;
                        SerializedObject LayersSO = new SerializedObject(target);
                        SerializedProperty LayersProperty = LayersSO.FindProperty("TerrainLayers");
                        EditorGUILayout.PropertyField(LayersProperty, true);
                        LayersSO.ApplyModifiedProperties();
                        foreach (var layer in TerrainLayers)
                            layer.ShowHeight = true;
                        GUILayout.EndVertical();

                        break;

                    case TextureMode.ShadedRelief:
                        
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("  Count Tiles ", " Number of Terrain Tiles in X-Y, This Option is avalaible Only when Texturing mode set to 'Without' or 'Splatmapping' "), GUILayout.MaxWidth(200));
                        terrainCount = EditorGUILayout.Vector2IntField("", terrainCount);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent(" Shader Type ", " Select terrain shader type "), GUILayout.MaxWidth(200));
                        TerrainShaderType = (ShaderType)EditorGUILayout.EnumPopup("", TerrainShaderType);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent(" UnderWater ", "Enable This Option to generate shaders to underwater terrains (Used to avoid blue color) "), GUILayout.MaxWidth(200));
                        UnderWaterShader = (OptionEnabDisab)EditorGUILayout.EnumPopup("", UnderWaterShader);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent(" Save Shader Texture ", "Enable This Option to save the generated shaders as textures (For Editor in 'GIS Terrain' Folder), the texture resolution equal to terrain hightmap resolution"), GUILayout.MaxWidth(200));
                        SaveShaderTextures = (OptionEnabDisab)EditorGUILayout.EnumPopup("", SaveShaderTextures);
                        GUILayout.EndHorizontal();


                        break;

                    case TextureMode.WithoutTexture:

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("  Count Tiles ", " Number of Terrain Tiles in X-Y, This Option is avalaible Only when Texturing mode set to 'Without' or 'Splatmapping' "), GUILayout.MaxWidth(200));
                        terrainCount = EditorGUILayout.Vector2IntField("", terrainCount);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("  Use Custom Terrain Color ", "Enable/Disable customize terrain color "), GUILayout.MaxWidth(200));
                        UseTerrainEmptyColor = (OptionEnabDisab)EditorGUILayout.EnumPopup("", UseTerrainEmptyColor, GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        if (UseTerrainEmptyColor== OptionEnabDisab.Enable)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(new GUIContent("  Terrain Color ", "Used to change the main terrain color"), GUILayout.MaxWidth(200));
                            TerrainEmptyColor = EditorGUILayout.ColorField("", TerrainEmptyColor, GUILayout.ExpandWidth(true));
                            GUILayout.EndHorizontal();
                        }

                        break;
                }


            }
            EditorGUILayout.EndVertical();
        }
        private void OnTerrainSmoothignOperationGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            GUI.backgroundColor = Color.green;
            EditorGUILayout.BeginVertical(GUI.skin.button);
            ShowSmoothingOpr = EditorGUILayout.Foldout(ShowSmoothingOpr, " Terrain Smoothing  ");
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;


            if (ShowSmoothingOpr)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(" Terrain Height Smoother ", "Used to softens the landscape and reduces the appearance of abrupt changes"), GUILayout.MaxWidth(200));
                UseTerrainHeightSmoother = (OptionEnabDisab)EditorGUILayout.EnumPopup("", UseTerrainHeightSmoother, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                if (UseTerrainHeightSmoother== OptionEnabDisab.Enable)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("  Terrain Height Smooth Factor ", GUILayout.MaxWidth(200));
                    TerrainHeightSmoothFactor = EditorGUILayout.Slider(TerrainHeightSmoothFactor, 0.0f, 1f, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                }

                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(" Terrain Surface Smoother ", " this operation is useful when for terrains with unwanted jaggies, terraces,banding and non-smoothed terrain heights. Changing the surface smoother value to higher means more smoothing on surface while 1 value means minimum smoothing"), GUILayout.MaxWidth(200));
                UseTerrainSurfaceSmoother = (OptionEnabDisab)EditorGUILayout.EnumPopup("", UseTerrainSurfaceSmoother, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();


                if (UseTerrainSurfaceSmoother == OptionEnabDisab.Enable)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("  Terrain Surface Smooth Factor ", ""), GUILayout.MaxWidth(200));
                    TerrainSurfaceSmoothFactor = EditorGUILayout.IntSlider(TerrainSurfaceSmoothFactor, 1, 15, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                }


            }

            EditorGUILayout.EndVertical();
        }
        private void OnTerrainVectorGenerationGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            GUI.backgroundColor = Color.green;
            EditorGUILayout.BeginVertical(GUI.skin.button);
            ShowOSMVectorData = EditorGUILayout.Foldout(ShowOSMVectorData, " Vector Data ");
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
 
            if (ShowOSMVectorData)
            {
                GUI.backgroundColor = Color.cyan;
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(" Vector Type ", "Select your vector type (Data must added to VectorData folder)"), GUILayout.MaxWidth(200));
                vectorType = (VectorType)EditorGUILayout.EnumPopup("", vectorType, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
                GUI.backgroundColor = Color.white;


                if ((vectorType != VectorType.GPX))
                {
                    Color EnableGeoPointColor = Color.green;
                    if (EnableGeoPointGeneration == OptionEnabDisab.Enable)
                        EnableGeoPointColor = Color.green;
                    else
                        EnableGeoPointColor = Color.red;

                    GUI.backgroundColor = EnableGeoPointColor;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent(" Generate GeoPoints ", " Enable this option to generate gamebjects according to geo-points coordinates found in the vector file"), GUILayout.MaxWidth(200));
                    EnableGeoPointGeneration = (OptionEnabDisab)EditorGUILayout.EnumPopup("", EnableGeoPointGeneration);
                    GUILayout.EndHorizontal();
                    GUI.backgroundColor = Color.white;



                    Color EnableTreeColor = Color.green;
                    if (EnableTreeGeneration == OptionEnabDisab.Enable)
                        EnableTreeColor = Color.green;
                    else
                        EnableTreeColor = Color.red;

                    GUI.backgroundColor = EnableTreeColor;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent(" Generate Trees ", "Enable/Disable Loading and Generating Trees from Vector File "), GUILayout.MaxWidth(200));
                    EnableTreeGeneration = (OptionEnabDisab)EditorGUILayout.EnumPopup("", EnableTreeGeneration);
                    GUILayout.EndHorizontal();
                    GUI.backgroundColor = Color.white;

                    if (EnableTreeGeneration == OptionEnabDisab.Enable)
                    {
                        //Tree Distance
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("  Tree Distance ", " The distance from the camera beyond which trees are culled "), GUILayout.MaxWidth(200));
                        TreeDistance = EditorGUILayout.Slider(TreeDistance, 1, 5000, GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        //Tree BillBoard Start Distance
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("  Tree BillBoard Start Distance ", "The distance from the camera at which Billboard images replace 3D Tree objects"), GUILayout.MaxWidth(200));
                        BillBoardStartDistance = EditorGUILayout.Slider(BillBoardStartDistance, 1, 2000, GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        //Tree Prefabs List
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("  Trees ", GUILayout.MaxWidth(200));

                        ScriptableObject target = this;
                        SerializedObject so = new SerializedObject(target);
                        SerializedProperty stringsProperty = so.FindProperty("TreePrefabs");

                        EditorGUILayout.PropertyField(stringsProperty, true);
                        so.ApplyModifiedProperties();

                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("                ", " "), GUILayout.MaxWidth(200));

                        if (GUILayout.Button(new GUIContent(" Load All ", "Click To Load all tree prefabs Located in 'GIS Tech/GIS Terrain Loader/Resources/Prefabs/Environment/Trees'"), GUILayout.ExpandWidth(true)))
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
                        GUILayout.EndHorizontal();

                    }

                    Color EnableGrassColor = Color.green;
                    if (EnableGrassGeneration == OptionEnabDisab.Enable)
                        EnableGrassColor = Color.green;
                    else
                        EnableGrassColor = Color.red;

                    GUI.backgroundColor = EnableGrassColor;
 
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent(" Generat Grass ", "Enable/Disable Loading and Generating Grass from Vector File "), GUILayout.MaxWidth(200));
                    EnableGrassGeneration = (OptionEnabDisab)EditorGUILayout.EnumPopup("", EnableGrassGeneration);
                    GUILayout.EndHorizontal();
                    GUI.backgroundColor = Color.white;

                    if (EnableGrassGeneration == OptionEnabDisab.Enable)
                    {
                        //Grass Scale Factor
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("  Grass Scale Factor ", GUILayout.MaxWidth(200));
                        GrassScaleFactor = EditorGUILayout.Slider(GrassScaleFactor, 0.1f, 100, GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        //Detail Distance
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("  Detail Distance ", "The distance from the camera beyond which details are culled"), GUILayout.MaxWidth(200));
                        DetailDistance = EditorGUILayout.Slider(DetailDistance, 10f, 400, GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        //Tree Prefabs List
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("  Grass ", GUILayout.MaxWidth(200));

                        ScriptableObject target = this;
                        SerializedObject so = new SerializedObject(target);
                        SerializedProperty stringsProperty = so.FindProperty("GrassPrefabs");

                        EditorGUILayout.PropertyField(stringsProperty, true);
                        so.ApplyModifiedProperties();

                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("                ", " "), GUILayout.MaxWidth(200));

                        if (GUILayout.Button(new GUIContent(" Load All ", "Click To Load all Grass prefabs Located in 'GIS Tech/GIS Terrain Loader/Resources/Prefabs/Environment/Grass'"), GUILayout.ExpandWidth(true)))
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
                        GUILayout.EndHorizontal();

                    }

                    Color EnableRoadColor = Color.green;
                    if (EnableRoadGeneration == OptionEnabDisab.Enable)
                        EnableRoadColor = Color.green;
                    else
                        EnableRoadColor = Color.red;

                    GUI.backgroundColor = EnableRoadColor;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent(" Generat Roads ", "Enable/Disable Loading and Generating Roads from OSM File "), GUILayout.MaxWidth(200));
                    EnableRoadGeneration = (OptionEnabDisab)EditorGUILayout.EnumPopup("", EnableRoadGeneration);
                    GUILayout.EndHorizontal();
                    GUI.backgroundColor = Color.white;

                    if (EnableRoadGeneration == OptionEnabDisab.Enable)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("  Road Generator Type ", "Select whiche type of road will be used (Note that EasyRoad3D must be existing in the project "), GUILayout.MaxWidth(200));
                        RoadType = (RoadGenerationType)EditorGUILayout.EnumPopup("", RoadType, GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        if (vectorType == VectorType.OpenStreetMap)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(new GUIContent("  Roads Lable ", "Add Roads name  "), GUILayout.MaxWidth(200));
                            EnableRoadName = EditorGUILayout.Toggle("", EnableRoadName, GUILayout.ExpandWidth(true));
                            GUILayout.EndHorizontal();
                        }


                    }

                    Color EnableBuildingColor = Color.green;
                    if (EnableBuildingGeneration == OptionEnabDisab.Enable)
                        EnableBuildingColor = Color.green;
                    else
                        EnableBuildingColor = Color.red;

                    GUI.backgroundColor = EnableBuildingColor;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent(" Generat Buildings ", "Enable/Disable Loading and Generating building from Vector File "), GUILayout.MaxWidth(200));
                    EnableBuildingGeneration = (OptionEnabDisab)EditorGUILayout.EnumPopup("", EnableBuildingGeneration);
                    GUILayout.EndHorizontal();
                    GUI.backgroundColor = Color.white;

                    if (EnableBuildingGeneration == OptionEnabDisab.Enable)
                    {

                    }
                }else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent(" Generat Tracks ", "Enable/Disable Loading and Generating Traks from GPX File "), GUILayout.MaxWidth(200));
                    EnableRoadGeneration = (OptionEnabDisab)EditorGUILayout.EnumPopup("", EnableRoadGeneration);
                    GUILayout.EndHorizontal();
 
                    if (EnableRoadGeneration == OptionEnabDisab.Enable)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("  Generator Type ", "Select with whiche the Track will be Generated (Note that EasyRoad3D must be existing in the project "), GUILayout.MaxWidth(200));
                        RoadType = (RoadGenerationType)EditorGUILayout.EnumPopup("", RoadType, GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("  Path Prefab ", ""), GUILayout.MaxWidth(200));
                        PathPrefab = (GISTerrainLoaderSO_Road)EditorGUILayout.ObjectField(PathPrefab, typeof(GISTerrainLoaderSO_Road), true, GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();

                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent(" Generat GeoLocation ", "Enable/Disable Loading and Generating GeoLocation Point from GPX File "), GUILayout.MaxWidth(200));
                    EnableGeoLocationPointGeneration = (OptionEnabDisab)EditorGUILayout.EnumPopup("", EnableGeoLocationPointGeneration);
                    GUILayout.EndHorizontal();

                    if (EnableGeoLocationPointGeneration == OptionEnabDisab.Enable)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("  GeoPoint Prefab ", ""), GUILayout.MaxWidth(200));
                        GeoPointPrefab = (GameObject)EditorGUILayout.ObjectField(GeoPointPrefab, typeof(UnityEngine.Object), true, GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();
                    }
                }



            }

            EditorGUILayout.EndVertical();
        }
        private void GeneratingBtn()
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(" Remove Previous Terrain ", "Enable this Option to Remove previous generated terrain existing in your scene"), GUILayout.MaxWidth(200));
            RemovePrvTerrain = (OptionEnabDisab)EditorGUILayout.EnumPopup("", RemovePrvTerrain);
            GUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(GUI.skin.box);


            if (State == GeneratorState.Generating)
            {
                GUI.backgroundColor = Color.blue;

                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(""), GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.Height(3));
                GUILayout.EndHorizontal();

                if (GUILayout.Button(new GUIContent(" Cancel ", "Click To Cancel the operation")))
                {
                    OnError();

                }
                GUI.backgroundColor = Color.white;
            }
            if (State == GeneratorState.idle)
            {
                if (GUILayout.Button(new GUIContent(" Generate Terrain ", "Click To Start Generating Terrains")))
                {
                    Repaint();
                    GTLGenerate();
                }
            }
    


            Rect rec = EditorGUILayout.BeginVertical();


            if (State == GeneratorState.Generating)
            {
                GUILayout.Label("Progress :");
                EditorGUI.ProgressBar(rec, s_progress / 100, s_phase + " " + Mathf.FloorToInt(s_progress) + "%");
            }
            else
            {
                EditorUtility.ClearProgressBar();
                GUILayout.Space(38);
            }
 
            EditorGUILayout.EndVertical();



           EditorGUILayout.EndScrollView();


        }
#endregion
#region Phases
        public async Task  Phases()
        {
            if (State == GeneratorState.Generating)
            {
                await LoadElevationFile(TerrainFilePath);

                if(ElevationInfo !=null)
                {
                    await GenerateContainer();

                    for (int i = 0; i < GeneratedContainer.terrains.Length; i++)
                        await GenerateTerrains(i);

                    for (int i = 0; i < GeneratedContainer.terrains.Length; i++)
                        await GenerateHeightmap(i);

                    RepareTerrains();

                    for (int i = 0; i < GeneratedContainer.terrains.Length; i++)
                        await GenerateTextures(i);

                    GenerateVectorData();

                    Finish();
                }else
                {
                    Finish();
                }

            }
            try
            {


            }
            catch (Exception ex)
            {
                Debug.Log("Couldn't Load Terrain file: " + ex.Message + "  " + Environment.NewLine);

                OnError();
            };
 
        }
        private async void CheckForFile()
        {
            if (File.Exists(TerrainFilePath))
            {
                if (textureMode == TextureMode.WithTexture)
                {
                    if (textureloadingMode == TexturesLoadingMode.AutoDetection)
                    {
                        var c_count = new Vector2(0, 0);
                        GISTerrainLoaderTextureGenerator.GetTilesNumberInTextureFolder(TerrainFilePath, out c_count);
                        terrainCount = new Vector2Int((int)c_count.x, (int)c_count.y);

                        if (c_count == Vector2.zero)
                        {
                            terrainCount = new Vector2Int(1, 1);
                        }

                    }
                }

                if(terrainMaterialMode == TerrainMaterialMode.Standard || terrainMaterial==null)
                {
                    terrainMaterial = (Material)Resources.Load("Materials/Default-Terrain-Standard", typeof(Material));

                    if (terrainMaterial == null)
                        Debug.LogError("Custom terrain material null or standard terrain material not found in 'Resources/Materials/Default-Terrain-Standard' ");
                }

                State = GeneratorState.Generating;

                await Task.Delay(1);

                await Phases();

                
            }
            else
            {
                Debug.LogError("Can't Load this File");
                OnError();
            }

        }
        public async Task LoadElevationFile(string filepath)
        {
            if (File.Exists(filepath))
            {
                LoadedFileExtension = Path.GetExtension(filepath);
 
                CurrentTerrainIndex = 0;

                ElevationInfo = null;
                switch (LoadedFileExtension)
                {
                    case ".flt":
                        {
                            ElevationInfo = new GISTerrainLoaderElevationInfo();

                            var floatReader = new GISTerrainLoaderFloatReader();
                            floatReader.data.TerrainMaxMinElevation = TerrainMaxMinElevation;

                            if (readingMode == ReadingMode.Full)
                                floatReader.LoadFloatGrid(filepath, terrainDimensionMode, TerrainFixOption);
                            else floatReader.LoadFloatGrid(SubRegionUpperLeftCoordiante, SubRegionDownRightCoordiante, filepath, terrainDimensionMode, TerrainFixOption);

                            ElevationInfo.GetData(floatReader.data);

                           await CheckForDimensionAndTiles(true);

                            if (floatReader.LoadComplet)
                            {
                                floatReader.LoadComplet = false;
                            }
                        }
                        break;

                    case ".bin":
                        {

                            ElevationInfo = new GISTerrainLoaderElevationInfo();


                            var binReader = new GISTerrainLoaderBinLoader();
                            binReader.data.TerrainMaxMinElevation = TerrainMaxMinElevation;

                            if (readingMode == ReadingMode.Full)
                                binReader.LoadFloatGrid(filepath,terrainDimensionMode,  TerrainFixOption);
                            else binReader.LoadFloatGrid(SubRegionUpperLeftCoordiante, SubRegionDownRightCoordiante, filepath, terrainDimensionMode, TerrainFixOption);

                            ElevationInfo.GetData(binReader.data);

                           await CheckForDimensionAndTiles(true);

                            if (binReader.LoadComplet)
                            {
                                binReader.LoadComplet = false;
                            }
                        }
                        break;
                    case ".bil":
                        {
                            ElevationInfo = new GISTerrainLoaderElevationInfo();


                            var BILReader = new GISTerrainLoaderBILReader();
                            BILReader.data.TerrainMaxMinElevation = TerrainMaxMinElevation;

                            if (readingMode == ReadingMode.Full)
                                BILReader.LoadFloatGrid(filepath, terrainDimensionMode, TerrainFixOption);
                            else BILReader.LoadFloatGrid(SubRegionUpperLeftCoordiante, SubRegionDownRightCoordiante, filepath, terrainDimensionMode, TerrainFixOption);

                            ElevationInfo.GetData(BILReader.data);

                            await CheckForDimensionAndTiles(true);

                            if (BILReader.LoadComplet)
                            {
                                BILReader.LoadComplet = false;
                            }
                        }
                        break;
                    case ".asc":
                        {
                            ElevationInfo = new GISTerrainLoaderElevationInfo();

                            var ASCIReader = new GISTerrainLoaderASCILoader();
                            ASCIReader.data.TerrainMaxMinElevation = TerrainMaxMinElevation;

                            if (readingMode == ReadingMode.Full)
                                ASCIReader.LoadASCIGrid(filepath, terrainDimensionMode, TerrainFixOption);
                            else ASCIReader.LoadASCIGrid(SubRegionUpperLeftCoordiante, SubRegionDownRightCoordiante, filepath, terrainDimensionMode, TerrainFixOption);

                            ElevationInfo.GetData(ASCIReader.data);

                            await CheckForDimensionAndTiles(true);

                            if (ASCIReader.LoadComplet)
                            {
                                ASCIReader.LoadComplet = false;

                            }
                        }
                        break;
                    case ".hgt":
                        {
                            ElevationInfo = new GISTerrainLoaderElevationInfo();

                            var hgtReader = new GISTerrainLoaderHGTLoader();
                            hgtReader.data.TerrainMaxMinElevation = TerrainMaxMinElevation;

                            if (readingMode == ReadingMode.Full)
                                hgtReader.LoadFloatGrid(filepath, TerrainFixOption);
                            else hgtReader.LoadFloatGrid(SubRegionUpperLeftCoordiante, SubRegionDownRightCoordiante, filepath, TerrainFixOption);

                            ElevationInfo.GetData(hgtReader.data);

                            await CheckForDimensionAndTiles(true);

                            if (hgtReader.LoadComplet)
                            {
                                hgtReader.LoadComplet = false;
                            }
                        }
                        break;
                    case ".tif":
                        {
                            ElevationInfo = new GISTerrainLoaderElevationInfo();

                            var TiffReader = new GISTerrainLoaderTIFFLoader();
                            TiffReader.data.TerrainMaxMinElevation = TerrainMaxMinElevation;

                            if (readingMode == ReadingMode.Full)
                                TiffReader.LoadTiff(filepath,terrainDimensionMode, null, TerrainFixOption,EPSGCode, tiffElevationSource);
                            else TiffReader.LoadTiff(SubRegionUpperLeftCoordiante, SubRegionDownRightCoordiante, filepath, terrainDimensionMode, null, TerrainFixOption);

                            while (!TiffReader.LoadComplet)
                                await Task.Delay(TimeSpan.FromSeconds(0.01));

                            ElevationInfo.GetData(TiffReader.data);

                            await CheckForDimensionAndTiles(true);




                        }
                        break;
                    case ".las":
                        {
#if GISTerrainLoaderPdal
                            ElevationInfo = new GISTerrainLoaderElevationInfo();

                            var lasReader = new GISTerrainLoaderLASLoader();

                            if (!lasReader.LoadComplet)
                                lasReader.LoadLasFile(filepath);

                            while (!lasReader.LoadComplet)
                                await Task.Delay(TimeSpan.FromSeconds(0.01));

                            ElevationInfo.GetData(lasReader.data);

                            if (lasReader.LoadComplet)
                            {
                                TerrainFilePath = lasReader.GeneratedFilePath;
                                await Task.Delay(TimeSpan.FromSeconds(1));

                                if (File.Exists(TerrainFilePath))
                                {
                                    var TiffReader = new GISTerrainLoaderTIFFLoader();
                                    TiffReader.data.TerrainMaxMinElevation = TerrainMaxMinElevation;

                                    if (readingMode == ReadingMode.Full)
                                        TiffReader.LoadTiff(TerrainFilePath, terrainDimensionMode, ElevationInfo.data, TerrainFixOption);
                                    else TiffReader.LoadTiff(SubRegionUpperLeftCoordiante, SubRegionDownRightCoordiante, TerrainFilePath, terrainDimensionMode, ElevationInfo.data, TerrainFixOption);

                                    while (!TiffReader.LoadComplet)
                                        await Task.Delay(TimeSpan.FromSeconds(0.01));

                                    ElevationInfo.GetData(TiffReader.data);
                                   await CheckForDimensionAndTiles(true);

                                    lasReader.LoadComplet = false;
                                }
                                else
                                    Debug.LogError("File Not exsiting " + TerrainFilePath);
                            }

#else
                            Debug.LogError("Pdal Plugin Not Configured ..");
#endif

                        }
                        break;
                    case ".raw":
                        {

                            ElevationInfo = new GISTerrainLoaderElevationInfo();
                            var RawReader = new GISTerrainLoaderRawLoader();
                            RawReader.m_ByteOrder = Raw_ByteOrder;
                            RawReader.m_Depth = Raw_Depth;

                            RawReader.LoadRawGrid(textureMode, filepath);

                            while (!RawReader.LoadComplet)
                                await Task.Delay(TimeSpan.FromSeconds(0.01));

                            ElevationInfo.GetData(RawReader.data);

                            await CheckForDimensionAndTiles(false);

                            RawReader.LoadComplet = false;
                        }
                        break;

                    case ".png":
                        {

                            ElevationInfo = new GISTerrainLoaderElevationInfo();
                            var PngReader = new GISTerrainLoaderDEMPngLoader();

                            PngReader.LoadPngGrid(textureMode, filepath);

                            while (!PngReader.LoadComplet)
                                await Task.Delay(TimeSpan.FromSeconds(0.01));

                            ElevationInfo.GetData(PngReader.data);

                            await CheckForDimensionAndTiles(false);

                            PngReader.LoadComplet = false;
                        }
                        break;

                    case ".ter":
                        {

                            ElevationInfo = new GISTerrainLoaderElevationInfo();
                            var TerReader = new GISTerrainLoaderTerraGenLoade();

                            TerReader.LoadTer(textureMode, filepath);

                            while (!TerReader.LoadComplet)
                                await Task.Delay(TimeSpan.FromSeconds(0.01));

                            ElevationInfo.GetData(TerReader.data);

                            await CheckForDimensionAndTiles(false);

                            TerReader.LoadComplet = false;
                        }
                        break;

                }


            }
        }
        private async Task GenerateContainer()
        {
            if (ElevationInfo == null)
            {
                Debug.LogError(" DEM not loaded correctly .. !");
                OnError();
                return;
            }


            const string containerName = "Terrains";
            string cName = containerName;
            //Destroy prv created terrain
            if (RemovePrvTerrain == OptionEnabDisab.Enable)
            {
                DestroyImmediate(GameObject.Find(cName));
            }
            else
            {
                int index_name = 1;
                while (GameObject.Find(cName) != null)
                {
                    cName = containerName + " " + index_name.ToString();
                    index_name++;
                }
            }


            var container = new GameObject(cName);
            container.transform.position = new Vector3(0, 0, 0);
            CurrentTerrainIndex = 0;

            Vector2Int tCount = new Vector2Int(terrainCount.x, terrainCount.y);

            float maxElevation = ElevationInfo.data.MaxElevation;
            float minElevation = ElevationInfo.data.MinElevation;
            float ElevationRange = maxElevation - minElevation;

            if(UnderWater == OptionEnabDisab.Enable)
            {
                if (minElevation <= 0 && maxElevation <= 0)
                    ElevationRange = Math.Abs(minElevation) - Math.Abs(maxElevation);
                else
                    if(maxElevation>=0 && minElevation<0)
                    ElevationRange = maxElevation +  Math.Abs(minElevation);

            }

            var sizeX = Mathf.Floor(m_terrainDimensions.x * terrainScale.x * ScaleFactor) / terrainCount.x;
            var sizeZ = Mathf.Floor(m_terrainDimensions.y * terrainScale.z * ScaleFactor) / terrainCount.y;
            var sizeY = (ElevationRange) / ElevationScaleValue * TerrainExaggeration * 100 * terrainScale.y * 10;


            Vector3 size;

            if (LoadedFileExtension == ".ter" || LoadedFileExtension == ".png" || LoadedFileExtension == ".raw")
            {
                if (terrainElevation == TerrainElevation.RealWorldElevation)
                {
                    sizeY = ((162)) * terrainScale.y;
                    size = new Vector3(sizeX, sizeY, sizeZ);
                }
                else
                {
                    sizeY = 300 * TerrainExaggeration * terrainScale.y;
                    size = new Vector3(sizeX, sizeY, sizeZ);
                }
            }
            else
            {
                if (terrainElevation == TerrainElevation.RealWorldElevation)
                {
                    sizeY = (ElevationRange / ElevationScaleValue) * 1000 * terrainScale.y;

                    size = new Vector3(sizeX, sizeY, sizeZ);
                }
                else
                {
                    sizeY = sizeY * 10;
                    size = new Vector3(sizeX, sizeY, sizeZ);
                }
            }



            string resultFolder = "Assets/Generated GIS Terrains";
            string resultFullPath = Path.Combine(Application.dataPath, "Generated GIS Terrains");

            if (!Directory.Exists(resultFullPath)) Directory.CreateDirectory(resultFullPath);
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd HH-mm-") + DateTime.Now.Second.ToString();
            resultFolder += "/" + dateStr;
            resultFullPath = Path.Combine(resultFullPath, dateStr);

            if (!Directory.Exists(resultFullPath)) Directory.CreateDirectory(resultFullPath);

            terrains = new TerrainObject[tCount.x, tCount.y];

            container.AddComponent<TerrainContainerObject>();

            var terrainContainer = container.GetComponent<TerrainContainerObject>();
 
            terrainContainer.terrainCount = new Vector2Int(terrainCount.x, terrainCount.y);

            terrainContainer.GeneratedTerrainfolder = resultFolder;

            terrainContainer.scale = terrainScale;

            terrainContainer.SubTerrainSize = size;
            terrainContainer.ContainerSize = new Vector3(size.x * tCount.x, size.y, size.z * tCount.y);
 


            //Set Terrain Coordinates to the container TerrainContainer script (Lat/lon) + Mercator
            terrainContainer.TopLeftLatLong = ElevationInfo.data.TopLeftPoint;
            terrainContainer.DownRightLatLong = ElevationInfo.data.DownRightPoint;

            terrainContainer.TLPointMercator = GeoRefConversion.LatLongToMercat(terrainContainer.TopLeftLatLong.x, terrainContainer.TopLeftLatLong.y);
            terrainContainer.DRPointMercator = GeoRefConversion.LatLongToMercat(terrainContainer.DownRightLatLong.x, terrainContainer.DownRightLatLong.y);

            if (GISTerrainLoaderSupport.IsGeoFile(LoadedFileExtension))
                terrainContainer.Dimensions = new Vector2((float)ElevationInfo.data.Terrain_Dimension.x, (float)ElevationInfo.data.Terrain_Dimension.y);
            else
                terrainContainer.Dimensions = TerrainDimensions.ToVector2();

            terrainContainer.MinMaxElevation = new Vector2((float)ElevationInfo.data.MinElevation, (float)ElevationInfo.data.MaxElevation);

            //Terrain Size Bounds 
            var centre = new Vector3(terrainContainer.ContainerSize.x / 2, 0, terrainContainer.ContainerSize.z / 2);
            terrainContainer.GlobalTerrainBounds = new Bounds(centre, new Vector3(centre.x + terrainContainer.ContainerSize.x / 2, 0, centre.z + terrainContainer.ContainerSize.z / 2));

            terrainContainer.terrains = terrains;

            GeneratedContainer = terrainContainer;

            terrainContainer.data = ElevationInfo.data;

            terrainContainer.data.Store();

            await Task.Delay(TimeSpan.FromSeconds(0.0005));

        }
        public async Task GenerateTerrains(int index)
        {
            if (index >= terrains.Length)
            {
                s_progress = 0;
                return;
            }

            int x = index % terrainCount.x;
            int y = index / terrainCount.x;

            OnProgress("Generating Terrains ", (index+1) * 100 / (terrains.Length));

            var terrain = await CreateTerrain(GeneratedContainer, x, y, GeneratedContainer.SubTerrainSize, terrainScale);
            terrains[x, y] = terrain;
            terrain.container = GeneratedContainer;
 
        }
        private async Task <TerrainObject> CreateTerrain(TerrainContainerObject parent, int x, int y, Vector3 size, Vector3 scale)
        {
            TerrainData tdata = new TerrainData
            {
                baseMapResolution = 32,
                heightmapResolution = 32
            };

            tdata.heightmapResolution = heightmapResolution;
            tdata.baseMapResolution = baseMapResolution;
            tdata.SetDetailResolution(detailResolution, resolutionPerPatch);
            tdata.size = size;
            

            GameObject GO = Terrain.CreateTerrainGameObject(tdata);
            GO.gameObject.SetActive(true);
            GO.name = string.Format("Tile__{0}__{1}", x, y);
            GO.transform.parent = parent.gameObject.transform;
            GO.transform.position = new Vector3(size.x * x, 0, size.z * y);
            GO.isStatic = false;

            if (TerrainLayerSet == OptionEnabDisab.Enable)
                GO.gameObject.layer = TerrainLayer;

            TerrainObject item = GO.AddComponent<TerrainObject>();
            item.Number = new Vector2Int(x, y);
            item.size = size;
            item.ElevationFilePath = TerrainFilePath;
 
            item.terrain = GO.GetComponent<Terrain>();
            item.terrainData = item.terrain.terrainData;

            item.terrain.heightmapPixelError = PixelErro;
            item.terrain.basemapDistance = BaseMapDistance;
            item.terrain.materialTemplate = terrainMaterial;

            string filename = Path.Combine(parent.GeneratedTerrainfolder, GO.name) + ".asset";
 
             AssetDatabase.CreateAsset(tdata, filename);

             AssetDatabase.SaveAssets();

            await Task.Delay(TimeSpan.FromSeconds(0.01));

            return item;
        }
        private async Task GenerateHeightmap(int index)
        {
            if (index >= terrains.Length)
            {
                s_progress = 0;
                return;
            }

            int x = index % terrainCount.x;
            int y = index / terrainCount.x;
 
            OnProgress("Generating Heightmap ", (index+1) * 100 / (terrains.Length));

             Prefs prefs = new Prefs(detailResolution, resolutionPerPatch, baseMapResolution, heightmapResolution, terrainCount, GeneratedContainer.ContainerSize,UnderWater,terrainScale);

           await ElevationInfo.GenerateHeightMap(prefs, terrains[x, y]);

        }
        public void RepareTerrains()
        {
            List<TerrainObject> List_terrainsObj = new List<TerrainObject>();

            foreach (var item in terrains)
            {
                if (item != null)
                {
                    List_terrainsObj.Add(item);
                }

            }

            if (UseTerrainHeightSmoother == OptionEnabDisab.Enable)
                GISTerrainLoaderTerrainSmoother.SmoothTerrainHeights(List_terrainsObj, 1 - TerrainHeightSmoothFactor);

            if (UseTerrainSurfaceSmoother == OptionEnabDisab.Enable)
                GISTerrainLoaderTerrainSmoother.SmoothTerrainSurface(List_terrainsObj, TerrainSurfaceSmoothFactor);

            if (UseTerrainHeightSmoother == OptionEnabDisab.Enable || UseTerrainSurfaceSmoother == OptionEnabDisab.Enable)
            {
                GISTerrainLoaderBlendTerrainEdge.StitchTerrain(List_terrainsObj, 50f, 20);

            }
        }
        private async Task GenerateTextures(int index)
        {
            switch (textureMode)
            {
                case TextureMode.WithTexture:

                    if (textureloadingMode == TexturesLoadingMode.Manual)
                    {

                        var FolderTiles_count = new Vector2(0, 0);

                        GISTerrainLoaderTextureGenerator.GetTilesNumberInTextureFolder(TerrainFilePath, out FolderTiles_count);

                        if (terrainCount != FolderTiles_count)
                        {
                            if (FolderTiles_count == Vector2.one)
                            {
                                await GISTerrainLoaderTextureGenerator.SplitTex(TerrainFilePath, terrainCount);
                                AssetDatabase.Refresh();
                            }
                            else
                            {
                                if (FolderTiles_count.x > 1 || FolderTiles_count.y > 1)
                                {
                                    GISTerrainLoaderTextureGenerator.CombienTerrainTextures(TerrainFilePath);
                                    AssetDatabase.Refresh();

                                    GISTerrainLoaderTextureGenerator.SplitTex(TerrainFilePath, terrainCount).Wait();
                                    AssetDatabase.Refresh();

                                    textureloadingMode = TexturesLoadingMode.AutoDetection;
                                }

                            }

                        }
                        else
                            textureloadingMode = TexturesLoadingMode.AutoDetection;

                    }

                    if (textureloadingMode == TexturesLoadingMode.AutoDetection)
                    {
                        if (index < terrains.Length)
                        {
                            int x = index % terrainCount.x;
                            int y = index / terrainCount.x;

                            OnProgress("Generating Textures ", (index+1) * 100 / (terrainCount.x * terrainCount.y));

                            DirectoryInfo di = new DirectoryInfo(TerrainFilePath);

                            var TextureFolderPath = TerrainFile.name + "_Textures";

                            for (int i = 0; i <= 5; i++)
                            {
                                di = di.Parent;
                                TextureFolderPath = di.Name + "/" + TextureFolderPath;

                                if (di.Name == "GIS Terrains") break;

                                if (i == 5)
                                {
                                    Debug.LogError("Texture folder not found! : Please put your terrain in GIS Terrain Loader/Recources/GIS Terrains/");

                                    return;

                                }

                            }

                          await  GISTerrainLoaderTextureGenerator.EditorAddTextureToTerrain(TerrainFilePath, TextureFolderPath, terrains[x, y]);

                            CurrentTerrainIndex++;
                        }
                        else
                        {
                            s_progress = 0;

                            return;
                        }
                    }
                    break;

                case TextureMode.Splatmapping:

                    if (index < terrains.Length)
                    {
                        int x = index % terrainCount.x;
                        int y = index / terrainCount.x;

                        OnProgress("Generating Splatmaps ", index * 100 / (terrainCount.x * terrainCount.y));

                        GISTerrainLoaderSplatMapping.SetTerrainSpaltMap(BaseTerrainLayers, TerrainLayers, Slope, terrains[x, y], MergeRaduis, MergingFactor);

                        CurrentTerrainIndex++;
                    }
                    else
                    {
                        s_progress = 0;

                        return;
                    }
                    break;

                case TextureMode.ShadedRelief:

                    if (index < terrains.Length)
                    {
                        int x = index % terrainCount.x;
                        int y = index / terrainCount.x;

                        OnProgress("Generating Terrain Shader ", index * 100 / (terrainCount.x * terrainCount.y));

                        var terrainItem = terrains[x, y];

                        await GISTerrainLoaderTerrainShader.GenerateShadedTextureEditor(TerrainShaderType, UnderWaterShader, terrainItem, new Vector2Int(heightmapResolution - 1, heightmapResolution - 1), true, SaveShaderTextures, TerrainFilePath);

                        CurrentTerrainIndex++;
                    }
                    else
                    {
                        s_progress = 0;

                        return;
                    }

                    break;

                case TextureMode.WithoutTexture:

                    if (UseTerrainEmptyColor == OptionEnabDisab.Enable)
                    {
                        Material mat = new Material(Shader.Find("Standard"));
                        mat.SetColor("_Color", TerrainEmptyColor);

                        if (index < terrains.Length)
                        {

                            int x = index % terrainCount.x;
                            int y = index / terrainCount.x;

                            OnProgress("Generating Terrain Color ", index * 100 / (terrainCount.x * terrainCount.y));

#if UNITY_2018
                            terrains[x, y].terrain.materialType = Terrain.MaterialType.Custom;
#endif


                            terrains[x, y].terrain.materialTemplate = mat;


                            CurrentTerrainIndex++;
                        }
                        else
                        {
                            s_progress = 0;

                            return;
                        }
                    }
                    else
                    {
                        return;
                    }



                    break;
            }

        }
        private async void GenerateVectorData()
        {
            if(IsVectorGenerationEnabled(LoadedFileExtension))
            {
                if (EnableGeoPointGeneration == OptionEnabDisab.Enable)
                    GeoPointsPrefab = GISTerrainLoaderGeoPointGenerator.GetPointsPrefab();

                if (EnableRoadGeneration == OptionEnabDisab.Enable)
                    RoadsPrefab = GISTerrainLoaderRoadsGenerator.GetRoadsPrefab(RoadType);

                if (EnableTreeGeneration == OptionEnabDisab.Enable)
                    GISTerrainLoaderTreeGenerator.AddTreePrefabsToTerrains(GeneratedContainer, TreePrefabs, TreeDistance, BillBoardStartDistance);

                if (EnableGrassGeneration == OptionEnabDisab.Enable)
                    GISTerrainLoaderGrassGenerator.AddDetailsLayersToTerrains(GeneratedContainer, GrassPrefabs, DetailDistance, GrassScaleFactor);

                if (EnableBuildingGeneration == OptionEnabDisab.Enable)
                    BuildingsPrefab = GISTerrainLoaderBuildingGenerator.GetBuildingPrefabs();

                GISTerrainLoaderGeoVectorData GeoData = new GISTerrainLoaderGeoVectorData();

                switch (vectorType)
                {
                    case VectorType.OpenStreetMap:

                        var OSMFiles = GISTerrainLoaderExtensions.GetOSMFiles(TerrainFilePath);

                        if (OSMFiles != null && OSMFiles.Length > 0)
                        {
                             foreach (var osm in OSMFiles)
                            {
                                GISTerrainLoaderOSMFileLoader osmloader = new GISTerrainLoaderOSMFileLoader(osm, GeneratedContainer);
                     
                                if (EnableGeoPointGeneration == OptionEnabDisab.Enable)
                                {
                                    osmloader.GetGeoVectorPointsData(GeoData);
                                    GISTerrainLoaderGeoPointGenerator.GenerateGeoPoint(GeneratedContainer, GeoData, GeoPointsPrefab);
                                }

                                if (EnableRoadGeneration == OptionEnabDisab.Enable)
                                {
                                    osmloader.GetGeoVectorRoadsData(GeoData);
                                    GISTerrainLoaderRoadsGenerator.GenerateTerrainRoades(GeneratedContainer, GeoData, RoadType, EnableRoadName, RoadsPrefab);
                                }


                                if (EnableTreeGeneration == OptionEnabDisab.Enable)
                                {
                                    if (TreePrefabs.Count > 0)
                                    {
                                        osmloader.GetGeoVectorTreesData(GeoData);
                                        GISTerrainLoaderTreeGenerator.GenerateTrees(GeneratedContainer, GeoData);
                                    }
                                    else
                                        Debug.LogError("Unable to generate Trees : Prefab List is empty ");
                                }

                                if (EnableGrassGeneration == OptionEnabDisab.Enable)
                                {
                                    if (GrassPrefabs.Count > 0)
                                    {
                                        osmloader.GetGeoVectorGrassData(GeoData);
                                        GISTerrainLoaderGrassGenerator.GenerateGrass(GeneratedContainer, GeoData);
                                    }
                                    else
                                        Debug.LogError("Unable to generate Grass : Prefab List is empty ");

                                }
                                if (EnableBuildingGeneration == OptionEnabDisab.Enable)
                                {
                                    osmloader.GetGeoVectorBuildingData(GeoData);
                                    GISTerrainLoaderBuildingGenerator.GenerateBuildings(GeneratedContainer, GeoData, BuildingsPrefab);
                                }
 
                            }
                         }
                        else
                        {
                            Debug.LogError("VectorData Folder is Empty ! : Please set your osm files into 'GIS Terrains'\'TerrainName'\'TerrainName_VectorData'");

                        }
                        break;

                    case VectorType.ShapeFile:

                        var shapes = GISTerrainLoaderShapeReader.LoadShapes(TerrainFilePath);

                        if (shapes != null && shapes.Count > 0)
                        {

                            foreach (var shape in shapes)
                            {

                                GISTerrainLoaderShapeFileLoader shapeloader = new GISTerrainLoaderShapeFileLoader(shape);

                                if (EnableRoadGeneration == OptionEnabDisab.Enable)
                                {
                                    shapeloader.GetGeoVectorRoadsData(GeoData);
                                    GISTerrainLoaderRoadsGenerator.GenerateTerrainRoades(GeneratedContainer, GeoData, RoadType, EnableRoadName, RoadsPrefab);
                                }

                                if (EnableTreeGeneration == OptionEnabDisab.Enable)
                                {
                                    if (TreePrefabs.Count > 0)
                                    {
                                        shapeloader.GetGeoVectorTreesData(GeoData);
                                        GISTerrainLoaderTreeGenerator.GenerateTrees(GeneratedContainer, GeoData);
                                    }
                                    else
                                        Debug.LogError("Error : Tree Prefabs List is empty ");
                                }


                                if (EnableGrassGeneration == OptionEnabDisab.Enable)
                                {
                                    if (GrassPrefabs.Count > 0)
                                    {
                                        shapeloader.GetGeoVectorGrassData(GeoData);
                                        GISTerrainLoaderGrassGenerator.GenerateGrass(GeneratedContainer, GeoData);
                                    }
                                    else
                                        Debug.LogError("Error : Grass Prefabs List is empty ");

                                }

                                if (EnableBuildingGeneration == OptionEnabDisab.Enable)
                                {
                                    shapeloader.GetGeoVectorBuildingData(GeoData);
                                    GISTerrainLoaderBuildingGenerator.GenerateBuildings(GeneratedContainer, GeoData, BuildingsPrefab);
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("No Shape file exist");
                        }
                        break;

                    case VectorType.GPX :

                        var GPXsFiles = GISTerrainLoaderGPXLoader.GetGPXs(TerrainFilePath);

                        if (GPXsFiles != null && GPXsFiles.Length > 0)
                        {
                            foreach (var gpx in GPXsFiles)
                            {
                                GISTerrainLoaderGPXFileData LoadGPXFile = GISTerrainLoaderGPXLoader.LoadGPXFile(gpx, GeneratedContainer);

                                if (EnableRoadGeneration == OptionEnabDisab.Enable)
                                    GISTerrainLoaderRoadsGenerator.GenerateTerrainRoades(LoadGPXFile,GeneratedContainer,RoadType, EnableRoadName, RoadsPrefab,PathPrefab);

                               if (EnableGeoLocationPointGeneration == OptionEnabDisab.Enable)
                                    GISTerrainLoaderGeoPointGenerator.GenerateGeoPoint(LoadGPXFile, GeneratedContainer, GeoPointPrefab);
                            }
                        }
                        else
                        {
                            Debug.LogError("VectorData Folder is Empty ! : Please set your osm files into 'GIS Terrains'\'TerrainName'\'TerrainName_VectorData'");
                        }
                       break;
                    //case VectorType.KML:

                    //    var KMLFiles = GISTerrainLoaderKMLLoader.GetKMLs(TerrainFilePath);

                    //    if (KMLFiles != null && KMLFiles.Length > 0)
                    //    {
                    //        foreach (var kml in KMLFiles)
                    //        {
                    //            GISTerrainLoaderKMLReader KMLFile = new GISTerrainLoaderKMLReader(kml);
                    //            //Customize KML projection
                    //            KMLFile.fileProj = FileProjection.Geographic_lat_lon;
                    //            KMLFile.epsg = EPSGCode;

                    //            KMLFile.LoadFile();

                    //        }
                    //    }
                    //    else
                    //    {
                    //        Debug.LogError("VectorData Folder is Empty ! : Please set your osm files into 'GIS Terrains'\'TerrainName'\'TerrainName_VectorData'");
                    //    }
                    //    break;
                }
            }
            else

            await Task.Delay(TimeSpan.FromSeconds(0.001));
        }
        private void Finish()
        {
            foreach (TerrainObject item in terrains)
                item.terrain.Flush();

            s_phase = "";
            s_progress = 0;



            State = GeneratorState.idle;
      
        }
#endregion
#region Events
        void OnError()
        {
            s_phase = "";
            s_progress = 0;

            Repaint();
            State = GeneratorState.idle;
            Repaint();

        }
        void OnProgress(string phasename, float value)
        {
            Repaint();
            s_phase = phasename;
            s_progress = value;
            Repaint();

        }
        private void OnDisable()
        {
            GISTerrainLoaderFloatReader.OnReadError -= OnError;
            GISTerrainLoaderTIFFLoader.OnReadError -= OnError;
            GISTerrainLoaderTerraGenLoade.OnReadError -= OnError;
            GISTerrainLoaderDEMPngLoader.OnReadError -= OnError;
            GISTerrainLoaderRawLoader.OnReadError -= OnError;
            GISTerrainLoaderASCILoader.OnReadError -= OnError;

#if GISTerrainLoaderPdal
            GISTerrainLoaderLASLoader.OnReadError -= OnError;
#endif

            GISTerrainLoaderFloatReader.OnProgress -= OnProgress;
            GISTerrainLoaderTIFFLoader.OnProgress -= OnProgress;
            GISTerrainLoaderTerraGenLoade.OnProgress -= OnProgress;
            GISTerrainLoaderDEMPngLoader.OnProgress -= OnProgress;
            GISTerrainLoaderRawLoader.OnProgress -= OnProgress;
            GISTerrainLoaderASCILoader.OnProgress -= OnProgress;

            SavePrefs();
        }
        private void OnDestroy()
        {
            SavePrefs();
        }
        private void OnEnable()
        {
            OnTerrainFileChanged(TerrainFile);

            window = this;

            GISTerrainLoaderFloatReader.OnReadError += OnError;
            GISTerrainLoaderTIFFLoader.OnReadError += OnError;
            GISTerrainLoaderTerraGenLoade.OnReadError += OnError;
            GISTerrainLoaderDEMPngLoader.OnReadError += OnError;
            GISTerrainLoaderRawLoader.OnReadError += OnError;
            GISTerrainLoaderASCILoader.OnReadError += OnError;
            GISTerrainLoaderHGTLoader.OnReadError += OnError;
            GISTerrainLoaderBILReader.OnReadError += OnError;
#if GISTerrainLoaderPdal
            GISTerrainLoaderLASLoader.OnReadError += OnError;
#endif



            GISTerrainLoaderFloatReader.OnProgress += OnProgress;
            GISTerrainLoaderTIFFLoader.OnProgress += OnProgress;
            GISTerrainLoaderTerraGenLoade.OnProgress += OnProgress;
            GISTerrainLoaderDEMPngLoader.OnProgress += OnProgress;
            GISTerrainLoaderRawLoader.OnProgress += OnProgress;
            GISTerrainLoaderASCILoader.OnProgress += OnProgress;
            GISTerrainLoaderHGTLoader.OnProgress += OnProgress;
            GISTerrainLoaderBILReader.OnProgress += OnProgress;


            LoadPrefs();

            if (m_terrain == null)
                m_terrain = LoadTexture("GTL_Terrain");

            if (m_downloaExamples == null)
                m_downloaExamples = LoadTexture("GTL_DownloaExamples");

            if (m_helpIcon == null)
                m_helpIcon = LoadTexture("GTL_HelpIcon");

            if (m_resetPrefs == null)
                m_resetPrefs = LoadTexture("GTL_ResetPrefs");

            if (m_aboutIcon == null)
                m_aboutIcon = LoadTexture("GTL_AboutPrefs");

        }
        private void OnTerrainFileChanged(UnityEngine.Object terrain)
        {

            var TerrainFilePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), AssetDatabase.GetAssetPath(TerrainFile));

            if (File.Exists(TerrainFilePath))
            {
                var fileExtension = Path.GetExtension(TerrainFilePath);

                if (GISTerrainLoaderSupport.GeoFile.Contains(fileExtension))
                {
                    TerrainHasDimensions = true;

                    if (fileExtension == ".tif")
                    {
#if DotSpatial
        ShowProjectionMode = true;
#endif

                        ShowTiffElevationSourceMode = true;
                    }
                    else
                    {
#if DotSpatial
        ShowProjectionMode =false ;
#endif
 
                        ShowTiffElevationSourceMode = false;
                    }


                }
                else
                {
                    TerrainHasDimensions = false;
                }




                if (!TerrainHasDimensions)
                {
                    if (fileExtension == ".raw")
                        ShowRawParameters = true;

                    ShowSubRegion = false;
                    terrainDimensionMode = TerrainDimensionsMode.Manual;
                }
                else
                {
                    ShowRawParameters = false;
                    ShowSubRegion = true;
                    terrainDimensionMode = TerrainDimensionsMode.AutoDetection;
                }

            }
            else
            {
                ShowRawParameters = false;
                terrainDimensionMode = TerrainDimensionsMode.AutoDetection;
                ShowSubRegion = false;
                TerrainHasDimensions = true;
            }
        }
#endregion
#region Other
        private async Task CheckForDimensionAndTiles(bool AutoDim)
        {
            if (terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
            {
                if (AutoDim)
                {
                    if (ElevationInfo.data.Terrain_Dimension.x == 0 || ElevationInfo.data.Terrain_Dimension.y == 0)
                    {
                        Debug.LogError("Can't detecte terrain dimension (Check your file projection) and please againe ");
                        OnError();
                        return;
                    }
                    else
                    if (ElevationInfo.data.Terrain_Dimension != new DVector2(0, 0))
                    {
                        m_terrainDimensions = new Vector2((float)ElevationInfo.data.Terrain_Dimension.x, (float)ElevationInfo.data.Terrain_Dimension.y);
                    }

                    if (ElevationInfo.data.Tiles != Vector2.zero)
                    {
                        terrainCount = new Vector2Int((int)ElevationInfo.data.Tiles.x, (int)ElevationInfo.data.Tiles.y);
                    }
                    else
                    {
                        //OnError();
                    }
                }
                else
                {
                    if (TerrainDimensions.x == 0 || TerrainDimensions.y == 0)
                    {
                        Debug.LogError("Reset Terrain dimensions ... try again  ");
                        OnError();
                        return;
                    }
                    else
        if (TerrainDimensions != new DVector2(0, 0))
                    {
                        m_terrainDimensions = new Vector2((float)TerrainDimensions.x, (float)TerrainDimensions.y);
                    }

                    if (ElevationInfo.data.Tiles != Vector2.zero)
                    {
                        terrainCount = new Vector2Int((int)ElevationInfo.data.Tiles.x, (int)ElevationInfo.data.Tiles.y);
                    }
                    else
                    {
                        if (textureMode == TextureMode.WithTexture)
                            Debug.LogError("Can't detecte terrain textures folder ... try again");

                        OnError();
                    }
                }
            }
            else
            {
                if (TerrainDimensions.x == 0 || TerrainDimensions.y == 0)
                {
                    Debug.LogError("Reset Terrain dimensions ... try again  ");
                    OnError();
                    return;
                }
                else
    if (TerrainDimensions != new DVector2(0, 0))
                {
                    m_terrainDimensions = new Vector2((float)TerrainDimensions.x, (float)TerrainDimensions.y);
                }
                if (ElevationInfo.data.Tiles != Vector2.zero)
                {

                    terrainCount = new Vector2Int((int)ElevationInfo.data.Tiles.x, (int)ElevationInfo.data.Tiles.y);
                }
                else
                {
                    OnError();
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(0.01));
        }

        private Texture2D LoadTexture(string m_iconeName)
        {
            var tex = new Texture2D(35, 35);

            string[] guids = AssetDatabase.FindAssets(m_iconeName + " t:texture");
            if (guids != null && guids.Length > 0)
            {
                string iconPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                tex = (Texture2D)AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture2D));
            }

            return tex;
        }

        private UnityEngine.Object[] LoadAll(string path)
        {
            return Resources.LoadAll(path);
        }
        public bool IsVectorGenerationEnabled(string fileExtension)
        {
            var isGeoFile = GISTerrainLoaderSupport.GeoFile.Contains(fileExtension);

            var val = false;

            if (isGeoFile && (EnableTreeGeneration == OptionEnabDisab.Enable || EnableGrassGeneration == OptionEnabDisab.Enable || EnableRoadGeneration == OptionEnabDisab.Enable || EnableBuildingGeneration == OptionEnabDisab.Enable || EnableGeoPointGeneration == OptionEnabDisab.Enable))
                val = true;
            else
                val = false;

            return val;
        }
        private void GTLGenerate()
        {

            TerrainFilePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), AssetDatabase.GetAssetPath(TerrainFile));

            if (GISTerrainLoaderSupport.IsValidTerrainFile(TerrainFilePath))
            {
                heightmapResolution = heightmapResolutions[heightmapResolution_index];
                detailResolution = availableHeights[detailResolution_index];
                resolutionPerPatch = availableHeightsResolutionPrePec[resolutionPerPatch_index];
                baseMapResolution = availableHeights[baseMapResolution_index];

                if (!string.IsNullOrEmpty(TerrainFilePath))
                {
                    CheckForFile();
                }
                else
                {
                    Debug.LogError(" Please set DEM File .. Try againe");
                    OnError();
                }
            }


        }
#endregion
#region SaveLoad
        private void SavePrefs()
        {
            GISTerrainLoaderSaveLoadPrefs.SavePref("ShowMainTerrainFile", ShowMainTerrainFile);

            GISTerrainLoaderSaveLoadPrefs.SavePref("TerrainFile", new List<UnityEngine.Object>() { TerrainFile });
            GISTerrainLoaderSaveLoadPrefs.SavePref("readingMode", (int)readingMode);
            GISTerrainLoaderSaveLoadPrefs.SavePref("projectionMode", (int)projectionMode);
            GISTerrainLoaderSaveLoadPrefs.SavePref("tiffElevationSource", (int)tiffElevationSource);
            GISTerrainLoaderSaveLoadPrefs.SavePref("EPSGCode", EPSGCode);
            

            GISTerrainLoaderSaveLoadPrefs.SavePref("ShowCoordinates", ShowCoordinates);
            GISTerrainLoaderSaveLoadPrefs.SavePref("SubRegionUpperLeftCoordiante", SubRegionUpperLeftCoordiante);
            GISTerrainLoaderSaveLoadPrefs.SavePref("SubRegionDownRightCoordiante", SubRegionDownRightCoordiante);

            ////////////////////////////////////////////////////////////////////////////////
            GISTerrainLoaderSaveLoadPrefs.SavePref("ShowTexturePref", ShowTexturePref);
            GISTerrainLoaderSaveLoadPrefs.SavePref("TerrainElevationMode", (int)terrainElevation);
            GISTerrainLoaderSaveLoadPrefs.SavePref("TerrainExaggeration", TerrainExaggeration);
            GISTerrainLoaderSaveLoadPrefs.SavePref("TerrainDimensionMode", (int)terrainDimensionMode);
            GISTerrainLoaderSaveLoadPrefs.SavePref("TerrainFixOption", (int)TerrainFixOption);
            GISTerrainLoaderSaveLoadPrefs.SavePref("TerrainMaxMinElevation", TerrainMaxMinElevation);
            GISTerrainLoaderSaveLoadPrefs.SavePref("UnderWater", (int)UnderWater);
            GISTerrainLoaderSaveLoadPrefs.SavePref("TerrainDimensions", TerrainDimensions);
            GISTerrainLoaderSaveLoadPrefs.SavePref("TerrainScale", terrainScale);

            ////////////////////////////////////////////////////////////////////////////////
            GISTerrainLoaderSaveLoadPrefs.SavePref("ShowTerrainPref", ShowTerrainPref);
            GISTerrainLoaderSaveLoadPrefs.SavePref("heightmapResolution_index", heightmapResolution_index);
            GISTerrainLoaderSaveLoadPrefs.SavePref("detailResolution_index", detailResolution_index);
            GISTerrainLoaderSaveLoadPrefs.SavePref("resolutionPerPatch_index", resolutionPerPatch_index);
            GISTerrainLoaderSaveLoadPrefs.SavePref("baseMapResolution_index", baseMapResolution_index);
            GISTerrainLoaderSaveLoadPrefs.SavePref("PixelErro", PixelErro);
            GISTerrainLoaderSaveLoadPrefs.SavePref("baseMapDistance", BaseMapDistance);
            GISTerrainLoaderSaveLoadPrefs.SavePref("terrainMaterialMode", (int)terrainMaterialMode);
            GISTerrainLoaderSaveLoadPrefs.SavePref("terrainMaterial", new List<UnityEngine.Object>() { terrainMaterial });


            ////////////////////////////////////////////////////////////////////////////////
            GISTerrainLoaderSaveLoadPrefs.SavePref("ShowTerrainPref", ShowTerrainPref);
            GISTerrainLoaderSaveLoadPrefs.SavePref("textureMode", (int)textureMode);
            GISTerrainLoaderSaveLoadPrefs.SavePref("textureWidth", textureWidth);
            GISTerrainLoaderSaveLoadPrefs.SavePref("textureHeight", textureHeight);
            GISTerrainLoaderSaveLoadPrefs.SavePref("textureEmptyColor", TerrainEmptyColor);
            GISTerrainLoaderSaveLoadPrefs.SavePref("useTerrainEmptyColor", (int)UseTerrainEmptyColor);



            GISTerrainLoaderSaveLoadPrefs.SavePref("Slope", Slope);
            GISTerrainLoaderSaveLoadPrefs.SavePref("MergeRaduis", MergeRaduis);
            GISTerrainLoaderSaveLoadPrefs.SavePref("MergingFactor", MergingFactor);
            GISTerrainLoaderSaveLoadPrefs.SavePref("BaseTerrainLayers", BaseTerrainLayers);
            GISTerrainLoaderSaveLoadPrefs.SavePref("TerrainLayers", TerrainLayers);
            ////////////////////////////////////////////////////////////////////////////////

            GISTerrainLoaderSaveLoadPrefs.SavePref("UseTerrainHeightSmoother", (int)UseTerrainHeightSmoother);
            GISTerrainLoaderSaveLoadPrefs.SavePref("TerrainHeightSmoothFactor", TerrainHeightSmoothFactor);
            GISTerrainLoaderSaveLoadPrefs.SavePref("UseTerrainSurfaceSmoother", (int)UseTerrainSurfaceSmoother);
            GISTerrainLoaderSaveLoadPrefs.SavePref("TerrainSurfaceSmoothFactor", TerrainSurfaceSmoothFactor);

            ////////////////////////////////////////////////////////////////////////////////

            GISTerrainLoaderSaveLoadPrefs.SavePref("EnableGeoPointGeneration", (int)EnableGeoPointGeneration);
            GISTerrainLoaderSaveLoadPrefs.SavePref("EnableTreeGeneration", (int)EnableTreeGeneration);
            GISTerrainLoaderSaveLoadPrefs.SavePref("TreePrefabs", TreePrefabs);
            GISTerrainLoaderSaveLoadPrefs.SavePref("TreeDistance", TreeDistance);
            GISTerrainLoaderSaveLoadPrefs.SavePref("BillBoardStartDistance", BillBoardStartDistance);

            GISTerrainLoaderSaveLoadPrefs.SavePref("EnableGrassGeneration", (int)EnableGrassGeneration);
            GISTerrainLoaderSaveLoadPrefs.SavePref("GrassScaleFactor", GrassScaleFactor);
            GISTerrainLoaderSaveLoadPrefs.SavePref("DetailDistance", DetailDistance);
            GISTerrainLoaderSaveLoadPrefs.SavePref("GrassPrefabs", GrassPrefabs);

            GISTerrainLoaderSaveLoadPrefs.SavePref("EnableRoadGeneration", (int)EnableRoadGeneration);
            GISTerrainLoaderSaveLoadPrefs.SavePref("RoadType", (int)RoadType);

            GISTerrainLoaderSaveLoadPrefs.SavePref("EnableGeoLocationPointGeneration", (int)EnableGeoLocationPointGeneration);
            GISTerrainLoaderSaveLoadPrefs.SavePref("GeoPointPrefabs", new List<UnityEngine.GameObject>() { GeoPointPrefab });
            GISTerrainLoaderSaveLoadPrefs.SavePref("PathPrefabs", new List<GISTerrainLoaderSO_Road>() { PathPrefab });

            GISTerrainLoaderSaveLoadPrefs.SavePref("RemovePrvTerrain", (int)RemovePrvTerrain);


        }
        private void LoadPrefs()
        {
            ShowMainTerrainFile = GISTerrainLoaderSaveLoadPrefs.LoadPref("ShowMainTerrainFile", true);

            var Terrains = GISTerrainLoaderSaveLoadPrefs.LoadPref("TerrainFile", new List<UnityEngine.Object>());
            if (Terrains.Count > 0)
            {
                if (Terrains[0] != null)
                    TerrainFile = Terrains[0];
            }

            readingMode = (ReadingMode)GISTerrainLoaderSaveLoadPrefs.LoadPref("readingMode", (int)ReadingMode.Full);
            projectionMode = (ProjectionMode)GISTerrainLoaderSaveLoadPrefs.LoadPref("projectionMode", (int)ProjectionMode.Auto);
            tiffElevationSource = (TiffElevationSource)GISTerrainLoaderSaveLoadPrefs.LoadPref("tiffElevationSource", (int)TiffElevationSource.DEM);
            EPSGCode = GISTerrainLoaderSaveLoadPrefs.LoadPref("EPSGCode", 0);

            ShowCoordinates = GISTerrainLoaderSaveLoadPrefs.LoadPref("ShowCoordinates", false);
            SubRegionUpperLeftCoordiante = GISTerrainLoaderSaveLoadPrefs.LoadPref("SubRegionUpperLeftCoordiante", new DVector2(0, 0));
            SubRegionDownRightCoordiante = GISTerrainLoaderSaveLoadPrefs.LoadPref("SubRegionDownRightCoordiante", new DVector2(0, 0));

            ////////////////////////////////////////////////////////////////////////////////

            ShowTexturePref = GISTerrainLoaderSaveLoadPrefs.LoadPref("ShowTexturePref", false);

            terrainElevation = (TerrainElevation)GISTerrainLoaderSaveLoadPrefs.LoadPref("TerrainElevationMode", (int)TerrainElevation.RealWorldElevation);
            TerrainExaggeration = GISTerrainLoaderSaveLoadPrefs.LoadPref("TerrainExaggeration", 0.27f);
            terrainDimensionMode = (TerrainDimensionsMode)GISTerrainLoaderSaveLoadPrefs.LoadPref("TerrainDimensionMode", (int)TerrainDimensionsMode.AutoDetection);
            TerrainDimensions = GISTerrainLoaderSaveLoadPrefs.LoadPref("TerrainDimensions", new DVector2(10, 10));
            TerrainFixOption = (FixOption)GISTerrainLoaderSaveLoadPrefs.LoadPref("TerrainFixOption", (int)FixOption.Disable);
            TerrainMaxMinElevation = GISTerrainLoaderSaveLoadPrefs.LoadPref("TerrainMaxMinElevation", new Vector2(0, 0));
            UnderWater = (OptionEnabDisab)GISTerrainLoaderSaveLoadPrefs.LoadPref("UnderWater", (int)OptionEnabDisab.Disable);
            terrainScale = GISTerrainLoaderSaveLoadPrefs.LoadPref("TerrainScale", new Vector3(1, 1, 1));



            ////////////////////////////////////////////////////////////////////////////////

            heightmapResolution_index = GISTerrainLoaderSaveLoadPrefs.LoadPref("heightmapResolution_index", 2);
            heightmapResolution = heightmapResolutions[heightmapResolution_index];

            detailResolution_index = GISTerrainLoaderSaveLoadPrefs.LoadPref("detailResolution_index", 4);
            detailResolution = availableHeights[detailResolution_index];

            resolutionPerPatch_index = GISTerrainLoaderSaveLoadPrefs.LoadPref("resolutionPerPatch_index", 1);
            resolutionPerPatch = availableHeightsResolutionPrePec[resolutionPerPatch_index];

            baseMapResolution_index = GISTerrainLoaderSaveLoadPrefs.LoadPref("baseMapResolution_index", 4);
            baseMapResolution = availableHeights[baseMapResolution_index];

            PixelErro = GISTerrainLoaderSaveLoadPrefs.LoadPref("PixelErro", 1.0f);
            BaseMapDistance = GISTerrainLoaderSaveLoadPrefs.LoadPref("baseMapDistance", 1000.0f);

            var TerrainsMat = GISTerrainLoaderSaveLoadPrefs.LoadPref("terrainMaterial", new List<UnityEngine.Object>());
            if (TerrainsMat.Count > 0)
            {
                if (TerrainsMat[0] != null)
                    terrainMaterial = TerrainsMat[0] as Material;
            }
            ////////////////////////////////////////////////////////////////////////////////

            textureMode = (TextureMode)GISTerrainLoaderSaveLoadPrefs.LoadPref("textureMode", (int)TextureMode.WithTexture);
            textureWidth = GISTerrainLoaderSaveLoadPrefs.LoadPref("textureWidth", 1024);
            textureHeight = GISTerrainLoaderSaveLoadPrefs.LoadPref("textureHeight", 1024);
            TerrainEmptyColor = GISTerrainLoaderSaveLoadPrefs.LoadPref("textureEmptyColor", Color.white);
            UseTerrainEmptyColor = (OptionEnabDisab)GISTerrainLoaderSaveLoadPrefs.LoadPref("useTerrainEmptyColor", (int)OptionEnabDisab.Disable);


            Slope = GISTerrainLoaderSaveLoadPrefs.LoadPref("Slope", 0f);
            MergeRaduis = GISTerrainLoaderSaveLoadPrefs.LoadPref("MergeRaduis", 1);
            MergingFactor = GISTerrainLoaderSaveLoadPrefs.LoadPref("MergingFactor", 1);
            BaseTerrainLayers = GISTerrainLoaderSaveLoadPrefs.LoadPref("BaseTerrainLayers", new GISTerrainLoaderTerrainLayer());
            TerrainLayers = GISTerrainLoaderSaveLoadPrefs.LoadPref("TerrainLayers", new List<GISTerrainLoaderTerrainLayer>());
            ////////////////////////////////////////////////////////////////////////////////

            UseTerrainHeightSmoother = (OptionEnabDisab)GISTerrainLoaderSaveLoadPrefs.LoadPref("UseTerrainHeightSmoother", (int)OptionEnabDisab.Disable);
            TerrainHeightSmoothFactor = GISTerrainLoaderSaveLoadPrefs.LoadPref("TerrainHeightSmoothFactor", 0.05f);
            UseTerrainSurfaceSmoother = (OptionEnabDisab)GISTerrainLoaderSaveLoadPrefs.LoadPref("UseTerrainSurfaceSmoother", (int)OptionEnabDisab.Disable);
            TerrainSurfaceSmoothFactor = GISTerrainLoaderSaveLoadPrefs.LoadPref("TerrainSurfaceSmoothFactor", 4);

            ////////////////////////////////////////////////////////////////////////////////

            EnableGeoPointGeneration = (OptionEnabDisab)GISTerrainLoaderSaveLoadPrefs.LoadPref("EnableGeoPointGeneration", (int)(OptionEnabDisab.Disable));
            EnableTreeGeneration = (OptionEnabDisab)GISTerrainLoaderSaveLoadPrefs.LoadPref("EnableTreeGeneration", (int)(OptionEnabDisab.Disable));
            TreePrefabs = GISTerrainLoaderSaveLoadPrefs.LoadPref("TreePrefabs", new List<GISTerrainLoaderSO_Tree>());
            TreeDistance = GISTerrainLoaderSaveLoadPrefs.LoadPref("TreeDistance", 4000f);
            BillBoardStartDistance = GISTerrainLoaderSaveLoadPrefs.LoadPref("BillBoardStartDistance", 300f);

            EnableGrassGeneration = (OptionEnabDisab)GISTerrainLoaderSaveLoadPrefs.LoadPref("EnableGrassGeneration", (int)(OptionEnabDisab.Disable));
            GrassScaleFactor = GISTerrainLoaderSaveLoadPrefs.LoadPref("GrassScaleFactor", 10f);
            DetailDistance = GISTerrainLoaderSaveLoadPrefs.LoadPref("DetailDistance", 380f);
            GrassPrefabs = GISTerrainLoaderSaveLoadPrefs.LoadPref("GrassPrefabs", new List<GISTerrainLoaderSO_GrassObject>());


            EnableRoadGeneration = (OptionEnabDisab)GISTerrainLoaderSaveLoadPrefs.LoadPref("EnableRoadGeneration", (int)(OptionEnabDisab.Disable));
            RoadType = (RoadGenerationType)GISTerrainLoaderSaveLoadPrefs.LoadPref("RoadType", (int)RoadGenerationType.Line);

            EnableGeoLocationPointGeneration = (OptionEnabDisab)GISTerrainLoaderSaveLoadPrefs.LoadPref("EnableGeoLocationPointGeneration", (int)(OptionEnabDisab.Disable));
            var GeoPointPrefabs = GISTerrainLoaderSaveLoadPrefs.LoadPref("GeoPointPrefabs", new List<UnityEngine.GameObject>());
            if (GeoPointPrefabs.Count > 0)
            {
                if (GeoPointPrefabs[0] != null)
                    GeoPointPrefab = GeoPointPrefabs[0];
            }

            var PathPrefabs = GISTerrainLoaderSaveLoadPrefs.LoadPref("PathPrefabs", new List<GISTerrainLoaderSO_Road>());
            if (PathPrefabs.Count > 0)
            {
                if (PathPrefabs[0] != null)
                    PathPrefab = PathPrefabs[0];
            }

            RemovePrvTerrain = (OptionEnabDisab)GISTerrainLoaderSaveLoadPrefs.LoadPref("RemovePrvTerrain", (int)(OptionEnabDisab.Disable));


        }
        private void ResetPrefs()
        {
            TerrainFile = null;
            readingMode = ReadingMode.Full;


            ////////////////////////////////////////////////////////////////////////////////

            terrainElevation = TerrainElevation.RealWorldElevation;
            TerrainExaggeration = 0.27f;
            terrainDimensionMode = TerrainDimensionsMode.AutoDetection;
            TerrainDimensions = new DVector2(10, 10);
            UnderWater = OptionEnabDisab.Disable;
            TerrainFixOption = FixOption.Disable;
            TerrainMaxMinElevation = new Vector2(0, 0);
            terrainScale = new Vector3(1, 1, 1);

            ////////////////////////////////////////////////////////////////////////////////

            heightmapResolution_index = 2;
            heightmapResolution = heightmapResolutions[heightmapResolution_index];

            detailResolution_index = 4;
            detailResolution = availableHeights[detailResolution_index];

            resolutionPerPatch_index = 1;
            resolutionPerPatch = availableHeightsResolutionPrePec[resolutionPerPatch_index];

            baseMapResolution_index = 4;
            baseMapResolution = availableHeights[baseMapResolution_index];

            PixelErro = 1;
            BaseMapDistance = 1000;

            ////////////////////////////////////////////////////////////////////////////////
            terrainMaterialMode = TerrainMaterialMode.Standard;
            textureMode = TextureMode.WithTexture;
            textureWidth = 1024;
            textureHeight = 1024;
            TerrainEmptyColor = Color.white;

            ////////////////////////////////////////////////////////////////////////////////

            UseTerrainHeightSmoother = OptionEnabDisab.Disable;
            TerrainHeightSmoothFactor = 0.05f;
            UseTerrainSurfaceSmoother = OptionEnabDisab.Disable;
            TerrainSurfaceSmoothFactor = 2;

            ////////////////////////////////////////////////////////////////////////////////

            EnableTreeGeneration = OptionEnabDisab.Disable;
            TreeDistance = 4000;
            BillBoardStartDistance = 300;
            TreePrefabs = new List<GISTerrainLoaderSO_Tree>();

            EnableGrassGeneration = OptionEnabDisab.Disable;
            GrassScaleFactor = 5f;
            DetailDistance = 350;
            GrassPrefabs = new List<GISTerrainLoaderSO_GrassObject>();

            EnableRoadGeneration = OptionEnabDisab.Disable;
            RoadType = RoadGenerationType.Line;

            EnableBuildingGeneration = OptionEnabDisab.Disable;

        }
#endregion
    }
}