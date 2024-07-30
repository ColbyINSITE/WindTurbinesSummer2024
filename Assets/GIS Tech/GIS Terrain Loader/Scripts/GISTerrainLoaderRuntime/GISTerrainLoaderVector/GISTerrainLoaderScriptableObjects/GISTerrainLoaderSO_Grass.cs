/*     Unity GIS Tech 2020-2021      */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderSO_Grass : ScriptableObject
    {
        [Tooltip("Enable Using this Mode")]
        public bool EnableModelUsing = true;

        public Texture2D DetailTexture;

        public float MinWidth = 1;
        public float MaxWidth = 2;

        public float MinHeight = 1;
        public float MaxHeight = 2;

        public float Noise = 0.1f;

        public Color32 HealthyColor = Color.green;

        public Color32 DryColor = Color.gray;


        public bool BillBoard = true;


    }
}