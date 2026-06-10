import { Exercise } from "./types";

// 20 starter exercises across all categories. IDs are stable so they survive resets.
export const SEED_EXERCISES: Exercise[] = [
  // Recovery (stretches)
  { id: "ex-stretch-hamstring", name: "Hamstring Stretch", category: "recovery", defaultReps: 1, defaultWeight: 0, defaultDurationSec: 60 },
  { id: "ex-stretch-shoulder", name: "Shoulder Stretch", category: "recovery", defaultReps: 1, defaultWeight: 0, defaultDurationSec: 45 },
  { id: "ex-stretch-childpose", name: "Child's Pose", category: "recovery", defaultReps: 1, defaultWeight: 0, defaultDurationSec: 60 },
  { id: "ex-stretch-pigeon", name: "Pigeon Pose", category: "recovery", defaultReps: 1, defaultWeight: 0, defaultDurationSec: 60 },
  { id: "ex-stretch-cobra", name: "Cobra Stretch", category: "recovery", defaultReps: 1, defaultWeight: 0, defaultDurationSec: 45 },

  // Cardio
  { id: "ex-cardio-jumprope", name: "Jump Rope", category: "cardio", defaultReps: 1, defaultWeight: 0, defaultDurationSec: 180 },
  { id: "ex-cardio-jumpingjacks", name: "Jumping Jacks", category: "cardio", defaultReps: 50, defaultWeight: 0 },
  { id: "ex-cardio-highknees", name: "High Knees", category: "cardio", defaultReps: 1, defaultWeight: 0, defaultDurationSec: 60 },
  { id: "ex-cardio-runinplace", name: "Run In Place", category: "cardio", defaultReps: 1, defaultWeight: 0, defaultDurationSec: 300 },
  { id: "ex-cardio-mountainclimb", name: "Mountain Climbers", category: "cardio", defaultReps: 30, defaultWeight: 0 },

  // Strength
  { id: "ex-str-pushup", name: "Push-Ups", category: "strength", defaultReps: 12, defaultWeight: 0 },
  { id: "ex-str-squat", name: "Bodyweight Squat", category: "strength", defaultReps: 15, defaultWeight: 0 },
  { id: "ex-str-deadlift", name: "Deadlift", category: "strength", defaultReps: 8, defaultWeight: 60 },
  { id: "ex-str-bench", name: "Bench Press", category: "strength", defaultReps: 8, defaultWeight: 40 },
  { id: "ex-str-overhead", name: "Overhead Press", category: "strength", defaultReps: 8, defaultWeight: 25 },
  { id: "ex-str-row", name: "Bent-Over Row", category: "strength", defaultReps: 10, defaultWeight: 30 },
  { id: "ex-str-plank", name: "Plank", category: "strength", defaultReps: 1, defaultWeight: 0, defaultDurationSec: 60 },

  // Beast (HIIT/explosive)
  { id: "ex-beast-burpee", name: "Burpees", category: "beast", defaultReps: 15, defaultWeight: 0 },
  { id: "ex-beast-kbswing", name: "Kettlebell Swing", category: "beast", defaultReps: 20, defaultWeight: 16 },
  { id: "ex-beast-thruster", name: "Thrusters", category: "beast", defaultReps: 12, defaultWeight: 20 },
];
