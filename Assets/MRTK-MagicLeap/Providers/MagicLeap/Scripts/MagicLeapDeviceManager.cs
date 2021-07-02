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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.XRSDK.Input;
using MagicLeap.MRTK.DeviceManagement.Input.Hands;
using Microsoft.MixedReality.Toolkit.XRSDK;
using UnityEngine.XR.MagicLeap;
using UnityEngine.XR.Management;

namespace MagicLeap.MRTK.DeviceManagement.Input
{
    // Added inside of the scope to prevent conflicts with Unity's 2020.3 Version Control package.
    using Microsoft.MixedReality.Toolkit;

    /// <summary>
    /// Manages Magic Leap Device
    /// </summary>
    [MixedRealityDataProvider(typeof(IMixedRealityInputSystem), SupportedPlatforms.Lumin, "Magic Leap Device Manager")]
    public class MagicLeapDeviceManager : XRSDKDeviceManager
    {
        List<IMixedRealityController> trackedControls = new List<IMixedRealityController>();
        Dictionary<Handedness, MagicLeapHand> trackedHands = new Dictionary<Handedness, MagicLeapHand>();


#if PLATFORM_LUMIN
        private readonly MLHandTracking.HandKeyPose[] supportedGestures = new[]
        {
            MLHandTracking.HandKeyPose.Finger,
            MLHandTracking.HandKeyPose.Pinch,
            MLHandTracking.HandKeyPose.Fist,
            MLHandTracking.HandKeyPose.Thumb,
            MLHandTracking.HandKeyPose.L,
            MLHandTracking.HandKeyPose.OpenHand,
            MLHandTracking.HandKeyPose.Ok,
            MLHandTracking.HandKeyPose.C,
            MLHandTracking.HandKeyPose.NoPose
        };
#endif

        private bool? IsActiveLoader =>
            LoaderHelpers.IsLoaderActive<MagicLeapLoader>();
        
        public bool MLControllerCallbacksActive = false;
        public bool MLHandTrackingActive = false;

        public static MagicLeapDeviceManager Instance = null;

        public enum HandSettings
        {
            None,
            Left,
            Right,
            Both
        }

        public HandSettings CurrentControllerSettings
        {
            get
            {
                return _CurrentControllerSettings;
            }

            set
            {
                _CurrentControllerSettings = value;
                // TODO: Update real-time controller settings
            }
        }

        public HandSettings CurrentHandSettings
        {
            get
            {
                return _CurrentHandSettings;
            }

            set
            {
                _CurrentHandSettings = value;
#if PLATFORM_LUMIN
                // TODO: Update real-time hand settings
                switch (value)
                {
                    case HandSettings.None:
                        RemoveAllHandDevices();
                        return;

                    case HandSettings.Left:
                        if (trackedHands.ContainsKey(Handedness.Right))
                        {
                            MagicLeapHand hand = trackedHands[Handedness.Right];
                            if (hand != null)
                            {
                                RemoveHandDevice(hand);
                            }
                        }
                        break;

                    case HandSettings.Right:
                        if (trackedHands.ContainsKey(Handedness.Left))
                        {
                            MagicLeapHand hand = trackedHands[Handedness.Left];
                            if (hand != null)
                            {
                                RemoveHandDevice(hand);
                            }
                        }
                        break;
                }
#endif
            }
        }

        public HandSettings _CurrentControllerSettings = HandSettings.Left;
        public HandSettings _CurrentHandSettings = HandSettings.Left;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="registrar">The <see cref="IMixedRealityServiceRegistrar"/> instance that loaded the data provider.</param>
        /// <param name="inputSystem">The <see cref="Microsoft.MixedReality.Toolkit.Input.IMixedRealityInputSystem"/> instance that receives data from this provider.</param>
        /// <param name="name">Friendly name of the service.</param>
        /// <param name="priority">Service priority. Used to determine order of instantiation.</param>
        /// <param name="profile">The service's configuration profile.</param>
        public MagicLeapDeviceManager(
            IMixedRealityInputSystem inputSystem,
            string name = null,
            uint priority = DefaultPriority,
            BaseMixedRealityProfile profile = null) : base(inputSystem, name, priority, profile)
        {

        }
        public override void Initialize()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            base.Initialize();
        }
        
        private async void EnableIfLoaderBecomesActive()
        {
            await new WaitUntil(() => IsActiveLoader.HasValue);
            if (IsActiveLoader.Value)
            {
                Enable();
            }
        }

