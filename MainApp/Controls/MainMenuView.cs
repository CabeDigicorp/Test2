using Commons;
using ComputoWpf;
using MasterDetailModel;
using PrezzariWpf;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows.Controls;

using System.Windows;
using System;
using ProtoBuf;
using System.IO;
using ProtoBuf.Meta;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using DivisioniWpf;
using MasterDetailView;
using Model;
using _3DModelExchange;
using log4net.Config;
using CommonResources;
using ElementiWpf;
using ContattiWpf;
using ContattiWpf.View;
using DatiGeneraliWpf.View;
using PrezzariWpf.View;
using System.Diagnostics;
using WebServiceClient;
using System.Threading.Tasks;
using StampeWpf.View;
using AttivitaWpf.View;
using System.Windows.Media;
using System.Security.Permissions;
using System.Security;
using System.Security.AccessControl;
using FogliDiCalcoloWpf;
using ModelData.Dto;
using AutoMapper;
using WebServiceClient.Clients;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Threading;

using DevExpress.Mvvm.Native;

using DevExpress.Data.Extensions;
using Syncfusion.Data.Extensions;
using System.Collections;




namespace MainApp
{
    //[System.Reflection.ObfuscationAttribute(Feature = "renaming", ApplyToMembers = true)]
    public class MainMenuView : NotificationBase
    {
        internal MainView MainView { get; set; }


        internal string ProjectFileExtension = "join";


        Int32 _currentFileVersion = (int) FileVersion.vLast;
        //Int32 _currentFileVersion = (int) FileVersion.v101;


        public Int32 CurrentFileVersion { get => _currentFileVersion; }

        private bool _saveAsJsonOnOpening = false;

        ProjectSource _currentProjectInfo = null;
        internal ProjectSource CurrentProjectSource
        {
            get => _currentProjectInfo;
            set { _currentProjectInfo = value; }
        }

        internal Project CurrentProject { get; set; }
        internal ProjectService ProjectService { get; set; } = null;
        public ClientDataService ClientDataService { get; set; } = null;
        public IEntityWindowService WindowService { get; set; } = null;
        public ModelActionsStack ModelActionsStack { get; set; } = null;
        public MessageBarView MessageBarView { get; set; } = new MessageBarView();
        internal DatiGeneraliView DatiGeneraliView { get; set; }
        internal DivisioniView DivisioniView { get; set; }
        //internal ContattiView ContattiView { get => DatiGeneraliView.ContattiView; }
        internal ElencoPrezziView ElencoPrezziView { get; set; }
        internal AttivitaView AttivitaView { get; set; }
        internal ComputoView ComputoView { get; set; }
        internal ElementiView ElementiView { get; set; }
        internal StampeView StampeView { get; set; }
        internal FogliDiCalcoloView FogliDiCalcoloView { get; set; }
        public Model3dFilesInfoView Model3dFilesInfoView { get; set; } = new Model3dFilesInfoView();
        public Dictionary<string, CalculatorFunction> CalculatorFunctions { get; protected set; } = new Dictionary<string, CalculatorFunction>();
        
        internal I3DModelService Model3dService { get; set; }
        internal Window BIMViewerWindow { get => _BIMViewerWindow; }

        internal AppSettings AppSettings { get; set; } = new AppSettings();
        public MainOperation MainOperation { get;set;}

        public SectionEnum CurrentSection { get => (SectionEnum)TabControlSelectedIndex; }

        public static string Join360MachineName => "SRVXA01";

        public static string MaxAppVersion => "999";

        /// <summary>
        /// Percorso file ini
        /// </summary>
        public static string AppSettingsPath { get; protected set; }
        public bool IsFirstRun { get; set; } = false;//primo avvio dopo installazione/aggiornamento (cambio versione)

        /// <summary>
        /// cache in memoria dei prezzari esterni precedentemente aperti
        /// key: nome file prezzario senza estensione
        /// </summary>
        internal Dictionary<string, IDataService> PrezzariCache = new Dictionary<string, IDataService>();

        DispatcherTimer _dispatcherTimer = new DispatcherTimer();//autosave

        internal bool IsProjectClosing { get; set; } = false;
        private ObservableCollection<FileInfoDetailView> _recentProjectsFileInfo = new ObservableCollection<FileInfoDetailView>();
        public ObservableCollection<FileInfoDetailView> RecentProjectsFileInfo
        {
            get { return _recentProjectsFileInfo; }
            set { _recentProjectsFileInfo = value; }
        }

        bool _isInitialized = false;

        public bool WindowsRegistryResult { get; set; } = false;


        public MainMenuView()
        {

        }

        internal async Task<bool> Init()
        {
            GenericResponse loginResponse = null;
            if (LicenseHelper.IsAnyFeaturePresent(new List<LicenseFeature>() { LicenseFeature.Feature_Web }, out _))
            {
                MessageBarView.Show(LocalizationProvider.GetString("AccessoWebInCorso"), false);
                loginResponse = await UtentiWebClient.Login();
            }
            else
                loginResponse = new GenericResponse(false);


            InitRuntimeTypeModel();
            LoadAppSettings();

            if (loginResponse != null)
            {
                LoadRecentsFileInfo();
                
            }
            
            InitAppCache();


            MainOperation = new MainOperation(this);

            WindowService wndService = new WindowService();
            wndService.PrezzarioItemsChanged += WndService_PrezzarioItemsChanged;
            wndService.CapitoliItemsChanged += WndService_CapitoliItemsChanged;
            wndService.ElementiItemsChanged += WndService_ElementiItemsChanged;
            wndService.DivisioneItemsChanged += WndService_DivisioneItemsChanged;
            wndService.ContattiItemsChanged += WndService_ContattiItemsChanged;
            wndService.AllegatiItemsChanged += WndService_AllegatiItemsChanged;
            wndService.CalendariItemsChanged += WndService_CalendariItemsChanged;
            wndService.ElencoAttivitaItemsChanged += WndService_ElencoAttivitaItemsChanged;
            WindowService = wndService;


#if !DEBUG
            WindowsRegistryResult = SetWindowRegistryInfo();
#endif

            //set FirstRun
            if (!string.IsNullOrEmpty(AppVersion) && AppVersion != AppSettings.AppVersion)
            {
                //app appena aggiornata (cambio di versione)
                MainAppLog.Info(System.Reflection.MethodBase.GetCurrentMethod(), "IsFirstRun");
                IsFirstRun = true;
                AppSettings.AppVersion = AppVersion;
            }

            RecentProjectsFileInfo.CollectionChanged += RecentProjectsFileInfo_CollectionChanged;

            OpenJoinFileOnStart();

            _currentLanguage = LanguageItems.FirstOrDefault(item => item.Code == LanguageHelper.CurrentLanguageCode);


            UpdateUI();

            InitAutoSave();

            _isInitialized = true;
            MessageBarView.Ok();
            
            return true;
        }



        private void WndService_ElencoAttivitaItemsChanged(object sender, EventArgs e)
        {
            AttivitaView.ElencoAttivitaView.ItemsView.Load();
        }

        private void WndService_CalendariItemsChanged(object sender, EventArgs e)
        {
            AttivitaView.CalendariView.ItemsView.Load();
        }

        private void WndService_AllegatiItemsChanged(object sender, EventArgs e)
        {
            DatiGeneraliView.AllegatiView.ItemsView.Load();
        }

        private void WndService_TagItemsChanged(object sender, EventArgs e)
        {
            DatiGeneraliView.TagView.ItemsView.Load();
        }



