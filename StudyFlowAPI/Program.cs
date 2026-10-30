using DAL;

var builder = WebApplication.CreateBuilder(args);

// רישום ה-Controllers של ה-API.
builder.Services.AddControllers();

// רישום OpenAPI לצורכי בדיקה ותיעוד בזמן פיתוח.
builder.Services.AddOpenApi();

// קריאת מחרוזת החיבור מתוך User Secrets / Configuration.
// אם היא לא קיימת, הפרויקט נעצר עם הודעת שגיאה ברורה.
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

// מוסיפים את שם בסיס הנתונים של StudyFlow למחרוזת החיבור.
var databaseConnectionString =
    connectionString + "Database=StudyFlowDB;";

// רישום UserDB כדי שה-Controller יוכל לקבל אותו דרך Dependency Injection.
builder.Services.AddSingleton(
    new UserDB(databaseConnectionString));

var app = builder.Build();

// יצירת בסיס הנתונים והטבלאות במקרה שהם עדיין לא קיימים.
await DatabaseInitializer.InitializeAsync(connectionString);

// בזמן פיתוח מאפשרים את OpenAPI.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// הפניית בקשות HTTP ל-HTTPS.
// זה כבר היה בפרויקט ולא חלק מהשינויים של V5.
app.UseHttpsRedirection();

// מאפשר שימוש במנגנון ההרשאות של ASP.NET Core.
app.UseAuthorization();

// מחבר את ה-routes של ה-Controllers לשרת.
app.MapControllers();

// הפעלת ה-API.
app.Run();