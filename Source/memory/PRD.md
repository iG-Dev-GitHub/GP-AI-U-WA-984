# Workout Drop Plinko Gym — PRD

## Overview
Offline Android workout tracker with Plinko ball mechanics that determine the day's workout. No auth, no cloud — all state stored locally via AsyncStorage. Built with Expo Router + React Native (Expo SDK 54), styled in the dark "Hacksaw Gaming Plinko" aesthetic.

## Key Features Implemented
- **Welcome / Tutorial**: 3-slide carousel (Set Up Exercises / Drop The Ball / Hit Beast Mode), `Build My Gym` CTA, persists first-launch flag.
- **Home tab**: Static Plinko board hero, `Drop For Workout` CTA, streak / week / total tiles, recent workouts list.
- **Drop screen**: Risk Level toggle (Easy / Beast), live Plinko animation (12 rows, 14 pegs at bottom, weighted random landing), result card with program type → starts workout.
- **Workout screen**: Per-exercise cards with rep/weight steppers, `LOG` and `Skip (fire)` buttons per set, sticky rest timer, planned program duration in header.
- **Summary screen**: Stats (Completed/Skipped/Time), PR list (gold), badges grid, gold flash overlay when PR achieved.
- **Progress tab**: Streak stats, 28-day calendar, workouts-by-type bar chart, badges grid (earned / locked), full history list.
- **Settings tab**: kg / lbs unit toggle, default Risk level, link to Manage Exercises, destructive Reset All Data flow.
- **Exercises CRUD**: Filter chips (All/Recovery/Cardio/Strength/Beast), add / edit / delete with category, reps, weight, optional duration.

## Game Mechanics
- 7 landing cells: `[beast, strength, cardio, recovery, cardio, strength, beast]` colored blue/green/yellow/red.
- Weighted probability per Risk Level (`easy` biases center, `beast` biases edges).
- Program → exercise builder:
  - Recovery: 3 stretches × 1 set
  - Cardio: 3 cardio × 1 set
  - Strength: 4 exercises × 3 sets
  - Beast Mode: 5 exercises × 4 sets + bonus 1.5× last exercise
- Beast Mode hit → red board overlay + fiery kettlebell with flame trail.
- PR detection: any set whose weight / reps / duration exceeds prior best (after baseline exists).

## Badges
- `Full Drop`: all sets completed, no skips.
- `Beast Mode`: completing a Beast Mode program with no skips.
- `Iron Week`: 5 finished workouts in the last 7 days.
- `PR`: at least one personal record set in the workout.

## Assets
All custom AI-generated PNGs via Gemini Nano Banana (Emergent LLM key), stored at `/app/frontend/assets/images/plinko/`:
- `kettlebell.png` (normal steel)
- `kettlebell_fire.png` (Beast Mode flaming)
- `badge_full_drop.png`, `badge_beast_mode.png`, `badge_iron_week.png`, `badge_pr.png`

Pegs are rendered procedurally (neon blue / green).

## Tech / Storage Schema (local)
| Key | Value |
|---|---|
| `wd:exercises` | `Exercise[]` (seeded with 20) |
| `wd:workouts` | `Workout[]` (capped at 200) |
| `wd:settings` | `{ weightUnit, riskLevel, firstLaunchDone }` |
| `wd:prs` | `Record<exerciseId, { maxWeight, maxReps, maxDurationSec }>` |
| `wd:currentWorkout` | active workout JSON |
| `wd:lastSummary` | latest finished workout for summary screen |

## Out of Scope (per user)
- Push notifications (skipped per user choice)
- Cloud sync / auth
- App name displayed on icon or in-screen (intentionally hidden)

## Default User Preferences (per user)
- Weight unit: `kg`
- Risk Level default: `easy`
- Seeded exercise pack: enabled (20 exercises)
