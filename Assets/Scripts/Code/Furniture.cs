using UnityEngine;
using System.Collections;
using Jerry;
using UnityEngine.EventSystems;

/// <summary>
/// 家具
/// </summary>
public class Furniture : MonoBehaviour
{
    /// <summary>
    /// id
    /// </summary>
    public uint m_Id;

    /// <summary>
    /// 选中
    /// </summary>
    private bool m_Selected = false;

    private Renderer m_Render;

    public FurnitureInitData m_InitData = null;
    public FurnitureConfig m_Config;
    public FurnitureSaveData m_SaveData = null;

    private bool _awaked = false;
    private bool _inited = false;

    void Awake()
    {
        m_Render = this.GetComponent<Renderer>();
        this.gameObject.layer = LayerMask.NameToLayer(Enum_Layer.Furniture.ToString());

        m_Id = Util.IDGenerator(m_Id);
        m_InitData = new FurnitureInitData();
        
        JerryEventMgr.AddEvent(Enum_Event.SetOneFurn.ToString(), EventSetOneFurn);
        JerryEventMgr.AddEvent(Enum_Event.SetFurn2Pos.ToString(), EventSetFurn2Pos);
        JerryEventMgr.AddEvent(Enum_Event.CancelSetFurn.ToString(), EventCancelSetFurn);
        JerryEventMgr.AddEvent(Enum_Event.SetFurn2Package.ToString(), EventSetFurn2Package);

        _awaked = true;
        TryWork();
    }

