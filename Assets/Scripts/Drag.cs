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
    private int m_Id;

    /// <summary>
    /// 选中
    /// </summary>
    private bool m_Selected = false;

    private Vector3 m_Pos;

    private DragInitData m_InitData = null;

    void Awake()
    {
        m_Id = Util.IDGenerator(m_Id);
        m_InitData = new DragInitData();
        m_InitData.isNew = true;

        JerryEventMgr.AddEvent(Enum_Event.SetOne.ToString(), EventSetOne);
        JerryEventMgr.AddEvent(Enum_Event.Place2Pos.ToString(), EventPlace2Pos); 
        JerryEventMgr.AddEvent(Enum_Event.BackOne.ToString(), EventBackOne);
    }

    public void Init(Enum_Wall wallType, Vector3 pos, bool first)
    {
        m_InitData = MapUtil.InitDrag(m_GridSize, m_OnFloor, m_InitData, wallType);

        switch (m_InitData.m_CurWall)
        {
            case Enum_Wall.LeftWall:
                {
                    this.transform.eulerAngles = new Vector3(0, -90, 0);
                }
                break;
            case Enum_Wall.RightWall:
                {
                    this.transform.eulerAngles = new Vector3(0, 90, 0);
                }
                break;
            case Enum_Wall.Wall:
                {
                    this.transform.eulerAngles = Vector3.zero;
                }
                break;
        }
        Place2Pos(pos, first);
    }

    IEnumerator OnMouseDown()
    {
        var camera = Camera.main;
        if (camera)
        {
            if (m_Selected == false)
            {
                if (MapUtil.m_SelectId != 0
                    && !MapUtil.m_SelectOK)
                {
                    if (m_InitData.isNew)
                    {
                        Debug.LogWarning("当前选中的还没放好");
                        yield break;
                    }
                    else
                    {
                        JerryEventMgr.DispatchEvent(Enum_Event.BackOne.ToString(), new object[] { MapUtil.m_SelectId });
                    }
                }

                if (m_InitData.isNew)
                {
                    FirstPos fp = MapUtil.GetFirstPos();
                    Init(fp.wallType, fp.pos, true);
                }
                else if (m_InitData.m_CurWall != Enum_Wall.None)
                {
                    //先浮起来，再记录，保持回退时一致性
                    m_Pos = this.transform.position;
                    MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ(m_GridSize, true, ref m_Pos);
                    this.transform.position = m_Pos;

                    m_InitData.m_LastPos = m_Pos;
                    m_InitData.m_LastWall = m_InitData.m_CurWall;

                    MapUtil.GetMap(m_InitData.m_CurWall).CleanOne(this.transform.position, m_GridSize);
                }

                m_InitData.isNew = false;
                MapUtil.m_SelectId = m_Id;
                m_Selected = true;
                MapUtil.m_SelectOK = false;
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

    private void Place2Pos(Vector3 pos, bool first = false)
    {
        MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ(m_GridSize, true, ref pos);
        m_Pos = AdjustPos(pos);
        Enum_Wall changeType = Enum_Wall.None;

        if (m_InitData.m_CurWall == Enum_Wall.Wall)
        {
            m_Pos.x = Mathf.Clamp(m_Pos.x, m_InitData.m_MinPos.x, m_InitData.m_MaxPos.x);
            if (m_Pos.x == m_InitData.m_MinPos.x)
            {
                if (first)
                {
                    m_Pos.x += MapUtil.m_MapGridUnityLen;
                }
                else
                {
                    changeType = Enum_Wall.LeftWall;
                }
            }
            else if (m_Pos.x == m_InitData.m_MaxPos.x)
            {
                if (first)
                {
                    m_Pos.x -= MapUtil.m_MapGridUnityLen;
                }
                else
                {
                    changeType = Enum_Wall.RightWall;
                }
            }
        }
        else
        {
            m_Pos.z = Mathf.Clamp(m_Pos.z, m_InitData.m_MinPos.z, m_InitData.m_MaxPos.z);
            if (m_Pos.z == m_InitData.m_MaxPos.z)
            {
                if (first)
                {
                    m_Pos.z -= MapUtil.m_MapGridUnityLen;
                }
                else
                {
                    changeType = Enum_Wall.Wall;
                }
            }
        }
        m_Pos.y = Mathf.Clamp(m_Pos.y, m_InitData.m_MinPos.y, m_InitData.m_MaxPos.y);
        transform.position = m_Pos;

        if (changeType != Enum_Wall.None)
        {
            Init(changeType, m_Pos, false);
        }
    }

    private Vector3 AdjustPos(Vector3 pos)
    {
        Vector3 v = new Vector3((int)(pos.x / m_InitData.m_AdjustPar.x), (int)(pos.y / m_InitData.m_AdjustPar.y), (int)(pos.z / m_InitData.m_AdjustPar.z));
        if (m_InitData.m_CurWall == Enum_Wall.Wall)
        {
            pos.x = MyClamp(pos.x, m_InitData.m_AdjustPar.x * v.x, m_InitData.m_AdjustPar.x * (v.x + 1 * Mathf.Sign(v.x)));
        }
        else
        {
            pos.z = MyClamp(pos.z, m_InitData.m_AdjustPar.z * v.z, m_InitData.m_AdjustPar.z * (v.z + 1 * Mathf.Sign(v.z)));
        }
        pos.y = MyClamp(pos.y, m_InitData.m_AdjustPar.y * v.y, m_InitData.m_AdjustPar.y * (v.y + 1 * Mathf.Sign(v.y)));
        return pos;
    }

    private float MyClamp(float x, float v1, float v2)
    {
        return Mathf.Abs(v1 - x) > Mathf.Abs(v2 - x) ? v2 : v1;
    }

    #region 事件

    private void EventPlace2Pos(object[] args)
    {
        if (m_Selected == false)
        {
            return;
        }
        FirstPos fp = (FirstPos)args[0];
        if (fp.wallType == m_InitData.m_CurWall)
        {
            Place2Pos(fp.pos);
        }
        else
        {
            Init(fp.wallType, fp.pos, false);
        }
    }

    private void EventBackOne(object[] args)
    {
        int id = (int)args[0];
        if (id != m_Id)
        {
            return;
        }

        if (m_InitData.m_LastWall != Enum_Wall.None)
        {
            if (MapUtil.GetMap(m_InitData.m_LastWall).SetOne(m_InitData.m_LastPos, m_GridSize))
            {
                m_InitData.m_CurWall = m_InitData.m_LastWall;
                
                m_Selected = false;
                m_Pos = m_InitData.m_LastPos;
                MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ(m_GridSize, false, ref m_Pos);
                this.transform.position = m_Pos;
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

        if (MapUtil.GetMap(m_InitData.m_CurWall).SetOne(this.transform.position, m_GridSize))
        {
            MapUtil.m_SelectOK = true;
            MapUtil.m_SelectId = 0;

            m_Selected = false;
            m_Pos = this.transform.position;
            MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ(m_GridSize, false, ref m_Pos);
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