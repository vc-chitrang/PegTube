using System;
using System.Collections.Generic;

using ViitorCloud.GameHelper.GoKit;

using UnityEngine;
using UnityEngine.UI;

namespace ViitorCloud.GameHelper.UI {
	public class GenericUnityMenu : MonoBehaviour {
		protected AbstractGoTween screenTween_ = null;
		protected AbstractGoTween showTween_ = null;

		public virtual bool Animating {
			get {
				if (screenTween_ != null && screenTween_.state == GoTweenState.Running) {
					return true;
				}

				if (showTween_ != null && showTween_.state == GoTweenState.Running) {
					return true;
				}

				return false;
			}
		}

		// =================================================== override these

		public virtual AbstractGoTween OnscreenTween() {
			return null;
		}

		public virtual AbstractGoTween ShowTween() {
			gameObject.SetActive(true);
			return null;
		}

		public virtual AbstractGoTween HideTween(bool back = false) {
			gameObject.SetActive(false);
			return null;
		}

		public virtual AbstractGoTween OffscreenTween(bool back = false) {
			return null;
		}

		// ==================================================

		// helper function
		virtual public void HandleButtonGeneric(GameObject button) {
//			UIManager.HandleButtonClick(button);
		}

		public Action<string> callback { get; set; }

		public virtual void HandleMenuButtonTitled(string title) {
			if (callback != null) {
				callback(title);
			}
		}

		private HashSet<GameObject> processed_ = new HashSet<GameObject>();

		virtual protected void Start() {
			GameObject processing;
			for (int i = 0; i < transform.childCount; i++) {
				processing = transform.GetChild(i).gameObject;
				foreach (Button button in processing.GetComponentsInChildren<Button>(true)) {
					button.onClick.AddListener(delegate {
						HandleButtonGeneric(button.gameObject);
					});
				}
				processed_.Add(processing);
			}
		}

		virtual protected void OnTransformChildrenChanged() {
			GameObject processing;
			for (int i = 0; i < transform.childCount; i++) {
				processing = transform.GetChild(i).gameObject;
				if (!processed_.Contains(processing)) {
					foreach (Button button in processing.GetComponentsInChildren<Button>(true)) {
						button.onClick.AddListener(delegate {
							HandleButtonGeneric(button.gameObject);
						});
					}
					processed_.Add(processing);
				}
			}
		}

		public virtual void Init() {
			// stub
		}

		virtual public AbstractGoTween OnOnscreen() {
			screenTween_ = OnscreenTween();
			return screenTween_;
		}

		virtual public void Reset() {
			Transition t = gameObject.GetComponent<Transition>();

			if (t != null) {
				t.CancelTransition();
				t.ResetTransition();
			}
		}

		virtual public AbstractGoTween Show() {
			Transition t = gameObject.GetComponent<Transition>();

			if (t != null) {
				t.CancelTransition();

				showTween_ = t.TransitionIn();
			} else {
				if (showTween_ != null) {
					showTween_.destroy();
				}

				showTween_ = ShowTween();
			}

			return showTween_;
		}

		virtual public AbstractGoTween Hide(bool back = false) {
			Transition t = gameObject.GetComponent<Transition>();

			if (t != null) {
				t.CancelTransition();

				if (back && t is BackTransition) {
					showTween_ = (t as BackTransition).TransitionBack();
				} else {
					showTween_ = t.TransitionOut();
				}
			} else {
				if (showTween_ != null) {
					showTween_.destroy();
				}

				showTween_ = HideTween(back);
			}

			return showTween_;
		}

		virtual public AbstractGoTween OnOffscreen(bool back = false) {
			if (screenTween_ != null) {
				screenTween_.destroy();
			}

			screenTween_ = OffscreenTween(back);
			return screenTween_;
		}

		public void OnAnimationsComplete(Action onComplete) {
			if (onComplete != null) {
				Transition t = gameObject.GetComponent<Transition>();
				if (t != null && t.CurrentTransition() != null && t.CurrentTransition().isValid() && t.CurrentTransition().state == GoTweenState.Running) {
					t.CurrentTransition().setOnCompleteHandler(agt => {
						onComplete();
					});
				} else {
					if (screenTween_ != null && screenTween_.isValid() && screenTween_.state == GoTweenState.Running) {
						screenTween_.setOnCompleteHandler(agt => {
							onComplete();
						});
					} else if (showTween_ != null && showTween_.isValid() && showTween_.state == GoTweenState.Running) {
						showTween_.setOnCompleteHandler(agt => {
							onComplete();
						});
					} else {
						onComplete();
					}
				}
			}
		}
	}
}
