# HandyRank
HandyRank is a web application built using the .NET Blazor framework that connects customers with local handymen for everyday services such as repairs, installations, and maintenance.

## Local database setup

Store the PostgreSQL connection string with .NET user-secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=handyrank;Username=postgres;Password=your-password"
```

Render-style URLs also work:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "postgresql://user:password@host/database?sslmode=require"
```

For deployment, set either `ConnectionStrings__DefaultConnection` or `DATABASE_URL` in the hosting environment. Do not commit real database credentials.
