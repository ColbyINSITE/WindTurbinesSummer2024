/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace GISTech.GISTerrainLoader
{

    public class GISTerrainLoaderTextureGenerator
    {
        static Texture2D terrainTexture = null;
 
        public static bool generateComplete;
#if UNITY_EDITOR
        public static async Task EditorAddTextureToTerrain(string terrainPath, string ResFolderpath, TerrainObject terrainItem)
        {

#if UNITY_EDITOR
 
            var folderPath = Path.GetDirectoryName(terrainPath);
            var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
            var TextureFolder = Path.Combine(folderPath, TerrainFilename + "_Textures");
            ResourceRequest resourcesRequest;

            if (Directory.Exists(TextureFolder))
            {
                var Tiles = GetTextureTiles(terrainPath);

                var textureSource = GetTextureSource(Tiles);

                bool texExist;

                var texPath = EditorCheckForTexture(textureSource, ResFolderpath, terrainItem, out texExist);

                if (texExist)
                {
                    resourcesRequest = Resources.LoadAsync(texPath, typeof(Texture2D));

                    while (!resourcesRequest.isDone)
                        await Task.Delay(TimeSpan.FromSeconds(0.01));

                    terrainTexture = resourcesRequest.asset as Texture2D;
                }
                else
                {
                    terrainTexture = (Texture2D)Resources.Load("Textures/NullTexture");
                    Debug.Log("Texture not found : " + texPath);
                }

#if UNITY_2018_1_OR_NEWER
                TerrainLayer NewterrainLayer = new TerrainLayer();

                string path = Path.Combine(terrainItem.container.GeneratedTerrainfolder, terrainItem.name + ".terrainlayer");
                AssetDatabase.CreateAsset(NewterrainLayer, path);

                TerrainLayer[] ExistingTerrainLayers = terrainItem.terrainData.terrainLayers;
                List<TerrainLayer> NewLayers = new List<TerrainLayer>();

                foreach (var l in ExistingTerrainLayers)
                {
                    NewLayers.Add(l);
                }


                NewterrainLayer.diffuseTexture = (Texture2D)Resources.Load(texPath);

                NewterrainLayer.tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z);
                NewterrainLayer.tileOffset = Vector2.zero;

                NewLayers.Add(NewterrainLayer);
                terrainItem.terrainData.terrainLayers = NewLayers.ToArray();

#else

            SplatPrototype sp = new SplatPrototype
            {
                texture = (Texture2D)Resources.Load(texPath),
                tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z),
                tileOffset = Vector2.zero
            };
            terrain.terrainData.splatPrototypes = new[] { sp };

#endif

            }
            else
            {
                Debug.LogError("Texture Folder not exist .. ");
                generateComplete = true;
            }

#endif

        }

        private static string EditorCheckForTexture(TextureSource textureSource, string TexturePath, TerrainObject terrain, out bool exist)
        {
            string terrainTexture = "";
            string textureFilePath = "";

            exist = false;

            if (textureSource == TextureSource.Globalmapper)
            {
                textureFilePath = Path.Combine(TexturePath, "Tile__" + terrain.Number.x.ToString() + "__" + terrain.Number.y.ToString());

                if (Resources.Load(textureFilePath) as Texture2D)
                {
                    terrainTexture = textureFilePath;
                    exist = true;
                }
            }
            else
                if (textureSource == TextureSource.SASPlanet)
            {
                textureFilePath = Path.Combine(TexturePath, "Tile_" + (terrain.Number.x + 1).ToString() + "-" + (terrain.container.terrainCount.y - terrain.Number.y).ToString());

                if (Resources.Load(textureFilePath) as Texture2D)
                {
                    terrainTexture = textureFilePath;
                    exist = true;
                }
            }
            return terrainTexture;
        }
