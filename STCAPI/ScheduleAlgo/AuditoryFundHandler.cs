using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.OpenApi.Models;
using STCAPI.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace STCAPI.ScheduleAlgo
{
    public class AuditoryFundHandler
    {
        StcdbContext db = new StcdbContext();
        // Функция проверяет входящий список аудиторий на то, какие аудитории резервированы уже, 
        // а после резервует оставшиеся.
        public ReservedInformation Reserve(string stringStartDate, string stringEndDate, List<string> auditoryFund, int userID)
        {
            DateTime StartDate = DateTime.Parse(stringStartDate);
            DateTime EndDate = DateTime.Parse(stringEndDate);

            // Ищем все аудитории, которые резервированы в ближайших диапазонах.
            var reservedAud = db.AuditoryReserves.ToList();
            List<BusyAuditory> busyAuditories = new List<BusyAuditory>();
            foreach (var rA in reservedAud) 
            {
                var CurSD = DateTime.Parse(rA.StartDate);
                var CurED = DateTime.Parse(rA.EndDate);
                if ((CurSD <= StartDate || StartDate <= CurED) 
                    || (CurSD <= EndDate || EndDate <= CurED) 
                    || (StartDate <= CurSD && CurED <= EndDate) 
                    || (CurSD <= StartDate && EndDate <= CurED))
                {
                    
                    busyAuditories.AddRange(db.BusyAuditories.Where(ba => ba.AuditoryReserveId == rA.Id && ba.BusyDate == null).ToList());
                }
            }

            // Проверка входного списка аудиторий на то, какие аудитории резервированы.(Если резервированы их нельзя использовать в создании шаблона)
            var busyAudCabinets = new HashSet<string>(busyAuditories.Select(b => b.Auditory));

            auditoryFund.RemoveAll(aud => busyAudCabinets.Contains(aud));

            var temp = new AuditoryReserve { StartDate = StartDate.ToString("dd.MM.yyyy"), EndDate = EndDate.ToString("dd.MM.yyyy"), UsersId = userID };
            db.AuditoryReserves.Add(temp);
            db.SaveChangesAsync();
            foreach(var af in auditoryFund)
            {
                db.BusyAuditories.Add(new BusyAuditory {AuditoryReserveId = temp.Id, Auditory = af});
                db.SaveChangesAsync();
            }

            return new ReservedInformation(temp.Id, auditoryFund);
        }

        // Подгрузка занятых аудиторий в ближайших диапазонах.
        public Dictionary<string, bool> CheckBusyAuditories(string stringStartDate, string stringEndDate)
        {
            DateTime StartDate = DateTime.Parse(stringStartDate);
            DateTime EndDate = DateTime.Parse(stringEndDate);

            // Ищем все аудитории, которые заняты в ближайших диапазонах.
            var reservedAud = db.AuditoryReserves.ToList();
            List<BusyAuditory> busyAuditories = new List<BusyAuditory>();
            foreach (var rA in reservedAud)
            {
                var CurSD = DateTime.Parse(rA.StartDate);
                var CurED = DateTime.Parse(rA.EndDate);
                if ((CurSD <= StartDate || StartDate <= CurED)
                    || (CurSD <= EndDate || EndDate <= CurED)
                    || (StartDate <= CurSD && CurED <= EndDate)
                    || (CurSD <= StartDate && EndDate <= CurED))
                {
                    busyAuditories.AddRange(db.BusyAuditories.Where(ba => ba.AuditoryReserveId == rA.Id && ba.BusyDate != null).ToList());
                }
            }

            Dictionary<string, bool> BusyAud = new Dictionary<string, bool>();
            foreach (var b in busyAuditories)
            {
                BusyAud[b.Auditory + " " + b.BusyDate] = true;
            }

            return BusyAud;
        }

        // Подтверждение о сохранении шаблона - загружаем занятость аудиторного фонда
        public int? ConfirmationFund(ScheduleInformation SchInfo, bool ClientAnswer)
        {
            var reserve = db.AuditoryReserves
                .Where( ar => ar.Id == SchInfo.AuditoryReserveId )
                .FirstOrDefault();
            var busyAuditories = db.BusyAuditories.Where(ba => ba.AuditoryReserveId == SchInfo.AuditoryReserveId && ba.BusyDate == null).ToList();
            for (int i = 0;i < busyAuditories.Count; i++)
            {
                db.BusyAuditories.Remove(busyAuditories[i]);
                db.SaveChanges();
            }

            if (ClientAnswer == true)
            {
                string json = SchInfo.ScTemplate.JsonTemplate;
                List<STElement>? examList = JsonSerializer.Deserialize<List<STElement>>(json);
                foreach (var exam in examList)
                {
                    var help = new BusyAuditory() {AuditoryReserveId = SchInfo.AuditoryReserveId, Auditory = exam.Auditoria, BusyDate  = exam.Exam.ToString("dd.MM.yyyy"), Subject = exam.Name};
                    db.BusyAuditories.Add(help);
                    db.SaveChanges();
                }
                ScheduleTemplate scheduleTemplate = new ScheduleTemplate();
                scheduleTemplate.Name = SchInfo.ScTemplate.Name;
                scheduleTemplate.JsonTemplate = json;

                scheduleTemplate.OwnerUserId = SchInfo.ScTemplate.OwnerUserId;
                scheduleTemplate.InstitutesId = SchInfo.ScTemplate.InstitutesId;

                db.ScheduleTemplates.Add(scheduleTemplate);
                db.SaveChanges();
                reserve.ScheduleTemplatesId = scheduleTemplate.Id;
                db.AuditoryReserves.Update(reserve);
                db.SaveChanges();
                return reserve.ScheduleTemplatesId;
            }
            else
            {
                var deleteBusyAud = db.BusyAuditories.Where(ba => ba.AuditoryReserveId == reserve.Id).ToList();
                foreach (var aud in deleteBusyAud) 
                {
                    db.BusyAuditories.Remove(aud);
                    db.SaveChanges();
                }
                db.AuditoryReserves.Remove(reserve);
                db.SaveChanges();
                return 0;
            }
        }

        public void UpdateFund(ScheduleInformation SchInfo)
        {
            var reserve = db.AuditoryReserves
                .Where(ar => ar.Id == SchInfo.AuditoryReserveId)
                .FirstOrDefault();

            var busyAuditories = db.BusyAuditories.Where(ba => ba.AuditoryReserveId == SchInfo.AuditoryReserveId && ba.BusyDate != null).ToList();
            for (int i = 0; i < busyAuditories.Count; i++)
            {
                db.BusyAuditories.Remove(busyAuditories[i]);
                db.SaveChanges();
            }
            string json = SchInfo.ScTemplate.JsonTemplate;
            List<STElement>? examList = JsonSerializer.Deserialize<List<STElement>>(json);
            foreach (var exam in examList)
            {
                var help = new BusyAuditory() { AuditoryReserveId = SchInfo.AuditoryReserveId, Auditory = exam.Auditoria, BusyDate = exam.Exam.ToString("dd.MM.yyyy"), Subject = exam.Name };
                db.BusyAuditories.Add(help);
                db.SaveChanges();
            }
            ScheduleTemplate scheduleTemplate = SchInfo.ScTemplate;
            int rowsAffected = db.Database.ExecuteSqlRaw("UPDATE schedule_templates SET json_template = {0} WHERE id = {1}", scheduleTemplate.JsonTemplate, scheduleTemplate.Id);
        }

    }
}
