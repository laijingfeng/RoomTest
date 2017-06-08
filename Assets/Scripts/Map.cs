using UnityEngine;

public class Map
{
    /// <summary>
    /// 地图大小
    /// </summary>
    public Vector3 m_Size;
    public Vector3 m_StartPos;
    public bool[,] m_Flag;
    public Enum_Wall m_Type;

    public void Init(Enum_Wall type)
    {
        m_Type = type;
        if (m_Type == Enum_Wall.Wall)
        {
            m_Flag = new bool[(int)m_Size.x, (int)m_Size.y];
            for (int i = 0; i < (int)m_Size.x; i++)
            {
                for (int j = 0; j < (int)m_Size.y; j++)
                {
                    m_Flag[i, j] = false;
                }
            }
        }
        else
        {
            m_Flag = new bool[(int)m_Size.z, (int)m_Size.y];
            for (int i = 0; i < (int)m_Size.z; i++)
            {
                for (int j = 0; j < (int)m_Size.y; j++)
                {
                    m_Flag[i, j] = false;
                }
            }
        }
    }

    public Vector3 AdjustZ(Vector3 size, bool floating, ref Vector3 pos)
    {
        if (m_Type == Enum_Wall.Wall)
        {
            pos.z = m_StartPos.z - size.z * MapUtil.m_MapGridUnityLen / 2.0f;
            if (floating)
            {
                pos.z -= 0.3f;
            }
        }
        else if(m_Type == Enum_Wall.LeftWall)
        {
            pos.x = m_StartPos.x + size.z * MapUtil.m_MapGridUnityLen / 2.0f;
            if (floating)
            {
                pos.x += 0.3f;
            }
        }
        else if (m_Type == Enum_Wall.RightWall)
        {
            pos.x = m_StartPos.x - size.z * MapUtil.m_MapGridUnityLen / 2.0f;
            if (floating)
            {
                pos.x -= 0.3f;
            }
        }
        return pos;
    }

    public void GetMinMaxPos(Vector3 size, bool onFloor, ref DragInitData data)
    {
        if (m_Type == Enum_Wall.Wall)
        {
            data.m_MinPos = m_StartPos
                + new Vector3(size.x * MapUtil.m_MapGridUnityLen / 2, size.y * MapUtil.m_MapGridUnityLen / 2, 0);

            data.m_MaxPos = m_StartPos
            + new Vector3(m_Size.x * MapUtil.m_MapGridUnityLen, m_Size.y * MapUtil.m_MapGridUnityLen, 0)
            - new Vector3(size.x * MapUtil.m_MapGridUnityLen / 2, size.y * MapUtil.m_MapGridUnityLen / 2, 0);

            if (onFloor)
            {
                data.m_MaxPos.y = data.m_MinPos.y;
            }
        }
        else
        {
            data.m_MinPos = m_StartPos
                + new Vector3(0, size.y * MapUtil.m_MapGridUnityLen / 2, size.x * MapUtil.m_MapGridUnityLen / 2);

            data.m_MaxPos = m_StartPos
            + new Vector3(0, m_Size.y * MapUtil.m_MapGridUnityLen, m_Size.z * MapUtil.m_MapGridUnityLen)
            - new Vector3(0, size.y * MapUtil.m_MapGridUnityLen / 2, size.x * MapUtil.m_MapGridUnityLen / 2);

            if (onFloor)
            {
                data.m_MaxPos.y = data.m_MinPos.y;
            }
        }

        if (m_Type == Enum_Wall.Wall)
        {
            data.m_MinPos.x -= MapUtil.m_MapGridUnityLen;
            data.m_MaxPos.x += MapUtil.m_MapGridUnityLen;
        }
        else
        {
            data.m_MaxPos.z += MapUtil.m_MapGridUnityLen;
        }

        //Debug.LogWarning("min=" + data.m_MinPos + " max=" + data.m_MaxPos + " size=" + size);
    }

    public Vector3 GetAdjustPar(Vector3 size)
    {
        Vector3 ret = Vector3.one;
        if (m_Type == Enum_Wall.Wall)
        {
            if (((int)size.x) % 2 == 0)
            {
                ret.x = MapUtil.m_MapGridUnityLen;
            }
            else
            {
                ret.x = MapUtil.m_MapGridUnityLen / 2;
            }
        }
        else
        {
            if (((int)size.z) % 2 == 0)
            {
                ret.z = MapUtil.m_MapGridUnityLen;
            }
            else
            {
                ret.z = MapUtil.m_MapGridUnityLen / 2;
            }
        }

        if (((int)size.y) % 2 == 0)
        {
            ret.y = MapUtil.m_MapGridUnityLen;
        }
        else
        {
            size.y = MapUtil.m_MapGridUnityLen / 2;
        }

        return ret;
    }

