﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace HoloToolkit.Unity.InputModule
{
    /// <summary>
    /// Event args for a phrase recognized event.
    /// </summary>
    public class PhraseRecognizedEventArgs : InputSourceEventArgs
    {
        /// <summary>
        /// A measure of correct recognition certainty.
        /// </summary>
        public UnityEngine.Windows.Speech.ConfidenceLevel Confidence { get; private set; }

        /// <summary>
        /// The time it took for the phrase to be uttered.
        /// </summary>
        public TimeSpan PhraseDuration { get; private set; }

        /// <summary>
        /// The moment in time when uttering of the phrase began.
        /// </summary>
        public DateTime PhraseStartTime { get; private set; }

        /// <summary>
        /// A semantic meaning of recognized phrase.
        /// </summary>
        public UnityEngine.Windows.Speech.SemanticMeaning[] SemanticMeanings { get; private set; }

        /// <summary>
        /// The text that was recognized.
        /// </summary>
        public string RecognizedText { get; private set; }

        public PhraseRecognizedEventArgs(IInputSource inputSource, uint sourceId, UnityEngine.Windows.Speech.ConfidenceLevel confidence, TimeSpan phraseDuration, DateTime phraseStartTime, UnityEngine.Windows.Speech.SemanticMeaning[] semanticMeanings, string recognizedText) : base(inputSource, sourceId)
        {
            Confidence = confidence;
            PhraseDuration = phraseDuration;
            PhraseStartTime = phraseStartTime;
            SemanticMeanings = semanticMeanings;
            RecognizedText = recognizedText;
        }
    }
}