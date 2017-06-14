using UnityEngine;

public class Map
{
    /// <summary>
    /// 地图大小，XYZ三个方向
    /// </summary>
    public MapUtil.IVector3 m_Size;
    public Vector3 m_StartPos;

    private Enum_Layer m_Type;

    /// <summary>
    /// 地图大小，XY两个方向
    /// </summary>
    private MapUtil.IVector3 m_SizeXY;
    /// <summary>
    /// 地图标记
    /// </summary>
    private bool[, ,] m_Flag;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="type"></param>
    public void Init(Enum_Layer type)
    {
        m_Type = type;

        m_SizeXY = new MapUtil.IVector3(0, 0, 0);
        m_SizeXY.x = (m_Type == Enum_Layer.Wall || m_Type == Enum_Layer.FloorWall) ? m_Size.x : m_Size.z;
        m_SizeXY.y = (m_Type == Enum_Layer.FloorWall) ? m_Size.z : m_Size.y;

        m_Flag = new bool[2, m_SizeXY.x, m_SizeXY.y];
        for (int k = 0; k < 2; k++)
        {
            for (int i = 0; i < m_SizeXY.x; i++)
            {
                for (int j = 0; j < m_SizeXY.y; j++)
                {
                    m_Flag[k, i, j] = false;
                }
            }
        }
    }

    public void ResetMapStartPosY()
    {
        m_StartPos.y = GameApp.Inst.GetHouseYOffset;
    }

    public void ResetMapFlag()
    {
        for (int i = 0; i < m_SizeXY.x; i++)
        {
            for (int j = 0; j < m_SizeXY.y; j++)
            {
                m_Flag[GameApp.Inst.CurNodeIdx, i, j] = false;
            }
        }
    }

    /// <summary>
    /// 粘着墙
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="interval">默认是影子的配置</param>
    /// <returns></returns>
    public Vector3 Adjust2Wall(Vector3 pos, float interval = 0.01f)
    {
        if (m_Type == Enum_Layer.Wall)
        {
            pos.z = m_StartPos.z - interval;
        }
        else if (m_Type == Enum_Layer.LeftWall)
        {
            pos.x = m_StartPos.x + interval;
        }
        else if (m_Type == Enum_Layer.RightWall)
        {
            pos.x = m_StartPos.x - interval;
        }
        else if (m_Type == Enum_Layer.FloorWall)
        {
            pos.y = m_StartPos.y + interval;
        }
        return pos;
    }

    /// <summary>
    /// 家具粘着墙
    /// </summary>
    /// <param name="size">size指的是xyz方向的长度，所以到侧墙，x和z应该互换</param>
    /// <param name="floating"></param>
    /// <param name="pos"></param>
    public Vector3 AdjustFurn2Wall2(MapUtil.IVector3 size, bool floating, Vector3 pos)
    {
        if (m_Type == Enum_Layer.Wall)
        {
            pos.z = m_StartPos.z - size.z * MapUtil.m_MapGridUnityLen * 0.5f;
            if (floating)
            {
                pos.z -= MapUtil.m_AdjustFurn2WallPar;
            }
        }
        else if (m_Type == Enum_Layer.LeftWall)
        {
            pos.x = m_StartPos.x + size.x * MapUtil.m_MapGridUnityLen * 0.5f;
            if (floating)
            {
                pos.x += MapUtil.m_AdjustFurn2WallPar;
            }
        }
        else if (m_Type == Enum_Layer.RightWall)
        {
            pos.x = m_StartPos.x - size.x * MapUtil.m_MapGridUnityLen * 0.5f;
            if (floating)
            {
                pos.x -= MapUtil.m_AdjustFurn2WallPar;
            }
        }
        else if (m_Type == Enum_Layer.FloorWall)
        {
            pos.y = m_StartPos.y + size.y * MapUtil.m_MapGridUnityLen * 0.5f;
            if (floating)
            {
                pos.y += MapUtil.m_AdjustFurn2WallPar;
            }
        }
        return pos;
    }

