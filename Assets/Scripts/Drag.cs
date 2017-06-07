using UnityEngine;
using System.Collections;
using Jerry;

public class Drag : MonoBehaviour
{
    /// <summary>
    /// 大小，Z轴是厚度，不是GridSize
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
        m_MinPos = MapUtil.m_MapStartPos + new Vector3(m_GridSize.x * MapUtil.m_MapGirdUnitySize.x / 2, m_GridSize.y * MapUtil.m_MapGirdUnitySize.y / 2, 0);
        m_MaxPos = MapUtil.m_MapStartPos
                + new Vector3(MapUtil.m_MapSize.x * MapUtil.m_MapGirdUnitySize.x, MapUtil.m_MapSize.y * MapUtil.m_MapGirdUnitySize.y, 0)
                - new Vector3(m_GridSize.x * MapUtil.m_MapGirdUnitySize.x / 2, m_GridSize.y * MapUtil.m_MapGirdUnitySize.y / 2, 0);

        if (m_OnFloor)
        {
            m_MaxPos.y = m_MinPos.y;
        }

        Debug.LogWarning(m_MinPos + " " + m_MaxPos);

        m_AdjustPar = Vector2.zero;
        if (((int)m_GridSize.x) % 2 == 0)
        {
            m_AdjustPar.x = MapUtil.m_MapGirdUnitySize.x;
        }
        else
        {
            m_AdjustPar.x = MapUtil.m_MapGirdUnitySize.x / 2;
        }
        if (((int)m_GridSize.y) % 2 == 0)
        {
            m_AdjustPar.y = MapUtil.m_MapGirdUnitySize.y;
        }
        else
        {
            m_AdjustPar.y = MapUtil.m_MapGirdUnitySize.y / 2;
        }

        JerryEventMgr.AddEvent(Enum_Event.SetOne.ToString(), EventSetOne);
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
                m_Pos.z = -m_GridSize.z / 2.0f - 0.5f;

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

                m_Pos = AdjustPos(m_Pos);

                m_Pos.x = Mathf.Clamp(m_Pos.x, m_MinPos.x, m_MaxPos.x);
                m_Pos.y = Mathf.Clamp(m_Pos.y, m_MinPos.y, m_MaxPos.y);

                transform.position = m_Pos;
                yield return new WaitForFixedUpdate();
            }
        }
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
            m_Pos.z = -m_GridSize.z / 2.0f;
            this.transform.position = m_Pos;

            Debug.LogWarning("设置OK");
        }
        else
        {
            Debug.LogWarning("重叠");
        }
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
}