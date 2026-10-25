using System;
using FileLogger;
using RDR2;
using RDR2.Math;
using RDR2.Native;


namespace PropManager {
    public class PropManager {
        private readonly string modelName;
        private readonly string ptfxDict;
        private readonly string ptfxName;
        private readonly FileLogger.FileLogger logger;
        public Model model = null;
        public Prop prop = null;
        public int? ptfx = null;

        public PropManager(string modelName, string ptfxDict, string ptfxName, string logPath, string logName, string serviceName = "PropManager") {
            logger = new FileLogger.FileLogger(logPath, logName, serviceName);
            this.modelName = modelName;
            this.ptfxDict = ptfxDict;
            this.ptfxName = ptfxName;
        }
        
        public bool LoadProp(Vector3 spawn, Action<int> WaitFunc) {
            if (model == null) {
                logger.Log("Creating model: {0}", modelName);
                model = new Model(modelName);
            }

            if (!model.IsLoaded) {
                logger.Log("Requesting model load");
                model.Request();

                int timeout = 0;
                while (!model.IsLoaded && timeout < 500) {
                    WaitFunc(1);
                    timeout++;
                }
            }

            if (!model.IsLoaded) {
                logger.Error("Model {0} failed to load", modelName);
                return false;
            }
            
            if (prop == null) {
                logger.Log("Creating prop");
                prop = World.CreateProp(model, spawn, true, true);
            }

            if (prop == null) {
                logger.Error("Propt cration failed");
                return false;
            }

            return true;
        }

        public void StartPtfxOnProp(Vector3 spawn, Action<int> WaitFunc) {
            if (!LoadProp(spawn, WaitFunc)) return;
            if (!LoadPtfxDict(WaitFunc)) return;

            logger.Log("Starting PTFX with name: {0}", ptfxName);

            // You need to let the game know you are going to use the loaded dict
            GRAPHICS.USE_PARTICLE_FX_ASSET(ptfxDict);
            
            ptfx = GRAPHICS.START_PARTICLE_FX_LOOPED_ON_ENTITY(
                ptfxName,
                prop.Handle,
                new Vector3(-0.05f, 0, 0),
                new Vector3(0, 0, 0),
                1,
                false,
                false,
                true
            );
            logger.Log("PTFX loaded with handle: {0}", ptfx);
        }

        public void StopPtfxOnProp() {
            if (ptfx.HasValue) {
                logger.Log("Stopping ptfx with handle: {0}", ptfx.Value);
                GRAPHICS.STOP_PARTICLE_FX_LOOPED(ptfx.Value, true);
                ptfx = null;
            }

            try {
                logger.Log("Removing PTFX dict: {0}", ptfxDict);
                STREAMING.REMOVE_NAMED_PTFX_ASSET(Game.Joaat(ptfxDict));
            } catch (Exception e) {
                logger.Error("Failed to remove ptfx core dict: {0}", e.Message);
            }
        }

        private bool LoadPtfxDict(Action<int> WaitFunc) {
            int timeout = 0;
            bool dictLoaded = STREAMING.HAS_NAMED_PTFX_ASSET_LOADED(Game.Joaat(ptfxDict));
            if (dictLoaded) return true;

            STREAMING.REQUEST_NAMED_PTFX_ASSET(Game.Joaat(ptfxDict));
            while (!dictLoaded && timeout < 2000) {
                dictLoaded = STREAMING.HAS_NAMED_PTFX_ASSET_LOADED(Game.Joaat(ptfxDict));
                timeout++;
                WaitFunc(1);
            }

            if (!dictLoaded) {
                Console.Error.WriteLine("Failed to load ptfx dict in time");
                return false;
            }

            return true;
        }
    }

    public class PropManagerFactory {
        public static PropManager GetCigarettePropManager(string logPath, string logName, string serviceName = "PropManager:Cigarette") {
            return new PropManager(
                "P_CIGARETTE_DYNAMIC_01X",
                "core",
                "ent_amb_cig_smoke",
                logPath,
                logName,
                serviceName
            );
        }
    }
}