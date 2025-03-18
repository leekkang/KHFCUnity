
using System;
using Unity.Services.Core;
using Unity.Services.Core.Environments;


public class InitializeUnityServices : UnityEngine.MonoBehaviour {
	const string ENV_NAME = "production";

	void Awake() {
		// Uncomment this line to initialize Unity Gaming Services.
		Initialize(OnSuccess, OnError);
	}

	void Initialize(Action onSuccess, Action<string> onError) {
		try {
			InitializationOptions options = new InitializationOptions().SetEnvironmentName(ENV_NAME);
			UnityServices.InitializeAsync(options).ContinueWith(task => onSuccess());
		} catch (System.Exception exception) {
			onError(exception.Message);
		}
	}

	void OnSuccess() {
		KHFC.Util.Log("Congratulations!\nUnity Gaming Services has been successfully initialized.");
	}

	void OnError(string msg) {
		KHFC.Util.LogError($"Unity Gaming Services failed to initialize with error: {msg}.");
	}

	void Start() {
		if (UnityServices.State == ServicesInitializationState.Uninitialized) {
			KHFC.Util.LogError(
				"Error: Unity Gaming Services not initialized.\n" +
				"To initialize Unity Gaming Services, open the file \"InitializeGamingServices.cs\" " +
				"and uncomment the line \"Initialize(OnSuccess, OnError);\" in the \"Awake\" method."
				);
		}
	}
}