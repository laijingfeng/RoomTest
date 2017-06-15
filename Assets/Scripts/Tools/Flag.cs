using UnityEngine;
using UnityEngine.UI;
using Jerry;

public class  Flag : SingletonMono<Flag>
{
    public Canvas m_Canvas;
    private Image m_Img;

    public override void Awake()
    {
        base.Awake();
        m_Img = this.GetComponent<Image>();
    }

    public void Set2Pos(Vector3 pos)
    {
        this.transform.localPosition = JerryUtil.PosScreen2Canvas(m_Canvas, pos, this.transform);
        m_Img.enabled = true;
        Debug.LogWarning("Flag==========================");
    }
}