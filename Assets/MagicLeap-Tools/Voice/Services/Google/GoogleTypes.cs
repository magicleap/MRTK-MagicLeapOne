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

namespace MagicLeapTools.Voice
{
    [Serializable]
    public class GoogleRequestData // These follow the same names Google looks for in their requests
    {
        public RecognitionConfig config = new RecognitionConfig();
        public RecognitionAudio  audio  = new RecognitionAudio();
    }

    [Serializable]
    public class RecognitionAudio
    {
        public string content; // byte string (base64 encoding)
    }

    [Serializable]
    public class RecognitionConfig
    {
        public string encoding = "LINEAR16"; // Required
        public int sampleRateHertz = 16000; // Required
        public int audioChannelCount = 0; // Optional
        public bool enableSeparateRecognitionPerChannel = false; //Optional
        public string languageCode = "en-US"; // Required
        public int maxAlternatives = 1; //Optional [0-30] 
        public bool profanityFilter = false; //Optional

        public SpeechContext[] speechContexts = new SpeechContext[1];
        public bool enableWordTimeOffsets = false;
    }

    [Serializable]
    public class SpeechContext
    {
        public string[] phrases = { "Enable Map", "Disable Map" }; // This makes it so it will be able to better pick up on these 
    }

    [Serializable]
    public class SpeechRecognitionResultObj
    {
        public SpeechRecognitionResult[] results = new SpeechRecognitionResult[1]; // same as maxAlternatives
    }

    [Serializable]
    public class SpeechRecognitionResult
    {
        public SpeechRecognitionAlternative[] alternatives = new SpeechRecognitionAlternative[1]; // same as maxAlternatives
    }

    [Serializable]
    public class SpeechRecognitionAlternative
    {
        public string transcript; // words the user spoke
        public float confidence; // [0.0 - 1.0]
    }
}
