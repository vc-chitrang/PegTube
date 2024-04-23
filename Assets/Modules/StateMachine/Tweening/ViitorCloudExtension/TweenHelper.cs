using UnityEngine;
using System;

namespace ViitorCloud.GameHelper.GoKit {
	public class TweenHelper {
		public static void AlphaMaterial(Material material, float alpha) {
			material.color = FadedColor(material.color, alpha);
		}

		public static Color FadedColor(Color color, float alpha = 0) {
			return new Color(color.r, color.g, color.b, alpha);
		}

		public static GoTween FadeOut(Renderer render, float time, GoEaseType type = GoEaseType.Linear, float delay = 0.0f) {
			return Go.to(render, time, new GoTweenConfig().materialColor(FadedColor(render.material.color, 0.0f)).setEaseType(type).setDelay(delay));
		}

		public static GoTween FadeIn(Renderer render, float time, GoEaseType type = GoEaseType.Linear, float delay = 0.0f) {
			return Go.to(render, time, new GoTweenConfig().materialColor(FadedColor(render.material.color, 1.0f)).setEaseType(type).setDelay(delay));
		}

		public static GoTween RunAction(object target, float delay, Action<AbstractGoTween> action) {
			return Go.to(target, 0.01f, new GoTweenConfig().onComplete(action).setDelay(delay));
		}

		public static void DelayedCall(object obj, Action function, float delay = 0f) {
			if (delay <= 0) {
				delay = 0.001f;
			}

			Go.to(obj, delay, new GoTweenConfig().onComplete(tween => {
				function();
			}));
		}

		public static GoTween FunctionTween(object obj, Action function, float delay = 0f) {
			if (delay <= 0) {
				delay = 0.001f;
			}

			return new GoTween(obj, delay, new GoTweenConfig().onComplete(tween => {
				function();
			}));
		}
	}
}


