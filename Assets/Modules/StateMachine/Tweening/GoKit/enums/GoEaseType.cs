using UnityEngine;
using System.Collections;


namespace ViitorCloud.GameHelper.GoKit {
	public enum GoEaseType {
		Linear,

		SineIn,
		SineOut,
		SineInOut,

		QuadIn,
		QuadOut,
		QuadInOut,

		CubicIn,
		CubicOut,
		CubicInOut,

		QuartIn,
		QuartOut,
		QuartInOut,

		QuintIn,
		QuintOut,
		QuintInOut,

		ExpoIn,
		ExpoOut,
		ExpoInOut,

		CircIn,
		CircOut,
		CircInOut,

		ElasticIn,
		ElasticOut,
		ElasticInOut,
		Punch,

		BackIn,
		BackOut,
		BackInOut,

		BounceIn,
		BounceOut,
		BounceInOut,

		// new types we added
		QuadOutIn,
		CubicOutIn
	}
}