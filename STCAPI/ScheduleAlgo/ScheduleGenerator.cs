using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using STCAPI.InteractionExcel;
using STCAPI.Models;
using System.Text.Json;

namespace STCAPI.ScheduleAlgo
{
    public class ScheduleGenerator
    {
        StcdbContext db = new StcdbContext();
        Dictionary<string, bool> BusyAud = new Dictionary<string, bool>(); // [Auditory + " " + Date]bool
        Dictionary<string, bool> BusyDay = new Dictionary<string, bool>(); // [Group + " " + Date]bool - Для проверок занятости дней у группы.
        Dictionary<string, bool> BusyLector = new Dictionary<string, bool>(); //[Name + " " + Date]bool

        // Проверка аудиторий из доп. условий, либо если нет доп. условия назначить свободную аудиторию из ауд. фонда. 
        private bool FindFreeAuditoria(STElement el, DateTime date, List<string> auditoryFund)
        {
            // Если нет доп. условия - взять из предложенного аудиторного фонда и выбрать первую свободную.
            List<string> auditorias = new List<string>();
            if (el.WishAuditoria == "")
            {
                auditorias = auditoryFund;
            }
            else
            {
                ConditionHandler ch = new ConditionHandler();
                auditorias = ch.ExtractAuditories(el.WishAuditoria);
            }

            foreach (var auditoria in auditorias)
            {
                if (!BusyAud.ContainsKey(auditoria + " " + date.ToString("dd.MM.yyyy")))
                {
                    el.Auditoria = auditoria;
                    return false;
                }
            }
            return true;
        }
        
        // Функция проверяет диапазон двух дней от текущей даты на наличие экзамена.
        private bool CheckDayRange(STElement el, DateTime date)
        {
            var OneDayUp = BusyDay.ContainsKey(el.Group + " " + (date.AddDays(1)).ToString("dd.MM.yyyy"));
            var TwoDayUp = BusyDay.ContainsKey(el.Group + " " + (date.AddDays(2)).ToString("dd.MM.yyyy"));
            var OneDayDown = BusyDay.ContainsKey(el.Group + " " + (date.AddDays(-1)).ToString("dd.MM.yyyy"));
            var TwoDayDown = BusyDay.ContainsKey(el.Group + " " + (date.AddDays(-2)).ToString("dd.MM.yyyy"));
            return OneDayUp || OneDayDown || TwoDayUp || TwoDayDown;
        } 

