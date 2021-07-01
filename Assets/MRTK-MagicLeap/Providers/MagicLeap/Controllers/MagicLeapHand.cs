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

using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

using MagicLeap.MRTK.DeviceManagement.Input.Hands;

namespace MagicLeap.MRTK.DeviceManagement.Input
{
    // Added inside of the scope to prevent conflicts with Unity's 2020.3 Version Control package.
    using Microsoft.MixedReality.Toolkit;
    
    [MixedRealityController(SupportedControllerType.ArticulatedHand,
        new[] { Handedness.Left, Handedness.Right },
        flags: MixedRealityControllerConfigurationFlags.UseCustomInteractionMappings)]
    public class MagicLeapHand : BaseHand, IMixedRealityHand
    {
        public const float shoulderWidth = 0.37465f;
        public const float shoulderDistanceBelowHead = 0.2159f;

        private MixedRealityPose currentPointerPose = MixedRealityPose.ZeroIdentity;
        private MixedRealityPose currentIndexPose = MixedRealityPose.ZeroIdentity;
        private MixedRealityPose currentGripPose = MixedRealityPose.ZeroIdentity;

        public bool IsPinching { get; private set; } = false;

        protected readonly Dictionary<TrackedHandJoint, MixedRealityPose> jointPoses = new Dictionary<TrackedHandJoint, MixedRealityPose>();

        public MagicLeapHand(TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null) : base(trackingState, controllerHandedness, inputSource, interactions)
        {
           
        }

        public override bool IsInPointingPose
        {
            get
            {
                if (!TryGetJoint(TrackedHandJoint.Palm, out var palmPose) || CameraCache.Main == null) return false;

                Transform cameraTransform = CameraCache.Main.transform;
                Vector3 projectedPalmUp = Vector3.ProjectOnPlane(-palmPose.Up, cameraTransform.up);

                // We check if the palm forward is roughly in line with the camera lookAt
                return Vector3.Dot(cameraTransform.forward, projectedPalmUp) > 0.3f;
            }
        }

        public override bool TryGetJoint(TrackedHandJoint joint, out MixedRealityPose pose)
        {
            if (jointPoses.TryGetValue(joint, out pose))
            {
                return true;
            }
            pose = MixedRealityPose.ZeroIdentity;
            return false;
        }

        public override MixedRealityInteractionMapping[] DefaultInteractions => new[]
        {
            new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer, new MixedRealityInputAction(4, "Pointer Pose", AxisType.SixDof)),
            new MixedRealityInteractionMapping(1, "Spatial Grip", AxisType.SixDof, DeviceInputType.SpatialGrip, new MixedRealityInputAction(3, "Grip Pose", AxisType.SixDof)),
            new MixedRealityInteractionMapping(2, "Select", AxisType.Digital, DeviceInputType.Select, new MixedRealityInputAction(1, "Select", AxisType.Digital)),
            new MixedRealityInteractionMapping(3, "Grab", AxisType.SingleAxis, DeviceInputType.TriggerPress, new MixedRealityInputAction(7, "Grip Press", AxisType.SingleAxis)),
            new MixedRealityInteractionMapping(4, "Index Finger Pose", AxisType.SixDof, DeviceInputType.IndexFinger,  new MixedRealityInputAction(13, "Index Finger Pose", AxisType.SixDof)),
            new MixedRealityInteractionMapping(5, "Teleport Pose", AxisType.None, DeviceInputType.None),
        };

#if PLATFORM_LUMIN
        protected ManagedHand managedHand = null;

        public void Initalize(ManagedHand newManagedHand)
        {
            managedHand = newManagedHand;
            IsPositionAvailable = true;
            IsPositionApproximate = true;
            IsRotationAvailable = true;
        }
        
        public override void SetupDefaultInteractions()
        {
            AssignControllerMappings(DefaultInteractions);
        }

