#include "clothesManager.h"


sGuid CreateNewGUID() {
    sGuid guid{};
    return guid;
}

bool GetGUIDFromItemID(int inventoryId, sGuid* pParentGuid, Hash slot, Hash subSlot, sGuid* pOutGuid) {
    return ITEMS::_0x886DFD3E185C8A89(
        inventoryId,
        (Any*)pParentGuid,
        slot,
        subSlot,
        (Any*)pOutGuid
    );
}

bool GetInventoryItemInUse(int inventoryId, sGuid* pItemGuid) {
    return invoke<bool>(0x70E3A884ED000A01, inventoryId, pItemGuid);
}

Hash GetDefaultItemSlot(Hash itemHash, Hash slotHash) {
    return ITEMS::_0x6452B1D357D81742(itemHash, slotHash);
}

int GetInventoryIdFromPed(Ped ped) {
    ITEMS::_0x13D234A2A3F66E63(ped);
}

bool IsPedComponentEquipped(Ped pedHandle, Hash componentCategoryHash) {
    return PED::_0xFB4891BD7578CDC1(pedHandle, componentCategoryHash);
}

bool IsPedComponentEquipped(Ped pedHandle, char* componentCategory) {
    return IsPedComponentEquipped(pedHandle, GAMEPLAY::GET_HASH_KEY(componentCategory));
}

bool IsPedMasked(Ped pedHandle, FileLogger* pFLogger = nullptr) {
    Hash maskHashes[] = {GAMEPLAY::GET_HASH_KEY("MASKS"), GAMEPLAY::GET_HASH_KEY("MASKS_LARGE")};
    int totalHashes = sizeof(maskHashes) / sizeof(maskHashes[0]);
    for (int i = 0; i < totalHashes; i++) {
        if (IsPedComponentEquipped(pedHandle, maskHashes[i])) {
            if (pFLogger != nullptr && sizeof(*pFLogger) >= sizeof(FileLogger)) {
                pFLogger->Log("Hash equipped: %d", maskHashes[i]);
            }

            return true;
        }
    }

    return false;
}

bool IsBandanaEquipped(int inventoryId) {
    sGuid _base = CreateNewGUID();
    sGuid rootGuid;
    sGuid wardrobeGuid;
    sGuid outGuid;

    Hash wardrobeItemId = GAMEPLAY::GET_HASH_KEY("WARDROBE");
    Hash kitBandanaItemId = GAMEPLAY::GET_HASH_KEY("KIT_BANDANA");

    // Get rootGuid
    GetGUIDFromItemID(inventoryId, &_base, GAMEPLAY::GET_HASH_KEY("CHARACTER"), GAMEPLAY::GET_HASH_KEY("SLOTID_NONE"), &rootGuid);
    GetGUIDFromItemID(inventoryId, &rootGuid, wardrobeItemId, GAMEPLAY::GET_HASH_KEY("SLOTID_WARDROBE"), &wardrobeGuid);
    
    Hash defaultSlot = GetDefaultItemSlot(kitBandanaItemId, wardrobeItemId);
    GetGUIDFromItemID(inventoryId, &wardrobeGuid, kitBandanaItemId, defaultSlot, &outGuid);

    return GetInventoryItemInUse(inventoryId, &outGuid);
}

bool IsBandanaEquipped() {
    int inventoryId = GetInventoryIdFromPed(PLAYER::GET_PLAYER_PED(PLAYER::GET_PLAYER_INDEX()));
    return IsBandanaEquipped(inventoryId);
}

bool IsPlayerMasked(int inventoryId, FileLogger* pFLogger = nullptr) {
    Ped playerPed = PLAYER::GET_PLAYER_PED(PLAYER::GET_PLAYER_INDEX());
    try {
        if (IsBandanaEquipped(inventoryId)) {
            if (pFLogger != nullptr && sizeof(*pFLogger) >= sizeof(FileLogger)) {
                pFLogger->Log("Mask equiped");
            }

            return true;
        }

        if (IsPedMasked(playerPed, pFLogger)) return true;

        return false;
    } catch (const std::exception& e) {
        if (pFLogger != nullptr && sizeof(*pFLogger) >= sizeof(FileLogger)) {
            pFLogger->Error("There was an error checking if player is masked: %s", e.what());
            return false;
        }
    } catch (...) {
        if (pFLogger != nullptr && sizeof(*pFLogger) >= sizeof(FileLogger)) {
            pFLogger->Error("There was an unexpected error when checking if player is masked");
            return false;
        }
    }
}

bool IsPlayerMasked(FileLogger* pFLogger = nullptr) {
    int inventoryId = GetInventoryIdFromPed(PLAYER::GET_PLAYER_PED(PLAYER::GET_PLAYER_INDEX()));
    return IsPlayerMasked(inventoryId, pFLogger);
}