        ///// <summary>
        ///// //Apertura del file .join con doubleClick sul file
        ///// </summary>
        private void OpenJoinFileOnStart()
        {
            try
            {
                string filename = null;

                if (AppDomain.CurrentDomain == null)
                    return;

                if (AppDomain.CurrentDomain.SetupInformation == null)
                    return;

                string str = Environment.CommandLine;

                MainAppLog.Info(MethodBase.GetCurrentMethod(), String.Format("OpenJoinFileOnStart {0}", str));

                string[] args = Environment.GetCommandLineArgs();
                
                string temp = string.Join(";", args);
            

                string[] strs = str.Split("\"");

                if (strs.Length > 1)
                {

                    filename = strs[1];

                    if (filename != null)
                    {
                        MainAppLog.Info(MethodBase.GetCurrentMethod(), String.Format("OpenJoinFileOnStart {0}", filename));

                        if (Path.GetExtension(filename) == string.Format(".{0}",ProjectFileExtension) && File.Exists(filename))
                        {

                            Uri uri = new Uri(filename);
                            filename = uri.LocalPath;

                            CurrentProjectSource = new ProjectSourceFile(new FileInfo(filename));

                            CurrentProjectOpenAsync();
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                MainAppLog.Error(MethodBase.GetCurrentMethod(), exc.Message, exc);
            }
        }

        private void WndService_DivisioneItemsChanged(object sender, EventArgs e)
        {
            DivisioneItemsChangedEventArgs divChangedArgs= e as DivisioneItemsChangedEventArgs;
          
            if (DivisioniView.CurrentDivisioneView.Id == divChangedArgs.Id)
                DivisioniView.CurrentDivisioneView.ItemsView.Load();
        }

        private void WndService_ElementiItemsChanged(object sender, EventArgs e)
        {
            ElementiView.ItemsView.Load();
        }

        private void WndService_PrezzarioItemsChanged(object sender, EventArgs e)
        {
            ElencoPrezziView.PrezzarioView.ItemsView.Load();
        }

        private void WndService_CapitoliItemsChanged(object sender, EventArgs e)
        {
            ElencoPrezziView.CapitoliView.ItemsView.Load();
        }

        private void WndService_ContattiItemsChanged(object sender, EventArgs e)
        {
            DatiGeneraliView.ContattiView.ItemsView.Load();
        }

        internal async Task<bool> OnClosing()
        {
            if (!_isInitialized)
                return false;

            try
            {
                if (!await ProjectClose())
                    return false;

                IsProjectClosing = true;

                if (BIMViewerWindow != null)
                    BIMViewerWindow.Close();

                if (_RevitModel3dService != null)
                    _RevitModel3dService.Stop();

            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message, e);
                //MessageBox.Show(e.Message);
            }



            SaveAppSettings();

            return true;
        }

        int _expandedWidth = 200;
        int _collapsedWidth = 40;
        public int CollapsedWidth { get => _collapsedWidth; }

        bool _isMenuExpanded = true;
        public bool IsMenuExpanded
        {
            get { return _isMenuExpanded; }
            set
            {
                if (SetProperty(ref _isMenuExpanded, value))
                {
                    RaisePropertyChanged(GetPropertyName(() => this.MenuWidth));
                }
            }
        }

        public int MenuWidth
        {
            get
            {
                if (IsMenuExpanded)
                    return _expandedWidth;
                else
                    return CollapsedWidth;
            }
        }



        int _tabControlSelectedIndex = 0;
        public int TabControlSelectedIndex
        {
            get => _tabControlSelectedIndex;
            set
            {
                if (SetProperty(ref _tabControlSelectedIndex, value))
                {
                    if (_tabControlSelectedIndex == (int) SectionEnum.Divisioni)//Divisioni
                    {
                        DivisioniView.Init();
                    }

                    UpdateMenu();
                    UpdateUI();
                }
            }
        }

        private void UpdateMenu()
        {
            RaisePropertyChanged(GetPropertyName(() => ProjectMenuItem0IsChecked));
            RaisePropertyChanged(GetPropertyName(() => ProjectMenuItem1IsChecked));
            RaisePropertyChanged(GetPropertyName(() => ProjectMenuItem2IsChecked));
            RaisePropertyChanged(GetPropertyName(() => ProjectMenuItem3IsChecked));
            RaisePropertyChanged(GetPropertyName(() => ProjectMenuItem4IsChecked));
            RaisePropertyChanged(GetPropertyName(() => ProjectMenuItem5IsChecked));
            RaisePropertyChanged(GetPropertyName(() => ProjectMenuItem6IsChecked));
            RaisePropertyChanged(GetPropertyName(() => ProjectMenuItem7IsChecked));
            RaisePropertyChanged(GetPropertyName(() => ProjectMenuItem8IsChecked));
            RaisePropertyChanged(GetPropertyName(() => ProjectMenuItem9IsChecked));
            RaisePropertyChanged(GetPropertyName(() => ProjectMenuItem10IsChecked));
            RaisePropertyChanged(GetPropertyName(() => DbMenuItem0IsChecked));
            RaisePropertyChanged(GetPropertyName(() => DbMenuItem1IsChecked));
            RaisePropertyChanged(GetPropertyName(() => DbMenuItem2IsChecked));
            RaisePropertyChanged(GetPropertyName(() => DbMenuItem3IsChecked));
            RaisePropertyChanged(GetPropertyName(() => DbMenuItem4IsChecked));
            //RaisePropertyChanged(GetPropertyName(() => AppMenuGuidaItemIsChecked));
            RaisePropertyChanged(GetPropertyName(() => AppMenuTeleassistenzaItemIsChecked));
            RaisePropertyChanged(GetPropertyName(() => AppMenuInfoItemIsChecked));
            RaisePropertyChanged(GetPropertyName(() => AppMenuImpostazioniItemIsChecked));
        }

        public bool ProjectMenuItem0IsChecked { get => _tabControlSelectedIndex == 0; set { TabControlSelectedIndex = 0; } }
        public bool ProjectMenuItem1IsChecked { get => _tabControlSelectedIndex == 1; set { TabControlSelectedIndex = 1; } }
        public bool ProjectMenuItem2IsChecked { get => _tabControlSelectedIndex == 2; set { TabControlSelectedIndex = 2; } }
        public bool ProjectMenuItem3IsChecked { get => _tabControlSelectedIndex == 3; set { TabControlSelectedIndex = 3; } }
        public bool ProjectMenuItem4IsChecked { get => _tabControlSelectedIndex == 4; set { TabControlSelectedIndex = 4; } }
        public bool ProjectMenuItem5IsChecked { get => _tabControlSelectedIndex == 5; set { TabControlSelectedIndex = 5; } }
        public bool ProjectMenuItem6IsChecked { get => _tabControlSelectedIndex == 6; set { TabControlSelectedIndex = 6; } }
        public bool ProjectMenuItem7IsChecked { get => _tabControlSelectedIndex == 7; set { TabControlSelectedIndex = 7; } }
        public bool ProjectMenuItem8IsChecked { get => _tabControlSelectedIndex == 8; set { TabControlSelectedIndex = 8; } }
        public bool ProjectMenuItem9IsChecked { get => _tabControlSelectedIndex == 9; set { TabControlSelectedIndex = 9; } }
        public bool ProjectMenuItem10IsChecked { get => _tabControlSelectedIndex == 10; set { TabControlSelectedIndex = 10; } }
        public bool DbMenuItem0IsChecked { get => _tabControlSelectedIndex == 11; set { TabControlSelectedIndex = 11; } }
        public bool DbMenuItem1IsChecked { get => _tabControlSelectedIndex == 12; set { TabControlSelectedIndex = 12; } }
        public bool DbMenuItem2IsChecked { get => _tabControlSelectedIndex == 13; set { TabControlSelectedIndex = 13; } }
        public bool DbMenuItem3IsChecked { get => _tabControlSelectedIndex == 14; set { TabControlSelectedIndex = 14; } }
        public bool DbMenuItem4IsChecked { get => _tabControlSelectedIndex == 15; set { TabControlSelectedIndex = 15; } }
        //public bool AppMenuGuidaItemIsChecked { get => _tabControlSelectedIndex == (int) SectionEnum.Guida; set { TabControlSelectedIndex = (int)SectionEnum.Guida; } }
        public bool AppMenuTeleassistenzaItemIsChecked { get => _tabControlSelectedIndex == (int)SectionEnum.Teleassistenza; set { TabControlSelectedIndex = (int)SectionEnum.Teleassistenza; } }
        public bool AppMenuInfoItemIsChecked { get => _tabControlSelectedIndex == (int)SectionEnum.Info; set { TabControlSelectedIndex = (int)SectionEnum.Info; } }
        public bool AppMenuImpostazioniItemIsChecked { get => _tabControlSelectedIndex == (int)SectionEnum.Impostazioni; set { TabControlSelectedIndex = (int)SectionEnum.Impostazioni; } }



        public ICommand ProjectCloseCommand
        {
            get
            {
                return new CommandHandler(() => this.ProjectClose());
            }
        }

        internal async Task<bool> ProjectClose()
        {
            bool ret = true;


            if (!IsProjectOpened)
                return ret;

            //if (CurrentProject == null)
            //    return ret;

            if (IsProjectSavable)
            {
                MessageBoxResult res = MessageBox.Show(LocalizationProvider.GetString("SalvareIlProgettoCorrente"), LocalizationProvider.GetString("AppName"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    ret = await ProjectSave();
                }
                else if (res == MessageBoxResult.No)
                {

                }
                else //cancel
                {
                    return false;
                }

            }

            if (ClientDataService != null)
            {
                ClientDataService.Suspended = true;
                ClientDataService.ResetCache();
            }

            CurrentProject = null;
            CurrentProjectSource = null;

            if (ModelActionsStack != null)
                ModelActionsStack.Clear();

            UpdateRecentsUI();

            MainView.UpdateMainWindowBar();

            IsProjectClosing = true;

            CloseProjectSectionsView();

            if (BIMViewerWindow != null)
                BIMViewerWindow.Close();

            if (_RevitModel3dService != null)
                _RevitModel3dService.ProjectClose();


            IsProjectSavable = false;
            //SaveIconKey = SaveIconKeyFile;
            RaisePropertyChanged(GetPropertyName(() => IsProjectSavable));

            ClientDataService = null;
            IsProjectClosing = false;
            UpdateUI();
            return ret;
        }

        private void MessageBarView_Confirmed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public ICommand ProjectOpenCommand { get => new CommandHandler(() => this.ProjectOpen()); }

        internal async void ProjectOpen()//sfoglia
        {

            

            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.DefaultExt = ProjectFileExtension;
                openFileDialog.Filter = string.Format("{0} files (*.{1})|*.{2}|{3} files (*.{4})|*.{5}|All files (*.*)|*.*",
                    ProjectFileExtension, ProjectFileExtension, ProjectFileExtension,
                    XpweImportExport.FileExtension, XpweImportExport.FileExtension, XpweImportExport.FileExtension);

                if (openFileDialog.ShowDialog() == true)
                {  
                    if (!await ProjectClose())
                        return;

                    string ext = Path.GetExtension(openFileDialog.FileName).Trim().Substring(1);
                    if (ext == ProjectFileExtension)
                    {
                        WindowService.ShowWaitCursor(true);
                        
                        CurrentProjectSource = new ProjectSourceFile(new FileInfo(openFileDialog.FileName));
                        CurrentProjectOpenAsync();
                    }
                    else if (ext == XpweImportExport.FileExtension)
                    {
                        WindowService.ShowWaitCursor(true);

                        XpweImportExport ieXpwe = new XpweImportExport();
                        ieXpwe.MainOperation = MainOperation;
                        ieXpwe.WindowService = WindowService;

                        MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Aggiornamento modello xpwe..."), false, 0);

                        string modelFullFileName = ieXpwe.GetModelloFullFileName();

                        await ieXpwe.UpdateXpweModel();

                        await ProjectNew(modelFullFileName);

                        if (ClientDataService == null)
                        {
                            MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Modello xpwe non presente"));
                            return;
                        }

                        WindowService.ShowWaitCursor(false);

                        MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Importazione xpwe..."), false, 0);

                        await ieXpwe.RunImport(ClientDataService, openFileDialog.FileName);

                        MainOperation.UpdateEntityTypesView(new List<string> { BuiltInCodes.EntityType.Prezzario, BuiltInCodes.EntityType.Computo, BuiltInCodes.EntityType.WBS });

                    }

                    
                }
            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message, e);
                //MessageBox.Show(e.Message);

            }
            finally
            {
                WindowService.ShowWaitCursor(false);
            }

        }


        public bool IsProjectWebOpenAllowed
        {
            get
            {
                string msg = null;
                if (LicenseHelper.IsAnyFeaturePresent(new List<LicenseFeature>() { LicenseFeature.Feature_Web }, out msg))
                {
                    return true;

                }
                return false;
            }
        }

        public ICommand ProjectWebOpenCommand { get => new CommandHandler(() => this.ProjectWebOpen());}
        internal async void ProjectWebOpen()
        {

            string msg = null;
            if (!LicenseHelper.IsAnyFeaturePresent(new List<LicenseFeature>() { LicenseFeature.Feature_Web }, out msg))
            {
                MainOperation.ShowMessageBarView(msg);
                return;
            }


            ModelHelper.ValidateModelData();



            var gr = await WebLogin();
            if (!gr.Success)
            {
                MessageBarView.Show(string.Format("{0} {1}", LocalizationProvider.GetString("Autenticazione fallita."), gr.Message));
                return;
            }
            else
            {

                WindowService wndService = WindowService as WindowService;

                wndService.DataService = ClientDataService;
                wndService.MainOperation = MainOperation;

                Guid operaId = Guid.Empty;
                Guid progettoId = Guid.Empty;

                string nomeProgetto = string.Empty;
                if (CurrentProjectSource != null)
                    nomeProgetto = Path.GetFileNameWithoutExtension(CurrentProjectSource.Name);

                if (wndService.WebOpenProjectWnd(out operaId, out progettoId, ref nomeProgetto))
                {
                    if (!await ProjectClose())
                        return;

                    CurrentProjectSource = new ProjectSourceWeb()
                    {
                        FullName = nomeProgetto,
                        Name = nomeProgetto,
                        OperaId = operaId,
                        ProgettoId = progettoId,
                    };

                    CurrentProjectOpenAsync();
                }
            }

        }

        public async void CurrentProjectOpenAsync()
        {
            if (CurrentProjectSource == null)
                return;


            Project project = null;
            Int32 projectVersion = 100;

           

            var gr = await CurrentProjectSource.CanLoadProject(MessageBarView);
            if (!gr.Success)
            {
                MessageBarView.Show(gr.Message);
                return;
            }

            await Task.Run(async () =>
            {

                var prjData = await CurrentProjectSource.LoadProject(MessageBarView);

                project = prjData.Project;
                projectVersion = prjData.ProjectVersion;

                if (project == null)
                    return;

                if (_saveAsJsonOnOpening)
                {
                    SaveProjectAsJson(project);
                }

                CurrentProject = project;


                //spedisco il project al server
                ProjectService = new ProjectService();
                ProjectService.Init(CurrentProject/*, _repository*/);


                if (projectVersion < CurrentFileVersion)
                    MessageBarView.Show(LocalizationProvider.GetString("Conversione in corso..."), false, 70);


                MessageBarView.Show(LocalizationProvider.GetString("InizializzazioneProgetto"), false, 75);

                InitProject(projectVersion);


            });



            if (!IsProjectOpened)
                return;

            MessageBarView.Show(LocalizationProvider.GetString("InizializzazioneSezioni"), false, 80);



            //if (CurrentProject == null)
            //    return;

            InitProjectSectionsView();

            LoadCalculatorModel3dValues();

            UpdateRecentProjectsPath(CurrentProjectSource);
            SaveAppSettings();
            LoadRecentsFileInfo();

            IsProjectSavable = false;
            

            //UpdateRecentsUI();

            MainView.UpdateMainWindowBar();

            TabControlSelectedIndex = 1;

            MessageBarView.Ok();

            UpdateUI();

            return;
        }

        private int ProjectFileRead(ref Project project)
        {
            int projectVersion;
            using (var file = File.OpenRead(CurrentProjectSource.FullName))
            {
                var pStream = new ProgressStream(file);
                pStream.BytesRead += new ProgressStreamReportDelegate(pStream_BytesRead);


                //BinaryReader reader = new BinaryReader(file);
                //projectVersion = reader.ReadInt32();

                //if (projectVersion <= (int)FileVersion.v101)
                //{
                //    try
                //    {
                //        //project = Serializer.Deserialize<Project>(file);
                //        project = Serializer.Deserialize<Project>(pStream);
                //    }
                //    catch (Exception exc)
                //    {
                //        MainAppLog.Error(MethodBase.GetCurrentMethod(), "Errore nell'apertura del file: ", exc);
                //    }

                //}
                //else if (projectVersion <= (int)FileVersion.v102)
                //{

                //    using (var brotli = new BrotliStream(pStream, CompressionMode.Decompress, true))
                //    {
                //        project = Serializer.Deserialize<Project>(brotli);
                //    }
                //}

                project = ModelSerializer.Deserialize(pStream, out projectVersion);

                if (project == null)
                {
                    MessageBarView.Show(LocalizationProvider.GetString("VersioneDelFileSuccessiva"));
                }
            }

            return projectVersion;
        }



        void pStream_BytesRead(object sender,
                              ProgressStreamReportEventArgs args)
        {
            int perc = (int)((50.0 * args.StreamPosition) / args.StreamLength);

            MessageBarView.Show(LocalizationProvider.GetString("CaricamentoInCorso"), false, perc);
           

        }

        void pStream_BytesWritten(object sender,
                      ProgressStreamReportEventArgs args)
        {

            //prendo come totale i byte del file precedente
            int perc = 0;
            if (CurrentProjectSource != null && /*CurrentProjectSource.Exists &&*/ CurrentProjectSource.Length > 0 && args.StreamPosition <= CurrentProjectSource.Length)
                perc = (int)((80 * args.StreamPosition) / CurrentProjectSource.Length);
            else
                perc = 80;

            
            MessageBarView.Show(LocalizationProvider.GetString("SalvataggioInCorso"), false, perc);


        }


        private void SaveProjectAsJson(Project project)
        {
            if (CurrentProjectSource == null)
                return;

            string json = "";
            JsonSerializer.JsonSerialize(project, out json);

            var result = Path.ChangeExtension(CurrentProjectSource.FullName, ".jj");
            using (var fileJson = File.Create(result))
            {
                Serializer.Serialize(fileJson, json);
            }
        }

        internal void SaveCurrentProjectAsJson()
        {
            if (!IsProjectOpened)
                return;

            //if (CurrentProject == null)
            //    return;

            if (CurrentProjectSource == null)
                return;

            string json = "";
            JsonSerializer.JsonSerialize(CurrentProject, out json);

            var result = Path.ChangeExtension(CurrentProjectSource.FullName, ".jj");
            using (var fileJson = File.Create(result))
            {
                Serializer.Serialize(fileJson, json);
            }
        }

        public string CurrentProjectInfoAsString
        {
            get
            {
                string info = string.Empty;
                if (CurrentProjectSource == null)
                {
                    
                    //if (CurrentProject == null)
                    if (!IsProjectOpened)
                        return LocalizationProvider.GetString("Nessuno");
                    else
                        return LocalizationProvider.GetString("Non ancora salvato");
                }
                else
                {
                    string projectKey = CurrentProjectSource.GetKey();
                    var projectInfo = RecentProjectsFileInfo.FirstOrDefault(item => item.Key == projectKey);
                    if (projectInfo != null)
                    {
                        info = projectInfo.GetInfo();
                    }

                    //CurrentProjectSource.Update();
                    //info = CurrentProjectSource.GetInfo();

                }
                return info;
            }
        }


        /// <summary>
        /// Creazione/Update di un nuovo tile recent
        /// </summary>
        /// <param name="currentProjectSource"></param>
        private void UpdateRecentProjectsPath(ProjectSource currentProjectSource)
        {

            RecentProjectInfo newRecent = currentProjectSource.CreateRecentProjectInfo();

            //string currentProjectPath = currentProjectSource.FullName;
            //RecentProjectInfo newRecent = new RecentProjectInfo() { Path = currentProjectPath, ProjectSourceType = currentProjectSource .Type};

            string recentKey = ProjectSource.GetKey(newRecent);

            if (currentProjectSource.Type == ProjectSourceType.File)
            {
                int index = AppSettings.RecentProjects.FindIndex(item => ProjectSource.GetKey(item) == recentKey);
                if (index >= 0)
                {
                    newRecent.Thumbnail = AppSettings.RecentProjects[index].Thumbnail;
                    AppSettings.RecentProjects.RemoveAt(index);
                }

                AppSettings.RecentProjects.Insert(0, newRecent);
            }
            else if (currentProjectSource.Type == ProjectSourceType.Web)
            {
                ProjectSourceWeb currentProjectSourceWeb = currentProjectSource as ProjectSourceWeb;
                if (currentProjectSourceWeb != null)
                {
                    newRecent.OperaId = currentProjectSourceWeb.OperaId;
                    newRecent.ProgettoId = currentProjectSourceWeb.ProgettoId;
                }

                int index = AppSettings.RecentProjects.FindIndex(item => ProjectSource.GetKey(item) == recentKey);
                if (index >= 0)
                {
                    newRecent.Thumbnail = AppSettings.RecentProjects[index].Thumbnail;
                    AppSettings.RecentProjects.RemoveAt(index);
                }

                AppSettings.RecentProjects.Insert(0, newRecent);
            }
        }

        public ICommand ProjectNewCommand
        {
            get
            {
                return new CommandHandler(() => this.ProjectNew());
            }
        }

        internal async Task ProjectNew(string modelFullFileName = null)
        {


            //if (!await ProjectClose())
            //    return;

            try
            {

                //string modelFullFileName = string.Empty;

                WindowService wndService = WindowService as WindowService;

                wndService.DataService = ClientDataService;
                wndService.MainOperation = MainOperation;

                Int32 modelFileVersion = 0;

                if (modelFullFileName == null)
                {
                    modelFullFileName = string.Empty;

                    if (wndService.NewProjectModelWindow(ref modelFullFileName))
                    {
                        WindowService.ShowWaitCursor(true);

                        if (!await ProjectClose())
                            return;

                        CurrentProject = wndService.MainOperation.CreateProjectByModel(modelFullFileName, out modelFileVersion);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    if (!await ProjectClose())
                        return;

                    CurrentProject = wndService.MainOperation.CreateProjectByModel(modelFullFileName, out modelFileVersion);
                }

                MessageBarView.Show(LocalizationProvider.GetString("CaricamentoInCorso"), false, 0);

                if (modelFullFileName == string.Empty)
                {
                    CurrentProject = new Project();
                    modelFileVersion = CurrentFileVersion;
                }

                //if (CurrentProject == null)
                if (!IsProjectOpened)
                    return;

                ProjectService = new ProjectService();
                ProjectService.Init(CurrentProject/*, _repository*/);

                if (modelFileVersion < CurrentFileVersion)
                    MessageBarView.Show(LocalizationProvider.GetString("Conversione in corso..."), false, 30);

                InitProject(modelFileVersion);

                MessageBarView.Show(LocalizationProvider.GetString("CaricamentoInCorso"), false, 80);

                InitProjectSectionsView();

                LoadCalculatorModel3dValues();

                CurrentProjectSource = null;

                IsProjectSavable = false;
                //SaveIconKey = SaveIconKeyFile;
                UpdateRecentsUI();
                MainView.UpdateMainWindowBar();

                TabControlSelectedIndex = 1;//dati generali

                MessageBarView.Ok();
                UpdateUI();

            }
            finally
            {
                WindowService.ShowWaitCursor(false);
            }

            

        }

        private void InitProject(Int32 projectVersion)
        {
            ModelActionsStack = new ModelActionsStack(ProjectService);
            if (DeveloperVariables.IsUndoActive)
                ModelActionsStack.ActiveUndoRecording();

            ModelActionsStack.ActionsChanged += ModelActionsStack_ActionsChanged;
            ModelActionsStack.ActionPatchCountChanged += ModelActionsStack_ActionPatchCountChanged;

            ClientDataService = new ClientDataService(ProjectService, ModelActionsStack);
            ClientDataService.Init();
            ClientDataService.ProgressChanged += ClientDataService_ProgressChanged;

            //if (projectVersion < CurrentFileVersion)
            //    MessageBarView.Show(LocalizationProvider.GetString("Conversione in corso..."), false, 80);

            MainOperation.UpgradeProjectToLastVersion(projectVersion);

            CalculatorFunctions.Clear();
            CalculatorFunctions.Add(Model3dCalculatorFunction.Names.Ifc, new ValoreM3dCalculatorFunction(ClientDataService, Model3dCalculatorFunction.Names.Ifc));
            CalculatorFunctions.Add(Model3dCalculatorFunction.Names.Rvt, new ValoreM3dCalculatorFunction(ClientDataService, Model3dCalculatorFunction.Names.Rvt));
            CalculatorFunctions.Add(NoteCalculatorFunction.Name, new NoteCalculatorFunction());
            CalculatorFunctions.Add(CmpCalculatorFunction.Name, new CmpCalculatorFunction(ClientDataService));
            CalculatorFunctions.Add(EPCalculatorFunction.Name, new EPCalculatorFunction(ClientDataService));
            CalculatorFunctions.Add(ElmCalculatorFunction.Name, new ElmCalculatorFunction(ClientDataService));
            CalculatorFunctions.Add(CntCalculatorFunction.Name, new CntCalculatorFunction(ClientDataService));
            CalculatorFunctions.Add(InfCalculatorFunction.Name, new InfCalculatorFunction(ClientDataService));
            CalculatorFunctions.Add(DivCalculatorFunction.Name, new DivCalculatorFunction(ClientDataService));
            CalculatorFunctions.Add(EAtCalculatorFunction.Name, new EAtCalculatorFunction(ClientDataService));
            CalculatorFunctions.Add(WBSCalculatorFunction.Name, new WBSCalculatorFunction(ClientDataService));
            CalculatorFunctions.Add(VarCalculatorFunction.Name, new VarCalculatorFunction(ClientDataService));
            CalculatorFunctions.Add(CapCalculatorFunction.Name, new CapCalculatorFunction(ClientDataService));

            //Serve per settare RtfEntityDataService come base dati per i Field nell'rtf
            ValoreHelper.FormattedTextHelper.RtfMailMerge(string.Empty, new RtfEntityDataService(MainOperation));

            IfcViewerInit();
        }

        private void ClientDataService_ProgressChanged(object sender, ProgressEventArgs e)
        {
            if (e.ProgressValue >= 100)
                MainOperation.ShowMessageBarView(e.Message, false, -1, true);
            else
                MainOperation.ShowMessageBarView(e.Message, false, e.ProgressValue);
        }

        private void InitProjectSectionsView()
        {
            try
            {

                

                WindowService windowService = WindowService as WindowService;
                windowService.DataService = ClientDataService;
                windowService.ModelActionsStack = ModelActionsStack;
                windowService.MainOperation = MainOperation;
                windowService.CalculatorFunctions = CalculatorFunctions;

                ViewSettings viewSettings = ProjectService.GetViewSettings();

                Model3dFilesInfoView.Init(ClientDataService, CurrentProjectSource, MainOperation);
                Model3dFilesInfoView.FilesChanged += Model3dFilesView_FilesChanged;

                //in ordine di dipendenza

                //Divisioni
                DivisioniView.CalculatorFunctions.Clear();
                DivisioniView.CalculatorFunctions.Add(CalculatorFunctions[NoteCalculatorFunction.Name]);
                DivisioniView.CalculatorFunctions.Add(CalculatorFunctions[Model3dCalculatorFunction.Names.Ifc]);
                DivisioniView.CalculatorFunctions.Add(CalculatorFunctions[DivCalculatorFunction.Name]);
                DivisioniView.DataService = ClientDataService;
                DivisioniView.WindowService = WindowService;
                DivisioniView.ModelActionsStack = ModelActionsStack;
                DivisioniView.MainOperation = MainOperation;
                //DivisioniView.ViewSettings = viewSettings;
                DivisioniView.Init();
                

                //Dati Generali
                DatiGeneraliView.CalculatorFunctions = CalculatorFunctions;
                DatiGeneraliView.DataService = ClientDataService;
                DatiGeneraliView.WindowService = WindowService;
                DatiGeneraliView.ModelActionsStack = ModelActionsStack;
                DatiGeneraliView.MainOperation = MainOperation;
                //DatiGeneraliView.ViewSettings = viewSettings;
                DatiGeneraliView.Init();


                //Contatti
                //ContattiView.CalculatorFunctions.Clear();
                //ContattiView.CalculatorFunctions.Add(CalculatorFunctions[NoteCalculatorFunction.Name]);
                //ContattiView.CalculatorFunctions.Add(CalculatorFunctions[CntCalculatorFunction.Name]);
                //ContattiView.DataService = ClientDataService;
                //ContattiView.WindowService = WindowService;
                //ContattiView.ModelActionsStack = ModelActionsStack;
                //ContattiView.MainOperation = MainOperation;
                //ContattiView.Init(viewSettings.EntityTypes[ContattiItemType.CreateKey()]);



                //Elementi
                ElementiView.CalculatorFunctions.Clear();
                ElementiView.CalculatorFunctions.Add(CalculatorFunctions[NoteCalculatorFunction.Name]);
                ElementiView.CalculatorFunctions.Add(CalculatorFunctions[Model3dCalculatorFunction.Names.Ifc]);
                ElementiView.CalculatorFunctions.Add(CalculatorFunctions[ElmCalculatorFunction.Name]);
                ElementiView.DataService = ClientDataService;
                ElementiView.WindowService = WindowService;
                ElementiView.ModelActionsStack = ModelActionsStack;
                ElementiView.MainOperation = MainOperation;
                ElementiView.Init(viewSettings.EntityTypes[ElementiItemType.CreateKey()]);

                //Prezzario
                //ElencoPrezziView.CalculatorFunctions.Clear();
                ElencoPrezziView.CalculatorFunctions = CalculatorFunctions;
                //ElencoPrezziView.CalculatorFunctions.Add(CalculatorFunctions[NoteCalculatorFunction.Name]);
                //ElencoPrezziView.CalculatorFunctions.Add(CalculatorFunctions[EPCalculatorFunction.Name]);
                ElencoPrezziView.DataService = ClientDataService;
                ElencoPrezziView.WindowService = WindowService;
                ElencoPrezziView.ModelActionsStack = ModelActionsStack;
                ElencoPrezziView.MainOperation = MainOperation;
                //ElencoPrezziView.Init(viewSettings.EntityTypes[PrezzarioItemType.CreateKey()]);
                //ElencoPrezziView.ViewSettings = viewSettings;
                ElencoPrezziView.Init();

                //Computo
                ComputoView.CalculatorFunctions.Clear();
                ComputoView.CalculatorFunctions.Add(CalculatorFunctions[Model3dCalculatorFunction.Names.Ifc]);
                ComputoView.CalculatorFunctions.Add(CalculatorFunctions[NoteCalculatorFunction.Name]);
                ComputoView.CalculatorFunctions.Add(CalculatorFunctions[CmpCalculatorFunction.Name]);
                ComputoView.CalculatorFunctions.Add(CalculatorFunctions[EPCalculatorFunction.Name]);

                ComputoView.DataService = ClientDataService;
                ComputoView.WindowService = WindowService;
                ComputoView.ModelActionsStack = ModelActionsStack;
                ComputoView.MainOperation = MainOperation;
                ComputoView.Init(viewSettings.EntityTypes[ComputoItemType.CreateKey()]);

                //Attivita
                AttivitaView.CalculatorFunctions = CalculatorFunctions;
                AttivitaView.DataService = ClientDataService;
                AttivitaView.WindowService = WindowService;
                AttivitaView.ModelActionsStack = ModelActionsStack;
                AttivitaView.MainOperation = MainOperation;
                //AttivitaView.ViewSettings = viewSettings;
                AttivitaView.Init();

                //FoglioDiCalcolo
                FogliDiCalcoloView.DataService = ClientDataService;
                FogliDiCalcoloView.WindowService = WindowService;
                FogliDiCalcoloView.ModelActionsStack = ModelActionsStack;
                FogliDiCalcoloView.MainOperation = MainOperation;
                FogliDiCalcoloView.GanttView = (GanttView)AttivitaView.WBSView.GanttView;
                FogliDiCalcoloView.Init();

                //Stampe
                StampeView.DataService = ClientDataService;
                StampeView.WindowService = WindowService;
                StampeView.ModelActionsStack = ModelActionsStack;
                StampeView.MainOperation = MainOperation;
                //StampeView.ViewSettings = viewSettings;
                StampeView.GanttView = (GanttView)AttivitaView.WBSView.GanttView;
                StampeView.FogliDiCalcoloView = (FogliDiCalcoloView)FogliDiCalcoloView;
                StampeView.Init();

             

                //Impostatzioni iniziali per le Window di Selezione e Filtro
                windowService.DefaultViewSettings = viewSettings.EntityTypes;



                //Model3dFilesInfoView.Init(ClientDataService, CurrentProjectSource);
                //Model3dFilesInfoView.FilesChanged += Model3dFilesView_FilesChanged;


            }
            catch (Exception e)
            {
                MainAppLog.Error(MethodBase.GetCurrentMethod(), e.Message, e);
            }
        }

        ///// <summary>
        ///// Scopo: Aggiornare il Project appena prima di salvarlo
        ///// </summary>
        //private void UpdateProjectViewSettings()
        //{

        //    ViewSettings viewSettings = ClientDataService.GetViewSettings();

        //    DatiGeneraliView.ContattiView.UpdateViewSettings(viewSettings.EntityTypes[ContattiItemType.CreateKey()]);
        //    DatiGeneraliView.InfoProgettoView.UpdateViewSettings(viewSettings.EntityTypes[ContattiItemType.CreateKey()]);
        //    ElementiView.UpdateViewSettings(viewSettings.EntityTypes[ElementiItemType.CreateKey()]);
        //    ElencoPrezziView.PrezzarioView.UpdateViewSettings(viewSettings.EntityTypes[PrezzarioItemType.CreateKey()]);
        //    ElencoPrezziView.CapitoliView.UpdateViewSettings(viewSettings.EntityTypes[CapitoliItemType.CreateKey()]);
        //    ComputoView.UpdateViewSettings(viewSettings.EntityTypes[ComputoItemType.CreateKey()]);
        //    StampeView.DocumentiView.UpdateViewSettings(viewSettings.EntityTypes[DocumentiItemType.CreateKey()]);
        //    StampeView.ReportView.UpdateViewSettings(viewSettings.EntityTypes[ReportItemType.CreateKey()]);

        //    ClientDataService.SetViewSettings(viewSettings);

        //}

        private void CloseProjectSectionsView()
        {


            ComputoView.Clear();
            //ComputoView.RightPanesView.Clear();
            //ComputoView.RightPanesView.ClosePanes();

            ElencoPrezziView.Clear();
            //PrezzarioView.RightPanesView.Clear();
            //PrezzarioView.RightPanesView.ClosePanes();

            ElementiView.Clear();
            //ElementiView.ElementiItemsView.EntityType.AttributiMasterCodes.Clear();
            //ElementiView.RightPanesView.Clear();
            //ElementiView.RightPanesView.ClosePanes();

            DatiGeneraliView.Clear();

            AttivitaView.Clear();

            //DatiGeneraliView.ContattiView.RightPanesView.Clear();
            //DatiGeneraliView.ContattiView.RightPanesView.ClosePanes();

            //DatiGeneraliView.InfoProgettoView.RightPanesView.Clear();
            //DatiGeneraliView.InfoProgettoView.RightPanesView.ClosePanes();


            StampeView.Clear();

            DivisioniView.Clear();

            Model3dFilesInfoView.Clear();


        }

        private void ModelActionsStack_ActionsChanged(object sender, ActionsChangedEventArgs e)
        {
            IsProjectSavable = true;
            UpdateRecentsUI();
            
        }

        private void ModelActionsStack_ActionPatchCountChanged(object sender, EventArgs e)
        {
            UndoActionsCount = ModelActionsStack.ModelActionPatchsCount;
            UndoToolTip = ModelActionsStack.GetUndoActionsName();
        }

        private void UpdateRecentsUI()
        {
            RaisePropertyChanged(GetPropertyName(() => IsProjectOpened));
            RaisePropertyChanged(GetPropertyName(() => CurrentProjectInfoAsString));
            RaisePropertyChanged(GetPropertyName(() => CurrentFileInfoDetailView));
            RaisePropertyChanged(GetPropertyName(() => IsProjectSavable));
        }




        public ICommand ProjectSaveCommand
        {
            get
            {
                return new CommandHandler(() => this.ProjectSave());
            }
        }

        public ICommand CurrentProjectOpenCommand { get { return new CommandHandler(() => this.CurrentProjectOpen()); }}
        void CurrentProjectOpen()
        {
            //apro il file 
            LastProjectOpenedAsync(CurrentFileInfoDetailView as FileInfoDetailView);
        }

        object _lastProjectOpened = null;
        public object LastProjectOpened
        {
            get => null;
            set
            {
                if (SetProperty(ref _lastProjectOpened, value))
                    LastProjectOpenedAsync(value as FileInfoDetailView);
            }
        }

        object _currentFileInfoDetailView = null;
        public object CurrentFileInfoDetailView
        {
            get
            {
                return _currentFileInfoDetailView;
            }
            set
            {
                SetProperty(ref _currentFileInfoDetailView, value);
            }

        }



        private async void LastProjectOpenedAsync(FileInfoDetailView fileInfoDetailView)
        {
            if (fileInfoDetailView == null)
                return;


            WindowService.ShowWaitCursor(true);
            try
            {

                if (await ProjectClose())
                {

                    var recentProjectInfo = AppSettings.RecentProjects.FirstOrDefault(item =>
                    {
                        string key = ProjectSource.GetKey(item);
                        if (key == fileInfoDetailView.Key)
                            return true;

                        return false;
                    });
                    if (recentProjectInfo != null)
                    {
                        CurrentProjectSource = ProjectSource.Create(recentProjectInfo);

                        //if (fileInfoDetailView.Type == ProjectSourceType.Web)
                        //{
                        //    MessageBarView.Show("Under Construction");
                        //    return;
                        //}

                        //CurrentProjectSource = new ProjectSourceFile(new FileInfo(fileInfoDetailView.Path));

                        CurrentProjectOpenAsync();

                        if (CurrentProjectSource == null)
                            MessageBarView.Show(LocalizationProvider.GetString("ImpossibileAprireIlProgetto"));
                    }

                }
            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message, e);
            }
            finally
            {
                WindowService.ShowWaitCursor(false);
            }
        }

        internal async Task<bool> ProjectSave()
        {
            bool ret = true;

            try
            {
                //PrepareForSave();

                if (CurrentProjectSource == null)
                {

                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.AddExtension = true;
                    saveFileDialog.DefaultExt = ProjectFileExtension;
                    saveFileDialog.Filter = string.Format("{0} files (*.{1})|*.{2}|All files (*.*)|*.*", ProjectFileExtension, ProjectFileExtension, ProjectFileExtension);
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        CurrentProjectSource = new ProjectSourceFile(new FileInfo(saveFileDialog.FileName));
                    }
                    else
                    {
                        return ret;
                    }
                }
                else
                {
                    if (CurrentProjectSource.Type == ProjectSourceType.File)
                    {
                        ////////////////////////////
                        //Backup

                        try
                        {

                            FileInfo backupProjectSourceFile = new FileInfo(CurrentProjectSource.FullName);
                            string fileBackupFullName = string.Format("{0}{1}", backupProjectSourceFile.FullName, ".bak");

                            if (backupProjectSourceFile.Exists)
                            {
                                if (File.Exists(fileBackupFullName))
                                    File.Delete(fileBackupFullName);

                                backupProjectSourceFile.MoveTo(fileBackupFullName);
                            }
                        }
                        catch (Exception ex)
                        {
                            MainAppLog.Error(MethodBase.GetCurrentMethod(), ex.Message, ex);
                        }
                        /////////////////////////////
                    }
                }


                PrepareBeforeSave(CurrentProjectSource);

                bool saveResult = false;


                if (CurrentProjectSource.Type == ProjectSourceType.File)
                {
                    await Task.Run(() =>
                    {
                        ProjectFileWrite(CurrentProjectSource.FullName);
                    });

                    UpdateRecentProjectsPath(CurrentProjectSource);
                    SetThumbnail();
                    SaveAppSettings();
                    LoadRecentsFileInfo();

                    saveResult = true;
                }
                else if (CurrentProjectSource.Type == ProjectSourceType.Web)
                {
                    var prjSourceWeb = CurrentProjectSource as ProjectSourceWeb;

                    AddResponse res = await MainOperation.SaveWebProject(CurrentProject, prjSourceWeb.OperaId, prjSourceWeb.Name);

                    if (res == null || res.Success)
                    {
                        UpdateRecentProjectsPath(CurrentProjectSource);
                        SetThumbnail();
                        SaveAppSettings();
                        LoadRecentsFileInfo();

                        saveResult = true;
                    }
                    else
                    {
                        MessageBarView.Show(res.Message);
                        saveResult = false;
                    }

                }

                if (saveResult)
                {

                    RaisePropertyChanged(GetPropertyName(() => CurrentProjectInfoAsString));
                    RaisePropertyChanged(GetPropertyName(() => CurrentFileInfoDetailView));


                    IsProjectSavable = false;
                    //UpdateRecentsUI();
                    MainView.UpdateMainWindowBar();
                    Model3dFilesInfoView.Init(ClientDataService, CurrentProjectSource, MainOperation);//occorre aggiornare per cambiare il percorso relativo dei file ifc


                    MessageBarView.Ok();

                }

            }
            catch (Exception exc)
            {
                string str = LocalizationProvider.GetString("Errore nel salvataggio del file");
                MessageBox.Show(string.Format("{0}\n {1}", str, exc.Message));
                //MainAppLog.Error(MethodBase.GetCurrentMethod(), string.Format("internal void ProjectSave(): {0}", exc.Message, exc));
            }

            return ret;

        }

