# VR Wind Turbine Underwater Project

**Documentation for VR Wind Turbine Underwater**  
**June 2024**  
**Oliver**

This project is an updated version of an older project by Prof. Stacy Doore.

## Bezier Follow Curve

**Feature:** This script allows the boat to move in a circle.  
**Tutorial:** [YouTube Tutorial](https://www.youtube.com/watch?v=11ofnLOE8pw)  
**Enhancements:** Added functionality to rotate the boat so that one face is always aimed at the center by rotating the boat using the following approach:
- Calculate the center of the curve.
- Update the boat's rotation to face the center of the curve in each frame.

## Space Station Project

Made a VR game by watching YouTube tutorials. This was a crucial part of becoming familiarized with VR design using XR interaction toolkits.  
**Tutorial:** [YouTube Tutorial by Valem](https://youtu.be/QCvqimfrMZw?si=MYUkZfgPRATdpvgC)  
**Highlights:**
- Unity environment setup and VR game features.
- Implement continuous movement, turning, teleportation, grabbing, virtual buttons, wheels, and levers.

## Updating the Old Project

Changed the old VR player to the XR origin. Implemented teleportation, continuous movement, and continuous turning.  
**Enhancements:**
- Implemented teleporting to different locations by pushing a button on the boat.
- Added select entered interactable event in the XR simple interactable in the Push button.
- Added event listeners to handle teleportation logic when the button is pressed.
- Added the bezier follow curve to the boat to move in a circle.
- Ensured the player can go with the boat after teleporting onto it without falling into the water.
- Added new features to the BezierFollow code.
- Added collision area to the boat to allow the player to move with it and turn the camera with the boat's turn.

**Related Scripts:** `ButtonPushTeleport`, `BoatBezierFollow`  
(All teleportation-related elements are detailed in Valem’s tutorial.)

## Underwater Environment Setup

**Feature:** Set up seamless transitions above and below water using Unity's built-in ocean system.  
**Tutorial:** [Unity HDRP Water System](https://unity.com/blog/engine-platform/new-hdrp-water-system-in-2022-lts-and-2023-1)  
**Enhancements:**
- Updated the project from Unity 2020 to Unity 2022.
- Followed Unity's tutorial to set up the Ocean scene.
- Customized the ocean's appearance by changing the color, wind speed, etc.

## Platform Teleportation

**Feature:** Teleport the player and the platform to specific locations in the ocean.  
**Challenges:** Ensured the player remains on the platform and the teleportation follows the order of locations.  
**Related Scripts:** `PlatformTeleport`

## Water Physics in Unity

**Feature:** Added scripts to enable objects with a rigid body to float on the ocean.  
**Tutorial:** [YouTube Tutorial](https://youtu.be/v7ag-NeSMSQ?si=MtTc-zf22QdLT3)  
**Related Scripts:** `OliverFloater`
