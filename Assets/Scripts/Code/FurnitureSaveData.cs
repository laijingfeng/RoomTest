using System.Collections.Generic;
using Jerry;
using UnityEngine;

public class FurnitureUtil
{
    public static FurnitureConfig FurnitureType2Config(FurnitureType type)
    {
        FurnitureConfig ret = new FurnitureConfig();
        ret.type = type;
        switch (type)
        {
            case FurnitureType.bed_001:
                {
                    ret.setType = MapUtil.SetType.WallOnFloor;
                    ret.size = new MapUtil.IVector3(3, 2, 4);
                }
                break;
            case FurnitureType.bed_002:
                {
                    ret.setType = MapUtil.SetType.WallOnFloor;
                    ret.size = new MapUtil.IVector3(4, 2, 5);
                }
                break;
            case FurnitureType.cabinet_001:
                {
                    ret.setType = MapUtil.SetType.WallOnFloor;
                    ret.size = new MapUtil.IVector3(3, 4, 1);
                }
                break;
            case FurnitureType.chair_001:
                {
                    ret.setType = MapUtil.SetType.Floor;
                    ret.size = new MapUtil.IVector3(1, 1, 1);
                }
                break;
            case FurnitureType.sofa_001:
                {
                    ret.setType = MapUtil.SetType.WallOnFloor;
                    ret.size = new MapUtil.IVector3(2, 2, 2);
                }
                break;
            case FurnitureType.sofa_002:
                {
                    ret.setType = MapUtil.SetType.WallOnFloor;
                    ret.size = new MapUtil.IVector3(5, 2, 2);
                }
                break;
            case FurnitureType.table_001:
                {
                    ret.setType = MapUtil.SetType.Floor;
                    ret.size = new MapUtil.IVector3(2, 1, 2);
                }
                break;
            case FurnitureType.table_002:
                {
                    ret.setType = MapUtil.SetType.Floor;
                    ret.size = new MapUtil.IVector3(2, 1, 2);
                }
                break;
            case FurnitureType.walllamp_001:
                {
                    ret.setType = MapUtil.SetType.Wall;
                    ret.size = new MapUtil.IVector3(1, 1, 1);
                }
                break;
            case FurnitureType.window_001:
                {
                    ret.setType = MapUtil.SetType.Wall;
                    ret.size = new MapUtil.IVector3(4, 4, 0);
                }
                break;
        }
        return ret;
    }

    #region 存档

    public static List<FurnitureSaveData> LoadFurnitureData(int floor)
    {
        List<FurnitureSaveData> ret = new List<FurnitureSaveData>();
        string sdata = PlayerPrefs.GetString(GetSaveKey(floor), "");
        if (!string.IsNullOrEmpty(sdata))
        {
            FurnitureSaveDataSet s = JsonUtility.FromJson<FurnitureSaveDataSet>(sdata);
            if (s != null)
            {
                //Debug.LogWarning("loadCnt=" + JsonUtility.ToJson(s, true));
                ret = s.saveDatas;
            }
        }
        return ret;
    }

    public static void SaveFurnitureData(int floor, List<FurnitureSaveData> datas)
    {
        FurnitureSaveDataSet s = new FurnitureSaveDataSet() { saveDatas = datas };
        //Debug.LogWarning("save=" + JsonUtility.ToJson(s, true) + " " + datas.Count);
        PlayerPrefs.SetString(GetSaveKey(floor), JsonUtility.ToJson(s));
    }

    public static string GetSaveKey(int floor)
    {
        return string.Format("Furn_{0}", floor);
    }

    #endregion 存档

    public static Furniture LoadFurnitureByType(FurnitureType type, Transform parent)
    {
        GameObject obj = Resources.Load<GameObject>("Prefabs/Furnitures/" + type.ToString());
        Furniture t = JerryUtil.CloneGo<Furniture>(new JerryUtil.CloneGoData()
        {
            active = true,
            clean = false,
            parant = parent,
            prefab = obj,
        });
        return t;
    }
}

[System.Serializable]
public class FurnitureSaveDataSet
{
    public List<FurnitureSaveData> saveDatas = new List<FurnitureSaveData>();
}

[System.Serializable]
public class FurnitureSaveData
{
    public FurnitureType type = FurnitureType.None;
    public Enum_Layer saveWall = Enum_Layer.None;
    /// <summary>
    /// 位置，半个格子的几倍
    /// </summary>
    public MapUtil.IVector3 savePos = new MapUtil.IVector3(0, 0, 0);
}

[System.Serializable]
public class FurnitureConfig
{
    public FurnitureType type = FurnitureType.None;
    public MapUtil.SetType setType = MapUtil.SetType.None;
    public MapUtil.IVector3 size = new MapUtil.IVector3(1, 1, 1);
}

[System.Serializable]
public enum FurnitureType
{
    None = 0,
    bed_001,
    bed_002,
    cabinet_001,
    chair_001,
    sofa_001,
    sofa_002,
    table_001,
    table_002,
    walllamp_001,
    window_001,
}