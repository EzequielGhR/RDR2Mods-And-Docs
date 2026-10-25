using RDR2;
using RDR2.Native;
using FileLogger;

namespace ItemManager {
    public class ItemManager {
        private readonly uint item;
        private readonly int inventoryId;
        private readonly FileLogger.FileLogger logger;
        private int itemCount = 0;
        
        public ItemManager(string itemStringId, Ped ped, string logPath, string logName, string serviceName = "ItemManager") {
            logger = new FileLogger.FileLogger(logPath, logName, serviceName);
            logger.Log("Initializing with itemStringId: {0}, ped: {1}", itemStringId, ped.Handle);
            item = Game.Joaat(itemStringId);
            inventoryId = INVENTORY._INVENTORY_GET_INVENTORY_ID_FROM_PED(ped.Handle);
            itemCount = INVENTORY._INVENTORY_GET_INVENTORY_ITEM_COUNT_WITH_ITEMID(inventoryId, item, false);
        }

        public int GetItemCount(bool force = false) {
            if (force) {
                logger.Log("Force fetching item count");
                itemCount = INVENTORY._INVENTORY_GET_INVENTORY_ITEM_COUNT_WITH_ITEMID(inventoryId, item, true);
            }
            return itemCount;
        }

        public int RemoveItem() {
            INVENTORY._INVENTORY_REMOVE_INVENTORY_ITEM_WITH_ITEMID(inventoryId, item, 1, (uint)eRemoveItemReason.Default);
            return GetItemCount(true);
        }
    }

    public class CigaretteBoxManager : ItemManager {
        public CigaretteBoxManager(Ped ped, string logPath, string logName) : base("CONSUMABLE_CIGARETTE_BOX", ped, logPath, logName, "CigaretteBoxManager") {}
    }

    public class ItemManagerFactory {
        public static CigaretteBoxManager GetPlayerCigaretteBoxManager(string logPath, string logName) {
            return new CigaretteBoxManager(Game.Player.Character, logPath, logName);
        }
    }
}