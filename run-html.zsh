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

find "$HTML_DIR" -type f -name "*.html" -exec gsed -i 's/&amp;bull;/\&bull;/g' {} +

find "$HTML_DIR" -type f -name "*.html" -exec gsed -i 's/VeryGood/Very Good/g' {} +
