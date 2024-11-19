using AttivitaWpf.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using Model;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using MasterDetailModel;
using DevExpress.XtraSpreadsheet.Model;
using PH.WorkingDaysAndTimeUtility.Configuration;
using Syncfusion.Windows.Controls;
using Syncfusion.ProjIO;
using Commons;
using Syncfusion.DocIO;
using DevExpress.Xpf.Charts;
using DevExpress.Xpf.Gantt;
using GanttView = AttivitaWpf.View.GanttView;
using System.Speech.Synthesis;
using System.Globalization;

namespace AttivitaWpf.ImportExport
{
    internal class MSProjectIO
    {
        private MSProject _msProject = null;

        private GanttView _ganttView = null;

        private IDataService _dataService = null;

        EntitiesHelper _entsHelper = null;

        private string DurationFormat { get => "PT{0}H{1}M0S"; }

        private Dictionary<string, int> FieldsIDByCodiceAttributo = new Dictionary<string, int>();

        public void Init(GanttView ganttView)
        {
            _ganttView = ganttView;
            _msProject = new MSProject();
            _dataService = ganttView.DataService;

            _entsHelper = new EntitiesHelper(_dataService);

            FieldsIDByCodiceAttributo.Add(BuiltInCodes.Attributo.QuantitaTotale, 188743767);
            FieldsIDByCodiceAttributo.Add(BuiltInCodes.Attributo.QuantitaEseguita, 188743768);
            FieldsIDByCodiceAttributo.Add(BuiltInCodes.Attributo.Importo, 188743786);
            FieldsIDByCodiceAttributo.Add(BuiltInCodes.Attributo.ImportoEseguito, 188743787);
            FieldsIDByCodiceAttributo.Add(BuiltInCodes.Attributo.ImportoManodopera, 188743788);

        }

        public void RunExport()
        {
            List<TreeEntity> TreeEntities = null;

            TreeEntities = _ganttView.RetrieveWBSData();

            


            DateTime minInizioItems = _ganttView.WBSView.GetMinDataInizioWBSItems();
            DateTime maxFineItems = _ganttView.WBSView.GetMaxDataFineWBSItems();
            
            
            //Add Extended Attribute (importi e quantità)
            AddExtendedAttributesDefinition();
            
            //init calendars
            _msProject.Calendars = new Calendars();
            _msProject.Calendars.Calendar = new List<Calendar>();
            Dictionary<Guid, int> calendarsMap = new Dictionary<Guid, int>();
            int calendarUID = 0;
            
            
            //init tasks
            _msProject.Tasks = new Tasks();
            _msProject.Tasks.Task = new List<Task>();
            Dictionary<Guid, int> tasksMap = new Dictionary<Guid, int>();
            _msProject.StartDate = _ganttView.GetDataInizioGantt();
            _msProject.FinishDate = maxFineItems;



            //Add task root
            Task taskRoot = new Task();
            taskRoot.UID = 0;
            taskRoot.GUID = Guid.NewGuid().ToString();
            taskRoot.ID = 0;
            taskRoot.Name = "Project1";
            taskRoot.OutlineLevel = 0;
            taskRoot.Start = RoundDateTime(minInizioItems);
            taskRoot.Finish = RoundDateTime(maxFineItems);
            taskRoot.ConstraintDate = taskRoot.Start;



            _msProject.Tasks.Task.Add(taskRoot);

            int i = 1;
            foreach (TreeEntity treeEntity in TreeEntities)
            {
                Task task = new Task();
                task.UID = i;
                task.GUID = Guid.NewGuid().ToString();
                task.ID = i;

                WBSItem entity = (WBSItem)treeEntity;

                tasksMap.Add(entity.EntityId, task.UID);


                Guid calendarioId = entity.GetCalendarioId();

                DateTimeCalculator timeCalc = null;

                ValoreTesto nome = (ValoreTesto)_entsHelper.GetValoreAttributo(entity, BuiltInCodes.Attributo.Nome, false, false);
                task.Name = nome?.ToPlainText();

                ValoreTesto code = (ValoreTesto)_entsHelper.GetValoreAttributo(entity, BuiltInCodes.Attributo.Codice, false, false);
                task.WBS = code?.ToPlainText();

                task.Start = RoundDateTime(entity.GetDataInizio());
                task.Finish = RoundDateTime(entity.GetDataFine());

                task.ConstraintDate = task.Start;

                task.OutlineLevel = entity.Depth + 1;

                if (entity.IsParent)
                {
                    task.Duration = string.Empty;
                    task.CalendarUID = -1;
                }
                else
                {    
                    //ricavo il calcolatore in base al calendario della voce 
                    CalendariItem calendario = _dataService.GetEntitiesById(BuiltInCodes.EntityType.Calendari, new List<Guid> { calendarioId }).FirstOrDefault() as CalendariItem;
                    if (calendario != null)
                        timeCalc = new DateTimeCalculator(calendario.GetWeekHours(), calendario.GetCustomDays());
                    else
                        timeCalc = new DateTimeCalculator(WeekHours.Default, new CustomDays());


                    //duration
                    double workingMinutes = timeCalc.GetWorkingMinutesBetween(task.Start, task.Finish);
                    int hours = (int)workingMinutes / 60;
                    int minutes = (int)(workingMinutes - (hours * 60));
                    //var timeSpan = new TimeSpan(hours, minutes, 0);
                    task.Duration = string.Format(DurationFormat, hours, minutes);

                    //calendario
                    if (!calendarsMap.ContainsKey(calendarioId))
                    {
                        calendarUID++;
                        calendarsMap.Add(calendarioId, calendarUID);
                        AddCalendar(calendarUID, calendarioId);
                    }
                    task.CalendarUID = calendarsMap[calendarioId];

                    //percent complete
                    double? taskProgress = entity.GetTaskProgress();
                    if (taskProgress.HasValue)
                    {
                        double workingMinutesComplete = taskProgress.Value * workingMinutes;
                        int hoursComplete = (int)workingMinutesComplete / 60;
                        int minutesComplete = (int)(workingMinutesComplete - (hoursComplete * 60));
                        task.ActualDuration = string.Format(DurationFormat, hoursComplete, minutesComplete);

                    }

                    //Nota
                    ValoreTesto valNota = (ValoreTesto)_entsHelper.GetValoreAttributo(entity, BuiltInCodes.Attributo.TaskNote, false, false);
                    string nota = valNota?.ToPlainText();
                    if (nota != null && nota.Trim().Any())
                        task.Notes = nota;
                    else
                        task.Notes = null;


                    //Extended attributes
                    task.ExtendedAttribute = new List<ExtendedAttribute>();

                    AddExtendedAttribute(entity, BuiltInCodes.Attributo.QuantitaTotale, task);

                    AddExtendedAttribute(entity, BuiltInCodes.Attributo.QuantitaEseguita, task);
                    
                    AddExtendedAttribute(entity, BuiltInCodes.Attributo.Importo, task);

                    AddExtendedAttribute(entity, BuiltInCodes.Attributo.ImportoEseguito, task);

                    AddExtendedAttribute(entity, BuiltInCodes.Attributo.ImportoManodopera, task);
                }

                _msProject.Tasks.Task.Add(task);
                i++;
            }

            AddPredecessors(tasksMap);



        }


