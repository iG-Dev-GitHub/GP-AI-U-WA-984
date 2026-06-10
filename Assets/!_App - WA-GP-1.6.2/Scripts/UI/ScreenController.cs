using UnityEngine.UIElements;
using WorkoutDrop.Data;

namespace WorkoutDrop.UI
{
    /// <summary>
    /// Base class for every screen. A screen clones its UXML, wires up behaviour, and exposes
    /// the resulting <see cref="Root"/>. Screens are rebuilt on every navigation, which gives
    /// the same "refresh on focus" behaviour the web app got from useFocusEffect.
    /// </summary>
    public abstract class ScreenController
    {
        protected AppContext Ctx { get; private set; }
        protected AppConfig Config => Ctx.Config;
        public VisualElement Root { get; private set; }

        public abstract ScreenId Id { get; }

        public VisualElement Build(AppContext ctx)
        {
            Ctx = ctx;
            var uxml = ctx.Config.ScreenUxml(Id);
            Root = uxml != null ? uxml.Instantiate() : new VisualElement();
            // Instantiate() wraps in a TemplateContainer; make it fill its parent.
            Root.style.flexGrow = 1;
            Root.AddToClassList("screen");
            OnBuild(Root);
            return Root;
        }

        /// <summary>Query a named element from the cloned UXML.</summary>
        protected T Q<T>(string name) where T : VisualElement => Root.Q<T>(name);
        protected VisualElement Q(string name) => Root.Q(name);

        /// <summary>Populate and wire the cloned tree.</summary>
        protected abstract void OnBuild(VisualElement root);

        /// <summary>Called by the router right before this screen is removed (stop timers, etc.).</summary>
        public virtual void OnDestroyScreen() { }
    }
}
