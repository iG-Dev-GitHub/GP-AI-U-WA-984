using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using WorkoutDrop.Core;
using WorkoutDrop.Data;

namespace WorkoutDrop.UI.Components
{
    /// <summary>
    /// The Plinko board. Faithful port of <c>src/components/PlinkoBoard.tsx</c> + the drop
    /// animation in <c>app/drop.tsx</c>: a triangle of neon pegs, a 7-cell program strip and an
    /// animated kettlebell that bounces down a jittered path toward the chosen cell.
    /// Layout is rebuilt from the resolved width so it is fully responsive.
    /// </summary>
    public class PlinkoBoardElement : VisualElement
    {
        private const float PegSize = 6f;
        private const float CellHeight = 56f;
        private const float CellInner = 48f;
        private const float RowGap = 22f;

        private readonly AppConfig _config;
        private readonly PlinkoConfig _plinko;
        private readonly IRng _rng;
        private readonly bool _staticPreview;

        private VisualElement _beastOverlay;
        private VisualElement _pegLayer;
        private VisualElement _cellsRow;
        private VisualElement _ball;
        private readonly List<VisualElement> _cells = new List<VisualElement>();

        private float _boardWidth;
        private bool _beastMode;
        private bool _ballOnFire;
        private int _highlight = -1;

        private IVisualElementScheduledItem _glow;
        private IVisualElementScheduledItem _dropAnim;

        public PlinkoBoardElement(AppConfig config, PlinkoConfig plinko, IRng rng, bool staticPreview)
        {
            _config = config;
            _plinko = plinko;
            _rng = rng;
            _staticPreview = staticPreview;
            _plinko.EnsurePopulated();

            AddToClassList("plinko-board");
            style.flexGrow = 0;

            _beastOverlay = new VisualElement();
            _beastOverlay.AddToClassList("plinko-beast-overlay");
            _beastOverlay.pickingMode = PickingMode.Ignore;
            _beastOverlay.style.opacity = 0;
            Add(_beastOverlay);

            _pegLayer = new VisualElement { pickingMode = PickingMode.Ignore };
            _pegLayer.style.position = Position.Absolute;
            _pegLayer.style.left = 0; _pegLayer.style.top = 0; _pegLayer.style.right = 0; _pegLayer.style.bottom = 0;
            Add(_pegLayer);

            _cellsRow = new VisualElement();
            _cellsRow.AddToClassList("plinko-cells");
            Add(_cellsRow);

            if (!_staticPreview)
            {
                _ball = new VisualElement();
                _ball.AddToClassList("plinko-ball");
                _ball.style.opacity = 0;
                _ball.pickingMode = PickingMode.Ignore;
                Add(_ball);
                SetBallSprite(false);
            }

            RegisterCallback<GeometryChangedEvent>(OnGeometry);

            // Looping glow for beast overlay.
            float t = 0;
            _glow = schedule.Execute(() =>
            {
                t += 0.05f;
                float v = (Mathf.Sin(t) + 1f) / 2f; // 0..1
                if (_beastMode) _beastOverlay.style.opacity = 0.5f + 0.3f * v;
            }).Every(70);
        }

        private void OnGeometry(GeometryChangedEvent evt)
        {
            float w = resolvedStyle.width;
            if (w <= 0 || Mathf.Approximately(w, _boardWidth)) return;
            _boardWidth = w;
            Rebuild();
        }

        private void Rebuild()
        {
            int rows = _plinko.plinkoRows;
            float boardHeight = rows * RowGap + CellHeight + 24f;
            style.height = boardHeight;

            BuildPegs(rows);
            BuildCells();
            ApplyHighlight();
            if (_ball != null) PositionBall(_boardWidth / 2f, 0f);
        }

        private void BuildPegs(int rows)
        {
            _pegLayer.Clear();
            for (int row = 0; row < rows; row++)
            {
                int cols = row + 3; // 3 pegs top row, widening
                float spacing = _boardWidth / (cols + 1);
                for (int c = 1; c <= cols; c++)
                {
                    bool green = (row + c) % 2 == 0;
                    var peg = new VisualElement();
                    peg.AddToClassList("plinko-peg");
                    peg.style.left = spacing * c - PegSize / 2f;
                    peg.style.top = 24f + row * RowGap - PegSize / 2f;
                    peg.style.backgroundColor = _beastMode ? Palette.PegBeast : (green ? Palette.PegGreen : Palette.PegBlue);
                    _pegLayer.Add(peg);
                }
            }
        }

        private string IconForProgram(ProgramType p)
        {
            switch (p)
            {
                case ProgramType.Recovery: return "leaf";
                case ProgramType.Cardio: return "pulse";
                case ProgramType.Strength: return "barbell";
                default: return "flame";
            }
        }

        private void BuildCells()
        {
            _cellsRow.Clear();
            _cells.Clear();
            int rows = _plinko.plinkoRows;
            int n = _plinko.CellCount;
            float cellWidth = _boardWidth / n;
            _cellsRow.style.top = 24f + rows * RowGap;
            _cellsRow.style.height = CellHeight;

            for (int i = 0; i < n; i++)
            {
                var program = _plinko.ProgramAt(i);
                var cell = new VisualElement();
                cell.AddToClassList("plinko-cell");
                cell.style.width = cellWidth - 4f;
                cell.style.height = CellInner;
                cell.SetBorderColor(Palette.CategoryColor(program));
                cell.style.backgroundColor = Palette.CategoryColorDark(program);

                var icon = Icons.Create(IconForProgram(program), 20, Palette.White);
                cell.Add(icon);

                _cellsRow.Add(cell);
                _cells.Add(cell);
            }
        }

