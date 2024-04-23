using System;

using UnityEngine;
using UnityEngine.Serialization;
namespace ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.GameInfo {
	[Serializable]
	public class ViitorCloudPhysics2DSettings {
		[FormerlySerializedAs("Gravity")]
		[Tooltip("The gravity applied to all rigid bodies in the Scene.")]
		public Vector2 gravity = new Vector2(0.0f, -9.81f);

		[FormerlySerializedAs("VelocityIterations")]
		[Tooltip("The number of iterations of the physics solver when considering objects' velocities. A higher number of interations will improve accuracy at the expense of processing overhead.")]
		public int velocityIterations = 8;

		[FormerlySerializedAs("PositionIterations")]
		[Tooltip("The number of iterations of the physics solver when considering objects' positions. A higher number of interations will improve accuracy at the expense of processing overhead.")]
		public int positionIterations = 3;

		[FormerlySerializedAs("VelocityThreshold")]
		[Tooltip("Any collisions with a relative linear velocity below this threshold will be treated as inelastic.")]
		public float velocityThreshold = 1.0f;

		[FormerlySerializedAs("MaxLinearCorrection")]
		[Tooltip("The maximum linear position correction used when solving constraints. This helps to prevent overshoot.")]
		public float maxLinearCorrection = 0.2f;

		[FormerlySerializedAs("MaxAngularCorrection")]
		[Tooltip("The maximum angular position correction used when solving constraints. This helps to prevent overshoot.")]
		public float maxAngularCorrection = 8f;

		[FormerlySerializedAs("MaxTranslationSpeed")]
		[Tooltip("The maximum linear speed of a rigid-body per physics update. Increasing this can cause numerical problems.")]
		public float maxTranslationSpeed = 100f;

		[FormerlySerializedAs("MaxRotationSpeed")]
		[Tooltip("The maximum angular speed of a rigid-body per physics update. Increasing this can cause numerical problems.")]
		public float maxRotationSpeed = 360f;

		[FormerlySerializedAs("BaumgarteScale")]
		[Tooltip("The scale factor that controls how fast overlaps are resolved.")]
		public float baumgarteScale = 0.2f;

		[FormerlySerializedAs("BaumgarteTOIScale")]
		[Tooltip("The scale factor that controls how fast TOI overlaps are resolved.")]
		public float baumgarteTOIScale = 0.75f;

		[FormerlySerializedAs("TimeToSleep")]
		[Tooltip("The time in seconds that a rigid-body must be still before it will go to sleep.")]
		public float timeToSleep = 0.5f;

		[FormerlySerializedAs("LinearSleepTolerance")]
		[Tooltip("A rigid-body cannot sleep if its linear velocity is above this tolerance.")]
		public float linearSleepTolerance = 0.01f;

		[FormerlySerializedAs("AngularSleepTolerance")]
		[Tooltip("A rigid-body cannot sleep if its angular velocity is above this tolerance.")]
		public float angularSleepTolerance = 2f;

		[FormerlySerializedAs("DefaultContactOffset")]
		[Tooltip("The default contact offset of the newly created colliders. Colliders whose distance is less than the sum of their contactOffset values will generate contacts. The contact offset must be positive. Contact offset allows the collision detection system to predictively enforce the contact constraint even when the objects are slightly separated.")]
		public float defaultContactOffset = 0.01f;

#if UNITY_2020_1_OR_NEWER
		[FormerlySerializedAs("SimulationMode")]
		[Tooltip("Controls when Unity executes the 2D physics simulation.")]
		public SimulationMode2D simulationMode = SimulationMode2D.FixedUpdate;
#else
		[Tooltip("Sets whether the physics should be simulated automatically or not.")]
		public bool AutoSimulation = true;
#endif

		[FormerlySerializedAs("QueriesHitTriggers")]
		[Tooltip("Do raycasts detect Colliders configured as triggers?")]
		public bool queriesHitTriggers = true;

		[FormerlySerializedAs("QueriesStartInColliders")]
		[Tooltip("Sets the raycasts or linecasts that start inside Colliders to detect or not detect those Colliders.")]
		public bool queriesStartInColliders = true;

		[FormerlySerializedAs("CallbacksOnDisable")]
		[Tooltip("Use this to control whether or not the appropriate OnCollisionExit2D or OnTriggerExit2D callbacks should be called when a Collider2D is disabled.")]
		public bool callbacksOnDisable = true;

		[FormerlySerializedAs("ReuseCollisionCallbacks")]
		[Tooltip("Determines whether the garbage collector should reuse only a single instance of a Collision2D type for all collision callbacks.")]
		public bool reuseCollisionCallbacks = true;

		[FormerlySerializedAs("AutoSyncTransforms")]
		[Tooltip("Whether or not to automatically sync transform changes with the physics system whenever a Transform component changes.")]
		public bool autoSyncTransforms = false;
	}
}
