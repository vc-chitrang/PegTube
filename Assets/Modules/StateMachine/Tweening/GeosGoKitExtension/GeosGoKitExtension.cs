using UnityEngine;
using System.Collections;

namespace ViitorCloud.GameHelper.GoKit {
	//Extension methods must be defined in a static class
	public static class GoKitExtension {
		/// <summary>
		/// Anchors center position tween
		/// </summary>
		public static GoTweenConfig anchorPositions(this GoTweenConfig goTweenConfig, Vector2 endValue, bool isRelative = false) {
			var prop = new AnchorPositionsTweenProperty(endValue, isRelative);
			goTweenConfig.tweenProperties.Add(prop);

			return goTweenConfig;
		}

		/// <summary>
		/// Anchored position tween
		/// </summary>
		public static GoTweenConfig anchoredPosition(this GoTweenConfig goTweenConfig, Vector2 endValue, bool isRelative = false) {
			var prop = new AnchoredPositionTweenProperty(endValue, isRelative);
			goTweenConfig.tweenProperties.Add(prop);

			return goTweenConfig;
		}
	}
}