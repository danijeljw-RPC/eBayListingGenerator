#!/usr/bin/env zsh

for f in ./INST/img/*.HEIC; do magick "$f" -auto-orient -colorspace sRGB -quality 92 -strip "${f%.HEIC}.jpg"; done
rm ./INST/img/*.HEIC
