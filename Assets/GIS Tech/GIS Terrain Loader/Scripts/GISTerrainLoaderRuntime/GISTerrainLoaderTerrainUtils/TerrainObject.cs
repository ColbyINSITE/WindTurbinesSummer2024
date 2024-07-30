/*     Unity GIS Tech 2020-2021     */

using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class TerrainObject : MonoBehaviour
    {
        [HideInInspector]
        public string ElevationFilePath;
        [HideInInspector]
        public GISTerrainLoaderRuntimePrefs prefs;
        [HideInInspector]
        public TerrainContainerObject container;
        [HideInInspector]
        public Vector3 size;
        [HideInInspector]
        public Vector2Int Number;

        public Terrain terrain;

        public TerrainData terrainData;
 
        [HideInInspector]
        public ElevationState ElevationState;
        [HideInInspector]
        public TextureState TextureState;
    }

}