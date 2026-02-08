using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "NewQuizData", menuName = "Engine/Quiz Data")]
public class QuizData : ScriptableObject
{
    [Serializable]
    public struct Question
    {
        [Header("Localization Keys")]
        public string questionPhrase;
        public string audioPhrase; // Added for consistency
        public string[] optionPhrases; // 4 options usually

        [Header("Fallbacks")]
        [TextArea(2, 5)]
        public string fallbackQuestion;
        public string[] fallbackOptions;

        public int correctOptionIndex; // 0 to 3
    }

    public List<Question> questions;
}
