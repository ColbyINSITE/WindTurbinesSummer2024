/*     Unity GIS Tech 2020-2021      */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if EASYROADS || EASYROADS3D
using EasyRoads3Dv3;
#endif
namespace GISTech.GISTerrainLoader
{
#if EASYROADS || EASYROADS3D
    using EasyRoads3Dv3;

    public class GISTerrainLoaderEasyRoadGenerator
    {
        public static ERRoadNetwork roadNetwork;
        public static ERRoad Eroad;
        private static Transform roadContiner ;
        private static TerrainContainerObject container;
        private static GameObject roadNetworkGO = null;

        public static GameObject CreateRoad(GISTerrainLoaderRoad m_road, TerrainContainerObject m_container, bool m_Runtime=false)
        {
            container = m_container;

            GameObject Road = null;

            if (!m_Runtime)
                Road = EditorCreateRoad(m_road);
            else
                Road = RuntimeCreateRoad(m_road);

            return Road;
        }
        public static GameObject EditorCreateRoad(GISTerrainLoaderRoad m_road)
        {
            if (Object.FindObjectOfType<ERModularBase>() == null)
            {

                var ERNet_01 = Resources.Load("ERRoadNetwork") as GameObject;
                var ERNet_02 = Resources.Load("ER Road Network") as GameObject;

                if (ERNet_01)
                {
                    roadNetworkGO = Object.Instantiate(ERNet_01);
                }
                else
                {
                    if (ERNet_02)
                        roadNetworkGO = Object.Instantiate(ERNet_02);
                }

                if (roadNetworkGO != null)
                {
                    roadNetworkGO.name = "Road Network";
                    roadNetworkGO.transform.position = Vector3.zero;
                    roadContiner = roadNetworkGO.transform.Find("Road Objects");
                }

            }

            roadNetwork = new ERRoadNetwork();
            roadNetwork.roadNetwork.importSideObjectsAlert = false;
            roadNetwork.roadNetwork.importRoadPresetsAlert = false;
            roadNetwork.roadNetwork.importCrossingPresetsAlert = false;
            roadNetwork.roadNetwork.importSidewalkPresetsAlert = false;

            ERRoadType roadType = new ERRoadType();

            roadType.roadWidth = m_road.width;
            roadType.roadMaterial = m_road.material;

            Eroad = roadNetwork.CreateRoad(m_road.highwayType.ToString(), roadType, m_road.Points);
            Eroad.SnapToTerrain(true);
            Eroad.gameObject.isStatic = false;

            if(roadContiner==null)
                roadContiner = roadNetworkGO.transform.Find("Road Objects");


            if (roadContiner != null)
                Eroad.gameObject.transform.parent = roadContiner;


            return Eroad.gameObject;
        }
        public static GameObject RuntimeCreateRoad(GISTerrainLoaderRoad m_road)
        {
            if (Object.FindObjectOfType<ERModularBase>() == null)
            {
                GameObject roadNetworkGO = null;
                var ERNet_01 = Resources.Load("ER Road Network") as GameObject;
                var ERNet_02 = Resources.Load("ERRoadNetwork") as GameObject;

                if (ERNet_01)
                {
                    roadNetworkGO = Object.Instantiate(ERNet_01);
                }
                else
                {
                    if (ERNet_02)
                        roadNetworkGO = Object.Instantiate(ERNet_02);
                }



                if (roadNetworkGO != null)
                {
                    roadNetworkGO.name = "Road Network";
                    roadNetworkGO.transform.position = Vector3.zero;
                    roadContiner = roadNetworkGO.transform.Find("Road Objects");
                }

            }

            roadNetwork = new ERRoadNetwork();
            roadNetwork.roadNetwork.importSideObjectsAlert = false;
            roadNetwork.roadNetwork.importRoadPresetsAlert = false;
            roadNetwork.roadNetwork.importCrossingPresetsAlert = false;
            roadNetwork.roadNetwork.importSidewalkPresetsAlert = false;

            ERRoadType roadType = new ERRoadType();

            roadType.roadWidth = m_road.width;
            roadType.roadMaterial = m_road.material;

            Eroad = roadNetwork.CreateRoad(m_road.highwayType.ToString(), roadType, m_road.Points);
            Eroad.SnapToTerrain(true);
            Eroad.gameObject.isStatic = false;

            Eroad.gameObject.transform.parent = container.transform;

            return Eroad.gameObject;
        }
        public static void Finilize()
        {
            if (roadNetwork != null)
                roadNetwork.roadNetwork.BuildTerrainRoutine(roadNetwork);
        }

        public static void DestroyRoads()
        {
            if (roadContiner != null)
                roadContiner.DestroyChildren();
        }
    }
#endif
}