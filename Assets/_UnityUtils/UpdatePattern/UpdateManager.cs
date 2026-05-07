using System.Collections.Generic;
using UnityEngine;

public class UpdateManager : MonoBehaviour
{
	public static List<IUpdate> updateList = new List<IUpdate>();

	private void Update()
	{
		if(updateList.Count == 0) return;
		for (int i = 0; i < updateList.Count; i++)
		{
			updateList[i].OnUpdate();
		}
	}
	
	public static void AddUpdate(IUpdate update)
	{
		if(!updateList.Contains(update)) updateList.Add(update);
	}
	
	public static void RemoveUpdate(IUpdate update)
	{
		if(updateList.Contains(update)) updateList.Remove(update);
	}
	
}
