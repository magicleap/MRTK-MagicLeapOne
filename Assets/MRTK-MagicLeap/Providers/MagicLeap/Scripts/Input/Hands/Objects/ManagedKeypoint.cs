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

using System.Collections.Generic;
using System;
using UnityEngine;

namespace MagicLeap.MRTK.DeviceManagement.Input.Hands
{
    public class ManagedKeypoint
    {
#if PLATFORM_LUMIN
        //Private Classes:
        private class Progress
        {
            //Public Variables:
            public Vector3 velocity;
            public List<Vector3> locationHistory = new List<Vector3>();
            public Vector3 target;
        }

        //Events:
        public event Action OnFound;
        public event Action OnLost;

        //Public Properties:
        public bool Visible
        {
            get;
            private set;
        }

        public bool VisibleStable
        {
            get
            {
                return Time.realtimeSinceStartup - _visibleStableFoundTime > _visibleStableTimeout;
            }
        }

        public bool InsideClipPlane
        {
            get;
            private set;
        }
        public Quaternion Rotation;

        //Public Variables:
        public Vector3 positionFiltered;
        public Vector3 positionRaw;

        //Private Properties:
        private float Stability
        {
            get
            {
                return _stability;
            }

            set
            {
                //slight filtering:
                _stability = Mathf.Lerp(_stability, value, Time.deltaTime * 5);
            }
        }

        //Private Variables:
        private Progress _progress;
        private float _stability;
        private float _minHeadDistance = 0.4f;
        private float _lostKeyPointDistance = 0.00762f;
        private float _foundKeyPointDistance = 0.01905f;
        private float _maxDistance = 0.0254f;
        private float _smoothTime = .1f;
        private Camera _mainCamera;
        private float _visibleStableTimeout = .25f;
        private float _visibleStableFoundTime;

        public bool HideInsideClipPlane = true;

        //Constructors:
        public ManagedKeypoint()
        {
            _progress = new Progress();
            _mainCamera = Camera.main;
        }

        //Public Methods:
        public void FireFoundEvent()
        {
            _visibleStableFoundTime = Time.realtimeSinceStartup;
            Visible = true;
            OnFound?.Invoke();
        }

        public void FireLostEvent()
        {
            Stability = 0;
            Visible = false;
            OnLost?.Invoke();
        }

        public void Update(ManagedHand managedHand, Vector3 keyPointLocation, params Vector3[] decayPoints)
        {
            if (!managedHand.Visible)
            {
                //lost:
                if (Visible)
                {
                    FireLostEvent();
                    _progress.locationHistory.Clear();
                }

                return;
            }

            //visibility status:
            bool currentVisibility = true;

            //too close to next joint in chain means visibility failed:
            if (Vector3.Distance(keyPointLocation, _mainCamera.transform.position) < _minHeadDistance)
            {
                currentVisibility = false;
            }
            else
            {
                for (int i = 0; i < decayPoints.Length; i++)
                {
                    if (Vector3.Distance(keyPointLocation, decayPoints[i]) < _lostKeyPointDistance)
                    {
                        currentVisibility = false;
                        break;
                    }
                }
            }

            positionRaw = keyPointLocation;

            //lost visibility:
            if (!currentVisibility && Visible)
            {
                FireLostEvent();
                _progress.locationHistory.Clear();
                return;
            }

            //history cache:
            _progress.locationHistory.Add(keyPointLocation);

            //only need 3 in our history:
            if (_progress.locationHistory.Count > 3)
            {
                _progress.locationHistory.RemoveAt(0);
            }

            //we have enough history:
            if (_progress.locationHistory.Count == 3)
            {
                //movement intent stats:
                Vector3 vectorA = _progress.locationHistory[_progress.locationHistory.Count - 2] - _progress.locationHistory[_progress.locationHistory.Count - 3];
                Vector3 vectorB = _progress.locationHistory[_progress.locationHistory.Count - 1] - _progress.locationHistory[_progress.locationHistory.Count - 2];
                float delta = Vector3.Distance(_progress.locationHistory[_progress.locationHistory.Count - 3], _progress.locationHistory[_progress.locationHistory.Count - 1]);
                float angle = Vector3.Angle(vectorA, vectorB);
                Stability = 1 - Mathf.Clamp01(delta / _maxDistance);

                //moving in a constant direction?
                if (angle < 90)
                {
                    _progress.target = _progress.locationHistory[_progress.locationHistory.Count - 1];
                }

                //snap or smooth:
                if (Stability == 0)
                {
                    positionFiltered = _progress.target;
                }
                else
                {
                    positionFiltered = Vector3.SmoothDamp(positionFiltered, _progress.target, ref _progress.velocity, _smoothTime * Stability);
                }
            }
            else
            {
                positionFiltered = keyPointLocation;
            }

            //inside the camera plane - flatten against the plane?
            InsideClipPlane = TransformUtilities.InsideClipPlane(positionFiltered);
            if (InsideClipPlane)
            {
                if (HideInsideClipPlane)
                {
                    positionFiltered = keyPointLocation;
                    FireLostEvent();
                    _progress.locationHistory.Clear();
                    return;
                }
                positionFiltered = TransformUtilities.LocationOnClipPlane(positionFiltered);
            }

            //gained visibility:
            if (currentVisibility && !Visible)
            {
                //we must also break distance for point proximity:
                for (int i = 0; i < decayPoints.Length; i++)
                {
                    if (Vector3.Distance(keyPointLocation, decayPoints[i]) < _foundKeyPointDistance)
                    {
                        currentVisibility = false;
                        break;
                    }
                }

                //still good?
                if (currentVisibility)
                {
                    FireFoundEvent();
                }
            }
        }

        public Vector3 GetPosition(FilterType type)
        {
            switch(type)
            {
                case FilterType.Filtered:
                    return positionFiltered;
                case FilterType.Raw:
                    return positionRaw;
                default:
                    return Vector3.zero;
            }
        }
#endif
    }
}