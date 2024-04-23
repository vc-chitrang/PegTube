using UnityEngine;
using System.Collections;

namespace ViitorCloud.GameHelper.GoKit {
	public class AnchoredPositionTweenProperty : AbstractVector3TweenProperty {
		protected new RectTransform _target;

		public AnchoredPositionTweenProperty(Vector2 endValue, bool isRelative = false)
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
					_startValue = _target.anchoredPosition3D + _endValue;
				else
					_startValue = _endValue;

				_endValue = _target.anchoredPosition3D;
			} else {
				_startValue = _target.anchoredPosition3D;
			}

			base.prepareForUse();
		}


		public override void tick(float totalElapsedTime) {
			var easedTime = _easeFunction(totalElapsedTime, 0, 1, _ownerTween.duration);
			var vec = GoTweenUtils.unclampedVector3Lerp(_startValue, _diffValue, easedTime);

			// update position
			_target.anchoredPosition3D = vec;
		}

	}
}
