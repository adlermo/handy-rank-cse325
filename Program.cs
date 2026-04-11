using Microsoft.EntityFrameworkCore;
using HandyRank.Components;
using HandyRank.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var connectionString = GetDatabaseConnectionString(builder.Configuration);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, o =>
        o.EnableRetryOnFailure()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/test-db", async (AppDbContext db) =>
    {
        var users = await db.Users.ToListAsync();
        return Results.Ok(users);
    });
}

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static string GetDatabaseConnectionString(IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        connectionString = configuration["DATABASE_URL"];
    }

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Database connection string is missing. Set ConnectionStrings:DefaultConnection with user-secrets locally or ConnectionStrings__DefaultConnection/DATABASE_URL in the host environment.");
    }

    if (Uri.TryCreate(connectionString, UriKind.Absolute, out var uri)
        && (uri.Scheme == "postgres" || uri.Scheme == "postgresql"))
    {
        return ConvertPostgresUrlToConnectionString(uri);
    }

    return connectionString;
}

static string ConvertPostgresUrlToConnectionString(Uri uri)
{
    var userInfo = uri.UserInfo.Split(':', 2);

    if (userInfo.Length != 2)
    {
        throw new InvalidOperationException("PostgreSQL URL must include both username and password.");
    }

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = Uri.UnescapeDataString(userInfo[1]),
        SslMode = SslMode.Require
    };

    return builder.ConnectionString;
}
