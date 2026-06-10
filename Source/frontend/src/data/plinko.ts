import { ProgramType, RiskLevel } from "./types";

// Cell layout from left to right (7 cells).
// Mirrors a classic Plinko payout strip: extremes are rare/dangerous (Beast Mode),
// center is the safety zone (Recovery).
export const CELL_PROGRAMS: ProgramType[] = [
  "beast",
  "strength",
  "cardio",
  "recovery",
  "cardio",
  "strength",
  "beast",
];

export const CELL_COLORS: Record<ProgramType, string> = {
  recovery: "#007AFF",
  cardio: "#34C759",
  strength: "#FFCC00",
  beast: "#FF3B30",
};

export const CELL_COLORS_DARK: Record<ProgramType, string> = {
  recovery: "#003B7A",
  cardio: "#1E6B30",
  strength: "#8A6E00",
  beast: "#8A1A14",
};

export const PROGRAM_INFO: Record<
  ProgramType,
  { label: string; tagline: string; durationMin: number; setsPerExercise: number; numExercises: number }
> = {
  recovery: { label: "Recovery", tagline: "Light stretching", durationMin: 20, setsPerExercise: 1, numExercises: 3 },
  cardio: { label: "Cardio", tagline: "Run & jump", durationMin: 30, setsPerExercise: 1, numExercises: 3 },
  strength: { label: "Strength", tagline: "Lift heavy", durationMin: 40, setsPerExercise: 3, numExercises: 4 },
  beast: { label: "Beast Mode", tagline: "Max HIIT + bonus", durationMin: 45, setsPerExercise: 4, numExercises: 5 },
};

// Probability weights per cell index for each risk level.
// Easy biases towards center (recovery/cardio); Beast biases towards edges.
export const CELL_WEIGHTS: Record<RiskLevel, number[]> = {
  easy: [2, 6, 14, 26, 14, 6, 2],     // softer bell curve, very rare beast
  beast: [18, 16, 10, 6, 10, 16, 18], // inverted curve, edges dominate
};

export function pickCellIndex(risk: RiskLevel): number {
  const weights = CELL_WEIGHTS[risk];
  const total = weights.reduce((a, b) => a + b, 0);
  let roll = Math.random() * total;
  for (let i = 0; i < weights.length; i++) {
    roll -= weights[i];
    if (roll <= 0) return i;
  }
  return weights.length - 1;
}

export const PLINKO_ROWS = 12; // 12 rows of pegs above the cells
