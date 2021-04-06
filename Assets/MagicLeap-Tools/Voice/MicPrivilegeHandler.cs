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

using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeapTools
{
    public class MicPrivilegeHandler : MonoBehaviour
    {
        public enum MicCaptureStateType
        {
            Unchecked,
            PrivilegeDeclined,
            PrivilegeActive,
            PrivilegeNotRequested
        }
        /* Whether microphone usage is allowed currently */
        public MicCaptureStateType MicCaptureState = MicCaptureStateType.Unchecked;

#if PLATFORM_LUMIN && !UNITY_EDITOR

    private MLPrivilegeRequesterBehavior _privilegeRequester;

    public delegate void HandleMicPrivilegeResult(bool canRecord);
    public HandleMicPrivilegeResult OnMicPrivilegeResult;

    private void Awake()
    {
        _privilegeRequester = GetComponent<MLPrivilegeRequesterBehavior>();

        /* If PrivilegeReqesterBehavior component is not on the gameObject, add it and setup for Microphone capture */
        if (_privilegeRequester == null)
        {
            Debug.LogWarning("Missing PrivilegeRequester component, this will be added with only the AudioCaptureMic privilege.");

            _privilegeRequester = gameObject.AddComponent<MLPrivilegeRequesterBehavior>();
            _privilegeRequester.enabled = false;

            _privilegeRequester.Privileges = new MLPrivileges.RuntimeRequestId[]
           {
               MLPrivileges.RuntimeRequestId.AudioCaptureMic
           };
        }
        _privilegeRequester.OnPrivilegesDone += HandlePrivilegesDone;
    }

    private void Start()
    {
        _privilegeRequester.enabled = true;
    }

    private void OnDestroy()
    {
        if (_privilegeRequester != null)
        {
            _privilegeRequester.OnPrivilegesDone -= HandlePrivilegesDone;
        }
    }

    void HandlePrivilegesDone(MLResult result)
    {
        /* Privilege was denied or there was an error during the request */
        if (!result.IsOk)
        {
            Debug.LogError("Failed to get all requested privileges. MLResult: " + result);
            enabled = false;

            if (result.Result == MLResult.Code.PrivilegeDenied)
            {
                Instantiate(Resources.Load("PrivilegeDeniedError"));
            }
            MicCaptureState = MicCaptureStateType.PrivilegeDeclined;
            OnMicPrivilegeResult?.Invoke(false);
            return;
        }

        /* All privileges requested were accepted and one of them was AudioCaptureMic */
        foreach (MLPrivileges.RuntimeRequestId privilege in _privilegeRequester.Privileges)
        {
            if (privilege == MLPrivileges.RuntimeRequestId.AudioCaptureMic)
            {
                MicCaptureState = MicCaptureStateType.PrivilegeActive;
                OnMicPrivilegeResult?.Invoke(true);
                return;
            }
        }
        /* The AudioCaptureMic privilege was not requested */
        Debug.LogError("AudioCaptureMic privilege was not requested by the Privilege Requester component.");
        MicCaptureState = MicCaptureStateType.PrivilegeNotRequested;
        OnMicPrivilegeResult?.Invoke(false);
    }
#endif
    }
}
