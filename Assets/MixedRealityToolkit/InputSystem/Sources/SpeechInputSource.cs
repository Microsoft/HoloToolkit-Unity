﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Internal.Definitions.InputSystem;
using Microsoft.MixedReality.Toolkit.Internal.Utilities.Async;
using Microsoft.MixedReality.Toolkit.Internal.Utilities.Async.AwaitYieldInstructions;
using System;
using UnityEngine;

#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
using UnityEngine.Windows.Speech;
#endif

namespace Microsoft.MixedReality.Toolkit.InputSystem.Sources
{
    /// <summary>
    /// SpeechInputSource allows you to specify keywords for use in your application.
    /// This also includes a setting to either automatically start the
    /// keyword recognizer or allow your code to start it.
    /// <remarks>
    /// IMPORTANT:
    /// If you're targeting the UWP platform, please make sure to add the microphone capability in your app,
    /// in Unity under Edit -> Project Settings -> Player -> Settings for Windows Store -> Publishing Settings -> Capabilities
    /// or in your Visual Studio Package.appxmanifest capabilities.
    /// </remarks>
    /// </summary>
    public class SpeechInputSource : BaseGenericInputSource
    {
        /// <summary>
        /// This enumeration gives the manager two different ways to handle the recognizer. Both will
        /// set up the recognizer and add all keywords. The first causes the recognizer to start
        /// immediately. The second allows the recognizer to be manually started at a later time.
        /// </summary>
        private enum RecognizerStartBehavior { AutoStart, ManualStart }

        /// <summary>
        /// Constructor.
        /// </summary>
        public SpeechInputSource() : base("SpeechInput", new[] { new InteractionDefinition(1, AxisType.None, InputType.Voice) })
        {
            if (keywords.Length == 0) { return; }

            var newKeywords = new string[keywords.Length];

            for (int i = 0; i < keywords.Length; i++)
            {
                newKeywords[i] = keywords[i].Keyword;
            }

            Setup(newKeywords);

            if (recognizerStart == RecognizerStartBehavior.AutoStart)
            {
                StartKeywordRecognition();
            }

            Run();
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~SpeechInputSource()
        {
            if (keywordRecognizer == null) { return; }
            StopKeywordRecognition();
            Cleanup();
        }

        [SerializeField]
        [Tooltip("Whether the recognizer should be activated on start.")]
        private RecognizerStartBehavior recognizerStart = RecognizerStartBehavior.AutoStart;

        [SerializeField]
        [Tooltip("The keywords to be recognized and optional keyboard shortcuts.")]
        private KeywordAndKeyCode[] keywords = null;

        /// <summary>
        /// The keywords to be recognized and optional keyboard shortcuts.
        /// </summary>
        public KeywordAndKeyCode[] Keywords => keywords;

        private readonly WaitForUpdate waitForUpdate = new WaitForUpdate();

        private async void Run()
        {
            while (keywordRecognizer != null && keywordRecognizer.IsRunning)
            {
                for (int i = 0; i < keywords.Length; i++)
                {
                    if (Input.GetKeyDown(keywords[i].KeyCode))
                    {
                        RaiseKeywordAction(keywords[i].Keyword);
                    }
                }

                await waitForUpdate;
            }
        }

        #region Platform Routing

        private void Setup(string[] newKeywords)
        {
#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
            WindowsSetup(newKeywords);
#else
            throw new NotImplementedException();
#endif
        }

        /// <summary>
        /// Raises events based on keyboard input.
        /// </summary>
        /// <param name="keyword"></param>
        private void RaiseKeywordAction(string keyword)
        {
#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
            OnPhraseRecognized(recognitionConfidenceLevel, TimeSpan.Zero, DateTime.Now, null, keyword);
#else
            throw new NotImplementedException();
#endif
        }

        private void Cleanup()
        {
#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
            WindowsCleanup();
#else
            throw new NotImplementedException();
#endif
        }

        /// <summary>
        /// Make sure the keyword recognizer is off, then start it.
        /// Otherwise, leave it alone because it's already in the desired state.
        /// </summary>
        public void StartKeywordRecognition()
        {
#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
            WindowsStartRecognition();
#else
            throw new NotImplementedException();
#endif
        }

        /// <summary>
        /// Make sure the keyword recognizer is on, then stop it.
        /// Otherwise, leave it alone because it's already in the desired state.
        /// </summary>
        public void StopKeywordRecognition()
        {
#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
            WindowsStopRecognition();
#else
            throw new NotImplementedException();
#endif
        }

        #endregion Platform Routing

        #region UWP and Windows Standalone Implementations
#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN

        private KeywordRecognizer keywordRecognizer;

        [SerializeField]
        [Tooltip("The confidence level for the keyword recognizer.")]
        // NOTE: The serialized data of this field will be lost when switching between platforms and re-serializing this class.
        private ConfidenceLevel recognitionConfidenceLevel = ConfidenceLevel.Medium;

        private void WindowsSetup(string[] newKeywords)
        {
            keywordRecognizer = new KeywordRecognizer(newKeywords, recognitionConfidenceLevel);
            keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        }

        private void WindowsCleanup()
        {
            keywordRecognizer.OnPhraseRecognized -= KeywordRecognizer_OnPhraseRecognized;
            keywordRecognizer.Dispose();
        }

        private void WindowsStartRecognition()
        {
            if (keywordRecognizer != null && !keywordRecognizer.IsRunning)
            {
                keywordRecognizer.Start();
            }
        }

        private void WindowsStopRecognition()
        {
            if (keywordRecognizer != null && keywordRecognizer.IsRunning)
            {
                keywordRecognizer.Stop();
            }
        }

        private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            OnPhraseRecognized(args.confidence, args.phraseDuration, args.phraseStartTime, args.semanticMeanings, args.text);
        }

        private void OnPhraseRecognized(ConfidenceLevel confidence, TimeSpan phraseDuration, DateTime phraseStartTime, SemanticMeaning[] semanticMeanings, string text)
        {
            InputSystem.RaiseSpeechKeywordPhraseRecognized(this, confidence, phraseDuration, phraseStartTime, semanticMeanings, text);
        }

#endif // UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
        #endregion UWP and Windows Standalone Implementations
    }
}
