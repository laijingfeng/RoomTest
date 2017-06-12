using System.Collections.Generic;
using Jerry;
using UnityEngine;

public class Wall : SingletonMono<Wall>
{
    public Vector3 m_WallStartPos;
    public MapUtil.IVector3 m_WallSize;

    public Vector3 m_LeftSideWallStartPos;
    public MapUtil.IVector3 m_LeftSideWallSize;

    public Vector3 m_RightSideWallStartPos;
    public MapUtil.IVector3 m_RightSideWallSize;

    public Vector3 m_FloorWallStartPos;
    public MapUtil.IVector3 m_FloorWallSize;

    public float m_MapGridUnityLen;
    public CtrObjType m_CtrType = CtrObjType.ClickAndDrag;

    public float m_OutScreenJudgeFactor = 50;
    public float m_OutScreenDragFactor = 5;

    public enum CtrObjType
    {
        OnlyClick = 0,
        OnlyDrag,
        ClickAndDrag,
    }

    private Ray m_Ray;
    private RaycastHit m_HitInfo;

    public override void Awake()
    {
        base.Awake();

        MapUtil.m_MapGridUnityLen = m_MapGridUnityLen;

        MapUtil.m_Wall.m_StartPos = m_WallStartPos;
        MapUtil.m_Wall.m_Size = m_WallSize;

        MapUtil.m_LeftSideWall.m_StartPos = m_LeftSideWallStartPos;
        MapUtil.m_LeftSideWall.m_Size = m_LeftSideWallSize;

        MapUtil.m_RightSideWall.m_StartPos = m_RightSideWallStartPos;
        MapUtil.m_RightSideWall.m_Size = m_RightSideWallSize;

        MapUtil.m_FloorWall.m_StartPos = m_FloorWallStartPos;
        MapUtil.m_FloorWall.m_Size = m_FloorWallSize;

        MapUtil.Init();

#if UNITY_EDITOR
        JerryDrawer.Draw<DrawerElementGrid>()
                    .SetMinPos(m_LeftSideWallStartPos)
                    .SetGridSize(new Vector3(0, MapUtil.m_MapGridUnityLen, MapUtil.m_MapGridUnityLen))
                    .SetSize(m_LeftSideWallSize.ToVector3())
                    .SetColor(Color.red);

        JerryDrawer.Draw<DrawerElementGrid>()
                    .SetMinPos(m_RightSideWallStartPos)
                    .SetGridSize(new Vector3(0, MapUtil.m_MapGridUnityLen, MapUtil.m_MapGridUnityLen))
                    .SetSize(m_RightSideWallSize.ToVector3())
                    .SetColor(Color.red);

        JerryDrawer.Draw<DrawerElementGrid>()
                    .SetMinPos(m_WallStartPos)
                    .SetGridSize(new Vector3(MapUtil.m_MapGridUnityLen, MapUtil.m_MapGridUnityLen, 0))
                    .SetSize(m_WallSize.ToVector3())
                    .SetColor(Color.black);

        JerryDrawer.Draw<DrawerElementGrid>()
                    .SetMinPos(m_FloorWallStartPos)
                    .SetGridSize(new Vector3(MapUtil.m_MapGridUnityLen, 0, MapUtil.m_MapGridUnityLen))
                    .SetSize(m_FloorWallSize.ToVector3())
                    .SetColor(Color.black);
#endif
    }

    void Update()
    {
        ClickPlaceObj();

        //if (Input.GetMouseButtonDown(0))
        //{
        //    Debug.LogWarning("xxx " + Util.ClickUI());
        //}
    }

