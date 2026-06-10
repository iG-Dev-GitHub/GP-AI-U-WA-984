using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using WorkoutDrop.Core;
using WorkoutDrop.Data;
using WorkoutDrop.UI.Components;

namespace WorkoutDrop.UI.Screens
{
    /// <summary>Progress tab. Mirrors <c>app/(tabs)/progress.tsx</c>.</summary>
    public class ProgressScreen : ScreenController
    {
        public override ScreenId Id => ScreenId.Progress;

        private static readonly BadgeId[] AllBadges = { BadgeId.FullDrop, BadgeId.BeastMode, BadgeId.IronWeek, BadgeId.Pr };
        private static readonly ProgramType[] Programs = { ProgramType.Recovery, ProgramType.Cardio, ProgramType.Strength, ProgramType.Beast };

        protected override void OnBuild(VisualElement root)
        {
            Q<ScrollView>("scroll").HideScrollbars();
            var content = Q("content");
            content.Clear();

            var workouts = Ctx.Store.GetWorkouts();
            int streak = ProgressMath.ComputeStreakDays(workouts);
            int total = workouts.Count;
            int week = ProgressMath.WorkoutsInLast7Days(workouts);

            content.Add(Eyebrow("YOUR JOURNEY"));
            content.Add(H1("Progress"));

            var stats = HomeScreen.StatsRow(
                ("Streak", $"{streak}d", Palette.PrimaryCyan),
                ("Total", total.ToString(), Palette.Strength),
                ("Week", $"{week}/7", Palette.NeonGreen));
            stats.style.marginTop = 0;
            content.Add(stats);

            // Calendar (last 28 days)
            content.Add(Section("Calendar"));
            var workoutDays = new HashSet<string>();
            foreach (var w in workouts)
                if (w.finishedAtMs != 0) workoutDays.Add(ProgressMath.DayKey(TimeUtil.ToLocal(w.finishedAtMs)));

            var grid = new VisualElement();
            grid.AddToClassList("calendar-grid");
            DateTime today = DateTime.Now.Date;
            for (int i = 27; i >= 0; i--)
            {
                DateTime d = today.AddDays(-i);
                bool isWorkout = workoutDays.Contains(ProgressMath.DayKey(d));
                var cell = new VisualElement();
                cell.AddToClassList("cal-cell");
                if (isWorkout) cell.AddToClassList("cal-cell--active");
                var lbl = new Label(d.Day.ToString());
                lbl.AddToClassList("cal-label");
                if (isWorkout) lbl.style.color = Palette.Black;
                cell.Add(lbl);
                grid.Add(cell);
            }
            content.Add(grid);

            // Workouts by type
            content.Add(Section("Workouts By Type"));
            var counts = new Dictionary<ProgramType, int>();
            foreach (var p in Programs) counts[p] = 0;
            foreach (var w in workouts) counts[w.programType] = counts.TryGetValue(w.programType, out var c) ? c + 1 : 1;
            int maxCount = 1;
            foreach (var p in Programs) maxCount = Math.Max(maxCount, counts[p]);

            var chart = new VisualElement();
            chart.AddToClassList("chart-card");
            foreach (var p in Programs)
            {
                var barRow = new VisualElement();
                barRow.AddToClassList("bar-row");

                var label = new Label(p.ToId().ToUpperInvariant());
                label.AddToClassList("bar-label");
                barRow.Add(label);

                var track = new VisualElement();
                track.AddToClassList("bar-track");
                var fill = new VisualElement();
                fill.AddToClassList("bar-fill");
                fill.style.width = Length.Percent((float)counts[p] / maxCount * 100f);
                fill.style.backgroundColor = Palette.CategoryColor(p);
                track.Add(fill);
                barRow.Add(track);

                var val = new Label(counts[p].ToString());
                val.AddToClassList("bar-val");
                barRow.Add(val);

                chart.Add(barRow);
            }
            content.Add(chart);

            // Badges
            content.Add(Section("Badges"));
            var earned = new HashSet<BadgeId>();
            foreach (var w in workouts) foreach (var b in w.badges) earned.Add(b);

            var badgesGrid = new VisualElement();
            badgesGrid.AddToClassList("badges-grid");
            foreach (var b in AllBadges)
            {
                var wrap = new VisualElement();
                wrap.style.width = Length.Percent(48);
                wrap.style.alignItems = Align.Center;
                wrap.Add(BadgeBuilder.Build(Config, b, earned.Contains(b), "sm"));
                badgesGrid.Add(wrap);
            }
            content.Add(badgesGrid);

            // History
            content.Add(Section("History"));
            if (workouts.Count == 0)
            {
                var empty = new VisualElement();
                empty.AddToClassList("empty-dashed");
                empty.Add(Icons.Create("time", 24, Palette.Hex("#52525B")));
                var et = new Label("Your history will show up here.");
                et.AddToClassList("muted");
                et.style.marginTop = 6;
                empty.Add(et);
                content.Add(empty);
            }
            else
            {
                foreach (var w in workouts)
                    content.Add(HistoryCard(w));
            }
        }

        private VisualElement HistoryCard(Workout w)
        {
            var card = new VisualElement();
            card.AddToClassList("history-card");
            card.SetBorderColor(Palette.CategoryColor(w.programType));

            var dot = new VisualElement();
            dot.AddToClassList("recent-dot");
            dot.style.width = 8; dot.style.height = 8;
            dot.style.backgroundColor = Palette.CategoryColor(w.programType);
            card.Add(dot);

            var col = new VisualElement();
            col.style.flexGrow = 1;
            var t = new Label(w.programType.ToId().ToUpperInvariant());
            t.AddToClassList("recent-title");
            col.Add(t);
            var sub = new Label($"{TimeUtil.ToLocal(w.dateMs):g}");
            sub.AddToClassList("recent-sub");
            col.Add(sub);
            card.Add(col);

            var right = new Label($"{w.setsCompleted}/{w.setsCompleted + w.setsSkipped}");
            right.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            right.style.color = Palette.White;
            card.Add(right);
            return card;
        }

        private static Label Eyebrow(string t) { var l = new Label(t); l.AddToClassList("eyebrow"); return l; }
        private static Label H1(string t) { var l = new Label(t); l.AddToClassList("h1"); return l; }
        private static Label Section(string t) { var l = new Label(t); l.AddToClassList("section-title"); return l; }
    }
}
