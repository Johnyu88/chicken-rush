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
public static class Phase8FSmokeRunner
{
    const string Key="ChickenRush.Tests.Phase8F";
    const string Fixture="Assets/Resources/Items/Phase8FFixture.asset";
    static int frames;
    static Phase8FSmokeRunner() { if(SessionState.GetBool(Key,false)) EditorApplication.update+=Tick; }
    public static void Run()
    {
        // This runner is intended for an isolated project copy, like the other Phase runners.
        var extra=AssetDatabase.LoadAssetAtPath<ItemDataSO>(Fixture);
        if(extra==null) { extra=ScriptableObject.CreateInstance<ItemDataSO>(); extra.itemId="test.phase8f.furniture"; extra.itemName="測試家具"; extra.itemType=ItemType.Furniture; AssetDatabase.CreateAsset(extra,Fixture); }
        MvpSceneBuilder.Build();
        var serialized=new SerializedObject(Object.FindFirstObjectByType<InventoryManager>());
        serialized.FindProperty("playerPrefsKey").stringValue=Key; serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        PlayerPrefs.DeleteKey(Key+".backup"); PlayerPrefs.SetString(Key,"{\"coins\":2000,\"homeNest\":{\"version\":1,\"placedFurniture\":[{\"itemId\":\"unknown.recovery\"}]}}"); PlayerPrefs.Save();
        SessionState.SetBool(Key,true); EditorApplication.EnterPlaymode();
    }
    static void Check(bool result,string message) { if(!result) throw new Exception(message); }
    static PlacementCommand Command(PlacementOperation operation,string id,Vector3 position=default,float yaw=0) => new PlacementCommand { operation=operation,itemId=id,position=position,yaw=yaw };
    static void Tick()
    {
        if(!EditorApplication.isPlaying || ++frames<8) return;
        try
        {
            var inventory=Object.FindFirstObjectByType<InventoryManager>(); var menu=Object.FindFirstObjectByType<MainMenuCanvas>();
            var ui=menu.GetComponentInChildren<MyNestCanvas>(true); ui.Open(); var home=ui.Prototype;
            var item=Resources.LoadAll<ItemDataSO>("Items").First(x=>x.itemType==ItemType.Furniture && x.itemId!="test.phase8f.furniture");
            var extra=Resources.LoadAll<ItemDataSO>("Items").First(x=>x.itemId=="test.phase8f.furniture");
            Check(ui.FurnitureLibrary.VisibleItemIds.Count==0,"Unowned furniture in library");
            string initial=JsonUtility.ToJson(inventory.GetSnapshot());
            Check(!home.SelectFurniture(item.itemId) && !home.AgentPreview.Preview(new[]{Command(PlacementOperation.PlaceFurniture,item.itemId)}),"Unowned player/AI accepted");
            Check(initial==JsonUtility.ToJson(inventory.GetSnapshot()),"Unowned mutation");
            Check(inventory.BuyItem(item),"Fixture ownership");
            Check(ui.FurnitureLibrary.VisibleItemIds.SequenceEqual(new[]{item.itemId}),"Library owned filter");
            ui.transform.Find("FurnitureInventory/Furniture:"+item.itemId).GetComponent<Button>().onClick.Invoke();
            Check(home.Draft.itemId==item.itemId && home.GetFurniture(item.itemId)!=null,"Card select/preview");
            string baseline=JsonUtility.ToJson(inventory.GetSnapshot());
            Check(home.MoveSelected(new Vector3(1,0,-1)),"Move"); home.RotateSelected(45);
            Check(home.Draft.rotation.y==45 && baseline==JsonUtility.ToJson(inventory.GetSnapshot()),"Preview leaked to save");
            Check(home.UndoEdit() && home.Draft.rotation.y==0,"Undo last unconfirmed operation");
            ui.transform.Find("CancelFurniture").GetComponent<Button>().onClick.Invoke();
            Check(home.GetFurniture(item.itemId)==null && baseline==JsonUtility.ToJson(inventory.GetSnapshot()),"Cancel mutated save");
            Check(home.SelectFurniture(item.itemId),"Reselect"); home.MoveSelected(new Vector3(1,0,-1)); home.RotateSelected(90);
            ui.transform.Find("SaveFurniture").GetComponent<Button>().onClick.Invoke();
            Check(!home.HasUnsavedChanges && inventory.GetActiveHomeFurniture()[0].rotation.y==90,"Confirm commit");
            baseline=JsonUtility.ToJson(inventory.GetSnapshot());
            ui.transform.Find("RemoveFurniture").GetComponent<Button>().onClick.Invoke();
            Check(home.GetFurniture(item.itemId)==null && baseline==JsonUtility.ToJson(inventory.GetSnapshot()),"Remove must preview");
            Check(home.UndoEdit() && home.GetFurniture(item.itemId)!=null,"Undo remove"); home.RemoveSelected();
            Check(home.SaveSelected() && inventory.GetActiveHomeFurniture().Count==0 && inventory.OwnsItem(item),"Remove confirm lost ownership");
            home.SelectFurniture(item.itemId); home.MoveSelected(new Vector3(999,0,-999));
            Check(home.Draft.position==new Vector3(3,0,-2.5f),"Bounds clamp");
            foreach(float bad in new[]{float.NaN,float.PositiveInfinity,float.NegativeInfinity})
            {
                Check(!home.MoveSelected(new Vector3(0,bad,0)),"Nonfinite position accepted");
                Check(!home.PreviewCommands(new[]{Command(PlacementOperation.RotateFurniture,item.itemId,yaw:bad)}),"Nonfinite yaw accepted");
            }
            Check(!home.PreviewCommands(new[]{Command((PlacementOperation)99,item.itemId)}) && !home.SelectFurniture("unknown.id"),"Invalid command/ID");
                        Check(home.SaveSelected(),"Save bounds");
            for(int i=0;i<300;i++) Check(home.MoveSelected(new Vector3(i*.001f,0,0)),"Long drag exhausted command budget");
            Check(home.UndoEdit() && home.Draft.position==new Vector3(3,0,-2.5f),"Undo continuous move");
            home.CancelEdit();
            Check(inventory.BuyItem(extra),"Second fixture");
            typeof(InventoryManager).GetField("homeFurnitureLimit",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(inventory,1);
            Check(!home.SelectFurniture(extra.itemId) && !home.AgentPreview.Preview(new[]{Command(PlacementOperation.PlaceFurniture,extra.itemId)}),"Furniture cap player/AI");
            baseline=JsonUtility.ToJson(inventory.GetSnapshot());
            var previewBefore=JsonUtility.ToJson(home.PreviewState);
            Check(!home.AgentPreview.Preview(new[]{Command(PlacementOperation.MoveFurniture,item.itemId,Vector3.zero),Command(PlacementOperation.PlaceFurniture,"missing")}),"Invalid batch accepted");
            Check(previewBefore==JsonUtility.ToJson(home.PreviewState) && baseline==JsonUtility.ToJson(inventory.GetSnapshot()),"Partial batch applied");
            var proposal=Command(PlacementOperation.MoveFurniture,item.itemId,new Vector3(-1,0,0));
            Check(home.AgentPreview.Preview(new[]{proposal}),"AI proposal rejected"); proposal.position=Vector3.one*99;
            Check(home.PreviewState.placedFurniture.First(x=>x.itemId==item.itemId).position==new Vector3(-1,0,0),"Proposal alias");
            Check(baseline==JsonUtility.ToJson(inventory.GetSnapshot()),"AI self committed"); home.CancelEdit();
            ui.transform.Find("PenguinPreview").GetComponent<Button>().onClick.Invoke();
            Check(home.HasUnsavedChanges && baseline==JsonUtility.ToJson(inventory.GetSnapshot()),"Penguin button self committed"); home.CancelEdit();
            home.SelectFurniture(item.itemId); home.MoveSelected(Vector3.zero); Check(home.SaveSelected(),"Prepare input");
            var input=ui.transform.Find("NestViewport").GetComponent<HomeNestPointerInput>();
            var pick=home.ViewCamera.WorldToViewportPoint(home.World.transform.position+new Vector3(0,.7f,0));
            Check(home.GetFurniture(item.itemId)!=null,"Unknown record consumed render capacity");
            input.BeginPointer(-1,pick); input.MovePointer(-1,home.ViewCamera.WorldToViewportPoint(home.World.transform.position+new Vector3(1,0,1))); input.EndPointer(-1);
            Check(Vector3.Distance(home.Draft.position,new Vector3(1,0,1))<.02f,"Single pointer move: "+home.Draft.position+" yaw="+home.Yaw);
            float yaw=home.Yaw; input.BeginPointer(1,new Vector2(.95f,.95f)); input.MovePointer(1,new Vector2(.85f,.95f)); input.EndPointer(1);
            Check(home.Yaw!=yaw,"Empty drag orbit");
            var beforePinch=JsonUtility.ToJson(home.PreviewState); float distance=home.Distance;
            input.BeginPointer(1,new Vector2(.2f,.9f)); input.BeginPointer(2,new Vector2(.8f,.9f)); input.MovePointer(2,new Vector2(.9f,.9f)); input.EndPointer(2);
            input.MovePointer(1,new Vector2(.1f,.9f)); input.EndPointer(1);
            Check(home.Distance<distance && beforePinch==JsonUtility.ToJson(home.PreviewState),"Pinch zoom moved furniture");
            home.CancelEdit(); home.SelectFurniture(item.itemId); home.MoveSelected(new Vector3(.5f,0,.5f));
            inventory.TryPlaceHomeFurniture(new PlacedHomeItemData { itemId=item.itemId,position=new Vector3(2,0,0) });
            Check(!home.SaveSelected(),"Stale home overwrote newer save"); home.CancelEdit();
            home.SelectFurniture(item.itemId); home.MoveSelected(new Vector3(-.5f,0,-.5f)); home.RotateSelected(45); Check(home.SaveSelected(),"Final confirm");
            var go=new GameObject("Reload"); go.SetActive(false); var reload=go.AddComponent<InventoryManager>();
            typeof(InventoryManager).GetField("playerPrefsKey",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(reload,Key); go.SetActive(true);
            var saved=reload.GetActiveHomeFurniture()[0]; Check(saved.position==new Vector3(-.5f,0,-.5f) && saved.rotation.y==45,"Save/reload");
            Check(reload.GetSnapshot().homeNest.placedFurniture.Any(x=>x.itemId=="unknown.recovery"),"Unknown recovery lost"); Object.Destroy(go);
            home.ViewCamera.Render();
            typeof(Phase8ESmokeRunner).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{menu});
            System.IO.File.Copy("Phase8EMyNest.png","Phase8FMyNest.png",true);
                        ui.Close();
            var future=inventory.GetSnapshot(); future.homeNest.version=99;
            PlayerPrefs.SetString(Key+".future",JsonUtility.ToJson(future)); PlayerPrefs.Save();
            var futureObject=new GameObject("FutureHome"); futureObject.SetActive(false); var futureInventory=futureObject.AddComponent<InventoryManager>();
            typeof(InventoryManager).GetField("playerPrefsKey",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(futureInventory,Key+".future"); futureObject.SetActive(true);
            home.Initialize(futureInventory,ui.transform.Find("NestViewport").GetComponent<RawImage>()); ui.Open();
            Check(home.GetFurniture(item.itemId)==null && !home.SelectFurniture(item.itemId),"Future home version activated");
            Check(futureInventory.GetSnapshot().homeNest.version==99,"Future data mutated");
            ui.Close(); Object.Destroy(futureObject); AssetDatabase.DeleteAsset(Fixture);
            Debug.Log("PHASE8F_SMOKE_PASS: owned library, select/preview/move/rotate/remove, cancel/confirm/undo, reload, ownership, bounds, finite values, cap, atomic commands, AI preview, stale session, synthetic pointer/pinch");
            SessionState.SetBool(Key,false); EditorApplication.Exit(0);
        }
        catch(Exception ex) { Debug.LogException(ex); AssetDatabase.DeleteAsset(Fixture); SessionState.SetBool(Key,false); EditorApplication.Exit(1); }
    }
}
