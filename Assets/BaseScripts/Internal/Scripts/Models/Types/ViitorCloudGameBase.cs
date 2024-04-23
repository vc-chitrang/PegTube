#if UNITY_EDITOR

using System.Collections.Generic;

using UnityEngine;

using ViitorCloud.Base.BaseScripts.BuildUtils;
using ViitorCloud.Base.BaseScripts.Internal.Editor.GameConfig;
using ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.GameInfo;
namespace ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Types {
    public abstract class ViitorCloudGameBase {
        public static void UpdatePhysicsSettings(GameConfigManager gameInfo) {
            //ResetPhysicsSettings();
            UpdatePhysics2DSettings(gameInfo);
            UpdatePhysics3DSettings(gameInfo);

            List<ViitorCloudPhysicsCollisionOverride> overrides2D = gameInfo.GetGameInfo()?.projectSettings?.physics?.collisionOverrides2D;
            if (overrides2D != null) {
                foreach (ViitorCloudPhysicsCollisionOverride collisionOverride in overrides2D) {
                    IgnoreLayerCollision(collisionOverride.layer1, collisionOverride.layer2, collisionOverride.ignore, _2d: true, _3d: false);
                }
            }

            var overrides3D = gameInfo.GetGameInfo()?.projectSettings?.physics?.collisionOverrides3D;
            if (overrides3D != null) {
                foreach (ViitorCloudPhysicsCollisionOverride collisionOverride in overrides3D) {
                    IgnoreLayerCollision(collisionOverride.layer1, collisionOverride.layer2, collisionOverride.ignore, _2d: false, _3d: true);
                }
            }
        }
        /// <summary>
        /// This is used to reset the physics settings back to the default of no inter-layer collisions
        /// </summary>
        private static void ResetPhysicsSettings() {
            foreach (ViitorCloudLayer i in System.Enum.GetValues(typeof(ViitorCloudLayer))) {
                foreach (ViitorCloudLayer j in System.Enum.GetValues(typeof(ViitorCloudLayer))) {
                    IgnoreLayerCollision(i, j, i != j, _2d: true, _3d: true);
                }
            }
        }

        /// <summary>v
        /// This is used to update the 2D physics settings.
        /// </summary>
        private static void UpdatePhysics2DSettings(GameConfigManager gameInfo) {
            ViitorCloudPhysics2DSettings physics2DSettings = gameInfo.GetGameInfo()?.projectSettings?.physics?.physics2DSettings;
            if (physics2DSettings == null)
                return;
            Physics2D.gravity = physics2DSettings.gravity;
            Physics2D.velocityIterations = physics2DSettings.velocityIterations;
            Physics2D.positionIterations = physics2DSettings.positionIterations;
            Physics2D.velocityThreshold = physics2DSettings.velocityThreshold;
            Physics2D.maxLinearCorrection = physics2DSettings.maxLinearCorrection;
            Physics2D.maxAngularCorrection = physics2DSettings.maxAngularCorrection;
            Physics2D.maxTranslationSpeed = physics2DSettings.maxTranslationSpeed;
            Physics2D.maxRotationSpeed = physics2DSettings.maxRotationSpeed;
            Physics2D.baumgarteScale = physics2DSettings.baumgarteScale;
            Physics2D.baumgarteTOIScale = physics2DSettings.baumgarteTOIScale;
            Physics2D.timeToSleep = physics2DSettings.timeToSleep;
            Physics2D.linearSleepTolerance = physics2DSettings.linearSleepTolerance;
            Physics2D.angularSleepTolerance = physics2DSettings.angularSleepTolerance;
            Physics2D.defaultContactOffset = physics2DSettings.defaultContactOffset;

#if UNITY_2020_1_OR_NEWER
            Physics2D.simulationMode = physics2DSettings.simulationMode;
#else
				Physics2D.autoSimulation = Physics2DSettings.AutoSimulation;
#endif

            Physics2D.queriesHitTriggers = physics2DSettings.queriesHitTriggers;
            Physics2D.queriesStartInColliders = physics2DSettings.queriesStartInColliders;
            Physics2D.callbacksOnDisable = physics2DSettings.callbacksOnDisable;
            Physics2D.reuseCollisionCallbacks = physics2DSettings.reuseCollisionCallbacks;
            Physics2D.autoSyncTransforms = physics2DSettings.autoSyncTransforms;
        }

        /// <summary>
        /// This is used to update the 3D physics settings.
        /// </summary>
        private static void UpdatePhysics3DSettings(GameConfigManager gameInfo) {
            ViitorCloudPhysics3DSettings physics3DSettings = gameInfo.GetGameInfo()?.projectSettings?.physics?.physics3DSettings;
            if (physics3DSettings == null)
                return;
            Physics.gravity = physics3DSettings.gravity;
            Physics.bounceThreshold = physics3DSettings.bounceThreshold;
            Physics.sleepThreshold = physics3DSettings.sleepThreshold;
            Physics.defaultContactOffset = physics3DSettings.defaultContactOffset;
            Physics.defaultSolverIterations = physics3DSettings.defaultSolverIterations;
            Physics.defaultSolverVelocityIterations = physics3DSettings.defaultSolverVelocityIterations;
            Physics.queriesHitBackfaces = physics3DSettings.queriesHitBackfaces;
            Physics.queriesHitTriggers = physics3DSettings.queriesHitTriggers;
            Physics.autoSimulation = physics3DSettings.autoSimulation;
            Physics.autoSyncTransforms = physics3DSettings.autoSyncTransforms;
            Physics.reuseCollisionCallbacks = physics3DSettings.reuseCollisionCallbacks;
        }
        private static void IgnoreLayerCollision(ViitorCloudLayer layer1, ViitorCloudLayer layer2, bool ignore, bool _2d, bool _3d) {
            if (_3d) {
                Physics.IgnoreLayerCollision((int)layer1, (int)layer2, ignore);
            }

            if (_2d) {
                Physics2D.IgnoreLayerCollision((int)layer1, (int)layer2, ignore);
            }
        }
    }
}

#endif