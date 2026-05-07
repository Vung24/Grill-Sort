#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEditorInternal;

public partial class SYMBOL_CHECKER : AssetPostprocessor
{
	public static bool AUTO_REFRESH = false;

	static void OnPostprocessAllAssets(string[] importedAssets, string[] _, string[] __, string[] ___)
	{
		if (importedAssets.Any(path => path.EndsWith(".cs")) && AUTO_REFRESH)
		{
			CHECK_SYMBOL();
		}
	}

	public static void CHECK_SYMBOL()
	{
		UTILS_CHECK_SYMBOL();
		MEDIATION_SYMBOL_CHECK();
	}

	public static partial void UTILS_CHECK_SYMBOL();
	public static partial void MEDIATION_SYMBOL_CHECK();
	public static partial void MEDIATION_SYMBOL_CHECK()
	{

	}

}


public partial class SYMBOL_CHECKER_EDITOR : EditorWindow
{
	private ReorderableList _list;

	private List<string> _prioritySymbols;


	[MenuItem("Unity Utils/Symbol Manager")]
	public static void ShowWindow()
	{
		GetWindow<SYMBOL_CHECKER_EDITOR>("SYMBOL MANAGER");
		SYMBOL_CHECKER.AUTO_REFRESH = EditorPrefs.GetBool("AUTO_REFRESH", true);
	}

	private void OnEnable()
	{
		_prioritySymbols = SYMBOL_CHECKER.LoadPriority(SYMBOL_CHECKER.tweenSymbols);

		_list = new ReorderableList(_prioritySymbols, typeof(string), true, true, false, false);

		_list.drawHeaderCallback = rect =>
		{
			EditorGUI.LabelField(rect, "Symbol Priority (Top = Highest)");
		};

		_list.drawElementCallback = (rect, index, _, _) =>
		{
			EditorGUI.LabelField(rect, _prioritySymbols[index]);
		};

		_list.onReorderCallback = _ =>
		{
			SYMBOL_CHECKER.SavePriority(_prioritySymbols);
			Debug.Log("Symbol priority saved");
		};

		SYMBOL_CHECKER.tweenSymbols = _prioritySymbols;
	}

	public void OnGUI()
	{
		GUILayout.Label("SYMBOL HAD:", EditorStyles.boldLabel);

		var target = EditorUserBuildSettings.selectedBuildTargetGroup;
		string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);

		string[] symbolList = symbols.Split(';');

		foreach (var s in symbolList)
		{
			GUILayout.Label("• " + s);
		}

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("AUTO REFRESH", GUILayout.Width(120));

		bool newValue = DrawSwitch(SYMBOL_CHECKER.AUTO_REFRESH);

		if (newValue != SYMBOL_CHECKER.AUTO_REFRESH)
		{
			SYMBOL_CHECKER.AUTO_REFRESH = newValue;
			EditorPrefs.SetBool("AUTO_REFRESH", newValue);
			if (newValue)
			{
				SYMBOL_CHECKER.CHECK_SYMBOL();
			}
		}

		EditorGUILayout.EndHorizontal();

		

		if (GUILayout.Button("SYMBOL CHECKER"))
		{
			EditorPrefs.SetString(SYMBOL_CHECKER.LAST_LIB_KEY, "");
			SYMBOL_CHECKER.CHECK_SYMBOL();
		}


		_list.DoLayoutList();
	}

	bool DrawSwitch(bool value)
    {
        Rect rect = GUILayoutUtility.GetRect(40, 20);
        rect.size = new Vector2(40, 20);
    
        Color bgColor = value ? new Color(0.3f, 0.8f, 0.4f) : new Color(0.6f, 0.6f, 0.6f);
        EditorGUI.DrawRect(rect, bgColor);
    
        float knobSize = rect.height - 4;
        Rect knob = new Rect(
            value ? rect.xMax - knobSize - 2 : rect.x + 2,
            rect.y + 2,
            knobSize,
            knobSize
        );
    
        EditorGUI.DrawRect(knob, Color.white);
    
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            value = !value;
            Event.current.Use();
            GUI.changed = true;
        }
    
        return value;
    }



}

#endif
