using System.Collections.Generic;
using UnityEngine;

namespace ChickenRush
{
    /// <summary>One scene-owned mixer. Reloading a scene replaces it without duplicate music.</summary>
    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        [Header("可替換正式音效；留空使用程序音效")]
        [SerializeField] private AudioClip nestClip;
        [SerializeField] private AudioClip angryClip;
        [SerializeField] private AudioClip burstClip;
        [SerializeField] private AudioClip bgmClip;
        [SerializeField, Range(0, 1)] private float sfxVolume = .6f;
        [SerializeField, Range(0, 1)] private float bgmVolume = .12f;
        [SerializeField, Range(1, 32)] private int voiceCount = 12;
        [SerializeField] private bool playMusicOnStart = true;
        private AudioSource[] voices;
        private AudioSource music;
        private int nextVoice;
        private readonly List<AudioClip> generated = new List<AudioClip>();
        public AudioSource MusicSource => music;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic() { Instance = null; }
        public static AudioManager EnsureExists()
        {
            if (Instance == null) Instance = FindFirstObjectByType<AudioManager>();
            if (Instance == null) new GameObject("AudioManager").AddComponent<AudioManager>();
            return Instance;
        }
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            voices = new AudioSource[Mathf.Clamp(voiceCount, 1, 32)];
            for (int i = 0; i < voices.Length; i++) voices[i] = CreateSource("SFX " + i);
            music = CreateSource("BGM"); music.loop = true;
            nestClip = Resolve(nestClip, "Nest Chime", 0);
            angryClip = Resolve(angryClip, "Angry Spark", 1);
            burstClip = Resolve(burstClip, "Nest Burst", 2);
            bgmClip = Resolve(bgmClip, "Chicken Theme", 3);
        }
        private AudioClip Resolve(AudioClip clip, string label, int kind)
        {
            if (clip != null) return clip;
            clip = JuiceAudioSynth.Create(label, kind); generated.Add(clip); return clip;
        }
        private AudioSource CreateSource(string label)
        {
            var obj = new GameObject(label); obj.transform.SetParent(transform, false);
            var source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false; source.spatialBlend = 0; source.dopplerLevel = 0;
            return source;
        }
        private void Start() { if (Instance == this && playMusicOnStart) PlayBGM(bgmClip); }
        public static float ComboPitch(int combo) => Mathf.Clamp(1f + Mathf.Max(0, combo) * .05f, 1f, 2f);
        public AudioSource PlayCombo(int combo) => PlaySFX(nestClip, ComboPitch(combo));
        public AudioSource PlayAngry() => PlaySFX(angryClip);
        public AudioSource PlayBurst() => PlaySFX(burstClip, 1, .9f);
        public AudioSource PlaySFX(AudioClip clip, float pitch = 1f, float volume = 1f)
        {
            if (!isActiveAndEnabled || clip == null || voices == null || float.IsNaN(pitch) || float.IsNaN(volume)) return null;
            // Each voice owns its pitch; subsequent combo notes cannot retune earlier notes.
            AudioSource voice = null;
            for (int i = 0; i < voices.Length; i++)
            {
                var candidate = voices[(nextVoice + i) % voices.Length];
                if (!candidate.isPlaying) { voice = candidate; nextVoice = (nextVoice + i + 1) % voices.Length; break; }
            }
            if (voice == null) { voice = voices[nextVoice]; nextVoice = (nextVoice + 1) % voices.Length; }
            voice.Stop(); voice.clip = clip; voice.pitch = Mathf.Clamp(pitch, .1f, 3f);
            voice.volume = sfxVolume * Mathf.Clamp01(volume); voice.Play();
            return voice;
        }
        public void PlayBGM(AudioClip clip, bool loop = true)
        {
            if (!isActiveAndEnabled || clip == null || music == null) return;
            if (music.clip == clip && music.isPlaying && music.loop == loop) return;
            music.clip = clip; music.loop = loop; music.pitch = 1; music.volume = bgmVolume; music.Play();
        }
        public void StopBGM() { if (music != null) music.Stop(); }
        private void OnDisable()
        {
            StopBGM();
            if (voices != null) foreach (var voice in voices) if (voice != null) voice.Stop();
        }
        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            foreach (var clip in generated) if (clip != null) Destroy(clip);
        }
    }
}
