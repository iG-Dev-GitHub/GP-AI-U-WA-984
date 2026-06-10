export type ExerciseCategory =
  | "recovery"
  | "cardio"
  | "strength"
  | "beast";

export type ProgramType = "recovery" | "cardio" | "strength" | "beast";

export type RiskLevel = "easy" | "beast";

export type WeightUnit = "kg" | "lbs";

export interface Exercise {
  id: string;
  name: string;
  category: ExerciseCategory;
  // Default suggestion shown when adding to a workout
  defaultReps: number;
  defaultWeight: number; // in kg (we convert on display)
  // For cardio/recovery — duration in seconds (used instead of reps)
  defaultDurationSec?: number;
}

export interface WorkoutSet {
  reps: number;
  weight: number; // kg
  durationSec?: number; // for time-based
  completed: boolean;
}

export interface WorkoutExercise {
  exerciseId: string;
  name: string;
  category: ExerciseCategory;
  sets: WorkoutSet[];
  bonus?: boolean; // beast mode bonus set
  prAchieved?: boolean;
}

export type BadgeId = "full_drop" | "beast_mode" | "iron_week" | "pr";

export interface Workout {
  id: string;
  dateISO: string;
  programType: ProgramType;
  riskLevel: RiskLevel;
  durationMin: number; // planned duration
  exercises: WorkoutExercise[];
  startedAt?: string;
  finishedAt?: string;
  totalElapsedSec?: number;
  badges: BadgeId[];
  completed: boolean;
  setsCompleted: number;
  setsSkipped: number;
}

export interface Settings {
  weightUnit: WeightUnit;
  riskLevel: RiskLevel;
  firstLaunchDone: boolean;
}

export interface PRRecord {
  // best weight at any reps
  maxWeight: number;
  // best reps at any weight
  maxReps: number;
  // best longest duration (cardio/recovery)
  maxDurationSec: number;
}

export type PRMap = Record<string, PRRecord>;
