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
using UnityEngine;

namespace MagicLeap.MRTK.DeviceManagement
{
    public static class TransformUtilities
    {
        //Private Variables:
        private static Camera _mainCamera;

        private static float _nearClipPlane = 0.37037f;
        //Private Properties:
        private static Camera MainCamera
        {
            get
            {
                if (_mainCamera == null)
                {
                    _mainCamera = Camera.main;
                }
                return _mainCamera;
            }
        }

        private static Plane CameraPlane
        {
            get
            {
                return new Plane(MainCamera.transform.forward, MainCamera.transform.position + MainCamera.transform.forward * (_nearClipPlane));
            }
        }

        //Public Methods:
        public static bool InsideClipPlane(Vector3 location, float clipPlaneOverride = 0)
        {
            if (clipPlaneOverride > 0)
            {
                _nearClipPlane = clipPlaneOverride;
            }
            else
            {
                _nearClipPlane = MainCamera.nearClipPlane;
            }
            return !CameraPlane.GetSide(location);
        }

        public static Vector3 LocationOnClipPlane(Vector3 location)
        {
            return CameraPlane.ClosestPointOnPlane(location);
        }

        public static float DistanceInsideClipPlane(Vector3 location)
        {
            return Vector3.Distance(LocationOnClipPlane(location), location);
        }

        /// <summary>
        /// Equivalent to Transform.InverseTransformPoint - from world space to local space.
        /// </summary>
        public static Vector3 LocalPosition(Vector3 worldPosition, Quaternion worldRotation, Vector3 targetWorldPosition)
        {
            worldRotation.Normalize();
            Matrix4x4 trs = Matrix4x4.TRS(worldPosition, worldRotation, Vector3.one);
            return trs.inverse.MultiplyPoint3x4(targetWorldPosition);
        }

        /// <summary>
        /// Equivalent to Transform.TransformPoint - from local space to world space.
        /// </summary>
        public static Vector3 WorldPosition(Vector3 worldPosition, Quaternion worldRotation, Vector3 localPosition)
        {
            worldRotation.Normalize();
            Matrix4x4 trs = Matrix4x4.TRS(worldPosition, worldRotation, Vector3.one);
            return trs.MultiplyPoint3x4(localPosition);
        }

        public static Quaternion GetRotationOffset(Quaternion from, Quaternion to)
        {
            from.Normalize();
            return Quaternion.Inverse(from) * to;
        }

        public static Quaternion ApplyRotationOffset(Quaternion from, Quaternion offset)
        {
            from.Normalize();
            return from * offset;
        }

        public static Quaternion RotateQuaternion(Quaternion rotation, Vector3 amount)
        {
            return Quaternion.AngleAxis(amount.x, rotation * Vector3.right) *
                Quaternion.AngleAxis(amount.y, rotation * Vector3.up) *
                Quaternion.AngleAxis(amount.z, rotation * Vector3.forward) *
                rotation;
        }
    }
}