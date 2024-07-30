/*     Unity GIS Tech 2020-2021      */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderOSMFiltredData
{
    public Dictionary<long, GISTerrainLoaderOSMNode> Nodes;
    public Dictionary<long, GISTerrainLoaderOSMWay> Ways;
    public GISTerrainLoaderOSMFiltredData()
    {
        Nodes = new Dictionary<long, GISTerrainLoaderOSMNode>();
        Ways = new Dictionary<long, GISTerrainLoaderOSMWay>();
    }

}}

 