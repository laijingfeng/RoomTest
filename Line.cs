using UnityEngine;
using Jerry;
using System.Collections.Generic;

public class Line : SingletonMono<Line>
{
    [Range(0, 6)]
    public int m_WallCnt = 0;
    public bool m_Floor = false;

    private LineRenderer m_LineRender;
    public Vector3 p1 = new Vector3(-6, 0, 0);
    public Vector3 p2 = new Vector3(6, 0, 0);

    public override void Awake()
    {
        base.Awake();
        m_LineRender = this.GetComponent<LineRenderer>();
        m_LineRender.SetWidth(0.02f, 0.02f);
    }

    void Start()
    {
        Vector3[] vs = GetFloorPos();
        m_LineRender.SetVertexCount(vs.Length);
        m_LineRender.SetPositions(vs);
    }

    public void HideGrid()
    {
        m_Floor = false;
        m_WallCnt = 0;
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
    }

    private Vector3 pos;

    void DoDo()
    {
        if (m_WallCnt > 0)
        {
            for (int i = 0; i <= m_WallCnt; i++)
            {
                pos = MapUtil.GetMap(Enum_Layer.Wall).m_StartPos + new Vector3(0, MapUtil.m_MapGridUnityLen * i, 0);
                GL.Vertex(pos);
                pos += new Vector3(MapUtil.GetMap(Enum_Layer.Wall).m_Size.x * MapUtil.m_MapGridUnityLen, 0, 0);
                GL.Vertex(pos);

                pos = MapUtil.GetMap(Enum_Layer.LeftWall).m_StartPos + new Vector3(0, MapUtil.m_MapGridUnityLen * i, 0);
                GL.Vertex(pos);
                pos += new Vector3(0, 0, MapUtil.GetMap(Enum_Layer.LeftWall).m_Size.z * MapUtil.m_MapGridUnityLen);
                GL.Vertex(pos);

                pos = MapUtil.GetMap(Enum_Layer.RightWall).m_StartPos + new Vector3(0, MapUtil.m_MapGridUnityLen * i, 0);
                GL.Vertex(pos);
                pos += new Vector3(0, 0, MapUtil.GetMap(Enum_Layer.RightWall).m_Size.z * MapUtil.m_MapGridUnityLen);
                GL.Vertex(pos);
            }

            for (int i = 0; i <= MapUtil.GetMap(Enum_Layer.Wall).m_Size.x; i++)
            {
                pos = MapUtil.GetMap(Enum_Layer.Wall).m_StartPos + new Vector3(MapUtil.m_MapGridUnityLen * i, 0, 0);
                GL.Vertex(pos);
                pos += new Vector3(0, m_WallCnt * MapUtil.m_MapGridUnityLen, 0);
                GL.Vertex(pos);
            }

            for (int i = 0; i <= MapUtil.GetMap(Enum_Layer.LeftWall).m_Size.z; i++)
            {
                pos = MapUtil.GetMap(Enum_Layer.LeftWall).m_StartPos + new Vector3(0, 0, MapUtil.m_MapGridUnityLen * i);
                GL.Vertex(pos);
                pos += new Vector3(0, m_WallCnt * MapUtil.m_MapGridUnityLen, 0);
                GL.Vertex(pos);
            }

            for (int i = 0; i <= MapUtil.GetMap(Enum_Layer.RightWall).m_Size.z; i++)
            {
                pos = MapUtil.GetMap(Enum_Layer.RightWall).m_StartPos + new Vector3(0, 0, MapUtil.m_MapGridUnityLen * i);
                GL.Vertex(pos);
                pos += new Vector3(0, m_WallCnt * MapUtil.m_MapGridUnityLen, 0);
                GL.Vertex(pos);
            }
        }

        if (m_Floor)
        {
            for (int i = 0; i <= MapUtil.GetMap(Enum_Layer.FloorWall).m_Size.z; i++)
            {
                pos = MapUtil.GetMap(Enum_Layer.FloorWall).m_StartPos + new Vector3(0, 0, MapUtil.m_MapGridUnityLen * i);
                GL.Vertex(pos);
                pos += new Vector3(MapUtil.GetMap(Enum_Layer.FloorWall).m_Size.x * MapUtil.m_MapGridUnityLen, 0, 0);
                GL.Vertex(pos);
            }

            for (int i = 0; i <= MapUtil.GetMap(Enum_Layer.FloorWall).m_Size.x; i++)
            {
                pos = MapUtil.GetMap(Enum_Layer.FloorWall).m_StartPos + new Vector3(MapUtil.m_MapGridUnityLen * i, 0, 0);
                GL.Vertex(pos);
                pos += new Vector3(0, 0, MapUtil.GetMap(Enum_Layer.FloorWall).m_Size.z * MapUtil.m_MapGridUnityLen);
                GL.Vertex(pos);
            }
        }
    }

    private Vector3[] GetFloorPos()
    {
        List<Vector3> ret = new List<Vector3>();
        Vector3 p1, p2;
        int idx = 0;

        for (int i = 0; i <= MapUtil.GetMap(Enum_Layer.FloorWall).m_Size.z; i++)
        {
            p1 = MapUtil.GetMap(Enum_Layer.FloorWall).m_StartPos + new Vector3(0, 0, MapUtil.m_MapGridUnityLen * i);
            p2 = p1 + new Vector3(MapUtil.GetMap(Enum_Layer.FloorWall).m_Size.x * MapUtil.m_MapGridUnityLen, 0, 0);

            if (idx % 2 == 0)
            {
                ret.Add(p1);
                ret.Add(p2);
            }
            else
            {
                ret.Add(p2);
                ret.Add(p1);
            }
            idx++;
        }

        //for (int i = 0; i <= MapUtil.GetMap(Enum_Layer.FloorWall).m_Size.x; i++)
        //{
        //    p1 = MapUtil.GetMap(Enum_Layer.FloorWall).m_StartPos + new Vector3(MapUtil.m_MapGridUnityLen * i, 0.01f, 0);
        //    p2 = p1 + new Vector3(0, 0, MapUtil.GetMap(Enum_Layer.FloorWall).m_Size.z * MapUtil.m_MapGridUnityLen);
        //    if (idx % 2 == 0)
        //    {
        //        ret.Add(p1);
        //        ret.Add(p2);
        //    }
        //    else
        //    {
        //        ret.Add(p2);
        //        ret.Add(p1);
        //    }
        //    idx++;
        //}

        return ret.ToArray();
    }
}