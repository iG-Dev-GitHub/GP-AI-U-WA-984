using System.Collections.Generic;
using UnityEngine.UIElements;
using WorkoutDrop.Core;
using WorkoutDrop.Data;
using WorkoutDrop.UI.Components;

namespace WorkoutDrop.UI.Screens
{
    /// <summary>Post-workout summary. Mirrors <c>app/summary.tsx</c>.</summary>
    public class SummaryScreen : ScreenController
    {
        public override ScreenId Id => ScreenId.Summary;

        protected override void OnBuild(VisualElement root)
        {
            var workout = Ctx.Store.GetLastSummary();
            if (workout == null)
            {
                Ctx.Router.GoTab(ScreenId.Home);
                return;
            }

            Q<ScrollView>("scroll").HideScrollbars();
            var content = Q("content");
            content.Clear();

            var info = Config.programs.Info(workout.programType);
            var prList = new List<WorkoutExercise>();
            foreach (var e in workout.exercises) if (e.prAchieved) prList.Add(e);

            int mins = (workout.totalElapsedSec) / 60;
            int secs = (workout.totalElapsedSec) % 60;

            // Banner
            var banner = new VisualElement();
            banner.AddToClassList("banner");
            banner.SetBorderColor(Palette.CategoryColor(workout.programType));
            var bLabel = new Label($"{workout.programType.ToId().ToUpperInvariant()} COMPLETE");
            bLabel.AddToClassList("banner__label");
            bLabel.style.color = Palette.CategoryColor(workout.programType);
            banner.Add(bLabel);
            var bTitle = new Label(info.label);
            bTitle.AddToClassList("banner__title");
            banner.Add(bTitle);
            long whenMs = workout.finishedAtMs != 0 ? workout.finishedAtMs : workout.dateMs;
            var bSub = new Label($"{TimeUtil.ToLocal(whenMs):g}");
            bSub.AddToClassList("banner__sub");
            banner.Add(bSub);
            content.Add(banner);

            // Stats
            var stats = HomeScreen.StatsRow(
                ("Completed", workout.setsCompleted.ToString(), Palette.NeonGreen),
                ("Skipped", workout.setsSkipped.ToString(), Palette.Beast),
                ("Time", $"{mins}:{secs:00}", Palette.PrimaryCyan));
            stats.style.marginTop = 0;
            stats.style.marginBottom = 18;
            content.Add(stats);

            // PR card
            if (prList.Count > 0)
            {
                var prCard = new VisualElement();
                prCard.AddToClassList("pr-card");
                var header = new VisualElement();
                header.AddToClassList("pr-card__header");
                header.Add(Icons.Create("ribbon", 18, Palette.Gold));
                var ht = new Label("PERSONAL RECORDS");
                ht.AddToClassList("pr-card__header-text");
                header.Add(ht);
                prCard.Add(header);
                foreach (var e in prList)
                {
                    var row = new VisualElement();
                    row.AddToClassList("pr-card__row");
                    row.Add(Icons.Create("trophy", 14, Palette.Gold));
                    var name = new Label(e.name);
                    name.AddToClassList("pr-card__text");
                    row.Add(name);
                    prCard.Add(row);
                }
                content.Add(prCard);
            }

            // Badges
            var section = new Label("Badges Earned");
            section.AddToClassList("section-title");
            content.Add(section);

            if (workout.badges.Count == 0)
            {
                var empty = new VisualElement();
                empty.AddToClassList("empty-dashed");
                var et = new Label("No badges this round — push harder next drop.");
                et.AddToClassList("empty-card__body");
                empty.Add(et);
                content.Add(empty);
            }
            else
            {
                var grid = new VisualElement();
                grid.AddToClassList("badges-grid");
                foreach (var b in workout.badges)
                    grid.Add(BadgeBuilder.Build(Config, b, true, "md"));
                content.Add(grid);
            }

            // Actions
            var home = UIUtils.TactileButton("Back to Home", "home", UIUtils.ButtonVariant.Primary,
                () => Ctx.Router.GoTab(ScreenId.Home));
            home.style.marginTop = 24;
            content.Add(home);

            var again = UIUtils.TactileButton("Drop Again", "arrow-down-circle", UIUtils.ButtonVariant.Secondary,
                () => Ctx.Router.Replace(ScreenId.Drop));
            again.style.marginTop = 10;
            content.Add(again);

            // Gold flash on PR.
            if (prList.Count > 0)
            {
                var flash = Q("gold-flash");
                if (flash != null)
                {
                    flash.experimental.animation
                        .Start(0f, 1f, 250, (e, v) => e.style.opacity = v)
                        .OnCompleted(() =>
                            flash.schedule.Execute(() =>
                                flash.experimental.animation.Start(1f, 0f, 800, (e, v) => e.style.opacity = v)
                            ).StartingIn(400));
                }
            }
        }
    }
}
