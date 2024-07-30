/*     Unity GIS Tech 2020-2021      */
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
 

namespace GISTech.GISTerrainLoader
{
    public enum PdalRader { LAS, Raster };
    public enum Unit { Degree, Grad, Radian, Meter };

    public class GISTerrainLoaderProjections  
    {
 

    }

    public class GTLGeographicCoordinateSystem
    {
        public int EPSG_Code;
        public string GEOGCSProjection="";
        public string Name="";
        public GTLRefDatum Datum;
        public GTLPriMem PrimeMeridian;
        public GTLUnitPr Units;
        public GTLProjection projectionData;


        public GTLUTMdata UTMData;
        public GTLLambertData LambertData;
        
        public GTLGeographicCoordinateSystem(string m_GEOGCSProjection, bool UTM=false)
        {
            GEOGCSProjection = m_GEOGCSProjection;

            if(UTM)
            UTMData = new GTLUTMdata();

            LambertData = new GTLLambertData();

        }
        public GTLGeographicCoordinateSystem()
        {
 
        }
    }

    public class GTLRefDatum
    {
        public string Name;
        public GTLRefSpheroid Spheroid;


    }
    public class GTLRefSpheroid
    {
        public string Name;
        public double InverseFlatteningRatio;
        public double Axis;

    }
    public class GTLPriMem
    {
        public string Name;
        public double Longitude;
    }
    public class GTLUnitPr
    {
        public string Name;
        public double ConversionFactor;
    }
    public class GTLProjection
    {
        public string Name;
        public GTLUnitPr Units;
        public List<GTLProjectionParameter> Parameters;

        public double GetParameter(string name)
        {
            name = name.ToLower();
            foreach (GTLProjectionParameter param in Parameters)
            {
                if (param.Name.ToLower() == name)
                {
                    return param.Value;
                }
            }
            return 0;
        }
    }
    public class GTLProjectionParameter
    {
        public string Name;
        public double Value;

        public GTLProjectionParameter() { }

        public GTLProjectionParameter(string name, double val)
        {
            this.Name = name;
            this.Value = val;

        }

    }
    public class GTLUTMdata
    {
        public int ZoneNum;
        public string ZoneLet;
        public GTLUTMdata ()
        {
            ZoneNum = 0;
            ZoneLet = "";
        }

    }
    public class GTLLambertData
    {
        public LambertZone Lambertzone = LambertZone.Lambert93;
    }
}