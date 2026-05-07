using UnityEngine;

public static class DebugLogger
{
	public static bool isDebugLog = true;

	public static void Log(this MonoBehaviour mono, object message)
	{
		string scriptName = mono.GetType().Name;
		Debug.Log($"{scriptName} Log: {message}", mono.gameObject);
	}

	public static void LogWarning(this MonoBehaviour mono, object message)
	{
		string scriptName = mono.GetType().Name;
		Debug.LogWarning($"{scriptName}: {message}", mono.gameObject);
	}

	public static void LogError(this MonoBehaviour mono, object message)
	{
		string scriptName = mono.GetType().Name;
		Debug.LogError($"{scriptName}: {message}", mono.gameObject);
	}

	public static void Log(object message)
	{

		Debug.Log($"Log: {message}");
	}

	public static void LogWarning(object message)
	{

		Debug.LogWarning($"{message}");
	}

	public static void LogError(object message)
	{
		Debug.LogError($"{message}");
	}

}
