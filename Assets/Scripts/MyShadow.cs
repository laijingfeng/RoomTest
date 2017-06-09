using UnityEngine;

public class MyShadow : MonoBehaviour
{
    private Renderer m_Render;
    private Color m_Color;
    
    private static MyShadow m_Inst;
    public static MyShadow Inst
    {
        get
        {
            return m_Inst;
        }
    }

    void Awake()
    {
        m_Render = this.transform.GetComponent<Renderer>();
        if (m_Render != null)
        {
            m_Render.enabled = false;
        }
        m_Color = Color.green;
        m_Inst = this;
    }

    public void SetSize(Vector3 size)
    {
        size = size * MapUtil.m_MapGridUnityLen;
        size.z = 0.001f;
        this.transform.localScale = size;
    }

    public void SetVisible(bool isShow)
    {
        if (m_Render == null)
        {
            return;
        }
        m_Render.enabled = isShow;
    }

    public void SetPos(Vector3 pos, Vector3 rotate)
    {
        this.transform.position = pos;
        this.transform.eulerAngles = rotate;
    }

    public void SetColor(Color col)
    {
        if (m_Render == null)
        {
            return;
        }

        if (col == m_Color)
        {
            return;
        }

        m_Color = col;
        m_Render.material.SetColor("_Color", col);
    }
}