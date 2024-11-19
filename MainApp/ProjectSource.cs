using CommonResources;
using Commons;
using DevExpress.Utils.Url;
using Microsoft.Isam.Esent.Interop;
using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WebServiceClient;
using WebServiceClient.Clients;

namespace MainApp
{
    public interface ProjectSource
    {

        string FullName { get; }
        string Name { get; }
        string BytesReadable { get; }
        DateTime LastAccessTime { get; }
        DateTime LastWriteTime { get; }
        long Length { get; }
        ProjectSourceType Type { get; }
        string GetSaveIconKey();
        Task<ProjectData> LoadProject(MessageBarView messageBarView = null);
        Task<GenericResponse> CanLoadProject(MessageBarView messageBarView = null);
        RecentProjectInfo CreateRecentProjectInfo();
        string GetKey();

        static ProjectSource Create(RecentProjectInfo recentProjectInfo)
        {
            if (recentProjectInfo.ProjectSourceType == ProjectSourceType.File)
            {
                return new ProjectSourceFile(new FileInfo(recentProjectInfo.Path));
            }
            else if (recentProjectInfo.ProjectSourceType == ProjectSourceType.Web)
            {
                return new ProjectSourceWeb()
                {
                    Name = recentProjectInfo.Path,
                    FullName = recentProjectInfo.Path,
                    OperaId = recentProjectInfo.OperaId,
                    ProgettoId = recentProjectInfo.ProgettoId
                };
            }
            else
                return null;

        }

        static string GetKey(RecentProjectInfo recentProjectInfo)
        {
            return GetKey(recentProjectInfo.ProjectSourceType, recentProjectInfo.Path, recentProjectInfo.OperaId, recentProjectInfo.ProgettoId);
        }
        static string GetKey(ProjectSourceType type, string path, Guid operaId, Guid progettoId)
        {
            if (type == ProjectSourceType.Web)
                return string.Format("{0}|{1}", operaId.ToString(), progettoId.ToString());
            else
                return path;
        }

    }

    public class ProjectSourceFile : ProjectSource
    {
        FileInfo _fileInfo = null;
        MessageBarView _messageBarView = null;

        public ProjectSourceFile(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
        }

        public string FullName => _fileInfo.FullName; 
        public string Name => _fileInfo.Name; 
        public string BytesReadable => _fileInfo.GetBytesReadable(); 
        public DateTime LastAccessTime => _fileInfo.LastAccessTime; 
        public DateTime LastWriteTime => _fileInfo.LastWriteTime; 
        public long Length => _fileInfo.Length; 
        public ProjectSourceType Type => ProjectSourceType.File;
        public string DirectoryName => _fileInfo.DirectoryName;

        public static string SaveIconKey => "\ue038";
        public string GetSaveIconKey() => SaveIconKey;

        public async Task<ProjectData> LoadProject(MessageBarView messageBarView = null)
        {
            
            Project prj = null;
            _messageBarView = messageBarView;
            int projectVersion = ProjectFileRead(ref prj);

            ProjectData prjData = new ProjectData();
            prjData.Project = prj;
            prjData.ProjectVersion = projectVersion;

            return prjData;
        }
        private int ProjectFileRead(ref Project project)
        {
            int projectVersion;
            using (var file = File.OpenRead(FullName))
            {
                var pStream = new ProgressStream(file);
                pStream.BytesRead += new ProgressStreamReportDelegate(pStream_BytesRead);

                project = ModelSerializer.Deserialize(pStream, out projectVersion);

                if (project == null)
                {
                    _messageBarView.Show(LocalizationProvider.GetString("VersioneDelFileSuccessiva"));
                }
            }

            return projectVersion;
        }

        void pStream_BytesRead(object sender, ProgressStreamReportEventArgs args)
        {
            int perc = (int)((50.0 * args.StreamPosition) / args.StreamLength);

            _messageBarView.Show(LocalizationProvider.GetString("CaricamentoInCorso"), false, perc);


        }

        public RecentProjectInfo CreateRecentProjectInfo()
        {
            return new RecentProjectInfo() { Path = FullName, ProjectSourceType = ProjectSourceType.File };
        }

        public string GetKey()
        {
            return ProjectSource.GetKey(Type, FullName, Guid.Empty, Guid.Empty);
        }

        public async Task<GenericResponse> CanLoadProject(MessageBarView messageBarView = null)
        {
            GenericResponse gr = new GenericResponse(false);

            if (_fileInfo.Exists)
                gr.Success = true;
            else
                gr.Message = LocalizationProvider.GetString("FileNonTrovato");

            return gr;
        }
    }

    public class ProjectSourceWeb : ProjectSource
    {

        public string FullName { get; set; } //nomeProgetto
        public string Name { get; set; } //nomeProgetto
        public string BytesReadable { get => "-"; }
        public DateTime LastAccessTime { get => DateTime.MinValue; }
        public DateTime LastWriteTime { get => DateTime.MinValue; }
        public long Length { get => -1; }
        public ProjectSourceType Type => ProjectSourceType.Web;
        public Guid OperaId { get; set; }
        public Guid ProgettoId { get; set; }

        public static string SaveIconKey => "\uE11A";
        public string GetSaveIconKey() => SaveIconKey;

        public async Task<ProjectData> LoadProject(MessageBarView messageBarView = null)
        {
            string msg = null;
            //if (LicenseHelper.IsAnyFeaturePresent(new List<LicenseFeature>() { LicenseFeature.Feature_Web }, out msg))
            //{

            GenericResponse gr = new GenericResponse(false);
                //var gr = await UtentiWebClient.RefreshToken();

                //if (gr.Success)
                //{
                    messageBarView.Show(LocalizationProvider.GetString("CaricamentoInCorso"), false, 20);

                    var projectData = await ProgettiWebClient.LoadProject(OperaId, ProgettoId, gr);
                    if (gr.Success)
                    {
                        if (projectData.ProjectVersion < 0 && !string.IsNullOrEmpty(gr.Message))
                            messageBarView.Show(gr.Message);

                        messageBarView.Show(LocalizationProvider.GetString("CaricamentoInCorso"), false, 50);
                    }
                    else
                    {
                        messageBarView.Show(gr.Message);
                    }

                    return projectData;
                //}
                //else
                //{
                //    messageBarView.Show(string.Format("{0} {1}", LocalizationProvider.GetString("Autenticazione fallita."), gr.Message));
                //}

            //}
            //else
            //{
            //    messageBarView.Show(msg);
            //}
            return new ProjectData();
        }

        public RecentProjectInfo CreateRecentProjectInfo()
        {
            return new RecentProjectInfo() { Path = FullName, ProjectSourceType = ProjectSourceType.Web, ProgettoId = this.ProgettoId, OperaId = this.OperaId };
        }

        public string GetKey()
        {
            return ProjectSource.GetKey(Type, FullName, OperaId, ProgettoId);
        }

        public async Task<GenericResponse> CanLoadProject(MessageBarView messageBarView = null)
        {
            GenericResponse gr = new GenericResponse(false);

            string msg = string.Empty;
            if (LicenseHelper.IsAnyFeaturePresent(new List<LicenseFeature>() { LicenseFeature.Feature_Web }, out msg))
            {
                messageBarView.Show(LocalizationProvider.GetString("LoginInCorso"), false, 20);
                gr = await UtentiWebClient.RefreshToken();
            }
            else
            {
                gr.Message = msg;
            }

            return gr;
        }
    }

    public enum ProjectSourceType
    {
        File = 0,
        Web = 1,
    }



}
