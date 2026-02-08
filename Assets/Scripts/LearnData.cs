using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "NewLearnData", menuName = "Engine/Learn Data")]
public class LearnData : ScriptableObject
{
    [Serializable]
    public struct LearnStep
    {
        [Header("Localization Keys")]
        public string titlePhrase;
        public string descriptionPhrase;
        public string audioPhrase;

        [Header("Animation")]
        [Tooltip("Name of the state in the Animator Controller to play for this step")]
        public string animationStateName;

        [Header("Fallbacks")]
        public string fallbackTitle;
        [TextArea(3, 10)]
        public string fallbackDescription;
    }

    public List<LearnStep> steps;
}
