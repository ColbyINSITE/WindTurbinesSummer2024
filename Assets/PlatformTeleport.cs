using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlatformTeleport : MonoBehaviour
{
    public Transform playerTransform; // The player's transform (XR Origin)
    public Transform platformTransform; // The platform's transform
    public Transform[] teleportLocations; // The specific depth positions in the ocean
    public XRSimpleInteractable teleportButton; // The XR Simple Interactable button

    private int currentTeleportIndex = 0; // Start at 0 to go to the first location initially

    private void OnEnable()
    {
        teleportButton.selectEntered.AddListener(OnTeleportButtonPressed);
    }

    private void OnDisable()
    {
        teleportButton.selectEntered.RemoveListener(OnTeleportButtonPressed);
    }

    private void OnTeleportButtonPressed(SelectEnterEventArgs args)
    {
        TeleportPlatformAndPlayer();
    }

    public void TeleportPlatformAndPlayer()
    {
        if (teleportLocations == null || teleportLocations.Length == 0)
        {
            Debug.LogError("No teleport locations set.");
            return;
        }

        // Debug the current index and location before teleporting
        Debug.Log($"Current teleport index: {currentTeleportIndex}");
        Debug.Log($"Teleporting to location: {teleportLocations[currentTeleportIndex].position}");

        // Calculate the offset of the player relative to the platform
        Vector3 playerOffset = playerTransform.position - platformTransform.position;

        // Teleport the platform to the new location
        platformTransform.position = teleportLocations[currentTeleportIndex].position;

        // Teleport the player to the new location, maintaining the same offset relative to the platform
        playerTransform.position = platformTransform.position + playerOffset;

        Debug.Log($"Player and platform teleported to: {teleportLocations[currentTeleportIndex].position}");

        // Update the teleport index for the next teleportation
        currentTeleportIndex++;
        if (currentTeleportIndex >= teleportLocations.Length)
        {
            currentTeleportIndex = 0;
        }

        // Debug the next index
        Debug.Log($"Next teleport index set to: {currentTeleportIndex}");
    }
}
