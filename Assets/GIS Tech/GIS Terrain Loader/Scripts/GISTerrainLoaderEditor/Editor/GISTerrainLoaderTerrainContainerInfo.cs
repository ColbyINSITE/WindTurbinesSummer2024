/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR 
using UnityEditor;
#endif
namespace GISTech.GISTerrainLoader
{

#if UNITY_EDITOR 
    [CustomEditor(typeof(TerrainContainerObject))]
    public class GISTerrainLoaderTerrainContainerInfo : Editor
    {
        private TabsBlock tabs;
        private Texture2D m_resetPrefs;

        public string[] availableHeightSrt = new string[] { "32", "64", "128", "256", "512", "1024", "2048", "4096" };
        public string[] availableHeightsResolutionPrePectSrt = new string[] { "4", "8", "16", "32" };
        public string[] availableExportFiles = new string[] { "Raw"};
        private ExportType exportType = ExportType.Raw;

        private RawDepth depth = RawDepth.Bit16;
        private RawByteOrder order = RawByteOrder.Windows;
        private string path;
        private string extension;

        private ExportAs exportAs = ExportAs.Png;
        private TerrainContainerObject ContainerObjectInfo { get { return target as TerrainContainerObject; } }
        private void OnEnable()
        {
            tabs = new TabsBlock(new Dictionary<string, System.Action>()
            {
                {"Terrain Metadata", TerrainMetadata},
                {"Terrain Parameters", TerrainParameters},
                   {"Export ", Export}
            });

            tabs.SetCurrentMethod(ContainerObjectInfo.lastTab);
        }
        private void TerrainMetadata()
        {
            using (new HorizontalBlock())
            {
                CoordinatesBarGUI();
            }

        }
        private void TerrainParameters()
        {
            using (new HorizontalBlock())
            {
                TerrainParametersGUI();
            }

        }
        private void Export()
        {
            using (new HorizontalBlock())
            {
                ExportGUI();
            }

        }
        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            Undo.RecordObject(ContainerObjectInfo, "GTL_TerrainContainerInfo");
            tabs.Draw();
            if (GUI.changed)
                ContainerObjectInfo.lastTab = tabs.curMethodIndex;
            EditorUtility.SetDirty(ContainerObjectInfo);
        }
        private void CoordinatesBarGUI()
        {
            using (new VerticalBlock(GUI.skin.box))
            {
                GUILayout.Label("Terrain Coordinates [Geographic Lat/Lon] ");

                using (new HorizontalBlock(GUI.skin.button))
                {
                    GUILayout.Label(" Upper-Left :    ");
                    GUILayout.Label("");
                    GUILayout.Label("  Latitude : ");
                    GUI.SetNextControlName("UpperLeftCoordianteLat");
                    GUILayout.Label(Math.Round(ContainerObjectInfo.TopLeftLatLong.y, 10).ToString());
                    GUI.SetNextControlName("UpperLeftCoordianteLon");

                    GUILayout.Label("  Longitude : ");
                    GUI.SetNextControlName("UpperLeftCoordianteLon");
                    GUILayout.Label(Math.Round(ContainerObjectInfo.TopLeftLatLong.x, 10).ToString());
                    GUI.SetNextControlName("UpperLeftCoordianteLon");

                }


                using (new HorizontalBlock(GUI.skin.button))
                {
                    GUILayout.Label(" Bottom-Right : ");
                    GUILayout.Label("", GUILayout.ExpandWidth(true));
                    GUILayout.Label("  Latitude : ");
                    GUI.SetNextControlName("UpperLeftCoordianteLat");
                    GUILayout.Label(Math.Round(ContainerObjectInfo.DownRightLatLong.y, 10).ToString());
                    GUI.SetNextControlName("UpperLeftCoordianteLon");

                    GUILayout.Label("  Longitude : ");
                    GUI.SetNextControlName("UpperLeftCoordianteLon");
                    GUILayout.Label(Math.Round(ContainerObjectInfo.DownRightLatLong.x, 10).ToString());
                    GUI.SetNextControlName("UpperLeftCoordianteLon");

                }

                GUILayout.Label("Terrain Dimension [Km] ");

                using (new HorizontalBlock(GUI.skin.button))
                {
                    GUILayout.Label("  Width :  ");
                    GUILayout.Label(Math.Round(ContainerObjectInfo.Dimensions.x, 2).ToString());

                    GUILayout.Label("  Lenght : ");
                    GUILayout.Label(Math.Round(ContainerObjectInfo.Dimensions.y, 2).ToString());

                }
                GUILayout.Label("Min Max Elevation [m] ");

                using (new HorizontalBlock(GUI.skin.button))
                {
                    GUILayout.Label("  Min :  ");
                    GUILayout.Label(Math.Round(ContainerObjectInfo.MinMaxElevation.x, 2).ToString());

                    GUILayout.Label("  Max :  ");
                    GUILayout.Label(Math.Round(ContainerObjectInfo.MinMaxElevation.y, 2).ToString());

                }

                GUILayout.Label("Terrain Scale Factor ");

                using (new HorizontalBlock(GUI.skin.button))
                {
                    GUILayout.Label("  X :  ");
                    GUILayout.Label(ContainerObjectInfo.scale.x.ToString());

                    GUILayout.Label("  Y : ");
                    GUILayout.Label(ContainerObjectInfo.scale.y.ToString());

                    GUILayout.Label("  Z : ");
                    GUILayout.Label(ContainerObjectInfo.scale.z.ToString());

                }

                GUILayout.Label("Terrain Total Size [Terrain Unite] ");

                using (new HorizontalBlock(GUI.skin.button))
                {
                    GUILayout.Label("  X :  ");
                    GUILayout.Label(ContainerObjectInfo.ContainerSize.x.ToString());

                    GUILayout.Label("  Y : ");
                    GUILayout.Label(ContainerObjectInfo.ContainerSize.y.ToString());

                    GUILayout.Label("  Z : ");
                    GUILayout.Label(ContainerObjectInfo.ContainerSize.z.ToString());

                }

                GUILayout.Label("Terrains Count ");

                using (new HorizontalBlock(GUI.skin.button))
                {
                    GUILayout.Label("  X :  ");
                    GUILayout.Label(ContainerObjectInfo.terrainCount.x.ToString());

                    GUILayout.Label("  Y : ");
                    GUILayout.Label(ContainerObjectInfo.terrainCount.y.ToString());

                    GUILayout.Label("      ");
                    GUILayout.Label(" ".ToString());

                }
            }
        }
        private void TerrainParametersGUI()
        {
            using (new VerticalBlock(GUI.skin.box))
            {
                using (new VerticalBlock(GUI.skin.box))
                {
                    GUILayout.Label("Terrain Base prefs ");

                    using (new VerticalBlock(GUI.skin.box))
                    {
                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent(" Pixel Error ", " The accuracy of the mapping between Terrain maps (such as heightmaps and Textures) and generated Terrain. Higher values indicate lower accuracy, but with lower rendering overhead. "), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.PixelErro = EditorGUILayout.Slider(ContainerObjectInfo.PixelErro, 1, 200, GUILayout.ExpandWidth(true));
                        }

                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent(" Base Map Dis ", " The maximum distance at which Unity displays Terrain Textures at full resolution. Beyond this distance, the system uses a lower resolution composite image for efficiency "), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.BaseMapDistance = EditorGUILayout.Slider(ContainerObjectInfo.BaseMapDistance, 1, 20000, GUILayout.ExpandWidth(true));
                        }

                    }
                }

