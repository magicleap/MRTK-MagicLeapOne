using MagicLeap.MRTK.DeviceManagement.Input;
using UnityEngine;
namespace MagicLeap.MRTK.Samples
{
    /// <summary>
    /// Demo script to show how to toggle hand tracking settings.
    /// The settings can be changed at runtime
    /// </summary>
    public class SetHandTrackingSettings : MonoBehaviour
    {
        public MagicLeapDeviceManager.HandSettings _settings;

        // Start is called before the first frame update
        void Start()
        {
            MagicLeapDeviceManager.Instance.CurrentHandSettings = _settings;
        }
    }
}