#endif
        public static void RuntimeAddTexturesToTerrain(string terrainPath, TerrainObject terrainItem, Vector2 texturedim)
        {
            var folderPath = Path.GetDirectoryName(terrainPath);
            var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
            var TextureFolder = Path.Combine(folderPath, TerrainFilename + "_Textures");
            terrainTexture = null;

            if (!Directory.Exists(TextureFolder))
            {
                terrainTexture = (Texture2D)Resources.Load("Textures/NullTexture");
                terrainItem.TextureState = TextureState.Loaded;
                Debug.LogError("Texture Folder not found !" );
            }
            else
            {
                var Tiles = GetTextureTiles(terrainPath);
                var textureSource = GetTextureSource(Tiles);
                var terrain = terrainItem.terrain;

                bool texExist;
                var texPath = RuntimeCheckForTexture(textureSource, terrainPath, terrainItem, out texExist);

                if (!texExist)
                {
                    terrainTexture = (Texture2D)Resources.Load("Textures/NullTexture");
                    Debug.LogError("Texture Tile not found : " + texPath);
                    terrainItem.TextureState = TextureState.Loaded;
                }else
                {
                    var textureWidth = (int)texturedim.x;
                    var textureHeight = (int)texturedim.y;

                    if (textureWidth <= 128 || textureHeight <= 128) return;
                    textureWidth /= 2;
                    textureHeight /= 2;

                    terrainTexture = new Texture2D(textureWidth, textureHeight);
                    terrainTexture = LoadedTextureTile(texPath);
                    terrainItem.TextureState = TextureState.Loaded;

                }
            }




#if UNITY_2018_1_OR_NEWER
            TerrainLayer NewterrainLayer = new TerrainLayer();

            TerrainLayer[] ExistingTerrainLayers = terrainItem.terrainData.terrainLayers;

            List<TerrainLayer> NewLayers = new List<TerrainLayer>();

            foreach (var l in ExistingTerrainLayers)
            {
                NewLayers.Add(l);
            }


            NewterrainLayer.diffuseTexture = terrainTexture;

            NewterrainLayer.tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z);
            NewterrainLayer.tileOffset = Vector2.zero;

            NewLayers.Add(NewterrainLayer);
            terrainItem.terrainData.terrainLayers = NewLayers.ToArray();


#else
            SplatPrototype sp = new SplatPrototype
            {
                texture = tex,
                tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z),
                tileOffset = Vector2.zero
            };
            terrain.terrainData.splatPrototypes = new[] { sp };
#endif
 
        }
        public static void RuntimeWebGLAddTexturesToTerrain(Texture2D texturePath, TerrainObject terrainItem)
        {

#if UNITY_2018_1_OR_NEWER
            TerrainLayer NewterrainLayer = new TerrainLayer();

            TerrainLayer[] ExistingTerrainLayers = terrainItem.terrainData.terrainLayers;

            List<TerrainLayer> NewLayers = new List<TerrainLayer>();

            foreach (var l in ExistingTerrainLayers)
            {
                NewLayers.Add(l);
            }


            NewterrainLayer.diffuseTexture = texturePath;

            NewterrainLayer.tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z);
            NewterrainLayer.tileOffset = Vector2.zero;

            NewLayers.Add(NewterrainLayer);
            terrainItem.terrainData.terrainLayers = NewLayers.ToArray();


#else
            SplatPrototype sp = new SplatPrototype
            {
                texture = tex,
                tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z),
                tileOffset = Vector2.zero
            };
            terrain.terrainData.splatPrototypes = new[] { sp };
