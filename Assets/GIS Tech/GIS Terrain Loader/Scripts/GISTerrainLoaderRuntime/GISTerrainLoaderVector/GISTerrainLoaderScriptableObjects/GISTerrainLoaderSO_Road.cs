/*     Unity GIS Tech 2020-2021      */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{

    public class GISTerrainLoaderSO_Road : ScriptableObject
    {
        public string RoadType;
        public MaterialSet MaterialType;
        public Material Roadmaterial;
        public float RoadWidth = 1;
        public Color32 RoadColor = Color.black;
    }
    /// <summary>
    /// Auto : Load Materials form "Resources\Environment\Roads\Materials" switching
    /// between ES3/LineRender in this case material will loaded according to S-Obj name
    /// Custom : Set Custom material
    /// </summary>
    public enum MaterialSet
    {
        Auto,
        Custom
    }
}