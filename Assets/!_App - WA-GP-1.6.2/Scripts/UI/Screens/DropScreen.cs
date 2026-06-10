using UnityEngine;
using UnityEngine.UIElements;
using WorkoutDrop.Core;
using WorkoutDrop.Data;
using WorkoutDrop.UI.Components;

namespace WorkoutDrop.UI.Screens
{
    /// <summary>The Plinko drop screen. Mirrors <c>app/drop.tsx</c>.</summary>
    public class DropScreen : ScreenController
    {
        public override ScreenId Id => ScreenId.Drop;

        private RiskLevel _risk = RiskLevel.Easy;
        private bool _dropping;
        private bool _hasResult;
        private ProgramType _resultProgram;
        private int _resultIdx = -1;
        private bool _beastOverlay;

        private PlinkoBoardElement _board;
        private VisualElement _riskRow;
        private VisualElement _bottom;

        protected override void OnBuild(VisualElement root)
        {
            _risk = Ctx.Store.GetSettings().riskLevel;

            var back = Q("back");
            back.Add(Icons.Create("chevron-back", 22, Palette.White));
            back.OnClick(() => Ctx.Router.Back());

            _riskRow = Q("risk-row");
            _board = new PlinkoBoardElement(Config, Config.plinko, Ctx.Rng, staticPreview: false);
            Q("plinko-wrap").Add(_board);
            _bottom = Q("bottom-host");

            RenderRisk();
            RenderBottom();
        }

        private void RenderRisk()
        {
            _riskRow.Clear();
            _riskRow.Add(RiskChip(RiskLevel.Easy, "leaf", "EASY", Palette.PrimaryCyan));
            _riskRow.Add(RiskChip(RiskLevel.Beast, "flame", "BEAST", Palette.Beast));
        }

        private VisualElement RiskChip(RiskLevel level, string icon, string text, Color accent)
        {
            bool selected = _risk == level;
            var chip = new VisualElement();
            chip.AddToClassList("risk-chip");
            if (selected) { chip.style.backgroundColor = accent; chip.SetBorderColor(accent); }

            Color contentColor = selected ? (level == RiskLevel.Beast ? Palette.White : Palette.Black) : accent;
            chip.Add(Icons.Create(icon, 16, contentColor));

            var label = new Label(text);
            label.AddToClassList("risk-chip__text");
            label.style.color = selected ? (level == RiskLevel.Beast ? Palette.White : Palette.Black) : Palette.White;
            chip.Add(label);

            chip.OnClick(() => PickRisk(level));
            return chip;
        }

        private void PickRisk(RiskLevel level)
        {
            if (_dropping) return;
            _risk = level;
            var s = Ctx.Store.GetSettings();
            s.riskLevel = level;
            Ctx.Store.SaveSettings(s);
            RenderRisk();
            if (!_hasResult) RenderBottom();
        }

        private void RenderBottom()
        {
            _bottom.Clear();
            if (_hasResult)
            {
                var info = Config.programs.Info(_resultProgram);
                var card = new VisualElement();
                card.AddToClassList("result-card");
                card.SetBorderColor(Palette.CategoryColor(_resultProgram));

                var label = new Label("TODAY'S PROGRAM");
                label.AddToClassList("result-label");
                label.style.color = Palette.CategoryColor(_resultProgram);
                card.Add(label);

                var title = new Label(info.label.ToUpperInvariant());
                title.AddToClassList("result-title");
                card.Add(title);

                var sub = new Label($"{info.tagline} • {info.durationMin} min");
                sub.AddToClassList("result-sub");
                card.Add(sub);

                var start = UIUtils.TactileButton("Start Workout", "play",
                    _resultProgram == ProgramType.Beast ? UIUtils.ButtonVariant.Beast : UIUtils.ButtonVariant.Primary,
                    StartWorkout);
                start.style.marginTop = 14;
                card.Add(start);

                _bottom.Add(card);
            }
            else
            {
                var wrap = new VisualElement();
                wrap.style.paddingLeft = 16; wrap.style.paddingRight = 16; wrap.style.paddingBottom = 16;
                string title = _dropping ? "Dropping..." : (_risk == RiskLevel.Beast ? "Drop Beast" : "Drop");
                var btn = UIUtils.TactileButton(title, "arrow-down-circle",
                    _risk == RiskLevel.Beast ? UIUtils.ButtonVariant.Beast : UIUtils.ButtonVariant.Primary, Drop);
                btn.SetDisabled(_dropping);
                wrap.Add(btn);
                _bottom.Add(wrap);
            }
        }

        private void Drop()
        {
            if (_dropping) return;
            _hasResult = false;
            _resultIdx = -1;
            _beastOverlay = false;
            Root.style.backgroundColor = Palette.Bg;
            _board.SetBeastMode(false);
            _board.ClearHighlight();
            _dropping = true;
            RenderBottom();

            int cellIdx = Config.plinko.PickCellIndex(_risk, Ctx.Rng);
            _board.SetBallFire(_risk == RiskLevel.Beast);
            _board.ShowBall(true);
            _board.Drop(cellIdx, () => AnnounceResult(cellIdx));
        }

        private void AnnounceResult(int idx)
        {
            var program = Config.plinko.ProgramAt(idx);
            _resultIdx = idx;
            _resultProgram = program;
            _hasResult = true;
            _dropping = false;
            _board.HighlightCell(idx);

            if (program == ProgramType.Beast)
            {
                _beastOverlay = true;
                Root.style.backgroundColor = Palette.Hex("#160404");
                _board.SetBeastMode(true);
                _board.SetBallFire(true);
            }
            RenderBottom();
        }

        private void StartWorkout()
        {
            if (!_hasResult) return;
            var pool = Ctx.Store.GetExercises();
            var workout = WorkoutBuilder.Build(_resultProgram, _risk, pool, Config.programs, Ctx.Rng);
            workout.startedAtMs = TimeUtil.NowMs();
            Ctx.Store.SetCurrentWorkout(workout);
            Ctx.Router.Replace(ScreenId.Workout);
        }

        public override void OnDestroyScreen()
        {
            _board?.StopAnimations();
        }
    }
}
