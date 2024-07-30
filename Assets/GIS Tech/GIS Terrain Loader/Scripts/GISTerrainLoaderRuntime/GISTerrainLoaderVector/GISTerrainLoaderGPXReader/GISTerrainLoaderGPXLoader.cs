/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderGPXLoader
    {
        public static string[] GetGPXs(string terrainPath)
        {
            var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
            var VectorFolder = Path.Combine(Path.GetDirectoryName(terrainPath), TerrainFilename + "_VectorData");
            string[] tiles = null;

            if (Directory.Exists(VectorFolder))
            {
                var supportedExtensions = new HashSet<string> { ".gpx" };
                tiles = Directory.GetFiles(VectorFolder, "*.*", SearchOption.AllDirectories).Where(f => supportedExtensions.Contains(new FileInfo(f).Extension, StringComparer.OrdinalIgnoreCase)).ToArray();
            }
            else
                Debug.LogError("VectorData directory not exist");

            return tiles;
        }
        public static GISTerrainLoaderGPXFileData LoadGPXFile(string gpxfile, TerrainContainerObject m_container)
        {
            GISTerrainLoaderGPXFileData GPXData = new GISTerrainLoaderGPXFileData();

            using (FileStream stream = File.Open(gpxfile, FileMode.Open))
            {
                using (GISTerrainLoaderGPXFileLoader reader = new GISTerrainLoaderGPXFileLoader(stream))
                {
                    while (reader.Read())
                    {
                        GPXData.Type = reader.ObjectType;

                        switch (reader.ObjectType)
                        {
                            case GpxObjectType.WayPoint:
                                GISTerrainLoaderGPXWayPoint waypoint = new GISTerrainLoaderGPXWayPoint();
                                waypoint.Name = reader.WayPoint.Name;
                                waypoint.Latitude = reader.WayPoint.Latitude;
                                waypoint.Longitude = reader.WayPoint.Longitude;

                                if (m_container.IncludePoint(waypoint.Longitude, waypoint.Latitude))
                                    GPXData.WayPoints.Add(waypoint);

                                break;
                            case GpxObjectType.Track:

                                GISTerrainLoaderGPXPath path = new GISTerrainLoaderGPXPath();

                                path.Name = reader.Track.Name;

                                foreach (var p in reader.Track.Segments)
                                {
                                    foreach (var s in p.TrackPoints)
                                    {
                                        if (m_container.IncludePoint(s.Longitude, s.Latitude))
                                            path.WayPoints.Add(new DVector2(s.Longitude, s.Latitude));
                                    }
                                }

                                GPXData.Paths.Add(path);

                                break;
                        }
                        try
                        {


                        }
                        catch
                        {
                            GPXData = null;
                            Debug.LogError("Couldn't read .gpx file " + gpxfile);
                        }
                    }
                }

                return GPXData;
            }
        }

    }

    public class GISTerrainLoaderGPXFileData
    {
        public GpxObjectType Type;
        public List<GISTerrainLoaderGPXWayPoint> WayPoints = new List<GISTerrainLoaderGPXWayPoint>();
        public List<GISTerrainLoaderGPXPath> Paths = new List<GISTerrainLoaderGPXPath>();
    }
    public class GISTerrainLoaderGPXWayPoint
    {
        public string Name;
        public double Latitude;
        public double Longitude;
    }
    public class GISTerrainLoaderGPXPath
    {
        public string Name;
        public List<DVector2> WayPoints = new List<DVector2>();
    }

}