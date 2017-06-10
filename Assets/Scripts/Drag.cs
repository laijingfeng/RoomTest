using UnityEngine;
using System.Collections;
using Jerry;

public class Drag : MonoBehaviour
{
    /// <summary>
    /// 大小
    /// </summary>
    public MapUtil.IVector3 m_GridSize = new MapUtil.IVector3(1, 1, 1);
    
    public MapUtil.SetType m_SetType = MapUtil.SetType.Wall;

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

    private Renderer m_Render;

    private DragInitData m_InitData = null;

    void Awake()
    {
        m_Render = this.GetComponent<Renderer>();
        this.gameObject.layer = LayerMask.NameToLayer(Enum_Layer.Cube.ToString());

        m_Id = Util.IDGenerator(m_Id);
        m_InitData = new DragInitData();
        m_InitData.isNew = true;

        JerryEventMgr.AddEvent(Enum_Event.SetOne.ToString(), EventSetOne);
        JerryEventMgr.AddEvent(Enum_Event.Place2Pos.ToString(), EventPlace2Pos);
        JerryEventMgr.AddEvent(Enum_Event.BackOne.ToString(), EventBackOne);
        JerryEventMgr.AddEvent(Enum_Event.Back2Package.ToString(), EventBack2Package);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="wallType"></param>
    /// <param name="pos"></param>
    /// <param name="first">刚进入到这个面</param>
    public void Init(Enum_Layer wallType, Vector3 pos, bool first, bool fromClick = false)
    {
        m_InitData = MapUtil.InitDrag(m_GridSize, m_SetType, m_InitData, wallType);

        switch (m_InitData.m_CurWall)
        {
            case Enum_Layer.LeftWall:
                {
                    this.transform.eulerAngles = new Vector3(0, -90, 0);
                }
                break;
            case Enum_Layer.RightWall:
                {
                    this.transform.eulerAngles = new Vector3(0, 90, 0);
                }
                break;
            case Enum_Layer.Wall:
            case Enum_Layer.FloorWall:
                {
                    this.transform.eulerAngles = Vector3.zero;
                }
                break;
        }
        Place2Pos(pos, first, fromClick);
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
        if (!Util.Vector3Equal(m_ClickUpPos, m_ClickDownPos))
        {
            return;
        }

        SelectSelf();
    }

    /// <summary>
    /// 选中
    /// </summary>
    private void SelectSelf()
    {
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
            FirstPos fp = MapUtil.GetFirstPos(m_SetType);
            Init(fp.wallType, fp.pos, true);
        }
        else if (m_InitData.m_CurWall != Enum_Layer.None)
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
        this.gameObject.layer = LayerMask.NameToLayer(Enum_Layer.ActiveCube.ToString());
        m_Selected = true;

        bool canSet = MapUtil.GetMap(m_InitData.m_CurWall).JudgeSet(this.transform.position, m_GridSize);

        SetOutLineVisible(true);
        SetOutLineColor(canSet ? Color.green : Color.red);
        MyShadow.Inst.SetSize(m_GridSize.ToVector3(), m_SetType);
        MyShadow.Inst.SetVisible(true);
        MyShadow.Inst.SetColor(canSet ? Color.green : Color.red);
        MyShadow.Inst.SetPos(MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ2(this.transform.position), this.transform.eulerAngles);
    }

    private Vector3 GetTmp()
    {
        Vector3 tmp = this.transform.position;
        switch (m_InitData.m_CurWall)
        {
            case Enum_Layer.FloorWall:
                {
                    tmp.y -= m_GridSize.y / 2.0f * MapUtil.m_MapGridUnityLen + MapUtil.m_AdjustZVal;
                }
                break;
            case Enum_Layer.Wall:
                {
                    tmp.z += m_GridSize.z / 2.0f * MapUtil.m_MapGridUnityLen + MapUtil.m_AdjustZVal;
                }
                break;
            case Enum_Layer.LeftWall:
                {
                    tmp.x -= m_GridSize.x / 2.0f * MapUtil.m_MapGridUnityLen + MapUtil.m_AdjustZVal;
                }
                break;
            case Enum_Layer.RightWall:
                {
                    tmp.x += m_GridSize.x / 2.0f * MapUtil.m_MapGridUnityLen + MapUtil.m_AdjustZVal;
                }
                break;
        }
        return tmp;
    }

    private Ray m_Ray;
    private RaycastHit m_HitInfo;