        private void ProjectFileWrite(string fullFileName, bool background = false)
        {
            Int32 nVersion = CurrentFileVersion;

            using (var file = File.Create(fullFileName))
            {   
                
                var pStream = new ProgressStream(file);
                if (!background)
                    pStream.BytesWritten += new ProgressStreamReportDelegate(pStream_BytesWritten);

                ModelSerializer.Serialize(pStream, CurrentProject, nVersion);
                    
                //BinaryWriter writer = new BinaryWriter(file);
                //writer.Write(nVersion);



                //if (nVersion == (int)FileVersion.v101)
                //{
                //    Serializer.Serialize(pStream, CurrentProject);
                //}
                //else if (nVersion == (int)FileVersion.v102)
                //{
                //    using (var brotli = new BrotliStream(pStream, CompressionMode.Compress, true))
                //    {
                //        Serializer.Serialize(brotli, CurrentProject);
                //    }
                //}
            }
        }

        //private void ProjectFileSave(string fullFileName)
        //{

        //    ModelData.Model.Project projectData = null;

        //    //Map CurrentProject to projectData
        //    try
        //    {

        //        var config = new MapperConfiguration(cfg => cfg.AddProfile<ProjectMapperProfile>());
        //        var mapper = new Mapper(config);
        //        projectData = mapper.Map<ModelData.Model.Project>(CurrentProject);



