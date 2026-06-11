using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TextCore.Text;
using WorkoutDrop.Core;
using WorkoutDrop.Data;

namespace WorkoutDrop.UI
{
    /// <summary>
    /// Single MonoBehaviour placed in the scene. It owns the runtime UI: it creates the
    /// UIDocument + PanelSettings + theme entirely in code (so no PanelSettings asset has to be
    /// wired by hand), builds the root layout, applies the global stylesheet, font and safe-area
    /// insets, then hands control to the <see cref="Router"/>.
    ///
    /// The ONLY serialized reference is <see cref="_config"/> — a strongly typed AppConfig.
    /// No Resources.Load, no asset path strings: obfuscation-safe by construction.
    /// </summary>
    [DisallowMultipleComponent]
    public class AppBootstrap : MonoBehaviour
    {
        [SerializeField, Tooltip("The single aggregate asset that references every UXML/USS/font/sprite/data SO.")]
        private AppConfig _config;

        private UIDocument _document;
        private PanelSettings _panel;
        private VisualElement _safeArea;
        private Rect _lastSafeArea;
        private ScreenOrientation _lastOrientation;
        private Vector2Int _lastScreen;

        private void Start()
        {
            if (_config == null)
            {
                Debug.LogError("[WorkoutDrop] AppConfig is not assigned on AppBootstrap.");
                return;
            }

            _config.EnsureData();

            CreatePanel();
            BuildRoot();

            var store = new Store(_config.exercises);
            var router = new Router(Q("content-host"), Q("tab-bar"));
            var ctx = new AppContext(_config, store, UnityRng.Shared, router, this);
            router.SetContext(ctx);
            router.Start();
        }

        private VisualElement _root;
        private VisualElement Q(string name) => _root.Q(name);

        private void CreatePanel()
        {
            // Runtime theme — an empty ThemeStyleSheet is enough; all styling comes from our USS.
            var theme = ScriptableObject.CreateInstance<ThemeStyleSheet>();
            theme.name = "WorkoutDropRuntimeTheme";

            _panel = ScriptableObject.CreateInstance<PanelSettings>();
            _panel.name = "WorkoutDropPanelSettings";
            _panel.themeStyleSheet = theme;
            if (_config.panelTextSettings != null) _panel.textSettings = _config.panelTextSettings;
            _panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            _panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            _panel.referenceResolution = new Vector2Int(390, 844); // portrait phone baseline
            _panel.match = 0f;                                       // scale by width → consistent proportions
            _panel.clearColor = true;
            _panel.colorClearValue = Palette.Bg;                     // fills background, no camera needed

            _document = GetComponent<UIDocument>();
            if (_document == null) _document = gameObject.AddComponent<UIDocument>();
            _document.panelSettings = _panel;
        }

        private void BuildRoot()
        {
            _root = _document.rootVisualElement;
            _root.Clear();
            _root.AddToClassList("app-root");
            _root.style.flexGrow = 1;
            _root.style.backgroundColor = Palette.Bg;

            if (_config.globalStyle != null && !_root.styleSheets.Contains(_config.globalStyle))
                _root.styleSheets.Add(_config.globalStyle);

            ApplyFonts();

            _safeArea = new VisualElement { name = "safe-area" };
            _safeArea.AddToClassList("safe-area");
            _safeArea.style.flexGrow = 1;

            var contentHost = new VisualElement { name = "content-host" };
            contentHost.AddToClassList("content-host");
            contentHost.style.flexGrow = 1;

            var tabBar = new VisualElement { name = "tab-bar" };
            tabBar.AddToClassList("tab-bar");
            tabBar.style.display = DisplayStyle.None;

            _safeArea.Add(contentHost);
            _safeArea.Add(tabBar);
            _root.Add(_safeArea);

            ApplySafeArea(true);
        }

