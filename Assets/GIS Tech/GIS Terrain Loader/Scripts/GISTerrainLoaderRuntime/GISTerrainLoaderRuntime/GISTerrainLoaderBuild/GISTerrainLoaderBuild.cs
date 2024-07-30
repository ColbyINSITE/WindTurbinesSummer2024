/*     Unity GIS Tech 2020-2021      */

using System.IO;
using UnityEngine;
using System;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
#endif
#if UNITY_EDITOR

public class GISTerrainLoaderBuild : IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
 
    public static void Copy(string sourceDirectory, string targetDirectory)
    {
#if GISTerrainLoaderPdal

        var diSource = new DirectoryInfo(sourceDirectory);
        var diTarget = new DirectoryInfo(targetDirectory);
        CopyAll(diSource, diTarget);

#endif
    }

    public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
    {
        Directory.CreateDirectory(target.FullName);
        foreach (FileInfo fi in source.GetFiles())
        {
            fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
        }
        foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
        {
            DirectoryInfo nextTargetSubDir =
                target.CreateSubdirectory(diSourceSubDir.Name);
            CopyAll(diSourceSubDir, nextTargetSubDir);
        }
    }
#if UNITY_2020_1_OR_NEWER
    public void OnPostprocessBuild(BuildReport report)
    {
#if GISTerrainLoaderPdal
        var Gdalsource = Path.Combine(Application.dataPath, "GIS Tech/GIS Terrain Loader/Plugins/Lidar/gdal/Data");
        var Projsource = Path.Combine(Application.dataPath, "GIS Tech/GIS Terrain Loader/Plugins/Lidar/proj4/Data");

        var PluginsPath = report.summary.outputPath.Replace(".exe","")+"_Data" + "/Plugins";
        var GdalDes = PluginsPath + "/Lidar/gdal/Data";
        var ProjDes = PluginsPath + "/Lidar/proj4/Data";

        Copy(Gdalsource, GdalDes);
        Copy(Projsource, ProjDes);
#endif
    }
#else
    public void OnPostprocessBuild(BuildTarget target, string path)
    {
        var Gdalsource = Path.Combine(Application.dataPath, "GIS Tech/GIS Terrain Loader/Plugins/Lidar/gdal/Data");
        var Projsource = Path.Combine(Application.dataPath, "GIS Tech/GIS Terrain Loader/Plugins/Lidar/proj4/Data");

        var PluginsPath = path.Replace(".exe","")+"_Data" + "/Plugins";
        var GdalDes = PluginsPath + "/Lidar/gdal/Data";
        var ProjDes = PluginsPath + "/Lidar/proj4/Data";

        Copy(Gdalsource, GdalDes);
        Copy(Projsource, ProjDes);
    }
#endif
}
#endif