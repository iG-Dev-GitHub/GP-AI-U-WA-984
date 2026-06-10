namespace WorkoutDrop.Core
{
    /// <summary>
    /// Exercise / program categories. Mirrors the web app's ExerciseCategory &amp; ProgramType
    /// unions ("recovery" | "cardio" | "strength" | "beast").
    /// </summary>
    public enum ProgramType
    {
        Recovery = 0,
        Cardio = 1,
        Strength = 2,
        Beast = 3,
    }

    /// <summary>Plinko risk profile. Mirrors RiskLevel ("easy" | "beast").</summary>
    public enum RiskLevel
    {
        Easy = 0,
        Beast = 1,
    }

    /// <summary>Display unit for weights. Mirrors WeightUnit ("kg" | "lbs").</summary>
    public enum WeightUnit
    {
        Kg = 0,
        Lbs = 1,
    }

    /// <summary>Achievement identifiers. Mirrors BadgeId.</summary>
    public enum BadgeId
    {
        FullDrop = 0,
        BeastMode = 1,
        IronWeek = 2,
        Pr = 3,
    }

    /// <summary>
    /// String-id helpers so persisted JSON and ScriptableObject data stay human readable and
    /// stable across versions, exactly like the web app's lowercase string unions.
    /// </summary>
    public static class EnumIds
    {
        public static string ToId(this ProgramType t)
        {
            switch (t)
            {
                case ProgramType.Recovery: return "recovery";
                case ProgramType.Cardio: return "cardio";
                case ProgramType.Strength: return "strength";
                default: return "beast";
            }
        }

        public static ProgramType ToProgramType(string id)
        {
            switch (id)
            {
                case "recovery": return ProgramType.Recovery;
                case "cardio": return ProgramType.Cardio;
                case "strength": return ProgramType.Strength;
                default: return ProgramType.Beast;
            }
        }

        public static string ToId(this RiskLevel r) => r == RiskLevel.Beast ? "beast" : "easy";

        public static RiskLevel ToRiskLevel(string id) => id == "beast" ? RiskLevel.Beast : RiskLevel.Easy;

        public static string ToId(this WeightUnit u) => u == WeightUnit.Lbs ? "lbs" : "kg";

        public static WeightUnit ToWeightUnit(string id) => id == "lbs" ? WeightUnit.Lbs : WeightUnit.Kg;

        public static string ToId(this BadgeId b)
        {
            switch (b)
            {
                case BadgeId.FullDrop: return "full_drop";
                case BadgeId.BeastMode: return "beast_mode";
                case BadgeId.IronWeek: return "iron_week";
                default: return "pr";
            }
        }

        public static BadgeId ToBadgeId(string id)
        {
            switch (id)
            {
                case "full_drop": return BadgeId.FullDrop;
                case "beast_mode": return BadgeId.BeastMode;
                case "iron_week": return BadgeId.IronWeek;
                default: return BadgeId.Pr;
            }
        }
    }
}
