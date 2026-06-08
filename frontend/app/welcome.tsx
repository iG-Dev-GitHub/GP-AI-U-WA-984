import { Ionicons } from "@expo/vector-icons";
import { useRouter } from "expo-router";
import { useState } from "react";
import {
  Image,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import TactileButton from "@/src/components/TactileButton";
import { getSettings, saveSettings } from "@/src/data/store";

const SLIDES = [
  {
    key: "1",
    title: "Set Up Your Exercises",
    body: "Build your gym. Pick from our starter pack or roll your own.",
    icon: "barbell" as const,
    accent: "#00D1FF",
    image: null as number | null,
  },
  {
    key: "2",
    title: "Drop The Ball",
    body: "Each day, drop the kettlebell on the Plinko board. Where it lands is your workout.",
    icon: "ellipse" as const,
    accent: "#00FF7A",
    image: require("../assets/images/plinko/kettlebell.png"),
  },
  {
    key: "3",
    title: "Hit Beast Mode",
    body: "Edges of the board mean HIIT. Push limits, unlock badges, build streaks.",
    icon: "flame" as const,
    accent: "#FF3B30",
    image: require("../assets/images/plinko/kettlebell_fire.png"),
  },
];

export default function Welcome() {
  const router = useRouter();
  const [page, setPage] = useState(0);

  const next = () => {
    if (page < SLIDES.length - 1) setPage(page + 1);
    else finish();
  };

  const finish = async () => {
    const s = await getSettings();
    await saveSettings({ ...s, firstLaunchDone: true });
    router.replace("/");
  };

  const slide = SLIDES[page];
  const last = page === SLIDES.length - 1;

  return (
    <SafeAreaView style={styles.root} edges={["top", "bottom"]}>
      <View style={styles.skipRow}>
        <View
          testID="welcome-dots"
          style={{ flexDirection: "row", alignItems: "center" }}
        >
          {SLIDES.map((_, i) => (
            <View
              key={i}
              style={[
                styles.dot,
                { backgroundColor: i === page ? "#FFF" : "#3F3F46" },
              ]}
            />
          ))}
        </View>
        <Text
          testID="welcome-skip"
          onPress={finish}
          style={styles.skip}
        >
          SKIP
        </Text>
      </View>

      <View style={styles.slide}>
        <View
          style={[
            styles.iconWrap,
            { borderColor: slide.accent, shadowColor: slide.accent },
          ]}
        >
          {slide.image ? (
            <Image
              source={slide.image}
              style={{ width: 160, height: 160 }}
              resizeMode="contain"
            />
          ) : (
            <Ionicons name={slide.icon} size={130} color={slide.accent} />
          )}
        </View>
        <Text style={styles.label}>STEP {page + 1}</Text>
        <Text testID="welcome-title" style={styles.title}>
          {slide.title}
        </Text>
        <Text style={styles.body}>{slide.body}</Text>
      </View>

      <View style={styles.ctaWrap}>
        <TactileButton
          testID="welcome-cta"
          title={last ? "Build My Gym" : "Next"}
          icon={last ? "construct" : "arrow-forward"}
          variant={last ? "primary" : "secondary"}
          onPress={next}
        />
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  root: { flex: 1, backgroundColor: "#050505" },
  skipRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    paddingHorizontal: 24,
    paddingTop: 12,
  },
  dot: {
    width: 10,
    height: 10,
    borderRadius: 5,
    marginRight: 6,
  },
  skip: {
    color: "#A1A1AA",
    fontWeight: "900",
    letterSpacing: 2,
    fontSize: 13,
  },
  slide: {
    flex: 1,
    alignItems: "center",
    justifyContent: "center",
    paddingHorizontal: 32,
  },
  iconWrap: {
    width: 220,
    height: 220,
    borderRadius: 110,
    borderWidth: 3,
    alignItems: "center",
    justifyContent: "center",
    marginBottom: 32,
    backgroundColor: "#0A0A0C",
    shadowOpacity: 0.5,
    shadowRadius: 24,
    shadowOffset: { width: 0, height: 0 },
    elevation: 6,
  },
  label: {
    color: "#52525B",
    fontWeight: "900",
    letterSpacing: 4,
    fontSize: 11,
    marginBottom: 8,
  },
  title: {
    color: "#FFF",
    fontWeight: "900",
    fontSize: 28,
    textAlign: "center",
    letterSpacing: -0.5,
    marginBottom: 12,
  },
  body: {
    color: "#A1A1AA",
    fontSize: 15,
    textAlign: "center",
    lineHeight: 22,
  },
  ctaWrap: {
    paddingHorizontal: 24,
    paddingBottom: 8,
  },
});
