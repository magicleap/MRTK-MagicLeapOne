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

using System.Reflection;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.XRSDK;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;
using UnityEngine.XR.MagicLeap.Meshing;
using UnityEngine.XR.Management;

namespace MagicLeap.MRTK.SpatialAwareness
{
    [MixedRealityDataProvider(
        typeof(IMixedRealitySpatialAwarenessSystem),
        SupportedPlatforms.Lumin,
        "Magic Leap Spatial Mesh Observer")]
    [HelpURL(
        "https://docs.microsoft.com/windows/mixed-reality/mrtk-unity/features/spatial-awareness/spatial-awareness-getting-started")]
    public class MagicLeapSpatialMeshObserver :
        GenericXRSDKSpatialMeshObserver
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="registrar">The <see cref="IMixedRealityServiceRegistrar"/> instance that loaded the service.</param>
        /// <param name="name">Friendly name of the service.</param>
        /// <param name="priority">Service priority. Used to determine order of instantiation.</param>
        /// <param name="profile">The service's configuration profile.</param>
        public MagicLeapSpatialMeshObserver(
            IMixedRealitySpatialAwarenessSystem spatialAwarenessSystem,
            string name = null,
            uint priority = DefaultPriority,
            BaseMixedRealityProfile profile = null) : base(spatialAwarenessSystem, name, priority, profile)
        {
        }

        private XRMeshSubsystem meshSubsystem;

        
        #region BaseSpatialObserver Implementation

        /// <summary>
        /// Creates the XRMeshSubsystem and handles the desired startup behavior.
        /// </summary>
        protected override void CreateObserver()
        {
            if (Service == null
#if XR_MANAGEMENT_ENABLED
                || XRGeneralSettings.Instance == null || XRGeneralSettings.Instance.Manager == null || XRGeneralSettings.Instance.Manager.activeLoader == null
#endif // XR_MANAGEMENT_ENABLED
            ) { return; }

#if XR_MANAGEMENT_ENABLED
            meshSubsystem = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRMeshSubsystem>();
#else
            meshSubsystem = XRSubsystemHelpers.MeshSubsystem;
#endif // XR_MANAGEMENT_ENABLED

            if (meshSubsystem != null)
            {
                ConfigureObserverVolume();
            }
            
            base.CreateObserver();
        }
        /// <summary>
        /// Sets the Magic Leap Mesh settings based on the profile assigned. 
        /// </summary>
        protected override void ReadProfile()
        {
            base.ReadProfile();
            
            MagicLeapSpatialAwarenessMeshObserverProfile profile = ConfigurationProfile as MagicLeapSpatialAwarenessMeshObserverProfile;
            if (profile == null)
            {
                Debug.LogWarning($" Use the `MagicLeapSpatialAwarenessMeshObserverProfile` configuration to set Magic Leap specific meshing settings.");

                //Magic Leap Default Meshing Settings
                var defaultFlags = MLMeshingFlags.IndexOrderCCW;
                defaultFlags |= MLMeshingFlags.RemoveMeshSkirt;

                var defaultsettings = new MLMeshingSettings
                {
                    flags = defaultFlags,
                    fillHoleLength = 1,
                    disconnectedComponentArea = .25f
                };
            
                MeshingSettings.meshingSettings = defaultsettings;
                MeshingSettings.batchSize = 16;
                return;
            }
            
            //Magic Leap Meshing Settings
            var flags = MLMeshingFlags.IndexOrderCCW;
           
            if(profile.RemoveMeshSkirt)
                flags |= MLMeshingFlags.RemoveMeshSkirt;

            if(profile.RequestVertexConfidence)
                flags |= MLMeshingFlags.ComputeConfidence;
            
            if(profile.Planarize)
                flags |= MLMeshingFlags.Planarize;
            
            var settings = new MLMeshingSettings
            {
                flags = flags,
                fillHoleLength =profile.FillHoleLength,
                disconnectedComponentArea = profile.DisconnectedComponentArea
            };
            
            MeshingSettings.meshingSettings = settings;
            MeshingSettings.batchSize = profile.BatchSize;
        }

        #endregion BaseSpatialObserver Implementation

        #region BaseSpatialMeshObserver Implementation

