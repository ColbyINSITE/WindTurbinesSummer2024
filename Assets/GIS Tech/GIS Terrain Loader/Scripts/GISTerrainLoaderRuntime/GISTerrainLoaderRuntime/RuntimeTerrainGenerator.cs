/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;


namespace GISTech.GISTerrainLoader
{
    public delegate void TerrainProgression(string phasename, float value);
    public delegate void RuntimeTerrainGeneratorEvents();
    public delegate void RuntimeTerrainGeneratorOrigine(DVector2 _origine, float minEle, float maxEle);

    
    public class RuntimeTerrainGenerator : MonoSingleton<RuntimeTerrainGenerator>
    {
        public static event TerrainProgression OnProgress;

        public static event RuntimeTerrainGeneratorEvents OnFinish;

        public static event RuntimeTerrainGeneratorOrigine SendTerrainOrigin;

        private GISTerrainLoaderElevationInfo ElevationInfo;

        private GISTerrainLoaderRuntimePrefs RuntimePrefs;

        public TerrainObject[,] terrains;

        private List<TerrainObject> ListTerrainObjects;

        [HideInInspector]
        public TerrainContainerObject GeneratedContainer;

        private Camera3D Cam3D;

        [HideInInspector]
        public string TerrainFilePath;

        float ElevationScaleValue = 1112.0f;
        float ScaleFactor = 1000;

        [HideInInspector]
        public bool RemovePrevTerrain;
 
        private string LoadedFileExtension = "";

        [HideInInspector]
        public bool Error;

        private GeneratorState Generatorstate = GeneratorState.idle;

        private GISTerrainLoaderWebGLData WebData = new GISTerrainLoaderWebGLData();
 
        void OnEnable()
        {
            if(Camera.main.GetComponent<Camera3D>())
            Cam3D = Camera.main.GetComponent<Camera3D>();
        }
        void Start()
        {
            RuntimePrefs = GISTerrainLoaderRuntimePrefs.Get;

        }
 
        public IEnumerator StartGenerating()
        {
            if (Generatorstate != GeneratorState.Generating)
            {
                Generatorstate = GeneratorState.Generating;

                yield return Try(CheckForFile());
                yield return Try(LoadElevationFile(TerrainFilePath));
                yield return Try(GenerateTerrains());
                yield return Try(GenerateHeightmap());
                yield return Try(RepareTerrains());
                yield return Try(GenerateTextures());
                yield return Try(GenerateVectorData());
                yield return Try(Finish());
            }
        }


