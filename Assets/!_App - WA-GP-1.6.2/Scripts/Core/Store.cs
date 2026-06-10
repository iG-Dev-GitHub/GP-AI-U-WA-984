using System;
using System.Collections.Generic;
using UnityEngine;
using WorkoutDrop.Data;

namespace WorkoutDrop.Core
{
    // Concrete JSON wrappers — JsonUtility cannot (de)serialize bare List<T> or generics.
    [Serializable] internal class ExerciseListJson { public List<ExerciseDef> items = new List<ExerciseDef>(); }
    [Serializable] internal class WorkoutListJson { public List<Workout> items = new List<Workout>(); }
    [Serializable] internal class PRListJson { public List<PREntry> items = new List<PREntry>(); }

    /// <summary>
    /// Offline persistence layer. Mirrors <c>src/data/store.ts</c> backed by AsyncStorage,
    /// using PlayerPrefs + JSON here. Reads never throw (return fallbacks); all keys and
    /// behaviours (seeding, 200-workout cap, reset) match the web app.
    /// </summary>
    public class Store
    {
        private const string KeyExercises = "wd:exercises";
        private const string KeyWorkouts = "wd:workouts";
        private const string KeySettings = "wd:settings";
        private const string KeyPrs = "wd:prs";
        private const string KeyCurrent = "wd:currentWorkout";
        private const string KeyLastSummary = "wd:lastSummary";

        private readonly ExerciseDatabase _seedDb;

        public Store(ExerciseDatabase seedDb)
        {
            _seedDb = seedDb;
        }

        // ---------- low level ----------
        private static string ReadRaw(string key) => PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null;

        private static void WriteRaw(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }

        private static T ReadJson<T>(string key) where T : class
        {
            var raw = ReadRaw(key);
            if (string.IsNullOrEmpty(raw)) return null;
            try { return JsonUtility.FromJson<T>(raw); }
            catch (Exception e) { Debug.LogWarning($"[store] parse {key} failed: {e.Message}"); return null; }
        }

        // ---------- Settings ----------
        public Settings GetSettings()
        {
            var s = ReadJson<Settings>(KeySettings);
            return s ?? new Settings();
        }

        public void SaveSettings(Settings s) => WriteRaw(KeySettings, JsonUtility.ToJson(s));

        // ---------- Exercises ----------
        public List<ExerciseDef> GetExercises()
        {
            var wrapped = ReadJson<ExerciseListJson>(KeyExercises);
            if (wrapped == null || wrapped.items == null || wrapped.items.Count == 0)
            {
                var seed = _seedDb != null ? _seedDb.CloneSeed() : new List<ExerciseDef>();
                SaveExercises(seed);
                return seed;
            }
            return wrapped.items;
        }

        public void SaveExercises(List<ExerciseDef> list) =>
            WriteRaw(KeyExercises, JsonUtility.ToJson(new ExerciseListJson { items = list }));

        public void AddExercise(ExerciseDef e)
        {
            var list = GetExercises();
            list.Add(e);
            SaveExercises(list);
        }

        public void UpdateExercise(ExerciseDef e)
        {
            var list = GetExercises();
            int idx = list.FindIndex(x => x.id == e.id);
            if (idx >= 0)
            {
                list[idx] = e;
                SaveExercises(list);
            }
        }

        public void DeleteExercise(string id)
        {
            var list = GetExercises();
            list.RemoveAll(x => x.id == id);
            SaveExercises(list);
        }

        // ---------- Workouts ----------
        public List<Workout> GetWorkouts()
        {
            var wrapped = ReadJson<WorkoutListJson>(KeyWorkouts);
            return wrapped?.items ?? new List<Workout>();
        }

        public void SaveWorkouts(List<Workout> list) =>
            WriteRaw(KeyWorkouts, JsonUtility.ToJson(new WorkoutListJson { items = list }));

        public void AddWorkout(Workout w)
        {
            var list = GetWorkouts();
            list.Insert(0, w); // unshift
            if (list.Count > 200) list = list.GetRange(0, 200);
            SaveWorkouts(list);
        }

        // ---------- PRs ----------
        public List<PREntry> GetPRs()
        {
            var wrapped = ReadJson<PRListJson>(KeyPrs);
            return wrapped?.items ?? new List<PREntry>();
        }

        public void SavePRs(List<PREntry> entries) =>
            WriteRaw(KeyPrs, JsonUtility.ToJson(new PRListJson { items = entries }));

        // ---------- transient workout handoff ----------
        public void SetCurrentWorkout(Workout w) => WriteRaw(KeyCurrent, JsonUtility.ToJson(w));
        public Workout GetCurrentWorkout() => ReadJson<Workout>(KeyCurrent);
        public void ClearCurrentWorkout() { PlayerPrefs.DeleteKey(KeyCurrent); PlayerPrefs.Save(); }

        public void SetLastSummary(Workout w) => WriteRaw(KeyLastSummary, JsonUtility.ToJson(w));
        public Workout GetLastSummary() => ReadJson<Workout>(KeyLastSummary);

        // ---------- Reset ----------
        public void ResetAll()
        {
            PlayerPrefs.DeleteKey(KeyExercises);
            PlayerPrefs.DeleteKey(KeyWorkouts);
            PlayerPrefs.DeleteKey(KeySettings);
            PlayerPrefs.DeleteKey(KeyPrs);
            PlayerPrefs.DeleteKey(KeyCurrent);
            PlayerPrefs.DeleteKey(KeyLastSummary);
            PlayerPrefs.Save();
        }
    }
}
