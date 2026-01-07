#!/usr/bin/env zsh
set -euo pipefail

rm -rf bin
rm -rf obj
dotnet restore
dotnet build --no-restore
# dotnet run -- extract \
#   --config /Users/danijeljw/Developer/EbayListingGenerator/config.json \
#   --dir /Users/danijeljw/Developer/EbayListingGenerator/INST/INST \
#   --serial PF1TYB36 \
#   --out ./PF1TYB36.listing.json




BASE_DIR="/Users/danijeljw/Developer/EbayListingGenerator/INST/INST"
CONFIG="/Users/danijeljw/Developer/EbayListingGenerator/config.json"
OUT_DIR="./out"

mkdir -p "$OUT_DIR"

for file in "$BASE_DIR"/*.txt; do
  name="$(basename "$file")"

  # skip files containing '_' in the filename
  if [[ "$name" == *"_"* ]]; then
    continue
  fi

  # strip .txt extension → serial
  serial="${name%.txt}"

  dotnet run -- extract \
    --config "$CONFIG" \
    --dir "$BASE_DIR" \
    --serial "$serial" \
    --out "$OUT_DIR/$serial.listing.json"
done
