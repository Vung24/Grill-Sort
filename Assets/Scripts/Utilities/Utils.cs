using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Utils : MonoBehaviour
{
    public static List<T> GetListInChild<T>(Transform parent)
    {
        List<T> result = new List<T>();
        for (int i = 0; i < parent.childCount; i++)
        {
            var component = parent.GetChild(i).GetComponent<T>();
            if (component != null)
            {
                result.Add(component);
            }
        }
        return result;
    }
    public static List<T> TakeAndRemoveRandom<T>(List<T> source, int n) //lay va xoa phan tu random trong danh sach
    {
        List<T> result = new List<T>(); //tao danh sach ket qua
        n = Mathf.Min(n, source.Count); //dam bao so luong can lay khong vuot qua so luong hien co
        for (int i = 0; i < n; i++)
        {
            int randIndex = Random.Range(0, source.Count); //lay chi so random trong khoang so luong con lai
            result.Add(source[randIndex]); //them phan tu vao danh sach ket qua
            source.RemoveAt(randIndex); //xoa phan tu da lay khoi danh sach nguon
        }
        return result;
    }
    public static T GetRayCastUI<T>(Vector2 position) where T : MonoBehaviour //lay component UI tu vi tri chon
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current); //tao su kien pointer
        pointerEventData.position = position; //dat vi tri su kien
        List<RaycastResult> list = new List<RaycastResult>(); //tao danh sach ket qua raycast
        EventSystem.current.RaycastAll(pointerEventData, list); //thuc hien raycast
        if (list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++) //duyet qua ket qua raycast
            {
                T component = list[i].gameObject.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }
        }
        return null;
    }
}
