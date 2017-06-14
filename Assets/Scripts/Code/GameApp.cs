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

        MapUtil.m_MapGridUnityLen = GameApp.Inst.m_MapGridUnityLen;

        MapUtil.m_Wall.m_StartPos = m_WallStartPos;
        MapUtil.m_Wall.m_Size = m_WallSize;

        MapUtil.m_LeftSideWall.m_StartPos = m_LeftSideWallStartPos;
        MapUtil.m_LeftSideWall.m_Size = m_LeftSideWallSize;

        MapUtil.m_RightSideWall.m_StartPos = m_RightSideWallStartPos;
        MapUtil.m_RightSideWall.m_Size = m_RightSideWallSize;

        MapUtil.m_FloorWall.m_StartPos = m_FloorWallStartPos;
        MapUtil.m_FloorWall.m_Size = m_FloorWallSize;

        MapUtil.Init();

        if (m_DrawGrid)
        {
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

        CreateHouse(0, 0);
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

    private bool m_IsUpDowning = false;
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

            Debug.LogWarning(" " + MapUtil.m_SelectId + " " + MapUtil.m_SelectNew + " " + MapUtil.m_SelectOK);

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

        if (GUILayout.Button("保存方案", m_GUIOpt1))
        {
            if (UpDowning)
            {
                return;
            }

            JerryEventMgr.DispatchEvent(Enum_Event.SaveCurHouseData.ToString());
        }

        if (GUILayout.Button("随机一个", m_GUIOpt1))
        {
            if (UpDowning)
            {
                return;
            }

            if (!m_EditorMode)
            {
                UI_Tip.Inst.ShowTip("请先进入[编辑模式]");
                return;
            }
            houses[CurNodeIdx].AddOneFurniture();
        }

        if (GUILayout.Button(string.Format("上楼({0})", curFloor), m_GUIOpt1))
        {
            if (UpDowning)
            {
                return;
            }

            if (m_EditorMode)
            {
                UI_Tip.Inst.ShowTip("请先退出[编辑模式]");
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

            if (m_EditorMode)
            {
                UI_Tip.Inst.ShowTip("请先退出[编辑模式]");
                return;
            }
            ToFloor(curFloor - 1);
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
}