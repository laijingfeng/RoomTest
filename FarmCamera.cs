using UnityEngine;
using System.Collections;

public class FarmCamera : MonoBehaviour
{
    public Vector3 posMin = Vector3.zero;
    public Vector3 posMax = Vector3.zero;

    public float dragFactor = 1f;
    public float lerpFactor = 10f;
    public float lerpMul = 8f;

    private Vector3 tmp1;

    private bool m_DragUsefull = false;

    private Vector3 m_LastPos;
    private Vector3 m_Offset;
    private float m_OffsetTime = 0;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            this.StopCoroutine("IE_AdjustCamera");

            m_LastPos = GetClickPos();
            m_DragUsefull = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (m_DragUsefull)
            {
                if (Time.realtimeSinceStartup - m_OffsetTime < 1)
                {
                    //Debug.LogWarning("dd v=" + m_Offset.x);
                    DoDrag(-m_Offset * lerpMul, true);
                }
                //else
                //{
                //    Debug.LogWarning("ddx " + Time.realtimeSinceStartup + " " + m_OffsetTime);
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
        m_Offset = GetClickPos() - m_LastPos;
        if (Mathf.Abs(m_Offset.x) < 3f
            && Mathf.Abs(m_Offset.y) < 3f)
        {
            return;
        }
        m_LastPos = GetClickPos();
        m_OffsetTime = Time.realtimeSinceStartup;
        //Debug.LogWarning(m_Offset + " off");
        DoDrag(-m_Offset);
    }

    public void DoDrag(Vector3 val, bool smooth = false)
    {
        Vector3 v2 = Vector3.zero;
        v2.y = 0;
        v2.x = (-val.x + val.y) * 1.414f;
        v2.z = (-val.x - val.y) * 1.414f;

        //Debug.LogWarning("val=" + val + " v2=" + v2);

        tmp1 = Camera.main.transform.position;
        tmp1 += v2 * dragFactor;

        tmp1 = Vector3Clamp(tmp1, posMin, posMax);

        if (smooth)
        {
            this.StopCoroutine("IE_AdjustCamera");
            this.StartCoroutine("IE_AdjustCamera", tmp1);
        }
        else
        {
            Camera.main.transform.position = tmp1;
        }
    }

    private IEnumerator IE_AdjustCamera(Vector3 pos)
    {
        tmp1 = pos;
        //Debug.LogWarning(MapUtil.Vector3String(Camera.main.transform.position) + " ||| " + MapUtil.Vector3String(tmp1));
        Vector3 v = Vector3.zero;
        while (!Vector3Equal(tmp1, Camera.main.transform.position, 0.005f))
        {
            Camera.main.transform.position = Vector3.Lerp/*SmoothDamp*/(Camera.main.transform.position, tmp1, /*ref v,*/ Time.deltaTime * lerpFactor);
            yield return new WaitForEndOfFrame();
        }
    }

    #region 辅助

    private Vector3 Vector3Clamp(Vector3 val, Vector3 min, Vector3 max)
    {
        val.x = Mathf.Clamp(val.x, min.x, max.x);
        val.y = Mathf.Clamp(val.y, min.y, max.y);
        val.z = Mathf.Clamp(val.z, min.z, max.z);
        return val;
    }

    private bool Vector3Equal(Vector3 a, Vector3 b, float val = 0.1f)
    {
        if (Mathf.Abs(a.x - b.x) > val
            || Mathf.Abs(a.y - b.y) > val
            || Mathf.Abs(a.z - b.z) > val)
        {
            return false;
        }
        return true;
    }

    private Vector3 GetClickPos()
    {
        Vector3 pos = Input.mousePosition;
#if UNITY_EDITOR
        pos = Input.mousePosition;
#else
#if UNITY_ANDROID || UNITY_IPHONE
            if(Input.touchCount > 0)
            {
                pos = Input.touches[0].position;
            }
            else
            {
                pos = Input.mousePosition;
            }
#else
            pos = Input.mousePosition;
#endif
#endif
        pos.z = 0;
        return pos;
    }
    #endregion 辅助
}
