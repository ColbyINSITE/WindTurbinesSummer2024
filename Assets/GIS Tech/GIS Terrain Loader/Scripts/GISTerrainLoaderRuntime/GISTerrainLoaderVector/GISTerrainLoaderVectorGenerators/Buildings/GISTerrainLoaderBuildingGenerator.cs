/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderBuildingGenerator
    {
        private static GameObject buildings;
        public const float multipler = 10000;
        public const string MaterialsFolderName = "Environment/Buildings/Materials";

        private static HashSet<string> alreadyCreated;
        private static HashSet<string> alreadyAdded;
        private static List<GISTerrainLoaderSO_Building> buildingsPrefab;

        public static void GenerateBuildings(TerrainContainerObject container, GISTerrainLoaderGeoVectorData GeoData, List<GISTerrainLoaderSO_Building> m_buildingsPrefab)
        {
            buildingsPrefab = m_buildingsPrefab;
            alreadyAdded = new HashSet<string>();
            alreadyCreated = new HashSet<string>();

            if (!buildings)
            {
                buildings = new GameObject();
                buildings.name = "buildings";
                buildings.transform.parent = container.transform;
            }

            alreadyAdded = new HashSet<string>();

            var buildingPrefabs_str = new List<string>();
            foreach (var p in m_buildingsPrefab)
            {
                if (p != null)
                    buildingPrefabs_str.Add(p.buildingType);
            }

            if (GeoData.GeoBuilding.Count == 0)
                return;

            GISTerrainLoaderSO_Building building_SO = null;

            string Buildingtype = "";

            for (int i = 0; i < GeoData.GeoBuilding.Count; i++)
            {
                var Poly = GeoData.GeoBuilding[i];


                Buildingtype = Poly.Tag;

                building_SO = GetBuildingPrefab(Buildingtype);

                if (building_SO != null)
                    GISTerrainLoaderBuilding.CreateBuilding(container, Poly,buildings.transform, building_SO, buildingsPrefab[0]);
            }
        }

        private static GISTerrainLoaderSO_Building GetBuildingPrefab(string buildingtype)
        {
            GISTerrainLoaderSO_Building building = null;
            foreach (var prefab in buildingsPrefab)
            {
                if (prefab != null)
                {
                    if (prefab.buildingType == buildingtype)
                        building = prefab;

                }
            }
            return building;
        }

        public static List<GISTerrainLoaderSO_Building> GetBuildingPrefabs()
        {
            var buildingPrefab = Resources.LoadAll("Prefabs/Environment/Buildings/", typeof(GISTerrainLoaderSO_Building));

            List<GISTerrainLoaderSO_Building> prefabs = new List<GISTerrainLoaderSO_Building>();

            foreach (var building in buildingPrefab)
            {
                var r = building as GISTerrainLoaderSO_Building;

                prefabs.Add(r);

            }

            return prefabs;
        }
    }
}