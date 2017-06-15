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

    void Update()
    {
        ClickPlaceObj();
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

    #region 点击放置

    private Vector3 m_ClickDownPos = Vector3.zero;
    private Vector3 m_ClickUpPos = Vector3.zero;

    private Ray m_Ray;
    private RaycastHit m_HitInfo;

    /// <summary>
    /// 点击放置
    /// </summary>
    private void ClickPlaceObj()
    {
        if (floor != GameApp.Inst.GetCurFloor)
        {
            return;
        }

        if (Util.ClickUI())
        {
            return;
        }

        if (MapUtil.m_SelectId == 0
            || MapUtil.m_SelectOK)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            m_ClickDownPos = JerryUtil.GetClickPos();
        }

        if (Input.GetMouseButtonUp(0))
        {
            m_ClickUpPos = JerryUtil.GetClickPos();
            if (!Util.Vector3Equal(m_ClickUpPos, m_ClickDownPos))
            {
                return;
            }

            m_Ray = Camera.main.ScreenPointToRay(JerryUtil.GetClickPos());

            if (Physics.Raycast(m_Ray, out m_HitInfo, 100))
            {
                if (m_HitInfo.collider == null
                    || m_HitInfo.collider.gameObject == null)
                {
                    return;
                }

                if (MapUtil.IsWallLayer(m_HitInfo.collider.gameObject.layer))
                {
                    RayClickPos fp = new RayClickPos();
                    fp.pos = m_HitInfo.point;
                    fp.wallType = MapUtil.WallLayer2WallEnum(m_HitInfo.collider.gameObject.layer);

                    //JerryDrawer.Draw<DrawerElementCube>()
                    //    .SetColor(Color.black)
                    //    .SetLife(3f)
                    //    .SetPos(m_HitInfo.point)
                    //    .SetSize(Vector3.one)
                    //    .SetWire(false)
                    //    .SetSizeFactor(0.2f);

                    JerryEventMgr.DispatchEvent(Enum_Event.SetFurn2Pos.ToString(), new object[] { fp });
                }
            }
        }
    }

    #endregion 点击放置
}