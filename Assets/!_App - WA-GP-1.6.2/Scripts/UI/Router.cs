using System.Collections.Generic;
using UnityEngine.UIElements;
using WorkoutDrop.Core;
using WorkoutDrop.Data;
using WorkoutDrop.UI.Screens;

namespace WorkoutDrop.UI
{
    /// <summary>
    /// Stack-based navigator that reproduces the expo-router flow (push / replace / back) plus
    /// the bottom tab navigator. Tab screens (Home, Progress, Settings) show the tab bar and
    /// reset the stack; the rest are full-screen pushes.
    /// </summary>
    public class Router
    {
        private static readonly HashSet<ScreenId> TabScreens = new HashSet<ScreenId>
        {
            ScreenId.Home, ScreenId.Progress, ScreenId.Settings,
        };

        private readonly VisualElement _host;
        private readonly VisualElement _tabBar;
        private AppContext _ctx;

        private readonly List<ScreenId> _stack = new List<ScreenId>();
        private ScreenController _current;

        public Router(VisualElement host, VisualElement tabBar)
        {
            _host = host;
            _tabBar = tabBar;
        }

        public void SetContext(AppContext ctx)
        {
            _ctx = ctx;
            BuildTabBar();
        }

        public ScreenId Current => _stack.Count > 0 ? _stack[_stack.Count - 1] : ScreenId.Home;

        // ----- entry point -----
        public void Start()
        {
            var settings = _ctx.Store.GetSettings();
            if (!settings.firstLaunchDone) ReplaceRoot(ScreenId.Welcome);
            else GoTab(ScreenId.Home);
        }

        // ----- navigation primitives (mirror expo-router) -----
        public void ReplaceRoot(ScreenId id) { _stack.Clear(); _stack.Add(id); Render(); }
        public void Push(ScreenId id) { _stack.Add(id); Render(); }
        public void Replace(ScreenId id) { if (_stack.Count == 0) _stack.Add(id); else _stack[_stack.Count - 1] = id; Render(); }
        public void Back() { if (_stack.Count > 1) _stack.RemoveAt(_stack.Count - 1); Render(); }
        public void GoTab(ScreenId id) { _stack.Clear(); _stack.Add(id); Render(); }

        private ScreenController CreateController(ScreenId id)
        {
            switch (id)
            {
                case ScreenId.Welcome: return new WelcomeScreen();
                case ScreenId.Home: return new HomeScreen();
                case ScreenId.Drop: return new DropScreen();
                case ScreenId.Workout: return new WorkoutScreen();
                case ScreenId.Summary: return new SummaryScreen();
                case ScreenId.Progress: return new ProgressScreen();
                case ScreenId.Settings: return new SettingsScreen();
                case ScreenId.Exercises: return new ExercisesScreen();
                default: return new HomeScreen();
            }
        }

        private void Render()
        {
            _current?.OnDestroyScreen();
            _host.Clear();

            var id = Current;
            _current = CreateController(id);
            var view = _current.Build(_ctx);
            _host.Add(view);

            bool showTabs = TabScreens.Contains(id);
            _tabBar.style.display = showTabs ? DisplayStyle.Flex : DisplayStyle.None;
            HighlightTab(id);
        }

        // ----- tab bar -----
        private readonly Dictionary<ScreenId, VisualElement> _tabItems = new Dictionary<ScreenId, VisualElement>();

        private void BuildTabBar()
        {
            _tabBar.Clear();
            _tabItems.Clear();
            AddTabItem(ScreenId.Home, "home", "Home");
            AddTabItem(ScreenId.Progress, "stats-chart", "Progress");
            AddTabItem(ScreenId.Settings, "settings", "Settings");
        }

        private void AddTabItem(ScreenId id, string icon, string label)
        {
            var item = new VisualElement();
            item.AddToClassList("tab-item");

            var ic = Icons.Create(icon, 22, Palette.Hex("#52525B"));
            ic.AddToClassList("tab-item__icon");
            var lb = new Label(label.ToUpperInvariant());
            lb.AddToClassList("tab-item__label");

            item.Add(ic);
            item.Add(lb);
            item.RegisterCallback<ClickEvent>(_ => GoTab(id));

            _tabBar.Add(item);
            _tabItems[id] = item;
        }

        private void HighlightTab(ScreenId id)
        {
            foreach (var kv in _tabItems)
            {
                bool active = kv.Key == id;
                var item = kv.Value;
                var ic = item.Q<Label>(className: "tab-item__icon");
                var lb = item.Q<Label>(className: "tab-item__label");
                var color = active ? Palette.PrimaryCyan : Palette.Hex("#52525B");
                if (ic != null) ic.style.color = color;
                if (lb != null) lb.style.color = color;
            }
        }
    }
}
