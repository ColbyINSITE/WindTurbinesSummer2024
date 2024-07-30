using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderHeightmapSerializer 
    {

        public static void Serialize(GISTerrainLoaderFileData source)
        {
#if UNITY_EDITOR
            var bytes = ToBytes(source.floatheightData);
            using (FileStream file = new FileStream("Assets/GIS Tech/GIS Terrain Loader/Resources/HeightmapData/File_Data.bytes", FileMode.Create, FileAccess.Write))
            {
                file.Write(bytes, 0, bytes.Length);
            }
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
        private static byte[] ToBytes<T>(T[,] array) where T : struct
        {
            var buffer = new byte[array.GetLength(0) * array.GetLength(1) * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))];
            Buffer.BlockCopy(array, 0, buffer, 0, buffer.Length);
            return buffer;
        }
        public static float[,] DeserializeHeightMap(byte[] bytes, Vector2 heightMapSize)
        {
            var heightMap = new float[(int)heightMapSize.x, (int)heightMapSize.y];
            FromBytes(heightMap, bytes);
            return heightMap;
        }
        private static void FromBytes<T>(T[,] array, byte[] buffer) where T : struct
        {
            var len = Math.Min(array.GetLength(0) * array.GetLength(1) * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), buffer.Length);
            Buffer.BlockCopy(buffer, 0, array, 0, len);
        }
    }
}