## 1. Runtime Super 组件落地

- [x] 1.1 新增 `SuperButton/SuperImage/SuperText` 组件文件并实现基础公开 API（点击、文本、置灰、语言入口）。
- [x] 1.2 新增 `SuperToggle/SuperToggleItem` 组件并实现选中状态管理与点击回调。
- [x] 1.3 新增 `SuperInputField/SuperDropdown/SuperPageScrollView` 组件并实现文档要求的基础方法。

## 2. LoopScroll 组件落地

- [x] 2.1 新增 `UILoopScroll` 组件并实现最小可用的列表初始化、刷新、滚动定位接口。
- [x] 2.2 新增 `UILoopScrollMul` 组件并实现多模板索引回调接口。
- [x] 2.3 新增 `UILoopScrollAndToggle` 组件并接入 `SuperToggle` 联动逻辑。

## 3. UIScriptGenerator 接入

- [x] 3.1 修改 `UICopyEditor`，将 `Button/Toggle/Slider` 事件生成从“字符串匹配”升级为“类型可赋值匹配”。
- [x] 3.2 为 `SuperToggle/SuperPageScrollView/UILoopScroll*` 增加专用绑定语句与方法桩生成逻辑。
- [x] 3.3 为 `SuperText` 增加生成期语言初始化调用代码，并确保现有模板不回归。

## 4. 验证与收敛

- [x] 4.1 扫描并修复新增代码的编译错误（含命名空间与程序集引用问题）。
- [ ] 4.2 执行一次 UI 自动生成流程，验证 `_Auto/_View/_UGUINodeProvider` 生成与编译通过。
- [x] 4.3 在 OpenSpec 中回填实现结果与任务状态。

> 说明：当前会话未直接启动 Unity Editor，因此 `4.2` 保持待验证；其余任务已完成代码实现与静态检查。
