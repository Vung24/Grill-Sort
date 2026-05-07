#if DOTWEEN
using DG.Tweening;
#elif PRIME_TWEEN
using PrimeTween;
#endif
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class F_Init : MonoBehaviour
{
	public GameObject loadingCanvas;
	[SerializeField] Image process;

	private bool LoadComplete;

	private void Awake()
	{
		Application.targetFrameRate = 60;
		DontDestroyOnLoad(gameObject);
		
#if UNITY_ANDROID && !UNITY_EDITOR
		if (!Debug.isDebugBuild)
		{
			DebugLogger.isDebugLog = false;
		}
#endif
	}

	private IEnumerator Start()
	{
		StartCoroutine(LoadSceneAsync(1));
		loadingCanvas.SetActive(true);

		float timeWaitAOA = 3;
#if UNITY_EDITOR
		timeWaitAOA = 3;
#if DOTWEEN
		process.DOFillAmount(.9f, timeWaitAOA).From(0);
#elif PRIME_TWEEN
		Tween.UIFillAmount(process, 0, .9f, timeWaitAOA);
#endif
		
#elif !UNITY_EDITOR
        process.DOFillAmount(.9f, timeWaitAOA).From(0);
#endif
		yield return Yielder.Get(timeWaitAOA);
#if DOTWEEN
		process.DOKill();
		process.DOFillAmount(1, .9f).SetEase(Ease.Linear);
#elif PRIME_TWEEN
		Tween.StopAll(process);
		Tween.UIFillAmount(process, 1, .9f, Ease.Linear);
#endif
		yield return Yielder.Get(.75f);
		LoadComplete = true;
		yield return Yielder.Get(.25f);
		loadingCanvas.SetActive(false);
	}

	IEnumerator LoadSceneAsync(int scene)
	{
		AsyncOperation async = SceneManager.LoadSceneAsync(scene);
		async.allowSceneActivation = false;
		yield return Yielder.Get(1f);
		
		while (async.progress < 0.9f || !LoadComplete)
		{
			yield return null;
		}
		
		async.allowSceneActivation = true;
	}


}