#endif

        }
        private static string RuntimeCheckForTexture(TextureSource textureSource, string terrainPath, TerrainObject terrain, out bool exist)
        {
            string terrainTexture = "";
            string textureFilePath = "";
            exist = false;
            var folderPath = Path.GetDirectoryName(terrainPath);
            var TexturesFolder = Path.Combine(folderPath, Path.GetFileNameWithoutExtension(terrainPath) + "_Textures");
            var texturePath = TexturesFolder;


            if (textureSource == TextureSource.Globalmapper)
            {
                textureFilePath = Path.Combine(texturePath, "Tile__" + terrain.Number.x.ToString() + "__" + terrain.Number.y.ToString());
                if (File.Exists(textureFilePath + ".png"))
                {
                    texturePath = textureFilePath + ".png";
                    terrainTexture = texturePath;
                    exist = true;
                }
                else
                {
                    texturePath = textureFilePath + ".jpg";
                    terrainTexture = texturePath;
                    exist = true;
                }

            }
            else
    if (textureSource == TextureSource.SASPlanet)
            {
                textureFilePath = Path.Combine(texturePath, "Tile_" + (terrain.Number.x + 1).ToString() + "-" + (terrain.container.terrainCount.y - terrain.Number.y).ToString());

                if (File.Exists(textureFilePath + ".png"))
                {
                    texturePath = textureFilePath + ".png";
                    terrainTexture = texturePath;
                    exist = true;
                }
                else
                {
                    texturePath = textureFilePath + ".jpg";
                    terrainTexture = texturePath;
                    exist = true;
                }
            }

            return terrainTexture;
        }
        public static Texture2D LoadedTextureAsync(string texturePath)
        {
            Texture2D tex = new Texture2D(2, 2);

            if (File.Exists(texturePath))
            {
                tex.wrapMode = TextureWrapMode.Repeat;
                tex.LoadImage(File.ReadAllBytes(texturePath));
                tex.LoadImage(tex.EncodeToJPG(100));
            }
            return tex;
        }
        public static Texture2D LoadedTextureTile(string TexturePath)
        {
            Texture2D tex = new Texture2D(2, 2);

            if (File.Exists(TexturePath))
            {
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.LoadImage(File.ReadAllBytes(TexturePath));
                tex.LoadImage(tex.EncodeToJPG(100));
            }
            return tex;
        }
        public static string[] GetTextureTiles(string terrainPath)
        {
            var folderPath = Path.GetDirectoryName(terrainPath);
            var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
            var TextureFolder = Path.Combine(folderPath, TerrainFilename + "_Textures");
            string[] tiles = null;

            if (Directory.Exists(TextureFolder))
            {
                var supportedExtensions = new HashSet<string> { ".png", ".jpg" };
                tiles = Directory.GetFiles(TextureFolder, "*.*", SearchOption.AllDirectories).Where(f => supportedExtensions.Contains(new FileInfo(f).Extension, StringComparer.OrdinalIgnoreCase)).ToArray();


            }

            return tiles;
        }
        public static TextureSource GetTextureSource(string[] Tiles)
        {
            var textureSource = TextureSource.Globalmapper;

            Dictionary<TextureSource, string> TexturesSources = new Dictionary<TextureSource, string>();

            //Load Texture Source Dic

            TextAsset Dic = (TextAsset)Resources.Load("TextureSourceDic/TextureSourceDic", typeof(TextAsset));

            if (Dic != null)
            {
                string[] lines = Dic.text.Split('\n');

                for (int i = 0; i < lines.Length; i++)
                {
                    if (!string.IsNullOrEmpty(lines[i]))
                    {
                        var lineData = lines[i].Split(':');

                        var source = lineData[0].Replace(" ", "");
                        var format = lineData[1].Trim();

                        switch (source)
                        {
                            case "Globalmapper":
                                TexturesSources.Add(TextureSource.Globalmapper, format);
                                break;
                            case "SASPlanet":
                                TexturesSources.Add(TextureSource.SASPlanet, format);
                                break;
                        }

                    }
                }
            }

            if (Tiles.Count() > 0)
            {
                var TileFormat = Path.GetFileNameWithoutExtension(Tiles[0]);

                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {

                        foreach (var Tsource in TexturesSources)
                        {
                            var format = string.Format(Tsource.Value.Trim(), x, y);

                            if (TileFormat.Equals(format))
                            {

                                textureSource = Tsource.Key;
                            }

                        }
                    }
                }


            }
            return textureSource;
        }
        public static void GetTilesNumberInTextureFolder(string terrainPath, out Vector2 Tiles)
        {
            Tiles = Vector2.zero;

            var folderPath = Path.GetDirectoryName(terrainPath);
            var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
            var TextureFolder = Path.Combine(folderPath, TerrainFilename + "_Textures");

            if (Directory.Exists(TextureFolder))
            {
                var supportedExtensions = new HashSet<string> { ".png", ".jpg" };
                var tiles = Directory.GetFiles(TextureFolder, "*.*", SearchOption.AllDirectories).Where(f => supportedExtensions.Contains(new FileInfo(f).Extension, StringComparer.OrdinalIgnoreCase)).ToArray();
                var tilestotalcount = tiles.Length;

                var textureSource = GetTextureSource(tiles);

                if (textureSource == TextureSource.Globalmapper)
                {
                    int xtilecount = 1; int ytilecount = 1;

                    for (int i = 0; i < tilestotalcount; i++)
                    {

                        var fileName = Path.GetFileNameWithoutExtension(tiles[i]);

                        if (IsCorrectFormat(textureSource, fileName))
                        {
                            var S1 = fileName.Replace("Tile__", "").Replace("__", "|").Split('|');
                            var x = int.Parse(S1[0])+1;
                            var y = int.Parse(S1[1])+1;

                            if (x > xtilecount)
                                xtilecount = x;

                            if (y > ytilecount)
                                ytilecount = y;

                        }else
                        {
                            Debug.LogError("Tiles Name not set correctly : Rename it to 'Tile__x__y' " + tiles[i]);
                        }

                    }
                    Tiles = new Vector2(xtilecount, ytilecount);
                }
                else
                  if (textureSource == TextureSource.SASPlanet)
                {
                    int xtilecount = 0; int ytilecount = 0;

                    for (int i = 0; i < tilestotalcount; i++)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(tiles[i]);

                        if (IsCorrectFormat(textureSource, fileName))
                        {
                            var S1 = fileName.Replace("Tile_", "").Split('-');
                            var x = int.Parse(S1[0]);
                            var y = int.Parse(S1[1]);

                            if (x > xtilecount)
                                xtilecount = x;

                            if (y > ytilecount)
                                ytilecount = y;

                        }
                        else
                        {
                            Debug.LogError("Tiles Name not set correctly : Rename it to 'Tile_x-y' " + tiles[i]);
                        }

                    }

                    Tiles = new Vector2(xtilecount, ytilecount);
                }


            }

        }
        private static bool IsCorrectFormat(TextureSource textureSource,string tilename)
        {
            bool correct = false;

            var pattern = @"\((?<AreaCode>\d{3})\)\s*(?<Number>\d{3}(?:-|\s*)\d{4})";
            var regexp = new System.Text.RegularExpressions.Regex(pattern);

            switch (textureSource)
            { 
                case TextureSource.Globalmapper:
                    correct = Regex.IsMatch(tilename, @"^Tile__\d*__\d*");
                    break;  
                case TextureSource.SASPlanet:
                    correct = Regex.IsMatch(tilename, @"^Tile_\d*-\d*");
                    break;
            }
            return correct;
 
           
        }
        public static Vector2Int GetTextureRealWidthAndHeightEditor(Texture2D tex, string texturepath)
        {
            int width = 0; int height = 0;

            if (Application.isPlaying)
            {
                width = tex.width;
                height = tex.height;
            }
            else
            {

#if UNITY_EDITOR
                TextureImporter textureImporter = AssetImporter.GetAtPath(texturepath) as TextureImporter;
                System.Type type = typeof(TextureImporter);
                System.Reflection.MethodInfo method = type.GetMethod("GetWidthAndHeight", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var args = new object[] { width, height };
                method.Invoke(textureImporter, args);
                width = (int)args[0];
                height = (int)args[1];
#endif
            }


            return new Vector2Int(width, height);
        }
        public static Texture2D[] GetTextureInFolder_Editor(string terrainPath)
        {

            var folderPath = Path.GetDirectoryName(terrainPath);
            var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
            var TextureFolder = Path.Combine(folderPath, TerrainFilename + "_Textures");

            string[] tiles = null;

            List<Texture2D> TextureTiles = new List<Texture2D>();

            if (Directory.Exists(TextureFolder))
            {
                var supportedExtensions = new HashSet<string> { ".png", ".jpg" };
                tiles = Directory.GetFiles(TextureFolder, "*.*", SearchOption.AllDirectories).Where(f => supportedExtensions.Contains(new FileInfo(f).Extension, StringComparer.OrdinalIgnoreCase)).ToArray();

                foreach (var tile in tiles)
                {

                    TextureTiles.Add(LoadedTextureTile(tile));

                }
            }
            return TextureTiles.ToArray();
        }
        public static void CombienTerrainTextures(string TerrainPath)
        {
            var TerrainFileName = Path.GetFileName(TerrainPath).Split('.')[0];
            var TextureFolder = Path.Combine(Path.GetDirectoryName(TerrainPath), Path.GetFileNameWithoutExtension(TerrainPath) + "_Textures");

            Vector2Int MaxTextureSize = new Vector2Int(0, 0);

            Vector2 TerrainsCount = new Vector2(0, 0);

            GetTilesNumberInTextureFolder(TerrainPath, out TerrainsCount);

            var listAccount = TerrainsCount.x * TerrainsCount.y;

            var texturesPath = GetTextureTiles(TerrainPath);

            var textures = GetTextureInFolder_Editor(TerrainPath);

            List<Vector2Int> offests = new List<Vector2Int>();
            List<Vector2Int> RealSizes = new List<Vector2Int>();

            int cx = -1;
            int cy = -1;
            for (int i = 0; i < listAccount; i = i + (int)TerrainsCount.x)
            {
                cx++;
                for (int j = i; j < i + TerrainsCount.y; j++)
                {
                    cy++;
                    var tileNumber = new Vector2Int(cx, cy);

                    var Texture = textures[j];

                    var TexturePath = "Assets" + texturesPath[j].Replace(Application.dataPath.Replace('/', '\\'), "");

                    var RealSize = GetTextureRealWidthAndHeightEditor(Texture, TexturePath);

                    if (tileNumber.x == 0)
                    {
                        MaxTextureSize.x += RealSize.x;
                    }
                    if (tileNumber.y == 0)
                    {
                        MaxTextureSize.y += RealSize.y;
                    }

                    var offest = new Vector2Int(RealSize.x * tileNumber.x, RealSize.y * tileNumber.y);
                    offests.Add(offest);

                    RealSizes.Add(RealSize);

                }
                cy = -1;
            }

            if (Directory.Exists(TextureFolder))
                Directory.Move(TextureFolder, TextureFolder + "_Original_" + UnityEngine.Random.Range(501, 1000));

            Directory.CreateDirectory(TextureFolder);

            if(!Application.isPlaying && !Application.isEditor)
            {
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }


            Texture2D Fileout = new Texture2D(MaxTextureSize.x, MaxTextureSize.y, TextureFormat.RGBA32, true);

            for (int s = 0; s < textures.Length; s++)
            {
                var tex = textures[s];
                var Rsize = RealSizes[s];
                var off = offests[s];
                var width = Rsize.x;
                var height = Rsize.y;
                RenderTexture tmp = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
                Graphics.Blit(tex, tmp);
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = tmp;
                Texture2D newFile = new Texture2D(width, height);
                newFile.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                newFile.Apply();
                Fileout.SetPixels(off.x, off.y, width, height, newFile.GetPixels());
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(tmp);
            }
            File.WriteAllBytes(TextureFolder + "/Tile__0__0.jpg", Fileout.EncodeToJPG());
        }
        private static async void SaveToFile(string savepath, byte[] result)
        {

            using (FileStream SourceStream = File.Open(savepath, FileMode.OpenOrCreate))
            {
                SourceStream.Seek(0, SeekOrigin.End);
                await SourceStream.WriteAsync(result, 0, result.Length);
            }
        }
        public static async Task SplitTex(string TerrainPath, Vector2 SplitCount)
        {

            int TileSize_w = 0; int TileSize_h = 0;

            Vector2Int MaxTextureSize = new Vector2Int(0, 0);

            Vector2 TerrainsCount = new Vector2(0, 0);

            GetTilesNumberInTextureFolder(TerrainPath, out TerrainsCount);

            var listAccount = TerrainsCount.x * TerrainsCount.y;

            var texturesPath = GetTextureTiles(TerrainPath);

            var textures = GetTextureInFolder_Editor(TerrainPath);

            List<Vector2Int> offests = new List<Vector2Int>();
            List<Vector2Int> RealSizes = new List<Vector2Int>();

            int cx = -1;
            int cy = -1;


            for (int i = 0; i < listAccount; i = i + (int)TerrainsCount.x)
            {
                cx++;
                for (int j = i; j < i + TerrainsCount.y; j++)
                {
                    cy++;
                    var tileNumber = new Vector2Int(cx, cy);

                    var Texture = textures[j];

                    var TexturePath = "Assets" + texturesPath[j].Replace(Application.dataPath.Replace('/', '\\'), "");

                    var RealSize = GetTextureRealWidthAndHeightEditor(Texture, TexturePath);

                    if (tileNumber.x == 0)
                    {
                        MaxTextureSize.x += RealSize.x;
                    }
                    if (tileNumber.y == 0)
                    {
                        MaxTextureSize.y += RealSize.y;
                    }

                    var offest = new Vector2Int(RealSize.x * tileNumber.x, RealSize.y * tileNumber.y);
                    offests.Add(offest);

                    RealSizes.Add(RealSize);

                }
                cy = -1;
            }

            TileSize_w = (int)(MaxTextureSize.x / SplitCount.x);
            TileSize_h = (int)(MaxTextureSize.y / SplitCount.y);

            //Case of one texture ----> Split Directly

            if (textures.Length == 1)
            {
                await Split(TerrainPath, textures[0], MaxTextureSize, new Vector2(TileSize_w, TileSize_h), SplitCount);
            }

        }
        private static async Task Split(string SavePath, Texture2D Maintex, Vector2 mainTexSize, Vector2 tileSize, Vector2 TerrainsCount)
        {
            var TerrainFileName = Path.GetFileName(SavePath).Split('.')[0];
            var TextureFolder = Path.Combine(Path.GetDirectoryName(SavePath), Path.GetFileNameWithoutExtension(SavePath) + "_Textures");

            if (Directory.Exists(TextureFolder))
                Directory.Move(TextureFolder, TextureFolder + "_Original_" + UnityEngine.Random.Range(0, 500));

            Directory.CreateDirectory(TextureFolder);

            if (!Application.isPlaying && !Application.isEditor)
            {
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }

            int w = (int)tileSize.x;
            int h = (int)tileSize.y;

            var pixels = Maintex.GetPixels();

            int cx = -1;
            int cy = -1;

            var listAccount = TerrainsCount.x * TerrainsCount.y;

            for (int i = 0; i < listAccount; i = i + (int)TerrainsCount.x)
            {
                cx++;
                for (int j = i; j < i + TerrainsCount.y; j++)
                {
                    cy++;

                    var tileNumber = new Vector2Int(cx, cy);

                    var offest = new Vector2Int(w * tileNumber.x, h * tileNumber.y);

                    Texture2D tmp = new Texture2D(w, h, TextureFormat.RGBA32, false);

                    for (int iw = offest.x; iw < offest.x + w; iw++)
                    {
                        for (int ih = offest.y; ih < offest.y + h; ih++)
                        {
                            var pix = Maintex.GetPixel(iw, ih);
                            tmp.SetPixel(iw, ih, pix);
                        }
                    }

                    string filePath = TextureFolder + "/Tile__" + cx + "__" + cy + ".jpg";

                    FileStream SourceStream = null;
                    var buffer = tmp.EncodeToJPG();

                    try

                    {
                        using (SourceStream = File.Open(filePath, FileMode.OpenOrCreate))
                        {
                            var t = tmp.EncodeToJPG();
                            SourceStream.Seek(0, SeekOrigin.End);

                            SourceStream.WriteAsync(t, 0, t.Length).Wait();
                        }
                    }
                    catch
                    {
                        Debug.Log("Error");
                    }
                    finally
                    {

                        if (SourceStream != null)

                            SourceStream.Dispose();

                    }
                }
                cy = -1;
            }

            await Task.Delay(1);
        }

    }

}
