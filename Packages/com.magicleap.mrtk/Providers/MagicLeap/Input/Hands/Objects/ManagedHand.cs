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

using UnityEngine;
using UnityEngine.XR.MagicLeap;
using System;

namespace MagicLeap.MRTK.DeviceManagement.Input.Hands
{
    public class ManagedHand
    {
#if PLATFORM_LUMIN
        //Events:
        public event Action<ManagedHand, bool> OnVisibilityChanged;

        //Public Properties:
        public ManagedHandSkeleton Skeleton
        {
            get;
            private set;
        }

        public ManagedHandGesture Gesture
        {
            get;
            private set;
        }

        public MLHandTracking.Hand Hand
        {
            get;
            private set;
        }

        public bool Visible
        {
            get;
            private set;
        }

        //Constructors:
        public ManagedHand(MLHandTracking.Hand hand)
        {
            Hand = hand;
            Skeleton = new ManagedHandSkeleton(this);
            Gesture = new ManagedHandGesture(this);
        }

        //Public Methods:
        public void Update()
        {
            //set visibility:
            if (Hand.HandConfidence > .85f && !Visible)
            {
                Visible = true;
                OnVisibilityChanged?.Invoke(this, true);
            }
            else if(Visible)
            {
                IntentPose pose = Gesture.Intent;
                var isPinching = (pose == IntentPose.Grasping || pose == IntentPose.Pinching);
                float minConfidence = isPinching ? .80f : .85f;
                if (Hand.HandConfidence <= minConfidence)
                {
                    Visible = false;
                    OnVisibilityChanged?.Invoke(this, false);
                }
            }

            Skeleton.Update();
            Gesture.Update();
        }
#endif
    }
}