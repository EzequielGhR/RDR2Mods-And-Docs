using System;
using RDR2;
using RDR2.Math;
using RDR2.Native;
using FileLogger;

namespace AnimationManager {
    public class PropAttachments {
        public readonly BoneID attachmentBone;
        public readonly Vector3 posOffset;
        public readonly Vector3 rotOffset;
        
        public PropAttachments(BoneID attachmentBone, Vector3 posOffset, Vector3 rotOffset) {
            this.attachmentBone = attachmentBone;
            this.posOffset = posOffset;
            this.rotOffset = rotOffset;
        }
    }

    public class AnimationManager {
        public readonly string dictName;
        public readonly string animName;
        public readonly int animationTime;
        public readonly PropAttachments initialAttach;
        public readonly PropAttachments finalAttach;
        private readonly FileLogger.FileLogger logger;

        public AnimationManager(
            string dictName,
            string animName,
            int animationTime,
            BoneID attachBone1,
            Vector3 posOffset1,
            Vector3 rotOffset1,
            BoneID attachBone2,
            Vector3 posOffset2,
            Vector3 rotOffset2,
            string logPath,
            string logName,
            string serviceName = "AnimationManager"
        ) {
            logger = new FileLogger.FileLogger(logPath, logName, serviceName);
            this.dictName = dictName;
            this.animName = animName;
            this.animationTime = animationTime;
            
            initialAttach = new PropAttachments(
                attachBone1,
                posOffset1,
                rotOffset1
            );

            finalAttach = new PropAttachments(
                attachBone2,
                posOffset2,
                rotOffset2
            );
        }

        public bool LoadAnimDict(Action<int> WaitFunc) {
            bool dictLoaded = STREAMING.HAS_ANIM_DICT_LOADED(dictName);
            if (dictLoaded) {
                logger.Log("Dict {0} already loaded", dictName);
                return true;
            }
            
            int timeout = 0;
            STREAMING.REQUEST_ANIM_DICT(dictName);
            logger.Log("Dict load requested for {0}", dictName);

            while (!dictLoaded && timeout < 500) {
                dictLoaded = STREAMING.HAS_ANIM_DICT_LOADED(dictName);
                timeout++;
                WaitFunc(1);
            }

            if (!dictLoaded) {
                logger.Error("Anim dict failed to load before timeout");
                return false;
            }
            
            
            return true;
        }

        public void DiscardAnimDict() {
            if (STREAMING.HAS_ANIM_DICT_LOADED(dictName)) {
                logger.Log("Discarding loaded anim dict: {0}", dictName);
                try {
                    STREAMING.REMOVE_ANIM_DICT(dictName);
                } catch (Exception e) {
                    logger.Error("Failed to discard anim dict: {0}. Error: {1}", dictName, e.Message);
                }
            }
        }

        public void PlayAnim(Ped ped, bool upperBody = false) {
            if (!STREAMING.HAS_ANIM_DICT_LOADED(dictName)) {
                logger.Error("Anim dict {0} not loaded", dictName);
                return;
            }

            TASK.TASK_PLAY_ANIM(
                ped.Handle, dictName, animName,
                8, -8, -1,
                (int)eScriptedAnimFlags.Secondary,
                0.0f, false,
                (int)(upperBody ? eIkControlFlags.Upperbody : eIkControlFlags.None),
                false, "", false
            );
        }

        public void WaitForAnimToFinish(Ped ped, Action<int> WaitFunc, int timeout = 1000 * 60, bool fallbackToAnimTime = false) {
            int timeCount = 0;
            
            int taskStatus = TASK.GET_SCRIPT_TASK_STATUS(ped, Game.Joaat("SCRIPT_TASK_PLAY_ANIM"), true);
            if (taskStatus == 7) {
                logger.Error("Task not found on waiting stage");
                return;
            }

            if (taskStatus == 8 && fallbackToAnimTime) {
                logger.Warn("Animation finished instantly, falling back to anim time");
                WaitFunc(animationTime);
                return;
            }

            while (timeCount < timeout) {
                timeCount++;
                taskStatus = TASK.GET_SCRIPT_TASK_STATUS(ped, Game.Joaat("SCRIPT_TASK_PLAY_ANIM"), false);
                // 8 Means the task finished
                if (taskStatus == 8) {
                    logger.Log("Task finished");
                    return;
                }
                WaitFunc(1);
            }

            logger.Warn("Timeout reached before finishing task");
        }
    }

    public class AnimationManagerFactory {
        public static AnimationManager GetStartSmokeAnimManager(string logPath, string logName, string serviceName = "AnimationManager:StartSmoke") {
            return new AnimationManager(
                "amb_rest@world_human_smoking@male_a@stand_enter",
                "enter_frontright",
                3000,
                BoneID.ID_SKEL_R_FINGER13,
                new Vector3(0, 0.01f, 0.01f),
                new Vector3(0, 0, -90),
                BoneID.FACIAL_R_LipCorner,
                new Vector3(0.01f, 0, 0.02f),
                new Vector3(0, 90, 0),
                logPath,
                logName,
                serviceName
            );
        }

        public static AnimationManager GetEndSmokeAnimManager(string logPath, string logName, string serviceName = "AnimationManager:EndSmoke") {
            return new AnimationManager(
                "amb_rest@world_human_smoking@male_a@stand_exit",
                "exit_frontright",
                500,
                BoneID.FACIAL_R_LipCorner,
                new Vector3(0.01f, 0, 0.02f),
                new Vector3(0, 90, 0),
                BoneID.ID_SKEL_R_FINGER13,
                new Vector3(0, 0.01f, 0.01f),
                new Vector3(0, 0, -90),
                logPath,
                logName,
                serviceName
            );
        }

        public static AnimationManager GetIdleSmokeAnimManager(string logPath, string logName, string serviceName = "AnimationManager:IdleSmoke") {
            return new AnimationManager(
                "amb_rest@world_human_smoking@male_a@idle_a",
                "idle_a",
                5000,
                BoneID.ID_SKEL_R_FINGER13,
                new Vector3(0, 0.01f, 0.01f),
                new Vector3(0, 0, -90),
                BoneID.ID_SKEL_R_FINGER13,
                new Vector3(0, 0.01f, 0.01f),
                new Vector3(0, 0, -90),
                logPath,
                logName,
                serviceName
            );
        }
    }
}