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

using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.XR.MagicLeap;

namespace MagicLeapTools.Voice
{
    public class GoogleSpeechToText : SpeechToText
    {
        #region Serialized Fields
        [Header("Google  Settings")]
        [SerializeField]
        private string _uri = "https://speech.googleapis.com/v1/speech:recognize?&key="; // receive results after all audio has been sent and processed.
        [SerializeField, Tooltip("This is where your Google API Key goes ")]
        private string _APIKey = "";
        #endregion

        #region Calculation Variables
        private int _micPrevPos = 0;
        private float _timeAtSilenceBegan = 0.0f;
        private float[] _samples;
        private bool _audioDetected = false;
        private bool _requestNeedsSending = false;
        private bool _currentlyRecording = false;
        private GoogleRequestData _googleRequestData = new GoogleRequestData(); // What is sent to Google
        private SpeechRecognitionResultObj _newResult;
        private AudioClip _audioRecording = null;
        #endregion

        #region Getters / Setters
        public SpeechRecognitionResultObj NewResult
        {
            set
            {
                if (value == null ||
                    value.results[0] == null ||
                    value.results[0].alternatives == null)
                {
                    _newResult = null;
                    Debug.Log("No words detected");
                    return;
                }
                _newResult = value;


                Debug.Log("Invoke speech processed with " + _newResult.results[0].alternatives[0].transcript);
                if (_detectPhrases)
                    CheckForTrackedWords(_newResult.results[0].alternatives[0].transcript);
                else
                    base.FireOnSpeechProcessed(_newResult.results[0].alternatives[0].transcript,
                                              _newResult.results[0].alternatives[0].confidence);
            }
        }
        #endregion

        #region Unity Methods
#if PLATFORM_LUMIN
        protected override void Awake()
        {
            base.Awake();
        }
#endif
        private void Start()
        {
            if(_canRecord)
            {
                SetupService(_detectPhrases,_autoDetectVoice);
            }
            if (_APIKey == "")
            {
                Debug.LogError("No APIKey set, voice to text will not be possible");
            }
        }

        private void Update()
        {
            if (_canRecord && _isSetup == false)
                SetupService(_detectPhrases,_autoDetectVoice);

            if (_autoDetectVoice == true && _microphoneActive == true)
            {
                DetectAudio();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause == true)
            {
                StopMicrophoneCapture(false);
            }
            else if (_autoDetectVoice == true) // Returning from pause and auto detect is on
            {
                StartMicrophoneCapture();
            }
        }
        #endregion

        #region Virtual Methods
        public override void  SetupService(bool detectPhrases, bool autoDetectVoice)
        {
            base.SetupService(detectPhrases, autoDetectVoice);
            if(_canRecord && autoDetectVoice)
            {
                _isSetup = true;
                ToggleActivelyRecording(true);
            }
        }

        protected override void SendToService()
        {
            base.SendToService();
            SendRequestToGoogle();

        }
        #endregion

        #region Google Methods
        private void SendRequestToGoogle()
        {
            if (_autoDetectVoice == false) // _samples has already been populated in DetectAudio if auto detection is used
            {
                FillSamples(0);
            }
                
            // _samples are in range [-1.0f, 1.0f];
            short shortSample;
            Byte[] bytesData = new Byte[_samples.Length * 2];

            float rescaleFactor = 32767; // to put float values in range of [-32767, 32767] for correct conversion
            Byte[] shortBytes = new Byte[2];
            for (int i = 0; i < _samples.Length; i++)
            {
                shortSample = (short)(_samples[i] * rescaleFactor); // convert to short
                shortBytes = BitConverter.GetBytes(shortSample);    // Get bytes from short
                shortBytes.CopyTo(bytesData, i * 2);                // Copy bytes to full array
            }
            // Convert bytes to the Base64 string that Google Speech wants
            _googleRequestData.audio.content = Convert.ToBase64String(bytesData, Base64FormattingOptions.None);

            StartCoroutine(PostGoogleRequest(_uri + _APIKey));
        }

