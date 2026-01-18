using UnityEngine;
using Lean.Localization;

public class LanguagePersistenceManager : MonoBehaviour
{
    private static LanguagePersistenceManager instance;
    private static string currentLanguage = "English";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("LanguagePersistenceManager initialized");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        ApplyStoredLanguage();
    }

    private void OnEnable()
    {
        LeanLocalization.OnLocalizationChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        LeanLocalization.OnLocalizationChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        string newLanguage = LeanLocalization.GetFirstCurrentLanguage();

        if (!string.IsNullOrEmpty(newLanguage))
        {
            currentLanguage = newLanguage;
            Debug.Log($"Language changed to: {currentLanguage}");
        }
    }

    private void ApplyStoredLanguage()
    {
        if (LeanLocalization.Instances != null && LeanLocalization.Instances.Count > 0)
        {
            LeanLocalization localization = LeanLocalization.Instances[0];
            localization.SetCurrentLanguage(currentLanguage);
            Debug.Log($"Applied language: {currentLanguage}");
        }
        else
        {
            Invoke(nameof(ApplyStoredLanguage), 0.1f);
        }
    }

    public static void SetLanguage(string languageName)
    {
        currentLanguage = languageName;

        if (LeanLocalization.Instances != null && LeanLocalization.Instances.Count > 0)
        {
            LeanLocalization.Instances[0].SetCurrentLanguage(languageName);
        }
    }

    public static string GetCurrentLanguage()
    {
        return currentLanguage;
    }
}
