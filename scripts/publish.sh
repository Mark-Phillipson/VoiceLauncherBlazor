#!/usr/bin/env bash
set -euo pipefail

RUNTIMES=("win-x64" "linux-x64" "osx-x64")
CONFIG=${1:-Release}

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/.."
REPO_ROOT="$SCRIPT_DIR"
cd "$REPO_ROOT"

declare -a PROJECTS=(
  "VoiceAdmin"
)
WINFORMS="WinFormsApp"

for RID in "${RUNTIMES[@]}"; do
  echo "\n--- Publishing runtime: $RID ---"
  OUTDIR="$REPO_ROOT/artifacts/$RID"
  mkdir -p "$OUTDIR"

  IS_WINDOWS=false
  if [[ $RID == win* ]]; then IS_WINDOWS=true; fi

  TO_PUBLISH=("${PROJECTS[@]}")
  if [ "$IS_WINDOWS" = true ]; then
    TO_PUBLISH+=("$WINFORMS")
  fi

  for P in "${TO_PUBLISH[@]}"; do
    PROJ_PATH="$REPO_ROOT/$P"
    if [ ! -d "$PROJ_PATH" ]; then echo "Warning: project path not found: $PROJ_PATH"; continue; fi
    CSProj=$(find "$PROJ_PATH" -maxdepth 2 -name "*.csproj" | head -n 1)
    if [ -z "$CSProj" ]; then echo "No csproj found in $PROJ_PATH"; continue; fi

    PROJECT_OUT="$OUTDIR/$P"
    rm -rf "$PROJECT_OUT" && mkdir -p "$PROJECT_OUT"

    echo "Publishing $P for $RID -> $PROJECT_OUT"
    dotnet publish "$CSProj" -c $CONFIG -r $RID --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "$PROJECT_OUT"

    ZIP_NAME="$P-$RID.zip"
    ZIP_PATH="$OUTDIR/$ZIP_NAME"
    rm -f "$ZIP_PATH"
    (cd "$PROJECT_OUT" && zip -r "$ZIP_PATH" .)
    echo "Created $ZIP_PATH"
  done
done

echo "Publish complete. Artifacts in ./artifacts"