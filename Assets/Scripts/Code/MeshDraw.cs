using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class MeshDraw : MonoBehaviour
{
    [Header("Common Settings")]

    public float m_GridWidth = 0.5f;

    [Header("Config")]

    public DrawConfig[] m_Config;

    private Mesh m_Mesh;
    private List<Vector3> m_DrawVertices = new List<Vector3>();
    private List<int> m_DrawIdx = new List<int>();

    public enum Plane
    {
        XY = 0,
        XZ,
        ZY,
    }

    [System.Serializable]
    public class DrawConfig
    {
        public Vector3 m_StartPos;
        public Vector2 m_Size;
        public Plane m_Plane;
        public float m_HLineWidthHalf;
        public float m_VLineWidthHalf;
        /// <summary>
        /// 正方向
        /// </summary>
        public bool m_ClockDir = true;
    }

    void Awake()
    {
        m_Mesh = this.gameObject.GetComponent<MeshFilter>().mesh;
        ReDraw();
    }

    private void DoClean()
    {
        m_Mesh.Clear();
        m_DrawVertices.Clear();
        m_DrawIdx.Clear();
    }

    private void ApplyDraw()
    {
        m_Mesh.vertices = m_DrawVertices.ToArray();
        m_Mesh.triangles = m_DrawIdx.ToArray();
    }

    [ContextMenu("ReDraw")]
    private void ReDraw()
    {
        DoClean();
        foreach (DrawConfig con in m_Config)
        {
            AddOneConfig(con);
        }
        ApplyDraw();
    }

    private void AddOneConfig(DrawConfig config)
    {
        int x = (int)config.m_Size.x;
        int y = (int)config.m_Size.y;

        int idx = m_DrawVertices.Count;

        //横线
        for (int i = 0; i <= y; i++)
        {
            AddHLine(x, i, config.m_HLineWidthHalf, config.m_VLineWidthHalf, config.m_ClockDir);
        }

        //竖线
        for (int i = 0; i <= x; i++)
        {
            AddVLine(y, i, config.m_VLineWidthHalf, config.m_HLineWidthHalf, config.m_ClockDir);
        }

        for (int i = idx, imax = m_DrawVertices.Count; i < imax; i++)
        {
            switch (config.m_Plane)
            {
                case Plane.XY:
                    {
                        m_DrawVertices[i] = config.m_StartPos + m_DrawVertices[i];
                    }
                    break;
                case Plane.XZ:
                    {
                        m_DrawVertices[i] = new Vector3(m_DrawVertices[i].x, 0, m_DrawVertices[i].y);
                        m_DrawVertices[i] = config.m_StartPos + m_DrawVertices[i];
                    }
                    break;
                case Plane.ZY:
                    {
                        m_DrawVertices[i] = new Vector3(0, m_DrawVertices[i].y, m_DrawVertices[i].x);
                        m_DrawVertices[i] = config.m_StartPos + m_DrawVertices[i];
                    }
                    break;
            }
        }
    }

    private void AddHLine(int x, int i, float widthHalf, float otherWidthHalf, bool clockDir)
    {
        //0,i*m_GridWidth,0
        //x*m_GridWidth,i*m_GridWidth,0

        int idx = m_DrawVertices.Count;//First Idx

        m_DrawVertices.Add(new Vector3(0 - otherWidthHalf, i * m_GridWidth + widthHalf, 0));
        m_DrawVertices.Add(new Vector3(x * m_GridWidth + otherWidthHalf, i * m_GridWidth + widthHalf, 0));
        m_DrawVertices.Add(new Vector3(x * m_GridWidth + otherWidthHalf, i * m_GridWidth - widthHalf, 0));
        m_DrawVertices.Add(new Vector3(0-otherWidthHalf, i * m_GridWidth - widthHalf, 0));

        AddIdx(idx, clockDir);
    }

    private void AddVLine(int y, int i, float widthHalf, float otherWidthHalf, bool clockDir)
    {
        //i*m_GridWidth,0,0
        //i*m_GridWidth,y*m_GridWidth,0

        int idx = m_DrawVertices.Count;//First Idx

        m_DrawVertices.Add(new Vector3(i * m_GridWidth - widthHalf, y * m_GridWidth + otherWidthHalf, 0));
        m_DrawVertices.Add(new Vector3(i * m_GridWidth + widthHalf, y * m_GridWidth + otherWidthHalf, 0));
        m_DrawVertices.Add(new Vector3(i * m_GridWidth + widthHalf, 0 - otherWidthHalf, 0));
        m_DrawVertices.Add(new Vector3(i * m_GridWidth - widthHalf, 0 - otherWidthHalf, 0));

        AddIdx(idx, clockDir);
    }

    private void AddIdx(int startIdx, bool clockDir)
    {
        if (clockDir)
        {
            m_DrawIdx.Add(startIdx + 0);
            m_DrawIdx.Add(startIdx + 1);
            m_DrawIdx.Add(startIdx + 2);
            m_DrawIdx.Add(startIdx + 2);
            m_DrawIdx.Add(startIdx + 3);
            m_DrawIdx.Add(startIdx + 0);
        }
        else
        {
            m_DrawIdx.Add(startIdx + 0);
            m_DrawIdx.Add(startIdx + 3);
            m_DrawIdx.Add(startIdx + 2);
            m_DrawIdx.Add(startIdx + 2);
            m_DrawIdx.Add(startIdx + 1);
            m_DrawIdx.Add(startIdx + 0);
        }
    }
}