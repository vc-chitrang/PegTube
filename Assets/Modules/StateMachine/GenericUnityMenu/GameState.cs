
namespace ViitorCloud.GameHelper.GameState {
	public class GameState {
		private bool usesVisionInput_;
		public bool useVisionInput { get { return usesVisionInput_; } }

		// states have a string name for ease of debugging
		private string name_;
		public string name { get { return name_; } }

		public GameState(string name, bool usesVision = false) {
			name_ = name;
			usesVisionInput_ = usesVision;
		}

		// ===========================

		// you should generally override the following

		/// <summary>
		/// Called after this state has been added to the stack
		/// </summary>
		virtual public void OnPush() {
	#if LOG_STATE_CHANGES
			Log ("OnPush: " + name);
	#endif
		}

		/// <summary>
		/// Called directly after OnEnter
		/// </summary>
		virtual public void OnLateEnter() {
	#if LOG_STATE_CHANGES
			Log ("OnLateEnter: " + name);
	#endif
		}

		/// <summary>
		/// Called whenever this state is revealed at the top of the stack (after OnPush)
		/// </summary>
		virtual public void OnEnter() {
	#if LOG_STATE_CHANGES
			Log ("OnEnter: " + name);
	#endif
		}

		/// <summary>
		/// Called before OnPop when state is being removed. Called before new state is being pushed.
		/// </summary>
		virtual public void OnExit() {
	#if LOG_STATE_CHANGES
			Log ("OnExit: " + name);
	#endif
		}

		/// <summary>
		/// Called before state is being popped off the stack
		/// </summary>
		virtual public void OnPop() {
	#if LOG_STATE_CHANGES
			Log ("OnPop: " + name);
	#endif
		}

		/// <summary>
		/// Called after OnPop
		/// </summary>
		virtual public void OnLatePop() {
	#if LOG_STATE_CHANGES
			Log ("OnLatePop: " + name);
	#endif
		}

		/// <summary>
		/// Called after state is completely off of the stack, before the next state's OnEnter has been called
		/// </summary>
		virtual public void OnPopComplete() {
#if LOG_STATE_CHANGES
			Log ("OnPopComplete: " + name);
#endif
		}

		virtual public void Update(float delta) {
			// ... stub
		}

		virtual public void DebugSolve() {
			// ... stub
		}

		//if considerRaycast is set to true, clicks will not register if another object takes it
		virtual public void OnScreenClick(bool considerRaycast) {
			// ... stub
		}
	}
}