        /// <summary>
        /// Build a TextCore <see cref="FontAsset"/> from the bundled TrueType font and set it as
        /// the inherited root font. This is REQUIRED for correct line-height measurement: a raw
        /// legacy <see cref="Font"/> reports its height from the import point size, which makes
        /// large text under-measure and stacked elements overlap. OS fallbacks keep symbol/emoji
        /// icon glyphs resolving. Falls back to the legacy font only if SDF creation fails.
        /// </summary>
        private void ApplyFonts()
        {
            if (_config.bodyFont == null) return;

            FontAsset body = null;
            try { body = FontAsset.CreateFontAsset(_config.bodyFont); }
            catch (Exception e) { Debug.LogWarning($"[WorkoutDrop] FontAsset.CreateFontAsset failed: {e.Message}"); }

            if (body == null)
            {
                // Last resort — keep rendering even if SDF generation is unavailable.
                _root.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(_config.bodyFont));
                return;
            }

            AddBundledFallbacks(body);
            AddOsFallbacks(body);
            _root.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromSDFFont(body));
        }

        /// <summary>
        /// Put the fonts referenced by the Panel Text Settings asset (bundled NotoEmoji SDF) at
        /// the FRONT of the fallback chain so every emoji/icon glyph resolves to the bundled font
        /// on all platforms, never to whatever emoji font the OS happens to ship.
        /// </summary>
        private void AddBundledFallbacks(FontAsset body)
        {
            var settings = _config.panelTextSettings;
            if (settings == null || settings.fallbackFontAssets == null) return;
            if (body.fallbackFontAssetTable == null) body.fallbackFontAssetTable = new List<FontAsset>();
            foreach (var fa in settings.fallbackFontAssets)
                if (fa != null && !body.fallbackFontAssetTable.Contains(fa))
                    body.fallbackFontAssetTable.Add(fa);
        }

        // Candidate OS fonts that cover the symbol/emoji glyphs used by the icon system.
        // Names that don't exist on the running platform simply return null and are skipped.
        private static readonly (string family, string style)[] OsFallbackFonts =
        {
            ("Segoe UI Emoji", "Regular"),
            ("Segoe UI Symbol", "Regular"),
            ("Arial Unicode MS", "Regular"),
            ("Noto Color Emoji", "Regular"),
            ("Noto Sans Symbols", "Regular"),
            ("Arial", "Regular"),
        };

        private static void AddOsFallbacks(FontAsset body)
        {
            if (body.fallbackFontAssetTable == null) body.fallbackFontAssetTable = new List<FontAsset>();
            foreach (var (family, style) in OsFallbackFonts)
            {
                try
                {
                    var fa = FontAsset.CreateFontAsset(family, style, 90);
                    if (fa != null) body.fallbackFontAssetTable.Add(fa);
                }
                catch { /* font not present on this platform — ignore */ }
            }
        }

        private void Update()
        {
            // Re-apply safe area when the device rotates / the safe area changes.
            if (_safeArea == null) return;
            if (Screen.safeArea != _lastSafeArea ||
                Screen.orientation != _lastOrientation ||
                _lastScreen.x != Screen.width || _lastScreen.y != Screen.height)
            {
                ApplySafeArea(false);
            }
        }

        private void ApplySafeArea(bool force)
        {
            _lastSafeArea = Screen.safeArea;
            _lastOrientation = Screen.orientation;
            _lastScreen = new Vector2Int(Screen.width, Screen.height);

            float sw = Mathf.Max(1, Screen.width);
            float sh = Mathf.Max(1, Screen.height);
            Rect sa = Screen.safeArea;

            // Panel size in panel-space px (after PanelSettings scaling).
            float panelW = _root.resolvedStyle.width;
            float panelH = _root.resolvedStyle.height;
            if (panelW <= 0 || panelH <= 0)
            {
                // Layout not resolved yet — retry next frame.
                _root.schedule.Execute(() => ApplySafeArea(false)).StartingIn(16);
                return;
            }

            float left = sa.xMin / sw * panelW;
            float right = (sw - sa.xMax) / sw * panelW;
            float top = (sh - sa.yMax) / sh * panelH;   // safeArea origin is bottom-left
            float bottom = sa.yMin / sh * panelH;

            _safeArea.style.paddingLeft = left;
            _safeArea.style.paddingRight = right;
            _safeArea.style.paddingTop = top;
            _safeArea.style.paddingBottom = bottom;
        }
    }
}
