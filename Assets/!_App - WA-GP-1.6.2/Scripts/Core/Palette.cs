using UnityEngine;

namespace WorkoutDrop.Core
{
    /// <summary>
    /// Central colour palette mirroring the web app's hard-coded hex values
    /// (CELL_COLORS, accents, surfaces, text). Used where colours must be applied
    /// inline from C# (Plinko board, category dots, dynamic borders). Static USS
    /// classes cover the rest.
    /// </summary>
    public static class Palette
    {
        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex.StartsWith("#") ? hex : "#" + hex, out var c);
            return c;
        }

        // Category colours (CELL_COLORS)
        public static readonly Color Recovery = Hex("#007AFF");
        public static readonly Color Cardio = Hex("#34C759");
        public static readonly Color Strength = Hex("#FFCC00");
        public static readonly Color Beast = Hex("#FF3B30");

        // Category colours, darkened (CELL_COLORS_DARK)
        public static readonly Color RecoveryDark = Hex("#003B7A");
        public static readonly Color CardioDark = Hex("#1E6B30");
        public static readonly Color StrengthDark = Hex("#8A6E00");
        public static readonly Color BeastDark = Hex("#8A1A14");

        // Accents
        public static readonly Color PrimaryCyan = Hex("#00D1FF");
        public static readonly Color NeonGreen = Hex("#00FF7A");
        public static readonly Color Gold = Hex("#FFD700");

        // Pegs
        public static readonly Color PegGreen = Hex("#00FF7A");
        public static readonly Color PegBlue = Hex("#00D1FF");
        public static readonly Color PegBeast = Hex("#FF6B5C");

        // Surfaces / text
        public static readonly Color Bg = Hex("#050505");
        public static readonly Color White = Hex("#FFFFFF");
        public static readonly Color Black = Hex("#000000");

        public static Color CategoryColor(ProgramType t)
        {
            switch (t)
            {
                case ProgramType.Recovery: return Recovery;
                case ProgramType.Cardio: return Cardio;
                case ProgramType.Strength: return Strength;
                default: return Beast;
            }
        }

        public static Color CategoryColorDark(ProgramType t)
        {
            switch (t)
            {
                case ProgramType.Recovery: return RecoveryDark;
                case ProgramType.Cardio: return CardioDark;
                case ProgramType.Strength: return StrengthDark;
                default: return BeastDark;
            }
        }
    }
}
