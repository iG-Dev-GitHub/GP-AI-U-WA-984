using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using WorkoutDrop.Core;
using WorkoutDrop.Data;
using WorkoutDrop.UI.Components;

namespace WorkoutDrop.UI.Screens
{
    /// <summary>Active workout screen. Mirrors <c>app/workout.tsx</c>.</summary>
    public class WorkoutScreen : ScreenController
    {
        public override ScreenId Id => ScreenId.Workout;

        private const int RestSeconds = 45;

        private Workout _workout;
        private WeightUnit _unit;
        private long _startMs;
        private bool _resting;
        private RestTimerElement _rest;
        private IVisualElementScheduledItem _clock;
        private VisualElement _content;

        protected override void OnBuild(VisualElement root)
        {
            _workout = Ctx.Store.GetCurrentWorkout();
            if (_workout == null)
            {
                Ctx.Router.GoTab(ScreenId.Home);
                return;
            }
            _unit = Ctx.Store.GetSettings().weightUnit;
            _startMs = _workout.startedAtMs > 0 ? _workout.startedAtMs : TimeUtil.NowMs();

            var back = Q("back");
            back.Add(Icons.Create("chevron-back", 22, Palette.White));
            back.OnClick(() => Ctx.Router.Back());

            var info = Config.programs.Info(_workout.programType);
            Q<Label>("eyebrow").text = $"{_workout.programType.ToId().ToUpperInvariant()} • {info.durationMin}M";

            var chip = Q("progress-chip");
            chip.SetBorderColor(_workout.programType == ProgramType.Beast ? Palette.Beast : Palette.PrimaryCyan);

            Q<ScrollView>("scroll").HideScrollbars();
            _content = Q("content");

            RenderCards();
            UpdateHeader();

            _clock = root.schedule.Execute(UpdateHeader).Every(1000);
        }

        private void UpdateHeader()
        {
            int elapsed = (int)((TimeUtil.NowMs() - _startMs) / 1000);
            var time = Q<Label>("time");
            if (time != null) time.text = UIUtils.FormatClock(elapsed);

            int total = WorkoutBuilder.CountTotalSets(_workout);
            int done = CountDone();
            var pt = Q<Label>("progress-text");
            if (pt != null) pt.text = $"{done}/{total}";
        }

        private int CountDone()
        {
            int n = 0;
            foreach (var e in _workout.exercises)
                foreach (var s in e.sets)
                    if (s.completed) n++;
            return n;
        }

        private int CountSkipped()
        {
            int n = 0;
            foreach (var e in _workout.exercises)
                foreach (var s in e.sets)
                    if (s.skipped) n++;
            return n;
        }

        private void Persist() => Ctx.Store.SetCurrentWorkout(_workout);

        private void RenderCards()
        {
            _content.Clear();

            for (int exIdx = 0; exIdx < _workout.exercises.Count; exIdx++)
            {
                var ex = _workout.exercises[exIdx];
                int capturedEx = exIdx;
                var card = ExerciseCardBuilder.Build(
                    ex.name, ex.category, ex.sets, _unit, ex.bonus, ex.prAchieved,
                    (setIdx, s) => SetActions(capturedEx, setIdx, s));
                _content.Add(card);
            }

            int total = WorkoutBuilder.CountTotalSets(_workout);
            int done = CountDone();
            int skipped = CountSkipped();
            int pending = total - done - skipped;

            var finish = UIUtils.TactileButton(
                pending == 0 ? "Finish Workout" : $"Finish ({pending} pending)",
                "trophy",
                _workout.programType == ProgramType.Beast ? UIUtils.ButtonVariant.Beast : UIUtils.ButtonVariant.Primary,
                FinishWorkout);
            finish.style.marginTop = 18;
            finish.style.marginBottom = _resting ? 80 : 0;
            _content.Add(finish);
        }

        private VisualElement SetActions(int exIdx, int setIdx, WorkoutSet s)
        {
            if (s.completed)
            {
                var done = new VisualElement();
                done.AddToClassList("status-done");
                done.Add(Icons.Create("checkmark-circle", 20, Palette.NeonGreen));
                return done;
            }
            if (s.skipped)
            {
                var skip = new VisualElement();
                skip.AddToClassList("status-skip");
                skip.Add(Icons.Create("close-circle", 20, Palette.Beast));
                return skip;
            }

            var row = new VisualElement();
            row.AddToClassList("set-actions");

            row.Add(Stepper("R", () => AdjustReps(exIdx, setIdx, -1), () => AdjustReps(exIdx, setIdx, 1)));

            bool showWeight = s.weight > 0 || s.durationSec == 0;
            if (showWeight)
                row.Add(Stepper("W", () => AdjustWeight(exIdx, setIdx, -2.5f), () => AdjustWeight(exIdx, setIdx, 2.5f)));

            var log = new Button { text = string.Empty };
            log.AddToClassList("log-btn");
            log.Add(Icons.Create("checkmark", 16, Palette.Black));
            var logText = new Label("LOG");
            logText.AddToClassList("log-btn__text");
            log.Add(logText);
            log.clicked += () => LogSet(exIdx, setIdx);
            row.Add(log);

            var skipBtn = new Button { text = string.Empty };
            skipBtn.AddToClassList("skip-btn");
            skipBtn.Add(Icons.Create("flame", 14, Palette.Beast));
            skipBtn.clicked += () => SkipSet(exIdx, setIdx);
            row.Add(skipBtn);

            return row;
        }

