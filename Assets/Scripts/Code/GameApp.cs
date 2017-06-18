using UnityEngine;
using System.Collections;
using Jerry;

public class GameApp : SingletonMono<GameApp>
{
    [Header("Wall Settings")]
    public Vector3 m_WallStartPos;
    public MapUtil.IVector3 m_WallSize;

    [Header("LeftSide Settings")]
    public Vector3 m_LeftSideWallStartPos;
    public MapUtil.IVector3 m_LeftSideWallSize;

    [Header("RightSide Settings")]
    public Vector3 m_RightSideWallStartPos;
    public MapUtil.IVector3 m_RightSideWallSize;

    [Header("Floor Settings")]
    public Vector3 m_FloorWallStartPos;
    public MapUtil.IVector3 m_FloorWallSize;

    [Header("Other Settings")]

    public float m_MapGridUnityLen;
    [HideInInspector]
    public float m_MapGridUnityLenHalf;

    /// <summary>
    /// 不要大于格子单位
    /// </summary>
    public float m_AdjustFurn2WallPar = 0.1f;

    public float m_OutScreenJudgeFactor = 50;
    public float m_OutScreenDragFactor = 5;
    public float m_HouseHeight = 4.6f;

    public bool m_DrawGrid = false;

    private const int HOUSE_NODE_CNT = 2;

    private GameObject space;
    private Transform[] houseNode;
    private Transform housePrefab;

    private int curHouseNodeIdx = 0;
    private int curFloor;
    public int GetCurFloor
    {
        get
        {
            return curFloor;
        }
    }

    private House[] houses;

    /// <summary>
    /// 房子高度Y轴偏移
    /// </summary>
    public float GetHouseYOffset
    {
        get
        {
            return curFloor * m_HouseHeight;
        }
    }

    public int CurNodeIdx
    {
        get
        {
            return curHouseNodeIdx;
        }
    }

    public override void Awake()
    {
        base.Awake();

        space = GameObject.Find("Space");
        houseNode = new Transform[HOUSE_NODE_CNT];
        for (int i = 0; i < HOUSE_NODE_CNT; i++)
        {
            houseNode[i] = space.transform.FindChild(string.Format("HouseNode{0}", i));
        }
        housePrefab = space.transform.FindChild("HousePrefab");
        housePrefab.gameObject.SetActive(false);

        houses = new House[HOUSE_NODE_CNT];

        m_MapGridUnityLenHalf = 0.5f * m_MapGridUnityLen;

        MapUtil.Init();

        if (m_DrawGrid)
        {
#if UNITY_EDITOR
            JerryDrawer.Draw<DrawerElementGrid>()
                        .SetMinPos(m_LeftSideWallStartPos + new Vector3(0.01f, 0, 0))
                        .SetGridSize(Vector2.one * GameApp.Inst.m_MapGridUnityLen)
                        .SetSize(SizeXYZ2XY(m_LeftSideWallSize, Enum_Wall.Left))
                        .SetPlaneType(DrawerElementGrid.PlaneType.ZY)
                        .SetColor(Color.red);

            JerryDrawer.Draw<DrawerElementGrid>()
                        .SetMinPos(m_RightSideWallStartPos + new Vector3(-0.01f, 0, 0))
                        .SetGridSize(Vector2.one * GameApp.Inst.m_MapGridUnityLen)
                        .SetSize(SizeXYZ2XY(m_RightSideWallSize, Enum_Wall.Right))
                        .SetPlaneType(DrawerElementGrid.PlaneType.ZY)
                        .SetColor(Color.red);

            JerryDrawer.Draw<DrawerElementGrid>()
                        .SetMinPos(m_WallStartPos + new Vector3(0, 0, -0.01f))
                        .SetGridSize(Vector2.one * GameApp.Inst.m_MapGridUnityLen)
                        .SetSize(SizeXYZ2XY(m_WallSize, Enum_Wall.Wall))
                        .SetPlaneType(DrawerElementGrid.PlaneType.XY)
                        .SetColor(Color.black);

            JerryDrawer.Draw<DrawerElementGrid>()
                        .SetMinPos(m_FloorWallStartPos + new Vector3(0, 0.01f, 0))
                        .SetGridSize(Vector2.one * GameApp.Inst.m_MapGridUnityLen)
                        .SetSize(SizeXYZ2XY(m_FloorWallSize, Enum_Wall.Floor))
                        .SetPlaneType(DrawerElementGrid.PlaneType.XZ)
                        .SetColor(Color.black);
#endif
        }

        JerryEventMgr.AddEvent(Enum_Event.Click3DObj.ToString(), EventClick3DObj);

        CreateHouse(0, 0);
    }

