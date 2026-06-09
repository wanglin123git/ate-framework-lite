# ate-framework-lite

[![CI](https://github.com/<your-username>/ate-framework-lite/actions/workflows/ci.yml/badge.svg)](https://github.com/<your-username>/ate-framework-lite/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)

A lightweight, **vendor-agnostic** automation framework for RF/microwave device
characterization. Test logic is written once against a single driver contract
and runs unchanged across instruments that speak **different command dialects**
(legacy short-mnemonic vs modern SCPI). Test plans are **configuration**, not
code, and reports are generated automatically.

This repository is the open, fully reproducible companion to the paper
*"A Vendor-Agnostic Automation Framework for RF/Microwave Device
Characterization Using Heterogeneous Instrumentation."* It uses **simulated
instruments** and a **synthetic LNA model**, so anyone can reproduce the
results with no hardware and no proprietary data.

## Key ideas
- **`IDeviceControl` / `ICapabilityInstrument`** — one driver contract; the
  engine never depends on a concrete vendor or dialect.
- **Two simulated instruments** — `LegacyMnemonicInstrument` (`cf <Hz>`, `mg?`)
  and `ScpiInstrument` (`FREQ:CENT <v> GHZ`, `MEAS:GAIN?`) expose the *same*
  operations through different command sets.
- **Configuration-driven engine** — `TestEngine` dispatches each step through a
  delegate table keyed on the step's quantity; plans live in CSV profiles.
- **Synthetic DUT** — `SyntheticLna` is a reproducible behavioral model of a
  wideband low-noise amplifier (gain, NF, input P-1, saturated power).

## Run
```bash
dotnet run -- profiles/lna_plan.csv
```
or use the one-click scripts:
```bash
./scripts/run.sh        # Linux/macOS
scripts\run.ps1         # Windows PowerShell
```
The program runs the same plan against both dialects, writes a CSV report per
instrument, and asserts that the results are identical across dialects.

## Tests & CI
```bash
dotnet test                  # 11 unit tests: model, cross-dialect, limits, CSV
```
Every push/PR runs build + tests on GitHub Actions (`.github/workflows/ci.yml`).

## Operator-effort benchmark
```bash
dotnet run -- --bench
```
Reports the engine run time and the operator actions / transcription steps a
manual workflow needs versus the framework (for the example 16-step plan:
48 actions and 16 transcriptions manually vs. 1 action and 0 transcriptions).

## Synthetic DUT model
With frequency `f` in GHz:
```
Gain(f) = 20.5 - 0.16*(f-1)        dB
NF(f)   = 1.5  + 0.12*f            dB
P1(f)   = -2.9 - 0.16*f            dBm   (input 1-dB compression)
Psat(f) = 20.0 - 0.18*f            dBm
```
plus optional Gaussian measurement noise N(0, sigma).

## Layout
```
src/
  IDeviceControl.cs            # driver + capability contracts
  Dut/SyntheticLna.cs          # reproducible DUT model
  Instruments/                 # simulated legacy + SCPI instruments
  Engine/                      # AutoTestItem, TestEngine, CsvProfile
  Benchmark.cs                 # operator-effort micro-benchmark
  Program.cs                   # demo runner
tests/                         # xUnit test project (11 tests)
profiles/lna_plan.csv          # example configuration
scripts/                       # one-click reproduce
.github/workflows/ci.yml       # build + test on every push/PR
```

## License
MIT — see [LICENSE](LICENSE).
