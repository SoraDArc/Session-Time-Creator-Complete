using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using STCAPI.InteractionExcel;
using STCAPI.Models;
using STCAPI.ScheduleAlgo;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System;
using Newtonsoft.Json.Schema;

namespace STCAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class STEController : ControllerBase
    {
        StcdbContext db = new StcdbContext();

        [HttpPost("login")]
        public IActionResult GetLogin() 
        {
            string? json = Request.Form["userData"];
            if (json == null)
            {
                return BadRequest("Неверный формат данных");
            }
            User userData = JsonSerializer.Deserialize<User>(json);
            User? userDB = db.Users.FirstOrDefault(u => u.Login == userData.Login && u.Password == userData.Password);
            if (userDB is null) return Unauthorized();

            var claims = new List<Claim> { new Claim(ClaimTypes.Name, userData.Login) };
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: claims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(300)), // время действия 30 минут
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            var response = new
            {
                access_token = encodedJwt,
                login = userData.Login
            };
            return Ok(response);
        }

        [HttpGet("hello")]
        public IActionResult Hello()
        {
            return Ok("HelloWorlds!");
        }

        [HttpGet("getUserData/{login}")]
        [Authorize]
        public IActionResult GetUserData(string login)
        {
            User user = db.Users.Include(u => u.Institutes).FirstOrDefault(u => u.Login == login);
            if (user == null) return Unauthorized();
            return Ok(user);
        }

        #region SessionTimeCreatorMainAPI
        [HttpPost("createST")]
        public async Task<IActionResult> CreateSchedule()
        {
            try
            {
                IFormFile? file = Request.Form.Files["file"];
                if (file == null || file.Length == 0)
                    return BadRequest("Файл не загружен.");

                string ext = Path.GetExtension(file.FileName).ToLower();
                if (ext != ".xlsx")
                    return BadRequest("Поддерживаются только файлы формата .xlsx");

                string? cabinets = Request.Form["cabinets"];
                string? startDate = Request.Form["startDate"];
                string? endDate = Request.Form["endDate"];
                string? userId = Request.Form["userID"];
                string? instituteId = Request.Form["instituteID"];
                if (string.IsNullOrEmpty(cabinets) ||
                    !DateTime.TryParse(startDate, out DateTime startDateTemp) ||
                    !DateTime.TryParse(endDate, out DateTime endDateTemp) ||
                    string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(instituteId))
                {
                    return BadRequest("Некорректные дополнительные данные (список аудиторий или даты).");
                }

                var uploadsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads", "ExcelFiles");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                IExcel IE = new IExcel();
                var tuple = IE.ImportDataExcel(filePath);
                var ExamList = tuple.Item1; 
                var errors = tuple.Item2;

                if (errors.Count > 0)
                {
                    return BadRequest(new
                    {
                        message = "Исправьте формат данных в excel таблице.",
                        errors = errors
                    }); 
                }
                IE.SaveUnknownData(ExamList, Int32.Parse(userId));

                ScheduleGenerator scheduleGenerator = new ScheduleGenerator();
                var result = scheduleGenerator.Generate(startDate, endDate, cabinets, ExamList, Int32.Parse(userId));
                var jsonTemplate = JsonSerializer.Serialize(result.Item1);
                ScheduleTemplate schedTemp = new ScheduleTemplate();
                schedTemp.JsonTemplate = jsonTemplate;
                schedTemp.Name = file.FileName;

                schedTemp.OwnerUserId = Int32.Parse(userId);
                schedTemp.InstitutesId = Int32.Parse(instituteId);

                ScheduleInformation schedInfo = new ScheduleInformation();
                schedInfo.AuditoryReserveId = result.Item2;
                schedInfo.ScTemplate = schedTemp;
                var jsonData = JsonSerializer.Serialize(schedInfo);
                return Ok(jsonData);
            }
            catch (Exception ex) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    error = "Ошибка при обработке запроса",
                    details = ex.Message
                });
            }
        }

        [HttpPost("confirmentST")]
        public async Task<IActionResult> ConfirmentST()
        {
            string? jsonScTemp = Request.Form["scheduleInformation"];
            string? jsonConfirment = Request.Form["confirment"];

            if (jsonScTemp == null || jsonConfirment == null)
            {
                return BadRequest("Неверный формат данных");
            }

            bool Confirment = JsonSerializer.Deserialize<bool>(jsonConfirment);
            ScheduleInformation schedInfo = JsonSerializer.Deserialize<ScheduleInformation>(jsonScTemp);
            var afh = new AuditoryFundHandler();
            var schId = afh.ConfirmationFund(schedInfo, Confirment);
            if (schId == 0) 
            {
                return Ok();
            }
            else
            {
                return Ok(schId);
            }
        }

        [HttpGet("exportToExcel/{id}")]
        public async Task<IActionResult> ExportSTToExcel(int id)
        {
            try
            {
                var schT = db.ScheduleTemplates.FirstOrDefault(st => st.Id == id);
                List<STElement> ExamList = JsonSerializer.Deserialize<List<STElement>>(schT.JsonTemplate);
                string FilePath = (new IExcel()).ExportDataExcel(ExamList, schT.Name);
                byte[] excelFileBytes = System.IO.File.ReadAllBytes(FilePath);

                return File(excelFileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Path.GetFileName(FilePath));
            }
            catch
            {
                return BadRequest("Не удалось перенести данные в excel");
            }  
        }

        [HttpGet("myTemplates/{id}")]
        public async Task<IActionResult> myTemplatesGet(int id)
        {
            List<ScheduleInformation> schedInfoResult = new List<ScheduleInformation>();
            var AudReserve = await db.AuditoryReserves.Where(ar => ar.UsersId == id).ToListAsync();
            foreach (var ar in AudReserve) 
            {
                var temp = new ScheduleInformation();   
                temp.AuditoryReserveId = ar.Id;
                temp.ScTemplate = db.ScheduleTemplates.FirstOrDefault(st => st.Id == ar.ScheduleTemplatesId);
                schedInfoResult.Add(temp);
            }
            return Ok(schedInfoResult);
        }

        [HttpPut("updateMyTemplate")]
        public async Task<IActionResult> UpdateMyTemplate()
        {
            string? jsonScTemp = Request.Form["scheduleInformation"];

            if (jsonScTemp == null)
            {
                return BadRequest("Неверный формат данных");
            }
            ScheduleInformation schedInfo = JsonSerializer.Deserialize<ScheduleInformation>(jsonScTemp);

            (new AuditoryFundHandler()).UpdateFund(schedInfo);
            return Ok();
        }

        [HttpDelete("deleteMyTemplate/{id}")]
        public async Task<IActionResult> DeleteMyTemplates(int id)
        {
            var auditory_reserve = await db.AuditoryReserves.FirstOrDefaultAsync(ar => ar.Id == id);
            if (auditory_reserve == null)
            {
                return NotFound();
            }

            // Пример правильного удаления шаблона:
            //DELETE FROM busy_auditory WHERE auditory_reserve_id = 17;
            //DELETE FROM auditory_reserve WHERE id = 17;
            //DELETE FROM schedule_templates WHERE id = 5
            try
            {
                var deletedAuditories = db.BusyAuditories
                .Where(ba => ba.AuditoryReserveId == id).ToList();

                db.BusyAuditories.RemoveRange(deletedAuditories);
                await db.SaveChangesAsync();

                var schId = auditory_reserve.ScheduleTemplatesId;

                db.AuditoryReserves.Remove(auditory_reserve);
                await db.SaveChangesAsync();

                var deletedSchTemp = db.ScheduleTemplates
                    .FirstOrDefault(s => s.Id == schId);
                db.ScheduleTemplates.Remove(deletedSchTemp);
                await db.SaveChangesAsync();

                return Ok();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest(ex.ToString());
            }
        }
        #endregion

        #region CRUD Lecturer
        [HttpGet("lecturer")]
        public async Task<ActionResult<IEnumerable<Lecturer>>> LecturerGet()
        {
            return await db.Lecturers.Include(l => l.Institutes).ToListAsync();
        }

        [HttpDelete("lecturer/delete/{id}")]
        public async Task<ActionResult<IEnumerable<Lecturer>>> LecturerDelete(int id)
        {
            Lecturer? lect = db.Lecturers.FirstOrDefault(x => x.Id == id);
            if (lect == null)
            {
                return NotFound();
            }
            db.Lecturers.Remove(lect);
            await db.SaveChangesAsync();
            return Ok(lect);
        }

        [HttpPost("lecturer/save")]
        public async Task<ActionResult<IEnumerable<Lecturer>>> LecturerSave(Lecturer lect)
        {
            if (lect == null)
            {
                return BadRequest();
            }

            var temp = new Lecturer() {Id = lect.Id, 
                Shortname = lect.Shortname, 
                Surname = lect.Surname,
                Name = lect.Name,
                Patronomyc = lect.Patronomyc,
                InstitutesId = lect.InstitutesId};
            // https://qna.habr.com/q/1344730 
            db.Lecturers.Add(temp);
            await db.SaveChangesAsync();
            lect.Id = temp.Id;
            return Ok(lect);
        }

        [HttpPut("lecturer/edit")]
        public async Task<ActionResult<IEnumerable<Lecturer>>> LecturerEdit(Lecturer lect)
        {
            if (lect == null)
            {
                return BadRequest();
            }
            if (!db.Lecturers.Any(x => x.Id == lect.Id))
            {
                return NotFound();
            }
            var temp = new Lecturer()
            {
                Id = lect.Id,
                Shortname = lect.Shortname,
                Surname = lect.Surname,
                Name = lect.Name,
                Patronomyc = lect.Patronomyc,
                InstitutesId = lect.InstitutesId
            };
            db.Update(temp);
            await db.SaveChangesAsync();
            return Ok(lect);
        }
        #endregion

        #region CRUD Cabinet
        [HttpGet("cabinet")]
        public async Task<ActionResult<IEnumerable<Cabinet>>> CabinetGet()
        {
            return await db.Cabinets.Include(g => g.Institutes).ToListAsync();
        }

        [HttpDelete("cabinet/delete/{id}")]
        public async Task<ActionResult<IEnumerable<Cabinet>>> CabinetDelete(int id)
        {
            Cabinet? cab = db.Cabinets.FirstOrDefault(x => x.Id == id);
            if (cab == null)
            {
                return NotFound();
            }
            db.Cabinets.Remove(cab);
            await db.SaveChangesAsync();
            return Ok(cab);
        }

        [HttpPost("cabinet/save")]
        public async Task<ActionResult<IEnumerable<Cabinet>>> CabinetSave(Cabinet cab)
        {
            if (cab == null)
            {
                return BadRequest();
            }

            var temp = new Cabinet() { 
                Id = cab.Id,
                Name = cab.Name,
                Type = cab.Type,
                InstitutesId = cab.InstitutesId};
            // https://qna.habr.com/q/1344730 
            db.Cabinets.Add(temp);
            await db.SaveChangesAsync();
            cab.Id = temp.Id;
            return Ok(cab);
        }

        [HttpPut("cabinet/edit")]
        public async Task<ActionResult<IEnumerable<Cabinet>>> CabinetEdit(Cabinet cab)
        {
            if (cab == null)
            {
                return BadRequest();
            }
            if (!db.Cabinets.Any(x => x.Id == cab.Id))
            {
                return NotFound();
            }
            var temp = new Cabinet()
            {
                Id = cab.Id,
                Name = cab.Name,
                Type = cab.Type,
                InstitutesId = cab.InstitutesId
            };
            db.Update(temp);
            await db.SaveChangesAsync();
            return Ok(cab);
        }
        #endregion

        #region CRUD Subject
        [HttpGet("subject")]
        public async Task<ActionResult<IEnumerable<Subject>>> SubjectGet()
        {
            return await db.Subjects.Include(g => g.Institutes).ToListAsync();
        }

        [HttpDelete("subject/delete/{id}")]
        public async Task<ActionResult<IEnumerable<Subject>>> SubjectDelete(int id)
        {
            Subject? sub = db.Subjects.FirstOrDefault(x => x.Id == id);
            if (sub == null)
            {
                return NotFound();
            }
            db.Subjects.Remove(sub);
            await db.SaveChangesAsync();
            return Ok(sub);
        }

        [HttpPost("subject/save")]
        public async Task<ActionResult<IEnumerable<Subject>>> SubjectSave(Subject sub)
        {
            if (sub == null)
            {
                return BadRequest();
            }

            var temp = new Subject() { Id = sub.Id, Name = sub.Name, InstitutesId = sub.InstitutesId };
            // https://qna.habr.com/q/1344730 
            db.Subjects.Add(temp);
            await db.SaveChangesAsync();
            sub.Id = temp.Id;
            return Ok(sub);
        }

        [HttpPut("subject/edit")]
        public async Task<ActionResult<IEnumerable<Subject>>> SubjectEdit(Subject sub)
        {
            if (sub == null)
            {
                return BadRequest();
            }
            if (!db.Subjects.Any(x => x.Id == sub.Id))
            {
                return NotFound();
            }
            var temp = new Subject() { Id = sub.Id, Name = sub.Name, InstitutesId = sub.InstitutesId };
            db.Update(temp);
            await db.SaveChangesAsync();
            return Ok(sub);
        }
        #endregion

        #region CRUD GroupOfStudent

        [HttpGet("groupsOfStudent")]
        public async Task<ActionResult<IEnumerable<GroupOfStudent>>> GroupOfStudentGet()
        {
            return await db.GroupOfStudents.Include(g => g.Institutes).ToListAsync();
        }

        [HttpDelete("groupsOfStudent/delete/{id}")]
        public async Task<ActionResult<IEnumerable<GroupOfStudent>>> GroupOfStudentDelete(int id)
        {
            GroupOfStudent? gos = db.GroupOfStudents.FirstOrDefault(x => x.Id == id);
            if (gos == null)
            {
                return NotFound();
            }
            db.GroupOfStudents.Remove(gos);
            await db.SaveChangesAsync();
            return Ok(gos);
        }

        [HttpPost("groupsOfStudent/save")]
        public async Task<ActionResult<IEnumerable<GroupOfStudent>>> GroupOfStudentSave(GroupOfStudent gos)
        {
            if (gos == null)
            {
                return BadRequest();
            }

            var temp = new GroupOfStudent() { Id = gos.Id, Title = gos.Title, InstitutesId = gos.InstitutesId };
            // https://qna.habr.com/q/1344730 
            db.GroupOfStudents.Add(temp);
            await db.SaveChangesAsync();
            gos.Id = temp.Id;
            return Ok(gos);
        }
            
        [HttpPut("groupsOfStudent/edit")]
        public async Task<ActionResult<IEnumerable<GroupOfStudent>>> GroupOfStudentEdit(GroupOfStudent gos)
        {
            if (gos == null)
            {
                return BadRequest();
            }
            if(!db.GroupOfStudents.Any(x => x.Id == gos.Id))
            {
                return NotFound();
            }
            var temp = new GroupOfStudent() { Id = gos.Id, Title = gos.Title, InstitutesId = gos.InstitutesId };
            db.Update(temp);
            await db.SaveChangesAsync();
            return Ok(gos);
        }
        #endregion

        #region CRUD Institute
        [HttpGet("institutes")]
        public async Task<ActionResult<IEnumerable<Institute>>> InstituteGet()
        {
            return await db.Institutes.ToListAsync();
        }

        [HttpDelete("institutes/delete/{id}")]
        public async Task<ActionResult<IEnumerable<Institute>>> InstituteDelete(int id)
        {
            Institute? inst = db.Institutes.FirstOrDefault(x => x.Id == id);
            if (inst == null)
            {
                return NotFound();
            }
            db.Institutes.Remove(inst);
            await db.SaveChangesAsync();
            return Ok(inst);
        }

        [HttpPost("institutes/save")]
        public async Task<ActionResult<IEnumerable<Institute>>> InstituteSave(Institute inst)
        {
            if (inst == null)
            {
                return BadRequest();
            }
            db.Institutes.Add(inst);
            await db.SaveChangesAsync();
            return Ok(inst);
        }

        [HttpPut("institutes/edit")]
        public async Task<ActionResult<IEnumerable<Institute>>> InstituteEdit(Institute inst)
        {
            if (inst == null)
            {
                return BadRequest();
            }
            if (!db.Institutes.Any(x => x.Id == inst.Id))
            {
                return NotFound();
            }
            db.Update(inst);
            await db.SaveChangesAsync();
            return Ok(inst);
        }
        #endregion
    }
}
