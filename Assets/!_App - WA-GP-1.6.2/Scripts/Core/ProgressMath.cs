using System;
using System.Collections.Generic;

namespace WorkoutDrop.Core
{
    /// <summary>
    /// PR tracking, streak and weekly counts. Faithful port of the corresponding helpers
    /// in <c>src/data/store.ts</c> (checkAndUpdatePRs, computeStreakDays, workoutsInLast7Days).
    /// </summary>
    public static class ProgressMath
    {
        public struct PRResult
        {
            public List<PREntry> Updated;
            public HashSet<string> PrExerciseIds;
        }

        /// <summary>
        /// Recompute PRs from completed sets. A PR is only flagged when a prior baseline
        /// existed (so the very first time you do an exercise is not a PR). Mirrors web.
        /// </summary>
        public static PRResult CheckAndUpdatePRs(List<WorkoutExercise> exercises, List<PREntry> existing)
        {
            var map = new Dictionary<string, PRRecord>();
            foreach (var e in existing)
                map[e.exerciseId] = e.record != null ? e.record.Clone() : new PRRecord();

            var prIds = new HashSet<string>();

            foreach (var we in exercises)
            {
                PRRecord prev = map.TryGetValue(we.exerciseId, out var r) ? r : new PRRecord();
                PRRecord next = prev.Clone();
                bool isPR = false;

                foreach (var s in we.sets)
                {
                    if (!s.completed) continue;
                    if (s.weight > next.maxWeight) { next.maxWeight = s.weight; isPR = true; }
                    if (s.reps > next.maxReps) { next.maxReps = s.reps; isPR = true; }
                    if (s.durationSec > 0 && s.durationSec > next.maxDurationSec) { next.maxDurationSec = s.durationSec; isPR = true; }
                }

                bool hadBaseline = prev.maxWeight + prev.maxReps + prev.maxDurationSec > 0;
                if (isPR && hadBaseline) prIds.Add(we.exerciseId);
                map[we.exerciseId] = next;
            }

            var updated = new List<PREntry>();
            foreach (var kv in map)
                updated.Add(new PREntry { exerciseId = kv.Key, record = kv.Value });

            return new PRResult { Updated = updated, PrExerciseIds = prIds };
        }

        /// <summary>Consecutive day streak ending today, looking back up to 30 days. Mirrors web.</summary>
        public static int ComputeStreakDays(List<Workout> workouts)
        {
            var days = new HashSet<string>();
            foreach (var w in workouts)
            {
                if (w.finishedAtMs == 0) continue;
                days.Add(DayKey(TimeUtil.ToLocal(w.finishedAtMs)));
            }

            int streak = 0;
            DateTime today = DateTime.Now.Date;
            for (int i = 0; i < 30; i++)
            {
                DateTime d = today.AddDays(-i);
                string key = DayKey(d);
                if (days.Contains(key)) streak++;
                else if (i > 0) break;
            }
            return streak;
        }

        public static int WorkoutsInLast7Days(List<Workout> workouts)
        {
            long cutoff = TimeUtil.NowMs() - 7L * 24 * 60 * 60 * 1000;
            int count = 0;
            foreach (var w in workouts)
                if (w.finishedAtMs != 0 && w.finishedAtMs >= cutoff) count++;
            return count;
        }

        public static string DayKey(DateTime d) => d.ToString("yyyy-MM-dd");
    }
}
