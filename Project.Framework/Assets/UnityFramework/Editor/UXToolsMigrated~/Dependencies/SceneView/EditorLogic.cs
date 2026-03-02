#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UnityFramework.Editor
{
    //UXTools涓礋璐ｇ鐞嗗父椹籐ogic鐨刴anager,鐢熷懡鍛ㄦ湡鍜孲ceneViewToolBar鐩稿悓
    //甯搁┗Logic: 鎷ユ湁Update鍔熻兘鎴栬€呴渶瑕佸瓨鍌ㄥ唴瀛樻暟鎹殑Logic
    public class EditorLogic
    {
        public void Init()
        {
            //Snap Logic
            LocationLineLogic.Instance.Init();
            LocationLine.Init();
            if (!SwitchSetting.CheckValid(SwitchSetting.SwitchType.AlignSnap)) return;
            EdgeSnapLineLogic.Instance.Init();
            IntervalLineLogic.Instance.Init();

            LocationLineLogic.Instance.InitAfter();
            EdgeSnapLineLogic.Instance.InitAfter();
            IntervalLineLogic.Instance.InitAfter();
        }

        public void Close()
        {
            IntervalLineLogic.Instance.CloseBefore();
            EdgeSnapLineLogic.Instance.CloseBefore();
            LocationLineLogic.Instance.CloseBefore();

            LocationLine.Close();
            IntervalLineLogic.Instance.Close();
            EdgeSnapLineLogic.Instance.Close();
            LocationLineLogic.Instance.Close();
        }

        public static bool ObjectFit(GameObject obj)
        {
            if (obj == null) return false;
            //Graphic[] components = obj.GetComponents<Graphic>();
            //if (components == null || components.Length == 0) return false;
            return obj.activeInHierarchy && obj.GetComponent<RectTransform>() != null;
        }
    }
}
#endif