        public (List<STElement>, int) Generate(string stringDateStart, string stringDateEnd, string cabinets, List<STElement> ExamList, int userID)
        {
            AuditoryFundHandler audFundHandler = new AuditoryFundHandler();
            var AuditoryFund = (new ConditionHandler()).ExtractAuditories(cabinets);
            ReservedInformation reservedInfo = audFundHandler.Reserve(stringDateStart, stringDateEnd, AuditoryFund, userID);
            BusyAud = audFundHandler.CheckBusyAuditories(stringDateStart,stringDateEnd);

            var StartDate = DateTime.Parse(stringDateStart);
            var EndDate = DateTime.Parse(stringDateEnd);


            var Result = new List<STElement>();

            var sorterExamsList = ExamList
                .GroupBy(e => e.Group) //Групируем по группам
                .OrderByDescending(g => g.Count()) // Сортируем по количеству в убывающей
                .SelectMany(g => g.OrderBy(e => e.Name)) // Распаковываем нашу группировку
                .ToList(); //Превращаем в лист

            var groupedSubject = sorterExamsList
                .GroupBy(e => e.Group)
                .Select(g => g.OrderBy(x => Guid.NewGuid().GetHashCode()));

            // Строки с доп условиями
            foreach (var group in groupedSubject)
            {
                foreach (var element in group)
                {
                    if (element.WishDate == "") continue;

                    ConditionHandler ch = new ConditionHandler();
                    var dates = ch.ExtractDates(element.WishDate);

                    foreach (var date in dates)
                    {
                        string[] d = date.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                        var StartDateCond = DateTime.Parse(d[0].Trim());
                        var EndDateCond = DateTime.Parse(d[1].Trim());

                        //Консультация
                        if (StartDateCond.DayOfWeek == DayOfWeek.Sunday)
                        {
                            StartDateCond = StartDateCond.AddDays(1);
                        }
                        var consultationDay = StartDateCond;
                        StartDateCond = StartDateCond.AddDays(1);
                        if (StartDateCond.DayOfWeek == DayOfWeek.Sunday)
                        {
                            StartDateCond = StartDateCond.AddDays(1);
                        }

                        // Экзамен
                        // Следующий код нужен чтобы найти свободный день для экзамена для группы и лектора. True - занято False - свободно.
                        // Условие в цикле: Работаем до тех пор пока заняты группа ИЛИ лектор ИЛИ аудитория ИЛИ в "окне" есть экзамен И начало даты < конца даты
                        while (((BusyDay.ContainsKey(element.Group + " " + StartDateCond.ToString("dd.MM.yyyy")) 
                            || BusyLector.ContainsKey(element.Lector + " " + StartDateCond.ToString("dd.MM.yyyy"))) 
                            || FindFreeAuditoria(element, StartDateCond, AuditoryFund) 
                            || CheckDayRange(element, StartDateCond)) && StartDateCond < EndDateCond)
                        {
                            consultationDay = StartDateCond;
                            StartDateCond = StartDateCond.AddDays(1);
                            if (StartDateCond.DayOfWeek == DayOfWeek.Sunday)
                            {
                                StartDateCond = StartDateCond.AddDays(1);
                            }

                        }
                        var examDay = StartDateCond;


                        //Завершающий этап
                        if (consultationDay <= EndDateCond && examDay <= EndDateCond)
                        {
                            element.HasDate = true;
                            element.Consultation = consultationDay;
                            element.Exam = examDay;
                        }
                        else
                        {
                            element.Failed = true;
                        }

                        if (!BusyDay.ContainsKey(element.Group + " " + element.Exam.ToString("dd.MM.yyyy")) 
                            && element.HasDate 
                            && !BusyLector.ContainsKey(element.Lector + " " + element.Exam.ToString("dd.MM.yyyy")) 
                            && !BusyAud.ContainsKey(element.Auditoria + " " + element.Exam.ToString("dd.MM.yyyy")))
                        {
                            BusyDay[element.Group + " " + element.Exam.ToString("dd.MM.yyyy")] = true;
                            BusyLector[element.Lector + " " + element.Exam.ToString("dd.MM.yyyy")] = true;
                            BusyAud[element.Auditoria + " " + element.Exam.ToString("dd.MM.yyyy")] = true;
                            break;
                        }
                    }
                }
            }

            // Составление шаблона для строк без доп. условий.
            foreach (var group in groupedSubject)
            {
                DateTime timeForGroup = StartDate; // Старт дата для каждой группы
                foreach (var element in group)
                {
                    if (element.HasDate || element.Failed == true)
                    {
                        Result.Add(element);
                        continue;
                    }

                    //Консультация
                    if (timeForGroup.DayOfWeek == DayOfWeek.Sunday)
                    {
                        timeForGroup = timeForGroup.AddDays(1);
                    }
                    var consultationDay = timeForGroup;
                    timeForGroup = timeForGroup.AddDays(1);
                    if (timeForGroup.DayOfWeek == DayOfWeek.Sunday)
                    {
                        timeForGroup = timeForGroup.AddDays(1);
                    }

                    // Экзамен
                    while (((BusyDay.ContainsKey(element.Group + " " + timeForGroup.ToString("dd.MM.yyyy"))
                            || BusyLector.ContainsKey(element.Lector + " " + timeForGroup.ToString("dd.MM.yyyy")))
                            || FindFreeAuditoria(element, timeForGroup, AuditoryFund)
                            || CheckDayRange(element, timeForGroup)) && timeForGroup < EndDate)
                    {
                        consultationDay = timeForGroup;
                        timeForGroup = timeForGroup.AddDays(1);
                        if (timeForGroup.DayOfWeek == DayOfWeek.Sunday)
                        {
                            timeForGroup = timeForGroup.AddDays(1);
                        }

                    }
                    var examDay = timeForGroup;
                    timeForGroup = timeForGroup.AddDays(2);

                    if (consultationDay <= EndDate && examDay <= EndDate)
                    {
                        element.HasDate = true;
                        element.Consultation = consultationDay;
                        element.Exam = examDay;
                    }
                    else
                    {
                        element.Failed = true;
                    }

                    // Если преподаватель не занят и раставлены даты и аудитория свободна, то занимаем всё это в эту дату.
                    if (!BusyLector.ContainsKey(element.Lector + " " + element.Exam.ToString("dd.MM.yyyy")) && element.HasDate && !BusyDay.ContainsKey(element.Group + " " + element.Exam.ToString("dd.MM.yyyy")))
                    {
                        BusyLector[element.Lector + " " + element.Exam.ToString("dd.MM.yyyy")] = true;
                        BusyDay[element.Group + " " + element.Exam.ToString("dd.MM.yyyy")] = true;
                        BusyAud[element.Auditoria + " " + element.Exam.ToString("dd.MM.yyyy")] = true;
                    }

                    Result.Add(element);
                }
            }
            return  (Result, reservedInfo.AuditoryReserve_id);
        }
    }
}


//Условия бывают такие:
/*
 1 
Период:  с 9.01.25 по 15.01.25
Несколько периодов:  с 13.01.25 по 18.01.25 или с 20.01.25 по 25.01.25
 2 
Аудитории: 1212 ауд.
 3 
 - Учитывание дней неделей: понедельник, среда, пятница / НЕ ставить в понедельник -> Sunday, 
Этап 4: проверка конфликтов и корректировка
var CommonCabs = new List<string>(){"0203", "0206", "0212", "0215", "0227", "0228", "0229", "0231", "0235", "0316", "0321", "0320", "0325", "0317", "0414", "0415", "0417","0419", "0416", "0418"};
 */