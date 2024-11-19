
using System;
using MasterDetailModel;
using Commons;
using _3DModelExchange;
using Model;
using System.Windows.Input;
using CommonResources;
using System.Threading.Tasks;
using System.Windows.Media;
using WebServiceClient.Clients;
using DevExpress.XtraSpreadsheet.Commands.Internal;
using WebServiceClient;
using System.Collections.Generic;

namespace MainApp
{
    public class MainView : NotificationBase
    {
        public MainMenuView MainMenuView { get; internal set; }

        string _title = "";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public event EventHandler WindowActivated;

        public void UpdateMainWindowBar()
        {
            UpdateTitle();
            UpdateUI();

        }

        public void UpdateTitle()
        {
            //string deploymentVersion = App.ShortDeploymentVersion;
            string deploymentVersion = MainMenuView.ShortDeploymentVersion;

            if (deploymentVersion == MainMenuView.MaxAppVersion)
                deploymentVersion = string.Empty;

            if (MainMenuView.IsProjectOpened)
            {
                if (MainMenuView.CurrentProjectSource == null)
                    Title = string.Format("{0} - {1} {2}", LocalizationProvider.GetString("SenzaNome"), LocalizationProvider.GetString("AppName"), deploymentVersion);
                else
                    Title = string.Format("{0} - {1} {2}", MainMenuView.CurrentProjectSource.Name, LocalizationProvider.GetString("AppName"), deploymentVersion);
            }
            else
                Title = string.Format("{0} {1}", LocalizationProvider.GetString("AppName"), deploymentVersion);
        }


        internal async void Init()
        {
            if (await MainMenuView.Init())
                UpdateMainWindowBar();
        }

        internal async Task<bool> OnClosing()
        {
            return await MainMenuView.OnClosing();
        }
        public ICommand ProjectSaveCommand
        {
            get
            {
                return new CommandHandler(() => this.ProjectSave());
            }
        }

        void ProjectSave()
        {
            MainMenuView.ProjectSave();
        }

        public bool IsProjectSavable
        {
            get { return (MainMenuView == null) ? false : MainMenuView.IsProjectSavable; }
            set { MainMenuView.IsProjectSavable = value; }
        }

        public string SaveIconKey
        {
            get { return (MainMenuView == null || MainMenuView.CurrentProjectSource == null) ? ProjectSourceFile.SaveIconKey : MainMenuView.CurrentProjectSource.GetSaveIconKey(); }
        }


        public ICommand ProjectModel3dLoadCommand
        {
            get
            {
                return new CommandHandler(() => this.ProjectModel3dLoad());
            }
        }
        void ProjectModel3dLoad()
        {
            MainMenuView.Model3dSwitch();

        }


        //public bool IsProjectModel3dLoadEnabled
        //{
        //    get
        //    {
        //        if (MainMenuView != null)
        //        {
        //            if (MainMenuView.IsIfcViewerInitialized)
        //            {
        //                return MainMenuView.IsModel3dLoadEnabled;
        //            }
        //            else if (MainMenuView.IsReJoInitialized)
        //            {
        //                return MainMenuView.IsModel3dLoadEnabled;
        //            }
        //        }
        //        return false;
        //    }
        //}

        public bool IsProjectModel3dLoadEnabled
        {
            get
            {
                if (MainMenuView != null)
                {
                    if (MainMenuView.IsIfcViewerInitialized)
                    {
                        return MainMenuView.IsModel3dLoadEnabled;
                    }
                    else if (MainMenuView.IsReJoInitialized)
                    {
                        return MainMenuView.IsModel3dLoadEnabled;
                    }
                }

                return false;
            }
        }
        public string ProjectModel3dLoadIcon
        {
            get
            {
                

                if (MainMenuView != null && MainMenuView.IsReJoInitialized)
                {
                    return "\ue141";
                }

                return "\ue098";
            }
        
        
        }

        

        bool _isMaximized = false;
        public bool IsMaximized
        {
            get { return _isMaximized; }
            set
            {
                SetProperty(ref _isMaximized, value);
            }
        }

        protected void OnWindowActivated(EventArgs e)
        {
            WindowActivated?.Invoke(this, e);
        }

