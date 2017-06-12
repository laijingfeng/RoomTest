using UnityEngine;
using Jerry;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class Wall : SingletonMono<Wall>, IDragHandler, IBeginDragHandler
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

    public bool m_UseDragOne = false;
    public float m_DragingFactor = 60f;

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
    }

    private GUILayoutOption[] m_GUIOpt1 = new GUILayoutOption[2] { GUILayout.MinWidth(100), GUILayout.MinHeight(60) };
    private bool m_EditorMode = false;

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("放置选中", m_GUIOpt1))
        {
            if (!m_EditorMode)
            {
                Tip.Inst.ShowTip("请先进入[编辑模式]");
                return;
            }

            if (MapUtil.m_SelectId == 0
                || MapUtil.m_SelectOK)
            {
                Tip.Inst.ShowTip("没有选中家具");
                return;
            }
            JerryEventMgr.DispatchEvent(Enum_Event.SetOne.ToString(), new object[] { MapUtil.m_SelectId });
        }

        if (GUILayout.Button("回收选中", m_GUIOpt1))
        {
            if (!m_EditorMode)
            {
                Tip.Inst.ShowTip("请先进入[编辑模式]");
                return;
            }

            if (MapUtil.m_SelectId == 0
                || MapUtil.m_SelectOK)
            {
                Tip.Inst.ShowTip("没有选中家具");
                return;
            }
            JerryEventMgr.DispatchEvent(Enum_Event.Back2Package.ToString(), new object[] { MapUtil.m_SelectId });
        }

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

        GUI.color = m_EditorMode ? Color.green : Color.white;
        if (GUILayout.Button("编辑模式", m_GUIOpt1))
        {
            m_EditorMode = !m_EditorMode;

            if (m_EditorMode)
            {
                AdjustCamera();
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

    #region 移动镜头

    public float m_DragFactor = 0.5f;
    public float m_RotateFactor = 0.5f;

    public float m_DragBoundEditor = 4.8f;
    public float m_DragBound = 4.4f;
    public float m_RotateBound = 25;

    public bool m_DragCameraInUse = true;

    private bool m_DragUsefull = false;
    //private Ray m_Ray;
    //private RaycastHit m_HitInfo;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!m_DragCameraInUse)
        {
            return;
        }

        m_Ray = Camera.main.ScreenPointToRay(Util.GetClickPos());
        if (Physics.Raycast(m_Ray, out m_HitInfo, 100,
            JerryUtil.MakeLayerMask(JerryUtil.MakeLayerMask(false),
                new string[]
                {
                    Enum_Layer.ActiveCube.ToString()
                })))
        {
            if (m_HitInfo.collider != null
                && m_HitInfo.collider.gameObject != null)
            {
                //点到选中的物体，是移动物体，不移动镜头
                m_DragUsefull = false;
                return;
            }
        }
        m_DragUsefull = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!m_DragCameraInUse)
        {
            return;
        }

        if (!m_DragUsefull)
        {
            return;
        }

        if (Mathf.Abs(eventData.delta.x) < 3f
            || Mathf.Abs(eventData.delta.x) < Mathf.Abs(eventData.delta.y))
        {
            return;
        }

        //Debug.LogWarning("drag");
        DoDrag(-eventData.delta.x);
    }

    private Vector3 tmp1;
    private Vector3 tmp2;

    private float GetDragBound
    {
        get
        {
            return m_EditorMode ? m_DragBoundEditor : m_DragBound;
        }
    }

    public void DoDrag(float val)
    {
        if (!m_DragCameraInUse)
        {
            return;
        }

        //Debug.LogWarning("val=" + val);
        tmp1 = Camera.main.transform.position;
        tmp2 = Camera.main.transform.eulerAngles;

        if (tmp1.x >= GetDragBound && (tmp2.y > 0 || val > 0))
        {
            tmp1.x = GetDragBound;
            tmp2.y += val * m_RotateFactor;
            tmp2.y = Mathf.Clamp(tmp2.y, 0, m_RotateBound);
        }
        else if (tmp1.x <= -GetDragBound && (tmp2.y > 360 - m_RotateBound || val < 0))
        {
            tmp1.x = -GetDragBound;
            if (tmp2.y > 0)
            {
                tmp2.y -= 360;
            }
            tmp2.y += val * m_RotateFactor;
            tmp2.y = Mathf.Clamp(tmp2.y, -m_RotateBound, 0);
        }
        else
        {
            tmp1.x += val * m_DragFactor;
            tmp1.x = Mathf.Clamp(tmp1.x, -GetDragBound, GetDragBound);
            tmp2.y = 0f;
        }

        Camera.main.transform.position = tmp1;
        Camera.main.transform.eulerAngles = tmp2;
    }

    private void AdjustCamera()
    {
        this.StopCoroutine("IE_AdjustCamera");
        this.StartCoroutine("IE_AdjustCamera");
    }

    private IEnumerator IE_AdjustCamera()
    {
        tmp1 = Camera.main.transform.position;
        tmp1.x = Mathf.Clamp(tmp1.x, -GetDragBound, GetDragBound);
        //Debug.LogWarning(MapUtil.Vector3String(Camera.main.transform.position) + " ||| " + MapUtil.Vector3String(tmp1));
        Vector3 v = Vector3.zero;
        while (!Util.Vector3Equal(tmp1, Camera.main.transform.position, 0.05f))
        {
            Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position, tmp1, ref v, Time.deltaTime * 5);
            yield return new WaitForEndOfFrame();
        }
        Camera.main.transform.position = tmp1;
    }

    #endregion 移动镜头
}