        public void StopGeneration()
        {
            StopAllCoroutines();
        }
        private IEnumerator CheckForFile()
        {

#if UNITY_WEBGL

            WebData = new GISTerrainLoaderWebGLData();

            var WebDataPath = "file:///" + Path.Combine(Path.GetDirectoryName(TerrainFilePath), "WebGL_Data.webgl").Replace("file:/", "");

            yield return StartCoroutine(GISTerrainLoaderWebGL.LoadFileWebGLData(WebDataPath, (data) =>
            {
                WebData = data;

                RuntimePrefs.terrainCount = WebData.Tiles_count;
            }));

            if (RuntimePrefs.terrainMaterialMode == TerrainMaterialMode.Standard || RuntimePrefs.terrainMaterial == null)
            {
                RuntimePrefs.terrainMaterial = (Material)Resources.Load("Materials/Default-Terrain-Standard", typeof(Material));

                if (RuntimePrefs.terrainMaterial == null)
                    Debug.LogError("Custom terrain material null or standard terrain material not found in 'Resources/Materials/Default-Terrain-Standard' ");
            }

#else
            if (GISTerrainLoaderSupport.IsValidTerrainFile(Path.GetExtension(TerrainFilePath)))
            {

                if (RuntimePrefs.textureMode == TextureMode.WithTexture)
                {
                    if (RuntimePrefs.textureloadingMode == TexturesLoadingMode.AutoDetection)
                    {
                        var c_count = new Vector2(0, 0);
                        GISTerrainLoaderTextureGenerator.GetTilesNumberInTextureFolder(TerrainFilePath, out c_count);
                        RuntimePrefs.terrainCount = new Vector2Int((int)c_count.x, (int)c_count.y);

                        if (c_count == Vector2.zero)
                        {
                            RuntimePrefs.terrainCount = new Vector2Int(1, 1);
                        }

                    }
                }

                if (RuntimePrefs.terrainMaterialMode == TerrainMaterialMode.Standard || RuntimePrefs.terrainMaterial == null)
                {
                    RuntimePrefs.terrainMaterial = (Material)Resources.Load("Materials/Default-Terrain-Standard", typeof(Material));

                    if (RuntimePrefs.terrainMaterial == null)
                        Debug.LogError("Custom terrain material null or standard terrain material not found in 'Resources/Materials/Default-Terrain-Standard' ");
                }



            }
            else
            {
                Debug.LogError("Can't Load this File or not exist..");
                OnError();
            }

            yield return null;
#endif



        }
        private IEnumerator LoadElevationFile(string filepath)
        {

            LoadedFileExtension = Path.GetExtension(filepath);

            switch (LoadedFileExtension)
            {
                case ".flt":
                    {

                        ElevationInfo = new GISTerrainLoaderElevationInfo();

                        var floatReader = new GISTerrainLoaderFloatReader();
                        floatReader.data.TerrainMaxMinElevation = RuntimePrefs.TerrainMaxMinElevation;

                        if (RuntimePrefs.readingMode == ReadingMode.Full)
                            floatReader.LoadFloatGrid(filepath, RuntimePrefs.terrainDimensionMode, RuntimePrefs.TerrainFixOption);
                        else
                            floatReader.LoadFloatGrid(RuntimePrefs.SubRegionUpperLeftCoordiante, RuntimePrefs.SubRegionDownRightCoordiante, filepath, RuntimePrefs.terrainDimensionMode, RuntimePrefs.TerrainFixOption);

                        yield return new WaitUntil(() => floatReader.LoadComplet == true);

                        ElevationInfo.GetData(floatReader.data);
                        CheckForDimensionAndTiles(true);


                    }
                    break;
                case ".bin":
                    {
                        ElevationInfo = new GISTerrainLoaderElevationInfo();

                        var binReader = new GISTerrainLoaderBinLoader();
                        binReader.data.TerrainMaxMinElevation = RuntimePrefs.TerrainMaxMinElevation;

                        if (RuntimePrefs.readingMode == ReadingMode.Full)
                            binReader.LoadFloatGrid(filepath, RuntimePrefs.terrainDimensionMode, RuntimePrefs.TerrainFixOption);
                        else
                        {
                            binReader.LoadFloatGrid(RuntimePrefs.SubRegionUpperLeftCoordiante, RuntimePrefs.SubRegionDownRightCoordiante, filepath, RuntimePrefs.terrainDimensionMode, RuntimePrefs.TerrainFixOption);
                        }

                        yield return new WaitUntil(() => binReader.LoadComplet == true);
                        ElevationInfo.GetData(binReader.data);
                        CheckForDimensionAndTiles(true);


                    }
                    break;
                case ".bil":
                    {
                        ElevationInfo = new GISTerrainLoaderElevationInfo();

                        var BILReader = new GISTerrainLoaderBILReader();
                        BILReader.data.TerrainMaxMinElevation = RuntimePrefs.TerrainMaxMinElevation;
                        if (RuntimePrefs.readingMode == ReadingMode.Full)
                            BILReader.LoadFloatGrid(filepath, RuntimePrefs.terrainDimensionMode, RuntimePrefs.TerrainFixOption);
                        else BILReader.LoadFloatGrid(RuntimePrefs.SubRegionUpperLeftCoordiante, RuntimePrefs.SubRegionDownRightCoordiante, filepath, RuntimePrefs.terrainDimensionMode, RuntimePrefs.TerrainFixOption);

                        yield return new WaitUntil(() => BILReader.LoadComplet == true);
                        ElevationInfo.GetData(BILReader.data);
                        CheckForDimensionAndTiles(true);

                    }
                    break;
                case ".asc":
                    {
                        ElevationInfo = new GISTerrainLoaderElevationInfo();

                        var ASCIReader = new GISTerrainLoaderASCILoader();
                        ASCIReader.data.TerrainMaxMinElevation = RuntimePrefs.TerrainMaxMinElevation;
                        if (RuntimePrefs.readingMode == ReadingMode.Full)
                            ASCIReader.LoadASCIGrid(filepath, RuntimePrefs.terrainDimensionMode, RuntimePrefs.TerrainFixOption);
                        else ASCIReader.LoadASCIGrid(RuntimePrefs.SubRegionUpperLeftCoordiante, RuntimePrefs.SubRegionDownRightCoordiante, filepath, RuntimePrefs.terrainDimensionMode, RuntimePrefs.TerrainFixOption);

                        yield return new WaitUntil(() => ASCIReader.LoadComplet == true);

                        ElevationInfo.GetData(ASCIReader.data);
                        CheckForDimensionAndTiles(true);

                    }
                    break;
                case ".hgt":
                    {
                        ElevationInfo = new GISTerrainLoaderElevationInfo();
                        var hgtReader = new GISTerrainLoaderHGTLoader();
                        hgtReader.data.TerrainMaxMinElevation = RuntimePrefs.TerrainMaxMinElevation;

                        if (RuntimePrefs.readingMode == ReadingMode.Full)
                            hgtReader.LoadFloatGrid(filepath, RuntimePrefs.TerrainFixOption);
                        else hgtReader.LoadFloatGrid(RuntimePrefs.SubRegionUpperLeftCoordiante, RuntimePrefs.SubRegionDownRightCoordiante, filepath, RuntimePrefs.TerrainFixOption);

                        yield return new WaitUntil(() => hgtReader.LoadComplet == true);
                        ElevationInfo.GetData(hgtReader.data);
                        CheckForDimensionAndTiles(true);


                    }
                    break;



                case ".tif":
                    {

                        ElevationInfo = new GISTerrainLoaderElevationInfo();

                        var TiffReader = new GISTerrainLoaderTIFFLoader();
                        TiffReader.data.TerrainMaxMinElevation = RuntimePrefs.TerrainMaxMinElevation;

                        byte[] WebData = new byte[0];

#if UNITY_WEBGL
                        yield return StartCoroutine(GISTerrainLoaderWebGL.LoadFileBytes(filepath, (data) =>
                        {
                            WebData = data;
                        }));
                        TiffReader.WebData = WebData;
#endif

                        if (RuntimePrefs.readingMode == ReadingMode.Full)
                            TiffReader.LoadTiff(filepath, RuntimePrefs.terrainDimensionMode, null, RuntimePrefs.TerrainFixOption, RuntimePrefs.EPSGCode, RuntimePrefs.tiffElevationSource);
                        else TiffReader.LoadTiff(RuntimePrefs.SubRegionUpperLeftCoordiante, RuntimePrefs.SubRegionDownRightCoordiante, filepath, RuntimePrefs.terrainDimensionMode, null, RuntimePrefs.TerrainFixOption);

                        yield return new WaitUntil(() => TiffReader.LoadComplet == true);

                        ElevationInfo.GetData(TiffReader.data);

                        CheckForDimensionAndTiles(true);

                    }

                    break;



                case ".las":
                    {
#if GISTerrainLoaderPdal

                            ElevationInfo = new GISTerrainLoaderElevationInfo();

                            var lasReader = new GISTerrainLoaderLASLoader();

                            if (!lasReader.LoadComplet)
                                lasReader.LoadLasFile(filepath);

                            ElevationInfo.GetData(lasReader.data);

                            yield return new WaitUntil(() => lasReader.LoadComplet == true);

                            yield return new WaitUntil(() => File.Exists(filepath) == true);

                            if (File.Exists(filepath))
                            {
                                TerrainFilePath = lasReader.GeneratedFilePath;
                                yield return new WaitForSeconds(1f);

                                var TiffReader = new GISTerrainLoaderTIFFLoader();
                                TiffReader.data.TerrainMaxMinElevation = RuntimePrefs.TerrainMaxMinElevation;

                                if (RuntimePrefs.readingMode == ReadingMode.Full)
                                    TiffReader.LoadTiff(TerrainFilePath, RuntimePrefs.terrainDimensionMode, ElevationInfo.data, RuntimePrefs.TerrainFixOption);
                                else TiffReader.LoadTiff(RuntimePrefs.SubRegionUpperLeftCoordiante, RuntimePrefs.SubRegionDownRightCoordiante, TerrainFilePath, RuntimePrefs.terrainDimensionMode, ElevationInfo.data, RuntimePrefs.TerrainFixOption);

                                yield return new WaitUntil(() => TiffReader.LoadComplet == true);
 
                                ElevationInfo.GetData(TiffReader.data);
                                CheckForDimensionAndTiles(true);
                                Generatorstate = GeneratorState.idle;
                                lasReader.LoadComplet = false;
                            }
                            else
                                Debug.LogError("File Not exsiting " + filepath);
#endif
                    }
                    break;
                case ".raw":
                    {

                        ElevationInfo = new GISTerrainLoaderElevationInfo();

                        var RawReader = new GISTerrainLoaderRawLoader();

                        RawReader.m_ByteOrder = RuntimePrefs.Raw_ByteOrder;
                        RawReader.m_Depth = RuntimePrefs.Raw_Depth;

                        RawReader.LoadRawGrid(RuntimePrefs.textureMode, filepath);

                        yield return new WaitUntil(() => RawReader.LoadComplet == true);
                        ElevationInfo.GetData(RawReader.data);
                        CheckForDimensionAndTiles(false);

                    }
                    break;
                case ".png":
                    {
                        ElevationInfo = new GISTerrainLoaderElevationInfo();

                        var PngReader = new GISTerrainLoaderDEMPngLoader();

                        PngReader.LoadPngGrid(RuntimePrefs.textureMode, filepath);

                        yield return new WaitUntil(() => PngReader.LoadComplet == true);

                        ElevationInfo.GetData(PngReader.data);
                        CheckForDimensionAndTiles(false);


                    }
                    break;

                case ".ter":
                    {

                        ElevationInfo = new GISTerrainLoaderElevationInfo();

                        var TerReader = new GISTerrainLoaderTerraGenLoade();

                        TerReader.LoadTer(RuntimePrefs.textureMode, filepath);

                        yield return new WaitUntil(() => TerReader.LoadComplet == true);

                        ElevationInfo.GetData(TerReader.data);
                        CheckForDimensionAndTiles(false);
                    }
                    break;
            }

        }
        private IEnumerator GenerateTerrains()
        {

            if (ElevationInfo.data.Terrain_Dimension.x == 0 || ElevationInfo.data.Terrain_Dimension.y == 0)
            {
                StopAllCoroutines();
                OnError();
                yield break;
            }

            ListTerrainObjects = new List<TerrainObject>();

            const string containerName = "Terrains";
            string cName = containerName;
            //Destroy prv created terrain
            if (RemovePrevTerrain)
            {
                Destroy(GameObject.Find(cName));
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            else
            {
                int index = 1;
                while (GameObject.Find(cName) != null)
                {
                    cName = containerName + " " + index.ToString();
                    index++;
                }
            }

            var container = new GameObject(cName);
            container.transform.position = new Vector3(0, 0, 0);
 
            Vector2 TlimiteFrom = new Vector2(RuntimePrefs.TerrainDimensions.x, 0);
            Vector2 TlimiteTo = new Vector2(RuntimePrefs.TerrainDimensions.y, 0);

            Vector2Int tCount = new Vector2Int((int)RuntimePrefs.terrainCount.x, (int)RuntimePrefs.terrainCount.y);

            float maxElevation = ElevationInfo.data.MaxElevation;
            float minElevation = ElevationInfo.data.MinElevation;
            float ElevationRange = maxElevation - minElevation;

            var sizeX = Mathf.Floor(RuntimePrefs.TerrainDimensions.x * RuntimePrefs.terrainScale.x * ScaleFactor) / RuntimePrefs.terrainCount.x;
            var sizeZ = Mathf.Floor(RuntimePrefs.TerrainDimensions.y * RuntimePrefs.terrainScale.z * ScaleFactor) / RuntimePrefs.terrainCount.y;
            var sizeY = (ElevationRange) / ElevationScaleValue * RuntimePrefs.TerrainExaggeration * 100 * RuntimePrefs.terrainScale.y * 10;


            Vector3 size;

            if (LoadedFileExtension == ".ter" || LoadedFileExtension == ".png" || LoadedFileExtension == ".raw")
            {
                if (RuntimePrefs.TerrainElevation == TerrainElevation.RealWorldElevation)
                {
                    sizeY = ((162)) * RuntimePrefs.terrainScale.y;
                    size = new Vector3(sizeX, sizeY, sizeZ);
                }
                else
                {
                    sizeY = 300 * RuntimePrefs.TerrainExaggeration * RuntimePrefs.terrainScale.y;
                    size = new Vector3(sizeX, sizeY, sizeZ);
                }

            }
            else
            {
                if (RuntimePrefs.TerrainElevation == TerrainElevation.RealWorldElevation)
                {
                    sizeY = (ElevationRange / ElevationScaleValue) * 1000 * RuntimePrefs.terrainScale.y;

                    size = new Vector3(sizeX, sizeY, sizeZ);
                }
                else
                {
                    sizeY = sizeY * 10;
                    size = new Vector3(sizeX, sizeY, sizeZ);
                }
            }


            terrains = new TerrainObject[tCount.x, tCount.y];

            container.AddComponent<TerrainContainerObject>();

            var terrainContainer = container.GetComponent<TerrainContainerObject>();

            GeneratedContainer = terrainContainer;

            terrainContainer.terrainCount = RuntimePrefs.terrainCount;

            terrainContainer.scale = RuntimePrefs.terrainScale;

            terrainContainer.ContainerSize = new Vector3(size.x * tCount.x, size.y, size.z * tCount.y);


            //Set Terrain Coordinates to the container TerrainContainer script (Lat/lon) + Mercator
            terrainContainer.TopLeftLatLong = ElevationInfo.data.TopLeftPoint;
            terrainContainer.DownRightLatLong = ElevationInfo.data.DownRightPoint;

            terrainContainer.TLPointMercator = GeoRefConversion.LatLongToMercat(terrainContainer.TopLeftLatLong.x, terrainContainer.TopLeftLatLong.y);
            terrainContainer.DRPointMercator = GeoRefConversion.LatLongToMercat(terrainContainer.DownRightLatLong.x, terrainContainer.DownRightLatLong.y);

            if (GISTerrainLoaderSupport.IsGeoFile(LoadedFileExtension))
                terrainContainer.Dimensions = new Vector2((float)ElevationInfo.data.Terrain_Dimension.x, (float)ElevationInfo.data.Terrain_Dimension.y);
            else
                terrainContainer.Dimensions = RuntimePrefs.TerrainDimensions;

            terrainContainer.MinMaxElevation = new Vector2((float)ElevationInfo.data.MinElevation, (float)ElevationInfo.data.MaxElevation);

            terrainContainer.MinMaxElevation = new Vector2((float)ElevationInfo.data.MinElevation, (float)ElevationInfo.data.MaxElevation);


            //Terrain Size Bounds 
            var centre = new Vector3(terrainContainer.ContainerSize.x / 2, 0, terrainContainer.ContainerSize.z / 2);
            terrainContainer.GlobalTerrainBounds = new Bounds(centre, new Vector3(centre.x + terrainContainer.ContainerSize.x / 2, 0, centre.z + terrainContainer.ContainerSize.z / 2));
 
            for (int x = 0; x < tCount.x; x++)
            {
                for (int y = 0; y < tCount.y; y++)
                {

                    terrains[x, y] = CreateTerrain(container.transform, x, y, size, RuntimePrefs.terrainScale);
                    terrains[x, y].container = terrainContainer;
                    ListTerrainObjects.Add(terrains[x, y]);
                }
            }

            terrainContainer.data = ElevationInfo.data;
            terrainContainer.terrains = terrains;

            yield return null;

        }
        private IEnumerator GenerateHeightmap()
        {
            int index = 0;

            foreach (var Tile in ListTerrainObjects)
            {
                if (index >= terrains.Length - 1)
                {
                    yield return null;
                }

                float prog = ((index * 100) / (ListTerrainObjects.Count));

                ElevationInfo.RuntimeGenerateHeightMap(RuntimePrefs, Tile);

                yield return new WaitUntil(() => terrains[Tile.Number.x, Tile.Number.y].ElevationState == ElevationState.Loaded);

                if (OnProgress != null)
                    OnProgress("Generating Heightmap", (float)prog);

                yield return new WaitUntil(() => terrains[Tile.Number.x, Tile.Number.y].ElevationState == ElevationState.Loaded);

                index++;
            }

        }
        private IEnumerator RepareTerrains()
        {
            if (RuntimePrefs.UseTerrainHeightSmoother)
                GISTerrainLoaderTerrainSmoother.SmoothTerrainHeights(ListTerrainObjects, 1 - RuntimePrefs.TerrainHeightSmoothFactor);

            if (RuntimePrefs.UseTerrainSurfaceSmoother)
                GISTerrainLoaderTerrainSmoother.SmoothTerrainSurface(ListTerrainObjects, RuntimePrefs.TerrainSurfaceSmoothFactor);

            if (RuntimePrefs.UseTerrainHeightSmoother || RuntimePrefs.UseTerrainSurfaceSmoother)
                GISTerrainLoaderBlendTerrainEdge.StitchTerrain(ListTerrainObjects, 50f, 20);

            yield return null;

        }
        private IEnumerator GenerateTextures()
        {
            int index = 0;

            switch (RuntimePrefs.textureMode)
            {
                case TextureMode.WithTexture:


#if UNITY_WEBGL
                    if (WebData.TextureFolderExist == 1)
                    {
                        foreach (var tile in WebData.textures)
                        {
                            var numbers = Regex.Matches(tile.Split('.')[0], @"\d+").OfType<Match>().Select(m => int.Parse(m.Value)).ToArray();
                            int x = numbers[0]; int y = numbers[1];

                            TerrainObject terrain = null;

                            foreach (var t in terrains)
                            {
                                if (t.Number.x == x && t.Number.y == y)
                                {
                                    terrain = t;
                                }

                            }
                            var texturePath = "file:///"+ (WebData.MainPath+ @"_Textures\" + tile).Replace("file:///", "");
                            yield return StartCoroutine(GISTerrainLoaderWebGL.LoadTexture(texturePath, new Vector2Int(RuntimePrefs.textureWidth, RuntimePrefs.textureHeight), (texture) =>
                            {
                                if (texture == null)
                                {
                                    texture = (Texture2D)Resources.Load("Textures/NullTexture");
                                }

                                GISTerrainLoaderTextureGenerator.RuntimeWebGLAddTexturesToTerrain(texture, terrain);
                            }));
                        }
                    }



#else
                    if (RuntimePrefs.textureloadingMode == TexturesLoadingMode.Manual)
                    {

                        var FolderTiles_count = new Vector2(0, 0);

                        GISTerrainLoaderTextureGenerator.GetTilesNumberInTextureFolder(TerrainFilePath, out FolderTiles_count);


                        if (RuntimePrefs.terrainCount != FolderTiles_count)
                        {
                            if (FolderTiles_count == Vector2.one)
                            {
                                GISTerrainLoaderTextureGenerator.SplitTex(TerrainFilePath, RuntimePrefs.terrainCount).Wait();
                            }
                            else
                            {
                                if (FolderTiles_count.x > 1 || FolderTiles_count.y > 1)
                                {
                                    GISTerrainLoaderTextureGenerator.CombienTerrainTextures(TerrainFilePath);

                                    GISTerrainLoaderTextureGenerator.SplitTex(TerrainFilePath, RuntimePrefs.terrainCount).Wait();

                                    RuntimePrefs.textureloadingMode = TexturesLoadingMode.AutoDetection;
                                }

                            }

                        }
                        else
                            RuntimePrefs.textureloadingMode = TexturesLoadingMode.AutoDetection;

                    }

                    if (RuntimePrefs.textureloadingMode == TexturesLoadingMode.AutoDetection)
                    {

                        foreach (var Tile in ListTerrainObjects)
                        {
                            if (index >= ListTerrainObjects.Count)
                            {
                                yield return null;
                            }

                            float prog = ((index * 100) / (ListTerrainObjects.Count));

                            if (OnProgress != null)
                                OnProgress("Generate Textures", prog);

                            GISTerrainLoaderTextureGenerator.RuntimeAddTexturesToTerrain(TerrainFilePath, Tile, new Vector2(RuntimePrefs.textureWidth, RuntimePrefs.textureHeight));

                            yield return new WaitUntil(() => Tile.TextureState == TextureState.Loaded);

                            index++;


                        }
                    }
#endif

                    break;

                case TextureMode.WithoutTexture:

                    if (RuntimePrefs.UseTerrainEmptyColor)
                    {

                        Material mat = new Material(Shader.Find("Standard"));
                        mat.SetColor("_Color", RuntimePrefs.TerrainEmptyColor);

                        foreach (var Tile in ListTerrainObjects)
                        {
                            float prog = ((index) * 100 / (terrains.GetLength(0) * terrains.GetLength(1))) / 100f;

                            Tile.terrain.materialTemplate = mat;

                            index++;

                            if (index >= terrains.Length)
                            {
                                yield return null;
                            }

                            if (OnProgress != null)
                                OnProgress("Generating Terrain Colors", prog);

                        }

                    }

                    yield return null;

                    break;

                case TextureMode.ShadedRelief:

                    foreach (var Tile in ListTerrainObjects)
                    {
                        if (index >= ListTerrainObjects.Count)
                        {
                            yield return null;
                        }

                        float prog = ((index * 100) / (ListTerrainObjects.Count));

                        if (OnProgress != null)
                            OnProgress("Generating Terrain Shader", prog);


                        GISTerrainLoaderTerrainShader.GenerateShadedTextureRuntime(RuntimePrefs.TerrainShaderType, RuntimePrefs.UnderWaterShader, Tile, new Vector2Int(RuntimePrefs.heightmapResolution - 1, RuntimePrefs.heightmapResolution - 1), true, RuntimePrefs.SaveShaderTextures, TerrainFilePath);

                        yield return new WaitUntil(() => Tile.TextureState == TextureState.Loaded);

                        index++;

                    }

                    break;
                case TextureMode.Splatmapping:

                    foreach (var Tile in ListTerrainObjects)
                    {
                        float prog = ((index) * 100 / (terrains.GetLength(0) * terrains.GetLength(1))) / 100f;

                        GISTerrainLoaderSplatMapping.SetTerrainSpaltMap(RuntimePrefs.BaseTerrainLayers, RuntimePrefs.TerrainLayers, RuntimePrefs.Slope, Tile, RuntimePrefs.MergeRaduis, RuntimePrefs.MergingFactor);

                        index++;

                        if (OnProgress != null)
                            OnProgress("Generating Splatmaps ", prog);

                        if (index >= terrains.Length)
                        {
                            yield return null;
                        }
                    }

                    break;
            }
            yield return null;
        }
        private IEnumerator GenerateVectorData()
        {
            if ((RuntimePrefs.IsVectorGenerationEnabled(LoadedFileExtension)))
            {
                if (RuntimePrefs.EnableGeoPointGeneration)
                    RuntimePrefs.GeoPointsPrefab = GISTerrainLoaderGeoPointGenerator.GetPointsPrefab();

                if (RuntimePrefs.EnableRoadGeneration)
                    RuntimePrefs.RoadsPrefab = GISTerrainLoaderRoadsGenerator.GetRoadsPrefab(RuntimePrefs.RoadType);

                if (RuntimePrefs.EnableTreeGeneration)
                    GISTerrainLoaderTreeGenerator.AddTreePrefabsToTerrains(GeneratedContainer, RuntimePrefs.TreePrefabs, RuntimePrefs.TreeDistance, RuntimePrefs.BillBoardStartDistance);

                if (RuntimePrefs.EnableGrassGeneration)
                    GISTerrainLoaderGrassGenerator.AddDetailsLayersToTerrains(GeneratedContainer, RuntimePrefs.GrassPrefabs, RuntimePrefs.DetailDistance, RuntimePrefs.GrassScaleFactor);

                if (RuntimePrefs.EnableBuildingGeneration)
                    RuntimePrefs.BuildingsPrefab = GISTerrainLoaderBuildingGenerator.GetBuildingPrefabs();

                GISTerrainLoaderGeoVectorData GeoData = new GISTerrainLoaderGeoVectorData();

                switch (RuntimePrefs.vectorType)
                {
                    case VectorType.OpenStreetMap:

                        var OSMFiles = GISTerrainLoaderExtensions.GetOSMFiles(TerrainFilePath);

                        if (OSMFiles != null && OSMFiles.Length > 0)
                        {

                            foreach (var osm in OSMFiles)
                            {
                                GISTerrainLoaderOSMFileLoader osmloader = new GISTerrainLoaderOSMFileLoader(osm, GeneratedContainer);

                                if (RuntimePrefs.EnableGeoPointGeneration)
                                {
                                    osmloader.GetGeoVectorPointsData(GeoData);
                                    GISTerrainLoaderGeoPointGenerator.GenerateGeoPoint(GeneratedContainer, GeoData, RuntimePrefs.GeoPointsPrefab);
                                }

                                if (RuntimePrefs.EnableRoadGeneration)
                                {
                                    osmloader.GetGeoVectorRoadsData(GeoData);
                                    GISTerrainLoaderRoadsGenerator.GenerateTerrainRoades(GeneratedContainer, GeoData, RuntimePrefs.RoadType, RuntimePrefs.EnableRoadName, RuntimePrefs.RoadsPrefab, true);

                                }

                                if (RuntimePrefs.EnableTreeGeneration)
                                {
                                    if (RuntimePrefs.TreePrefabs.Count > 0)
                                    {
                                        osmloader.GetGeoVectorTreesData(GeoData);
                                        GISTerrainLoaderTreeGenerator.GenerateTrees(GeneratedContainer, GeoData);
                                    }
                                    else
                                        Debug.LogError("Unable to generate Trees : Prefab List is empty ");
                                }

                                if (RuntimePrefs.EnableGrassGeneration)
                                {
                                    if (RuntimePrefs.GrassPrefabs.Count > 0)
                                    {
                                        osmloader.GetGeoVectorGrassData(GeoData);
                                        GISTerrainLoaderGrassGenerator.GenerateGrass(GeneratedContainer, GeoData);

                                    }
                                    else
                                        Debug.LogError("Unable to generate Grass : Prefab List is empty ");

                                }
                                if (RuntimePrefs.EnableBuildingGeneration)
                                {
                                    osmloader.GetGeoVectorBuildingData(GeoData);
                                    GISTerrainLoaderBuildingGenerator.GenerateBuildings(GeneratedContainer, GeoData, RuntimePrefs.BuildingsPrefab);
                                }

                            }
                            yield return null;
                        }
                        else
                        {
                            Debug.LogError("OSM File Folder is Empty ! : please set your osm file into 'GIS Terrains'\'TerrainName'\'TerrainName_VectorData'");
                            yield return null;
                        }

                        break;

                    case VectorType.ShapeFile:

                        var shapes = GISTerrainLoaderShapeReader.LoadShapes(TerrainFilePath);

                        if (shapes != null && shapes.Count > 0)
                        {

                            foreach (var shape in shapes)
                            {
                                GISTerrainLoaderShapeFileLoader shapeloader = new GISTerrainLoaderShapeFileLoader(shape);

                                if (RuntimePrefs.EnableRoadGeneration)
                                {
                                    shapeloader.GetGeoVectorRoadsData(GeoData);
                                    GISTerrainLoaderRoadsGenerator.GenerateTerrainRoades(GeneratedContainer, GeoData, RuntimePrefs.RoadType, RuntimePrefs.EnableRoadName, RuntimePrefs.RoadsPrefab,true);
                                }

                                if (RuntimePrefs.EnableTreeGeneration)
                                {
                                    if (RuntimePrefs.TreePrefabs.Count > 0)
                                    {
                                        shapeloader.GetGeoVectorTreesData(GeoData);
                                        GISTerrainLoaderTreeGenerator.GenerateTrees(GeneratedContainer, GeoData);
                                    }
                                    else
                                        Debug.LogError("Error : Tree Prefabs List is empty ");
                                }

                                if (RuntimePrefs.EnableGrassGeneration)
                                {
                                    if (RuntimePrefs.GrassPrefabs.Count > 0)
                                    {
                                        shapeloader.GetGeoVectorGrassData(GeoData);
                                        GISTerrainLoaderGrassGenerator.GenerateGrass(GeneratedContainer, GeoData);

                                    }
                                    else
                                        Debug.LogError("Error : Grass Prefabs List is empty ");

                                }

                                if (RuntimePrefs.EnableBuildingGeneration)
                                {
                                    shapeloader.GetGeoVectorBuildingData(GeoData);
                                    GISTerrainLoaderBuildingGenerator.GenerateBuildings(GeneratedContainer, GeoData, RuntimePrefs.BuildingsPrefab);
                                 }


                            }
                            yield return null;
                        }
                        else
                        {
                            Debug.Log("No Shape file exist");
                            yield return null;
                        }
                        break;

                    case VectorType.GPX:

                        var GPXsFiles = GISTerrainLoaderGPXLoader.GetGPXs(TerrainFilePath);

                        if (GPXsFiles != null && GPXsFiles.Length > 0)
                        {
                            foreach (var gpx in GPXsFiles)
                            {
                                GISTerrainLoaderGPXFileData LoadGPXFile = GISTerrainLoaderGPXLoader.LoadGPXFile(gpx, GeneratedContainer);

                                if (RuntimePrefs.EnableRoadGeneration)
                                    GISTerrainLoaderRoadsGenerator.GenerateTerrainRoades(LoadGPXFile, GeneratedContainer, RuntimePrefs.RoadType, RuntimePrefs.EnableRoadName, RuntimePrefs.RoadsPrefab, RuntimePrefs.PathPrefab);

                                if (RuntimePrefs.EnableGeoLocationPointGeneration)
                                    GISTerrainLoaderGeoPointGenerator.GenerateGeoPoint(LoadGPXFile, GeneratedContainer, RuntimePrefs.GeoPointPrefab);
                            }
                        }
                        else
                        {
                            Debug.LogError("VectorData Folder is Empty ! : Please set your osm files into 'GIS Terrains'\'TerrainName'\'TerrainName_VectorData'");

                        }


                        break;

                }
            }
            else
            {
                yield return null;
            }
            yield return null;
        }
        private IEnumerator Finish()
        {
            foreach (TerrainObject item in terrains)
                item.terrain.Flush();

            if (SendTerrainOrigin != null)
                SendTerrainOrigin(ElevationInfo.data.DownLeftPoint, ElevationInfo.data.MinElevation, ElevationInfo.data.MaxElevation);


            ResetCameraPosition();

            Generatorstate = GeneratorState.idle;

            StopAllCoroutines();

            if (OnFinish != null)
                OnFinish();

            if (OnProgress != null)
                OnProgress("Finalization", 100);

            yield return null;
        }
        public void SetGeneratedTerrain(TerrainContainerObject container)
        {
            GeneratedContainer = container;

            ResetCameraPosition();

            if (SendTerrainOrigin != null)
            {
                var m_Origin = new DVector2(container.TopLeftLatLong.x, container.DownRightLatLong.y);
                SendTerrainOrigin(m_Origin, container.MinMaxElevation.x, container.MinMaxElevation.y);
            }


        }
        void OnError()
        {
            Error = true;
            Generatorstate = GeneratorState.idle;
        }
        void OnEnabel()
        {
            GISTerrainLoaderFloatReader.OnReadError += OnError;
            GISTerrainLoaderTIFFLoader.OnReadError += OnError;
            GISTerrainLoaderTerraGenLoade.OnReadError += OnError;
            GISTerrainLoaderDEMPngLoader.OnReadError += OnError;
            GISTerrainLoaderRawLoader.OnReadError += OnError;
            GISTerrainLoaderASCILoader.OnReadError += OnError;
            GISTerrainLoaderBILReader.OnReadError += OnError;
            GISTerrainLoaderHGTLoader.OnReadError += OnError;
#if GISTerrainLoaderPdal
            GISTerrainLoaderLASLoader.OnReadError += OnError;
#endif
        }
        void OnDisable()
        {
            if (Cam3D)
                Cam3D.enabled = false;

            GISTerrainLoaderFloatReader.OnReadError -= OnError;
            GISTerrainLoaderTIFFLoader.OnReadError -= OnError;
            GISTerrainLoaderTerraGenLoade.OnReadError -= OnError;
            GISTerrainLoaderDEMPngLoader.OnReadError -= OnError;
            GISTerrainLoaderRawLoader.OnReadError -= OnError;
            GISTerrainLoaderASCILoader.OnReadError -= OnError;
            GISTerrainLoaderBILReader.OnReadError -= OnError;
            GISTerrainLoaderHGTLoader.OnReadError -= OnError;

#if GISTerrainLoaderPdal
            GISTerrainLoaderLASLoader.OnReadError -= OnError;
#endif
        }
        private void CheckForDimensionAndTiles(bool AutoDim)
        {
            if (RuntimePrefs.terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
            {
                if (AutoDim)
                {
                    if (ElevationInfo.data.Terrain_Dimension.x == 0 || ElevationInfo.data.Terrain_Dimension.y == 0)
                    {
                        Debug.LogError("Can't detecte terrain dimension (Check your file projection) and try againe ");
                        StopAllCoroutines();
                        return;
                    }
                    else
                    if (ElevationInfo.data.Terrain_Dimension != new DVector2(0, 0))
                    {
                        RuntimePrefs.TerrainDimensions = new Vector2((float)ElevationInfo.data.Terrain_Dimension.x, (float)ElevationInfo.data.Terrain_Dimension.y);
                    }

                    if (ElevationInfo.data.Tiles != Vector2.zero)
                    {
                        RuntimePrefs.terrainCount = new Vector2Int((int)ElevationInfo.data.Tiles.x, (int)ElevationInfo.data.Tiles.y);
                    }
                    else
                    {
                        StopAllCoroutines();
                    }
                }
                else
                {
                    if (RuntimePrefs.TerrainDimensions.x == 0 || RuntimePrefs.TerrainDimensions.y == 0)
                    {
                        Debug.LogError("Reset Terrain dimensions ... try again  ");
                        StopAllCoroutines();
                        return;
                    }
                    else
        if (RuntimePrefs.TerrainDimensions != new Vector2(0, 0))
                    {
                        RuntimePrefs.TerrainDimensions = new Vector2(RuntimePrefs.TerrainDimensions.x, RuntimePrefs.TerrainDimensions.y);
                    }

                    if (ElevationInfo.data.Tiles != Vector2.zero)
                    {
                        RuntimePrefs.terrainCount = new Vector2Int((int)ElevationInfo.data.Tiles.x, (int)ElevationInfo.data.Tiles.y);
                    }

                }
            }
            else
            {
                if (RuntimePrefs.TerrainDimensions.x == 0 || RuntimePrefs.TerrainDimensions.y == 0)
                {
                    Debug.LogError("Reset Terrain dimensions ... try again  ");
                    StopAllCoroutines();
                    return;
                }
                else
    if (RuntimePrefs.TerrainDimensions != new Vector2(0, 0))
                {
                    RuntimePrefs.TerrainDimensions = new Vector2((float)RuntimePrefs.TerrainDimensions.x, RuntimePrefs.TerrainDimensions.y);
                }

                if (ElevationInfo.data.Tiles != Vector2.zero)
                {
                    RuntimePrefs.terrainCount = new Vector2Int((int)ElevationInfo.data.Tiles.x, (int)ElevationInfo.data.Tiles.y);
                }
                else
                {
                    if (RuntimePrefs.textureMode == TextureMode.WithTexture)
                        Debug.LogError("Can't detecte terrain textures folder ... try again");

                    StopAllCoroutines();
                }
            }

        }
        private TerrainObject CreateTerrain(Transform parent, int x, int y, Vector3 size, Vector3 scale)
        {
            TerrainData tdata = new TerrainData
            {
                baseMapResolution = 32,
                heightmapResolution = 32
            };

            tdata.heightmapResolution = RuntimePrefs.heightmapResolution;
            tdata.baseMapResolution = RuntimePrefs.baseMapResolution;
            tdata.SetDetailResolution(RuntimePrefs.detailResolution, RuntimePrefs.resolutionPerPatch);
            tdata.size = size;

            GameObject GO = Terrain.CreateTerrainGameObject(tdata);
            GO.gameObject.SetActive(true);
            GO.name = string.Format("{0}-{1}", x, y);
            GO.transform.parent = parent;
            GO.transform.position = new Vector3(size.x * x, 0, size.z * y);


            TerrainObject item = GO.AddComponent<TerrainObject>();
            item.Number = new Vector2Int(x, y);
            item.size = size;
            item.ElevationFilePath = TerrainFilePath;
            item.prefs = RuntimePrefs;

            var t = GO.GetComponent<Terrain>();
            item.terrain = t;
            item.terrainData = t.terrainData;

            item.terrain.heightmapPixelError = RuntimePrefs.PixelError;
            item.terrain.basemapDistance = RuntimePrefs.BaseMapDistance;
            item.terrain.materialTemplate = RuntimePrefs.terrainMaterial;


            if (RuntimePrefs.TerrainLayerSet == OptionEnabDisab.Enable)
                item.terrain.gameObject.layer = RuntimePrefs.TerrainLayer;


            item.TextureState = TextureState.Wait;
            item.ElevationState = ElevationState.Wait;

            float prog = ((terrains.GetLength(0) * terrains.GetLength(1) * 100f) / (RuntimePrefs.terrainCount.x * RuntimePrefs.terrainCount.y)) / 100f;

            if (OnProgress != null)
            {
                OnProgress("Generating Terrains", prog);
            }


            return item;
        }
        public void ResetCameraPosition()
        {
            if (Cam3D)
            {
                Cam3D.enabled = false;

                if (GeneratedContainer != null)
                {
                    Cam3D.bound = new Rect(new Vector2(-GeneratedContainer.ContainerSize.x / 2, -GeneratedContainer.ContainerSize.z / 2), new Vector2(GeneratedContainer.ContainerSize.x * 2, GeneratedContainer.ContainerSize.z * 2));
                    var pos = new Vector3(GeneratedContainer.ContainerSize.x / 2, 50000, GeneratedContainer.ContainerSize.z / 2);
                    var elevation = GeoRefConversion.GetHeight(pos);
                    var camPos = new Vector3(pos.x, (elevation + 200) *GeneratedContainer.scale.y, pos.z);
                    Cam3D.cameraTarget = camPos;
                    Cam3D.transform.position = camPos;
                    Cam3D.lastpos = camPos;
                    Cam3D.enabled = true;
                }

            }
        }
        public IEnumerator Try(IEnumerator enumerator)
        {
            while (true)
            {
                object current;
                try
                {
                    if (enumerator.MoveNext() == false)
                    {
                        break;
                    }
                    current = enumerator.Current;
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error while generating terrain : " + ex.Message);
                    OnError();
                    yield break;
                }
                yield return current;
            }
        }

    }
}