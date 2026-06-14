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

For Docker deployments such as Render, configure `MAXMIND_LICENSE_KEY` as a secret
environment variable. The container downloads the database to `/app/GeoIP/GeoLite2-City.mmdb`
before starting the API.
