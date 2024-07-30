using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

public class FogLevel : MonoBehaviour
{
    public GameObject turbine;
    public Volume volume;
    private float dist;
    private float preDist;
    Fog fog;

    private float startingAtt;
    public float minDist;


    // Start is called before the first frame update
    void Start()
    {
        volume.profile.TryGet(out fog);
        startingAtt = fog.meanFreePath.value;
    }

    // Update is called once per frame
    void Update()
    {
        if (this != null && turbine != null)
        {
            dist = Vector3.Distance(this.transform.position, turbine.transform.position);
        }
    
        if (dist < minDist && preDist > dist)
        {
            fog.meanFreePath.value = startingAtt * (Mathf.Min(minDist / Mathf.Max((minDist - dist), 0.1f), 7));
        }
        preDist = dist;
    }
}
