/*     Unity GIS Tech 2020-2021      */

using System.IO;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderDEMPngLoader
    {
        public static event ReaderEvents OnReadError;

        public static event TerrainProgression OnProgress;

        public GISTerrainLoaderFileData data;

        public bool LoadComplet;

        public void LoadPngGrid(TextureMode textureMode, string filepath)
        {
            LoadComplet = false;

            if (!File.Exists(filepath))
            {
                Debug.LogError("Please select a correct DEM Png file.");

                if (OnReadError != null)
                {
                    OnReadError();
                }
                return;
            }


            data = new GISTerrainLoaderFileData();
 
            data.floatheightData = LoadPNG(filepath);

            LoadComplet = true;
        }

        public Color waterColor = new Color(0.427f, 0.588f, 0.737f);
        private float[,] LoadPNG(string filename)
        {
            var heightmap = LoadedTextureTile(filename);

            int w = heightmap.width;
            int h = heightmap.height;

            data.mapSize_col_x = w;

            data.mapSize_row_y = h;

            var floatdata = new float[w, h];

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var x1 = 1.0f / w * x * w;
                    var y1 = 1.0f / h * y * h;

                    var pixel = heightmap.GetPixel((int)x1, (int)y1);

                    var el = pixel.grayscale; 

                    floatdata[x, y] = el;

                    if (el < data.MinElevation)
                        data.MinElevation = el;

                    if (el > data.MaxElevation)
                        data.MaxElevation = el;
                }


                var prog = (y * 100 / h);

                if (prog <= 99)
                {
                    if (OnProgress != null)
                        OnProgress("Loading File ", prog);
                }
            }

            return floatdata;
        }
        Texture2D LoadedTextureTile(string TexturePath)
        {
            Texture2D tex = new Texture2D(2, 2);

            if (File.Exists(TexturePath))
            {
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.LoadImage(File.ReadAllBytes(TexturePath));
                tex.LoadImage(tex.EncodeToPNG());
            }
            return tex;
        }

    }

}