using System.Collections.Generic;
using System.Globalization;
using UnityEngine.UIElements;
using WorkoutDrop.Core;
using WorkoutDrop.Data;

namespace WorkoutDrop.UI.Screens
{
    /// <summary>Exercise library manager. Mirrors <c>app/exercises.tsx</c>.</summary>
    public class ExercisesScreen : ScreenController
    {
        public override ScreenId Id => ScreenId.Exercises;

        private static readonly ProgramType[] Categories = { ProgramType.Recovery, ProgramType.Cardio, ProgramType.Strength, ProgramType.Beast };

        private List<ExerciseDef> _list;
        private int _filter = -1; // -1 == all, else (int)ProgramType
        private ExerciseDef _editing;
        private VisualElement _content;

        protected override void OnBuild(VisualElement root)
        {
            var back = Q("back");
            back.Add(Icons.Create("chevron-back", 22, Palette.White));
            back.OnClick(() => Ctx.Router.Back());

            var add = Q("add");
            add.Add(Icons.Create("add", 22, Palette.Black));
            add.OnClick(OpenNew);

            Q<ScrollView>("scroll").HideScrollbars();
            var chipsScroll = Q<ScrollView>("chips-scroll");
            if (chipsScroll != null) chipsScroll.HideScrollbars();
            _content = Q("content");

            Refresh();
            RenderChips();
        }

        private void Refresh()
        {
            _list = Ctx.Store.GetExercises();
            RenderList();
        }

        private void RenderChips()
        {
            var row = Q("chips-row");
            row.Clear();
            row.Add(FilterChip(-1, "ALL"));
            foreach (var c in Categories)
                row.Add(FilterChip((int)c, c.ToId().ToUpperInvariant()));
        }

        private VisualElement FilterChip(int value, string text)
        {
            bool selected = _filter == value;
            var chip = new VisualElement();
            chip.AddToClassList("chip");
            if (selected)
            {
                var color = value == -1 ? Palette.White : Palette.CategoryColor((ProgramType)value);
                chip.style.backgroundColor = color;
                chip.SetBorderColor(color);
            }
            var lbl = new Label(text);
            lbl.AddToClassList("chip__text");
            if (selected) lbl.style.color = Palette.Black;
            chip.Add(lbl);
            chip.OnClick(() => { _filter = value; RenderChips(); RenderList(); });
            return chip;
        }

        private void RenderList()
        {
            _content.Clear();
            var filtered = new List<ExerciseDef>();
            foreach (var e in _list)
                if (_filter == -1 || (int)e.category == _filter) filtered.Add(e);

            foreach (var e in filtered)
                _content.Add(Row(e));

            if (filtered.Count == 0)
            {
                var empty = new VisualElement();
                empty.AddToClassList("empty-dashed");
                var t = new Label("No exercises here yet.");
                t.AddToClassList("muted");
                empty.Add(t);
                _content.Add(empty);
            }
        }

        private VisualElement Row(ExerciseDef e)
        {
            var row = new VisualElement();
            row.AddToClassList("ex-row");
            row.SetBorderColor(Palette.CategoryColor(e.category));

            var cat = new VisualElement();
            cat.AddToClassList("ex-row__cat");
            cat.style.backgroundColor = Palette.CategoryColor(e.category);
            var catText = new Label(e.category.ToId().Substring(0, 1).ToUpperInvariant());
            catText.AddToClassList("ex-row__cat-text");
            cat.Add(catText);
            row.Add(cat);

            var col = new VisualElement();
            col.style.flexGrow = 1;
            var name = new Label(e.name);
            name.AddToClassList("ex-row__name");
            col.Add(name);
            var meta = new Label(MetaText(e));
            meta.AddToClassList("ex-row__meta");
            col.Add(meta);
            row.Add(col);

            var edit = new VisualElement();
            edit.AddToClassList("ex-row__icon-btn");
            edit.Add(Icons.Create("pencil", 16, Palette.Hex("#A1A1AA")));
            edit.OnClick(() => OpenEdit(e));
            row.Add(edit);

            var del = new VisualElement();
            del.AddToClassList("ex-row__icon-btn");
            del.Add(Icons.Create("trash", 16, Palette.Beast));
            del.OnClick(() => { Ctx.Store.DeleteExercise(e.id); Refresh(); });
            row.Add(del);

            return row;
        }

        private static string MetaText(ExerciseDef e)
        {
            var sb = new System.Text.StringBuilder();
            if (e.defaultReps > 0) sb.Append($"{e.defaultReps} reps");
            if (e.defaultWeight > 0) sb.Append($"{(sb.Length > 0 ? " • " : "")}{e.defaultWeight:0.##} kg");
            if (e.defaultDurationSec > 0) sb.Append($"{(sb.Length > 0 ? " • " : "")}{e.defaultDurationSec}s");
            return sb.ToString();
        }

        // ---------- editor modal ----------
        private void OpenNew()
        {
            _editing = new ExerciseDef($"ex-{TimeUtil.NowMs()}", "", ProgramType.Strength, 10, 20, 0);
            ShowModal();
        }

