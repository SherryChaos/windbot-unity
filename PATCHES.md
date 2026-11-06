# windbot-unity — Unity 适配分支（`unity`）

本分支 = [WindBot](https://github.com/SherryChaos/windbot) 上游 + **最小 Unity 适配**。
原则：**上游文件尽量不改**，能加 `#if UNITY_5_3_OR_NEWER` 就不重写；新增代码全部放 `Unity/`。
这样 `git fetch upstream && git merge upstream/master` 的冲突面接近 0。

## 集成方式（已定：A. UPM git 包）

Unity 工程 `Packages/manifest.json`：

```json
"com.sherry.windbot": "https://github.com/SherryChaos/windbot-unity.git#<commit-or-tag>"
```

Unity 会为 Editor/Win/macOS/Android/iOS 自动编同一份；更新 = 改 pin 的 commit。

## 必须改的 seam（P1 执行）

| # | 文件 | 处理 |
|---|---|---|
| 1 | `Program.cs`（`HttpListener`/`System.Web`/`ConfigurationManager`） | 整文件 `#if !UNITY_5_3_OR_NEWER`；Unity 入口改为 `Unity/WindBotRunner.cs`（后台 `Thread`，`Start/Stop/IsRunning`） |
| 2 | `Config.cs` 的 `ConfigurationManager.AppSettings` | `#if !UNITY` 包住 |
| 3 | `Properties/AssemblyInfo.cs`、`BotWrapper/BotWrapper.cs` | `#if !UNITY` 包住（`DllImport` 不进 IL2CPP） |
| 4 | `ThirdParty/Mono.Data.Sqlite/**`（10+ 文件） | 每文件 `#if !UNITY`；`NamedCardsManager.Init` 换成 `ICardDataProvider`（宿主 `OcgCardDatabase` 提供完整行：code/atk/def/level/race/attr/alias/setcode） |
| 5 | `Game/AI/DecksManager.cs:33` `asm.GetTypes()` + `Activator.CreateInstance` | `#if UNITY` 用生成的 `Unity/ExecutorRegistry.g.cs`（73 个 `new`） |
| 6 | `Logger.cs`（`Console.*`） | `#if UNITY` 走 `Debug.Log`/文件；后台线程调用 marshal 到主线程 |
| 7 | `Game/Deck.cs`、`Game/AI/Dialogs`（文件读取） | `#if UNITY` 用 `TextAsset`/`Resources` 注入；不读 `StreamingAssets` |
| 8 | `GameBehavior.OnStart/OnPacket` 线程边界 | 保持后台线程，Unity API 只经 `WindBotRunner` 队列 |

## IL2CPP / 平台

- `link.xml` 已 `preserve="all"`，配合 `ExecutorRegistry` 双保险。
- 无 `DllImport`（sqlite 已去）⇒ iOS AOT 安全。
- `System.Net.Sockets` / `Thread` 在 iOS/Android 均可用；不用 `Thread.Abort`，`Stop()` 走 `Connection.Close()`。
- `Application.runInBackground = true`（运行 bot 期间）。

## 测试

- Editor：`ygoserver.dll` + `WindBotRunner.Start(new WindBotInfo { Host="127.0.0.1", Port=..., Name="BotA", Deck="AI_BlueEyes" })`。
- 注意：**WindBot 的 `Decks/*.ydk` 必须与宿主喂给 server 的卡表一致**；不一致时 server 回 `ERRMSG_DECKERROR`，
  WindBot 的 `GameAI.OnDeckError` 会静默 `Connection.Close()`（无日志）——这就是之前「MSG_START 后掉线」的根因。
  内嵌方案里 server 与 bot 共用宿主的卡表，天然消除该问题；独立进程调试时要给 server 准备匹配的 `cards.cdb`。
