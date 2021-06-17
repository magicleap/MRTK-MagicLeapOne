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

using MagicLeap.MRTK.SpatialAwareness;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace MagicLeap.MRTK.SpatialAwareness
{
    /// <summary>
    /// Configuration profile settings for spatial awareness mesh observers.
    /// </summary>
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Profiles/Magic Leap Spatial Awareness Mesh Observer Profile",
        fileName = "MagicLeapSpatialAwarenessMeshObserverProfile",
        order = (int) CreateProfileMenuItemIndices.SpatialAwarenessMeshObserver)]
    [MixedRealityServiceProfile(typeof(MagicLeapSpatialMeshObserver))]
    [HelpURL(
        "https://docs.microsoft.com/windows/mixed-reality/mrtk-unity/features/spatial-awareness/configuring-spatial-awareness-mesh-observer")]
    public class MagicLeapSpatialAwarenessMeshObserverProfile : MixedRealitySpatialAwarenessMeshObserverProfile
    {
        [Tooltip(
            "When enabled, the system will generate confidence values for each vertex, ranging from 0-1, which can be accessed using TryGetConfidence(MeshId, List{float})")]
        public bool RequestVertexConfidence = false;

        [Tooltip("When enabled, the system will planarize the returned mesh (planar regions will be smoothed out).")]
        public bool Planarize = false;

        [Tooltip("When enabled, the mesh skirt (overlapping area between two mesh blocks) will be removed.")]
        public bool RemoveMeshSkirt = true;

        [Tooltip("Boundary distance (in meters) of holes you wish to have filled.")]
        public float FillHoleLength = 1;

        [Tooltip(
            "Any component that is disconnected from the main mesh and which has an area less than this size will be removed.")]
        public float DisconnectedComponentArea = .25f;

        [Tooltip("How many meshes to update per batch. Larger values are more efficient, but have higher latency.")]
        public int BatchSize = 16;

    }
}