    /// <summary>
    /// 格子是(x,y)
    /// </summary>
    /// <param name="grid"></param>
    /// <returns></returns>
    public Vector3 Grid2Pos(Vector3 grid)
    {
        Vector3 ret = Vector3.zero;
        if (m_Type == Enum_Wall.Wall)
        {
            ret.x = m_StartPos.x + MapUtil.m_MapGridUnityLen / 2 + grid.x * MapUtil.m_MapGridUnityLen;
        }
        else
        {
            ret.z = m_StartPos.z + MapUtil.m_MapGridUnityLen / 2 + grid.x * MapUtil.m_MapGridUnityLen;
        }
        ret.y = m_StartPos.y + MapUtil.m_MapGridUnityLen / 2 + grid.y * MapUtil.m_MapGridUnityLen;
        return ret;
    }

    /// <summary>
    /// 格子归到(x,y)表示
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Vector3 Pos2Grid(Vector3 pos)
    {
        Vector3 ret = Vector3.zero;
        if (m_Type == Enum_Wall.Wall)
        {
            ret.x = (int)((pos.x - (m_StartPos.x + MapUtil.m_MapGridUnityLen / 2)) / MapUtil.m_MapGridUnityLen);
        }
        else
        {
            ret.x = (int)((pos.z - (m_StartPos.z + MapUtil.m_MapGridUnityLen / 2)) / MapUtil.m_MapGridUnityLen);
        }
        ret.y = (int)((pos.y - (m_StartPos.y + MapUtil.m_MapGridUnityLen / 2)) / MapUtil.m_MapGridUnityLen);
        return ret;
    }

    public bool SetOne(Vector3 pos, Vector3 size)
    {
        Vector3 min = Pos2Grid(GetCornerPos(pos, size, true));
        Vector3 max = Pos2Grid(GetCornerPos(pos, size, false));
        for (int i = (int)min.x; i <= (int)max.x; i++)
        {
            for (int j = (int)min.y; j <= (int)max.y; j++)
            {
                if (m_Flag[i, j] == true)
                {
                    return false;
                }
            }
        }
        for (int i = (int)min.x; i <= (int)max.x; i++)
        {
            for (int j = (int)min.y; j <= (int)max.y; j++)
            {
                m_Flag[i, j] = true;
            }
        }
        return true;
    }

    public void CleanOne(Vector3 pos, Vector3 size)
    {
        Vector3 min = Pos2Grid(GetCornerPos(pos, size, true));
        Vector3 max = Pos2Grid(GetCornerPos(pos, size, false));
        for (int i = (int)min.x; i <= (int)max.x; i++)
        {
            for (int j = (int)min.y; j <= (int)max.y; j++)
            {
                m_Flag[i, j] = false;
            }
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="size"></param>
    /// <param name="min"></param>
    /// <returns></returns>
    public Vector3 GetCornerPos(Vector3 pos, Vector3 size, bool min)
    {
        Vector3 ret = Vector3.zero;
        if (min)
        {
            if (((int)size.y) % 2 == 0)
            {
                ret.y = pos.y - ((int)((size.y - 1) / 2) + 0.5f) * MapUtil.m_MapGridUnityLen;
            }
            else
            {
                ret.y = pos.y - ((int)(size.y / 2)) * MapUtil.m_MapGridUnityLen * 0.5f;
            }

            if (m_Type == Enum_Wall.Wall)
            {
                if (((int)size.x) % 2 == 0)
                {
                    ret.x = pos.x - ((int)((size.x - 1) / 2) + 0.5f) * MapUtil.m_MapGridUnityLen;
                }
                else
                {
                    ret.x = pos.x - ((int)(size.x / 2)) * MapUtil.m_MapGridUnityLen * 0.5f;
                }
            }
            else
            {
                if (((int)size.y) % 2 == 0)
                {
                    ret.z = pos.z - ((int)((size.x - 1) / 2) + 0.5f) * MapUtil.m_MapGridUnityLen;
                }
                else
                {
                    ret.z = pos.z - ((int)(size.x / 2)) * MapUtil.m_MapGridUnityLen * 0.5f;
                }
            }
        }
        else
        {
            if (((int)size.y) % 2 == 0)
            {
                ret.y = pos.y + ((int)((size.y - 1) / 2) + 0.5f) * MapUtil.m_MapGridUnityLen;
            }
            else
            {
                ret.y = pos.y + ((int)(size.y / 2)) * MapUtil.m_MapGridUnityLen * 0.5f;
            }

            if (m_Type == Enum_Wall.Wall)
            {
                if (((int)size.x) % 2 == 0)
                {
                    ret.x = pos.x + ((int)((size.x - 1) / 2) + 0.5f) * MapUtil.m_MapGridUnityLen;
                }
                else
                {
                    ret.x = pos.x + ((int)(size.x / 2)) * MapUtil.m_MapGridUnityLen * 0.5f;
                }
            }
            else
            {
                if (((int)size.y) % 2 == 0)
                {
                    ret.z = pos.z + ((int)((size.x - 1) / 2) + 0.5f) * MapUtil.m_MapGridUnityLen;
                }
                else
                {
                    ret.z = pos.z + ((int)(size.x / 2)) * MapUtil.m_MapGridUnityLen * 0.5f;
                }
            }
        }
        //Debug.LogWarning("corner=" + ret + " min=" + min + " pos=" + pos);
        return ret;
    }
}