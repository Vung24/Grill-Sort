#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

public class DEV_DUPLICATE_TRANSFROM : MonoBehaviour
{
	public List<Transform> copyTransform;
	public List<Transform> pasteTransform;

	public bool CHANGE_NAME = true;
	
	[Button]
	private void DEV_COPY_AND_PASTE_TRANSFORM(){
		for (int i = 0; i < copyTransform.Count; i++)
		{
			if(CHANGE_NAME) pasteTransform[i].name = copyTransform[i].name;
			pasteTransform[i].position = copyTransform[i].position;
			pasteTransform[i].rotation = copyTransform[i].rotation;
			pasteTransform[i].localScale = copyTransform[i].localScale;
		}
	}
}
#endif