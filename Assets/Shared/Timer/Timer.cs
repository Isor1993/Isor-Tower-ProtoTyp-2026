using UnityEngine;
public class Timer
{
    private float _currentTime;
    public float CurrentTime => _currentTime;
    public void Tick(float deltaTime)
    {
        _currentTime += deltaTime;
    }
    public void Reset()
    {
        _currentTime = 0f;
    }
    public bool IsFinished(float duration) 
    {
        return _currentTime >= duration; 
    }
}