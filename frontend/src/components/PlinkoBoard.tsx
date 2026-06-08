import React, { useEffect } from "react";
import { Image, StyleSheet, View, ViewStyle } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import Animated, {
  useAnimatedStyle,
  useSharedValue,
  withRepeat,
  withSequence,
  withTiming,
  Easing,
} from "react-native-reanimated";

import {
  CELL_COLORS,
  CELL_COLORS_DARK,
  CELL_PROGRAMS,
  PLINKO_ROWS,
} from "@/src/data/plinko";
import { ProgramType } from "@/src/data/types";

const KETTLEBELL_NORMAL = require("../../assets/images/plinko/kettlebell.png");
const KETTLEBELL_FIRE = require("../../assets/images/plinko/kettlebell_fire.png");

// Map each program type to a small descriptive icon shown inside its landing cell.
const CELL_ICONS: Record<ProgramType, keyof typeof Ionicons.glyphMap> = {
  recovery: "leaf",
  cardio: "pulse",
  strength: "barbell",
  beast: "flame",
};

interface Props {
  boardWidth: number;
  highlightCellIndex?: number;
  beastMode?: boolean;
  ballX?: Animated.SharedValue<number>;
  ballY?: Animated.SharedValue<number>;
  ballVisible?: boolean;
  ballOnFire?: boolean;
  staticPreview?: boolean;
  style?: ViewStyle;
}

const PEG_SIZE = 6;
const CELL_HEIGHT = 56;
const ROW_GAP = 22;

export default function PlinkoBoard({
  boardWidth,
  highlightCellIndex,
  beastMode,
  ballX,
  ballY,
  ballVisible,
  ballOnFire,
  staticPreview,
  style,
}: Props) {
  const boardHeight = PLINKO_ROWS * ROW_GAP + CELL_HEIGHT + 24;
  const cellWidth = boardWidth / 7;
  const glow = useSharedValue(0);

  useEffect(() => {
    glow.value = withRepeat(
      withSequence(
        withTiming(1, { duration: 1400, easing: Easing.inOut(Easing.quad) }),
        withTiming(0, { duration: 1400, easing: Easing.inOut(Easing.quad) }),
      ),
      -1,
      false,
    );
  }, [glow]);

  const ballStyle = useAnimatedStyle(() => ({
    transform: [
      { translateX: (ballX?.value ?? boardWidth / 2) - 16 },
      { translateY: (ballY?.value ?? 0) - 16 },
    ],
    opacity: ballVisible ? 1 : 0,
  }));

  // Build peg positions (triangle widening downwards)
  const pegs: { x: number; y: number; isGreen: boolean }[] = [];
  for (let row = 0; row < PLINKO_ROWS; row++) {
    const cols = row + 3; // top row 3 pegs, last row 14 pegs
    const spacing = boardWidth / (cols + 1);
    for (let c = 1; c <= cols; c++) {
      pegs.push({
        x: spacing * c,
        y: 24 + row * ROW_GAP,
        // alternate neon blue / neon green for that Hacksaw vibe
        isGreen: (row + c) % 2 === 0,
      });
    }
  }

  const overlayStyle = useAnimatedStyle(() => ({
    opacity: beastMode ? 0.5 + 0.3 * glow.value : 0,
  }));

  return (
    <View
      testID="plinko-board"
      style={[
        styles.board,
        { width: boardWidth, height: boardHeight },
        beastMode && styles.boardBeast,
        style,
      ]}
    >
      <Animated.View
        pointerEvents="none"
        style={[styles.beastOverlay, overlayStyle]}
      />

      {/* Pegs */}
      {pegs.map((p, i) => (
        <View
          key={i}
          style={[
            styles.peg,
            {
              left: p.x - PEG_SIZE / 2,
              top: p.y - PEG_SIZE / 2,
              backgroundColor: beastMode
                ? "#FF6B5C"
                : p.isGreen
                  ? "#00FF7A"
                  : "#00D1FF",
              shadowColor: beastMode
                ? "#FF3B30"
                : p.isGreen
                  ? "#00FF7A"
                  : "#00D1FF",
            },
          ]}
        />
      ))}

      {/* Cells (bottom row of programs) */}
      <View
        style={[
          styles.cellsRow,
          { top: 24 + PLINKO_ROWS * ROW_GAP, height: CELL_HEIGHT },
        ]}
      >
        {CELL_PROGRAMS.map((program, i) => {
          const isHighlight = highlightCellIndex === i;
          return (
            <View
              key={i}
              testID={`plinko-cell-${i}`}
              style={[
                styles.cell,
                {
                  width: cellWidth - 4,
                  backgroundColor: isHighlight
                    ? CELL_COLORS[program]
                    : CELL_COLORS_DARK[program],
                  borderColor: CELL_COLORS[program],
                  shadowColor: CELL_COLORS[program],
                  shadowOpacity: isHighlight ? 1 : 0.4,
                },
              ]}
            >
              <Ionicons
                name={CELL_ICONS[program]}
                size={20}
                color={isHighlight ? "#000" : "#FFFFFF"}
              />
            </View>
          );
        })}
      </View>

      {/* Ball */}
      {!staticPreview && (
        <Animated.View
          testID="plinko-ball"
          style={[styles.ball, ballStyle, ballOnFire && styles.ballFire]}
        >
          <Image
            source={ballOnFire ? KETTLEBELL_FIRE : KETTLEBELL_NORMAL}
            style={styles.ballImg}
            resizeMode="contain"
          />
        </Animated.View>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  board: {
    backgroundColor: "#0A0A0C",
    borderRadius: 24,
    borderWidth: 2,
    borderColor: "#27272A",
    overflow: "hidden",
    position: "relative",
  },
  boardBeast: {
    borderColor: "#FF3B30",
  },
  beastOverlay: {
    position: "absolute",
    top: 0,
    bottom: 0,
    left: 0,
    right: 0,
    backgroundColor: "#2A0808",
  },
  peg: {
    position: "absolute",
    width: PEG_SIZE,
    height: PEG_SIZE,
    borderRadius: PEG_SIZE / 2,
    shadowRadius: 6,
    shadowOpacity: 1,
    shadowOffset: { width: 0, height: 0 },
    elevation: 4,
  },
  cellsRow: {
    position: "absolute",
    left: 2,
    right: 2,
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
  },
  cell: {
    height: 48,
    borderRadius: 10,
    borderWidth: 2,
    alignItems: "center",
    justifyContent: "center",
    shadowOffset: { width: 0, height: 0 },
    shadowRadius: 10,
    marginHorizontal: 1,
  },
  ball: {
    position: "absolute",
    width: 36,
    height: 36,
    alignItems: "center",
    justifyContent: "center",
  },
  ballImg: {
    width: 36,
    height: 36,
  },
  ballFire: {
    shadowColor: "#FF3B30",
    shadowOpacity: 1,
    shadowRadius: 12,
    shadowOffset: { width: 0, height: 0 },
    elevation: 6,
  },
  flame: {
    position: "absolute",
    top: -10,
  },
});

export { CELL_HEIGHT, ROW_GAP };
