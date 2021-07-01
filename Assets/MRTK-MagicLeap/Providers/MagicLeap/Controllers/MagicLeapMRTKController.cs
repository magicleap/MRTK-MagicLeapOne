// -------------------------------------------------------------------------------
// MRTK - MagicLeap
// https://github.com/magicleap/MRTK-MagicLeap
// -------------------------------------------------------------------------------
//
// MIT License
//
// Copyright(c) 2021 Magic Leap, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// -------------------------------------------------------------------------------
//
// Note code inspired from https://github.com/provencher/MRTK-MagicLeap
//

using System;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
#if PLATFORM_LUMIN
using UnityEngine.XR.MagicLeap;
#endif

namespace MagicLeap.MRTK.DeviceManagement.Input
{
    // Added inside of the scope to prevent conflicts with Unity's 2020.3 Version Control package.
    using Microsoft.MixedReality.Toolkit;
    
    [MixedRealityController(SupportedControllerType.GenericUnity,
        new[] { Handedness.Left, Handedness.Right })]
    public class MagicLeapMRTKController : BaseController, IMixedRealityHapticFeedback
    {
#if PLATFORM_LUMIN
        MLInput.Controller mlController;
#endif
        
        public MagicLeapMRTKController(TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null) : base(trackingState, controllerHandedness, inputSource, interactions)
        {
        }

#if PLATFORM_LUMIN
        public MagicLeapMRTKController(MLInput.Controller controller, TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null) : base(trackingState, controllerHandedness, inputSource, interactions)
        {
            mlController = controller;
            MLInput.OnControllerButtonDown += MLControllerButtonDown;
            MLInput.OnControllerButtonUp += MLControllerButtonUp;
            MLInput.OnTriggerDown += MLControllerTriggerDown;
            MLInput.OnTriggerUp += MLControllerTriggerUp;

            IsPositionAvailable = true;
            IsPositionApproximate = true;
            IsRotationAvailable = true;
        }
#endif
        public void CleanupController()
        {
#if PLATFORM_LUMIN
            MLInput.OnControllerButtonDown -= MLControllerButtonDown;
            MLInput.OnControllerButtonUp -= MLControllerButtonUp;
            MLInput.OnTriggerDown -= MLControllerTriggerDown;
            MLInput.OnTriggerUp -= MLControllerTriggerUp;
#endif
        }
        