        private void OpenEdit(ExerciseDef e)
        {
            _editing = e.Clone();
            ShowModal();
        }

        private TextField _nameField, _repsField, _weightField, _durationField;
        private VisualElement _catRow;

        private void ShowModal()
        {
            bool isExisting = _list.Exists(x => x.id == _editing.id);

            var host = Q("modal-host");
            host.style.display = DisplayStyle.Flex;
            host.Clear();
            host.AddToClassList("modal-root");

            var card = new VisualElement();
            card.AddToClassList("modal-card");

            var title = new Label($"{(isExisting ? "Edit" : "New")} Exercise");
            title.AddToClassList("modal-title");
            card.Add(title);

            card.Add(FieldLabel("NAME"));
            _nameField = MakeField(_editing.name, "e.g. Goblet Squat");
            card.Add(_nameField);

            card.Add(FieldLabel("CATEGORY"));
            _catRow = new VisualElement();
            _catRow.AddToClassList("cat-row");
            RenderCatChips();
            card.Add(_catRow);

            var twoCol = new VisualElement();
            twoCol.style.flexDirection = FlexDirection.Row;

            var repsCol = new VisualElement();
            repsCol.style.flexGrow = 1; repsCol.style.flexBasis = 0; repsCol.style.marginRight = 8;
            repsCol.Add(FieldLabel("REPS"));
            _repsField = MakeField(_editing.defaultReps.ToString(), null);
            repsCol.Add(_repsField);
            twoCol.Add(repsCol);

            var wtCol = new VisualElement();
            wtCol.style.flexGrow = 1; wtCol.style.flexBasis = 0;
            wtCol.Add(FieldLabel("WEIGHT (KG)"));
            _weightField = MakeField(_editing.defaultWeight.ToString(CultureInfo.InvariantCulture), null);
            wtCol.Add(_weightField);
            twoCol.Add(wtCol);
            card.Add(twoCol);

            card.Add(FieldLabel("DURATION (SEC, OPTIONAL)"));
            _durationField = MakeField(_editing.defaultDurationSec > 0 ? _editing.defaultDurationSec.ToString() : "", "e.g. 60");
            card.Add(_durationField);

            var actions = new VisualElement();
            actions.style.marginTop = 12;
            actions.Add(UIUtils.TactileButton("Save Exercise", "checkmark", UIUtils.ButtonVariant.Primary, Save));
            var gap = new VisualElement(); gap.style.height = 8; actions.Add(gap);
            actions.Add(UIUtils.TactileButton("Cancel", null, UIUtils.ButtonVariant.Secondary, HideModal));
            card.Add(actions);

            host.Add(card);
        }

        private void RenderCatChips()
        {
            _catRow.Clear();
            foreach (var c in Categories)
            {
                bool selected = _editing.category == c;
                var chip = new VisualElement();
                chip.AddToClassList("cat-chip");
                chip.style.backgroundColor = selected ? Palette.CategoryColor(c) : Palette.Hex("#0A0A0C");
                chip.SetBorderColor(Palette.CategoryColor(c));
                var lbl = new Label(c.ToId().ToUpperInvariant());
                lbl.AddToClassList("cat-chip__text");
                lbl.style.color = selected ? Palette.Black : Palette.White;
                chip.Add(lbl);
                chip.OnClick(() => { _editing.category = c; RenderCatChips(); });
                _catRow.Add(chip);
            }
        }

        private void Save()
        {
            // Read fields back into the working copy.
            _editing.name = _nameField.value?.Trim() ?? "";
            if (string.IsNullOrEmpty(_editing.name)) return;
            _editing.defaultReps = ParseInt(_repsField.value);
            _editing.defaultWeight = ParseFloat(_weightField.value);
            _editing.defaultDurationSec = string.IsNullOrWhiteSpace(_durationField.value) ? 0 : ParseInt(_durationField.value);

            if (_list.Exists(x => x.id == _editing.id)) Ctx.Store.UpdateExercise(_editing);
            else Ctx.Store.AddExercise(_editing);

            _editing = null;
            HideModal();
            Refresh();
        }

        private void HideModal()
        {
            var host = Q("modal-host");
            host.Clear();
            host.RemoveFromClassList("modal-root");
            host.style.display = DisplayStyle.None;
        }

        private static int ParseInt(string s) => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0;
        private static float ParseFloat(string s) => float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0f;

        private static Label FieldLabel(string text) { var l = new Label(text); l.AddToClassList("field-label"); return l; }

        private static TextField MakeField(string value, string placeholder)
        {
            var tf = new TextField { value = value ?? "" };
            tf.AddToClassList("text-input");
            // Placeholder text is set via the UXML-equivalent attribute when supported by the
            // running Unity version; kept cosmetic-only to avoid hard API coupling.
            if (!string.IsNullOrEmpty(placeholder))
                tf.SetPlaceholderText(placeholder);
            return tf;
        }
    }
}