        /// <inheritdoc />
        protected override int LookupTriangleDensity(SpatialAwarenessMeshLevelOfDetail levelOfDetail)
        {
            // For non-custom levels, the enum value is the appropriate triangles per cubic meter.
            int level = (int)levelOfDetail;
            if (meshSubsystem != null)
            {
                if (levelOfDetail == SpatialAwarenessMeshLevelOfDetail.Unlimited)
                {
                    MeshingSettings.density =  meshSubsystem.meshDensity = 1;
                }
                else
                {
                    MeshingSettings.density =  meshSubsystem.meshDensity = level / (float)SpatialAwarenessMeshLevelOfDetail.Fine; // For now, map Coarse to 0.0 and Fine to 1.0
                }
            }
            return level;
        }

        #endregion BaseSpatialMeshObserver Implementation

        #region IMixedRealitySpatialAwarenessObserver Implementation

        private static readonly ProfilerMarker ResumePerfMarker =
            new ProfilerMarker("[MRTK] MagicLeapSpatialMeshObserver.Resume");

        /// <inheritdoc/>
        public override void Resume()
        {
            if (IsRunning)
            {
                Debug.LogWarning("The Magic Leap spatial observer is currently running.");
                return;
            }
            using (ResumePerfMarker.Auto())
            {
                StartMLMeshSubsystem();
            }
            base.Resume();
        }

        private void StartMLMeshSubsystem()
        {
            
            MagicLeapLoader m_Loader = XRGeneralSettings.Instance.Manager.ActiveLoaderAs<MagicLeapLoader>();

            //The loader can be null if the has not finished initializing.
            if(m_Loader == null){ 
                return;
            }
            
            //Use of reflections is required because of Unity's current implementation of MagicLeap's Meshing subsystem.
            //Unity loads the system but does not initialize or suspend it.
            MethodInfo dynMethod = m_Loader.GetType().GetMethod("StartMeshSubsystem",
                BindingFlags.NonPublic | BindingFlags.Instance);
           
            //Invoke the internal StartMeshSubsystem function.
            if (dynMethod != null)
                dynMethod.Invoke(m_Loader, null);
            
        }

        private static readonly ProfilerMarker SuspendPerfMarker =
            new ProfilerMarker("[MRTK] MagicLeapSpatialMeshObserver.Suspend");

        /// <inheritdoc/>
        public override void Suspend()
        {
            if (!IsRunning)
            {
                Debug.LogWarning("The XR SDK spatial observer is currently stopped.");
                return;
            }
            
            using (SuspendPerfMarker.Auto())
            {
                StopMLMeshSubsystem();
            }
            
            base.Suspend();
        }

        private void StopMLMeshSubsystem()
        {
           
            MagicLeapLoader m_Loader = XRGeneralSettings.Instance.Manager.ActiveLoaderAs<MagicLeapLoader>();

            //The loader can be null if the application is quitting.
            if(m_Loader == null){ 
                return;
            }
            
            //Use of reflections is required because of Unity's current implementation of MagicLeap's Meshing subsystem.
            //Unity loads the system but does not initialize or suspend it.
            MethodInfo dynMethod = m_Loader.GetType().GetMethod("StopMeshSubsystem",
                BindingFlags.NonPublic | BindingFlags.Instance);
           
            //Invoke the internal StopMeshSubsystem function.
            if (dynMethod != null)
                dynMethod.Invoke(m_Loader, null);
            
        }

        #endregion IMixedRealitySpatialAwarenessObserver Implementation

        #region Helpers

        private static readonly ProfilerMarker ConfigureObserverVolumePerfMarker =
            new ProfilerMarker("[MRTK] MagicLeapSpatialMeshObserver.ConfigureObserverVolume");

        /// <summary>
        /// Applies the configured observation extents.
        /// </summary>
        protected override void ConfigureObserverVolume()
        {
            base.ConfigureObserverVolume();
            
            using (ConfigureObserverVolumePerfMarker.Auto())
            {
                // Update the observer
                MeshingSettings.SetBounds(ObserverOrigin, ObserverRotation, ObservationExtents);
            }
        }
        
        #endregion Helpers

    }
}
