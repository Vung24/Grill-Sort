#if DOTWEEN
using DG.Tweening;
#elif PRIME_TWEEN
using PrimeTween;
#endif
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

public static class Utils
{
	private static readonly int UILayer = LayerMask.NameToLayer("UI");

	public static bool IsPointerOverUIElement()
	{
		var raycastResults = ListPool<RaycastResult>.Get();
		try
		{
			GetEventSystemRaycastResults(raycastResults);
			return raycastResults.Exists(result => result.gameObject.layer == UILayer);
		}
		finally
		{
			ListPool<RaycastResult>.Release(raycastResults);
		}
	}

	// Gets all event system raycast results of current mouse or touch position.
	private static void GetEventSystemRaycastResults(List<RaycastResult> raycastResults)
	{
		var eventData = new PointerEventData(EventSystem.current)
		{
			position = Input.mousePosition
		};
		EventSystem.current.RaycastAll(eventData, raycastResults);
	}


	public static string GetNumberAroundString(this int input)
	{
		if (input < 5000)
		{
			return input.ToString();
		}
		else if (input < 1_000_000)
		{
			return input / 1000 + "K";
		}
		else if (input < 1_000_000_000)
		{
			return input / 1_000_000 + "M";
		}
		/*
		else if (input < 1_000_000_000_000)
		{
		    return input / 1_000_000_000 + "B";
		}*/

		return input.ToString();
	}
	public static string GetNumberAroundString(this long input)
	{
		if (input < 5000)
		{
			return input.ToString();
		}
		else if (input < 1_000_000)
		{
			return input / 1000 + "K";
		}
		else if (input < 1_000_000_000)
		{
			return input / 1_000_000 + "M";
		}
		else if (input < 1_000_000_000_000)
		{
			return input / 1_000_000_000 + "B";
		}

		return input.ToString();
	}
	public static int GetNumberAround(this int input)
	{
		if (input < 5000)
		{
			return input;
		}
		else if (input < 1_000_000)
		{
			return (input / 1000) * 1000;
		}
		else if (input < int.MaxValue)
		{
			return (input / 1_000_000) * 1_000_000;
		}
		return input;
	}
	public static long GetNumberAround(this long input)
	{
		if (input < 5000)
		{
			return input;
		}
		else if (input < 1_000_000)
		{
			return (input / 1000) * 1000;
		}
		else if (input < 1_000_000_000)
		{
			return (input / 1_000_000) * 1_000_000;
		}
		else if (input < 1_000_000_000_000)
		{
			return (input / 1_000_000_000) * 1_000_000_000;
		}

		return input;
	}

	public static string GetNumberFormat(this int input)
	{
		string formatted = string.Format(CultureInfo.InvariantCulture, "{0:N0}", input);
		return formatted;
	}

	public static void AnimateMoneyChange(Text moneyText, int preMoney, int curMoney)
	{
		//AudioManager.Instance.PlayCashRegister();
#if DOTWEEN
		DOTween.To(() => preMoney, x => preMoney = x, curMoney, 2f)
			.OnUpdate(() => { moneyText.text = preMoney.ToString(); })
			.OnComplete(() => { moneyText.text = curMoney.ToString(); });
#elif PRIME_TWEEN
		Tween.Custom(preMoney, curMoney, 2f, value =>
		{
			preMoney = Mathf.RoundToInt(value);
			moneyText.text = preMoney.ToString();
		})
		.OnComplete(() =>
		{
			moneyText.text = curMoney.ToString();
		});

#endif

	}

	public static void SetNativeImage(this Image image, Vector2 parentSize, float scale = 1f)
	{
		//var parentSize = image.GetComponent<RectTransform>().sizeDelta;
		var imgSize = new Vector2(image.sprite.texture.width, image.sprite.texture.height);

		var parentRatio = parentSize.x / parentSize.y;
		var imgRatio = imgSize.x / imgSize.y;

		if (parentRatio > imgRatio)
		{
			var yMax = parentSize.y;
			var x = (parentSize.y / imgSize.y) * imgSize.x;
			image.rectTransform.sizeDelta = new Vector2(x, yMax) * scale;
		}
		else
		{
			var y = (parentSize.x / imgSize.x) * imgSize.y;
			var xMax = parentSize.x;
			image.rectTransform.sizeDelta = new Vector2(xMax, y) * scale;
		}
	}
	
	public static Vector3 RandomAround(this Vector3 center, float range = 2.5f)
	{
		float randX = UnityEngine.Random.Range(-range, range);
		float randZ = UnityEngine.Random.Range(-range, range);
		return new Vector3(center.x + randX, center.y, center.z + randZ);
	}

	public static Vector3 PosDistanceArray(this Transform[] listTransform, Transform target, bool isMax = true)
	{
		if (listTransform == null || listTransform.Length == 0 || target == null)
			return Vector3.zero;

		float bestDistance = isMax ? float.MinValue : float.MaxValue;
		Vector3 result = Vector3.zero;

		foreach (var t in listTransform)
		{
			if (t == null || t == target) continue;

			float dist = Vector3.SqrMagnitude(t.position - target.position);

			if ((isMax && dist > bestDistance) || (!isMax && dist < bestDistance))
			{
				bestDistance = dist;
				result = t.position;
			}
		}

		return result;
	}

    #if UNITY_EDITOR

	public static GameObject FindInChildren(GameObject parent, string childName)
	{
		foreach (Transform child in parent.transform)
		{
			if (child.name == childName)
			{
				return child.gameObject;
			}
			GameObject result = FindInChildren(child.gameObject, childName);
			if (result != null)
			{
				return result;
			}
		}
		return null;
	}

	public static void SetDirty<T>(this T target) where T : UnityEngine.Object
	{
		UnityEditor.EditorUtility.SetDirty(target);
	}


	public static void SO_SetDirty<T>(this T target) where T : ScriptableObject
	{
		UnityEditor.EditorUtility.SetDirty(target);
	}

    #endif
}


public static class Vibration
{
#if UNITY_ANDROID && !UNITY_EDITOR
    public static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    public static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    public static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#else
	public static AndroidJavaClass unityPlayer;
	public static AndroidJavaObject currentActivity;
	public static AndroidJavaObject vibrator;
#endif

	public static void Vibrate()
	{
		// if (UIController.instace.vibrate == 0)
		// {
		//     return;
		// }

		if (isAndroid())
			vibrator.Call("vibrate");
		else
			Handheld.Vibrate();
	}


	public static void Vibrate(long milliseconds)
	{
		// if (UIController.instace.vibrate == 0)
		// {
		//     return;
		// }

		if (isAndroid())
			vibrator.Call("vibrate", milliseconds);
		else
			Handheld.Vibrate();
	}

	public static bool HasVibrator()
	{
		return isAndroid();
	}

	public static void Cancel()
	{
		if (isAndroid())
			vibrator.Call("cancel");
	}

	private static bool isAndroid()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
	return true;
#else
		return false;
#endif
	}
}
