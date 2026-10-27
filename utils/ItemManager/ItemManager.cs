using RDR2;
using RDR2.Native;
using FileLogger;
using System;
using System.Runtime.CompilerServices;

namespace ItemManager {
    public abstract class ItemManager {
        private readonly uint item;
        private readonly int inventoryId;
        protected readonly FileLogger.FileLogger logger;
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

        public abstract void ApplyItemEffects_Manual(Ped ped, Action<int> WaitFunc);
    }

    public class CigaretteBoxManager : ItemManager {
        public CigaretteBoxManager(Ped ped, string logPath, string logName) : base("CONSUMABLE_CIGARETTE_BOX", ped, logPath, logName, "CigaretteBoxManager") {}

        public override void ApplyItemEffects_Manual(Ped ped, Action<int> WaitFunc) {
            logger.Log("Manually applying cigarette effects");
            try {
                int deadEyeCore = ped.Cores.DeadEye.Value;
                int healthCore = ped.Cores.Health.Value;

                // +25% deadeye -10% health cores
                ped.Cores.DeadEye.Value = Math.Min(100, (int)(deadEyeCore * 1.25));
                int finalHealth = ped.Cores.Health.Value = Math.Max(0, (int)Math.Floor(healthCore * 0.9));

                int efxTimeout = finalHealth == 0 ? 50 : 200;
                ApplyAnimPostEffects_Manual("PlayerRPGCoreDeadEye", WaitFunc, efxTimeout);

                if (finalHealth == 0) ApplyAnimPostEffects_Manual("PlayerRPGEmptyCoreHealth", WaitFunc);
            } catch (Exception e) {
                logger.Error("Failed to apply cigarette effects: {0}", e.Message);
            }
        }

        private void ApplyAnimPostEffects_Manual(string effectName, Action<int> WaitFunc, int timeout = 200) {
            int timeTrack = 0;
            try {
                GRAPHICS.ANIMPOSTFX_PLAY(effectName);
                while (timeTrack < timeout) {
                    timeTrack++;
                    WaitFunc(1);
                }
                GRAPHICS.ANIMPOSTFX_STOP(effectName);
            } catch (Exception e) {
                logger.Error("Failed to apply post fx: {0}", effectName);
                throw e;
            }
        }
    }

    public class ItemManagerFactory {
        public static CigaretteBoxManager GetPlayerCigaretteBoxManager(string logPath, string logName) {
            return new CigaretteBoxManager(Game.Player.Character, logPath, logName);
        }
    }
}