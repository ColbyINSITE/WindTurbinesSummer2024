using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleLights : MonoBehaviour
{
    public GameObject largeLight;
    public GameObject smallLights;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)){
            largeLight.SetActive(!largeLight.activeInHierarchy);
            smallLights.SetActive(!smallLights.activeInHierarchy);
        }
    }
}
