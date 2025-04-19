## 程序集版本号生成工具

这是一个用于管理.NET程序集版本号的工具，通过命令行参数控制版本号的修改操作。支持正式版本和预发布版本(alpha/beta/rc)的设置，以及主版本号、次版本号和修订号的递增。

### 使用场景

- 自动化构建流程中自动递增版本号
- 在不同开发阶段设置对应的预发布标签
- 管理软件版本的生命周期(开发版、测试版、候选版、正式版)

### 注意事项

1. 需要确保AssemblyInfo.cs文件路径正确
2. 需要有文件写入权限
3. 预发布标签应符合语义化版本规范
4. 版本号格式为：Major.Minor.Patch[-PreRelease]

### 基本用法

1. 把AssemblyTool.exe内容复制到其他项目的跟目录下：如在AssemblyTool文件中

![image](E:\Console\AssemblyTool\AssemblyTool\assets\image-20250418173544-hdzw8qs.png)

1. 在项目生成前事件中加入

```csharp
"$(TargetDir)\AssemblyTool\AssemblyTool.exe" "setPre_beta"  //设置预发布版本号  beta - 测试版本，功能基本完整但可能有bug
"$(TargetDir)\AssemblyTool\AssemblyTool.exe" "patch+" // 递增修订号
```

### 版本控制操作列表

| 命令参数     | 功能描述                                                     | 适用场景                         |
| ------------ | ------------------------------------------------------------ | -------------------------------- |
| setMajor     | 移除预发布标签，设置为正式版本                               | 发布正式版本时使用               |
| setPre_alpha | 设置预发布版本为alpha（早期开发版本，功能不完整）            | 早期开发阶段                     |
| setPre_beta  | 设置预发布版本为beta（测试版本，功能基本完整但可能有bug）    | 功能测试阶段                     |
| setPre_rc    | 设置预发布版本为rc（Release Candidate，发布候选版本，接近最终版） | 发布前的候选版本                 |
| major+       | 递增主版本号（Major）                                        | 重大功能变更/不兼容API修改时使用 |
| minor+       | 递增次版本号（Minor），保持当前预发布标签                    | 新增功能但向下兼容时使用         |
| patch+       | 递增修订号（Patch），保持当前预发布标签                      | Bug修复或小改动时使用            |

### 路径查找说明

1. 自动定位程序集所在目录
2. 向上查找3级目录找到Properties/AssemblyInfo.cs文件
3. 所有版本操作都作用于该AssemblyInfo.cs文件

这个列表可以作为使用该版本控制工具的快速参考指南，方便开