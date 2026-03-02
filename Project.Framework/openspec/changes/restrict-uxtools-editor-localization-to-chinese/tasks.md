## 1. 设置窗口语言收敛

- [x] 1.1 调整 `ConfigurationWindow` 语言控件，仅显示简体中文并禁用切换。
- [x] 1.2 调整 `ConfigurationWindow.ConfirmOnClick`，确认时强制写入 `EditorLocalName.Chinese`。

## 2. 去本地化调用

- [x] 2.1 移除 `ConfigurationWindow` 中对 `EditorLocalization.GetLocalization` 的调用，改为中文直写。
- [x] 2.2 调整 `EditorLocalizationSettings` 默认值和写入逻辑为中文。

## 3. 验证与收敛

- [ ] 3.1 执行与本变更相关的编译检查，修复新增编译错误。（受阻：当前环境未发现 Unity Editor 可执行文件）
- [x] 3.2 核对 OpenSpec artifacts 与代码实现一致，并更新任务完成状态。
