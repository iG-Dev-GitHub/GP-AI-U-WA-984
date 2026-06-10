using System;
using UnityEngine.UIElements;
using WorkoutDrop.Core;

namespace WorkoutDrop.UI.Components
{
    /// <summary>
    /// One exercise card with its set table. Mirrors <c>src/components/ExerciseCard.tsx</c>.
    /// The action cell for each set is supplied by the caller (the workout screen) so the same
    /// card can render either interactive steppers or static status icons.
    /// </summary>
    public static class ExerciseCardBuilder
    {
        public static VisualElement Build(
            string name,
            ProgramType category,
            System.Collections.Generic.List<WorkoutSet> sets,
            WeightUnit unit,
            bool bonus,
            bool prAchieved,
            Func<int, WorkoutSet, VisualElement> renderSetActions)
        {
            var card = new VisualElement();
            card.AddToClassList("ex-card");
            card.SetBorderColor(Palette.CategoryColor(category));

            // header
            var header = new VisualElement();
            header.AddToClassList("ex-card__header");

            var titleRow = new VisualElement();
            titleRow.AddToClassList("ex-card__title-row");

            var dot = new VisualElement();
            dot.AddToClassList("ex-card__dot");
            dot.style.backgroundColor = Palette.CategoryColor(category);
            titleRow.Add(dot);

            var title = new Label(name);
            title.AddToClassList("ex-card__title");
            titleRow.Add(title);

            if (bonus)
            {
                var bonusBadge = new VisualElement();
                bonusBadge.AddToClassList("ex-card__bonus");
                bonusBadge.Add(Icons.Create("flame", 12, Palette.Black));
                var bt = new Label("BONUS");
                bt.AddToClassList("ex-card__bonus-text");
                bonusBadge.Add(bt);
                titleRow.Add(bonusBadge);
            }

            header.Add(titleRow);

            if (prAchieved)
            {
                var prBadge = new VisualElement();
                prBadge.AddToClassList("ex-card__pr");
                prBadge.Add(Icons.Create("trophy", 14, Palette.Black));
                var pt = new Label("PR");
                pt.AddToClassList("ex-card__pr-text");
                prBadge.Add(pt);
                header.Add(prBadge);
            }

            card.Add(header);

            // sets header
            var setsHeader = new VisualElement();
            setsHeader.AddToClassList("ex-card__sets-header");
            setsHeader.Add(ColLabel("SET", "col--narrow"));
            setsHeader.Add(ColLabel("REPS", null));
            setsHeader.Add(ColLabel("WEIGHT", null));
            setsHeader.Add(ColLabel("ACTION", "col--action"));
            card.Add(setsHeader);

            // set rows
            for (int i = 0; i < sets.Count; i++)
            {
                var s = sets[i];
                var row = new VisualElement();
                row.AddToClassList("ex-card__set-row");

                var setNo = new Label((i + 1).ToString());
                setNo.AddToClassList("ex-card__set-text");
                setNo.AddToClassList("col--narrow");
                row.Add(setNo);

                var reps = new Label(s.durationSec > 0 ? $"{s.durationSec}s" : s.reps.ToString());
                reps.AddToClassList("ex-card__set-text");
                row.Add(reps);

                var weight = new Label(UIUtils.FormatWeight(s.weight, unit));
                weight.AddToClassList("ex-card__set-text");
                row.Add(weight);

                var actionWrap = new VisualElement();
                actionWrap.AddToClassList("ex-card__action");
                if (renderSetActions != null)
                {
                    var node = renderSetActions(i, s);
                    if (node != null) actionWrap.Add(node);
                }
                row.Add(actionWrap);

                card.Add(row);
            }

            return card;
        }

        private static Label ColLabel(string text, string extra)
        {
            var l = new Label(text);
            l.AddToClassList("ex-card__col-label");
            if (!string.IsNullOrEmpty(extra)) l.AddToClassList(extra);
            return l;
        }
    }
}
