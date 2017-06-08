using UnityEngine;

public enum Enum_Event
{
    None = 0,
    SetOne,
    Place2Pos,
}

public class Util
{
    /// <summary>
    /// <para>获得点击位置</para>
    /// <para>移动设备用第一个触摸点</para>
    /// <para>返回值z轴为0</para>
    /// </summary>
    /// <returns></returns>
    public static Vector3 GetClickPos()
    {
        Vector3 pos = Input.mousePosition;
#if UNITY_EDITOR
        pos = Input.mousePosition;
#else
#if UNITY_ANDROID || UNITY_IPHONE
            if(Input.touchCount > 0)
            {
                pos = Input.touches[0].position;
            }
            else
            {
                pos = Input.mousePosition;
            }
#else
            pos = Input.mousePosition;
#endif
#endif
        pos.z = 0;
        return pos;
    }
}