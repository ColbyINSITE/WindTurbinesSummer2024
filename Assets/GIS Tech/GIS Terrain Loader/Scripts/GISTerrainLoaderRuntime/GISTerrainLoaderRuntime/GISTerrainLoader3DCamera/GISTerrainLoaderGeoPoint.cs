/*     Unity GIS Tech 2020-2021      */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderGeoPoint : MonoBehaviour
    {
        [Header("Object Parameters")]
        public bool RandomScale = false;
        [Range(0.1f,10)]
        public float RandomMaxScale = 1;
        public Transform ObjectBody;

        [Header("UI and TextMesh Parameters")]
        public Vector3 Maxscale = new Vector3(10, 10, 10);
        public float MaxDistance = 30f;// Km
        public TextMesh Unite_TextMesh;
        public Transform UIObject;
        private Vector3 Dir1 = new Vector3(1, 360, 1);
        private Camera MainCamera;
        private TerrainContainerObject container;

        // Start is called before the first frame update
        void Start()
        {
            if(RandomScale)
            {
                if(ObjectBody)
                {
                    var RandomScale = Random.Range(0.1f, RandomMaxScale);
                    ObjectBody.localScale = new Vector3(RandomScale, RandomScale, RandomScale);
                }
            }

            MainCamera = Camera.main;
            container = FindObjectOfType<TerrainContainerObject>();
            UIObject.localScale = new Vector3(1, 1, 1);
            UpdateUI();

        }
        void FixedUpdate()
        {
            UpdateUI();
        }
        void UpdateUI()
        {
            if (UIObject.hasChanged)
            {
                if (UIObject && MainCamera)
                {
                    var m_dis = MaxDistance * 1000 * container.scale.y;
                    var dis = Vector3.Distance(MainCamera.transform.position, UIObject.position);
                    var y_pos = ((dis/1000) * container.scale.y) * 2;
                    if (y_pos < 5) y_pos = 5;

                    UIObject.transform.localPosition = new Vector3(UIObject.transform.localPosition.x, y_pos, UIObject.transform.localPosition.z);
                    if (dis > m_dis)
                        UIObject.localScale = Maxscale;
                    else
                    {

                        var value = dis * Maxscale.x / m_dis;
                        if (value > 1)
                            UIObject.localScale = new Vector3(value, value, value);
                        else if (value < 1)
                        {
                            value = 1; UIObject.localScale = new Vector3(value, value, value);
                        }

                    }
                    LookAt();
                }
            }

        }
        void LookAt()
        {
            var target = MainCamera.transform.position;
            target.y = UIObject.position.y;

            if (MainCamera.gameObject)
            {
                if (MainCamera.transform.localRotation.eulerAngles.x > 70)
                {
                    UIObject.rotation = Quaternion.LookRotation(Dir1, new Vector3(0, 1, 1));
                }

                else UIObject.LookAt(target);
            }
            else
                UIObject.LookAt(target);

        }
        public void SetName(string Unite_Name)
        {
            Unite_TextMesh.text = Unite_Name;
        }
 
    }
}