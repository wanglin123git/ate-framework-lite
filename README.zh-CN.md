# ate-framework-lite

[English](README.md) | **简体中文**

[![CI](https://github.com/wanglin123git/ate-framework-lite/actions/workflows/ci.yml/badge.svg)](https://github.com/wanglin123git/ate-framework-lite/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)

一个轻量级、**厂商无关**的射频/微波器件自动化测试框架。测试逻辑只需面向**单一驱动契约**编写一次，即可在使用**不同指令方言**（传统短助记符 vs 现代 SCPI）的仪器上**无改动运行**。测试方案是**配置**而非代码，报表自动生成。

本仓库是论文 *《A Vendor-Agnostic Automation Framework for RF/Microwave Device Characterization Using Heterogeneous Instrumentation》* 的开源、完全可复现配套实现。它使用**仿真仪器**和**合成 LNA 模型**，任何人无需硬件、无需专有数据即可复现结果。

## 核心思想
- **`IDeviceControl` / `ICapabilityInstrument`** —— 单一驱动契约；引擎从不依赖具体厂商或方言。
- **两台仿真仪器** —— `LegacyMnemonicInstrument`（`cf <Hz>`、`mg?`）与 `ScpiInstrument`（`FREQ:CENT <v> GHZ`、`MEAS:GAIN?`）用**不同指令集**暴露**相同**操作。
- **配置驱动引擎** —— `TestEngine` 按每个步骤的测量量，通过委托表（delegate table）派发；测试方案以 CSV 配置档形式存在。
- **合成被测件（DUT）** —— `SyntheticLna` 是一个可复现的宽带低噪放行为模型（增益、噪声系数、输入 P-1、饱和功率）。

## 运行

**最快（Windows）：** 双击仓库根目录的 **`demo.cmd`**。它会编译并运行 demo，跑完**保持窗口不关**，方便你查看结果。

**独立 demo.exe（目标机无需安装 .NET 运行时）：**
```powershell
powershell -ExecutionPolicy Bypass -File scripts\build-demo.ps1
```
这会发布一个自包含的 **`dist\demo.exe`**，可复制到任意位置双击运行。示例配置档会随 exe 一起放在同目录，因此在任何文件夹下都能跑。

**直接用 .NET SDK：**
```bash
dotnet run -- profiles/lna_plan.csv
```
或用一键脚本：
```bash
./scripts/run.sh        # Linux/macOS
scripts\run.ps1         # Windows PowerShell
```

以上所有方式，程序都会用**同一份测试方案**分别驱动两种方言的仪器，为每台仪器写出一份 CSV 报表，并断言两种方言下结果完全一致。加 `--no-pause` 可跳过"按回车关闭"提示（CI 与脚本即用此参数）。

## 测试与 CI
```bash
dotnet test                  # 11 个单元测试：模型、跨方言、限值、CSV
```
每次 push/PR 都会在 GitHub Actions 上运行构建 + 测试（`.github/workflows/ci.yml`）。

## 操作工作量基准
```bash
dotnet run -- --bench
```
报告引擎运行耗时，以及手工流程相对本框架所需的操作次数 / 誊抄次数（以示例的 16 步方案为例：手工需 48 次操作、16 次誊抄，本框架仅 1 次操作、0 次誊抄）。

## 合成 DUT 模型
频率 `f` 单位为 GHz：
```
Gain(f) = 20.5 - 0.16*(f-1)        dB
NF(f)   = 1.5  + 0.12*f            dB
P1(f)   = -2.9 - 0.16*f            dBm   （输入 1 dB 压缩点）
Psat(f) = 20.0 - 0.18*f            dBm
```
另可叠加高斯测量噪声 N(0, sigma)。

## 目录结构
```
src/
  IDeviceControl.cs            # 驱动 + 能力契约
  Dut/SyntheticLna.cs          # 可复现的 DUT 模型
  Instruments/                 # 仿真的传统 + SCPI 仪器
  Engine/                      # AutoTestItem、TestEngine、CsvProfile
  Benchmark.cs                 # 操作工作量微基准
  Program.cs                   # demo 运行器（带交互式保持窗口提示）
tests/                         # xUnit 测试工程（11 个测试）
profiles/lna_plan.csv          # 示例配置
demo.cmd                       # 双击即可构建 + 运行（Windows）
scripts/build-demo.ps1         # 发布独立的 dist/demo.exe
scripts/run.sh、run.ps1        # 一键复现
.github/workflows/ci.yml       # 每次 push/PR 构建 + 测试
```

## 许可证
MIT —— 见 [LICENSE](LICENSE)。
