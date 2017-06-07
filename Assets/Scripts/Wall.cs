using UnityEngine;
using Jerry;

public class Wall : MonoBehaviour
{
    public Vector3 m_MapStartPos;
    /// <summary>
    /// 宽*高=x*y
    /// </summary>
    public Vector2 m_MapSize;
    public Vector2 m_MapGirdUnitySize;

    void Awake()
    {
        MapUtil.m_MapStartPos = m_MapStartPos;
        MapUtil.m_MapSize = m_MapSize;
        MapUtil.m_MapGirdUnitySize = m_MapGirdUnitySize;
        MapUtil.Init();

        DrawMap();
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

    private void DrawMap()
    {
        for (int i = 0; i <= m_MapSize.x; i++)//竖线
        {
            JerryDrawer.Draw<DrawerElementPath>()
                .SetPoints(m_MapStartPos + new Vector3(i * m_MapGirdUnitySize.x, 0, 0), m_MapStartPos + new Vector3(i * m_MapGirdUnitySize.x, m_MapSize.y * m_MapGirdUnitySize.y, 0))
                .SetColor(Color.black);
        }

        for (int j = 0; j <= m_MapSize.y; j++)//横线
        {
            JerryDrawer.Draw<DrawerElementPath>()
                .SetPoints(m_MapStartPos + new Vector3(0, j * m_MapGirdUnitySize.y, 0), m_MapStartPos + new Vector3(m_MapSize.x * m_MapGirdUnitySize.x, j * m_MapGirdUnitySize.y, 0))
                .SetColor(Color.black);
        }
    }
}
