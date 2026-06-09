#!/usr/bin/env bash
# One-click reproduce (Linux/macOS)
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet run -- profiles/lna_plan.csv
