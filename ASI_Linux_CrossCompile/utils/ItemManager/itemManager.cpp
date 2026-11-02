#include "itemManager.h"
#include "../enums.h"
#include "../ClothesManager/clothesManager.h"
#include "../FileLogger/fileLogger.h"
#include "../../SDK/inc/natives.h"


int maxInt(int a, int b) { return a > b ? a : b; }
int minInt(int a, int b) { return a > b ? b : a; }

class ItemManager {
    private:
        Hash item;
        int inventoryId;
        int itemCount = 0;
    
    protected:
        FileLogger* logger;

    public:
        ItemManager(char* itemStringId, Ped ped, char* logPath, char* logName, char* serviceName = "ItemManager") {
            logger = new FileLogger(logPath, logName, serviceName);
            logger->Log("Initializing with itemStringId: %s, ped: %d", itemStringId, ped);
            item = GAMEPLAY::GET_HASH_KEY(itemStringId);
            inventoryId = GetInventoryIdFromPed(ped);
            itemCount = GetItemCount(inventoryId, item);
        }

        ~ItemManager() { delete logger; }

        sGuid static GetItemGuid(int inventoryId, Hash item) {
            sGuid _base = CreateNewGUID();
            sGuid rootGuid;
            sGuid satchelGuid;
            sGuid outGuid;

            GetGUIDFromItemID(
                inventoryId,
                &_base,
                GAMEPLAY::GET_HASH_KEY("CHARACTER"),
                GAMEPLAY::GET_HASH_KEY("SLOTID_NONE"),
                &rootGuid
            );

            GetGUIDFromItemID(
                inventoryId,
                &rootGuid,
                GAMEPLAY::GET_HASH_KEY("SATCHEL"),
                GAMEPLAY::GET_HASH_KEY("SLOTID_SATCHEL"),
                &satchelGuid
            );

            Hash defaultItemSlot = GetDefaultItemSlot(item, GAMEPLAY::GET_HASH_KEY("SATCHEL"));
            
            GetGUIDFromItemID(
                inventoryId,
                &satchelGuid,
                item,
                defaultItemSlot,
                &outGuid
            );

            return outGuid;
        }

        int static GetItemCount(int inventoryId, Hash item) {
            sGuid outGuid = GetItemGuid(inventoryId, item);
            // _PLAYER_INVENTORY_GET_ITEM_COUNT
            return ITEMS::_0xC97E0D2302382211(inventoryId, (Any*)&outGuid);
        }

        int GetItemCount(bool force = false) {
            if (force) {
                logger->Log("Force fetching item count");
                itemCount = GetItemCount(inventoryId, item);
            }

            return itemCount;
        }

        int RemoveItem() {
            sGuid itemGuid = GetItemGuid(inventoryId, item);
            //_INVENTORY_REMOVE_INVENTORY_ITEM_WITH_GUID
            ITEMS::_0x3E4E811480B3AE79(inventoryId, (Any*)&itemGuid, 1, eRemoveItemReason::REMOVE_REASON_DEFAULT);
        }

        void ApplyItemEffects_Manual(Ped ped, void (*WaitFunc)(int)) {}
};


class CigaretteBoxManager : public ItemManager {
    private:
        void ApplyAnimPostEffects_Manual(char* effectName, void (*WaitFunc)(int), int timeout = 200) {
            int timeTrack = 0;
            try {
                GRAPHICS::ANIMPOSTFX_PLAY(effectName);
                while (timeTrack < timeout) {
                    timeTrack++;
                    WaitFunc(1);
                }
                GRAPHICS::ANIMPOSTFX_STOP(effectName);
            } catch (std::exception& e) {
                logger->Error("There was an issue applying animpostfx: %s", e.what());
            } catch (...) {
                logger->Error("There was an unknown error applying animpostfx");
            }
        }

    public:
        CigaretteBoxManager(Ped ped, char* logPath, char* logName) : ItemManager("CONSUMABLE_CIGARETTE_BOX", ped, logPath, logName, "CigaretteBoxManager") {};
        
        void ApplyItemEffects_Manual(Ped ped, void (*WaitFunc)(int)) {
            logger->Log("Manually applying cigarette effects");
            try {
                // _GET_ATTRIBUTE_CORE_VALUE
                int deadEyeCore = ATTRIBUTE::_0x36731AC041289BB1(ped, eCoreType::DEADEYE);
                int healthCore = ATTRIBUTE::_0x36731AC041289BB1(ped, eCoreType::HEALTH);

                // +25% deadeye -10% health cores
                // _SET_ATTRIBUTE_CORE_VALUE
                ATTRIBUTE::_0xC6258F41D86676E0(ped, eCoreType::DEADEYE, minInt(deadEyeCore * 1.25, 100));
                
                int finalHealth = maxInt((int)((double)healthCore * 0.9 - 1), 0);
                ATTRIBUTE::_0xC6258F41D86676E0(ped, eCoreType::HEALTH, finalHealth);

                int efxTimeout = finalHealth == 0 ? 50 : 200;
                ApplyAnimPostEffects_Manual("PlayerRPGCoreDeadEye", WaitFunc, efxTimeout);

                if (finalHealth == 0) ApplyAnimPostEffects_Manual("PlayerRPGEmptyCoreHealth", WaitFunc);
            } catch (std::exception& e) {
                logger->Error("There was an issue applying manual effects: %s", e.what());
            } catch (...) {
                logger->Error("There was an unknown error applying manual effects");
            }
        }
};


class ItemManagerFactory {
    static CigaretteBoxManager GetPlayerCigaretteBoxManager(char* logPath, char* logName) {
        return CigaretteBoxManager(
            PLAYER::GET_PLAYER_PED(PLAYER::GET_PLAYER_INDEX()),
            logPath,
            logName
        );
    }
};