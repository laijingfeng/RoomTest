using UnityEngine;
using Jerry;

public class Line : SingletonMono<Line>
{
    [Range(0, 6)]
    public int m_WallCnt = 0;
    public bool m_Floor = false;

    public Transform m_FloorGrid;
    public Transform[] m_WallGrid;

    void Start()
    {
    }

    public void HideGrid()
    {
        m_Floor = false;
        m_WallCnt = 0;
        RefreshGrid();
    }

    public void ShowGrid(MapUtil.SetType setType, int par = 6)
    {
        switch (setType)
        {
            case MapUtil.SetType.Floor:
                {
                    m_Floor = true;
                }
                break;
            case MapUtil.SetType.WallOnFloor:
                {
                    m_WallCnt = par;
                }
                break;
            case MapUtil.SetType.Wall:
                {
                    m_WallCnt = 6;
                }
                break;
        }
        RefreshGrid();
    }

    private void RefreshGrid()
    {
        if (m_FloorGrid != null)
        {
            m_FloorGrid.gameObject.SetActive(m_Floor);
        }
        
        if (m_WallGrid != null)
        {
            for (int i = 0; i < 6; i++)
            {
                if (i < m_WallGrid.Length)
                {
                    m_WallGrid[i].gameObject.SetActive(m_WallCnt > i);
                }
            }
        }
    }
}