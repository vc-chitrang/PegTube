using System;
using System.Collections;
using UnityEngine;

namespace ViitorCloud.GameHelper.GoKit.GoTweenCoroutines {
	public static class EasingCoroutines {
		public static IEnumerator DoEaseFor(float duration, GoEaseType easeType, Action<float> lerpCallback) {
			// are you serious GoTween...
			Func<float, float, float, float, float> easeFunction = GoTweenUtils.easeFunctionForType(easeType);

			for (float time = 0.0f; time <= duration; time += Time.deltaTime) {
				float percentage = easeFunction.Invoke(time, 0, 1, duration);
				lerpCallback.Invoke(percentage);
				yield return null;
			}

			lerpCallback.Invoke(1.0f);
		}
	}
}