import React from "react";
import { Image, StyleSheet, Text, View } from "react-native";

import { BadgeId } from "@/src/data/types";

const BADGE_META: Record<
  BadgeId,
  { label: string; color: string; image: number }
> = {
  full_drop: {
    label: "Full Drop",
    color: "#FFD700",
    image: require("../../assets/images/plinko/badge_full_drop.png"),
  },
  beast_mode: {
    label: "Beast Mode",
    color: "#FF3B30",
    image: require("../../assets/images/plinko/badge_beast_mode.png"),
  },
  iron_week: {
    label: "Iron Week",
    color: "#00D1FF",
    image: require("../../assets/images/plinko/badge_iron_week.png"),
  },
  pr: {
    label: "Personal Record",
    color: "#FFD700",
    image: require("../../assets/images/plinko/badge_pr.png"),
  },
};

interface Props {
  id: BadgeId;
  earned?: boolean;
  size?: "sm" | "md" | "lg";
  testID?: string;
}

export default function BadgeView({
  id,
  earned = true,
  size = "md",
  testID,
}: Props) {
  const meta = BADGE_META[id];
  const dim = size === "lg" ? 120 : size === "md" ? 96 : 72;
  return (
    <View testID={testID ?? `badge-${id}`} style={{ alignItems: "center" }}>
      <View
        style={[
          styles.circle,
          {
            width: dim,
            height: dim,
            borderRadius: dim / 2,
            shadowColor: meta.color,
            opacity: earned ? 1 : 0.35,
          },
        ]}
      >
        <Image
          source={meta.image}
          style={{ width: dim, height: dim }}
          resizeMode="contain"
        />
      </View>
      <Text
        style={[
          styles.label,
          { color: meta.color, opacity: earned ? 1 : 0.5 },
        ]}
      >
        {meta.label}
      </Text>
    </View>
  );
}

const styles = StyleSheet.create({
  circle: {
    alignItems: "center",
    justifyContent: "center",
    shadowOpacity: 0.6,
    shadowRadius: 14,
    shadowOffset: { width: 0, height: 0 },
    elevation: 4,
  },
  label: {
    marginTop: 8,
    fontWeight: "900",
    letterSpacing: 1.2,
    fontSize: 11,
    textTransform: "uppercase",
  },
});

export { BADGE_META };
