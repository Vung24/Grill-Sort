using System.Collections.Generic;
using UnityEngine;

public static class Yielder {
	static Dictionary<float, WaitForSeconds> timeInterval = new Dictionary<float, WaitForSeconds>();

	public static WaitForSeconds Get(float time) {
		if (!timeInterval.ContainsKey(time)) {
			timeInterval.Add(time, new WaitForSeconds(time));
		}

		return timeInterval[time];
	}

	public static WaitForSeconds WaitForSeconds(float time)
	{
		return new (time);
	}
}
