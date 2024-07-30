using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatKeyboardMovement : MonoBehaviour
{
    // Boat Alignment Script
    public BoatAlignNormal boat;

    public float throttle_max;
    public float steer_max;

    public float throttle_sensitivity;
    public float steer_sensitivity;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            boat._throttleBias = Mathf.Min(throttle_max, boat._throttleBias + throttle_sensitivity);
        }
        else
        {
            boat._throttleBias = Mathf.Max(0, boat._throttleBias - throttle_sensitivity);
        }


        if (Input.GetKey(KeyCode.RightArrow))
        {
            boat._steerBias = Mathf.Min(steer_max, boat._steerBias + steer_sensitivity);
        }
        else
        {
            boat._steerBias = Mathf.Max(0, boat._steerBias - steer_sensitivity);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            boat._steerBias = Mathf.Min(steer_max, boat._steerBias - steer_sensitivity);
        }
        else
        {
            boat._steerBias = Mathf.Min(0, boat._steerBias + steer_sensitivity);
        }


    }
}
