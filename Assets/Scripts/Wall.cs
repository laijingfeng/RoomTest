using UnityEngine;
using Jerry;

public class Wall : MonoBehaviour
{
    public Vector3 m_WallStartPos;
    /// <summary>
    /// 宽*高=x*y
    /// </summary>
    public Vector2 m_WallSize;
    public float m_MapGridUnityLen;
    public bool m_CanClickPlaceObj = false;

    private Ray m_Ray;
    private RaycastHit m_HitInfo;

    void Awake()
    {
        MapUtil.m_MapStartPos = m_WallStartPos;
        MapUtil.m_MapSize = m_WallSize;
        MapUtil.m_MapGridUnityLen = m_MapGridUnityLen;
        MapUtil.Init();

#if UNITY_EDITOR
        DrawLeftSideWall();
        DrawWall();
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
        if (GUILayout.Button("Init", GUILayout.MinHeight(40), GUILayout.MinWidth(80)))
        {
            Drag[] drags = this.transform.parent.GetComponentsInChildren<Drag>();
            foreach (Drag d in drags)
            {
                d.Init();
            }
        }
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
                Debug.LogWarning(layerName + " " + m_HitInfo.point);
                if (layerName == "Wall")
                {
                    JerryEventMgr.DispatchEvent(Enum_Event.Place2Pos.ToString(), new object[] { m_HitInfo.point });
                }
            }
        }
    }

    #endregion 点击放置

#if UNITY_EDITOR
    private void DrawWall()
    {
        for (int i = 0; i <= m_WallSize.x; i++)//竖线
        {
            JerryDrawer.Draw<DrawerElementPath>()
                .SetPoints(m_WallStartPos + new Vector3(i * MapUtil.m_MapGridUnityLen, 0, 0), m_WallStartPos + new Vector3(i * MapUtil.m_MapGridUnityLen, m_WallSize.y * MapUtil.m_MapGridUnityLen, 0))
                .SetColor(Color.black);
        }

        for (int j = 0; j <= m_WallSize.y; j++)//横线
        {
            JerryDrawer.Draw<DrawerElementPath>()
                .SetPoints(m_WallStartPos + new Vector3(0, j * MapUtil.m_MapGridUnityLen, 0), m_WallStartPos + new Vector3(m_WallSize.x * MapUtil.m_MapGridUnityLen, j * MapUtil.m_MapGridUnityLen, 0))
                .SetColor(Color.black);
        }
    }

    private void DrawLeftSideWall()
    {
        for (int i = 0; i <= m_WallSize.x; i++)//竖线
        {
            JerryDrawer.Draw<DrawerElementPath>()
                .SetPoints(m_WallStartPos + new Vector3(i * MapUtil.m_MapGridUnityLen, 0, 0), m_WallStartPos + new Vector3(i * MapUtil.m_MapGridUnityLen, m_WallSize.y * MapUtil.m_MapGridUnityLen, 0))
                .SetColor(Color.red);
        }

        for (int j = 0; j <= m_WallSize.y; j++)//横线
        {
            JerryDrawer.Draw<DrawerElementPath>()
                .SetPoints(m_WallStartPos + new Vector3(0, j * MapUtil.m_MapGridUnityLen, 0), m_WallStartPos + new Vector3(m_WallSize.x * MapUtil.m_MapGridUnityLen, j * MapUtil.m_MapGridUnityLen, 0))
                .SetColor(Color.red);
        }
    }
#endif
}
