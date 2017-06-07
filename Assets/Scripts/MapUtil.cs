using UnityEngine;

public class MapUtil
{
    /// <summary>
    /// 选中的ID
    /// </summary>
    public static int m_SelectId = 0;
    /// <summary>
    /// 选中的已经放好了
    /// </summary>
    public static bool m_SelectOK = true;

    public static Vector3 m_MapStartPos;
    /// <summary>
    /// 宽*高=x*y
    /// </summary>
    public static Vector2 m_MapSize;
    public static Vector2 m_MapGirdUnitySize;

    public static bool[,] m_MapFlag;

    public static void Init()
    {
        m_SelectId = 0;
        m_SelectOK = true;

        m_MapFlag = new bool[(int)m_MapSize.x, (int)m_MapSize.y];
        for (int i = 0; i < (int)m_MapSize.x; i++)
        {
            for (int j = 0; j < (int)m_MapSize.y; j++)
            {
                m_MapFlag[i, j] = false;
            }
        }
    }

    public static bool SetOne(Vector3 pos, Vector3 size)
    {
        Vector3 min = Pos2Grid(GetCorner(pos, size, true));
        Vector3 max = Pos2Grid(GetCorner(pos, size, false));
        for (int i = (int)min.x; i <= (int)max.x; i++)
        {
            for (int j = (int)min.y; j <= (int)max.y; j++)
            {
                if (m_MapFlag[i, j] == true)
                {
                    return false;
                }
            }
        }
        for (int i = (int)min.x; i <= (int)max.x; i++)
        {
            for (int j = (int)min.y; j <= (int)max.y; j++)
            {
                m_MapFlag[i, j] = true;
            }
        }
        return true;
    }

    public static bool CleanOne(Vector3 pos, Vector3 size)
    {
        Vector3 min = Pos2Grid(GetCorner(pos, size, true));
        Vector3 max = Pos2Grid(GetCorner(pos, size, false));
        for (int i = (int)min.x; i <= (int)max.x; i++)
        {
            for (int j = (int)min.y; j <= (int)max.y; j++)
            {
                m_MapFlag[i, j] = false;
            }
        }
        return true;
    }

    public static Vector3 GetCorner(Vector3 pos, Vector3 size, bool min)
    {
        Vector3 ret = Vector3.zero;
        if (min)
        {
            if (((int)size.x) % 2 == 0)
            {
                ret.x = pos.x - ((int)((size.x - 1) / 2) + 0.5f) * m_MapGirdUnitySize.x;
            }
            else
            {
                ret.x = pos.x - ((int)(size.x / 2)) * m_MapGirdUnitySize.x * 0.5f;
            }

            if (((int)size.y) % 2 == 0)
            {
                ret.y = pos.y - ((int)((size.y - 1) / 2) + 0.5f) * m_MapGirdUnitySize.y;
            }
            else
            {
                ret.y = pos.y - ((int)(size.y / 2)) * m_MapGirdUnitySize.y * 0.5f;
            }
        }
        else
        {
            if (((int)size.x) % 2 == 0)
            {
                ret.x = pos.x + ((int)((size.x - 1) / 2) + 0.5f) * m_MapGirdUnitySize.x;
            }
            else
            {
                ret.x = pos.x + ((int)(size.x / 2)) * m_MapGirdUnitySize.x * 0.5f;
            }

            if (((int)size.y) % 2 == 0)
            {
                ret.y = pos.y + ((int)((size.y - 1) / 2) + 0.5f) * m_MapGirdUnitySize.y;
            }
            else
            {
                ret.y = pos.y + ((int)(size.y / 2)) * m_MapGirdUnitySize.y * 0.5f;
            }
        }

        return ret;
    }

    public static Vector3 Grid2Pos(Vector3 grid)
    {
        Vector3 ret = Vector3.zero;
        ret.x = m_MapStartPos.x + m_MapGirdUnitySize.x / 2 + grid.x * m_MapGirdUnitySize.x;
        ret.y = m_MapStartPos.y + m_MapGirdUnitySize.y / 2 + grid.x * m_MapGirdUnitySize.y;
        return ret;
    }

    public static Vector3 Pos2Grid(Vector3 pos)
    {
        Vector3 ret = Vector3.zero;
        ret.x = (int)((pos.x - (m_MapStartPos.x + m_MapGirdUnitySize.x / 2)) / m_MapGirdUnitySize.x);
        ret.y = (int)((pos.y - (m_MapStartPos.y + m_MapGirdUnitySize.y / 2)) / m_MapGirdUnitySize.y);
        return ret;
    }
}