using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class GameManagers : MonoBehaviour
{
    [SerializeField] private int _totalFood; //tong so thuc an
    [SerializeField] private int _totalGrill; //tong so bep nuong
    [SerializeField] private Transform _gridGrill; //container chua cac bep nuong
    private List<GrillStation> _listGrills;
    private float _avgTray; // gia tri trung binh mon/1dia
    private List<Sprite> _totalSpriteFood; //danh sach hinh anh thuc an

    void Awake()
    {
        _listGrills = Utils.GetListInChild<GrillStation>(_gridGrill);
        Sprite[] loadedSprites = Resources.LoadAll<Sprite>("Items");
        _totalSpriteFood = loadedSprites.ToList();
    }
    void Start()
    {
        OnInitLevel();
    }
    private void OnInitLevel()
    {
        List<Sprite> takeFood = _totalSpriteFood.OrderBy(x => Random.value).Take(_totalFood).ToList(); //random va lay so luong thuc an can thiet
        List<Sprite> useFood = new List<Sprite>();
        for (int i = 0; i < takeFood.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                useFood.Add(takeFood[i]);
            }
        }
        //random vi tri item
        for (int i = 0; i < useFood.Count; i++)
        {
            int rand = Random.Range(i, useFood.Count);
            (useFood[i], useFood[rand]) = (useFood[rand], useFood[i]);//hoan doi vi tri i hien tai cua vong lap va tri rand duoc random
        }
        _avgTray = Random.Range(1.5f, 2f); //gia tri trung binh mon/1 dia
        int totalTray = Mathf.RoundToInt(useFood.Count / _avgTray); //tong so dia can thiet

        List<int> trayPerGrill = this.DistributeEvelyn(_totalGrill, totalTray); //phan bo so dia cho moi bep nuong
        List<int> foodPerGrill = this.DistributeEvelyn(_totalGrill, useFood.Count); //phan bo so mon an cho moi bep nuong

        for (int i = 0; i < _listGrills.Count; i++)
        {
            bool activeGrill = i < _totalGrill;
            _listGrills[i].gameObject.SetActive(activeGrill);
            if (activeGrill)
            {
                List<Sprite> listFood = Utils.TakeAndRemoveRandom<Sprite>(useFood, foodPerGrill[i]);
                _listGrills[i].OnInitGrill(trayPerGrill[i], listFood);

            }
        }
    }
    private List<int> DistributeEvelyn(int grillCount, int totalTray)
    {
        List<int> result = new List<int>(); //tinh trung binh so luong dia tren moi bep nuong
        float avg = (float)totalTray / grillCount;
        int low = Mathf.FloorToInt(avg); //3
        int high = Mathf.CeilToInt(avg); //4
        int highCount = totalTray - (low * grillCount); //so bep nuong can 4 dia
        int lowCount = grillCount - highCount; //so bep nuong can 3 dia
        for (int i = 0; i < lowCount; i++)
        {
            result.Add(low);
        }
        for (int j = 0; j < highCount; j++)
        {
            result.Add(high);
        }
        //dao vi tri
        for (int i = 0; i < result.Count; i++)
        {
            int rand = Random.Range(i, result.Count);
            (result[i], result[rand]) = (result[rand], result[i]);
        }

        return result;
    }
}