        private void AddExtendedAttribute(WBSItem entity, string codiceAttributo, Task task)
        {
            //Importo
            Valore val = _entsHelper.GetValoreAttributo(entity, codiceAttributo, false, false);

            //Oss: Si suppone che il numero venga scritto senza il separatore dei decimali (che sono fissi a 2)

            int result = 0;

            if (val is ValoreReale)
            {
                ValoreReale reale = val as ValoreReale;
                if (reale.RealResult.HasValue)
                    result = (int) (reale.RealResult.Value * 100);

            }
            else if (val is ValoreContabilita)
            {
                ValoreContabilita cont = val as ValoreContabilita;
                if (cont.RealResult.HasValue)
                    result = (int) (cont.RealResult.Value * 100);
            }

            

            task.ExtendedAttribute.Add(new ExtendedAttribute()
            {
                FieldID = FieldsIDByCodiceAttributo[codiceAttributo],
                Value = result.ToString(CultureInfo.InvariantCulture),
            });
        }

        private void AddExtendedAttributesDefinition()
        {
            EntityType entType = _dataService.GetEntityType(BuiltInCodes.EntityType.WBS);
            Attributo att = null;



            _msProject.ExtendedAttributes = new ExtendedAttributes();
            _msProject.ExtendedAttributes.ExtendedAttribute = new List<ExtendedAttribute>();

            if (entType.Attributi.TryGetValue(BuiltInCodes.Attributo.QuantitaTotale, out att))
            {
                _msProject.ExtendedAttributes.ExtendedAttribute.Add(new ExtendedAttribute()
                {
                    FieldID = FieldsIDByCodiceAttributo[BuiltInCodes.Attributo.QuantitaTotale],
                    FieldName = "Numero1",
                    Alias = att.Etichetta,
                    Guid = "000039B7-8BBE-4CEB-82C4-FA8C0B400057",
                    SecondaryPID = "255868988",
                    SecondaryGuid = "000039B7-8BBE-4CEB-82C4-FA8C0F40403C",
                });
            }

            if (entType.Attributi.TryGetValue(BuiltInCodes.Attributo.QuantitaEseguita, out att))
            {
                _msProject.ExtendedAttributes.ExtendedAttribute.Add(new ExtendedAttribute()
                {
                    FieldID = FieldsIDByCodiceAttributo[BuiltInCodes.Attributo.QuantitaEseguita],
                    FieldName = "Numero2",
                    Alias = att.Etichetta,
                    Guid = "000039B7-8BBE-4CEB-82C4-FA8C0B400058",
                    SecondaryPID = "255868989",
                    SecondaryGuid = "000039B7-8BBE-4CEB-82C4-FA8C0F40403D",
                });
            }

            if (entType.Attributi.TryGetValue(BuiltInCodes.Attributo.Importo, out att))
            {
                _msProject.ExtendedAttributes.ExtendedAttribute.Add(new ExtendedAttribute()
                {
                    FieldID = FieldsIDByCodiceAttributo[BuiltInCodes.Attributo.Importo],
                    FieldName = "Costo1",
                    Alias = att.Etichetta,
                    Guid = "000039B7-8BBE-4CEB-82C4-FA8C0B40006A",
                    SecondaryPID = "255868958",
                    SecondaryGuid = "000039B7-8BBE-4CEB-82C4-FA8C0F40401E",
                });
            }

            if (entType.Attributi.TryGetValue(BuiltInCodes.Attributo.ImportoEseguito, out att))
            {
                _msProject.ExtendedAttributes.ExtendedAttribute.Add(new ExtendedAttribute()
                {
                    FieldID = FieldsIDByCodiceAttributo[BuiltInCodes.Attributo.ImportoEseguito],
                    FieldName = "Costo2",
                    Alias = att.Etichetta,
                    Guid = "000039B7-8BBE-4CEB-82C4-FA8C0B40006B",
                    SecondaryPID = "255868959",
                    SecondaryGuid = "000039B7-8BBE-4CEB-82C4-FA8C0F40401F",
                });
            }

            if (entType.Attributi.TryGetValue(BuiltInCodes.Attributo.ImportoManodopera, out att))
            {
                _msProject.ExtendedAttributes.ExtendedAttribute.Add(new ExtendedAttribute()
                {
                    FieldID = FieldsIDByCodiceAttributo[BuiltInCodes.Attributo.ImportoManodopera],
                    FieldName = "Costo3",
                    Alias = att.Etichetta,
                    Guid = "000039B7-8BBE-4CEB-82C4-FA8C0B40006C",
                    SecondaryPID = "255868960",
                    SecondaryGuid = "000039B7-8BBE-4CEB-82C4-FA8C0F404020",
                });
            }


        }

