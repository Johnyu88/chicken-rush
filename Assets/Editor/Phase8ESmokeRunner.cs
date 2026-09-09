using System;
using System.Linq;
using System.Reflection;
using ChickenRush;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class Phase8ESmokeRunner
{
    const string Key="ChickenRush.Tests.Phase8E";
    static int frames;
    static Phase8ESmokeRunner() { if(SessionState.GetBool(Key,false)) EditorApplication.update+=Tick; }
    public static void Run()
    {
        MvpSceneBuilder.Build();
        var serialized=new SerializedObject(Object.FindFirstObjectByType<InventoryManager>());
        serialized.FindProperty("playerPrefsKey").stringValue=Key; serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        PlayerPrefs.DeleteKey(Key+".backup"); PlayerPrefs.SetString(Key,"{\"coins\":2000}"); PlayerPrefs.Save();
        SessionState.SetBool(Key,true); EditorApplication.EnterPlaymode();
    }
    static void Check(bool result,string message) { if(!result) throw new Exception(message); }
    static void Tick()
    {
        if(!EditorApplication.isPlaying || ++frames<8) return;
        try
        {
            var inventory=Object.FindFirstObjectByType<InventoryManager>(); var menu=Object.FindFirstObjectByType<MainMenuCanvas>();
            var ui=menu.GetComponentInChildren<MyNestCanvas>(true);
            Check(!ui.gameObject.activeSelf,"Home defaults closed");
            menu.transform.Find("Menu/MyNestButton").GetComponent<Button>().onClick.Invoke();
            var home=ui.Prototype;
            Check(ui.gameObject.activeSelf && home.World!=null && home.ViewCamera!=null,"Home entry/environment");
            foreach(var name in new[]{"Floor","BackWall","LeftWall","RightWall","ChickenPlaceholder","PenguinPlaceholder","HomeLight"})
                Check(home.World.transform.Find(name)!=null,"Missing "+name);
            Check(ui.transform.Find("ButlerGreeting").GetComponent<Text>().text.Contains("歡迎回家"),"Butler guidance");
            var item=Resources.LoadAll<ItemDataSO>("Items").First(x=>x.IsValid && x.itemType==ItemType.Furniture);
            var before=JsonUtility.ToJson(inventory.GetSnapshot());
            Check(!home.SelectFurniture(item.itemId) && !home.SaveSelected(),"Unowned UI placement accepted");
            Check(!inventory.TryPlaceHomeFurniture(new PlacedHomeItemData{itemId=item.itemId}),"Unowned direct placement accepted");
            Check(before==JsonUtility.ToJson(inventory.GetSnapshot()),"Unowned operation mutated data");
            ui.transform.Find("OrbitRight").GetComponent<Button>().onClick.Invoke(); Check(home.Yaw==15,"Camera button");
            home.Orbit(999); home.Zoom(-999); Check(home.Yaw==55 && home.Distance==9,"Camera lower bounds");
            home.Orbit(-999); home.Zoom(999); Check(home.Yaw==-55 && home.Distance==14,"Camera upper bounds");
            home.Orbit(float.NaN); home.Zoom(float.NaN); Check(home.Yaw==-55 && home.Distance==14,"Nonfinite camera input");
            home.Orbit(55); home.Zoom(-3);
            Check(inventory.BuyItem(item),"Owned furniture fixture"); int owned=inventory.OwnedItemIds.Count,coins=inventory.Coins;
            ui.transform.Find("FurnitureInventory/Furniture:"+item.itemId).GetComponent<Button>().onClick.Invoke();
            Check(home.Draft.itemId==item.itemId && home.GetFurniture(item.itemId)!=null,"Owned furniture selection");
            Check(home.MoveSelected(new Vector3(1.25f,0,-.75f)),"Move");
            ui.transform.Find("RotateFurniture").GetComponent<Button>().onClick.Invoke();
            Check(inventory.GetActiveHomeFurniture().Count==0 && home.HasUnsavedChanges,"Preview must not save");
            ui.transform.Find("SaveFurniture").GetComponent<Button>().onClick.Invoke();
            Check(!home.HasUnsavedChanges && inventory.GetActiveHomeFurniture().Count==1,"Save button");
            var saved=inventory.GetActiveHomeFurniture()[0];
            Check(saved.position==new Vector3(1.25f,0,-.75f) && saved.rotation.y==45,"Saved transform");
            home.MoveSelected(new Vector3(99,0,99)); Check(home.Draft.position==new Vector3(3,0,2.5f),"Placement bounds");
            Check(!home.MoveSelected(new Vector3(float.NaN,0,0)),"Invalid position");
            var oldWorld=home.World; ui.transform.Find("BackButton").GetComponent<Button>().onClick.Invoke();
            Check(!ui.gameObject.activeSelf && home.World==null && !oldWorld.activeSelf,"Exit cleanup");
            ui.Open(); Check(home.GetFurniture(item.itemId).transform.localPosition==saved.position && home.Draft.rotation==saved.rotation,"Reenter restores/discards preview");
            home.MoveSelected(Vector3.zero);
            var screen=home.ViewCamera.WorldToViewportPoint(home.World.transform.position+new Vector3(-1,0,-1));
            home.PlaceAtViewport(new Vector2(screen.x,screen.y));
            Check(Vector3.Distance(home.Draft.position,new Vector3(-1,0,-1))<.01f,"Viewport floor picking");
            // Reload through a new InventoryManager with the persisted key.
            var go=new GameObject("ReloadInventory"); go.SetActive(false); var loaded=go.AddComponent<InventoryManager>();
            typeof(InventoryManager).GetField("playerPrefsKey",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(loaded,Key); go.SetActive(true);
            Check(loaded.GetActiveHomeFurniture()[0].position==saved.position && loaded.GetActiveHomeFurniture()[0].rotation==saved.rotation,"Disk reload transform");
            Check(loaded.OwnedItemIds.Count==owned && loaded.Coins==coins,"Ownership/currency unchanged");
            Object.Destroy(go);
            var unknown=inventory.GetSnapshot(); unknown.homeNest.placedFurniture.Add(new PlacedHomeItemData{itemId="unknown.future"});
            PlayerPrefs.SetString(Key+".unknown",JsonUtility.ToJson(unknown)); PlayerPrefs.Save();
            var other=new GameObject("UnknownInventory"); other.SetActive(false); var unknownInventory=other.AddComponent<InventoryManager>();
            typeof(InventoryManager).GetField("playerPrefsKey",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(unknownInventory,Key+".unknown"); other.SetActive(true);
            ui.Close(); home.Initialize(unknownInventory,ui.transform.Find("NestViewport").GetComponent<RawImage>()); ui.Open();
            Check(home.GetFurniture("unknown.future")==null && unknownInventory.GetSnapshot().homeNest.placedFurniture.Count==2,"Unknown ID recovery");
            Check(!home.SelectFurniture("unknown.future"),"Unknown selection");
            // Optional prefab representation uses the same ownership and is replaceable.
            var model=GameObject.CreatePrimitive(PrimitiveType.Cube); model.name="FixtureModel"; model.transform.position=new Vector3(2000,0,0); item.model3DPrefab=model;
            ui.Close(); ui.Open();
            Check(home.GetFurniture(item.itemId).transform.Find("FixtureModel(Clone)")!=null,"3D representation binding");
            Check(unknownInventory.OwnedItemIds.Count==owned,"Representation duplicated ownership");
            item.model3DPrefab=null; Object.Destroy(model); ui.Close(); ui.Open();
            home.ViewCamera.Render();
            Capture(menu);
            ui.Close(); Object.Destroy(other);
            Debug.Log("PHASE8E_SMOKE_PASS: entry/exit, environment, characters, camera bounds, owned placement, preview, save/reload, picking, unknown IDs, representation ownership");
            SessionState.SetBool(Key,false); EditorApplication.Exit(0);
        }
        catch(Exception ex) { Debug.LogException(ex); SessionState.SetBool(Key,false); EditorApplication.Exit(1); }
    }
    static void Capture(MainMenuCanvas menu)
    {
        if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        var canvas=menu.GetComponent<Canvas>(); var camera=Camera.main; var target=new RenderTexture(720,960,24);
        camera.targetTexture=target; canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=camera; canvas.planeDistance=1;
        Canvas.ForceUpdateCanvases(); camera.Render(); var old=RenderTexture.active; RenderTexture.active=target;
        var image=new Texture2D(720,960,TextureFormat.RGB24,false); image.ReadPixels(new Rect(0,0,720,960),0,0); image.Apply();
        System.IO.File.WriteAllBytes("Phase8EMyNest.png",image.EncodeToPNG()); RenderTexture.active=old; camera.targetTexture=null; canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        Object.Destroy(image); Object.Destroy(target);
    }
}
