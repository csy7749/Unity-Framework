#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using UnityEngine.UIElements;
using System;

namespace UnityFramework.Editor
{
    /// <summary>
    /// Snap鐩稿叧鐨勯€昏緫鍜屽叏灞€鍙橀噺
    /// </summary>
    public class SnapLogic
    {
        //灞忓箷绌洪棿鍚搁檮璺濈锛屽悗缁彲浠ユ坊鍔犲埌璁剧疆涓箣绫?
        public static float SnapSceneDistance = 8f;
        /// <summary>
        /// 闂磋窛鍚搁檮鐨勫彲鎺ュ彈璇樊
        /// </summary>
        private static float SnapEps = 0.5f;
        /// <summary>
        /// 褰撳墠甯х墿浣撴渶缁堝惛闄勭殑浣嶇疆
        /// </summary>
        public static Vector3 ObjFinalPos;
        /// <summary>
        /// 琛ㄧず鏈EditorApplication.update鍚搁檮鍒扮殑杈呭姪绾胯窛绂?
        /// Vert浠ｈ〃绔栫洿锛孒oriz浠ｈ〃姘村钩
        /// </summary>
        public static float SnapLineDisVert, SnapLineDisHoriz;
        /// <summary>
        /// 琛ㄧず鏈EditorApplication.update鍚搁檮鍒扮殑杈圭紭璺濈
        /// Vert浠ｈ〃绔栫洿锛孒oriz浠ｈ〃姘村钩
        /// </summary>
        public static float SnapEdgeDisVert, SnapEdgeDisHoriz;
        /// <summary>
        /// 琛ㄧず鏈EditorApplication.update鍚搁檮鍒扮殑Interval璺濈
        /// Vert浠ｈ〃绔栫洿锛孒oriz浠ｈ〃姘村钩
        /// </summary>
        public static float SnapIntervalDisVert, SnapIntervalDisHoriz;
        //TODO 鍙互鏀规垚鍙湪婊氳疆婊氬姩涔嬪悗鏇存柊鍑犳
        public static float SnapWorldDistance
        {
            get
            {
                SceneView sceneView = SceneView.lastActiveSceneView;
                Vector3 v1 = sceneView.camera.ScreenToWorldPoint(new Vector3(SnapSceneDistance, 0, 0));
                Vector3 v2 = sceneView.camera.ScreenToWorldPoint(new Vector3(0, 0, 0));
                return Mathf.Abs(v1.x - v2.x);
            }
        }
        public static float SnapEpsDistance
        {
            get
            {
                SceneView sceneView = SceneView.lastActiveSceneView;
                Vector3 v1 = sceneView.camera.ScreenToWorldPoint(new Vector3(SnapEps, 0, 0));
                Vector3 v2 = sceneView.camera.ScreenToWorldPoint(new Vector3(0, 0, 0));
                return Mathf.Abs(v1.x - v2.x);
            }
        }
    }

}
#endif
