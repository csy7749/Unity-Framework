using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using ThunderFireUITool;


namespace UnityEditor
{


    public sealed class CustomEditorGUILayout
    {
        private static readonly Dictionary<string, string> InspectorLocalizationMap = new Dictionary<string, string>
        {
            ["UXText|localizationPreviewText"] = "本地预览文本",
            ["UXText|m_FontData"] = "字号",
            ["UXText|m_Color"] = "颜色",
            ["UXText|m_RaycastTarget"] = "射线投射目标",
            ["UXText|localizationType"] = "文本类型",
            ["UXText|ellipsisOnRight"] = "超框时省略号在右侧（否则在左侧）",
            ["UXText|Enable localization"] = "开启本地化",
            ["UXText|EnableArabicFix"] = "使用阿拉伯语修正",
            ["UXText|UseTashkeel"] = "添加音标(Tashkeel)",
            ["UXText|UseHinduNumber"] = "转换数字",
            ["UXText|m_Text"] = "文本",
            ["UXText|RuntimeUseText"] = "静态文本",
            ["UXText|DynamicText"] = "动态文本",
            ["UXImage|FlipMode"] = "镜像模式",
            ["UXImage|FlipEdge"] = "镜像方向",
            ["UXImage|FlipFill"] = "填充中心点",
            ["UXImage|Copy"] = "复制",
            ["UXImage|UXEffect"] = "效果样式",
            ["UXImage|UXShadow"] = "阴影",
            ["UXImage|UXOutline"] = "描边",
            ["UXImage|Enable localization"] = "开启本地化",
            ["UXImage|m_ColorType"] = "颜色类型",
            ["UXImage|UseUIColorConfig"] = "使用UIColorConfig",
            ["UXImage|m_Direction"] = "渐变方向",
            ["UXImage|LeftTop"] = "左上",
            ["UXImage|RightTop"] = "右上",
            ["UXImage|RightBottom"] = "右下",
            ["UXImage|LeftBottom"] = "左下",
            ["UXImage|Up"] = "向上",
            ["UXImage|Down"] = "向下",
            ["UXImage|Left"] = "向左",
            ["UXImage|Right"] = "向右",
            ["UXImage|Middle"] = "中间",
            ["UXImage|FourCorner"] = "四周",
            ["UXImage|Solid_Color"] = "纯色",
            ["UXImage|Gradient_Color"] = "渐变色",
            ["UXImage|Vertical"] = "垂直",
            ["UXImage|Horizontal"] = "水平",
            ["UXImage|None"] = "无",
            ["UXImage|ColorSpace"] = "颜色空间",
            ["UXImage|GammaToLinear"] = "线性空间",
            ["UXImage|LinearToGamma"] = "Gamma空间",
            ["UXImage|GreyEffect"] = "置灰",
            ["UXImage|Contrast"] = "对比度",
            ["UXImage|Saturation"] = "饱和度",
            ["UXImage|Radius"] = "圆角",
            ["UXScrollRect|Vertical"] = "垂直布局",
            ["UXScrollRect|Horizontal"] = "水平布局",
            ["UXScrollRect|Grid"] = "网格布局",
            ["UIBeginnerData|GuideID"] = "引导ID",
            ["UIBeginnerData|FinishType"] = "引导类型",
            ["UIBeginnerData|Duration"] = "引导时长",
            ["UIBeginnerData|ShowData"] = "显示数据",
            ["UIBeginnerData|Edit"] = "编辑",
            ["UIBeginnerData|GuidePrefab"] = "引导模板",
            ["UIBeginnerData|StrongGuide"] = "强引导需要点击镂空才会消失",
            ["UIBeginnerData|MiddleGuide"] = "中引导点击任意位置会消失",
            ["UIBeginnerData|WeakGuide"] = "弱引导镂空会过一段时间消失",
            ["UIBeginnerData|UseOwnPrefab"] = "使用自定义模板",
            ["UIBeginnerData|ChooseGuideTemplate"] = "选择引导模板",
            ["UIBeginnerData|muban0"] = "镂空点击模板",
            ["UIBeginnerData|muban1"] = "镂空高亮模板",
            ["UIBeginnerData|muban2"] = "手柄模板",
            ["UIBeginnerData|muban3"] = "文本模板",
            ["UIBeginnerData|muban4"] = "全组件模板",
            ["UIBeginnerData|GestureTemplate"] = "手势模板",
            ["UIBeginnerData|GamePadTemplate"] = "手柄模板",
            ["UIBeginnerData|Strong"] = "强引导",
            ["UIBeginnerData|Middle"] = "中引导",
            ["UIBeginnerData|Weak"] = "弱引导",
            ["UIBeginnerGuide|GuideTemplate"] = "新手引导面板",
            ["UIBeginnerGuide|TextWidget"] = "文字组件",
            ["UIBeginnerGuide|GestureWidget"] = "手势组件",
            ["UIBeginnerGuide|GamePadWidget"] = "手柄组件",
            ["UIBeginnerGuide|HighLightWidget"] = "镂空组件",
            ["UIBeginnerGuide|TargetStrokeWidget"] = "强调组件",
            ["UIBeginnerGuide|ArrowLineWidget"] = "连线组件",
            ["UIBeginnerGuide|SetAsHighLight"] = "设置为引导",
            ["UIBeginnerGuide|OpenComponent"] = "打开控件",
            ["UIBeginnerGuide|highlightchild"] = "镂空区域",
            ["GestureData|UseCustomGesture"] = "使用自定义手势",
            ["GestureData|gesture"] = "手势类型",
            ["GestureData|customGesture"] = "自定义手势",
            ["GestureData|objecttype"] = "对象类型",
            ["GestureData|selectedObject"] = "指定对象",
            ["GestureData|DragCurve"] = "拖拽曲线",
            ["GestureData|StartPosController"] = "拖拽起点标识",
            ["GestureData|EndPosController"] = "拖拽终点标识",
            ["GestureData|ThumbClick"] = "（拇指）点击",
            ["GestureData|ThumbDrag"] = "（拇指）拖动",
            ["GestureData|ThumbLongPress"] = "（拇指）长按",
            ["GestureData|ThumbRotate"] = "（拇指）旋转",
            ["GestureData|ThumbSlideUp"] = "（拇指）上滑",
            ["GestureData|ThumbSlideDown"] = "（拇指）下滑",
            ["GestureData|ThumbSlideLeft"] = "（拇指）左滑",
            ["GestureData|ThumbSlideRight"] = "（拇指）右滑",
            ["GestureData|ForeFingerClick"] = "（食指）点击",
            ["GestureData|ForeFingerDrag"] = "（食指）拖动",
            ["GestureData|ForeFingerLongPress"] = "（食指）长按",
            ["GestureData|ForeFingerRotate"] = "（食指）旋转",
            ["GestureData|ForeFingerSlideUp"] = "（食指）上滑",
            ["GestureData|ForeFingerSlideDown"] = "（食指）下滑",
            ["GestureData|ForeFingerSlideLeft"] = "（食指）左滑",
            ["GestureData|ForeFingerSlideRight"] = "（食指）右滑",
            ["GestureData|auto"] = "自定",
            ["GestureData|select"] = "指定",
            ["GuideTextData|TextBgStyle"] = "文字组件格式",
            ["GuideTextData|default"] = "单一文字",
            ["GuideTextData|withTitle"] = "带标题文字",
            ["GuideHighLightData|Circle"] = "圆形",
            ["GuideHighLightData|Square"] = "方形",
            ["GuideHighLightData|UseCustomTarget"] = "使用自定义镂空对象",
            ["GuideHighLightData|HighLightType"] = "镂空形状",
            ["GuideHighLightData|target"] = "镂空对象",
            ["TargetStroke|PlayAnimator"] = "播放动画",
            ["TargetStroke|StrokeType"] = "强调类型",
            ["TargetStroke|TargetType"] = "对象类型",
            ["GuideGamePadData|GuideList"] = "引导列表",
            ["GuideGamePadData|GamePadTip"] = "Tip: 按键闪烁一次为0.5秒",
            ["GuideGamePadData|AnimationStr"] = "引导序列化信息",
            ["UIBeginnerTimeAndKeys|Actions"] = "按键",
            ["UIBeginnerTimeAndKeys|Time"] = "时长",
            ["UIStateAnimator|Preview"] = "预览",
            ["UIStateAnimator|StartPreview"] = "启用动画预览/编辑",
            ["UIStateAnimator|PrintGraphNodeState"] = "日志输入当前Graph各个节点状态",
        };

