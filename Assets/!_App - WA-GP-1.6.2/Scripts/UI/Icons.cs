using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace WorkoutDrop.UI
{
    /// <summary>
    /// Glyph-based icon system replacing the web app's Ionicons.
    /// Each logical icon maps to a Unicode glyph rendered with the body font; missing
    /// glyphs fall back to the device system font (which also supplies emoji on Android).
    /// Centralised so the mapping can be swapped for a dedicated icon font later.
    /// </summary>
    public static class Icons
    {
        // Ionicons name -> Unicode glyph.
        private static readonly Dictionary<string, string> Map = new Dictionary<string, string>
        {
            { "barbell", "\U0001F3CB" },          // 🏋
            { "ellipse", "●" },              // ●
            { "flame", "\U0001F525" },            // 🔥
            { "construct", "\U0001F6E0" },        // 🛠
            { "arrow-forward", "→" },        // →
            { "arrow-down-circle", "⬇" },    // ⬇
            { "chevron-back", "‹" },         // ‹
            { "chevron-forward", "›" },      // ›
            { "leaf", "\U0001F343" },             // 🍃
            { "play", "▶" },                 // ▶
            { "play-skip-forward", "⏭" },    // ⏭
            { "add", "＋" },                  // ＋
            { "remove", "−" },               // −
            { "pencil", "✎" },               // ✎
            { "trash", "\U0001F5D1" },            // 🗑
            { "checkmark", "✔" },            // ✔
            { "checkmark-circle", "✅" },     // ✅
            { "close-circle", "❌" },         // ❌
            { "hourglass", "⏳" },            // ⏳
            { "ribbon", "\U0001F396" },           // 🎖
            { "trophy", "\U0001F3C6" },           // 🏆
            { "home", "\U0001F3E0" },             // 🏠
            { "stats-chart", "\U0001F4CA" },      // 📊
            { "settings", "⚙" },             // ⚙
            { "warning", "⚠" },              // ⚠
            { "moon", "\U0001F319" },             // 🌙
            { "time", "\U0001F552" },             // 🕒
            { "pulse", "♥" },                // ♥
            { "flash", "⚡" },                // ⚡
        };

        public static string Glyph(string name) => Map.TryGetValue(name, out var g) ? g : "•"; // • fallback

        /// <summary>Create a Label that renders the given icon glyph.</summary>
        public static Label Create(string name, float size, Color color)
        {
            var label = new Label(Glyph(name));
            label.AddToClassList("icon");
            label.style.fontSize = size;
            label.style.color = color;
            label.pickingMode = PickingMode.Ignore;
            return label;
        }
    }
}