    private Vector2 SizeXYZ2XY(MapUtil.IVector3 size, Enum_Wall type)
    {
        Vector2 ret;
        ret.x = (type == Enum_Wall.Wall || type == Enum_Wall.Floor) ? size.x : size.z;
        ret.y = (type == Enum_Wall.Floor) ? size.z : size.y;
        return ret;
    }

    void Update()
    {
        Check3DClick();
    }

    void OnDestroy()
    {
        JerryEventMgr.RemoveEvent(Enum_Event.Click3DObj.ToString(), EventClick3DObj);
    }

    #region GUI

    private GUILayoutOption[] m_GUIOpt1 = new GUILayoutOption[2] { GUILayout.MinWidth(80), GUILayout.MinHeight(80) };
    private bool m_EditorMode = false;
    public bool EditorMode
    {
        get
        {
            return m_EditorMode;
        }
    }

    /// <summary>
    /// 正则切换楼层
    /// </summary>
    private bool m_IsUpDowning = false;
    /// <summary>
    /// 正则切换楼层
    /// </summary>
    public bool UpDowning
    {
        get
        {
            return m_IsUpDowning;
        }
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();

        GUI.color = m_EditorMode ? Color.green : Color.white;
        if (GUILayout.Button("编辑模式", m_GUIOpt1))
        {
            if (UpDowning)
            {
                return;
            }

            m_EditorMode = !m_EditorMode;

            if (m_EditorMode)
            {
                CameraCtr.Inst.AdjustCamera();
            }

            if (MapUtil.m_SelectId == 0
                || MapUtil.m_SelectOK)
            {
                return;
            }

            //Debug.LogWarning(" " + MapUtil.m_SelectId + " " + MapUtil.m_SelectNew + " " + MapUtil.m_SelectOK);

            if (MapUtil.m_SelectNew)
            {
                JerryEventMgr.DispatchEvent(Enum_Event.SetFurn2Package.ToString(), new object[] { MapUtil.m_SelectId });
            }
            else
            {
                JerryEventMgr.DispatchEvent(Enum_Event.CancelSetFurn.ToString(), new object[] { MapUtil.m_SelectId });
            }
        }
        GUI.color = Color.white;

        if (m_EditorMode)
        {
            if (GUILayout.Button("保存方案", m_GUIOpt1))
            {
                JerryEventMgr.DispatchEvent(Enum_Event.SaveCurHouseData.ToString());
            }

            if (GUILayout.Button("随机一个", m_GUIOpt1))
            {
                houses[CurNodeIdx].AddOneFurniture();
            }
        }

        if (!m_EditorMode)
        {
            if (GUILayout.Button(string.Format("上楼({0})", curFloor), m_GUIOpt1))
            {
                if (UpDowning)
                {
                    return;
                }

                ToFloor(curFloor + 1);
            }

            if (GUILayout.Button(string.Format("下楼({0})", curFloor), m_GUIOpt1))
            {
                if (UpDowning)
                {
                    return;
                }

                ToFloor(curFloor - 1);
            }
        }

        GUILayout.EndHorizontal();
    }

    private void ToFloor(int f)
    {
        m_IsUpDowning = true;
        this.StopCoroutine("IE_ToFloor");
        this.StartCoroutine("IE_ToFloor", f);
    }

