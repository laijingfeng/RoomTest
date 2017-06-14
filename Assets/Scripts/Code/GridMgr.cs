using Jerry;
using UnityEngine;

public class GridMgr : SingletonMono<GridMgr>
{
    [Range(0, 6)]
    public int m_WallStart = 0;
    [Range(0, 6)]
    public int m_WallEnd = 0;
    public bool m_Floor = false;

    private const int WALL_GRID_CNT = 6;

    private Transform m_FloorGrid;
    private Transform[] m_WallGrid;

    public override void Awake()
    {
        base.Awake();
        m_FloorGrid = this.transform.FindChild("FloorGrid");
        m_WallGrid = new Transform[WALL_GRID_CNT];
        for (int i = 0; i < WALL_GRID_CNT; i++)
        {
            m_WallGrid[i] = this.transform.FindChild(string.Format("WallGrid{0}", i));
        }
    }

    public void RefreshPos()
    {
        Vector3 pos = this.transform.position;
        pos.y = GameApp.Inst.GetHouseYOffset;
        this.transform.position = pos;
    }

    public void HideGrid()
    {
        m_Floor = false;
        m_WallEnd = 0;
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
                    m_WallStart = 0;
                    m_WallEnd = par;
                }
                break;
            case MapUtil.SetType.Wall:
                {
                    m_WallStart = 1;
                    m_WallEnd = 6;
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
                    m_WallGrid[i].gameObject.SetActive(m_WallEnd > i && i >= m_WallStart);
                }
            }
        }
    }
}