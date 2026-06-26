#!/bin/bash
# Package the extension as a VSIX
set -e
cd "$(git rev-parse --show-toplevel)"

npm run compile
npm run publish-server

npx @vscode/vsce package

echo "\n✓ VSIX created:"
ls -la *.vsix
