using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StepByStepController : MonoBehaviour
{
    [System.Serializable]
    public class StepData
    {
        public string stepDescription;
        public AnimationClip animationClip;
        public AudioClip audioClip;
    }

    [Header("Steps Data")]
    public StepData[] steps;

    [Header("References")]
    public Animator animator;
    public AudioSource audioSource;
    public TextMeshProUGUI descriptionText;

    private int currentStepIndex = 0;

    void Start()
    {
        LoadStep(0);
    }

    public void NextStep()
    {
        if (currentStepIndex < steps.Length - 1)
        {
            currentStepIndex++;
            LoadStep(currentStepIndex);
        }
    }

    public void PreviousStep()
    {
        if (currentStepIndex > 0)
        {
            currentStepIndex--;
            LoadStep(currentStepIndex);
        }
    }

    void LoadStep(int index)
    {
        // Update Text
        descriptionText.text = steps[index].stepDescription;

        // Play Animation
        if (steps[index].animationClip != null)
        {
            animator.Play(steps[index].animationClip.name);
        }

        // Play Audio
        if (steps[index].audioClip != null)
        {
            audioSource.Stop();
            audioSource.clip = steps[index].audioClip;
            audioSource.Play();
        }
    }
}
