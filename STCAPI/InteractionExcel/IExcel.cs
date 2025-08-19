using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OfficeOpenXml;
using STCAPI.Models;
using STCAPI.ScheduleAlgo;
using System.Globalization;


namespace STCAPI.InteractionExcel
{
    public class IExcel
    {
        StcdbContext db = new StcdbContext();

        public async void SaveUnknownData(List<STElement> examList, int userID)
        {
            var user = db.Users.FirstOrDefault(u => u.Id == userID);
            foreach (var exam in examList)
            {
                Subject subj = new Subject();
                subj.Name = exam.Name.Trim();
                subj.InstitutesId = user.InstitutesId;
                var subjExist = await db.Subjects.AnyAsync(s => s.Name == subj.Name && s.InstitutesId == subj.InstitutesId);
                if (!subjExist)
                {
                    db.Subjects.Add(subj);
                    await db.SaveChangesAsync();
                }

                GroupOfStudent gos = new GroupOfStudent();
                gos.Title = exam.Group.Trim();
                gos.InstitutesId = user.InstitutesId;
                var gosExist = await db.GroupOfStudents.AnyAsync(g => g.Title == gos.Title && gos.InstitutesId == g.InstitutesId);
                if (!gosExist) 
                {
                    db.GroupOfStudents.Add(gos);
                    await db.SaveChangesAsync();
                }

                Lecturer lect = new Lecturer();
                lect.Shortname = exam.Lector.Trim();
                lect.InstitutesId = user.InstitutesId;
                var lectExits = await db.Lecturers.AnyAsync(l => l.Shortname == lect.Shortname && l.InstitutesId == lect.InstitutesId);
                if (!lectExits) 
                {
                    db.Lecturers.Add(lect);
                    await db.SaveChangesAsync();
                }
            }
        }
        // Выгрузка из xlsx
        public (List<STElement>, List<string>) ImportDataExcel(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage package = new ExcelPackage(fileInfo);
            ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();

            var errors = new List<string>();
            var ExamList = new List<STElement>();
            int rows = worksheet.Dimension.Rows;
            for (int i = 1; i <= rows; i++)
            {
                string nameSubject = worksheet.Cells[i, 1].Value.ToString();
                string course = worksheet.Cells[i, 2].Value.ToString();
                string group = worksheet.Cells[i, 3].Value.ToString();
                string lector = worksheet.Cells[i, 4].Value.ToString();
                string? wishDate = "";
                if (worksheet.Cells[i, 5].Value != null) wishDate = worksheet.Cells[i, 5].Value.ToString();
                string? wishAuditoria = "";
                if (worksheet.Cells[i, 6].Value != null) wishAuditoria = worksheet.Cells[i, 6].Value.ToString();
                var ExamElement = new STElement();
                ExamElement.Name = nameSubject;
                ExamElement.Course = course;
                ExamElement.Group = group;
                ExamElement.Lector = lector;
                ExamElement.WishDate = wishDate;
                ExamElement.WishAuditoria = wishAuditoria;

                if (nameSubject == null)
                {
                    errors.Add($"{i} неверный формат названия дисциплины");
                }else if (group == null)
                {
                    errors.Add($"{i} неверный формат группы студентов");
                }else if (lector == null)
                {
                    errors.Add($"{i} неверный формат ввода преподавателя");
                }else if (IsValidDates(wishDate))
                {
                    errors.Add($"{i} неверный формат предпочтительных дат");
                }

                ExamList.Add(ExamElement);
            }

            return (ExamList, errors);
        }

        private bool IsValidDates(string wishDate)
        {
            //DateTime temp1;
            //DateTime temp2;
            var dates = (new ConditionHandler()).ExtractDates(wishDate);
            foreach (var date in dates) 
            {
                if (!date.Contains(" - "))
                {
                    return true;
                }

                var parts = date.Split(new[] { " - " }, StringSplitOptions.None);

                if (parts.Length != 2)
                {
                    return true;
                }

                //string format = "dd.MM.yyyy";
                //var provider = CultureInfo.InvariantCulture;

                //if (!DateTime.TryParseExact(parts[0], format, provider, DateTimeStyles.None, out temp1))
                //{
                //    return true;
                //}

                //if (!DateTime.TryParseExact(parts[1], format, provider, DateTimeStyles.None, out temp2))
                //{
                //    return true;
                //}
            }
            return false;
        }

        // Загрузка в xlsx
        public string ExportDataExcel(List<STElement> data, string FileName)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Расписание");

                worksheet.Cells["A1"].Value = "Предмет";
                worksheet.Cells["B1"].Value = "Курс";
                worksheet.Cells["C1"].Value = "Группа";
                worksheet.Cells["D1"].Value = "Преподаватель";
                worksheet.Cells["E1"].Value = "Консультация";
                worksheet.Cells["F1"].Value = "Экзамен";
                worksheet.Cells["G1"].Value = "Аудитория";
                worksheet.Cells["H1"].Value = "Рекоменд. даты";
                worksheet.Cells["I1"].Value = "Рекоменд. аудитории";

                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["B1"].Style.Font.Bold = true;
                worksheet.Cells["C1"].Style.Font.Bold = true;
                worksheet.Cells["D1"].Style.Font.Bold = true;
                worksheet.Cells["E1"].Style.Font.Bold = true;
                worksheet.Cells["F1"].Style.Font.Bold = true;
                worksheet.Cells["G1"].Style.Font.Bold = true;
                worksheet.Cells["H1"].Style.Font.Bold = true;
                worksheet.Cells["I1"].Style.Font.Bold = true;

                for (int i = 0; i < data.Count; i++)
                {
                    worksheet.Cells[$"A{2 + i}"].Value = data[i].Name;
                    worksheet.Cells[$"B{2 + i}"].Value = data[i].Course;
                    worksheet.Cells[$"C{2 + i}"].Value = data[i].Group;
                    worksheet.Cells[$"D{2 + i}"].Value = data[i].Lector;
                    worksheet.Cells[$"E{2 + i}"].Value = data[i].Consultation.ToString("dd.MM.yyyy");
                    worksheet.Cells[$"F{2 + i}"].Value = data[i].Exam.ToString("dd.MM.yyyy");
                    worksheet.Cells[$"G{2 + i}"].Value = data[i].Auditoria;
                    worksheet.Cells[$"H{2 + i}"].Value = data[i].WishDate;
                    worksheet.Cells[$"I{2 + i}"].Value = data[i].WishAuditoria;
                }
                worksheet.Cells.AutoFitColumns();

                var FolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AlgoResult");
                if (!Directory.Exists(FolderPath))
                    Directory.CreateDirectory(FolderPath);

                string filePath = Path.Combine(FolderPath, FileName);
                package.SaveAs(new FileInfo(filePath));

                return filePath;
            }

        }
    }
}