        public override void Enable()
        {
            if (!IsActiveLoader.HasValue)
            {
                IsEnabled = false;
                EnableIfLoaderBecomesActive();
                return;
            }
            else if (!IsActiveLoader.Value)
            {
                IsEnabled = false;
                return;
            }
            
            SetupInput();
            
            base.Enable();
            
        }

        private void SetupInput()
        {
#if PLATFORM_LUMIN
            if (!MLControllerCallbacksActive )
            {
                MLInput.OnControllerConnected += MLControllerConnected;
                MLInput.OnControllerDisconnected += MLControllerDisconnected;

                MLControllerCallbacksActive = true;
            }

            if (!MLHandTrackingActive && MLHandTracking.IsStarted &&  MLHandTracking.KeyPoseManager != null)
            {
                MLHandTracking.KeyPoseManager.SetKeyPointsFilterLevel(MLHandTracking.KeyPointFilterLevel.Raw);
                MLHandTracking.KeyPoseManager.EnableKeyPoses(supportedGestures, true, false);

                MLHandTrackingActive = true;
            }
#endif
        }

        public override void Update()
        {
            
            if (!IsEnabled)
            {
                return;
            }
            
#if PLATFORM_LUMIN
            // Ensure input is active
            if (MLDevice.IsReady())
            {
                UpdateHands();
                
                foreach (MLControllerContainer controllerContainer in ConnectedControllers.Values)
                {
                    controllerContainer.mrtkController.UpdatePoses();
                }

            }
#endif
        }
        
        protected void UpdateHands()
        {
#if PLATFORM_LUMIN
            UpdateHand(MLHandTracking.Right, Handedness.Right);
            UpdateHand(MLHandTracking.Left, Handedness.Left);
#endif

        }

        public override void Disable()
        {
#if PLATFORM_LUMIN
            if (MLControllerCallbacksActive)
            {
                RemoveAllControllerDevices();
                MLInput.OnControllerConnected -= MLControllerConnected;
                MLInput.OnControllerDisconnected -= MLControllerDisconnected;

                MLControllerCallbacksActive = false;
            }

            if (MLHandTrackingActive)
            {
                RemoveAllHandDevices();

                MLHandTrackingActive = true;
            }

            if (Instance == this)
            {
                Instance = null;
            }
#endif
        }
        
        public override IMixedRealityController[] GetActiveControllers()
        {
            return trackedControls.ToArray<IMixedRealityController>();
        }

        /// <inheritdoc />
        public override bool CheckCapability(MixedRealityCapability capability)
        {
            return (capability == MixedRealityCapability.ArticulatedHand) ||
                   (capability == MixedRealityCapability.MotionController);
        }

#if PLATFORM_LUMIN
#region Hand Management

        protected void UpdateHand(MLHandTracking.Hand mlHand, Handedness handedness)
        {
           
            if(!IsHandednessValid(handedness,CurrentHandSettings))
                return;
            
            if (mlHand != null && mlHand.IsVisible)
            {
                var hand = GetOrAddHand(mlHand, handedness);
                hand.DoUpdate();
            }
            else
            {
                RemoveHandDevice(handedness);
            }
        }
        
        private void RemoveHandDevice(Handedness handedness)
        {
            if (trackedHands.TryGetValue(handedness, out MagicLeapHand hand))
            {
                RemoveHandDevice(hand);
            }
        }

        private bool IsHandednessValid(Handedness handedness, HandSettings settings)
        {
            switch (settings)
            {
                case HandSettings.None:
                    return false;

                case HandSettings.Left:
                    if (handedness != Handedness.Left)
                    {
                        return false;
                    }
                    break;

                case HandSettings.Right:
                    if (handedness != Handedness.Right)
                    {
                        return false;
                    }
                    break;

                case HandSettings.Both:
                    if (handedness != Handedness.Left && handedness != Handedness.Right)
                    {
                        return false;
                    }
                    break;
            }

            return true;
        }

        private MagicLeapHand GetOrAddHand(MLHandTracking.Hand mlHand, Handedness handedness)
        {
            if (trackedHands.ContainsKey(handedness))
            {
                return trackedHands[handedness];
            }

            // Add new hand
            var pointers = RequestPointers(SupportedControllerType.ArticulatedHand, handedness);
            var inputSourceType = InputSourceType.Hand;

            var inputSource = Service?.RequestNewGenericInputSource($"Magic Leap {handedness} Hand", pointers, inputSourceType);
            
            var controller = new MagicLeapHand(TrackingState.Tracked, handedness, inputSource);
            controller.Initalize(new ManagedHand(mlHand));

            for (int i = 0; i < controller.InputSource?.Pointers?.Length; i++)
            {
                controller.InputSource.Pointers[i].Controller = controller;
            }

            Service?.RaiseSourceDetected(controller.InputSource, controller);
            trackedHands.Add(handedness, controller);

            return controller;
        }

