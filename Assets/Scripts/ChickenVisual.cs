using UnityEngine;

namespace ChickenRush
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ChickenAnger))]
    public sealed class ChickenVisual : MonoBehaviour
    {
        public enum VisualState { Normal, Angry, Happy }
        [SerializeField] private Sprite normalSprite, angrySprite, happySprite;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private SpriteRenderer bodyRenderer, costumeAnchor;
        [SerializeField, Min(.01f)] private float pulseDuration = .24f;
        [SerializeField, Range(0, .5f)] private float pulseAmount = .15f;
        private ChickenAnger anger;
        private float pulseTime;
        private bool pulsing, celebrating;
        private Vector3 baseScale = Vector3.one;
        public VisualState State { get; private set; }
        public SpriteRenderer BodyRenderer => bodyRenderer;
        public SpriteRenderer CostumeAnchor => costumeAnchor;
        public Transform VisualRoot => visualRoot;

        public void SetSprites(Sprite normal, Sprite angry, Sprite happy)
        {
            normalSprite = normal; angrySprite = angry; happySprite = happy;
            Configure();
        }
        private void ApplySprite(Sprite sprite)
        {
            bodyRenderer.sprite = sprite;
            float scale = ChickenArtSet.UnitScale(sprite);
            bodyRenderer.transform.localScale = Vector3.one * scale;
            bodyRenderer.transform.localPosition = sprite != null ? -sprite.bounds.center * scale : Vector3.zero;
        }
        // Also used by the prefab migration; only visual children are created or changed.
        public void Configure()
        {
            var art = ChickenArtSet.Load();
            if (normalSprite == null && art != null) normalSprite = art.normal;
            if (angrySprite == null && art != null) angrySprite = art.angry;
            if (happySprite == null && art != null) happySprite = art.happy;
            if (visualRoot == null)
            {
                visualRoot = transform.Find("VisualRoot");
                if (visualRoot == null) { visualRoot = new GameObject("VisualRoot").transform; visualRoot.SetParent(transform, false); }
            }
            if (bodyRenderer == null) bodyRenderer = ChildRenderer("Body");
            if (costumeAnchor == null) costumeAnchor = ChildRenderer("CostumeAnchor");
            var legacy = GetComponent<SpriteRenderer>();
            if (legacy != null)
            {
                bodyRenderer.sortingLayerID = legacy.sortingLayerID;
                bodyRenderer.sortingOrder = legacy.sortingOrder;
                if (normalSprite != null) { legacy.sprite = normalSprite; legacy.enabled = false; }
            }
            ApplySprite(normalSprite); bodyRenderer.color = Color.white;
            costumeAnchor.sortingLayerID = bodyRenderer.sortingLayerID;
            costumeAnchor.sortingOrder = bodyRenderer.sortingOrder + 1;
            costumeAnchor.enabled = costumeAnchor.sprite != null;
        }
        private SpriteRenderer ChildRenderer(string label)
        {
            var child = visualRoot.Find(label);
            if (child == null) { child = new GameObject(label).transform; child.SetParent(visualRoot, false); }
            var renderer = child.GetComponent<SpriteRenderer>();
            return renderer != null ? renderer : child.gameObject.AddComponent<SpriteRenderer>();
        }
        private void Awake() { Configure(); baseScale = visualRoot.localScale; anger = GetComponent<ChickenAnger>(); }
        private void OnEnable()
        {
            if (anger == null) anger = GetComponent<ChickenAnger>();
            anger.OnAngerStateChanged += OnAngerStateChanged;
            SetState(celebrating ? VisualState.Happy : anger.isAngry ? VisualState.Angry : VisualState.Normal, false);
        }
        private void OnAngerStateChanged(bool angry) { if (!celebrating) SetState(angry ? VisualState.Angry : VisualState.Normal, angry); }
        public void Celebrate() { if (celebrating) return; celebrating = true; SetState(VisualState.Happy, true); }
        private void SetState(VisualState state, bool pulse)
        {
            State = state;
            if (bodyRenderer == null) return;
            var sprite = state == VisualState.Happy ? happySprite : state == VisualState.Angry ? angrySprite : normalSprite;
            ApplySprite(sprite != null ? sprite : normalSprite);
            if (pulse) { pulseTime = 0; pulsing = true; }
        }
        private void Update()
        {
            if (!pulsing) return;
            pulseTime += Time.deltaTime;
            float t = Mathf.Clamp01(pulseTime / Mathf.Max(.01f, pulseDuration));
            visualRoot.localScale = baseScale * (1 + pulseAmount * Mathf.Sin(t * Mathf.PI));
            if (t >= 1) { pulsing = false; visualRoot.localScale = baseScale; }
        }
        private void OnDisable()
        {
            if (anger != null) anger.OnAngerStateChanged -= OnAngerStateChanged;
            pulsing = false;
            if (visualRoot != null) visualRoot.localScale = baseScale;
        }
    }
}
