
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace UnityFramework.Editor
{
    //UXTools中的路径和常量
    public partial class ThunderFireUIToolConfig
    {
        public static readonly string UXCommonPath = $"{AssetsRootPath}UX-GUI-Editor-Common/";
        public static readonly string UXToolsPath = $"{AssetsRootPath}UX-GUI-Editor-Tools/";

        #region Editor Res
        public static readonly string UIBuilderPath = UXToolsPath + "Assets/Editor/Window_uibuilder/";
        public static readonly string ScenePath = UXToolsPath + "Assets/Editor/Scene/";
        #endregion

        #region Widget Setting
        public static readonly string WidgetLibrarySettingsPath = UXToolsPath + "Assets/Editor/Settings/Widget/";
        //组件库-组件类型数据
        public static readonly string WidgetLabelsPath = WidgetLibrarySettingsPath + "WidgetLabels.json";
        //组件库-被认定为组件的Prefab信息
        public static readonly string WidgetListPath = WidgetLibrarySettingsPath + "WidgetList.json";
        #endregion

        #region User Data
        public static readonly string UserDataPath = UXToolsPath + "UserDatas/Editor/";
        //辅助线数据
        public static readonly string LocationLinesDataPath = UserDataPath + "LocationLinesData.json";
        //最近打开的Prefab数据
        public static readonly string PrefabRecentOpenedPath = UserDataPath + "PrefabRecentlyOpenedData.json";
        //Scene窗口Tab页签数据
        public static readonly string PrefabTabsPath = UserDataPath + "PrefabTabsData.json";
        //快速背景图数据
        public static readonly string QuickBackgroundDataPath = UserDataPath + "QuickBackgroundData.json";
        //功能开关数据
        public static readonly string SwitchSettingPath = UserDataPath + "SwitchSetting.json";
        //最近选中文件数据
        public static readonly string FilesRecentSelectedPath = UserDataPath + "FilesRecentlySelectedData.json";
        //工具全局数据
        public static readonly string GlobalDataPath = $"{UXCommonPath}Assets/Editor/ToolGlobalData/ToolGlobalData.json";
        #endregion

        #region MenuItem Name
        public const string MenuName = "UnityFramework/";
        
        public const string CreateBeginnerGuide = "创建新手引导(Create BeginnerGuide)";

        public const string Menu_CreateBeginnerGuide = MenuName + CreateBeginnerGuide;  //55

        

        #endregion

        #region EditorPref Name
        //用于存储需要在Play状态前后保持，但是又没有重要到需要持久化的Editor数据
        //其实就是持久化数据的简便做法
        public const string PreviewPrefabPath = "PreviewPrefabPath";
        public const string PreviewOriginScene = "PreviewOriginScene";
        #endregion

    }
}
