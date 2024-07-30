/*     Unity GIS Tech 2020-2021      */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderSO_GrassObject : ScriptableObject
    {
        public string grassType;
        [Range(1, 100)]
        public float GrassDensity;
        public List<GISTerrainLoaderSO_Grass> GrassPrefab = new List<GISTerrainLoaderSO_Grass>();
    }
}