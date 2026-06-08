import { Ionicons } from "@expo/vector-icons";
import { useRouter } from "expo-router";
import { useEffect, useState } from "react";
import { ScrollView, StyleSheet, Text, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import Animated, {
  useAnimatedStyle,
  useSharedValue,
  withDelay,
  withSequence,
  withTiming,
} from "react-native-reanimated";

import BadgeView from "@/src/components/BadgeView";
import StatTile from "@/src/components/StatTile";
import TactileButton from "@/src/components/TactileButton";
import { CELL_COLORS, PROGRAM_INFO } from "@/src/data/plinko";
import { Workout } from "@/src/data/types";
import { storage } from "@/src/utils/storage";

export default function Summary() {
  const router = useRouter();
  const [workout, setWorkout] = useState<Workout | null>(null);
  const flash = useSharedValue(0);

  useEffect(() => {
    (async () => {
      const raw = await storage.getItem<string>("wd:lastSummary", "");
      if (!raw) {
        router.replace("/(tabs)");
        return;
      }
      const w: Workout = JSON.parse(raw);
      setWorkout(w);
      if (w.exercises.some((e) => e.prAchieved)) {
        flash.value = withSequence(
          withTiming(1, { duration: 250 }),
          withDelay(400, withTiming(0, { duration: 800 })),
        );
      }
    })();
  }, [flash, router]);

  const flashStyle = useAnimatedStyle(() => ({
    opacity: flash.value,
  }));

  if (!workout) return <View style={styles.root} />;

  const mins = Math.floor((workout.totalElapsedSec ?? 0) / 60);
  const secs = (workout.totalElapsedSec ?? 0) % 60;
  const prList = workout.exercises.filter((e) => e.prAchieved);

  return (
    <SafeAreaView style={styles.root} edges={["top", "bottom"]}>
      <Animated.View
        pointerEvents="none"
        style={[styles.goldFlash, flashStyle]}
      />
      <ScrollView contentContainerStyle={styles.content}>
        <View
          style={[
            styles.banner,
            { borderColor: CELL_COLORS[workout.programType] },
          ]}
        >
          <Text
            style={[
              styles.bannerLabel,
              { color: CELL_COLORS[workout.programType] },
            ]}
          >
            {workout.programType.toUpperCase()} COMPLETE
          </Text>
          <Text style={styles.bannerTitle}>
            {PROGRAM_INFO[workout.programType].label}
          </Text>
          <Text style={styles.bannerSub}>
            {new Date(workout.finishedAt ?? workout.dateISO).toLocaleString()}
          </Text>
        </View>

        <View style={styles.statsRow}>
          <StatTile
            testID="summary-completed"
            title="Completed"
            value={workout.setsCompleted}
            accent="#00FF7A"
          />
          <View style={{ width: 10 }} />
          <StatTile
            testID="summary-skipped"
            title="Skipped"
            value={workout.setsSkipped}
            accent="#FF3B30"
          />
          <View style={{ width: 10 }} />
          <StatTile
            testID="summary-time"
            title="Time"
            value={`${mins}:${secs.toString().padStart(2, "0")}`}
            accent="#00D1FF"
          />
        </View>

        {prList.length > 0 ? (
          <View style={styles.prCard} testID="summary-pr-card">
            <View style={styles.prHeader}>
              <Ionicons name="ribbon" size={18} color="#FFD700" />
              <Text style={styles.prHeaderText}>PERSONAL RECORDS</Text>
            </View>
            {prList.map((e) => (
              <View key={e.exerciseId} style={styles.prRow}>
                <Ionicons name="trophy" size={14} color="#FFD700" />
                <Text style={styles.prText}>{e.name}</Text>
              </View>
            ))}
          </View>
        ) : null}

        <Text style={styles.sectionTitle}>Badges Earned</Text>
        {workout.badges.length === 0 ? (
          <View style={styles.empty}>
            <Text style={styles.emptyText}>No badges this round — push harder next drop.</Text>
          </View>
        ) : (
          <View style={styles.badgesGrid}>
            {workout.badges.map((b) => (
              <View key={b} style={styles.badgeWrap}>
                <BadgeView id={b} size="md" />
              </View>
            ))}
          </View>
        )}

        <TactileButton
          testID="summary-done"
          title="Back to Home"
          icon="home"
          variant="primary"
          onPress={() => router.replace("/(tabs)")}
          style={{ marginTop: 24 }}
        />
        <TactileButton
          testID="summary-drop-again"
          title="Drop Again"
          icon="arrow-down-circle"
          variant="secondary"
          onPress={() => router.replace("/drop")}
          style={{ marginTop: 10 }}
        />
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  root: { flex: 1, backgroundColor: "#050505" },
  goldFlash: {
    position: "absolute",
    top: 0,
    bottom: 0,
    left: 0,
    right: 0,
    backgroundColor: "#FFD700",
    zIndex: 10,
  },
  content: { padding: 16, paddingBottom: 32 },
  banner: {
    backgroundColor: "#0F0F12",
    borderWidth: 2,
    borderRadius: 18,
    padding: 22,
    alignItems: "center",
    marginBottom: 16,
  },
  bannerLabel: {
    fontSize: 11,
    fontWeight: "900",
    letterSpacing: 3,
    marginBottom: 4,
  },
  bannerTitle: {
    color: "#FFF",
    fontSize: 30,
    fontWeight: "900",
    letterSpacing: -0.5,
  },
  bannerSub: {
    color: "#A1A1AA",
    fontSize: 12,
    marginTop: 4,
  },
  statsRow: { flexDirection: "row", marginBottom: 18 },
  prCard: {
    backgroundColor: "#1A1A0A",
    borderColor: "#FFD700",
    borderWidth: 2,
    borderRadius: 16,
    padding: 14,
    marginBottom: 18,
  },
  prHeader: {
    flexDirection: "row",
    alignItems: "center",
    marginBottom: 8,
  },
  prHeaderText: {
    color: "#FFD700",
    fontWeight: "900",
    letterSpacing: 2,
    fontSize: 12,
    marginLeft: 6,
  },
  prRow: {
    flexDirection: "row",
    alignItems: "center",
    paddingVertical: 4,
  },
  prText: {
    color: "#FFF",
    fontWeight: "700",
    marginLeft: 8,
  },
  sectionTitle: {
    color: "#FFF",
    fontWeight: "900",
    fontSize: 18,
    letterSpacing: -0.3,
    marginBottom: 12,
  },
  badgesGrid: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 16,
  },
  badgeWrap: {
    minWidth: 110,
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
  emptyText: {
    color: "#A1A1AA",
    fontSize: 13,
    textAlign: "center",
  },
});
