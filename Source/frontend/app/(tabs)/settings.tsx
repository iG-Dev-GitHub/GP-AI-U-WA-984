import { Ionicons } from "@expo/vector-icons";
import { useRouter, useFocusEffect } from "expo-router";
import { useCallback, useState } from "react";
import {
  Modal,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import TactileButton from "@/src/components/TactileButton";
import { CELL_COLORS } from "@/src/data/plinko";
import {
  getSettings,
  resetAll,
  saveSettings,
} from "@/src/data/store";
import { Settings, WeightUnit } from "@/src/data/types";

export default function SettingsScreen() {
  const router = useRouter();
  const [settings, setSettings] = useState<Settings | null>(null);
  const [confirmReset, setConfirmReset] = useState(false);

  useFocusEffect(
    useCallback(() => {
      (async () => setSettings(await getSettings()))();
    }, []),
  );

  const update = async (patch: Partial<Settings>) => {
    if (!settings) return;
    const next = { ...settings, ...patch };
    setSettings(next);
    await saveSettings(next);
  };

  const doReset = async () => {
    await resetAll();
    setConfirmReset(false);
    router.replace("/welcome");
  };

  if (!settings) return <View style={styles.root} />;

  return (
    <SafeAreaView style={styles.root} edges={["top"]}>
      <ScrollView contentContainerStyle={styles.content}>
        <Text style={styles.eyebrow}>PREFERENCES</Text>
        <Text style={styles.h1}>Settings</Text>

        <Text style={styles.sectionTitle}>Weight Unit</Text>
        <View style={styles.unitRow}>
          {(["kg", "lbs"] as WeightUnit[]).map((u) => (
            <TouchableOpacity
              key={u}
              testID={`unit-${u}`}
              onPress={() => update({ weightUnit: u })}
              style={[
                styles.unitChip,
                settings.weightUnit === u && {
                  backgroundColor: "#00D1FF",
                  borderColor: "#00D1FF",
                },
              ]}
            >
              <Text
                style={[
                  styles.unitText,
                  { color: settings.weightUnit === u ? "#000" : "#FFF" },
                ]}
              >
                {u.toUpperCase()}
              </Text>
            </TouchableOpacity>
          ))}
        </View>

        <Text style={styles.sectionTitle}>Default Risk Level</Text>
        <View style={styles.unitRow}>
          {(["easy", "beast"] as const).map((r) => (
            <TouchableOpacity
              key={r}
              testID={`risk-default-${r}`}
              onPress={() => update({ riskLevel: r })}
              style={[
                styles.unitChip,
                settings.riskLevel === r && {
                  backgroundColor: r === "beast" ? "#FF3B30" : "#00D1FF",
                  borderColor: r === "beast" ? "#FF3B30" : "#00D1FF",
                },
              ]}
            >
              <Ionicons
                name={r === "easy" ? "leaf" : "flame"}
                size={14}
                color={settings.riskLevel === r ? (r === "beast" ? "#FFF" : "#000") : "#FFF"}
              />
              <Text
                style={[
                  styles.unitText,
                  {
                    color:
                      settings.riskLevel === r
                        ? r === "beast"
                          ? "#FFF"
                          : "#000"
                        : "#FFF",
                    marginLeft: 6,
                  },
                ]}
              >
                {r.toUpperCase()}
              </Text>
            </TouchableOpacity>
          ))}
        </View>

        <Text style={styles.sectionTitle}>Library</Text>
        <TouchableOpacity
          testID="settings-manage-exercises"
          onPress={() => router.push("/exercises")}
          style={styles.linkRow}
        >
          <View style={styles.linkLeft}>
            <View
              style={[
                styles.linkIcon,
                { backgroundColor: CELL_COLORS.strength },
              ]}
            >
              <Ionicons name="barbell" size={18} color="#000" />
            </View>
            <View>
              <Text style={styles.linkTitle}>Manage Exercises</Text>
              <Text style={styles.linkSub}>Add, edit, delete</Text>
            </View>
          </View>
          <Ionicons name="chevron-forward" size={20} color="#52525B" />
        </TouchableOpacity>

        <Text style={styles.sectionTitle}>Danger Zone</Text>
        <TactileButton
          testID="settings-reset"
          title="Reset All Data"
          icon="trash"
          variant="beast"
          onPress={() => setConfirmReset(true)}
        />

        <Text style={styles.footer}>Offline tracker — no account, no cloud.</Text>
      </ScrollView>

      <Modal visible={confirmReset} transparent animationType="fade">
        <View style={styles.modalRoot}>
          <View style={styles.modalCard}>
            <Ionicons name="warning" size={32} color="#FF3B30" />
            <Text style={styles.modalTitle}>Reset everything?</Text>
            <Text style={styles.modalBody}>
              This clears exercises, history, PRs, badges and settings. Cannot be undone.
            </Text>
            <View style={{ height: 12 }} />
            <TactileButton
              testID="reset-confirm"
              title="Yes, Wipe It"
              icon="trash"
              variant="beast"
              onPress={doReset}
            />
            <View style={{ height: 8 }} />
            <TactileButton
              testID="reset-cancel"
              title="Cancel"
              variant="secondary"
              onPress={() => setConfirmReset(false)}
            />
          </View>
        </View>
      </Modal>
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
  sectionTitle: {
    color: "#FFF",
    fontWeight: "900",
    fontSize: 14,
    letterSpacing: 0,
    marginTop: 20,
    marginBottom: 10,
  },
  unitRow: { flexDirection: "row", gap: 10 },
  unitChip: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#121214",
    paddingVertical: 12,
    paddingHorizontal: 18,
    borderRadius: 12,
    borderWidth: 2,
    borderColor: "#27272A",
  },
  unitText: {
    fontWeight: "900",
    letterSpacing: 2,
    fontSize: 13,
  },
  linkRow: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    backgroundColor: "#121214",
    borderWidth: 2,
    borderColor: "#27272A",
    borderRadius: 14,
    padding: 14,
  },
  linkLeft: { flexDirection: "row", alignItems: "center" },
  linkIcon: {
    width: 38,
    height: 38,
    borderRadius: 12,
    alignItems: "center",
    justifyContent: "center",
    marginRight: 12,
  },
  linkTitle: { color: "#FFF", fontWeight: "800", fontSize: 15 },
  linkSub: { color: "#A1A1AA", fontSize: 12, marginTop: 2 },
  footer: {
    color: "#3F3F46",
    textAlign: "center",
    marginTop: 24,
    fontSize: 11,
    letterSpacing: 2,
    textTransform: "uppercase",
  },
  modalRoot: {
    flex: 1,
    backgroundColor: "rgba(0,0,0,0.7)",
    alignItems: "center",
    justifyContent: "center",
    padding: 24,
  },
  modalCard: {
    backgroundColor: "#121214",
    borderWidth: 2,
    borderColor: "#FF3B30",
    borderRadius: 18,
    padding: 24,
    width: "100%",
    alignItems: "center",
  },
  modalTitle: {
    color: "#FFF",
    fontWeight: "900",
    fontSize: 20,
    marginTop: 12,
    marginBottom: 6,
  },
  modalBody: {
    color: "#A1A1AA",
    textAlign: "center",
    lineHeight: 18,
  },
});