        private void AddPredecessors(Dictionary<Guid, int> tasksMap)
        {
            //oss: i predecessori vanno aggiunti alla fine altrimenti quando sono stati creati tutti i task

            var tasksMapInverse = tasksMap.ReverseKeyValue();
            //Add predecessori
            foreach (Task task in _msProject.Tasks.Task)
            {
                Guid entityId = Guid.Empty;
                tasksMapInverse.TryGetValue(task.UID, out entityId);

                WBSItem entity = (WBSItem)_dataService.GetEntityById(BuiltInCodes.EntityType.WBS, entityId);

                if (entity == null)
                    continue;

                //ricavo il calcolatore in base al calendario della voce 
                Guid calendarioId = entity.GetCalendarioId();
                DateTimeCalculator timeCalc = null;
                CalendariItem calendario = _dataService.GetEntitiesById(BuiltInCodes.EntityType.Calendari, new List<Guid> { calendarioId }).FirstOrDefault() as CalendariItem;
                if (calendario != null)
                    timeCalc = new DateTimeCalculator(calendario.GetWeekHours(), calendario.GetCustomDays());
                else
                    timeCalc = new DateTimeCalculator(WeekHours.Default, new CustomDays());


                task.PredecessorLink = new List<PredecessorLink>();

                //predecessori
                WBSPredecessors wbsPredecessors = entity.GetPredecessors();
                foreach (WBSPredecessor wbsPred in wbsPredecessors.Items)
                {
                    var predecessorLink = new PredecessorLink();

                    //wbsPred.WBSItemId
                    if (tasksMap.ContainsKey(wbsPred.WBSItemId))
                        predecessorLink.PredecessorUID = tasksMap[wbsPred.WBSItemId];

                    WBSItem entityPredSource = (WBSItem)_dataService.GetEntityById(BuiltInCodes.EntityType.WBS, wbsPred.WBSItemId);

                    if (wbsPred.Type == WBSPredecessorType.FinishToFinish)
                        predecessorLink.Type = 0;
                    if (wbsPred.Type == WBSPredecessorType.FinishToStart)
                        predecessorLink.Type = 1;
                    if (wbsPred.Type == WBSPredecessorType.StartToFinish)
                        predecessorLink.Type = 2;
                    if (wbsPred.Type == WBSPredecessorType.StartToStart)
                        predecessorLink.Type = 3;


                    //calcolo linkLag
                    DateTime data = DateTime.MinValue;
                    if (wbsPred.Type == WBSPredecessorType.FinishToStart || wbsPred.Type == WBSPredecessorType.FinishToFinish)
                        data = entityPredSource.GetDataFine().Value;
                    else
                        data = entityPredSource.GetDataInizio().Value;

                    data = timeCalc.AsStartingDateTime(data);
                    DateTime dataInizioDelayed = timeCalc.AddWorkingDays(data, wbsPred.DelayDays);
                    int minutes = (int)timeCalc.GetWorkingMinutesBetween(data, dataInizioDelayed);
                    predecessorLink.LinkLag = minutes * 10;//Amount of lag time (in tenths of a minute) from the predecessor task.



                    predecessorLink.CrossProject = 0;
                    predecessorLink.LagFormat = 7;

                    task.PredecessorLink.Add(predecessorLink);
                }
            }
        }

