using JoinWebUI.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Popups;
using System.ComponentModel;
using System.Data;
using ModelData.Model;
using ModelData.Dto;
using Blazored.LocalStorage;
using BracketPipe;
using JoinWebUI.Extensions;
using System.Collections.ObjectModel;
using RtfPipe;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Cards;
using Syncfusion.Blazor.Spinner;
using System.Collections.Specialized;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using BlazorPro.BlazorSize;
using ModelData.Utilities;
using JoinWebUI.Models;
using System.Text;

namespace JoinWebUI.Components
{
    public partial class ModuloProgettoComputo
    {

        [Parameter]
        public Guid ProgettoId { get; set; } = new Guid();

        [Parameter]
        public EventCallback<List<GlobalIdPair>> OnShowIn3D { get; set; }

        [CascadingParameter(Name = "OnInteractWithProjectDetails")]
        public List<Func<bool, Task<bool>>> OnInteractWithProjectDetails { get; set; }

        public bool IsDialogVisible { get; set; } = false;
        public int?[] ComboIndexes { get; set; } = new int?[10];
        public string? ContentType { get; set; } = "plainText";
        public string? AllValori { get; set; } = "Tutti i valori";
        public string? ContentBarVoicesNumber
        {
            get => $"{(string.IsNullOrEmpty(_contentBarVoicesNumber) ? "0" : _contentBarVoicesNumber)}/{(string.IsNullOrEmpty(_contentBarPageNumber) ? "0" : _contentBarPageNumber)}";
            set => _contentBarVoicesNumber = value;
        }
        public string? ContentBarPagesNumber
        {
            get => $"{_contentBarPagesNumber}/{(ValoriValidiComboBox.Any() ? (ValoriValidiComboBox.Where(x => x.SequenceNumber == _comboBoxSelected)?.FirstOrDefault()?.SottoGruppo?.Count() - 1) : "0")}";
            set => _contentBarPagesNumber = value;
        }
        public string? DialogHeather { get; set; } = "Dettagli attributo:";
        public string? ComboPopupWidthFloored { get; set; } = "300px";
        public string? ComboPopupHeightFloored { get; set; } = "500px";

        public ResizeDirection[] DialogResizeDirections { get; set; } = new ResizeDirection[] { ResizeDirection.All };
        public ObservableDictionary<int, SfComboBox<AttributoRaggruppatoreDto, AttributoRaggruppatoreDto>> ComboBoxRefs { get; set; } = new ObservableDictionary<int, SfComboBox<AttributoRaggruppatoreDto, AttributoRaggruppatoreDto>>();
        public ObservableCollection<ComputoDto>? InfoComputo { get; set; } = new();
        public List<AttributoRaggruppatoreDto>? AttributiGrouperAvailable { get; set; } = new();
        public ObservableCollection<AttributoRaggruppatoreDto> ValoriValidiComboBox { get; set; } = new ObservableCollection<AttributoRaggruppatoreDto>();
        public DialogEffect AnimationEffect { get; set; } = DialogEffect.Zoom;
        public AttributoFiltroMultiploDto FiltriRaggruppatoriInput { get; set; } = new();
        public AttributoFiltroMultiploDto FiltriRaggruppatoriInputHistory { get; set; } = new();
        public int CounterRaggruppatoriAttivi { get; set; } = 0;
        public int CounterFiltriAttivi { get; set; } = 0;
        public int CounterAggregatiAttivi { get; set; } = 0;
        public int[] AccordionExpandedIndexes { get; set; } = new int[0];
        public bool isTyped { get; set; } = false;

        private PageComputoNames _pageSelected = PageComputoNames.Computo;
        private int? _comboBoxSelected = 0;
        private int? _comboBoxSelectedIndex = 0;
        //private bool _isAfterCreatedRaggruppatori = false;
        private bool _userValueChanged = false;
        private string? _contentBarPagesNumber = "0";
        private string? _contentBarPageNumber = "0";
        private string? _contentBarVoicesNumber = "0";
        private List<AttributoRaggruppatoreDto>? _attributiGrouperSetted = new();
        private bool _isComputoLoading = true;
        private bool _computoLoadingStopSpinner = false;
        private bool _isNoDataLoaded = true;
        private bool _allowRefreshGrouperData = false;
        private bool _allowRefreshFilterData = false;
        private bool _isAccordionVisible = true;
        private string? _contentDialog = string.Empty;
        private List<string> _lstGruppiAccordionExpandend = new List<string>();
        private BrowserWindowSize screenDimensions = new();
        private SfToast sfToastObj;
        private int sfToastMsTimeout = 1500;
        private string sfToastPositionX = "Right";
        private string sfToastPositionY = "Bottom";
        private string sfToastWidth = "300";
        private string sfToastHeight = "auto";
        private string sfToastContent = "";
        private string sfToastIcon = "icon-info";
        private ToastEffect sfToastShowAnimation = ToastEffect.FadeIn;
        private ToastEffect sfToastHideAnimation = ToastEffect.FadeOut;

        private string sfToastTitle = "Avviso notifica";

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ValoriValidiComboBox.CollectionChanged += ValoriValidiComboBoxChanged!;
                (InfoComputo ?? new()).CollectionChanged += InfoComputoChanged!;
                FiltriRaggruppatoriInput.PropertyChanged += FiltriRaggruppatoriInputChanged;
                ComboBoxRefs.CollectionChanged += ComboBoxRefsChanged;

                await GetComputoSummary();
                ExpandGroupContainingCodice(InfoComputo ?? new(), AccordionExpandedIndexes, _lstGruppiAccordionExpandend, ModelData.Model.BuiltInCodes.Attributo.Importo);