    /// <summary>
    /// 家具粘着墙
    /// </summary>
    /// <param name="size">size指的是xyz方向的长度，所以到侧墙，x和z应该互换</param>
    /// <param name="floating"></param>
    /// <param name="pos"></param>
    public void AdjustFurn2Wall(MapUtil.IVector3 size, bool floating, ref Vector3 pos)
    {
        pos = AdjustFurn2Wall2(size, floating, pos);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="size">size指的是xyz方向的长度，所以到侧墙，x和z应该互换</param>
    /// <param name="setType"></param>
    /// <param name="data"></param>
    public void GetMinMaxPos(MapUtil.IVector3 size, MapUtil.SetType setType, ref DragInitData data)
    {
        data.m_MinPos = m_StartPos + size.MulVal(MapUtil.m_MapGridUnityLen * 0.5f);
        data.m_MaxPos = m_StartPos + m_Size.MulVal(MapUtil.m_MapGridUnityLen) - size.MulVal(MapUtil.m_MapGridUnityLen * 0.5f);
        if (setType == MapUtil.SetType.WallOnFloor)
        {
            data.m_MaxPos.y = data.m_MinPos.y;
        }
        else if (setType == MapUtil.SetType.Wall)//墙上的不能贴地
        {
            data.m_MinPos.y += MapUtil.m_MapGridUnityLen;
        }

        //墙面可以互跳
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

        data.m_MinPos = Adjust2Wall(data.m_MinPos, 0f);
        data.m_MaxPos = Adjust2Wall(data.m_MaxPos, 0f);

        //Debug.LogWarning("min=" + MapUtil.Vector3String(data.m_MinPos) + " max=" + MapUtil.Vector3String(data.m_MaxPos) + " size=" + size);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="size">size指的是xyz方向的长度，所以到侧墙，x和z应该互换</param>
    /// <returns></returns>
    public Vector3 GetAdjustPar(MapUtil.IVector3 size)
    {
        Vector3 ret = Vector3.one;

        if ((size.x & 1) == 0)
        {
            ret.x = 0;
        }
        else
        {
            ret.x = MapUtil.m_MapGridUnityLen / 2;
        }

        if ((size.y & 1) == 0)
        {
            ret.y = 0;
        }
        else
        {
            ret.y = MapUtil.m_MapGridUnityLen / 2;
        }

        if ((size.z & 1) == 0)
        {
            ret.z = 0;
        }
        else
        {
            ret.z = MapUtil.m_MapGridUnityLen / 2;
        }

        //Debug.LogWarning("size=" + size + " ret=" + MapUtil.Vector3String(ret) + " " + m_Type);

        return ret;
    }

    /// <summary>
    /// 格子是(x,y,z)
    /// </summary>
    /// <param name="grid"></param>
    /// <returns></returns>
    public Vector3 Grid2Pos(MapUtil.IVector3 grid)
    {
        Vector3 ret = m_StartPos + Vector3.one * MapUtil.m_MapGridUnityLen * 0.5f + grid.MulVal(MapUtil.m_MapGridUnityLen);
        ret = Adjust2Wall(ret, 0f);
        return ret;
    }

    /// <summary>
    /// 格子归到(x,y,z)表示
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public MapUtil.IVector3 Pos2Grid(Vector3 pos)
    {
        MapUtil.IVector3 ret = new MapUtil.IVector3(0, 0, 0);
        pos = (pos - (m_StartPos + Vector3.one * MapUtil.m_MapGridUnityLen * 0.5f)) / MapUtil.m_MapGridUnityLen;

        ret.x = Mathf.RoundToInt(pos.x);
        ret.y = Mathf.RoundToInt(pos.y);
        ret.z = Mathf.RoundToInt(pos.z);

        return ret;
    }

    /// <summary>
    /// 格子(x,y,z)->(x,y)
    /// </summary>
    /// <param name="grid"></param>
    /// <returns></returns>
    private MapUtil.IVector3 GridXYZ2XY(MapUtil.IVector3 grid)
    {
        MapUtil.IVector3 ret = new MapUtil.IVector3(0, 0, 0);
        ret.x = (m_Type == Enum_Layer.Wall || m_Type == Enum_Layer.FloorWall) ? grid.x : grid.z;
        ret.y = (m_Type == Enum_Layer.FloorWall) ? grid.z : grid.y;
        return ret;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="size">size指的是xyz方向的长度，所以到侧墙，x和z应该互换</param>
    /// <param name="mainJudge"></param>
    /// <returns></returns>
    public bool JudgeSet(Vector3 pos, MapUtil.IVector3 size, bool mainJudge = true)
    {
        MapUtil.IVector3 min = GridXYZ2XY(Pos2Grid(GetCornerPos(pos, size, true)));
        MapUtil.IVector3 max = GridXYZ2XY(Pos2Grid(GetCornerPos(pos, size, false)));

        //Debug.LogWarning("min=" + min.ToString() + " max=" + max.ToString() + " size=" + size
        //+ " type=" + m_Type + " pos=" + MapUtil.Vector3String(pos) + " main=" + mainJudge + " " + GameApp.Inst.CurNodeIdx);

        for (int i = min.x; i <= max.x; i++)
        {
            for (int j = min.y; j <= max.y; j++)
            {
                if (m_Flag[GameApp.Inst.CurNodeIdx, i, j] == true)
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
                    ret = MapUtil.GetMap(Enum_Layer.LeftWall).JudgeSet(AdjustFurn2Wall2(size, false, pos), size, false);
                    if (ret == false)
                    {
                        return ret;
                    }
                }
                if (max.x + 1 == m_Size.x)
                {
                    ret = MapUtil.GetMap(Enum_Layer.RightWall).JudgeSet(AdjustFurn2Wall2(size, false, pos), size, false);
                    if (ret == false)
                    {
                        return ret;
                    }
                }

                if (min.y == 0)
                {
                    ret = MapUtil.GetMap(Enum_Layer.FloorWall).JudgeSet(AdjustFurn2Wall2(size, false, pos), size, false);
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
                    ret = MapUtil.GetMap(Enum_Layer.Wall).JudgeSet(AdjustFurn2Wall2(size, false, pos), size, false);
                    if (ret == false)
                    {
                        return ret;
                    }
                }

                if (min.y == 0)
                {
                    ret = MapUtil.GetMap(Enum_Layer.FloorWall).JudgeSet(AdjustFurn2Wall2(size, false, pos), size, false);
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
                    ret = MapUtil.GetMap(Enum_Layer.Wall).JudgeSet(AdjustFurn2Wall2(size, false, pos), size, false);
                    if (ret == false)
                    {
                        return ret;
                    }
                }

                if (min.y == 0)
                {
                    ret = MapUtil.GetMap(Enum_Layer.FloorWall).JudgeSet(AdjustFurn2Wall2(size, false, pos), size, false);
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
                    ret = MapUtil.GetMap(Enum_Layer.Wall).JudgeSet(AdjustFurn2Wall2(size, false, pos), size, false);
                    if (ret == false)
                    {
                        return ret;
                    }
                }

                if (min.x == 0)
                {
                    ret = MapUtil.GetMap(Enum_Layer.LeftWall).JudgeSet(AdjustFurn2Wall2(size, false, pos), size, false);
                    if (ret == false)
                    {
                        return ret;
                    }
                }

                if (max.x + 1 == m_Size.x)
                {
                    ret = MapUtil.GetMap(Enum_Layer.RightWall).JudgeSet(AdjustFurn2Wall2(size, false, pos), size, false);
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

        MapUtil.IVector3 min = GridXYZ2XY(Pos2Grid(GetCornerPos(pos, size, true)));
        MapUtil.IVector3 max = GridXYZ2XY(Pos2Grid(GetCornerPos(pos, size, false)));

        for (int i = min.x; i <= max.x; i++)
        {
            for (int j = min.y; j <= max.y; j++)
            {
                m_Flag[GameApp.Inst.CurNodeIdx, i, j] = true;
            }
        }

        //Debug.LogWarning("main=" + mainJudge + " min=" + min + " max=" + max + " size=" + size + " pos=" + pos);

        if (mainJudge)
        {
            if (m_Type == Enum_Layer.Wall)
            {
                if (min.x == 0)
                {
                    MapUtil.GetMap(Enum_Layer.LeftWall).SetOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }
                if (max.x + 1 == m_Size.x)
                {
                    MapUtil.GetMap(Enum_Layer.RightWall).SetOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }
                if (min.y == 0)
                {
                    MapUtil.GetMap(Enum_Layer.FloorWall).SetOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }
            }
            else if (m_Type == Enum_Layer.LeftWall)
            {
                if (max.x + 1 == m_Size.z)
                {
                    MapUtil.GetMap(Enum_Layer.Wall).SetOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }

                if (min.y == 0)
                {
                    MapUtil.GetMap(Enum_Layer.FloorWall).SetOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }
            }
            else if (m_Type == Enum_Layer.RightWall)
            {
                if (max.x + 1 == m_Size.z)
                {
                    MapUtil.GetMap(Enum_Layer.Wall).SetOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }

                if (min.y == 0)
                {
                    MapUtil.GetMap(Enum_Layer.FloorWall).SetOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }
            }
            else if (m_Type == Enum_Layer.FloorWall)
            {
                if (max.y + 1 == m_Size.z)
                {
                    MapUtil.GetMap(Enum_Layer.Wall).SetOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }

                if (min.x == 0)
                {
                    MapUtil.GetMap(Enum_Layer.LeftWall).SetOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }

                if (max.x + 1 == m_Size.x)
                {
                    MapUtil.GetMap(Enum_Layer.RightWall).SetOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }
            }
        }

        return true;
    }

    public void CleanOne(Vector3 pos, MapUtil.IVector3 size, bool mainJudge = true)
    {
        MapUtil.IVector3 min = GridXYZ2XY(Pos2Grid(GetCornerPos(pos, size, true)));
        MapUtil.IVector3 max = GridXYZ2XY(Pos2Grid(GetCornerPos(pos, size, false)));

        //Debug.LogWarning("xxx " + GameApp.Inst.CurNodeIdx + " size=" + size + " pos=" + MapUtil.Vector3String(pos)
        //    + " min=" + min + " max=" + max);

        for (int i = min.x; i <= max.x; i++)
        {
            for (int j = min.y; j <= max.y; j++)
            {
                m_Flag[GameApp.Inst.CurNodeIdx, i, j] = false;
            }
        }

        if (mainJudge)
        {
            if (m_Type == Enum_Layer.Wall)
            {
                if (min.x == 0)
                {
                    MapUtil.GetMap(Enum_Layer.LeftWall).CleanOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }
                if (max.x + 1 == m_Size.x)
                {
                    MapUtil.GetMap(Enum_Layer.RightWall).CleanOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }

                if (min.y == 0)
                {
                    MapUtil.GetMap(Enum_Layer.FloorWall).CleanOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }
            }
            else if (m_Type == Enum_Layer.LeftWall)
            {
                if (max.x + 1 == m_Size.z)
                {
                    MapUtil.GetMap(Enum_Layer.Wall).CleanOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }

                if (min.y == 0)
                {
                    MapUtil.GetMap(Enum_Layer.FloorWall).CleanOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }
            }
            else if (m_Type == Enum_Layer.RightWall)
            {
                if (max.x + 1 == m_Size.z)
                {
                    MapUtil.GetMap(Enum_Layer.Wall).CleanOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }

                if (min.y == 0)
                {
                    MapUtil.GetMap(Enum_Layer.FloorWall).CleanOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }
            }
            else if (m_Type == Enum_Layer.FloorWall)
            {
                if (max.y + 1 == m_Size.z)
                {
                    MapUtil.GetMap(Enum_Layer.Wall).CleanOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }

                if (min.x == 0)
                {
                    MapUtil.GetMap(Enum_Layer.LeftWall).CleanOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }

                if (max.x + 1 == m_Size.x)
                {
                    MapUtil.GetMap(Enum_Layer.RightWall).CleanOne(AdjustFurn2Wall2(size, false, pos), size, false);
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="size">size指的是xyz方向的长度，所以到侧墙，x和z应该互换</param>
    /// <param name="min"></param>
    /// <returns></returns>
    public Vector3 GetCornerPos(Vector3 pos, MapUtil.IVector3 size, bool min)
    {
        Vector3 ret = m_StartPos;
        int mulPar = min ? 1 : -1;

        if ((size.x & 1) == 0)
        {
            if (size.x == 0)
            {
                ret.x = m_StartPos.x + (m_Size.x + 1 + mulPar) * MapUtil.m_MapGridUnityLen;//MIN要大于MAX，都大于边界
            }
            else
            {
                ret.x = pos.x - mulPar * ((size.x - 1) / 2 + 0.5f) * MapUtil.m_MapGridUnityLen;
            }
        }
        else
        {
            ret.x = pos.x - mulPar * (size.x / 2) * MapUtil.m_MapGridUnityLen;
        }

        if ((size.y & 1) == 0)
        {
            if (size.y == 0)
            {
                ret.y = m_StartPos.y + (m_Size.y + 1 + mulPar) * MapUtil.m_MapGridUnityLen;//MIN要大于MAX，都大于边界
            }
            else
            {
                ret.y = pos.y - mulPar * ((size.y - 1) / 2 + 0.5f) * MapUtil.m_MapGridUnityLen;
            }
        }
        else
        {
            ret.y = pos.y - mulPar * (size.y / 2) * MapUtil.m_MapGridUnityLen;
        }

        if ((size.z & 1) == 0)
        {
            if (size.z == 0)
            {
                ret.z = m_StartPos.z + (m_Size.z + 1 + mulPar) * MapUtil.m_MapGridUnityLen;//MIN要大于MAX，都大于边界
            }
            else
            {
                ret.z = pos.z - mulPar * (size.z / 2 - 1 + 0.5f) * MapUtil.m_MapGridUnityLen;
            }
        }
        else
        {
            ret.z = pos.z - mulPar * (size.z / 2) * MapUtil.m_MapGridUnityLen;
        }

        ret = Adjust2Wall(ret, 0f);

        //Debug.LogWarning("corner=" + ret + " min=" + min + " pos=" + pos);
        return ret;
    }

    public Vector3 GetObjEulerAngles()
    {
        switch (m_Type)
        {
            case Enum_Layer.LeftWall:
                {
                    return new Vector3(0, -90, 0);
                }
            case Enum_Layer.RightWall:
                {
                    return new Vector3(0, 90, 0);
                }
            case Enum_Layer.Wall:
            case Enum_Layer.FloorWall:
                {
                    return Vector3.zero;
                }
        }
        return Vector3.zero;
    }
}