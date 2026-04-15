# HandyRank

HandyRank is a web application built using the .NET Blazor framework that connects customers with local handymen for everyday services such as repairs, installations, and maintenance.

The platform includes service requests, applications, reviews, and a location-aware system powered by external APIs to improve matching between customers and professionals.

---

## Local database setup

Store the PostgreSQL connection string with .NET user-secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=handyrank;Username=postgres;Password=your-password"
```

Render-style URLs also work:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "postgresql://user:password@host/database?sslmode=require"
```

For deployment, set either `ConnectionStrings__DefaultConnection` or `DATABASE_URL` in the hosting environment.
Do not commit real database credentials.

---

## Google Maps (Location Autocomplete)

HandyRank uses the Google Maps Platform to provide location autocomplete and geocoding.

### Required API

* Places API (Autocomplete + Place Details)

---

### Local setup (development)

Store the API key using user-secrets:

```bash
dotnet user-secrets set "GoogleMaps:ApiKey" "YOUR_API_KEY"
```

---

### Production setup

Set the following environment variable:

```bash
GoogleMaps__ApiKey=YOUR_API_KEY
```

---

### Security recommendations

* Do not store API keys in `appsettings.json`
* Restrict the key in Google Cloud:

  * Application restriction: **IP address (server)**
  * API restriction: **Places API only**

---

## Location Architecture

The application uses a layered approach for location handling:

```
Blazor Components
   ↓
/api/location endpoints
   ↓
LocationService
   ↓
Google Maps API
```

### Responsibilities

* **Blazor components**
  Handle user input and call internal endpoints

* **LocationEndpoints**
  Expose `/api/location/autocomplete` and `/details`

* **LocationService**
  Integrates with Google APIs and maps responses to internal DTOs

---

## Notes

* Autocomplete is powered by Google Places API (`types=geocode`)
* Selected locations are resolved into latitude/longitude coordinates
* The system is designed to support future features such as:

  * distance-based search
  * map visualization
  * ranking by proximity

---

## Development notes

* Internal API calls use `NavigationManager.BaseUri` to ensure compatibility across environments (local and production)
* External API calls (Google) are handled server-side to avoid exposing API keys
* Avoid hardcoding URLs such as `localhost` in client code

---

## Summary

HandyRank combines Blazor Server, PostgreSQL, and external location services to provide a scalable foundation for marketplace-style applications with real-world geographic awareness.