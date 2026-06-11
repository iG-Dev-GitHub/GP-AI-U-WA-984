using UnityEngine;
using UnityEngine.UIElements;
using WorkoutDrop.Core;

namespace WorkoutDrop.Data
{
    /// <summary>Logical screens of the app (used to look up the right UXML).</summary>
    public enum ScreenId
    {
        Welcome,
        Home,
        Drop,
        Workout,
        Summary,
        Progress,
        Settings,
        Exercises,
    }

    /// <summary>
    /// Single strongly-typed entry point for every asset the app needs: UI documents,
    /// stylesheet, font and sprites, plus the data ScriptableObjects. The scene wires
    /// exactly ONE reference (this asset) into <c>AppBootstrap</c>.
    ///
    /// This is the architecture that replaces Resources.Load: nothing is fetched by name
    /// or path at runtime, so the build is safe to obfuscate and rename assets freely.
    /// </summary>
    [CreateAssetMenu(menuName = "WorkoutDrop/App Config", fileName = "AppConfig")]
    public class AppConfig : ScriptableObject
    {
        [Header("Data")]
        public ExerciseDatabase exercises;
        public PlinkoConfig plinko;
        public ProgramConfig programs;

        [Header("UI Toolkit")]
        public StyleSheet globalStyle;
        [Tooltip("Body font (TrueType). Falls back to the device system font for missing glyphs / emoji.")]
        public Font bodyFont;
        [Tooltip("Panel text settings supplying the bundled emoji/icon font (NotoEmoji SDF) for all icon glyphs.")]
        public PanelTextSettings panelTextSettings;

        [Header("Screens (UXML)")]
        public VisualTreeAsset welcomeScreen;
        public VisualTreeAsset homeScreen;
        public VisualTreeAsset dropScreen;
        public VisualTreeAsset workoutScreen;
        public VisualTreeAsset summaryScreen;
        public VisualTreeAsset progressScreen;
        public VisualTreeAsset settingsScreen;
        public VisualTreeAsset exercisesScreen;

        [Header("Sprites")]
        public Sprite kettlebell;
        public Sprite kettlebellFire;
        public Sprite badgeFullDrop;
        public Sprite badgeBeastMode;
        public Sprite badgeIronWeek;
        public Sprite badgePr;

        public VisualTreeAsset ScreenUxml(ScreenId id)
        {
            switch (id)
            {
                case ScreenId.Welcome: return welcomeScreen;
                case ScreenId.Home: return homeScreen;
                case ScreenId.Drop: return dropScreen;
                case ScreenId.Workout: return workoutScreen;
                case ScreenId.Summary: return summaryScreen;
                case ScreenId.Progress: return progressScreen;
                case ScreenId.Settings: return settingsScreen;
                case ScreenId.Exercises: return exercisesScreen;
                default: return null;
            }
        }

        public Sprite Kettlebell(bool onFire) => onFire ? kettlebellFire : kettlebell;

        public Sprite BadgeSprite(BadgeId id)
        {
            switch (id)
            {
                case BadgeId.FullDrop: return badgeFullDrop;
                case BadgeId.BeastMode: return badgeBeastMode;
                case BadgeId.IronWeek: return badgeIronWeek;
                default: return badgePr;
            }
        }

        /// <summary>Make sure all referenced data assets have their canonical defaults loaded.</summary>
        public void EnsureData()
        {
            if (exercises != null) exercises.EnsurePopulated();
            if (plinko != null) plinko.EnsurePopulated();
            if (programs != null) programs.EnsurePopulated();
        }
    }
}
