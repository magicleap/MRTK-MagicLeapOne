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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using System;

using UnityEngine.EventSystems;
namespace MagicLeap.MRTK.DeviceManagement.Input.Voice
{
    public class SpeechToMRTK : MonoBehaviour
    {
        public class MagicLeapSpeechInputProvider : BaseInputDeviceManager, IMixedRealitySpeechSystem
        {
            public MagicLeapSpeechInputProvider(
               IMixedRealityInputSystem inputSystem,
               string name = null,
               uint priority = DefaultPriority,
               BaseMixedRealityProfile profile = null) : base(inputSystem, name, priority, profile) { } // This gets pulled in from the first side. 

            /// <summary>
            /// The keywords to be recognized and optional keyboard shortcuts.
            /// </summary>
            private SpeechCommands[] Commands => InputSystemProfile.SpeechCommandsProfile.SpeechCommands;


            /// <summary>
            /// The reference to the Magic Leap Speech System
            /// </summary>
            private MagicLeapTools.Voice.SpeechToText MLSpeech;

            /// <summary>
            /// The Input Source for Speech Input.
            /// </summary>
            public IMixedRealityInputSource InputSource = null;

            /// <summary>
            /// The minimum confidence level for the recognizer to fire an event.
            /// </summary>
            public RecognitionConfidenceLevel RecognitionConfidenceLevel { get; set; }

            // Required InheritedMethods: 
            public bool IsRecognitionActive { get { return _keywordRecognizerActive; } }
            public void StartRecognition() { MLSpeech?.ToggleActivelyRecording(true); }
            public void StopRecognition() { MLSpeech?.ToggleActivelyRecording(false); }

            private bool _keywordRecognizerActive = false;

            private void InitializeKeywordRecognizer()
            {
                IMixedRealityInputSystem inputSystem = Service as IMixedRealityInputSystem;

                InputSource = inputSystem?.RequestNewGenericInputSource("Magic Leap Speech Input Source", sourceType: InputSourceType.Voice);

                InitializeMLSpeech();
            }

            private void InitializeMLSpeech()
            {
                MLSpeech = MagicLeapTools.Voice.SpeechToText.Instance;

                if (MLSpeech)
                {
                    _keywordRecognizerActive = true;

                    MLSpeech.OnSpeechProcessed += HandleSpeechProcessed;
                    MLSpeech.SetupService(false, true);
                }
            }


            public override void Enable()
            {
                if (InputSystemProfile.SpeechCommandsProfile.SpeechRecognizerStartBehavior == AutoStartBehavior.AutoStart)
                {
                    InitializeKeywordRecognizer();
                }
            }

            public override void Update()
            {
                if (!MLSpeech)
                {
                    InitializeMLSpeech();
                }

                base.Update();
            }

            private void OnPhraseRecognized(RecognitionConfidenceLevel confidence, TimeSpan phraseDuration, DateTime phraseStartTime, string text)
            {
                IMixedRealityInputSystem inputSystem = Service as IMixedRealityInputSystem;

                for (int i = 0; i < Commands?.Length; i++)
                {
                    if (Commands[i].LocalizedKeyword.ToUpper() == text.ToUpper())
                    {
                        inputSystem?.RaiseSpeechCommandRecognized(InputSource, confidence, phraseDuration, phraseStartTime, Commands[i]);
                        break;
                    }
                }
            }

            private void HandleSpeechProcessed(string phrase, float confidence)
            {
                OnPhraseRecognized(ConfidencePercentageToMRTK(confidence), TimeSpan.Zero, DateTime.UtcNow, phrase);
            }

            private RecognitionConfidenceLevel ConfidencePercentageToMRTK(float confidence)
            {
                if (confidence > 0.90f)
                    return RecognitionConfidenceLevel.High;
                if (confidence > 0.50f)
                    return RecognitionConfidenceLevel.Medium;
                if (confidence > 0.10f)
                    return RecognitionConfidenceLevel.Low;
                return RecognitionConfidenceLevel.Unknown;
            }
        }
    }
}
