using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFramework
{
    public readonly struct TimerDebugInfo
    {
        public readonly int Id;
        public readonly float Delay;
        public readonly float Remaining;
        public readonly bool Loop;
        public readonly bool Unscaled;
        public readonly bool IsRunning;
        public readonly double CreatedAt;
        public readonly string StackTrace;

        public TimerDebugInfo(int id, float delay, float remaining, bool loop, bool unscaled, bool isRunning, double createdAt, string stackTrace)
        {
            Id = id;
            Delay = delay;
            Remaining = remaining;
            Loop = loop;
            Unscaled = unscaled;
            IsRunning = isRunning;
            CreatedAt = createdAt;
            StackTrace = stackTrace;
        }

        public TimerDebugInfo WithState(float remaining, bool isRunning)
            => new(Id, Delay, remaining, Loop, Unscaled, isRunning, CreatedAt, StackTrace);

        public TimerDebugInfo WithOptions(TimerOptions options, float remaining, bool isRunning)
            => new(Id, options.Delay, remaining, options.Loop, options.Unscaled, isRunning, CreatedAt, StackTrace);
    }

    public static class TimerDebugBridge
    {
#if UNITY_EDITOR
        private static readonly Dictionary<int, TimerDebugInfo> Timers = new();
#endif

        public static bool Enable { get; set; } = true;
        public static bool CaptureStackTrace { get; set; }

        public static void RecordAdd(int id, TimerOptions options, float remaining, bool isRunning)
        {
#if UNITY_EDITOR
            if (!Enable) return;

            Timers[id] = new TimerDebugInfo(
                id,
                options.Delay,
                remaining,
                options.Loop,
                options.Unscaled,
                isRunning,
                Time.realtimeSinceStartupAsDouble,
                BuildStackTrace());
#endif
        }

        public static void RecordOptions(int id, TimerOptions options, float remaining, bool isRunning)
        {
#if UNITY_EDITOR
            if (!Enable) return;
            if (!Timers.TryGetValue(id, out var oldInfo)) return;

            Timers[id] = oldInfo.WithOptions(options, remaining, isRunning);
#endif
        }

        public static void RecordState(int id, float remaining, bool isRunning)
        {
#if UNITY_EDITOR
            if (!Enable) return;
            if (!Timers.TryGetValue(id, out var oldInfo)) return;

            Timers[id] = oldInfo.WithState(remaining, isRunning);
#endif
        }

        public static void RecordRemove(int id)
        {
#if UNITY_EDITOR
            Timers.Remove(id);
#endif
        }

        public static void RecordClear()
        {
#if UNITY_EDITOR
            Timers.Clear();
#endif
        }

        public static List<TimerDebugInfo> GetSnapshot()
        {
#if UNITY_EDITOR
            var result = new List<TimerDebugInfo>(Timers.Values);
            result.Sort((a, b) => a.Id.CompareTo(b.Id));
            return result;
#else
            return new List<TimerDebugInfo>();
#endif
        }

#if UNITY_EDITOR
        private static string BuildStackTrace()
            => CaptureStackTrace ? Environment.StackTrace : string.Empty;
#endif
    }
}
