#!/usr/bin/env bash
set -euo pipefail

# --- Validate argument ---
if [[ $# -ne 1 ]]; then
  echo "Usage: ./release.sh <version>"
  echo "Example: ./release.sh 0.1.0"
  exit 1
fi

VERSION="$1"

if ! [[ "$VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
  echo "Error: version must be in x.y.z format (got '$VERSION')"
  exit 1
fi

# --- Ensure on main branch ---
BRANCH=$(git branch --show-current)
if [[ "$BRANCH" != "main" ]]; then
  echo "Error: must be on 'main' branch (currently on '$BRANCH')"
  exit 1
fi

# --- Ensure working tree is clean ---
if ! git diff --quiet || ! git diff --cached --quiet; then
  echo "Error: working tree is dirty. Commit or stash changes first."
  exit 1
fi

# --- Update package.json version ---
echo "Updating package.json version to $VERSION..."
npm version "$VERSION" --no-git-tag-version --allow-same-version

# --- Prompt user to update changelog ---
echo ""
echo "=== ACTION REQUIRED ==="
echo "Add a changelog section for v$VERSION in CHANGELOG.md."
echo "Opening CHANGELOG.md in your editor..."
echo ""
code --wait CHANGELOG.md

# --- Confirm ---
echo ""
read -rp "Done editing changelog? Commit and publish v$VERSION? [y/N] " CONFIRM
if [[ "$CONFIRM" != "y" && "$CONFIRM" != "Y" ]]; then
  echo "Aborting. package.json version was updated but nothing was committed."
  exit 1
fi

# --- Commit ---
git add package.json package-lock.json CHANGELOG.md
git commit -m "release: v$VERSION"

# --- Tag ---
git tag "v$VERSION"

# --- Push ---
echo "Pushing to main..."
git push origin main
git push origin "v$VERSION"

# --- Publish to Marketplace (platform-specific) ---
# Requires: az login (one-time Azure CLI login)
# Requires: a publisher registered at https://marketplace.visualstudio.com/manage
#
# Each platform gets a self-contained native binary so users don't need dotnet installed.

TARGETS=(
  "win32-x64:win-x64"
  "win32-arm64:win-arm64"
  "linux-x64:linux-x64"
  "linux-arm64:linux-arm64"
  "darwin-x64:osx-x64"
  "darwin-arm64:osx-arm64"
)

echo ""
echo "Building and publishing platform-specific packages..."

for entry in "${TARGETS[@]}"; do
  VSCE_TARGET="${entry%%:*}"
  DOTNET_RID="${entry##*:}"

  echo ""
  echo "--- $VSCE_TARGET ($DOTNET_RID) ---"

  # Clean previous platform's server binaries
  rm -rf ./out/server

  # Build self-contained single-file binary
  dotnet publish ./server/CSharpLearningServer.csproj \
    -c Release \
    -r "$DOTNET_RID" \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -o ./out/server

  # Compile TypeScript
  npm run compile

  # Package for this platform (upload manually via Marketplace portal)
  npx @vscode/vsce package --target "$VSCE_TARGET"
  # Package and publish for this platform
  # npx @vscode/vsce publish --azure-credential --target "$VSCE_TARGET"
done

echo ""
echo "✓ v$VERSION released and published for all platforms!"
