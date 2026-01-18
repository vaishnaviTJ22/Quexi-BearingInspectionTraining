using UnityEngine;
using TMPro;
using Lean.Localization;
using TamilEncoder;

namespace TamilUI
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    [AddComponentMenu("Lean/Localization/Localized Tamil TextMeshProUGUI")]
    public class LeanLocalizedTamilTextMeshProUGUI : LeanLocalizedBehaviour
    {
        [Tooltip("If PhraseName couldn't be found, this text will be used")]
        public string FallbackText;

        [Header("Tamil/Hindi Font Settings")]
        [Tooltip("Font to use for Tamil language")]
        [SerializeField] private TMP_FontAsset tamilFont;

        [Tooltip("Font to use for Hindi language")]
        [SerializeField] private TMP_FontAsset hindiFont;

        [Tooltip("Default font for other languages")]
        [SerializeField] private TMP_FontAsset defaultFont;

        [Header("Tamil Encoding Settings")]
        [Tooltip("Enable Tamil encoding conversion (for Tamil language only)")]
        [SerializeField] private bool useTamilEncoding = true;

        [SerializeField] private TamilFontEncoding tamilEncoding = TamilFontEncoding.TSCII;

        [Header("Font Style Settings")]
        [Tooltip("Store original font style to restore when switching back from Tamil/Hindi")]
        private FontStyles originalFontStyle;
        private bool hasStoredOriginalStyle = false;

        private TextMeshProUGUI textMesh;

        public override void UpdateTranslation(LeanTranslation translation)
        {
            if (textMesh == null)
                textMesh = GetComponent<TextMeshProUGUI>();

            // FIX: Ensure defaultFont is captured from the current font if it hasn't been set yet.
            // This prevents the component from thinking the 'Tamil' font is the 'Default' font
            // if UpdateTranslation() runs before Awake() (which captures defaultFont).
            if (defaultFont == null)
            {
                defaultFont = textMesh.font;
            }

            if (!hasStoredOriginalStyle)
            {
                originalFontStyle = textMesh.fontStyle;
                hasStoredOriginalStyle = true;
            }

            string translatedText = "";

            if (translation != null && translation.Data is string)
            {
                translatedText = LeanTranslation.FormatText((string)translation.Data, textMesh.text, this, gameObject);
            }
            else
            {
                translatedText = LeanTranslation.FormatText(FallbackText, textMesh.text, this, gameObject);
            }

            string currentLanguageCode = LeanLocalization.GetFirstCurrentLanguage();

            if (currentLanguageCode == "Tamil" && useTamilEncoding && !string.IsNullOrEmpty(translatedText))
            {
                textMesh.text = TamilEncoding.ConvertFromUnicode(translatedText, tamilEncoding);
                if (tamilFont != null)
                    textMesh.font = tamilFont;

                textMesh.fontStyle = RemoveUppercaseStyle(originalFontStyle);
            }
            else if (currentLanguageCode == "Hindi")
            {
                textMesh.text = translatedText;
                if (hindiFont != null)
                    textMesh.font = hindiFont;

                textMesh.fontStyle = RemoveUppercaseStyle(originalFontStyle);
            }
            else
            {
                textMesh.text = translatedText;
                // FIX: Always restore the default font for other languages (like English)
                if (defaultFont != null)
                    textMesh.font = defaultFont;

                textMesh.fontStyle = originalFontStyle;
            }
        }

        private FontStyles RemoveUppercaseStyle(FontStyles style)
        {
            return style & ~FontStyles.UpperCase;
        }

        protected virtual void Awake()
        {
            textMesh = GetComponent<TextMeshProUGUI>();

            if (string.IsNullOrEmpty(FallbackText))
            {
                FallbackText = textMesh.text;
            }

            if (defaultFont == null)
            {
                defaultFont = textMesh.font;
            }

            originalFontStyle = textMesh.fontStyle;
            hasStoredOriginalStyle = true;
        }
    }
}
