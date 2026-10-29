#include <stdio.h>
#include <windows.h>
#include "../SDK/inc/main.h"
#include "../SDK/inc/natives.h"


// <<< GUID definition >>>

// 4 segments 8-byte aligned
struct sGuid {
    alignas(8) int data1;
    alignas(8) int data2;
    alignas(8) int data3;
    alignas(8) int data4;
};


// <<< Aux functions >>>

sGuid CreateNewGUID() {
    sGuid guid{};
    return guid;
}


sGuid GetInventoryItemGUID(int inventoryId, Hash itemHash, sGuid parentGuid, Hash slotId) {
    sGuid outGuid{};
    
    // INVENTORY_GET_GUID_FROM_ITEMID (https://www.rdr2mods.com/nativedb/index/_namespace32/_0x886dfd3e185c8a89-r1862/)
    ITEMS::_0x886DFD3E185C8A89(inventoryId, (Any*)&parentGuid, itemHash, slotId, (Any*)&outGuid);

    return outGuid;
}


int GetPremiumCigCount() {
    // _INVENTORY_GET_INVENTORY_ID_FROM_PED (https://www.rdr2mods.com/nativedb/index/_namespace32/_0x13d234a2a3f66e63/)
    int inventoryId = ITEMS::_0x13D234A2A3F66E63(PLAYER::PLAYER_PED_ID());

    Hash premCigHash = GAMEPLAY::GET_HASH_KEY((char*)"CONSUMABLE_CIGARETTE_BOX");
    
    sGuid rootPlayerInvGUID = GetInventoryItemGUID(
        inventoryId,
        GAMEPLAY::GET_HASH_KEY((char*)"CHARACTER"),
        CreateNewGUID(),
        GAMEPLAY::GET_HASH_KEY((char*)"SLOTID_NONE")
    );

    Hash satchelSlot = GAMEPLAY::GET_HASH_KEY((char*)"SLOTID_SATCHEL");

    sGuid premiumCigGUID = GetInventoryItemGUID(inventoryId, premCigHash, rootPlayerInvGUID, satchelSlot);

    // Validation
    if (premiumCigGUID.data1 == 0 && premiumCigGUID.data2 == 0) {
        return 0;
    }

    // _PLAYER_INVENTORY_GET_ITEM_COUNT (https://www.rdr2mods.com/nativedb/index/_namespace32/_player_inventory_get_item_count-r1888/)
    return (int)ITEMS::_0xC97E0D2302382211(inventoryId, (Any*)&premiumCigGUID);
}


void DrawCigCount(int count) {
    const char* format = "Current count of cigs: %d";
    char buffer[30];

    snprintf(buffer, 29, format, count);

    // _UILOG_SET_CACHED_OBJECTIVE (https://www.rdr2mods.com/nativedb/index/_namespace73/_log_set_cached_objective/)
    UILOG::_0xFA233F8FE190514C((Any*)buffer);
    // _UILOG_PRINT_CACHED_OBJECTIVE (https://www.rdr2mods.com/nativedb/index/_namespace73/_log_print_cached_objective/)
    UILOG::_0xE9990552DEC71600();
    // _UILOG_CLEAR_HAS_DISPLAYED_CACHED_OBJECTIVE (https://www.rdr2mods.com/nativedb/index/_namespace73/_log_clear_has_displayed_cached_objective/)
    UILOG::_0xA3108D6981A5CADB();
    // _UILOG_CLEAR_CACHED_OBJECTIVE (https://www.rdr2mods.com/nativedb/index/_namespace73/_log_clear_cached_objective/)
    UILOG::_0xDFF0D417277B41F8();
}


// <<< Main loop >>>

void MainScriptLoop() {
    while (true) {
        // INPUT_LOOK_BEHIND = 0x9959a6f0 (https://www.rdr2mods.com/forums/topic/1575-list-of-keyboard-enums/)
        if (CONTROLS::IS_CONTROL_JUST_PRESSED(0, 0x9959a6f0)) {
            int count = GetPremiumCigCount();
            DrawCigCount(count);
            scriptWait(10);
        }
        WAIT(0);
    }
}


// <<< Entrypoint >>>

BOOL APIENTRY DllMain(HMODULE hInstance, DWORD reason, LPVOID lpReserved) {
    switch(reason) {
        case DLL_PROCESS_ATTACH:
            scriptRegister(hInstance, MainScriptLoop);
            break;
        case DLL_PROCESS_DETACH:
            scriptUnregister(hInstance);
            break;
    }

    return TRUE;
}
