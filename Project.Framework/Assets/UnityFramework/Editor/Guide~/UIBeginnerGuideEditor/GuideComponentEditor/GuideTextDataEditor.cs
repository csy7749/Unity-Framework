using GameLogic.Guide;
using UnityEngine;
using UnityEditor;

namespace UnityFramework.Editor
{
    [CustomEditor(typeof(GuideTextData))]
    public class GuideTextDataEditor : UnityEditor.Editor
    {
        SerializedProperty openProperty;
        SerializedProperty textBgStyleProperty;
        
        GuideTextData data;
        
        private void OnEnable()
        {
            openProperty = serializedObject.FindProperty("Open");
            textBgStyleProperty = serializedObject.FindProperty("textBgStyle");
            
            data = target as GuideTextData;
            //data.GetComponent<GuideText>().defaultStyle.name = "单一文字";
            //data.GetComponent<GuideText>().withTitleStyle.name ="带标题文字";
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.PropertyField(openProperty, new GUIContent("打开控件"));
            string[] value1 = {"单一文字", "带标题文字"};
            textBgStyleProperty.intValue = Utils.EnumPopupLayoutEx("文字组件格式", typeof(TextBgStyle),
                textBgStyleProperty.intValue, value1);
            //EditorGUILayout.PropertyField(textBgStyleProperty, new GUIContent("文字组件格式"));
            
            serializedObject.ApplyModifiedProperties();
            
            if (EditorGUI.EndChangeCheck())
            {
                data.gameObject.SetActive(openProperty.boolValue);
                data.GetComponent<GuideText>().defaultStyle.SetActive(textBgStyleProperty.intValue == 0);
                data.GetComponent<GuideText>().withTitleStyle.SetActive(textBgStyleProperty.intValue == 1);
            }
        }
    }
}
