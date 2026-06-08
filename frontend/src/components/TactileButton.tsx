import React from "react";
import { StyleSheet, Text, TouchableOpacity, View, ViewStyle } from "react-native";
import { Ionicons } from "@expo/vector-icons";

interface Props {
  title: string;
  onPress?: () => void;
  variant?: "primary" | "beast" | "secondary" | "ghost";
  icon?: keyof typeof Ionicons.glyphMap;
  disabled?: boolean;
  testID?: string;
  style?: ViewStyle;
}

const VARIANT_BG: Record<string, string> = {
  primary: "#00D1FF",
  beast: "#FF3B30",
  secondary: "#1A1A1E",
  ghost: "transparent",
};

const VARIANT_BORDER: Record<string, string> = {
  primary: "#0099CC",
  beast: "#B22019",
  secondary: "#27272A",
  ghost: "#27272A",
};

const VARIANT_TEXT: Record<string, string> = {
  primary: "#000",
  beast: "#FFF",
  secondary: "#FFF",
  ghost: "#FFF",
};

export default function TactileButton({
  title,
  onPress,
  variant = "primary",
  icon,
  disabled,
  testID,
  style,
}: Props) {
  return (
    <TouchableOpacity
      testID={testID}
      onPress={onPress}
      disabled={disabled}
      activeOpacity={0.85}
      style={[
        styles.btn,
        {
          backgroundColor: VARIANT_BG[variant],
          borderBottomColor: VARIANT_BORDER[variant],
          borderColor: variant === "ghost" ? VARIANT_BORDER[variant] : "transparent",
          borderWidth: variant === "ghost" ? 2 : 0,
          opacity: disabled ? 0.5 : 1,
        },
        variant === "beast" && styles.beastShadow,
        style,
      ]}
    >
      <View style={styles.row}>
        {icon ? (
          <Ionicons
            name={icon}
            size={20}
            color={VARIANT_TEXT[variant]}
            style={{ marginRight: 8 }}
          />
        ) : null}
        <Text style={[styles.text, { color: VARIANT_TEXT[variant] }]}>
          {title}
        </Text>
      </View>
    </TouchableOpacity>
  );
}

const styles = StyleSheet.create({
  btn: {
    paddingVertical: 16,
    paddingHorizontal: 20,
    borderRadius: 18,
    borderBottomWidth: 5,
    alignItems: "center",
    justifyContent: "center",
  },
  beastShadow: {
    shadowColor: "#FF3B30",
    shadowOpacity: 0.7,
    shadowRadius: 14,
    shadowOffset: { width: 0, height: 0 },
    elevation: 8,
  },
  text: {
    fontWeight: "900",
    fontSize: 16,
    letterSpacing: 2,
    textTransform: "uppercase",
  },
  row: {
    flexDirection: "row",
    alignItems: "center",
  },
});