    private IEnumerator IE_ToFloor(int f)
    {
        CreateHouse((curHouseNodeIdx + 1) % HOUSE_NODE_CNT, f);

        yield return new WaitForEndOfFrame();

        Vector3 pos = Camera.main.transform.position;
        pos.y = 3.38f + f * m_HouseHeight;
        while (!Util.Vector3Equal(pos, Camera.main.transform.position, 0.01f))
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, pos, Time.deltaTime * 5);
            yield return new WaitForEndOfFrame();
        }
        Camera.main.transform.position = pos;
        m_IsUpDowning = false;
    }

    #endregion GUI

    /// <summary>
    /// 创建房子
    /// </summary>
    /// <param name="houseNodeIdx">节点索引</param>
    /// <param name="floor">层数，第一层是0</param>
    private void CreateHouse(int houseNodeIdx, int floor)
    {
        curHouseNodeIdx = houseNodeIdx;
        curFloor = floor;
        GridMgr.Inst.RefreshPos();
        MapUtil.ResetMapStartPosY();

        //回到上一次的房子
        if (houses[curHouseNodeIdx] != null
            && houses[curHouseNodeIdx].floor == curFloor)
        {
            //Debug.LogWarning(curFloor);
            return;
        }

        MapUtil.ResetMapFlag();
        //新房子
        houses[curHouseNodeIdx] = JerryUtil.CloneGo<House>(new JerryUtil.CloneGoData()
        {
            parant = houseNode[curHouseNodeIdx],
            prefab = housePrefab.gameObject,
            active = true,
            clean = true,
        });
        houseNode[curHouseNodeIdx].position = new Vector3(0, GetHouseYOffset, 0);
        houses[curHouseNodeIdx].Init(curFloor);
    }

    #region 3D点击

    private Ray m_Ray;
    private RaycastHit m_HitInfo;

    private RayClickInfo m_ClickDownInfo = new RayClickInfo();
    private RayClickInfo m_ClickUpInfo = new RayClickInfo();
    private RayClickInfo m_LastClickInfo = new RayClickInfo();
    private RayClickInfo m_ClickInfoTmp = new RayClickInfo();

    private void Check3DClick()
    {
        if (Input.GetMouseButtonDown(0)
            && !Util.ClickUI())
        {
            m_ClickDownInfo.Init(DoRayClick());
            JerryEventMgr.DispatchEvent(Enum_Event.Click3DDown.ToString(), new object[] { m_ClickDownInfo });
        }

        if (Input.GetMouseButtonUp(0)
            && !Util.ClickUI())
        {
            m_ClickUpInfo.Init(DoRayClick());
            JudgeClick();
        }
    }

    private RayClickInfo DoRayClick()
    {
        m_ClickInfoTmp.Init();
        m_Ray = Camera.main.ScreenPointToRay(JerryUtil.GetClickPos());
        if (Physics.Raycast(m_Ray, out m_HitInfo, 100))
        {
            if (m_HitInfo.collider != null)
            {
                m_ClickInfoTmp.pos = m_HitInfo.point;
                m_ClickInfoTmp.col = m_HitInfo.collider;
                m_ClickInfoTmp.time = Time.realtimeSinceStartup;
                m_ClickInfoTmp.screenPos = JerryUtil.GetClickPos();
            }
        }
        return m_ClickInfoTmp;
    }

    private void JudgeClick()
    {
        if (m_ClickUpInfo.col != m_ClickDownInfo.col
            || m_ClickUpInfo.col == null)
        {
            //Debug.LogWarning("11");
            return;
        }
        if (m_ClickUpInfo.time < m_ClickDownInfo.time
            || m_ClickUpInfo.time - m_ClickDownInfo.time > 0.3f)
        {
            //Debug.LogWarning("12");
            return;
        }
        if (!Util.Vector3Equal(m_ClickUpInfo.pos, m_ClickDownInfo.pos, 0.01f))
        {
            //Debug.LogWarning("13");
            return;
        }
        if (!Util.Vector3Equal(m_ClickUpInfo.screenPos, m_ClickDownInfo.screenPos, 2f))
        {
            //Debug.LogWarning("13");
            return;
        }
        if (m_LastClickInfo.col == m_ClickUpInfo.col
            && m_ClickUpInfo.time - m_LastClickInfo.time < 0.5f)
        {
            //Debug.LogWarning("14 " + (m_LastClickInfo.col == m_ClickUpInfo.col) + " " + (m_ClickUpInfo.time - m_LastClickInfo.time));
            return;
        }
        m_LastClickInfo.Init(m_ClickUpInfo);
        //Debug.LogWarning("Click " + m_ClickUpInfo.ToString() + " downPos=" + MapUtil.Vector3String(m_ClickDownInfo.pos));
        JerryEventMgr.DispatchEvent(Enum_Event.Click3DObj.ToString(), new object[] { m_ClickUpInfo });
    }

    #endregion 3D点击

    #region 事件

    private void EventClick3DObj(object[] args)
    {
        if (args == null || args.Length != 1)
        {
            return;
        }
        if (MapUtil.m_SelectId == 0
            || MapUtil.m_SelectOK)
        {
            return;
        }

        RayClickInfo info = (RayClickInfo)args[0];
        if (MapUtil.IsWallLayer(info.col.gameObject.layer))
        {
            RayClickPos fp = new RayClickPos();
            fp.pos = info.pos;
            fp.wallType = MapUtil.WallLayer2WallEnum(info.col.gameObject.layer);

            //JerryDrawer.Draw<DrawerElementCube>()
            //    .SetColor(Color.black)
            //    .SetLife(3f)
            //    .SetPos(fp.pos)
            //    .SetSize(Vector3.one)
            //    .SetWire(false)
            //    .SetSizeFactor(0.2f);

            JerryEventMgr.DispatchEvent(Enum_Event.SetFurn2Pos.ToString(), new object[] { fp });
        }
    }

    #endregion 事件
}