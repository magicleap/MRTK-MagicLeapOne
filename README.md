# MRTK 2.7 Integration Guide for Unity 2020.3.x / MLSDK 0.26.0
### Last Updated: 11/12/2021  

## Overview 
[MRTK-MagicLeap](https://github.com/magicleap/MRTK-MagicLeap) is an extension to Microsoft's open source Mixed Reality Toolkit ([MRTK](https://docs.microsoft.com/en-us/windows/mixed-reality/develop/unity/mrtk-getting-started "Microsoft MRTK documentation")) for Unity. It adds compatibility for the Magic Leap platform, including head and hand tracking, and controller support. Using this plug-in, applications built using MRTK will be able to add support for Magic Leap devices. Some features such as native keyboard support are not yet implemented. View the [notes](#notes) for development tips and information about limitations.

## In this Article
- [Features](#features)
- [Installation Guide](#installation-guide)
- [Examples and Scene Settings](#examples-and-scene-settings)
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

## Installation Guide

## Prerequisites

The install instructions assume you that you have completed the Magic Leap 1 development enviornment setup, and that your project is configured to run properly on the Magic Leap.

- [Set Up the Development Environment](https://developer.magicleap.com/en-us/learn/guides/unity-setup-intro)


## Setup Instructions

1. Import MRTK https://github.com/microsoft/MixedRealityToolkit-Unity/releases/tag/v2.7.2 and into your project.
    1. The MRTK Foundation package is required for core functionality
    2. The MRTK Example Package is required test/view the MRTK Magic Leap Examples
2. Copy the git URL from this project [_Package URL_](https://github.com/ababilinski/magic-leap-setup-tool.git)
3. Navigate to **Window** and then click on **Package Manager**
4. Select the **+** button and click **Add packages from git URL** 
5. Paste this [project's url](https://github.com/magicleap/MRTK-MagicLeap.git), then click **Add**
6. Examples can be installed via the package manager 

<img width="795" alt="MRTK Magic Leap Examples" src="https://user-images.githubusercontent.com/10122344/148241652-afe2ea02-48f5-48d5-97b8-375b59f32b9d.png">


## Permissions
For your application to have access certain Magic Leap features, you need to configure your project's permissions. In **Project Settings** window, navigate to **Magic Leap > Manifest Settings** and add the following privileges:
   - ControllerPose
   - GesturesConfig
   - GesturesSubscribe
   - HandMesh (optional)
   - Internet (optional)
   - PcfRead (optional)
   - WorldReconstruction
   - LocalAreaNetwork

# Examples and Scene Settings

These scenes do not require additional configuration and serve as a blueprint for Magic Leap's MRTK integration. They can also be used as a guide for creating custom MRTK Configuration Profiles.

**Note**:
If you install MRTK through the Package Manager you will need to import the samples by selecting the sample from the samples dropdown under the MRTK Magic Leap Package description.

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


# Upgrade Guide
If you are upgrading from a previous version of the MRTK Magic Leap Package follow the steps below. 

1. Delete existing the following folders:  
    - `Assets/MRTK-MagicLeap`
    - `Assets/MagicLeap-Tools` (If present)
1. If you are upgrading to a newer version of both MRTK and the MRTK Magic Leap, follow Microsoft's [MRTK Upgrade Guide](https://docs.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/updates-deployment/updating?view=mrtkunity-2021-05) to update the MRTK components. Otherwise, continue to the next step.
1. Download and install the MRTK Magic Leap package by following the [Install MRTK Magic Leap](#installation-guide) instructions.  

# Notes
## Development Tips 
### Track Both Hands
- To enable hand tracking on both hands programmatically, set the Magic Leap Device Manager's hand settings to `Both` by adding the following line to one of your script's `Start` method:
```c#
MagicLeapDeviceManager.Instance.CurrentHandSettings == MagicLeapDeviceManager.HandSettings.Both
```
### Set the Magic Leap controller ray to "Always On"
- To prevent the controller's ray from being disabled when close to 3D objects, change the motion controller pointer behaviour to `AlwaysOn` by adding the following line to one of your script's `Start` method:
```c#
PointerUtils.SetMotionControllerRayPointerBehavior(PointerBehavior.AlwaysOn);
```

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
