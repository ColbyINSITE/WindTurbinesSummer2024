using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscillation : MonoBehaviour
{
    // oscillate on the y-z plane
    private Vector3 oscAxis = new Vector3(1, 0, 0);

    // add noise to the oscillation
    private float noise;

    // 0.036 Hz frequency
    public float frequency;

    // 2 degree rotation
    public float max_degree_rotation;

    // Update is called once per frame
    void Update()
    {
        noise = (Mathf.PerlinNoise(Time.time, 0) - 0.5f);
        transform.rotation = Quaternion.Euler(Mathf.Sin(2 * Mathf.PI * frequency * Time.time + noise) * max_degree_rotation, 0 ,0);
    }
}
