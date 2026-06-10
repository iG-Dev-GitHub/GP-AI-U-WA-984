using System.Collections.Generic;
using UnityEngine;
using WorkoutDrop.Core;

namespace WorkoutDrop.Data
{
    /// <summary>Per-program metadata. Mirrors one entry of web <c>PROGRAM_INFO</c>.</summary>
    [System.Serializable]
    public class ProgramInfo
    {
        public ProgramType programType;
        public string label;
        public string tagline;
        public int durationMin;
        public int setsPerExercise;
        public int numExercises;
    }

    /// <summary>
    /// Program definitions (label, tagline, duration, set counts). Mirrors <c>PROGRAM_INFO</c>.
    /// </summary>
    [CreateAssetMenu(menuName = "WorkoutDrop/Program Config", fileName = "ProgramConfig")]
    public class ProgramConfig : ScriptableObject
    {
        public List<ProgramInfo> programs = new List<ProgramInfo>();

        public void EnsurePopulated()
        {
            if (programs != null && programs.Count >= 4) return;
            programs = new List<ProgramInfo>
            {
                new ProgramInfo { programType = ProgramType.Recovery, label = "Recovery", tagline = "Light stretching", durationMin = 20, setsPerExercise = 1, numExercises = 3 },
                new ProgramInfo { programType = ProgramType.Cardio, label = "Cardio", tagline = "Run & jump", durationMin = 30, setsPerExercise = 1, numExercises = 3 },
                new ProgramInfo { programType = ProgramType.Strength, label = "Strength", tagline = "Lift heavy", durationMin = 40, setsPerExercise = 3, numExercises = 4 },
                new ProgramInfo { programType = ProgramType.Beast, label = "Beast Mode", tagline = "Max HIIT + bonus", durationMin = 45, setsPerExercise = 4, numExercises = 5 },
            };
        }

        public ProgramInfo Info(ProgramType type)
        {
            EnsurePopulated();
            for (int i = 0; i < programs.Count; i++)
                if (programs[i].programType == type) return programs[i];
            return programs[0];
        }
    }
}
