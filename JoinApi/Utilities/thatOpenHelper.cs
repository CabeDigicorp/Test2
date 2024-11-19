using Jint.Native;
using Jint.Runtime;
using Microsoft.AspNetCore.NodeServices;
using JoinApi.Service;
using Microsoft.JSInterop;
using ModelData.Dto;
using System.ComponentModel;

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;



namespace JoinWebUI.Utilities
{
    public class thatOpenHelper : IDisposable, INotifyPropertyChanged
    {
        public static string NullPlaceHolder { get; } = "[non definito]";

        private int _fileChunkSize = 10000000;

        //private Guid _progettoId;
        //private Guid _operaId;

        private MongoDbService _mongoDbService;

        private DotNetObjectReference<thatOpenHelper>? dotNetReference;
        //private IJSRuntime _jsRuntime;

        private Jint.Engine? _jsModule;
        private static SemaphoreSlim _loadMutex = new SemaphoreSlim(1);
        private static SemaphoreSlim _uploadMutex = new SemaphoreSlim(1);

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

 
        public thatOpenHelper(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
            dotNetReference = DotNetObjectReference.Create(this);
        }

        public async Task ConvertModel(byte[] ifcFile, string fileName, Guid ifcId, Guid operaId)
        {
            Log.ForContext("FileName", fileName).
                    Information("Accesso task di conversione modello ConvertModel, inizio processo in corso...");

            await _loadMutex.WaitAsync();

            try 
            {

                if (_jsModule == null)
                {
                    var sourceCode = File.ReadAllText(@"Scripts/dist/thatOpen.mjs");
                    _jsModule = new Jint.Engine();
                    _jsModule.Execute(sourceCode);

                    _jsModule.Invoke("initComponents", dotNetReference);
                }

                if (_jsModule != null)
                {
                    try
                    {
                        var res = _jsModule.Invoke("convertModelFromIFC", ifcFile, fileName, operaId).ToObject();
                        await Task.Delay(50);
                        if (res != null)
                        {
                            int[] lengths = (int[])res;
                            await SetFragments(fileName, ifcId, operaId, lengths[0]);
                            await SetCompleteProperties(fileName, ifcId, operaId, lengths[1]);
                        }
                    }
                    catch (Exception exc)
                    {
                        Log.ForContext("FileName", fileName).
                            Error($"Uscita task di conversione modello ConvertModel, operazione completata con errori. Dettaglio eccezione: {exc}.");
                        // _requestErrorMessage = exc.Message;
                        // _requestError = true;
                        Console.WriteLine(exc.Message);
                    }
                    finally
                    {
                        GC.Collect();
                    }

                }

            }
            catch (Exception ex)
            {
                Log.ForContext("FileName", fileName).
                    Error($"Uscita task di conversione modello ConvertModel, operazione completata con errori. Dettaglio eccezione: {ex}.");
                Console.WriteLine(ex.Message);
            }

            _loadMutex.Release();

            Log.ForContext("FileName", fileName).
                    Information("Uscita task di conversione modello ConvertModel, operazione completata.");
        }


        #region IFC to frag/json conversion

        public async Task SetCompleteProperties(string fileName, Guid ifcId, Guid operaId, int length)
        {
            if (length > 0)
            {
                await _uploadMutex.WaitAsync();

                byte[]? properties = new byte[length];

                int offset = 0;

                while (offset < length)
                {
                    var tmp = _jsModule!.Invoke("getPropertiesChunk", new object[] { offset, offset + Math.Min(_fileChunkSize, length - offset) });

                    var decoded = System.Convert.FromBase64String(tmp.ToString());
                    decoded.CopyTo(properties, offset);
                    offset += decoded.Length;

                    tmp = "";
                    decoded = new byte[0];

                }

                GC.Collect();

                _jsModule!.Invoke("clearPropertiesExport");

                //_mongoDbService.UploadFile(new MemoryStream(properties), fileName + ".prop", "application/octet-stream", new Dictionary<string, object>() { { "operaId", operaId }, { "parentGuid", ifcId }, { "compressed", true } });

                //await _apiClient.UploadAllegatoAsync(_uploadAllegatiUrl, _uploadingFileName + ".prop", "application/octet-stream", properties, _operaId, _uploadingFileParentGuid, JoinWebApiClient.CompressionOptions.AlreadyCompressed, true);

                properties = null;

                _uploadMutex.Release();

                GC.Collect();

            }
            else
            {

            }
        }

        //[JSInvokable("SetFragments")]
        public async Task SetFragments(string fileName, Guid ifcId, Guid operaId, int length)
        {
            if (length > 0)
            {
                await _uploadMutex.WaitAsync();

                byte[] fragments = new byte[length];

                int offset = 0;

                while (offset < length)
                {
                    var tmp = _jsModule!.Invoke("getFragmentsChunk", new object[] { offset, offset + Math.Min(_fileChunkSize, length - offset) });

                    var decoded = System.Convert.FromBase64String(tmp.ToString());
                    decoded.CopyTo(fragments, offset);
                    offset += decoded.Length;

                    tmp = "";
                    decoded = new byte[0];

                }

                GC.Collect();

                _jsModule!.Invoke("clearFragmentsExport");

                //_mongoDbService.UploadFile(new MemoryStream(fragments), fileName + ".frag", "application/octet-stream", new Dictionary<string, object>() { { "operaId", operaId }, { "parentGuid", ifcId }, { "compressed", true } });
                //await _apiClient.UploadAllegatoAsync(_uploadAllegatiUrl, _uploadingFileName + ".frag", "application/octet-stream", fragments, _operaId, _uploadingFileParentGuid, JoinWebApiClient.CompressionOptions.AlreadyCompressed, true);

                fragments = null;

                _uploadMutex.Release();

                GC.Collect();

            }
            else
            {

            }
        }

        #endregion

        //public static class PropertyNames
        //{
        //    public const string SpatialStructure = "SpatialStructure";
        //    public const string ClassesTypesTree = "ClassesTypesTree";
        //    public const string GroupsTree = "GroupsTree";
        //    public const string Properties = "Properties";
        //    public const string Materials = "Materials";
        //}



        public async void Dispose()
        {
            GC.SuppressFinalize(this);

            if (_jsModule != null)
            {
                _jsModule.Dispose();
                _jsModule = null;

                dotNetReference?.Dispose();
                dotNetReference = null;
            }

            GC.Collect();

        }
    }


}
