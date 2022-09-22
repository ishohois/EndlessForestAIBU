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

    public class BaseAudioEvent : Event
    {
        public AudioClipType AudioClipType { get; set; }
    }

    public class NarrativeAudioEvent : BaseAudioEvent
    {
       
    }

    public class AmbientAudioEvent : BaseAudioEvent
    {
        
    }

    public class MemoryAudioEvent : BaseAudioEvent
    {
        
    }

    public class DespawnGrassEvent : Event
    {

    }

    public class MemoryCollectedEvent : Event
    {
        public MemoryObjectType MemoryObjectType { get; set; }
    }
}