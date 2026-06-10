using UnityEngine;
using UnityEngine.UIElements;
using WorkoutDrop.Core;
using WorkoutDrop.Data;

namespace WorkoutDrop.UI.Screens
{
    /// <summary>Onboarding carousel. Mirrors <c>app/welcome.tsx</c>.</summary>
    public class WelcomeScreen : ScreenController
    {
        public override ScreenId Id => ScreenId.Welcome;

        private struct Slide
        {
            public string Title;
            public string Body;
            public string Icon;        // used when HasImage == false
            public bool HasImage;
            public bool Fire;          // which kettlebell sprite
            public Color Accent;
        }

        private static readonly Slide[] Slides =
        {
            new Slide { Title = "Set Up Your Exercises", Body = "Build your gym. Pick from our starter pack or roll your own.", Icon = "barbell", HasImage = false, Accent = default },
            new Slide { Title = "Drop The Ball", Body = "Each day, drop the kettlebell on the Plinko board. Where it lands is your workout.", HasImage = true, Fire = false, Accent = default },
            new Slide { Title = "Hit Beast Mode", Body = "Edges of the board mean HIIT. Push limits, unlock badges, build streaks.", HasImage = true, Fire = true, Accent = default },
        };

        private static readonly Color[] Accents =
        {
            new Color(0f, 0.819f, 1f),       // #00D1FF
            new Color(0f, 1f, 0.478f),       // #00FF7A
            new Color(1f, 0.231f, 0.188f),   // #FF3B30
        };

        private int _page;

        protected override void OnBuild(VisualElement root)
        {
            Q<Label>("skip").OnClick(Finish);
            RenderPage();
        }

        private void RenderPage()
        {
            var slide = Slides[_page];
            var accent = Accents[_page];
            bool last = _page == Slides.Length - 1;

            // Dots
            var dots = Q("dots");
            dots.Clear();
            for (int i = 0; i < Slides.Length; i++)
            {
                var dot = new VisualElement();
                dot.AddToClassList("welcome-dot");
                if (i == _page) dot.AddToClassList("welcome-dot--active");
                dots.Add(dot);
            }

            // Icon / image
            var wrap = Q("icon-wrap");
            wrap.SetBorderColor(accent);
            var host = Q("icon-host");
            host.Clear();
            if (slide.HasImage)
            {
                var img = new VisualElement();
                img.AddToClassList("welcome-image");
                var sprite = Config.Kettlebell(slide.Fire);
                if (sprite != null) img.style.backgroundImage = new StyleBackground(sprite);
                host.Add(img);
            }
            else
            {
                host.Add(Icons.Create(slide.Icon, 130, accent));
            }

            Q<Label>("step").text = $"STEP {_page + 1}";
            Q<Label>("title").text = slide.Title;
            Q<Label>("body").text = slide.Body;

            var cta = Q("cta-host");
            cta.Clear();
            cta.Add(UIUtils.TactileButton(
                last ? "Build My Gym" : "Next",
                last ? "construct" : "arrow-forward",
                last ? UIUtils.ButtonVariant.Primary : UIUtils.ButtonVariant.Secondary,
                Next));
        }

        private void Next()
        {
            if (_page < Slides.Length - 1) { _page++; RenderPage(); }
            else Finish();
        }

        private void Finish()
        {
            var s = Ctx.Store.GetSettings();
            s.firstLaunchDone = true;
            Ctx.Store.SaveSettings(s);
            Ctx.Router.GoTab(ScreenId.Home);
        }
    }
}
