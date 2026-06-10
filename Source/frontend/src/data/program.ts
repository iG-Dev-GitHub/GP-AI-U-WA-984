import { PROGRAM_INFO } from "./plinko";
import {
  Exercise,
  ProgramType,
  RiskLevel,
  Workout,
  WorkoutExercise,
} from "./types";

function pickN<T>(arr: T[], n: number): T[] {
  const copy = [...arr];
  const out: T[] = [];
  for (let i = 0; i < n && copy.length > 0; i++) {
    const idx = Math.floor(Math.random() * copy.length);
    out.push(copy.splice(idx, 1)[0]);
  }
  return out;
}

function buildSets(ex: Exercise, count: number): WorkoutExercise["sets"] {
  const sets: WorkoutExercise["sets"] = [];
  for (let i = 0; i < count; i++) {
    sets.push({
      reps: ex.defaultReps,
      weight: ex.defaultWeight,
      durationSec: ex.defaultDurationSec,
      completed: false,
    });
  }
  return sets;
}

export function buildWorkout(
  programType: ProgramType,
  riskLevel: RiskLevel,
  pool: Exercise[],
): Workout {
  const info = PROGRAM_INFO[programType];

  // Pool selection: prefer exercises matching the category, fallback to others
  let matching = pool.filter((e) => e.category === programType);
  if (matching.length < info.numExercises) {
    // pad from any other exercise that isn't 'recovery' for non-recovery programs
    const padPool = pool.filter((e) => !matching.includes(e));
    matching = matching.concat(padPool);
  }
  const chosen = pickN(matching, info.numExercises);

  const exercises: WorkoutExercise[] = chosen.map((ex) => ({
    exerciseId: ex.id,
    name: ex.name,
    category: ex.category,
    sets: buildSets(ex, info.setsPerExercise),
  }));

  // Beast Mode bonus: add a final bonus set to the toughest exercise
  if (programType === "beast" && exercises.length > 0) {
    const target = exercises[exercises.length - 1];
    target.sets.push({
      reps: Math.max(1, Math.floor((target.sets[0]?.reps ?? 1) * 1.5)),
      weight: target.sets[0]?.weight ?? 0,
      durationSec: target.sets[0]?.durationSec,
      completed: false,
    });
    target.bonus = true;
  }

  return {
    id: `w-${Date.now()}`,
    dateISO: new Date().toISOString(),
    programType,
    riskLevel,
    durationMin: info.durationMin,
    exercises,
    badges: [],
    completed: false,
    setsCompleted: 0,
    setsSkipped: 0,
  };
}

export function countTotalSets(w: Workout): number {
  return w.exercises.reduce((acc, e) => acc + e.sets.length, 0);
}