        //        using (var file = File.Create(fullFileName))
        //        {
        //            Int32 nVersion = CurrentFileVersion;
        //            BinaryWriter writer = new BinaryWriter(file);
        //            writer.Write(nVersion);

        //            var pStream = new ProgressStream(file);
        //            pStream.BytesWritten += new ProgressStreamReportDelegate(pStream_BytesWritten);

        //            //Serializer.Serialize(file, CurrentProject);
        //            Serializer.Serialize(pStream, projectData);
        //        }  
        //    }
        //    catch (Exception exc)
        //    {
        //        MainAppLog.Error(MethodBase.GetCurrentMethod(), exc.Message);
        //        return;
        //    }
        //}


        /// <summary>
        /// RuntimeTypeModel di protobuf serve per definire le classi derivate da salvare su file
        /// </summary>
        void InitRuntimeTypeModel()//protobuf
        {
            //Entity
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1001, typeof(TreeEntity));
                RuntimeTypeModel.Default[typeof(TreeEntity)].AddSubType(1003, typeof(PrezzarioItem));
                RuntimeTypeModel.Default[typeof(TreeEntity)].AddSubType(1004, typeof(DivisioneItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1002, typeof(ComputoItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1005, typeof(ElementiItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1006, typeof(ContattiItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1007, typeof(InfoProgettoItem));
            RuntimeTypeModel.Default[typeof(TreeEntity)].AddSubType(1008, typeof(CapitoliItem));
            RuntimeTypeModel.Default[typeof(TreeEntity)].AddSubType(1009, typeof(DocumentiItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1010, typeof(ReportItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1011, typeof(StiliItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1012, typeof(ElencoAttivitaItem));
            RuntimeTypeModel.Default[typeof(TreeEntity)].AddSubType(1013, typeof(WBSItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1014, typeof(CalendariItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1015, typeof(VariabiliItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1016, typeof(AllegatiItem));
            RuntimeTypeModel.Default[typeof(Entity)].AddSubType(1017, typeof(TagItem));


            //EntityType        
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1001, typeof(TreeEntityType));
                RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1002, typeof(PrezzarioItemType));
                RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1003, typeof(PrezzarioItemParentType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1004, typeof(ComputoItemType));
                RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1005, typeof(DivisioneItemType));
                RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1006, typeof(DivisioneItemParentType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1007, typeof(ElementiItemType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1008, typeof(ContattiItemType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1009, typeof(InfoProgettoItemType));
            RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1010, typeof(CapitoliItemType));
            RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1011, typeof(CapitoliItemParentType));
            RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1012, typeof(DocumentiItemType));
            RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1013, typeof(DocumentiItemParentType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1014, typeof(ReportItemType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1015, typeof(StiliItemType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1016, typeof(ElencoAttivitaItemType));
            RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1017, typeof(WBSItemType));
            RuntimeTypeModel.Default[typeof(TreeEntityType)].AddSubType(1018, typeof(WBSItemParentType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1019, typeof(CalendariItemType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1020, typeof(VariabiliItemType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1021, typeof(AllegatiItemType));
            RuntimeTypeModel.Default[typeof(EntityType)].AddSubType(1022, typeof(TagItemType));



            //Attributo
            RuntimeTypeModel.Default[typeof(Attributo)].AddSubType(1001, typeof(AttributoRiferimento));
            //RuntimeTypeModel.Default[typeof(Attributo)].AddSubType(1002, typeof(AttributoDivisione));

            //ValoreCollection
            RuntimeTypeModel.Default[typeof(ValoreCollection)].AddSubType(1001, typeof(ValoreTestoCollection));
            RuntimeTypeModel.Default[typeof(ValoreCollection)].AddSubType(1002, typeof(ValoreGuidCollection));

            //ValoreCollectionItem
            RuntimeTypeModel.Default[typeof(ValoreCollectionItem)].AddSubType(1001, typeof(ValoreTestoCollectionItem));
            RuntimeTypeModel.Default[typeof(ValoreCollectionItem)].AddSubType(1002, typeof(ValoreGuidCollectionItem));

            //ValoreCondition
            RuntimeTypeModel.Default[typeof(ValoreCondition)].AddSubType(1001, typeof(ValoreConditionsGroup));
            RuntimeTypeModel.Default[typeof(ValoreCondition)].AddSubType(1002, typeof(AttributoValoreConditionSingle));

            ////FilterDataItem
            //RuntimeTypeModel.Default[typeof(IFilterDataItem)].AddSubType(1001, typeof(FilterDataItemsGroup));
            //RuntimeTypeModel.Default[typeof(IFilterDataItem)].AddSubType(1002, typeof(FilterDataItem));

        }


        public ICommand ProjectSaveAsCommand
        {
            get
            {
                return new CommandHandler(() => this.ProjectSaveAs());
            }
        }

        void PrepareBeforeSave(ProjectSource projectSource)
        {
            //preparo Project per essere salvato
            
            Model3dFilesInfoView.PrepareBeforeSave(projectSource);//Project.Model3dFilesInfo
            ClientDataService.PrepareBeforeSave();//Project.Model3dValuesData
            MainOperation.UpdateProjectViewSettings();//VIEW_SETTINGS
            FogliDiCalcoloView.UpdateFogliDiCalcoloData();//SAVE_FOGLIDICALCOLODATA
        }

        async void ProjectSaveAs()
        {
            //PrepareForSave();

            try
            {

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.AddExtension = true;
                saveFileDialog.DefaultExt = ProjectFileExtension;
                saveFileDialog.Filter = string.Format("{0} files (*.{1})|*.{2}|All files (*.*)|*.*", ProjectFileExtension, ProjectFileExtension, ProjectFileExtension);
                if (saveFileDialog.ShowDialog() == true)
                {
                    /////////////////////////
                    //backup
                    try
                    {
                        if (CurrentProjectSource != null && saveFileDialog.FileName == CurrentProjectSource.FullName)
                        {
                            FileInfo backupProjectSourceFile = new FileInfo(saveFileDialog.FileName);
                            string fileBackupFullName = string.Format("{0}{1}", backupProjectSourceFile.FullName, ".bak");

                            if (backupProjectSourceFile.Exists)
                            {
                                if (File.Exists(fileBackupFullName))
                                    File.Delete(fileBackupFullName);

                                backupProjectSourceFile.MoveTo(fileBackupFullName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MainAppLog.Error(MethodBase.GetCurrentMethod(), ex.Message, ex);
                    }
                    //////////////////////////

                    PrepareBeforeSave(new ProjectSourceFile(new FileInfo(saveFileDialog.FileName)));

                    //await Task.Run(() =>
                    //{
                    //    using (var file = File.Create(saveFileDialog.FileName))
                    //    {
                    //        Int32 nVersion = CurrentFileVersion;
                    //        BinaryWriter writer = new BinaryWriter(file);
                    //        writer.Write(nVersion);

                    //        var pStream = new ProgressStream(file);
                    //        pStream.BytesWritten += new ProgressStreamReportDelegate(pStream_BytesWritten);

                    //        Serializer.Serialize(pStream, CurrentProject);
                    //    }
                    //});

                    await Task.Run(() =>
                    {
                        ProjectFileWrite(saveFileDialog.FileName);
                    });

                    CurrentProjectSource = new ProjectSourceFile(new FileInfo(saveFileDialog.FileName));

                    UpdateRecentProjectsPath(CurrentProjectSource);
                    SetThumbnail();
                    SaveAppSettings();
                    LoadRecentsFileInfo();
                    RaisePropertyChanged(GetPropertyName(() => CurrentProjectInfoAsString));
                    RaisePropertyChanged(GetPropertyName(() => CurrentFileInfoDetailView));


                    IsProjectSavable = false;
                    //SaveIconKey = SaveIconKeyFile;
                    //UpdateRecentsUI();
                    MainView.UpdateMainWindowBar();


                }

                MessageBarView.Ok();

            }
            catch (Exception exc)
            {
                string str = LocalizationProvider.GetString("Errore nel salvataggio del file");
                MessageBox.Show(string.Format("{0}\n {1}", str, exc.Message));
            }

        }

        

        public ICommand ProjectSaveModelCommand
        {
            get
            {
                return new CommandHandler(() => this.ProjectSaveModel());
            }
        }

        void ProjectSaveModel()
        {
            //salva come modello

        }

        public bool IsProjectOpened
        {
            get { return CurrentProject != null; }
        }


        bool _isProjectSavable = false;
        public bool IsProjectSavable
        {
            get
            {
                return _isProjectSavable;
            }
            set
            {
                SetProperty(ref _isProjectSavable, value);
                //_isProjectSavable = value;
                MainView.UpdateMainWindowBar();
            }
        }

        public string SaveIconKey
        { 
            get => (CurrentProjectSource == null) ? ProjectSourceFile.SaveIconKey : CurrentProjectSource.GetSaveIconKey();
            //set
            //{
            //    SetProperty(ref _saveIconKey, value);
            //    MainView.UpdateMainWindowBar();
            //}
        }

        bool _isProjectWebSaveAllowed = false;
        public bool IsProjectWebSaveAllowed
        {
            get
            {
                string msg = null;
                if (LicenseHelper.IsAnyFeaturePresent(new List<LicenseFeature>() { LicenseFeature.Feature_Web }, out msg))
                {
                    return IsProjectOpened;

                }
                return false;
                //return IsProjectOpened;
            }
            set
            {
                SetProperty(ref _isProjectWebSaveAllowed, value);
            }
        }

        public ICommand ProjectWebSaveCommand {get => new CommandHandler(() => this.ProjectWebSave());}
        async void ProjectWebSave()
        {

            string msg = null;
            if (!LicenseHelper.IsAnyFeaturePresent(new List<LicenseFeature>() { LicenseFeature.Feature_Web }, out msg))
            {
                MainOperation.ShowMessageBarView(msg);
                return;
            }


            ModelHelper.ValidateModelData();

            

            var gr = await WebLogin();
            if (!gr.Success)
            {
                MessageBarView.Show(string.Format("{0} {1}", LocalizationProvider.GetString("Autenticazione fallita."), gr.Message));
                return;
            }
            else
            {

                WindowService wndService = WindowService as WindowService;

                wndService.DataService = ClientDataService;
                wndService.MainOperation = MainOperation;

                Guid operaId = Guid.Empty;
                Guid progettoId = Guid.Empty;

                string nomeProgetto = string.Empty;
                if (CurrentProjectSource != null)
                    nomeProgetto = Path.GetFileNameWithoutExtension(CurrentProjectSource.Name);

                if (wndService.WebSaveProjectWnd(out operaId, out progettoId, ref nomeProgetto))
                {
                    wndService.ShowWaitCursor(true);

                    AddResponse res = await MainOperation.SaveWebProject(CurrentProject, operaId, nomeProgetto);



                    if (res != null && !res.Success)
                        MessageBarView.Show(res.Message);
                    else
                    {
                        CurrentProjectSource = new ProjectSourceWeb()
                        {
                            FullName = nomeProgetto,
                            Name = nomeProgetto,
                            OperaId = operaId,
                            ProgettoId = res.NewId,
                        };

                        Model3dFilesInfoView.Init(ClientDataService, CurrentProjectSource, MainOperation);

                        UpdateRecentProjectsPath(CurrentProjectSource);
                        SetThumbnail();
                        SaveAppSettings();
                        LoadRecentsFileInfo();

                        RaisePropertyChanged(GetPropertyName(() => CurrentProjectInfoAsString));
                        RaisePropertyChanged(GetPropertyName(() => CurrentFileInfoDetailView));
                        IsProjectSavable = false;
                        //SaveIconKey = SaveIconKeyWeb;
                        //UpdateRecentsUI();
                        MainView.UpdateMainWindowBar();

                        MessageBarView.Show(LocalizationProvider.GetString("Salvataggio su Web avvenuto correttamente"), true);
                    }
                }
                else
                {

                }

                wndService.ShowWaitCursor(false);
            }
        }



#region Modello 3D

        I3DModelService _IfcModel3dService = null;
        I3DModelService _RevitModel3dService = null;
        Window _BIMViewerWindow = null;
        

        string Model3dViewerSettingsFileName { get => "Model3dViewerSettings.ini"; }

        public ICommand IfcLoadCommand { get { return new CommandHandler(() => this.IfcLoad(false));} }
        public void Model3dLoad()
        {
            
        }

        public ICommand IfcLoadWithPreferencesCommand { get { return new CommandHandler(() => this.IfcLoadWithPreferences()); } }
        public void IfcLoadWithPreferences()
        {
            IfcLoad(true);
        }

        public async void IfcLoad(bool showPreferences)
        {

            try
            {

                Model3dFiles model3DFiles = await Model3dFilesInfoView.GetFilesToProcess(Model3dType.Ifc);
                if (!model3DFiles.Items.Any())
                {
                    string msg = LocalizationProvider.GetString("NessunModello3dDaCaricare");
                    MessageBarView.Show(msg);
                    return;
                }
                model3DFiles.ShowPreferencesBeforeLoad = showPreferences;

                FilesChanged = false;
                RaisePropertyChanged(GetPropertyName(() => FilesChanged));

                //if (_IfcModel3dService == null)
                //    CreateIfcModel3dService();

                if (_IfcModel3dService == null)
                    return;


                if (_BIMViewerWindow == null)
                {
                    _BIMViewerWindow = _IfcModel3dService.Create3DModelWindow();
                    _BIMViewerWindow.Closing += _BIMViewerWindow_Closing;
                    _BIMViewerWindow.Closed += _BIMViewerWindow_Closed;

                    //SetMainVersionWindowBorder(_BIMViewerWindow);

                    _BIMViewerWindow.WindowState = WindowState.Normal;
                    _BIMViewerWindow.Show();
                }
                else
                {
                    if (_BIMViewerWindow.WindowState == WindowState.Minimized)
                        _BIMViewerWindow.WindowState = WindowState.Normal;

                    _BIMViewerWindow.Focus();
                }


                _IfcModel3dService.LoadModels(model3DFiles);
                Model3dFilesInfoView.FilesProcessed = model3DFiles;

                Model3dFilesInfoView.Load();

                //ProjectService.SetModel3dService(Model3dService);
                //ComputoView.Model3dService = Model3dService;
                //ElementiView.Model3dService = Model3dService;
                //AttivitaView.WBSView.Model3dService = Model3dService;
                //FogliDiCalcoloView.Model3dService = Model3dService;

                RaisePropertyChanged(GetPropertyName(() => IsIfcWindowOpened));
            }
            catch (Exception exc)
            {
                MainAppLog.Error(MethodBase.GetCurrentMethod(), exc.Message, exc);
            }

        }

        //public void SetMainVersionWindowBorder(Window wnd)
        //{
        //    //MainVersion OneDrive
        //    //wnd.BorderBrush = Brushes.Orange;
        //    //wnd.BorderThickness = new Thickness(10);
        //}

        public void Model3dSwitch()
        {


            if (IsIfcViewerInitialized)
            {

                if (_BIMViewerWindow == null)
                    IfcLoad(false);
                else
                {
                    if (_BIMViewerWindow.WindowState == WindowState.Minimized)
                        _BIMViewerWindow.WindowState = WindowState.Normal;

                    _BIMViewerWindow.Focus();
                }
            }
            else if (IsReJoInitialized)
            {
                ReJoLoad();

                //attivazione istanza attiva di revit
            }


        }
        public ICommand IfcViewerOptionsCommand { get { return new CommandHandler(() => this.IfcViewerOptions()); } }
        void IfcViewerOptions()
        {
            if (_IfcModel3dService == null)
                CreateIfcModel3dService();

            if (_IfcModel3dService == null)
                return;

            _IfcModel3dService.ShowOptions();
        }

        //public bool AnyRvtFile { get => Model3dFilesInfoView.AnyRvtFile; }
        //public bool AnyIfcFile { get => Model3dFilesInfoView.AnyIfcFile; }


        /// <summary>
        /// Carico dal Project tutti i valori che fanno riferimento al modello 3d cioè ifc(...) precedentemente utilizzati 
        /// Tipicamente all'apertura del progetto
        /// </summary>
        public void LoadCalculatorModel3dValues()
        {
            //Aggiorno il calcolatore locale
            //
            Model3dValuesData model3dValuesData = ClientDataService.GetModel3dValuesData();
            
            //oss: il calcolatore ifc di ProjectService viene aggiornato durante ProjectService.Init()
            ValoreM3dCalculatorFunction ifcCalculatorFunction = CalculatorFunctions[Model3dCalculatorFunction.Names.Ifc] as ValoreM3dCalculatorFunction;
            ifcCalculatorFunction.Model3dService = _IfcModel3dService;
            ifcCalculatorFunction.SetValues(model3dValuesData.Values.Where(item => item.Model3dType == Model3dType.Ifc));

            ValoreM3dCalculatorFunction rvtCalculatorFunction = CalculatorFunctions[Model3dCalculatorFunction.Names.Rvt] as ValoreM3dCalculatorFunction;
            rvtCalculatorFunction.Model3dService = _RevitModel3dService;
            rvtCalculatorFunction.SetValues(model3dValuesData.Values.Where(item => item.Model3dType == Model3dType.Revit));
        }

        /// <summary>
        /// Alla fine del caricamento dei modelli 3d aggiorno tutti i valori a cui si fa riferimento e li tengo in memoria in model3dCalculatorFunction fino al salvataggio del progetto
        /// </summary>
        public void UpdateValuesFromModel3d()
        {
            ////Elimino e aggiorno i valori nel calcolatore in ProjectService e nel Project
            ////ClientDataService.ClearValuesFromModel3d();
            //ClientDataService.UpdateValuesFromModel3d();

            ////Aggiorno i valori nel calcolatore locale
            //Model3dValuesData model3dValuesData = ClientDataService.GetModel3dValuesData();
            //MainAppM3dCalculatorFunction model3dCalculatorFunction = CalculatorFunctions[MainAppM3dCalculatorFunction.Name] as MainAppM3dCalculatorFunction;
            //model3dCalculatorFunction.Model3dService = Model3dService;

            ////elimino i valori precedentemente calcolati 
            ////model3dCalculatorFunction.Clear();
            //model3dCalculatorFunction.SetValues(model3dValuesData);


            CalcolaModel3dAttributiValore();
        }

        public ICommand CalcolaIfcValuesCommand { get { return new CommandHandler(() => this.CalcolaModel3dAttributiValore()); } }
        void CalcolaModel3dAttributiValore()
        {

            if (ClientDataService == null)
                return;

            //Pulisce tutti i valori precedentemente calcolati derivati da model3d

            var model3dType = Model3dService.GetModel3dType();

            //lato client
            if (model3dType == Model3dType.Ifc)
            {
                ValoreM3dCalculatorFunction mainAppM3dCalcFunc = CalculatorFunctions[Model3dCalculatorFunction.Names.Ifc] as ValoreM3dCalculatorFunction;
                mainAppM3dCalcFunc.Clear();
            }
            else if (model3dType == Model3dType.Revit)
            {
                ValoreM3dCalculatorFunction mainAppM3dCalcFunc = CalculatorFunctions[Model3dCalculatorFunction.Names.Rvt] as ValoreM3dCalculatorFunction;
                mainAppM3dCalcFunc.Clear();
            }

            //lato service
            ClientDataService.ClearValuesFromModel3d(model3dType);

            //esteraggo le suddivisioni da aggiornare
            Dictionary<string, EntityType> entTypes = ClientDataService.GetEntityTypes();
            List<string> entTypesKey = entTypes.Values.Where(item =>
            {
                if (item is DivisioneItemType divItemType)
                {
                    if (Model3dHelper.GetModel3dType(divItemType.Model3dClassName) == model3dType)
                        return true;
                }
                return false;
            }).Select(item => item.GetKey()).ToList();

            entTypesKey.Add(BuiltInCodes.EntityType.Elementi);

            //Calcolo le divisioni modello3d e elementi
            foreach (string entTypeKey in entTypesKey)
            {
                EntityType entType = entTypes[entTypeKey];
                ClientDataService.SetEntityType(entType, false, true);
            }
            MainOperation.UpdateEntityTypesView(entTypesKey);
            ClientDataService.UpdateValuesFromModel3d(model3dType, false);


            LoadCalculatorModel3dValues();

            UpdateUI();
                
        }    
        
        public string IfcValuesCount
        {
            get
            { 
                if (CalculatorFunctions.ContainsKey(Model3dCalculatorFunction.Names.Ifc))
                    return (CalculatorFunctions[Model3dCalculatorFunction.Names.Ifc] as ValoreM3dCalculatorFunction).GetValues().Count().ToString();

                return string.Empty;
            }
        }
        private void Model3dFilesView_FilesChanged(object sender, EventArgs e)
        {
            FilesChanged = true;
            RaisePropertyChanged(GetPropertyName(() => FilesChanged));

        }

        bool _filesChanged = false;
        public bool FilesChanged
        {
            get => _filesChanged;
            set
            {
                _filesChanged = value;
                IsProjectSavable = true;
                RaisePropertyChanged(GetPropertyName(() => IsModel3dLoadEnabled));
            }
        }

        public bool IsModel3dLoadEnabled
        {
            get
            {
                if (IsProjectOpened == false)
                    return false;

                return Model3dFilesInfoView.IsLoadEnabled;
            }

            //set => SetProperty(ref _isModel3dLoadEnabled, value);
        }

       

        private void _BIMViewerWindow_Closed(object sender, EventArgs e)
        {
            _BIMViewerWindow = null;


            //_IfcModel3dService = null;
            //ProjectService.SetModel3dService(null);
            //ComputoView.Model3dService = null;
            //ElementiView.Model3dService = null;
            //AttivitaView.WBSView.Model3dService = null;
            //FogliDiCalcoloView.Model3dService = null;

            Model3dFilesInfoView.FilesProcessed = null;


            if (!IsProjectClosing)
                Model3dFilesInfoView.Load();

            RaisePropertyChanged(GetPropertyName(() => IsIfcWindowOpened));
        }

        private void _BIMViewerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsProjectClosing)
                return;
        }

        string _testResult = "";
        public string TestResult
        {
            get => _testResult;
            set { SetProperty(ref _testResult, value); }
        }

        private void CreateIfcModel3dService()
        {
            try
            {
                _IfcModel3dService = XbimWindow.MainAppEntry.Create3DModelService(new MainAppService(MainView));
            }
            catch (Exception exc)
            {
                MainAppLog.Error(MethodBase.GetCurrentMethod(), exc.Message, exc);
            }
        }

        public ICommand IfcViewerInitCommand { get { return new CommandHandler(() => this.IfcViewerInit()); } }
        void IfcViewerInit()
        {

            if (_IfcModel3dService == null)
                CreateIfcModel3dService();

            Model3dService = _IfcModel3dService;
            IsReJoInitialized = false;
            IsIfcViewerInitialized = true;

            ProjectService.SetIfcService(Model3dService);
            ComputoView.Model3dService = Model3dService;
            ElementiView.Model3dService = Model3dService;
            AttivitaView.WBSView.Model3dService = Model3dService;
            FogliDiCalcoloView.Model3dService = Model3dService;

            MainView.UpdateMainWindowBar();
        }

        Model3dType _model3dType = Model3dType.Unknown;
        public bool IsIfcViewerInitialized
        {
            get => _model3dType == Model3dType.Ifc;
            set => SetProperty(ref _model3dType, (value==true)?Model3dType.Ifc: Model3dType.Unknown);
        } 
        
        


        public bool IsIfcWindowOpened { get => _BIMViewerWindow != null; }

        public ICommand ReJoInitCommand{ get { return new CommandHandler(() => this.ReJoInit()); } }
        void ReJoInit()
        {
            string msg = string.Empty;
            if (!LicenseHelper.IsAnyFeaturePresent(new List<LicenseFeature>() { LicenseFeature.Feature_ReJo }, out msg))
            {
                MessageBarView.Show(msg);
                return;
            }



            if (_RevitModel3dService == null)
            {
                //int nAddinAdded = CreateReJoAddins();
                //if (nAddinAdded > 0)
                //{
                //    bool copyOk = CopyReJoAssembly();
                //    string aggiornato = string.Empty;
                //    if (copyOk)
                //        aggiornato = LocalizationProvider.GetString("aggiornato");

                //    msg = string.Format("{0}: {1} {2}", LocalizationProvider.GetString("Revit Addin aggiunti"), nAddinAdded, aggiornato);
                //}

                if (InitReJoAddin() < 0)
                    return;

                CreateRvtModel3dService();
            }

            Model3dService = _RevitModel3dService;


            ProjectService.SetRvtService(Model3dService);
            ComputoView.Model3dService = Model3dService;
            ElementiView.Model3dService = Model3dService;
            AttivitaView.WBSView.Model3dService = Model3dService;
            FogliDiCalcoloView.Model3dService = Model3dService;


            IsIfcViewerInitialized = false;
            IsReJoInitialized = true;

            MainView.UpdateMainWindowBar();

            if (msg.Any())
                MessageBarView.Show(msg);
        }

        private void CreateRvtModel3dService()
        {

            try
            {
                _RevitModel3dService = new RevitService(new MainAppService(MainView));
            }
            catch (Exception exc)
            {
                MainAppLog.Error(MethodBase.GetCurrentMethod(), exc.Message, exc);
            }
        }


        public bool IsReJoInitialized
        {
            get => _model3dType == Model3dType.Revit;
            set => SetProperty(ref _model3dType, (value == true) ? Model3dType.Revit : Model3dType.Unknown);
        }


        private int CreateReJoAddins_old()
        {
            int nAddInAdded = 0;

            try
            {


                Assembly currentAssem = Assembly.GetExecutingAssembly();
                string baseLocation = Path.GetDirectoryName(currentAssem.Location);

                string vendorId = "DCRL";
                string vendorDescription = "Digi Corp Srl, www.digicorp.it";
                string name = "ReJo";

                Guid addInId = new Guid("a082d678-cb5c-4c34-9bb3-77e976775f7f");
                string fullClassName = "ReJo.Application";

                Assembly RevitAddInUtility = Assembly.LoadFrom("Resources\\ReJo\\RevitAddInUtility.dll");
                Type t = RevitAddInUtility.GetType("Autodesk.RevitAddIns.RevitProductUtility");
                MethodInfo method = t.GetMethod("GetAllInstalledRevitProducts", BindingFlags.Public | BindingFlags.Static);
                var revitProducts = (IList) method.Invoke(null, null);

                foreach (var item in revitProducts)
                {
                    var type = item.GetType();
                    var property = type.GetProperty("AllUsersAddInFolder");
                    var addInPath = property.GetValue(item) as string;
                    addInPath = string.Format("{0}\\DigiCorp.ReJo.addin", addInPath);


                    if (File.Exists(addInPath) && IsFirstRun)
                    {
                        File.Delete(addInPath);
                    }


                    if (!File.Exists(addInPath))
                    {
                        string location = string.Format("{0}\\DigiCorp.ReJo.dll", baseLocation);

                        // Creare un'istanza di RevitAddInManifest usando reflection
                        var manifestType = RevitAddInUtility.GetType("Autodesk.RevitAddIns.RevitAddInManifest");
                        var manifest = RevitAddInUtility.CreateInstance("Autodesk.RevitAddIns.RevitAddInManifest");

                        // Creare un'istanza di RevitAddInApplication usando reflection
                        var applicationType = RevitAddInUtility.GetType("Autodesk.RevitAddIns.RevitAddInApplication");
                        var application = RevitAddInUtility.CreateInstance("Autodesk.RevitAddIns.RevitAddInApplication", false, BindingFlags.CreateInstance,null, new object[] { name, location, new Guid("a082d678-cb5c-4c34-9bb3-77e976775f7f"), fullClassName, vendorId }, null, null);

                        // Aggiungere l'applicazione al manifest
                        var addInApplicationsProperty = manifestType.GetProperty("AddInApplications");
                        var addInApplications = addInApplicationsProperty.GetValue(manifest) as System.Collections.IList;


                        //var addInApplication = addInApplications.Cast<object>().FirstOrDefault(app =>
                        //{
                        //    var addInIdProperty = app.GetType().GetProperty("AddInId");
                        //    var existingAddInId = (Guid)addInIdProperty.GetValue(app);
                        //    return existingAddInId == addInId;
                        //});








                        addInApplications.Add(application);

                        // Salvare il manifest
                        var saveAsMethod = manifestType.GetMethod("SaveAs");
                        saveAsMethod.Invoke(manifest, new object[] { addInPath });


                        nAddInAdded++;
                    }


                    //save manifest to a file
                    //var revitProducts = RevitProductUtility.GetAllInstalledRevitProducts();
                    //foreach (RevitProduct product in revitProducts)
                    //{
                    //    string addinPath = string.Format("{0}\\ReJo.addin", product.AllUsersAddInFolder);

                    //    if (File.Exists(addinPath) && IsFirstRun)
                    //    {
                    //        File.Delete(addinPath);
                    //    }


                    //    if (!File.Exists(addinPath))
                    //    {
                    //        string location = string.Empty;
                    //        if (product.Name == "Revit 2025")
                    //            location = string.Format("{0}\\DigiCorp.ReJo.dll", baseLocation);
                    //        else
                    //            continue;

                    //        RevitAddInManifest Manifest = new RevitAddInManifest();

                    //        //create an external application
                    //        RevitAddInApplication application = new RevitAddInApplication(name,
                    //        location, new Guid("a082d678-cb5c-4c34-9bb3-77e976775f7f"), fullClassName, vendorId);


                    //        Manifest.AddInApplications.Add(application);
                    //        Manifest.SaveAs(addinPath);
                    //        nAddInAdded++;
                    //    }




                    //}




                }
            }
            catch (Exception ex)
            {
                MessageBarView.Show(ex.Message);
            }

            return nAddInAdded;
        }

        private int InitReJoAddin()
        {
            int nAddInAdded = 0;

            try
            {

                Assembly currentAssem = Assembly.GetExecutingAssembly();
                string baseLocation = Path.GetDirectoryName(currentAssem.Location);
                string location = string.Format("{0}\\DigiCorp.ReJo.dll", baseLocation);

                string vendorId = "DCRL";
                string vendorDescription = "Digi Corp Srl, www.digicorp.it";
                string name = "ReJo";

                Guid addInId = new Guid("a082d678-cb5c-4c34-9bb3-77e976775f7f");
                string fullClassName = "ReJo.Application";

                Assembly RevitAddInUtility = Assembly.LoadFrom("Resources\\ReJo\\RevitAddInUtility.dll");
                Type t = RevitAddInUtility.GetType("Autodesk.RevitAddIns.RevitProductUtility");
                MethodInfo method = t.GetMethod("GetAllInstalledRevitProducts", BindingFlags.Public | BindingFlags.Static);
                var revitProducts = (IList)method.Invoke(null, null);


                foreach (var revitProduct in revitProducts)
                {
                    var type = revitProduct.GetType();
                    var property = type.GetProperty("AllUsersAddInFolder");
                    var addInPath = property.GetValue(revitProduct) as string;
                    addInPath = string.Format("{0}\\DigiCorp.ReJo.addin", addInPath);

                    // Creare un'istanza di RevitAddInManifest usando reflection
                    var manifestType = RevitAddInUtility.GetType("Autodesk.RevitAddIns.RevitAddInManifest");
                    //var manifest = RevitAddInUtility.CreateInstance("Autodesk.RevitAddIns.RevitAddInManifest");

                    
                    var addInApplicationsProperty = manifestType.GetProperty("AddInApplications");


                    // Ottieni il tipo della classe AddInManifestUtility
                    Type addInManifestUtilityType = RevitAddInUtility.GetType("Autodesk.RevitAddIns.AddInManifestUtility");
                    // Ottieni il metodo GetRevitAddInManifest
                    MethodInfo getRevitAddInManifestMethod = addInManifestUtilityType.GetMethod("GetRevitAddInManifest");


                    object revitAddInManifest = null;
                    
                    if (File.Exists(addInPath))
                    {
                        // Invoca il metodo per ottenere il manifest
                        revitAddInManifest = getRevitAddInManifestMethod.Invoke(null, new object[] { addInPath });
                        IList addInApplications = addInApplicationsProperty.GetValue(revitAddInManifest) as System.Collections.IList;

                        var addInApplication = addInApplications.Cast<object>().FirstOrDefault(app =>
                        {
                            var addInIdProperty = app.GetType().GetProperty("AddInId");
                            var existingAddInId = (Guid)addInIdProperty.GetValue(app);
                            return existingAddInId == addInId;
                        });


                        if (addInApplication != null)
                        {
                            var addInLocationProperty = addInApplication.GetType().GetProperty("Assembly");
                            var addInLocation = addInLocationProperty.GetValue(addInApplication) as string;

                            if (addInLocation != location)
                            {
                                File.Delete(addInPath);
                            }
                        }
                    }
                    //else
                    //{
                    //    revitAddInManifest = RevitAddInUtility.CreateInstance("Autodesk.RevitAddIns.RevitAddInManifest");
                    //    addInApplications = addInApplicationsProperty.GetValue(revitAddInManifest) as System.Collections.IList;
                    //}



                    if (!File.Exists(addInPath))
                    {

                        bool isReJoAssemblyOk = CopyReJoAssembly();
                        if (!isReJoAssemblyOk)
                        {
                            
                            MessageBarView.Show(LocalizationProvider.GetString("ChiudereRevit"));
                            return -1;
                        }

                        revitAddInManifest = RevitAddInUtility.CreateInstance("Autodesk.RevitAddIns.RevitAddInManifest");
                        IList addInApplications = addInApplicationsProperty.GetValue(revitAddInManifest) as System.Collections.IList;

                        // Creare un'istanza di RevitAddInApplication usando reflection
                        var applicationType = RevitAddInUtility.GetType("Autodesk.RevitAddIns.RevitAddInApplication");
                        var application = RevitAddInUtility.CreateInstance("Autodesk.RevitAddIns.RevitAddInApplication", false, BindingFlags.CreateInstance, null, new object[] { name, location, new Guid("a082d678-cb5c-4c34-9bb3-77e976775f7f"), fullClassName, vendorId }, null, null);

                        // Aggiungere l'applicazione al manifest
                        addInApplications.Add(application);

                        // Salvare il manifest
                        var saveAsMethod = manifestType.GetMethod("SaveAs");
                        saveAsMethod.Invoke(revitAddInManifest, new object[] { addInPath });

                        nAddInAdded++;
                    }

                }

                if (nAddInAdded > 0)
                {
                    string msg = string.Format("{0}: {1}", LocalizationProvider.GetString("Revit Addin aggiunti"), nAddInAdded);
                    MessageBarView.Show(msg);
                }
            }
            catch (Exception ex)
            {
                MessageBarView.Show(ex.Message);
            }

            return nAddInAdded;
        }

        bool CopyReJoAssembly()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            var dir = Path.GetDirectoryName(assemblyPath);
            var filePath = string.Format("{0}\\DigiCorp.ReJo.dll", dir);
            var filePath1 = string.Format("{0}\\Resources\\ReJo\\DigiCorp.ReJo.dll", dir);

            FileInfo fileInfo = new FileInfo(filePath);
            FileInfo fileInfo1 = new FileInfo(filePath1);

            if (fileInfo.Exists && fileInfo1.Exists)
            {
                var lastWriteTime = fileInfo.LastWriteTimeUtc;
                var lastWriteTime1 = fileInfo1.LastWriteTimeUtc;

                if (lastWriteTime1 > lastWriteTime)
                {
                    try
                    {
                        File.Copy(filePath1, filePath, true);
                        return true;
                    }
                    catch (Exception exc)
                    {
                    }

                    return false;
                }
                
            }

            return true;
        }

        public ICommand ReJoLoadCommand { get { return new CommandHandler(() => this.ReJoLoad()); } }
        async void ReJoLoad()
        {

            Model3dFiles model3DFiles = await Model3dFilesInfoView.GetFilesToProcess(Model3dType.Revit);

            _RevitModel3dService.LoadModels(model3DFiles);



        }


        #endregion Modello 3D

        #region AppSettings

        private void SaveAppSettings()
        {
            AppSettings.LanguageCode = LanguageHelper.CurrentLanguageCode;

            //limito a 100 i recenti
            AppSettings.RecentProjects = AppSettings.RecentProjects.Take(20).ToList();

            string json = null;
            JsonSerializer.JsonSerialize(AppSettings, out json);

            if (json != null)
            {
                string appSettingsFullFileName = string.Format("{0}{1}", AppSettingsPath, AppSettings.AppSettingsFileName);

                try
                {
                    File.WriteAllText(appSettingsFullFileName, json);
                }
                catch (UnauthorizedAccessException exc)
                {
                    //MainOperation.ShowMessageBarView(exc.Message);
                    MessageBox.Show(exc.Message, LocalizationProvider.GetString("AppName"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
                

            }
        }

        void SetThumbnail()
        {
            if (Model3dService != null)
            {
                RecentProjectInfo rp = AppSettings.RecentProjects.FirstOrDefault(item => ProjectSource.GetKey(item) == CurrentProjectSource.GetKey());
                if (rp != null)
                    rp.Thumbnail = Model3dService.GetThumbnail();
            }
        }

        private void LoadAppSettings()
        {

            //Carico il file AppSettings.ini
            try
            {

                AppSettings appSettings = null;
                string json = null;

                if (AppSettingsPath != null && AppSettingsPath.Any())
                {
                    string appSettingsFullFileName = string.Format("{0}{1}", AppSettingsPath, AppSettings.AppSettingsFileName);
                    if (File.Exists(appSettingsFullFileName))
                        json = System.IO.File.ReadAllText(appSettingsFullFileName);
                }
                else
                {
                    if (File.Exists(AppSettings.AppSettingsFileName))
                        json = System.IO.File.ReadAllText(AppSettings.AppSettingsFileName);

                }

                if (json != null)
                    JsonSerializer.JsonDeserialize<AppSettings>(json, out appSettings, typeof(AppSettings));


                if (appSettings != null)
                    AppSettings = appSettings;

                if (!AppSettings.PrezzariPath.Trim().Any())
                    AppSettings.PrezzariPath = LocalizationProvider.GetString("PrezzariFolder");

                if (!AppSettings.ModelliPath.Trim().Any())
                    AppSettings.ModelliPath = LocalizationProvider.GetString("ModelliFolder");

                ServerAddress.SetCurrentServer(AppSettings.ServerIndex);

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);

            }
        }


        /// <summary>
        /// Percorso file ini
        /// </summary>
        public static void InitAppSettingsPath()
        {
            try
            {

                //var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
                //string appSettingsPath = string.Format("{0}\\{1}\\{2}\\",
                //        GetApplicationDataFolder(),
                //        versionInfo.CompanyName,//Digi Corp
                //        versionInfo.ProductName);//Join

                string appSettingsPath = GetApplicationDataFolder();

                if (!Directory.Exists(appSettingsPath))
                    Directory.CreateDirectory(appSettingsPath);

                AppSettingsPath = appSettingsPath;
                LicenseHelper.LicenseFilePath = appSettingsPath;

            }
            catch (Exception exc)
            {
                
            }
            
        }

        public static string GetApplicationDataFolder()
        {
            if (Environment.MachineName == Join360MachineName)
            {
                //string userName = @"DIGICORP\859.1";

                var myDocumentPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);//\\srvdc01\UserData$\corso.1\Documents
                string appSettingsPath = string.Format("{0}\\Digi Corp\\Join\\", myDocumentPath);

                return appSettingsPath;
            }
            else
            {
                string commonAppDataPath =  System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);//data comune a tutti gli utenti 
                var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
                string appSettingsPath = string.Format("{0}\\{1}\\{2}\\",
                        commonAppDataPath,
                        versionInfo.CompanyName,//Digi Corp
                        versionInfo.ProductName);//Join

                return appSettingsPath;
            }
        }

#endregion AppSettings

#region Recent projects
        private async void LoadRecentsFileInfo()
        {

            GenericResponse gr = null;
            string msg = string.Empty;
            if (LicenseHelper.IsAnyFeaturePresent(new List<LicenseFeature>() { LicenseFeature.Feature_Web }, out msg))
                gr = new GenericResponse(true);
            else
                gr = new GenericResponse(false);

            IEnumerable<OperaDto> opereDto = null;
            IEnumerable<ProgettoDto> progettiDto = null;

            RecentProjectsFileInfo.Clear();


            int count = AppSettings.RecentProjects.Count;

            //foreach (var rp in AppSettings.RecentProjects)
            for (int i = 0; i< count; i++)
            {
                RecentProjectInfo rp = null;
                AppSettings.RecentProjects.TryGetValue(i, out rp);
                if (rp == null)
                    continue;


                if (rp.ProjectSourceType == ProjectSourceType.File)
                {
                    string path = rp.Path;
                    FileInfo fileInfo = new FileInfo(path);
                    FileInfoDetailView fileInfoView = new FileInfoDetailView(this);

                    fileInfoView.Type = ProjectSourceType.File;
                    fileInfoView.FileName = Path.GetFileName(path);
                    if (fileInfo.Exists)
                    {
                        fileInfoView.Size = fileInfo.GetBytesReadable();
                        fileInfoView.CreationDate = fileInfo.CreationTime.ToShortDateString();
                        fileInfoView.LastWriteDate = fileInfo.LastWriteTime.ToShortDateString();
                    }
                    fileInfoView.Path = path;
                    fileInfoView.Key = ProjectSource.GetKey(rp);

                    if (rp.Thumbnail != null)
                    {
                        byte[] bytes = rp.Thumbnail;
                        using (MemoryStream ms = new MemoryStream(bytes))
                        {
                            var decoder = BitmapDecoder.Create(ms,
                                BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

                            fileInfoView.Model3dThumbnail = decoder.Frames[0];
                        }
                    }


                    fileInfoView.UpdateUI();

                    RecentProjectsFileInfo.Add(fileInfoView);
                }
                else if (rp.ProjectSourceType == ProjectSourceType.Web)
                {
                    FileInfoDetailView fileInfoView = new FileInfoDetailView(this);

                    fileInfoView.Key = ProjectSource.GetKey(rp);
                    fileInfoView.FileName = rp.Path;
                    fileInfoView.Type = ProjectSourceType.Web;

                    fileInfoView.Path = string.Empty;

                    ProgettoDto progettoDto = null;

                    //carico le opere per aggiornare i nomi 
                    if (opereDto == null && gr.Success)
                    {

                        try
                        {

                            gr = await UtentiWebClient.RefreshToken();
                            if (gr.Success)
                            {
                                opereDto = await OpereWebClient.GetOpere(gr);
                                //progettoDto = await ProgettiWebClient.GetProgetto(rp.ProgettoId, gr);

                                if (DeveloperVariables.IsUnderConstruction)
                                    progettiDto = await ProgettiWebClient.GetProgetti();
                            }

                        }
                        catch(Exception exc)
                        {
                            gr.Success = false;
                            gr.Message = exc.Message;
                        }

                        if (!gr.Success)
                        {
                            //if (!DeveloperVariables.IsDebug)
                            //    MessageBarView.Show(gr.Message);
                        }
                    }

                    if (DeveloperVariables.IsUnderConstruction)
                    {
                        if (progettiDto != null)
                            progettoDto = progettiDto.FirstOrDefault(item => item.Id == rp.ProgettoId);
                    }
                    else
                    {

                        progettoDto = await ProgettiWebClient.GetProgetto(rp.ProgettoId, gr);
                    }

                    if (!gr.Success)
                        continue;

                    if (opereDto != null)
                    {
                        var operaDto = opereDto.FirstOrDefault(item => item.Id == rp.OperaId);

                        if (operaDto != null && progettoDto != null)
                        {
                            fileInfoView.Path = operaDto.Nome;
                            fileInfoView.CreationDate = progettoDto.ContentCreationDate?.ToShortDateString();
                            fileInfoView.LastWriteDate = progettoDto.ContentLastWriteDate?.ToShortDateString();
                            fileInfoView.Size = Extensions.GetBytesReadable(progettoDto.ContentSize);
                        }
                    }


                    if (rp.Thumbnail != null)
                    {
                        byte[] bytes = rp.Thumbnail;
                        using (MemoryStream ms = new MemoryStream(bytes))
                        {
                            var decoder = BitmapDecoder.Create(ms,
                                BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

                            fileInfoView.Model3dThumbnail = decoder.Frames[0];
                        }
                    }


                    fileInfoView.UpdateUI();

                    RecentProjectsFileInfo.Add(fileInfoView);

                }


            }
            UpdateRecentsUI();

        }

        public bool RecentProjectsListViewIsChecked
        {
            get => RecentProjectsTabControlSelectedIndex == 0;
            set { RecentProjectsTabControlSelectedIndex = value?0:1;}
        }

        int _recentProjectsTabControlSelectedIndex = 1;
        public int RecentProjectsTabControlSelectedIndex
        {
            get => _recentProjectsTabControlSelectedIndex;
            set
            {
                if (SetProperty(ref _recentProjectsTabControlSelectedIndex, value))
                {
                    RaisePropertyChanged(GetPropertyName(() => RecentProjectsListViewIsChecked));
                }
            }
        }


        internal void Remove(FileInfoDetailView fileInfoDetailView)
        {
            //AppSettings.RecentProjectsPath.Remove(fileInfoDetailView.Path);
            //LoadRecentsFileInfo();

            int index = AppSettings.RecentProjects.FindIndex(item => ProjectSource.GetKey(item) == fileInfoDetailView.Key);
            if (index >= 0)
            {
                AppSettings.RecentProjects.RemoveAt(index);
                //RecentProjectsFileInfo.RemoveAt(index);
            }

            var found = RecentProjectsFileInfo.FirstOrDefault(item => item.Key == fileInfoDetailView.Key);
            if (found != null)
            {
                RecentProjectsFileInfo.Remove(found);
            }
        }

        private void RecentProjectsFileInfo_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (FileInfoDetailView item in e.OldItems)
                {
                    AppSettings.RecentProjects.RemoveAll(rp => rp.Path == item.Path);
                }
            }
        }

        #endregion Recent projects


        void CreateLocalFolder(string folderFullName)
        {
            // Specify the directory you want to manipulate.
            string path = folderFullName;//@"c:\MyDir";

            try
            {
                // Determine whether the directory exists.
                if (Directory.Exists(path))
                {
                    //Console.WriteLine("That path exists already.");
                    return;
                }

                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(path);

            }
            catch (Exception e)
            {
                //Console.WriteLine("The process failed: {0}", e.ToString());
            }
            finally { }
        }

        //AU for NET6
        //public string AppVersion
        //{
        //    get
        //    {

        //        if (ApplicationDeployment.IsNetworkDeployed)
        //        {
        //            string strVersion = string.Empty;
        //            Version v = ApplicationDeployment.CurrentDeployment.CurrentVersion;

        //            UpdateCheckInfo updInfo = ApplicationDeployment.CurrentDeployment.CheckForDetailedUpdate(false);
        //            if (updInfo.UpdateAvailable)
        //            {
        //                IsAppVersionAvailable = true;
        //                Version lastVers = updInfo.AvailableVersion;
        //                AppLastVersionAvailable = string.Format("{0}.{1}.{2}.{3}", lastVers.Major, lastVers.Minor, lastVers.Build, lastVers.Revision);
        //            }

        //            string sVersion = string.Format("{0} {1}.{2}.{3}.{4}", LocalizationProvider.GetString("Versione"), v.Major, v.Minor, v.Build, v.Revision);
        //            return sVersion;
        //        }
        //        return LocalizationProvider.GetString("VersioneNonDistribuita");
        //    }
        //}

        string _appVersion = string.Empty;
        public string AppVersion { get => _appVersion; set => SetProperty(ref _appVersion, value); }

        public string ShortDeploymentVersion { get; set; } = MaxAppVersion;

        string _appUpdateLocation = LocalizationProvider.GetString("Versione non distribuita");
        public string AppUpdateLocation { get => _appUpdateLocation; set => SetProperty(ref _appUpdateLocation, value); }

        public string AppDescription
        {
            get
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
                return versionInfo.Comments;
            }
        }


        bool _isAppVersionAvailable = false;
        public bool IsAppVersionAvailable
        {
            get => _isAppVersionAvailable;
            set => SetProperty(ref _isAppVersionAvailable, value);
        }

        string _appLastVersionAvailable = string.Empty;
        public string AppLastVersionAvailable
        {
            get => _appLastVersionAvailable;
            set => SetProperty(ref _appLastVersionAvailable, value);
        }


        //AU for NET6
        //public ICommand UpdateToLastVersionCommand { get => new CommandHandler(() => this.UpdateToLastVersion()); }
        //void UpdateToLastVersion()
        //{
        //    if (ApplicationDeployment.IsNetworkDeployed)
        //    {
        //        ApplicationDeployment.CurrentDeployment.UpdateAsync();
        //    }
        //}

        //private void CurrentDeployment_UpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        //{
        //    MessageBarView.Show(LocalizationProvider.GetString("Per rendere effettive le modifiche riavviare Lapplicazione"));
        //    IsAppVersionAvailable = false;
        //}

        //AU for NET6
        //private void CurrentDeployment_UpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        //{
        //    MessageBarView.Show(LocalizationProvider.GetString("AggiornamentoInCorso"), false, e.ProgressPercentage);
        //}

        public string AppCopyright
        {
            get
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
                return versionInfo.LegalCopyright;
            }
        }

        

        //AU for NET6
        //public string AppUpdateLocation
        //{
        //    get
        //    {
        //        if (ApplicationDeployment.IsNetworkDeployed)
        //        {
        //            string strVersion = string.Empty;

        //            string updateLocation = ApplicationDeployment.CurrentDeployment.UpdateLocation.ToString();

        //            if (!updateLocation.StartsWith("http:"))
        //                return string.Format("{0}: {1}", LocalizationProvider.GetString("VersioneLocale"), Path.GetDirectoryName(updateLocation));

        //            if (updateLocation.Contains("Test"))
        //                return "Versione di Test";
        //        }
        //        return string.Empty;
        //    }
        //}

        public ICommand ChangeLicenseCodeCommand
        {
            get
            {
                return new CommandHandler(() => this.ChangeLicenseCode());
            }
        }
        void ChangeLicenseCode()
        {
            LicenseWnd licenseWnd = new LicenseWnd();
            licenseWnd.SourceInitialized += (x, y) => licenseWnd.HideMinimizeAndMaximizeButtons();
            licenseWnd.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            licenseWnd.Title = LocalizationProvider.GetString("Licenza");
            //licenseWnd.LicenseCode.Text = LicenseHelper.GetLicenseCode();
            licenseWnd.ShowDialog();
            RaisePropertyChanged(GetPropertyName(() => LicenseInfo));


            //con questa istruzione si forza una volta riavviato Join a comportarsi come fosse il primo avvio (appena aggiornato)
            AppSettings.AppVersion = string.Empty;
            
        }

        public string LicenseInfo { get => LicenseHelper.GetLicenseInfo(); }
        public ICommand RemoteAssistanceCommand { get { return new CommandHandler(() => this.RemoteAssistance()); } }
        void RemoteAssistance()
        {
            try
            {
                string appPath = System.AppDomain.CurrentDomain.BaseDirectory;
                string fullFilePath = Path.Combine(appPath, "Resources\\TeamViewer.exe");

                var process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = fullFilePath;
                process.Start();

                //Process.Start(fullFilePath);
            }
            catch (Exception exc)
            {
                MainAppLog.Error(MethodBase.GetCurrentMethod(), exc.Message);
            }
        }

        public ICommand GuideCommand { get { return new CommandHandler(() => this.Guide()); } }
        public void Guide()
        {
            try
            {
                var process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = @"https://join.digicorp.it/guide";
                process.Start();


                //string guideLink = @"https://join.digicorp.it/guide";
                //Process.Start(guideLink);
            }
            catch (Exception exc)
            {
                MainAppLog.Error(MethodBase.GetCurrentMethod(), exc.Message);
            }
        }

        #region Import
        public ICommand ImportModelByFileCommand { get { return new CommandHandler(() => this.ImportModelByFile()); } }

        /// <summary>
        /// Importa il modello (attributi, stampe,...) da un file selezionato
        /// </summary>
        void ImportModelByFile()
        {
            if (!IsProjectOpened)
                return;

            WindowService.ShowWaitCursor(true);

            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.DefaultExt = ProjectFileExtension;
                openFileDialog.Filter = string.Format("{0} files (*.{1})|*.{2}|All files (*.*)|*.*", ProjectFileExtension, ProjectFileExtension, ProjectFileExtension);

                string modelliFolder = MainOperation.GetModelliFolder();

                try
                {
                    if (!Directory.Exists(modelliFolder))
                    {
                        Directory.CreateDirectory(modelliFolder);
                    }
                }
                catch (Exception exc)
                {
                    MainAppLog.Info(System.Reflection.MethodBase.GetCurrentMethod(), exc.Message);
                }
                openFileDialog.InitialDirectory = modelliFolder;

                if (openFileDialog.ShowDialog() == true)
                {
                    ImportModel(openFileDialog.FileName);
                }

            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message, e);
            }
            finally
            {
                WindowService.ShowWaitCursor(false);
            }


        }

        public ICommand ImportModelCommand { get { return new CommandHandler(() => this.ImportModel()); } }

        /// <summary>
        /// Importa il modello (attributi, stampe,...) da un file selezionato
        /// </summary>
        void ImportModel()
        {
            if (!IsProjectOpened)
                return;



            try
            {
                string modelFullFileName = string.Empty;
                if (WindowService.ImportProjectModelWindow(ref modelFullFileName))
                {
                    WindowService.ShowWaitCursor(true);

                    ImportModel(modelFullFileName);
                }

            }
            catch (Exception e)
            {
                MainAppLog.Error(System.Reflection.MethodBase.GetCurrentMethod(), e.Message, e);
            }
            finally
            {
                WindowService.ShowWaitCursor(false);
            }


        }

        void ImportModel(string fullFileName)
        {

            //open modello
            IDataService ds = MainOperation.GetDataServiceByFile(fullFileName, out _);

            string ext = Path.GetExtension(fullFileName).Trim();
            if (ext != string.Format(".{0}", MainOperation.GetProjectFileExtension()))
            {
                MainOperation.ShowMessageBarView(string.Format("{0} .join", LocalizationProvider.GetString("RichiestoFileDiEstensione")));
                return;
            }

            if (ds == null)
            {
                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("VersioneDelFileSuccessiva"));
                return;
            }

            MainOperation.ImportModel(ds);


            ClientDataService.Init();
            InitProjectSectionsView();

        }
        //public ICommand ImportXpweCommand { get { return new CommandHandler(() => this.ImportXpwe()); } }
        //async void ImportXpwe()
        //{
            //if (!await ProjectClose())
            //    return;

            //ImportExportXpwe ieXpwe = new ImportExportXpwe();
            //ieXpwe.MainOperation = MainOperation;
            //ieXpwe.WindowService = WindowService;

            //string modelFullFileName = ieXpwe.GetModelloFullFileName();

            //Int32 modelFileVersion = 0;
            //CurrentProject = MainOperation.CreateProjectByModel(modelFullFileName, out modelFileVersion);

            //if (modelFullFileName == string.Empty)
            //{
            //    CurrentProject = new Project();
            //    modelFileVersion = CurrentFileVersion;
            //}

            //if (!IsProjectOpened)
            //    return;

            //ProjectService = new ProjectService();
            //ProjectService.Init(CurrentProject/*, _repository*/);

            //if (modelFileVersion < CurrentFileVersion)
            //    MessageBarView.Show(LocalizationProvider.GetString("Conversione in corso..."), false, 30);

            //InitProject(modelFileVersion);

            //////////////////////////////
            /////import

            //await ieXpwe.RunImport(ClientDataService);


            //////////////////////////////


            //MessageBarView.Show(LocalizationProvider.GetString("CaricamentoInCorso"), false, 80);

            //InitProjectSectionsView();

            //CurrentProjectSource = null;

            //IsProjectSavable = false;
            //SaveIconKey = SaveIconKeyFile;
            //UpdateRecentsUI();
            //MainView.UpdateMainWindowBar();

            //TabControlSelectedIndex = 1;//dati generali

            //MessageBarView.Ok();
            //UpdateUI();

        //    if (!IsProjectOpened)
        //        return;

        //    ImportExportXpwe ieXpwe = new ImportExportXpwe();
        //    ieXpwe.MainOperation = MainOperation;
        //    ieXpwe.WindowService = WindowService;

        //    await ieXpwe.RunImport(ClientDataService);


        //    MainOperation.UpdateEntityTypesView(new List<string> { BuiltInCodes.EntityType.Prezzario, BuiltInCodes.EntityType.Computo });


        //}

        #endregion

        public bool IsAdvancedMode
        {
            get => (MainOperation != null)? MainOperation.IsAdvancedMode(): false;
        }


        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => IsAdvancedMode));
            RaisePropertyChanged(GetPropertyName(() => IfcValuesCount));
            RaisePropertyChanged(GetPropertyName(() => CurrentLanguageItem));
            RaisePropertyChanged(GetPropertyName(() => ModelliPath));
            RaisePropertyChanged(GetPropertyName(() => PrezzariPath));
            RaisePropertyChanged(GetPropertyName(() => UndoActionsCount));
            RaisePropertyChanged(GetPropertyName(() => AppVersion));
            RaisePropertyChanged(GetPropertyName(() => AppUpdateLocation));
            RaisePropertyChanged(GetPropertyName(() => IsProjectOpened));
            RaisePropertyChanged(GetPropertyName(() => IsProjectWebSaveAllowed));
            RaisePropertyChanged(GetPropertyName(() => SaveIconKey));
            RaisePropertyChanged(GetPropertyName(() => IsModel3dLoadEnabled));
            RaisePropertyChanged(GetPropertyName(() => CacheMaxMegabytes));
            RaisePropertyChanged(GetPropertyName(() => IsAutoSaveActive));
            RaisePropertyChanged(GetPropertyName(() => AutoSaveInterval)); 
            //RaisePropertyChanged(GetPropertyName(() => AnyRvtFile));
            //RaisePropertyChanged(GetPropertyName(() => AnyIfcFile));

            UpdateSvuotaCacheTextAsync();

        }

#region Impostazioni

        public List<LanguageItem> LanguageItems { get; } = LanguageHelper.LanguagesItems;
        
        LanguageItem _currentLanguage = null;
        public LanguageItem CurrentLanguageItem
        {
            get => _currentLanguage;
            set
            {
                if (SetProperty(ref _currentLanguage, value))
                {
                    if (LanguageHelper.CurrentLanguageCode != value.Code)
                    {
                        LanguageHelper.CurrentLanguageCode =  value.Code;
                        MessageBarView.Show(LocalizationProvider.GetString("Per rendere effettive le modifiche riavviare Lapplicazione"));
                    }
                    
                }
            }
        }

        public string ModelliPath
        {
            get
            {
                if (MainOperation != null)
                {
                    string modelliFolder = MainOperation.GetModelliFolder();
                    return modelliFolder;
                }
                return string.Empty;
            }
        }

        public ICommand ModelliPathBrowseCommand { get { return new CommandHandler(() => this.ModelliPathBrowse()); } }
        void ModelliPathBrowse()
        {
            if (AppSettings == null)
                return;

            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = LocalizationProvider.GetString("SelezionaCartellaModelli");
            dialog.SelectedPath = MainOperation.GetModelliFolder();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AppSettings.ModelliPath = dialog.SelectedPath;
                UpdateUI();
            }

        }

