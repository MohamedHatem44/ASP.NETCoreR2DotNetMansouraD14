using ASP.NETCoreD14.Data.Context;
using ASP.NETCoreD14.Data.Models;
using ASP.NETCoreD14.Srttings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

namespace ASP.NETCoreD14
{
    public class Program
    {
        public static void Main(string[] args)
        {
            /*------------------------------------------------------------------*/
            var builder = WebApplication.CreateBuilder(args);
            /*------------------------------------------------------------------*/
            // Add services to the container.
            builder.Services.AddControllers();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("ASPNETCoreD14"));
            });
            /*------------------------------------------------------------------*/
            // Configure strongly typed settings objects
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
            /*------------------------------------------------------------------*/
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
            /*------------------------------------------------------------------*/
            builder.Services.Configure<IdentityOptions>(options =>
            {
                //options.Lockout.MaxFailedAccessAttempts = 3;
                //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                //options.Lockout.AllowedForNewUsers = true;

                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;

                options.User.RequireUniqueEmail = false;

                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 4;
            });
            /*------------------------------------------------------------------*/
            // Authenticate – Check if the request has valid credentials(like a JWT token).
            // This uses the DefaultAuthenticateScheme.
            // If valid, User is populated.
            // Challenge – What to do if authentication fails(no token or invalid token).
            // This uses the DefaultChallengeScheme.
            // By default, it could redirect to login, return 404, or return 401 depending on the scheme.
            /*------------------------------------------------------------------*/
            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtSettings.SecretKey)),
                    //IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SecretKey)),
                };
            });
            /*------------------------------------------------------------------*/
            builder.Services.AddAuthorization(options =>
            {
                // Example 1: Role-based policy
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));

                // Example 2: Claim-based policy
                options.AddPolicy("EmployeeOnly", policy =>
                    policy.RequireClaim("EmployeeNumber"));

                // Example 1: Role-based policy - Claim Based Policy
                options.AddPolicy("Employee", policy =>
                    policy.RequireRole("Admin").RequireClaim("EmployeeNumber"));

                // Example 4: Custom requirement policy
                options.AddPolicy("Over18", policy =>
                policy.RequireAssertion(context =>
                context.User.HasClaim(c => c.Type == "Age" && int.Parse(c.Value) >= 18)));
            });
            /*------------------------------------------------------------------*/
            // OpenAPI
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
            /*------------------------------------------------------------------*/
        }
    }
}
