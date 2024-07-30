/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderTerrainShader
    {
        private static Texture2D Main_Gradien_Pos;
        private static Texture2D Negative_Gradien_Pos;
        private static Texture2D Color_Gradien;

        private static Vector2Int TextureSize;
        private static float Terrain_Size_Y;

        private static float[,] data;
        private static Texture2D ShadedTexture;

        public static async Task GenerateShadedTextureEditor(ShaderType shaderType, OptionEnabDisab UnderWater, TerrainObject item, Vector2Int m_TextureSize, bool colored, OptionEnabDisab saveShaderTextures, string RuntimePath)
        {
            TextureSize = m_TextureSize;
            bool color = true;
            bool invers = false;

            if (shaderType == ShaderType.Slop || shaderType == ShaderType.ElevationGrayScale || shaderType == ShaderType.ElevationInversGrayScale)
                color = false;
            if (shaderType == ShaderType.SlopInvers || shaderType == ShaderType.ElevationInversGrayScale)
            {
                color = false; invers = true;
            }
                

             GenerateBaseShaders(color, invers);

            data = item.terrainData.GetHeights(0, 0, TextureSize.x, TextureSize.x);

            if (UnderWater == OptionEnabDisab.Enable)
            {
                if (item.container.MinMaxElevation.y < 0)
                    Terrain_Size_Y = item.container.ContainerSize.y * -1;

            }
            else
                Terrain_Size_Y = item.container.ContainerSize.y;

            ShadedTexture = new Texture2D(TextureSize.x, TextureSize.y, TextureFormat.RGB24, false);

            switch (shaderType)
            {
                case ShaderType.ColorRamp:
                    GenerateElevationShader(item, UnderWater);
                    break;
                case ShaderType.ElevationGrayScale:
                    GenerateElevationShader(item, UnderWater);
                    break;
                case ShaderType.ElevationInversGrayScale:
                    GenerateElevationShader(item, UnderWater);
                    break;
                case ShaderType.Slop:
                    GenerateSlopShader(item);
                    break;
                case ShaderType.SlopInvers:
                    GenerateSlopShader(item);
                    break;
                case ShaderType.NormalMap:
                    GenerateNormalMapShader(item);
                    break;
            }


            ShadedTexture.Apply();

            await SaveShadersAsTexturesAsync(item, RuntimePath);
 
            item.TextureState = TextureState.Loaded;
        }
        public static void GenerateShadedTextureRuntime(ShaderType shaderType, OptionEnabDisab UnderWater, TerrainObject item, Vector2Int m_TextureSize, bool colored, OptionEnabDisab saveShaderTextures, string RuntimePath = "")
        {
            TextureSize = m_TextureSize;

            bool color = true;
            bool invers = false;

            if (shaderType == ShaderType.Slop || shaderType == ShaderType.ElevationGrayScale || shaderType == ShaderType.ElevationInversGrayScale)
                color = false;
            if (shaderType == ShaderType.SlopInvers || shaderType == ShaderType.ElevationInversGrayScale)
            {
                color = false; invers = true;
            }

            GenerateBaseShaders(color, invers);


            data = item.terrainData.GetHeights(0, 0, TextureSize.x, TextureSize.x);

            if (UnderWater == OptionEnabDisab.Enable)
            {
                if (item.container.MinMaxElevation.y < 0)
                    Terrain_Size_Y = item.container.ContainerSize.y * -1;

            }
            else
                Terrain_Size_Y = item.container.ContainerSize.y;

            ShadedTexture = new Texture2D(TextureSize.x, TextureSize.y, TextureFormat.RGB24, false);

            switch (shaderType)
            {
                case ShaderType.ColorRamp:
                    GenerateElevationShader(item, UnderWater);
                    break;
                case ShaderType.ElevationGrayScale:
                    GenerateElevationShader(item, UnderWater);
                    break;
                case ShaderType.ElevationInversGrayScale:
                    GenerateElevationShader(item, UnderWater);
                    break;
                case ShaderType.Slop:
                    GenerateSlopShader(item);
                    break;
                case ShaderType.SlopInvers:
                    GenerateSlopShader(item);
                    break;
                case ShaderType.NormalMap:
                    GenerateNormalMapShader(item);
                    break;
            }


            ShadedTexture.Apply();

            if (saveShaderTextures == OptionEnabDisab.Enable)
                SaveShadersAsTexturesRuntime(item, RuntimePath);

            AddTextureToTerrainRuntime(item, ShadedTexture);

            item.TextureState = TextureState.Loaded;
        }
        private static void GenerateBaseShaders(bool SetColor = true, bool invers = true)
        {
            if(SetColor)
            {
                Color_Gradien = GetGradientColor(ShaderColor.GradientColor);
                Main_Gradien_Pos = GetGradientColor(ShaderColor.MainGradient);
                Negative_Gradien_Pos = GetGradientColor(ShaderColor.NegativeGradient);
            }
            else
            {
                if (invers)
                {
                    Color_Gradien = GetGradientColor(ShaderColor.GreyToBlack);
                    Main_Gradien_Pos = GetGradientColor(ShaderColor.GreyToWhite);
                    Negative_Gradien_Pos = GetGradientColor(ShaderColor.BlackToWhite);
                }
                else
                {
                    Color_Gradien = GetGradientColor(ShaderColor.BlackToWhite);
                    Main_Gradien_Pos = GetGradientColor(ShaderColor.GreyToWhite);
                    Negative_Gradien_Pos = GetGradientColor(ShaderColor.GreyToBlack);
                }
            }
 
            Color_Gradien.Apply();
            Main_Gradien_Pos.Apply();
            Negative_Gradien_Pos.Apply();
        }
        private static Texture2D GetGradientColor(ShaderColor shadercolor)
        {
            var tex = new Texture2D(0, 0);

            switch (shadercolor)
            {
                case ShaderColor.GradientColor:

                    tex = new Texture2D(9, 1, TextureFormat.ARGB32, false, true);
                    tex.SetPixel(0, 0, new Color32(80, 80, 230, 255));
                    tex.SetPixel(1, 0, new Color32(80, 180, 230, 255));
                    tex.SetPixel(2, 0, new Color32(80, 230, 230, 255));
                    tex.SetPixel(3, 0, new Color32(80, 230, 180, 255));
                    tex.SetPixel(4, 0, new Color32(80, 230, 80, 255));
                    tex.SetPixel(5, 0, new Color32(180, 230, 80, 255));
                    tex.SetPixel(6, 0, new Color32(230, 230, 80, 255));
                    tex.SetPixel(7, 0, new Color32(230, 180, 80, 255));
                    tex.SetPixel(8, 0, new Color32(230, 80, 80, 255));
                    tex.wrapMode = TextureWrapMode.Clamp;

                    break;
                case ShaderColor.MainGradient:

                    tex = new Texture2D(5, 1, TextureFormat.ARGB32, false, true);
                    tex.SetPixel(0, 0, new Color32(80, 230, 80, 255));
                    tex.SetPixel(1, 0, new Color32(180, 230, 80, 255));
                    tex.SetPixel(2, 0, new Color32(230, 230, 80, 255));
                    tex.SetPixel(3, 0, new Color32(230, 180, 80, 255));
                    tex.SetPixel(4, 0, new Color32(230, 80, 80, 255));
                    tex.wrapMode = TextureWrapMode.Clamp;

                    break;
                case ShaderColor.NegativeGradient:

                    tex = new Texture2D(5, 1, TextureFormat.ARGB32, false, true);
                    tex.SetPixel(0, 0, new Color32(80, 230, 80, 255));
                    tex.SetPixel(1, 0, new Color32(80, 230, 180, 255));
                    tex.SetPixel(2, 0, new Color32(80, 230, 230, 255));
                    tex.SetPixel(3, 0, new Color32(80, 180, 230, 255));
                    tex.SetPixel(4, 0, new Color32(80, 80, 230, 255));
                    tex.wrapMode = TextureWrapMode.Clamp;

                    break;
                case ShaderColor.BlackToWhite:

                    tex = new Texture2D(5, 1, TextureFormat.ARGB32, false, true);
                    tex.SetPixel(0, 0, new Color32(0, 0, 0, 255));
                    tex.SetPixel(1, 0, new Color32(64, 64, 64, 255));
                    tex.SetPixel(2, 0, new Color32(128, 128, 128, 255));
                    tex.SetPixel(3, 0, new Color32(192, 192, 192, 255));
                    tex.SetPixel(4, 0, new Color32(255, 255, 255, 255));
                    tex.wrapMode = TextureWrapMode.Clamp;

                    break;
                case ShaderColor.GreyToWhite:

                    tex = new Texture2D(3, 1, TextureFormat.ARGB32, false, true);
                    tex.SetPixel(0, 0, new Color32(128, 128, 128, 255));
                    tex.SetPixel(1, 0, new Color32(192, 192, 192, 255));
                    tex.SetPixel(2, 0, new Color32(255, 255, 255, 255));
                    tex.wrapMode = TextureWrapMode.Clamp;

                    break;
                case ShaderColor.GreyToBlack:

                    tex = new Texture2D(3, 1, TextureFormat.ARGB32, false, true);
                    tex.SetPixel(0, 0, new Color32(128, 128, 128, 255));
                    tex.SetPixel(1, 0, new Color32(64, 64, 64, 255));
                    tex.SetPixel(2, 0, new Color32(0, 0, 0, 255));
                    tex.wrapMode = TextureWrapMode.Clamp;

                    break;

            }
            return tex;
        }
        private static void GenerateElevationShader(TerrainObject item, OptionEnabDisab UnderWater)
        {
            for (int m_y = 0; m_y < TextureSize.y; m_y++)
            {
                for (int m_x = 0; m_x < TextureSize.x; m_x++)
                {
                    float el = GetNormalizedHeight(m_y, m_x);

                    if (UnderWater == OptionEnabDisab.Enable)
                    {
                        if (item.container.MinMaxElevation.y < 0)
                            el += item.container.MinMaxElevation.x;
                    }

                    Color color = GetColor(el, 0, true);
                    ShadedTexture.SetPixel(m_x, m_y, color);

                }
            }
        }
        private static void GenerateSlopShader(TerrainObject item)
        {
            for (int m_y = 0; m_y < TextureSize.y; m_y++)
            {
                for (int m_x = 0; m_x < TextureSize.x; m_x++)
                {
                    Vector2 d1 = DerivativeCal(m_y, m_x);

                    float slope = GISTerrainLoaderExtensions.SlopeCal(d1.x, d1.y);

                    Color color = GetColor(slope, 0.5f, true);

                    ShadedTexture.SetPixel(m_x, m_y, color);

                }
            }
        }
        private static void GenerateNormalMapShader(TerrainObject item)
        {
            for (int m_y = 0; m_y < TextureSize.y; m_y++)
            {
                for (int m_x = 0; m_x < TextureSize.x; m_x++)
                {
                    Vector2 Der = DerivativeCal(m_y, m_x);
 
                    var Normal = new Vector3(Der.x * 0.7f + 0.7f, -Der.y * 0.7f + 0.7f, 1.2f);

                    Normal.Normalize();

                    ShadedTexture.SetPixel(m_x, m_y, new Color(Normal.x, Normal.y, Normal.z, 1));
                }
            }
        }
        private static async Task SaveShadersAsTexturesAsync(TerrainObject item, string terrainPath)
        {

#if UNITY_EDITOR
                if (!Application.isPlaying)
            {
                var folderPath = Path.GetDirectoryName(terrainPath);
                var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
                var ShaderTexturesFolder = Path.Combine(folderPath, TerrainFilename + "_ShaderTextures");

                if (!Directory.Exists(ShaderTexturesFolder))
                    Directory.CreateDirectory(ShaderTexturesFolder);

                DirectoryInfo di = new DirectoryInfo(terrainPath);

                var ResourceShaderPath = Path.GetFileNameWithoutExtension(terrainPath) + "_ShaderTextures";

                for (int i = 0; i <= 5; i++)
                {
                    di = di.Parent;
                    ResourceShaderPath = di.Name + "/" + ResourceShaderPath;

                    if (di.Name == "GIS Terrains") break;

                    if (i == 5)
                    {
                        Debug.LogError("Texture folder not found! : Please put your terrain in GIS Terrain Loader/Recources/GIS Terrains/");

                        return;

                    }

                }

                var TexturePath = ShaderTexturesFolder + "/" + "Tile__" + (item.Number.x).ToString() + "__" + item.Number.y + ".jpg";
                await WriteShaderAsync(ShadedTexture, TexturePath);
 
                AssetDatabase.Refresh();

                var ResourceTexturePath = ResourceShaderPath + "/" + "Tile__" + (item.Number.x).ToString() + "__" + item.Number.y;
                AddTextureToTerrainEditor(item, ResourceTexturePath);
            }
            else
            {
                if (!string.IsNullOrEmpty(terrainPath))
                {
                    var folderPath = Path.GetDirectoryName(terrainPath);
                    var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
                    var ShaderFolder = Path.Combine(folderPath, TerrainFilename + "_ShaderTextures");

                    if (!Directory.Exists(ShaderFolder))
                        Directory.CreateDirectory(ShaderFolder);

                    var TexturePath = ShaderFolder + "/" + "Tile__" + (item.Number.x).ToString() + "__" + item.Number.y + ".jpg";
                    await WriteShaderAsync(ShadedTexture, TexturePath);

                }
            }

#else
                if (!string.IsNullOrEmpty(terrainPath))
                {
                    var folderPath = Path.GetDirectoryName(terrainPath);
                    var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
                    var ShaderFolder = Path.Combine(folderPath, TerrainFilename + "_ShaderTextures");

                    if (!Directory.Exists(ShaderFolder))
                        Directory.CreateDirectory(ShaderFolder);

                    var TexturePath = ShaderFolder + "/" + "Tile__" + (item.Number.x).ToString() + "__" + item.Number.y + ".jpg";
                    await WriteShaderAsync(ShadedTexture, TexturePath);

                }
#endif

        }
        private static void SaveShadersAsTexturesRuntime(TerrainObject item, string RuntimePath = "")
        {
            var TexturePath = ""; 

            if (!string.IsNullOrEmpty(RuntimePath))
            {
                var folderPath = Path.GetDirectoryName(RuntimePath);
                var TerrainFilename = Path.GetFileNameWithoutExtension(RuntimePath);
                var ShaderFolder = Path.Combine(folderPath, TerrainFilename + "_ShaderTextures");

                if (!Directory.Exists(ShaderFolder))
                    Directory.CreateDirectory(ShaderFolder);

                TexturePath = ShaderFolder + "/" + item.name + ".jpg";
                WriteShaderRuntime(ShadedTexture, TexturePath);
  
            }

            //AddTextureToTerrainRuntime(item, ShadedTexture);
        }
        private static void AddTextureToTerrainEditor(TerrainObject item, string texPath)
        {
 
#if UNITY_2018_1_OR_NEWER

                TerrainLayer NewterrainLayer = new TerrainLayer();

                string path = Path.Combine(item.container.GeneratedTerrainfolder, item.name + ".terrainlayer");

#if UNITY_EDITOR
                AssetDatabase.CreateAsset(NewterrainLayer, path);
#endif
                TerrainLayer[] ExistingTerrainLayers = item.terrainData.terrainLayers;
                List<TerrainLayer> NewLayers = new List<TerrainLayer>();
                foreach (var l in ExistingTerrainLayers)
                {
                    NewLayers.Add(l);
                }
#if UNITY_EDITOR
                NewterrainLayer.diffuseTexture = (Texture2D)Resources.Load(texPath); 
#endif

                NewterrainLayer.tileSize = new Vector2(item.size.x, item.size.z);
                NewterrainLayer.tileOffset = Vector2.zero;

                NewLayers.Add(NewterrainLayer);

                item.terrainData.terrainLayers = NewLayers.ToArray();

#else

            SplatPrototype sp = new SplatPrototype
            {
                texture = tex,
                tileSize = new Vector2(item.size.x, item.size.z),
                tileOffset = Vector2.zero
            };
            item.terrainData.splatPrototypes = new[] { sp };

#endif
 

        }
        private static void AddTextureToTerrainRuntime(TerrainObject item, Texture2D generatedText)
        {
     
#if UNITY_2018_1_OR_NEWER

            TerrainLayer NewterrainLayer = new TerrainLayer();

                TerrainLayer[] ExistingTerrainLayers = item.terrainData.terrainLayers;
                List<TerrainLayer> NewLayers = new List<TerrainLayer>();
                foreach (var l in ExistingTerrainLayers)
                {
                    NewLayers.Add(l);
                }
 
            NewterrainLayer.diffuseTexture = generatedText;

            NewterrainLayer.tileSize = new Vector2(item.size.x, item.size.z);
            NewterrainLayer.tileOffset = Vector2.zero;

            NewLayers.Add(NewterrainLayer);

            item.terrainData.terrainLayers = NewLayers.ToArray();

#else

            SplatPrototype sp = new SplatPrototype
            {
                texture = tex,
                tileSize = new Vector2(item.size.x, item.size.z),
                tileOffset = Vector2.zero
            };
            item.terrainData.splatPrototypes = new[] { sp };

#endif

        }
        private static void ChangeDiffuseTexture(TerrainObject item, Texture2D generatedText)
        {
            if(item.terrainData.terrainLayers == null || item.terrainData.terrainLayers.Length==0)
            {
#if UNITY_2018_1_OR_NEWER

                TerrainLayer NewterrainLayer = new TerrainLayer();

                TerrainLayer[] ExistingTerrainLayers = item.terrainData.terrainLayers;
                List<TerrainLayer> NewLayers = new List<TerrainLayer>();
                foreach (var l in ExistingTerrainLayers)
                {
                    NewLayers.Add(l);
                }

                NewterrainLayer.diffuseTexture = generatedText;

                NewterrainLayer.tileSize = new Vector2(item.size.x, item.size.z);
                NewterrainLayer.tileOffset = Vector2.zero;

                NewLayers.Add(NewterrainLayer);

                item.terrainData.terrainLayers = NewLayers.ToArray();

#else

            SplatPrototype sp = new SplatPrototype
            {
                texture = tex,
                tileSize = new Vector2(item.size.x, item.size.z),
                tileOffset = Vector2.zero
            };
            item.terrainData.splatPrototypes = new[] { sp };
#endif
            }
            else
            {
                item.
                    terrainData.
                    terrainLayers[0].
                    diffuseTexture = 
                    generatedText;
            }


        }
        public static async Task WriteShaderAsync(Texture2D texture, string path)
        {
            byte[] bytes = texture.EncodeToPNG();
            await GISTerrainLoaderFileAsync.WriteAllBytes(path, bytes);
        }
        public static void WriteShaderRuntime(Texture2D texture, string path)
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
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


        private static float GetHeight(int x, int y)
        {
            return GetNormalizedHeight(x, y) * Terrain_Size_Y;
        }
        private static float GetNormalizedHeight(int x, int y)
        {
            x = Mathf.Clamp(x, 0, TextureSize.x - 1);
            y = Mathf.Clamp(y, 0, TextureSize.y - 1);

            return data[x, y];
        }
        public static Vector2 DerivativeCal(int x, int y)
        {
            float CellPixelSize = 10;
            float El1 = GetHeight(x - 1, y + 1);
            float El2 = GetHeight(x + 0, y + 1);
            float El3 = GetHeight(x + 1, y + 1);
            float El4 = GetHeight(x - 1, y + 0);
            float El6 = GetHeight(x + 1, y + 0);
            float El7 = GetHeight(x - 1, y - 1);
            float El8 = GetHeight(x + 0, y - 1);
            float El9 = GetHeight(x + 1, y - 1);

            float El_x = (El3 + El6 + El9 - El1 - El4 - El7) / (6.0f * CellPixelSize);
            float El_y = (El1 + El2 + El3 - El7 - El8 - El9) / (6.0f * CellPixelSize);

            return new Vector2(-El_x, -El_y);
        }
        private static Color GetColor(float v, float exponent, bool nonNegative)
        {
            if (exponent > 0)
            {
                float sign = GISTerrainLoaderExtensions.SignOrZero(v);
                float pow = Mathf.Pow(10, exponent);
                float log = Mathf.Log(1.0f + pow * Mathf.Abs(v));

                v = sign * log;
            }

            if (nonNegative)
                return Color_Gradien.GetPixelBilinear(v, 0);
            else
            {
                if (v > 0)
                    return Main_Gradien_Pos.GetPixelBilinear(v, 0);
                else
                    return Negative_Gradien_Pos.GetPixelBilinear(-v, 0);
            }
        }
   

    }
}