    IEnumerator OnMouseDown()
    {
        if (m_Selected == false)
        {
            m_ClickDownPos = JerryUtil.GetClickPos();
        }

        //下面是拖拽
        if (m_Selected == false
            || Wall.Inst.m_CtrType == Wall.CtrObjType.OnlyClick)
        {
            yield break;
        }

        var camera = Camera.main;
        if (camera)
        {
            while (Input.GetMouseButton(0))
            {
                m_Ray = Camera.main.ScreenPointToRay(JerryUtil.GetClickPos());

                if (Physics.Raycast(m_Ray, out m_HitInfo, 100,
                    JerryUtil.MakeLayerMask(JerryUtil.MakeLayerMask(false),
                    MapUtil.GetWallLayerNames())))
                {
                    if (m_HitInfo.collider != null
                        && m_HitInfo.collider.gameObject != null)
                    {
                        FirstPos fp = new FirstPos();
                        fp.pos = m_HitInfo.point;
                        fp.wallType = MapUtil.WallLayer2Enum(m_HitInfo.collider.gameObject.layer);

                        //Debug.LogWarning(fp.wallType + " xxx " + m_InitData.m_CurWall);

                        if (!Util.Vector3Equal(fp.pos, m_LastPos))
                        {
                            m_LastPos = fp.pos;

                            if (fp.wallType == m_InitData.m_CurWall)
                            {
                                Place2Pos(fp.pos);
                            }
                            else
                            {
                                if (fp.wallType != Enum_Layer.FloorWall
                                    && m_SetType != MapUtil.SetType.Floor)
                                {
                                    Init(fp.wallType, fp.pos, true);
                                }
                            }
                        }
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }
    }

    private void JudgePosOutScreen()
    {
        if (JerryUtil.GetClickPos().x < 50)
        {
            DragCamera.Inst.DoDrag(-10);
        }
        else if(Screen.width - JerryUtil.GetClickPos().x < 50)
        {
            DragCamera.Inst.DoDrag(10);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="first">当前面第一次设置位置</param>
    private void Place2Pos(Vector3 pos, bool first = false, bool fromClick = false)
    {
        //Debug.LogWarning("pos1 " + MapUtil.Vector3String(pos) + " " + m_InitData.m_CurWall + " " + MapUtil.GetMap(m_InitData.m_CurWall).Pos2Grid(pos));
        
        MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ(m_GridSize, true, ref pos);
        //Debug.LogWarning("pos2 " + MapUtil.Vector3String(pos));
        m_Pos = AdjustPos(pos);
        Enum_Layer changeType = Enum_Layer.None;

        //Debug.LogWarning(m_Pos.x + "  " + m_Pos.z + " sss " + m_InitData.m_MaxPos.x + " " + m_InitData.m_MaxPos.y + " " + m_InitData.m_MaxPos.z);

        if (m_InitData.m_CurWall == Enum_Layer.Wall)
        {
            m_Pos.x = Mathf.Clamp(m_Pos.x, m_InitData.m_MinPos.x, m_InitData.m_MaxPos.x);
            if (m_Pos.x <= m_InitData.m_MinPos.x)
            {
                //点的时候，不能会自动换墙
                if (first || fromClick)
                {
                    m_Pos.x = m_InitData.m_MinPos.x + MapUtil.m_MapGridUnityLen;
                }
                else
                {
                    changeType = Enum_Layer.LeftWall;
                }
            }
            else if (m_Pos.x >= m_InitData.m_MaxPos.x)
            {
                //点的时候，不能会自动换墙
                if (first || fromClick)
                {
                    m_Pos.x = m_InitData.m_MaxPos.x - MapUtil.m_MapGridUnityLen;
                }
                else
                {
                    changeType = Enum_Layer.RightWall;
                }
            }
            m_Pos.y = Mathf.Clamp(m_Pos.y, m_InitData.m_MinPos.y, m_InitData.m_MaxPos.y);
        }
        else if(m_InitData.m_CurWall == Enum_Layer.LeftWall
            || m_InitData.m_CurWall == Enum_Layer.RightWall)
        {
            m_Pos.z = Mathf.Clamp(m_Pos.z, m_InitData.m_MinPos.z, m_InitData.m_MaxPos.z);
            if (m_Pos.z >= m_InitData.m_MaxPos.z)
            {
                //点的时候，不能会自动换墙
                if (first || fromClick)
                {
                    m_Pos.z = m_InitData.m_MaxPos.z - MapUtil.m_MapGridUnityLen;
                }
                else
                {
                    changeType = Enum_Layer.Wall;
                }
            }

            m_Pos.y = Mathf.Clamp(m_Pos.y, m_InitData.m_MinPos.y, m_InitData.m_MaxPos.y);
        }
        else if (m_InitData.m_CurWall == Enum_Layer.FloorWall)
        {
            m_Pos.x = Mathf.Clamp(m_Pos.x, m_InitData.m_MinPos.x, m_InitData.m_MaxPos.x);
            m_Pos.z = Mathf.Clamp(m_Pos.z, m_InitData.m_MinPos.z, m_InitData.m_MaxPos.z);
        }
        
        if (changeType != Enum_Layer.None)
        {
            //Debug.LogWarning("yyyyyyyyyyyy " + m_Pos.x);
            //这一步不标记状态，因为已经越界了

            MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ(m_GridSize, false, ref m_Pos);
            transform.position = m_Pos;

            Init(changeType, m_Pos, true);
        }
        else
        {
            //Debug.LogWarning("xxxxxxxxxxxxxx " + m_Pos.x);
            transform.position = m_Pos;

            bool canSet = MapUtil.GetMap(m_InitData.m_CurWall).JudgeSet(this.transform.position, m_GridSize);
            MyShadow.Inst.SetPos(MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ2(this.transform.position), this.transform.eulerAngles);
            SetOutLineColor(canSet ? Color.green : Color.red);
            MyShadow.Inst.SetColor(canSet ? Color.green : Color.red);
        }

        if (Wall.Inst.m_CtrType != Wall.CtrObjType.OnlyClick)
        {
            JudgePosOutScreen();
        }
    }

    private Vector3 AdjustPos(Vector3 pos)
    {
        //Debug.LogWarning("pp1=" + MapUtil.Vector3String(pos));
        pos = pos - m_InitData.m_AdjustPar - MapUtil.GetMap(m_InitData.m_CurWall).m_StartPos;

        Vector3 p1 = pos / MapUtil.m_MapGridUnityLen;
        p1.x = Mathf.RoundToInt(p1.x);
        p1.y = Mathf.RoundToInt(p1.y);
        p1.z = Mathf.RoundToInt(p1.z);

        //Debug.LogWarning("pp2=" + MapUtil.Vector3String(pos) + " " + MapUtil.Vector3String(p1));

        pos = MapUtil.GetMap(m_InitData.m_CurWall).m_StartPos + m_InitData.m_AdjustPar;
        pos += p1 * MapUtil.m_MapGridUnityLen;

        //Debug.LogWarning("pp3=" + MapUtil.Vector3String(pos));

        return pos;
    }

    private float MyClamp(float x, float v1, float v2)
    {
        return Mathf.Abs(v1 - x) > Mathf.Abs(v2 - x) ? v2 : v1;
    }

    private void SetOutLineVisible(bool show)
    {
        m_Render.material.SetFloat("_Scale", show ? 1.02f : 1f);
    }

    private void SetOutLineColor(Color col)
    {
        m_Render.material.SetColor("_OutlineColor", col);
    }

    #region 事件

    private void EventPlace2Pos(object[] args)
    {
        if (m_Selected == false)
        {
            return;
        }

        FirstPos fp = (FirstPos)args[0];
        
        Debug.LogWarning(MapUtil.Vector3String(fp.pos) + " grid=" + MapUtil.GetMap(m_InitData.m_CurWall).Pos2Grid(fp.pos));

        if (fp.wallType == m_InitData.m_CurWall)
        {
            Place2Pos(fp.pos, false, true);
        }
        else
        {
            if(m_SetType != MapUtil.SetType.Floor
                && fp.wallType != Enum_Layer.FloorWall)
            {
                Init(fp.wallType, fp.pos, true, true);
            }
        }
    }

    private void EventBack2Package(object[] args)
    {
        int id = (int)args[0];
        if (id != m_Id)
        {
            return;
        }

        m_InitData.isNew = true;

        MapUtil.m_SelectOK = true;
        MapUtil.m_SelectId = 0;

        m_Selected = false;
        this.gameObject.layer = LayerMask.NameToLayer("Cube");

        MyShadow.Inst.SetVisible(false);
        SetOutLineVisible(false);

        this.transform.position = new Vector3(0, 7, 0);
    }

    private void EventBackOne(object[] args)
    {
        int id = (int)args[0];
        if (id != m_Id)
        {
            return;
        }

        if (m_InitData.m_LastWall != Enum_Layer.None)
        {
            if (MapUtil.GetMap(m_InitData.m_LastWall).SetOne(m_InitData.m_LastPos, m_GridSize))
            {
                m_InitData.m_CurWall = m_InitData.m_LastWall;

                m_Selected = false;
                this.gameObject.layer = LayerMask.NameToLayer(Enum_Layer.Cube.ToString());
                m_Pos = m_InitData.m_LastPos;
                MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ(m_GridSize, false, ref m_Pos);
                this.transform.position = m_Pos;

                MyShadow.Inst.SetVisible(false);
                SetOutLineVisible(false);
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
            SetOutLineVisible(false);

            Debug.LogWarning("设置OK");
        }
        else
        {
            Debug.LogWarning("重叠");
        }
    }

    #endregion 事件

    [ContextMenu("放到屏幕中")]
    private void ToScreen()
    {
        SelectSelf();
    }
}