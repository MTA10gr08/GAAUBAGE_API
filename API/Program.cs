using API.Endpoints;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<AppSettings>(builder.Configuration);

builder.Services.AddSingleton<TokenProvider>();

var appSettings = builder.Configuration.Get<AppSettings>();
if (appSettings == null) throw new Exception("Missing configuration value: AppSettings");

if (appSettings.DB == null)
{
    builder.Services.AddDbContext<DataContext>(options => options.UseSqlite("Data Source=file::memory:?cache=shared", x => x.UseNetTopologySuite()));
}
else
{
    var connectionStringBuilder = new MySqlConnectionStringBuilder
    {
        Server = appSettings.DB.Address,
        Port = appSettings.DB.Port,
        Database = appSettings.DB.Database,
        UserID = appSettings.DB.User,
        Password = appSettings.DB.Password
    };
    var serverVersion = ServerVersion.AutoDetect(connectionStringBuilder.ConnectionString);
    builder.Services.AddDbContext<DataContext>(options => options.UseMySql(connectionStringBuilder.ConnectionString, serverVersion, x => x.UseNetTopologySuite()));
}

//Set up Authentication and Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = appSettings.Jwt.Issuer,
        ValidAudience = appSettings.Jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Jwt.Key))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

//Set up OpenAPI Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    c.EnableAnnotations();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DataContext>();
    context.Database.OpenConnection();
    context.Database.EnsureCreated();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapConfigurationEndpoints();
app.MapUserEndpoints();
app.MapImageEndpoints();
app.MapContextClassificationEndpoints();
app.MapBackgroundclassifiCationEndpoints();
app.MapSubImageEndpoints();
app.MapTrashSubCatagoryEndpoints();
app.MapTrashSuperCategoryEndpoints();
app.MapLeaderboardEndpoints();
app.MapImageAnnotationEndpoints();
app.MapSegmentationEndpoints();

if (builder.Environment.IsDevelopment())
{
    app.MapDebugEndpoints(builder);
}

app.Run();