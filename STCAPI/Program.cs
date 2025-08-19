using STCAPI;
using STCAPI.InteractionExcel;
using STCAPI.ScheduleAlgo;
using STCAPI.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // указывает, будет ли валидироваться издатель при валидации токена
            ValidateIssuer = true,
            // строка, представляющая издателя
            ValidIssuer = AuthOptions.ISSUER,
            // будет ли валидироваться потребитель токена
            ValidateAudience = true,
            // установка потребителя токена
            ValidAudience = AuthOptions.AUDIENCE,
            // будет ли валидироваться время существования
            ValidateLifetime = true,
            // установка ключа безопасности
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
            // валидация ключа безопасности
            ValidateIssuerSigningKey = true,
        };
    });

builder.Services.AddControllers();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// В будущем список аудиторий будет приходить в качестве входных данных и их нужно резервировать. Так же нужно будет из список пожеланий выписать и тоже резервировать.
//List<string> aud = new List<string>() { "0203", "0206", "0212", "0215", "0227", "0228", "0229", "0231", "0235", "0316", "0321", "0320", "0325", "0317", "0414", "0415", "0417", "0419", "0416", "0418", "0318", "0326", "0329", "0332", "120а", "120б", "2405" };
// 0203, 0206, 0212, 0215, 0227, 0228, 0229, 0231, 0235, 0316, 0321, 0320, 0325, 0317, 0414, 0415, 0417, 0419, 0416, 0418, 0318, 0326, 0329, 0332, 120а, 120б, 2405
//var r = new IExcel();
//var examList = r.ImportDataExcel();
//string StartDate = "16.06.2025";
//string EndDate = "28.06.2025";
//AuditoryFundHandler afHandler = new AuditoryFundHandler();
//ReservedInformation resHandler = afHandler.Reserve(StartDate, EndDate, aud);
//var res = (new ScheduleGenerator()).Generate(StartDate, EndDate, resHandler);
//// Для записи в BusyAuditory данных как занятые аудитории будет использоваться конечный шаблон - из него вытаскиваем данные.
//r.ExportDataExcel(res);
//Console.WriteLine("Программа выполнена успешно!");


//input 1 - 02.06.2025 - 14.06.2025
//input 2 - 09.06.2025 - 25.06.2025
//input 3 - 16.06.2025 - 05.07.2025
//input 4 - 26.05.2025 - 14.06.2025