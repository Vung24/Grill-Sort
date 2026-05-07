using UnityEngine;

public class DEV_UI_PROCESSING : MonoBehaviour
{
	public float scaleRatio;
	public bool scaleParent;
	

	[Button]
	private void DEV_SCALE_UI_ELEMENT()
	{
		ScaleAllChildRectTransforms(transform);
		if(scaleParent) transform.GetComponent<RectTransform>().sizeDelta *= scaleRatio;
	}
	
	private void ScaleAllChildRectTransforms(Transform parent)
	{
		foreach (Transform child in parent)
		{
			RectTransform rectTransform = child.GetComponent<RectTransform>();
			if (rectTransform != null)
			{
				// Scale the sizeDelta
				rectTransform.sizeDelta *= scaleRatio;
			}

			// Recursively scale child transforms
			ScaleAllChildRectTransforms(child);
		}
	}	
}
