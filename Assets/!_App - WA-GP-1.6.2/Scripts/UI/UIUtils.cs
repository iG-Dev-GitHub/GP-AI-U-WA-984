using System;
using UnityEngine;
using UnityEngine.UIElements;
using WorkoutDrop.Core;

namespace WorkoutDrop.UI
{
    /// <summary>Shared UI Toolkit helpers: scrollbar hiding, button factory, stat tiles, etc.</summary>
    public static class UIUtils
    {
        /// <summary>
        /// Hide both scrollbars while keeping scrolling fully functional (touch drag,
        /// mouse wheel, pointer drag). Requirement #3.
        /// </summary>
        public static ScrollView HideScrollbars(this ScrollView sv)
        {
            sv.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            sv.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            sv.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped; // enables touch/drag scroll
            return sv;
        }

        /// <summary>Create a vertical scroll content host with hidden scrollbars.</summary>
        public static ScrollView VerticalScroll()
        {
            var sv = new ScrollView(ScrollViewMode.Vertical);
            sv.HideScrollbars();
            sv.AddToClassList("scroll");
            return sv;
        }

        public static T WithClass<T>(this T el, params string[] classes) where T : VisualElement
        {
            foreach (var c in classes)
                if (!string.IsNullOrEmpty(c)) el.AddToClassList(c);
            return el;
        }

        public static T OnClick<T>(this T el, Action cb) where T : VisualElement
        {
            el.RegisterCallback<ClickEvent>(_ => cb?.Invoke());
            return el;
        }

        // ---------- Tactile button (mirrors src/components/TactileButton.tsx) ----------
        public enum ButtonVariant { Primary, Beast, Secondary, Ghost }

        public static Button TactileButton(string title, string iconName, ButtonVariant variant, Action onClick)
        {
            var btn = new Button();
            btn.AddToClassList("tactile-btn");
            btn.AddToClassList(VariantClass(variant));
            btn.text = string.Empty;

            var row = new VisualElement();
            row.AddToClassList("tactile-btn__row");

            Color textColor = variant == ButtonVariant.Primary ? Palette.Black : Palette.White;
            if (!string.IsNullOrEmpty(iconName))
            {
                var icon = Icons.Create(iconName, 20, textColor);
                icon.AddToClassList("tactile-btn__icon");
                row.Add(icon);
            }

            var label = new Label(title.ToUpperInvariant());
            label.AddToClassList("tactile-btn__label");
            label.style.color = textColor;
            row.Add(label);

            btn.Add(row);
            if (onClick != null) btn.clicked += onClick;
            return btn;
        }

        public static void SetDisabled(this Button btn, bool disabled)
        {
            btn.SetEnabled(!disabled);
            btn.style.opacity = disabled ? 0.5f : 1f;
        }

        private static string VariantClass(ButtonVariant v)
        {
            switch (v)
            {
                case ButtonVariant.Primary: return "tactile-btn--primary";
                case ButtonVariant.Beast: return "tactile-btn--beast";
                case ButtonVariant.Ghost: return "tactile-btn--ghost";
                default: return "tactile-btn--secondary";
            }
        }

        // ---------- Stat tile (mirrors src/components/StatTile.tsx) ----------
        public static VisualElement StatTile(string title, string value, Color accent)
        {
            var tile = new VisualElement();
            tile.AddToClassList("stat-tile");
            tile.style.borderTopColor = accent;
            tile.style.borderBottomColor = accent;
            tile.style.borderLeftColor = accent;
            tile.style.borderRightColor = accent;

            var v = new Label(value);
            v.AddToClassList("stat-tile__value");
            v.style.color = accent;

            var t = new Label(title.ToUpperInvariant());
            t.AddToClassList("stat-tile__title");

            tile.Add(v);
            tile.Add(t);
            return tile;
        }

        public static void SetBorderColor(this VisualElement el, Color c)
        {
            el.style.borderTopColor = c;
            el.style.borderBottomColor = c;
            el.style.borderLeftColor = c;
            el.style.borderRightColor = c;
        }

        /// <summary>
        /// Set a TextField placeholder without a hard compile-time dependency on a specific
        /// Unity version's API (the property moved across versions). Reflection makes it a no-op
        /// if unavailable, so the project always compiles.
        /// </summary>
        public static void SetPlaceholderText(this TextField field, string text)
        {
            if (field == null) return;
            var prop = typeof(TextField).GetProperty("textEdition");
            var edition = prop?.GetValue(field);
            edition?.GetType().GetProperty("placeholder")?.SetValue(edition, text);
        }

        public static string FormatWeight(float weight, WeightUnit unit)
        {
            if (weight == 0) return "BW";
            int v = unit == WeightUnit.Lbs ? Mathf.RoundToInt(weight * 2.20462f) : Mathf.RoundToInt(weight);
            return $"{v} {(unit == WeightUnit.Lbs ? "lbs" : "kg")}";
        }

        public static string FormatClock(int totalSeconds)
        {
            if (totalSeconds < 0) totalSeconds = 0;
            int mm = totalSeconds / 60;
            int ss = totalSeconds % 60;
            return $"{mm:00}:{ss:00}";
        }
    }
}
