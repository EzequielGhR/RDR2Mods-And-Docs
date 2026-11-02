#ifndef CLOTHES_MANAGER_H
    #define CLOTHES_MANAGER_H

    #include "../FileLogger/fileLogger.h"
    #include "../../SDK/inc/natives.h"

    struct sGuid {
        alignas(8) int data1;
        alignas(8) int data2;
        alignas(8) int data3;
        alignas(8) int data4;
    };

    sGuid CreateNewGUID();
    bool GetGUIDFromItemID(int inventoryId, sGuid* pParentGuid, Hash slot, Hash subSlot, sGuid* pOutGuid);
    bool GetInventoryItemInUse(int inventoryId, sGuid* pItemGuid);
    Hash GetDefaultItemSlot(Hash itemHash, Hash slotHash);
    int GetInventoryIdFromPed(Ped ped);

    bool IsPedComponentEquipped(Ped pedHandle, Hash componentCategoryHash);
    bool IsPedComponentEquipped(Ped pedHandle, const char* componentCategory);
    bool IsPedMasked(Ped pedHandle, FileLogger* pFLogger = nullptr);
    bool IsBandanaEquipped(int inventoryId);
    bool IsBandanaEquipped();
    bool IsPlayerMasked(int inventoryId, FileLogger* pFLogger = nullptr);
    bool IsPlayerMasked(FileLogger* pFLogger = nullptr);
#endif