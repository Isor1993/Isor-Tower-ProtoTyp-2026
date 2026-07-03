/*****************************************************************************
* Project : Isors Tower Prototype
* File    : IngameTime.cs
* Date    : xx.xx.2026
* Author  : Eric Rosenberg
*
* Description :
* Provides a global ingame time system for the world simulation. The class
* converts real-time seconds into configurable ingame seconds, minutes, hours
* and days. It exposes the current time values, total seconds within the current
* day and the total number of seconds per day for other systems such as the
* day-night cycle.
*
* History :
* xx.xx.2026 ER Created
******************************************************************************/
using UnityEngine;

/// <summary>
/// Tracks and converts ingame time using configurable conversion rates and a global singleton instance.
/// </summary>
public class IngameTime : MonoBehaviour
{
    /// <summary>
    /// Gets the active global ingame time instance.
    /// </summary>
    public static IngameTime Instance { get; private set; }

    /// <summary>
    /// Gets the current ingame seconds within the active minute.
    /// </summary>
    public int Seconds => _currentSeconds;

    /// <summary>
    /// Gets the current ingame minutes within the active hour.
    /// </summary>
    public int Minutes => _currentMinutes;

    /// <summary>
    /// Gets the current ingame hours within the active day.
    /// </summary>
    public int Hours => _currentHours;

    /// <summary>
    /// Gets the current number of passed ingame days.
    /// </summary>
    public int Days => _currentDays;

    /// <summary>
    /// Gets the total number of ingame seconds that make up one full ingame day.
    /// </summary>
    public int SecondsPerDay =>
     _timeConversionRateHours *
     _timeConversionRateMinutes *
     _timeConversionRateSeconds;

    /// <summary>
    /// Gets the total number of seconds that have passed within the current ingame day.
    /// </summary>
    public int CurrentTotalSeconds =>
    _currentHours * _timeConversionRateMinutes * _timeConversionRateSeconds +
    _currentMinutes * _timeConversionRateSeconds +
    _currentSeconds;

    /// <summary>
    /// Gets or sets the speed multiplier used to advance ingame time.
    /// </summary>
    public float TimeScale
    {
        get => _timeScale;
        set => _timeScale = Mathf.Max(0f, value);
    }

    [Header("Time Settings")]

    [Range(0.01f, 100f)]
    [Tooltip("How many real-time seconds are required for one ingame second to pass.")]
    [SerializeField] private float _realSecondsPerIngameSecond = 1f;
    [Range(1, 100000)]
    [Tooltip("How many ingame seconds are required for one ingame minute.")]
    [SerializeField] private int _timeConversionRateSeconds = 60;
    [Range(1, 100000)]
    [Tooltip("How many ingame minutes are required for one ingame hour.")]
    [SerializeField] private int _timeConversionRateMinutes = 60;
    [Range(1, 100000)]
    [Tooltip("How many ingame hours are required for one ingame day.")]
    [SerializeField] private int _timeConversionRateHours = 24;

    [Header("Debug Time Controls")]
    [SerializeField] private bool _setDebugTime;
    [SerializeField, Range(0, 23)] private int _debugHour;
    [SerializeField, Range(0, 59)] private int _debugMinute;
    [SerializeField, Range(0, 59)] private int _debugSecond;

    private int _currentSeconds;
    private int _currentMinutes;
    private int _currentHours;
    private int _currentDays;
    private int _lastCurrentSecond;

    private float _elapsedTime;
    // TODO später in testing tool
    [Tooltip("Runtime multiplier for advancing ingame time. Values below zero are clamped through the TimeScale property.")]
    [SerializeField] private float _timeScale;

