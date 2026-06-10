import { Ionicons } from "@expo/vector-icons";
import { useFocusEffect, useRouter } from "expo-router";
import { useCallback, useEffect, useState } from "react";
import {
  Dimensions,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import PlinkoBoard from "@/src/components/PlinkoBoard";
import StatTile from "@/src/components/StatTile";
import TactileButton from "@/src/components/TactileButton";
import { CELL_COLORS } from "@/src/data/plinko";
import {
  computeStreakDays,
  getSettings,
  getWorkouts,
  workoutsInLast7Days,
} from "@/src/data/store";
import { Workout } from "@/src/data/types";

const { width: SCREEN_W } = Dimensions.get("window");
const BOARD_W = Math.min(SCREEN_W, 480) - 32;

export default function Home() {
  const router = useRouter();
  const [workouts, setWorkouts] = useState<Workout[]>([]);
  const [ready, setReady] = useState(false);

  // Initial guard: route fresh users to /welcome.
  useEffect(() => {
    let cancelled = false;
    (async () => {
      const s = await getSettings();
      if (cancelled) return;
      if (!s.firstLaunchDone) {
        router.replace("/welcome");
        return;
      }
      setWorkouts(await getWorkouts());
      setReady(true);
    })();
    return () => {
      cancelled = true;
    };
  }, [router]);

  // Refresh on every focus so finishing a workout/changing settings reflects here.
  useFocusEffect(
    useCallback(() => {
      (async () => {
        const s = await getSettings();
        if (!s.firstLaunchDone) return;
        setWorkouts(await getWorkouts());
      })();
    }, []),
  );

  if (!ready) {
    return <View style={[styles.root, { alignItems: "center", justifyContent: "center" }]} />;
  }

  const streak = computeStreakDays(workouts);
  const weekCount = workoutsInLast7Days(workouts);
  const total = workouts.length;

  return (
    <SafeAreaView style={styles.root} edges={["top"]}>
      <ScrollView contentContainerStyle={styles.content}>
        <View style={styles.heroLabel}>
          <Ionicons name="flash" size={14} color="#00D1FF" />
          <Text style={styles.heroLabelText}>TODAY{"'"}S DROP</Text>
        </View>
        <Text style={styles.title}>What will the{"\n"}board give you?</Text>

        <View style={styles.boardWrap}>
          <PlinkoBoard boardWidth={BOARD_W} staticPreview />
        </View>

        <TactileButton
          testID="home-drop-cta"
          title="Drop For Workout"
          icon="arrow-down-circle"
          variant="primary"
          onPress={() => router.push("/drop")}
          style={{ marginTop: 18 }}
        />

        <View style={styles.statsRow}>
          <StatTile
            testID="stat-streak"
            title="Streak"
            value={`${streak}d`}
            accent="#00D1FF"
          />
          <View style={{ width: 12 }} />
          <StatTile
            testID="stat-week"
            title="This Week"
            value={`${weekCount}/7`}
            accent="#00FF7A"
          />
          <View style={{ width: 12 }} />
          <StatTile
            testID="stat-total"
            title="Total"
            value={total}
            accent="#FFCC00"
          />
        </View>

        <View style={styles.recentHeader}>
          <Text style={styles.sectionTitle}>Recent Workouts</Text>
          {workouts.length > 0 ? (
            <Text style={styles.sectionMore}>{workouts.length}</Text>
          ) : null}
        </View>

        {workouts.length === 0 ? (
          <View style={styles.emptyCard} testID="empty-recent">
            <Ionicons name="moon" size={28} color="#52525B" />
            <Text style={styles.emptyTitle}>No workouts yet</Text>
            <Text style={styles.emptyBody}>
              Drop the ball above to roll your first program.
            </Text>
          </View>
        ) : (
          workouts.slice(0, 5).map((w) => (
            <View
              key={w.id}
              testID={`recent-${w.id}`}
              style={[
                styles.recentCard,
                { borderColor: CELL_COLORS[w.programType] },
              ]}
            >
              <View
                style={[
                  styles.recentDot,
                  { backgroundColor: CELL_COLORS[w.programType] },
                ]}
              />
              <View style={{ flex: 1 }}>
                <Text style={styles.recentTitle}>
                  {w.programType.toUpperCase()}
                </Text>
                <Text style={styles.recentSubtitle}>
                  {new Date(w.dateISO).toLocaleDateString(undefined, {
                    weekday: "short",
                    month: "short",
                    day: "numeric",
                  })}
                  {"  •  "}
                  {w.setsCompleted}/{w.setsCompleted + w.setsSkipped} sets
                </Text>
              </View>
              {w.badges.length > 0 ? (
                <View style={styles.badgeChip}>
                  <Ionicons name="trophy" size={12} color="#FFD700" />
                  <Text style={styles.badgeChipText}>{w.badges.length}</Text>
                </View>
              ) : null}
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
  heroLabel: {
    flexDirection: "row",
    alignItems: "center",
    marginBottom: 6,
  },
  heroLabelText: {
    color: "#00D1FF",
    fontSize: 11,
    fontWeight: "900",
    letterSpacing: 3,
    marginLeft: 6,
  },
  title: {
    color: "#FFF",
    fontWeight: "900",
    fontSize: 26,
    letterSpacing: -0.5,
    lineHeight: 32,
    marginBottom: 18,
  },
  boardWrap: { alignItems: "center" },
  statsRow: { flexDirection: "row", marginTop: 24 },
  recentHeader: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    marginTop: 30,
    marginBottom: 12,
  },
  sectionTitle: {
    color: "#FFF",
    fontWeight: "900",
    fontSize: 18,
    letterSpacing: -0.3,
  },
  sectionMore: {
    color: "#52525B",
    fontWeight: "800",
    letterSpacing: 1.5,
    fontSize: 12,
  },
  recentCard: {
    backgroundColor: "#121214",
    borderWidth: 2,
    borderRadius: 16,
    padding: 14,
    flexDirection: "row",
    alignItems: "center",
    marginBottom: 10,
  },
  recentDot: { width: 10, height: 10, borderRadius: 5, marginRight: 10 },
  recentTitle: {
    color: "#FFF",
    fontWeight: "900",
    letterSpacing: 1.5,
    fontSize: 13,
  },
  recentSubtitle: {
    color: "#A1A1AA",
    fontSize: 12,
    marginTop: 2,
  },
  badgeChip: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#1A1A0A",
    borderColor: "#FFD700",
    borderWidth: 1,
    borderRadius: 6,
    paddingHorizontal: 6,
    paddingVertical: 3,
  },
  badgeChipText: {
    color: "#FFD700",
    fontWeight: "900",
    fontSize: 11,
    marginLeft: 4,
  },
  emptyCard: {
    backgroundColor: "#0A0A0C",
    borderWidth: 2,
    borderColor: "#27272A",
    borderRadius: 18,
    padding: 24,
    alignItems: "center",
    borderStyle: "dashed",
  },
  emptyTitle: {
    color: "#A1A1AA",
    fontWeight: "900",
    fontSize: 15,
    marginTop: 8,
    letterSpacing: 0.5,
  },
  emptyBody: {
    color: "#52525B",
    fontSize: 13,
    marginTop: 4,
    textAlign: "center",
  },
});
