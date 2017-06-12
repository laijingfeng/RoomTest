using UnityEngine;
using Jerry;

public class Line : SingletonMono<Line>
{
    public Material mat;
    public Color color = Color.red;
    public Vector3 pos1;
    public Vector3 pos2;

    [Range(0, 6)]
    public int m_WallCnt = 0;
    public bool m_Floor = false;

    void Start()
    {
        mat.color = color;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pos1 = Input.mousePosition;
        }
        if (Input.GetMouseButton(0))
        {
            pos2 = Input.mousePosition;
        }
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

    void OnPostRender()
    {
        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadOrtho();
        GL.Begin(GL.LINES);
        GL.Color(color);

        //GL.Vertex3(pos1.x / Screen.width, pos1.y / Screen.height, 0);
        //GL.Vertex3(pos2.x / Screen.width, pos2.y / Screen.height, 0);

        if (m_WallCnt > 0)
        {
            for (int i = 0; i <= m_WallCnt; i++)
            {
                pos = MapUtil.GetMap(Enum_Layer.Wall).m_StartPos + new Vector3(0, MapUtil.m_MapGridUnityLen * i, 0);
                GL.Vertex(PosWorld2GL(pos));
                pos += new Vector3(MapUtil.GetMap(Enum_Layer.Wall).m_Size.x * MapUtil.m_MapGridUnityLen, 0, 0);
                GL.Vertex(PosWorld2GL(pos));

                pos = MapUtil.GetMap(Enum_Layer.LeftWall).m_StartPos + new Vector3(0, MapUtil.m_MapGridUnityLen * i, 0);
                GL.Vertex(PosWorld2GL(pos));
                pos += new Vector3(0, 0, MapUtil.GetMap(Enum_Layer.LeftWall).m_Size.z * MapUtil.m_MapGridUnityLen);
                GL.Vertex(PosWorld2GL(pos));

                pos = MapUtil.GetMap(Enum_Layer.RightWall).m_StartPos + new Vector3(0, MapUtil.m_MapGridUnityLen * i, 0);
                GL.Vertex(PosWorld2GL(pos));
                pos += new Vector3(0, 0, MapUtil.GetMap(Enum_Layer.RightWall).m_Size.z * MapUtil.m_MapGridUnityLen);
                GL.Vertex(PosWorld2GL(pos));
            }

            for (int i = 0; i <= MapUtil.GetMap(Enum_Layer.Wall).m_Size.x; i++)
            {
                pos = MapUtil.GetMap(Enum_Layer.Wall).m_StartPos + new Vector3(MapUtil.m_MapGridUnityLen * i, 0, 0);
                GL.Vertex(PosWorld2GL(pos));
                pos += new Vector3(0, m_WallCnt * MapUtil.m_MapGridUnityLen, 0);
                GL.Vertex(PosWorld2GL(pos));
            }

            for (int i = 0; i <= MapUtil.GetMap(Enum_Layer.LeftWall).m_Size.z; i++)
            {
                pos = MapUtil.GetMap(Enum_Layer.LeftWall).m_StartPos + new Vector3(0, 0, MapUtil.m_MapGridUnityLen * i);
                GL.Vertex(PosWorld2GL(pos));
                pos += new Vector3(0, m_WallCnt * MapUtil.m_MapGridUnityLen, 0);
                GL.Vertex(PosWorld2GL(pos));
            }

            for (int i = 0; i <= MapUtil.GetMap(Enum_Layer.RightWall).m_Size.z; i++)
            {
                pos = MapUtil.GetMap(Enum_Layer.RightWall).m_StartPos + new Vector3(0, 0, MapUtil.m_MapGridUnityLen * i);
                GL.Vertex(PosWorld2GL(pos));
                pos += new Vector3(0, m_WallCnt * MapUtil.m_MapGridUnityLen, 0);
                GL.Vertex(PosWorld2GL(pos));
            }
        }

        if (m_Floor)
        {
            for (int i = 0; i <= MapUtil.GetMap(Enum_Layer.FloorWall).m_Size.z; i++)
            {
                pos = MapUtil.GetMap(Enum_Layer.FloorWall).m_StartPos + new Vector3(0, 0, MapUtil.m_MapGridUnityLen * i);
                GL.Vertex(PosWorld2GL(pos));
                pos += new Vector3(MapUtil.GetMap(Enum_Layer.FloorWall).m_Size.x * MapUtil.m_MapGridUnityLen, 0, 0);
                GL.Vertex(PosWorld2GL(pos));
            }

            for (int i = 0; i <= MapUtil.GetMap(Enum_Layer.FloorWall).m_Size.x; i++)
            {
                pos = MapUtil.GetMap(Enum_Layer.FloorWall).m_StartPos + new Vector3(MapUtil.m_MapGridUnityLen * i, 0, 0);
                GL.Vertex(PosWorld2GL(pos));
                pos += new Vector3(0, 0, MapUtil.GetMap(Enum_Layer.FloorWall).m_Size.z * MapUtil.m_MapGridUnityLen);
                GL.Vertex(PosWorld2GL(pos));
            }
        }

        GL.End();
        GL.PopMatrix();
    }

    private Vector3 PosWorld2GL(Vector3 pos)
    {
        return ScreenPosNormalize(Camera.main.WorldToScreenPoint(pos));
    }

    private Vector3 ScreenPosNormalize(Vector3 pos)
    {
        pos.z = 0;
        pos.x /= Screen.width;
        pos.y /= Screen.height;
        return pos;
    }
}