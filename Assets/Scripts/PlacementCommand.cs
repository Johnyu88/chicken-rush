using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChickenRush
{
    public enum PlacementOperation { PlaceFurniture, MoveFurniture, RotateFurniture, RemoveFurniture }
    [Serializable]
    public sealed class PlacementCommand
    {
        public PlacementOperation operation;
        public string itemId;
        public Vector3 position;
        public float yaw;
        public PlacementCommand Copy() => new PlacementCommand { operation=operation, itemId=itemId, position=position, yaw=yaw };
    }

    // Shared by player previews, agent proposals and the final Inventory transaction.
    public static class PlacementRules
    {
        public const int MaxCommands = 256;
        public static bool Finite(float x) => !float.IsNaN(x) && !float.IsInfinity(x);
        public static bool InArea(PlacedHomeItemData p) => p != null && p.IsValid && Mathf.Abs(p.position.x)<=3 &&
            Mathf.Abs(p.position.z)<=2.5f && p.position.y==0 && p.rotation.x==0 && p.rotation.z==0 && p.scale==Vector3.one;
        public static bool TryApply(HomeNestData source, IReadOnlyList<PlacementCommand> commands,
            Func<string,bool> ownsFurniture, int limit, out HomeNestData result)
        {
            result=null;
            if(source==null || source.version!=HomeNestData.CurrentVersion || commands==null || commands.Count>MaxCommands || limit<1) return false;
            var next=source.Copy();
            foreach(var c in commands)
            {
                if(c==null || !Enum.IsDefined(typeof(PlacementOperation),c.operation) || string.IsNullOrWhiteSpace(c.itemId) ||
                    c.itemId!=c.itemId.Trim() || !ownsFurniture(c.itemId) || !Finite(c.position.x) || !Finite(c.position.y) ||
                    !Finite(c.position.z) || !Finite(c.yaw)) return false;
                var existing=next.placedFurniture.FirstOrDefault(x=>x.itemId==c.itemId);
                if(c.operation==PlacementOperation.RemoveFurniture)
                {
                    if(existing==null) return false;
                    next.placedFurniture.RemoveAll(x=>x.itemId==c.itemId); continue;
                }
                if(c.operation!=PlacementOperation.PlaceFurniture && existing==null) return false;
                if(c.operation==PlacementOperation.PlaceFurniture && existing!=null) return false;
                var changed=existing?.Copy() ?? new PlacedHomeItemData { itemId=c.itemId };
                if(c.operation!=PlacementOperation.RotateFurniture)
                {
                    changed.position=new Vector3(Mathf.Clamp(c.position.x,-3,3),0,Mathf.Clamp(c.position.z,-2.5f,2.5f));
                    changed.scale=Vector3.one;
                }
                if(c.operation!=PlacementOperation.MoveFurniture) changed.rotation=new Vector3(0,Mathf.Repeat(c.yaw,360),0);
                if(!InArea(changed)) return false;
                next.placedFurniture.RemoveAll(x=>x.itemId==c.itemId); next.placedFurniture.Add(changed);
                if(next.placedFurniture.Where(x=>ownsFurniture(x.itemId)).Select(x=>x.itemId).Distinct().Count()>limit) return false;
            }
            result=next; return true;
        }
    }

    // This is the only capability handed to a future agent. It has no Confirm operation.
    public interface IHomeDecorationPreview { bool Preview(IReadOnlyList<PlacementCommand> commands); }
    public sealed class FurnitureEditSession
    {
        private readonly InventoryManager inventory;
        private HomeNestData baseline, preview;
        private List<PlacementCommand> commands=new List<PlacementCommand>(), undo;
        public HomeNestData Snapshot => preview.Copy();
        public bool IsDirty => commands.Count>0;
        public bool CanUndo => undo!=null;
        public FurnitureEditSession(InventoryManager owner) { inventory=owner; Cancel(); }
        public bool Preview(IReadOnlyList<PlacementCommand> proposal)
        {
            if(proposal==null || proposal.Count==0 || proposal.Any(x=>x==null)) return false;
                        bool continuingMove=proposal.Count==1 && commands.Count>0 && proposal[0].operation==PlacementOperation.MoveFurniture &&
                commands[commands.Count-1].operation==PlacementOperation.MoveFurniture && commands[commands.Count-1].itemId==proposal[0].itemId;
            var combined=commands.Select(x=>x.Copy()).ToList();
            if(continuingMove) combined.RemoveAt(combined.Count-1);
            combined.AddRange(proposal.Select(x=>x.Copy()));
            if(!PlacementRules.TryApply(baseline,combined,inventory.IsOwnedFurniture,inventory.HomeFurnitureLimit,out var next)) return false;
            if(!continuingMove) undo=commands; commands=combined; preview=next; return true;
        }
        public bool Undo()
        {
            if(undo==null || !PlacementRules.TryApply(baseline,undo,inventory.IsOwnedFurniture,inventory.HomeFurnitureLimit,out var next)) return false;
            commands=undo; undo=null; preview=next; return true;
        }
        public void Cancel() { baseline=inventory.GetSnapshot().homeNest; preview=baseline.Copy(); commands.Clear(); undo=null; }
        // UI owns this capability; proposal sources receive IHomeDecorationPreview instead.
        public bool Confirm()
        {
            if(!IsDirty) return false;
            if(!inventory.TryCommitFurnitureCommands(JsonUtility.ToJson(baseline),commands)) return false;
            Cancel(); return true;
        }
    }
}
