using UnityEngine;
using Jerry;

public class Wall : MonoBehaviour
{
    public Vector3 m_WallStartPos;
    /// <summary>
    /// 宽*高=x*y
    /// </summary>
    public Vector3 m_WallSize;

    public Vector3 m_LeftSideWallStartPos;
    /// <summary>
    /// 宽*高=x*y
    /// </summary>
    public Vector3 m_LeftSideWallSize;

    public Vector3 m_RightSideWallStartPos;
    /// <summary>
    /// 宽*高=x*y
    /// </summary>
    public Vector3 m_RightSideWallSize;

    public float m_MapGridUnityLen;
    public bool m_CanClickPlaceObj = true;

    private Ray m_Ray;
    private RaycastHit m_HitInfo;

    private static Wall m_Inst;
    public static Wall Inst
    {
        get
        {
            return m_Inst;
        }
    }

    void Awake()
    {
        m_Inst = this;

        MapUtil.m_MapGridUnityLen = m_MapGridUnityLen;

        MapUtil.m_Wall.m_StartPos = m_WallStartPos;
        MapUtil.m_Wall.m_Size = m_WallSize;

        MapUtil.m_LeftSideWall.m_StartPos = m_LeftSideWallStartPos;
        MapUtil.m_LeftSideWall.m_Size = m_LeftSideWallSize;

        MapUtil.m_RightSideWall.m_StartPos = m_RightSideWallStartPos;
        MapUtil.m_RightSideWall.m_Size = m_RightSideWallSize;

        MapUtil.Init();

#if UNITY_EDITOR
        JerryDrawer.Draw<DrawerElementGrid>()
                    .SetMinPos(m_LeftSideWallStartPos)
                    .SetGridSize(new Vector3(0, 0.5f, 0.5f))
                    .SetSize(m_LeftSideWallSize)
                    .SetColor(Color.red);

        JerryDrawer.Draw<DrawerElementGrid>()
                    .SetMinPos(m_RightSideWallStartPos)
                    .SetGridSize(new Vector3(0, 0.5f, 0.5f))
                    .SetSize(m_RightSideWallSize)
                    .SetColor(Color.red);

        JerryDrawer.Draw<DrawerElementGrid>()
                    .SetMinPos(m_WallStartPos)
                    .SetGridSize(new Vector3(0.5f, 0.5f, 0))
                    .SetSize(m_WallSize)
                    .SetColor(Color.black);
#endif
    }

    void Update()
    {
        ClickPlaceObj();
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("放置选中", GUILayout.MinHeight(60), GUILayout.MinWidth(100)))
        {
            if (MapUtil.m_SelectId == 0
                || MapUtil.m_SelectOK)
            {
                Debug.LogWarning("没有选中家具");
                return;
            }
            JerryEventMgr.DispatchEvent(Enum_Event.SetOne.ToString(), new object[] { MapUtil.m_SelectId });
        }
        if (GUILayout.Button("回收选中", GUILayout.MinHeight(60), GUILayout.MinWidth(100)))
        {
            if (MapUtil.m_SelectId == 0
                || MapUtil.m_SelectOK)
            {
                Debug.LogWarning("没有选中家具");
                return;
            }
            JerryEventMgr.DispatchEvent(Enum_Event.Back2Package.ToString(), new object[] { MapUtil.m_SelectId });
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
        if (!m_CanClickPlaceObj)
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

                    JerryEventMgr.DispatchEvent(Enum_Event.Place2Pos.ToString(), new object[] { fp });
                }
            }
        }
    }

    #endregion 点击放置
}
