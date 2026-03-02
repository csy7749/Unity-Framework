using GameLogic.Guide;
using UnityEngine;
using UnityEditor;

namespace UnityFramework.Editor
{
    [CustomEditor(typeof(GuideHighLightData))]
    public class GuideHighLightDataEditor : UnityEditor.Editor
    {
        SerializedProperty openProperty;
        SerializedProperty highLightTypeProperty;
        SerializedProperty m_UseCustomTarget;
        SerializedProperty m_target;

        // UIBeginnerGuide guide;
        GuideHighLightData data;

        private void OnEnable()
        {
            openProperty = serializedObject.FindProperty("Open");
            highLightTypeProperty = serializedObject.FindProperty("highLightType");
            m_UseCustomTarget = serializedObject.FindProperty("UseCustomTarget");
            m_target = serializedObject.FindProperty("target");

            data = target as GuideHighLightData;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(openProperty, new GUIContent("打开控件"));
            string[] value = {"圆形", "方形"};
            highLightTypeProperty.intValue = Utils.EnumPopupLayoutEx("镂空形状", typeof(HighLightType),
                highLightTypeProperty.intValue, value);
            //EditorGUILayout.PropertyField(highLightTypeProperty, new GUIContent("镂空形状"));
            EditorGUILayout.PropertyField(m_UseCustomTarget,new GUIContent("使用自定义镂空对象"));
            if(m_UseCustomTarget.boolValue==true){
                EditorGUILayout.PropertyField(m_target,new GUIContent("镂空对象"));
            }
            else {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("镂空对象", GUILayout.Width(EditorGUIUtility.labelWidth));
                if(GUILayout.Button("编辑")){
                    GameObject obj = data.transform.GetChild(0).gameObject;
                    Selection.activeGameObject = obj;
                }
                EditorGUILayout.EndHorizontal();
            }
            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                data.gameObject.SetActive(openProperty.boolValue);
            }
        }
    }
}
