using System.Collections.Generic;
using UnityEngine;
using WorkoutDrop.Core;

namespace WorkoutDrop.Data
{
    /// <summary>
    /// Seed exercise library. Mirrors <c>SEED_EXERCISES</c> from <c>src/data/seed.ts</c>.
    /// Used only to seed empty storage on first run; the live list lives in the store
    /// thereafter. Falls back to the canonical 20 exercises if the .asset shipped empty.
    /// </summary>
    [CreateAssetMenu(menuName = "WorkoutDrop/Exercise Database", fileName = "ExerciseDatabase")]
    public class ExerciseDatabase : ScriptableObject
    {
        public List<ExerciseDef> seedExercises = new List<ExerciseDef>();

        public void EnsurePopulated()
        {
            if (seedExercises != null && seedExercises.Count > 0) return;
            seedExercises = new List<ExerciseDef>
            {
                // Recovery (stretches)
                new ExerciseDef("ex-stretch-hamstring", "Hamstring Stretch", ProgramType.Recovery, 1, 0, 60),
                new ExerciseDef("ex-stretch-shoulder", "Shoulder Stretch", ProgramType.Recovery, 1, 0, 45),
                new ExerciseDef("ex-stretch-childpose", "Child's Pose", ProgramType.Recovery, 1, 0, 60),
                new ExerciseDef("ex-stretch-pigeon", "Pigeon Pose", ProgramType.Recovery, 1, 0, 60),
                new ExerciseDef("ex-stretch-cobra", "Cobra Stretch", ProgramType.Recovery, 1, 0, 45),
                // Cardio
                new ExerciseDef("ex-cardio-jumprope", "Jump Rope", ProgramType.Cardio, 1, 0, 180),
                new ExerciseDef("ex-cardio-jumpingjacks", "Jumping Jacks", ProgramType.Cardio, 50, 0),
                new ExerciseDef("ex-cardio-highknees", "High Knees", ProgramType.Cardio, 1, 0, 60),
                new ExerciseDef("ex-cardio-runinplace", "Run In Place", ProgramType.Cardio, 1, 0, 300),
                new ExerciseDef("ex-cardio-mountainclimb", "Mountain Climbers", ProgramType.Cardio, 30, 0),
                // Strength
                new ExerciseDef("ex-str-pushup", "Push-Ups", ProgramType.Strength, 12, 0),
                new ExerciseDef("ex-str-squat", "Bodyweight Squat", ProgramType.Strength, 15, 0),
                new ExerciseDef("ex-str-deadlift", "Deadlift", ProgramType.Strength, 8, 60),
                new ExerciseDef("ex-str-bench", "Bench Press", ProgramType.Strength, 8, 40),
                new ExerciseDef("ex-str-overhead", "Overhead Press", ProgramType.Strength, 8, 25),
                new ExerciseDef("ex-str-row", "Bent-Over Row", ProgramType.Strength, 10, 30),
                new ExerciseDef("ex-str-plank", "Plank", ProgramType.Strength, 1, 0, 60),
                // Beast (HIIT / explosive)
                new ExerciseDef("ex-beast-burpee", "Burpees", ProgramType.Beast, 15, 0),
                new ExerciseDef("ex-beast-kbswing", "Kettlebell Swing", ProgramType.Beast, 20, 16),
                new ExerciseDef("ex-beast-thruster", "Thrusters", ProgramType.Beast, 12, 20),
            };
        }

        /// <summary>Deep copy of the seed list (so callers never mutate the asset).</summary>
        public List<ExerciseDef> CloneSeed()
        {
            EnsurePopulated();
            var list = new List<ExerciseDef>(seedExercises.Count);
            foreach (var e in seedExercises) list.Add(e.Clone());
            return list;
        }
    }
}
