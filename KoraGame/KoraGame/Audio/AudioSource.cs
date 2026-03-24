using SDL;
using System.Runtime.Serialization;

namespace KoraGame.Audio
{
    [Serializable]
    public unsafe class AudioSource : Component
    {
        // Private
        [DataMember(Name = "Clip")]
        private AudioClip clip;
        [DataMember(Name = "Volume")]
        private float volume = 1f;
        [DataMember(Name = "Loop")]
        private bool loop = false;
        [DataMember(Name = "SpatialZone")]
        private float spatialZone = 1f;

        // Internal
        internal MIX_Track* track;
        internal SDL_PropertiesID props;

        // Properties
        public bool IsPlaying => SDL3_mixer.MIX_TrackPlaying(track);
        public bool IsPaused => SDL3_mixer.MIX_TrackPaused(track);

        public float Volume
        {
            get => volume;
            set
            {
                volume = Math.Clamp(value, 0f, 1f);
                SDL3_mixer.MIX_SetTrackGain(track, volume);
            }
        }

        public bool Loop
        {
            get => loop;
            set
            {
                loop = value;
                SDL3.SDL_SetNumberProperty(props, SDL3_mixer.MIX_PROP_PLAY_LOOPS_NUMBER, 
                    loop == true ? -1 : 0);
            }
        }

        public float SpatialZone
        {
            get => spatialZone;
            set
            {
                spatialZone = Math.Clamp(value, 0f, 1f);
            }
        }

        public bool Paused
        {
            get => SDL3_mixer.MIX_TrackPaused(track);
            set
            {
                if (value == true)
                {
                    SDL3_mixer.MIX_PauseTrack(track);
                }
                else
                {
                    SDL3_mixer.MIX_ResumeTrack(track);
                }
            }
        }

        public double Time
        {
            get
            {
                return clip != null && clip.audioSpec.freq != 0
                    ? TimeSamples / clip.audioSpec.freq : 0d;
            }
            set
            {
                // Do nothing
                if (clip == null || clip.audioSpec.freq == 0)
                    return;

                // Calculate the sample
                long sample = (long)(value * clip.audioSpec.freq);

                // Clamp
                sample = Math.Clamp(sample, 0L, clip.DurationSamples);

                // Set time samples
                TimeSamples = sample;
            }
        }

        public long TimeSamples
        {
            get => SDL3_mixer.MIX_GetTrackPlaybackPosition(track);
            set
            {
                SDL3_mixer.MIX_SetTrackPlaybackPosition(track, value);
            }
        }

        // Constructor
        public AudioSource()
        {
            // Create track
            track = SDL3_mixer.MIX_CreateTrack(Game.Audio.mixer);

            // Create props
            props = SDL3.SDL_CreateProperties();
        }

        // Methods
        protected override void OnDestroy()
        {            
            if(track != null)
            {
                // Destroy track
                SDL3_mixer.MIX_DestroyTrack(track);
                track = null;

                // Destroy properties
                SDL3.SDL_DestroyProperties(props);
                props = 0;
            }
        }

        public void Play()
        {
            // Check for clip
            if (clip == null)
                return;

            // Apply the clip
            SDL3_mixer.MIX_SetTrackAudio(track, clip.audio);

            // Set volume
            SDL3_mixer.MIX_SetTrackGain(track, volume);

            // Set looping
            SDL3.SDL_SetNumberProperty(props, SDL3_mixer.MIX_PROP_PLAY_LOOPS_NUMBER,
                loop == true ? -1 : 0);

            // Play the track
            SDL3_mixer.MIX_PlayTrack(track, props);
        }

        public void Stop()
        {
            // Stop playing
            SDL3_mixer.MIX_StopTrack(track, 0);
        }
    }
}
