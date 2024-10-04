using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Blitter : MonoBehaviour {
	private RenderTexture m_TempRT;
	private Camera mainCamera;

	private RenderTexture target;

	private void Awake() {
		mainCamera = GetComponent<Camera>();
	}

	private void OnPreRender() {
		m_TempRT = RenderTexture.GetTemporary(Screen.width, Screen.height);
		mainCamera.targetTexture = m_TempRT;
	}

	private void OnPostRender() {
		Graphics.Blit(m_TempRT, target);
		mainCamera.targetTexture = null;

		RenderTexture.ReleaseTemporary(m_TempRT);
	}
}