        public override MixedRealityInteractionMapping[] DefaultLeftHandedInteractions => new[]
        {
            new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer),
            new MixedRealityInteractionMapping(1, "Select", AxisType.Digital, DeviceInputType.Select),
            new MixedRealityInteractionMapping(2, "Bumper Press", AxisType.Digital, DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping(3, "Home Press", AxisType.Digital, DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping(4, "Touchpad Touch", AxisType.Digital, DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping(5, "Touchpad Position", AxisType.DualAxis, DeviceInputType.Touchpad, ControllerMappingLibrary.AXIS_17, ControllerMappingLibrary.AXIS_18),
            new MixedRealityInteractionMapping(6, "Touchpad Press", AxisType.Digital, DeviceInputType.TouchpadPress),
        };

        public override MixedRealityInteractionMapping[] DefaultRightHandedInteractions => new[]
        {
            new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer),
            new MixedRealityInteractionMapping(1, "Select", AxisType.Digital, DeviceInputType.Select),
            new MixedRealityInteractionMapping(2, "Bumper Press", AxisType.Digital, DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping(3, "Home Press", AxisType.Digital, DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping(4, "Touchpad Touch", AxisType.Digital, DeviceInputType.TouchpadTouch),
            new MixedRealityInteractionMapping(5, "Touchpad Position", AxisType.DualAxis, DeviceInputType.Touchpad, ControllerMappingLibrary.AXIS_17, ControllerMappingLibrary.AXIS_18),
            new MixedRealityInteractionMapping(6, "Touchpad Press", AxisType.Digital, DeviceInputType.TouchpadPress),
        };

#if PLATFORM_LUMIN
        public override bool IsInPointingPose
        {
            get
            {
                return true;
            }
        }

        Vector3 lastTouchVector;
        float lastPressure;

        public void UpdatePoses()
        {
            
            MixedRealityPose pointerPose = new MixedRealityPose(mlController.Position, mlController.Orientation); ;
            Interactions[0].PoseData = pointerPose;
            
            CoreServices.InputSystem?.RaiseSourcePoseChanged(InputSource, this, pointerPose);
            CoreServices.InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, Interactions[0].MixedRealityInputAction, pointerPose);
         
            //This is also a good time to implement the Touchpad if you want to update that source type
            if (Interactions.Length > 4)
            {
                IMixedRealityInputSystem inputSystem = CoreServices.InputSystem;

                // Test out touch
                Interactions[4].BoolData = mlController.Touch1Active;
                if (mlController.Touch1Active)
                {
                    lastTouchVector = mlController.Touch1PosAndForce;
                    lastPressure = mlController.Touch1PosAndForce.z;
                }

                if (Interactions[4].Changed)
                {
                    if (mlController.Touch1Active)
                    {
                        inputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, Interactions[4].MixedRealityInputAction);
                    }
                    else
                    {
                        inputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, Interactions[4].MixedRealityInputAction);
                    }
                }

                if (mlController.Touch1Active)
                {
                    Interactions[5].Vector2Data = lastTouchVector;
                    if (Interactions[5].Changed)
                    {
                        inputSystem?.RaisePositionInputChanged(InputSource, ControllerHandedness, Interactions[5].MixedRealityInputAction, Interactions[5].Vector2Data);
                    }

                    if (Interactions[6].BoolData) // Pressure was last down
                    {
                        if (lastPressure < 0.25f)
                        {
                            Interactions[6].BoolData = false;
                            inputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, Interactions[6].MixedRealityInputAction);
                        }
                    }
                    else // Pressure was last off
                    {
                        if (lastPressure > 0.5f)
                        {
                            Interactions[6].BoolData = true;
                            inputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, Interactions[6].MixedRealityInputAction);
                        }
                    }

                } else if (Interactions[6].BoolData)
                {
                    Interactions[6].BoolData = false;
                    inputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, Interactions[6].MixedRealityInputAction);
                    lastPressure = 0;
                }
            }

        }

        void MLControllerButtonDown(byte controllerId, MLInput.Controller.Button button)
        {
            // Implement bumper down; do no implement home button down (bc of the implementation of it)
            if (controllerId == this.mlController.Id)
            {
                IMixedRealityInputSystem inputSystem = CoreServices.InputSystem;
                switch (button)
                {
                    case MLInput.Controller.Button.Bumper:
                        Interactions[2].BoolData = true;
                        inputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, Interactions[2].MixedRealityInputAction);
                        break;

                    case MLInput.Controller.Button.HomeTap:
                        Interactions[3].BoolData = true;
                        inputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, Interactions[3].MixedRealityInputAction);
                        break;
                }
            }
        }

        void MLControllerButtonUp(byte controllerId, MLInput.Controller.Button button)
        {
            //Implement bumper and home buttons up
            if (controllerId == this.mlController.Id)
            {
                IMixedRealityInputSystem inputSystem = CoreServices.InputSystem;
                switch (button)
                {
                    case MLInput.Controller.Button.Bumper:
                        Interactions[2].BoolData = false;
                        inputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, Interactions[2].MixedRealityInputAction);
                        break;

                    case MLInput.Controller.Button.HomeTap:
                        Interactions[3].BoolData = false;
                        inputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, Interactions[3].MixedRealityInputAction);
                        break;
                }
            }
        }

        void MLControllerTriggerDown(byte controllerId, float triggerValue)
        {
            if (controllerId == this.mlController.Id)
            {
                IMixedRealityInputSystem inputSystem = CoreServices.InputSystem;
                Interactions[1].BoolData = true;
                inputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, Interactions[1].MixedRealityInputAction);
            }
        }

        void MLControllerTriggerUp(byte controllerId, float triggerValue)
        {
            if (controllerId == this.mlController.Id)
            {
                IMixedRealityInputSystem inputSystem = CoreServices.InputSystem;
                Interactions[1].BoolData = false;
                inputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, Interactions[1].MixedRealityInputAction);
            }
        }

#endif // PLATFORM_LUMIN
        
        public bool StartHapticImpulse(float intensity, float durationInSeconds = Single.MaxValue)
        {
#if PLATFORM_LUMIN
            MLInput.Controller.FeedbackIntensity mlIntensity = (MLInput.Controller.FeedbackIntensity)((int)(intensity * 2.0f));
            var result = mlController.StartFeedbackPatternVibe(MLInput.Controller.FeedbackPatternVibe.Buzz, mlIntensity);
            return result == MLResult.Code.Ok;
#else
            return false;
#endif
        }
        public void StopHapticFeedback()
        {
#if PLATFORM_LUMIN
            mlController.StopFeedbackPatternVibe();
#endif
        }
    }
}
