import React from "react";
import { StyleSheet, Text, View } from "react-native";
import { LinearGradient } from "expo-linear-gradient";

interface Props {
  title: string;
  value: string | number;
  accent?: string;
  testID?: string;
}

export default function StatTile({ title, value, accent = "#00D1FF", testID }: Props) {
  return (
    <View testID={testID} style={[styles.wrap, { borderColor: accent }]}>
      <LinearGradient
        colors={[`${accent}25`, "transparent"]}
        style={StyleSheet.absoluteFillObject}
      />
      <Text style={[styles.value, { color: accent }]}>{value}</Text>
      <Text style={styles.title}>{title}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  wrap: {
    flex: 1,
    backgroundColor: "#121214",
    borderWidth: 2,
    borderRadius: 18,
    paddingVertical: 18,
    paddingHorizontal: 14,
    overflow: "hidden",
  },
  value: {
    fontSize: 32,
    fontWeight: "900",
    letterSpacing: 1,
  },
  title: {
    color: "#A1A1AA",
    fontSize: 11,
    fontWeight: "800",
    letterSpacing: 2,
    marginTop: 4,
    textTransform: "uppercase",
  },
});
