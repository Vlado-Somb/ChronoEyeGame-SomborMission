// Copyright 2022 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Google.XR.ARCoreExtensions;
using System.Collections;

namespace Google.XR.ARCoreExtensions.Samples.Geospatial
{
    public class GeoLite : MonoBehaviour
    {
        public static GeoLite Instance { get; private set; }

        [Header("AR Components")]
        [SerializeField] private ARSession session;
        [SerializeField] private ARCoreExtensions arCoreExtensions;
        [SerializeField] private AREarthManager earthManager;

        public bool IsReady =>
            session != null &&
            earthManager != null &&
            ARSession.state == ARSessionState.SessionTracking &&
            earthManager.EarthState == EarthState.Enabled &&
            earthManager.EarthTrackingState == TrackingState.Tracking;

        public GeospatialPose CurrentPose =>
            IsReady ? earthManager.CameraGeospatialPose : default;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private IEnumerator Start()
        {
            yield return StartCoroutine(StartLocationService());

            while (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
                yield return null;

            if (ARSession.state == ARSessionState.Unsupported)
            {
                Debug.LogError("[GeoLite] AR is not supported on this device.");
                yield break;
            }

            Debug.Log("[GeoLite] ARCore session initialized.");
        }

        private IEnumerator StartLocationService()
        {
            if (!Input.location.isEnabledByUser)
            {
                Debug.Log("[GeoLite] Location service not enabled by user.");
                yield break;
            }

            Input.location.Start();
            int maxWait = 20;

            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }

            if (maxWait <= 0 || Input.location.status == LocationServiceStatus.Failed)
            {
                Debug.Log("[GeoLite] Location service failed or timed out.");
                yield break;
            }

            Debug.Log("[GeoLite] Location service started successfully.");
        }
    }
}
