using System;
using System.Collections.Generic;

namespace NodeManager
{
    public class ExecuteModifyChest : IActionClient
    {

        public int RecipientEntityId
        {
            get => -1; set => throw new
                System.Exception("Can send stop signal from clients!");
        }

        public Vector3i Position;

        public Func<TileEntityLootContainer, List<ItemStack>, bool> Predicate;

        public void Setup(Vector3i position, Func<TileEntityLootContainer, List<ItemStack>, bool> predicate)
        {
            Position = position;
            Predicate = predicate;

        }

        public void ProcessOnMainThread(NodeManagerMother mother)
        {
            Log.Warning("Lock the TE for the chest");
            var world = GameManager.Instance.World;
            var te = world.GetTileEntity(0, Position);
            if (te == null) Log.Warning("Tile Entity not found!");
            if (te is TileEntityLootContainer container)
            {
                if (container.IsUserAccessing()) return;
                var taken = new List<ItemStack>();
                if (!Predicate(container, taken)) return;
                var response = new ActionModifyChest();
                response.Setup(taken.ToArray());
                mother.ToWorker.Enqueue(response);
                //
                //for (int i = 0; i < Stack.Length; i++)
                //{
                //    ItemStack item = Stack[i];
                //    int count = item.count;
                //    item.count = 0;
                //    if (count < 0)
                //    {
                //        while (container.HasItem(item.itemValue))
                //        {
                //            container.RemoveItem(item.itemValue);
                //            item.count += 1; // Count when taken
                //        }
                //    }
                //}
            }

        }

    }
}
