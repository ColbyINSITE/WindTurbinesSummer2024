using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayer : MonoBehaviour
{
    public Transform[] player_locations;
    public Transform[] boat_locations;
    public Transform boat;
    public Transform player;
    private int player_currentLocation = 0;
    private int boat_currentLocation = 0;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && player_locations.Length > 0 && boat_locations.Length > 0)
        {
            Debug.Log("Key Down");
            if (player_currentLocation >= 2)
            {

                boat.position = boat_locations[(++boat_currentLocation) % boat_locations.Length].position;
                player.position = boat.position;
            }
            else
            {
                player.position = player_locations[(++player_currentLocation) % player_locations.Length].position;
            }
        }
    }
}
