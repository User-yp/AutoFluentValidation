# AutoFluentValidation 发布指南

本文档详细说明如何将 AutoFluentValidation 发布到 [NuGet.org](https://www.nuget.org/)。

---

## 目录

- [前置准备](#前置准备)
- [方式一：手动发布（命令行）](#方式一手动发布命令行)
- [方式二：GitHub Actions 自动发布](#方式二github-actions-自动发布)
- [发布后验证](#发布后验证)
- [常见问题](#常见问题)

---

## 前置准备

### 1. NuGet 账户

确认你已有 NuGet.org 账户：

[https://www.nuget.org/profiles/user_pengye](https://www.nuget.org/profiles/user_pengye)

```bash
# 如果本地未保存 API Key，需要先获取。已保存可跳过此步。
```

### 2. 更新版本号

发布前，修改 `AutoFluentValidation/AutoFluentValidation.csproj` 中的版本号：

```xml
<Version>1.2.0</Version>
```

以及 `PackageReleaseNotes`，记录本次发布的变更。

### 3. 确认构建通过

```bash
dotnet build -c Release
dotnet pack AutoFluentValidation/AutoFluentValidation.csproj -c Release -o ./nupkgs
```

---

## 方式一：手动发布（命令行）

适用于本地直接发布，需要已保存 NuGet API Key。

### 1. 获取 API Key

1. 登录 [nuget.org/account/apikeys](https://www.nuget.org/account/apikeys)
2. 点击 **Create**
3. 配置：

| 字段 | 值 |
|------|-----|
| Key Name | `Local Publish` |
| Scopes | 勾选 **Push** |
| Glob Pattern | `AutoFluentValidation` |

4. 点击 **Create**，复制生成的 API Key（仅显示一次）

### 2. 保存 API Key 到本地

```bash
# 保存 API Key（仅需执行一次）
dotnet nuget set-api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

> **安全提示**：API Key 会被加密存储在 `%APPDATA%\NuGet\NuGet.Config` 中。

### 3. 构建并打包

```bash
# 进入项目目录
cd AutoFluentValidation

# 构建（Release 模式，所有目标框架）
dotnet build -c Release

# 打包
dotnet pack -c Release --no-build -o ./nupkgs
```

完成后，`./nupkgs` 目录下会生成：

```
nupkgs/
├── AutoFluentValidation.1.2.0.nupkg    # NuGet 包
└── AutoFluentValidation.1.2.0.snupkg   # 符号包（调试用）
```

### 4. 推送

```bash
# 推送包和符号包
dotnet nuget push ./nupkgs/AutoFluentValidation.1.2.0.nupkg \
  --source https://api.nuget.org/v3/index.json
```

```bash
# 已完成上一步会自动推送 snupkg。也可以单独推送符号包：
dotnet nuget push ./nupkgs/AutoFluentValidation.1.2.0.snupkg \
  --source https://api.nuget.org/v3/index.json
```

### 5. 打 Tag（可选）

```bash
git tag v1.2.0
git push origin v1.2.0
```

---

## 方式二：GitHub Actions 自动发布

推送 tag 即可触发自动构建和发布，无需本地操作。

### 1. 申请 NuGet API Key

1. 登录 [nuget.org/account/apikeys](https://www.nuget.org/account/apikeys)
2. 点击 **Create**
3. 配置：

| 字段 | 值 |
|------|-----|
| Key Name | `GitHub Actions AutoFluentValidation` |
| Scopes | 勾选 **Push** |
| Glob Pattern | `AutoFluentValidation` |

4. 点击 **Create**，**复制生成的 API Key**

### 2. 添加 GitHub Secret

1. 打开 Settings 页面：
   [github.com/User-yp/AutoFluentValidation/settings/secrets/actions](https://github.com/User-yp/AutoFluentValidation/settings/secrets/actions)

2. 点击 **New repository secret**

| 字段 | 值 |
|------|-----|
| Name | `NUGET_API_KEY` |
| Secret | 粘贴上一步复制的 API Key |

3. 点击 **Add secret**

> 添加后页面显示 `NUGET_API_KEY` 即配置成功。

### 3. 工作流说明

工作流文件位于 `.github/workflows/publish.yml`，触发条件：

| 触发方式 | 说明 |
|----------|------|
| **推送 Tag** | 推送 `v1.0.0` 格式的 tag 时自动运行 |
| **手动触发** | 在 Actions 页面点击 **Run workflow** |

流程概览：

```
推送 tag v1.2.0
    │
    ▼
检出代码 + 安装 .NET SDK (6/7/8/9)
    │
    ▼
解析版本号 1.2.0（去掉 v 前缀）
    │
    ▼
dotnet build -c Release
    │
    ▼
dotnet pack -c Release --no-build
    │
    ▼
dotnet nuget push → NuGet.org
    │
    ▼
发布完成 ✅
```

### 4. 发布操作

```bash
# 1. 确保代码已提交
git add -A
git commit -m "准备发布 v1.2.0"

# 2. 打 tag 并推送（自动触发发布）
git tag v1.2.0
git push origin master --tags
```

推送后：
1. 打开 [Actions](https://github.com/User-yp/AutoFluentValidation/actions) 页面
2. 可以看到 `发布 NuGet 包` 工作流正在运行
3. 等待约 2~3 分钟，显示 ✅ 即发布成功

#### 手动触发（备选）

如果不想打 tag，可以直接在 Actions 页面手动运行：

1. [Actions](https://github.com/User-yp/AutoFluentValidation/actions) → **发布 NuGet 包** → **Run workflow**
2. 可选择性输入版本号，留空则使用 csproj 中的版本

---

## 发布后验证

### 检查 NuGet.org 页面

发布成功后，稍等几分钟（NuGet 索引延迟），访问：

[https://www.nuget.org/packages/AutoFluentValidation](https://www.nuget.org/packages/AutoFluentValidation)

确认以下内容显示正确：

- [ ] 包图标（绿色勾号）
- [ ] README 内容完整渲染
- [ ] 版本号和 Release Notes
- [ ] 依赖项列表
- [ ] 支持的框架：net6.0 / net7.0 / net8.0 / net9.0
- [ ] License: MIT
- [ ] Repository 链接指向 GitHub

### 验证包安装

```bash
# 新建一个测试项目
dotnet new console -n TestPackage
cd TestPackage

# 安装刚发布的包
dotnet add package AutoFluentValidation --version 1.2.0

# 确认安装成功
dotnet list package
```

### 验证 XML 文档

在 IDE（Visual Studio / Rider / VS Code）中：
1. 安装包后输入 `new ValidatorControl(`
2. 确认能看到参数说明的智能提示

---

## 常见问题

### Q: 推送 tag 后 Actions 没有触发？

检查 tag 格式是否为 `v` + 三段式版本号：

```bash
# ✅ 正确
git tag v1.0.0
git tag v2.1.3

# ❌ 错误
git tag 1.0.0        # 缺少 v 前缀
git tag v1.0          # 不是三段式
git tag release-1.0   # 格式不匹配
```

### Q: Actions 报错 `401 Unauthorized`？

NUGET_API_KEY 可能已过期或配置错误：

1. 检查 Secret 名称是否完全一致：`NUGET_API_KEY`（注意大小写）
2. 去 NuGet.org 重新生成 API Key
3. 更新 GitHub Secret 中的值

### Q: 提示 "package already exists"？

版本号已存在。NuGet.org 不允许覆盖已发布的版本，需要使用新版本号：

```bash
# 更新 csproj 中的 Version
# 然后重新打 tag
git tag v1.2.1
git push origin v1.2.1
```

### Q: 包的大小正常吗？

Release 构建下包大小约 85 KB，包含：

- 4 个 TFM 的 DLL（每个约 15 KB）
- 4 个 TFM 的 XML 文档
- README.md
- icon.png

如果体积异常大，检查是否为 Debug 构建。

### Q: 如何彻底废弃一个版本？

NuGet.org 不允许删除已发布的版本。你可以：

1. 在包管理页面点击 **Contact Support** 申请废弃（deprecation）
2. 或发布新版本替代

---

## 快速参考

```bash
# ─── 方式一：手动发布 ───
dotnet build -c Release
dotnet pack -c Release --no-build -o ./nupkgs
dotnet nuget push ./nupkgs/AutoFluentValidation.*.nupkg --source https://api.nuget.org/v3/index.json

# ─── 方式二：GitHub Actions ───
git tag v1.2.0
git push origin master --tags
```

---

> 最后更新：2025-07-02
