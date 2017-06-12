using UnityEngine;
using UnityEngine.UI;
using Jerry;

public class UICtr : SingletonMono<UICtr>
{
    public Canvas m_Canvas;

    private Transform m_Node;
    private Transform m_Set;
    private Image m_SetBkg;
    /// <summary>
    /// 取消
    /// </summary>
    private Transform m_Cancel;
    /// <summary>
    /// 撤回
    /// </summary>
    private Transform m_Back;

    private bool m_CanSet = false;

    public override void Awake()
    {
        base.Awake();

        m_Node = this.transform.FindChild("Node");
        m_Set = m_Node.FindChild("Set");
        m_SetBkg = m_Set.GetComponent<Image>();
        m_Cancel = m_Node.FindChild("Cancel");
        m_Back = m_Node.FindChild("Back");

        UGUIEventListener.Get(m_Set.gameObject).onClick += (go) =>
        {
            if (m_CanSet)
            {
                JerryEventMgr.DispatchEvent(Enum_Event.SetOne.ToString(), new object[] { MapUtil.m_SelectId });
            }
            else
            {
                Tip.Inst.ShowTip("重叠");
            }
        };

        UGUIEventListener.Get(m_Cancel.gameObject).onClick += (go) =>
        {
            JerryEventMgr.DispatchEvent(Enum_Event.BackOne.ToString(), new object[] { MapUtil.m_SelectId });
        };

        UGUIEventListener.Get(m_Back.gameObject).onClick += (go) =>
        {
            JerryEventMgr.DispatchEvent(Enum_Event.Back2Package.ToString(), new object[] { MapUtil.m_SelectId });
        };

        m_CanSet = false;

        HideCtr();
    }

    public void HideCtr()
    {
        m_Node.gameObject.SetActive(false);
    }

    public void ShowCtr()
    {
        DoAdjustPos();
        
        m_Cancel.gameObject.SetActive(!MapUtil.m_SelectNew);
        
        m_CanSet = MyShadow.Inst.CanSet;
        m_SetBkg.color = m_CanSet ? Color.green : Color.red;
        
        m_Node.gameObject.SetActive(true);
    }

    public void AdjustPos()
    {
        if (!m_Node.gameObject.activeSelf)
        {
            return;
        }
        DoAdjustPos();
    }

    private void DoAdjustPos()
    {
        if(MapUtil.m_SelectDrag == null)
        {
            return ;
        }
        Vector3 wordPos = MapUtil.m_SelectDrag.transform.position;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(wordPos);
        screenPos.z = 0;
        this.transform.localPosition = JerryUtil.PosScreen2Canvas(m_Canvas, screenPos, this.transform);
    }
}