        public string PrezzariPath
        {
            get
            {
                if (MainOperation != null)
                {
                    string prezzariFolder = MainOperation.GetPrezzariFolder();
                    return prezzariFolder;
                }
                return string.Empty;
            }
        }

        public ICommand PrezzariPathBrowseCommand { get { return new CommandHandler(() => this.PrezzariPathBrowse()); } }
        void PrezzariPathBrowse()
        {
            if (AppSettings == null)
                return;

            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = LocalizationProvider.GetString("SelezionaCartellaPrezzari");
            dialog.SelectedPath = MainOperation.GetPrezzariFolder();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AppSettings.PrezzariPath = dialog.SelectedPath;
                UpdateUI();
            }

        }

#region Cache
        void InitAppCache()
        {
            CacheManager.FolderPath = string.Format("{0}{1}\\", AppSettingsPath, "Cache");

            if (AppSettings.CacheMaxBytes == 0)
                AppSettings.CacheMaxBytes = 524288000; //500 MB

            CacheManager.MaxBytes = AppSettings.CacheMaxBytes;

            //UpdateUI();
        }

        public ICommand ClearCacheCommand { get { return new CommandHandler(() => this.ClearCache()); } }
        void ClearCache()
        {
            WindowService.ShowWaitCursor(true);

            CacheManager.Clear();
            
            WindowService.ShowWaitCursor(false);

            UpdateSvuotaCacheTextAsync();
        }


