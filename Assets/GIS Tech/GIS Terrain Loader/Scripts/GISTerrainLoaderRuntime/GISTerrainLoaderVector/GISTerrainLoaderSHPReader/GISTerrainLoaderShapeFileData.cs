/*     Unity GIS Tech 2020-2021      */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderShapeFileData
    {
        public ShapeType ShapeType;
        public Dictionary<string, string> DataBase
        {
            get;
            set;
        }
        public string Id
        {
            get;
            set;
        }
        public Dictionary<string, string> FiltredDataBase
        {
            get;
            set;
        }

        public GTLGeographicCoordinateSystem CoordinateReferenceSystem = null;

        public GISTerrainLoaderShpRecord ShapeRecord;

        public GISTerrainLoaderShapeFileData(ShapeType m_ShapeType, GISTerrainLoaderShpRecord m_shapeRecord, GTLGeographicCoordinateSystem m_CoordinateReferenceSystem)
        {
            ShapeType = m_ShapeType;
            ShapeRecord = m_shapeRecord;
            CoordinateReferenceSystem = m_CoordinateReferenceSystem;

            DataBase = new Dictionary<string, string>();
            FiltredDataBase = new Dictionary<string, string>();

            foreach (var data in m_shapeRecord.DataBase)
            {
                try
                {
                    var tagKeyEnum = data.Col_Name;

                    if (data.Col_Name.Trim() == "id")
                        Id = data.Row_Value;

                    if (tagKeyEnum != "None" && !string.IsNullOrEmpty(data.Row_Value.Trim()))
                    {
                        if (!DataBase.ContainsKey(tagKeyEnum))
                            DataBase.Add(tagKeyEnum, data.Row_Value.Trim());


                    }

                }
                catch (Exception)
                {
                    continue;
                }
            }
        }


    }
}