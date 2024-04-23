using System;

using UnityEngine;
using UnityEngine.Serialization;
namespace ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.GameInfo {
	[Serializable]
	public class ViitorCloudPhysics3DSettings {
		[FormerlySerializedAs("Gravity")]
		[Tooltip("The gravity applied to all rigid bodies in the Scene.")]
		public Vector3 gravity = new Vector3(0f, -9.81f, 0f);

		[FormerlySerializedAs("BounceThreshold")]
		[Tooltip("Two colliding objects with a relative velocity below this will not bounce (default 2). Must be positive.")]
		public float bounceThreshold = 2f;

		[FormerlySerializedAs("SleepThreshold")]
		[Tooltip("The mass-normalized energy threshold, below which objects start going to sleep.")]
		public float sleepThreshold = 0.005f;

		[FormerlySerializedAs("DefaultContactOffset")]
		[Tooltip("The default contact offset of the newly created colliders. Colliders whose distance is less than the sum of their contactOffset values will generate contacts. The contact offset must be positive. Contact offset allows the collision detection system to predictively enforce the contact constraint even when the objects are slightly separated.")]
		public float defaultContactOffset = 0.01f;

		[FormerlySerializedAs("DefaultSolverIterations")]
		[Tooltip("The defaultSolverIterations determines how accurately Rigidbody joints and collision contacts are resolved. (default 6). Must be positive.")]
		public int defaultSolverIterations = 6;

		[FormerlySerializedAs("DefaultSolverVelocityIterations")]
		[Tooltip("The defaultSolverVelocityIterations affects how accurately the Rigidbody joints and collision contacts are resolved. (default 1). Must be positive.")]
		public int defaultSolverVelocityIterations = 1;

		[FormerlySerializedAs("QueriesHitBackfaces")]
		[Tooltip("Whether physics queries should hit back-face triangles.")]
		public bool queriesHitBackfaces = false;

		[FormerlySerializedAs("QueriesHitTriggers")]
		[Tooltip("Specifies whether queries (raycasts, sphere-casts, overlap tests, etc.) hit Triggers by default.")]
		public bool queriesHitTriggers = true;

		[FormerlySerializedAs("AutoSimulation")]
		[Tooltip("Sets whether the physics should be simulated automatically or not.")]
		public bool autoSimulation = true;

		[FormerlySerializedAs("AutoSyncTransforms")]
		[Tooltip("Whether or not to automatically sync transform changes with the physics system whenever a Transform component changes.")]
		public bool autoSyncTransforms = false;

		[FormerlySerializedAs("ReuseCollisionCallbacks")]
		[Tooltip("Determines whether the garbage collector should reuse only a single instance of a Collision type for all collision callbacks.")]
		public bool reuseCollisionCallbacks = true;
	}
}
