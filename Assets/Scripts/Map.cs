using UnityEngine;

public class Map
{
    /// <summary>
    /// 地图大小
    /// </summary>
    public MapUtil.IVector3 m_Size;
    public Vector3 m_StartPos;
    public bool[,] m_Flag;
    public Enum_Layer m_Type;

    public void Init(Enum_Layer type)
    {
        m_Type = type;
        if (m_Type == Enum_Layer.Wall)
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
        else if (m_Type == Enum_Layer.FloorWall)
        {
            m_Flag = new bool[(int)m_Size.x, (int)m_Size.z];
            for (int i = 0; i < (int)m_Size.x; i++)
            {
                for (int j = 0; j < (int)m_Size.z; j++)
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

    public Vector3 AdjustZ2(Vector3 pos)
    {
        if (m_Type == Enum_Layer.Wall)
        {
            pos.z = m_StartPos.z - 0.01f;
        }
        else if (m_Type == Enum_Layer.LeftWall)
        {
            pos.x = m_StartPos.x + 0.01f;
        }
        else if (m_Type == Enum_Layer.RightWall)
        {
            pos.x = m_StartPos.x - 0.01f;
        }
        else if (m_Type == Enum_Layer.FloorWall)
        {
            pos.y = m_StartPos.y + 0.01f;
        }
        return pos;
    }

    public void AdjustZ(MapUtil.IVector3 size, bool floating, ref Vector3 pos)
    {
        if (m_Type == Enum_Layer.Wall)
        {
            pos.z = m_StartPos.z - size.z * MapUtil.m_MapGridUnityLen / 2.0f;
            if (floating)
            {
                pos.z -= MapUtil.m_AdjustZVal;
            }
        }
        else if (m_Type == Enum_Layer.LeftWall)
        {
            pos.x = m_StartPos.x + size.z * MapUtil.m_MapGridUnityLen / 2.0f;
            if (floating)
            {
                pos.x += MapUtil.m_AdjustZVal;
            }
        }
        else if (m_Type == Enum_Layer.RightWall)
        {
            pos.x = m_StartPos.x - size.z * MapUtil.m_MapGridUnityLen / 2.0f;
            if (floating)
            {
                pos.x -= MapUtil.m_AdjustZVal;
            }
        }
        else if (m_Type == Enum_Layer.FloorWall)
        {
            pos.y = m_StartPos.y + size.y * MapUtil.m_MapGridUnityLen / 2.0f;
            if (floating)
            {
                pos.y += MapUtil.m_AdjustZVal;
            }
        }
    }

    public void GetMinMaxPos(MapUtil.IVector3 size, MapUtil.SetType setType, ref DragInitData data)
    {
        if (m_Type == Enum_Layer.Wall)
        {
            data.m_MinPos = m_StartPos
                + new Vector3(size.x * MapUtil.m_MapGridUnityLen / 2, size.y * MapUtil.m_MapGridUnityLen / 2, 0);

            data.m_MaxPos = m_StartPos
            + new Vector3(m_Size.x * MapUtil.m_MapGridUnityLen, m_Size.y * MapUtil.m_MapGridUnityLen, 0)
            - new Vector3(size.x * MapUtil.m_MapGridUnityLen / 2, size.y * MapUtil.m_MapGridUnityLen / 2, 0);

            if (setType == MapUtil.SetType.WallOnFloor)
            {
                data.m_MaxPos.y = data.m_MinPos.y;
            }
        }
        else if (m_Type == Enum_Layer.LeftWall
            || m_Type == Enum_Layer.RightWall)
        {
            data.m_MinPos = m_StartPos
                + new Vector3(0, size.y * MapUtil.m_MapGridUnityLen / 2, size.x * MapUtil.m_MapGridUnityLen / 2);

            data.m_MaxPos = m_StartPos
            + new Vector3(0, m_Size.y * MapUtil.m_MapGridUnityLen, m_Size.z * MapUtil.m_MapGridUnityLen)
            - new Vector3(0, size.y * MapUtil.m_MapGridUnityLen / 2, size.x * MapUtil.m_MapGridUnityLen / 2);

            if (setType == MapUtil.SetType.WallOnFloor)
            {
                data.m_MaxPos.y = data.m_MinPos.y;
            }
        }
        else if (m_Type == Enum_Layer.FloorWall)
        {
            data.m_MinPos = m_StartPos
                + new Vector3(size.x * MapUtil.m_MapGridUnityLen / 2, 0, size.z * MapUtil.m_MapGridUnityLen / 2);

            data.m_MaxPos = m_StartPos
            + new Vector3(m_Size.x * MapUtil.m_MapGridUnityLen, 0, m_Size.z * MapUtil.m_MapGridUnityLen)
            - new Vector3(size.x * MapUtil.m_MapGridUnityLen / 2, 0, size.z * MapUtil.m_MapGridUnityLen / 2);
        }

        if (m_Type == Enum_Layer.Wall)
        {
            data.m_MinPos.x -= MapUtil.m_MapGridUnityLen;
            data.m_MaxPos.x += MapUtil.m_MapGridUnityLen;
        }
        else if (m_Type == Enum_Layer.LeftWall
            || m_Type == Enum_Layer.RightWall)
        {
            data.m_MaxPos.z += MapUtil.m_MapGridUnityLen;
        }

        //Debug.LogWarning("min=" + MapUtil.Vector3String(data.m_MinPos) + " max=" + MapUtil.Vector3String(data.m_MaxPos) + " size=" + size);
    }

    public Vector3 GetAdjustPar(MapUtil.IVector3 size)
    {
        Vector3 ret = Vector3.one;
        if (m_Type == Enum_Layer.Wall)
        {
            if (size.x % 2 == 0)
            {
                ret.x = 0;
            }
            else
            {
                ret.x = MapUtil.m_MapGridUnityLen / 2;
            }

            if (size.y % 2 == 0)
            {
                ret.y = 0;
            }
            else
            {
                ret.y = MapUtil.m_MapGridUnityLen / 2;
            }
        }
        else if (m_Type == Enum_Layer.FloorWall)
        {
            if (size.x % 2 == 0)
            {
                ret.x = 0;
            }
            else
            {
                ret.x = MapUtil.m_MapGridUnityLen / 2;
            }

            if (size.z % 2 == 0)
            {
                ret.z = 0;
            }
            else
            {
                ret.z = MapUtil.m_MapGridUnityLen / 2;
            }
        }
        else if(m_Type == Enum_Layer.LeftWall
            || m_Type == Enum_Layer.RightWall)
        {
            if (size.z % 2 == 0)
            {
                ret.z = 0;
            }
            else
            {
                ret.z = MapUtil.m_MapGridUnityLen / 2;
            }

            if (size.y % 2 == 0)
            {
                ret.y = 0;
            }
            else
            {
                ret.y = MapUtil.m_MapGridUnityLen / 2;
            }
        }

        //Debug.LogWarning("ret " + ret.x + " " + ret.y + " " + ret.z);

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
        if (m_Type == Enum_Layer.Wall)
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
    public MapUtil.IVector3 Pos2Grid(Vector3 pos)
    {
        MapUtil.IVector3 ret = new MapUtil.IVector3(0, 0, 0);
        if (m_Type == Enum_Layer.Wall)
        {
            ret.x = Mathf.RoundToInt((pos.x - (m_StartPos.x + MapUtil.m_MapGridUnityLen / 2)) / MapUtil.m_MapGridUnityLen);
            ret.y = Mathf.RoundToInt((pos.y - (m_StartPos.y + MapUtil.m_MapGridUnityLen / 2)) / MapUtil.m_MapGridUnityLen);
        }
        else if(m_Type == Enum_Layer.LeftWall
            || m_Type == Enum_Layer.RightWall)
        {
            ret.x = Mathf.RoundToInt((pos.z - (m_StartPos.z + MapUtil.m_MapGridUnityLen / 2)) / MapUtil.m_MapGridUnityLen);
            ret.y = Mathf.RoundToInt((pos.y - (m_StartPos.y + MapUtil.m_MapGridUnityLen / 2)) / MapUtil.m_MapGridUnityLen);
        }
        else if(m_Type == Enum_Layer.FloorWall)
        {
            ret.x = Mathf.RoundToInt((pos.x - (m_StartPos.x + MapUtil.m_MapGridUnityLen / 2)) / MapUtil.m_MapGridUnityLen);
            ret.y = Mathf.RoundToInt((pos.z - (m_StartPos.z + MapUtil.m_MapGridUnityLen / 2)) / MapUtil.m_MapGridUnityLen);
        }
        return ret;
    }

    public bool JudgeSet(Vector3 pos, MapUtil.IVector3 size, bool mainJudge = true)
    {
        MapUtil.IVector3 min = Pos2Grid(GetCornerPos(pos, size, true));
        MapUtil.IVector3 max = Pos2Grid(GetCornerPos(pos, size, false));

        //Debug.LogWarning("min=" + min.ToString() + " max=" + max.ToString() + " size=" + size
        //    + " type=" + m_Type + " pos=" + MapUtil.Vector3String(pos) + " main=" + mainJudge);

        for (int i = min.x; i <= max.x; i++)
        {
            for (int j = min.y; j <= max.y; j++)
            {
                if (m_Flag[i, j] == true)
                {
                    return false;
                }
            }
        }
        bool ret = true;
        if (mainJudge)
        {
            if (m_Type == Enum_Layer.Wall)
            {
                if (min.x == 0)
                {
                    ret = MapUtil.GetMap(Enum_Layer.LeftWall).JudgeSet(pos + new Vector3(0, 0, MapUtil.m_AdjustZVal), new MapUtil.IVector3(size.z, size.y, size.x), false);
                    if (ret == false)
                    {
                        return ret;
                    }
                }
                if (max.x + 1 == m_Size.x)
                {
                    ret = MapUtil.GetMap(Enum_Layer.RightWall).JudgeSet(pos + new Vector3(0, 0, MapUtil.m_AdjustZVal), new MapUtil.IVector3(size.z, size.y, size.x), false);
                    if (ret == false)
                    {
                        return ret;
                    }
                }

                if (min.y == 0)
                {
                    ret = MapUtil.GetMap(Enum_Layer.FloorWall).JudgeSet(pos + new Vector3(0, 0, MapUtil.m_AdjustZVal), new MapUtil.IVector3(size.x, size.y, size.z), false);
                    if (ret == false)
                    {
                        return ret;
                    }
                }
            }
            else if (m_Type == Enum_Layer.LeftWall)
            {
                if (max.x + 1 == m_Size.z)
                {
                    ret = MapUtil.GetMap(Enum_Layer.Wall).JudgeSet(pos - new Vector3(MapUtil.m_AdjustZVal, 0, 0), new MapUtil.IVector3(size.z, size.y, size.x), false);
                    if (ret == false)
                    {
                        return ret;
                    }
                }

                if (min.y == 0)
                {
                    ret = MapUtil.GetMap(Enum_Layer.FloorWall).JudgeSet(pos - new Vector3(0, 0, MapUtil.m_AdjustZVal), new MapUtil.IVector3(size.z, size.y, size.x), false);
                    if (ret == false)
                    {
                        return ret;
                    }
                }
            }
            else if (m_Type == Enum_Layer.RightWall)
            {
                if (max.x + 1 == m_Size.z)
                {
                    ret = MapUtil.GetMap(Enum_Layer.Wall).JudgeSet(pos + new Vector3(MapUtil.m_AdjustZVal, 0, 0), new MapUtil.IVector3(size.z, size.y, size.x), false);
                    if (ret == false)
                    {
                        return ret;
                    }
                }

                if (min.y == 0)
                {
                    ret = MapUtil.GetMap(Enum_Layer.FloorWall).JudgeSet(pos + new Vector3(0, 0, MapUtil.m_AdjustZVal), new MapUtil.IVector3(size.z, size.y, size.x), false);
                    if (ret == false)
                    {
                        return ret;
                    }
                }
            }
            else if (m_Type == Enum_Layer.FloorWall)
            {
                if (max.y + 1 == m_Size.z)
                {
                    ret = MapUtil.GetMap(Enum_Layer.Wall).JudgeSet(pos - new Vector3(0, MapUtil.m_AdjustZVal, 0), new MapUtil.IVector3(size.x, size.y, size.z), false);
                    if (ret == false)
                    {
                        return ret;
                    }
                }

                if (min.x == 0)
                {
                    ret = MapUtil.GetMap(Enum_Layer.LeftWall).JudgeSet(pos - new Vector3(0, MapUtil.m_AdjustZVal, 0), new MapUtil.IVector3(size.z, size.y, size.x), false);
                    if (ret == false)
                    {
                        return ret;
                    }
                }

                if (max.x + 1 == m_Size.x)
                {
                    ret = MapUtil.GetMap(Enum_Layer.RightWall).JudgeSet(pos - new Vector3(0, MapUtil.m_AdjustZVal, 0), new MapUtil.IVector3(size.z, size.y, size.x), false);
                    if (ret == false)
                    {
                        return ret;
                    }
                }
            }
        }
        return true;
    }

    public bool SetOne(Vector3 pos, MapUtil.IVector3 size, bool mainJudge = true)
    {
        if (mainJudge)
        {
            if (!JudgeSet(pos, size, true))
            {
                return false;
            }
        }

        MapUtil.IVector3 min = Pos2Grid(GetCornerPos(pos, size, true));
        MapUtil.IVector3 max = Pos2Grid(GetCornerPos(pos, size, false));

        for (int i = min.x; i <= max.x; i++)
        {
            for (int j = min.y; j <= max.y; j++)
            {
                m_Flag[i, j] = true;
            }
        }

        //Debug.LogWarning("main=" + mainJudge + " min=" + min + " max=" + max + " size=" + size + " pos=" + pos);

        if (mainJudge)
        {
            if (m_Type == Enum_Layer.Wall)
            {
                if (min.x == 0)
                {
                    MapUtil.GetMap(Enum_Layer.LeftWall).SetOne(pos + new Vector3(0, 0, MapUtil.m_AdjustZVal), new MapUtil.IVector3(size.z, size.y, size.x), false);
                }
                if (max.x + 1 == m_Size.x)
                {
                    MapUtil.GetMap(Enum_Layer.RightWall).SetOne(pos + new Vector3(0, 0, MapUtil.m_AdjustZVal), new MapUtil.IVector3(size.z, size.y, size.x), false);
                }
                if (min.y == 0)
                {
                    MapUtil.GetMap(Enum_Layer.FloorWall).SetOne(pos + new Vector3(0, 0, MapUtil.m_AdjustZVal), new MapUtil.IVector3(size.x, size.y, size.z), false);
                }
            }
            else if (m_Type == Enum_Layer.LeftWall)
            {
                if (max.x + 1 == m_Size.z)
                {
                    MapUtil.GetMap(Enum_Layer.Wall).SetOne(pos - new Vector3(MapUtil.m_AdjustZVal, 0, 0), new MapUtil.IVector3(size.z, size.y, size.x), false);
                }

                if (min.y == 0)
                {
                    MapUtil.GetMap(Enum_Layer.FloorWall).SetOne(pos - new Vector3(0, 0, MapUtil.m_AdjustZVal), new MapUtil.IVector3(size.z, size.y, size.x), false);
                }
            }
            else if (m_Type == Enum_Layer.RightWall)
            {
                if (max.x + 1 == m_Size.z)
                {
                    MapUtil.GetMap(Enum_Layer.Wall).SetOne(pos + new Vector3(MapUtil.m_AdjustZVal, 0, 0), new MapUtil.IVector3(size.z, size.y, size.x), false);
                }

                if (min.y == 0)
                {
                    MapUtil.GetMap(Enum_Layer.FloorWall).SetOne(pos + new Vector3(0, 0, MapUtil.m_AdjustZVal), new MapUtil.IVector3(size.z, size.y, size.x), false);
                }
            }
            else if (m_Type == Enum_Layer.FloorWall)
            {
                if (max.y + 1 == m_Size.z)
                {
                    MapUtil.GetMap(Enum_Layer.Wall).SetOne(pos - new Vector3(0, MapUtil.m_AdjustZVal, 0), new MapUtil.IVector3(size.x, size.y, size.z), false);
                }

                if (min.x == 0)
                {
                    MapUtil.GetMap(Enum_Layer.LeftWall).SetOne(pos - new Vector3(0, MapUtil.m_AdjustZVal, 0), new MapUtil.IVector3(size.z, size.y, size.x), false);
                }

                if (max.x + 1 == m_Size.x)
                {
                    MapUtil.GetMap(Enum_Layer.RightWall).SetOne(pos - new Vector3(0, MapUtil.m_AdjustZVal, 0), new MapUtil.IVector3(size.z, size.y, size.x), false);
                }
            }
        }

        return true;
    }

    public void CleanOne(Vector3 pos, MapUtil.IVector3 size, bool mainJudge = true)
    {
        MapUtil.IVector3 min = Pos2Grid(GetCornerPos(pos, size, true));
        MapUtil.IVector3 max = Pos2Grid(GetCornerPos(pos, size, false));
        for (int i = min.x; i <= max.x; i++)
        {
            for (int j = min.y; j <= max.y; j++)
            {
                m_Flag[i, j] = false;
            }
        }

        if (mainJudge)
        {
            if (m_Type == Enum_Layer.Wall)
            {
                if (min.x == 0)
                {
                    MapUtil.GetMap(Enum_Layer.LeftWall).CleanOne(pos + new Vector3(0, 0, MapUtil.m_AdjustZVal), new MapUtil.IVector3(size.z, size.y, size.x), false);
                }
                if (max.x + 1 == m_Size.x)
                {
                    MapUtil.GetMap(Enum_Layer.RightWall).CleanOne(pos + new Vector3(0, 0, MapUtil.m_AdjustZVal), new MapUtil.IVector3(size.z, size.y, size.x), false);
                }

                if (min.y == 0)
                {
                    MapUtil.GetMap(Enum_Layer.FloorWall).CleanOne(pos + new Vector3(0, 0, MapUtil.m_AdjustZVal), new MapUtil.IVector3(size.x, size.y, size.z), false);
                }
            }
            else if (m_Type == Enum_Layer.LeftWall)
            {
                if (max.x + 1 == m_Size.z)
                {
                    MapUtil.GetMap(Enum_Layer.Wall).CleanOne(pos - new Vector3(MapUtil.m_AdjustZVal, 0, 0), new MapUtil.IVector3(size.z, size.y, size.x), false);
                }

                if (min.y == 0)
                {
                    MapUtil.GetMap(Enum_Layer.FloorWall).CleanOne(pos - new Vector3(0, 0, MapUtil.m_AdjustZVal), new MapUtil.IVector3(size.z, size.y, size.x), false);
                }
            }
            else if(m_Type == Enum_Layer.RightWall)
            {
                if (max.x + 1 == m_Size.z)
                {
                    MapUtil.GetMap(Enum_Layer.Wall).CleanOne(pos + new Vector3(MapUtil.m_AdjustZVal, 0, 0), new MapUtil.IVector3(size.z, size.y, size.x), false);
                }

                if (min.y == 0)
                {
                    MapUtil.GetMap(Enum_Layer.FloorWall).CleanOne(pos + new Vector3(0, 0, MapUtil.m_AdjustZVal), new MapUtil.IVector3(size.z, size.y, size.x), false);
                }
            }
            else if (m_Type == Enum_Layer.FloorWall)
            {
                if (max.y + 1 == m_Size.z)
                {
                    MapUtil.GetMap(Enum_Layer.Wall).CleanOne(pos - new Vector3(0, MapUtil.m_AdjustZVal, 0), new MapUtil.IVector3(size.x, size.y, size.z), false);
                }

                if (min.x == 0)
                {
                    MapUtil.GetMap(Enum_Layer.LeftWall).CleanOne(pos - new Vector3(0, MapUtil.m_AdjustZVal, 0), new MapUtil.IVector3(size.z, size.y, size.x), false);
                }

                if (max.x + 1 == m_Size.x)
                {
                    MapUtil.GetMap(Enum_Layer.RightWall).CleanOne(pos - new Vector3(0, MapUtil.m_AdjustZVal, 0), new MapUtil.IVector3(size.z, size.y, size.x), false);
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="size"></param>
    /// <param name="min"></param>
    /// <returns></returns>
    public Vector3 GetCornerPos(Vector3 pos, MapUtil.IVector3 size, bool min)
    {
        Vector3 ret = m_StartPos;
        if (min)
        {
            if (m_Type == Enum_Layer.Wall)
            {
                if (size.x % 2 == 0)
                {
                    if (size.x == 0)
                    {
                        ret.x = m_StartPos.x + (m_Size.x + 1) * MapUtil.m_MapGridUnityLen;//变成MAX+1
                    }
                    else
                    {
                        ret.x = pos.x - ((size.x - 1) / 2 + 0.5f) * MapUtil.m_MapGridUnityLen;
                    }
                }
                else
                {
                    ret.x = pos.x - (size.x / 2) * MapUtil.m_MapGridUnityLen;
                }

                if (size.y % 2 == 0)
                {
                    if(size.y == 0)
                    {
                        ret.y = m_StartPos.y + (m_Size.y + 1) * MapUtil.m_MapGridUnityLen;//变成MAX+1
                    }
                    else
                    {
                        ret.y = pos.y - ((size.y - 1) / 2 + 0.5f) * MapUtil.m_MapGridUnityLen;
                    }
                }
                else
                {
                    ret.y = pos.y - (size.y / 2) * MapUtil.m_MapGridUnityLen;
                }
            }
            else if(m_Type == Enum_Layer.LeftWall
                || m_Type == Enum_Layer.RightWall)
            {
                if (size.x % 2 == 0)
                {
                    if (size.x == 0)
                    {
                        ret.z = m_StartPos.z + (m_Size.z + 1) * MapUtil.m_MapGridUnityLen;//变成MAX+1
                    }
                    else
                    {
                        ret.z = pos.z - (size.x / 2 - 1 + 0.5f) * MapUtil.m_MapGridUnityLen;
                    }
                }
                else
                {
                    ret.z = pos.z - (size.x / 2) * MapUtil.m_MapGridUnityLen;
                }

                if (size.y % 2 == 0)
                {
                    if (size.y == 0)
                    {
                        ret.y = m_StartPos.y + (m_Size.y + 1) * MapUtil.m_MapGridUnityLen;//变成MAX+1
                    }
                    else
                    {
                        ret.y = pos.y - (size.y / 2 - 1 + 0.5f) * MapUtil.m_MapGridUnityLen;
                    }
                }
                else
                {
                    ret.y = pos.y - (size.y / 2) * MapUtil.m_MapGridUnityLen;
                }
            }
            else if (m_Type == Enum_Layer.FloorWall)
            {
                if (size.x % 2 == 0)
                {
                    if (size.x == 0)
                    {
                        ret.x = m_StartPos.x + (m_Size.x + 1) * MapUtil.m_MapGridUnityLen;//变成MAX+1
                    }
                    else
                    {
                        ret.x = pos.x - (size.x / 2 - 1 + 0.5f) * MapUtil.m_MapGridUnityLen;
                    }
                }
                else
                {
                    ret.x = pos.x - (size.x / 2) * MapUtil.m_MapGridUnityLen;
                }

                if (size.z % 2 == 0)
                {
                    if (size.z == 0)
                    {
                        ret.z = m_StartPos.z + (m_Size.z + 1) * MapUtil.m_MapGridUnityLen;//变成MAX+1
                    }
                    else
                    {
                        ret.z = pos.z - (size.z / 2 - 1 + 0.5f) * MapUtil.m_MapGridUnityLen;
                    }
                }
                else
                {
                    ret.z = pos.z - (size.z / 2) * MapUtil.m_MapGridUnityLen;
                }
            }
        }
        else
        {
            if (m_Type == Enum_Layer.Wall)
            {
                if (size.x % 2 == 0)
                {
                    if (size.x == 0)
                    {
                        ret.x = m_StartPos.x + m_Size.x * MapUtil.m_MapGridUnityLen;//变成MAX
                    }
                    else
                    {
                        ret.x = pos.x + (size.x / 2 - 1 + 0.5f) * MapUtil.m_MapGridUnityLen;
                    }
                }
                else
                {
                    ret.x = pos.x + (size.x / 2) * MapUtil.m_MapGridUnityLen;
                }

                if (size.y % 2 == 0)
                {
                    if (size.y == 0)
                    {
                        ret.y = m_StartPos.y + m_Size.y * MapUtil.m_MapGridUnityLen;//变成MAX
                    }
                    else
                    {
                        ret.y = pos.y + (size.y / 2 - 1 + 0.5f) * MapUtil.m_MapGridUnityLen;
                    }
                }
                else
                {
                    ret.y = pos.y + (size.y / 2) * MapUtil.m_MapGridUnityLen;
                }
            }
            else if(m_Type == Enum_Layer.LeftWall
                || m_Type == Enum_Layer.RightWall)
            {
                if (size.x % 2 == 0)
                {
                    if (size.x == 0)
                    {
                        ret.z = m_StartPos.z + m_Size.z * MapUtil.m_MapGridUnityLen;//变成MAX
                    }
                    else
                    {
                        ret.z = pos.z + (size.x / 2 - 1 + 0.5f) * MapUtil.m_MapGridUnityLen;
                    }
                }
                else
                {
                    ret.z = pos.z + (size.x / 2) * MapUtil.m_MapGridUnityLen;
                }

                if (size.y % 2 == 0)
                {
                    if (size.y == 0)
                    {
                        ret.y = m_StartPos.y + m_Size.y * MapUtil.m_MapGridUnityLen;//变成MAX
                    }
                    else
                    {
                        ret.y = pos.y + (size.y / 2 - 1 + 0.5f) * MapUtil.m_MapGridUnityLen;
                    }
                }
                else
                {
                    ret.y = pos.y + (size.y / 2) * MapUtil.m_MapGridUnityLen;
                }
            }
            else if(m_Type == Enum_Layer.FloorWall)
            {
                if (size.x % 2 == 0)
                {
                    if (size.x == 0)
                    {
                        ret.x = m_StartPos.x + m_Size.x * MapUtil.m_MapGridUnityLen;//变成MAX
                    }
                    else
                    {
                        ret.x = pos.x + ((size.x - 1) / 2 + 0.5f) * MapUtil.m_MapGridUnityLen;
                    }
                }
                else
                {
                    ret.x = pos.x + (size.x / 2) * MapUtil.m_MapGridUnityLen;
                }

                if (size.z % 2 == 0)
                {
                    if (size.z == 0)
                    {
                        ret.z = m_StartPos.z + m_Size.z * MapUtil.m_MapGridUnityLen;//变成MAX
                    }
                    else
                    {
                        ret.z = pos.z + (size.z / 2 - 1 + 0.5f) * MapUtil.m_MapGridUnityLen;
                    }
                }
                else
                {
                    ret.z = pos.z + (size.z / 2) * MapUtil.m_MapGridUnityLen;
                }
            }
        }
        //Debug.LogWarning("corner=" + ret + " min=" + min + " pos=" + pos);
        return ret;
    }
}