    private GUILayoutOption[] m_GUIOpt1 = new GUILayoutOption[2] { GUILayout.MinWidth(100), GUILayout.MinHeight(60) };
    private bool m_EditorMode = false;
    public bool EditorMode
    {
        get
        {
            return m_EditorMode;
        }
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        //if (GUILayout.Button("放置选中", m_GUIOpt1))
        //{
        //    if (!m_EditorMode)
        //    {
        //        Tip.Inst.ShowTip("请先进入[编辑模式]");
        //        return;
        //    }

        //    if (MapUtil.m_SelectId == 0
        //        || MapUtil.m_SelectOK)
        //    {
        //        Tip.Inst.ShowTip("没有选中家具");
        //        return;
        //    }
        //    JerryEventMgr.DispatchEvent(Enum_Event.SetOne.ToString(), new object[] { MapUtil.m_SelectId });
        //}

        //if (GUILayout.Button("回收选中", m_GUIOpt1))
        //{
        //    if (!m_EditorMode)
        //    {
        //        Tip.Inst.ShowTip("请先进入[编辑模式]");
        //        return;
        //    }

        //    if (MapUtil.m_SelectId == 0
        //        || MapUtil.m_SelectOK)
        //    {
        //        Tip.Inst.ShowTip("没有选中家具");
        //        return;
        //    }
        //    JerryEventMgr.DispatchEvent(Enum_Event.Back2Package.ToString(), new object[] { MapUtil.m_SelectId });
        //}

        GUI.color = m_EditorMode ? Color.green : Color.white;
        if (GUILayout.Button("编辑模式", m_GUIOpt1))
        {
            m_EditorMode = !m_EditorMode;

            if (m_EditorMode)
            {
                DragCamera.Inst.AdjustCamera();
            }

            if (MapUtil.m_SelectId == 0
                || MapUtil.m_SelectOK)
            {
                return;
            }

            if (MapUtil.m_SelectNew)
            {
                JerryEventMgr.DispatchEvent(Enum_Event.Back2Package.ToString(), new object[] { MapUtil.m_SelectId });
            }
            else
            {
                JerryEventMgr.DispatchEvent(Enum_Event.BackOne.ToString(), new object[] { MapUtil.m_SelectId });
            }
        }
        GUI.color = Color.white;

        if (GUILayout.Button("随机一个", m_GUIOpt1))
        {
            if (!m_EditorMode)
            {
                Tip.Inst.ShowTip("请先进入[编辑模式]");
                return;
            }

            Drag[] drags = this.transform.parent.GetComponentsInChildren<Drag>();
            if (drags == null || drags.Length <= 0)
            {
                Tip.Inst.ShowTip("没有可用家具");
                return;
            }
            List<Drag> usefullDrags = new List<Drag>();
            for (int i = 0; i < drags.Length; i++)
            {
                if (drags[i].m_InitData.isNew)
                {
                    usefullDrags.Add(drags[i]);
                }
            }
            if (usefullDrags.Count <= 0)
            {
                Tip.Inst.ShowTip("没有可用家具");
                return;
            }
            int idx = Random.Range(0, usefullDrags.Count);
            usefullDrags[idx].ToScreen();
        }

        GUILayout.EndHorizontal();
    }

    #region 点击放置

    private Vector3 m_ClickDownPos = Vector3.zero;
    private Vector3 m_ClickUpPos = Vector3.zero;

    /// <summary>
    /// 点击放置
    /// </summary>
    private void ClickPlaceObj()
    {
        if (Util.ClickUI())
        {
            return;
        }

        if (m_CtrType == CtrObjType.OnlyDrag)
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

            m_Ray = Camera.main.ScreenPointToRay(Util.GetClickPos());

            if (Physics.Raycast(m_Ray, out m_HitInfo, 100))
            {
                if (m_HitInfo.collider == null
                    || m_HitInfo.collider.gameObject == null)
                {
                    return;
                }
                if (MapUtil.IsWallLayer(m_HitInfo.collider.gameObject.layer))
                {
                    FirstPos fp = new FirstPos();
                    fp.pos = m_HitInfo.point;
                    fp.wallType = MapUtil.WallLayer2Enum(m_HitInfo.collider.gameObject.layer);

                    //JerryDrawer.Draw<DrawerElementCube>()
                    //    .SetColor(Color.black)
                    //    .SetLife(3f)
                    //    .SetPos(m_HitInfo.point)
                    //    .SetSize(Vector3.one)
                    //    .SetWire(false)
                    //    .SetSizeFactor(0.2f);

                    JerryEventMgr.DispatchEvent(Enum_Event.Place2Pos.ToString(), new object[] { fp });
                }
                else if (m_CtrType == CtrObjType.OnlyClick
                    && m_HitInfo.collider.gameObject.layer == LayerMask.NameToLayer(Enum_Layer.ActiveCube.ToString()))
                {
                    if (Physics.Raycast(m_Ray, out m_HitInfo, 100, JerryUtil.MakeLayerMask(JerryUtil.MakeLayerMask(false), MapUtil.GetWallLayerNames())))
                    {
                        if (m_HitInfo.collider != null
                            && m_HitInfo.collider.gameObject != null)
                        {
                            FirstPos fp = new FirstPos();
                            fp.pos = m_HitInfo.point;
                            fp.wallType = MapUtil.WallLayer2Enum(m_HitInfo.collider.gameObject.layer);

                            JerryEventMgr.DispatchEvent(Enum_Event.Place2Pos.ToString(), new object[] { fp });
                        }
                    }
                }
            }
        }
    }

    #endregion 点击放置
}