using UnityEngine;
using Jerry;
using UnityEngine.EventSystems;

public class DragCamera : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    public float m_DragFactor = 0.5f;
    public float m_RotateFactor = 0.5f;

    private bool m_DragUsefull = false;
    private Ray m_Ray;
    private RaycastHit m_HitInfo;

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_Ray = Camera.main.ScreenPointToRay(Util.GetClickPos());
        if (Physics.Raycast(m_Ray, out m_HitInfo, 100,
            JerryUtil.MakeLayerMask(JerryUtil.MakeLayerMask(false),
                new string[]
                {
                    "ActiveCube"
                })))
        {
            if (m_HitInfo.collider != null
                && m_HitInfo.collider.gameObject != null)
            {
                m_DragUsefull = false;
                return;
            }
        }
        m_DragUsefull = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!m_DragUsefull)
        {
            return;
        }

        if (Mathf.Abs(eventData.delta.x) < 3f
            || Mathf.Abs(eventData.delta.x) < Mathf.Abs(eventData.delta.y))
        {
            return;
        }
        DoDrag(eventData.delta.x);
    }

    private Vector3 tmp1;
    private Vector3 tmp2;

    private void DoDrag(float val)
    {
        //Debug.LogWarning("val=" + val);
        tmp1 = Camera.main.transform.position;
        tmp2 = Camera.main.transform.eulerAngles;

        if (tmp1.x >= 3.78f && (tmp2.y > 0 || val > 0))
        {
            tmp1.x = 3.78f;
            tmp2.y += val * m_RotateFactor;
            tmp2.y = Mathf.Clamp(tmp2.y, 0, 30);
        }
        else if (tmp1.x <= -3.78f && (tmp2.y > 330 || val < 0))
        {
            tmp1.x = -3.78f;
            if (tmp2.y > 0)
            {
                tmp2.y -= 360;
            }
            tmp2.y += val * m_RotateFactor;
            tmp2.y = Mathf.Clamp(tmp2.y, -30, 0);
        }
        else
        {
            tmp1.x += val * m_DragFactor;
            tmp1.x = Mathf.Clamp(tmp1.x, -3.78f, 3.78f);
            tmp2.y = 0f;
        }

        Camera.main.transform.position = tmp1;
        Camera.main.transform.eulerAngles = tmp2;
    }
}