/*     Unity GIS Tech 2020-2021      */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderSaveLoadPrefs
    {
        private const string Key = "GTL_";

        #region LoadingPrefs
        public static int LoadPref(string id, int defVal)
        {
            string key = Key + id;
            return EditorPrefs.HasKey(key) ? EditorPrefs.GetInt(key) : defVal;
        }
        public static float LoadPref(string id, float defVal)
        {
            string key = Key + id;
            return EditorPrefs.HasKey(key) ? EditorPrefs.GetFloat(key) : defVal;
        }
        public static string LoadPref(string id, string defVal)
        {
            string key = Key + id;
            return EditorPrefs.HasKey(key) ? EditorPrefs.GetString(key) : defVal;
        }
        public static Vector2 LoadPref(string id, Vector2 defVal)
        {
            return new Vector2(LoadPref(id + "_X", defVal.x), LoadPref(id + "_Y", defVal.y));
        }
        public static DVector2 LoadPref(string id, DVector2 defVal)
        {
            return new DVector2(double.Parse(LoadPref(id + "_X", defVal.x.ToString())), double.Parse((LoadPref(id + "_Y", defVal.y.ToString()))));
        }
        public static Vector3 LoadPref(string id, Vector3 defVal)
        {
            return new Vector3(LoadPref(id + "_X", defVal.x), LoadPref(id + "_Y", defVal.y), LoadPref(id + "_Z", defVal.z));
        }
        public static Color LoadPref(string id, Color defVal)
        {
            return new Color(LoadPref(id + "_R", defVal.r), LoadPref(id + "_G", defVal.g), LoadPref(id + "_B", defVal.b), LoadPref(id + "_A", defVal.a));
        }
        private static Texture2D LoadPrefTexture(string id, Texture2D defVal)
        {
            int TexID = LoadPref(id, -1);
            if (TexID == -1) return defVal;
            Texture2D Tex = EditorUtility.InstanceIDToObject(TexID) as Texture2D;
            return Tex ?? defVal;
        }
        public static GISTerrainLoaderTerrainLayer LoadPref(string id, GISTerrainLoaderTerrainLayer defVal)
        {
            return new GISTerrainLoaderTerrainLayer
            (
                LoadPrefTexture(id + "_Diffuse", defVal.Diffuse)  ,
                  LoadPrefTexture(id + "_NormalMap", defVal.NormalMap),
                    LoadPref(id + "_TextureSize", defVal.TextureSize),
                          LoadPref(id + "_X_Height", defVal.X_Height),
                                LoadPref(id + "_Y_Height", defVal.Y_Height),
                                      LoadPref(id + "_ShowHeight", true)
            );
        }
        public static bool LoadPref(string id, bool defVal)
        {
            string key = Key + id;
            return EditorPrefs.HasKey(key) ? EditorPrefs.GetBool(key) : defVal;
        }
        public static List<GISTerrainLoaderTerrainLayer> LoadPref(string id, List<GISTerrainLoaderTerrainLayer> defVals)
        {
            string key = Key + id + "_Count";
            if (EditorPrefs.HasKey(key))
            {
                int count = EditorPrefs.GetInt(Key + id + "_Count");
                List<GISTerrainLoaderTerrainLayer> retVal = new List<GISTerrainLoaderTerrainLayer>();
                for (int i = 0; i < count; i++)
                {
                    GISTerrainLoaderTerrainLayer TL = LoadPref(Key + id + "_" + i,new GISTerrainLoaderTerrainLayer()) ;
                    retVal.Add(TL);
                }
                   
                return retVal;
            }
            return defVals;
        }
        public static List<GameObject> LoadPref(string id, List<GameObject> defVals)
        {
            string key = Key + id + "_Count";
            if (EditorPrefs.HasKey(key))
            {
                int count = EditorPrefs.GetInt(Key + id + "_Count");
                List<GameObject> retVal = new List<GameObject>();
                for (int i = 0; i < count; i++) retVal.Add(EditorUtility.InstanceIDToObject(EditorPrefs.GetInt(Key + id + "_" + i)) as GameObject);
                return retVal;
            }
            return defVals;
        }
        public static List<GISTerrainLoaderSO_GrassObject> LoadPref(string id, List<GISTerrainLoaderSO_GrassObject> defVals)
        {
            string key = Key + id + "_Count";
            if (EditorPrefs.HasKey(key))
            {
                int count = EditorPrefs.GetInt(Key + id + "_Count");
                List<GISTerrainLoaderSO_GrassObject> retVal = new List<GISTerrainLoaderSO_GrassObject>();
                for (int i = 0; i < count; i++) retVal.Add(EditorUtility.InstanceIDToObject(EditorPrefs.GetInt(Key + id + "_" + i)) as GISTerrainLoaderSO_GrassObject);
                return retVal;
            }
            return defVals;
        }
        public static List<GISTerrainLoaderSO_Tree> LoadPref(string id, List<GISTerrainLoaderSO_Tree> defVals)
        {
            string key = Key + id + "_Count";
            if (EditorPrefs.HasKey(key))
            {
                int count = EditorPrefs.GetInt(Key + id + "_Count");
                List<GISTerrainLoaderSO_Tree> retVal = new List<GISTerrainLoaderSO_Tree>();
                for (int i = 0; i < count; i++) retVal.Add(EditorUtility.InstanceIDToObject(EditorPrefs.GetInt(Key + id + "_" + i)) as GISTerrainLoaderSO_Tree);
                return retVal;
            }
            return defVals;
        }
        public static List<GISTerrainLoaderSO_Road> LoadPref(string id, List<GISTerrainLoaderSO_Road> defVals)
        {
            string key = Key + id + "_Count";
            if (EditorPrefs.HasKey(key))
            {
                int count = EditorPrefs.GetInt(Key + id + "_Count");
                List<GISTerrainLoaderSO_Road> retVal = new List<GISTerrainLoaderSO_Road>();
                for (int i = 0; i < count; i++) retVal.Add(EditorUtility.InstanceIDToObject(EditorPrefs.GetInt(Key + id + "_" + i)) as GISTerrainLoaderSO_Road);
                return retVal;
            }
            return defVals;
        }
        public static List<UnityEngine.Object> LoadPref(string id, List<UnityEngine.Object> defVals)
        {
            string key = Key + id + "_Count";
            if (EditorPrefs.HasKey(key))
            {
                int count = EditorPrefs.GetInt(Key + id + "_Count");
                List<UnityEngine.Object> retVal = new List<UnityEngine.Object>();
                for (int i = 0; i < count; i++) retVal.Add(EditorUtility.InstanceIDToObject(EditorPrefs.GetInt(Key + id + "_" + i)) as UnityEngine.Object);
                return retVal;
            }
            return defVals;
        }
        #endregion

        #region SavingPrefs
        public static void SavePref(string id, float val)
        {
            EditorPrefs.SetFloat(Key + id, val);
        }
        public static void SavePref(string id, string val)
        {
            EditorPrefs.SetString(Key + id, val);
        }
        public static void SavePref(string id, int val)
        {
            EditorPrefs.SetInt(Key + id, val);
        }
        public static void SavePref(string id, Vector2 val)
        {
            SavePref(id + "_X", val.x);
            SavePref(id + "_Y", val.y);
        }
        public static void SavePref(string id, DVector2 val)
        {
            SavePref(id + "_X", val.x.ToString());
            SavePref(id + "_Y", val.y.ToString());
        }
        public static void SavePref(string id, Vector3 val)
        {
            SavePref(id + "_X", val.x);
            SavePref(id + "_Y", val.y);
            SavePref(id + "_Z", val.z);
        }
        public static void SavePref(string id, Color val)
        {
            SavePref(id + "_R", val.r);
            SavePref(id + "_G", val.g);
            SavePref(id + "_B", val.b);
            SavePref(id + "_A", val.a);
        }
        public static void SavePref(string id, bool val)
        {
            EditorPrefs.SetBool(Key + id, val);
        }
        public static void SavePref(string id, GISTerrainLoaderTerrainLayer val)
        {
            SetPref(id + "_Diffuse", val.Diffuse);
            SetPref(id + "_NormalMap", val.NormalMap);
            SavePref(id + "_TextureSize", val.TextureSize);
            SavePref(id + "_X_Height", val.X_Height);
            SavePref(id + "_Y_Height", val.Y_Height);
            SavePref(id + "_ShowHeight", true);
        }
        public static void SavePref(string id, List<GameObject> vals)
        {
            if (vals != null)
            {
                EditorPrefs.SetInt(Key + id + "_Count", vals.Count);

                for (int i = 0; i < vals.Count; i++)
                {
                    Object val = vals[i];
                    if (val != null) EditorPrefs.SetInt(Key + id + "_" + i, val.GetInstanceID());
                }
            }
            else EditorPrefs.SetInt(Key + id + "_Count", 0);
        }
        public static void SavePref(string id, List<GISTerrainLoaderSO_GrassObject> vals)
        {
            if (vals != null)
            {
                EditorPrefs.SetInt(Key + id + "_Count", vals.Count);

                for (int i = 0; i < vals.Count; i++)
                {
                    Object val = vals[i];
                    if (val != null) EditorPrefs.SetInt(Key + id + "_" + i, val.GetInstanceID());
                }
            }
            else EditorPrefs.SetInt(Key + id + "_Count", 0);
        }
        public static void SavePref(string id, List<GISTerrainLoaderSO_Tree> vals)
        {
            if (vals != null)
            {
                EditorPrefs.SetInt(Key + id + "_Count", vals.Count);

                for (int i = 0; i < vals.Count; i++)
                {
                    Object val = vals[i];
                    if (val != null) EditorPrefs.SetInt(Key + id + "_" + i, val.GetInstanceID());
                }
            }
            else EditorPrefs.SetInt(Key + id + "_Count", 0);
        }
        public static void SavePref(string id, List<GISTerrainLoaderSO_Road> vals)
        {
            if (vals != null)
            {
                EditorPrefs.SetInt(Key + id + "_Count", vals.Count);

                for (int i = 0; i < vals.Count; i++)
                {
                    Object val = vals[i];
                    if (val != null) EditorPrefs.SetInt(Key + id + "_" + i, val.GetInstanceID());
                }
            }
            else EditorPrefs.SetInt(Key + id + "_Count", 0);
        }
        private static void SetPref(string id, Object val)
        {
            if (val != null) EditorPrefs.SetInt(Key + id, val.GetInstanceID());
        }
        public static void SavePref(string id, List<UnityEngine.Object> vals)
        {
            if (vals != null)
            {
                EditorPrefs.SetInt(Key + id + "_Count", vals.Count);

                for (int i = 0; i < vals.Count; i++)
                {
                    Object val = vals[i];
                    if (val != null) EditorPrefs.SetInt(Key + id + "_" + i, val.GetInstanceID());
                }
            }
            else EditorPrefs.SetInt(Key + id + "_Count", 0);
        }
        public static void SavePref(string id, List<GISTerrainLoaderTerrainLayer> vals)
        {
            if (vals != null)
            {
                EditorPrefs.SetInt(Key + id + "_Count", vals.Count);

                for (int i = 0; i < vals.Count; i++)
                {
                    var val = vals[i];
                    if (val != null) SavePref(Key + id + "_" + i, val);
                }
            }
            else EditorPrefs.SetInt(Key + id + "_Count", 0);
        }
        #endregion

    }
}