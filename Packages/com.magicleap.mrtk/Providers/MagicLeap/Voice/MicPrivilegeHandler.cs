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

using System;
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
    
    public delegate void HandleMicPrivilegeResult(bool canRecord);
    public HandleMicPrivilegeResult OnMicPrivilegeResult;

    private MLResult RequestPrivilegesAsync(Action<MLResult> callback, params MLPrivileges.Id[] privileges)
    {
        int numPrivilegesToRequest = privileges.Length;
        MLResult _result = MLResult.Create(MLResult.Code.Pending);
        for(int i = 0; i < privileges.Length; i++)
        {
            MLPrivileges.Id privilege = privileges[i];

            _result = CheckPrivilege(privilege);
            if (_result.Result == MLResult.Code.PrivilegeGranted)
            {
                numPrivilegesToRequest--;
                if(numPrivilegesToRequest == 0)
                {
                    callback?.Invoke(_result);
                }
                continue;
            }

            _result = MLPrivileges.RequestPrivilegeAsync(privilege, (MLResult result, MLPrivileges.Id priv) =>
            {
                numPrivilegesToRequest--;

                if (result.Result == MLResult.Code.PrivilegeGranted)
                {
                    if (numPrivilegesToRequest == 0)
                    {
                        callback?.Invoke(result);
                    }
                }

                // Privilege was not granted
                else
                {
                    numPrivilegesToRequest = 0;
                    if (numPrivilegesToRequest == 0)
                    {
                        callback?.Invoke(result);
                    }
                }
            });

            if (!_result.IsOk)
            {
                return _result;
            }
        }

        // Override result in case privilege was already granted.
        if(_result.Result == MLResult.Code.PrivilegeGranted)
        {
            _result = MLResult.Create(MLResult.Code.Ok);
        }

        return _result;
    }
    
    /// <summary>
    /// Used to check if your privilege has already been granted.
    /// </summary>
    /// <param name="privilege">The privilege to check for.</param>
    public static MLResult CheckPrivilege(MLPrivileges.Id privilege)
    {
#if PLATFORM_LUMIN
        var _result = MLPrivileges.CheckPrivilege(privilege);

        if (_result.Result != MLResult.Code.PrivilegeGranted && _result.Result != MLResult.Code.PrivilegeNotGranted)
        {
            Debug.LogErrorFormat("Error: MLPrivilegesStarterKit.CheckPrivilege failed for the privilege {0}. Reason: {1}", privilege, _result);
        }
#endif

        return _result;
    }

    private void Start()
    {
        RequestPrivilegesAsync(HandlePrivilegesDone, new MLPrivileges.Id[]
        {
            MLPrivileges.Id.AudioCaptureMic
        });
    }

    private void OnDestroy()
    {
      
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
        else
        {
            MicCaptureState = MicCaptureStateType.PrivilegeActive;
            OnMicPrivilegeResult?.Invoke(true);
            return;
        }
    }
#endif
    }
}
