/*****************************************************************************
* Project : Isors Tower Prototype
* File    : SkyController.cs
* Date    : xx.xx.2026
* Author  : Eric Rosenberg
*
* Description :
* Controls the visual rotation of the celestial pivot based on the current
* day progress provided by the DayNightCycle. This script is responsible for
* moving sky-related objects, such as the sun and moon, through a full daily
* rotation while allowing an inspector-based rotation offset.
*
* History :
* xx.xx.2026 ER Created
******************************************************************************/
using UnityEngine;
/// <summary>
/// Rotates a celestial pivot according to the current day progress of the day-night cycle.
/// </summary>
public class SkyController : MonoBehaviour
{
    private const float FULL_ROTATION_DEGREES = 360f;

    [Header("Settings")]
    [Tooltip("The transform used as the pivot for rotating celestial objects such as the sun and moon.")]
    [SerializeField] private Transform _celestialPivot;
    [Tooltip("Reference to the day-night cycle that provides the current normalized day progress.")]
    [SerializeField] private DayNightCycle _dayNightCycle;

    [Tooltip("Additional rotation offset in degrees used to align the celestial cycle visually.")]
    [SerializeField] private float _rotationOffset = -90f;

    [Header("Debug (Runtime)")]
    [Tooltip("The current calculated rotation angle of the celestial pivot in degrees.")]
    [SerializeField] private float _currentAngle;
    [Tooltip("The current normalized day progress received from the day-night cycle.")]
    [SerializeField] private float _dayProgress;

    private void Awake()
    {
        if (_celestialPivot == null)
        {
            Debug.LogError("Celestial pivot is missing.");
            enabled = false;
            return;
        }

        if (_dayNightCycle == null)
        {
            Debug.LogError("DayNightCycle reference is missing.");
            enabled = false;
            return;
        }
    }
    private void Update()
    {
        _dayProgress = _dayNightCycle.DayProgress;
        UpdateCelestialRotation();
    }

    /// <summary>
    /// Calculates and applies the celestial pivot rotation based on the current day progress.
    /// </summary>
    private void UpdateCelestialRotation()
    {
        _currentAngle =
            (_dayProgress * FULL_ROTATION_DEGREES) +
            _rotationOffset;

        _currentAngle %= FULL_ROTATION_DEGREES;

        _celestialPivot.localRotation =
            Quaternion.Euler(-_currentAngle, 0f, 0f);
    }
}