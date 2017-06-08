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
        DrawWall(m_LeftSideWallStartPos, m_LeftSideWallSize, Color.red);
        DrawWall(m_RightSideWallStartPos, m_RightSideWallSize, Color.red);
        DrawWall(m_WallStartPos, m_WallSize, Color.black);
#endif
    }

    void Update()
    {
        ClickPlaceObj();
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();
        if (GUILayout.Button("Set", GUILayout.MinHeight(40), GUILayout.MinWidth(80)))
        {
            if (MapUtil.m_SelectId == 0
                || MapUtil.m_SelectOK)
            {
                Debug.LogWarning("空");
                return;
            }
            JerryEventMgr.DispatchEvent(Enum_Event.SetOne.ToString(), new object[] { MapUtil.m_SelectId });
        }
        //if (GUILayout.Button("Init", GUILayout.MinHeight(40), GUILayout.MinWidth(80)))
        //{
        //    Drag[] drags = this.transform.parent.GetComponentsInChildren<Drag>();
        //    foreach (Drag d in drags)
        //    {
        //        d.Init();
        //    }
        //}
        GUILayout.EndVertical();
    }

    #region 点击放置

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
            m_Ray = Camera.main.ScreenPointToRay(Util.GetClickPos());

            if (Physics.Raycast(m_Ray, out m_HitInfo, 100))
            {
                if (m_HitInfo.collider == null
                    || m_HitInfo.collider.gameObject == null)
                {
                    return;
                }
                string layerName = LayerMask.LayerToName(m_HitInfo.collider.gameObject.layer);
                //Debug.LogWarning(layerName + " " + m_HitInfo.point);
                if (layerName == "Wall")
                {
                    JerryEventMgr.DispatchEvent(Enum_Event.Place2Pos.ToString(), new object[] { m_HitInfo.point });
                }
            }
        }
    }

    #endregion 点击放置

#if UNITY_EDITOR

    private void DrawWall(Vector3 startPos, Vector3 size, Color color)
    {
        if (size.z == 0)
        {
            for (int i = 0; i <= size.x; i++)//竖线
            {
                DrawLine(startPos + new Vector3(i * MapUtil.m_MapGridUnityLen, 0, 0)
                    , startPos + new Vector3(i * MapUtil.m_MapGridUnityLen, size.y * MapUtil.m_MapGridUnityLen, 0)
                    , color);
            }

            for (int i = 0; i <= size.y; i++)//横线
            {
                DrawLine(startPos + new Vector3(0, i * MapUtil.m_MapGridUnityLen, 0)
                    , startPos + new Vector3(size.x * MapUtil.m_MapGridUnityLen, i * MapUtil.m_MapGridUnityLen, 0)
                    , color);
            }
        }
        else if (size.x == 0)
        {
            for (int i = 0; i <= size.z; i++)//竖线
            {
                DrawLine(startPos + new Vector3(0, 0, i * MapUtil.m_MapGridUnityLen)
                    , startPos + new Vector3(0, size.y * MapUtil.m_MapGridUnityLen, i * MapUtil.m_MapGridUnityLen)
                    , color);
            }

            for (int i = 0; i <= size.y; i++)//横线
            {
                DrawLine(startPos + new Vector3(0, i * MapUtil.m_MapGridUnityLen, 0)
                    , startPos + new Vector3(0, i * MapUtil.m_MapGridUnityLen, size.z * MapUtil.m_MapGridUnityLen)
                    , color);
            }
        }
    }

    private void DrawLine(Vector3 from, Vector3 to, Color color)
    {
        JerryDrawer.Draw<DrawerElementPath>()
                    .SetPoints(from, to)
                    .SetColor(color);
    }

#endif
}
