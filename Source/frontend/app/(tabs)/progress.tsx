import { Ionicons } from "@expo/vector-icons";
import { useFocusEffect } from "expo-router";
import { useCallback, useMemo, useState } from "react";
import {
  ScrollView,
  StyleSheet,
  Text,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import BadgeView from "@/src/components/BadgeView";
import StatTile from "@/src/components/StatTile";
import { CELL_COLORS } from "@/src/data/plinko";
import {
  computeStreakDays,
  getWorkouts,
  workoutsInLast7Days,
} from "@/src/data/store";
import { BadgeId, Workout } from "@/src/data/types";

const ALL_BADGES: BadgeId[] = ["full_drop", "beast_mode", "iron_week", "pr"];

function isoDay(d: Date): string {
  return d.toISOString().slice(0, 10);
}

export default function Progress() {
  const [workouts, setWorkouts] = useState<Workout[]>([]);

  useFocusEffect(
    useCallback(() => {
      (async () => setWorkouts(await getWorkouts()))();
    }, []),
  );

  const earnedBadges = useMemo(() => {
    const set = new Set<BadgeId>();
    workouts.forEach((w) => w.badges.forEach((b) => set.add(b)));
    return set;
  }, [workouts]);

  // Calendar: last 28 days
  const calendarDays = useMemo(() => {
    const days = new Set<string>();
    workouts.forEach((w) => {
      if (w.finishedAt) days.add(w.finishedAt.slice(0, 10));
    });
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const cells: { key: string; isWorkout: boolean; label: string }[] = [];
    for (let i = 27; i >= 0; i--) {
      const d = new Date(today);
      d.setDate(d.getDate() - i);
      cells.push({
        key: isoDay(d),
        isWorkout: days.has(isoDay(d)),
        label: d.getDate().toString(),
      });
    }
    return cells;
  }, [workouts]);

  // Per-program count for the simple bar chart
  const programCounts = useMemo(() => {
    const counts: Record<string, number> = {
      recovery: 0,
      cardio: 0,
      strength: 0,
      beast: 0,
    };
    workouts.forEach((w) => (counts[w.programType] += 1));
    return counts;
  }, [workouts]);

  const maxCount = Math.max(1, ...Object.values(programCounts));
  const streak = computeStreakDays(workouts);
  const total = workouts.length;
  const week = workoutsInLast7Days(workouts);

  return (
    <SafeAreaView style={styles.root} edges={["top"]}>
      <ScrollView contentContainerStyle={styles.content}>
        <Text style={styles.eyebrow}>YOUR JOURNEY</Text>
        <Text style={styles.h1}>Progress</Text>

        <View style={styles.statsRow}>
          <StatTile title="Streak" value={`${streak}d`} accent="#00D1FF" testID="progress-streak" />
          <View style={{ width: 10 }} />
          <StatTile title="Total" value={total} accent="#FFCC00" testID="progress-total" />
          <View style={{ width: 10 }} />
          <StatTile title="Week" value={`${week}/7`} accent="#00FF7A" testID="progress-week" />
        </View>

        <Text style={styles.sectionTitle}>Calendar</Text>
        <View style={styles.calendarGrid} testID="progress-calendar">
          {calendarDays.map((d) => (
            <View
              key={d.key}
              style={[
                styles.calCell,
                d.isWorkout && styles.calCellActive,
              ]}
            >
              <Text
                style={[
                  styles.calLabel,
                  d.isWorkout && { color: "#000" },
                ]}
              >
                {d.label}
              </Text>
            </View>
          ))}
        </View>

        <Text style={styles.sectionTitle}>Workouts By Type</Text>
        <View style={styles.chartCard}>
          {(["recovery", "cardio", "strength", "beast"] as const).map((p) => (
            <View key={p} style={styles.barRow}>
              <Text style={styles.barLabel}>{p.toUpperCase()}</Text>
              <View style={styles.barTrack}>
                <View
                  style={[
                    styles.barFill,
                    {
                      width: `${(programCounts[p] / maxCount) * 100}%`,
                      backgroundColor: CELL_COLORS[p],
                    },
                  ]}
                />
              </View>
              <Text style={styles.barVal}>{programCounts[p]}</Text>
            </View>
          ))}
        </View>

        <Text style={styles.sectionTitle}>Badges</Text>
        <View style={styles.badgesGrid}>
          {ALL_BADGES.map((b) => (
            <View key={b} style={styles.badgeWrap}>
              <BadgeView id={b} earned={earnedBadges.has(b)} size="sm" />
            </View>
          ))}
        </View>

        <Text style={styles.sectionTitle}>History</Text>
        {workouts.length === 0 ? (
          <View style={styles.empty}>
            <Ionicons name="time" size={24} color="#52525B" />
            <Text style={styles.emptyText}>Your history will show up here.</Text>
          </View>
        ) : (
          workouts.map((w) => (
            <View
              key={w.id}
              testID={`history-${w.id}`}
              style={[
                styles.historyCard,
                { borderColor: CELL_COLORS[w.programType] },
              ]}
            >
              <View
                style={[
                  styles.histDot,
                  { backgroundColor: CELL_COLORS[w.programType] },
                ]}
              />
              <View style={{ flex: 1 }}>
                <Text style={styles.histTitle}>
                  {w.programType.toUpperCase()}
                </Text>
                <Text style={styles.histSub}>
                  {new Date(w.dateISO).toLocaleString()}
                </Text>
              </View>
              <Text style={styles.histRight}>
                {w.setsCompleted}/{w.setsCompleted + w.setsSkipped}
              </Text>
            </View>
          ))
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  root: { flex: 1, backgroundColor: "#050505" },
  content: { padding: 16, paddingBottom: 32 },
  eyebrow: {
    color: "#52525B",
    letterSpacing: 4,
    fontWeight: "900",
    fontSize: 11,
  },
  h1: {
    color: "#FFF",
    fontWeight: "900",
    fontSize: 32,
    letterSpacing: -1,
    marginTop: 4,
    marginBottom: 20,
  },
  statsRow: { flexDirection: "row" },
  sectionTitle: {
    color: "#FFF",
    fontWeight: "900",
    fontSize: 16,
    letterSpacing: -0.3,
    marginTop: 26,
    marginBottom: 10,
  },
  calendarGrid: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 6,
  },
  calCell: {
    width: `${100 / 7 - 1}%`,
    aspectRatio: 1,
    backgroundColor: "#0A0A0C",
    borderWidth: 1,
    borderColor: "#27272A",
    borderRadius: 8,
    alignItems: "center",
    justifyContent: "center",
  },
  calCellActive: {
    backgroundColor: "#00D1FF",
    borderColor: "#00D1FF",
  },
  calLabel: {
    color: "#52525B",
    fontSize: 11,
    fontWeight: "800",
  },
  chartCard: {
    backgroundColor: "#0A0A0C",
    borderWidth: 2,
    borderColor: "#27272A",
    borderRadius: 16,
    padding: 14,
  },
  barRow: {
    flexDirection: "row",
    alignItems: "center",
    marginVertical: 6,
  },
  barLabel: {
    color: "#A1A1AA",
    fontWeight: "900",
    letterSpacing: 1,
    fontSize: 10,
    width: 80,
  },
  barTrack: {
    flex: 1,
    height: 12,
    backgroundColor: "#1A1A1E",
    borderRadius: 6,
    overflow: "hidden",
    marginHorizontal: 8,
  },
  barFill: {
    height: "100%",
    borderRadius: 6,
  },
  barVal: {
    color: "#FFF",
    fontWeight: "900",
    width: 28,
    textAlign: "right",
  },
  badgesGrid: {
    flexDirection: "row",
    flexWrap: "wrap",
    justifyContent: "space-around",
    rowGap: 18,
  },
  badgeWrap: {
    width: "48%",
    alignItems: "center",
  },
  empty: {
    backgroundColor: "#0A0A0C",
    borderWidth: 2,
    borderColor: "#27272A",
    borderStyle: "dashed",
    borderRadius: 16,
    padding: 20,
    alignItems: "center",
  },
  emptyText: { color: "#A1A1AA", marginTop: 6 },
  historyCard: {
    backgroundColor: "#121214",
    borderWidth: 2,
    borderRadius: 14,
    padding: 12,
    flexDirection: "row",
    alignItems: "center",
    marginBottom: 8,
  },
  histDot: { width: 8, height: 8, borderRadius: 4, marginRight: 10 },
  histTitle: {
    color: "#FFF",
    fontWeight: "900",
    letterSpacing: 1,
    fontSize: 13,
  },
  histSub: { color: "#A1A1AA", fontSize: 11, marginTop: 2 },
  histRight: { color: "#FFF", fontWeight: "900" },
});
