#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class DEV_ANIMATOR_PROCESSING : MonoBehaviour
{
	public AnimationClip clip;



	// Example: offset keyframes của 1 property
	public void OffsetProperty(string propertyName, float offset)
	{
		var bindings = AnimationUtility.GetCurveBindings(clip);

		foreach (var bind in bindings)
		{
			if (bind.propertyName == propertyName)
			{
				var curve = AnimationUtility.GetEditorCurve(clip, bind);

				for (int i = 0; i < curve.keys.Length; i++)
				{
					Keyframe k = curve.keys[i];
					k.value += offset;              // cộng offset
					curve.MoveKey(i, k);
				}

				AnimationUtility.SetEditorCurve(clip, bind, curve);
			}
		}

		AssetDatabase.SaveAssets();
	}
}

#endif
