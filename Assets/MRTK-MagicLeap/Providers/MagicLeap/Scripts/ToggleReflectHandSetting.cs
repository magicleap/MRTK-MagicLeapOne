/// -------------------------------------------------------------------------------
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit.UI;

namespace MagicLeap.MRTK.DeviceManagement.Input
{
    [RequireComponent(typeof(Interactable))]
    public class ToggleReflectHandSetting : MonoBehaviour
    {
        public MagicLeapDeviceManager.HandSettings SettingToReflect;
        Interactable interactable;

        private void Awake()
        {
            interactable = GetComponent<Interactable>();
        }

        private void Start()
        {
            UpdateToggle();
        }

        private void UpdateToggle()
        {
            if (MagicLeapDeviceManager.Instance == null)
            {
                Debug.Log("Device Manager Not here");
                return;
            }

            switch(SettingToReflect)
            {
                case MagicLeapDeviceManager.HandSettings.Both:
                    interactable.IsToggled =
                        MagicLeapDeviceManager.Instance.CurrentHandSettings == MagicLeapDeviceManager.HandSettings.Both;
                    break;

                case MagicLeapDeviceManager.HandSettings.Left:
                    interactable.IsToggled =
                        MagicLeapDeviceManager.Instance.CurrentHandSettings == MagicLeapDeviceManager.HandSettings.Both ||
                        MagicLeapDeviceManager.Instance.CurrentHandSettings == MagicLeapDeviceManager.HandSettings.Left;
                    break;

                case MagicLeapDeviceManager.HandSettings.Right:
                    interactable.IsToggled =
                        MagicLeapDeviceManager.Instance.CurrentHandSettings == MagicLeapDeviceManager.HandSettings.Both ||
                        MagicLeapDeviceManager.Instance.CurrentHandSettings == MagicLeapDeviceManager.HandSettings.Right;
                    break;
            }
        }

        public void ReflectToggleButton(Interactable interactable)
        {
            MagicLeapDeviceManager.HandSettings CurrentHandSettings = MagicLeapDeviceManager.Instance.CurrentHandSettings;
            MagicLeapDeviceManager.HandSettings NewHandSettings = MagicLeapDeviceManager.Instance.CurrentHandSettings;

            switch (SettingToReflect)
            {
                case MagicLeapDeviceManager.HandSettings.Left:
                    switch (CurrentHandSettings)
                    {
                        case MagicLeapDeviceManager.HandSettings.Left:
                            if (!interactable.IsToggled) // Turn off Left
                            {
                                NewHandSettings = MagicLeapDeviceManager.HandSettings.None;
                            }
                            break;

                        case MagicLeapDeviceManager.HandSettings.Right:
                            if (interactable.IsToggled)
                            {
                                NewHandSettings = MagicLeapDeviceManager.HandSettings.Both;
                            }
                            break;

                        case MagicLeapDeviceManager.HandSettings.None:
                            if (interactable.IsToggled)
                            {
                                NewHandSettings = MagicLeapDeviceManager.HandSettings.Left;
                            }
                            break;

                        case MagicLeapDeviceManager.HandSettings.Both:
                            if (!interactable.IsToggled)
                            {
                                NewHandSettings = MagicLeapDeviceManager.HandSettings.Right;
                            }
                            break;
                    }
                    break;

                case MagicLeapDeviceManager.HandSettings.Right:
                    switch (MagicLeapDeviceManager.Instance.CurrentHandSettings)
                    {
                        case MagicLeapDeviceManager.HandSettings.Right:
                            if (!interactable.IsToggled) // Turn off Right
                            {
                                NewHandSettings = MagicLeapDeviceManager.HandSettings.None;
                            }
                            break;

                        case MagicLeapDeviceManager.HandSettings.Left:
                            if (interactable.IsToggled)
                            {
                                NewHandSettings = MagicLeapDeviceManager.HandSettings.Both;
                            }
                            break;

                        case MagicLeapDeviceManager.HandSettings.None:
                            if (interactable.IsToggled)
                            {
                                NewHandSettings = MagicLeapDeviceManager.HandSettings.Right;
                            }
                            break;

                        case MagicLeapDeviceManager.HandSettings.Both:
                            if (!interactable.IsToggled)
                            {
                                NewHandSettings = MagicLeapDeviceManager.HandSettings.Left;
                            }
                            break;
                    }
                    break;

                case MagicLeapDeviceManager.HandSettings.Both:
                    NewHandSettings = interactable.IsToggled ?
                        MagicLeapDeviceManager.HandSettings.Both : MagicLeapDeviceManager.HandSettings.None;
                    break;
            }
            MagicLeapDeviceManager.Instance.CurrentHandSettings = NewHandSettings;
            //Debug.Log("New Hand Settings: " + MagicLeapDeviceManager.Instance.CurrentHandSettings);
        }
    }
}
