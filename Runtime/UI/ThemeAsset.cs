using System.Collections.Generic;
using UnityEngine;

namespace SturfeeVPS.UI
{
    [CreateAssetMenu(menuName = "Sturfee/Theme Asset")]
    public class ThemeAsset : ScriptableObject
    {
        public Color PrimaryColor;
        public Color PrimaryColorHighlight;
        public Color SecondaryColor;
        public Color SecondaryColorHighlight;

        public Color ForegroundColor;
        public Color BackgroundColor;

        [Tooltip("The locale code to use for Localization (see 'Sturfee.StringResources.en-US' file)")]
        public string Locale = "en-US";
        public List<string> StringResources = null;

        [ContextMenu("Reset To Default")]
        private void Reset()
        {
            PrimaryColor = new Color(0.09803922f, 0.7490196f, 0.7843137f, 1);
            SecondaryColor = new Color(0.9098039f, 0.3647059f, 0.4588235f, 1);
            ForegroundColor = new Color(0.1f, 0.1f, 0.1f, 1);
            BackgroundColor = new Color(0.8901961f, 0.9411765f, 1, 1);
            Locale = "en-US";
            StringResources = new List<string> { "Strings/Sturfee.StringResources" };

            CreateHighlights();
        }

        [ContextMenu("Create Highlights")]
        private void CreateHighlights()
        {
            var hsbPrimary = HSBColor.FromColor(PrimaryColor);
            hsbPrimary.b *= 1.2f;
            hsbPrimary.s *= 0.8f;
            PrimaryColorHighlight = hsbPrimary.ToColor();

            var hsbSecondary = HSBColor.FromColor(SecondaryColor);
            hsbSecondary.b *= 1.2f;
            hsbSecondary.s *= 0.8f;
            SecondaryColorHighlight = hsbSecondary.ToColor();
        }
    }
}
