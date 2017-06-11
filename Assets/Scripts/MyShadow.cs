using UnityEngine;
using Jerry;

public class MyShadow : SingletonMono<MyShadow>
{
    private Renderer m_Render;
    private Color m_Color;

    public override void Awake()
    {
        base.Awake();
        m_Render = this.transform.GetComponent<Renderer>();
        if (m_Render != null)
        {
            m_Render.enabled = false;
        }
        m_Color = Color.green;
    }

    /// <param name="size"></param>
    public void SetSize(Vector3 size, MapUtil.SetType setType)
    {
        size = size * MapUtil.m_MapGridUnityLen;
        if (setType == MapUtil.SetType.Floor)
        {
            size.y = 0.001f;
        }
        else
        {
            size.z = 0.001f;
        }
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