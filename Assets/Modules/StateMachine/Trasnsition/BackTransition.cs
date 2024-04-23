using ViitorCloud.GameHelper.GoKit;

namespace ViitorCloud.GameHelper.UI {
	internal interface BackTransition {
		void SetBack();

		AbstractGoTween TransitionBack();
	}
}