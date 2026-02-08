using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "NewInspectionData", menuName = "Engine/Inspection Data")]
public class InspectionData : ScriptableObject
{
    [Serializable]
    public struct InspectionStep
    {
        [Tooltip("Image showing the defect or check to perform")]
        public Sprite stepImage;
        
        [Header("Localization Keys")]
        public string descriptionPhrase;
        //public string audioPhrase; // Added for consistency with Learn mode

        [Header("Fallbacks")]
        [TextArea(3, 10)]
        public string fallbackDescription;
    }

    public List<InspectionStep> steps;
}
