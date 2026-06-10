using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using WorkoutDrop.Core;
using WorkoutDrop.Data;
using WorkoutDrop.UI.Components;

namespace WorkoutDrop.UI.Screens
{
    /// <summary>Home tab. Mirrors <c>app/(tabs)/index.tsx</c>.</summary>
    public class HomeScreen : ScreenController
    {
        public override ScreenId Id => ScreenId.Home;

        protected override void OnBuild(VisualElement root)
        {
            // Fresh-user guard, exactly like the web Home screen.
            var settings = Ctx.Store.GetSettings();
            if (!settings.firstLaunchDone)
            {
                Ctx.Router.ReplaceRoot(ScreenId.Welcome);
                return;
            }

            Q<ScrollView>("scroll").HideScrollbars();
            var content = Q("content");
            content.Clear();

            var workouts = Ctx.Store.GetWorkouts();
            int streak = ProgressMath.ComputeStreakDays(workouts);
            int week = ProgressMath.WorkoutsInLast7Days(workouts);
            int total = workouts.Count;

            // Hero
            var heroLabel = new VisualElement();
            heroLabel.AddToClassList("hero-label");
            heroLabel.Add(Icons.Create("flash", 14, Palette.PrimaryCyan));
            var heroText = new Label("TODAY'S DROP");
            heroText.AddToClassList("hero-label__text");
            heroLabel.Add(heroText);
            content.Add(heroLabel);

            var title = new Label("What will the\nboard give you?");
            title.AddToClassList("hero-title");
            content.Add(title);

            // Plinko preview
            var wrap = new VisualElement();
            wrap.AddToClassList("plinko-wrap");
            wrap.Add(new PlinkoBoardElement(Config, Config.plinko, Ctx.Rng, staticPreview: true));
            content.Add(wrap);

            var cta = UIUtils.TactileButton("Drop For Workout", "arrow-down-circle", UIUtils.ButtonVariant.Primary,
                () => Ctx.Router.Push(ScreenId.Drop));
            cta.style.marginTop = 18;
            content.Add(cta);

            // Stats
            content.Add(StatsRow(
                ("Streak", $"{streak}d", Palette.PrimaryCyan),
                ("This Week", $"{week}/7", Palette.NeonGreen),
                ("Total", total.ToString(), Palette.Strength)));

            // Recent header
            var recentHeader = new VisualElement();
            recentHeader.AddToClassList("recent-header");
            var rhTitle = new Label("Recent Workouts");
            rhTitle.AddToClassList("section-title");
            rhTitle.style.marginTop = 0;
            rhTitle.style.marginBottom = 0;
            recentHeader.Add(rhTitle);
            if (workouts.Count > 0)
            {
                var count = new Label(workouts.Count.ToString());
                count.AddToClassList("dim");
                count.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
                recentHeader.Add(count);
            }
            content.Add(recentHeader);

            if (workouts.Count == 0)
            {
                var empty = new VisualElement();
                empty.AddToClassList("empty-card");
                empty.Add(Icons.Create("moon", 28, Palette.Hex("#52525B")));
                var et = new Label("No workouts yet");
                et.AddToClassList("empty-card__title");
                empty.Add(et);
                var eb = new Label("Drop the ball above to roll your first program.");
                eb.AddToClassList("empty-card__body");
                empty.Add(eb);
                content.Add(empty);
            }
            else
            {
                int shown = Math.Min(5, workouts.Count);
                for (int i = 0; i < shown; i++)
                    content.Add(RecentCard(workouts[i]));
            }
        }

        private VisualElement RecentCard(Workout w)
        {
            var card = new VisualElement();
            card.AddToClassList("recent-card");
            card.SetBorderColor(Palette.CategoryColor(w.programType));

            var dot = new VisualElement();
            dot.AddToClassList("recent-dot");
            dot.style.backgroundColor = Palette.CategoryColor(w.programType);
            card.Add(dot);

            var col = new VisualElement();
            col.style.flexGrow = 1;
            var t = new Label(w.programType.ToId().ToUpperInvariant());
            t.AddToClassList("recent-title");
            col.Add(t);
            var date = TimeUtil.ToLocal(w.dateMs);
            var sub = new Label($"{date:ddd, MMM d}  •  {w.setsCompleted}/{w.setsCompleted + w.setsSkipped} sets");
            sub.AddToClassList("recent-sub");
            col.Add(sub);
            card.Add(col);

            if (w.badges.Count > 0)
            {
                var chip = new VisualElement();
                chip.AddToClassList("badge-chip");
                chip.Add(Icons.Create("trophy", 12, Palette.Gold));
                var ct = new Label(w.badges.Count.ToString());
                ct.AddToClassList("badge-chip__text");
                chip.Add(ct);
                card.Add(chip);
            }
            return card;
        }

        internal static VisualElement StatsRow(params (string title, string value, UnityEngine.Color accent)[] tiles)
        {
            var row = new VisualElement();
            row.AddToClassList("stats-row");
            row.style.marginTop = 24;
            for (int i = 0; i < tiles.Length; i++)
            {
                if (i > 0)
                {
                    var gap = new VisualElement();
                    gap.AddToClassList("stats-gap");
                    row.Add(gap);
                }
                row.Add(UIUtils.StatTile(tiles[i].title, tiles[i].value, tiles[i].accent));
            }
            return row;
        }
    }
}
