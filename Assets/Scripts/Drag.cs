﻿using UnityEngine;
using System.Collections;
using Jerry;
using UnityEngine.EventSystems;

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
    public int m_Id;

    /// <summary>
    /// 选中
    /// </summary>
    private bool m_Selected = false;

    private Renderer m_Render;

    public DragInitData m_InitData = null;

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
        JerryEventMgr.AddEvent(Enum_Event.SavePos.ToString(), EventSavePos);
        JerryEventMgr.AddEvent(Enum_Event.LoadData.ToString(), EventLoadData);
    }

    void Update()
    {
        //UpdateCtr();
        JudgeDrag();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="wallType"></param>
    /// <param name="pos"></param>
    public void Init(Enum_Layer wallType, Vector3 pos)
    {
        m_InitData = MapUtil.InitDrag(m_GridSize, m_SetType, m_InitData, wallType);
        this.transform.eulerAngles = MapUtil.GetMap(m_InitData.m_CurWall).GetObjEulerAngles();
        Place2Pos(pos, false);
    }

    #region 拖拽和选中

    private bool m_InDraging = false;

    private Ray m_Ray;
    private RaycastHit m_HitInfo;
    private Vector3 m_LastDragingPos;
    private Vector3 m_Offset;

    private Vector3 m_ClickDownPos = Vector3.zero;
    private Vector3 m_ClickUpPos = Vector3.zero;

    private void JudgeDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Util.ClickUI()
                || !ClickMe())
            {
                return;
            }

            if (!m_Selected)
            {
                m_ClickDownPos = JerryUtil.GetClickPos();
                return;
            }
            else
            {
                if (Wall.Inst.m_CtrType != Wall.CtrObjType.OnlyClick)
                {
                    m_InDraging = true;
                    this.StopCoroutine("IE_DoDrag");
                    this.StartCoroutine("IE_DoDrag");
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            m_InDraging = false;

            if (Util.ClickUI())
            {
                return;
            }

            if (!m_Selected)
            {
                m_ClickUpPos = JerryUtil.GetClickPos();
                if (Util.Vector3Equal(m_ClickUpPos, m_ClickDownPos, 2)
                    && Wall.Inst.EditorMode)
                {
                    SelectSelf();
                    return;
                }
            }
        }
    }

    private IEnumerator IE_DoDrag()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            yield break;
        }

        CalOffset();

        //第一步，先记录一下位置，不给走
        m_LastDragingPos = JerryUtil.GetClickPos() - m_Offset;

        while (m_InDraging)
        {
            if (Util.Vector3Equal(JerryUtil.GetClickPos() - m_Offset, m_LastDragingPos, 2)
                    && !JudgePosOutScreen())//移动屏幕的时候，相对位置永远不变，这样物体不会更随
            {
                //Debug.LogWarning("d");
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();//等两帧，减小频率
                continue;
            }

            m_LastDragingPos = JerryUtil.GetClickPos() - m_Offset;
            m_Ray = Camera.main.ScreenPointToRay(m_LastDragingPos);

            if (Physics.Raycast(m_Ray, out m_HitInfo, 100,
                    JerryUtil.MakeLayerMask(JerryUtil.MakeLayerMask(false),
                    MapUtil.GetWallLayerNames(m_SetType))))
            {
                if (m_HitInfo.collider != null
                    && m_HitInfo.collider.gameObject != null)
                {
                    FirstPos fp = new FirstPos();
                    fp.pos = m_HitInfo.point;
                    fp.wallType = MapUtil.WallLayer2Enum(m_HitInfo.collider.gameObject.layer);

                    //Debug.LogWarning(fp.wallType + " xxx " + m_InitData.m_CurWall);

                    if (fp.wallType != m_InitData.m_CurWall
                        || !Util.Vector3Equal(fp.pos, m_LastPos))
                    {
                        m_LastPos = fp.pos;

                        if (fp.wallType == m_InitData.m_CurWall)
                        {
                            //Debug.LogWarning("aaaabbbb");
                            UICtr.Inst.HideCtr();
                            if (Place2Pos(fp.pos, true))
                            {
                                CalOffset();
                                //yield break;
                            }
                        }
                        else
                        {
                            if (fp.wallType != Enum_Layer.FloorWall
                                && m_SetType != MapUtil.SetType.Floor)
                            {
                                //Debug.LogWarning("aaaa");
                                UICtr.Inst.HideCtr();
                                Init(fp.wallType, fp.pos);
                                //yield return new WaitForEndOfFrame();
                                CalOffset();
                                //yield break;
                            }
                        }
                    }
                }
            }

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();//等两帧，减小频率
        }

        if (m_Selected)
        {
            UICtr.Inst.ShowCtr();
            //Debug.LogWarning("Click ShowCtr");
        }
    }

    private bool JudgePosOutScreen()
    {
        if (Wall.Inst.m_CtrType == Wall.CtrObjType.OnlyClick)
        {
            return false;
        }

        if (JerryUtil.GetClickPos().x < Wall.Inst.m_OutScreenJudgeFactor)
        {
            DragCamera.Inst.DoDrag(-Wall.Inst.m_OutScreenDragFactor);
            return true;
        }
        else if (Screen.width - JerryUtil.GetClickPos().x < Wall.Inst.m_OutScreenJudgeFactor)
        {
            DragCamera.Inst.DoDrag(Wall.Inst.m_OutScreenDragFactor);
            return true;
        }
        return false;
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
                JerryEventMgr.DispatchEvent(Enum_Event.Back2Package.ToString(), new object[] { MapUtil.m_SelectId });
            }
            else
            {
                JerryEventMgr.DispatchEvent(Enum_Event.BackOne.ToString(), new object[] { MapUtil.m_SelectId });
            }
        }

        if (m_InitData.isNew)
        {
            FirstPos fp = MapUtil.GetFirstPos(m_SetType);
            //Debug.LogWarning(fp.pos + " x " + fp.wallType + " " + m_SetType);
            Init(fp.wallType, fp.pos);
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

        //Debug.LogWarning("xxx");

        MapUtil.m_SelectId = m_Id;
        MapUtil.m_SelectOK = false;
        MapUtil.m_SelectNew = m_InitData.isNew;
        MapUtil.m_SelectDrag = this;

        m_InitData.isNew = false;
        this.gameObject.layer = LayerMask.NameToLayer(Enum_Layer.ActiveCube.ToString());
        m_Selected = true;
        m_InitData.isSeted = false;

        bool canSet = MapUtil.GetMap(m_InitData.m_CurWall).JudgeSet(this.transform.position, m_GridSize);

        Line.Inst.ShowGrid(m_SetType, m_GridSize.y);
        SetOutLineVisible(true);
        SetOutLineColor(canSet ? Color.green : Color.red);
        MyShadow.Inst.SetSize(m_GridSize.ToVector3(), m_SetType);
        MyShadow.Inst.SetVisible(true);
        MyShadow.Inst.SetColor(canSet ? Color.green : Color.red);
        MyShadow.Inst.SetPos(MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ2(this.transform.position), this.transform.eulerAngles);
        UICtr.Inst.ShowCtr();
    }

    #region 辅助

    private bool ClickMe()
    {
        m_Ray = Camera.main.ScreenPointToRay(JerryUtil.GetClickPos());

        if (Physics.Raycast(m_Ray, out m_HitInfo, 100))
        {
            if (m_HitInfo.collider.gameObject != null
                && m_HitInfo.collider.gameObject == this.gameObject)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 获得贴墙点
    /// </summary>
    /// <returns></returns>
    private Vector3 GetWallPos()
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
                    tmp.x -= m_GridSize.z / 2.0f * MapUtil.m_MapGridUnityLen + MapUtil.m_AdjustZVal;
                }
                break;
            case Enum_Layer.RightWall:
                {
                    tmp.x += m_GridSize.z / 2.0f * MapUtil.m_MapGridUnityLen + MapUtil.m_AdjustZVal;
                }
                break;
        }
        return tmp;
    }

    private void CalOffset()
    {
        Vector3 pos = GetWallPos();
        m_Offset = JerryUtil.GetClickPos() - Camera.main.WorldToScreenPoint(pos);
    }

    #endregion 辅助

    #endregion 拖拽和选中

    #region 放置

    private Vector3 m_Pos;
    private Vector3 m_LastPos;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="canChangeWall">是否检测换面</param>
    /// <returns>是否切换了面</returns>
    private bool Place2Pos(Vector3 pos, bool canChangeWall = false)
    {
        //Debug.LogWarning("pos1 " + MapUtil.Vector3String(pos) + " " + m_InitData.m_CurWall + " " + MapUtil.GetMap(m_InitData.m_CurWall).Pos2Grid(pos));

        //Debug.LogWarning("pos2 " + MapUtil.Vector3String(pos));
        m_Pos = AdjustPos(pos);
        MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ(m_GridSize, true, ref m_Pos);
        Enum_Layer changeType = Enum_Layer.None;

        //Debug.LogWarning("pos=" + MapUtil.Vector3String(m_Pos)
        //    + " Min:" + MapUtil.Vector3String(m_InitData.m_MinPos)
        //    + " Max:" + MapUtil.Vector3String(m_InitData.m_MaxPos));

        if (m_InitData.m_CurWall == Enum_Layer.Wall)
        {
            m_Pos.x = Mathf.Clamp(m_Pos.x, m_InitData.m_MinPos.x, m_InitData.m_MaxPos.x);
            if (m_Pos.x <= m_InitData.m_MinPos.x)
            {
                if (!canChangeWall)
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
                if (!canChangeWall)
                {
                    m_Pos.x = m_InitData.m_MaxPos.x - MapUtil.m_MapGridUnityLen;
                }
                else
                {
                    changeType = Enum_Layer.RightWall;
                }
            }
            else
            {
                m_Pos.x = Mathf.Clamp(m_Pos.x, m_InitData.m_MinPos.x + MapUtil.m_MapGridUnityLen, m_InitData.m_MaxPos.x - MapUtil.m_MapGridUnityLen);
            }
            m_Pos.y = Mathf.Clamp(m_Pos.y, m_InitData.m_MinPos.y, m_InitData.m_MaxPos.y);
        }
        else if (m_InitData.m_CurWall == Enum_Layer.LeftWall
            || m_InitData.m_CurWall == Enum_Layer.RightWall)
        {
            m_Pos.z = Mathf.Clamp(m_Pos.z, m_InitData.m_MinPos.z, m_InitData.m_MaxPos.z);
            if (m_Pos.z >= m_InitData.m_MaxPos.z)
            {
                if (!canChangeWall)
                {
                    m_Pos.z = m_InitData.m_MaxPos.z - MapUtil.m_MapGridUnityLen;
                }
                else
                {
                    changeType = Enum_Layer.Wall;
                }
            }
            else
            {
                m_Pos.z = Mathf.Clamp(m_Pos.z, m_InitData.m_MinPos.z, m_InitData.m_MaxPos.z - MapUtil.m_MapGridUnityLen);
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

            Init(changeType, m_Pos);
        }
        else
        {
            //Debug.LogWarning("xxxxxxxxxxxxxx " + MapUtil.Vector3String(m_Pos)
            //    + " Min:" + MapUtil.Vector3String(m_InitData.m_MinPos) 
            //    + " Max:" + MapUtil.Vector3String(m_InitData.m_MaxPos)
            //    + " wall:" + m_InitData.m_CurWall);
            transform.position = m_Pos;

            bool canSet = MapUtil.GetMap(m_InitData.m_CurWall).JudgeSet(this.transform.position, m_GridSize);
            MyShadow.Inst.SetPos(MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ2(this.transform.position), this.transform.eulerAngles);
            SetOutLineColor(canSet ? Color.green : Color.red);
            MyShadow.Inst.SetColor(canSet ? Color.green : Color.red);
        }

        return changeType == Enum_Layer.None ? false : true;
    }

    private Vector3 AdjustPos(Vector3 pos)
    {
        //Debug.LogWarning("pp1=" + MapUtil.Vector3String(pos) + " ad=" + MapUtil.Vector3String(m_InitData.m_AdjustPar));
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

    #endregion 放置

    #region 描边

    private void SetOutLineVisible(bool show)
    {
        m_Render.material.SetFloat("_Scale", show ? 1.02f : 1f);
    }

    private void SetOutLineColor(Color col)
    {
        m_Render.material.SetColor("_OutlineColor", col);
    }

    #endregion 描边

    #region 事件

    /// <summary>
    /// 点击放到一个位置
    /// </summary>
    /// <param name="args"></param>
    private void EventPlace2Pos(object[] args)
    {
        if (m_Selected == false)
        {
            return;
        }

        FirstPos fp = (FirstPos)args[0];

        //Debug.LogWarning(MapUtil.Vector3String(fp.pos) + " grid=" + MapUtil.GetMap(m_InitData.m_CurWall).Pos2Grid(fp.pos));

        if (fp.wallType == m_InitData.m_CurWall)
        {
            Place2Pos(fp.pos, false);
        }
        else
        {
            if (m_SetType != MapUtil.SetType.Floor
                && fp.wallType != Enum_Layer.FloorWall)
            {
                Init(fp.wallType, fp.pos);
            }
        }
        UICtr.Inst.ShowCtr();
    }

    /// <summary>
    /// 撤回背包
    /// </summary>
    /// <param name="args"></param>
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

        Line.Inst.HideGrid();
        MyShadow.Inst.SetVisible(false);
        SetOutLineVisible(false);
        UICtr.Inst.HideCtr();

        this.transform.position = new Vector3(0, 7, 0);
    }

    /// <summary>
    /// 放回原处
    /// </summary>
    /// <param name="args"></param>
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
                m_InitData.isSeted = true;
                this.gameObject.layer = LayerMask.NameToLayer(Enum_Layer.Cube.ToString());
                m_Pos = m_InitData.m_LastPos;
                MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ(m_GridSize, false, ref m_Pos);
                this.transform.position = m_Pos;

                Line.Inst.HideGrid();
                MyShadow.Inst.SetVisible(false);
                SetOutLineVisible(false);
                UICtr.Inst.HideCtr();
            }
        }
    }

    /// <summary>
    /// 放置
    /// </summary>
    /// <param name="args"></param>
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
            m_InitData.isSeted = true;
            this.gameObject.layer = LayerMask.NameToLayer("Cube");
            m_Pos = this.transform.position;
            MapUtil.GetMap(m_InitData.m_CurWall).AdjustZ(m_GridSize, false, ref m_Pos);
            this.transform.position = m_Pos;

            Line.Inst.HideGrid();
            MyShadow.Inst.SetVisible(false);
            SetOutLineVisible(false);
            UICtr.Inst.HideCtr();
            //Debug.LogWarning("SetHideCtr");

            Tip.Inst.ShowTip("设置OK");
        }
        else
        {
            Tip.Inst.ShowTip("重叠");
        }
    }

    /// <summary>
    /// 存档
    /// </summary>
    /// <param name="args"></param>
    private void EventSavePos(object[] args)
    {
        if (!m_InitData.isSeted)
        {
            return;
        }
        SaveData data = new SaveData();
        data.wallCode = m_InitData.m_CurWall.GetHashCode();
        data.pos = new MapUtil.IVector3((this.transform.position - MapUtil.GetMap(m_InitData.m_CurWall).m_StartPos) / (0.5f * MapUtil.m_MapGridUnityLen));
        PlayerPrefs.SetString("cube_" + m_Id, JsonUtility.ToJson(data));
    }

    private void EventLoadData(object[] args)
    {
        string sdata = PlayerPrefs.GetString("cube_" + m_Id, "");
        if (string.IsNullOrEmpty(sdata))
        {
            return;
        }
        SaveData data = JsonUtility.FromJson<SaveData>(sdata);
        if (data == null)
        {
            return;
        }
        Enum_Layer wall = (Enum_Layer)data.wallCode;

        m_InitData = MapUtil.InitDrag(m_GridSize, m_SetType, m_InitData, wall);
        m_InitData.isSeted = true;
        m_InitData.isNew = false;
        this.transform.eulerAngles = MapUtil.GetMap(m_InitData.m_CurWall).GetObjEulerAngles();
        this.transform.position = MapUtil.GetMap(m_InitData.m_CurWall).m_StartPos + data.pos.MulVal(0.5f * MapUtil.m_MapGridUnityLen);
    }

    [System.Serializable]
    public class SaveData
    {
        /// <summary>
        /// 位置半个格子的几倍
        /// </summary>
        public MapUtil.IVector3 pos;

        public int wallCode;
    }

    #endregion 事件

    [ContextMenu("放到屏幕中")]
    public void ToScreen()
    {
        Debug.LogWarning(this.name);

        SelectSelf();
    }

    private void UpdateCtr()
    {
        if (m_Selected && Input.GetKeyDown(KeyCode.M))
        {
            m_Ray = Camera.main.ScreenPointToRay(JerryUtil.GetClickPos());
            if (Physics.Raycast(m_Ray, out m_HitInfo, 100,
                    JerryUtil.MakeLayerMask(JerryUtil.MakeLayerMask(false),
                    MapUtil.GetWallLayerNames(m_SetType))))
            {
                if (m_HitInfo.collider != null
                    && m_HitInfo.collider.gameObject != null)
                {
                    FirstPos fp = new FirstPos();
                    fp.pos = m_HitInfo.point;
                    fp.wallType = MapUtil.WallLayer2Enum(m_HitInfo.collider.gameObject.layer);

                    Debug.LogWarning("hi " + fp.wallType + " " + MapUtil.GetMap(fp.wallType).Pos2Grid(fp.pos));
                }
            }

            m_Ray = Camera.main.ScreenPointToRay(JerryUtil.GetClickPos() - m_Offset);
            if (Physics.Raycast(m_Ray, out m_HitInfo, 100,
                    JerryUtil.MakeLayerMask(JerryUtil.MakeLayerMask(false),
                    MapUtil.GetWallLayerNames(m_SetType))))
            {
                if (m_HitInfo.collider != null
                    && m_HitInfo.collider.gameObject != null)
                {
                    FirstPos fp = new FirstPos();
                    fp.pos = m_HitInfo.point;
                    fp.wallType = MapUtil.WallLayer2Enum(m_HitInfo.collider.gameObject.layer);

                    Debug.LogWarning("hi_or " + fp.wallType + " " + MapUtil.GetMap(fp.wallType).Pos2Grid(fp.pos));
                }
            }
        }
        else if (m_Selected && Input.GetKeyDown(KeyCode.N))
        {
            Flag.Inst.Set2Pos(JerryUtil.GetClickPos() - m_Offset);
        }
    }
}