                using (new VerticalBlock(GUI.skin.box))
                {
                    GUILayout.Label("Tree & Details objects ");

                    using (new VerticalBlock(GUI.skin.box))
                    {
                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent("  Detail Distance ", " The distance from the camera beyond which details are culled "), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.DetailDistance = EditorGUILayout.Slider(ContainerObjectInfo.DetailDistance, 10f, 400, GUILayout.ExpandWidth(true));
                        }

                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent("  Detail Density ", " The number of detail/grass objects in a given unit of area. Set this value lower to reduce rendering overhead "), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.DetailDensity = EditorGUILayout.Slider(ContainerObjectInfo.DetailDensity, 0, 1, GUILayout.ExpandWidth(true));
                        }
                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent("  Tree Distance ", " The distance from the camera beyond which trees are culled "), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.TreeDistance = EditorGUILayout.Slider(ContainerObjectInfo.TreeDistance, 1, 5000, GUILayout.ExpandWidth(true));
                        }
                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent("  Tree BillBoard Start Distance ", "The distance from the camera at which Billboard images replace 3D Tree objects"), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.BillBoardStartDistance = EditorGUILayout.Slider(ContainerObjectInfo.BillBoardStartDistance, 1, 2000, GUILayout.ExpandWidth(true));
                        }
                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent("  Fade Length ", "The distance over which Trees transition between 3D objects and Billboards."), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.FadeLength = EditorGUILayout.Slider(ContainerObjectInfo.FadeLength, 1, 200, GUILayout.ExpandWidth(true));
                        }
                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent(" Detail Resolution ", "The number of cells available for placing details onto the Terrain tile used to controls grass and detail meshes. Lower you set this number performance will be better"), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.DetailResolution_index = EditorGUILayout.Popup(ContainerObjectInfo.DetailResolution_index, availableHeightSrt, GUILayout.ExpandWidth(true));
                        }

                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent(" Resolution Per Patch ", "The number of cells in a single patch (mesh), recommended value is 16 for very large detail object distance "), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.ResolutionPerPatch_index = EditorGUILayout.Popup(ContainerObjectInfo.ResolutionPerPatch_index, availableHeightsResolutionPrePectSrt, GUILayout.ExpandWidth(true));
                        }

                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent(" Base Map Resolution ", "Resolution of the composite texture used on the terrain when viewed from a distance greater than the Basemap Distance"), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.BaseMapResolution_index = EditorGUILayout.Popup(ContainerObjectInfo.BaseMapResolution_index, availableHeightSrt, GUILayout.ExpandWidth(true));
                        }

                    }
                }
            }
        }
        private void ExportGUI()
        {
            using (new VerticalBlock(GUI.skin.box))
            {
                using (new VerticalBlock(GUI.skin.box))
                {
                    GUILayout.Label(" Export To File ");

                    using (new VerticalBlock(GUI.skin.box))
                    {
                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent(" Export To ", " Output file type "), GUILayout.MaxWidth(200));
                            exportType = (ExportType)EditorGUILayout.EnumPopup("", exportType);
                        }

                        if(exportType == ExportType.Raw)
                        {
                            extension = "raw";
                            using (new HorizontalBlock(GUI.skin.button))
                            {
                                GUILayout.Label(new GUIContent(" Depth ", "  "), GUILayout.MaxWidth(200));
                                depth = (RawDepth)EditorGUILayout.EnumPopup("", depth);
                            }
                            using (new HorizontalBlock(GUI.skin.button))
                            {
                                GUILayout.Label(new GUIContent(" ByteOrder ", " Output file type "), GUILayout.MaxWidth(200));
                                order = (RawByteOrder)EditorGUILayout.EnumPopup("", order);
                            }
                        }

                        if (exportType == ExportType.Png)
                        {
                            extension = "png";

                            using (new HorizontalBlock(GUI.skin.button))
                            {
                                GUILayout.Label(new GUIContent(" Save File As ", " PNG/JPG "), GUILayout.MaxWidth(200));
                                exportAs = (ExportAs)EditorGUILayout.EnumPopup("", exportAs);
                            }
 
                        }
                    }
                    using (new VerticalBlock(GUI.skin.box))
                    {
                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent(" Path ", " Selete a location to save the exported file "), GUILayout.MaxWidth(200));
                            path =   EditorGUILayout.TextField("", path);

                            if (GUILayout.Button(new GUIContent(" Select Location ", "Save heightmap to specific file by opening dialoge"),  GUILayout.ExpandWidth(false)))
                            {
                                path = EditorUtility.SaveFilePanel("Export Heightmap", Application.dataPath, "heightmap." + extension, extension);
                                if (string.IsNullOrEmpty(path)) return;
                            }
                        }
                        using (new HorizontalBlock(GUI.skin.box))
                        {
                            if (GUILayout.Button(new GUIContent(" Export ", "Click to export file"), GUILayout.ExpandWidth(true)))
                            {
                                if (string.IsNullOrEmpty(path)) return;

                                switch(extension)
                                {
                                    case "raw":

                                        GISTerrainLoaderRawExporter RawExporter = new GISTerrainLoaderRawExporter(path, depth, order, ContainerObjectInfo);
                                        RawExporter.ExportToRaw();
                                        break;

                                    case "png":

                                        GISTerrainLoaderPngExporter PngExporter = new GISTerrainLoaderPngExporter(path, ContainerObjectInfo,exportAs);
                                        PngExporter.ExportToPng();
                                        break;
                                }
 
                            }
                        }
                    }

                }

            }
        }
    }

#endif
}