# 轨道扫码与 MES 上传系统

基于 .NET Framework 4.0 + WinForms 开发的桌面应用，用于从轨道扫码枪采集条码数据并上传至 MES 系统。

## 功能

- 支持 4 路串口条码枪同时采集
- 单面/双面模式切换（V2.1+）
- NOREAD 自动拦截，不推送至 MES
- 串口参数配置保存
- MES HTTP API 数据上传
- 操作日志记录（Access .mdb）

## 技术栈

- .NET Framework 4.0 Client Profile
- C# WinForms
- Access 数据库 (.mdb)
- Newtonsoft.Json
- System.Web.Extensions

## 版本历史

| 版本 | 日期 | 说明 |
|------|------|------|
| V1.0 | 2026-04-09 | 初始版本 |
| V1.1 | 2026-04-12 | - |
| V2.0 | 2026-07-02 | - |
| V2.1 | 2026-07-14 | 增加单面模式复选框，单面模式参数可保存，NOREAD 不推送 MES，单面模式下仅一路可设置参数 |
| **V2.11** | **2026-07-17** | **最新版本** |

## 项目结构

- `smtomes/` — 主项目源码
- `修改版/` — 修改版源码
- `备份版/` — 备份版源码

## 运行环境

- Windows XP / 7 / 10 / 11
- .NET Framework 4.0+
