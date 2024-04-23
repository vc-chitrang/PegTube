using UnityEngine;
using System;
using System.Collections;

namespace ViitorCloud.GameHelper.GoKit {
	public class AnimationCurveTweenProperty : FloatTweenProperty, IGenericProperty {
		public AnimationCurve Curve { get; private set; }
		public new string propertyName { get; private set; }
		private Action<float> _setter;

		public AnimationCurveTweenProperty(string propertyName, float endValue, AnimationCurve curve, bool isRelative = false) : base(propertyName, endValue, isRelative) {
			this.propertyName = propertyName;
			if (curve != null) {
				Curve = curve;
			} else {
				throw (new ArgumentNullException("curve"));
			}
		}

		/// <summary>
		/// validation checks to make sure the target has a valid property with an accessible setter
		/// </summary>
		public override bool validateTarget(object target) {
			// cache the setter
			_setter = GoTweenUtils.setterForProperty<Action<float>>(target, propertyName);
			return _setter != null;
		}

		public override void prepareForUse() {
			// retrieve the getter
			var getter = GoTweenUtils.getterForProperty<Func<float>>(_ownerTween.target, propertyName);

			_endValue = _originalEndValue;

			// if this is a from tween we need to swap the start and end values
			if (_ownerTween.isFrom) {
				_startValue = _endValue;
				_endValue = getter();
			} else {
				_startValue = getter();
			}

			// setup the diff value
			if (_isRelative && !_ownerTween.isFrom)
				_diffValue = _endValue;
			else
				_diffValue = _endValue - _startValue;
		}

		public override void tick(float totalElapsedTime) {
			var easedValue = _startValue + _diffValue * Curve.Evaluate(totalElapsedTime / _ownerTween.duration);
			_setter(easedValue);
		}
	}
}