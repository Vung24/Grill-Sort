using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DEV_ANIMATOR_PROCESSING))]
public class DEV_ANIMATOR_PROCESSING_EDITOR : Editor
{
	private string[] propertyList;
	private int selectedIndex = 0;
	private float offsetValue = 0f;

	private void OnSceneGUI()
	{
		var clip = (target as DEV_ANIMATOR_PROCESSING).clip;

		// lấy danh sách property
		var bindings = AnimationUtility.GetCurveBindings(clip);
		propertyList = bindings.Select(b => b.propertyName).Distinct().ToArray();
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (propertyList == null || propertyList.Length == 0)
		{
			EditorGUILayout.HelpBox("No anim properties found.", MessageType.Info);
			return;
		}

		EditorGUILayout.Space(10);

		GUILayout.Label("Dev Animator Tools", EditorStyles.boldLabel);

		// Dropdown property
		selectedIndex = EditorGUILayout.Popup("Property", selectedIndex, propertyList);

		
		// Offset input
		offsetValue = EditorGUILayout.FloatField("Offset", offsetValue);

		if (GUILayout.Button("Apply Offset"))
		{
			var clip = target as AnimationClip;
			var processor = target as DEV_ANIMATOR_PROCESSING;

			processor.OffsetProperty(propertyList[selectedIndex], offsetValue);

			Debug.Log("Offset applied!");
		}
	}
}
