# MRTK 2.6 Integration Guide for Unity 2020.2 / MLSDK 0.25.0
### Last Updated: 3/17/2021

## Summary

MRTK-MagicLeap is an extension to Microsoft's open source Mixed Reality Toolkit (MRTK) for Unity. This adds basic compatibility with the Magic Leap platform, including head tracking, Magic Leap hand tracking, and Magic Leap controller support to MRTK. Using this plug-in, applications built using MRTK will be able to use this basic functionality on Magic Leap devices in a few simple steps. Note that some features such as native keyboard support are not yet implemented.  

This is an experimental release, and not all Magic Leap device functionality is exposed via the MRTK. Please see the Notes below for more details.


## Setup Steps

1. Create New Unity 2020.2 Project
2. Change Build Platform to Lumin in File -> Build Settings
3. XR Plug-in Management: Install XR Plugin Management 4.0.1 (verified) or later from Window -> Package Manager, or click "Install XR Plug-In Management" from Edit -> Project Settings -> XR Plug-in Management.
4. XR Plug-in Management: Add Magic Leap to Lumin target
5. In Window -> Package Manager, update the Magic Leap XR Plugin version to 6.2.2 or later.  If not already set to this version, click on the arrow to expand, click “See other versions”, click on the version number (e.g. 6.2.2), then click “Install” on the bottom right of the window.
6. Install MRTK from https://github.com/Microsoft/MixedRealityToolkit-Unity/releases:
   - Import MRTK Foundation 2.6.x. Apply Recommended MRTK Settings from the popup window that appears after doing so.
   - Import MRTK Examples 2.6.x.
8. Open the HandInteractionExamples Scene (Assets -> MRTK -> Examples -> Demos -> HandTracking) and a popup will prompt you to Import TMP Essentials (TextMeshPro).
9. Click Import TMP Essentials. Close the prompt.
10. Import Magic Leap UnityPackage 0.25.0 (and set Lumin SDK in Unity -> Preferences to mlsdk 0.25.0, e.g. /Users/[username]/MagicLeap/mlsdk/v0.25.0). These packages are downloadable from The Lab, available at https://developer.magicleap.com/downloads/lab. Import all files.
11. Import the provided MRTK2.6-MagicLeap Unity Package distribution file and import all files.
12. Open the included HandInteractionExamplesMagicLeap scene.
13. In the scene:
    - Verify on the MixedRealityToolkit object that MagicLeap1InputProfile is being used.
    - Find the MixedRealityInputModule component on the Main Camera, and check the box “Force Module Active”. Remember to do this for any scene that you wish to use the UGUI (Unity GUI) input.
    - Note that for porting your own projects, the key elements will be:
      - Creating a single Camera object in the scene tagged as Main Camera.
      - Setting the MixedRealityToolkit profile to the Magic Leap profile.
      - Turning on “Force Module Active” if using UGUI components
      - Setting the controller and hand settings so the appropriate interactions are being used for your app. See Note 7 below in “Notes”.
14. In Edit -> Project Settings -> Player:
    - Set Company Name and Product Name
    - In Other Settings, set Identification -> Bundle Identifier
       - If needed, check "Override Default Bundle Identifier" to allow text entry.
       - Then edit the Bundle identifier to something like com.yourcompanyname.xxx, make sure it is all lowercase unlike what is prefilled.
       - (optional) Set the Version Code to 1, set the Version Name to 1.0.
    - In Publishing Settings, set the developer certificate. You can generate a developer certificate [here](https://developer.magicleap.com/) under the Publish tab.
15. In Project Settings, navigate to Magic Leap -> Manifest Settings and add the following privileges:
    - ControllerPose
    - GesturesConfig
    - GesturesSubscribe
    - HandMesh (optional)
    - Internet (optional)
    - PcfRead (optional)
    - WorldReconstruction (optional)
    — ---
    - LocalAreaNetwork
16. In scene (can be the HandInteraction Example or other MRTK Scene):
    - Find Camera object
    - Add TrackedPoseDriver if one doesn’t exist, with default settings:
      - Generic XR Device
      - Center Eye - HMD Reference
      - Rotation & Position
      - Update & Before Render
    - Find MixedRealityToolkit object
      - Change Pulldown Profile to “MagicLeap1 Configuration Profile”
17. Build & Run.

## Troubleshooting
- Make sure that Zero Iteration is disconnected if you are trying to interact with the project in the Unity Editor Game window.
- If you created a new Camera object, make sure it is tagged as the Main Camera. 
- If you are trying to port an existing Magic Leap Unity project to MRTK, remove the Magic Leap Main Camera prefab from the scene hierarchy and create a new Camera object tagged as Main Camera before adding the Mixed Reality Toolkit to the scene.


## Notes
1. Experimental. Still subject to errors, use at your own risk. Recommend backing up your project before implementing.
3. Hand Mesh is not fully implemented.
4. Spatial Mapping, Eye Tracking, Voice Recognition & System Keyboard are not currently integrated.
5. Change _CurrentHandSettings or _CurrentControllerSettings in MagicLeapDeviceManager.cs to ignore specific Controllers or Hands as needed prior to building (not yet implemented for dynamically setting at run-time).
6. Note you can use the Trigger, Bumper, and HomeTap as digital input, but that HomeTap down and HomeTap up are both executed simultaneously on HomeTap up; no response is provided for HomeTap down, due to this being a system-level button.
7. You can also use the Touchpad Touch, Touchpad Press, and Touchpad position as input.
8. To use any other MRTK Example Scene, open the MRTK Example scene and follow Step 12 above.
