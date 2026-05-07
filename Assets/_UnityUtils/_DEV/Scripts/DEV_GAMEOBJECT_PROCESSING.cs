using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DEV_GAMEOBJECT_PROCESSING : MonoBehaviour
{
	public string AddFrontSting, AddBackString, ReplaceString;
	public List<GameObject> targets;

	public bool ADD_INDEX;
	
	[Button]
	public void DEV_NAME()
	{
		var backString = AddBackString;
		
		for (int i = 0; i < targets.Count; i++)
		{
			if (!String.IsNullOrEmpty(ReplaceString))
			{
				targets[i].name = ReplaceString;
			}
			
			
			if (!String.IsNullOrEmpty(AddFrontSting))
			{
				if (targets[i].name.Contains(AddFrontSting))
				{
				
				}
				else
				{
					targets[i].name = targets[i].name + AddFrontSting;
				}
			}
			if (ADD_INDEX)
			{
				backString = AddBackString + i;
			}
			if(DebugLogger.isDebugLog) this.Log(backString);

			if (!String.IsNullOrEmpty(backString))
			{
				if (targets[i].name.Substring(targets[i].name.Length - AddBackString.Length, AddBackString.Length).Contains(backString))
				{
				
				}
				else
				{
					targets[i].name =  targets[i].name + backString;
					if(DebugLogger.isDebugLog) this.Log(targets[i].name);
				}
			}
			#if UNIRT_EDITOR
			targets[i].SetDirty();
#endif
		}
	}

}
