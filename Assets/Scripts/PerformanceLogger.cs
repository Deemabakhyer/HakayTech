using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;
using System.Collections.Generic;

/// <summary>
/// A centralized performance measurement utility for HakayTech.
/// Tracks and logs response times for key system operations
/// to support Performance Benchmarks evaluation in Chapter 7.
/// </summary>
public class PerformanceLogger : MonoBehaviour
{
    public static PerformanceLogger Instance;

    private Dictionary<string, List<long>> _measurements = new Dictionary<string, List<long>>();
    private Dictionary<string, Stopwatch> _activeTimers = new Dictionary<string, Stopwatch>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public void StartMeasure(string label)
    {
        if (!_activeTimers.ContainsKey(label))
            _activeTimers[label] = new Stopwatch();

        _activeTimers[label].Restart();
        Debug.Log($"[PERF START] {label}");
    }

    public long StopMeasure(string label)
    {
        if (!_activeTimers.ContainsKey(label)) return -1;

        _activeTimers[label].Stop();
        long ms = _activeTimers[label].ElapsedMilliseconds;

        if (!_measurements.ContainsKey(label))
            _measurements[label] = new List<long>();

        _measurements[label].Add(ms);
        Debug.Log($"[PERF END] {label}: {ms}ms");
        return ms;
    }

    public void PrintSummary()
    {
        Debug.Log("===== PERFORMANCE SUMMARY =====");
        foreach (var entry in _measurements)
        {
            long total = 0;
            foreach (var val in entry.Value) total += val;
            long avg = total / entry.Value.Count;
            Debug.Log($"[{entry.Key}] Runs: {entry.Value.Count} | Avg: {avg}ms");
        }
        Debug.Log("================================");
    }
}