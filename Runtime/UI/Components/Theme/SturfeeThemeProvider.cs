using System.Collections;
using System.IO;
using System.Linq;
using SturfeeVPS.Core;
using SturfeeVPS.SDK;
using UnityEngine;

namespace SturfeeVPS.UI
{
    [DefaultExecutionOrder(-100)] // run before the others
    public class SturfeeThemeProvider : SimpleSingleton<SturfeeThemeProvider>
    {
        public ThemeAsset Theme;

        private void Awake()
        {
            Theme = LoadTheme();
            if (Theme == null) { Theme = Resources.Load<ThemeAsset>("Sturfee/Themes/SturfeeTheme"); }
            Debug.Log($"Locale Found: {Theme.Locale}");

            var initLocale = SturfeeLocalizationProvider.Instance;
            initLocale.Init(Theme);
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1);            

            Debug.Log($"Applying Theme...");
            ApplyTheme();
            yield return new WaitForSeconds(1);

            Debug.Log($"Applying Theme...");
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in roots)
            {
                ApplyStyles(root);
            }            
        }

        public void ApplyStyles(GameObject root)
        {
            var disabledColor = new Color(Theme.BackgroundColor.r, Theme.BackgroundColor.g, Theme.BackgroundColor.b, 0.5f);

            UIStyle[] styles = root.GetComponentsInChildren<UIStyle>(true).ToArray();
            for (int i = 0; i < styles.Length; ++i)
            {
                UIStyle style = styles[i];
                switch (style.Name)
                {
                    // IMAGES

                    case "PrimaryColor":
                        style.ApplyImageColor(Theme.PrimaryColor);
                        break;
                    case "SecondaryColor":
                        style.ApplyImageColor(Theme.SecondaryColor);
                        break;
                    case "PrimaryColorHighlight":
                        style.ApplyImageColor(Theme.PrimaryColorHighlight);
                        break;
                    case "SecondaryColorHighlight":
                        style.ApplyImageColor(Theme.SecondaryColorHighlight);
                        break;
                    case "ForegroundColor":
                        style.ApplyImageColor(Theme.ForegroundColor);
                        break;
                    case "BackgroundColor":
                        style.ApplyImageColor(Theme.BackgroundColor);
                        break;


                    // Selectables (buttons, toggles, etc)
                    case "ButtonColor":
                        style.ApplySelectableColor(Theme.BackgroundColor, Theme.PrimaryColorHighlight, Theme.ForegroundColor, disabledColor, Theme.PrimaryColorHighlight);
                        break;
                    case "ButtonColorAltSecondary":
                        style.ApplySelectableColor(Theme.BackgroundColor, Theme.SecondaryColorHighlight, Theme.ForegroundColor, disabledColor, Theme.SecondaryColorHighlight);
                        break;
                    case "PrimaryButtonColor":
                        style.ApplySelectableColor(Theme.PrimaryColor, Theme.PrimaryColorHighlight, Theme.PrimaryColor, disabledColor, Theme.PrimaryColor);
                        break;
                    case "SecondaryButtonColor":
                        style.ApplySelectableColor(Theme.SecondaryColor, Theme.SecondaryColorHighlight, Theme.SecondaryColor, disabledColor, Theme.SecondaryColor);
                        break;

                }
            }
        }

        private ThemeAsset LoadTheme()
        {
            var config = SturfeeWindow.Config;
            if (config != null && !string.IsNullOrEmpty(config.Theme))
            {
                return Resources.Load<ThemeAsset>(config.Theme);
            }

            SturfeeDebug.LogError(" Cannot load editor config");
            return null;
        }
    }
}

