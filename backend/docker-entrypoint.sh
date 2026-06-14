#!/bin/sh
set -u

geoip_dir="/app/GeoIP"
geoip_database="${geoip_dir}/GeoLite2-City.mmdb"

if [ ! -f "$geoip_database" ] && [ -n "${MAXMIND_LICENSE_KEY:-}" ]; then
    archive="$(mktemp)"
    extract_dir="$(mktemp -d)"
    download_url="https://download.maxmind.com/app/geoip_download?edition_id=GeoLite2-City&license_key=${MAXMIND_LICENSE_KEY}&suffix=tar.gz"

    echo "Downloading MaxMind GeoLite2 City database..."
    if curl -fsSL "$download_url" -o "$archive" \
        && tar -xzf "$archive" -C "$extract_dir"; then
        database_file="$(find "$extract_dir" -name GeoLite2-City.mmdb -print -quit)"
        if [ -n "$database_file" ]; then
            mkdir -p "$geoip_dir"
            mv "$database_file" "$geoip_database"
            echo "MaxMind GeoLite2 City database installed."
        else
            echo "Warning: GeoLite2 archive did not contain GeoLite2-City.mmdb." >&2
        fi
    else
        echo "Warning: Could not download GeoLite2 database; geo-location fallback will be used." >&2
    fi

    rm -rf "$archive" "$extract_dir"
elif [ ! -f "$geoip_database" ]; then
    echo "Warning: MAXMIND_LICENSE_KEY is not configured; geo-location fallback will be used." >&2
fi

export ASPNETCORE_URLS="http://+:${PORT:-8080}"
exec dotnet PetHaven.Api.dll
