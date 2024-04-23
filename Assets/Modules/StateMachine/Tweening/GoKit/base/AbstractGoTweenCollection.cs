using static Modules.Utility.Utility;
using System.Collections.Generic;

namespace ViitorCloud.GameHelper.GoKit {
	/// <summary>
	/// base class for TweenChains and TweenFlows
	/// </summary>
	public class AbstractGoTweenCollection : AbstractGoTween {
		protected List<TweenFlowItem> _tweenFlows = new List<TweenFlowItem>();


		/// <summary>
		/// data class that wraps an AbstractTween and its start time for the timeline
		/// </summary>
		protected class TweenFlowItem {
			public float startTime;
			public float duration;
			public AbstractGoTween tween;


			public TweenFlowItem(float startTime, AbstractGoTween tween) {
				this.tween = tween;
				this.startTime = startTime;
				this.duration = tween.totalDuration;
			}


			public TweenFlowItem(float startTime, float duration) {
				this.duration = duration;
				this.startTime = startTime;
			}

		}

		public AbstractGoTweenCollection(GoTweenCollectionConfig config) {
			// copy the TweenConfig info over
			id = config.id;
			loopType = config.loopType;
			iterations = config.iterations;
#if UNITY_EDITOR && !IGNORE_NOISY_TWEEN_WARNING
			if (iterations != 1) {
				LogWarning("Since this tween flow is looping, we will be updating after a tween is completed. Otherwise the tweens will not correctly loop");
			}
#endif
			updateType = config.propertyUpdateType;
			_onComplete = config.onCompleteHandler;
			_onStart = config.onStartHandler;
			timeScale = 1;
			state = GoTweenState.Paused;
			Go.addTween(this);
		}


		#region AbstractTween overrides

		/// <summary>
		/// returns a list of all Tweens with the given target in the collection
		/// technically, this should be marked as internal
		/// </summary>
		public List<GoTween> tweensWithTarget(object target) {
			List<GoTween> list = new List<GoTween>();

			foreach (var item in _tweenFlows) {
				// skip TweenFlowItems with no target
				if (item.tween == null)
					continue;

				// check Tweens first
				var tween = item.tween as GoTween;
				if (tween != null && tween.target == target)
					list.Add(tween);

				// check for TweenCollections
				if (tween == null) {
					var tweenCollection = item.tween as AbstractGoTweenCollection;
					if (tweenCollection != null) {
						var tweensInCollection = tweenCollection.tweensWithTarget(target);
						if (tweensInCollection.Count > 0)
							list.AddRange(tweensInCollection);
					}
				}
			}

			return list;
		}


		public override bool removeTweenProperty(AbstractTweenProperty property) {
			foreach (var tweenFlowItem in _tweenFlows) {
				// skip delay items which have no tween
				if (tweenFlowItem.tween == null)
					continue;

				if (tweenFlowItem.tween.removeTweenProperty(property))
					return true;
			}

			return false;
		}


		public override bool containsTweenProperty(AbstractTweenProperty property) {
			foreach (var tweenFlowItem in _tweenFlows) {
				// skip delay items which have no tween
				if (tweenFlowItem.tween == null)
					continue;

				if (tweenFlowItem.tween.containsTweenProperty(property))
					return true;
			}

			return false;
		}


		public override List<AbstractTweenProperty> allTweenProperties() {
			var propList = new List<AbstractTweenProperty>();

			foreach (var tweenFlowItem in _tweenFlows) {
				// skip delay items which have no tween
				if (tweenFlowItem.tween == null)
					continue;

				propList.AddRange(tweenFlowItem.tween.allTweenProperties());
			}

			return propList;
		}


		/// <summary>
		/// we are always considered valid because our constructor adds us to Go and we start paused
		/// </summary>
		public override bool isValid() {
			return true;
		}


		/// <summary>
		/// tick method. if it returns true it indicates the tween is complete
		/// </summary>
		public override bool update(float deltaTime) {
			base.update(deltaTime);

			// if we are looping back on a PingPong loop
			var convertedElapsedTime = _isLoopingBackOnPingPong ? duration - _elapsedTime : _elapsedTime;

			if (state == GoTweenState.Complete) {

				//Make sure all tweens get completed
				//Sometimes, it happens the last tween has not yet completed when the tweenchain is completed (maybe for a 0.0000000001 difference in the way convertedElapsedTime calculated)
				foreach (var flow in _tweenFlows) {
					// only update flows that have a Tween and whose startTime has passed
					if (flow.tween != null && flow.tween.state != GoTweenState.Complete) {

						//Finish the tween and update it to call onComplete if needed
						flow.tween.complete();
						flow.tween.update(0);
					}
				}

				if (!_didComplete)
					onComplete();

				return true; //true if complete

			} else {
				if (_tweenFlows.Count <= 0) {
					return true; //if there are no tweens, then this object is complete
				}

				try {
					// update all properties
					foreach (var flow in _tweenFlows) {
						// only update flows that have a Tween and whose startTime has passed
						if (flow.tween != null && flow.startTime < convertedElapsedTime && (iterations != 1 || (flow.tween.state != GoTweenState.Complete && flow.tween.state != GoTweenState.Destroyed))) {
							// TODO: further narrow down who gets an update for efficiency
							var tweenConvertedElapsed = convertedElapsedTime - flow.startTime;
							flow.tween.goTo(tweenConvertedElapsed);
						}
					}
				} catch (System.Exception e) {
					LogError(string.Format("[GoTween] Exception during update(), were you deleting something while the collection was iterating? {0}", e));
					return false;
				}

				return false; //false if not complete

			}
		}


		public override void rewind() {
			state = GoTweenState.Paused;

			// reset all state here
			_elapsedTime = _totalElapsedTime = 0;
			_isLoopingBackOnPingPong = false;
			_completedIterations = 0;
		}


		/// <summary>
		/// completes the tween. sets the object to it's final position as if the tween completed normally
		/// </summary>
		public override void complete() {
			if (iterations < 0)
				return;

			base.complete();

			foreach (var flow in _tweenFlows) {
				// only update flows that have a Tween and whose startTime has passed
				if (flow.tween != null)
					flow.tween.goTo(flow.tween.totalDuration);
			}
		}

		#endregion

	}
}