        public void DoUpdate()
        {
            if (!Enabled || managedHand == null) return;

            managedHand.Update();
            IntentPose pose = managedHand.Gesture.Intent;
            IsPinching = false;

            IsPositionAvailable = IsRotationAvailable = managedHand.Visible;
           
            if (IsPositionAvailable)
            {
              
                UpdateHandData(managedHand.Skeleton);
                
                currentGripPose = pose == IntentPose.Pinching
                    ? new MixedRealityPose(managedHand.Gesture.Pinch.position, jointPoses[TrackedHandJoint.Palm].Rotation)
                    : jointPoses[TrackedHandJoint.Palm];
                
                currentPointerPose = jointPoses[TrackedHandJoint.Palm];

                currentIndexPose = jointPoses[TrackedHandJoint.IndexTip];

                
                //The pinch pose is lost if the hand joints are not visible because they are in the camera's clip plane.
                //The clip plane value is set to the default Magic Leap clip plane to insure a valid hand pose.
                IsPinching = (pose == IntentPose.Grasping || pose == IntentPose.Pinching);

                UpdateHandRay(managedHand.Skeleton);

                UpdateVelocity();
                
                CoreServices.InputSystem?.RaiseSourcePoseChanged(InputSource, this, currentGripPose);

            }
           
            for (int i = 0; i < Interactions?.Length; i++)
            {
                switch (Interactions[i].InputType)
                {
                    case DeviceInputType.SpatialPointer:
                        Interactions[i].PoseData =  currentPointerPose;
                        if (Interactions[i].Changed)
                        {
                            CoreServices.InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction, currentPointerPose);
                        }
                        break;
                    case DeviceInputType.SpatialGrip:
                        Interactions[i].PoseData = currentGripPose;
                        if (Interactions[i].Changed)
                        {
                            CoreServices.InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction, currentGripPose);
                        }
                        break;
                    case DeviceInputType.Select:
                        Interactions[i].BoolData = IsPinching;

                        if (Interactions[i].Changed)
                        {
                            if (Interactions[i].BoolData)
                            {
                                CoreServices.InputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction);
                            }
                            else
                            {
                                CoreServices.InputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction);
                            }
                        }
                        break;
                    case DeviceInputType.TriggerPress:
                        Interactions[i].BoolData = IsPinching;
                        if (Interactions[i].Changed)
                        {
                            if (Interactions[i].BoolData)
                            {
                                CoreServices.InputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction);
                            }
                            else
                            {
                                CoreServices.InputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction);
                            }
                        }
                        break;
                    case DeviceInputType.IndexFinger:
                        Interactions[i].PoseData = currentIndexPose;
                        // If our value changed raise it.
                        if (Interactions[i].Changed)
                        {
                            // Raise input system Event if it enabled
                            CoreServices.InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction, currentIndexPose);
                        }
                        break;
                }
            }
        }
        protected void UpdateHandRay(ManagedHandSkeleton hand)
        {
            //shoulder:
            float shoulderDistance = shoulderWidth * .5f;
            
            //swap for the left shoulder:
            if (ControllerHandedness == Handedness.Left)
            {
                shoulderDistance *= -1;
            }

            Transform camera = CameraCache.Main.transform;

            //source locations:
            Vector3 flatForward = Vector3.ProjectOnPlane(camera.forward, Vector3.up);
            Vector3 shoulder = TransformUtilities.WorldPosition(camera.position, Quaternion.LookRotation(flatForward), new Vector2(shoulderDistance, Mathf.Abs(shoulderDistanceBelowHead) * -1));
            
            Vector3 pointerOrigin = Vector3.Lerp(hand.Thumb.Knuckle._lastValidPosition, hand.HandCenter._lastValidPosition, .5f);
            
            //direction:
            Quaternion orientation = Quaternion.LookRotation(Vector3.Normalize(pointerOrigin - shoulder), hand.Rotation * Vector3.up);
            currentPointerPose.Position = pointerOrigin;
            currentPointerPose.Rotation = orientation;
            
        }

        // Magic Leap conversion code inspired by the work of Tarukosu
        // https://github.com/HoloLabInc/MRTKExtensionForMagicLeap/blob/master/Assets/MixedRealityToolkit.ThirdParty/MagicLeapInput/Scripts/MagicLeapHand.cs
        // Combined with usage of Magic Leap Toolkit
        protected void UpdateHandData(ManagedHandSkeleton hand)
        {
            // Update joint positions
            var pinky = hand.Pinky;
            ConvertMagicLeapKeyPoint(pinky.Tip, TrackedHandJoint.PinkyTip);
            ConvertMagicLeapKeyPoint(pinky.Knuckle, TrackedHandJoint.PinkyKnuckle);

            var ring = hand.Ring;
            ConvertMagicLeapKeyPoint(ring.Tip, TrackedHandJoint.RingTip);
            ConvertMagicLeapKeyPoint(ring.Knuckle, TrackedHandJoint.RingKnuckle);

            var middle = hand.Middle;
            ConvertMagicLeapKeyPoint(middle.Tip, TrackedHandJoint.MiddleTip);
            ConvertMagicLeapKeyPoint(middle.Joint, TrackedHandJoint.MiddleMiddleJoint);
            ConvertMagicLeapKeyPoint(middle.Knuckle, TrackedHandJoint.MiddleKnuckle);

            var index = hand.Index;
            ConvertMagicLeapKeyPoint(index.Tip, TrackedHandJoint.IndexTip);
            ConvertMagicLeapKeyPoint(index.Joint, TrackedHandJoint.IndexMiddleJoint);
            ConvertMagicLeapKeyPoint(index.Knuckle, TrackedHandJoint.IndexKnuckle);

            var thumb = hand.Thumb;
            ConvertMagicLeapKeyPoint(thumb.Tip, TrackedHandJoint.ThumbTip);
            ConvertMagicLeapKeyPoint(thumb.Joint, TrackedHandJoint.ThumbDistalJoint);
            ConvertMagicLeapKeyPoint(thumb.Knuckle, TrackedHandJoint.ThumbProximalJoint);

            // Wrist and palm reference hand skeleton rotation directly
            UpdateJointPose(TrackedHandJoint.Palm, hand.HandCenter.GetPosition(FilterType.Filtered), hand.Rotation);
            UpdateJointPose(TrackedHandJoint.Wrist, hand.WristCenter.GetPosition(FilterType.Filtered), hand.Rotation);

            CoreServices.InputSystem?.RaiseHandJointsUpdated(InputSource, ControllerHandedness, jointPoses);
        }

        protected void ConvertMagicLeapKeyPoint(ManagedKeypoint keyPoint, TrackedHandJoint joint)
        {
            if(keyPoint == null) return;
            UpdateJointPose(joint, keyPoint.GetPosition(FilterType.Filtered), keyPoint.Rotation);

        }

        protected void UpdateJointPose(TrackedHandJoint joint, Vector3 position, Quaternion rotation)
        {
            var pose = new MixedRealityPose(position, rotation);
            if (!jointPoses.ContainsKey(joint))
            {
                jointPoses.Add(joint, pose);
            }
            else
            {
                jointPoses[joint] = pose;
            }
        }

#endif // PLATFORM_LUMIN

    }
}
