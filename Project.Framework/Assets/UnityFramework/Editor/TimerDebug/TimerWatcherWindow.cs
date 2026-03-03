#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityFramework.Editor
{
    public sealed class TimerWatcherWindow : EditorWindow
    {
        private const double RefreshInterval = 0.2d;

        private readonly List<TimerDebugInfo> _timers = new();
        private Vector2 _scroll;
        private string _search = string.Empty;
        private bool _autoRefresh = true;
        private bool _showScaled = true;
        private bool _showUnscaled = true;
        private bool _showLoop = true;
        private bool _showOnce = true;
        private double _nextRefreshTime;

        [MenuItem("Tools/Timer/Watcher", false, 210)]
        private static void OpenWindow()
        {
            var window = GetWindow<TimerWatcherWindow>("Timer Watcher");
            window.minSize = new Vector2(860f, 320f);
            window.Show();
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
            RefreshNow();
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (!_autoRefresh) return;
            if (EditorApplication.timeSinceStartup < _nextRefreshTime) return;

            _nextRefreshTime = EditorApplication.timeSinceStartup + RefreshInterval;
            RefreshNow();
            Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawFilters();
            DrawTable();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(80f)))
                RefreshNow();

            _autoRefresh = GUILayout.Toggle(_autoRefresh, "Auto", EditorStyles.toolbarButton, GUILayout.Width(60f));
            TimerDebugBridge.Enable = GUILayout.Toggle(TimerDebugBridge.Enable, "Enable Debug", EditorStyles.toolbarButton, GUILayout.Width(100f));
            TimerDebugBridge.CaptureStackTrace = GUILayout.Toggle(TimerDebugBridge.CaptureStackTrace, "Capture Stack", EditorStyles.toolbarButton, GUILayout.Width(110f));

            GUILayout.FlexibleSpace();
            GUILayout.Label($"Count: {_timers.Count}", EditorStyles.miniLabel, GUILayout.Width(80f));

            EditorGUILayout.EndHorizontal();
        }

        private void DrawFilters()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Search", GUILayout.Width(50f));
            _search = EditorGUILayout.TextField(_search);
            _showScaled = GUILayout.Toggle(_showScaled, "Scaled", GUILayout.Width(70f));
            _showUnscaled = GUILayout.Toggle(_showUnscaled, "Unscaled", GUILayout.Width(80f));
            _showLoop = GUILayout.Toggle(_showLoop, "Loop", GUILayout.Width(60f));
            _showOnce = GUILayout.Toggle(_showOnce, "Once", GUILayout.Width(60f));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTable()
        {
            DrawHeader();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (int i = 0; i < _timers.Count; i++)
            {
                var info = _timers[i];
                if (!CanShow(info)) continue;
                DrawRow(info);
            }
            EditorGUILayout.EndScrollView();
        }

        private static void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal("box");
            GUILayout.Label("ID", GUILayout.Width(60f));
            GUILayout.Label("Remaining", GUILayout.Width(90f));
            GUILayout.Label("Delay", GUILayout.Width(90f));
            GUILayout.Label("Loop", GUILayout.Width(50f));
            GUILayout.Label("Unscaled", GUILayout.Width(70f));
            GUILayout.Label("Running", GUILayout.Width(60f));
            GUILayout.Label("Age", GUILayout.Width(80f));
            GUILayout.Label("StackTrace");
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawRow(TimerDebugInfo info)
        {
            EditorGUILayout.BeginHorizontal("box");
            GUILayout.Label(info.Id.ToString(), GUILayout.Width(60f));
            GUILayout.Label(info.Remaining.ToString("0.000"), GUILayout.Width(90f));
            GUILayout.Label(info.Delay.ToString("0.000"), GUILayout.Width(90f));
            GUILayout.Label(info.Loop ? "Y" : "N", GUILayout.Width(50f));
            GUILayout.Label(info.Unscaled ? "Y" : "N", GUILayout.Width(70f));
            GUILayout.Label(info.IsRunning ? "Y" : "N", GUILayout.Width(60f));

            double age = Time.realtimeSinceStartupAsDouble - info.CreatedAt;
            GUILayout.Label(age.ToString("0.00"), GUILayout.Width(80f));

            if (string.IsNullOrEmpty(info.StackTrace))
                GUILayout.Label("-");
            else
                GUILayout.Label(info.StackTrace, EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.EndHorizontal();
        }

        private bool CanShow(TimerDebugInfo info)
        {
            if (!MatchScale(info)) return false;
            if (!MatchLoop(info)) return false;
            if (!MatchSearch(info)) return false;
            return true;
        }

        private bool MatchScale(TimerDebugInfo info)
            => info.Unscaled ? _showUnscaled : _showScaled;

        private bool MatchLoop(TimerDebugInfo info)
            => info.Loop ? _showLoop : _showOnce;

        private bool MatchSearch(TimerDebugInfo info)
        {
            if (string.IsNullOrWhiteSpace(_search)) return true;
            return info.Id.ToString().Contains(_search);
        }

        private void RefreshNow()
        {
            _timers.Clear();
            _timers.AddRange(TimerDebugBridge.GetSnapshot());
        }
    }
}
#endif
