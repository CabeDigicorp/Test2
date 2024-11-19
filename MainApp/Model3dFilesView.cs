using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using _3DModelExchange;
using Model;
using System.Windows.Input;
using Commons;
using System.IO;
using Microsoft.Win32;
using CommonResources;
using WebServiceClient;
using System.Web;
using ModelData.Dto;
using WebServiceClient.Clients;



namespace MainApp
{

    enum Model3dFileInfoStatus
    {
        Nothing=0,
        Loaded=1,
        NotFound=2,
        LoadError=3,
    }


    public class Model3dFilesInfoView : NotificationBase
    {
        ClientDataService DataService { get; set; }
        IMainOperation MainOperation { get; set; }

        //List<string> FileExtensions { get; set; } = new List<string>(){ "ifc", "ifcXml", "ifcZip"};

        HashSet<string> IfcFileExtensions { get; set; } = new HashSet<string>() { "ifc", "ifcXml", "ifcZip" };

        HashSet<string> RvtFileExtensions { get; set; } = new HashSet<string>() { "rvt", "rte" };

        private ObservableCollection<Model3dFileInfoView> _model3dFiles = new ObservableCollection<Model3dFileInfoView>();
        public ObservableCollection<Model3dFileInfoView> Model3dFilesView
        {
            get { return _model3dFiles; }
            set { _model3dFiles = value; }
        }

        


        /// <summary>
        /// File della griglia
        /// </summary>
        Model3dFilesInfo FilesInfo { get; set; } = new Model3dFilesInfo();

        /// <summary>
        /// File passati al visualizzatore del modello 3d
        /// </summary>
        public Model3dFiles FilesProcessed { get; set; } = null;

        /// <summary>
        /// Informazioni sul file di progetto corrente
        /// </summary>
        public ProjectSource CurrentProjectSource { get; set; } = null;

        //public Model3dType Model3dType { get; set; }


        public Model3dFilesInfoView()
        {

            //FileExtensions = IfcFileExtensions.ToList();

            //if (LicenseHelper.IsAnyFeaturePresent(new List<LicenseFeature>() { LicenseFeature.Feature_ReJo }, out _))
            //    FileExtensions.AddRange(RvtFileExtensions.ToList());
        }

        internal void Init(ClientDataService clientDataService, ProjectSource currentProjectSource, IMainOperation mainOperation)
        {
            DataService = clientDataService;
            CurrentProjectSource = currentProjectSource;
            FilesInfo = null;
            MainOperation = mainOperation;
            Load();
        }



