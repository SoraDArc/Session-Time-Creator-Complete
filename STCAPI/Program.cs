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
            // ���������, ����� �� �������������� �������� ��� ��������� ������
            ValidateIssuer = true,
            // ������, �������������� ��������
            ValidIssuer = AuthOptions.ISSUER,
            // ����� �� �������������� ����������� ������
            ValidateAudience = true,
            // ��������� ����������� ������
            ValidAudience = AuthOptions.AUDIENCE,
            // ����� �� �������������� ����� �������������
            ValidateLifetime = true,
            // ��������� ����� ������������
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
            // ��������� ����� ������������
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
// � ������� ������ ��������� ����� ��������� � �������� ������� ������ � �� ����� �������������. ��� �� ����� ����� �� ������ ��������� �������� � ���� �������������.
//List<string> aud = new List<string>() { "0203", "0206", "0212", "0215", "0227", "0228", "0229", "0231", "0235", "0316", "0321", "0320", "0325", "0317", "0414", "0415", "0417", "0419", "0416", "0418", "0318", "0326", "0329", "0332", "120�", "120�", "2405" };
// 0203, 0206, 0212, 0215, 0227, 0228, 0229, 0231, 0235, 0316, 0321, 0320, 0325, 0317, 0414, 0415, 0417, 0419, 0416, 0418, 0318, 0326, 0329, 0332, 120�, 120�, 2405
//var r = new IExcel();
//var examList = r.ImportDataExcel();
//string StartDate = "16.06.2025";
//string EndDate = "28.06.2025";
//AuditoryFundHandler afHandler = new AuditoryFundHandler();
//ReservedInformation resHandler = afHandler.Reserve(StartDate, EndDate, aud);
//var res = (new ScheduleGenerator()).Generate(StartDate, EndDate, resHandler);
//// ��� ������ � BusyAuditory ������ ��� ������� ��������� ����� �������������� �������� ������ - �� ���� ����������� ������.
//r.ExportDataExcel(res);
//Console.WriteLine("��������� ��������� �������!");


//input 1 - 02.06.2025 - 14.06.2025
//input 2 - 09.06.2025 - 25.06.2025
//input 3 - 16.06.2025 - 05.07.2025
//input 4 - 26.05.2025 - 14.06.2025