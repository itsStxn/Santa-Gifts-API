using Microsoft.AspNetCore.Authentication;
using Santa_Gifts_API.AI.Services;
using Santa_Gifts_API.Tools;


var builder = WebApplication.CreateBuilder(args);

//? Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IRecommenderService, RecommenderService>();

//? Add API key authentication layer
builder.Services.AddAuthentication("ApiKeyScheme")
	.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKeyScheme", null);
builder.Services.AddSwaggerGen(c => {
	c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
		Description = "API Key needed to access the endpoints. Use: MY-API-KEY: {your_api_key}",
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Name = "MY-API-KEY",
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
		Scheme = "ApiKeyScheme"
	});
	c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {{
		new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
			Reference = new Microsoft.OpenApi.Models.OpenApiReference {
				Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
				Id = "ApiKey"
			}
		},
		new List<string>()
	}});
});


//? Add app settings
var app = builder.Build();

if (app.Environment.IsDevelopment()) {
	app.UseSwagger();
	app.UseSwaggerUI();
	app.MapGet("/", (HttpContext context) => {
		context.Response.Redirect("/swagger");
	});
}

app.UseCors("AllowAllOrigins");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
