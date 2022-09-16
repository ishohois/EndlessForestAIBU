using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace EventCallbacksSystem
{
    public abstract class Event
    {
        public string Description { get; set; }
    }

    public class UpdatedTerrainInfoEvent : Event
    {

    }

    public class UpdatedNoiseInfoEvent : Event
    {

    }

    public class UpdatedTextureInfoEvent : Event
    {

    }
}