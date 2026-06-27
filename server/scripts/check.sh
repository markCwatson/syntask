#!/bin/bash
# Quick check: compile + build + test in one shot
set -e
cd "$(git rev-parse --show-toplevel)"

echo "=== Compiling TypeScript ==="
npm run compile

echo "=== Building server ==="
dotnet build server/

echo "=== Publishing server ==="
npm run publish-server

echo "=== Running tests ==="
cd server/tests/CSharpLearningServer.Tests
dotnet test --verbosity quiet

echo "\n✓ All checks passed"
