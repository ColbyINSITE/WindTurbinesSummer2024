/*     Unity GIS Tech 2020-2021      */
namespace GISTech.GISTerrainLoader
{
    public enum Projections
    {
        Geographic_LatLon_Decimale = 0,
        Geographic_LatLon_DegMinSec,
        UTM,
        UTM_MGRUTM,
        Lambert,
        UTM_Nad83

    }
    public enum TerrainOriginMode
    {
        FromEditor = 0,
        FromPlayMode
    }
    public enum RealWorldElevation
    {
        Elevation = 0, // Real world elevation on a position
        Height, //  Height from the ground
        Altitude, // Real world elevation + Height
    }
    public enum SetElevationMode
    {
        OnTheGround = 0, // Relative to the ground 
        RelativeToTheGround, // Relative to the ground 
        RelativeToSeaLevel// Relative to the sea level
    }
    public enum ReadingMode
    {
        Full = 0,
        SubRegion
    }
    public enum ProjectionMode
    {
        Auto = 0,
        Custom
    }
    public enum TerrainElevation
    {
        ExaggerationTerrain = 1,
        RealWorldElevation = 0,
    }

     public enum TerrainDimensionsMode
    {
        AutoDetection = 1,
        Manual = 0
    }
    public enum TerrainMaterialMode
    {
        Standard = 0,
        Custom
    }
    public enum OptionEnabDisab
    {
        Disable = 0,
        Enable
    }
    public enum FixOption
    {
        Disable,
        AutoFix ,
        ManualFix

    }
    public enum TextureMode
    {
        WithoutTexture,
        WithTexture,
        ShadedRelief,
        Splatmapping
    }
    public enum ShaderType {
        ColorRamp=0,
        ElevationGrayScale,
        ElevationInversGrayScale,
        Slop,
        SlopInvers,
        NormalMap
    };
    public enum ShaderColor
    {
        GradientColor,
        MainGradient,
        NegativeGradient,
        BlackToWhite,
        GreyToWhite,
        GreyToBlack,
    };

    public enum TexturesLoadingMode
    {
        AutoDetection = 1,
        Manual = 0
    }
    public enum TextureSource
    {
        Globalmapper,
        SASPlanet
    }

    public enum GeneratorState
    {
        idle,
        Generating
    }

    public enum ElevationState
    {
        Wait,
        Loading,
        Loaded,
        Error
    }
    public enum TextureState
    {
        Wait,
        Loading,
        Loaded,
        Error
    }
    public enum RoadGenerationType
    {
        Line,
        EasyRoad3D,
    }

    public enum VectorType
    {
        OpenStreetMap = 0,
        ShapeFile = 1,
        GPX=2
        //KML=3
    }

    public enum BuildingRoofType
    {
        dome=0,
        flat
    }
    public enum TiffElevationSource
    {
        DEM = 0,
        GrayScale,
        BandsData
    }
    
}