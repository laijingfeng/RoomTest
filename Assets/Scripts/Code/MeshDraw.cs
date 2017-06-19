using System.Collections.Generic;
using Jerry;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class MeshDraw : SingletonMono<MeshDraw>
{
    #region 对外接口

    private DrawConfig m_Left;
    private DrawConfig m_Right;
    private DrawConfig m_Wall;
    private DrawConfig m_Floor;

    public void RefreshPos()
    {
        m_Left.m_StartPos.y = GameApp.Inst.GetHouseYOffset;
        m_Right.m_StartPos.y = GameApp.Inst.GetHouseYOffset;
        m_Wall.m_StartPos.y = GameApp.Inst.GetHouseYOffset;
        m_Floor.m_StartPos.y = GameApp.Inst.GetHouseYOffset;
    }

    public void HideGrid()
    {
        DoClean();
    }

    public void ShowGrid(MapUtil.SetType setType, int par = 6)
    {
        DoClean();
        
        switch (setType)
        {
            case MapUtil.SetType.Floor:
                {
                    AddOneConfig(m_Floor);
                }
                break;
            case MapUtil.SetType.WallOnFloor:
                {
                    m_Left.m_Size.y = par;
                    m_Right.m_Size.y = par;
                    m_Wall.m_Size.y = par;

                    m_Left.m_StartPos.y = GameApp.Inst.GetHouseYOffset;
                    m_Right.m_StartPos.y = GameApp.Inst.GetHouseYOffset;
                    m_Wall.m_StartPos.y = GameApp.Inst.GetHouseYOffset;

                    AddOneConfig(m_Left);
                    AddOneConfig(m_Right);
                    AddOneConfig(m_Wall);
                }
                break;
            case MapUtil.SetType.Wall:
                {
                    m_Left.m_Size.y = 5;
                    m_Right.m_Size.y = 5;
                    m_Wall.m_Size.y = 5;

                    m_Left.m_StartPos.y = GameApp.Inst.GetHouseYOffset + GameApp.Inst.m_MapGridUnityLen;
                    m_Right.m_StartPos.y = GameApp.Inst.GetHouseYOffset + GameApp.Inst.m_MapGridUnityLen;
                    m_Wall.m_StartPos.y = GameApp.Inst.GetHouseYOffset + GameApp.Inst.m_MapGridUnityLen;

                    AddOneConfig(m_Left);
                    AddOneConfig(m_Right);
                    AddOneConfig(m_Wall);
                }
                break;
        }
        ApplyDraw();
    }

    public void InitConfig()
    {
        m_Left =new DrawConfig()
        {
            m_StartPos = GameApp.Inst.m_LeftSideWallStartPos + new Vector3(0.01f, 0, 0),
            m_Size = GameApp.SizeXYZ2XY(GameApp.Inst.m_LeftSideWallSize, Enum_Wall.Left),
            m_ClockDir = true,
            m_Plane = Plane.ZY,
            m_HLineWidthHalf = 0.01f,
            m_VLineWidthHalf = 0.04f,
        };
        m_Right = new DrawConfig()
        {
            m_StartPos = GameApp.Inst.m_RightSideWallStartPos + new Vector3(-0.01f, 0, 0),
            m_Size = GameApp.SizeXYZ2XY(GameApp.Inst.m_RightSideWallSize, Enum_Wall.Right),
            m_ClockDir = false,
            m_Plane = Plane.ZY,
            m_HLineWidthHalf = 0.01f,
            m_VLineWidthHalf = 0.04f,
        };
        m_Wall = new DrawConfig()
        {
            m_StartPos = GameApp.Inst.m_WallStartPos + new Vector3(0, 0, -0.01f),
            m_Size = GameApp.SizeXYZ2XY(GameApp.Inst.m_WallSize, Enum_Wall.Wall),
            m_ClockDir = true,
            m_Plane = Plane.XY,
            m_HLineWidthHalf = 0.01f,
            m_VLineWidthHalf = 0.01f,
        };
        m_Floor = new DrawConfig()
        {
            m_StartPos = GameApp.Inst.m_FloorWallStartPos + new Vector3(0, 0.01f, 0),
            m_Size = GameApp.SizeXYZ2XY(GameApp.Inst.m_FloorWallSize, Enum_Wall.Floor),
            m_ClockDir = true,
            m_Plane = Plane.XZ,
            m_HLineWidthHalf = 0.02f,
            m_VLineWidthHalf = 0.01f,
        };
    }

    #endregion 对外接口

    [Header("Common Settings")]

    public float m_GridWidth = 0.5f;

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

    public override void Awake()
    {
        base.Awake();
        m_Mesh = this.gameObject.GetComponent<MeshFilter>().mesh;
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
        m_DrawVertices.Add(new Vector3(0 - otherWidthHalf, i * m_GridWidth - widthHalf, 0));

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