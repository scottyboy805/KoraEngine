using SDL;

namespace KoraGame.Audio
{
    public unsafe sealed class AudioDevice
    {
        // Internal
        internal const SDL_AudioDeviceID defaultAudioDevice = (SDL_AudioDeviceID)0xFFFFFFFF;
        internal readonly MIX_Mixer* mixer;

        // Properties
        public float MasterVolume
        {
            get => SDL3_mixer.MIX_GetMasterGain(mixer);
            set => SDL3_mixer.MIX_SetMasterGain(mixer, value);
        }

        // Constructor
        public AudioDevice()
        {
            // Create mixer
            mixer = SDL3_mixer.MIX_CreateMixerDevice(defaultAudioDevice, null);
        }

        ~AudioDevice()
        {
            // Release mixer
            SDL3_mixer.MIX_DestroyMixer(mixer);
        }

        // Methods
        public void PlayOnce(AudioClip clip)
        {
            // Check for null
            if (clip == null)
                return;

            // Play the audio
            SDL3_mixer.MIX_PlayAudio(mixer, clip.audio);
        }
    }
}
