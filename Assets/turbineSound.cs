using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class turbineSound : MonoBehaviour
{
    public Transform[] teleportLocations;
    public Transform boat;

    // an array to store Volume in AudioSource of turbines' MotorSound
    // 5 mile, 2.5 mile, 1 mile, 400f, within array distance respectively
    public float[] volumes;

    // an array to store Max Distance in 3D Sound Settings turbines' MotorSound
    // 5 mile, 2.5 mile, 1 mile, 400f, within array distance respectively
    public float[] maxDis;

    public Transform turbine_1;
    public Transform turbine_2;
    public Transform[] motorSounds;

    // Update is called once per frame
    void Update()
    {
        // If player is at the beach (5 mile distance)
        if (this.transform.position == teleportLocations[0].position)
        {
            for (int i = 0; i < motorSounds.Length; i++)
            {
                motorSounds[i].transform.GetComponent<AudioSource>().volume = volumes[0];
                motorSounds[i].transform.GetComponent<AudioSource>().maxDistance = maxDis[0];
            }
        }

        // If player is at 2.5 mile distance
        if (boat.position == teleportLocations[1].position)
        {
            for (int i = 0; i < motorSounds.Length; i++)
            {
                motorSounds[i].transform.GetComponent<AudioSource>().volume = volumes[1];
                motorSounds[i].transform.GetComponent<AudioSource>().maxDistance = maxDis[1];
            }
        }

        // If player is at 1 mile distance
        if (boat.position == teleportLocations[2].position)
        {
            for (int i = 0; i < motorSounds.Length; i++)
            {
                motorSounds[i].transform.GetComponent<AudioSource>().volume = volumes[2];
                motorSounds[i].transform.GetComponent<AudioSource>().maxDistance = maxDis[2];
            }
        }

        // If player is at 400f distance
        if (boat.position == teleportLocations[3].position)
        {
            for (int i = 0; i < motorSounds.Length; i++)
            {
                motorSounds[i].transform.GetComponent<AudioSource>().volume = volumes[3];
                motorSounds[i].transform.GetComponent<AudioSource>().maxDistance = maxDis[3];
            }
        }

        // If player is within the array
        if (boat.position == teleportLocations[4].position)
        {
            for (int i = 0; i < motorSounds.Length; i++)
            {
                motorSounds[i].transform.GetComponent<AudioSource>().volume = volumes[4];
                motorSounds[i].transform.GetComponent<AudioSource>().maxDistance = maxDis[4];
            }
        }

        // After the transit 
        if (turbine_1.position == teleportLocations[5].position)
        {
            for (int i = 0; i < motorSounds.Length; i++)
            {
                motorSounds[i].transform.GetComponent<AudioSource>().volume = volumes[5];
                motorSounds[i].transform.GetComponent<AudioSource>().maxDistance = maxDis[5];
            }
        }
    }
}