        private static bool IsChildrenIncluded(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.Vector4:
                    return true;
                default:
                    return false;
            }
        }

        public static bool PropertyField(SerializedProperty property, GUIContent label, params GUILayoutOption[] options)
        {
            return PropertyField(property, label, IsChildrenIncluded(property), options);
        }

        public static bool PropertyField(SerializedProperty property, params GUILayoutOption[] options)
        {
            return PropertyField(property, null, IsChildrenIncluded(property), options);
        }

        public static bool PropertyField(SerializedProperty property, bool includeChildren, params GUILayoutOption[] options)
        {
            return PropertyField(property, null, includeChildren, options);
        }

        public static bool PropertyField(SerializedProperty property, GUIContent label, bool includeChildren, params GUILayoutOption[] options)
        {
            GUIContent l18NLabel = GetGUIContent(property);
            return EditorGUILayout.PropertyField(property, l18NLabel, includeChildren, options);
        }

        private static GUIContent GetGUIContent(SerializedProperty property)
        {
            string propertyType = property.serializedObject.targetObject.GetType().Name;
            string propertyName = property.name;
            string name = GetL18NTextByTypeAndFieldName(propertyType, propertyName);
            return new GUIContent(name);
        }

        public static string GetL18NTextByTypeAndFieldName(string type, string fieldName)
        {
            if (InspectorLocalizationMap.TryGetValue($"{type}|{fieldName}", out string value))
            {
                return value;
            }

            return fieldName;
        }
    }
}