        // ----- public API -----
        public void SetBeastMode(bool on)
        {
            _beastMode = on;
            EnableInClassList("plinko-board--beast", on);
            if (!on) _beastOverlay.style.opacity = 0;
            if (_boardWidth > 0) BuildPegs(_plinko.plinkoRows);
        }

        public void SetBallFire(bool on)
        {
            _ballOnFire = on;
            if (_ball != null)
            {
                SetBallSprite(on);
                _ball.EnableInClassList("plinko-ball--fire", on);
            }
        }

        private void SetBallSprite(bool fire)
        {
            var sprite = _config.Kettlebell(fire);
            if (sprite != null) _ball.style.backgroundImage = new StyleBackground(sprite);
        }

        public void HighlightCell(int index)
        {
            _highlight = index;
            ApplyHighlight();
        }

        public void ClearHighlight()
        {
            _highlight = -1;
            ApplyHighlight();
        }

        private void ApplyHighlight()
        {
            for (int i = 0; i < _cells.Count; i++)
            {
                var program = _plinko.ProgramAt(i);
                bool hi = i == _highlight;
                var cell = _cells[i];
                cell.style.backgroundColor = hi ? Palette.CategoryColor(program) : Palette.CategoryColorDark(program);
                var icon = cell.Q<Label>(className: "icon");
                if (icon != null) icon.style.color = hi ? Palette.Black : Palette.White;
            }
        }

        public void ShowBall(bool visible)
        {
            if (_ball != null) _ball.style.opacity = visible ? 1 : 0;
        }

        private void PositionBall(float cx, float cy)
        {
            if (_ball == null) return;
            _ball.style.left = cx - 18f; // ball is 36 wide
            _ball.style.top = cy - 18f;
        }

        /// <summary>
        /// Run the bounce-down animation toward <paramref name="cellIndex"/>, then invoke
        /// <paramref name="onLanded"/>. Mirrors the keyframe path built in app/drop.tsx.
        /// </summary>
        public void Drop(int cellIndex, Action onLanded)
        {
            if (_ball == null || _boardWidth <= 0) { onLanded?.Invoke(); return; }

            int rows = _plinko.plinkoRows;
            int n = _plinko.CellCount;
            float cellWidth = _boardWidth / n;
            float targetX = cellIndex * cellWidth + cellWidth / 2f;
            const float totalDuration = 1400f;
            float perRow = totalDuration / rows;

            // X keyframes (rows + final settle).
            var kx = new List<float>();
            for (int i = 0; i < rows; i++)
            {
                float t = (i + 1f) / rows;
                float baseX = _boardWidth / 2f + (targetX - _boardWidth / 2f) * t;
                float jitter = (_rng.Value01() - 0.5f) * cellWidth * 0.9f;
                kx.Add(Mathf.Clamp(baseX + jitter, 16f, _boardWidth - 16f));
            }
            kx.Add(targetX);

            // Y keyframes.
            var ky = new List<float>();
            for (int i = 1; i <= rows; i++) ky.Add(24f + i * RowGap);
            ky.Add(24f + rows * RowGap + 24f);

            // Build aligned segments (rows + 1 segments). Start at (W/2, 0).
            int segCount = kx.Count; // == rows + 1 == ky.Count
            var segDur = new float[segCount];
            for (int i = 0; i < segCount; i++) segDur[i] = (i == segCount - 1) ? 300f : perRow;

            float startX = _boardWidth / 2f;
            float startY = 0f;
            ShowBall(true);
            PositionBall(startX, startY);

            // Cumulative timings.
            var segStart = new float[segCount];
            float acc = 0;
            for (int i = 0; i < segCount; i++) { segStart[i] = acc; acc += segDur[i]; }
            float total = acc;

            float elapsed = 0;
            _dropAnim?.Pause();
            float lastTime = Time.realtimeSinceStartup;
            _dropAnim = schedule.Execute(() =>
            {
                float now = Time.realtimeSinceStartup;
                elapsed += (now - lastTime) * 1000f;
                lastTime = now;

                if (elapsed >= total)
                {
                    PositionBall(targetX, ky[segCount - 1]);
                    _dropAnim.Pause();
                    onLanded?.Invoke();
                    return;
                }

                // Find current segment.
                int seg = 0;
                while (seg < segCount - 1 && elapsed >= segStart[seg] + segDur[seg]) seg++;
                float segT = Mathf.Clamp01((elapsed - segStart[seg]) / segDur[seg]);

                float prevX = seg == 0 ? startX : kx[seg - 1];
                float prevY = seg == 0 ? startY : ky[seg - 1];
                float curX = Mathf.Lerp(prevX, kx[seg], segT);                  // X linear
                float curY = Mathf.Lerp(prevY, ky[seg], EaseInCubic(segT));     // Y ease-in-cubic
                PositionBall(curX, curY);
            }).Every(16);
        }

        private static float EaseInCubic(float t) => t * t * t;

        public void StopAnimations()
        {
            _glow?.Pause();
            _dropAnim?.Pause();
        }
    }
}
