using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ButtonPushTeleport : MonoBehaviour
{
    public Transform[] teleportLocations;
    public Transform[] boatLocations;
    public Transform boatTransform;
    public Transform oldTurbine;
    public Transform newTurbineLocation;
    private int currentPlayerLocation = 0;
    private int currentBoatLocation = 0;

    public void TeleportPlayer()
    {
        if (teleportLocations.Length > 0 && boatLocations.Length > 0)
        {
            // Update boat location first
            if (boatTransform != null)
            {
                currentBoatLocation = (currentBoatLocation + 1) % boatLocations.Length;
                boatTransform.position = boatLocations[currentBoatLocation].position;
            }

            // Then update player location
            currentPlayerLocation = (currentPlayerLocation + 1) % teleportLocations.Length;
            this.transform.position = teleportLocations[currentPlayerLocation].position;

            // Move the turbine if conditions are met
            if (currentPlayerLocation >= 2 && currentBoatLocation >= 4 && oldTurbine != null)
            {
                oldTurbine.position = newTurbineLocation.position;
            }
        }
    }
}