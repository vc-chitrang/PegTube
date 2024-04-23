using UnityEngine;
using System.Collections;

namespace ViitorCloud.GameHelper.GoKit {
	public class AnchorPositionsTweenProperty : AbstractVector3TweenProperty {
		protected new RectTransform _target;

		public AnchorPositionsTweenProperty(Vector2 endValue, bool isRelative = false)
			: base(new Vector3(endValue.x, endValue.y, 0f), isRelative) {
		}

		#region Object overrides

		public override bool validateTarget(object target) {
			return target is RectTransform;
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}


		public override bool Equals(object obj) {
			return base.Equals(obj);
		}

		#endregion


		public override void prepareForUse() {
			_target = _ownerTween.target as RectTransform;

			_endValue = _originalEndValue;

			// if this is a from tween we need to swap the start and end values
			if (_ownerTween.isFrom) {
				if (_isRelative)
					_startValue = calcAnchorCenterAsVector3(_target) + _endValue;
				else
					_startValue = _endValue;

				_endValue = calcAnchorCenterAsVector3(_target);
			} else {
				_startValue = calcAnchorCenterAsVector3(_target);
			}

			base.prepareForUse();
		}


		public override void tick(float totalElapsedTime) {
			var easedTime = _easeFunction(totalElapsedTime, 0, 1, _ownerTween.duration);
			var vec = GoTweenUtils.unclampedVector3Lerp(_startValue, _diffValue, easedTime);

			// calc new anchor positions based on anchor center
			float diffX = vec.x - (_target.anchorMin.x + _target.anchorMax.x) * 0.5f;
			float diffY = vec.y - (_target.anchorMin.y + _target.anchorMax.y) * 0.5f;
			Vector2 min = _target.anchorMin;
			min.x += diffX;
			min.y += diffY;
			Vector2 max = _target.anchorMax;
			max.x += diffX;
			max.y += diffY;

			// update anchors
			_target.anchorMin = min;
			_target.anchorMax = max;

		}

		protected Vector3 calcAnchorCenterAsVector3(RectTransform _target) {
			Vector3 center = new Vector3();
			center.x = (_target.anchorMin.x + _target.anchorMax.x) * 0.5f;
			center.y = (_target.anchorMin.y + _target.anchorMax.y) * 0.5f;
			return center;
		}

	}
}