        public void WindowActivate()
        {
            OnWindowActivated(new EventArgs());
        }


        public ICommand ProjectUndoCommand { get { return new CommandHandler(() => this.ProjectUndo()); } }
        void ProjectUndo()
        {
            MainMenuView.ProjectUndo();
        }

        public bool AnyToUndo
        {
            get { return (MainMenuView == null) ? false : MainMenuView.UndoActionsCount > 0; }
        }

        public string UndoText
        {
            get
            {
                if (MainMenuView == null)
                    return "U";
                else
                    return string.Format("U{0}", MainMenuView.UndoActionsCount.ToString());
            }
        }

        public string UndoToolTip
        {
            get
            {
                if (MainMenuView == null)
                    return string.Empty;
                else
                    return string.Format("{0}", MainMenuView.UndoToolTip);
            }
        }


        public bool IsUndoActive
        {
            get
            {
                return DeveloperVariables.IsUndoActive;
            }
        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => IsProjectSavable));
            RaisePropertyChanged(GetPropertyName(() => IsProjectModel3dLoadEnabled));
            RaisePropertyChanged(GetPropertyName(() => AnyToUndo));
            RaisePropertyChanged(GetPropertyName(() => UndoText));
            RaisePropertyChanged(GetPropertyName(() => UndoToolTip));
            RaisePropertyChanged(GetPropertyName(() => SaveIconKey));
            RaisePropertyChanged(GetPropertyName(() => UserEmail));
            RaisePropertyChanged(GetPropertyName(() => IsLogged));
            RaisePropertyChanged(GetPropertyName(() => UserIcon));
            RaisePropertyChanged(GetPropertyName(() => UserName));
            RaisePropertyChanged(GetPropertyName(() => UserFullName));
            RaisePropertyChanged(GetPropertyName(() => UserProfileWebUILink));
            RaisePropertyChanged(GetPropertyName(() => ProjectModel3dLoadIcon));
        }


        #region UserInfo

        public ICommand WebUserInfoCommand { get { return new CommandHandler(() => this.UserInfo()); } }
        void UserInfo()
        {
            if (IsLogged)
            {
                IsUserInfoPopupOpen = false;
                IsUserInfoPopupOpen = true;
            }
            else
            {
                Login();
            }

        }

        bool _isUserInfoPopupOpen = false;
        public bool IsUserInfoPopupOpen {get => _isUserInfoPopupOpen; set => SetProperty(ref _isUserInfoPopupOpen, value); }

        public ICommand LoginCommand { get { return new CommandHandler(() => this.Login()); } }
        async void Login()
        {
            GenericResponse gr = null;
            gr = await UtentiWebClient.Login();
            if (gr != null)
            {
                UpdateMainWindowBar();
                IsUserInfoPopupOpen = false;
            }
        }

        public ICommand LogoutCommand { get { return new CommandHandler(() => this.Logout()); } }
        async void Logout()
        {
            GenericResponse gr = null;
            gr = await UtentiWebClient.Logout();
            if (gr != null)
            {
                UpdateMainWindowBar();
                IsUserInfoPopupOpen = false;
            }
        }

        public string UserEmail
        { get => UtentiWebClient.GetCurrentUserEmail(); }

        public string UserName
        { get => UtentiWebClient.GetCurrentUserName(); }

        public string UserFullName
        { get => string.Format("{0} {1}", UtentiWebClient.GetCurrentUserName(), UtentiWebClient.GetCurrentUserSurname()); }

        public bool IsLogged { get => !string.IsNullOrEmpty(UserEmail); }

        public string UserIcon
        {
            get
            {
                if (IsLogged)
                    return "\ue129";//loggato
                else
                    return "\ue128";//log-in
            }
        }

        public string UserProfileWebUILink
        {
            get => UtentiWebClient.GetCurrentUserProfileWebUILink();
        }

        public bool IsWebUserInfoEnabled
        {
            get
            {
                if (LicenseHelper.IsAnyFeaturePresent(new List<LicenseFeature>() { LicenseFeature.Feature_Web }, out _))
                    return true;

                return false;
            }
        }

        #endregion


    }
}
