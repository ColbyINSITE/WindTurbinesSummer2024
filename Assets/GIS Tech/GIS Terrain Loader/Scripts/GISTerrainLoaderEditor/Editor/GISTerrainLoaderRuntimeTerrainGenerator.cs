/*     Unity GIS Tech 2020-2021      */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR 
using UnityEditor;
#endif
namespace GISTech.GISTerrainLoader
{
#if UNITY_EDITOR
    [CustomEditor(typeof(GISTerrainLoaderRuntimePrefs))]
    public class GISTerrainLoaderRuntimeTerrainGenerator : Editor
    {
        private GISTerrainLoaderRuntimePrefs RuntimePrefs { get { return target as GISTerrainLoaderRuntimePrefs; } }

        private TabsBlock tabs;

        private Texture2D m_resetPrefs;

        private void OnEnable()
        {
            tabs = new TabsBlock(new Dictionary<string, System.Action>()
            {
                {"DEM Terrain", DEMFileTab},
                {"Elevation,Scaling..", ElevationScalingTab},
                {"Terrain Preferences", TerrainPreferencesTab},
                {"Texturing", TexturingTab},
                {"Smoothing", SmoothingTab},
                {"Vector Data", VectorDataTab},
                {"Options", OptionsTab}
            });
            tabs.SetCurrentMethod(RuntimePrefs.lastTab);

            if (m_resetPrefs == null)
                m_resetPrefs = LoadTexture("GTL_ResetPrefs");
        }
        private void DEMFileTab()
        {
            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" File Reading Mode "," Default is full heightmap mode, used to read whole hightmap file; sub region mode used to import a sub region of the file instead, note that coordinates of sub regions is needed; this option available only for GeoRefenced files (Tiff,HGT,BIL,ASC,FLT)"), GUILayout.MaxWidth(200));
                RuntimePrefs.readingMode = (ReadingMode)EditorGUILayout.EnumPopup("", RuntimePrefs.readingMode);
            }

            using (new VerticalBlock())
            {
                if (RuntimePrefs.readingMode == ReadingMode.SubRegion)
                {
                    CoordinatesBarGUI();
                }

            }
        }
        private void ElevationScalingTab()
        {
            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Terrain Elevation Mode ", "Generate Terrain By Loading Real Elevation Data or By using 'Exaggeration' value to set manualy terrain elevation factor"), GUILayout.MaxWidth(200));
                RuntimePrefs.TerrainElevation = (TerrainElevation)EditorGUILayout.EnumPopup("", RuntimePrefs.TerrainElevation);
            }
            using (new VerticalBlock())
            {
                if (RuntimePrefs.TerrainElevation == TerrainElevation.RealWorldElevation)
                {

                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent(" Exaggeration value ", "Vertical exaggeration can be used to emphasize subtle changes in a surface. This can be useful in creating visualizations of terrain where the horizontal extent of the surface is significantly greater than the amount of vertical change in the surface. A fractional vertical exaggeration can be used to flatten surfaces or features that have extreme vertical variation"), GUILayout.MaxWidth(200));
                    RuntimePrefs.TerrainExaggeration = EditorGUILayout.Slider(RuntimePrefs.TerrainExaggeration, 0, 1);
                    GUILayout.EndHorizontal();
                }
 

            }

            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Terrain Dimensions Mode ", "This Option let us to load real terrain Width/Lenght for almost of supported types - We can set it to manual to make terrain small or large as we want by setting new W/L values in 'KM' "), GUILayout.MaxWidth(200));
                RuntimePrefs.terrainDimensionMode = (TerrainDimensionsMode)EditorGUILayout.EnumPopup("", RuntimePrefs.terrainDimensionMode);
            }
            using (new HorizontalBlock())
            {
            if (RuntimePrefs.terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
            {

                if (!RuntimePrefs.TerrainHasDimensions)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label(new GUIContent(" Terrain Dimensions (Km)    ", "This Showing because DEM file not loaded yet or has no real dimensions so we have to set Manualy terrain width/lenght in KM"), GUILayout.MaxWidth(220));

                    GUILayout.Label(" Width ");
                    RuntimePrefs.TerrainDimensions.x = EditorGUILayout.FloatField(RuntimePrefs.TerrainDimensions.x, GUILayout.ExpandWidth(true));

                    GUILayout.Label(" Lenght ");
                    RuntimePrefs.TerrainDimensions.y = EditorGUILayout.FloatField(RuntimePrefs.TerrainDimensions.y, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                }
            }
            else
if (RuntimePrefs.terrainDimensionMode == TerrainDimensionsMode.Manual)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label(new GUIContent(" Terrain Dimensions (Km)     ", "Set Manualy terrain width/lenght in KM"), GUILayout.MaxWidth(220));

                GUILayout.Label(" Width ");
                RuntimePrefs.TerrainDimensions.x = EditorGUILayout.FloatField(RuntimePrefs.TerrainDimensions.x, GUILayout.ExpandWidth(true));

                GUILayout.Label(" Lenght ");
                RuntimePrefs.TerrainDimensions.y = EditorGUILayout.FloatField(RuntimePrefs.TerrainDimensions.y, GUILayout.ExpandWidth(true));

                GUILayout.EndHorizontal();

            }



            }
 
            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" UnderWater ", "Enable This Option to load negative values from DEM files "), GUILayout.MaxWidth(200));
                RuntimePrefs.UnderWater = (OptionEnabDisab)EditorGUILayout.EnumPopup("", RuntimePrefs.UnderWater);
            }
            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Fix Terrain ", " (Only for Real World Data) Use this option to fix null terrain elevations + unkown Min/Max values  + avoid extrem elevation values in order to generate terrain without any deformation Manually or Automatically "), GUILayout.MaxWidth(200));
                RuntimePrefs.TerrainFixOption = (FixOption)EditorGUILayout.EnumPopup("", RuntimePrefs.TerrainFixOption);
            }

            if (RuntimePrefs.TerrainFixOption == FixOption.ManualFix)
            {
                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent(" Elevation [m] ", "Set Manually terrain Max and Min Elevation in [m]"), GUILayout.MaxWidth(200));
                    GUILayout.Label("  Min ");
                    RuntimePrefs.TerrainMaxMinElevation.x = EditorGUILayout.FloatField(RuntimePrefs.TerrainMaxMinElevation.x, GUILayout.ExpandWidth(true));

                    GUILayout.Label("  Max ");
                    RuntimePrefs.TerrainMaxMinElevation.y = EditorGUILayout.FloatField(RuntimePrefs.TerrainMaxMinElevation.y, GUILayout.ExpandWidth(true));

                }
 
            }

            using (new HorizontalBlock())
            {

                GUILayout.Label(new GUIContent(" Terrain Scale ", " Specifies the terrain scale factor in three directions (if terrain is large with 1 value you can set small float value like 0.5f - 0.1f - 0.01f"), GUILayout.MaxWidth(200));
                RuntimePrefs.terrainScale = EditorGUILayout.Vector3Field("", RuntimePrefs.terrainScale);
            }

            if (RuntimePrefs.terrainScale.x == 0 || RuntimePrefs.terrainScale.y == 0 || RuntimePrefs.terrainScale.z == 0)
                EditorGUILayout.HelpBox("Check your Terrain Scale (Terrain Scale is null !)", MessageType.Warning);


        }
        private void TerrainPreferencesTab()
        {

            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Heightmap Resolution ", "The pixel resolution of the Terrain’s heightmap"), GUILayout.MaxWidth(200));
                RuntimePrefs.heightmapResolution_index = EditorGUILayout.Popup(RuntimePrefs.heightmapResolution_index, RuntimePrefs.heightmapResolutionsSrt, GUILayout.ExpandWidth(true));
            }

            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Detail Resolution ", "The number of cells available for placing details onto the Terrain tile used to controls grass and detail meshes. Lower you set this number performance will be better"), GUILayout.MaxWidth(200));
                RuntimePrefs.detailResolution_index = EditorGUILayout.Popup(RuntimePrefs.detailResolution_index, RuntimePrefs.availableHeightSrt, GUILayout.ExpandWidth(true));
            }


            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Resolution Per Patch ", "The number of cells in a single patch (mesh), recommended value is 16 for very large detail object distance "), GUILayout.MaxWidth(200));
                RuntimePrefs.resolutionPerPatch_index = EditorGUILayout.Popup(RuntimePrefs.resolutionPerPatch_index, RuntimePrefs.availableHeightsResolutionPrePectSrt, GUILayout.ExpandWidth(true));
            }

            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Base Map Resolution ", "Resolution of the composite texture used on the terrain when viewed from a distance greater than the Basemap Distance"), GUILayout.MaxWidth(200));
                RuntimePrefs.baseMapResolution_index = EditorGUILayout.Popup(RuntimePrefs.baseMapResolution_index, RuntimePrefs.availableHeightSrt, GUILayout.ExpandWidth(true));
            }
            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Pixel Error ", " The accuracy of the mapping between Terrain maps (such as heightmaps and Textures) and generated Terrain. Higher values indicate lower accuracy, but with lower rendering overhead. "), GUILayout.MaxWidth(200));
                RuntimePrefs.PixelError = EditorGUILayout.Slider(RuntimePrefs.PixelError, 1f, 200f, GUILayout.ExpandWidth(true));
            }
            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" BaseMap Distance ", " The maximum distance at which Unity displays Terrain Textures at full resolution. Beyond this distance, the system uses a lower resolution composite image for efficiency "), GUILayout.MaxWidth(200));
                RuntimePrefs.BaseMapDistance = EditorGUILayout.IntSlider(RuntimePrefs.BaseMapDistance, 1, 20000, GUILayout.ExpandWidth(true));
            }

            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Terrain Material Mode ", "This option used to cutomize terrain material ex : in case of using HDRP "), GUILayout.MaxWidth(200));
                RuntimePrefs.terrainMaterialMode = (TerrainMaterialMode)EditorGUILayout.EnumPopup("", RuntimePrefs.terrainMaterialMode, GUILayout.ExpandWidth(true));
            }


            if (RuntimePrefs.terrainMaterialMode == TerrainMaterialMode.Custom)
            {

                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent("  Terrain Material ", "Materail that will be used in the generated terrains "), GUILayout.MaxWidth(200));
                    RuntimePrefs.terrainMaterial = (Material)EditorGUILayout.ObjectField(RuntimePrefs.terrainMaterial, typeof(UnityEngine.Material),true ,GUILayout.ExpandWidth(true));
                }
 

            }

            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Set Terrain Layer ", "This option cutomize terrain Layer "), GUILayout.MaxWidth(200));
                RuntimePrefs.TerrainLayerSet = (OptionEnabDisab)EditorGUILayout.EnumPopup("", RuntimePrefs.TerrainLayerSet, GUILayout.ExpandWidth(true));
            }

            if (RuntimePrefs.TerrainLayerSet == OptionEnabDisab.Enable)
            {

                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent(" Terrain Layer ", " Set Terrain Layer"), GUILayout.MaxWidth(220));
 
                    RuntimePrefs.TerrainLayer = EditorGUILayout.IntField(RuntimePrefs.TerrainLayer, GUILayout.ExpandWidth(true));
                }


            }
        }
        private void TexturingTab()
        {
            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Texturing Mode ", "Generate Terrain with or without textures (Specifies the count terrains is needed when selecting 'Without' because Texture folder will not readed "), GUILayout.MaxWidth(200));
                RuntimePrefs.textureMode = (TextureMode)EditorGUILayout.EnumPopup("", RuntimePrefs.textureMode, GUILayout.ExpandWidth(true));
            }
 
            switch(RuntimePrefs.textureMode)
            {
                case TextureMode.WithTexture:

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent("  Textures Loading Mode ", " The Creation of terrain tiles is based on the number of texture tiles existing in the terrain texture folder, setting this parameter to Auto means that GTL will load and generate terrains by loading directly textures from texture folder /// if it set to Manually, GTL will make some operations of merging and spliting existing textures to make them simulair to terrain tiles count ' Attention' : this operation may consume memory when textures are larges '"), GUILayout.MaxWidth(200));
                       RuntimePrefs.textureloadingMode = (TexturesLoadingMode)EditorGUILayout.EnumPopup("", RuntimePrefs.textureloadingMode, GUILayout.ExpandWidth(true));
                    }

                    if (RuntimePrefs.textureloadingMode == TexturesLoadingMode.Manual)
                    {
                        using (new HorizontalBlock())
                        {
                            GUILayout.Label(new GUIContent("   Count Tiles ", " Specifie the number of terrain tiles , ' Attention '  Count Tiles set is different than the number of terrain textures tiles (Located in the Terrain texture folder), some operations (Spliting/mergins) textures will excuted so becarful when textures are large"), GUILayout.MaxWidth(200));
                            RuntimePrefs.terrainCount = EditorGUILayout.Vector2IntField("", RuntimePrefs.terrainCount);
                        }
                        EditorGUILayout.HelpBox("' Attention Memory ' When terrain Count Tiles is different than the number of terrain textures tiles(Located in the Terrain texture folder), some operations(Spliting / Mergins) textures will excuted so becarful for large textures ", MessageType.Warning);
                    }
                    break;

                case TextureMode.WithoutTexture:

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent(" Count Tiles ", " Number of Terrain Tiles in X-Y, This Option is avalaible Only when Texturing mode set to 'Without' or 'Splatmapping' "), GUILayout.MaxWidth(200));
                        RuntimePrefs.terrainCount = EditorGUILayout.Vector2IntField("", RuntimePrefs.terrainCount);
                    }

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent(" Use Custom Terrain Color ", "Enable/Disable customize terrain color "), GUILayout.MaxWidth(200));
                        RuntimePrefs.UseTerrainEmptyColor = EditorGUILayout.Toggle("", RuntimePrefs.UseTerrainEmptyColor, GUILayout.ExpandWidth(true));
                    }

                    if (RuntimePrefs.UseTerrainEmptyColor)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("  Terrain Color ", "Used to change the main terrain color"), GUILayout.MaxWidth(200));
                        RuntimePrefs.TerrainEmptyColor = EditorGUILayout.ColorField("", RuntimePrefs.TerrainEmptyColor, GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();
                    }

                    break;

                case TextureMode.ShadedRelief:

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent(" Count Tiles ", " Number of Terrain Tiles in X-Y, This Option is avalaible Only when Texturing mode set to 'Without' or 'Splatmapping' "), GUILayout.MaxWidth(200));
                        RuntimePrefs.terrainCount = EditorGUILayout.Vector2IntField("", RuntimePrefs.terrainCount);
                    }

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent(" Shader Type ", " Select terrain type "), GUILayout.MaxWidth(200));
                        RuntimePrefs.TerrainShaderType = (ShaderType)EditorGUILayout.EnumPopup("", RuntimePrefs.TerrainShaderType, GUILayout.ExpandWidth(true));
                    }

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent(" UnderWater ", "Enable This Option to generate shaders to underwater terrains (Used to avoid blue color) "), GUILayout.MaxWidth(200));
                        RuntimePrefs.UnderWaterShader = (OptionEnabDisab)EditorGUILayout.EnumPopup("", RuntimePrefs.UnderWaterShader);
                    }

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent(" Save Shader Texture ", "Enable This Option to save the generated shaders as textures (For Runtime in ' Source Terrain Folder'), the texture resolution equal to terrain hightmap resolution"), GUILayout.MaxWidth(200));
                        RuntimePrefs.SaveShaderTextures = (OptionEnabDisab)EditorGUILayout.EnumPopup("", RuntimePrefs.SaveShaderTextures);
                    }


                    break;

                case TextureMode.Splatmapping:

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent(" Count Tiles ", " Number of Terrain Tiles in X-Y, This Option is avalaible Only when Texturing mode set to 'Without' or 'Splatmapping' "), GUILayout.MaxWidth(200));
                        RuntimePrefs.terrainCount = EditorGUILayout.Vector2IntField("", RuntimePrefs.terrainCount);
                    }
 

                    if (GUILayout.Button(new GUIContent(" Distributing Values ", m_resetPrefs, "Set All Splatmapping values to default and distributing slopes values "), new GUIStyle(EditorStyles.toolbarButton), GUILayout.ExpandWidth(true)))
                    {
                        RuntimePrefs.Slope = 0.1f;
                        RuntimePrefs.MergeRaduis = 1;
                        RuntimePrefs.MergingFactor = 1;

                        float step = 1f / RuntimePrefs.TerrainLayers.Count;

                        for (int i = 0; i < RuntimePrefs.TerrainLayers.Count; i++)
                        {
                            RuntimePrefs.TerrainLayers[i].X_Height = i * step;
                            RuntimePrefs.TerrainLayers[i].Y_Height = (i + 1) * step;
                        }

                    }
                    using (new VerticalBlock())
                    {
                        using (new HorizontalBlock())
                        {
                            GUILayout.Label(new GUIContent("  Slope ", " Used to normalized the slope in Y dir, The default value = 0"), GUILayout.MaxWidth(200));
                            RuntimePrefs.Slope = EditorGUILayout.Slider(RuntimePrefs.Slope, 0.0f, 1, GUILayout.MaxWidth(200), GUILayout.ExpandWidth(true));
                        }
                    }

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent("  Merging Raduis ", " Used to precise the raduis of merging between layers, 0 value means that no merging operation will apply  "), GUILayout.MaxWidth(200));
                        RuntimePrefs.MergeRaduis = EditorGUILayout.IntSlider(RuntimePrefs.MergeRaduis, 0, 500, GUILayout.MaxWidth(200), GUILayout.ExpandWidth(true));
                    }
                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent("  Merging Factor ", " Used to precise how many times the merging will applyed on the terrain, the default is 1 "), GUILayout.MaxWidth(200));
                        RuntimePrefs.MergingFactor = EditorGUILayout.IntSlider(RuntimePrefs.MergingFactor, 1, 5, GUILayout.MaxWidth(200), GUILayout.ExpandWidth(true));
                    }
                    using (new VerticalBlock())
                    {
                        GUILayout.Label(" ");
                        GUILayout.Label(" ");
                        GUILayout.Label(new GUIContent("  Base Terrain Map ", " this will be the first splatmap for slope = 0"), GUILayout.MaxWidth(200));
                        RuntimePrefs.BaseTerrainLayers.ShowHeight = false;

                        using (new HorizontalBlock())
                        {
                            SerializedObject BaseLayerso = new SerializedObject(RuntimePrefs);
                            SerializedProperty BaseLayerProperty = BaseLayerso.FindProperty("BaseTerrainLayers");
                            EditorGUILayout.PropertyField(BaseLayerProperty, true, GUILayout.MinWidth(0), GUILayout.MaxWidth(50), GUILayout.ExpandWidth(true));
                            BaseLayerso.ApplyModifiedProperties();
                        }

                        GUILayout.Label(" ");

                        using (new VerticalBlock())
                        {
                            SerializedObject LayersSO = new SerializedObject(RuntimePrefs);
                            SerializedProperty LayersProperty = LayersSO.FindProperty("TerrainLayers");
                            EditorGUILayout.PropertyField(LayersProperty, true);
                            LayersSO.ApplyModifiedProperties();

                            foreach (var layer in RuntimePrefs.TerrainLayers)
                                layer.ShowHeight = true;
                        }
                    }

                    break;


            }
 
  
        }
        private void SmoothingTab()
        {


            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Terrain Height Smoother ", "Used to softens the landscape and reduces the appearance of abrupt changes"), GUILayout.MaxWidth(200));
                RuntimePrefs.UseTerrainHeightSmoother = EditorGUILayout.Toggle("", RuntimePrefs.UseTerrainHeightSmoother, GUILayout.ExpandWidth(true));
            }

            if (RuntimePrefs.UseTerrainHeightSmoother)
            {
                using (new HorizontalBlock())
                {
                    GUILayout.Label("  Terrain Height Smooth Factor ", GUILayout.MaxWidth(200));
                    RuntimePrefs.TerrainHeightSmoothFactor = EditorGUILayout.Slider(RuntimePrefs.TerrainHeightSmoothFactor, 0.0f, 0.3f, GUILayout.ExpandWidth(true));
                }

            }

            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Terrain Surface Smoother ", " this operation is useful when for terrains with unwanted jaggies, terraces,banding and non-smoothed terrain heights. Changing the surface smoother value to higher means more smoothing on surface while 1 value means minimum smoothing"), GUILayout.MaxWidth(200));
                RuntimePrefs.UseTerrainSurfaceSmoother = EditorGUILayout.Toggle("", RuntimePrefs.UseTerrainSurfaceSmoother, GUILayout.ExpandWidth(true));
            }
 


            if (RuntimePrefs.UseTerrainSurfaceSmoother)
            {
                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent("  Terrain Surface Smooth Factor ", ""), GUILayout.MaxWidth(200));
                    RuntimePrefs.TerrainSurfaceSmoothFactor = EditorGUILayout.IntSlider(RuntimePrefs.TerrainSurfaceSmoothFactor, 1, 15, GUILayout.ExpandWidth(true));
                }

            }
        }
        private void VectorDataTab()
        {
            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Vector Type ", "Select your vector type (Data must added to VectorData folder)"), GUILayout.MaxWidth(200));
                RuntimePrefs.vectorType = (VectorType)EditorGUILayout.EnumPopup("", RuntimePrefs.vectorType, GUILayout.ExpandWidth(true));
            }

            if ((RuntimePrefs.vectorType != VectorType.GPX))
            {


                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent(" Generate Trees ", "Enable/Disable Loading and Generating Trees from Vector Files "), GUILayout.MaxWidth(200));
                    RuntimePrefs.EnableTreeGeneration = EditorGUILayout.Toggle("", RuntimePrefs.EnableTreeGeneration, GUILayout.ExpandWidth(true));
                }

                if (RuntimePrefs.EnableTreeGeneration)
                {
                    //Tree Distance
                    using (new HorizontalBlock())
                    {
                        GUILayout.Label("  Tree Distance ", GUILayout.MaxWidth(200));
                        RuntimePrefs.TreeDistance = EditorGUILayout.Slider(RuntimePrefs.TreeDistance, 1, 5000, GUILayout.ExpandWidth(true));

                    }

                    //Tree BillBoard Start Distance
                    using (new HorizontalBlock())
                    {
                        GUILayout.Label("  Tree BillBoard Start Distance ", GUILayout.MaxWidth(200));
                        RuntimePrefs.BillBoardStartDistance = EditorGUILayout.Slider(RuntimePrefs.BillBoardStartDistance, 1, 2000, GUILayout.ExpandWidth(true));

                    }
                    //Tree Prefabs List
                    using (new HorizontalBlock())
                    {
                        GUILayout.Label("  Trees ", GUILayout.MaxWidth(200));
                        SerializedObject so = new SerializedObject(RuntimePrefs);
                        SerializedProperty stringsProperty = so.FindProperty("TreePrefabs");
                        EditorGUILayout.PropertyField(stringsProperty, true);
                        so.ApplyModifiedProperties();
                    }

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent("                ", " "), GUILayout.MaxWidth(200));

                        if (GUILayout.Button(new GUIContent(" Load All ", "Click To Load all tree prefabs Located in 'GIS Tech/GIS Terrain Loader/Resources/Prefabs/Environment/Trees'"), GUILayout.ExpandWidth(true)))
                        {
                            RuntimePrefs.LoadAllTreePrefabs();
                        }
                    }

                }


                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent(" Generate Grass ", "Enable/Disable Loading and Generating Grass from OSM File "), GUILayout.MaxWidth(200));
                    RuntimePrefs.EnableGrassGeneration = EditorGUILayout.Toggle("", RuntimePrefs.EnableGrassGeneration, GUILayout.ExpandWidth(true));
                }

                if (RuntimePrefs.EnableGrassGeneration)
                {

                    //Grass Scale Factor
                    using (new HorizontalBlock())
                    {
                        GUILayout.Label("  Grass Scale Factor ", GUILayout.MaxWidth(200));
                        RuntimePrefs.GrassScaleFactor = EditorGUILayout.Slider(RuntimePrefs.GrassScaleFactor, 0.1f, 100, GUILayout.ExpandWidth(true));
                    }


                    //Detail Distance
                    using (new HorizontalBlock())
                    {
                        GUILayout.Label("  Detail Distance ", GUILayout.MaxWidth(200));
                        RuntimePrefs.DetailDistance = EditorGUILayout.Slider(RuntimePrefs.DetailDistance, 10f, 400, GUILayout.ExpandWidth(true));

                    }


                    //Tree Prefabs List
                    using (new HorizontalBlock())
                    {
                        GUILayout.Label("  Grass ", GUILayout.MaxWidth(200));

                        SerializedObject so = new SerializedObject(RuntimePrefs);
                        SerializedProperty stringsProperty = so.FindProperty("GrassPrefabs");

                        EditorGUILayout.PropertyField(stringsProperty, true);
                        so.ApplyModifiedProperties();
                    }

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent("                ", " "), GUILayout.MaxWidth(200));

                        if (GUILayout.Button(new GUIContent(" Load All ", "Click To Load all Grass prefabs Located in 'GIS Tech/GIS Terrain Loader/Resources/Prefabs/Environment/Grass'"), GUILayout.ExpandWidth(true)))
                        {
                            RuntimePrefs.LoadAllGrassPrefabs();
                        }
                    }
                }

                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent(" Generate GeoPoints ", " Enable this option to generate gamebjects according to geo-points coordinates found in the vector file"), GUILayout.MaxWidth(200));
                    RuntimePrefs.EnableGeoPointGeneration = EditorGUILayout.Toggle("", RuntimePrefs.EnableGeoPointGeneration, GUILayout.ExpandWidth(true));

                }
                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent(" Generate Roads ", "Enable/Disable Loading and Generating Roads from OSM File "), GUILayout.MaxWidth(200));
                    RuntimePrefs.EnableRoadGeneration = EditorGUILayout.Toggle("", RuntimePrefs.EnableRoadGeneration, GUILayout.ExpandWidth(true));

                }

                if (RuntimePrefs.EnableRoadGeneration)
                {
                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent("  Road Generator Type ", "Select whiche type of road will be used (Note that EasyRoad3D must be existing in the project "), GUILayout.MaxWidth(200));
                        RuntimePrefs.RoadType = (RoadGenerationType)EditorGUILayout.EnumPopup("", RuntimePrefs.RoadType, GUILayout.ExpandWidth(true));

                    }

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent("  Roads Lable ", "Add Roads name  "), GUILayout.MaxWidth(200));
                        RuntimePrefs.EnableRoadName = EditorGUILayout.Toggle("", RuntimePrefs.EnableRoadName, GUILayout.ExpandWidth(true));

                    }


                }
                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent(" Generate Buildings ", "Enable/Disable Loading and Generating buildings from Vector File "), GUILayout.MaxWidth(200));
                    RuntimePrefs.EnableBuildingGeneration = EditorGUILayout.Toggle("", RuntimePrefs.EnableBuildingGeneration, GUILayout.ExpandWidth(true));
                }

            }else
            {
                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent(" Generat Tracks ", "Enable/Disable Loading and Generating Traks from GPX File "), GUILayout.MaxWidth(200));
                    RuntimePrefs.EnableRoadGeneration = EditorGUILayout.Toggle("", RuntimePrefs.EnableRoadGeneration, GUILayout.ExpandWidth(true));

                }

                if (RuntimePrefs.EnableRoadGeneration)
                {
                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent("  Generator Type ", "Select with whiche the Track will be Generated (Note that EasyRoad3D must be existing in the project "), GUILayout.MaxWidth(200));
                        RuntimePrefs.RoadType = (RoadGenerationType)EditorGUILayout.EnumPopup("", RuntimePrefs.RoadType, GUILayout.ExpandWidth(true));
                    }

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent("  Path Prefab ", ""), GUILayout.MaxWidth(200));
                        RuntimePrefs.PathPrefab = (GISTerrainLoaderSO_Road)EditorGUILayout.ObjectField(RuntimePrefs.PathPrefab, typeof(GISTerrainLoaderSO_Road), true, GUILayout.ExpandWidth(true));
                    }

                }

                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent(" Generat GeoLocation ", "Enable/Disable Loading and Generating GeoLocation Point from GPX File "), GUILayout.MaxWidth(200));
                    RuntimePrefs.EnableGeoLocationPointGeneration = EditorGUILayout.Toggle("", RuntimePrefs.EnableGeoLocationPointGeneration, GUILayout.ExpandWidth(true));

                }


                if (RuntimePrefs.EnableGeoLocationPointGeneration)
                {
                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent("  GeoPoint Prefab ", ""), GUILayout.MaxWidth(200));
                        RuntimePrefs.GeoPointPrefab = (GameObject)EditorGUILayout.ObjectField(RuntimePrefs.GeoPointPrefab, typeof(UnityEngine.Object), true, GUILayout.ExpandWidth(true));

                    }
 
                }
            }

        }
        private void OptionsTab()
        {

            GUIStyle buttonStyle = new GUIStyle(EditorStyles.toolbarButton);

            using (new HorizontalBlock())
            {
                if (GUILayout.Button(new GUIContent(" Reset", m_resetPrefs, " Reset all Prefs to default "), buttonStyle, GUILayout.ExpandWidth(false)))
                {
                    RuntimePrefs.ResetPrefs();
                }
            }

        }
        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            Undo.RecordObject(RuntimePrefs, "GTL_Runtime");
            tabs.Draw();
            if (GUI.changed)
                RuntimePrefs.lastTab = tabs.curMethodIndex;
            EditorUtility.SetDirty(RuntimePrefs);
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
        private void CoordinatesBarGUI()
        {
            using (new VerticalBlock())
            {
                using (new VerticalBlock())
                {
                    RuntimePrefs.ShowCoordinates = EditorGUILayout.Foldout(RuntimePrefs.ShowCoordinates, "Sub Region Coordinates");
                }

                if (RuntimePrefs.ShowCoordinates)
                {
                    EditorGUILayout.HelpBox(" Set Sub Region Heightmap coordinates ", MessageType.Info);

                    GUILayout.Label("Upper-Left : ", GUILayout.ExpandWidth(false));

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label("Latitude : ", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
                        GUI.SetNextControlName("UpperLeftCoordianteLat");
                        RuntimePrefs.SubRegionUpperLeftCoordiante.y = EditorGUILayout.DoubleField(RuntimePrefs.SubRegionUpperLeftCoordiante.y, GUILayout.ExpandWidth(true));

                        GUILayout.Label("Longitude : ", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
                        GUI.SetNextControlName("UpperLeftCoordianteLon");
                        RuntimePrefs.SubRegionUpperLeftCoordiante.x = EditorGUILayout.DoubleField(RuntimePrefs.SubRegionUpperLeftCoordiante.x, GUILayout.ExpandWidth(true));

                    }
                    GUILayout.Label("Down-Right : ", GUILayout.ExpandWidth(false));

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label("Latitude : ", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
                        GUI.SetNextControlName("DownRightCoordianteLat");
                        RuntimePrefs.SubRegionDownRightCoordiante.y = EditorGUILayout.DoubleField(RuntimePrefs.SubRegionDownRightCoordiante.y, GUILayout.ExpandWidth(true));

                        GUILayout.Label("Longitude : ", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
                        GUI.SetNextControlName("DownRightCoordianteLon");
                        RuntimePrefs.SubRegionDownRightCoordiante.x = EditorGUILayout.DoubleField(RuntimePrefs.SubRegionDownRightCoordiante.x, GUILayout.ExpandWidth(true));
                    }

                    GUILayout.Label("", GUILayout.ExpandWidth(false));

                }

            }
 
        }
    }
#endif
}