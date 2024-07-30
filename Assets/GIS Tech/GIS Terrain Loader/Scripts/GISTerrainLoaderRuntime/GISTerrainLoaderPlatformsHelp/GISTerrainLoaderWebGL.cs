/*     Unity GIS Tech 2020-2021      */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderWebGL
    {

        public static IEnumerator LoadFileBytes(string url, System.Action<byte[]> outData)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();
            bool downloaded = false;

#if UNITY_2020_1_OR_NEWER

           if(www.result != UnityWebRequest.Result.ConnectionError)
            downloaded = true;
#else
            if (!www.isHttpError)
                downloaded = true;
#endif            

            if (downloaded)
            {
                if (www.isDone)
                {
                    var data = www.downloadHandler.data;
                    outData(data);
                }
            }
        }

        public static IEnumerator LoadTexture(string url, Vector2Int Dim, System.Action<Texture2D> outData)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();
            bool downloaded = false;

#if UNITY_2020_1_OR_NEWER

            if (www.result != UnityWebRequest.Result.ConnectionError)
            downloaded = true;
#else
            if (!www.isHttpError)
                downloaded = true;
#endif            

            if (downloaded)
            {
                if (www.isDone)
                {
                    var Texturedata = www.downloadHandler.data;

                    var texture = new Texture2D(Dim.x, Dim.y);
                    texture.LoadImage(Texturedata);
                    outData(texture);
                }
            }
        }

        public static IEnumerator LoadFileWebGLData(string url, System.Action<GISTerrainLoaderWebGLData> outData)
        {
            GISTerrainLoaderWebGLData data = new GISTerrainLoaderWebGLData();

            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();
            bool downloaded = false;

#if UNITY_2020_1_OR_NEWER

            if (www.result != UnityWebRequest.Result.ConnectionError)
            downloaded = true;
#else
            if (!www.isHttpError)
                downloaded = true;
#endif            

            if (downloaded)
            {
                if (www.isDone)
                {
                    var data_text = www.downloadHandler.text.Split('|');

                    foreach (var D_line in data_text)
                    {

                        var line = D_line.Split('=');
                        if(line.Length>1)
                        {
                            var name = line[0].Trim();
                            var value = line[1].Trim();

                            switch (name)
                            {
                                case "Tiles_Count_x":
                                    data.Tiles_count.x = int.Parse(value);
                                    break;
                                case "Tiles_Count_y":
                                    data.Tiles_count.y = int.Parse(value);
                                    break;
                                case "MainFolder":
                                    data.MainPath = "file:///" + Application.streamingAssetsPath + "/" +value ;
                                    break;                                   
                                case "TextureFolder":
                                    data.TextureFolderExist = int.Parse(value);
                                    break;
                                case "Texture_Tile":
                                    data.textures.Add(value);
                                    break;
                                case "VectorFolder":
                                    data.VectorFolderExist = int.Parse(value);
                                    break;
                                case "Vector_Tile":
                                    data.vectors.Add(value);
                                    break;
                            }
                        }

                    }

                    outData(data);
                }
            }
        }

     }
    public class GISTerrainLoaderWebGLData
    {
        public Vector2Int Tiles_count = new Vector2Int(0, 0);
        public string MainPath=""; 
        public int TextureFolderExist = 0;
        public List<string> textures = new List<string>();
        public int VectorFolderExist = 0;
        public List<string> vectors = new List<string>();
    }
}