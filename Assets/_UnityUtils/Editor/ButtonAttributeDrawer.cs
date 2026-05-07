using UnityEditor;
using UnityEngine;
using System.Reflection;
using UnityEngine.UI;

[CustomPropertyDrawer(typeof(ButtonAttribute))]
public class ButtonAttributeDrawer : PropertyDrawer
{
}

[CustomEditor(typeof(MonoBehaviour), true)]
public class ButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var methods = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (var method in methods)
        {
            var bMethod = method.GetCustomAttribute(typeof(ButtonAttribute), true);
            if (bMethod != null)
            {
                if (GUILayout.Button(method.Name))
                {
                    method.Invoke(target, null);
                }
            }
        }

        DrawDefaultInspector();
    }
}

public static class MenuExtention
{
    [MenuItem("GameObject/UI/button_effect $b")]
    public static void CreateButtonEffect()
    {
        var btn = new GameObject("button_effect", typeof(RectTransform),
            typeof(UnityEngine.UI.Image));
        btn.transform.SetParent(Selection.transforms[0]);
        btn.transform.localScale = Vector3.one;
        btn.transform.localPosition = Vector3.zero;
        var bImg = btn.GetComponent<Image>();
        bImg.color = new Color32(0, 0, 0, 0);
        var render = new GameObject("Render", typeof(RectTransform)).transform;
        render.SetParent(btn.transform);
        render.localPosition = Vector3.zero;
        render.localScale = Vector3.one;
        var img = new GameObject("sprite", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        img.raycastTarget = false;
        img.maskable = false;
        img.transform.SetParent(render);
        img.transform.localPosition = Vector3.zero;
        img.transform.localScale = Vector3.one;
        Selection.activeObject = btn;
        var buttonEffectLogic = btn.AddComponent<ButtonEffectLogic>();
        buttonEffectLogic.DEV_SET_RENDER_TRANSFORM();
    }


    [MenuItem("GameObject/UI/button_hold $b")]
    public static void CreateButtonHold()
    {
        var btn = new GameObject("button_hold", typeof(RectTransform),
            typeof(UnityEngine.UI.Image));
        btn.transform.SetParent(Selection.transforms[0]);
        btn.transform.localScale = Vector3.one;
        btn.transform.localPosition = Vector3.zero;
        var bImg = btn.GetComponent<Image>();
        bImg.color = new Color32(0, 0, 0, 0);
        var render = new GameObject("Render", typeof(RectTransform)).transform;
        render.SetParent(btn.transform);
        render.localPosition = Vector3.zero;
        render.localScale = Vector3.one;
        var img = new GameObject("sprite", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        img.raycastTarget = false;
        img.maskable = false;
        img.transform.SetParent(render);
        img.transform.localPosition = Vector3.zero;
        img.transform.localScale = Vector3.one;
        
        var filler = new GameObject("filler", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        filler.raycastTarget = false;
        filler.maskable = false;
        filler.transform.SetParent(render);
        filler.transform.localPosition = Vector3.zero;
        filler.transform.localScale = Vector3.one;
        filler.sprite = Resources.Load<Sprite>("Sprites/Common/Circle_Filler"); 
        filler.color = Color.green;
        filler.type = Image.Type.Filled;
        filler.fillOrigin = 2;
        filler.fillAmount = .5f;
        
        Selection.activeObject = btn;
        var buttonEffectLogic = btn.AddComponent<ButtonHoldLogic>();
        buttonEffectLogic.DEV_SET_RENDER_TRANSFORM();
        
        UpdateManager manager = Object.FindObjectOfType<UpdateManager>();
        if (manager == null)
        {
            GameObject obj = new GameObject("UpdateManager");
            obj.AddComponent<UpdateManager>();
        }
        
    }
}