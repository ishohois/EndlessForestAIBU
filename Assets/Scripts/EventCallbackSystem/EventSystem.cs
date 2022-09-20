using System;
using System.Collections.Generic;
using UnityEngine;


namespace EventCallbacksSystem
{
    public class EventSystem : MonoBehaviour
    {
        public delegate void EventListener(Event e);
        private Dictionary<Type, HashSet<EventListener>> eventListeners = new Dictionary<Type, HashSet<EventListener>>();

        private static EventSystem instance;
        public static EventSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<EventSystem>();
                }
                return instance;
            }
        }

        private void OnEnable()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }

        }

        public bool HasRegisteredListener<T>(Action<T> listener) where T : Event
        {
            return eventListeners.ContainsKey(typeof(T));
        }

        public void RegisterListener<T>(Action<T> listener) where T : Event
        {
            Type eventType = typeof(T);
            EventListener wrapper = (ev) => { listener((T)ev); };
            if (!eventListeners.ContainsKey(eventType))
            {
                eventListeners.Add(eventType, new HashSet<EventListener>());
            }
            eventListeners[eventType].Add(wrapper);
        }

        public void UnregisterListener<T>(Action<T> listener) where T : Event
        {
            Type eventType = typeof(T);
            EventListener wrapper = (ev) => { listener((T)ev); };
            if (!eventListeners.ContainsKey(eventType) || !eventListeners[eventType].Contains(wrapper))
            {
                return;
            }
            eventListeners[eventType].Remove(wrapper);
        }
        
        public void FireEvent(Event e)
        {
            Type eventType = e.GetType();
            if (eventListeners[eventType] == null)
            {
                return;
            }

            foreach (EventListener eventListener in eventListeners[eventType])
            {
                eventListener(e);
            }
        }
    }
}