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
        
        [TextArea(3, 10)]
        [Tooltip("Description of what to look for")]
        public string stepDescription;
    }

    public List<InspectionStep> steps;
}