    //TODO Later auslagern
    [Tooltip("Determines whether the ingame time should currently advance.")]
    [SerializeField] private bool _isPlaying=true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _elapsedTime = 0f;
        if (_timeScale <= 0f)
        {
            TimeScale = 1f;
        }
        LoadSavedTime();
    }

    private void Update()
    {
        if (_setDebugTime)
        {
            _setDebugTime = false;
            SetTime(_debugHour, _debugMinute, _debugSecond);
            return;
        }

        if (!_isPlaying)
        {
            return;
        }
        _elapsedTime += Time.deltaTime * _timeScale;

        while (_elapsedTime >= _realSecondsPerIngameSecond)
        {
            _currentSeconds += 1;
            _elapsedTime -= _realSecondsPerIngameSecond;
        }

        ConvertTimeUnit(ref _currentSeconds, _timeConversionRateSeconds, ref _currentMinutes);
        ConvertTimeUnit(ref _currentMinutes, _timeConversionRateMinutes, ref _currentHours);
        ConvertTimeUnit(ref _currentHours, _timeConversionRateHours, ref _currentDays);

#if UNITY_EDITOR
        if (_currentSeconds != _lastCurrentSecond)
        {
            // Debug.Log($"Day : {_currentDays} Time:[{_currentHours}:{_currentMinutes}:{_currentSeconds}]");
            _lastCurrentSecond = _currentSeconds;
        }
#endif
    }

    /// <summary>
    /// Sets the current ingame time to a specific day, hour, minute, and second.
    /// Values are clamped to the configured time conversion ranges.
    /// </summary>
    /// <param name="days">The ingame day value to set.</param>
    /// <param name="hours">The ingame hour value to set.</param>
    /// <param name="minutes">The ingame minute value to set.</param>
    /// <param name="seconds">The ingame second value to set.</param>
    public void SetTime(int days, int hours, int minutes, int seconds)
    {
        _currentDays = Mathf.Max(0, days);
        _currentHours = Mathf.Clamp(hours, 0, _timeConversionRateHours - 1);
        _currentMinutes = Mathf.Clamp(minutes, 0, _timeConversionRateMinutes - 1);
        _currentSeconds = Mathf.Clamp(seconds, 0, _timeConversionRateSeconds - 1);

        _elapsedTime = 0f;
    }

    /// <summary>
    /// Sets the current ingame time to a specific hour, minute, and second while keeping the current day.
    /// </summary>
    /// <param name="hours">The ingame hour value to set.</param>
    /// <param name="minutes">The ingame minute value to set.</param>
    /// <param name="seconds">The ingame second value to set.</param>
    public void SetTime(int hours, int minutes, int seconds)
    {
        SetTime(_currentDays, hours, minutes, seconds);
    }

    /// <summary>
    /// Sets the current ingame hour while keeping the current day, minute, and second.
    /// </summary>
    /// <param name="hours">The ingame hour value to set.</param>
    public void SetHour(int hours)
    {
        SetTime(_currentDays, hours, _currentMinutes, _currentSeconds);
    }


    /// <summary>
    /// aves the current ingame time state.
    /// </summary>
    private void SaveTime()
    {
        //TODO
#if (UNITY_EDITOR)
        Debug.Log("Saved game !");
#endif
      
    }

    /// <summary>
    /// Converts one time unit into the next higher unit when the configured conversion rate is reached.
    /// </summary>
    /// <param name="currentUnit">The current time unit that should be converted when it reaches the conversion rate.</param>
    /// <param name="conversionRate">The amount of the current unit required to increase the next unit.</param>
    /// <param name="nextUnit">The next higher time unit that receives converted values.</param>
    private void ConvertTimeUnit(ref int currentUnit, int conversionRate, ref int nextUnit)
    {
        while (currentUnit >= conversionRate)
        {
            currentUnit -= conversionRate;
            nextUnit += 1;
        }
    }

    /// <summary>
    /// Loads the saved ingame time state.
    /// </summary>
    private void LoadSavedTime()
    {
        //TODO 
#if (UNITY_EDITOR)
        Debug.Log("Load saved Time !");
#endif

        SetTime(6, 0, 0);

    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SaveTime();
            Instance = null;
        }
    }

    private void OnApplicationQuit()
    {
        if (Instance == this)
        {
            SaveTime();
        }
    }
}