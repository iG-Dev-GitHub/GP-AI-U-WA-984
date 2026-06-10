using UnityEngine;
using UnityEngine.UIElements;
using WorkoutDrop.Core;
using WorkoutDrop.Data;

namespace WorkoutDrop.UI
{
    /// <summary>
    /// Dependency container handed to every screen. Replaces the web app's module-level
    /// singletons (store, router) with explicit injection — nothing is resolved by string.
    /// </summary>
    public class AppContext
    {
        public readonly AppConfig Config;
        public readonly Store Store;
        public readonly IRng Rng;
        public readonly Router Router;
        public readonly MonoBehaviour Runner; // for any coroutine needs / scheduling root

        public AppContext(AppConfig config, Store store, IRng rng, Router router, MonoBehaviour runner)
        {
            Config = config;
            Store = store;
            Rng = rng;
            Router = router;
            Runner = runner;
        }
    }
}
