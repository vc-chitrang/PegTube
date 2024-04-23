using ViitorCloud;
//#define LOG_TRANSITION
using ViitorCloud.GameHelper.GoKit;

using UnityEngine;

namespace ViitorCloud.GameHelper.UI {
	/** 
	* The basic transition only vanishes an object, 
	* it's meant to be subbed by actually useful classes.
	*/
	public class Transition : MonoBehaviour {
		[SerializeField] protected bool startOnScreen = false;

		public void SetStartOnScreen(bool toVal) {
			startOnScreen = toVal;
		}

		private bool initialized = false;

		protected virtual void Awake() {
			SafeInit();
		}

		// USE SAFE INIT if you might be pre-empting awake
		// backstory: sometimes you might instantiate a transition
		// and awake get's called after you've called functions on the transition
		// this isn't good, safe init will prevent two inits! -Henry
		public void SafeInit() {
			if (!initialized) {
				Init();
			}
		}

		virtual public void Init() {
			ResetTransition();
			initialized = true;
		}

		virtual public void ResetTransition() {
			if (startOnScreen) {
				SetIn();
			} else {
				SetOut();
			}
		}

		virtual public void Reparent() {
			// stub
		}

		virtual public AbstractGoTween CurrentTransition() {
			return null;
		}

		virtual public void SetIn() {
#if LOG_TRANSITION
			Log("set in: " + name);
#endif
			if (gameObject != null) {
				gameObject.SetActive(true);
			}
		}

		virtual public void SetOut() {
	#if LOG_TRANSITION
			Log("set out: " + name);
	#endif

			// sometimes the parent might be already destroyed!
			if (gameObject != null) {
				gameObject.SetActive(false);
			}
		}


		virtual public AbstractGoTween TransitionIn() {
	#if LOG_TRANSITION
			Log("transition in: " + name);
	#endif

			if (gameObject != null) {
				gameObject.SetActive(true);
			}
			return null;
		}

		virtual public AbstractGoTween TransitionOut() {
	#if LOG_TRANSITION
			Log("transition out: " + name);
	#endif

			if (gameObject != null) {
				gameObject.SetActive(false);
			}
			return null;
		}

		virtual public void CancelTransition() {
			// does nothing in the default case...
			AbstractGoTween current = CurrentTransition();
			if (current != null) {
				current.destroy();
			}
		}

		// not all transitions will have rush functions!
		// they exist for places where players might get impatient!
		// for those that do, override these functions:
		virtual public AbstractGoTween RushIn() {
			return TransitionIn();
		}

		virtual public AbstractGoTween RushOut() {
			return TransitionOut();
		}
	}
}