        async void UpdateSvuotaCacheTextAsync()
        {
            SvuotaCacheText = LocalizationProvider.GetString("Svuota");

            long currentCacheBytes = await CacheManager.GetCurrentBytesAsync();

            if (currentCacheBytes > CacheManager.MaxBytes)
                currentCacheBytes = CacheManager.MaxBytes;

            string currentCacheSize = Extensions.GetBytesReadable(currentCacheBytes);
            SvuotaCacheText = string.Format("{0} {1}", LocalizationProvider.GetString("Svuota"), currentCacheSize);
        }

        string _svuotaCacheText = string.Empty;
        public string SvuotaCacheText
        {
            get => _svuotaCacheText;
            set => SetProperty(ref _svuotaCacheText, value);
        }

        public string CacheMaxMegabytes
        {
            get
            {
                long mb = AppSettings.CacheMaxBytes / 1048576;
                return string.Format("{0}", mb);
            }
            set
            {
                var regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
                if (!regex.IsMatch(value))
                {
                    long mb = 0;
                    long.TryParse(value, out mb);

                    long cacheMaxBytes = mb * 1048576;
                    AppSettings.CacheMaxBytes = cacheMaxBytes;
                    InitAppCache();
                    UpdateUI();
                }
            }
        } 
#endregion



#endregion Impostazioni

