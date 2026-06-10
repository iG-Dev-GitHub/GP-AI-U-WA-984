import React from "react";
import { StyleSheet, Text, View } from "react-native";
import { Ionicons } from "@expo/vector-icons";

import { CELL_COLORS } from "@/src/data/plinko";
import { ProgramType, WeightUnit } from "@/src/data/types";

interface SetRow {
  reps: number;
  weight: number;
  durationSec?: number;
  completed: boolean;
}

interface Props {
  name: string;
  category: ProgramType;
  sets: SetRow[];
  weightUnit: WeightUnit;
  prAchieved?: boolean;
  bonus?: boolean;
  renderSetActions?: (index: number, set: SetRow) => React.ReactNode;
}

function formatWeight(weight: number, unit: WeightUnit): string {
  if (weight === 0) return "BW";
  const v = unit === "lbs" ? Math.round(weight * 2.20462) : weight;
  return `${v} ${unit}`;
}

export default function ExerciseCard({
  name,
  category,
  sets,
  weightUnit,
  prAchieved,
  bonus,
  renderSetActions,
}: Props) {
  return (
    <View
      testID={`exercise-card-${name.toLowerCase().replace(/\s+/g, "-")}`}
      style={[styles.card, { borderColor: CELL_COLORS[category] }]}
    >
      <View style={styles.header}>
        <View style={styles.titleRow}>
          <View
            style={[styles.dot, { backgroundColor: CELL_COLORS[category] }]}
          />
          <Text style={styles.title}>{name}</Text>
          {bonus ? (
            <View style={styles.bonusBadge}>
              <Ionicons name="flame" size={12} color="#000" />
              <Text style={styles.bonusText}>BONUS</Text>
            </View>
          ) : null}
        </View>
        {prAchieved ? (
          <View style={styles.prBadge}>
            <Ionicons name="trophy" size={14} color="#000" />
            <Text style={styles.prText}>PR</Text>
          </View>
        ) : null}
      </View>

      <View style={styles.setsHeader}>
        <Text style={[styles.colLabel, { flex: 0.5 }]}>SET</Text>
        <Text style={styles.colLabel}>REPS</Text>
        <Text style={styles.colLabel}>WEIGHT</Text>
        <Text style={[styles.colLabel, { flex: 1.2, textAlign: "right" }]}>
          ACTION
        </Text>
      </View>

      {sets.map((s, i) => (
        <View key={i} style={styles.setRow}>
          <Text style={[styles.setText, { flex: 0.5 }]}>{i + 1}</Text>
          <Text style={styles.setText}>
            {s.durationSec ? `${s.durationSec}s` : s.reps}
          </Text>
          <Text style={styles.setText}>{formatWeight(s.weight, weightUnit)}</Text>
          <View style={{ flex: 1.2, alignItems: "flex-end" }}>
            {renderSetActions ? renderSetActions(i, s) : null}
          </View>
        </View>
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: "#121214",
    borderWidth: 2,
    borderRadius: 18,
    padding: 16,
    marginBottom: 14,
  },
  header: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 12,
  },
  titleRow: {
    flexDirection: "row",
    alignItems: "center",
    flex: 1,
    flexWrap: "wrap",
  },
  dot: {
    width: 10,
    height: 10,
    borderRadius: 5,
    marginRight: 10,
  },
  title: {
    color: "#FFF",
    fontSize: 17,
    fontWeight: "800",
    letterSpacing: 0.5,
  },
  bonusBadge: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#FF3B30",
    borderRadius: 6,
    paddingHorizontal: 6,
    paddingVertical: 2,
    marginLeft: 8,
  },
  bonusText: {
    color: "#000",
    fontSize: 10,
    fontWeight: "900",
    letterSpacing: 1,
    marginLeft: 3,
  },
  prBadge: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#FFD700",
    borderRadius: 6,
    paddingHorizontal: 6,
    paddingVertical: 3,
  },
  prText: {
    color: "#000",
    fontSize: 10,
    fontWeight: "900",
    marginLeft: 3,
    letterSpacing: 1,
  },
  setsHeader: {
    flexDirection: "row",
    alignItems: "center",
    paddingBottom: 8,
    borderBottomWidth: 1,
    borderBottomColor: "#27272A",
    marginBottom: 6,
  },
  colLabel: {
    color: "#52525B",
    fontSize: 10,
    fontWeight: "800",
    letterSpacing: 1.5,
    flex: 1,
  },
  setRow: {
    flexDirection: "row",
    alignItems: "center",
    paddingVertical: 8,
  },
  setText: {
    color: "#FFF",
    fontSize: 15,
    fontWeight: "700",
    flex: 1,
  },
});
