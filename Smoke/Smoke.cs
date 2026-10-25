using System;
using RDR2;
using RDR2.UI;
using RDR2.Math;
using RDR2.Native;

using AnimationManager;
using FileLogger;
using ItemManager;
using PropManager;


namespace Smoke {
    public class Smoke : Script {
        private const string LOG_PATH = "scripts/Smoke.log";
        private const string LOG_NAME = "Smoke";
        private readonly CigaretteBoxManager cigItemManager = ItemManagerFactory.GetPlayerCigaretteBoxManager(LOG_PATH, LOG_NAME);
        private readonly AnimationManager.AnimationManager startAnimManager = AnimationManagerFactory.GetStartSmokeAnimManager(LOG_PATH, LOG_NAME);
        private readonly AnimationManager.AnimationManager endAnimationManager = AnimationManagerFactory.GetEndSmokeAnimManager(LOG_PATH, LOG_NAME);
        private readonly PropManager.PropManager cigarPropManager = PropManagerFactory.GetCigarettePropManager(LOG_PATH, LOG_NAME);
        private readonly FileLogger.FileLogger logger = new FileLogger.FileLogger(LOG_PATH, LOG_NAME);
        private bool smoking = false;
        private bool inHand = false;

        public Smoke() {
            Interval = 1;
            Tick += SmokeV3Tick;
        }

        private void SmokeV3Tick(object sender, EventArgs args) {
            if (ShouldSpawnCig()) {
                SpawnCig();
            } else if (ShouldDropCig()) {
                DropCig();
            } else if (smoking && IsPlayerKo()) {
                logger.Log("Smoking interrupted");
                KoSummaryLog();
                DropCig(true);
            } else if (ShouldSwitchStance()) {
                SwitchStance();
            }
        }

        private bool ShouldSpawnCig() {
            if (smoking) return false;
            Ped playerPed = Game.Player.Character;

            if (playerPed == null || !playerPed.IsIdle) return false;
            if (!ComboPressed(true)) return false;

            // Fallback to forcing the call, the player might have bought more cigs
            if (cigItemManager.GetItemCount() <= 0 && cigItemManager.GetItemCount(force: true) <= 0) {
                Screen.PrintSubtitle("You don't have any cigarettes on you!");
                playerPed.PlaySpeech("GENERIC_CURSE_MED", "ARTHUR", eSpeechParams.Force);
                return false;
            }

            return true;
        }

        private void SpawnCig() {
            Ped playerPed = Game.Player.Character;
            Vector3 spawnPos = playerPed.Position.Around(10);

            if (!cigarPropManager.LoadProp(spawnPos, Wait)) return;
            if (!startAnimManager.LoadAnimDict(Wait)) return;

            startAnimManager.PlayAnim(playerPed, true);
            cigarPropManager.prop.AttachTo(
                playerPed,
                playerPed.GetBoneIndex(startAnimManager.initialAttach.attachmentBone),
                startAnimManager.initialAttach.posOffset,
                startAnimManager.initialAttach.rotOffset
            );
            Wait(startAnimManager.animationTime);
            cigarPropManager.prop.AttachTo(
                playerPed,
                playerPed.GetBoneIndex(startAnimManager.finalAttach.attachmentBone),
                startAnimManager.finalAttach.posOffset,
                startAnimManager.finalAttach.rotOffset
            );

            cigarPropManager.StartPtfxOnProp(spawnPos, Wait);

            smoking = true;
            inHand = false;

            cigItemManager.RemoveItem();
            Screen.PrintSubtitle(String.Format("Cig equiped. Count: {0}", cigItemManager.GetItemCount()));
        }

        private bool ShouldDropCig() {
            if (!smoking) return false;

            Ped playerPed = Game.Player.Character;

            return (
                playerPed == null
                || !playerPed.IsAlive
                || playerPed.IsIdle && ComboPressed(true)
            );
        }

