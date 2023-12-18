using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using YI.Core.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSession();
builder.Services.AddMemoryCache();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            SaveSigninToken = true,//保存token,后台验证token是否生效(重要)
            ValidateIssuer = true,//是否验证Issuer
            ValidateAudience = true,//是否验证Audience
            ValidateLifetime = true,//是否验证失效时间
            ValidateIssuerSigningKey = true,//是否验证SecurityKey
            ValidAudience = AppSetting.Secret.Audience,//Audience
            ValidIssuer = AppSetting.Secret.Issuer,//Issuer，这两项和前面签发jwt的设置一致
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSetting.Secret.JWT))
        };
    });


// 设置文件上传大小限制
builder.Services.Configure<FormOptions>(x =>
{
    x.MultipartBodyLengthLimit = 1024 * 1024 * 100;//100M
});

builder.Services.Configure<KestrelServerOptions>(x =>
{
    x.Limits.MaxRequestBodySize = 1024 * 1024 * 100;//100M
});

builder.Services.Configure<IISServerOptions>(x =>
{
    x.MaxRequestBodySize = 1024 * 1024 * 100;//100M
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
