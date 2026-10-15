using DAL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

var databaseConnectionString =
    connectionString + "Database=StudyFlowDB;";

builder.Services.AddSingleton(
    new UserDB(databaseConnectionString));

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(connectionString);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();