import { Ionicons } from "@expo/vector-icons";
import { useFocusEffect, useRouter } from "expo-router";
import { useCallback, useState } from "react";
import {
  KeyboardAvoidingView,
  Modal,
  Platform,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import TactileButton from "@/src/components/TactileButton";
import { CELL_COLORS } from "@/src/data/plinko";
import {
  addExercise,
  deleteExercise,
  getExercises,
  updateExercise,
} from "@/src/data/store";
import { Exercise, ExerciseCategory } from "@/src/data/types";

const CATEGORIES: ExerciseCategory[] = [
  "recovery",
  "cardio",
  "strength",
  "beast",
];

export default function Exercises() {
  const router = useRouter();
  const [list, setList] = useState<Exercise[]>([]);
  const [filter, setFilter] = useState<ExerciseCategory | "all">("all");
  const [editing, setEditing] = useState<Exercise | null>(null);

  const refresh = useCallback(async () => {
    setList(await getExercises());
  }, []);

  useFocusEffect(
    useCallback(() => {
      refresh();
    }, [refresh]),
  );

  const filtered = list.filter((e) => filter === "all" || e.category === filter);

  const openNew = () => {
    setEditing({
      id: `ex-${Date.now()}`,
      name: "",
      category: "strength",
      defaultReps: 10,
      defaultWeight: 20,
    });
  };

  const save = async () => {
    if (!editing || !editing.name.trim()) return;
    const exists = list.some((e) => e.id === editing.id);
    if (exists) await updateExercise(editing);
    else await addExercise(editing);
    setEditing(null);
    refresh();
  };

  const remove = async (id: string) => {
    await deleteExercise(id);
    refresh();
  };

  return (
    <SafeAreaView style={styles.root} edges={["top", "bottom"]}>
      <View style={styles.header}>
        <TouchableOpacity
          testID="exercises-back"
          onPress={() => router.back()}
          style={styles.backBtn}
        >
          <Ionicons name="chevron-back" size={22} color="#FFF" />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>EXERCISES</Text>
        <TouchableOpacity
          testID="exercises-add"
          onPress={openNew}
          style={[styles.backBtn, { backgroundColor: "#00D1FF" }]}
        >
          <Ionicons name="add" size={22} color="#000" />
        </TouchableOpacity>
      </View>

      <ScrollView
        horizontal
        showsHorizontalScrollIndicator={false}
        contentContainerStyle={styles.chipsRow}
      >
        {(["all", ...CATEGORIES] as const).map((c) => (
          <TouchableOpacity
            key={c}
            testID={`filter-${c}`}
            onPress={() => setFilter(c as never)}
            style={[
              styles.chip,
              filter === c && {
                backgroundColor:
                  c === "all" ? "#FFF" : CELL_COLORS[c as ExerciseCategory],
                borderColor:
                  c === "all" ? "#FFF" : CELL_COLORS[c as ExerciseCategory],
              },
            ]}
          >
            <Text
              style={[
                styles.chipText,
                filter === c && { color: "#000" },
              ]}
            >
              {c.toUpperCase()}
            </Text>
          </TouchableOpacity>
        ))}
      </ScrollView>

      <ScrollView contentContainerStyle={styles.list}>
        {filtered.map((e) => (
          <View
            key={e.id}
            testID={`exercise-${e.id}`}
            style={[styles.row, { borderColor: CELL_COLORS[e.category] }]}
          >
            <View
              style={[
                styles.cat,
                { backgroundColor: CELL_COLORS[e.category] },
              ]}
            >
              <Text style={styles.catText}>{e.category[0].toUpperCase()}</Text>
            </View>
            <View style={{ flex: 1 }}>
              <Text style={styles.name}>{e.name}</Text>
              <Text style={styles.meta}>
                {e.defaultReps > 0 ? `${e.defaultReps} reps` : ""}
                {e.defaultWeight > 0 ? ` • ${e.defaultWeight} kg` : ""}
                {e.defaultDurationSec ? ` • ${e.defaultDurationSec}s` : ""}
              </Text>
            </View>
            <TouchableOpacity
              testID={`edit-${e.id}`}
              onPress={() => setEditing(e)}
              style={styles.iconBtn}
            >
              <Ionicons name="pencil" size={16} color="#A1A1AA" />
            </TouchableOpacity>
            <TouchableOpacity
              testID={`delete-${e.id}`}
              onPress={() => remove(e.id)}
              style={[styles.iconBtn, { marginLeft: 6 }]}
            >
              <Ionicons name="trash" size={16} color="#FF3B30" />
            </TouchableOpacity>
          </View>
        ))}

        {filtered.length === 0 ? (
          <View style={styles.empty}>
            <Text style={styles.emptyText}>No exercises here yet.</Text>
          </View>
        ) : null}
      </ScrollView>

      <Modal
        visible={!!editing}
        transparent
        animationType="slide"
        onRequestClose={() => setEditing(null)}
      >
        <KeyboardAvoidingView
          behavior={Platform.OS === "ios" ? "padding" : undefined}
          style={styles.modalRoot}
        >
          <View style={styles.modalCard}>
            <Text style={styles.modalTitle}>
              {list.some((x) => x.id === editing?.id) ? "Edit" : "New"} Exercise
            </Text>

            <Text style={styles.field}>NAME</Text>
            <TextInput
              testID="exercise-name"
              value={editing?.name ?? ""}
              onChangeText={(t) =>
                setEditing((e) => (e ? { ...e, name: t } : e))
              }
              placeholder="e.g. Goblet Squat"
              placeholderTextColor="#52525B"
              style={styles.input}
            />

            <Text style={styles.field}>CATEGORY</Text>
            <View style={styles.catRow}>
              {CATEGORIES.map((c) => (
                <TouchableOpacity
                  key={c}
                  testID={`cat-${c}`}
                  onPress={() =>
                    setEditing((e) => (e ? { ...e, category: c } : e))
                  }
                  style={[
                    styles.catChip,
                    {
                      backgroundColor:
                        editing?.category === c
                          ? CELL_COLORS[c]
                          : "#0A0A0C",
                      borderColor: CELL_COLORS[c],
                    },
                  ]}
                >
                  <Text
                    style={{
                      color: editing?.category === c ? "#000" : "#FFF",
                      fontWeight: "900",
                      letterSpacing: 1,
                      fontSize: 10,
                    }}
                  >
                    {c.toUpperCase()}
                  </Text>
                </TouchableOpacity>
              ))}
            </View>

            <View style={{ flexDirection: "row" }}>
              <View style={{ flex: 1, marginRight: 8 }}>
                <Text style={styles.field}>REPS</Text>
                <TextInput
                  testID="exercise-reps"
                  value={String(editing?.defaultReps ?? "")}
                  onChangeText={(t) =>
                    setEditing((e) =>
                      e ? { ...e, defaultReps: parseInt(t || "0", 10) } : e,
                    )
                  }
                  keyboardType="numeric"
                  style={styles.input}
                />
              </View>
              <View style={{ flex: 1 }}>
                <Text style={styles.field}>WEIGHT (KG)</Text>
                <TextInput
                  testID="exercise-weight"
                  value={String(editing?.defaultWeight ?? "")}
                  onChangeText={(t) =>
                    setEditing((e) =>
                      e ? { ...e, defaultWeight: parseFloat(t || "0") } : e,
                    )
                  }
                  keyboardType="numeric"
                  style={styles.input}
                />
              </View>
            </View>

            <Text style={styles.field}>DURATION (SEC, OPTIONAL)</Text>
            <TextInput
              testID="exercise-duration"
              value={String(editing?.defaultDurationSec ?? "")}
              onChangeText={(t) =>
                setEditing((e) =>
                  e
                    ? {
                        ...e,
                        defaultDurationSec: t ? parseInt(t, 10) : undefined,
                      }
                    : e,
                )
              }
              placeholder="e.g. 60"
              placeholderTextColor="#52525B"
              keyboardType="numeric"
              style={styles.input}
            />

            <View style={{ marginTop: 12 }}>
              <TactileButton
                testID="exercise-save"
                title="Save Exercise"
                icon="checkmark"
                variant="primary"
                onPress={save}
                disabled={!(editing?.name ?? "").trim()}
              />
              <View style={{ height: 8 }} />
              <TactileButton
                testID="exercise-cancel"
                title="Cancel"
                variant="secondary"
                onPress={() => setEditing(null)}
              />
            </View>
          </View>
        </KeyboardAvoidingView>
      </Modal>
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
    paddingVertical: 12,
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
  headerTitle: {
    color: "#FFF",
    fontSize: 16,
    fontWeight: "900",
    letterSpacing: 3,
  },
  chipsRow: {
    paddingHorizontal: 16,
    paddingBottom: 12,
    gap: 8,
  },
  chip: {
    paddingHorizontal: 14,
    paddingVertical: 8,
    borderRadius: 10,
    borderWidth: 2,
    borderColor: "#27272A",
    backgroundColor: "#121214",
    height: 36,
    flexShrink: 0,
    justifyContent: "center",
  },
  chipText: {
    color: "#FFF",
    fontWeight: "900",
    letterSpacing: 1.5,
    fontSize: 11,
  },
  list: { paddingHorizontal: 16, paddingBottom: 24 },
  row: {
    backgroundColor: "#121214",
    borderWidth: 2,
    borderRadius: 14,
    padding: 12,
    flexDirection: "row",
    alignItems: "center",
    marginBottom: 8,
  },
  cat: {
    width: 32,
    height: 32,
    borderRadius: 10,
    alignItems: "center",
    justifyContent: "center",
    marginRight: 10,
  },
  catText: { color: "#000", fontWeight: "900" },
  name: { color: "#FFF", fontWeight: "800", fontSize: 15 },
  meta: { color: "#A1A1AA", fontSize: 12, marginTop: 2 },
  iconBtn: {
    width: 32,
    height: 32,
    borderRadius: 10,
    backgroundColor: "#0A0A0C",
    alignItems: "center",
    justifyContent: "center",
    borderWidth: 1,
    borderColor: "#27272A",
  },
  empty: {
    backgroundColor: "#0A0A0C",
    borderWidth: 2,
    borderColor: "#27272A",
    borderStyle: "dashed",
    borderRadius: 14,
    padding: 20,
    alignItems: "center",
  },
  emptyText: { color: "#A1A1AA" },
  modalRoot: {
    flex: 1,
    backgroundColor: "rgba(0,0,0,0.7)",
    justifyContent: "flex-end",
  },
  modalCard: {
    backgroundColor: "#121214",
    borderTopLeftRadius: 24,
    borderTopRightRadius: 24,
    padding: 20,
    borderWidth: 2,
    borderColor: "#27272A",
  },
  modalTitle: {
    color: "#FFF",
    fontWeight: "900",
    fontSize: 18,
    marginBottom: 16,
    letterSpacing: -0.3,
  },
  field: {
    color: "#52525B",
    fontWeight: "900",
    fontSize: 10,
    letterSpacing: 2,
    marginBottom: 6,
    marginTop: 8,
  },
  input: {
    backgroundColor: "#0A0A0C",
    borderWidth: 2,
    borderColor: "#27272A",
    borderRadius: 12,
    paddingVertical: 12,
    paddingHorizontal: 14,
    color: "#FFF",
    fontWeight: "700",
    fontSize: 15,
  },
  catRow: { flexDirection: "row", gap: 6, marginBottom: 4 },
  catChip: {
    paddingHorizontal: 10,
    paddingVertical: 8,
    borderRadius: 10,
    borderWidth: 2,
  },
});
