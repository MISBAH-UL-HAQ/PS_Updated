using PatientSimulatorAPI.Filters;
using PatientSimulatorAPI.Interfaces;
using PatientSimulatorAPI.Models;
using PatientSimulatorAPI.Repositories;
using PatientSimulatorAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "PatientSimulatorAPI",
        Version = "v1"
    });
    // This tells Swagger to apply our file-upload handling
    c.OperationFilter<FormFileOperationFilter>();
});
// Add CORS configuration.

// Bind AzureSettings from appsettings.json.
builder.Services.Configure<AzureSettings>(builder.Configuration.GetSection("AzureSettings"));

// Register our speech service.
builder.Services.AddSingleton<IOpenAIService, AzureOpenAIService>();
builder.Services.AddSingleton<ISpeechService, AzureSpeechService>();

// Register our custom services and repository interfaces
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IPatientPromptRepository, PatientSimulatorAPI.Repositories.PatientPromptRepository>();
builder.Services.AddSingleton<PatientSimulatorAPI.Interfaces.IChatService, PatientSimulatorAPI.Services.ChatService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PatientSimulatorAPI v1");
    });
}

app.UseHttpsRedirection();

// Apply the CORS policy.
app.UseCors("AllowAllPolicy");

app.UseAuthorization();
app.UseMiddleware<PatientSimulatorAPI.Middlewares.ErrorHandlingMiddleware>();
app.MapControllers();

app.Run();