        public async void Load(int currentIndex = -1)
        {

            GenericResponse gr = null;
            string msg = string.Empty;
            if (LicenseHelper.IsAnyFeaturePresent(new List<LicenseFeature>() { LicenseFeature.Feature_Web }, out msg))
                gr = new GenericResponse(true);
            else
                gr = new GenericResponse(false);

            IEnumerable<AllegatoDto> allegatiDto = null;




            FilesInfo = DataService.GetModel3dFilesInfo();

            Model3dFilesView.Clear();
            foreach (Model3dFileInfo fileInfo in FilesInfo.Items)
            {
                Model3dFileInfoView fileInfoView = new Model3dFileInfoView(this);

                if (CacheManager.IsJoinApiSource(fileInfo.FilePath))
                {
                    fileInfoView.Path = fileInfo.FilePath;

                    Uri uri = new Uri(fileInfo.FilePath);
                    string operaId = HttpUtility.ParseQueryString(uri.Query).Get("operaId");
                    string uploadId = HttpUtility.ParseQueryString(uri.Query).Get("uploadGuid");


                    //carico le opere per aggiornare i nomi 
                    if (allegatiDto == null && gr.Success)
                    {
                        gr = await UtentiWebClient.RefreshToken();
                        if (gr.Success)
                        {
                            allegatiDto = await AllegatiWebClient.GetAllegati(new Guid(operaId));
                        }

                        if (!gr.Success)
                            MainOperation.ShowMessageBarView(gr.Message);
                    }

                    if (!gr.Success)
                        continue;

                    var allegatoDto = allegatiDto.FirstOrDefault(item => item.Id == new Guid(uploadId));
                    if (allegatoDto != null)
                    {
                        fileInfoView.Name = allegatoDto.FileName;
                        fileInfoView.Note = fileInfo.Note;
                        fileInfoView.IsChecked = fileInfo.IsChecked;
                        fileInfoView.Model3dType = Model3dTypeAsString(GetModel3DType(allegatoDto.FileName));
                    }

                }
                else
                {
                    string fullFilePath = CreateFullFilePath(fileInfo.FilePath);

                    FileInfo f = new FileInfo(fullFilePath);
                    fileInfoView.Name = fileInfo.FileName;//CreateProjectFileName(fileInfo.FilePath); //Path.GetFileName(fileInfo.FilePath);
                    fileInfoView.Path = CreateProjectFilePath(CurrentProjectSource, fileInfo.FilePath);
                    fileInfoView.Note = fileInfo.Note;
                    fileInfoView.IsChecked = fileInfo.IsChecked;

                    //Set status
                    fileInfoView.Status = StatusAsString(Model3dFileInfoStatus.Nothing);

                    fileInfoView.Model3dType = Model3dTypeAsString(GetModel3DType(fileInfo.FileName));

                    if (!f.Exists)
                        fileInfoView.Status = StatusAsString(Model3dFileInfoStatus.NotFound);

                    if (FilesProcessed != null)
                    {
                        Model3dFile fileProcessed = FilesProcessed.Items.FirstOrDefault(item => item.FileFullPath == fullFilePath);
                        if (fileProcessed != null)
                        {
                            if (fileProcessed.Status == Model3dFileStatus.Loaded)
                                fileInfoView.Status = StatusAsString(Model3dFileInfoStatus.Loaded);
                            else if (fileProcessed.Status == Model3dFileStatus.LoadError)
                                fileInfoView.Status = StatusAsString(Model3dFileInfoStatus.LoadError);
                        }
                    }
                }

                Model3dFilesView.Add(fileInfoView);
            }

            if (currentIndex < 0)
                CurrentItem = Model3dFilesView.FirstOrDefault();
            else
                CurrentItem = Model3dFilesView[currentIndex];

            UpdateUI();
        }



        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => CurrentItem));
            RaisePropertyChanged(GetPropertyName(() => IsLoadEnabled));
            //RaisePropertyChanged(GetPropertyName(() => AnyIfcFile));
            //RaisePropertyChanged(GetPropertyName(() => AnyRvtFile));
            //RaisePropertyChanged(GetPropertyName(() => IsViewPreferencesBeforeLoad));
        }

        public void Commit()
        {
            if (Validate())
            {
                DataService.SetModel3dFilesInfo(FilesInfo);
                OnFilesChanged(new EventArgs());
            }
            else
            {
                MainOperation.ShowMessageBarView(LocalizationProvider.GetString("Non è consentito caricare files con lo stesso nome"));
                FilesInfo = null;
            }
        }

        public bool Validate()
        {
            var duplicatedFilesName = FilesInfo.Items.GroupBy(item =>
            {
                if (Path.IsPathRooted(item.FilePath))
                    return CreateProjectFileName(item.FilePath);
                else
                    return item.FilePath;
            }).Where(group => group.Count()>1).Select(group => group.Key);
            
            if (duplicatedFilesName.Any())
                return false;

            return true;
        }

        public event EventHandler FilesChanged;
        protected virtual void OnFilesChanged(EventArgs e)
        {
            FilesChanged?.Invoke(this, e);
        }

        Model3dFileInfoView _currentItem = null;
        public object CurrentItem
        {
            get
            {
                return _currentItem;
            }
            set
            {
                _currentItem = value as Model3dFileInfoView;
            }
        }

        int CurrentRowIndex { get => Model3dFilesView.IndexOf(_currentItem); }


        static string StatusAsString(Model3dFileInfoStatus fileStatus)
        {
            switch (fileStatus)
            {
                case Model3dFileInfoStatus.Nothing:
                    return "";
                case Model3dFileInfoStatus.NotFound:
                    return LocalizationProvider.GetString("Non trovato");
                //case Model3dFileInfoStatus.Loaded:
                //    return LocalizationProvider.GetString("Caricato");
                case Model3dFileInfoStatus.LoadError:
                    return LocalizationProvider.GetString("Errore al caricamento");

            }
            return "";
        }

        static string Model3dTypeAsString(Model3dType type)
        {
            switch (type)
            {
                case Model3dType.Ifc:
                    return "\ue0FF";
                case Model3dType.Revit:
                    return "\ue140";
            }
            return "";
        }

        public string CreateFullFilePath(string filePath)
        {
            string fullFilePath = filePath;
            if (!Path.IsPathRooted(fullFilePath))
            {
                if (CurrentProjectSource != null)
                {
                    if (CurrentProjectSource is ProjectSourceFile)
                    {
                        var projectSource = (ProjectSourceFile)CurrentProjectSource;
                        fullFilePath = string.Format("{0}\\{1}", projectSource.DirectoryName, filePath);
                    }
                    //else if (CurrentProjectSource.Type == ProjectSourceType.Web)
                    //    fullFilePath = string.Format("{0}\\{1}", "Web:\\", filePath);
                }
                else
                {
                    fullFilePath = string.Format("{0}\\{1}", "C:\\", filePath);
                }

            }

            return fullFilePath;

        }
        static string CreateProjectFilePath(ProjectSource projectSource, string fullFilePath)
        {
            string fileName = fullFilePath;
            if (projectSource != null)
            {
                if (Path.IsPathFullyQualified(fileName))
                {
                    Uri uri = new Uri(fullFilePath);
                    if (uri.IsFile)
                    {
                        var projSourceFile = projectSource as ProjectSourceFile;
                        if (projSourceFile != null && fileName.StartsWith(projSourceFile.DirectoryName))
                        {
                            int l = projSourceFile.DirectoryName.Length + 1;
                            fileName = fullFilePath.Substring(l, fullFilePath.Length - l);
                        }
                    }
                    //else
                    //{
                    //    fileName = string.Format("[{0}]", LocalizationProvider.GetString("Web"));
                    //}
                }
            }

            //string fileName = fullFilePath;
            //if (projectFileInfo != null && fileName.StartsWith(projectFileInfo.DirectoryName))
            //{
            //    int l = projectFileInfo.DirectoryName.Length + 1;
            //    fileName = fullFilePath.Substring(l, fullFilePath.Length - l);
            //}
            return fileName;
        }

        private string CreateProjectFileName(string fullFilePath)
        {
            string fileName = fullFilePath;

            if (Path.IsPathFullyQualified(fileName))
            {

                Uri uri = new Uri(fullFilePath);
                if (uri.IsFile)
                {
                    fileName = Path.GetFileName(fullFilePath);
                }
                else
                {
                    fileName = HttpUtility.ParseQueryString(uri.Query).Get("filename");
                }
            }
            else if (CacheManager.IsJoinApiSource(fullFilePath))
            {
                //web
                Uri uri = new Uri(fileName);
                fileName = HttpUtility.ParseQueryString(uri.Query).Get("fileName");
            }



            return fileName;

        }

        //string CreateProjectFilePath(string fullFilePath)
        //{
        //    string fileName = fullFilePath;
        //    if (CurrentProjectFileInfo != null && fileName.StartsWith(CurrentProjectFileInfo.DirectoryName))
        //    {
        //        int l = CurrentProjectFileInfo.DirectoryName.Length + 1;
        //        fileName = fullFilePath.Substring(l, fullFilePath.Length - l);
        //    }
        //    return fileName;
        //}

        public ICommand AddFilePathCommand { get { return new CommandHandler(() => this.AddFilePath()); } }
        void AddFilePath()
        {
            List<string> fileExtension = new List<string>();
            if (MainOperation.GetModel3dType() == Model3dType.Ifc)
                fileExtension = IfcFileExtensions.ToList();
            else if (MainOperation.GetModel3dType() == Model3dType.Revit)
                fileExtension = RvtFileExtensions.ToList();


            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.DefaultExt = fileExtension[0];

            if (CurrentProjectSource is ProjectSourceFile)
            {
                var projectSourceFile = CurrentProjectSource as ProjectSourceFile;

                if (CurrentProjectSource != null)
                    openFileDialog.InitialDirectory = projectSourceFile.DirectoryName;
            }

            

            IEnumerable<string> filesfilters = fileExtension.Select(item => string.Format("{0} files (*.{1})|*.{2}", item, item, item));
            string filesFilter = string.Format("{0}|All files (*.*)|*.*", string.Join("|", filesfilters));
            openFileDialog.Filter = filesFilter;

            //openFileDialog.Filter = string.Format("{0} files (*.{1})|*.{2}|All files (*.*)|*.*", FileExtension, FileExtension, FileExtension);
            if (openFileDialog.ShowDialog() == true)
            {

                foreach (string f in openFileDialog.FileNames)
                {
                    string ext = Path.GetExtension(f).Substring(1);
                    
                    if (!fileExtension.Contains(ext))
                        continue;
                    
                    string fileName = CreateProjectFilePath(CurrentProjectSource, f);

                    Model3dFileInfo fileInfo = new Model3dFileInfo();
                    fileInfo.FilePath = fileName;
                    fileInfo.FileName = CreateProjectFileName(f);
                    fileInfo.IsChecked = true;
                    //fileInfo.FileId = Guid.Empty;
                    FilesInfo.Items.Add(fileInfo);
                }


                Commit();
                Load();
            }


        }

        public ICommand RemoveFilePathCommand { get { return new CommandHandler(() => this.RemoveFilePath()); } }
        void RemoveFilePath()
        {
            if (CurrentRowIndex < 0)
                return;

            int currentRowIndex = CurrentRowIndex;

            FilesInfo.Items.RemoveAt(currentRowIndex);

            Commit();
            Load(currentRowIndex - 1);
        }

        public ICommand EditFilePathCommand { get { return new CommandHandler(() => this.EditFilePath()); } }
        void EditFilePath()
        {
            if (CurrentRowIndex < 0)
                return;

            List<string> fileExtension = new List<string>();
            if (MainOperation.GetModel3dType() == Model3dType.Ifc)
                fileExtension = IfcFileExtensions.ToList();
            else if (MainOperation.GetModel3dType() == Model3dType.Revit)
                fileExtension = RvtFileExtensions.ToList();


            Model3dFileInfo fileInfo = FilesInfo.Items[CurrentRowIndex];
            string path = Path.GetDirectoryName(fileInfo.FilePath);

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.DefaultExt = fileExtension[0];

            IEnumerable<string> filesfilters = fileExtension.Select(item => string.Format("{0} files (*.{1})|*.{2}", item, item, item));
            string filesFilter = string.Format("{0}|All files (*.*)|*.*", string.Join("|", filesfilters));
            openFileDialog.Filter = filesFilter;

            if (Directory.Exists(path))
                openFileDialog.InitialDirectory = path;

            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = CreateProjectFilePath(CurrentProjectSource, openFileDialog.FileName);
                fileInfo.FilePath = fileName;
                fileInfo.FileName = CreateProjectFileName(openFileDialog.FileName);
                fileInfo.IsChecked = true;
                //fileInfo.FileId = Guid.Empty;

                Commit();
                Load();
            }

        }


        //public bool IsViewPreferencesBeforeLoad
        //{
        //    get => FilesInfo.ViewPreferencesBeforeLoad;
        //    set
        //    {
        //        FilesInfo.ViewPreferencesBeforeLoad = value;
        //        Commit();
        //    }
        //}


        public bool IsLoadEnabled
        {
            get
            {
                return FilesInfo.Items.Any();
            }
        }

      

        public void OnPropertyChanged(Model3dFileInfoView fileInfoView)
        {
            int index = Model3dFilesView.IndexOf(fileInfoView);
            if (index < 0)
                return;

            FilesInfo.Items[index].FilePath = fileInfoView.Path;
            FilesInfo.Items[index].Note = fileInfoView.Note;
            FilesInfo.Items[index].FileName = CreateProjectFileName(fileInfoView.Path);
            FilesInfo.Items[index].IsChecked = fileInfoView.IsChecked;



            Commit();
            Load(index);
        }

        public async Task<Model3dFiles> GetFilesToProcess(Model3dType model3dType)
        {
            //Esempi di FilePath salvati
            //FilePath: "Condominio 2022.ifc"
            //FilePath: "D:\Test\ifc\Condominio 2022.ifc"
            //FilePath: "https:/servizio.digicorp.it/Test/Condominio 2022.ifc"

            Model3dFiles model3DFiles = new Model3dFiles();
            foreach (Model3dFileInfo fi in FilesInfo.Items)
            {
                if (!fi.IsChecked)
                    continue;

                string fileName = fi.FileName;

                if (fileName != null)
                {
                    var type = GetModel3DType(fi.FileName);

                    if (model3dType != type)
                        continue;
                }

                string fullFilePath = null;
                if (CacheManager.IsJoinApiSource(fi.FilePath))
                    fullFilePath = fi.FilePath;
                else
                    fullFilePath = CreateFullFilePath(fi.FilePath);

                FileInfo fileInfo = new FileInfo(fullFilePath);

                if (fileInfo.Exists)
                    model3DFiles.Items.Add(new Model3dFile() { FileFullPath = fullFilePath, Status = Model3dFileStatus.Nothing });
                else if (!Path.IsPathFullyQualified(fullFilePath))
                {
                    //cerca di scaricarlo nella cache se è un percorso web

                    string cacheFilePath = await CacheManager.Download(/*fi.FileId, */fi.FilePath);

                    if (File.Exists(cacheFilePath))
                        model3DFiles.Items.Add(new Model3dFile() { FileFullPath = cacheFilePath, Status = Model3dFileStatus.Nothing });
                }
            }
            return model3DFiles;

        }

        
        /// <summary>
        /// Vale solo per file locali
        /// </summary>
        /// <param name="model3dType"></param>
        /// <returns></returns>
        //public Model3dFiles GetLocalFilesToProcess(Model3dType model3dType)
        //{

        //    Model3dFiles model3DFiles = new Model3dFiles();
        //    foreach (Model3dFileInfo fi in FilesInfo.Items)
        //    {
        //        if (!fi.IsChecked)
        //            continue;

        //        string fileName = fi.FileName;

        //        if (fileName != null)
        //        {
        //            var type = GetModel3DType(fi.FileName);

        //            if (model3dType != type)
        //                continue;

        //            string fullFilePath = null;
        //            if (CacheManager.IsJoinApiSource(fi.FilePath))
        //                fullFilePath = fi.FilePath;
        //            else
        //                fullFilePath = CreateFullFilePath(fi.FilePath);

        //            FileInfo fileInfo = new FileInfo(fullFilePath);

        //            if (fileInfo.Exists)
        //                model3DFiles.Items.Add(new Model3dFile() { FileFullPath = fullFilePath, Status = Model3dFileStatus.Nothing });
        //        }

        //    }

        //    return model3DFiles;
        //}

        Model3dType GetModel3DType(string filename)
        {

            string ext = Path.GetExtension(filename)?.Substring(1);

            if (IfcFileExtensions.Contains(ext))
                return Model3dType.Ifc;

            if (RvtFileExtensions.Contains(ext))
                return Model3dType.Revit;

            return Model3dType.Unknown;

        }

        public void PrepareBeforeSave(ProjectSource projectSource)
        {
            if (FilesInfo == null)
                FilesInfo = DataService.GetModel3dFilesInfo();

            foreach (Model3dFileInfo model3dFileInfo in FilesInfo.Items)
            {
                model3dFileInfo.FilePath = CreateProjectFilePath(projectSource, model3dFileInfo.FilePath);
                //model3dFileInfo.FileName = CreateProjectFileName(model3dFileInfo.FilePath);
            }
            Commit();
        }

        public void Clear()
        {
            if (FilesInfo == null)
                return;

            FilesInfo.Items.Clear();
            _model3dFiles.Clear();
        }




        //public bool AnyRvtFile
        //{
        //    get
        //    {
        //        if (FilesInfo == null)
        //            FilesInfo = DataService.GetModel3dFilesInfo();

        //        foreach (Model3dFileInfo model3dFileInfo in FilesInfo.Items)
        //        {
        //            string fileName = model3dFileInfo.FileName;

        //            if (fileName != null)
        //            {
        //                string ext = Path.GetExtension(model3dFileInfo.FileName).Substring(1);
        //                if (RvtFileExtensions.Contains(ext))
        //                    return true;
        //            }

        //        }
        //        return false;
        //    }
        //}

        //public bool AnyIfcFile
        //{
        //    get
        //    {
        //        if (FilesInfo == null)
        //            FilesInfo = DataService.GetModel3dFilesInfo();

        //        foreach (Model3dFileInfo model3dFileInfo in FilesInfo.Items)
        //        {

        //            string fileName = model3dFileInfo.FileName;

        //            if (fileName != null)
        //            {
        //                string ext = Path.GetExtension(fileName).Substring(1);
        //                if (IfcFileExtensions.Contains(ext))
        //                    return true;
        //            }
        //        }
        //        return false;
        //    }
        //}

    }

    //[System.Reflection.ObfuscationAttribute(Feature = "renaming", ApplyToMembers = false)]
    [System.Reflection.ObfuscationAttribute(Feature = "properties renaming")]
    public class Model3dFileInfoView
    {
        Model3dFilesInfoView _owner = null;
        public Model3dFileInfoView(Model3dFilesInfoView owner)
        {
            _owner = owner;
        }

        bool _isChecked = false;
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
                _owner.OnPropertyChanged(this);
            }
        }

        public string Model3dType { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        string _path = "";
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                _owner.OnPropertyChanged(this);
            }
        }


        string _note = "";
        public string Note
        {
            get
            {
                return _note;
            }
            set
            {
                _note = value;
                _owner.OnPropertyChanged(this);
            }
        }
        public string Status { get; set; } = string.Empty;


    }

}
