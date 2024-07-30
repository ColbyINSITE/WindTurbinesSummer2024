/*     Unity GIS Tech 2020-2021      */

using System;
using System.IO;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderRawLoader
    {
        public static event ReaderEvents OnReadError;

        public GISTerrainLoaderFileData data;

        public RawByteOrder m_ByteOrder = RawByteOrder.Windows;

        public RawDepth m_Depth = RawDepth.Bit16;

        public static event TerrainProgression OnProgress;

        public bool LoadComplet;

    

        private int m_Width = 1;

        private int m_Height = 1;


        private string errorString;

        private int DefaultResolution;


        public void LoadRawGrid(TextureMode textureMode, string filepath)
        {
            LoadComplet = false;
            if (!File.Exists(filepath))
            {
                Debug.LogError("Please select a correct Raw file.");

                if (OnReadError != null)
                {
                    OnReadError();
                }
                return;
            }

            data = new GISTerrainLoaderFileData();
 
            PickRawDefaults(filepath);

            if (IsValidFile())
            {
                data.floatheightData = LoadPixelsFromFile(filepath);

                LoadComplet = true;
            }
            else
                LoadComplet = false;

        }
        private void PickRawDefaults(string path)
        {
            FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read);
            int num = (int)fileStream.Length;
            fileStream.Close();

            this.m_Depth = RawDepth.Bit16;

            int num2 = num / (int)this.m_Depth;
            int num3 = Mathf.RoundToInt(Mathf.Sqrt((float)num2));
            int num4 = Mathf.RoundToInt(Mathf.Sqrt((float)num2));

            if (num3 * num4 * (int)this.m_Depth == num)
            {
                this.m_Width = num3;
                this.m_Height = num4;

                DefaultResolution = m_Width;
            }
            else
            {
                this.m_Depth = RawDepth.Bit8;
                num2 = num / (int)this.m_Depth;
                num3 = Mathf.RoundToInt(Mathf.Sqrt((float)num2));
                num4 = Mathf.RoundToInt(Mathf.Sqrt((float)num2));
                if (num3 * num4 * (int)this.m_Depth == num)
                {
                    this.m_Width = num3;
                    this.m_Height = num4;

                    DefaultResolution = m_Width;
                }
                else
                {
                    this.m_Depth = RawDepth.Bit16;
                }
            }

            data.mapSize_col_x = m_Width;
            data.mapSize_row_y = m_Height;
        }
        private float[,] LoadPixelsFromFile(string filename)
        {
            byte[] buffer;

            using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read)))
            {
                buffer = reader.ReadBytes(this.m_Width * this.m_Height * (int)this.m_Depth);
                reader.Close();
            }

            int heightmapWidth = m_Width;
            int heightmapHeight = m_Height;

            data.mapSize_col_x = m_Width;

            data.mapSize_row_y = m_Height;

            int size = heightmapHeight;

            float[,] heights = new float[heightmapHeight, heightmapWidth];
            bool flag = m_Depth == RawDepth.Bit16;
            if (flag)
            {
                float num = 1.525879E-05f;

                for (int i = 0; i < heightmapHeight; i++)
                {
                    for (int j = 0; j < heightmapWidth; j++)
                    {
                        int num2 = Mathf.Clamp(size - j - 1, 0, size - 1) + (Mathf.Clamp(0, size - i - 1, size - 1) * size);
                        bool flag2 = m_ByteOrder == RawByteOrder.Mac == BitConverter.IsLittleEndian;
                        if (flag2)
                        {
                            byte b = buffer[num2 * 2];
                            buffer[num2 * 2] = buffer[num2 * 2 + 1];
                            buffer[num2 * 2 + 1] = b;
                        }
                        ushort num33 = BitConverter.ToUInt16(buffer, num2 * 2);
                        float num4 = (float)num33 * num;
                        int num5 = (heightmapHeight - 1 - i);

                        var el = num33;
                        heights[j, i] = el;

                        if (el < data.MinElevation)
                            data.MinElevation = el;
                        if (el > data.MaxElevation)
                            data.MaxElevation = el;
                        if (OnProgress != null)
                            OnProgress("Loading File ", i * j * 100 / (data.mapSize_row_y * data.mapSize_col_x));
                    }

                }
            }


            return heights;

        }
        private bool IsValidFile()
        {
            bool valid = false;

            if (this.m_Width > 4097 || this.m_Height > 4097)
            {
                valid = false;
                 Debug.LogError("Heightmaps above 4097x4097 in resolution are not supported");
            }
            else
                valid = true;

            return valid;

        }
    }




    public enum RawDepth
    {
        Bit8 = 1,
        Bit16
    }

    public enum RawByteOrder
    {
        Mac = 1,
        Windows
    }

    public enum ExportType
    {
        Raw = 1,
        Png,
    }
    public enum ExportAs
    {
        Png = 1,
        jpg,
    }
}