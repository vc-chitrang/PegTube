﻿using UnityEngine;

namespace ViitorCloud.GameHelper.GoKit {
	public static class GoEaseLinear {
		public static float EaseNone(float t, float b, float c, float d) {
			return c * t / d + b;
		}
	}
}