    void OnDestroy()
    {
        JerryEventMgr.RemoveEvent(Enum_Event.SetOneFurn.ToString(), EventSetOneFurn);
        JerryEventMgr.RemoveEvent(Enum_Event.SetFurn2Pos.ToString(), EventSetFurn2Pos);
        JerryEventMgr.RemoveEvent(Enum_Event.CancelSetFurn.ToString(), EventCancelSetFurn);
        JerryEventMgr.RemoveEvent(Enum_Event.SetFurn2Package.ToString(), EventSetFurn2Package);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="saveData"></param>
    public void InitData(FurnitureConfig config, FurnitureSaveData saveData)
    {
        m_Config = config;
        m_SaveData = saveData;
        _inited = true;
        TryWork();
    }

    private void TryWork()
    {
        if (!_awaked || !_inited)
        {
            return;
        }

        if (m_SaveData.saveWall != Enum_Wall.None)
        {
            Set2SavePos();
        }
        else
        {
            ToScreen();
        }
    }

    void Update()
    {
        //UpdateCtr();
        TryDrag();
    }

    #region 拖拽和选中

    private bool m_InDraging = false;

    private Ray m_Ray;
    private RaycastHit m_HitInfo;
    private Vector3 m_LastDragingPos;
    private Vector3 m_Offset;

    private Vector3 m_ClickDownPos = Vector3.zero;
    private Vector3 m_ClickUpPos = Vector3.zero;

    private void TryDrag()
    {
        if (GameApp.Inst.UpDowning)
        {
            //Debug.LogWarning("click 0");
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (Util.ClickUI()
                || !ClickMe())
            {
                //Debug.LogWarning("click 1");
                return;
            }

            if (!m_Selected)
            {
                //Debug.LogWarning("click 2");
                m_ClickDownPos = JerryUtil.GetClickPos();
                return;
            }
            else
            {
                //Debug.LogWarning("click 3");
                m_InDraging = true;
                this.StopCoroutine("IE_DoDrag");
                this.StartCoroutine("IE_DoDrag");
            }

            //Debug.LogWarning("click 3_1");
        }

        if (Input.GetMouseButtonUp(0))
        {
            m_InDraging = false;

            if (Util.ClickUI())
            {
                //Debug.LogWarning("click 4");
                return;
            }

            if (!m_Selected)
            {
                //Debug.LogWarning("click 5");
                m_ClickUpPos = JerryUtil.GetClickPos();
                if (Util.Vector3Equal(m_ClickUpPos, m_ClickDownPos, 2)
                    && GameApp.Inst.EditorMode)
                {
                    //Debug.LogWarning("click 6");
                    SelectSelf();
                    return;
                }
            }

            //Debug.LogWarning("click 6_1");
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
        
        while (m_InDraging)
        {
            if (Util.Vector3Equal(JerryUtil.GetClickPos() - m_Offset, m_LastDragingPos, 2)
                && !JudgePosOutScreen())//移动屏幕的时候，相对位置永远不变，这样物体不会更随
            {
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();//等两帧，减小频率
                continue;
            }

            m_LastDragingPos = JerryUtil.GetClickPos() - m_Offset;
            m_Ray = Camera.main.ScreenPointToRay(m_LastDragingPos);

            if (Physics.Raycast(m_Ray, out m_HitInfo, 100,
                    JerryUtil.MakeLayerMask(JerryUtil.MakeLayerMask(false),
                    MapUtil.GetWallLayerNames(m_Config.setType))))
            {
                if (m_HitInfo.collider != null
                    && m_HitInfo.collider.gameObject != null)
                {
                    RayClickPos fp = new RayClickPos();
                    fp.pos = m_HitInfo.point;
                    fp.wallType = MapUtil.WallLayer2WallEnum(m_HitInfo.collider.gameObject.layer);

                    //Debug.LogWarning(fp.wallType + " xxx " + m_InitData.m_CurWall);

                    if (fp.wallType != m_InitData.m_CurWall
                        || !Util.Vector3Equal(fp.pos, m_LastPos))
                    {
                        m_LastPos = fp.pos;

                        if (fp.wallType == m_InitData.m_CurWall)
                        {
                            UI_Ctr.Inst.HideCtr();
                            //Debug.LogWarning("bbbb================ " + MapUtil.Vector3String(fp.pos));
                            if (Place2Pos(fp.pos, true))
                            {
                                CalOffset();
                                //yield break;
                            }
                        }
                        else
                        {
                            if (fp.wallType != Enum_Wall.Floor
                                && m_Config.setType != MapUtil.SetType.Floor)
                            {
                                //Debug.LogWarning("aaaa================");
                                UI_Ctr.Inst.HideCtr();
                                SelectChange2Wall(m_InitData.m_CurWall, fp.wallType, fp.pos);
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
            UI_Ctr.Inst.ShowCtr();
            //Debug.LogWarning("Click ShowCtr");
        }
    }

    private bool JudgePosOutScreen()
    {
        if (JerryUtil.GetClickPos().x < GameApp.Inst.m_OutScreenJudgeFactor)
        {
            CameraCtr.Inst.DoDrag(-GameApp.Inst.m_OutScreenDragFactor);
            return true;
        }
        else if (Screen.width - JerryUtil.GetClickPos().x < GameApp.Inst.m_OutScreenJudgeFactor)
        {
            CameraCtr.Inst.DoDrag(GameApp.Inst.m_OutScreenDragFactor);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 选中
    /// </summary>
    private void SelectSelf(bool isNew = false)
    {
        if (MapUtil.m_SelectId != 0
            && !MapUtil.m_SelectOK)
        {
            if (MapUtil.m_SelectNew)
            {
                JerryEventMgr.DispatchEvent(Enum_Event.SetFurn2Package.ToString(), new object[] { MapUtil.m_SelectId });
            }
            else
            {
                JerryEventMgr.DispatchEvent(Enum_Event.CancelSetFurn.ToString(), new object[] { MapUtil.m_SelectId });
            }
        }

        if (isNew)
        {
            RayClickPos fp = MapUtil.GetFirstPos(m_Config.setType);
            //Debug.LogWarning(fp.pos + " x " + fp.wallType + " " + m_Config.setType);
            SelectChange2Wall(Enum_Wall.Wall, fp.wallType, fp.pos);
        }
        else if (m_InitData.m_CurWall != Enum_Wall.None)
        {
            //先浮起来，再记录，保持回退时一致性
            this.transform.position = MapUtil.GetMap(m_InitData.m_CurWall).AdjustFurn2Wall(m_Config.size, true, this.transform.position);

            m_InitData.m_LastPos = this.transform.position;
            m_InitData.m_LastWall = m_InitData.m_CurWall;

            MapUtil.GetMap(m_InitData.m_CurWall).CleanOne(this.transform.position, m_Config.size);
        }

        //Debug.LogWarning("xxx " + m_InitData.isNew + " " + m_InitData.m_CurWall);

        MapUtil.m_SelectId = m_Id;
        MapUtil.m_SelectOK = false;
        MapUtil.m_SelectNew = isNew;
        MapUtil.m_SelectFurn = this;

        this.gameObject.layer = LayerMask.NameToLayer(Enum_Layer.ActiveFurniture.ToString());
        m_Selected = true;
        m_InitData.isSeted = false;

        bool canSet = MapUtil.GetMap(m_InitData.m_CurWall).JudgeSet(this.transform.position, m_Config.size);

        GridMgr.Inst.ShowGrid(m_Config.setType, m_Config.size.y);
        SetOutLineVisible(true);
        SetOutLineColor(canSet ? Color.green : Color.red);
        FurnitureShadow.Inst.SetSize(m_Config.size.ToVector3(), m_InitData.m_CurWall);
        FurnitureShadow.Inst.SetVisible(true);
        FurnitureShadow.Inst.SetPosColor(MapUtil.GetMap(m_InitData.m_CurWall).Adjust2Wall(this.transform.position), canSet ? Color.green : Color.red);
        UI_Ctr.Inst.ShowCtr();
    }

    #region 辅助

    private bool ClickMe()
    {
        m_Ray = Camera.main.ScreenPointToRay(JerryUtil.GetClickPos());

        if (Physics.Raycast(m_Ray, out m_HitInfo, 100))
        {
            //Debug.LogWarning("Hit=" + m_HitInfo.collider.gameObject.name);
            if (m_HitInfo.collider.gameObject != null
                && m_HitInfo.collider.gameObject == this.gameObject)
            {
                return true;
            }
        }
        return false;
    }

    private void CalOffset()
    {
        Vector3 pos = MapUtil.GetMap(m_InitData.m_CurWall).Adjust2Wall(this.transform.position);
        //Debug.LogWarning("calOffset " + pos + " wall=" + m_InitData.m_CurWall + " " + MapUtil.Vector3String(this.transform.position));
        m_Offset = JerryUtil.GetClickPos() - Camera.main.WorldToScreenPoint(pos);
        //第一步，先记录一下位置，不给走
        m_LastDragingPos = JerryUtil.GetClickPos() - m_Offset;
    }

    #endregion 辅助

    #endregion 拖拽和选中

    #region 放置

    private Vector3 m_LastPos;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="canChangeWall">是否检测换面</param>
    /// <returns>是否切换了面</returns>
    private bool Place2Pos(Vector3 pos, bool canChangeWall = false)
    {
        //Debug.LogWarning("pos1 " + MapUtil.Vector3String(pos) 
        //    + " wall:" + m_InitData.m_CurWall 
        //    + " posGrid:" + MapUtil.GetMap(m_InitData.m_CurWall).Pos2Grid(pos));

        pos = AdjustPos(pos);
        pos = MapUtil.GetMap(m_InitData.m_CurWall).AdjustFurn2Wall(m_Config.size, true, pos);
        Enum_Wall changeType = Enum_Wall.None;

        //Debug.LogWarning("pos=" + MapUtil.Vector3String(m_Pos)
        //    + " Min:" + MapUtil.Vector3String(m_InitData.m_MinPos)
        //    + " Max:" + MapUtil.Vector3String(m_InitData.m_MaxPos)
        //    + " ad:" + MapUtil.Vector3String(m_InitData.m_AdjustPar)
        //    + " size:" + m_Config.size);

        if (m_InitData.m_CurWall == Enum_Wall.Wall)
        {
            pos.x = Mathf.Clamp(pos.x, m_InitData.m_MinPos.x, m_InitData.m_MaxPos.x);
            if (pos.x <= m_InitData.m_MinPos.x)
            {
                if (!canChangeWall)
                {
                    pos.x = m_InitData.m_MinPos.x + GameApp.Inst.m_MapGridUnityLen;
                }
                else
                {
                    changeType = Enum_Wall.Left;
                }
            }
            else if (pos.x >= m_InitData.m_MaxPos.x)
            {
                if (!canChangeWall)
                {
                    pos.x = m_InitData.m_MaxPos.x - GameApp.Inst.m_MapGridUnityLen;
                }
                else
                {
                    changeType = Enum_Wall.Right;
                }
            }
            else
            {
                pos.x = Mathf.Clamp(pos.x, m_InitData.m_MinPos.x + GameApp.Inst.m_MapGridUnityLen, m_InitData.m_MaxPos.x - GameApp.Inst.m_MapGridUnityLen);
            }
            pos.y = Mathf.Clamp(pos.y, m_InitData.m_MinPos.y, m_InitData.m_MaxPos.y);
        }
        else if (m_InitData.m_CurWall == Enum_Wall.Left
            || m_InitData.m_CurWall == Enum_Wall.Right)
        {
            pos.z = Mathf.Clamp(pos.z, m_InitData.m_MinPos.z, m_InitData.m_MaxPos.z);
            if (pos.z >= m_InitData.m_MaxPos.z)
            {
                if (!canChangeWall)
                {
                    pos.z = m_InitData.m_MaxPos.z - GameApp.Inst.m_MapGridUnityLen;
                }
                else
                {
                    changeType = Enum_Wall.Wall;
                }
            }
            else
            {
                pos.z = Mathf.Clamp(pos.z, m_InitData.m_MinPos.z, m_InitData.m_MaxPos.z - GameApp.Inst.m_MapGridUnityLen);
            }

            pos.y = Mathf.Clamp(pos.y, m_InitData.m_MinPos.y, m_InitData.m_MaxPos.y);
        }
        else if (m_InitData.m_CurWall == Enum_Wall.Floor)
        {
            pos.x = Mathf.Clamp(pos.x, m_InitData.m_MinPos.x, m_InitData.m_MaxPos.x);
            pos.z = Mathf.Clamp(pos.z, m_InitData.m_MinPos.z, m_InitData.m_MaxPos.z);
        }

        if (changeType != Enum_Wall.None)
        {
            //Debug.LogWarning("yyyyyyyyyyyy " + MapUtil.Vector3String(m_Pos));
            //这一步不标记状态，因为已经越界了

            pos = MapUtil.GetMap(m_InitData.m_CurWall).AdjustFurn2Wall(m_Config.size, false, pos);
            transform.position = pos;

            SelectChange2Wall(m_InitData.m_CurWall, changeType, MapUtil.GetMap(changeType).ChangeWallAdjust2Bound(pos, m_InitData.m_CurWall));
        }
        else
        {
            //Debug.LogWarning("xxxxxxxxxxxxxx " + MapUtil.Vector3String(m_Pos)
            //    + "\nMin:" + MapUtil.Vector3String(m_InitData.m_MinPos)
            //    + " Max:" + MapUtil.Vector3String(m_InitData.m_MaxPos)
            //    + "\nWall:" + m_InitData.m_CurWall);
            transform.position = pos;

            bool canSet = MapUtil.GetMap(m_InitData.m_CurWall).JudgeSet(this.transform.position, m_Config.size);
            //Debug.LogWarning("xxxxxxxxxxxxx");
            FurnitureShadow.Inst.SetPosColor(MapUtil.GetMap(m_InitData.m_CurWall).Adjust2Wall(this.transform.position), canSet ? Color.green : Color.red);
            SetOutLineColor(canSet ? Color.green : Color.red);
        }

        return changeType == Enum_Wall.None ? false : true;
    }

    private Vector3 AdjustPos(Vector3 pos)
    {
        //Debug.LogWarning("pp1=" + MapUtil.Vector3String(pos) + " ad=" + MapUtil.Vector3String(m_InitData.m_AdjustPar));
        pos = pos - m_InitData.m_AdjustPar - MapUtil.GetMap(m_InitData.m_CurWall).m_StartPos;

        Vector3 p1 = pos / GameApp.Inst.m_MapGridUnityLen;
        p1.x = Mathf.RoundToInt(p1.x);
        p1.y = Mathf.RoundToInt(p1.y);
        p1.z = Mathf.RoundToInt(p1.z);

        //Debug.LogWarning("pp2=" + MapUtil.Vector3String(pos) + " " + MapUtil.Vector3String(p1));

        pos = MapUtil.GetMap(m_InitData.m_CurWall).m_StartPos + m_InitData.m_AdjustPar + p1 * GameApp.Inst.m_MapGridUnityLen;

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
    private void EventSetFurn2Pos(object[] args)
    {
        if (m_Selected == false)
        {
            return;
        }

        RayClickPos fp = (RayClickPos)args[0];

        //Debug.LogWarning(MapUtil.Vector3String(fp.pos) + " grid=" + MapUtil.GetMap(m_InitData.m_CurWall).Pos2Grid(fp.pos));

        //Debug.LogWarning("name=" + this.name + " wall=" + m_InitData.m_CurWall);

        if (fp.wallType == m_InitData.m_CurWall)
        {
            Place2Pos(fp.pos, false);
        }
        else
        {
            if (m_Config.setType != MapUtil.SetType.Floor
                && fp.wallType != Enum_Wall.Floor)
            {
                SelectChange2Wall(m_InitData.m_CurWall, fp.wallType, fp.pos);
            }
        }
        UI_Ctr.Inst.ShowCtr();
    }

    /// <summary>
    /// 撤回背包
    /// </summary>
    /// <param name="args"></param>
    private void EventSetFurn2Package(object[] args)
    {
        uint id = (uint)args[0];
        if (id != m_Id)
        {
            return;
        }

        UnSelect(false);

        GameObject.Destroy(this.gameObject);
    }

    /// <summary>
    /// 放回原处
    /// </summary>
    /// <param name="args"></param>
    private void EventCancelSetFurn(object[] args)
    {
        uint id = (uint)args[0];
        if (id != m_Id)
        {
            return;
        }

        if (m_InitData.m_LastWall != Enum_Wall.None)
        {
            Init2Wall(m_InitData.m_CurWall, m_InitData.m_LastWall);

            MapUtil.GetMap(m_InitData.m_CurWall).SetOne(m_InitData.m_LastPos, m_Config.size);
            UnSelect(true);
            this.transform.position = MapUtil.GetMap(m_InitData.m_CurWall).AdjustFurn2Wall(m_Config.size, false, m_InitData.m_LastPos);
        }
    }

    /// <summary>
    /// 放置
    /// </summary>
    /// <param name="args"></param>
    private void EventSetOneFurn(object[] args)
    {
        uint id = (uint)args[0];
        if (id != m_Id)
        {
            return;
        }

        //Debug.LogWarning((m_InitData == null) + " x " + m_InitData.m_CurWall + " " + this.name);

        if (MapUtil.GetMap(m_InitData.m_CurWall).SetOne(this.transform.position, m_Config.size))
        {
            UnSelect(true);
            this.transform.position = MapUtil.GetMap(m_InitData.m_CurWall).AdjustFurn2Wall(m_Config.size, false, this.transform.position);

            UI_Tip.Inst.ShowTip("设置OK");
        }
        else
        {
            UI_Tip.Inst.ShowTip("重叠");
        }
    }

    #endregion 事件

    [ContextMenu("放到屏幕中")]
    public void ToScreen()
    {
        Debug.LogWarning(this.name);
        SelectSelf(true);
    }

    private void UpdateCtr()
    {
        if (m_Selected && Input.GetKeyDown(KeyCode.N))
        {
            Flag.Inst.Set2Pos(JerryUtil.GetClickPos() - m_Offset);
        }
    }

    #region 存档

    /// <summary>
    /// 刷新保存信息的位置
    /// </summary>
    public void RefreshSaveDataPos()
    {
        m_SaveData.type = m_Config.type;
        m_SaveData.saveWall = m_InitData.m_CurWall;
        m_SaveData.savePos = new MapUtil.IVector3((this.transform.position - MapUtil.GetMap(m_InitData.m_CurWall).m_StartPos) / GameApp.Inst.m_MapGridUnityLenHalf);
    }
    
    private void Set2SavePos()
    {
        Init2Wall(Enum_Wall.Wall, m_SaveData.saveWall);
        
        m_InitData.isSeted = true;
        this.transform.position = MapUtil.GetMap(m_InitData.m_CurWall).m_StartPos + m_SaveData.savePos.MulVal(GameApp.Inst.m_MapGridUnityLenHalf);
        MapUtil.GetMap(m_InitData.m_CurWall).SetOne(this.transform.position, m_Config.size);
        SetOutLineVisible(false);
    }

    #endregion 存档

    #region 辅助
    
    /// <summary>
    /// 取消选中
    /// </summary>
    /// <param name="isSeted"></param>
    private void UnSelect(bool isSeted = false)
    {
        GridMgr.Inst.HideGrid();
        FurnitureShadow.Inst.SetVisible(false);
        SetOutLineVisible(false);
        UI_Ctr.Inst.HideCtr();

        MapUtil.m_SelectId = 0;
        MapUtil.m_SelectOK = true;
        MapUtil.m_SelectNew = false;
        MapUtil.m_SelectFurn = null;

        m_Selected = false;
        m_InitData.isSeted = isSeted;
        m_InDraging = false;

        this.gameObject.layer = LayerMask.NameToLayer(Enum_Layer.Furniture.ToString());
    }

    /// <summary>
    /// 初始化到墙
    /// </summary>
    /// <param name="fromWall"></param>
    /// <param name="toWall"></param>
    private void Init2Wall(Enum_Wall fromWall, Enum_Wall toWall)
    {
        m_Config.size = MapUtil.ChangeObjSize(m_Config.size, fromWall, toWall);
        m_InitData = MapUtil.InitFurn(m_Config.size, m_Config.setType, m_InitData, toWall);
        this.transform.eulerAngles = MapUtil.GetObjEulerAngles(m_InitData.m_CurWall);
    }

    /// <summary>
    /// 选中转墙
    /// </summary>
    /// <param name="fromWall"></param>
    /// <param name="toWall"></param>
    /// <param name="pos"></param>
    private void SelectChange2Wall(Enum_Wall fromWall, Enum_Wall toWall, Vector3 pos)
    {
        Init2Wall(fromWall, toWall);
        FurnitureShadow.Inst.SetSize(m_Config.size.ToVector3(), m_InitData.m_CurWall);
        Place2Pos(pos, false);
    }

    #endregion 辅助
}