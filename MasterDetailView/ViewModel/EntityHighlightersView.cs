using CommonResources;
using Commons;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDetailView
{
    public class EntityHighlightersView : NotificationBase
    {
        public IDataService DataService { get; set; }
        ViewSettings _viewSettings = null;
        EntityType _entityType = null;
        List<Attributo> _attributi = null;
        EntitiesHelper _entitiesHelper = null;
        EntityHighlighters _entityHighlightersOnLoad = null;
        Dictionary<string, ColorInfo> _coloriMacchina = null;
        string NessunoLoc { get; set; }



        ObservableCollection<string> _attributiEtichetta = new ObservableCollection<string>();
        public ObservableCollection<string> AttributiEtichetta
        { 
            get => _attributiEtichetta;
            set => SetProperty(ref _attributiEtichetta, value);
        }

        ObservableCollection<EntityHighlighterView> _highlighters = new ObservableCollection<EntityHighlighterView>();
        public ObservableCollection<EntityHighlighterView> Highlighters
        {
            get => _highlighters;
            set => SetProperty(ref _highlighters, value);
        }

        ObservableCollection<HighlighterColor> _highlighterColors = new ObservableCollection<HighlighterColor>();
        public ObservableCollection<HighlighterColor> HighlighterColors
        {
            get => _highlighterColors;
            set => SetProperty(ref _highlighterColors, value);
        }



        public void Load(string entityTypeKey)
        {
            NessunoLoc = string.Format("<{0}>", LocalizationProvider.GetString("Nessuno"));

            _entitiesHelper = new EntitiesHelper(DataService);

            _coloriMacchina = ColorInfo.ColoriInstallatiInMacchina.ToDictionary(item => item.Name, item => item);

            LoadHighlighterColors();

            _viewSettings = DataService.GetViewSettings();
            _entityHighlightersOnLoad = _viewSettings.EntityTypes[entityTypeKey].EntityHighlighters;

            _entityType = DataService.GetEntityType(entityTypeKey);
            LoadAttributi();

            Attributo att = null;
            if (_entityHighlightersOnLoad != null)
                att = _attributi.FirstOrDefault(item => item != null && item.Codice == _entityHighlightersOnLoad.CodiceAttributo);

            if (att == null)
                att = _attributi.FirstOrDefault();

            _currentAttributoIndex = _attributi.IndexOf(att);
            SetCurrentAttributo(att, true);

            UpdateUI();



        }

        private void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => CurrentAttributoIndex));
            RaisePropertyChanged(GetPropertyName(() => IsAcceptButtonEnabled));
        }

        public void Accept()
        {
            EntityHighlighters entityHighlighters = null;
            Attributo att = _attributi[CurrentAttributoIndex];

            if (att != null)
            {
                entityHighlighters = new EntityHighlighters();
                entityHighlighters.CodiceAttributo = att.Codice;
                entityHighlighters.EntityTypeKey = _entityType.GetKey();

                entityHighlighters.Highlighters = Highlighters.Select(item => new ValoreHighlighter()
                {
                    Valore = item.Value,
                    Colore = new ValoreColore()
                    {
                        Hexadecimal = item.Color.HexValue,
                        V = item.Color.Text,
                    }
                }).ToDictionary(item => item.Valore, item => item);
            }  
            
            _viewSettings.EntityTypes[_entityType.GetKey()].EntityHighlighters = entityHighlighters;
            DataService.SetViewSettings(_viewSettings);
        } 
        
        private void LoadAttributi()
        {
            _attributi = _entityType.Attributi.Values.Where(item =>
            {
                if (item.IsInternal)
                    return false;

                if (item.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Riferimento)
                {
                    Attributo sourceAtt = _entitiesHelper.GetSourceAttributo(item);
                    if (!IsDefinizioneAttributoAllowed(sourceAtt))
                        return false;
                }
                else if (!IsDefinizioneAttributoAllowed(item))
                    return false;

                return true;

            }).OrderBy(item => item.DetailViewOrder).ToList();

            _attributi.Insert(0, null);

            AttributiEtichetta = new ObservableCollection<string>(_attributi.Select(item => (item == null)? NessunoLoc : item.Etichetta));

            
        }

        bool IsDefinizioneAttributoAllowed(Attributo att)
        {
            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Testo)
                return true;

            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Elenco)
                return true;

            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Reale)
                return true;

            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Contabilita)
                return true;

            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Colore)
                return true;

            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Data)
                return true;

            if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Booleano)
                return true;

            return false;

        }

        int _currentAttributoIndex = -1;
        public int CurrentAttributoIndex
        {
            get => _currentAttributoIndex;
            set
            {
                if (SetProperty(ref _currentAttributoIndex, value))
                {
                    Attributo currentAtt = _attributi[_currentAttributoIndex];
                    SetCurrentAttributo(currentAtt, false);
                }
            }
        }

        public void SetCurrentAttributo(Attributo att, bool isLoadingWnd)
        {
            LoadHighlighters(att);
        }

        async void LoadHighlighters(Attributo att)
        {
            List<EntityHighlighterView> items = new List<EntityHighlighterView>();

            if (att != null)
            {
                //if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Colore)
                //{
                //    Highlighters = new ObservableCollection<EntityHighlighterView>(items);
                //    return;
                //}

                List<Guid> entitiesFound = null;
                if (_entityType.IsTreeMaster)
                {   
                    //Prendo solo le foglie
                    List<TreeEntityMasterInfo> entsInfo = DataService.GetFilteredTreeEntities(_entityType.GetKey(), null, null, out entitiesFound);
                    HashSet<Guid> parentsId = new HashSet<Guid>(entsInfo.Select(item => item.ParentId));
                    entitiesFound.RemoveAll(item => parentsId.Contains(item));
                }
                else
                {
                    DataService.GetFilteredEntities(_entityType.GetKey(), null, null, null, out entitiesFound);
                }


                List<string> valoriUnivoci = await DataService.GetValoriUnivociAsync(_entityType.GetKey(), entitiesFound, att.Codice, 0, null);

                bool useCustomColors = false;
                if (valoriUnivoci.Count < ColorsHelper.DissimilarColors.Count)
                    useCustomColors = true;


                List<string> dissimilarColors = ColorsHelper.DissimilarColors;
                int dissimilarColorIndex = 1;

                HashSet<string> definedColorNames = new HashSet<string>();
                if (_entityHighlightersOnLoad != null && _entityHighlightersOnLoad.CodiceAttributo == att.Codice)
                {
                    definedColorNames = new HashSet<string>(_entityHighlightersOnLoad.Highlighters.Values.Select(item => item.Colore.PlainText));
                }


                foreach (string val in valoriUnivoci.OrderBy(item => item))
                {
                    EntityHighlighterView itemView = new EntityHighlighterView(this);
                    itemView.Value = val;

                    string colorName = null;
                    ValoreHighlighter valHighlighter = null;
                    if (_entityHighlightersOnLoad != null && _entityHighlightersOnLoad.CodiceAttributo == att.Codice && _entityHighlightersOnLoad.Highlighters.TryGetValue(val, out valHighlighter))
                    {
                        colorName = valHighlighter.Colore.PlainText;
                    }
                    else if (useCustomColors)
                    {
                        if (att.DefinizioneAttributoCodice == BuiltInCodes.DefinizioneAttributo.Colore)
                        {
                            if (string.IsNullOrEmpty(val))
                                colorName = MyColorsEnum.Transparent.ToString();
                            else
                                colorName = val;
                        }
                        else
                        {
                            //Prendo il primo colore dissimile che non è già stato utilizzato.
                            while (dissimilarColorIndex < dissimilarColors.Count)
                            {
                                string dissimilarColorName = dissimilarColors[dissimilarColorIndex];
                                if (!definedColorNames.Contains(dissimilarColorName))
                                    break;

                                dissimilarColorIndex++;
                            }
                            colorName = dissimilarColors[dissimilarColorIndex];
                        }
                    }
                    else
                    {
                        colorName = MyColorsEnum.Transparent.ToString();
                    }

                    definedColorNames.Add(colorName);

                    ColorInfo colorInfo = null;
                    if (_coloriMacchina.TryGetValue(colorName, out colorInfo))
                    {
                        itemView.Color = new HighlighterColor()
                        {
                            Text = colorInfo.Name,
                            HexValue = colorInfo.HexValue,
                            Color = colorInfo.Color,
                        };
                    }
                    items.Add(itemView);

                    
                }
            }
            Highlighters = new ObservableCollection<EntityHighlighterView>(items);

            //foreach (var item in Highlighters)
            //{
            //    HighlighterColor temp = item.Color;
            //    item.Color = null;
            //    item.Color = temp;
            //}

        }

        void LoadHighlighterColors()
        {
            List<HighlighterColor> lista = new List<HighlighterColor>();

            ColorInfo colInfo = null;
            //if (_coloriMacchina.TryGetValue(MyColorsEnum.Transparent.ToString(), out colInfo))
            //{
            //    HighlighterColor item = new HighlighterColor();
            //    item.HexValue = colInfo.HexValue;
            //    item.Text = colInfo.Name;
            //    item.Color = colInfo.Color;
            //    lista.Add(item);
            //}

            foreach (string colName in ColorsHelper.OrderedColorsName)
            {
                
                if (_coloriMacchina.TryGetValue(colName, out colInfo))
                {
                    HighlighterColor item = new HighlighterColor();
                    item.HexValue = colInfo.HexValue;
                    item.Text = colInfo.Name;
                    item.Color = colInfo.Color;
                    lista.Add(item);
                }
            }
            HighlighterColors = new ObservableCollection<HighlighterColor>(lista);
        }

        EntityHighlighterView _currentHighlighter = null;
        public EntityHighlighterView CurrentHighlighter
        {
            get
            {
                return _currentHighlighter;
            }
            set
            {
                if (SetProperty(ref _currentHighlighter, value))
                {
                    //_currentHighlighter.Color = _entityHighlightersOnLoad.Highlighters[_currentHighlighter.Value].Colore
                }
            }
        }

        public bool IsAcceptButtonEnabled
        {
            get
            {
                
                if (DataService != null && !DataService.IsReadOnly)
                    return true;

                return false;
            }
        }
    }

    public class EntityHighlighterView : NotificationBase
    {
        EntityHighlightersView _owner = null;
        public EntityHighlighterView(EntityHighlightersView owner)
        {
            _owner = owner;
        }

        public string Value { get; set; } = string.Empty;

        HighlighterColor _color = null;
        public HighlighterColor Color
        { 
            get => _color; 
            set => SetProperty(ref _color, value);
        }

        public ObservableCollection<HighlighterColor> HighlighterColors
        {
            get => _owner.HighlighterColors;
        }

        public void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => Color));
        }
          
    }

    public class HighlighterColor
    {
        public string Text { get; set; }
        public string HexValue { get; set; }
        public System.Windows.Media.Color Color { get; set; }
        public System.Windows.Media.SolidColorBrush SampleBrush { get => new System.Windows.Media.SolidColorBrush(Color); }
    }


}
