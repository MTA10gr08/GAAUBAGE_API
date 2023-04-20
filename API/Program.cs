using API.Endpoints;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//Check required Environment variables
var Configuration = builder.Configuration;

builder.Services.Configure<AppSettings>(Configuration);

//string DBAddress = Configuration["Db:Address"] ?? throw new Exception("Missing configuration value: Db:Address");
//uint DBPort = uint.Parse((Configuration["DB:Port"] ?? throw new Exception("Missing configuration value: DB:Port")));
//string DBDatabase = Configuration["DB:Database"] ?? throw new Exception("Missing configuration value: DB:Database");
//string DBUser = Configuration["DB:User"] ?? throw new Exception("Missing configuration value: DB:User");
//string DBPassword = Configuration["DB:Password"] ?? throw new Exception("Missing configuration value: DB:Password");

/*string JwtIssuer = Configuration["Jwt:Issuer"] ?? throw new Exception("Missing configuration value: Jwt:Issuer");
string JwtAudience = Configuration["Jwt:Audience"] ?? throw new Exception("Missing configuration value: Jwt:Audience");
string JwtKey = Configuration["Jwt:Key"] ?? throw new Exception("Missing configuration value: Jwt:Key");*/

builder.Services.AddSingleton<TokenProvider>();

//Connect to DB
//var connectionStringBuilder = new MySqlConnectionStringBuilder{
//    Server = DBAddress,
//    Port = DBPort,
//    Database = DBDatabase,
//    UserID = DBUser,
//    Password = DBPassword
//};
//var serverVersion = ServerVersion.AutoDetect(connectionStringBuilder.ConnectionString);
//builder.Services.AddDbContext<DataContext>(options => options.UseMySql(connectionStringBuilder.ConnectionString, serverVersion));

//builder.Services.AddDbContext<DataContext>(options => options.UseSqlite("Data Source=:memory:", x => x.UseNetTopologySuite()));
builder.Services.AddDbContext<DataContext>(options => options.UseSqlite("Data Source=file::memory:?cache=shared", x => x.UseNetTopologySuite()));

//Set up Authentication and Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Configuration.GetSection("Jwt:Issuer").Value,
        ValidAudience = Configuration.GetSection("Jwt:Audience").Value,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetSection("Jwt:Key").Value))
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
app.MapImageAnnotationEndpoints();
/*app.MapContextClassificationEndpoints();
app.MapTrashCountEndpoints();
app.MapTrashBoundingBoxEndpoints();
app.MapTrashSuperCategoryEndpoints();
app.MapTrashCategoryEndpoints();
app.MapSegmentationEndpoints();*/

if (builder.Environment.IsDevelopment())
{
    app.MapDebugEndpoints(builder);
}

app.Run();