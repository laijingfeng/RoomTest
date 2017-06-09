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
    private Vector3 m_LastPos;

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="wallType"></param>
    /// <param name="pos"></param>
    /// <param name="first">刚进入到这个面</param>
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

    private Vector3 m_ClickDownPos = Vector3.zero;
    private Vector3 m_ClickUpPos = Vector3.zero;

    void OnMouseUp()
    {
        if (m_Selected)
        {
            return;
        }

        m_ClickUpPos = JerryUtil.GetClickPos();
        if (Mathf.Abs(m_ClickUpPos.x - m_ClickDownPos.x) > 0.1f
            || Mathf.Abs(m_ClickUpPos.y - m_ClickDownPos.y) > 0.1f
            || Mathf.Abs(m_ClickUpPos.z - m_ClickDownPos.z) > 0.1f)
        {
            return;
        }

        if (MapUtil.m_SelectId != 0
            && !MapUtil.m_SelectOK)
        {
            if (MapUtil.m_SelectNew)
            {
                Debug.LogWarning("当前选中的还没放好");
                return;
            }
            JerryEventMgr.DispatchEvent(Enum_Event.BackOne.ToString(), new object[] { MapUtil.m_SelectId });
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

        MapUtil.m_SelectId = m_Id;
        MapUtil.m_SelectOK = false;
        MapUtil.m_SelectNew = m_InitData.isNew;

        m_InitData.isNew = false;
        this.gameObject.layer = LayerMask.NameToLayer("ActiveCube");
        m_Selected = true;

        MyShadow.Inst.SetSize(m_GridSize);
        MyShadow.Inst.SetVisible(true);
        MyShadow.Inst.SetPos(MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ2(m_GridSize, false, this.transform.position), this.transform.eulerAngles);
    }

    private Ray m_Ray;
    private RaycastHit m_HitInfo;

    IEnumerator OnMouseDown()
    {
        if (m_Selected == false)
        {
            m_ClickDownPos = JerryUtil.GetClickPos();
        }

        if (m_Selected == false)
        {
            yield break;
        }

        var camera = Camera.main;
        if (camera)
        {
            //Vector3 screenPosition = camera.WorldToScreenPoint(transform.position);
            //Vector3 mScreenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z);

            //Vector3 offset = transform.position - camera.ScreenToWorldPoint(mScreenPosition);

            //while (Input.GetMouseButton(0))
            //{
            //    mScreenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z);
            //    m_Pos = offset + camera.ScreenToWorldPoint(mScreenPosition);

            //    if (Mathf.Abs(m_Pos.x - m_LastPos.x) > 0.1f
            //        || Mathf.Abs(m_Pos.y - m_LastPos.y) > 0.1f
            //        || Mathf.Abs(m_Pos.z - m_LastPos.z) > 0.1f)
            //    {
            //        m_LastPos = m_Pos;
            //        Place2Pos(m_Pos);
            //    }
            //    yield return new WaitForFixedUpdate();
            //}

            while (Input.GetMouseButton(0))
            {
                m_Ray = Camera.main.ScreenPointToRay(Util.GetClickPos());

                if (Physics.Raycast(m_Ray, out m_HitInfo, 100,
                    JerryUtil.MakeLayerMask(JerryUtil.MakeLayerMask(false),
                    new string[]
                    {
                        Enum_Wall.Wall.ToString(),
                        Enum_Wall.LeftWall.ToString(),
                        Enum_Wall.RightWall.ToString(),
                    })))
                {
                    if (m_HitInfo.collider != null
                        && m_HitInfo.collider.gameObject != null)
                    {
                        FirstPos fp = new FirstPos();
                        fp.pos = m_HitInfo.point;
                        string layerName = LayerMask.LayerToName(m_HitInfo.collider.gameObject.layer);
                        switch (layerName)
                        {
                            case "Wall":
                                {
                                    fp.wallType = Enum_Wall.Wall;
                                }
                                break;
                            case "LeftWall":
                                {
                                    fp.wallType = Enum_Wall.LeftWall;
                                }
                                break;
                            case "RightWall":
                                {
                                    fp.wallType = Enum_Wall.RightWall;
                                }
                                break;
                        }

                        if (Mathf.Abs(fp.pos.x - m_LastPos.x) > 0.1f
                            || Mathf.Abs(fp.pos.y - m_LastPos.y) > 0.1f
                            || Mathf.Abs(fp.pos.z - m_LastPos.z) > 0.1f)
                        {
                            m_LastPos = fp.pos;

                            if (fp.wallType == m_InitData.m_CurWall)
                            {
                                Place2Pos(fp.pos);
                            }
                            else
                            {
                                Init(fp.wallType, fp.pos, false);
                            }
                        }
                    }
                }
                yield return new WaitForFixedUpdate();
            }
        }
    }

    private void JudgePosOutScreen()
    {
        Vector3 p = transform.position;
        Vector3 p1 = Vector3.zero, p2 = Vector3.zero;
        switch (m_InitData.m_CurWall)
        {
            case Enum_Wall.Wall:
                {
                    p1 = p - new Vector3(m_GridSize.x / 2 * MapUtil.m_MapGridUnityLen + MapUtil.m_MapGridUnityLen, 0, 0);
                    p2 = p + new Vector3(m_GridSize.x / 2 * MapUtil.m_MapGridUnityLen + MapUtil.m_MapGridUnityLen, 0, 0);
                }
                break;
            case Enum_Wall.LeftWall:
                {
                    p1 = p - new Vector3(m_GridSize.z / 2 * MapUtil.m_MapGridUnityLen + MapUtil.m_MapGridUnityLen, 0, m_GridSize.x / 2 * MapUtil.m_MapGridUnityLen);
                    p2 = p + new Vector3(m_GridSize.z / 2 * MapUtil.m_MapGridUnityLen, 0, m_GridSize.x / 2 * MapUtil.m_MapGridUnityLen);
                }
                break;
            case Enum_Wall.RightWall:
                {
                    p1 = p + new Vector3(-(m_GridSize.z / 2 * MapUtil.m_MapGridUnityLen), 0, m_GridSize.x / 2 * MapUtil.m_MapGridUnityLen);
                    p2 = p - new Vector3(-(m_GridSize.z / 2 * MapUtil.m_MapGridUnityLen + MapUtil.m_MapGridUnityLen), 0, m_GridSize.x / 2 * MapUtil.m_MapGridUnityLen);
                }
                break;
        }
        Vector3 sp = Camera.main.WorldToScreenPoint(p1);
        if (sp.x <= 0)
        {
            DragCamera.Inst.DoDrag(-10);
            return;
        }
        sp = Camera.main.WorldToScreenPoint(p2);
        if (sp.x >= Screen.width)
        {
            DragCamera.Inst.DoDrag(10);
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="first">当前面第一次设置位置</param>
    private void Place2Pos(Vector3 pos, bool first = false)
    {
        //Debug.LogWarning("one " + pos + " " + pos.z);
        MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ(m_GridSize, true, ref pos);
        //Debug.LogWarning("one11111 " + pos + "  " + pos.z);
        m_Pos = AdjustPos(pos);
        Enum_Wall changeType = Enum_Wall.None;

        //Debug.LogWarning(m_Pos.x + "  " + m_Pos.z + " sss " + m_InitData.m_MaxPos.x + " " + m_InitData.m_MaxPos.y + " " + m_InitData.m_MaxPos.z);

        if (m_InitData.m_CurWall == Enum_Wall.Wall)
        {
            m_Pos.x = Mathf.Clamp(m_Pos.x, m_InitData.m_MinPos.x, m_InitData.m_MaxPos.x);
            if (m_Pos.x <= m_InitData.m_MinPos.x)
            {
                if (first)
                {
                    m_Pos.x = m_InitData.m_MinPos.x + MapUtil.m_MapGridUnityLen;
                }
                else
                {
                    changeType = Enum_Wall.LeftWall;
                }
            }
            else if (m_Pos.x >= m_InitData.m_MaxPos.x)
            {
                if (first)
                {
                    m_Pos.x = m_InitData.m_MaxPos.x - MapUtil.m_MapGridUnityLen;
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
            if (m_Pos.z >= m_InitData.m_MaxPos.z)
            {
                if (first)
                {
                    m_Pos.z = m_InitData.m_MaxPos.z - MapUtil.m_MapGridUnityLen;
                }
                else
                {
                    changeType = Enum_Wall.Wall;
                }
            }
        }
        m_Pos.y = Mathf.Clamp(m_Pos.y, m_InitData.m_MinPos.y, m_InitData.m_MaxPos.y);

        if (changeType != Enum_Wall.None)
        {
            MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ(m_GridSize, false, ref m_Pos);
            transform.position = m_Pos;

            MyShadow.Inst.SetPos(MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ2(m_GridSize, false, this.transform.position), this.transform.eulerAngles);
            //Debug.LogWarning("ddd " + changeType + " " + m_Pos.x + " " + m_Pos.y + " " + m_Pos.z);

            Init(changeType, m_Pos, true);
        }
        else
        {
            //Debug.LogWarning("xxxxxxxxxxxxxx " + m_Pos.z);
            transform.position = m_Pos;

            MyShadow.Inst.SetPos(MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ2(m_GridSize, false, this.transform.position), this.transform.eulerAngles);
        }

        JudgePosOutScreen();
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
                this.gameObject.layer = LayerMask.NameToLayer("Cube");
                m_Pos = m_InitData.m_LastPos;
                MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ(m_GridSize, false, ref m_Pos);
                this.transform.position = m_Pos;

                MyShadow.Inst.SetVisible(false);
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
            this.gameObject.layer = LayerMask.NameToLayer("Cube");
            m_Pos = this.transform.position;
            MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ(m_GridSize, false, ref m_Pos);
            this.transform.position = m_Pos;

            MyShadow.Inst.SetVisible(false);
            Debug.LogWarning("设置OK");
        }
        else
        {
            Debug.LogWarning("重叠");
        }
    }

    #endregion 事件
}