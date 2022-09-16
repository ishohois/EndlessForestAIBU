using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventCallbacksSystem;

public class UpdatableInfo: ScriptableObject 
{
    [SerializeField] protected bool autoUpdate;

    protected EventCallbacksSystem.Event infoEvent;

    protected virtual void OnValidate()
    {
        if (autoUpdate)
        {
            FireUpdatedInfoEvent();
        }
    }

    public void FireUpdatedInfoEvent()
    {
        EventSystem.Instance.FireEvent(infoEvent);
    }
}
