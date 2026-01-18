using UnityEngine;

[CreateAssetMenu(fileName = "EnginePartData", menuName = "Engine/Part Data")]
public class EnginePartData : ScriptableObject
{
    [Header("LeanLocalization Phrase Names")]
    [Tooltip("Phrase name for the part title (e.g., 'Part_Balls_Title')")]
    public string partName;

    [Tooltip("Phrase name for the part description (e.g., 'Part_Balls_Description')")]
    public string partDescription;

    [Header("Audio Localization")]
    [Tooltip("Phrase name for audio clip (e.g., 'Part_Balls_Audio')")]
    public string audioPhrase;

    [Header("Fallback (if phrases not found)")]
    public string fallbackName = "Unknown Part";
    [TextArea(3, 10)]
    public string fallbackDescription = "No description available.";
}
