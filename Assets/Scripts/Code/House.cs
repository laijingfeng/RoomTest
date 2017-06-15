using System;
using System.Collections.Generic;
using Jerry;
using UnityEngine;

public class House : MonoBehaviour 
{
    public int floor = 0;

    private Transform furnituresParent;

    private bool _awaked = false;
    private bool _inited = false;

    void Awake()
    {
        furnituresParent = this.transform.FindChild("Furnitures");

        JerryEventMgr.AddEvent(Enum_Event.SaveCurHouseData.ToString(), EventSaveCurHouseData);

        _awaked = true;
        TryWork();
    }

    void OnDestroy()
    {
        JerryEventMgr.RemoveEvent(Enum_Event.SaveCurHouseData.ToString(), EventSaveCurHouseData);
    }

    public void Init(int _floor)
    {
        floor = _floor;
        _inited = true;
        TryWork();
    }

    private void TryWork()
    {
        if (!_awaked || !_inited)
        {
            return;
        }
        LoadSetFurnitures();
    }

    #region 事件

    private void EventSaveCurHouseData(object[] args)
    {
        if (floor != GameApp.Inst.GetCurFloor)
        {
            return;
        }
        SaveSetFurnitures();
    }

    #endregion 事件

    #region 存档

    /// <summary>
    /// 加载放好的家具
    /// </summary>
    private void LoadSetFurnitures()
    {
        List<FurnitureSaveData> datas = FurnitureUtil.LoadFurnitureData(floor);
        if (datas == null || datas.Count <= 0)
        {
            return;
        }

        foreach (FurnitureSaveData sd in datas)
        {
            Furniture f = FurnitureUtil.LoadFurnitureByType(sd.type, furnituresParent);
            f.InitData(FurnitureUtil.FurnitureType2Config(sd.type), sd);
        }
    }

    private void SaveSetFurnitures()
    {
        Furniture[] fs = furnituresParent.GetComponentsInChildren<Furniture>();
        List<FurnitureSaveData> sds = new List<FurnitureSaveData>();
        foreach (Furniture f in fs)
        {
            if (f.m_InitData.isSeted)
            {
                f.RefreshSaveDataPos();
                sds.Add(f.m_SaveData);
            }
        }
        FurnitureUtil.SaveFurnitureData(floor, sds);
    }

    #endregion 存档

    public void AddOneFurniture()
    {
        Array arr = Enum.GetValues(typeof(FurnitureType));
        int idx = UnityEngine.Random.Range(1, arr.Length);
        FurnitureType type = (FurnitureType)arr.GetValue(idx);
        Furniture f = FurnitureUtil.LoadFurnitureByType(type, furnituresParent);
        f.InitData(FurnitureUtil.FurnitureType2Config(type) ,new FurnitureSaveData());
    }
}