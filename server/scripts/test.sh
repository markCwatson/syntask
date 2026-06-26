#!/bin/bash
# Run all backend tests
set -e
cd "$(git rev-parse --show-toplevel)/tests/CSharpLearningServer.Tests"
dotnet test --verbosity normal