        public void ProjectUndo()
        {

            WindowService.ShowWaitCursor(true);

            ModelAction modelAction = ModelActionsStack.GetLastUndoAction();

            string patchName = ModelActionsStack.GetLastUndoName();

            //Aggiornamento interfaccia dopo l'undo

            if (modelAction != null)
            {
                Debug.WriteLine(string.Format("ProjectUndo entity type: {0}", modelAction.EntityTypeKey));

                Project project = ModelActionsStack.ProjectUndo();
                if (project != null)
                {

                    CurrentProject = project;


                    switch (modelAction.ActionName)
                    {
                        case ActionName.DIVISIONE_ADD:
                        case ActionName.DIVISIONE_RENAME:
                        case ActionName.DIVISIONI_SORT:
                            ClientDataService.ResetCache();
                            this.DivisioniView.Init();
                            break;
                        case ActionName.SETMODEL3D_FILEINFO:
                            Model3dFilesInfoView.Load();
                            break;
                        case ActionName.SETMODEL3D_FILTERDATA:
                        case ActionName.SETMODEL3D_USERVIEWS:
                        case ActionName.SETMODEL3D_TAGSDATA:
                        case ActionName.SETMODEL3D_PREFERENCESDATA:
                        case ActionName.SETMODEL3D_USERROTOTRANSLATION:
                            //

                            break;
                        case ActionName.UNDOGROUP:
                            {
                                //DIVISIONE_REMOVE
                                if (patchName == UndoGroupsName.DeleteDivisione)
                                {
                                    ClientDataService.ResetCache();
                                    this.DivisioniView.Init();
                                }
                                else if (patchName == UndoGroupsName.SetEntityType ||
                                         patchName == UndoGroupsName.Coding)
                                {
                                    ClientDataService.ResetCache();

                                    EntitiesHelper entsHelper = new EntitiesHelper(ClientDataService);
                                    List<string> dependentEntityTypes = entsHelper.GetDependentEntityTypesKey(modelAction.EntityTypeKey);
                                    dependentEntityTypes.Insert(0, modelAction.EntityTypeKey);
                                    MainOperation.UpdateEntityTypesView(dependentEntityTypes);
                                }
                                else if (patchName == UndoGroupsName.AggiornaDataInizioLavori) //gantt 
                                {
                                    //nb: non posso lanciare una action...
                                    ProjectService.CalcolaEntities(BuiltInCodes.EntityType.WBS,
                                        new Model.Calculators.EntityCalcOptions() { CalcolaAttributiResults = true, ResetCalulatedValues = true });

                                    AttivitaView.WBSView.WBSItemsView.UpdateWBSView(WBSViewSyncEnum.OnUpdateWBSItems, null);
                                }
                                else if (patchName == UndoGroupsName.SetTimeRulerToGanttData)//gantt
                                {
                                    AttivitaView.WBSView.WBSItemsView.UpdateWBSView(WBSViewSyncEnum.OnUpdateWBSItems, null);
                                }
                                else if (patchName == UndoGroupsName.SetChartSetting)//gantt
                                {
                                    AttivitaView.WBSView.WBSItemsView.UpdateWBSView(WBSViewSyncEnum.OnUpdateWBSItems, null);
                                    AttivitaView.GanttView.UpdateChartUI();
                                }
                                else if (patchName == UndoGroupsName.ProgrammazioneSAL)//gantt
                                {
                                    AttivitaView.WBSView.WBSItemsView.UpdateWBSView(WBSViewSyncEnum.OnUpdateWBSItems, null);
                                    AttivitaView.GanttView.UpdateChartUI();
                                }
                                else if (patchName == UndoGroupsName.ProgrammaLavori)//foglio di calcolo
                                {
                                    //AttivitaView.WBSView.WBSItemsView.UpdateWBSView(WBSViewSyncEnum.OnUpdateWBSItems, null);
                                    //AttivitaView.GanttView.UpdateUIChartSettings();

                                }
                                else if (patchName == UndoGroupsName.ApplyComputoRules)
                                {
                                    EntitiesHelper entsHelper = new EntitiesHelper(ClientDataService);

                                    //Aggiorno le suddivisioni
                                    var entTypes = ClientDataService.GetEntityTypes();

                                    foreach (var x in entTypes.Values)
                                    //entTypes.Values.ForEach(x =>
                                    {
                                        if (x is DivisioneItemType)
                                        {
                                            var divType = (DivisioneItemType)x;
                                            if (divType.Model3dClassName != Model3dClassEnum.Nothing)
                                            {
                                                MainOperation.UpdateEntityTypesView(new List<string>() { divType.GetKey() });
                                            }
                                        }

                                    };

                                    //Aggiorno elementi e le sezioni che dipendono da elementi
                                    List<string> dependentEntityTypes = entsHelper.GetDependentEntityTypesKey(BuiltInCodes.EntityType.Elementi);
                                    dependentEntityTypes.Insert(0, BuiltInCodes.EntityType.Elementi);
                                    MainOperation.UpdateEntityTypesView(dependentEntityTypes);
                                }

                            }
                            break;
                        default:
                            {

                                if (modelAction.EntityTypeKey == BuiltInCodes.EntityType.WBS)
                                {
                                    AttivitaView.WBSView.WBSItemsView.UpdateWBSView(WBSViewSyncEnum.OnUpdateWBSItems, null);
                                }
                                else if (modelAction.EntityTypeKey != null)
                                {
                                    EntitiesHelper entsHelper = new EntitiesHelper(ClientDataService);
                                    List<string> dependentEntityTypes = entsHelper.GetDependentEntityTypesKey(modelAction.EntityTypeKey);
                                    dependentEntityTypes.Insert(0, modelAction.EntityTypeKey);
                                    MainOperation.UpdateEntityTypesView(dependentEntityTypes);
                                }
                            }
                            break;
                    

                    }


                }
                else
                {
                    Debug.WriteLine("ProjectUndo failed");
                }
            }

            UndoActionsCount = ModelActionsStack.ModelActionPatchsCount;
            UndoToolTip = ModelActionsStack.GetUndoActionsName();
            MainView.UpdateMainWindowBar();

            WindowService.ShowWaitCursor(false);
        }

