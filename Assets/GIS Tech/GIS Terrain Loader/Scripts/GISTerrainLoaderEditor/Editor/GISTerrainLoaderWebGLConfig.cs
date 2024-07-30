/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderWebGLConfig : EditorWindow
    {
        public string TerrinFilePath = "";

        public string[] SupportedDEMFiles = new string[] { "tif" };

        [MenuItem("Tools/GIS Tech/GIS Terrain Loader/WebGL Config", false, 4)]
        public static void Init()
        {
            GISTerrainLoaderWebGLConfig window = GetWindow<GISTerrainLoaderWebGLConfig>(true, "WebGL Config", true);
            window.minSize = new Vector2(600, 300);
            window.maxSize = new Vector2(600, 300);
        }
        public void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(""), GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.Height(10));
            GUILayout.EndHorizontal();

            EditorGUILayout.HelpBox("Select DEM file location and click on Generate WebGL Config", MessageType.Info);

            if (GUILayout.Button(" Select Location ", GUI.skin.button))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(" Download Path ", " Select the location where data will be downloaded "), GUILayout.MaxWidth(200));
                TerrinFilePath = EditorUtility.OpenFilePanelWithFilters("Open DEM ", "", new[] { "GeoTiff ", "tif"});
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(" Terrain File ", " Select DEM File "), GUILayout.MaxWidth(200));
            TerrinFilePath = EditorGUILayout.TextField("", TerrinFilePath);
            GUILayout.EndHorizontal();

            if (GUILayout.Button(" Generate Config ", GUI.skin.button))
            {
                GenerateConfig();
            }

        }
        private void GenerateConfig()
        {
            if (!string.IsNullOrEmpty(TerrinFilePath) && File.Exists(TerrinFilePath))
            {
                var Ext = Path.GetExtension(TerrinFilePath).Replace(".","");

                if (IsSupportedDEM(Ext))
                {
                    WriteTerrainData(TerrinFilePath);
                }
                else
                Debug.LogError("DEM still not supported in WEBGL ..");
            }
            else
                Debug.LogError("File not found .. !");
        }
        public bool IsSupportedDEM(string ext)
        {
            bool valid = false;

            if (SupportedDEMFiles.Contains(ext))
            {
                valid = true;
            }
            return valid;
        }
        public void  WriteTerrainData(string terrinFilePath)
        {


            var DirParts = terrinFilePath.Replace('/', Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);

            var RelativePath = "";

            for (int i = DirParts.Length-1; i >= 0; i--)
            {
                var part = DirParts[i];  

                RelativePath  = part + Path.DirectorySeparatorChar+ RelativePath  ;

                if (part == "GIS Terrains") break;

                if (i <= 2)
                {
                    Debug.LogError("Texture folder not found! : Please put your terrain in GIS Terrain Loader/Recources/GIS Terrains/");

                    return;

                }

            }

            RelativePath = RelativePath.Split('.')[0];

            var Tiles_count = new Vector2(0, 0);
            GISTerrainLoaderTextureGenerator.GetTilesNumberInTextureFolder(terrinFilePath, out Tiles_count);
            int TextureFolderExist = 0;

            List<string> textures = new List<string>();

            if(Tiles_count.x>0 && Tiles_count.y > 0)
            {
                 var TextureFolder = Path.Combine(Path.GetDirectoryName(terrinFilePath), Path.GetFileNameWithoutExtension(terrinFilePath) + "_Textures");

                if (Directory.Exists(TextureFolder))
                {
                    TextureFolderExist = 1;
                    var supportedExtensions = new HashSet<string> {".png", ".jpg"};
                    textures = Directory.GetFiles(TextureFolder, "*.*", SearchOption.AllDirectories).Where(f => supportedExtensions.Contains(new FileInfo(f).Extension, StringComparer.OrdinalIgnoreCase)).ToList();
                }
            }

            int VectorFolderExist = 0;
            List<string> vectors = new List<string>();
            var VectorFolder = Path.Combine(Path.GetDirectoryName(terrinFilePath), Path.GetFileNameWithoutExtension(terrinFilePath) + "_VectorData");
            if (Directory.Exists(VectorFolder))
            {
                VectorFolderExist = 1;
                var supportedExtensions = new HashSet<string> { ".osm", ".shp" };
                vectors = Directory.GetFiles(VectorFolder, "*.*", SearchOption.AllDirectories).Where(f => supportedExtensions.Contains(new FileInfo(f).Extension, StringComparer.OrdinalIgnoreCase)).ToList();
            }

            var WebGLData = "";
 
            WebGLData += "Tiles_Count_x  =" + Tiles_count.x.ToString() + "|";
            WebGLData += "Tiles_Count_y  =" + Tiles_count.y.ToString() + "|";
            WebGLData += "MainFolder  =" +  RelativePath + "|";
            WebGLData += "TextureFolder  =" + TextureFolderExist.ToString() + "|";

            if (TextureFolderExist==1 && Tiles_count.x > 0 && Tiles_count.y > 0)
            {
                foreach(var tex in textures)
                {
                    var TParts = tex.Split(Path.DirectorySeparatorChar);
                    var TileName = TParts[TParts.Length - 1];
                    WebGLData += "Texture_Tile  =" + TileName + "|";
                }
            }

            WebGLData += "VectorFolder  =" + VectorFolderExist.ToString() + "|";

            if (VectorFolderExist==1 && vectors.Count>0)
            {
                foreach (var vector in vectors)
                {
                    var TParts = vector.Split(Path.DirectorySeparatorChar);
                    var TileName = TParts[TParts.Length - 1];
                    WebGLData += "Vector_Tile  =" + TileName + "|";
                 }
            }

            var SavePath = Path.Combine(Path.GetDirectoryName(terrinFilePath), "WebGL_Data.webgl");
            if (File.Exists(SavePath)) File.Delete(SavePath);

            using (StreamWriter file = new StreamWriter(SavePath))
            {
                file.Write(WebGLData);
            }

        }


    }

  
}
