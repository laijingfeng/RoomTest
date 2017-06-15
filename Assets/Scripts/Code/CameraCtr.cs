using System.Collections;
using Jerry;
using UnityEngine;

/// <summary>
/// 相机控制
/// </summary>
public class CameraCtr : SingletonMono<CameraCtr>
{
    public bool m_DragCameraInUse = true;
    
    public float m_DragFactor = 0.05f;
    public float m_RotateFactor = 0.5f;

    public float m_DragBoundEditor = 4.8f;
    public float m_DragBound = 4.4f;
    public float m_RotateBound = 25;

    private float GetDragBound
    {
        get
        {
            return GameApp.Inst.EditorMode ? m_DragBoundEditor : m_DragBound;
        }
    }

    private Ray m_Ray;
    private RaycastHit m_HitInfo;
    private bool m_DragUsefull = false;
    private Vector3 m_LastPos;
    private Vector3 m_Offset;
    private Vector3 m_LastOffset = Vector3.zero;
    private float m_LastOffsetTime = 0;

    private Vector3 tmp1;
    private Vector3 tmp2;

    public override void Awake()
    {
        base.Awake();
        JerryEventMgr.AddEvent(Enum_Event.Click3DDown.ToString(), EventClickDown);
    }

    void Update()
    {
        TryDrag();
    }

    void OnDestroy()
    {
        JerryEventMgr.RemoveEvent(Enum_Event.Click3DDown.ToString(), EventClickDown);
    }

    private void EventClickDown(object[] args)
    {
        if (args == null || args.Length != 1)
        {
            return;
        }
        RayClickInfo info = (RayClickInfo)args[0];
        //点到选中的物体，是移动物体，不移动镜头
        if (info.col == null
            || info.col.gameObject.layer != LayerMask.NameToLayer(Enum_Layer.ActiveFurniture.ToString()))
        {
            OnCameraDown(true);
        }
        else
        {
            OnCameraDown(false);
        }
    }

    private void OnCameraDown(bool usefull)
    {
        this.StopCoroutine("IE_AdjustCamera");
        m_DragUsefull = usefull;
        if (!usefull)
        {
            return;
        }
        m_LastPos = JerryUtil.GetClickPos();
    }

    private void TryDrag()
    {
        if (GameApp.Inst.UpDowning)
        {
            this.StopCoroutine("IE_AdjustCamera");//TODO:一直Stop会不会耗性能?
            m_DragUsefull = false;
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (m_DragUsefull)
            {
                if (Time.realtimeSinceStartup - m_LastOffsetTime < 0.2f)
                {
                    //Debug.LogWarning("dd v=" + m_LastOffset.x);
                    DoDrag(-m_LastOffset.x * 8, true);
                }
                //else
                //{
                //    Debug.LogWarning("ddx " + Time.realtimeSinceStartup + " " + m_LastOffsetTime);
                //}
            }
            m_DragUsefull = false;
        }

        if (m_DragUsefull)
        {
            JudgeDrag();
        }
    }

    private void JudgeDrag()
    {
        m_Offset = JerryUtil.GetClickPos() - m_LastPos;
        if (Mathf.Abs(m_Offset.x) < 3f)
        {
            return;
        }
        m_LastPos = JerryUtil.GetClickPos();
        m_LastOffset = m_Offset;
        m_LastOffsetTime = Time.realtimeSinceStartup;
        DoDrag(-m_Offset.x);
    }

    public void DoDrag(float val, bool smooth = false)
    {
        if (!m_DragCameraInUse)
        {
            return;
        }

        //Debug.LogWarning("val=" + val);
        tmp1 = this.transform.position;
        tmp2 = this.transform.eulerAngles;

        if (tmp1.x >= GetDragBound && (tmp2.y > 0 || val > 0))
        {
            tmp1.x = GetDragBound;
            tmp2.y += val * m_RotateFactor;
            tmp2.y = Mathf.Clamp(tmp2.y, 0, m_RotateBound);
        }
        else if (tmp1.x <= -GetDragBound && (tmp2.y > 360 - m_RotateBound || val < 0))
        {
            tmp1.x = -GetDragBound;
            if (tmp2.y > 0)
            {
                tmp2.y -= 360;
            }
            tmp2.y += val * m_RotateFactor;
            tmp2.y = Mathf.Clamp(tmp2.y, -m_RotateBound, 0);
        }
        else
        {
            tmp1.x += val * m_DragFactor;
            tmp1.x = Mathf.Clamp(tmp1.x, -GetDragBound, GetDragBound);
            tmp2.y = 0f;
        }

        if (smooth)
        {
            this.StopCoroutine("IE_AdjustCamera");
            this.StartCoroutine("IE_AdjustCamera", tmp1);
        }
        else
        {
            this.transform.position = tmp1;
        }
        this.transform.eulerAngles = tmp2;

        UI_Ctr.Inst.AdjustPos();
    }

    public void AdjustCamera()
    {
        Vector3 pos = this.transform.position;
        pos.x = Mathf.Clamp(tmp1.x, -GetDragBound, GetDragBound);

        this.StopCoroutine("IE_AdjustCamera");
        this.StartCoroutine("IE_AdjustCamera", pos);
    }

    private IEnumerator IE_AdjustCamera(Vector3 pos)
    {
        tmp1 = pos;
        //Debug.LogWarning(MapUtil.Vector3String(this.transform.position) + " ||| " + MapUtil.Vector3String(tmp1));
        Vector3 v = Vector3.zero;
        while (!Util.Vector3Equal(tmp1, this.transform.position, 0.05f))
        {
            this.transform.position = Vector3.SmoothDamp(this.transform.position, tmp1, ref v, Time.deltaTime * 15);
            yield return new WaitForEndOfFrame();
            UI_Ctr.Inst.AdjustPos();
        }
        this.transform.position = tmp1;
        UI_Ctr.Inst.AdjustPos();
    }
}