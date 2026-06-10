import { storage } from "@/src/utils/storage";
import { SEED_EXERCISES } from "./seed";
import {
  Exercise,
  PRMap,
  PRRecord,
  Settings,
  Workout,
  WorkoutExercise,
} from "./types";

const KEYS = {
  EXERCISES: "wd:exercises",
  WORKOUTS: "wd:workouts",
  SETTINGS: "wd:settings",
  PRS: "wd:prs",
};

const DEFAULT_SETTINGS: Settings = {
  weightUnit: "kg",
  riskLevel: "easy",
  firstLaunchDone: false,
};

const EMPTY_PR: PRRecord = { maxWeight: 0, maxReps: 0, maxDurationSec: 0 };

async function readJSON<T>(key: string, fallback: T): Promise<T> {
  const raw = await storage.getItem<string>(key, "");
  if (!raw) return fallback;
  try {
    return JSON.parse(raw) as T;
  } catch {
    return fallback;
  }
}

async function writeJSON<T>(key: string, value: T): Promise<void> {
  await storage.setItem(key, JSON.stringify(value));
}

// ---------- Settings ----------
export async function getSettings(): Promise<Settings> {
  const s = await readJSON<Settings>(KEYS.SETTINGS, DEFAULT_SETTINGS);
  return { ...DEFAULT_SETTINGS, ...s };
}

export async function saveSettings(s: Settings): Promise<void> {
  await writeJSON(KEYS.SETTINGS, s);
}

// ---------- Exercises ----------
export async function getExercises(): Promise<Exercise[]> {
  const list = await readJSON<Exercise[]>(KEYS.EXERCISES, []);
  if (list.length === 0) {
    await writeJSON(KEYS.EXERCISES, SEED_EXERCISES);
    return SEED_EXERCISES;
  }
  return list;
}

export async function saveExercises(list: Exercise[]): Promise<void> {
  await writeJSON(KEYS.EXERCISES, list);
}

export async function addExercise(e: Exercise): Promise<void> {
  const list = await getExercises();
  list.push(e);
  await saveExercises(list);
}

export async function updateExercise(e: Exercise): Promise<void> {
  const list = await getExercises();
  const idx = list.findIndex((x) => x.id === e.id);
  if (idx >= 0) {
    list[idx] = e;
    await saveExercises(list);
  }
}

export async function deleteExercise(id: string): Promise<void> {
  const list = await getExercises();
  await saveExercises(list.filter((x) => x.id !== id));
}

// ---------- Workouts ----------
export async function getWorkouts(): Promise<Workout[]> {
  return readJSON<Workout[]>(KEYS.WORKOUTS, []);
}

export async function saveWorkouts(list: Workout[]): Promise<void> {
  await writeJSON(KEYS.WORKOUTS, list);
}

export async function addWorkout(w: Workout): Promise<void> {
  const list = await getWorkouts();
  list.unshift(w);
  await saveWorkouts(list.slice(0, 200));
}

// ---------- PRs ----------
export async function getPRs(): Promise<PRMap> {
  return readJSON<PRMap>(KEYS.PRS, {});
}

export async function checkAndUpdatePRs(
  exercises: WorkoutExercise[],
): Promise<{ updated: PRMap; prExerciseIds: Set<string> }> {
  const map = await getPRs();
  const prIds = new Set<string>();
  for (const we of exercises) {
    const prev = map[we.exerciseId] ?? { ...EMPTY_PR };
    let next = { ...prev };
    let isPR = false;
    for (const s of we.sets) {
      if (!s.completed) continue;
      if (s.weight > next.maxWeight) {
        next.maxWeight = s.weight;
        isPR = true;
      }
      if (s.reps > next.maxReps) {
        next.maxReps = s.reps;
        isPR = true;
      }
      if (s.durationSec && s.durationSec > next.maxDurationSec) {
        next.maxDurationSec = s.durationSec;
        isPR = true;
      }
    }
    // Only flag a PR if there was a prior baseline (avoid first-time PR for every set).
    const hadBaseline = prev.maxWeight + prev.maxReps + prev.maxDurationSec > 0;
    if (isPR && hadBaseline) prIds.add(we.exerciseId);
    map[we.exerciseId] = next;
  }
  await writeJSON(KEYS.PRS, map);
  return { updated: map, prExerciseIds: prIds };
}

// ---------- Streak & badges ----------
export function computeStreakDays(workouts: Workout[]): number {
  // Count distinct YYYY-MM-DD finished workouts going back from today.
  const days = new Set<string>();
  for (const w of workouts) {
    if (!w.finishedAt) continue;
    days.add(w.finishedAt.slice(0, 10));
  }
  let streak = 0;
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  for (let i = 0; i < 30; i++) {
    const d = new Date(today);
    d.setDate(d.getDate() - i);
    const key = d.toISOString().slice(0, 10);
    if (days.has(key)) streak++;
    else if (i > 0) break;
  }
  return streak;
}

export function workoutsInLast7Days(workouts: Workout[]): number {
  const cutoff = Date.now() - 7 * 24 * 60 * 60 * 1000;
  return workouts.filter(
    (w) => w.finishedAt && new Date(w.finishedAt).getTime() >= cutoff,
  ).length;
}

// ---------- Reset ----------
export async function resetAll(): Promise<void> {
  await storage.removeItem(KEYS.EXERCISES);
  await storage.removeItem(KEYS.WORKOUTS);
  await storage.removeItem(KEYS.SETTINGS);
  await storage.removeItem(KEYS.PRS);
}
