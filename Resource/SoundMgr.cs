using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace KHFC {
	public class SoundMgr : Singleton<SoundMgr> {
		public const string EFX_ASSET_PREFIX = "efx_";

		/// <summary> Efx 사운드용 구조체 </summary>
		[System.Serializable]
		public class SoundSource {
			public string m_Name;
			public AudioSource m_Source;
			public Coroutine m_CoEnd;

			public bool m_IgnoreTimescale;	// 타임스케일과 무관하게 플레이

			public void Reset() {
				m_Name = string.Empty;
				m_Source = null;
				m_IgnoreTimescale = false;
				if (m_CoEnd != null)
					SoundMgr.inst.StopCoroutine(m_CoEnd);
				m_CoEnd = null;
			}
		}
		[SerializeField]
		bool m_MuteBgm;
		public bool muteBgm {
			get => m_MuteBgm;
			set {
				m_MuteEfx = value;
				if (m_CurBgm != null)
					m_CurBgm.mute = value;
			}
		}
		float m_MasterBgmVolume = 0.6f;
		public float bgmVolume {
			get => m_MasterBgmVolume;
			set {
				m_MasterBgmVolume = value;
				if (m_CurBgm != null)
					m_CurBgm.volume = m_MasterBgmVolume;
			}
		}
		[SerializeField]
		bool m_MuteEfx;
		public bool muteEfx {
			get => m_MuteEfx;
			set => m_MuteEfx = value;
		}
		float m_MasterEfxVolume = 1f;
		public float efxVolume {
			get => m_MasterEfxVolume;
			set {
				m_MasterEfxVolume = value;
				for (int i = 0; i < m_ListEfx.Count; i++) {
					m_ListEfx[i].m_Source.volume = m_MasterEfxVolume;
				}
			}
		}

		public AudioSource m_CurBgm;
		public List<SoundSource> m_ListEfx = new List<SoundSource>();

		private float m_FadeoutTime = 0.4f;
		private float m_EfxSpeed = 1f;

		Stack<SoundSource> m_PoolSource = new Stack<SoundSource>();


		public void Open() {

		}

		/// <summary>
		/// 모든 사운드 실행을 종료하고, 리스트화된 정보들을 지우는 함수
		/// </summary>
		public void Close() {
			StopAllCoroutines();

			int count = m_ListEfx.Count;
			for (int i = 0; i < count; i++) {
				StopEfx(m_ListEfx[i], false);
			}
			m_ListEfx.Clear();
		}

		/// <summary> EFX 오디오의 출력을 확인하는 함수 </summary>
		public bool CheckPlaying(string name) {
			if (m_ListEfx.TryGetValue(out SoundSource sound, item => item.m_Name == name))
				return sound.m_Source.isPlaying;
			return false;
		}

		/// <summary> AudioSource의 속도를 변경해주는 함수 </summary>
		public void ChangeEfxSoundSpeed(float speed) {
			m_EfxSpeed = speed;

			for (int i = 0; i < m_ListEfx.Count; i++) {
				SoundSource sound = m_ListEfx[i];
				if (sound == null || sound.m_Source == null)
					continue;

				if (!sound.m_IgnoreTimescale)
					sound.m_Source.pitch = m_EfxSpeed;
			}
		}


		public void PlayEfx(string name, bool ignoreScale = false, float delay = 0) {
			PlayEfx(name, delay, -1f, Vector3.zero, null, 0f, ignoreScale);
		}
		public void PlayEfx(string name, Transform src, bool ignoreScale = false, float delay = 0) {
			PlayEfx(name, delay, -1f, Vector3.zero, src, 0f, ignoreScale);
		}
		public void PlayEfx(string name, Vector3 pos, bool ignoreScale = false, float delay = 0) {
			PlayEfx(name, delay, -1f, pos, null, 0f, ignoreScale);
		}
		/// <param name="volume"> 계산된 오디오 볼륨은 마스터 볼륨에 영향을 받는다. </param>
		public void PlayEfx(string name, float volume, bool ignoreScale = false, float delay = 0) {
			PlayEfx(name, delay, volume, Vector3.zero, null, 0f, ignoreScale);
		}
		/// <param name="volume"> 계산된 오디오 볼륨은 마스터 볼륨에 영향을 받는다. </param>
		public void PlayEfx(string name, float volume, float startTime, bool ignoreScale = false, float delay = 0) {
			PlayEfx(name, delay, volume, Vector3.zero, null, startTime, ignoreScale);
		}

		/// <summary> AudioSource를 정지 하고 싶을 때 사용하는 함수 </summary>
		public void StopEfx(string name, bool unloadObject = false) {
			SoundSource sound = m_ListEfx.Find(item => item.m_Name == name);
			if (sound == null)
				return;

			StopEfx(sound, unloadObject);
		}

		public void UnloadEfx(string name) {
			if (m_ListEfx.TryGetValue(out SoundSource sound, item => item.m_Name == name))
				StopEfx(sound, true);
			else
				PoolMgr.inst.UnLoadObject(name);
		}

		public void CleanEfx() {
			for (int i = 0; i < m_ListEfx.Count; i++)
				CleanAudio(m_ListEfx[i].m_Source, true);

			//PoolMgr.inst.UnLoadPattern(EFX_ASSET_PREFIX);

			m_ListEfx.Clear();
			m_PoolSource.Clear();
		}


		public void PreSpawnEfx(string name) {
			if (m_MuteEfx || string.IsNullOrEmpty(name))
				return;
			PrespawnAudio(name);
		}
		public void PreSpawnEfxAsync(string name, System.Action<GameObject> actOnAfter) {
			if (m_MuteEfx || string.IsNullOrEmpty(name)) {
				actOnAfter?.Invoke(null);
				return;
			}
			PrespawnAudioAsync(name, actOnAfter);
		}


		/// <summary> BGM을 플레이 하고 싶을때 사용하는 함수 </summary>
		/// <param name="unloadPrev"> 이미 BGM이 있었다면, 해당 사운드를 Despawn이 아닌 바로 언로드 </param>
		public void PlayBgm(string name, float delay = 0f, bool unloadPrev = true) {
			if (m_MuteBgm) {
				if (m_CurBgm != null)
					CleanAudio(m_CurBgm, true);
				return;
			}
			if (CheckExistBGM(name))
				return;

			StartCoroutine(CoPlayBgmSound(name, delay, unloadPrev));
		}

		public void StopBgm(bool unload = false) {
			if (m_CurBgm == null)
				return;

			m_CurBgm.Stop();
			CleanAudio(m_CurBgm, unload);

			m_CurBgm = null;
		}

		public void PreSpawnBgm(string name) {
			if (m_MuteBgm || CheckExistBGM(name))
				return;
			PrespawnAudio(name);
		}
		public void PrespawnBgmAsync(string name, System.Action<GameObject> actOnAfter) {
			if (m_MuteBgm || CheckExistBGM(name)) {
				actOnAfter?.Invoke(null);
				return;
			}
			PrespawnAudioAsync(name, actOnAfter, false);
		}

		public void PrespawnAudio(string name, bool isEfx = true) {
			GameObject obj = SpawnSound(name, Vector3.zero);
			AfterSpawn(obj, isEfx);
		}
		public void PrespawnAudioAsync(string name, System.Action<GameObject> actOnAfter, bool isEfx = true) {
			SpawnSoundAsync(name, (obj) => {
				if (obj == null)
					Debug.LogWarning("Can't LoadSound : " + name);
				else
					AfterSpawn(obj, isEfx);

				actOnAfter?.Invoke(obj);
			});
		}

		/// <summary> 스폰할 필요가 있으면 true, 없으면 false </summary>
		bool CheckExistBGM(string name) {
			return m_CurBgm != null && m_CurBgm.name.Equals(name);
		}

		GameObject SpawnSound(string name, Vector3 pos = default, Transform parent = null) {
			return PoolMgr.inst.SpawnGameObject(name, pos, parent, AssetType.Audio);
		}
		void SpawnSoundAsync(string name, System.Action<GameObject> actOnAfter, Vector3 pos = default, Transform parent = null) {
			PoolMgr.inst.SpawnGameObjectAsync(name, actOnAfter, pos, parent, AssetType.Audio);
		}

		void AfterSpawn(GameObject obj, bool isEfx = true) {
			if (isEfx) {
				SoundSource sound = GetSoundSource();
				sound.m_Source = obj.GetComponent<AudioSource>();
				sound.m_Name = name;

				m_ListEfx.Add(sound);
			}
			PoolMgr.inst.DespawnObject(obj);
		}

		void PlayEfx(string name, float delay, float volume, Vector3 pos, Transform parent, float startTime, bool ignoreScale) {
			if (m_MuteEfx || string.IsNullOrEmpty(name))
				return;
			StartCoroutine(CoPlayEfxSound(name, delay, volume, pos, parent, startTime, ignoreScale));
		}
		void StopEfx(SoundSource targetSource, bool unloadObject = false) {
			if (targetSource == null)
				return;

			m_ListEfx.Remove(targetSource);
			CleanAudio(targetSource.m_Source, unloadObject);

			targetSource.Reset();
			m_PoolSource.Push(targetSource);
		}

		/// <summary> 사운드 게임 오브젝트를 생성하고 출력하는 함수 </summary>
		/// <param name="pos"> <see cref="Transform.localPosition"/> 에 값이 들어간다 </param>
		/// <param name="parent"> 3D 사운드를 사용할 때 음원 대상으로 할 오브젝트 </param>
		/// <param name="ignoreScale"> true 이면 사운드 출력이 <see cref="Time.timeScale"/>에 영향을 받지 않는다. </param>
		/// <returns></returns>
		IEnumerator CoPlayEfxSound(string name, float delay, float volume = -1f, Vector3 pos = default, Transform parent = null, float startTime = 0f, bool ignoreScale = false) {

			GameObject sounObj = SpawnSound(name, pos, parent);
			if (sounObj == null) {
				Debug.LogWarning("Can't LoadSound : " + name);
				yield break;
			}
			AudioSource newSource = sounObj.GetComponent<AudioSource>();

			SoundSource sound = GetSoundSource();
			sound.m_Source = newSource;
			sound.m_IgnoreTimescale = ignoreScale;
			sound.m_Name = name;
			sound.m_Source.mute = m_MuteEfx;

			m_ListEfx.Add(sound);

			newSource.volume = volume < 0f ? m_MasterEfxVolume : m_MasterEfxVolume * volume;
			newSource.pitch = ignoreScale ? 1 : m_EfxSpeed;
			newSource.time = startTime;
			newSource.PlayDelayed(delay);

			if (delay > 0) {
				yield return CachedYield.GetWFS(delay);
			}

			sound.m_CoEnd = StartCoroutine(CoWaitForEnd(sound));
		}

		/// <summary> 사운드가 종료될때까지 기다리는 함수 </summary>
		IEnumerator CoWaitForEnd(SoundSource sound) {
			if (sound == null || sound.m_Source == null)
				yield break;

			float duration = sound.m_Source.clip.length;
			if (sound.m_IgnoreTimescale)
				duration *= Time.timeScale;

			yield return CachedYield.GetWFS(duration);

			StopEfx(sound);
		}

		IEnumerator CoPlayBgmSound(string name, float delay, bool unloadPrev) {
			if (string.IsNullOrEmpty(name))
				yield break;

			GameObject soundObj = SpawnSound(name);
			if (soundObj == null) {
				Debug.LogWarning("Can't Load Sound : " + name);
				yield break;
			}

			if (m_CurBgm != null)
				yield return StartCoroutine(CoFadeOutBgm(unloadPrev));

			AudioSource newSource = soundObj.GetComponent<AudioSource>();
			m_CurBgm = newSource;
			m_CurBgm.volume = m_MasterBgmVolume;
			m_CurBgm.loop = true;
			m_CurBgm.mute = m_MuteBgm;

			m_CurBgm.PlayDelayed(delay);
		}

		IEnumerator CoFadeOutBgm(bool unloadPrev) {
			if (m_CurBgm == null)
				yield break;

			float curVolume = m_CurBgm.volume;

			float t = 0f;
			while (t < m_FadeoutTime) {
				t += Time.deltaTime / m_FadeoutTime;
				if (m_CurBgm == null)
					yield break;

				m_CurBgm.volume = Mathf.Lerp(curVolume, 0, t);
				yield return null;
			}

			StopBgm(unloadPrev);
		}

		void CleanAudio(AudioSource audioSource, bool unloadObject = false) {
			if (audioSource == null)
				return;

			if (unloadObject)
				PoolMgr.inst.UnLoadObject(audioSource.name);
			else
				PoolMgr.inst.DespawnObject(audioSource.gameObject);
		}

		SoundSource GetSoundSource() {
			if (m_PoolSource.Count <= 0)
				return new SoundSource();
			else
				return m_PoolSource.Pop();
		}
	}
}
