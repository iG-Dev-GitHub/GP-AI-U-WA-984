import { Ionicons } from "@expo/vector-icons";
import { useRouter } from "expo-router";
import { useEffect, useRef, useState } from "react";
import {
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import ExerciseCard from "@/src/components/ExerciseCard";
import RestTimer from "@/src/components/RestTimer";
import TactileButton from "@/src/components/TactileButton";
import { PROGRAM_INFO } from "@/src/data/plinko";
import { addWorkout, checkAndUpdatePRs, getSettings, getWorkouts, workoutsInLast7Days } from "@/src/data/store";
import { BadgeId, Workout, WorkoutExercise, WeightUnit } from "@/src/data/types";
import { storage } from "@/src/utils/storage";

const REST_SECONDS = 45;

export default function WorkoutScreen() {
  const router = useRouter();
  const [workout, setWorkout] = useState<Workout | null>(null);
  const [resting, setResting] = useState(false);
  const [unit, setUnit] = useState<WeightUnit>("kg");
  const [, setTick] = useState(0);
  const startMsRef = useRef<number>(Date.now());

  useEffect(() => {
    (async () => {
      const raw = await storage.getItem<string>("wd:currentWorkout", "");
      if (!raw) {
        router.replace("/(tabs)");
        return;
      }
      const w: Workout = JSON.parse(raw);
      setWorkout(w);
      startMsRef.current = w.startedAt ? new Date(w.startedAt).getTime() : Date.now();
      const s = await getSettings();
      setUnit(s.weightUnit);
    })();
  }, [router]);

  // Update elapsed clock display every second
  useEffect(() => {
    const id = setInterval(() => setTick((t) => t + 1), 1000);
    return () => clearInterval(id);
  }, []);

  if (!workout) return <View style={styles.root} />;

  const totalSets = workout.exercises.reduce(
    (acc, e) => acc + e.sets.length,
    0,
  );
  const doneSets = workout.exercises.reduce(
    (acc, e) => acc + e.sets.filter((s) => s.completed).length,
    0,
  );

  const elapsedSec = Math.floor((Date.now() - startMsRef.current) / 1000);
  const mm = Math.floor(elapsedSec / 60).toString().padStart(2, "0");
  const ss = (elapsedSec % 60).toString().padStart(2, "0");

  const updateSet = (
    exIdx: number,
    setIdx: number,
    patch: Partial<WorkoutExercise["sets"][number]>,
  ) => {
    const next = { ...workout };
    next.exercises = next.exercises.map((e, i) =>
      i !== exIdx
        ? e
        : {
            ...e,
            sets: e.sets.map((s, si) =>
              si !== setIdx ? s : { ...s, ...patch },
            ),
          },
    );
    setWorkout(next);
    storage.setItem("wd:currentWorkout", JSON.stringify(next));
  };

  const logSet = (exIdx: number, setIdx: number) => {
    updateSet(exIdx, setIdx, { completed: true });
    setResting(true);
  };

  const skipSet = (exIdx: number, setIdx: number) => {
    // Mark a sentinel by setting weight = -1 / completed = false but a separate flag — keep it simple:
    // Setting reps to 0 and weight stays — but we already track via completed=false; we need a
    // separate skipped marker so we still count it toward "skipped" rather than "pending".
    const next = { ...workout };
    next.exercises = next.exercises.map((e, i) =>
      i !== exIdx
        ? e
        : {
            ...e,
            sets: e.sets.map((s, si) =>
              si !== setIdx
                ? s
                : { ...s, completed: false, reps: -1 }, // -1 marks skipped
            ),
          },
    );
    setWorkout(next);
    storage.setItem("wd:currentWorkout", JSON.stringify(next));
  };

  const adjustReps = (exIdx: number, setIdx: number, delta: number) => {
    const cur = workout.exercises[exIdx].sets[setIdx];
    const next = Math.max(0, (cur.reps < 0 ? 0 : cur.reps) + delta);
    updateSet(exIdx, setIdx, { reps: next });
  };

  const adjustWeight = (exIdx: number, setIdx: number, delta: number) => {
    const cur = workout.exercises[exIdx].sets[setIdx];
    const next = Math.max(0, cur.weight + delta);
    updateSet(exIdx, setIdx, { weight: next });
  };

  const finishWorkout = async () => {
    let completed = 0;
    let skipped = 0;
    workout.exercises.forEach((e) => {
      e.sets.forEach((s) => {
        if (s.completed) completed++;
        else skipped++;
      });
    });
    const { prExerciseIds } = await checkAndUpdatePRs(workout.exercises);
    const exercisesWithPr = workout.exercises.map((e) => ({
      ...e,
      prAchieved: prExerciseIds.has(e.exerciseId),
    }));

    const badges: BadgeId[] = [];
    if (skipped === 0 && completed > 0) badges.push("full_drop");
    if (workout.programType === "beast" && skipped === 0 && completed > 0)
      badges.push("beast_mode");
    if (prExerciseIds.size > 0) badges.push("pr");

    const finishedAt = new Date().toISOString();
    const totalElapsedSec = Math.floor((Date.now() - startMsRef.current) / 1000);

    const finalWorkout: Workout = {
      ...workout,
      exercises: exercisesWithPr,
      finishedAt,
      totalElapsedSec,
      completed: true,
      setsCompleted: completed,
      setsSkipped: skipped,
      badges,
    };

    // Iron Week: 5 workouts in last 7 days INCLUDING this one
    const prior = await getWorkouts();
    const weekCount = workoutsInLast7Days([finalWorkout, ...prior]);
    if (weekCount >= 5 && !badges.includes("iron_week")) {
      finalWorkout.badges = [...finalWorkout.badges, "iron_week"];
    }

    await addWorkout(finalWorkout);
    await storage.removeItem("wd:currentWorkout");
    await storage.setItem("wd:lastSummary", JSON.stringify(finalWorkout));
    router.replace("/summary");
  };

  const pendingSets = totalSets - doneSets - workout.exercises
    .reduce((acc, e) => acc + e.sets.filter((s) => !s.completed && s.reps < 0).length, 0);

  return (
    <SafeAreaView style={styles.root} edges={["top", "bottom"]}>
      <View style={styles.header}>
        <TouchableOpacity
          testID="workout-back"
          onPress={() => router.back()}
          style={styles.backBtn}
        >
          <Ionicons name="chevron-back" size={22} color="#FFF" />
        </TouchableOpacity>
        <View style={{ alignItems: "center" }}>
          <Text style={styles.headerEyebrow}>
            {workout.programType.toUpperCase()} •{" "}
            {PROGRAM_INFO[workout.programType].durationMin}M
          </Text>
          <Text style={styles.headerTime}>
            {mm}:{ss}
          </Text>
        </View>
        <View
          testID="workout-progress"
          style={[
            styles.progressChip,
            { borderColor: workout.programType === "beast" ? "#FF3B30" : "#00D1FF" },
          ]}
        >
          <Text style={styles.progressText}>
            {doneSets}/{totalSets}
          </Text>
        </View>
      </View>

      <ScrollView
        contentContainerStyle={styles.content}
        showsVerticalScrollIndicator={false}
      >
        {workout.exercises.map((ex, exIdx) => (
          <ExerciseCard
            key={`${ex.exerciseId}-${exIdx}`}
            name={ex.name}
            category={ex.category}
            sets={ex.sets}
            weightUnit={unit}
            bonus={ex.bonus}
            renderSetActions={(setIdx, s) => {
              if (s.completed) {
                return (
                  <View style={styles.statusDone}>
                    <Ionicons name="checkmark-circle" size={20} color="#00FF7A" />
                  </View>
                );
              }
              if (s.reps < 0) {
                return (
                  <View style={styles.statusSkip}>
                    <Ionicons name="close-circle" size={20} color="#FF3B30" />
                  </View>
                );
              }
              return (
                <View style={styles.setActions}>
                  <View style={styles.stepperGroup}>
                    <TouchableOpacity
                      testID={`set-${exIdx}-${setIdx}-rep-minus`}
                      onPress={() => adjustReps(exIdx, setIdx, -1)}
                      style={styles.stepBtn}
                    >
                      <Ionicons name="remove" size={14} color="#FFF" />
                    </TouchableOpacity>
                    <Text style={styles.stepperLabel}>R</Text>
                    <TouchableOpacity
                      testID={`set-${exIdx}-${setIdx}-rep-plus`}
                      onPress={() => adjustReps(exIdx, setIdx, 1)}
                      style={styles.stepBtn}
                    >
                      <Ionicons name="add" size={14} color="#FFF" />
                    </TouchableOpacity>
                  </View>
                  {s.weight > 0 || !s.durationSec ? (
                    <View style={styles.stepperGroup}>
                      <TouchableOpacity
                        testID={`set-${exIdx}-${setIdx}-wt-minus`}
                        onPress={() => adjustWeight(exIdx, setIdx, -2.5)}
                        style={styles.stepBtn}
                      >
                        <Ionicons name="remove" size={14} color="#FFF" />
                      </TouchableOpacity>
                      <Text style={styles.stepperLabel}>W</Text>
                      <TouchableOpacity
                        testID={`set-${exIdx}-${setIdx}-wt-plus`}
                        onPress={() => adjustWeight(exIdx, setIdx, 2.5)}
                        style={styles.stepBtn}
                      >
                        <Ionicons name="add" size={14} color="#FFF" />
                      </TouchableOpacity>
                    </View>
                  ) : null}
                  <TouchableOpacity
                    testID={`set-${exIdx}-${setIdx}-log`}
                    onPress={() => logSet(exIdx, setIdx)}
                    style={styles.logBtn}
                  >
                    <Ionicons name="checkmark" size={16} color="#000" />
                    <Text style={styles.logText}>LOG</Text>
                  </TouchableOpacity>
                  <TouchableOpacity
                    testID={`set-${exIdx}-${setIdx}-skip`}
                    onPress={() => skipSet(exIdx, setIdx)}
                    style={styles.skipBtn}
                  >
                    <Ionicons name="flame" size={14} color="#FF3B30" />
                  </TouchableOpacity>
                </View>
              );
            }}
          />
        ))}

        <TactileButton
          testID="workout-finish"
          title={pendingSets === 0 ? "Finish Workout" : `Finish (${pendingSets} pending)`}
          icon="trophy"
          variant={workout.programType === "beast" ? "beast" : "primary"}
          onPress={finishWorkout}
          style={{ marginTop: 18, marginBottom: resting ? 80 : 0 }}
        />
      </ScrollView>

      {resting ? (
        <RestTimer
          seconds={REST_SECONDS}
          onDone={() => setResting(false)}
          onSkip={() => setResting(false)}
        />
      ) : null}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  root: { flex: 1, backgroundColor: "#050505" },
  header: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    paddingHorizontal: 16,
    paddingVertical: 10,
    borderBottomWidth: 1,
    borderBottomColor: "#1A1A1E",
  },
  backBtn: {
    width: 36,
    height: 36,
    borderRadius: 12,
    backgroundColor: "#121214",
    alignItems: "center",
    justifyContent: "center",
    borderWidth: 1,
    borderColor: "#27272A",
  },
  headerEyebrow: {
    color: "#A1A1AA",
    fontSize: 10,
    fontWeight: "900",
    letterSpacing: 3,
  },
  headerTime: {
    color: "#FFF",
    fontSize: 22,
    fontWeight: "900",
    letterSpacing: 1,
    marginTop: 2,
  },
  progressChip: {
    backgroundColor: "#121214",
    borderWidth: 2,
    borderRadius: 12,
    paddingHorizontal: 10,
    paddingVertical: 6,
  },
  progressText: {
    color: "#FFF",
    fontWeight: "900",
    letterSpacing: 1.5,
  },
  content: { padding: 16, paddingBottom: 32 },
  setActions: {
    flexDirection: "row",
    alignItems: "center",
    gap: 6,
    flexWrap: "wrap",
    justifyContent: "flex-end",
  },
  stepperGroup: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#0A0A0C",
    borderWidth: 1,
    borderColor: "#27272A",
    borderRadius: 10,
    paddingHorizontal: 4,
    paddingVertical: 2,
  },
  stepBtn: {
    paddingHorizontal: 4,
    paddingVertical: 4,
  },
  stepperLabel: {
    color: "#52525B",
    fontWeight: "900",
    fontSize: 10,
    paddingHorizontal: 2,
  },
  logBtn: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#00FF7A",
    borderRadius: 10,
    paddingHorizontal: 10,
    paddingVertical: 6,
  },
  logText: {
    color: "#000",
    fontWeight: "900",
    fontSize: 11,
    letterSpacing: 1,
    marginLeft: 4,
  },
  skipBtn: {
    backgroundColor: "#2A0808",
    borderWidth: 1,
    borderColor: "#FF3B30",
    borderRadius: 10,
    padding: 6,
  },
  statusDone: {
    backgroundColor: "#0A1A0F",
    borderRadius: 10,
    padding: 6,
  },
  statusSkip: {
    backgroundColor: "#2A0808",
    borderRadius: 10,
    padding: 6,
  },
});
