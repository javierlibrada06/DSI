using UnityEngine;

namespace RootsOfLife
{
    /// <summary>
    /// Gestiona la música de fondo y los efectos de sonido.
    /// Conecta con SettingsData para aplicar volúmenes guardados.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Fuentes de audio")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Clips")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameMusic;
        [SerializeField] private AudioClip clickSfx;
        [SerializeField] private AudioClip upgradeSfx;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource   == null) sfxSource   = gameObject.AddComponent<AudioSource>();

            musicSource.loop = true;
        }

        // musica

        public void PlayMenuMusic()  => PlayMusic(menuMusic);
        public void PlayGameMusic()  => PlayMusic(gameMusic);

        private void PlayMusic(AudioClip clip)
        {
            if (clip == null || musicSource.clip == clip) return;
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void StopMusic() => musicSource.Stop();

        // sfx

        public void PlayClick()   => sfxSource.PlayOneShot(clickSfx);
        public void PlayUpgrade() => sfxSource.PlayOneShot(upgradeSfx);

        // volumenn

        public void ApplySettings(SettingsData settings)
        {
            if (settings == null) return;
            musicSource.volume = settings.musicVolume / 100f;
            sfxSource.volume   = settings.sfxVolume   / 100f;
        }

        public void SetMusicVolume(float zeroToHundred)
        {
            musicSource.volume = Mathf.Clamp01(zeroToHundred / 100f);
        }

        public void SetSfxVolume(float zeroToHundred)
        {
            sfxSource.volume = Mathf.Clamp01(zeroToHundred / 100f);
        }
    }
}
