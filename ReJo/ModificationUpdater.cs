using Autodesk.Revit.DB;
using CommonResources;
using ReJo.UI;
using System.Linq;
using System.Windows.Controls;


namespace ReJo
{
    /// <summary>
    /// Classe che serve per monitorare le modifiche nel disegno e non consentirle sugli elementi protetti.
    /// Ad esempio non vengono consentite le modifiche su qualsiasi elemento se è aperto il dialogo modeless
    /// </summary>
    class ModificationUpdater : IUpdater
    {
        static AddInId _appId;
        UpdaterId _updaterId;
        FailureDefinitionId _failureId = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="addInId">Add-in id of the 
        /// add-in associated with this updater.</param>
        public ModificationUpdater(AddInId addInId)
        {
            _appId = addInId;
            //_updaterId = new UpdaterId(_appId, new Guid("6f453eba-4b9a-40df-b637-eb72a9ebf008"));
            //_failureId = new FailureDefinitionId(new Guid("33ba8315-e031-493f-af92-4f417b6ccf70"));

            _updaterId = new UpdaterId(_appId, Guid.NewGuid());
            _failureId = new FailureDefinitionId(Guid.NewGuid());

            string strFailure = LocalizationProvider.GetString("FinestraRegoleAttiva");
            FailureDefinition failureDefinition = FailureDefinition.CreateFailureDefinition(_failureId, FailureSeverity.Error, strFailure);
        }


        public void Execute(UpdaterData data)
        {
            if (!CmdInit.IsInitialized)
                return;

            Document doc = data.GetDocument();
            
            HashSet<long> filterElemsIdChanged = new HashSet<long>();


            foreach (ElementId id in data.GetDeletedElementIds())
            {
                if (FiltersPane.This.View?.ContainsFilter(id.Value) == true)
                    filterElemsIdChanged.Add(id.Value);

                if (RulesWnd.IsLoaded)
                {
                    FailureMessage failureMessage = new FailureMessage(_failureId);

                    failureMessage.SetFailingElement(id);
                    doc.PostFailure(failureMessage);
                }

                //if (CSCmdAssocia.AnyProtectedElement())
                //{
                //    FailureMessage failureMessage = new FailureMessage(_failureId);

                //    failureMessage.SetFailingElement(id);
                //    doc.PostFailure(failureMessage);
                //}


                //Element el = doc.GetElement(id);

                //doc.ActiveView.GetFilters()

                //if (CSCmdAssocia.AnyProtectedElement())
                //{
                //    FailureMessage failureMessage = new FailureMessage(_failureId);

                //    failureMessage.SetFailingElement(id);
                //    doc.PostFailure(failureMessage);
                //}
            }

            foreach (ElementId id in data.GetModifiedElementIds())
            {
                Element el = doc.GetElement(id);


                if (el is FilterElement)
                    filterElemsIdChanged.Add(id.Value);

                if (RulesWnd.IsLoaded)
                {
                    FailureMessage failureMessage = new FailureMessage(_failureId);

                    failureMessage.SetFailingElement(id);
                    doc.PostFailure(failureMessage);
                }


                //if (CSCmdAssocia.AnyProtectedElement())
                //{
                //    FailureMessage failureMessage = new FailureMessage(_failureId);

                //    failureMessage.SetFailingElement(id);
                //    doc.PostFailure(failureMessage);
                //}

                //if (el is FilterElement filterEl)
                //{
                //    if (filterEl is ParameterFilterElement parFilterEl)
                //    {

                //        ElementFilter elFilter = parFilterEl.GetElementFilter();
                //        parFilterEl.GetElementFilterParameters

                //        if (elFilter is ElementLogicalFilter elLogicalFilter)
                //        {

                //        }
                //    }
                //}

                //ParameterFilterElement
                //    FilterElement

            }
            foreach (ElementId id in data.GetAddedElementIds())
            {
                Element el = doc.GetElement(id);

                if (el is FilterElement)
                    filterElemsIdChanged.Add(id.Value);

                if (RulesWnd.IsLoaded)
                {
                    FailureMessage failureMessage = new FailureMessage(_failureId);

                    failureMessage.SetFailingElement(id);
                    doc.PostFailure(failureMessage);
                }


            }

            if (filterElemsIdChanged.Any())
            {
                //_AllFiltersId = Utils.GetFilters(doc).Select(item => item.Id.Value).ToHashSet();
                FiltersPane.This.Update(filterElemsIdChanged);
            }


        }

        public string GetAdditionalInformation()
        {
            return LocalizationProvider.GetString("Chiudere il dialogo modeless per rilasciare gli elementi");
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FloorsRoofsStructuralWalls;
        }

        public UpdaterId GetUpdaterId()
        {
            return _updaterId;
        }

        public string GetUpdaterName()
        {
            return "ModificationUpdater"; 
        }
    }
}
