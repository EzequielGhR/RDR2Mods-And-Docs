using RDR2;
using RDR2.Native;
using System;
using System.Runtime.InteropServices;

using FileLogger;


namespace ClothesManager {
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct sGuid {
        public int data1;
        public int data2;
        public int data3;
        public int data4;
    }

    public class ClothesHelper {
        public static bool IsPedComponentEquipped(Ped ped, uint componentCategory) {
            return Function.Call<bool>(0xFB4891BD7578CDC1, ped.Handle, componentCategory);
        }

        public static bool IsPedComponentEquipped(Ped ped, string componentCategoryName) {
            return IsPedComponentEquipped(ped, Game.Joaat(componentCategoryName));
        }

        public static bool IsPedMasked(Ped ped, ModFileLogger fileLogger = null) {
            uint[] maskHashes = {Game.Joaat("MASKS"), Game.Joaat("MASKS_LARGE")};
            foreach(uint hash in maskHashes) {
                if (IsPedComponentEquipped(ped, hash)) {
                    fileLogger?.Log("Hash equipped: {0}", hash);
                    return true;
                }
            }

            return false;
        }

        public static unsafe bool IsBandanaEquipped(int inventoryId) {
            sGuid _base = default;
            sGuid rootGuid = default;
            sGuid wardrobeGuid = default;
            sGuid outGuid = default;

            uint wardrobeItemId = Game.Joaat("WARDROBE");
            uint kitBandanaItemId = Game.Joaat("KIT_BANDANA");
            
            // Fetch root GUID
            INVENTORY.INVENTORY_GET_GUID_FROM_ITEMID(
                inventoryId,
                (ulong*)&_base,
                Game.Joaat("CHARACTER"),
                Game.Joaat("SLOTID_NONE"),
                (ulong*)&rootGuid
            );
            
            // Fetch Wardrobe GUID
            INVENTORY.INVENTORY_GET_GUID_FROM_ITEMID(
                inventoryId,
                (ulong*)&rootGuid,
                wardrobeItemId,
                Game.Joaat("SLOTID_WARDROBE"),
                (ulong*)&wardrobeGuid
            );
            
            // Get the bandana item slot
            uint defaultItemSlot = INVENTORY._GET_DEFAULT_ITEM_SLOT_INFO(
                kitBandanaItemId,
                wardrobeItemId
            );

            // Fetch the bandana GUID
            INVENTORY.INVENTORY_GET_GUID_FROM_ITEMID(
                inventoryId,
                (ulong*)&wardrobeGuid,
                kitBandanaItemId,
                defaultItemSlot,
                (ulong*)&outGuid
            );

            return INVENTORY._INVENTORY_GET_INVENTORY_ITEM_IN_USE(inventoryId, (ulong*)&outGuid);
        }

        public static bool IsBandanaEquipped() {
            int inventoryId = INVENTORY._INVENTORY_GET_INVENTORY_ID_FROM_PED(Game.Player.Character.Handle);
            return IsBandanaEquipped(inventoryId);
        }

        public static bool IsPlayerMasked(int inventoryId, ModFileLogger fileLogger = null) {
            Ped playerPed = Game.Player.Character;
            try {
                if (IsBandanaEquipped(inventoryId)) {
                    fileLogger?.Log("Mask equipped");
                    return true;
                }

                if (IsPedMasked(playerPed, fileLogger)) return true;
            } catch (Exception e) {
                fileLogger?.Error("Failed to call component equiped raw native: {0}", e.Message);
                return false;
            }

            return false;
        }
    }
}