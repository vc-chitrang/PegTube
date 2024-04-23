using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

using ViitorCloud.Container.Internal.Data;
namespace ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.GameInfo {
    [Serializable]
    public class ViitorCloudCustomPhysicsSettings {
        [FormerlySerializedAs("Physics2DSettings")]
        [Tooltip("By default, settings are same as [Edit->Project Settings->Physics 2D]")]
        public ViitorCloudPhysics2DSettings physics2DSettings = null;

        [FormerlySerializedAs("Physics3DSettings")]
        [Tooltip("By default, settings are same as [Edit->Project Settings->Physics]")]
        public ViitorCloudPhysics3DSettings physics3DSettings = null;

        [FormerlySerializedAs("CollisionOverrides2D")]
        [Tooltip("By default, all layers collide with themselves and that is it")]
        public List<ViitorCloudPhysicsCollisionOverride> collisionOverrides2D = null;

        [FormerlySerializedAs("CollisionOverrides3D")]
        [Tooltip("By default, all layers collide with themselves and that is it")]
        public List<ViitorCloudPhysicsCollisionOverride> collisionOverrides3D = null;

        public ViitorCloudCustomPhysicsSettings() {
            physics2DSettings = new ViitorCloudPhysics2DSettings();
            physics3DSettings = new ViitorCloudPhysics3DSettings();
            collisionOverrides2D = new List<ViitorCloudPhysicsCollisionOverride>();
            collisionOverrides3D = new List<ViitorCloudPhysicsCollisionOverride>();
        }
    }
}