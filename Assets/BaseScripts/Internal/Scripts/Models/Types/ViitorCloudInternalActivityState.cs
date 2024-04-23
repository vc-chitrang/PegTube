namespace ViitorCloud.Container.Internal {
	/// <summary>
	/// These states are set by the container and should never be used by games
	/// Reserved integers 1-999 for internal states (above will be used for game states)
	/// </summary>
	public enum ViitorCloudInternalActivityState {
		///<summary> default/invalid error state </summary>
		None = 0,

		///<summary> this state notifies the helper is initialized/loading is started </summary>
		Init = 1,

		///<summary> this state notifies assets are being preloaded </summary>
		Preload = 2,

		///<summary> this state notifies the setup flow is being shown </summary>
		Setup = 3,

		///<summary> this state notifies loading is started </summary>
		Load = 4,

		///<summary> this state notifies scene is ready and app transition screen is removed to show respective activity screen </summary>
		Start = 5,

		/// <summary> full screen profile screen is up </summary>
		Profiles = 6,

		///<summary> the container has regaining control. this state notifies Activity is finished and App Transitions screen is to be loaded for switch </summary>
		Finish = 999,

		/// <summary> the container has released control, game is in gameplay </summary>
		GamePlaying = 1000,

		/// <summary> the container has released control, game is on its main menu </summary>
		GameMainMenu= 1001,
	}
}
