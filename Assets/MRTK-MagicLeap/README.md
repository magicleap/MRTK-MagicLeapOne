# MRTK 2.7 Integration Guide for Unity 2020.2.x / MLSDK 0.25.0
### Last Updated: 6/17/2021  

## Overview 
[MRTK-MagicLeap](https://github.com/magicleap/MRTK-MagicLeap) is an extension to Microsoft's open source Mixed Reality Toolkit ([MRTK](https://docs.microsoft.com/en-us/windows/mixed-reality/develop/unity/mrtk-getting-started "Microsoft MRTK documentation ")) for Unity. It adds compatibility for the Magic Leap platform, including head and hand tracking, and controller support. Using this plug-in, applications built using MRTK will be able to add support for Magic Leap devices. Some features such as native keyboard support are not yet implemented. 

## In this Article
- [Features](#features)
- [Installation Guide](#installation-guide)
- [Examples and Scene Settings](#examples-and-scene-settings)
- [Build Your Application](#build-your-application)
- [Notes](#notes)

# Features
This following MRTK Features are supported in the latest release. 

Supported Features in MRTK 
- Hand Tracking
- Hand Skeleton / Gestures
- Magic Leap Controller
- Voice Input (Using Google speech to text)
- Scene Meshing
- Gaze Input (aka Eye Tracking)

Unsupported Features in MRTK
- Hand Meshing
- Planes Detection
- Native Keyboard
   
This package also includes support for Zero Iteration, which allows you to test your application without having to build and install it on your device.  

# Installation Guide
## Platform Settings
To use Magic Leap's MRTK feature, your Unity project needs to have Lumin as the build target.
1. Create a new Unity 2020.2.x project.
1. In the menu, go to **File** and select **Build Settings**.
1. Under **Platform**, select **Lumin**.
1. Click **Switch Platform**.  

## Configure XR Plugin Management
Configure Unity's XR plug-in framework to integrate Magic Leap into Unity’s engine and make full use of its features. 

1. To install the latest version of the **Magic Leap XR Plugin**, open the Package Manager **Window > Package Manager**.
1. Select **Unity Registry** from the package registry dropdown to view all packages provided by Unity.
1. Locate the **Magic Leap XR Plugin**, select the arrow to expand the package options then **See other versions**. Select version **6.2.2** and click **Install**.
1. After the package is installed, open the **XR Plugin Management** settings **File > Build Settings > Player Settings > XR Plug-in Management** and enable **Magic Leap** as a Plug-in Provider on the **Lumin** Platform.  

## Install the Package Dependencies 
Before importing MRTK Magic Leap, install the package's dependencies - The Magic Leap SDK, MRTK Foundations, MRTK Examples, and Text Mesh Pro. 
#### Magic Leap Unity SDK

1. From the menu, go to **Assets > Import Package > Custom Package**.
1. Find and Open the **Magic Leap Unity Package**. ex:  `C:/Users/YourUserName/MagicLeap/tools/unity/0.25.0/MagicLeap.unitypackage`
1. In the **Import Unity Package** window, make sure everything is selected, click **Import**.  

\* If you cannot locate the Magic Leap SDK Unity Package, make sure that you've installed the Unity bundle from [The Lab](https://developer.magicleap.com/downloads/lab). 
#### Microsoft Mixed Reality Toolkit 2.7

1. Download version 2.7.x of **MRTK Foundation** and **MRTK Examples** from the MRTK [GitHub](https://github.com/Microsoft/MixedRealityToolkit-Unity/releases).
1. Import the **MRTK Foundation 2.7.x** package into your Unity project. Apply the recommended MRTK settings from the popup window that appears after doing so.
1. Next, import the **MRTK Examples 2.7.x** package into your project.  

#### Text Mesh Pro
1. Import the **TMP Essential Resources** by selecting **Window > TextMeshPro > Import TMP Essential Resources**.  

## Install MRTK Magic Leap
After all of the dependencies are installed, download and install the MRTK Magic Leap package.
1. Download the latest version of the MRTK Magic Leap package from the [release sections](https://github.com/magicleap/MRTK-MagicLeap/releases) on the GitHub page.
1. Import the [MRTK1.3-MagicLeap Unity Package](https://github.com/magicleap/MRTK-MagicLeap/releases/) by going to **Assets > Import Package > Custom Package**. Import all of its contents.

\* If you are upgrading from an previous version, follow the instructions provided in the [Upgrade Guide](#upgrade-guide).

# Examples and Scene Settings
This project includes two pre-configured scenes:
- SpatialAwarenessMeshDemoMagicLeap
- HandInteractionExamplesMagicLeap

These scenes do not require additional configuration and serve as a blueprint for Magic Leap's MRTK integration. They can also be used as a guide for creating custom MRTK Configuration Profiles.

## Add support to existing scenes.
If you want to test other MRTK Scenes, additional configuration is required. The steps below explain how to configure other scenes to support the Magic Leap platform. Please note that some Magic Leap features are still not supported. See the [limitations](#limitations) section for more details.

1. Open the **HandInteractionExamples** scene.
1. Select the **MixedRealityToolkit** in the Hierarchy. Set the configuration to the **MagicLeap1 ConfigurationProfile**.
1. Select the **Main Camera** and in the **MixedRealityInputModule** select **Force Module Active** is enable. This enables interaction with Unity's canvas components.
1. Verify that the Camera has a **TrackedPoseDriver** with the default values:  
    - Generic XR Device
    - Center Eye - HMD Reference
    - Rotation & Position
    - Update & Before Render
1. Some examples may require additional controller and hand configuration to insure proper interactions - see the [Input](#input) section for details.  

# Build Your Application
## Publishing Settings
1. First, set the project's identity and certificate settings. Navigate to **Edit > Project Settings > Player**.  
    1. Set **Company Name** and **Product Name**.  
    2. Under **Other Settings > Identification > Bundle Identifier**, enable **Override Default Bundle Identifier** to allow text entry.  
    3. Set the **Bundle Identifier**. Make sure to use lowercase letters only. ex: *com.yourcompanyname.xxx*  
    4. Under **Publishing Settings**, set the developer cert. You can generate a developer certificate by going to the [Publish](https://developer.magicleap.com/dashboard) section of the Magic Leap website and selecting certificates.  

## Permissions
1. For your application to have access certain Magic Leap features, you need to configure your project's permissions. In **Project Settings** window, navigate to **Magic Leap > Manifest Settings** and add the following privileges:
   - ControllerPose
   - GesturesConfig
   - GesturesSubscribe
   - HandMesh (optional)
   - Internet (optional)
   - PcfRead (optional)
   - WorldReconstruction
   - LocalAreaNetwork

## Build and Run
1. Open the **Build Settings** window and add the scenes you want to build to the **Scenes In Build**. 
1. Select **Build And Run**.

**Note**: To publish your app to the store, the **Version Name**, in the **Player Settings > Identification** needs to be set to at least one decimal place (ex: *Version Name: 1.0*). This step is not required for development builds.


# Upgrade Guide
If you are upgrading from a previous version of the MRTK Magic Leap Package follow the steps below. 

1. Delete existing the following folders:  
    - `Assets/MRTK-MagicLeap`
    - `Assets/MagicLeap-Tools` (If present)
1. If you are upgrading to a newer version of both MRTK and the MRTK Magic Leap, follow Microsoft's [MRTK Upgrade Guide](https://docs.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/updates-deployment/updating?view=mrtkunity-2021-05) to update the MRTK components. Otherwise, continue to the next step.
1. Download and install the MRTK Magic Leap package by following the [Install MRTK Magic Leap](#install-mrtk-magic-leap) instructions.  

# Notes

## Spatial Awareness
- Magic Leap's specific mesh features such as Vertex Confidence and Planarization can be enabled in the inspector when selecting the Magic Leap's SpatialAwarenessMeshOberverProfile.
- The MRTK spatial mesh shaders require **Force Multipass** to be enabled. This can be done in Project Settings > Magic Leap Settings.
- Meshing Settings can be changed at runtime using Magic Leap's MeshingSettings API.
- Scene Understanding and Plane Finding are not supported.

## Input
- To ignore specific Controllers or Hands, edit the `_CurrentHandSettings` and `_CurrentControllerSettings` values before building in the `MagicLeapDeviceManager.cs` script. Run-time settings changes are not yet implemented.
- You can use the `Trigger`, `Bumper`, and `HomeTap` as digital inputs. However,  no response is provided for `HomeTap down`. Instead, `HomeTap down` and `HomeTap up` are both executed simultaneously on `HomeTap up`. This is due to it being a system-level button.
- You can also use the *Touchpad Touch*, *Touchpad Press*, and *Touchpad position* as input.

## Limitations
- This package is experimental and subject to errors, use at your own risk. We recommend backing up your project before implementing.
- The Hand Mesh feature is not fully implemented.
- The System Keyboard is not currently integrated.
- Run-time input settings changes are not yet implemented.
- No response is provided for the *HomeTap down* input. This is due to it being a system-level button.
