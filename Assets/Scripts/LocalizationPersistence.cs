using UnityEngine;
using Lean.Localization;

public class LeanLocalizationPersistence : MonoBehaviour
{
    private static LeanLocalizationPersistence instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("LeanLocalization will persist across scenes");
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
