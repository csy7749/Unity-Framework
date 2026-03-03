## 1. 信息采集与溯源

- [ ] 1.1 读取本地 WooTimer 仓库（`F:/Ugit/Frameworks/Unity-Framework/WooTimer-main`）并提取关键行为证据
- [ ] 1.2 读取本项目 Timer 相关源码并提炼关键行为事实
- [ ] 1.3 确认本项目 Timer 的调用链路与典型调用点

## 2. 对比分析文档产出

- [ ] 2.1 按 8 个维度完成 WooTimer 与 Framework Timer 的差异矩阵（源码证据版）
- [ ] 2.2 输出三类决策结论（可借鉴/需改造/暂不建议）
- [ ] 2.3 生成 `timer-comparison.md` 并补充关键风险条目

## 3. OpenSpec 完整性校验

- [ ] 3.1 检查 proposal/design/specs/tasks 文件结构与路径是否符合 schema
- [ ] 3.2 运行 `openspec status --change analyze-wootimer-vs-framework-timer` 校验状态
- [ ] 3.3 准备后续 `/opsx:apply` 所需的实现任务入口
