using System;
using System.Collections.Generic;
using UnityEngine;
using WorkoutDrop.Data;

namespace WorkoutDrop.Core
{
    /// <summary>
    /// Builds a workout from a landed program + risk + exercise pool.
    /// Faithful port of <c>buildWorkout</c> / <c>pickN</c> / <c>buildSets</c> from
    /// <c>src/data/program.ts</c>, including the Beast Mode bonus set.
    /// </summary>
    public static class WorkoutBuilder
    {
        private static List<T> PickN<T>(List<T> arr, int n, IRng rng)
        {
            var copy = new List<T>(arr);
            var outList = new List<T>();
            for (int i = 0; i < n && copy.Count > 0; i++)
            {
                int idx = Mathf.FloorToInt(rng.Value01() * copy.Count);
                if (idx >= copy.Count) idx = copy.Count - 1;
                outList.Add(copy[idx]);
                copy.RemoveAt(idx);
            }
            return outList;
        }

        private static List<WorkoutSet> BuildSets(ExerciseDef ex, int count)
        {
            var sets = new List<WorkoutSet>(count);
            for (int i = 0; i < count; i++)
            {
                sets.Add(new WorkoutSet
                {
                    reps = ex.defaultReps,
                    weight = ex.defaultWeight,
                    durationSec = ex.defaultDurationSec,
                    completed = false,
                    skipped = false,
                });
            }
            return sets;
        }

        public static Workout Build(ProgramType programType, RiskLevel riskLevel, List<ExerciseDef> pool,
            ProgramConfig programConfig, IRng rng)
        {
            var info = programConfig.Info(programType);

            // Prefer exercises matching the category; pad from the rest (matching-first), like the web app.
            var matching = new List<ExerciseDef>();
            foreach (var e in pool)
                if (e.category == programType) matching.Add(e);

            if (matching.Count < info.numExercises)
            {
                foreach (var e in pool)
                    if (!matching.Contains(e)) matching.Add(e);
            }

            var chosen = PickN(matching, info.numExercises, rng);

            var exercises = new List<WorkoutExercise>(chosen.Count);
            foreach (var ex in chosen)
            {
                exercises.Add(new WorkoutExercise
                {
                    exerciseId = ex.id,
                    name = ex.name,
                    category = ex.category,
                    sets = BuildSets(ex, info.setsPerExercise),
                });
            }

            // Beast Mode bonus: add a final heavier set to the toughest (last) exercise.
            if (programType == ProgramType.Beast && exercises.Count > 0)
            {
                var target = exercises[exercises.Count - 1];
                var first = target.sets.Count > 0 ? target.sets[0] : null;
                target.sets.Add(new WorkoutSet
                {
                    reps = Mathf.Max(1, Mathf.FloorToInt((first?.reps ?? 1) * 1.5f)),
                    weight = first?.weight ?? 0,
                    durationSec = first?.durationSec ?? 0,
                    completed = false,
                    skipped = false,
                });
                target.bonus = true;
            }

            long now = TimeUtil.NowMs();
            return new Workout
            {
                id = $"w-{now}",
                dateMs = now,
                programType = programType,
                riskLevel = riskLevel,
                durationMin = info.durationMin,
                exercises = exercises,
                badges = new List<BadgeId>(),
                completed = false,
                setsCompleted = 0,
                setsSkipped = 0,
            };
        }

        public static int CountTotalSets(Workout w)
        {
            int acc = 0;
            foreach (var e in w.exercises) acc += e.sets.Count;
            return acc;
        }
    }

    /// <summary>Epoch helpers so timestamps stay comparable across sessions.</summary>
    public static class TimeUtil
    {
        public static long NowMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public static DateTime ToLocal(long ms) =>
            DateTimeOffset.FromUnixTimeMilliseconds(ms).ToLocalTime().DateTime;
    }
}
