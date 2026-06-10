using UnityEngine.UIElements;
using WorkoutDrop.Core;
using WorkoutDrop.Data;

namespace WorkoutDrop.UI.Screens
{
    /// <summary>Settings tab. Mirrors <c>app/(tabs)/settings.tsx</c>.</summary>
    public class SettingsScreen : ScreenController
    {
        public override ScreenId Id => ScreenId.Settings;

        private Settings _settings;
        private VisualElement _content;

        protected override void OnBuild(VisualElement root)
        {
            _settings = Ctx.Store.GetSettings();
            Q<ScrollView>("scroll").HideScrollbars();
            _content = Q("content");
            Render();
        }

        private void Render()
        {
            _content.Clear();

            _content.Add(Label("PREFERENCES", "eyebrow"));
            _content.Add(Label("Settings", "h1"));

            _content.Add(Label("Weight Unit", "section-title"));
            var unitRow = new VisualElement();
            unitRow.AddToClassList("unit-row");
            unitRow.Add(UnitChip("KG", _settings.weightUnit == WeightUnit.Kg, () => Update(s => s.weightUnit = WeightUnit.Kg), false, false));
            unitRow.Add(UnitChip("LBS", _settings.weightUnit == WeightUnit.Lbs, () => Update(s => s.weightUnit = WeightUnit.Lbs), false, false));
            _content.Add(unitRow);

            _content.Add(Label("Default Risk Level", "section-title"));
            var riskRow = new VisualElement();
            riskRow.AddToClassList("unit-row");
            riskRow.Add(RiskChip(RiskLevel.Easy, "leaf", "EASY"));
            riskRow.Add(RiskChip(RiskLevel.Beast, "flame", "BEAST"));
            _content.Add(riskRow);

            _content.Add(Label("Library", "section-title"));
            _content.Add(ManageLink());

            _content.Add(Label("Danger Zone", "section-title"));
            _content.Add(UIUtils.TactileButton("Reset All Data", "trash", UIUtils.ButtonVariant.Beast, ShowResetConfirm));

            var footer = new Label("OFFLINE TRACKER — NO ACCOUNT, NO CLOUD.");
            footer.AddToClassList("footer");
            _content.Add(footer);
        }

        private void Update(System.Action<Settings> mutate)
        {
            mutate(_settings);
            Ctx.Store.SaveSettings(_settings);
            Render();
        }

        private VisualElement UnitChip(string text, bool selected, System.Action onClick, bool withIcon, bool beast)
        {
            var chip = new VisualElement();
            chip.AddToClassList("unit-chip");
            if (selected) { chip.style.backgroundColor = Palette.PrimaryCyan; chip.SetBorderColor(Palette.PrimaryCyan); }
            var lbl = new Label(text);
            lbl.AddToClassList("unit-text");
            lbl.style.color = selected ? Palette.Black : Palette.White;
            chip.Add(lbl);
            chip.OnClick(onClick);
            return chip;
        }

        private VisualElement RiskChip(RiskLevel level, string icon, string text)
        {
            bool selected = _settings.riskLevel == level;
            bool beast = level == RiskLevel.Beast;
            var accent = beast ? Palette.Beast : Palette.PrimaryCyan;

            var chip = new VisualElement();
            chip.AddToClassList("unit-chip");
            if (selected) { chip.style.backgroundColor = accent; chip.SetBorderColor(accent); }

            var contentColor = selected ? (beast ? Palette.White : Palette.Black) : Palette.White;
            chip.Add(Icons.Create(icon, 14, contentColor));

            var lbl = new Label(text);
            lbl.AddToClassList("unit-text");
            lbl.style.marginLeft = 6;
            lbl.style.color = contentColor;
            chip.Add(lbl);

            chip.OnClick(() => Update(s => s.riskLevel = level));
            return chip;
        }

        private VisualElement ManageLink()
        {
            var row = new VisualElement();
            row.AddToClassList("link-row");

            var left = new VisualElement();
            left.AddToClassList("link-left");
            var iconBox = new VisualElement();
            iconBox.AddToClassList("link-icon");
            iconBox.style.backgroundColor = Palette.Strength;
            iconBox.Add(Icons.Create("barbell", 18, Palette.Black));
            left.Add(iconBox);
            var col = new VisualElement();
            var t = new Label("Manage Exercises");
            t.AddToClassList("link-title");
            col.Add(t);
            var s = new Label("Add, edit, delete");
            s.AddToClassList("link-sub");
            col.Add(s);
            left.Add(col);
            row.Add(left);

            row.Add(Icons.Create("chevron-forward", 20, Palette.Hex("#52525B")));
            row.OnClick(() => Ctx.Router.Push(ScreenId.Exercises));
            return row;
        }

        private void ShowResetConfirm()
        {
            var host = Q("modal-host");
            host.style.display = DisplayStyle.Flex;
            host.Clear();
            host.AddToClassList("modal-root");
            host.AddToClassList("modal-root--center");

            var card = new VisualElement();
            card.AddToClassList("modal-card");
            card.AddToClassList("modal-card--center");

            card.Add(Icons.Create("warning", 32, Palette.Beast));
            var title = new Label("Reset everything?");
            title.AddToClassList("modal-title");
            title.style.marginTop = 12;
            title.style.marginBottom = 6;
            card.Add(title);
            var body = new Label("This clears exercises, history, PRs, badges and settings. Cannot be undone.");
            body.AddToClassList("modal-body");
            card.Add(body);

            var spacer = new VisualElement(); spacer.style.height = 12; card.Add(spacer);

            var confirm = UIUtils.TactileButton("Yes, Wipe It", "trash", UIUtils.ButtonVariant.Beast, DoReset);
            card.Add(confirm);
            var spacer2 = new VisualElement(); spacer2.style.height = 8; card.Add(spacer2);
            var cancel = UIUtils.TactileButton("Cancel", null, UIUtils.ButtonVariant.Secondary, HideModal);
            card.Add(cancel);

            host.Add(card);
        }

        private void HideModal()
        {
            var host = Q("modal-host");
            host.Clear();
            host.RemoveFromClassList("modal-root");
            host.RemoveFromClassList("modal-root--center");
            host.style.display = DisplayStyle.None;
        }

        private void DoReset()
        {
            Ctx.Store.ResetAll();
            HideModal();
            Ctx.Router.ReplaceRoot(ScreenId.Welcome);
        }

        private static Label Label(string text, string cls) { var l = new Label(text); l.AddToClassList(cls); return l; }
    }
}
