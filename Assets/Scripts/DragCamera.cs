using System.Collections;
using Jerry;
using UnityEngine;

public class DragCamera : SingletonMono<DragCamera>
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
            return Wall.Inst.EditorMode ? m_DragBoundEditor : m_DragBound;
        }
    }

    private Ray m_Ray;
    private RaycastHit m_HitInfo;
    private bool m_DragUsefull = false;
    private Vector3 m_LastPos;
    private Vector3 m_Offset;

    private Vector3 tmp1;
    private Vector3 tmp2;

    public override void Awake()
    {
        base.Awake();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) 
            && !Util.ClickUI())
        {
            m_Ray = Camera.main.ScreenPointToRay(Util.GetClickPos());
            if (Physics.Raycast(m_Ray, out m_HitInfo, 100,
                JerryUtil.MakeLayerMask(JerryUtil.MakeLayerMask(false),
                    new string[]
                {
                    Enum_Layer.ActiveCube.ToString()
                })))
            {
                if (m_HitInfo.collider != null
                    && m_HitInfo.collider.gameObject != null)
                {
                    //点到选中的物体，是移动物体，不移动镜头
                    m_DragUsefull = false;
                    return;
                }
            }
            m_LastPos = JerryUtil.GetClickPos();
            m_DragUsefull = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
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
        DoDrag(-m_Offset.x);
    }

    public void DoDrag(float val)
    {
        if (!m_DragCameraInUse)
        {
            return;
        }

        //Debug.LogWarning("val=" + val);
        tmp1 = Camera.main.transform.position;
        tmp2 = Camera.main.transform.eulerAngles;

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

        Camera.main.transform.position = tmp1;
        Camera.main.transform.eulerAngles = tmp2;

        UICtr.Inst.AdjustPos();
    }

    public void AdjustCamera()
    {
        this.StopCoroutine("IE_AdjustCamera");
        this.StartCoroutine("IE_AdjustCamera");
    }

    private IEnumerator IE_AdjustCamera()
    {
        tmp1 = Camera.main.transform.position;
        tmp1.x = Mathf.Clamp(tmp1.x, -GetDragBound, GetDragBound);
        //Debug.LogWarning(MapUtil.Vector3String(Camera.main.transform.position) + " ||| " + MapUtil.Vector3String(tmp1));
        Vector3 v = Vector3.zero;
        while (!Util.Vector3Equal(tmp1, Camera.main.transform.position, 0.05f))
        {
            Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position, tmp1, ref v, Time.deltaTime * 5);
            yield return new WaitForEndOfFrame();
        }
        Camera.main.transform.position = tmp1;
        UICtr.Inst.AdjustPos();
    }
}