        private void RemoveAllHandDevices()
        {
            if (trackedHands.Count == 0) return;

            // Create a new list to avoid causing an error removing items from a list currently being iterated on.
            foreach (MagicLeapHand hand in new List<MagicLeapHand>(trackedHands.Values))
            {
                RemoveHandDevice(hand);
            }
            trackedHands.Clear();
        }

        private void RemoveHandDevice(MagicLeapHand hand)
        {
            if (hand == null) return;

            CoreServices.InputSystem?.RaiseSourceLost(hand.InputSource, hand);
            trackedHands.Remove(hand.ControllerHandedness);
            trackedControls.Remove(hand);

            RecyclePointers(hand.InputSource);
        }

#endregion

#region Controller Management

        class MLControllerContainer
        {
            public byte controllerId;
            public MagicLeapMRTKController mrtkController;
        };

        Dictionary<byte, MLControllerContainer> ConnectedControllers = new Dictionary<byte, MLControllerContainer>();

        MLControllerContainer GetConnectedController(byte controllerId)
        {
            if (ConnectedControllers.ContainsKey(controllerId))
            {
                return ConnectedControllers[controllerId];
            }
            return null;
        }
        

        void MLControllerConnected(byte controllerId)
        {
            CameraCache.Main.GetComponent<MonoBehaviour>().StartCoroutine(ConnectOnValidPosition(controllerId));
        }

        IEnumerator ConnectOnValidPosition(byte controllerId)
        {
#if PLATFORM_LUMIN
            MLInput.Controller mlController = MLInput.GetController(controllerId);
            Vector3 initialPosition = mlController.Position;
            while (initialPosition == mlController.Position)
            {
                yield return null;
            }
            if (mlController.Type == MLInput.Controller.ControlType.Control)
            {
                if (!ConnectedControllers.ContainsKey(controllerId))
                {
                    Handedness handedness = mlController.Hand == MLInput.Hand.Right ? Handedness.Right : Handedness.Left;
                    if(!IsHandednessValid(handedness , CurrentControllerSettings))
                        yield break ;

                    var pointers = RequestPointers(SupportedControllerType.GenericUnity, handedness);
                    var inputSourceType = InputSourceType.Controller;

                    var inputSource = Service?.RequestNewGenericInputSource($"Magic Leap {handedness} Controller", pointers, inputSourceType);

                    MagicLeapMRTKController controller = new MagicLeapMRTKController(mlController, TrackingState.Tracked, handedness, inputSource);
                    for (int i = 0; i < controller.InputSource?.Pointers?.Length; i++)
                    {
                        controller.InputSource.Pointers[i].Controller = controller;
                    }
                    
                    Service?.RaiseSourceDetected(controller.InputSource, controller);
                    ConnectedControllers[controllerId] = new MLControllerContainer()
                    {
                        controllerId = controllerId,
                        mrtkController = controller
                    };
                    trackedControls.Add(controller);
                }
            }
#endif
        }

        void MLControllerDisconnected(byte controllerId)
        {
            MLControllerContainer controllerContainer = GetConnectedController(controllerId);
            if (controllerContainer != null)
            {
                IMixedRealityInputSystem inputSystem = Service as IMixedRealityInputSystem;
                inputSystem?.RaiseSourceLost(controllerContainer.mrtkController.InputSource, controllerContainer.mrtkController);
                trackedControls.Remove(controllerContainer.mrtkController);
                RecyclePointers(controllerContainer.mrtkController.InputSource);
                controllerContainer.mrtkController.CleanupController();
                ConnectedControllers.Remove(controllerId);
            }
            return;
        }

        private void RemoveAllControllerDevices()
        {
            if (ConnectedControllers.Count == 0) return;

            // Create a new list to avoid causing an error removing items from a list currently being iterated on.
            foreach (byte controllerId in new List<byte>(ConnectedControllers.Keys))
            {
                MLControllerDisconnected(controllerId);
            }
            ConnectedControllers.Clear();
        }

#endregion
#endif // PLATFORM_LUMIN
    }
}
