using UnityEngine;

[CreateAssetMenu(fileName = "EnginePartData", menuName = "Engine/Part Data")]
public class EnginePartData : ScriptableObject
{
    [Header("Part Information")]
    public string partName;
    [TextArea(3, 10)]
    public string description;

    [Header("Audio")]
    public AudioClip explanationAudio;
}