        private void AddCalendar(int calendarUID, Guid calendarioId)
        {
            CalendariItem calendario = (CalendariItem)_dataService.GetEntityById(BuiltInCodes.EntityType.Calendari, calendarioId);

            if (calendario == null)
                return;


            //Calendario della settimana lavorativa
            WeekHours weekHours = calendario.GetWeekHours();
            

            Calendar calendar = new Calendar();
            calendar.UID = calendarUID;
            calendar.GUID = Guid.NewGuid().ToString();
            calendar.Name = ((ValoreTesto)calendario.GetValoreAttributo(BuiltInCodes.Attributo.Codice, false, false))?.V;

            calendar.WeekDays = new WeekDays();
            calendar.WeekDays.WeekDay = new List<WeekDay>();



            foreach (Model.WeekDay myDay in weekHours.Days)
            {
                var weekDay = new WeekDay();
                weekDay.DayType = ((int)myDay.Id) + 1; //0:sunday?
                weekDay.DayWorking = 1;
                WorkDaySpan myDaySpan = new WorkDaySpan(myDay.Hours);


                weekDay.WorkingTimes = new WorkingTimes();
                weekDay.WorkingTimes.WorkingTime = new List<WorkingTime>();
                foreach (var myTimeSpan in myDaySpan.TimeSpans)
                {
                    WorkingTime workingTime = new WorkingTime();
                    workingTime.FromTime = myTimeSpan.Start.ToString();
                    workingTime.ToTime = myTimeSpan.End.ToString();

                    weekDay.WorkingTimes.WorkingTime.Add(workingTime);
                }

                calendar.WeekDays.WeekDay.Add(weekDay);
            }

            //Eccezioni 

            CustomDays customDays = calendario.GetCustomDays();

            calendar.Exceptions = new Exceptions();
            calendar.Exceptions.Exception = new List<Exception>();
            
            foreach (var myDay in customDays.Days)
            {
                Exception exc = new Exception();

                exc.TimePeriod = new TimePeriod();
                exc.TimePeriod.FromDate = myDay.Day;
                exc.TimePeriod.ToDate = new DateTime(myDay.Day.Year, myDay.Day.Month, myDay.Day.Day, 23, 59, 0);

                exc.Name = myDay.Day.ToShortDateString();
                exc.Occurrences = 1;
                exc.EnteredByOccurrences = 1;
                exc.Type = 1;

                WorkDaySpan myDaySpan = new WorkDaySpan(myDay.Hours);

                exc.WorkingTimes = new WorkingTimes();
                exc.WorkingTimes.WorkingTime = new List<WorkingTime>();
                foreach (var myTimeSpan in myDaySpan.TimeSpans)
                {
                    WorkingTime workingTime = new WorkingTime();
                    workingTime.FromTime = myTimeSpan.Start.ToString();
                    workingTime.ToTime = myTimeSpan.End.ToString();

                    exc.WorkingTimes.WorkingTime.Add(workingTime);
                }

                if (exc.WorkingTimes.WorkingTime.Any())
                    exc.DayWorking = 1;
                else
                    exc.DayWorking = 0;

                calendar.Exceptions.Exception.Add(exc);
            }


            _msProject.Calendars.Calendar.Add(calendar);

        }

        DateTime RoundDateTime(DateTime? date)
        {
            if (date.HasValue)
                return new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, date.Value.Hour, date.Value.Minute, 0);
            else
                return DateTime.MinValue;
        }

        internal void Save(string filePath)
        {
            TextWriter writer = null;
            try
            {
                var serializer = new XmlSerializer(typeof(MSProject));
                writer = new StreamWriter(filePath, false);

                
                serializer.Serialize(writer, _msProject);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }
    }




}