        private void DropCig(bool skipAnim = false) {
            logger.Log("Dropping cig");
            Ped playerPed = Game.Player.Character;

            if (endAnimationManager.LoadAnimDict(Wait) && !skipAnim) {
                endAnimationManager.PlayAnim(playerPed, playerPed.IsOnHorse);
                
                Wait(endAnimationManager.animationTime);

                // Null propagation is not supported on RDR2 Scripthookdotnet CS version
                if (cigarPropManager.prop != null) {
                    cigarPropManager.prop.AttachTo(
                        playerPed,
                        playerPed.GetBoneIndex(endAnimationManager.finalAttach.attachmentBone),
                        endAnimationManager.finalAttach.posOffset,
                        endAnimationManager.finalAttach.rotOffset
                    );
                }
            }

            startAnimManager.DiscardAnimDict();
            endAnimationManager.DiscardAnimDict();

            if (cigarPropManager.prop != null) {
                logger.Log("Detaching cig");
                cigarPropManager.prop.Detach();
                unsafe {
                    int propHandle = cigarPropManager.prop.Handle;
                    logger.Warn("[UNSAFE] Marking cigar prop as no longer needed");
                    ENTITY.SET_ENTITY_AS_NO_LONGER_NEEDED(&propHandle);
                }
                cigarPropManager.prop = null;
            }

            if (cigarPropManager.model != null) {
                logger.Log("Mark cigar model as no longer needed");
                STREAMING.SET_MODEL_AS_NO_LONGER_NEEDED(cigarPropManager.model);
                cigarPropManager.model = null;
            }

            cigarPropManager.StopPtfxOnProp();

            smoking = false;
            inHand = false;
            Screen.PrintSubtitle("Cigar dropped");
        }

        private bool ShouldSwitchStance() {
            if (!smoking || !Game.Player.Character.IsIdle) return false;

            int timeout = 0;
            while (Game.IsControlPressed(eInputType.Jump)) {
                timeout++;
                Wait(1);
                if (timeout > 100) return true;
            }

            return false;
        }

        private void SwitchStance() {
            logger.Log("Switching stance to: {0}", inHand ? "Lip" : "Hand");
            Ped playerPed = Game.Player.Character;
            Vector3 spawnPos = playerPed.Position.Around(10);

            if (!cigarPropManager.LoadProp(spawnPos, Wait)) return;
            if (!startAnimManager.LoadAnimDict(Wait)) return;

            // When switching from lip to hand, we extend the wait time for the anim
            int animTime = startAnimManager.animationTime;
            if (!inHand) animTime += 4000;

            startAnimManager.PlayAnim(playerPed, true);
            Wait(animTime);
            if (inHand) {
                logger.Log("Attaching to Lip");
                cigarPropManager.prop.AttachTo(
                    playerPed,
                    playerPed.GetBoneIndex(startAnimManager.finalAttach.attachmentBone),
                    startAnimManager.finalAttach.posOffset,
                    startAnimManager.finalAttach.rotOffset
                );
            } else {
                logger.Log("Attaching to finger");
                cigarPropManager.prop.AttachTo(
                    playerPed,
                    playerPed.GetBoneIndex(startAnimManager.initialAttach.attachmentBone),
                    startAnimManager.initialAttach.posOffset,
                    startAnimManager.initialAttach.rotOffset
                );
            }

            inHand = !inHand;

            Screen.PrintSubtitle(String.Format("Cig equiped. Count: {0}", cigItemManager.GetItemCount()));
        }

        private bool ComboPressed() {
            int timeout = 0;
            while (Game.IsControlPressed(eInputType.LookBehind)) {
                timeout++;
                Wait(1);
                if (timeout > 100) return true;
            }

            return false;
        }

        private bool ComboPressed(bool debug = false) {
            bool val = ComboPressed();
            if (debug && val) {
                Console.Out.WriteLine("Combo Pressed");
            }
            return val;
        }

        private bool IsPlayerKo() {
            Ped playerPed = Game.Player.Character;
            return (
                playerPed.IsFallingOver
                || playerPed.IsBeingArrested
                || playerPed.IsRagdoll
                || playerPed.IsBeingJacked
                || playerPed.IsBeingStunned
                || playerPed.IsDeadOrDying
                || playerPed.IsFatallyInjured
                || playerPed.IsIncapacitated
            );
        }

        private void KoSummaryLog() {
            Ped playerPed = Game.Player.Character;
            string[] bools = {
                playerPed.IsFallingOver.ToString(),
                playerPed.IsBeingArrested.ToString(),
                playerPed.IsRagdoll.ToString(),
                playerPed.IsBeingJacked.ToString(),
                playerPed.IsBeingStunned.ToString(),
                playerPed.IsDeadOrDying.ToString(),
                playerPed.IsFatallyInjured.ToString(),
                playerPed.IsIncapacitated.ToString()
            };

            logger.Log("KO Reason: {0}", String.Join(", ", bools));
        }
    }
}