        private VisualElement Stepper(string letter, System.Action minus, System.Action plus)
        {
            var group = new VisualElement();
            group.AddToClassList("stepper-group");

            var minusBtn = new Button { text = string.Empty };
            minusBtn.AddToClassList("step-btn");
            minusBtn.Add(Icons.Create("remove", 14, Palette.White));
            minusBtn.clicked += minus;
            group.Add(minusBtn);

            var lbl = new Label(letter);
            lbl.AddToClassList("stepper-label");
            group.Add(lbl);

            var plusBtn = new Button { text = string.Empty };
            plusBtn.AddToClassList("step-btn");
            plusBtn.Add(Icons.Create("add", 14, Palette.White));
            plusBtn.clicked += plus;
            group.Add(plusBtn);

            return group;
        }

        private void LogSet(int exIdx, int setIdx)
        {
            _workout.exercises[exIdx].sets[setIdx].completed = true;
            _workout.exercises[exIdx].sets[setIdx].skipped = false;
            Persist();
            _resting = true;
            RenderCards();
            UpdateHeader();
            ShowRest();
        }

        private void SkipSet(int exIdx, int setIdx)
        {
            var set = _workout.exercises[exIdx].sets[setIdx];
            set.completed = false;
            set.skipped = true;
            Persist();
            RenderCards();
            UpdateHeader();
        }

        private void AdjustReps(int exIdx, int setIdx, int delta)
        {
            var set = _workout.exercises[exIdx].sets[setIdx];
            set.reps = Mathf.Max(0, set.reps + delta);
            Persist();
            RenderCards();
        }

        private void AdjustWeight(int exIdx, int setIdx, float delta)
        {
            var set = _workout.exercises[exIdx].sets[setIdx];
            set.weight = Mathf.Max(0, set.weight + delta);
            Persist();
            RenderCards();
        }

        private void ShowRest()
        {
            var host = Q("rest-host");
            host.Clear();
            _rest = new RestTimerElement(RestSeconds, EndRest, EndRest);
            host.Add(_rest);
        }

        private void EndRest()
        {
            _resting = false;
            var host = Q("rest-host");
            host.Clear();
            _rest = null;
            RenderCards();
        }

        private void FinishWorkout()
        {
            int completed = 0, skipped = 0;
            foreach (var e in _workout.exercises)
                foreach (var s in e.sets)
                {
                    if (s.completed) completed++;
                    else skipped++;
                }

            var prResult = ProgressMath.CheckAndUpdatePRs(_workout.exercises, Ctx.Store.GetPRs());
            Ctx.Store.SavePRs(prResult.Updated);
            foreach (var e in _workout.exercises)
                e.prAchieved = prResult.PrExerciseIds.Contains(e.exerciseId);

            var badges = new List<BadgeId>();
            if (skipped == 0 && completed > 0) badges.Add(BadgeId.FullDrop);
            if (_workout.programType == ProgramType.Beast && skipped == 0 && completed > 0) badges.Add(BadgeId.BeastMode);
            if (prResult.PrExerciseIds.Count > 0) badges.Add(BadgeId.Pr);

            _workout.finishedAtMs = TimeUtil.NowMs();
            _workout.totalElapsedSec = (int)((TimeUtil.NowMs() - _startMs) / 1000);
            _workout.completed = true;
            _workout.setsCompleted = completed;
            _workout.setsSkipped = skipped;
            _workout.badges = badges;

            // Iron Week: >= 5 finished workouts in the last 7 days including this one.
            var prior = Ctx.Store.GetWorkouts();
            var combined = new List<Workout> { _workout };
            combined.AddRange(prior);
            if (ProgressMath.WorkoutsInLast7Days(combined) >= 5 && !_workout.badges.Contains(BadgeId.IronWeek))
                _workout.badges.Add(BadgeId.IronWeek);

            Ctx.Store.AddWorkout(_workout);
            Ctx.Store.ClearCurrentWorkout();
            Ctx.Store.SetLastSummary(_workout);
            Ctx.Router.Replace(ScreenId.Summary);
        }

        public override void OnDestroyScreen()
        {
            _clock?.Pause();
            _rest?.Stop();
        }
    }
}
