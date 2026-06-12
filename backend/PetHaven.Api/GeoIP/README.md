# GeoIP Database

Place the MaxMind GeoLite2 City database file here:

```text
GeoLite2-City.mmdb
```

The API reads it from:

```json
"GeoLocation": {
  "DatabasePath": "GeoIP/GeoLite2-City.mmdb"
}
```

The `.mmdb` file is not committed because MaxMind requires an account/license for download.