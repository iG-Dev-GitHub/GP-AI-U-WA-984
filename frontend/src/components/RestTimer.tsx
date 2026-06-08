import React, { useEffect, useRef, useState } from "react";
import { StyleSheet, Text, View } from "react-native";
import { Ionicons } from "@expo/vector-icons";

import TactileButton from "./TactileButton";

interface Props {
  seconds: number;
  onDone: () => void;
  onSkip: () => void;
}

export default function RestTimer({ seconds, onDone, onSkip }: Props) {
  const [remaining, setRemaining] = useState(seconds);
  const finishedRef = useRef(false);

  useEffect(() => {
    if (finishedRef.current) return;
    if (remaining <= 0) {
      finishedRef.current = true;
      onDone();
      return;
    }
    const t = setTimeout(() => setRemaining((r) => r - 1), 1000);
    return () => clearTimeout(t);
  }, [remaining, onDone]);

  const mm = Math.floor(remaining / 60)
    .toString()
    .padStart(2, "0");
  const ss = (remaining % 60).toString().padStart(2, "0");

  return (
    <View testID="rest-timer" style={styles.wrap}>
      <View style={styles.row}>
        <Ionicons name="hourglass" size={20} color="#00D1FF" />
        <Text style={styles.label}>REST</Text>
      </View>
      <Text style={styles.timeText}>
        {mm}:{ss}
      </Text>
      <TactileButton
        testID="rest-timer-skip"
        title="Skip Rest"
        variant="secondary"
        icon="play-skip-forward"
        onPress={onSkip}
        style={styles.skip}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  wrap: {
    backgroundColor: "#0A0A0C",
    borderTopWidth: 2,
    borderTopColor: "#00D1FF",
    paddingHorizontal: 18,
    paddingVertical: 14,
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    shadowColor: "#00D1FF",
    shadowOpacity: 0.4,
    shadowRadius: 14,
    shadowOffset: { width: 0, height: -4 },
    elevation: 8,
  },
  row: { flexDirection: "row", alignItems: "center" },
  label: {
    color: "#00D1FF",
    fontWeight: "900",
    fontSize: 12,
    letterSpacing: 3,
    marginLeft: 6,
  },
  timeText: {
    color: "#FFF",
    fontSize: 32,
    fontWeight: "900",
    letterSpacing: 2,
  },
  skip: { paddingVertical: 10, paddingHorizontal: 14 },
});
