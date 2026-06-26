#!/bin/bash
# Build everything: server + extension + publish
set -e
cd "$(git rev-parse --show-toplevel)"
dotnet build server/ && npm run compile && npm run publish-server
echo "\n✓ Ready — press F5 to test"