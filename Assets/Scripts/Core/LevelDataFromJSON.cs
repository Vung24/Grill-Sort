using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelDataFromJSON
{
    public int difficult;
    public int levelSeconds;
    public BoardData boardData;
    public SpawnWareData spawnWareData;

    public static LevelDataFromJSON LoadFromResources(string fileName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>($"Levels/{fileName}");

        if (jsonFile == null)
        {
            return null;
        }
        LevelDataFromJSON data = JsonUtility.FromJson<LevelDataFromJSON>(jsonFile.text);
        if (data.boardData != null && data.boardData.listTrayData != null)
        {
            for (int i = 0; i < data.boardData.listTrayData.Count; i++)
            {
                if (data.boardData.listTrayData[i] == null)
                {
                    Debug.Log($"  Tray {i}: NULL");
                }
                else if (string.IsNullOrEmpty(data.boardData.listTrayData[i].id))
                {
                    data.boardData.listTrayData[i] = null;
                }
            }
        }
        return data;
    }

    public static LevelDataFromJSON LoadFromJSON(string jsonText)
    {
        try
        {
            return JsonUtility.FromJson<LevelDataFromJSON>(jsonText);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing JSON: {e.Message}");
            return null;
        }
    }
}

[System.Serializable]
public class BoardData
{
    public List<TrayPosition> listTrayPos;
    public string id;
    public int width;
    public int height;
    public List<TrayDataInfo> listTrayData;
}

[System.Serializable]
public class TrayPosition
{
    public float x;
    public float y;
    public float z;

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}

[System.Serializable]
public class TrayDataInfo
{
    public string id;
    public int size;
    public int specificLayer;
    public int requiredMatch;
}

[System.Serializable]
public class SpawnWareData
{
    public int totalWare;
    public int totalWarePattern;
    public List<string> listWareSet;
    public List<LayerData> listLayerData;
    public int numberIce;
    public int maxNumberIceInTray;
    public int numberTrayNoIce;
    public int numberHidden;
    public int maxNumberHiddenInTray;
}

[System.Serializable]
public class LayerData
{
    public int numberEmptySlot;
    public float match3Ratio;
    public float match2Ratio;
    public float match1Ratio;
}