        int _undoActionsCount = 0;
        public int UndoActionsCount
        {
            get
            {
                return _undoActionsCount;
            }
            set
            {
                if (SetProperty(ref _undoActionsCount, value))
                    MainView.UpdateMainWindowBar();

            }
        }
        string _undoToolTip = string.Empty;
        public string UndoToolTip
        {
            get
            {
                return _undoToolTip;
            }
            set
            {
                if (SetProperty(ref _undoToolTip, value))
                    MainView.UpdateMainWindowBar();
            }
        }

        private bool SetWindowRegistryInfo()
        {

            bool ret = false;

            try
            {

                //////////////////////////////////////////////////////////////////
                //Aggiornamento dell'icona e raccolata informazioni

                RegistryKey myUninstallKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
                string[] mySubKeyNames = myUninstallKey.GetSubKeyNames();
                for (int i = 0; i < mySubKeyNames.Length; i++)
                {
                    RegistryKey myKey = myUninstallKey.OpenSubKey(mySubKeyNames[i], true);
                    object displayName = myKey.GetValue("DisplayName");
                    if (displayName != null && displayName.ToString() == "Join")
                    {
                        string displayIcon = myKey.GetValue("DisplayIcon").ToString();
                        string displayVersion = myKey.GetValue("DisplayVersion").ToString();
                        string urlUpdateInfo = myKey.GetValue("UrlUpdateInfo").ToString();


                        string iconSourcePath = Path.Combine(System.Windows.Forms.Application.StartupPath, "JoinIcon.ico");
                        if (File.Exists(iconSourcePath))
                        {
                            if (displayIcon != iconSourcePath)
                                myKey.SetValue("DisplayIcon", iconSourcePath);
                        }

                        string updateLocation = urlUpdateInfo;

                        if (updateLocation.StartsWith("http:"))
                            updateLocation = string.Format("{0}: {1}", LocalizationProvider.GetString("Origine"), Path.GetDirectoryName(updateLocation));
                        else
                            updateLocation = string.Format("{0}: {1}", LocalizationProvider.GetString("VersioneLocale"), Path.GetDirectoryName(updateLocation));

                        if (updateLocation.Contains("Test"))
                            updateLocation = "Versione di Test";

                        AppUpdateLocation = updateLocation;

                        FileInfo exeInfo = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);

                        AppVersion = string.Format("{0}: {1} {2} {3}", LocalizationProvider.GetString("Versione"), displayVersion, exeInfo.LastWriteTime.ToShortDateString(), exeInfo.LastWriteTime.ToShortTimeString());

                        string[] displayVersionArray = displayVersion.Split('.');
                        ShortDeploymentVersion = string.Format("{0}.{1}", displayVersionArray[0], displayVersionArray[1]);

                        ret = true;
                        break;
                    }

                    myKey.Close();
                }


                //////////////////////////////////////////////////////////////////////////////////////////
                /////Imposto la stringa per l'apertura di Join da file.join
                //"Computer\HKEY_CLASSES_ROOT\Applications\DigiCorp.Join.exe\shell\open\command"

                RegistryKey myApplicationsKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Applications", true);

                RegistryKey myJoinExeKey = (RegistryKey)myApplicationsKey.CreateSubKey("DigiCorp.Join.exe", true);

                RegistryKey myShellKey = (RegistryKey) myJoinExeKey.CreateSubKey("shell", true);

                RegistryKey myOpenExeKey = (RegistryKey) myShellKey.CreateSubKey("open", true);

                RegistryKey myCommandExeKey = (RegistryKey)myOpenExeKey.CreateSubKey("command", true);

                string oldValue = myCommandExeKey.GetValue(null) as string;

                Assembly exeAssembly = Assembly.GetExecutingAssembly();
                string exeLocation = exeAssembly.Location;
                exeLocation = Path.ChangeExtension(exeLocation, "exe");

                string newValue = string.Format("\"{0}\" \"%1\"", exeLocation);

                if (oldValue != newValue)
                    myCommandExeKey.SetValue(null, newValue);

                myCommandExeKey.Close();
                myOpenExeKey.Close();
                myShellKey.Close();
                myJoinExeKey.Close();
                myApplicationsKey.Close();

                //////////////////////////////////////////////////////////////////////


            }
            catch (Exception ex)
            {
                MainAppLog.Error(MethodBase.GetCurrentMethod(), string.Format("SetWindowRegistryInfo {0}", ex.Message));
                return false;
            }

            return ret;

        }


        async Task<GenericResponse> WebLogin()
        {
            GenericResponse gr = new GenericResponse(false);

            gr = await UtentiWebClient.RefreshToken();

            if (gr != null)
            {
                // Brings this app back to the foreground.
                Window mainWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.Name.Contains("JoinWindow"));
                if (mainWindow != null)
                {
                    mainWindow.Activate();
                }
                MainView.UpdateMainWindowBar();
            }

            if (gr.Success)
            {

            }
            else
            {
                MessageBarView.Show(gr.Message);
            }

            return gr;

        }

        //async Task<GenericResponse> WebLogin()
        //{
        //    GenericResponse gr = new GenericResponse(false);

        //    WindowService.ShowWaitCursor(true);

        //    string userPassword = String.Empty;
        //    if (!string.IsNullOrEmpty(AppSettings.UserEncryptedPassword))
        //        userPassword = LicenseHelper.DecryptString(AppSettings.UserEncryptedPassword);

        //    string codiceCliente = LicenseHelper.GetCodiceCliente();


        //    var loginDto = new LoginDto
        //    {
        //        //CodiceCliente = codiceCliente,
        //        Email = AppSettings.UserEmail,
        //        Password = userPassword,
        //        RememberMe = false,
        //    };



        //    //login in base al precedente login
        //    var gr = await AccountWebClient.Login(loginDto);


        //    WindowService.ShowWaitCursor(false);

        //    //login con finestra
        //    if (!gr.Success)
        //    {
        //        show web login dialog
        //    bool res = WindowService.WebLoginWnd(loginDto);

        //        if (res == true)
        //        {
        //            WindowService.ShowWaitCursor(true);
        //            gr = await AccountWebClient.Login(loginDto);
        //            WindowService.ShowWaitCursor(false);

        //            if (gr.Success)
        //            {
        //                if (loginDto.RememberMe)
        //                {
        //                    string userEncryptedPassword = LicenseHelper.EncryptString(loginDto.Password);
        //                    AppSettings.UserEmail = loginDto.Email;
        //                    AppSettings.UserEncryptedPassword = userEncryptedPassword;
        //                }
        //            }
        //            else
        //            {
        //                MessageBarView.Show(gr.Message);
        //            }

        //        }
        //    }


        //    return gr;

        //}

        //private async Task<bool> AccountRegister()
        //{
        //    var registerChiaveLic = new RegisterBeginDto();
        //    registerChiaveLic.ChiaveLicenza = LicenseHelper.GetLicenseCode();

        //    var res = await AccountWebClient.RegisterChiaveLicenza(registerChiaveLic);
        //    if (res.Success)
        //    {

        //        var registerDto = new RegisterDto
        //        {
        //            CodiceCliente = LicenseHelper.GetCodiceCliente(),
        //            Nome = "Alessandro",
        //            Cognome = "Uliana",
        //            Email = "alessandro.uliana@digicorp.it",
        //            Password = "d8!Gssdf7adfg"//password JoinWeb
        //        };


        //        res = await AccountWebClient.Register(registerDto);
        //    }
        //    return res.Success;
        //}

        private async Task<bool> CreaOpera(string nome)
        {
            var operaDto = new OperaCreateDto() { Nome = nome, Descrizione = "XXX" };

            var res = await OpereWebClient.AddOpera(operaDto);

            return res.Success;
        }


#region AutoSave

        public bool IsAutoSaveEnabled { get => true; }


        void InitAutoSave()
        {
            //start autosave timer
            _dispatcherTimer.Tick += OnTick;

            if (AppSettings.AutoSaveInterval > 0)
                IsAutoSaveActive = true;     
            else
                IsAutoSaveActive = false;
        }

        int _autoSaveDefaultMinutes = 30;
        int _autoSaveMinMinutes = 1;


        public bool IsAutoSaveActive
        {
            get => AppSettings.AutoSaveInterval > 0;
            set
            {
                if (value == true)
                {
                    if (AppSettings.AutoSaveInterval < _autoSaveMinMinutes)
                        AppSettings.AutoSaveInterval = _autoSaveDefaultMinutes;

                    _dispatcherTimer.Interval = TimeSpan.FromMinutes(AppSettings.AutoSaveInterval);
                    _dispatcherTimer.Start();
                }
                else
                {
                    _dispatcherTimer.Stop();
                    AppSettings.AutoSaveInterval = 0;
                    
                }
                
                UpdateUI();
            }
        }

        /// <summary>
        /// Salvataggio automatico
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnTick(object sender, EventArgs e)
        {
            if (CurrentProjectSource != null && CurrentProjectSource.Type == ProjectSourceType.File && IsProjectSavable)
            {
                Task.Run(() =>
                {
                    try
                    {
                        ProjectFileWrite(CurrentProjectSource.FullName, true);
                        IsProjectSavable = false;
                        UpdateRecentsUI();
                    }
                    catch (Exception exc)
                    {
                        MainAppLog.Error(MethodBase.GetCurrentMethod(), exc.Message);
                    }
                });
            }
        }




        public int AutoSaveInterval
        {
            get => AppSettings.AutoSaveInterval;
            set
            {
                int minutes = value;
                AppSettings.AutoSaveInterval = minutes;
                IsAutoSaveActive = true;
            }
        } 

#endregion



    }

     
    public class FileInfoDetailView : NotificationBase
    {
        MainMenuView MainMenuView { get; set; }

        public FileInfoDetailView(MainMenuView mainMenuView)
        {
            MainMenuView = mainMenuView;
            
        }

        //[Display(Name = "Nome file")]
        public string FileName { get; set; }

        //[Display(Name = "Dimensione")]
        public string Size { get; set; }

        //[Display(Name = "Data creazione")]
        public string CreationDate { get; set; }

        //[Display(Name = "Data ultima modifica")]
        public string LastWriteDate { get; set; }
        
        public string PathLabel
        {
            get
            {
                if (Type == ProjectSourceType.Web)
                    return LocalizationProvider.GetString("Opera");
                else
                    return LocalizationProvider.GetString("Percorso");
            }
        }
        //[Display(Name = "Percorso")]
        public string Path { get; set; }

        public string Key { get; set; }

        public bool IsWeb { get => Type == ProjectSourceType.Web; }

        public BitmapSource Model3dThumbnail { get; set; }

        public ProjectSourceType Type { get; set; }


        public ICommand RemoveCommand
        {
            get
            {
                return new CommandHandler(() => this.Remove());
            }
        }
        void Remove()
        {
            MainMenuView.Remove(this);

        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => Model3dThumbnail));
            RaisePropertyChanged(GetPropertyName(() => PathLabel));
            RaisePropertyChanged(GetPropertyName(() => IsWeb));

        }

        public string GetInfo()
        {
            string str = string.Format("{0}: {1}\n{2}: {3}\n{4}: {5}\n{6}: {7}\n{8}: {9} ",
                LocalizationProvider.GetString("Nome file"), FileName,
                LocalizationProvider.GetString("Dimensione"), Size,
                LocalizationProvider.GetString("Data creazione"), CreationDate,
                LocalizationProvider.GetString("Data ultima modifica"), LastWriteDate,
                LocalizationProvider.GetString("Percorso"), Path);
            return str;
        }

    }

    public enum SectionEnum
    {
        Progetto = 0,
        DatiGenerali,
        Contatti,
        Divisioni,
        ElencoPrezzi,
        Computo,
        Attivita,
        Elementi,
        FogliDiCalcolo,
        Modello3d,
        Stampe,
        //Archivio,
        //Prezzari,
        //Regole,
        //Filtri,
        //Contatti2,
        //Guida = 15,
        Teleassistenza = 17,
        Info = 18,
        Impostazioni = 19,
    }



    public static class BitmapConversion
    {
        public static BitmapSource BitmapToSource(Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                          source.GetHbitmap(),
                          IntPtr.Zero,
                          Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
        }   
        
        public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }


            return bitmap;
        }

        public static bool ThumbnailCallback()
        {
            return false;
        }

        public static System.Drawing.Image GetThumbnail(BitmapSource bitmapsource)
        {
            System.Drawing.Image.GetThumbnailImageAbort myCallback = new System.Drawing.Image.GetThumbnailImageAbort(ThumbnailCallback);
            Bitmap bitmap = BitmapFromSource(bitmapsource);
            return bitmap.GetThumbnailImage(100, 100, myCallback, IntPtr.Zero);

        }
    }



  
}
