using Jerry;
using UnityEngine;

public class Click3DCheck
{
    #region 点击检测

    private RayClickInfo clickDownInfo = new RayClickInfo();
    private RayClickInfo clickUpInfo = new RayClickInfo();
    private RayClickInfo lastClickInfo = new RayClickInfo();

    public void Update()
    {
        if (Input.GetMouseButtonDown(0)
            && !Util.ClickUI())
        {
            clickDownInfo.Init(DoRayClick());
            if (clickDownInfo.col != null)
            {
                JerryEventMgr.DispatchEvent(Enum_Event.Click3DDown.ToString(), new object[] { clickDownInfo });
            }
        }

        if (Input.GetMouseButtonUp(0)
            && !Util.ClickUI())
        {
            clickUpInfo.Init(DoRayClick());
            JudgeClick();
        }
    }

    private void JudgeClick()
    {
        if (clickUpInfo.col != clickDownInfo.col
            || clickUpInfo.col == null)
        {
            return;
        }
        if (clickUpInfo.time < clickDownInfo.time
            || clickUpInfo.time - clickDownInfo.time > 0.3f)
        {
            return;
        }

        if (!Util.Vector3Equal(clickUpInfo.pos, clickDownInfo.pos, 0.15f))
        {
            return;
        }
        if (!Util.Vector3Equal(clickUpInfo.screenPos, clickDownInfo.screenPos, 10f))
        {
            return;
        }
        //防止连点
        if (lastClickInfo.col == clickUpInfo.col
            && clickUpInfo.time - lastClickInfo.time < 0.3f)
        {
            return;
        }
        lastClickInfo.Init(clickUpInfo);
        JerryEventMgr.DispatchEvent(Enum_Event.Click3DObj.ToString(), new object[] { clickUpInfo });
    }

    #endregion 点击检测

    #region RayClick

    private static Ray ray;
    private static RaycastHit hitInfo;
    private static RayClickInfo clickInfo = new RayClickInfo();

    /// <summary>
    /// 静态化可以其他地方复用
    /// </summary>
    /// <returns></returns>
    public static RayClickInfo DoRayClick()
    {
        clickInfo.Init();
        ray = Camera.main.ScreenPointToRay(JerryUtil.GetClickPos());
        if (Physics.Raycast(ray, out hitInfo, 100))
        {
            if (hitInfo.collider != null)
            {
                clickInfo.pos = hitInfo.point;
                clickInfo.col = hitInfo.collider;
                clickInfo.time = Time.realtimeSinceStartup;
                clickInfo.screenPos = JerryUtil.GetClickPos();
            }
        }
        return clickInfo;
    }

    #endregion RayClick

    #region 结构

    public class RayClickInfo
    {
        /// <summary>
        /// 时间
        /// </summary>
        public float time;
        /// <summary>
        /// 盒子
        /// </summary>
        public Collider col;
        /// <summary>
        /// 点击的3D坐标
        /// </summary>
        public Vector3 pos;
        /// <summary>
        /// 点击的屏幕坐标
        /// </summary>
        public Vector3 screenPos;

        public RayClickInfo()
        {
            Init();
        }

        public RayClickInfo(RayClickInfo info)
        {
            Init(info);
        }

        public void Init()
        {
            time = 0;
            col = null;
            pos = Vector3.zero;
            screenPos = Vector3.zero;
        }

        public void Init(RayClickInfo info)
        {
            time = info.time;
            col = info.col;
            pos = info.pos;
            screenPos = info.screenPos;
        }

        public override string ToString()
        {
            return string.Format("time={0},col={1},pos={2},screenPos={3}", time, col == null ? "无" : col.name, MapUtil.Vector3String(pos), MapUtil.Vector3String(screenPos));
        }
    }

    #endregion 结构
}