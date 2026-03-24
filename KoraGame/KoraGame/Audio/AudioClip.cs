using SDL;
using System.Text;

namespace KoraGame.Audio
{
    public unsafe sealed class AudioClip : GameElement
    {
        // Internal
        internal MIX_Audio* audio;
        internal SDL_AudioSpec audioSpec;

        // Properties
        public double Duration => audioSpec.freq != 0 ? (double)DurationSamples / audioSpec.freq : 0d;
        public long DurationSamples => SDL3_mixer.MIX_GetAudioDuration(audio);

        // Methods
        protected override void OnDestroy()
        {            
            if(audio != null)
            {
                SDL3_mixer.MIX_DestroyAudio(audio);
                audio = null;
            }
        }

        internal override void CloneInstantiate(GameElement element)
        {
            base.CloneInstantiate(element);

            // Get the clone
            AudioClip clone = (AudioClip)element;

            clone.audio = audio;
            clone.audioSpec = audioSpec;
        }

        public static AudioClip LoadWav(AudioDevice device, string path)
        {
            // Create the clip
            AudioClip clip = new();
            
            // Convert string to null-terminated UTF-8 bytes
            byte[] pathBytes = Encoding.UTF8.GetBytes(path + '\0');

            fixed (byte* pathPtr = pathBytes)
            {
                // Load wav
                clip.audio = SDL3_mixer.MIX_LoadAudio(device.mixer, pathPtr, false);
            }

            // Use file name
            clip.Name = Path.GetFileNameWithoutExtension(path);

            // Get the spec
            if(clip.audio != null)
            {
                // Try to get the format
                SDL_AudioSpec spec = default;
                SDL3_mixer.MIX_GetAudioFormat(clip.audio, &spec);

                // Store the format
                clip.audioSpec = spec;
            }
            
            return clip;
        }
    }
}
