using UnityEngine;
using System.Collections;
using Jerry;

public class Drag : MonoBehaviour
{
    /// <summary>
    /// 大小
    /// </summary>
    public Vector3 m_GridSize = new Vector3(2, 2, 2);
    /// <summary>
    /// 贴地
    /// </summary>
    public bool m_OnFloor = false;
    /// <summary>
    /// id
    /// </summary>
    public int m_Id;

    /// <summary>
    /// 选中
    /// </summary>
    private bool m_Selected = false;

    /// <summary>
    /// 曾经放过
    /// </summary>
    private bool m_HadSet = false;

    private Vector3 m_Pos;

    private Vector2 m_MinPos;
    private Vector2 m_MaxPos;
    private Vector3 m_AdjustPar;

    [ContextMenu("DoInit")]
    public void Init()
    {
        m_MinPos = MapUtil.m_MapStartPos + new Vector3(m_GridSize.x * MapUtil.m_MapGridUnityLen / 2, m_GridSize.y * MapUtil.m_MapGridUnityLen / 2, 0);
        m_MaxPos = MapUtil.m_MapStartPos
                + new Vector3(MapUtil.m_MapSize.x * MapUtil.m_MapGridUnityLen, MapUtil.m_MapSize.y * MapUtil.m_MapGridUnityLen, 0)
                - new Vector3(m_GridSize.x * MapUtil.m_MapGridUnityLen / 2, m_GridSize.y * MapUtil.m_MapGridUnityLen / 2, 0);

        if (m_OnFloor)
        {
            m_MaxPos.y = m_MinPos.y;
        }

        Debug.LogWarning(m_MinPos + " " + m_MaxPos);

        m_AdjustPar = Vector2.zero;
        if (((int)m_GridSize.x) % 2 == 0)
        {
            m_AdjustPar.x = MapUtil.m_MapGridUnityLen;
        }
        else
        {
            m_AdjustPar.x = MapUtil.m_MapGridUnityLen / 2;
        }
        if (((int)m_GridSize.y) % 2 == 0)
        {
            m_AdjustPar.y = MapUtil.m_MapGridUnityLen;
        }
        else
        {
            m_AdjustPar.y = MapUtil.m_MapGridUnityLen / 2;
        }

        JerryEventMgr.AddEvent(Enum_Event.SetOne.ToString(), EventSetOne);
        JerryEventMgr.AddEvent(Enum_Event.Place2Pos.ToString(), EventPlace2Pos);
    }

    IEnumerator OnMouseDown()
    {
        var camera = Camera.main;
        if (camera)
        {
            if (m_Selected == false)
            {
                if (!MapUtil.m_SelectOK)
                {
                    Debug.LogWarning("当前选中的还没放好");
                    yield break;
                }

                MapUtil.m_SelectId = m_Id;

                m_Pos = this.transform.position;
                m_Pos = AdjustZ(m_Pos, true);
                this.transform.position = m_Pos;

                m_Selected = true;
                MapUtil.m_SelectOK = false;

                if (m_HadSet)
                {
                    MapUtil.CleanOne(this.transform.position, m_GridSize);
                }
            }

            Vector3 screenPosition = camera.WorldToScreenPoint(transform.position);
            Vector3 mScreenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z);

            Vector3 offset = transform.position - camera.ScreenToWorldPoint(mScreenPosition);

            while (Input.GetMouseButton(0))
            {
                mScreenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z);
                m_Pos = offset + camera.ScreenToWorldPoint(mScreenPosition);

                Place2Pos(m_Pos);

                yield return new WaitForFixedUpdate();
            }
        }
    }

    private void Place2Pos(Vector3 pos)
    {
        pos = AdjustZ(pos, true);
        m_Pos = AdjustPos(pos);
        m_Pos.x = Mathf.Clamp(m_Pos.x, m_MinPos.x, m_MaxPos.x);
        m_Pos.y = Mathf.Clamp(m_Pos.y, m_MinPos.y, m_MaxPos.y);
        transform.position = m_Pos;
    }

    private Vector3 AdjustPos(Vector3 pos)
    {
        Vector3 v = new Vector3((int)(pos.x / m_AdjustPar.x), (int)(pos.y / m_AdjustPar.y));
        pos.x = MyClamp(pos.x, m_AdjustPar.x * v.x, m_AdjustPar.x * (v.x + 1 * Mathf.Sign(v.x)));
        pos.y = MyClamp(pos.y, m_AdjustPar.y * v.y, m_AdjustPar.y * (v.y + 1 * Mathf.Sign(v.y)));
        return pos;
    }


    private float MyClamp(float x, float v1, float v2)
    {
        return Mathf.Abs(v1 - x) > Mathf.Abs(v2 - x) ? v2 : v1;
    }

    private Vector3 AdjustZ(Vector3 pos, bool floating)
    {
        pos.z = MapUtil.m_MapStartPos.z - m_GridSize.z * MapUtil.m_MapGridUnityLen / 2.0f;
        if (floating)
        {
            pos.z -= 0.3f;
        }
        return pos;
    }

    #region 事件

    private void EventPlace2Pos(object[] args)
    {
        if (m_Selected == false)
        {
            return;
        }
        Vector3 pos = (Vector3)args[0];
        Place2Pos(pos);
    }

    private void EventSetOne(object[] args)
    {
        int id = (int)args[0];
        if (id != m_Id)
        {
            return;
        }

        if (MapUtil.SetOne(this.transform.position, m_GridSize))
        {
            MapUtil.m_SelectOK = true;
            MapUtil.m_SelectId = 0;

            m_Selected = false;
            m_HadSet = true;

            m_Pos = this.transform.position;
            m_Pos = AdjustZ(m_Pos, false);
            this.transform.position = m_Pos;

            Debug.LogWarning("设置OK");
        }
        else
        {
            Debug.LogWarning("重叠");
        }
    }

    #endregion 事件
}