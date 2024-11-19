using AttivitaWpf.View;
using CommonResources;
using Commons;
using MasterDetailModel;
using MasterDetailView;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttivitaWpf.View
{
    public class CalendariItemView : MasterDetailListItemView
    {
        public CalendariItemView(EntitiesListMasterDetailView master, Entity ent = null) : base(master, ent)
        {
            _master = master;
        }
    }


    public class CalendariView : MasterDetailListView, SectionItemTemplateView
    {

        public CalendariView()
        {
            _itemsView = new CalendariItemsViewVirtualized(this);
        }
        public int Code => (int)AttivitaSectionItemsId.Calendari;

    }


    public class CalendariItemsViewVirtualized : MasterDetailListViewVirtualized
    {

        public CalendariItemsViewVirtualized(MasterDetailListView owner) : base(owner)
        {
        }

        public override void Init()
        {
            base.Init();
            EntityType = DataService.GetEntityTypes()[BuiltInCodes.EntityType.Calendari];

            AttributiEntities.Load(new HashSet<Guid>());
        }

        protected override EntityView NewItemView(Entity entity)
        {
            return new CalendariItemView(this, entity);
        }

        public override void ReplaceValore(ValoreView valoreView)
        {

            string codiceAttributo = valoreView.Tag as string;
            if (codiceAttributo == BuiltInCodes.Attributo.WeekHoursText)
            {
                ReplaceWeekHoursText();
            }
            else if (codiceAttributo == BuiltInCodes.Attributo.CustomDaysText)
            {
                ReplaceCustomDaysText();
            }
            else
            {
                base.ReplaceValore(valoreView);
            }

        }

        private void ReplaceWeekHoursText()
        {
            WeekHours attWeekHours = null;
            string json = EntitiesHelper.GetValorePlainText(SelectedEntityView.Entity, BuiltInCodes.Attributo.WeekHours, false, false);
            //DEVO CREARE OGGETTO DA SERIALIZZARE

            JsonSerializer.JsonDeserialize(json, out attWeekHours, typeof(WeekHours));

            if (WindowService.SelectAttributoWeekHoursWindow(ref attWeekHours))
            {
                if (JsonSerializer.JsonSerialize(attWeekHours, out json))
                {

                    //prima action
                    ModelAction attributoWeekHoursAction = new ModelAction()
                    {
                        ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                        EntityTypeKey = BuiltInCodes.EntityType.Calendari,
                        EntitiesId = new HashSet<Guid> { SelectedEntityId },
                        AttributoCode = BuiltInCodes.Attributo.WeekHours,
                    };
                    attributoWeekHoursAction.NewValore = new ValoreTesto() { V = json };

                    //seconda action
                    ModelAction attributoWeekHoursTextAction = new ModelAction()
                    {
                        ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                        EntityTypeKey = BuiltInCodes.EntityType.Calendari,
                        EntitiesId = new HashSet<Guid> { SelectedEntityId },
                        AttributoCode = BuiltInCodes.Attributo.WeekHoursText,
                    };
                    string desc = GetAttributoFilterTextDescription(attWeekHours);
                    attributoWeekHoursTextAction.NewValore = new ValoreTesto() { V = desc, Result = desc };


                    //Main action
                    ModelAction action = new ModelAction()
                    {
                        ActionName = ActionName.MULTI,
                        EntityTypeKey = BuiltInCodes.EntityType.Calendari,
                    };

                    action.NestedActions.Add(attributoWeekHoursAction);
                    action.NestedActions.Add(attributoWeekHoursTextAction);


                    ModelActionResponse mar = DataService.CommitAction(action);
                    if (mar.ActionResponse == ActionResponse.OK)
                    {
                        //MainOperation.UpdateEntityTypesView(new List<string>() { BuiltInCodes.EntityType.Calendari });
                        ApplyFilterAndSort(_selectedEntityId);
                        UpdateCache(true);
                    }
                }
            }

            return;
        }

        public static string GetAttributoFilterTextDescription(WeekHours WeekHoursDataw)
        {

            string str = string.Empty;

            if (WeekHoursDataw == null)
                return string.Empty;

            foreach (WeekDay Item in WeekHoursDataw.Days)
            {
                string Giorno = null;
                if (Item.Id == DayOfWeek.Monday)
                    Giorno = GiorniSettimana.Lunedi;
                if (Item.Id == DayOfWeek.Tuesday)
                    Giorno = GiorniSettimana.Martedi;
                if (Item.Id == DayOfWeek.Wednesday)
                    Giorno = GiorniSettimana.Mercoledi;
                if (Item.Id == DayOfWeek.Thursday)
                    Giorno = GiorniSettimana.Giovedi;
                if (Item.Id == DayOfWeek.Friday)
                    Giorno = GiorniSettimana.Venerdi;
                if (Item.Id == DayOfWeek.Saturday)
                    Giorno = GiorniSettimana.Sabato;
                if (Item.Id == DayOfWeek.Sunday)
                    Giorno = GiorniSettimana.Domenica;
                str = str + Giorno + " " + Item.Hours + "\n";
            }

            return str;
        }

        private void ReplaceCustomDaysText()
        {
            WeekHours attWeekHours = null;
            string jsonattWeekHours = EntitiesHelper.GetValorePlainText(SelectedEntityView.Entity, BuiltInCodes.Attributo.WeekHours, false, false);
            //DEVO CREARE OGGETTO DA SERIALIZZARE
            if (!JsonSerializer.JsonDeserialize(jsonattWeekHours, out attWeekHours, typeof(WeekHours)))
                return;

            CustomDays attCustomDays = null;
            string jsonattCustomDays = EntitiesHelper.GetValorePlainText(SelectedEntityView.Entity, BuiltInCodes.Attributo.CustomDays, false, false);
            //DEVO CREARE OGGETTO DA SERIALIZZARE
            //if (!JsonSerializer.JsonDeserialize(jsonattCustomDays, out attCustomDays, typeof(CustomDays)))
            //    return;
            JsonSerializer.JsonDeserialize(jsonattCustomDays, out attCustomDays, typeof(CustomDays));

            if (WindowService.SelectAttributoCustomDaysWindow(attWeekHours, ref attCustomDays))
            {
                if (JsonSerializer.JsonSerialize(attCustomDays, out jsonattCustomDays))
                {
                    //prima action
                    ModelAction attributoCustomDaysAction = new ModelAction()
                    {
                        ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                        EntityTypeKey = BuiltInCodes.EntityType.Calendari,
                        EntitiesId = new HashSet<Guid> { SelectedEntityId },
                        AttributoCode = BuiltInCodes.Attributo.CustomDays,
                    };
                    attributoCustomDaysAction.NewValore = new ValoreTesto() { V = jsonattCustomDays };

                    //seconda action
                    ModelAction attributoCustomDaysTextAction = new ModelAction()
                    {
                        ActionName = ActionName.ATTRIBUTO_VALORE_MODIFY,
                        EntityTypeKey = BuiltInCodes.EntityType.Calendari,
                        EntitiesId = new HashSet<Guid> { SelectedEntityId },
                        AttributoCode = BuiltInCodes.Attributo.CustomDaysText,
                    };
                    string desc = GetAttributoAttributoCustomDaystDescription(attCustomDays); ;
                    attributoCustomDaysTextAction.NewValore = new ValoreTesto() { V = desc, Result = desc };

                    //Main action
                    ModelAction action = new ModelAction()
                    {
                        ActionName = ActionName.MULTI,
                        EntityTypeKey = BuiltInCodes.EntityType.Calendari,
                    };

                    action.NestedActions.Add(attributoCustomDaysAction);
                    action.NestedActions.Add(attributoCustomDaysTextAction);


                    ModelActionResponse mar = DataService.CommitAction(action);
                    if (mar.ActionResponse == ActionResponse.OK)
                    {
                        MainOperation.UpdateEntityTypesView(new List<string>() { BuiltInCodes.EntityType.Calendari });
                        UpdateCache(true);
                    }
                }
            }
            return;
        }

        private string GetAttributoAttributoCustomDaystDescription(CustomDays attCustomDays)
        {
            string str = string.Empty;

            if (attCustomDays == null)
                return string.Empty;

            foreach (CustomDay Item in attCustomDays.Days)
                str = str + Item.Day.ToString("dd/MM/yy") + " " + Item.Hours + "\n";

            return str;
        }
    }
}