using System.Collections.Generic;
using UnityEngine;
using WorkoutDrop.Core;

namespace WorkoutDrop.Data
{
    /// <summary>
    /// Plinko board configuration. Mirrors <c>src/data/plinko.ts</c>:
    /// the 7-cell program strip, per-risk weight curves and the row count.
    /// Authored as an asset so designers can tweak the board without touching code.
    /// Falls back to the canonical web values if the serialized data is empty.
    /// </summary>
    [CreateAssetMenu(menuName = "WorkoutDrop/Plinko Config", fileName = "PlinkoConfig")]
    public class PlinkoConfig : ScriptableObject
    {
        [Tooltip("Program landing strip, left to right (7 cells). Extremes = Beast, centre = Recovery.")]
        public List<ProgramType> cellPrograms = new List<ProgramType>();

        [Tooltip("Selection weights per cell for EASY risk (softer bell curve).")]
        public List<int> cellWeightsEasy = new List<int>();

        [Tooltip("Selection weights per cell for BEAST risk (edges dominate).")]
        public List<int> cellWeightsBeast = new List<int>();

        [Tooltip("Rows of pegs above the cells.")]
        public int plinkoRows = 12;

        public int CellCount => cellPrograms != null && cellPrograms.Count > 0 ? cellPrograms.Count : 7;

        /// <summary>Ensure canonical defaults exist even if the .asset shipped empty.</summary>
        public void EnsurePopulated()
        {
            if (cellPrograms == null || cellPrograms.Count == 0)
            {
                cellPrograms = new List<ProgramType>
                {
                    ProgramType.Beast, ProgramType.Strength, ProgramType.Cardio, ProgramType.Recovery,
                    ProgramType.Cardio, ProgramType.Strength, ProgramType.Beast,
                };
            }
            if (cellWeightsEasy == null || cellWeightsEasy.Count == 0)
                cellWeightsEasy = new List<int> { 2, 6, 14, 26, 14, 6, 2 };
            if (cellWeightsBeast == null || cellWeightsBeast.Count == 0)
                cellWeightsBeast = new List<int> { 18, 16, 10, 6, 10, 16, 18 };
            if (plinkoRows <= 0) plinkoRows = 12;
        }

        public IReadOnlyList<int> WeightsFor(RiskLevel risk)
        {
            EnsurePopulated();
            return risk == RiskLevel.Beast ? cellWeightsBeast : cellWeightsEasy;
        }

        public ProgramType ProgramAt(int index)
        {
            EnsurePopulated();
            index = Mathf.Clamp(index, 0, cellPrograms.Count - 1);
            return cellPrograms[index];
        }

        /// <summary>
        /// Weighted random cell pick. Mirrors <c>pickCellIndex</c> from the web app.
        /// </summary>
        public int PickCellIndex(RiskLevel risk, IRng rng)
        {
            var weights = WeightsFor(risk);
            int total = 0;
            for (int i = 0; i < weights.Count; i++) total += weights[i];
            float roll = rng.Value01() * total;
            for (int i = 0; i < weights.Count; i++)
            {
                roll -= weights[i];
                if (roll <= 0) return i;
            }
            return weights.Count - 1;
        }
    }
}
