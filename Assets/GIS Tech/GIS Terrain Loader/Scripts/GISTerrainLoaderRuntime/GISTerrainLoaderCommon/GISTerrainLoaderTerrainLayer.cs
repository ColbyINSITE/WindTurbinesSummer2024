/*     Unity GIS Tech 2020-2021      */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    [System.Serializable]
    public class GISTerrainLoaderTerrainLayer
    {
        public Texture2D Diffuse;
        public Texture2D NormalMap;
        public Vector2 TextureSize = new Vector2(15, 15);
        public float X_Height;
        public float Y_Height;

        public bool ShowHeight = true;
        public GISTerrainLoaderTerrainLayer()
        {
 
        }
        public GISTerrainLoaderTerrainLayer(Texture2D m_Diffuse, Texture2D m_NormalMap, Vector2 m_TextureSize)
        {
            Diffuse = m_Diffuse;
            NormalMap = m_NormalMap;
            TextureSize = m_TextureSize;
        }
        public GISTerrainLoaderTerrainLayer (Texture2D m_Diffuse, Texture2D m_NormalMap, Vector2 m_TextureSize, float m_X_Height, float m_Y_Height,bool m_ShowHeight)
        {
            Diffuse = m_Diffuse;
            NormalMap = m_NormalMap;
            TextureSize = m_TextureSize;
            X_Height = m_X_Height;
            Y_Height = m_Y_Height;
            ShowHeight = m_ShowHeight;
 
        }

 
    }
}