#!/bin/bash
# Send a hover request to the server for quick testing
# Usage: ./smoke.sh "public abstract class X {}" 0 7
set -e
cd "$(git rev-parse --show-toplevel)/server"

TEXT="${1:-public abstract class Animal {}}"
LINE="${2:-0}"
CHAR="${3:-7}"

echo "{\"id\":1,\"method\":\"hover\",\"params\":{\"text\":\"$TEXT\",\"filePath\":null,\"line\":$LINE,\"character\":$CHAR,\"detailLevel\":\"beginner\",\"includeExamples\":true}}" \
  | dotnet run --no-build \
  | python3 -m json.tool
