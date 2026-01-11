# eBay Listing Generator

## Convert all HEIC to JPG

```sh
for f in *.HEIC; do magick "$f" -auto-orient -colorspace sRGB -quality 92 -strip "${f%.HEIC}.jpg"; done
```
