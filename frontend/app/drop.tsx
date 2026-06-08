import { Ionicons } from "@expo/vector-icons";
import * as Haptics from "expo-haptics";
import { useRouter } from "expo-router";
import { useEffect, useState } from "react";
import {
  Dimensions,
  Platform,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import {
  Easing,
  runOnJS,
  useSharedValue,
  withSequence,
  withTiming,
} from "react-native-reanimated";

import PlinkoBoard from "@/src/components/PlinkoBoard";
import TactileButton from "@/src/components/TactileButton";
import { buildWorkout } from "@/src/data/program";
import {
  CELL_COLORS,
  CELL_PROGRAMS,
  PLINKO_ROWS,
  PROGRAM_INFO,
  pickCellIndex,
} from "@/src/data/plinko";
import { getExercises, getSettings, saveSettings } from "@/src/data/store";
import { ProgramType, RiskLevel, Workout } from "@/src/data/types";
import { storage } from "@/src/utils/storage";

const { width: SCREEN_W } = Dimensions.get("window");
const BOARD_W = SCREEN_W - 32;
const ROW_GAP = 22;

export default function Drop() {
  const router = useRouter();
  const [risk, setRisk] = useState<RiskLevel>("easy");
  const [dropping, setDropping] = useState(false);
  const [resultIdx, setResultIdx] = useState<number | undefined>();
  const [resultProgram, setResultProgram] = useState<ProgramType | undefined>();
  const [beastOverlay, setBeastOverlay] = useState(false);

  const ballX = useSharedValue(BOARD_W / 2);
  const ballY = useSharedValue(-20);
  const [ballVisible, setBallVisible] = useState(false);

  useEffect(() => {
    (async () => {
      const s = await getSettings();
      setRisk(s.riskLevel);
    })();
  }, []);

  const onPickRisk = async (r: RiskLevel) => {
    setRisk(r);
    const s = await getSettings();
    await saveSettings({ ...s, riskLevel: r });
  };

  const announceResult = async (idx: number) => {
    const program = CELL_PROGRAMS[idx];
    setResultIdx(idx);
    setResultProgram(program);
    if (program === "beast") {
      setBeastOverlay(true);
      if (Platform.OS !== "web") {
        Haptics.notificationAsync(Haptics.NotificationFeedbackType.Warning);
      }
    } else if (Platform.OS !== "web") {
      Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Medium);
    }
  };

  const drop = async () => {
    if (dropping) return;
    setResultIdx(undefined);
    setResultProgram(undefined);
    setBeastOverlay(false);
    setDropping(true);

    const cellIdx = pickCellIndex(risk);
    const cellWidth = BOARD_W / 7;
    const targetX = cellIdx * cellWidth + cellWidth / 2;

    // Build the bounce path: per row, jitter left/right around eventual target.
    setBallVisible(true);
    ballX.value = BOARD_W / 2;
    ballY.value = 0;

    const ROWS = PLINKO_ROWS;
    const totalDuration = 1400;
    const perRow = totalDuration / ROWS;

    const xKeyframes: number[] = [];
    for (let i = 0; i < ROWS; i++) {
      // Bias gradually towards targetX
      const t = (i + 1) / ROWS;
      const base = BOARD_W / 2 + (targetX - BOARD_W / 2) * t;
      const jitter = (Math.random() - 0.5) * cellWidth * 0.9;
      xKeyframes.push(Math.max(16, Math.min(BOARD_W - 16, base + jitter)));
    }
    xKeyframes.push(targetX); // final settle

    const yKeyframes: number[] = [];
    for (let i = 1; i <= ROWS; i++) {
      yKeyframes.push(24 + i * ROW_GAP);
    }
    yKeyframes.push(24 + ROWS * ROW_GAP + 24); // into the cell

    // Animate Y as a chained sequence so it visibly hops down.
    let ySeq: any = withTiming(yKeyframes[0], {
      duration: perRow,
      easing: Easing.in(Easing.cubic),
    });
    for (let i = 1; i < yKeyframes.length; i++) {
      ySeq = withSequence(
        ySeq,
        withTiming(yKeyframes[i], {
          duration: i === yKeyframes.length - 1 ? 300 : perRow,
          easing: Easing.in(Easing.cubic),
        }),
      );
    }
    let xSeq: any = withTiming(xKeyframes[0], { duration: perRow });
    for (let i = 1; i < xKeyframes.length; i++) {
      xSeq = withSequence(
        xSeq,
        withTiming(xKeyframes[i], {
          duration: i === xKeyframes.length - 1 ? 300 : perRow,
        }),
      );
    }

    ballY.value = ySeq;
    ballX.value = withSequence(
      xSeq,
      withTiming(targetX, {
        duration: 0,
      }, (finished) => {
        if (finished) runOnJS(announceResult)(cellIdx);
      }),
    );

    if (Platform.OS !== "web") {
      Haptics.selectionAsync();
    }

    setTimeout(() => {
      setDropping(false);
    }, totalDuration + 400);
  };

  const startWorkout = async () => {
    if (resultProgram === undefined) return;
    const exercises = await getExercises();
    const w: Workout = buildWorkout(resultProgram, risk, exercises);
    w.startedAt = new Date().toISOString();
    await storage.setItem("wd:currentWorkout", JSON.stringify(w));
    router.replace("/workout");
  };

  const beastBall = resultProgram === "beast" && resultIdx !== undefined;

  return (
    <SafeAreaView style={[styles.root, beastOverlay && styles.rootBeast]} edges={["top", "bottom"]}>
      <View style={styles.header}>
        <TouchableOpacity
          testID="drop-back"
          onPress={() => router.back()}
          style={styles.backBtn}
        >
          <Ionicons name="chevron-back" size={22} color="#FFF" />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>DROP ZONE</Text>
        <View style={{ width: 36 }} />
      </View>

      <View style={styles.riskWrap}>
        <Text style={styles.riskLabel}>RISK LEVEL</Text>
        <View style={styles.riskRow}>
          <TouchableOpacity
            testID="risk-easy"
            onPress={() => onPickRisk("easy")}
            style={[
              styles.riskChip,
              risk === "easy" && {
                backgroundColor: "#00D1FF",
                borderColor: "#00D1FF",
              },
            ]}
          >
            <Ionicons
              name="leaf"
              size={16}
              color={risk === "easy" ? "#000" : "#00D1FF"}
            />
            <Text
              style={[
                styles.riskText,
                { color: risk === "easy" ? "#000" : "#FFF" },
              ]}
            >
              EASY
            </Text>
          </TouchableOpacity>
          <TouchableOpacity
            testID="risk-beast"
            onPress={() => onPickRisk("beast")}
            style={[
              styles.riskChip,
              risk === "beast" && {
                backgroundColor: "#FF3B30",
                borderColor: "#FF3B30",
              },
            ]}
          >
            <Ionicons
              name="flame"
              size={16}
              color={risk === "beast" ? "#FFF" : "#FF3B30"}
            />
            <Text
              style={[
                styles.riskText,
                { color: risk === "beast" ? "#FFF" : "#FFF" },
              ]}
            >
              BEAST
            </Text>
          </TouchableOpacity>
        </View>
      </View>

      <View style={styles.boardWrap}>
        <PlinkoBoard
          boardWidth={BOARD_W}
          ballX={ballX}
          ballY={ballY}
          ballVisible={ballVisible}
          ballOnFire={beastBall || (dropping && risk === "beast")}
          beastMode={beastOverlay}
          highlightCellIndex={resultIdx}
        />
      </View>

      {resultProgram ? (
        <View
          testID="drop-result"
          style={[
            styles.resultCard,
            { borderColor: CELL_COLORS[resultProgram] },
          ]}
        >
          <Text style={[styles.resultLabel, { color: CELL_COLORS[resultProgram] }]}>
            TODAY{"'"}S PROGRAM
          </Text>
          <Text style={styles.resultTitle}>
            {PROGRAM_INFO[resultProgram].label.toUpperCase()}
          </Text>
          <Text style={styles.resultSub}>
            {PROGRAM_INFO[resultProgram].tagline} • {PROGRAM_INFO[resultProgram].durationMin} min
          </Text>
          <TactileButton
            testID="drop-start-workout"
            title="Start Workout"
            icon="play"
            variant={resultProgram === "beast" ? "beast" : "primary"}
            onPress={startWorkout}
            style={{ marginTop: 14 }}
          />
        </View>
      ) : (
        <View style={styles.dropBtnWrap}>
          <TactileButton
            testID="drop-button"
            title={dropping ? "Dropping..." : risk === "beast" ? "Drop Beast" : "Drop"}
            icon="arrow-down-circle"
            variant={risk === "beast" ? "beast" : "primary"}
            disabled={dropping}
            onPress={drop}
          />
        </View>
      )}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  root: { flex: 1, backgroundColor: "#050505" },
  rootBeast: { backgroundColor: "#160404" },
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
  riskWrap: {
    paddingHorizontal: 16,
    paddingBottom: 12,
  },
  riskLabel: {
    color: "#52525B",
    fontSize: 10,
    fontWeight: "900",
    letterSpacing: 3,
    marginBottom: 8,
  },
  riskRow: {
    flexDirection: "row",
    gap: 10,
  },
  riskChip: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: "#121214",
    paddingVertical: 10,
    paddingHorizontal: 16,
    borderRadius: 12,
    borderWidth: 2,
    borderColor: "#27272A",
  },
  riskText: {
    fontSize: 13,
    fontWeight: "900",
    letterSpacing: 2,
    marginLeft: 6,
  },
  boardWrap: {
    alignItems: "center",
    paddingHorizontal: 16,
    flex: 1,
    justifyContent: "center",
  },
  dropBtnWrap: {
    paddingHorizontal: 16,
    paddingBottom: 16,
  },
  resultCard: {
    margin: 16,
    backgroundColor: "#0F0F12",
    borderWidth: 2,
    borderRadius: 18,
    padding: 18,
    alignItems: "center",
  },
  resultLabel: {
    fontSize: 11,
    fontWeight: "900",
    letterSpacing: 3,
    marginBottom: 4,
  },
  resultTitle: {
    color: "#FFF",
    fontSize: 28,
    fontWeight: "900",
    letterSpacing: -0.5,
  },
  resultSub: {
    color: "#A1A1AA",
    fontSize: 13,
    marginTop: 4,
  },
});