        IEnumerator PostGoogleRequest(string uri)
        {
            FireOnSpeechStateUpdated(SpeechToTextStates.ProcessingSpeech);

            UnityWebRequest uwr = new UnityWebRequest(uri, "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(JsonUtility.ToJson(_googleRequestData, false));
            uwr.uploadHandler = new UploadHandlerRaw(jsonToSend);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            //Send the request then wait here until it returns
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error While Sending: " + uwr.error);
            }
            else
            {
                FireOnSpeechStateUpdated(SpeechToTextStates.ProcessedSpeech);
                NewResult = JsonUtility.FromJson<SpeechRecognitionResultObj>(uwr.downloadHandler.text);
            }
            yield break;
        }
        #endregion

        #region Microphone / Voice Input Helpers
        

        public override void ToggleActivelyRecording(bool enable)
        {
            if (_canRecord == false)
                return;

            if (enable)
            {
                StartMicrophoneCapture();
            }
            else
            {
                StopMicrophoneCapture();
            }
        }

        private void StartMicrophoneCapture()
        {
            if (_canRecord && _currentlyRecording == false)
            {
                FireOnSpeechStateUpdated(SpeechToTextStates.RecordingSpeech);

                _microphoneActive = true;
                _audioRecording = Microphone.Start(Microphone.devices[0], true, _maxRecordingLength, _googleRequestData.config.sampleRateHertz);
                _currentlyRecording = true;
            }
        }

        private void StopMicrophoneCapture(bool sendLastRequest = true)
        {
            Microphone.End(Microphone.devices[0]);
            FireOnSpeechStateUpdated(SpeechToTextStates.MicrophoneStopped);

            _currentlyRecording = false;
            if (sendLastRequest == true)
            {
                SendRequestToGoogle();
            }
        }

        /* Used to determine when t he user has started and stopped speaking */
        private void DetectAudio()
        {
            FillSamples(_micPrevPos);

            // Determine if the microphone noise levels have been loud enough
            float maxVolume = 0.0f;
            for (int i = _micPrevPos + 1; i < Microphone.GetPosition(null); ++i)
            {
                if (i >= _samples.Length)
                    Debug.LogError("WAS " + i + "in lenght: " + _samples.Length);
                if (_samples[i] > maxVolume)
                {
                    maxVolume = _samples[i];
                }
            }

            if (maxVolume > _minimumSpeakingSampleValue)
            {
                if (_audioDetected == false) // User first starts talking after a gap
                {
                    _audioDetected = true;
                    _requestNeedsSending = true;
                }
            }
            else // max volume below threshold
            {
                if (_audioDetected == true) // User first stopped talking after talking
                {
                    _timeAtSilenceBegan = Time.time;
                    _audioDetected = false;
                }
                else if (_requestNeedsSending == true) // while no new voice input is detected
                {
                    if (Time.time - _timeAtSilenceBegan > _silenceTimer)
                    {
                        _audioDetected = false;
                        _requestNeedsSending = false;
                        SendRequestToGoogle();
                        ClearSamples();
                    }
                }
            }
            _micPrevPos = Microphone.GetPosition(null);
        }

        /* Compare returned string with Tracked words and send delegate */
        private bool CheckForTrackedWords(string newPhrase)
        {
            for (int i = 0; i < TrackedPhrases?.Length; ++i)
            {
                if (newPhrase == TrackedPhrases[i]) 
                {
                    FireOnTrackedPhraseFound(i);
                    return true;
                }
            }
            FireOnUntrackedPhraseFound(newPhrase);
            return false;
        }

        void FillSamples(int micPosition)
        {
            _samples = new float[_audioRecording.samples]; // make a float array to hold the samples
            _audioRecording.GetData(_samples, micPosition); // Fill that array (values [-1.0f -> 1.0]
        }

        void ClearSamples()
        {
            for (int i = 0; i < _samples.Length; ++i)
            {
                _samples[i] = 0.0f;
            }
            
            _audioRecording.SetData(_samples, 0);
        }
        #endregion
    }
}
