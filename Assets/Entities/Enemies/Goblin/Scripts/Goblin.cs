/*****************************************************************************
* Project : Isors Tower Prototype
* File    : Goblin.cs
* Date    : xx.xx.2026
* Author  : Eric Rosenberg
*
* Description :
* 
* 
* 
*
* History :
* xx.xx.2026 ER Created
******************************************************************************/
using UnityEngine;

public class Goblin : MonoBehaviour, IDayNightListener
{
   [SerializeField] private DayNightCycleEventManager _eventManager;

   

    private void OnEnable()
    {
        if (_eventManager == null)
        {
            return;
        }

        _eventManager.Subscribe(this);
    }

    private void OnDisable()
    {
        if (_eventManager == null)
        {
            return;
        }

        _eventManager.Unsubscribe(this);
    }

    public void OnDayPhaseChanged(DayPhase previousphase, DayPhase currentPhase)
    {
        Debug.Log("Goblin reacting on changed DayPhase");
    }
}