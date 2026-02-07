using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "NewQuizData", menuName = "Engine/Quiz Data")]
public class QuizData : ScriptableObject
{
    [Serializable]
    public struct Question
    {
        [TextArea(2, 5)]
        public string questionText;
        public string[] options; // Should be 4 options usually
        public int correctOptionIndex; // 0 to 3
    }

    public List<Question> questions;
}
