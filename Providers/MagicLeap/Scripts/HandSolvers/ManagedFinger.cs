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
    public class ManagedFinger
    {
#if PLATFORM_LUMIN
        //Events:
        public event Action<ManagedFinger, bool> OnVisibilityChanged;

        //Public Variables:
        public ManagedKeypoint[] points;

        //Public Properties:
        public ManagedKeypoint Knuckle
        {
            get
            {
                return points[0];
            }
        }

        public ManagedKeypoint Joint
        {
            get
            {
                if (points.Length == 3)
                {
                    return points[1];
                }
                else
                {
                    return null;
                }
            }
        }

        public ManagedKeypoint Tip
        {
            get
            {
                return points[points.Length - 1];
            }
        }

        public float DotProduct
        {
            get
            {
                if (IsVisible)
                {
                    if (points.Length == 2)
                    {
                        return 1;
                    }
                    else
                    {
                        Vector3 a = Vector3.Normalize(points[1].positionFiltered - points[0].positionFiltered);
                        Vector3 b = Vector3.Normalize(points[2].positionFiltered - points[1].positionFiltered);
                        return Vector3.Dot(a, b);
                    }
                }
                else
                {
                    return 0;
                }
            }
        }

        public FingerType FingerType
        {
            get;
            private set;
        }


        public bool IsVisible
        {
            get;
            private set;
        }

        public bool PartiallyVisible
        {
            get
            {
                foreach (var item in points)
                {
                    if (item.Visible)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public Vector3 DirectionFiltered
        {
            get
            {
                for (int i = points.Length - 1; i >= 1; i--)
                {
                    if (points[i].Visible && points[i - 1].Visible)
                    {
                        _directionFiltered = Vector3.Normalize(points[i].positionFiltered - points[i - 1].positionFiltered);
                    }
                }

                return _directionFiltered;
            }
        }

        public Vector3 DirectionRaw
        {
            get
            {
                for (int i = points.Length - 1; i >= 1; i--)
                {
                    if (points[i].Visible && points[i - 1].Visible)
                    {
                        _directionRaw = Vector3.Normalize(points[i].positionRaw - points[i - 1].positionRaw);
                    }
                }

                return _directionRaw;
            }
        }

        public Vector3 End
        {
            get
            {
                return _lastReliableEndPosition;
            }
        }

        public Vector3[] PointLocationsFiltered
        {
            get
            {
                Vector3[] pointLocations = new Vector3[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    pointLocations[i] = points[i].positionFiltered;
                }
                return pointLocations;
            }
        }

        public Vector3[] PointLocationsRaw
        {
            get
            {
                Vector3[] pointLocations = new Vector3[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    pointLocations[i] = points[i].positionRaw;
                }
                return pointLocations;
            }
        }


        //Private Variables:
        private MLHandTracking.Hand _hand;
        private Vector3 _directionFiltered;
        private Vector3 _directionRaw;
        private ManagedKeypoint _lastReliableEnd;
        private Vector3 _lastReliableEndPosition;
        private bool _endIsTransitioning;
        private float _endTransitionTime = .1f;
        private float _endTransitionMaxDuration = .5f;
        private Vector3 _endArrivalVelocity;
        private float _endTransitionStartTime;
        private RollingAverageFloat _length = new RollingAverageFloat();

        //Constructors:
        public ManagedFinger(MLHandTracking.Hand hand, FingerType fingerType, params ManagedKeypoint[] points)
        {
            //sets:
            _hand = hand;
            this.points = points;
            _lastReliableEnd = points[0];
            FingerType = fingerType;

            //hooks:
            foreach (var item in points)
            {
                item.OnFound += HandlePointVisibilityChanged;
                item.OnLost += HandlePointVisibilityChanged;
            }

            //initial events:
            OnVisibilityChanged?.Invoke(this, false);
            HandlePointVisibilityChanged();
        }

        //Public Methods:
        public void FireFoundEvent()
        {
            IsVisible = true;
            OnVisibilityChanged?.Invoke(this, true);
        }

        public void FireLostEvent()
        {
            IsVisible = false;
            OnVisibilityChanged?.Invoke(this, false);
        }

        //Event Handlers:
        private void HandlePointVisibilityChanged()
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].Visible && _lastReliableEnd != points[i])
                {
                    _lastReliableEnd = points[i];
                    _endTransitionStartTime = Time.realtimeSinceStartup;
                    _endIsTransitioning = true;
                }
            }
        }

        //Loops:
        public void Update()
        {
            //hand not tracked:
            if (IsVisible && !_hand.IsVisible)
            {
                FireLostEvent();
            }

            //don't process if there is no hand:
            if (!_hand.IsVisible)
            {
                return;
            }

            //visibility status:
            bool currentVisibility = true;

            //if all points are visible then this digit is visible:
            foreach (var item in points)
            {
                if (!item.Visible)
                {
                    currentVisibility = false;
                    break;
                }
            }

            //if root point is not visible then it is likely far down in view and should be considered not visible:
            if (!points[0].Visible)
            {
                currentVisibility = false;
            }

            //found:
            if (currentVisibility && !IsVisible)
            {
                FireFoundEvent();
            }

            //lost:
            if (!currentVisibility && IsVisible)
            {
                FireLostEvent();
            }

            //move end if it changed:
            if (_endIsTransitioning)
            {
                //position:
                _lastReliableEndPosition = Vector3.SmoothDamp(_lastReliableEndPosition, _lastReliableEnd.positionFiltered, ref _endArrivalVelocity, _endTransitionTime);
                float endDelta = Vector3.Distance(_lastReliableEndPosition, _lastReliableEnd.positionFiltered);

                //close enough to hand off?
                if (endDelta < .001f)
                {
                    _endIsTransitioning = false;
                }

                //taking too long?
                if (Time.realtimeSinceStartup - _endTransitionStartTime > _endTransitionMaxDuration)
                {
                    _endIsTransitioning = false;
                }
            }
            else
            {
                _lastReliableEndPosition = _lastReliableEnd.positionFiltered;
            }

            if (Joint == null)
            {
                _length.AddData(Vector3.Distance(Knuckle.positionRaw, Tip.positionRaw));
            }
            else
            {
                _length.AddData(Vector3.Distance(Knuckle.positionRaw, Joint.positionRaw) + Vector3.Distance(Joint.positionRaw, Tip.positionRaw));
            }
        }

        public ManagedKeypoint GetKeypoint(KeypointType keypointType)
        {
            switch (keypointType)
            {
                case KeypointType.MCP:
                    return points[0];
                case KeypointType.PIP:
                    return points.Length == 2 ? null : points[1];
                case KeypointType.Tip:
                    return points[points.Length - 1];
            }
            return null;
        }
#endif
    }
}