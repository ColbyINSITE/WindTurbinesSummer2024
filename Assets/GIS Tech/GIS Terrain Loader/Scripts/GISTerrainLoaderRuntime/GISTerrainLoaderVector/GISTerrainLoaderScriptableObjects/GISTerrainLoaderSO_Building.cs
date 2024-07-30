/*     Unity GIS Tech 2020-2021      */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderSO_Building : ScriptableObject
    {
        public string buildingType;
        public BuildingRoofType roofType;
        public float height;

        public Material wall;
        public Material roof;

        public Vector2 WallTextureTiling = Vector2.one;
        public Vector2 RoofTextureTiling = Vector2.one;

        [HideInInspector]
        public Mesh mesh;

    }
}