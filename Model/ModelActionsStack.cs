
using CommonResources;
using Commons;
using MasterDetailModel;
using Model;
using ModelData.Model;
using Net.Sgoliver.NRtfTree.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Model
{


    public class ModelActionsStack
    {
        IDataService _dataService = null;
        public event EventHandler<ActionsChangedEventArgs> ActionsChanged;
        public event EventHandler<EventArgs> ActionPatchCountChanged;

        private List<ModelAction> Actions { get; set; } = new List<ModelAction>();
        private List<ModelActionPatchs> ModelActionPatchs { get; set; } = new List<ModelActionPatchs>();

        bool _isUndoRecordingActive = false;


        ModelAction _modelActionUndoGroup = null;
        string _undoGroupName = string.Empty;
        int _maxUndo = 3;
        Dictionary<ActionName, string> _undoActionsAllowed = new Dictionary<ActionName, string>();

        public int ModelActionPatchsCount { get => ModelActionPatchs.Count; }

        public ModelActionsStack(IDataService dataService)
        {
            _dataService = dataService;

            _undoActionsAllowed.Add(ActionName.ATTRIBUTO_VALORE_MODIFY, LocalizationProvider.GetString("ModificaValoreAttributo"));

            _undoActionsAllowed.Add(ActionName.ENTITY_DELETE, LocalizationProvider.GetString("RimuoviVoce"));
            _undoActionsAllowed.Add(ActionName.TREEENTITY_DELETE, LocalizationProvider.GetString("RimuoviVoce"));

            _undoActionsAllowed.Add(ActionName.ENTITY_MOVE, LocalizationProvider.GetString("SpostaVoce"));
            _undoActionsAllowed.Add(ActionName.TREEENTITY_MOVE, LocalizationProvider.GetString("SpostaVoce"));
            _undoActionsAllowed.Add(ActionName.TREEENTITY_MOVE_CHILD_OF, LocalizationProvider.GetString("SpostaVoce"));

            _undoActionsAllowed.Add(ActionName.ENTITY_ADD, LocalizationProvider.GetString("AggiungiVoce"));
            _undoActionsAllowed.Add(ActionName.TREEENTITY_ADD, LocalizationProvider.GetString("AggiungiVoce"));
            _undoActionsAllowed.Add(ActionName.TREEENTITY_ADD_CHILD, LocalizationProvider.GetString("AggiungiVoce"));
            _undoActionsAllowed.Add(ActionName.TREEENTITY_ADD_PARENT, LocalizationProvider.GetString("AggiungiVoce"));

            _undoActionsAllowed.Add(ActionName.ENTITY_INSERT, LocalizationProvider.GetString("InserisciVoce"));
            _undoActionsAllowed.Add(ActionName.TREEENTITY_INSERT, LocalizationProvider.GetString("InserisciVoce"));

            _undoActionsAllowed.Add(ActionName.ATTRIBUTO_VALORECOLLECTION_ADD, LocalizationProvider.GetString("AggiungiAdAttributoMultivalore"));
            _undoActionsAllowed.Add(ActionName.ATTRIBUTO_VALORECOLLECTION_REMOVE, LocalizationProvider.GetString("RimuoviDaAttributoMultivalore"));
            _undoActionsAllowed.Add(ActionName.ATTRIBUTO_VALORECOLLECTION_REPLACE, LocalizationProvider.GetString("SostituisciInAttributoMultivalore"));
            _undoActionsAllowed.Add(ActionName.ATTRIBUTO_VALORE_REPLACEINTEXT, LocalizationProvider.GetString("SostituisciNelTesto"));

            _undoActionsAllowed.Add(ActionName.DIVISIONE_ADD, LocalizationProvider.GetString("AggiungiSuddivisione"));
            _undoActionsAllowed.Add(ActionName.DIVISIONE_RENAME, LocalizationProvider.GetString("RinominaSuddivisione"));
            _undoActionsAllowed.Add(ActionName.DIVISIONI_SORT, LocalizationProvider.GetString("SpostaSuddivisioni"));

            _undoActionsAllowed.Add(ActionName.CALCOLA_ENTITES, LocalizationProvider.GetString("CalcolaVoci"));
            _undoActionsAllowed.Add(ActionName.MULTI, LocalizationProvider.GetString("Multi"));
            _undoActionsAllowed.Add(ActionName.MULTI_AND_CALCOLA, LocalizationProvider.GetString("MultiCalcolo"));

            _undoActionsAllowed.Add(ActionName.CREATE_WBS_ITEMS, LocalizationProvider.GetString("GeneraWBS"));

            _undoActionsAllowed.Add(ActionName.ENTITIES_PASTE, LocalizationProvider.GetString("Incolla"));
            _undoActionsAllowed.Add(ActionName.TREEENTITIES_PASTE, LocalizationProvider.GetString("Incolla"));

            _undoActionsAllowed.Add(ActionName.NUMERICFORMAT_COMMIT, LocalizationProvider.GetString("FormatoNumero"));


            _undoActionsAllowed.Add(ActionName.SAVE_GANTTDATA, LocalizationProvider.GetString("SaveGantt"));

                //model 3d action
                //_undoActionsAllowed.Add(ActionName.SETMODEL3D_FILTERDATA, LocalizationProvider.GetString("Model3dFilters"));
                //_undoActionsAllowed.Add(ActionName.SETMODEL3D_FILEINFO, LocalizationProvider.GetString("Model3dFiles"));
                //_undoActionsAllowed.Add(ActionName.SETMODEL3D_USERROTOTRANSLATION, LocalizationProvider.GetString("Model3dUserRototranslation"));
                //_undoActionsAllowed.Add(ActionName.SETMODEL3D_TAGSDATA, LocalizationProvider.GetString("Model3dTags"));
                //_undoActionsAllowed.Add(ActionName.SETMODEL3D_USERVIEWS, LocalizationProvider.GetString("Model3dUserViews"));
                //_undoActionsAllowed.Add(ActionName.SETMODEL3D_PREFERENCESDATA, LocalizationProvider.GetString("Model3dPreferences"));


            //*****************************************
            //Action gestite all'interno di un UndoGroup
            //DIVISIONE_REMOVE
            //CODING
            //SETENTITYTYPE
            //...
            _undoActionsAllowed.Add(ActionName.UNDOGROUP, "UndoGroup");
            //*****************************************

            //mancano...
            //ENTITIES_IMPORT,
            //VIEW_SETTINGS,//attenzione: viene usata in PrepareBeforeSave
            //SAVE_FOGLIDICALCOLODATA, //attenzione: viene usata in PrepareBeforeSave

        }
        public bool AnyToUndo
        {
            get
            {
                if (_isUndoRecordingActive)
                    return ModelActionPatchs.Any();
                else
                    return false;
            }
        }

        protected void OnActionsChanged(ActionsChangedEventArgs e)
        {
            ActionsChanged?.Invoke(this, e);
        }

        protected void OnActionPatchCountChanged(EventArgs e)
        {
            ActionPatchCountChanged?.Invoke(this, e);
        }

        public void CommitAction(ModelAction action)
        {
            if (_modelActionUndoGroup != null)
            {
                _modelActionUndoGroup.NestedActions.Add(action);
                return;
            }

            Actions.Add(action);
            if (DeveloperVariables.IsUndoActive)
            {
                string patchName = string.Empty;

                if (action.ActionName == ActionName.UNDOGROUP)
                    patchName = _undoGroupName;
                else
                    _undoActionsAllowed.TryGetValue(action.ActionName, out patchName);

                AddModelActionPatchs(patchName, action);
            }

            OnActionsChanged(new ActionsChangedEventArgs() { ActionAdded = action, ActionRemoved = null });
        }

        private async void AddModelActionPatchs(string name, ModelAction action)
        {
            if (!_isUndoRecordingActive)
            {
                ClearModelActionPatchs();
                OnActionPatchCountChanged(new EventArgs());
                return;
            }

            if (!_undoActionsAllowed.Keys.Contains(action.ActionName))
            {
                await _dataService.GetProjectPatchsAsync(true);//reset

                ClearModelActionPatchs();
                OnActionPatchCountChanged(new EventArgs());
                return;
            }

            List<Patch> patch = await _dataService.GetProjectPatchsAsync();
            if (patch != null && patch.Any())
            {
                ModelActionPatchs.Insert(0, new ModelActionPatchs() {Name = name,  ModelAction = action, Patchs = patch });
                Debug.WriteLine(string.Format("Action for undo added: {0} - {1} - {2} - {3}", name, action.EntityTypeKey, action.ActionName, action.AttributoCode));

                if (ModelActionPatchs.Count > _maxUndo)
                    ModelActionPatchs.RemoveAt(ModelActionPatchs.Count - 1);

                OnActionPatchCountChanged(new EventArgs());
            }
            else
            {
                await _dataService.GetProjectPatchsAsync(true);//reset

                ClearModelActionPatchs();
                OnActionPatchCountChanged(new EventArgs());
            }


        }

        private void ClearModelActionPatchs()
        {
            ModelActionPatchs.Clear();
        }

        public Project ProjectUndo()
        {
            ModelActionPatchs actionPatchs = GetLastUndoPatch();

            if (actionPatchs == null)
                return null;

            Project project = _dataService.ProjectUndo(actionPatchs.Patchs);
            if (project == null)
            {
                //undo non a buon fine
                Clear();
                ClearModelActionPatchs();
            }
            else
            {
                ModelActionPatchs.RemoveAt(0);
            }

            return project;
        }

        public HashSet<Guid> GetEntitiesIdActioned()
        {
            return new HashSet<Guid>(Actions.SelectMany(item => item.EntitiesId).ToList());
        }

        /// <summary>
        /// Ritorna gli id delle entità modificate di un certo tipo da un indice in poi
        /// </summary>
        /// <param name="entityTypeKey"></param>
        /// <param name="startingIndex"></param>
        /// <returns></returns>
        public HashSet<Guid> GetEntitiesIdActioned(string entityTypeKey, int startingIndex)
        {
            HashSet<Guid> ids = new HashSet<Guid>();

            for (int i = startingIndex; i < Actions.Count; i++)
            {
                if (Actions[i].EntityTypeKey == entityTypeKey)
                {
                    ids.UnionWith(Actions[i].EntitiesId);
                }
            }
            return ids;
        }

        public void Clear()
        {
            Actions.Clear();
        }

        public int GetCount() { return Actions.Count; }

        public bool AnyActions() { return Actions.Any(); }

        public void OnViewSettingsChanged()
        {
            OnActionsChanged(new ActionsChangedEventArgs() { ActionAdded = null, ActionRemoved = null });
        }

        public async void ActiveUndoRecording()
        {
            Clear();
            List<Patch> patch = await _dataService.GetProjectPatchsAsync();
            _isUndoRecordingActive = true;
        }

        public ModelAction GetLastUndoAction()
        {
            ModelActionPatchs actionPatch = GetLastUndoPatch();
            if (actionPatch != null)
                return actionPatch.ModelAction;

            return null;
        }

        public string GetLastUndoName()
        {
            ModelActionPatchs actionPatch = GetLastUndoPatch();
            if (actionPatch != null)
                return actionPatch.Name;

            return null;
        }

        public ModelActionPatchs GetLastUndoPatch()
        {
            if (!ModelActionPatchs.Any())
                return null;

            ModelActionPatchs actionPatch = ModelActionPatchs.FirstOrDefault();
            return actionPatch;
        }



        public void UndoGroupBegin(string undoGroupName, string entityTypeKey)
        {
            _modelActionUndoGroup = new ModelAction() { ActionName = ActionName.UNDOGROUP, EntityTypeKey = entityTypeKey };
            _undoGroupName = undoGroupName;

            Debug.WriteLine(string.Format("UndoGroupBegin: {0}", undoGroupName));
        }

        public string UndoGroupEnd()
        {
            Debug.WriteLine(string.Format("UndoGroupEnd: {0}", _undoGroupName));

            if (_modelActionUndoGroup == null)
                return string.Empty;

            ModelAction modelActionUndoGroup = _modelActionUndoGroup;
            string commitActionUndoGroupName = _undoGroupName;

            _modelActionUndoGroup = null;

            CommitAction(modelActionUndoGroup);
            return commitActionUndoGroupName;
        }

        public void UndoGroupCancel()
        {
            Debug.WriteLine(string.Format("UndoGroupCancel: {0}", _undoGroupName));

            _modelActionUndoGroup = null;
            _undoGroupName = string.Empty;

        }

        public string GetUndoActionsName()
        {
            //return string.Join('\n', ModelActionPatchs.Select(item => _undoActionsAllowed[item.ModelAction.ActionName]));
            return string.Join('\n', ModelActionPatchs.Select(item => item.Name));
        }
    }

    public class ActionsChangedEventArgs : EventArgs
    {
        
        public ModelAction ActionAdded { get; set; }
        public ModelAction ActionRemoved { get; set; }

    }

    public class ModelActionPatchs
    {
        public string Name { get; set; }
        public ModelAction ModelAction { get; set; } = null;
        public List<Patch> Patchs { get; set; } = new List<Patch>();

    }

    public class UndoGroupsName
    {
        public static string DeleteDivisione => LocalizationProvider.GetString("DeleteDivisione");
        public static string Coding => LocalizationProvider.GetString("Coding");
        public static string SetEntityType => LocalizationProvider.GetString("SetEntityType");
        public static string AggiornaDataInizioLavori => LocalizationProvider.GetString("AggiornaDataInizioLavori");
        public static string SetTimeRulerToGanttData => LocalizationProvider.GetString("SetTimeRulerToGanttData");
        public static string SetChartSetting => LocalizationProvider.GetString("SetChartSetting");
        public static string ProgrammazioneSAL => LocalizationProvider.GetString("ProgrammazioneSAL");
        public static string ProgrammaLavori => LocalizationProvider.GetString("ProgrammaLavori");
        public static string ApplyComputoRules => LocalizationProvider.GetString("GeneraComputo");
    }



}
