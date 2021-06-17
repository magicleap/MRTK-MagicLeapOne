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

namespace MagicLeap.MRTK.DeviceManagement
{
    public static class MotionUtilities
    {
        public static Quaternion SmoothDamp(Quaternion current, Quaternion target, ref Quaternion velocity, float duration)
        {
            //double-cover correction:
            float dot = Quaternion.Dot(current, target);
            float multi = dot > 0f ? 1f : -1f;
            target.x *= multi;
            target.y *= multi;
            target.z *= multi;
            target.w *= multi;

            //smooth damp:
            Vector4 smoothDamped = Vector4.Normalize(new Vector4(
                Mathf.SmoothDamp(current.x, target.x, ref velocity.x, duration),
                Mathf.SmoothDamp(current.y, target.y, ref velocity.y, duration),
                Mathf.SmoothDamp(current.z, target.z, ref velocity.z, duration),
                Mathf.SmoothDamp(current.w, target.w, ref velocity.w, duration)
            ));

            //velocities:
            var dtInv = 1f / Time.deltaTime;
            velocity.x = (smoothDamped.x - current.x) * dtInv;
            velocity.y = (smoothDamped.y - current.y) * dtInv;
            velocity.z = (smoothDamped.z - current.z) * dtInv;
            velocity.w = (smoothDamped.w - current.w) * dtInv;

            return new Quaternion(smoothDamped.x, smoothDamped.y, smoothDamped.z, smoothDamped.w);
        }
    }
}