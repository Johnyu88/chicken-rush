using System.Collections.Generic;
using UnityEngine;

namespace ChickenRush
{
    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    public sealed class EffectManager : MonoBehaviour
    {
        public static EffectManager Instance { get; private set; }
        [SerializeField] private ParticleSystem featherPrefab;
        [SerializeField] private ParticleSystem angerPrefab;
        [SerializeField, Range(1, 48)] private int maxBursts = 24;
        private readonly Queue<ParticleSystem> bursts = new Queue<ParticleSystem>();
        private readonly System.Random random = new System.Random();
        private Material material;
        private Material sparkMaterial;
        private Texture2D sparkTexture;
        private Texture2D featherTexture;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic() { Instance = null; }
        public static EffectManager EnsureExists()
        {
            if (Instance == null) Instance = FindFirstObjectByType<EffectManager>();
            if (Instance == null) new GameObject("EffectManager").AddComponent<EffectManager>();
            return Instance;
        }
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            featherTexture = new Texture2D(16, 32, TextureFormat.RGBA32, false);
            featherTexture.name = "Procedural Feather";
            var pixels = new Color[16 * 32];
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 16; x++)
                {
                    float width = Mathf.Sin(Mathf.PI * y / 31f) * 5;
                    bool vane = y > 5 && Mathf.Abs(x - 7.5f) < width && (x + y) % 5 != 0;
                    bool stem = Mathf.Abs(x - 7.5f) < 1 && y < 29;
                    pixels[y * 16 + x] = vane || stem ? Color.white : Color.clear;
                }
            featherTexture.SetPixels(pixels); featherTexture.Apply();
            material = new Material(Shader.Find("Sprites/Default"));
            material.name = "Juice Feather Material"; material.mainTexture = featherTexture;
            sparkTexture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            sparkTexture.name = "Procedural Spark";
            var sparks = new Color[256];
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    float distance = Mathf.Abs(x - 7.5f) / 4f + Mathf.Abs(y - 7.5f) / 8f;
                    sparks[y * 16 + x] = new Color(1, 1, 1, Mathf.Clamp01(1.5f - distance));
                }
            sparkTexture.SetPixels(sparks); sparkTexture.Apply();
            sparkMaterial = new Material(Shader.Find("Sprites/Default"));
            sparkMaterial.name = "Juice Spark Material"; sparkMaterial.mainTexture = sparkTexture;
        }
        private ParticleSystem Create(string label, Transform parent, bool loop)
        {
            var obj = new GameObject(label); obj.transform.SetParent(parent, false);
            var system = obj.AddComponent<ParticleSystem>(); system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main; main.playOnAwake = false; main.loop = loop;
            main.duration = 1; main.startLifetime = new ParticleSystem.MinMaxCurve(.35f, .8f);
            main.startSize = new ParticleSystem.MinMaxCurve(.08f, .16f);
            main.startSpeed = .6f; main.maxParticles = 64;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Shape;
            main.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1, .8f, .2f), Color.white);
            var emission = system.emission; emission.enabled = loop; emission.rateOverTime = 20;
            var shape = system.shape; shape.enabled = loop; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = .8f; shape.radiusThickness = .1f;
            var fade = system.colorOverLifetime; fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
            fade.color = gradient;
            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = loop ? sparkMaterial : material; renderer.sortingOrder = 20;
            return system;
        }
        public ParticleSystem PlayFeathers(Vector3 position, bool burst = false)
        {
            if (!isActiveAndEnabled) return null;
            while (bursts.Count > 0 && bursts.Peek() == null) bursts.Dequeue();
            while (bursts.Count >= Mathf.Max(1, maxBursts))
            {
                var oldest = bursts.Dequeue();
                if (oldest != null) { oldest.gameObject.SetActive(false); Destroy(oldest.gameObject); }
            }
            var system = featherPrefab != null ? Instantiate(featherPrefab, position, Quaternion.identity, transform) : Create("Feather Burst", transform, false);
            system.transform.position = position;
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main; main.loop = false; main.stopAction = ParticleSystemStopAction.Destroy;
            main.gravityModifier = .25f;
            var emission = system.emission; emission.enabled = false;
            system.Play();
            for (int i = 0; i < (burst ? 36 : 12); i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI;
                float speed = (burst ? 2.8f : 1.2f) * (.5f + (float)random.NextDouble());
                var particle = new ParticleSystem.EmitParams { velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * speed };
                system.Emit(particle, 1);
            }
            bursts.Enqueue(system); return system;
        }
        public ParticleSystem AttachAnger(Transform chicken)
        {
            if (!isActiveAndEnabled || chicken == null) return null;
            var system = angerPrefab != null ? Instantiate(angerPrefab, chicken, false) : Create("Anger Aura", chicken, true);
            system.transform.localPosition = Vector3.zero;
            if (angerPrefab == null)
            {
                var main = system.main;
                main.startColor = new ParticleSystem.MinMaxGradient(new Color(1, .2f, .02f), new Color(1, .8f, .1f));
                main.gravityModifier = -.06f;
            }
            system.Play(); return system;
        }
        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (material != null) Destroy(material);
            if (featherTexture != null) Destroy(featherTexture);
            if (sparkMaterial != null) Destroy(sparkMaterial);
            if (sparkTexture != null) Destroy(sparkTexture);
        }
    }
}