                if (!OnInteractWithProjectDetails.Contains(AggiornaComputoProgetto))
                {
                    OnInteractWithProjectDetails.Add(AggiornaComputoProgetto);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        private async Task<bool> AggiornaComputoProgetto(bool data)
        {
            try
            {
                // Implementa la logica per aggiornare le variabili qui
                Log.Information("Aggiorna Computo clicked");
                _ = ShowToast("Aggiornamento Computo in corso...");
                var isComputoUpdated = await _apiClient.JsonGetAsync<bool>($"computo/get-computo-clear/{ProgettoId}");
                if (isComputoUpdated.Success && isComputoUpdated.ResponseContentData)
                {
                    // salvataggio pagina per gestire i casi in cui si aggiorna restando dentro una delle viste filtro o raggruppatori
                    PageComputoNames _savedPageSelected;
                    if (_pageSelected != PageComputoNames.Computo)
                    {
                        _savedPageSelected = _pageSelected;
                    }
                    else
                    {
                        _savedPageSelected = PageComputoNames.Computo;
                    }

                    switch (FiltriRaggruppatoriInputHistory)
                    {
                        case var expression when (expression.AttributiRaggruppatori != null && expression.AttributiRaggruppatori.Any()):
                            {
                                await OnGrouperShow((true, expression.AttributiRaggruppatori, false));
                            }
                            break;
                        case var expression when (expression.AttributiRaggruppatori == null && expression.AttributiFiltri != null):
                            {
                                await OnFilerShow((true, expression.AttributiFiltri!));
                            }
                            break;
                        default:
                            {
                                await GetComputoSummary();
                            }
                            break;
                    }
                    _pageSelected = _savedPageSelected;
                    _allowRefreshGrouperData = true;
                    _allowRefreshFilterData = true;
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return true;
        }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            try
            {
                if (firstRender)
                {
                    try
                    {
                        _listener.OnResized += WindowResized;
                        await SetContainerDim();
                    }
                    catch (Exception ex)
                    {
                        _ = ShowToast($"Errore caricamento: {ex.Message}.");
                        Log.Error($"Errore di inizializzazione modulo filtro - condizione. Dettaglio eccezione: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }


        private IEnumerable<AttributoRaggruppatoreDto?> GetRaggruppatoData(IEnumerable<AttributoRaggruppatoreDto> items)
        {
            try
            {
                var groupedItems = new List<AttributoRaggruppatoreDto>();

                foreach (var item in items.Where(x => x != null))
                {
                    if (item != null && item.ValoreAttributo != null && item.ValoreAttributo.Contains('¦'))
                    {
                        var lines = item.ValoreAttributo.Split('¦').Select(x => x.Trim()).ToList();

                        var antenati = string.Join('¦', lines.Take(lines.Count - 1));

                        var figlio = lines.Last();

                        item.ValoreEtichetta = figlio;

                        item.Antenati = antenati;
                        groupedItems.Add(item);
                    }
                    else if (item != null && item.ValoreAttributo != null)
                    {
                        groupedItems.Add(item);
                    }
                }

                return groupedItems;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return default(List<AttributoRaggruppatoreDto>?) ?? new();
            }
        }

        // Funzione che restituisce true o false in base all'id
        private bool IsSubPageHidden(PageComputoNames pageToVerify)
        {
            return _pageSelected == pageToVerify ? false : true;
        }

        private async void ValoriValidiComboBoxChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        private async void InfoComputoChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        private async void ComboBoxRefsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        void FiltriRaggruppatoriInputChanged(object? sender, PropertyChangedEventArgs e)
        {
            FiltriRaggruppatoriInput.ProgettoId = ProgettoId;
        }

        private async Task GetComputoSummary()
        {
            try
            {
                var computoSummary = await _apiClient.JsonGetAsync<List<ComputoDto>>($"computo/get-computo-progetto-summary/{ProgettoId}");
                if (!computoSummary.Success)
                {
                    _isNoDataLoaded = true;
                }
                else
                {
                    PreserveAccordionExpandedState(computoSummary.ResponseContentData ?? new(), _lstGruppiAccordionExpandend);
                    InfoComputo = new ObservableCollection<ComputoDto>(computoSummary.ResponseContentData ?? new List<ComputoDto>());

                    ContentBarPagesNumber = $"{((InfoComputo != null) ? "0" : "0")}";
                    _contentBarPageNumber = InfoComputo?.FirstOrDefault()!.EntitiesIdNum;
                    ContentBarVoicesNumber = $"{InfoComputo?.FirstOrDefault()?.EntitiesIdNum ?? "0"}";
                    _comboBoxSelected = 0;
                    _comboBoxSelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                _ = ShowToast($"Errore: {ex.Message}", "Errore");
                Log.Error(ex.Message);
            }
            finally
            {
                await Task.Run(() => LoadComputoRtfData());
                await GetAttributiGrouper();
                _isNoDataLoaded = false;
                _isComputoLoading = false;
            }
        }

        private async Task LoadComputoRtfData()
        {
            try
            {
                List<string?>? valoriTestoRTF = InfoComputo?
                    .Where(dto => dto.DefinizioneAttributoCodice == "TestoRTF")
                    .Select(dto => dto.ValoreAttributo)
                    .ToList();

                await Task.WhenAll(valoriTestoRTF?.SelectMany(item =>
                    (item?.Split("~!@#$%^&*()_+") ?? Enumerable.Empty<string>())
                        .Select(async element =>
                        {
                            string trimmedElement = (element ?? "").Trim();
                            string sKey = Utilities.DataStorageHelper.GenerateKey(trimmedElement);

                            if (!await LocalStorage.ContainKeyAsync(sKey))
                            {
                                await ConvertRtfToHtml(trimmedElement, sKey);
                            }
                        })) ?? Enumerable.Empty<Task>());
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        //// <summary>
        /// Esecuzione del filtro aggregato sul computo da modello IFC
        /// </summary>
        /// <param name="filtriInput"></param>
        /// <returns></returns>
        public async Task OnFilterAggregatoShow(List<GlobalIdPair> filtriInput)
        {
            try
            {
                _isComputoLoading = true;
                _pageSelected = PageComputoNames.Computo;

                FiltriRaggruppatoriInput!.AttributiAggregati = ConvertGlobalIdPairToAggregatoDto(filtriInput.ToList());
                FiltriRaggruppatoriInputHistory.AttributiAggregati = ConvertGlobalIdPairToAggregatoDto(filtriInput.ToList());
                FiltriRaggruppatoriInput.AttributiRaggruppatori = new List<AttributoRaggruppatoreDto>(FiltriRaggruppatoriInputHistory?.AttributiRaggruppatori ?? new List<AttributoRaggruppatoreDto>());    // per permettere che il computo si imposti su "tutti i valori" di default
                CounterAggregatiAttivi = (FiltriRaggruppatoriInput.AttributiAggregati != null && FiltriRaggruppatoriInput.AttributiAggregati.Any()) ? FiltriRaggruppatoriInput.AttributiAggregati.Count() : 0;

                ResetComboBoxState();

                // se ho raggruppatori e filtri assieme, calcolo computo ed estraggo lista dei valori validi solo per le voci di interesse (gli eventuali aggregati vanno dentro da soli)
                if (FiltriRaggruppatoriInput != null &&
                FiltriRaggruppatoriInput.AttributiFiltri != null &&
                FiltriRaggruppatoriInput.AttributiFiltri.Children!.Any() &&
                FiltriRaggruppatoriInput.AttributiRaggruppatori != null &&
                FiltriRaggruppatoriInput.AttributiRaggruppatori.Any())
                {
                    var requestFilterAttributes = await _apiClient.JsonPostAsync<AttributoFiltroMultiploDto, List<ComputoDto>?>($"computo/post-valori-da-attributi", FiltriRaggruppatoriInput);
                    if (!requestFilterAttributes.Success)
                    {
                        _isNoDataLoaded = true;
                    }
                    else
                    {
                        _isNoDataLoaded = false;
                        PreserveAccordionExpandedState(requestFilterAttributes.ResponseContentData ?? new(), _lstGruppiAccordionExpandend);
                        InfoComputo = new ObservableCollection<ComputoDto>(requestFilterAttributes.ResponseContentData ?? new List<ComputoDto>());
                        ContentBarVoicesNumber = $"{InfoComputo?.FirstOrDefault()?.EntitiesIdNum ?? "0"}";
                        FiltriRaggruppatoriInput!.AttributiRaggruppatori!.FirstOrDefault()!.EntitiesId = new List<Guid>(InfoComputo?.FirstOrDefault()?.EntitiesId ?? new());
                        await GetValoriUnivociGrouperAsync(FiltriRaggruppatoriInput.AttributiRaggruppatori.OfType<AttributoRaggruppatoreDto>().ToList(), false);
                    }
                    _isComputoLoading = false;
                }
                // se ho i filtri e non i raggruppatori, aggiorno il computo per i soli filtri (gli eventuali aggregati vanno dentro da soli)
                else if ((FiltriRaggruppatoriInput != null &&
                FiltriRaggruppatoriInput.AttributiFiltri != null &&
                FiltriRaggruppatoriInput.AttributiFiltri.Children!.Any() &&
                (FiltriRaggruppatoriInput.AttributiRaggruppatori == null || !FiltriRaggruppatoriInput.AttributiRaggruppatori.Any())))
                {
                    var requestFilterAttributes = await _apiClient.JsonPostAsync<AttributoFiltroMultiploDto, List<ComputoDto>?>($"computo/post-valori-da-attributi", FiltriRaggruppatoriInput);
                    if (!requestFilterAttributes.Success)
                    {
                        _isNoDataLoaded = true;
                    }
                    else
                    {
                        _isNoDataLoaded = false;
                        PreserveAccordionExpandedState(requestFilterAttributes.ResponseContentData ?? new(), _lstGruppiAccordionExpandend);
                        InfoComputo = new ObservableCollection<ComputoDto>(requestFilterAttributes.ResponseContentData ?? new List<ComputoDto>());
                        ContentBarVoicesNumber = $"{InfoComputo?.FirstOrDefault()?.EntitiesIdNum ?? "0"}";
                    }
                    _isComputoLoading = false;
                }
                // se non ho filtri ma solo raggruppatori, calcolo il computo per i raggruppatori ed i vari dati delle listbox (gli eventuali aggregati vanno dentro da soli)
                else if (FiltriRaggruppatoriInput != null &&
                FiltriRaggruppatoriInput.AttributiFiltri == null &&
                FiltriRaggruppatoriInput.AttributiRaggruppatori != null &&
                FiltriRaggruppatoriInput.AttributiRaggruppatori.Any())
                {
                    var requestFilterAggregatiAttributes = await _apiClient.JsonPostAsync<AttributoFiltroMultiploDto, List<ComputoDto>?>($"computo/post-valori-da-attributi", FiltriRaggruppatoriInput);
                    if (!requestFilterAggregatiAttributes.Success)
                    {
                        _isNoDataLoaded = true;
                    }
                    else
                    {
                        _isNoDataLoaded = false;
                        PreserveAccordionExpandedState(requestFilterAggregatiAttributes.ResponseContentData ?? new(), _lstGruppiAccordionExpandend);
                        InfoComputo = new ObservableCollection<ComputoDto>(requestFilterAggregatiAttributes.ResponseContentData ?? new List<ComputoDto>());
                        ContentBarVoicesNumber = $"{InfoComputo?.FirstOrDefault()?.EntitiesIdNum ?? "0"}";
                        FiltriRaggruppatoriInput!.AttributiRaggruppatori!.FirstOrDefault()!.EntitiesId = new List<Guid>(InfoComputo?.FirstOrDefault()?.EntitiesId ?? new());
                        await GetValoriUnivociGrouperAsync(FiltriRaggruppatoriInput.AttributiRaggruppatori.OfType<AttributoRaggruppatoreDto>().ToList(), false);
                    }
                    _isComputoLoading = false;
                    // await GetComputoSummary();
                    // FiltriRaggruppatoriInput!.AttributiRaggruppatori!.FirstOrDefault()!.EntitiesId = new List<Guid>(InfoComputo?.FirstOrDefault()?.EntitiesId ?? new());
                    // await GetValoriUnivociGrouperAsync(FiltriRaggruppatoriInput.AttributiRaggruppatori.OfType<AttributoRaggruppatoreDto>().ToList(), false);
                }
                // se ho filtri aggregati, ricalcolo il computo dando cura di considerare solo il filtro
                else if (FiltriRaggruppatoriInput != null &&
                FiltriRaggruppatoriInput.AttributiAggregati != null &&
                FiltriRaggruppatoriInput.AttributiAggregati.Any())
                {
                    var requestFilterAttributes = await _apiClient.JsonPostAsync<AttributoFiltroMultiploDto, List<ComputoDto>?>($"computo/post-valori-da-attributi", FiltriRaggruppatoriInput);
                    if (!requestFilterAttributes.Success)
                    {
                        _isNoDataLoaded = true;
                    }
                    else
                    {
                        _isNoDataLoaded = false;
                        PreserveAccordionExpandedState(requestFilterAttributes.ResponseContentData ?? new(), _lstGruppiAccordionExpandend);
                        InfoComputo = new ObservableCollection<ComputoDto>(requestFilterAttributes.ResponseContentData ?? new List<ComputoDto>());
                        ContentBarVoicesNumber = $"{InfoComputo?.FirstOrDefault()?.EntitiesIdNum ?? "0"}";
                    }
                    _isComputoLoading = false;
                }
                // se non ho ne filtri ne raggruppatori, aggiorno il computo pulito
                else
                {
                    if (FiltriRaggruppatoriInput != null)
                        FiltriRaggruppatoriInput.AttributiRaggruppatori = null;
                    await GetComputoSummary();
                }

                _ = ShowToast($"Caricamento completato per {CounterAggregatiAttivi} elementi ricevuti dal modello IFC.");

            }
            catch (Exception ex)
            {
                _ = ShowToast($"Errore: {ex.Message}");
                Log.Error(ex.Message);
                _isComputoLoading = false;
                _isNoDataLoaded = true;
                StateHasChanged();
            }
        }

        /// <summary>
        /// Conversione da lista di GlobalIdPair in oggetto AttributoAggregatoDto
        /// </summary>
        /// <param name="lstCodiciIfc"></param>
        /// <returns></returns>
        private List<AttributoAggregatoDto> ConvertGlobalIdPairToAggregatoDto(List<GlobalIdPair> filtriInput)
        {
            List<AttributoAggregatoDto> lstAttributiAggregati = new();

            try
            {
                foreach (var item in filtriInput)
                {
                    AttributoAggregatoDto attributoAggregatoTemp = new()
                    {
                        IfcProjectGlobalId = item.ModelGlobalID,
                        IfcElemGlobalId = item.ObjectGlobalID
                    };
                    lstAttributiAggregati.Add(attributoAggregatoTemp);
                }
            }
            catch (Exception ex)
            {
                _ = ShowToast($"Errore: {ex.Message}", "Errore");
                Log.Error(ex.Message);
                _isComputoLoading = false;
                _isNoDataLoaded = true;
                StateHasChanged();
            }

            return lstAttributiAggregati;
        }

        /// <summary>
        /// Esecuzione del filtro raggruppatore sul computo proveniendo da vista "Raggruppatori"
        /// </summary>
        /// <param name="raggruppatoriInput">bool: mostra o meno, IEnumerable: raggruppatori, bool: seleziona primo elemento per ogni combo</param>
        /// <returns></returns>
        private async Task OnGrouperShow((bool, IEnumerable<AttributoRaggruppatoreDto>?, bool) raggruppatoriInput)
        {
            try
            {
                _isComputoLoading = true;
                _pageSelected = PageComputoNames.Computo;

                if (!raggruppatoriInput.Item1)
                {
                    _isComputoLoading = false;
                }
                else
                {
                    FiltriRaggruppatoriInput!.AttributiRaggruppatori = raggruppatoriInput.Item2;                       // per permettere che il computo si imposti su "tutti i valori" di default
                    FiltriRaggruppatoriInputHistory.AttributiRaggruppatori = new List<AttributoRaggruppatoreDto>(raggruppatoriInput.Item2 ?? new List<AttributoRaggruppatoreDto>());
                    FiltriRaggruppatoriInput.AttributiFiltri = FiltriRaggruppatoriInputHistory.AttributiFiltri;     // per permettere che il computo si imposti su "tutti i valori" di default

                    CounterRaggruppatoriAttivi = (FiltriRaggruppatoriInput?.AttributiRaggruppatori != null && FiltriRaggruppatoriInput.AttributiRaggruppatori.Any()) ? FiltriRaggruppatoriInput.AttributiRaggruppatori.Count() : 0;

                    ResetComboBoxState();

                    // se ho raggruppatori e filtri assieme, calcolo computo ed estraggo lista dei valori validi solo per le voci di interesse (gli eventuali aggregati vanno dentro da soli)
                    if (FiltriRaggruppatoriInput != null &&
                    FiltriRaggruppatoriInput.AttributiFiltri != null &&
                    FiltriRaggruppatoriInput.AttributiFiltri.Children!.Any() &&
                    FiltriRaggruppatoriInput.AttributiRaggruppatori != null &&
                    FiltriRaggruppatoriInput.AttributiRaggruppatori.Any())
                    {
                        var requestFilterAttributes = await _apiClient.JsonPostAsync<AttributoFiltroMultiploDto, List<ComputoDto>?>($"computo/post-valori-da-attributi", FiltriRaggruppatoriInput);
                        if (!requestFilterAttributes.Success)
                        {
                            _isNoDataLoaded = true;
                        }
                        else
                        {
                            _isNoDataLoaded = false;
                            PreserveAccordionExpandedState(requestFilterAttributes.ResponseContentData ?? new(), _lstGruppiAccordionExpandend);
                            InfoComputo = new ObservableCollection<ComputoDto>(requestFilterAttributes.ResponseContentData ?? new List<ComputoDto>());
                            ContentBarVoicesNumber = $"{InfoComputo?.FirstOrDefault()?.EntitiesIdNum ?? "0"}";
                            FiltriRaggruppatoriInput!.AttributiRaggruppatori!.FirstOrDefault()!.EntitiesId = new List<Guid>(InfoComputo?.FirstOrDefault()?.EntitiesId ?? new());
                            await GetValoriUnivociGrouperAsync(FiltriRaggruppatoriInput.AttributiRaggruppatori.OfType<AttributoRaggruppatoreDto>().ToList(), false);
                        }
                        _isComputoLoading = false;
                    }
                    // se ho i filtri e non i raggruppatori, aggiorno il computo per i soli filtri (gli eventuali aggregati vanno dentro da soli)
                    else if ((FiltriRaggruppatoriInput != null &&
                    FiltriRaggruppatoriInput.AttributiFiltri != null &&
                    FiltriRaggruppatoriInput.AttributiFiltri.Children!.Any() &&
                    (FiltriRaggruppatoriInput.AttributiRaggruppatori == null || !FiltriRaggruppatoriInput.AttributiRaggruppatori.Any())))
                    {
                        var requestFilterAttributes = await _apiClient.JsonPostAsync<AttributoFiltroMultiploDto, List<ComputoDto>?>($"computo/post-valori-da-attributi", FiltriRaggruppatoriInput);
                        if (!requestFilterAttributes.Success)
                        {
                            _isNoDataLoaded = true;
                        }
                        else
                        {
                            _isNoDataLoaded = false;
                            PreserveAccordionExpandedState(requestFilterAttributes.ResponseContentData ?? new(), _lstGruppiAccordionExpandend);
                            InfoComputo = new ObservableCollection<ComputoDto>(requestFilterAttributes.ResponseContentData ?? new List<ComputoDto>());
                            ContentBarVoicesNumber = $"{InfoComputo?.FirstOrDefault()?.EntitiesIdNum ?? "0"}";
                        }
                        _isComputoLoading = false;
                    }
                    // se non ho filtri ma solo raggruppatori, calcolo il computo per i raggruppatori ed i vari dati delle listbox (gli eventuali aggregati vanno dentro da soli)
                    else if (FiltriRaggruppatoriInput != null &&
                    FiltriRaggruppatoriInput.AttributiFiltri == null &&
                    FiltriRaggruppatoriInput.AttributiRaggruppatori != null &&
                    FiltriRaggruppatoriInput.AttributiRaggruppatori.Any())
                    {
                        var requestFilterAggregatiAttributes = await _apiClient.JsonPostAsync<AttributoFiltroMultiploDto, List<ComputoDto>?>($"computo/post-valori-da-attributi", FiltriRaggruppatoriInput);
                        if (!requestFilterAggregatiAttributes.Success)
                        {
                            _isNoDataLoaded = true;
                        }
                        else
                        {
                            _isNoDataLoaded = false;
                            PreserveAccordionExpandedState(requestFilterAggregatiAttributes.ResponseContentData ?? new(), _lstGruppiAccordionExpandend);
                            InfoComputo = new ObservableCollection<ComputoDto>(requestFilterAggregatiAttributes.ResponseContentData ?? new List<ComputoDto>());
                            ContentBarVoicesNumber = $"{InfoComputo?.FirstOrDefault()?.EntitiesIdNum ?? "0"}";
                            FiltriRaggruppatoriInput!.AttributiRaggruppatori!.FirstOrDefault()!.EntitiesId = new List<Guid>(InfoComputo?.FirstOrDefault()?.EntitiesId ?? new());
                            await GetValoriUnivociGrouperAsync(FiltriRaggruppatoriInput.AttributiRaggruppatori.OfType<AttributoRaggruppatoreDto>().ToList(), false);
                        }
                        _isComputoLoading = false;
                    }
                    // se ho filtri aggregati, ricalcolo il computo dando cura di considerare solo il filtro
                    else if (FiltriRaggruppatoriInput != null &&
                    FiltriRaggruppatoriInput.AttributiAggregati != null &&
                    FiltriRaggruppatoriInput.AttributiAggregati.Any())
                    {
                        var requestFilterAttributes = await _apiClient.JsonPostAsync<AttributoFiltroMultiploDto, List<ComputoDto>?>($"computo/post-valori-da-attributi", FiltriRaggruppatoriInput);
                        if (!requestFilterAttributes.Success)
                        {
                            _isNoDataLoaded = true;
                        }
                        else
                        {
                            _isNoDataLoaded = false;
                            PreserveAccordionExpandedState(requestFilterAttributes.ResponseContentData ?? new(), _lstGruppiAccordionExpandend);
                            InfoComputo = new ObservableCollection<ComputoDto>(requestFilterAttributes.ResponseContentData ?? new List<ComputoDto>());
                            ContentBarVoicesNumber = $"{InfoComputo?.FirstOrDefault()?.EntitiesIdNum ?? "0"}";
                        }
                        _isComputoLoading = false;
                    }
                    // se non ho ne filtri ne raggruppatori, aggiorno il computo pulito
                    else
                    {
                        if (FiltriRaggruppatoriInput != null)
                            FiltriRaggruppatoriInput.AttributiRaggruppatori = null;
                        await GetComputoSummary();
                    }
                }

                if (raggruppatoriInput.Item3)
                {
                    await PopulateFirstElementComboBox();
                    return;
                }

                _ = ShowToast($"Caricamento computo completato!");
            }
            catch (Exception ex)
            {
                _ = ShowToast($"Errore: {ex.Message}");
                Log.Error(ex.Message);
                _isComputoLoading = false;
                _isNoDataLoaded = true;
                StateHasChanged();
            }
            finally
            {
                //_isAfterCreatedRaggruppatori = true;// booleano per indicare la necessità di forzare la selezione del primo elemento.
            }
        }

        /// <summary>
        /// Esecuzione del filtro semplice sul computo proveniendo da vista "Filtri"
        /// </summary>
        /// <param name="filtriInput"></param>
        /// <returns></returns>
        private async Task OnFilerShow((bool, AttributoFiltroCompositoDto?) filtriInput)
        {
            try
            {
                _isComputoLoading = true;
                _pageSelected = PageComputoNames.Computo;

                if (!filtriInput.Item1)
                {
                    _isComputoLoading = false;
                }
                else
                {
                    FiltriRaggruppatoriInputHistory.AttributiFiltri = FiltriRaggruppatoriInput.AttributiFiltri = filtriInput.Item2;                                                         // per permettere che il computo si imposti su "tutti i valori" di default
                    FiltriRaggruppatoriInput.AttributiRaggruppatori = new List<AttributoRaggruppatoreDto>(FiltriRaggruppatoriInputHistory?.AttributiRaggruppatori ?? new List<AttributoRaggruppatoreDto>());    // per permettere che il computo si imposti su "tutti i valori" di default

                    CounterFiltriAttivi = (FiltriRaggruppatoriInput != null && FiltriRaggruppatoriInput.AttributiFiltri != null && FiltriRaggruppatoriInput.AttributiFiltri.Children != null && FiltriRaggruppatoriInput.AttributiFiltri.Children.Any()) ? FiltriRaggruppatoriInput.AttributiFiltri.Children.Count() : 0;

                    ResetComboBoxState();

                    // se ho raggruppatori e filtri assieme, calcolo computo ed estraggo lista dei valori validi solo per le voci di interesse (gli eventuali aggregati vanno dentro da soli)
                    if (FiltriRaggruppatoriInput != null &&
                    FiltriRaggruppatoriInput.AttributiFiltri != null &&
                    FiltriRaggruppatoriInput.AttributiFiltri.Children!.Any() &&
                    FiltriRaggruppatoriInput.AttributiRaggruppatori != null &&
                    FiltriRaggruppatoriInput.AttributiRaggruppatori.Any())
                    {
                        var requestFilterAttributes = await _apiClient.JsonPostAsync<AttributoFiltroMultiploDto, List<ComputoDto>?>($"computo/post-valori-da-attributi", FiltriRaggruppatoriInput);
                        if (!requestFilterAttributes.Success)
                        {
                            _isNoDataLoaded = true;
                        }
                        else
                        {
                            _isNoDataLoaded = false;
                            PreserveAccordionExpandedState(requestFilterAttributes.ResponseContentData ?? new(), _lstGruppiAccordionExpandend);
                            InfoComputo = new ObservableCollection<ComputoDto>(requestFilterAttributes.ResponseContentData ?? new List<ComputoDto>());
                            ContentBarVoicesNumber = $"{InfoComputo?.FirstOrDefault()?.EntitiesIdNum ?? "0"}";
                            FiltriRaggruppatoriInput!.AttributiRaggruppatori!.FirstOrDefault()!.EntitiesId = new List<Guid>(InfoComputo?.FirstOrDefault()?.EntitiesId ?? new());
                            await GetValoriUnivociGrouperAsync(FiltriRaggruppatoriInput.AttributiRaggruppatori.OfType<AttributoRaggruppatoreDto>().ToList(), false);
                        }
                        _isComputoLoading = false;
                    }
                    // se ho i filtri e non i raggruppatori, aggiorno il computo per i soli filtri (gli eventuali aggregati vanno dentro da soli)
                    else if ((FiltriRaggruppatoriInput != null &&
                    FiltriRaggruppatoriInput.AttributiFiltri != null &&
                    FiltriRaggruppatoriInput.AttributiFiltri.Children!.Any() &&
                    (FiltriRaggruppatoriInput.AttributiRaggruppatori == null || !FiltriRaggruppatoriInput.AttributiRaggruppatori.Any())))
                    {
                        var requestFilterAttributes = await _apiClient.JsonPostAsync<AttributoFiltroMultiploDto, List<ComputoDto>?>($"computo/post-valori-da-attributi", FiltriRaggruppatoriInput);
                        if (!requestFilterAttributes.Success)
                        {
                            _isNoDataLoaded = true;
                        }
                        else
                        {
                            _isNoDataLoaded = false;
                            PreserveAccordionExpandedState(requestFilterAttributes.ResponseContentData ?? new(), _lstGruppiAccordionExpandend);
                            InfoComputo = new ObservableCollection<ComputoDto>(requestFilterAttributes.ResponseContentData ?? new List<ComputoDto>());
                            ContentBarVoicesNumber = $"{InfoComputo?.FirstOrDefault()?.EntitiesIdNum ?? "0"}";
                        }
                        _isComputoLoading = false;
                    }
                    // se non ho filtri ma solo raggruppatori, calcolo il computo per i raggruppatori ed i vari dati delle listbox (gli eventuali aggregati vanno dentro da soli)
                    else if (FiltriRaggruppatoriInput != null &&
                    FiltriRaggruppatoriInput.AttributiFiltri == null &&
                    FiltriRaggruppatoriInput.AttributiRaggruppatori != null &&
                    FiltriRaggruppatoriInput.AttributiRaggruppatori.Any())
                    {
                        var requestFilterAggregatiAttributes = await _apiClient.JsonPostAsync<AttributoFiltroMultiploDto, List<ComputoDto>?>($"computo/post-valori-da-attributi", FiltriRaggruppatoriInput);
                        if (!requestFilterAggregatiAttributes.Success)
                        {
                            _isNoDataLoaded = true;
                        }
                        else
                        {
                            _isNoDataLoaded = false;
                            PreserveAccordionExpandedState(requestFilterAggregatiAttributes.ResponseContentData ?? new(), _lstGruppiAccordionExpandend);
                            InfoComputo = new ObservableCollection<ComputoDto>(requestFilterAggregatiAttributes.ResponseContentData ?? new List<ComputoDto>());
                            ContentBarVoicesNumber = $"{InfoComputo?.FirstOrDefault()?.EntitiesIdNum ?? "0"}";
                            FiltriRaggruppatoriInput!.AttributiRaggruppatori!.FirstOrDefault()!.EntitiesId = new List<Guid>(InfoComputo?.FirstOrDefault()?.EntitiesId ?? new());
                            await GetValoriUnivociGrouperAsync(FiltriRaggruppatoriInput.AttributiRaggruppatori.OfType<AttributoRaggruppatoreDto>().ToList(), false);
                        }
                        _isComputoLoading = false;
                    }
                    // se ho filtri aggregati, ricalcolo il computo dando cura di considerare solo il filtro
                    else if (FiltriRaggruppatoriInput != null &&
                    FiltriRaggruppatoriInput.AttributiAggregati != null &&
                    FiltriRaggruppatoriInput.AttributiAggregati.Any())
                    {
                        var requestFilterAttributes = await _apiClient.JsonPostAsync<AttributoFiltroMultiploDto, List<ComputoDto>?>($"computo/post-valori-da-attributi", FiltriRaggruppatoriInput);
                        if (!requestFilterAttributes.Success)
                        {
                            _isNoDataLoaded = true;
                        }
                        else
                        {
                            _isNoDataLoaded = false;
                            PreserveAccordionExpandedState(requestFilterAttributes.ResponseContentData ?? new(), _lstGruppiAccordionExpandend);
                            InfoComputo = new ObservableCollection<ComputoDto>(requestFilterAttributes.ResponseContentData ?? new List<ComputoDto>());
                            ContentBarVoicesNumber = $"{InfoComputo?.FirstOrDefault()?.EntitiesIdNum ?? "0"}";
                        }
                        _isComputoLoading = false;
                    }
                    // se non ho ne filtri ne raggruppatori, aggiorno il computo pulito
                    else
                    {
                        if (FiltriRaggruppatoriInput != null)
                            FiltriRaggruppatoriInput.AttributiRaggruppatori = null;
                        await GetComputoSummary();
                    }
                }
                _ = ShowToast($"Caricamento computo completato!");

            }
            catch (Exception ex)
            {
                _ = ShowToast($"Errore: {ex.Message}");
                Log.Error(ex.Message);
                _isComputoLoading = false;
                _isNoDataLoaded = true;
                StateHasChanged();
            }
        }

        private async void HandleGrouperButtonClick()
        {
            try
            {
                _pageSelected = PageComputoNames.Raggruppatori;
                Log.Information("Attivazione raggruppamento personalizzato.");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                StateHasChanged();
            }
            finally
            {
                _allowRefreshGrouperData = false;

            }
        }

        private async void HandleFilterButtonClick()
        {
            try
            {
                _pageSelected = PageComputoNames.Filtri;
                Log.Information("Attivazione filtro gruppi.");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                StateHasChanged();
            }
            finally
            {
                _allowRefreshFilterData = false;
            }
        }

        /// <summary>
        /// Pulizia filtro aggregato
        /// </summary>
        private async void HandleClearAggregatiButtonClick()
        {

            // Pulizia temporanea in caso di filtri già presenti
            if (CounterAggregatiAttivi > 0)
            {
                try
                {
                    if (FiltriRaggruppatoriInput != null)
                        FiltriRaggruppatoriInput.AttributiAggregati = new List<AttributoAggregatoDto>();

                    CounterAggregatiAttivi = 0;

                    if (FiltriRaggruppatoriInput != null && FiltriRaggruppatoriInput.AttributiRaggruppatori != null && FiltriRaggruppatoriInput.AttributiRaggruppatori.Any())
                        await OnGrouperShow((true, FiltriRaggruppatoriInputHistory.AttributiRaggruppatori, false)); // richiamo il refresh del computo (filtri o raggruppatori è uguale)
                    else if (FiltriRaggruppatoriInput != null && FiltriRaggruppatoriInput.AttributiFiltri != null && FiltriRaggruppatoriInput.AttributiFiltri.Children != null && FiltriRaggruppatoriInput.AttributiFiltri.Children.Any())
                        await OnFilerShow((true, FiltriRaggruppatoriInputHistory.AttributiFiltri)); // richiamo il refresh del computo (filtri o raggruppatori è uguale)
                    else
                        await GetComputoSummary();

                    _ = ShowToast($"Pulizia filtro aggregato completato.");
                }
                catch (Exception ex)
                {
                    _ = ShowToast($"Errore pulizia del filtro aggregato: {ex.Message}", "Errore");
                    Log.Error(ex.Message);
                }
                finally
                {
                    StateHasChanged();
                }
            }
            else
            {
                _ = ShowToast($"Nessun filtro aggregato da rimuovere!");
            }
        }

        /// <summary>
        /// Invocazione nel modello tramite HightsLightGlobalIds()
        /// </summary>
        private async void HandleShowOnIfcButtonClick()
        {
            try
            {
                if (InfoComputo != null && InfoComputo.Any() && InfoComputo.FirstOrDefault() != null && InfoComputo.FirstOrDefault().EntitiesId != null && InfoComputo.FirstOrDefault().EntitiesId.Any())
                {
                    var valoriIfcCodesReceived = await _apiClient.JsonPostAsync<List<GlobalIdPair>>($"computo/post-ifc-ids/{ProgettoId}", InfoComputo?.FirstOrDefault()?.EntitiesId);
                    var lstValoriIfcCodesReceived = valoriIfcCodesReceived.ResponseContentData;

                    if (!valoriIfcCodesReceived.Success)
                    {
                        Log.Error($"Errore interno al server nella richiesta delle coppie di valori ifcProjectGlobalId e ifcElemGlobalId per:{String.Join(",", InfoComputo?.FirstOrDefault()?.EntitiesId ?? new())}");
                        _ = ShowToast($"Errore interno al server nella richiesta delle coppie di valori ifcProjectGlobalId e ifcElemGlobalId.", "Errore");
                    }
                    else
                    {
                        var lstIfcCodes = valoriIfcCodesReceived.ResponseContentData;
                        if (lstIfcCodes != null && !lstIfcCodes.Any())
                        {
                            _ = ShowToast($"Nessuna corrispondenza trovata nel computo da visualizzare nel modello Ifc!");
                            return;
                        }

                        Log.Information($"Estrazione dei dati per le coppie di valori ifcProjectGlobalId e ifcElemGlobalId per:{String.Join(",", InfoComputo?.FirstOrDefault()?.EntitiesId ?? new())}, con {String.Join(",", lstIfcCodes ?? new())}");

                        if (OnShowIn3D.HasDelegate)
                        {
                            await OnShowIn3D.InvokeAsync(lstIfcCodes);
                        }
                    }

                    Log.Information("Attivazione evidenzia nel modello 3D.");
                }
                else
                {
                    Log.Warning("Nessun elemento da visualizzare nel modello Ifc.");
                    _ = ShowToast($"Nessun elemento da visualizzare nel modello Ifc.");
                }
            }
            catch (Exception ex)
            {
                _ = ShowToast($"Errore su modello IFC: {ex.Message}", "Errore");
                Log.Error(ex.Message);
            }
            finally
            {
                StateHasChanged();
            }
        }

        private async void HandlePageBackClick()
        {
            try
            {
                ComboIndexes[_comboBoxSelected == null ? 0 : (int)_comboBoxSelected] =
                (ComboIndexes[_comboBoxSelected == null ? 0 : (int)_comboBoxSelected] - 1) > 0 ? (ComboIndexes[_comboBoxSelected == null ? 0 : (int)_comboBoxSelected] - 1) :
                0;

                int? vociTotaliPerComboBox = ValoriValidiComboBox?.FirstOrDefault(x => x.SequenceNumber == _comboBoxSelected)?
                                                                                           .SottoGruppo?
                                                                                           .Count();
                int vocePrecedente = (((int)(_comboBoxSelectedIndex ?? 0)) - 1);
                var previewElement = ValoriValidiComboBox?.FirstOrDefault(x => x.SequenceNumber == _comboBoxSelected)?
                         .SottoGruppo?
                         .FirstOrDefault(y => y.SequenceNumber == (vocePrecedente > 0 ? vocePrecedente : 0));

                PrepareDataForComboBox(previewElement!);
            }
            catch (Exception ex)
            {
                _ = ShowToast($"Errore: {ex.Message}", "Errore");

                Log.Error(ex.Message);
                StateHasChanged();
            }
        }

        private async void HandlePageNextClick()
        {
            try
            {
                ComboIndexes[_comboBoxSelected == null ? 0 : (int)_comboBoxSelected] =
                (ComboIndexes[_comboBoxSelected == null ? 0 : (int)_comboBoxSelected] + 1) < ComboBoxRefs.FirstOrDefault(x => x.Key == _comboBoxSelected).Value.DataSource.Count() ?
                (ComboIndexes[_comboBoxSelected == null ? 0 : (int)_comboBoxSelected] + 1) : ComboIndexes[_comboBoxSelected == null ? 0 : (int)_comboBoxSelected];
                int? vociTotaliPerComboBox = ValoriValidiComboBox?.FirstOrDefault(x => x.SequenceNumber == _comboBoxSelected)?
                                                                               .SottoGruppo?
                                                                               .Count();
                int voceSuccessiva = (((int)(_comboBoxSelectedIndex ?? 0)) + 1);
                var nextElement = ValoriValidiComboBox?.FirstOrDefault(x => x.SequenceNumber == _comboBoxSelected)?
                         .SottoGruppo?
                         .FirstOrDefault(y => y.SequenceNumber == (voceSuccessiva >= vociTotaliPerComboBox ? vociTotaliPerComboBox : voceSuccessiva));

                PrepareDataForComboBox(nextElement!);
            }
            catch (Exception ex)
            {
                _ = ShowToast($"Errore: {ex.Message}", "Errore");

                Log.Error(ex.Message);
                StateHasChanged();
            }
        }

        private async Task GetAttributiGrouper()
        {
            try
            {
                int groupIndex = 0;
                AttributiGrouperAvailable = (await Task.FromResult(InfoComputo ?? new ObservableCollection<ComputoDto>()))
        .Where(item => item.IsAllowMasterGrouping)
        .GroupBy(item => item.NomeGruppo)
        .Select(group =>
        {
            int childIndex = 0;
            var sottoGruppo = group.Select(item => new AttributoRaggruppatoreDto
            {
                ProgettoId = ProgettoId,
                DefinizionAttributoCodice = item.DefinizioneAttributoCodice,
                IsAllowMasterGrouping = item.IsAllowMasterGrouping,
                AttributoId = item.EntityId,
                Codice = item.Codice,
                IsVisible = item.IsVisible,
                CodiceGruppo = $"{(group.FirstOrDefault() ?? new()).Codice}_gruppo",
                ValoreAttributo = item.Etichetta,
                ValoreEtichetta = item.ValoreEtichetta,
                NomeGruppo = item.ValoreEtichetta,
                Descrizione = item.Descrizione,
                Tooltip = item.ValoreDescrizione,
                IsSelected = false,
                SequenceNumber = childIndex++,
                ValoriUnivociOrdered = new List<string>(item.ValoriUnivociOrdered ?? new List<string>()),
            }).ToList();

            return new AttributoRaggruppatoreDto
            {
                ProgettoId = ProgettoId,    // TODO: per completezza portarsi dietro anche l'ID dell'attributo!
                AttributoId = (group.FirstOrDefault() ?? new()).EntityId,
                Codice = $"{(group.FirstOrDefault() ?? new()).Codice}_gruppo",
                ValoreAttributo = (group.FirstOrDefault() ?? new()).NomeGruppo,
                ValoreEtichetta = (group.FirstOrDefault() ?? new()).NomeGruppo,
                NomeGruppo = (group.FirstOrDefault() ?? new()).NomeGruppo,
                Descrizione = (group.FirstOrDefault() ?? new()).Descrizione,
                Tooltip = (group.FirstOrDefault() ?? new()).ValoreDescrizione,
                CodiceGruppo = (group.FirstOrDefault() ?? new()).Codice,
                IsVisible = sottoGruppo.Any(sg => sg.IsVisible),
                IsSelected = false,
                SequenceNumber = groupIndex++,
                SottoGruppo = sottoGruppo
            };
        }).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        private async Task GetValoriUnivociGrouperAsync(List<AttributoRaggruppatoreDto>? ieAttributiGroupers, bool computoRefresh = false)
        {
            try
            {
                if (ieAttributiGroupers != null && ieAttributiGroupers.Any())
                {
                    var valoriValidiReceived = await _apiClient.JsonPostAsync<List<AttributoRaggruppatoreDto>?>($"computo/post-computo-valori-univoci", ieAttributiGroupers);
                    var lstValoriValidiReceived = valoriValidiReceived.ResponseContentData;

                    if (!valoriValidiReceived.Success)
                    {
                        Log.Error($"Errore nella richiesta al server per i valori validi degli attributi:{valoriValidiReceived.ResponseStatusCode}");
                    }
                    else
                    {
                        ValoriValidiComboBox?.ObservableForEach(x => x.Dispose());
                        ValoriValidiComboBox?.Clear();
                        // Ricostruzione combobox secondo quanto desiderato
                        foreach ((var item, int indexExternal) in (lstValoriValidiReceived ?? new List<AttributoRaggruppatoreDto>()).Select((item, index) => (item, index)))
                        {
                            int indexInternal = 0;
                            AttributoRaggruppatoreDto attributiGrouperModel = new();
                            attributiGrouperModel.ProgettoId = item.ProgettoId;
                            attributiGrouperModel.Codice = item.Codice;
                            attributiGrouperModel.ValoreAttributo = item.ValoreAttributo;
                            attributiGrouperModel.ValoreEtichetta = item.ValoreEtichetta;
                            attributiGrouperModel.SequenceNumber = indexExternal; //item.SequenceNumber;
                            attributiGrouperModel.NomeGruppo = item.NomeGruppo;
                            attributiGrouperModel.CodiceGruppo = item.CodiceGruppo;
                            attributiGrouperModel.EntitiesId = new List<Guid>(item.EntitiesId ?? new());
                            attributiGrouperModel.SottoGruppo = new();
                            attributiGrouperModel.SottoGruppo.Add(new AttributoRaggruppatoreDto() { ProgettoId = item.ProgettoId, AttributoId = item.AttributoId, Codice = null, ValoreEtichetta = AllValori, ValoreAttributo = AllValori, NomeGruppo = item.NomeGruppo, CodiceGruppo = $"{item.CodiceGruppo}_{indexExternal}", SequenceNumber = indexInternal++ });
                            item.ValoriUnivociOrdered?.ForEach(x => attributiGrouperModel.SottoGruppo.Add(new AttributoRaggruppatoreDto() { ProgettoId = item.ProgettoId, AttributoId = item.AttributoId, Codice = item.Codice, ValoreEtichetta = x, NomeGruppo = item.NomeGruppo, ValoreAttributo = x, CodiceGruppo = $"{item.CodiceGruppo}_{indexExternal}", SequenceNumber = indexInternal++ }));
                            (ValoriValidiComboBox ?? new()).Add(attributiGrouperModel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            finally
            {
                if (computoRefresh)
                {
                    await GetComputoSummary();
                }
                else
                {
                    _isComputoLoading = false;
                }
            }
        }

        /// Evento precedente alla selezione vera e propria
        private void OnComboBoxValueSelected(SelectEventArgs<AttributoRaggruppatoreDto> args)
        {
            args.Cancel = !PrepareDataForComboBox(args.ItemData);
        }

        /// Funzione per raggogliere tutti i valori delle combobox selezionati ed inserirli nella struttura dati (sincrono, precedente alla post selezione)
        private bool PrepareDataForComboBox(AttributoRaggruppatoreDto itemData)
        {
            try
            {
                if (itemData == null)
                {
                    return false;
                }

                // rimuove gli elementi successivi ad una combobox specifica, solamente quest'ultima ed i suoi padri verranno considerati validi.
                int? indexToRemove = ValoriValidiComboBox?
                        .Where(x => (x.SottoGruppo ?? new()).Any(y => y.CodiceGruppo == itemData.CodiceGruppo))?.FirstOrDefault()?.SequenceNumber;
                if (indexToRemove < ValoriValidiComboBox?.Count)
                {
                    _attributiGrouperSetted?.RemoveRangeSafely<AttributoRaggruppatoreDto>(indexToRemove, (_attributiGrouperSetted.Count - indexToRemove));
                    if (ComboIndexes.ElementAtOrDefaultNullable(indexToRemove) != null && indexToRemove != null) // reset combobox successive nello stato zero
                    {
                        ComboIndexes = ComboIndexes.Select((x, index) => index > indexToRemove ? 0 : x).ToArray();
                    }
                }

                AttributoRaggruppatoreDto? itemAlreadyPresent = _attributiGrouperSetted?.FirstOrDefault(x => x.CodiceGruppo == itemData.CodiceGruppo);
                if (_attributiGrouperSetted != null && itemAlreadyPresent != null)
                {
                    _attributiGrouperSetted.Remove(itemAlreadyPresent);
                }

                _attributiGrouperSetted?.Add(itemData);
                itemData.SottoGruppo?.ForEach(x => x.IsSelected = false); // resetto tutti campi i selected delle altre combobox
                itemData.FiltroCondizione = ModelData.Model.ValoreConditionEnum.Equal.ToString();
                itemData.IsSelected = true;
                _userValueChanged = true;
                FiltriRaggruppatoriInput.AttributiRaggruppatori = _attributiGrouperSetted?.SaveSnapshot();

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return false;
            }
        }

        /// Funzione di post selezione, con richiesta computo e popolamento delle successive
        private async Task OnComboBoxValueChanged(ChangeEventArgs<AttributoRaggruppatoreDto, AttributoRaggruppatoreDto> args)
        {
            try
            {
                if (_userValueChanged)
                {
                    _isComputoLoading = true;

                    var computoSummary = await _apiClient.JsonPostAsync<AttributoFiltroMultiploDto, List<ComputoDto>?>($"computo/post-valori-da-attributi", FiltriRaggruppatoriInput);
                    if (computoSummary.Success)
                    {
                        var receivedComputoSummary = computoSummary.ResponseContentData ?? new();

                        ContentBarVoicesNumber = receivedComputoSummary.FirstOrDefault()?.EntitiesIdNum;
                        ContentBarPagesNumber = (args.Value.SequenceNumber).ToString();

                        PreserveAccordionExpandedState(receivedComputoSummary ?? new(), _lstGruppiAccordionExpandend);
                        InfoComputo = new ObservableCollection<ComputoDto>(receivedComputoSummary ?? new List<ComputoDto>());

                        List<AttributoRaggruppatoreDto> deepLstCopy = (ValoriValidiComboBox ?? new()).SaveSnapshot<AttributoRaggruppatoreDto>();
                        _comboBoxSelectedIndex = args.Value.SequenceNumber == null ? 0 : args.Value.SequenceNumber;

                        var _comboBoxSelected = deepLstCopy
        .Select((elementoPadre, index) => new { elementoPadre, index })  // Seleziona elemento e indice
        .Where(x => x.elementoPadre.SottoGruppo != null &&
                    x.elementoPadre.SottoGruppo.Any(s => s.AttributoId == ((AttributoRaggruppatoreDto)args.Value).AttributoId
                    && s.ProgettoId == ((AttributoRaggruppatoreDto)args.Value).ProgettoId
                    && s.Codice == ((AttributoRaggruppatoreDto)args.Value).Codice
                    && s.ValoreAttributo == ((AttributoRaggruppatoreDto)args.Value).ValoreAttributo))
        .Select(x => x.index)  // Ottiene l'indice
        .FirstOrDefault();
                        Log.Information($"Selezionata combobox raggruppatori numero {_comboBoxSelected} e indice {_comboBoxSelectedIndex}.;");

                        AttributoRaggruppatoreDto? nextComboBoxElement = deepLstCopy.FirstOrDefault(x => x.SequenceNumber == _comboBoxSelected + 1);
                        foreach (var item in deepLstCopy.Where(x => x.SequenceNumber > _comboBoxSelected + 1))
                        {
                            if (receivedComputoSummary != null)
                            {
                                item.EntitiesId = new();
                            }
                        }

                        if (nextComboBoxElement != null && receivedComputoSummary != null)
                        {
                            nextComboBoxElement.EntitiesId = receivedComputoSummary.FirstOrDefault()?.EntitiesId;
                            await Task.Run(() => GetValoriUnivociGrouperAsync(deepLstCopy));
                        }
                        else
                        {
                            _ = ShowToast($"Caricamento computo completato!", "Avviso notifica", true);
                        }
                    }
                    _userValueChanged = false;
                }
            }
            catch (Exception ex)
            {
                _ = ShowToast($"Errore di selezione del raggruppatore: {ex.Message}.", "Errore");
                Log.Error(ex.Message);
                _userValueChanged = false;
            }
            finally
            {
                _isComputoLoading = false;
            }
        }

        public void ResetComboBoxState()
        {
            try
            {
                ComboIndexes = ComboIndexes.Select((x, index) => index >= 0 ? 0 : x).ToArray();
                _attributiGrouperSetted?.ForEach(x => x.Dispose());
                _attributiGrouperSetted?.Clear();
                ValoriValidiComboBox.ObservableForEach(x => x.Dispose());
                ValoriValidiComboBox?.Clear();
                ContentBarVoicesNumber = $"{InfoComputo?.FirstOrDefault()?.EntitiesIdNum ?? "0"}";
                ContentBarPagesNumber = "0";
            }
            catch (Exception ex)
            {
                _ = ShowToast($"Errore: {ex.Message}", "Errore");
                Log.Error(ex.Message);
            }
        }

        /// Funzione per auto selezionare il primo elemento di ogni combobox dei raggruppatori
        public async Task PopulateFirstElementComboBox()
        {
            int indexProcessed = 0;
            AttributoRaggruppatoreDto? comboBoxSelectedItem = new();

            try
            {
                if (ValoriValidiComboBox != null && ValoriValidiComboBox.Any())
                {
                    for (int i = 0; i < ValoriValidiComboBox.Count; i++)
                    {
                        if (ValoriValidiComboBox[i] != null &&
                            ValoriValidiComboBox[i].SottoGruppo != null &&
                            ValoriValidiComboBox[i].SottoGruppo?.Count > 1 &&
                            ValoriValidiComboBox[i].SottoGruppo?.ElementAtOrDefault(1) != null)
                        {
                            indexProcessed = i;
                            comboBoxSelectedItem = ValoriValidiComboBox.FirstOrDefault(x => x.SequenceNumber == i);
                            var item = (ValoriValidiComboBox[i].SottoGruppo?.ElementAtOrDefault(1) ?? new());

                            if (comboBoxSelectedItem != null && PrepareDataForComboBox(item))
                            {
                                var args = new ChangeEventArgs<AttributoRaggruppatoreDto, AttributoRaggruppatoreDto>
                                {
                                    Value = item,
                                };

                                await OnComboBoxValueChanged(args);
                                ComboIndexes[i] = 1;
                            }
                            else
                            {
                                Log.Error($"Impossibile selezionare l'elemento {indexProcessed} del ragguppatore {item.ValoreEtichetta}.");
                                _ = ShowToast($"Impossibile selezionare l'elemento {indexProcessed} del ragguppatore {item.ValoreEtichetta}.", "Errore");
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ShowToast($"Errore auto selezione del raggruppatore {indexProcessed}, {comboBoxSelectedItem?.ValoreEtichetta}: {ex.Message}", "Errore");
                Log.Error(ex.Message);
            }
        }

        private async Task DialogHandleClick(string? sCodice, string? sAttributoCodice)
        {
            try
            {
                var tupleCorrispondente = AttributiConfig.ShowSettings.FirstOrDefault(t => t.Item1 == sAttributoCodice && t.Item2);
                if (tupleCorrispondente != null)
                {
                    await Task.Run(() => OpenDialogClicked(sCodice, sAttributoCodice));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Funzione di preparazione del dato al fine di adattarlo al componente DataToMarkupComponent
        /// </summary>
        /// <param name="sCodice"></param>
        /// <param name="sAttributoCodice"></param>
        /// <returns></returns>
        private async Task OpenDialogClicked(string? sCodice, string? sAttributoCodice)
        {
            try
            {
                ContentType = sAttributoCodice;
                ComputoDto objSelectedItem = InfoComputo?.Where(x => x.Codice == sCodice).FirstOrDefault() ?? new();
                _contentDialog = string.Empty;
                if (objSelectedItem != null)
                {
                    switch (objSelectedItem.DefinizioneAttributoCodice)
                    {
                        case var expression when expression == ModelData.Model.BuiltInCodes.DefinizioneAttributo.TestoRTF:
                            {
                                _contentDialog = await Task.Run(() => GetHtmlData(objSelectedItem.ValoreAttributo));
                            }
                            break;
                        case var expression when (expression == ModelData.Model.BuiltInCodes.DefinizioneAttributo.Reale) ||
                        (expression == ModelData.Model.BuiltInCodes.DefinizioneAttributo.Contabilita):
                            {
                                if (!string.IsNullOrEmpty(objSelectedItem.Descrizione) && string.IsNullOrEmpty(objSelectedItem.ValoreDescrizione))
                                {
                                    _contentDialog = $"{objSelectedItem.Descrizione}\n";
                                }
                                else
                                {
                                    _contentDialog =
                                    $"{(string.IsNullOrEmpty(objSelectedItem.ValoreFormula) ? string.Empty : objSelectedItem.ValoreFormula + "\t")}" +
                                    $"{(string.IsNullOrEmpty(objSelectedItem.ValoreDescrizione) ? string.Empty : objSelectedItem.ValoreDescrizione + "\t")}" +
                                    $"{(string.IsNullOrEmpty(objSelectedItem.Descrizione) ? string.Empty : objSelectedItem.Descrizione + "\n")}";
                                }
                            }
                            break;
                        default:
                            {
                                _contentDialog = objSelectedItem.Descrizione;
                            }
                            break;
                    }
                    DialogHeather = objSelectedItem.Etichetta;
                }

                this.IsDialogVisible = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                this.IsDialogVisible = false;
            }
        }

        private async Task<string?> GetHtmlData(string? sValore)
        {
            string? sContentData = string.Empty;
            try
            {
                foreach (string? element in (sValore != null ? sValore.Split("~!@#$%^&*()_+") : new string?[0]))
                {
                    string sKey = Utilities.DataStorageHelper.GenerateKey(element ?? "");
                    if (await LocalStorage.ContainKeyAsync(sKey))
                    {
                        DataPersistence dataPersistence = await LocalStorage.GetItemAsync<DataPersistence>(sKey) ?? new();
                        sContentData = sContentData != string.Empty ? (sContentData += " \\ " + dataPersistence.HtmlData ?? "") : dataPersistence.HtmlData ?? "";
                    }
                    else
                    {
                        sContentData = sContentData != string.Empty ? (sContentData += " \\ " + await ConvertRtfToHtml(element ?? "", sKey)) : await ConvertRtfToHtml(element ?? "", sKey) ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            finally
            {
            }
            return sContentData;
        }

        private async Task<string> ConvertRtfToHtml(string rtfContent, string? key)
        {
            string sHtml = string.Empty;
            try
            {
                System.Text.EncodingProvider ppp = System.Text.CodePagesEncodingProvider.Instance;
                Encoding.RegisterProvider(ppp);
                sHtml = Rtf.ToHtml(rtfContent);

                string plainText = string.Empty;
                using (var w = new System.IO.StringWriter())
                using (var md = new PlainTextWriter(w))
                {
                    Rtf.ToHtml(rtfContent, md);
                    md.Flush();
                    plainText = w.ToString();
                }

                DataPersistence dataPersistence = new();
                dataPersistence.HashCode = key;
                dataPersistence.HtmlData = sHtml == string.Empty ? "" : sHtml;
                dataPersistence.TextData = plainText == string.Empty ? "" : plainText;
                dataPersistence.RtfData = rtfContent;

                if (!string.IsNullOrEmpty(sHtml) && !string.IsNullOrEmpty(key))
                {
                    await LocalStorage.SetItemAsync(key, dataPersistence);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return (string.IsNullOrEmpty(sHtml) ? string.Empty : sHtml);
        }

        private void OverlayClick(OverlayModalClickEventArgs args)
        {
            this.IsDialogVisible = false;
        }


        /// <summary>
        /// Salvataggio stato espansione degli accordion dentro gli oggetti Datasource, salvataggio in struttura dati esterna per sopperire alla mancanza di un evento specifico
        /// </summary>
        /// <param name="expandedIndices"></param>
        private async void OnAccordionExpandedCollapsed(int[] expandedIndices)
        {
            try
            {
                List<string> lstGruppiAccordionExpandend = GetExpandedGroupNames(expandedIndices, _lstGruppiAccordionExpandend);

                foreach (var groupName in lstGruppiAccordionExpandend)
                {
                    Console.WriteLine($"NomeGruppo espanso: {groupName}");
                }

                AccordionExpandedIndexes = expandedIndices;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Funzione per mantenere aggiornato lo stato degli accordion al combio di computo
        /// </summary>
        /// <param name="oldInfoComputo"></param>
        /// <param name="newInfoComputo"></param>
        private void PreserveAccordionExpandedState(List<ComputoDto> newInfoComputo, List<string> lstLocalIndexes)
        {
            try
            {
                List<int> newExpandedIndices = new();

                if (lstLocalIndexes == null || !lstLocalIndexes.Any())
                    return;

                HashSet<string> expandedGroupNames = new HashSet<string>(lstLocalIndexes);

                var groupedNewInfoComputo = newInfoComputo
                    .GroupBy(c => c.NomeGruppo)
                    .ToList();

                for (int i = 0; i < groupedNewInfoComputo.Count; i++)
                {
                    var groupName = groupedNewInfoComputo[i].Key;

                    if (!string.IsNullOrEmpty(groupName) && expandedGroupNames.Contains(groupName))
                    {
                        newExpandedIndices.Add(i);
                    }
                }

                AccordionExpandedIndexes = newExpandedIndices.ToArray();

                // lstGruppiAccordionExpandend = newExpandedIndices.Select(i => groupedNewInfoComputo[i].Key).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Funzione per estrarre i nomi dei titoli degli accordion (gruppi), in base agli indici
        /// </summary>
        /// <param name="expandedIndices"></param>
        /// <param name="lstLocalIndexes"></param>
        /// <returns></returns>
        public List<string> GetExpandedGroupNames(int[] expandedIndices, List<string> lstLocalIndexes)
        {
            try
            {
                lstLocalIndexes.Clear();
                if (InfoComputo == null || !InfoComputo.Any())
                    return lstLocalIndexes;

                var groupedComputo = InfoComputo
                    .GroupBy(c => c.NomeGruppo)
                    .ToList();

                HashSet<string> uniqueGroupNames = new();

                foreach (var index in expandedIndices)
                {
                    if (index >= 0 && index < groupedComputo.Count)
                    {
                        var groupName = groupedComputo[index].Key;

                        if (!string.IsNullOrEmpty(groupName) && !uniqueGroupNames.Contains(groupName))
                        {
                            uniqueGroupNames.Add(groupName);
                            lstLocalIndexes.Add(groupName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return lstLocalIndexes;
        }

        /// <summary>
        /// Funzione che espande l'accordion in base '
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="codice"></param>
        private void ExpandGroupContainingCodice(ObservableCollection<ComputoDto> newInfoComputo, int[] arrayIndexes, List<string> lstStringGroupNames, string codice)
        {
            try
            {
                if (newInfoComputo == null || !newInfoComputo.Any())
                    return;

                var groupedComputo = newInfoComputo
                    .GroupBy(c => c.NomeGruppo)
                    .ToList();

                List<int> newExpandedIndices = arrayIndexes.ToList();
                bool foundMatch = false;

                for (int i = 0; i < groupedComputo.Count; i++)
                {
                    var group = groupedComputo[i];

                    if (group.Any(c => c.Codice == codice))
                    {
                        foundMatch = true;

                        if (!newExpandedIndices.Contains(i))
                        {
                            newExpandedIndices.Add(i);
                        }

                        if (!lstStringGroupNames.Contains(group?.Key))
                        {
                            lstStringGroupNames.Add(group.Key);
                        }
                    }
                }

                if (foundMatch)
                {
                    AccordionExpandedIndexes = newExpandedIndices.ToArray();

                    OnAccordionExpandedCollapsed(AccordionExpandedIndexes);
                }
                else
                {
                    Console.WriteLine("Nessun Codice trovato in nessun gruppo.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        private string ShowButtonBarByState(ButtonComputoNames buttonComputoNames)
        {
            try
            {
                switch (buttonComputoNames)
                {
                    case var expression when expression == ButtonComputoNames.Raggruppatori:
                        {
                            return FiltriRaggruppatoriInput != null ?
                            (FiltriRaggruppatoriInput.AttributiRaggruppatori == null ?
                                "e-customtoolbar-notactive-button" : FiltriRaggruppatoriInput.AttributiRaggruppatori.Any() ?
                                "e-customtoolbar-active-button" : "e-customtoolbar-notactive-button") : "e-customtoolbar-notactive-button";
                        }
                    case var expression when expression == ButtonComputoNames.Filtri:
                        {
                            return FiltriRaggruppatoriInput != null ?
                            (FiltriRaggruppatoriInput.AttributiFiltri == null ?
                                "e-customtoolbar-notactive-button" : FiltriRaggruppatoriInput.AttributiFiltri.Children == null ?
                                "e-customtoolbar-notactive-button" : FiltriRaggruppatoriInput.AttributiFiltri.Children.Any() ?
                                "e-customtoolbar-active-button" : "e-customtoolbar-notactive-button") : "e-customtoolbar-notactive-button";
                        }
                    case var expression when expression == ButtonComputoNames.Ifc:
                        {
                            return "e-customtoolbar-notactive-button";
                        }
                    case var expression when expression == ButtonComputoNames.Aggregati:
                        {
                            return FiltriRaggruppatoriInput != null ?
                            (FiltriRaggruppatoriInput.AttributiAggregati == null ?
                                "e-customtoolbar-notactive-button" : FiltriRaggruppatoriInput.AttributiAggregati.Any() ?
                                "e-customtoolbar-active-button" : "e-customtoolbar-notactive-button") : "e-customtoolbar-notactive-button";
                        }
                    default:
                        {
                            return "e-customtoolbar-notactive-button";
                        }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Errore di visualizzazione dei pulsanti di stato nella barra: {ex.Message}");
                return "e-customtoolbar-notactive-button";
            }
        }

        private string ShowButtonIconByState(ButtonComputoNames buttonComputoNames)
        {
            try
            {
                switch (buttonComputoNames)
                {
                    case var expression when expression == ButtonComputoNames.Raggruppatori:
                        {
                            return "badgegray";
                        }
                    case var expression when expression == ButtonComputoNames.Filtri:
                        {
                            return "badgegray";
                        }
                    case var expression when expression == ButtonComputoNames.Aggregati:
                        {
                            return FiltriRaggruppatoriInput != null ?
                            (FiltriRaggruppatoriInput.AttributiAggregati == null ?
                                "badgegray" : FiltriRaggruppatoriInput.AttributiAggregati.Any() ?
                                "badgered" : "badgegray") : "badgegray";
                        }
                    default:
                        {
                            return "badgegray";
                        }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Errore di rappresentazione dei pulsanti di stato nella barra: {ex.Message}");
                return "badgegray";
            }
        }

        private async Task ShowToast(string sMessage, string sTitle = "Avviso notifica", bool left = false)
        {
            try
            {
                sfToastContent = sMessage;
                await sfToastObj.ShowAsync(new ToastModel
                {
                    Content = sMessage,
                    Title = sTitle,
                    Icon = sfToastIcon,
                    Position = (left == true) ? (new ToastPosition { X = "Left", Y = $"Bottom" }) : (new ToastPosition { X = "Right", Y = "Bottom" }),
                    Timeout = sfToastMsTimeout,
                    ShowProgressBar = true,
                    Height = sfToastHeight,
                    Width = sfToastWidth,
                    ShowCloseButton = true,
                    ProgressDirection = ProgressDirection.LTR,
                    NewestOnTop = true,
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        private void OnToastClose(ToastBeforeCloseArgs args)
        {
            // Logica dopo la chiusura del messaggio, se necessaria
        }

        private async void WindowResized(object _, BrowserWindowSize window)
        {
            await SetContainerDim();
            ComboBoxRefs.Values.ToList<SfComboBox<AttributoRaggruppatoreDto, AttributoRaggruppatoreDto>>().ForEach(x => x.HidePopupAsync());
            StateHasChanged();
        }

        public void Dispose()
        {
            _listener.OnResized -= WindowResized;
        }

        private async Task SetContainerDim()
        {
            try
            {
                screenDimensions = await _jsRuntime.InvokeAsync<BrowserWindowSize>("window.getWindowDimensions");
                var comboPopupWidth = screenDimensions.Height > screenDimensions.Width ? 0.90 * screenDimensions.Width : 0.85 * screenDimensions.Width;
                var comboPopupHeight = screenDimensions.Height > screenDimensions.Width ? 0.50 * screenDimensions.Height : 0.70 * screenDimensions.Height;
                ComboPopupWidthFloored = $"{Math.Floor(comboPopupWidth)}px"; // Arrotonda il valore al numero intero più vicino
                ComboPopupHeightFloored = $"{Math.Floor(comboPopupHeight)}px"; // Arrotonda il valore al numero intero più vicino
            }
            catch (Exception ex)
            {
                Log.Error($"Errore di impostazione dimensioni popup combobox modulo filtro - condizione. Dettaglio eccezione: {ex.Message}");
            }
        }

        public enum PageComputoNames
        {
            Computo = 0,
            Raggruppatori = 1,
            Filtri = 2,
            Ifc = 3
        }

        public enum ButtonComputoNames
        {
            Raggruppatori = 0,
            Filtri = 1,
            Aggregati = 2,
            Ifc = 3,
        }

    }
}
