using UnityEngine;
using UnityEngine.UIElements;
using WorkoutDrop.Core;
using WorkoutDrop.Data;

namespace WorkoutDrop.UI.Components
{
    /// <summary>Achievement badge. Mirrors <c>src/components/BadgeView.tsx</c>.</summary>
    public static class BadgeBuilder
    {
        private struct Meta { public string Label; public Color Color; }

        private static Meta MetaFor(BadgeId id)
        {
            switch (id)
            {
                case BadgeId.FullDrop: return new Meta { Label = "Full Drop", Color = Palette.Gold };
                case BadgeId.BeastMode: return new Meta { Label = "Beast Mode", Color = Palette.Beast };
                case BadgeId.IronWeek: return new Meta { Label = "Iron Week", Color = Palette.PrimaryCyan };
                default: return new Meta { Label = "Personal Record", Color = Palette.Gold };
            }
        }

        public static VisualElement Build(AppConfig config, BadgeId id, bool earned, string size = "md")
        {
            float dim = size == "lg" ? 120 : size == "sm" ? 72 : 96;
            var meta = MetaFor(id);

            var wrap = new VisualElement();
            wrap.AddToClassList("badge");

            var circle = new VisualElement();
            circle.AddToClassList("badge__circle");
            circle.style.width = dim;
            circle.style.height = dim;
            circle.style.opacity = earned ? 1f : 0.35f;

            var sprite = config.BadgeSprite(id);
            if (sprite != null) circle.style.backgroundImage = new StyleBackground(sprite);

            var label = new Label(meta.Label.ToUpperInvariant());
            label.AddToClassList("badge__label");
            label.style.color = meta.Color;
            label.style.opacity = earned ? 1f : 0.5f;

            wrap.Add(circle);
            wrap.Add(label);
            return wrap;
        }
    }
}
