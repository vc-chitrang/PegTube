using static Modules.Utility.Utility;
using UnityEngine;
using ViitorCloud.GameHelper.UI;
using ViitorCloud.GameHelper.Util;

namespace ViitorCloud.GameHelper.GameState {
	public abstract class GenericUnityMenuState : GameState {
		protected GenericUnityMenu prefab_;
		protected bool animateBack_ = false;
		protected bool hideOnExit_ = true;

		private string prefabName_;
		//private UIManager uiMgr_;
		private Transform uiRoot_;
		private GenericUnityMenu menuUi_;

		//public GenericUnityMenuState(string title, string prefabName, UIManager uiMgr, bool usesVision = false, bool hideOnExit = true) : base(title, usesVision) {
		//	prefabName_ = prefabName;
		//	uiMgr_ = uiMgr;
		//	hideOnExit_ = hideOnExit;
		//}

		public GenericUnityMenuState(string title, string prefabName, Transform uiRoot, bool usesVision = false, bool hideOnExit = true) : base(title, usesVision) {
			prefabName_ = prefabName;
			uiRoot_ = uiRoot;
			hideOnExit_ = hideOnExit;
		}

		abstract protected void HandleButton(string title);

		public GenericUnityMenu MenuUI { get { return menuUi_; } }

		virtual protected bool IsValid() {
			return prefab_ != null;
		}

		virtual protected void CleanUp() {
			// Log.WarningFormat("[GameState] {0} CleanUp()", name);
			GameObject.Destroy(prefab_.gameObject);
		}

		virtual protected GenericUnityMenu BuildMenu() {
			menuUi_ = (GameObject.Instantiate(ResourcesManager.Instance.Load(prefabName_) as GameObject, uiRoot_, false)).GetComponent<GenericUnityMenu>();
			if (menuUi_ == null) {
				LogError(string.Format("Could not menu prefab: {0}", prefabName_));
				return null;
			}

			menuUi_.Init();
			return menuUi_;
		}

		override public void OnPush() {
			base.OnPush();

			prefab_ = BuildMenu();
			prefab_.Reset();
			prefab_.OnOnscreen();
		}

		override public void OnEnter() {
			base.OnEnter();

			animateBack_ = false;

			prefab_.Show();
			prefab_.callback += HandleButton;
		}

		override public void OnExit() {
			base.OnExit();

			if (hideOnExit_) {
				prefab_.Hide(animateBack_);
			}
			
			prefab_.callback -= HandleButton;
		}

		override public void OnPop() {
			base.OnPop();

			if (!hideOnExit_) {
				prefab_.Hide(animateBack_);
			}

			prefab_.OnOffscreen(animateBack_);
			prefab_.OnAnimationsComplete(delegate {
				if (IsValid()) {
					CleanUp();
				}
			});
		}
	}	
}
