#include "propManager.h"
#include "../FileLogger/fileLogger.h"
#include "../../SDK/inc/natives.h"


class PropManager {
    private:
        const char* modelName;
        const char* ptfxDict;
        const char* ptfxName;
        FileLogger* logger;

        bool LoadPtfxDict(void (*WaitFunc)(int)) {
            int timeout = 0;
            bool dictLoaded = STREAMING::HAS_NAMED_PTFX_ASSET_LOADED((char*)ptfxDict);
            if (dictLoaded) return true;

            STREAMING::REQUEST_NAMED_PTFX_ASSET((char*)ptfxDict);
            while (!dictLoaded && timeout < 2000) {
                dictLoaded = STREAMING::HAS_NAMED_PTFX_ASSET_LOADED((char*)ptfxDict);
                timeout++;
                WaitFunc(1);
            }

            if (!dictLoaded) {
                logger->Error("Failed to load ptfx dict in time");
                return false;
            }

            return true;
        } 
    
    public:
        Hash* pModel = nullptr;
        Object* pProp = nullptr;
        int* pPtfxHandle = nullptr;

        PropManager(
            const char* modelName,
            const char* ptfxDict,
            const char* ptfxName,
            const char* logPath,
            const char* logName,
            const char* serviceName = "PropManager"
        ) {
            logger = new FileLogger(logPath, logName, serviceName);
            this->modelName = modelName;
            this->ptfxDict = ptfxDict;
            this->ptfxName = ptfxName;
        }

        ~PropManager() {
            StopPtfxOnProp();
            MarkPropAsNoLongerNeeded();
            MarkModelAsNoLongerNeeded();
            delete logger;
        }

        bool LoadProp(Vector3 spawn, void (*WaitFunc)(int)) {
            if (pModel == nullptr) {
                logger->Log("Creating model: %s", modelName);
                pModel = new Hash;
                *pModel = GAMEPLAY::GET_HASH_KEY((char*)modelName);
            }

            if (!STREAMING::HAS_MODEL_LOADED(*pModel)) {
                logger->Log("Requesting Model Load");
                if (STREAMING::IS_MODEL_VALID(*pModel)) {
                    STREAMING::REQUEST_MODEL(*pModel, false);
                }

                int timeout = 0;
                while (!STREAMING::HAS_MODEL_LOADED(*pModel)) {
                    WaitFunc(1);
                    timeout++;
                }
            }

            if (!STREAMING::HAS_MODEL_LOADED(*pModel)) {
                logger->Error("Model '%s' failed to load", modelName);
                return false;
            }

            if (pProp == nullptr) {
                logger->Log("Creating prop");
                pProp = new Object;
                *pProp = OBJECT::CREATE_OBJECT(
                    *pModel, spawn.x, spawn.y, spawn.z,
                    true, true, true, false, true
                );
            }

            if (pProp == nullptr) {
                logger->Error("Prop creation failed");
                return false;
            }

            return true;
        }

        void StartPtfxOnProp(Vector3 spawn, void (*WaitFunc)(int)) {
            if (!LoadProp(spawn, WaitFunc)) return;
            if (!LoadPtfxDict(WaitFunc)) return;

            logger->Log("Starting PTFX with name: %s", ptfxName);

            GRAPHICS::USE_PARTICLE_FX_ASSET((char*)ptfxDict);
            
            pPtfxHandle = new int;
            *pPtfxHandle = GRAPHICS::START_PARTICLE_FX_LOOPED_ON_ENTITY(
                (char*)ptfxName, *pProp,
                -0.05f, 0, 0,
                0, 0, 0,
                1,
                false,
                false,
                true
            );

            logger->Log("Loaded PTFX with handle: %d", *pPtfxHandle);
        }

        void StopPtfxOnProp() {
            if (pPtfxHandle != nullptr) {
                logger->Log("Stopping ptfx with handle: %d", *pPtfxHandle);
                GRAPHICS::STOP_PARTICLE_FX_LOOPED(*pPtfxHandle, true);
                
                delete pPtfxHandle;
                pPtfxHandle = nullptr;
            }

            try {
                logger->Log("Removing PTFX dict: %s", ptfxDict);
                STREAMING::_REMOVE_NAMED_PTFX_ASSET((char*)ptfxDict);
            } catch (std::exception& e) {
                logger->Error("Failed to remove ptfx dict: %s", e.what());
            } catch (...) {
                logger->Error("Unknown error attempting to remove ptfx dict");
            }
        }

        void MarkPropAsNoLongerNeeded() {
            if (pProp == nullptr) return;

            logger->Log("Marking prop as no longer needed");
            ENTITY::SET_ENTITY_AS_NO_LONGER_NEEDED(pProp);
            delete pProp;
            pProp = nullptr;
        }

        void MarkModelAsNoLongerNeeded() {
            if (pModel == nullptr) return;

            logger->Log("Marking model as no longer needed");
            STREAMING::SET_MODEL_AS_NO_LONGER_NEEDED(*pModel);
            delete pModel;
            pModel = nullptr;
        }
};

