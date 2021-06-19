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
//

using UnityEngine;

namespace MagicLeapTools.Voice
{
    public enum SpeechToTextStates
    {
        MicrophoneUnavailable,
        MicrophoneStopped,
        MicrophoneStarted,
        RecordingSpeech,
        ProcessingSpeech,
        ProcessedSpeech
    }

    public delegate void StateUpdated(SpeechToTextStates state);

    /// If Detect Phrases is false the following event will be sent
    public delegate void SpeechProcessed(string phrase, float confidence);

    // If DetectPhrases is true, comparing will happen here
    public delegate void TrackedPhraseFound(int phraseIndex);
    public delegate void UntrackedPhraseFound(string phrase);

    public class SpeechToText : MonoBehaviour
    {
        #region Events
        public event TrackedPhraseFound OnTrackedPhraseFound;
        public event UntrackedPhraseFound OnUntrackedPhraseFound;
        public event StateUpdated OnSpeechStateUpdated;
        public event SpeechProcessed OnSpeechProcessed;

        protected void FireOnSpeechProcessed(string phrase, float confidence)
        {
            OnSpeechProcessed?.Invoke(phrase, confidence);
        }

        protected void FireOnSpeechStateUpdated(SpeechToTextStates state)
        {
            OnSpeechStateUpdated?.Invoke(state);
        }

        protected void FireOnUntrackedPhraseFound(string phrase)
        {
            OnUntrackedPhraseFound?.Invoke(phrase);
        }

        protected void FireOnTrackedPhraseFound(int phraseIndex)
        {
            OnTrackedPhraseFound?.Invoke(phraseIndex);
        }
        #endregion

        #region Serialized Fields
        [Header("Voice Input Settings")]
        [SerializeField, Tooltip("If true, voice input will be automatically detected, otherwise hold down bumper to speak and release bumper to send for conversion")]
        protected bool _autoDetectVoice = true;
        [SerializeField, Tooltip("Maximum length of recording in seconds.")]
        protected int _maxRecordingLength = 5;
        [SerializeField, Tooltip("Time in seconds of detected silence before voice request is sent")]
        protected float _silenceTimer = 1.0f;
        [SerializeField, Tooltip("The minimum volume to detect voice input for"), Range(0.0f, 1.0f)]
        protected float _minimumSpeakingSampleValue = 0.05f;
        #endregion

        public static SpeechToText Instance;
        public string[] TrackedPhrases;

        protected bool _detectPhrases;
        protected bool _microphoneActive = true;
        protected bool _canRecord = false;
        protected bool _isSetup = false;

        #region Magic Leap Components
        protected MicPrivilegeHandler _micPrivilegeHandler;
        #endregion


        public virtual void ToggleActivelyRecording(bool enable) { }

        public virtual void SetupService(bool detectPhrases, bool autoDetectVoice) {
            _detectPhrases = detectPhrases;
            _autoDetectVoice = autoDetectVoice;
        }

        private void SetupSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        protected virtual void Awake()
        {
            SetupSingleton();

#if PLATFORM_LUMIN && !UNITY_EDITOR
            if (_micPrivilegeHandler == null)
            {
                _micPrivilegeHandler = GetComponent<MicPrivilegeHandler>();
            }

            if (_micPrivilegeHandler == null)
            {
                Debug.LogError("Missing required component");
                enabled = false;
                return;
            }

            _micPrivilegeHandler.OnMicPrivilegeResult += HandleMicPrivilegeResult;
#else
            _canRecord = true;
#endif

        }

        void HandleMicPrivilegeResult(bool canRecord)
        {
#if PLATFORM_LUMIN && !UNITY_EDITOR
            _micPrivilegeHandler.OnMicPrivilegeResult -= HandleMicPrivilegeResult;
            _canRecord = canRecord;
#endif
            if (!canRecord)
            {
                _canRecord = false;
                OnSpeechStateUpdated?.Invoke(SpeechToTextStates.MicrophoneUnavailable);
            }
        }

        protected virtual void SendToService() { }
    }
}
