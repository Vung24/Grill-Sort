#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

public partial class SYMBOL_CHECKER 
{
	private const string DOTWEEN_SYMBOL = "DOTWEEN";
	private const string PRIMETWEEN_SYMBOL = "PRIME_TWEEN";
	private const string LITMOTION_SYMBOL = "LITMOTION";

	private const string ADDRESSABLE_SYMBOL = "ADDRESSABLE_AVAILABLE";


	public const string LAST_LIB_KEY = "dev_sysmob_checker";
	public const string SYMBOL_PRIORITY_KEY = "TweenSymbol_Priority";

	public static List<string> tweenSymbols = new ()
	{
		"DOTWEEN",
		"PRIME_TWEEN",
		"LITMOTION"
	};
	

	public static void SavePriority(List<string> list)
	{
		EditorPrefs.SetString(SYMBOL_PRIORITY_KEY, string.Join("|", list));
	}

	public static List<string> LoadPriority(List<string> defaultList)
	{
		if (!EditorPrefs.HasKey(SYMBOL_PRIORITY_KEY))
			return new List<string>(defaultList);

		return EditorPrefs.GetString(SYMBOL_PRIORITY_KEY)
			.Split('|')
			.Where(s => !string.IsNullOrEmpty(s))
			.ToList();
	}
	
	public static partial void UTILS_CHECK_SYMBOL()
	{
		TWEEN_SYMBOL_CHECK();
		AA_SYMBOL_CHECK();
	}

	public static void TWEEN_SYMBOL_CHECK()
	{
		tweenSymbols = LoadPriority(tweenSymbols);
		var buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
		var currentSymbols = PlayerSettings
			.GetScriptingDefineSymbolsForGroup(buildTarget)
			.Split(';')
			.Where(s => !string.IsNullOrEmpty(s))
			.ToList();


		foreach (var s in tweenSymbols) currentSymbols.Remove(s);

		string detectedSymbol = null;

		foreach (var symbol in tweenSymbols)
		{
			if (IsLibraryDetected(symbol))
			{
				detectedSymbol = symbol;
				break;
			}
		}

		string lastLib = EditorPrefs.GetString(LAST_LIB_KEY, "None");

		if (detectedSymbol == lastLib)
		{
			Debug.Log($"ℹ️ Tween symbol unchanged: {detectedSymbol}");
			return;
		}

		if (!string.IsNullOrEmpty(detectedSymbol))
		{
			currentSymbols.Add(detectedSymbol);
			Debug.Log($"✅ Detected {detectedSymbol} → applied by priority");
		}
		else
		{
			Debug.LogWarning("⚠️ No Tween library detected.");
			detectedSymbol = "None";
		}

		PlayerSettings.SetScriptingDefineSymbolsForGroup(
			buildTarget,
			string.Join(";", currentSymbols.Distinct())
		);

		EditorPrefs.SetString(LAST_LIB_KEY, detectedSymbol);
	}

	public static void AA_SYMBOL_CHECK()
	{
		var buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
		var currentSymbols = PlayerSettings
			.GetScriptingDefineSymbolsForGroup(buildTarget)
			.Split(';')
			.Where(s => !string.IsNullOrEmpty(s))
			.ToList();
		
		currentSymbols.Remove(ADDRESSABLE_SYMBOL);
		string detectedSymbol = null;
		if (IsLibraryDetected(ADDRESSABLE_SYMBOL))
		{
			detectedSymbol = ADDRESSABLE_SYMBOL;
		}
		
		string lastLib = EditorPrefs.GetString(LAST_LIB_KEY, "None");

		if (detectedSymbol == lastLib)
		{
			Debug.Log($"ℹ️ Tween symbol unchanged: {detectedSymbol}");
			return;
		}
		
		if (!string.IsNullOrEmpty(detectedSymbol))
		{
			currentSymbols.Add(detectedSymbol);
			Debug.Log($"✅ Detected {detectedSymbol} → add to symbols");
		}
		else
		{
//			Debug.LogWarning("⚠️ No Tween library detected.");
			detectedSymbol = "None";
		}
		
		PlayerSettings.SetScriptingDefineSymbolsForGroup(
			buildTarget,
			string.Join(";", currentSymbols.Distinct())
		);

		EditorPrefs.SetString(LAST_LIB_KEY, detectedSymbol);
	}

	static bool IsLibraryDetected(string symbol)
	{
		return symbol switch
		{
			DOTWEEN_SYMBOL =>
				AssetDatabase.FindAssets("DOTween t:Script").Length > 0 ||
				AssetDatabase.FindAssets("DG.Tweening t:Script").Length > 0,
			PRIMETWEEN_SYMBOL =>
				AssetDatabase.FindAssets("PrimeTween t:Script").Length > 0,
			LITMOTION_SYMBOL =>
				AssetDatabase.FindAssets("LitMotion t:Script").Length > 0,
			ADDRESSABLE_SYMBOL =>
				AssetDatabase.FindAssets("Addressable t:Script").Length > 0,
			
			_ => false
		};
	}


}
#endif
