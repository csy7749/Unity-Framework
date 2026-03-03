## ADDED Requirements

### Requirement: Timer 对比文档覆盖核心维度
系统 SHALL 产出一份 Timer 对比文档，至少覆盖以下维度：调度模型、时间源与缩放、API 语义、生命周期、内存行为、错误处理、测试可验证性、适用边界。

#### Scenario: 输出完整维度对比
- **WHEN** 文档完成并提交到 OpenSpec 变更目录
- **THEN** 文档必须包含上述 8 个维度的明确结论

### Requirement: WooTimer 结论必须标注证据级别
系统 SHALL 对每一条 WooTimer 相关结论标注证据类型（直接源码证据 / 推断），并优先以本地源码证据作为结论依据。

#### Scenario: 本地仓库可读时的证据落地
- **WHEN** 本地存在可读取的 WooTimer 仓库
- **THEN** 文档中的 WooTimer 结论必须能映射到对应源码文件与行为

### Requirement: 当前框架 Timer 结论必须可追溯到本地源码
系统 SHALL 为本项目 Timer 相关结论提供本地代码依据，至少包含 `TimerModule`、`ModuleSystem`、`RootModule`、`GameModule` 与至少一个调用点示例。

#### Scenario: 溯源检查
- **WHEN** 读者检查文档中的框架侧结论
- **THEN** 每个关键结论都能映射到对应源码文件与行为描述

### Requirement: 文档必须给出决策导向输出
系统 SHALL 在对比结果后输出“可借鉴项 / 需改造项 / 暂不建议引入项”三类结论，供后续实现任务直接消费。

#### Scenario: 形成可执行结论
- **WHEN** 技术评审使用该文档进行讨论
- **THEN** 评审参与者可以直接基于三类结论决定下一步是否进入实现阶段
