using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ChickenRush
{
    // Local preview/controller. Ownership and durable writes stay in InventoryManager.
    public sealed class HomeNestPrototype : MonoBehaviour
    {
        private FurnitureEditSession session;
        private string selectedId;
        private InventoryManager inventory;
        private GameObject world;
        private Camera viewCamera;
        private RenderTexture target;
        private RawImage viewport;
        private readonly List<Material> materials = new List<Material>();
        private readonly Dictionary<string, GameObject> furniture = new Dictionary<string, GameObject>();
        public IHomeDecorationPreview AgentPreview => new PreviewPort(this);
        private sealed class PreviewPort : IHomeDecorationPreview
        {
            private readonly HomeNestPrototype home;
            public PreviewPort(HomeNestPrototype owner) { home=owner; }
            public bool Preview(IReadOnlyList<PlacementCommand> commands) => home.PreviewCommands(commands);
        }
        private float yaw, distance = 11;
        public Camera ViewCamera => viewCamera;
        public GameObject World => world;
        public float Yaw => yaw;
        public float Distance => distance;
        public int FurnitureLimit => inventory.HomeFurnitureLimit;
        public PlacedHomeItemData Draft => session?.Snapshot.placedFurniture.FirstOrDefault(x=>x.itemId==selectedId)?.Copy();
        public bool HasUnsavedChanges => session != null && session.IsDirty;
        public HomeNestData PreviewState => session?.Snapshot;
        public bool IsFurnitureSaved(string id) => inventory.GetActiveHomeFurniture().Any(x=>x.itemId==id);
        public event System.Action Changed;
        private const int Layer = 30;
        public void Initialize(InventoryManager owner, RawImage image) { inventory = owner; viewport = image; }
        public IReadOnlyList<ItemDataSO> OwnedFurniture => Resources.LoadAll<ItemDataSO>("Items")
            .Where(x => x != null && x.IsValid).GroupBy(x => x.itemId)
            .Where(g => g.Count() == 1).Select(g => g.First())
            .Where(x => x.itemType == ItemType.Furniture && inventory.OwnsItem(x))
            .OrderBy(x => x.itemId, System.StringComparer.Ordinal).ToList().AsReadOnly();
        public void Enter()
        {
            Exit();
            world = new GameObject("MyNest3DEnvironment"); world.transform.position = new Vector3(1000, 0, 1000);
            Part("Floor", world.transform, new Vector3(0,-.15f,0), new Vector3(8,.3f,7), new Color(.65f,.47f,.28f));
            Part("BackWall",world.transform,new Vector3(0,1.3f,3.5f),new Vector3(8,2.6f,.2f),new Color(.55f,.72f,.72f));
            Part("LeftWall",world.transform,new Vector3(-4,.6f,0),new Vector3(.2f,1.2f,7),new Color(.55f,.72f,.72f));
            Part("RightWall",world.transform,new Vector3(4,.6f,0),new Vector3(.2f,1.2f,7),new Color(.55f,.72f,.72f));
            Character("ChickenPlaceholder", new Vector3(-2,0,1.5f), false);
            var penguin = Character("PenguinPlaceholder", new Vector3(2,0,1.5f), true);
            var butler = inventory.GetActiveHomeButler();
            if (butler != null && butler.model3DPrefab != null)
            {
                penguin.SetActive(false); Representation(butler, world.transform, new Vector3(2,0,1.5f));
            }
            var lightObject = new GameObject("HomeLight", typeof(Light)); lightObject.transform.SetParent(world.transform, false);
            var light = lightObject.GetComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.2f;
            light.cullingMask = 1 << Layer; light.shadows = LightShadows.None; light.transform.rotation = Quaternion.Euler(45,-35,0);
            var cameraObject = new GameObject("HomeCamera", typeof(Camera)); cameraObject.transform.SetParent(world.transform, false);
            viewCamera = cameraObject.GetComponent<Camera>(); viewCamera.cullingMask = 1 << Layer;
            viewCamera.clearFlags = CameraClearFlags.SolidColor; viewCamera.backgroundColor = new Color(.08f,.15f,.2f);
            viewCamera.nearClipPlane = .1f; viewCamera.farClipPlane = 40; viewCamera.fieldOfView = 48;
            target = new RenderTexture(1024,Mathf.Max(1,Mathf.RoundToInt(1024f*viewport.rectTransform.rect.height/Mathf.Max(1,viewport.rectTransform.rect.width))),16); target.Create(); viewCamera.targetTexture = target;
            viewport.texture = target; yaw = 0; distance = 11; UpdateCamera();
            session=new FurnitureEditSession(inventory); RefreshPreview();
        }
        public void Exit()
        {
            session = null; selectedId = null; furniture.Clear();
            if (viewport != null) viewport.texture = null;
            if (viewCamera != null) { viewCamera.enabled = false; viewCamera.targetTexture = null; }
            if (world != null) { world.SetActive(false); Destroy(world); } world = null; viewCamera = null;
            if (target != null) { target.Release(); Destroy(target); target = null; }
            foreach (var material in materials) if (material != null) Destroy(material); materials.Clear();
        }
        private void OnDisable() { Exit(); }
        private void OnDestroy() { Exit(); }
        public void Orbit(float degrees) { if (!Finite(degrees)) return; yaw = Mathf.Clamp(yaw + degrees,-55,55); UpdateCamera(); }
        public void Zoom(float amount) { if (!Finite(amount)) return; distance = Mathf.Clamp(distance + amount,9,14); UpdateCamera(); }
        private void UpdateCamera()
        {
            if (viewCamera == null) return;
            var focus = world.transform.position + new Vector3(0,.3f,0);
            viewCamera.transform.position = focus + Quaternion.Euler(35,yaw,0) * new Vector3(0,0,-distance);
            viewCamera.transform.LookAt(focus);
        }
        public bool SelectFurniture(string itemId)
        {
            if(world==null || session==null || session.Snapshot.version!=HomeNestData.CurrentVersion || !inventory.IsOwnedFurniture(itemId)) return false;
            var existing=session.Snapshot.placedFurniture.FirstOrDefault(x=>x.itemId==itemId);
            if(existing==null && !session.Preview(new[]{new PlacementCommand { operation=PlacementOperation.PlaceFurniture,itemId=itemId }})) return false;
            selectedId=itemId; RefreshPreview(); return true;
        }
        public bool PreviewCommands(IReadOnlyList<PlacementCommand> commands)
        {
            if(session==null || !session.Preview(commands)) return false;
            RefreshPreview(); return true;
        }
        public bool MoveSelected(Vector3 position) => Draft!=null && PreviewCommands(new[]{new PlacementCommand
            { operation=PlacementOperation.MoveFurniture,itemId=selectedId,position=position }});
        public void RotateSelected(float degrees)
        {
            var draft=Draft;
            if(draft!=null && Finite(degrees)) PreviewCommands(new[]{new PlacementCommand
                { operation=PlacementOperation.RotateFurniture,itemId=selectedId,yaw=draft.rotation.y+degrees }});
        }
        public bool RemoveSelected() => Draft!=null && PreviewCommands(new[]{new PlacementCommand
            { operation=PlacementOperation.RemoveFurniture,itemId=selectedId }});
        public void CancelEdit() { session?.Cancel(); RefreshPreview(); }
        public bool UndoEdit() { if(session==null || !session.Undo()) return false; RefreshPreview(); return true; }
        // Kept as the Phase 8E compatibility name; only the player's Confirm button invokes this.
        public bool SaveSelected()
        {
            if(session==null || !session.Confirm()) return false;
            RefreshPreview(); return true;
        }
        private void RefreshPreview()
        {
            if(session==null || world==null) return;
            foreach(var obj in furniture.Values) obj.SetActive(false);
            foreach(var saved in session.Snapshot.placedFurniture.Where(x=>session.Snapshot.version==HomeNestData.CurrentVersion && InArea(x) && inventory.IsOwnedFurniture(x.itemId)).Take(FurnitureLimit))
            {
                var item=OwnedFurniture.FirstOrDefault(x=>x.itemId==saved.itemId);
                if(item==null) continue;
                Show(item,saved);
                furniture[saved.itemId].transform.Find("SelectionMarker").gameObject.SetActive(saved.itemId==selectedId);
            }
            Changed?.Invoke();
        }
        public string PickFurniture(Vector2 uv)
        {
            if(viewCamera==null || !Finite(uv.x) || !Finite(uv.y)) return null;
            var ray=viewCamera.ViewportPointToRay(uv); string picked=null; float nearest=float.MaxValue;
            foreach(var entry in furniture)
            {
                if(!entry.Value.activeSelf) continue;
                foreach(var renderer in entry.Value.GetComponentsInChildren<Renderer>())
                                    {
                    // Mesh-local bounds remain valid even before Unity updates world renderer bounds.
                    var mesh=renderer.GetComponent<MeshFilter>();
                    var bounds=mesh!=null && mesh.sharedMesh!=null ? mesh.sharedMesh.bounds : renderer.localBounds;
                    var localRay=new Ray(renderer.transform.InverseTransformPoint(ray.origin),renderer.transform.InverseTransformVector(ray.direction));
                    if(!bounds.IntersectRay(localRay,out float hit)) continue;
                    float distance=Vector3.Distance(ray.origin,renderer.transform.TransformPoint(localRay.GetPoint(hit)));
                    if(distance<nearest) { nearest=distance; picked=entry.Key; }
                }
            }
            return picked;
        }
        public void PlaceAtViewport(Vector2 uv)
        {
            if (viewCamera == null || !Finite(uv.x) || !Finite(uv.y) || uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) return;
            var ray = viewCamera.ViewportPointToRay(uv);
            if (new Plane(Vector3.up,world.transform.position).Raycast(ray,out float hit)) MoveSelected(ray.GetPoint(hit)-world.transform.position);
        }
        public GameObject GetFurniture(string id) => id != null && furniture.TryGetValue(id,out var obj) && obj.activeSelf ? obj : null;
        private static bool Finite(float x) => !float.IsNaN(x) && !float.IsInfinity(x);
        private static bool InArea(PlacedHomeItemData p) => PlacementRules.InArea(p);
        private void Show(ItemDataSO item, PlacedHomeItemData data)
        {
            if (!furniture.TryGetValue(item.itemId,out var obj))
            {
                obj = new GameObject("Furniture:"+item.itemId); obj.transform.SetParent(world.transform,false);
                if (item.model3DPrefab != null) Representation(item,obj.transform,Vector3.zero);
                else
                {
                    Part("TableTop",obj.transform,new Vector3(0,.75f,0),new Vector3(1.2f,.18f,.8f),new Color(.86f,.48f,.24f));
                    foreach (float x in new[]{-.45f,.45f}) foreach (float z in new[]{-.25f,.25f})
                        Part("Leg",obj.transform,new Vector3(x,.35f,z),new Vector3(.12f,.7f,.12f),new Color(.45f,.26f,.14f));
                }
                Part("SelectionMarker",obj.transform,new Vector3(0,.015f,0),new Vector3(1.35f,.02f,1.05f),new Color(.2f,.9f,.6f));
                furniture.Add(item.itemId,obj);
            }
            obj.SetActive(true); Apply(obj,data);
        }
        private static void Apply(GameObject obj, PlacedHomeItemData p)
        { obj.transform.localPosition=p.position; obj.transform.localEulerAngles=p.rotation; obj.transform.localScale=p.scale; }
        private void Representation(ItemDataSO item,Transform parent,Vector3 position)
        {
            var obj=Instantiate(item.model3DPrefab,parent,false); obj.transform.localPosition=position;
            foreach(var t in obj.GetComponentsInChildren<Transform>(true)) t.gameObject.layer=Layer;
            foreach(var collider in obj.GetComponentsInChildren<Collider>(true)) collider.enabled=false;
            foreach(var body in obj.GetComponentsInChildren<Rigidbody>(true)) { body.isKinematic=true; body.detectCollisions=false; }
            // Fit the supplied visual into the same one-unit footprint as the placeholder.
            var renderers=obj.GetComponentsInChildren<Renderer>();
            if(renderers.Length>0)
            {
                var bounds=renderers[0].bounds; foreach(var r in renderers) bounds.Encapsulate(r.bounds);
                float size=Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);
                if(size>.001f) obj.transform.localScale*=1.2f/size;
                bounds=renderers[0].bounds; foreach(var r in renderers) bounds.Encapsulate(r.bounds);
                obj.transform.position+= new Vector3(parent.position.x+position.x-bounds.center.x,parent.position.y+position.y-bounds.min.y,parent.position.z+position.z-bounds.center.z);
            }
        }
        private GameObject Character(string name,Vector3 position,bool penguin)
        {
            var root=new GameObject(name); root.transform.SetParent(world.transform,false); root.transform.localPosition=position;
            Part("Body",root.transform,new Vector3(0,.55f,0),new Vector3(.75f,1,.65f),penguin?new Color(.08f,.12f,.19f):Color.yellow,PrimitiveType.Sphere);
            if(penguin) Part("Belly",root.transform,new Vector3(0,.5f,-.27f),new Vector3(.5f,.65f,.15f),Color.white,PrimitiveType.Sphere);
            Part("Beak",root.transform,new Vector3(0,.82f,-.4f),new Vector3(.23f,.13f,.25f),new Color(1,.45f,.05f));
            foreach(float x in new[]{-.17f,.17f}) Part("Eye",root.transform,new Vector3(x,.93f,-.28f),Vector3.one*.1f,Color.black,PrimitiveType.Sphere);
            return root;
        }
        private void Part(string name,Transform parent,Vector3 position,Vector3 scale,Color color,PrimitiveType type=PrimitiveType.Cube)
        {
            var obj=GameObject.CreatePrimitive(type); obj.name=name; obj.layer=Layer; obj.transform.SetParent(parent,false);
            obj.transform.localPosition=position; obj.transform.localScale=scale; obj.GetComponent<Collider>().enabled=false;
            var shader=UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline == null ? Shader.Find("Standard") : Shader.Find("Universal Render Pipeline/Lit");
            shader = shader ?? Shader.Find("Unlit/Color");
            var material=new Material(shader); material.color=color; materials.Add(material); obj.GetComponent<Renderer>().sharedMaterial=material;
        }
    }
}
