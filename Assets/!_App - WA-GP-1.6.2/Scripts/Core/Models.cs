using System;
using System.Collections.Generic;

namespace WorkoutDrop.Core
{
    /// <summary>
    /// A library exercise definition. Mirrors the web <c>Exercise</c> interface.
    /// <see cref="defaultDurationSec"/> of 0 means the exercise is rep-based (web used an
    /// optional field; here 0 == "no duration", matching every truthy check in the web app).
    /// </summary>
    [Serializable]
    public class ExerciseDef
    {
        public string id;
        public string name;
        public ProgramType category;
        public int defaultReps;
        public float defaultWeight; // kg
        public int defaultDurationSec; // 0 == rep based

        public ExerciseDef() { }

        public ExerciseDef(string id, string name, ProgramType category, int reps, float weight, int durationSec = 0)
        {
            this.id = id;
            this.name = name;
            this.category = category;
            defaultReps = reps;
            defaultWeight = weight;
            defaultDurationSec = durationSec;
        }

        public ExerciseDef Clone() => new ExerciseDef(id, name, category, defaultReps, defaultWeight, defaultDurationSec);
    }

    /// <summary>One logged set inside a workout. Mirrors web <c>WorkoutSet</c>.</summary>
    [Serializable]
    public class WorkoutSet
    {
        public int reps;
        public float weight; // kg
        public int durationSec; // 0 == rep based
        public bool completed;
        public bool skipped; // web encoded this as reps == -1; modelled explicitly here
    }

    /// <summary>An exercise as it appears within a workout, with its sets. Mirrors web <c>WorkoutExercise</c>.</summary>
    [Serializable]
    public class WorkoutExercise
    {
        public string exerciseId;
        public string name;
        public ProgramType category;
        public List<WorkoutSet> sets = new List<WorkoutSet>();
        public bool bonus;
        public bool prAchieved;
    }

    /// <summary>A full workout session. Mirrors web <c>Workout</c>.</summary>
    [Serializable]
    public class Workout
    {
        public string id;
        public long dateMs; // unix epoch millis of creation
        public ProgramType programType;
        public RiskLevel riskLevel;
        public int durationMin; // planned
        public List<WorkoutExercise> exercises = new List<WorkoutExercise>();
        public long startedAtMs; // 0 == not started
        public long finishedAtMs; // 0 == not finished
        public int totalElapsedSec;
        public List<BadgeId> badges = new List<BadgeId>();
        public bool completed;
        public int setsCompleted;
        public int setsSkipped;
    }

    /// <summary>Persisted user preferences. Mirrors web <c>Settings</c>.</summary>
    [Serializable]
    public class Settings
    {
        public WeightUnit weightUnit = WeightUnit.Kg;
        public RiskLevel riskLevel = RiskLevel.Easy;
        public bool firstLaunchDone;

        public Settings Clone() => new Settings
        {
            weightUnit = weightUnit,
            riskLevel = riskLevel,
            firstLaunchDone = firstLaunchDone,
        };
    }

    /// <summary>Best-effort personal record per exercise. Mirrors web <c>PRRecord</c>.</summary>
    [Serializable]
    public class PRRecord
    {
        public float maxWeight;
        public int maxReps;
        public int maxDurationSec;

        public PRRecord Clone() => new PRRecord { maxWeight = maxWeight, maxReps = maxReps, maxDurationSec = maxDurationSec };
    }

    /// <summary>One entry of the PR map (JsonUtility cannot serialize dictionaries directly).</summary>
    [Serializable]
    public class PREntry
    {
        public string exerciseId;
        public PRRecord record = new PRRecord();
    }
}
