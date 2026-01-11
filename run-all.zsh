#!/usr/bin/env zsh
set -euo pipefail

rm -rf bin
rm -rf obj
dotnet restore
dotnet build --no-restore

BASE_DIR="./INST/INST"
CONFIG="./config.json"
OUT_DIR="./out"
HTML_DIR="./html"

# reset output dirs
for dir in "$OUT_DIR" "$HTML_DIR"; do
  if [ -d "$dir" ]; then
    rm -rf "$dir"
  fi
  mkdir -p "$dir"
done

# ---------------------------
# PASS 1: generate JSON
# ---------------------------
for file in "$BASE_DIR"/*.txt; do
  name="$(basename "$file")"

  # skip files containing '_'
  if [[ "$name" == *"_"* ]]; then
    continue
  fi

  serial="${name%.txt}"

  dotnet run -- extract \
    --config "$CONFIG" \
    --dir "$BASE_DIR" \
    --serial "$serial" \
    --out "$OUT_DIR/$serial.listing.json"
done

# ---------------------------
# PASS 2: render HTML
# ---------------------------
for json in "$OUT_DIR"/*.listing.json; do
  serial="$(basename "$json" .listing.json)"

  dotnet run -- render \
    --json "$json" \
    --template default \
    --out "$HTML_DIR/$serial.html"
done
