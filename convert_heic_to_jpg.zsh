#!/usr/bin/env zsh

for f in *.HEIC; do magick "$f" -auto-orient -colorspace sRGB -quality 92 -strip "${f%.HEIC}.jpg"; done

