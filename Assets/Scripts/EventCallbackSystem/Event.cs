using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace EventCallbacksSystem
{
    public abstract class Event
    {
        public string Description { get; set; }
    }

    public class DebugEvent : Event
    {
        public int VerbosityLevel { get; set; }
        public string DebugInfo { get; set; }
    }

    public class DeathEvent : Event
    {
        public GameObject GameObjectUnit { get; set; }
    }

    public class TriggerEvent : Event
    {
        private bool isTriggered = true;
        public bool IsTriggered { get { return isTriggered; } }
    }
}