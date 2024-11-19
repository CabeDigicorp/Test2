using CommonResources;
using Commons;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MasterDetailView
{

    public class NumericFormatView : NotificationBase
    {
        public IDataService DataService { get; set; }
        public IMainOperation MainOperation { get; set; }
        public ModelActionsStack ModelActionsStack { get; set; }

        double ExampleNumber { get; } = 1234567.056;

        string _currentFormat = null;

        string _formatSpecialChars = "[0#.,]";

        

        public event EventHandler SelectionChanged;

        List<NumericFormat> _numericFormats = new List<NumericFormat>();

        ObservableCollection<NumericFormatItemView> _formatItemsView  = new ObservableCollection<NumericFormatItemView>();
        public ObservableCollection<NumericFormatItemView> FormatItemsView
        {
            get => _formatItemsView; 
            set { SetProperty(ref _formatItemsView, value); } 
        }

        public void Init()
        {
            if (DataService == null)
                return;

            NumericFormatHelper.UpdateCulture(IsCurrency);
            IsAddFormatPanelOpen = false;

            Load();
            
        }

        void Load()
        {
            _numericFormats = DataService.GetNumericFormats();

            _formatItemsView.Clear();
            foreach (NumericFormat format in _numericFormats)
            {
                _formatItemsView.Add(new NumericFormatItemView()
                {
                    Format = format.Format,
                    UserFormat = NumericFormatHelper.ConvertToUserFormat(format.Format),
                });
            }

            UpdateUI();
        }

        public void Commit()
        {
            //_numericFormats.Clear();

            
            //foreach (string format in UserFormatItems)
            //{
            //    string f = string.Empty;
            //    char exludedChar;
            //    NumberFormatHelpers.ConvertFromUserFormat(format, out f, out exludedChar);
            //    _numericFormats.Add(new NumericFormat() { Format = f });
            //}

            //foreach (NumericFormatItemView format in FormatItemsView)
            //{
            //    _numericFormats.Add(new NumericFormat() { Format = format.Format });
            //}

            DataService.SetNumericFormats(_numericFormats);

            ModelActionsStack.CommitAction(new ModelAction() { ActionName = ActionName.NUMERICFORMAT_COMMIT });
        }

        void UpdateUI()
        {
            RaisePropertyChanged(GetPropertyName(() => CurrentFormatUser));
            RaisePropertyChanged(GetPropertyName(() => FormattedExample));
            RaisePropertyChanged(GetPropertyName(() => DecimalDigitCount));
            RaisePropertyChanged(GetPropertyName(() => IsSymbolAtLeft));
            RaisePropertyChanged(GetPropertyName(() => IsSymbolSeparated));
            RaisePropertyChanged(GetPropertyName(() => UseThousandSeparator));
            RaisePropertyChanged(GetPropertyName(() => SymbolText));
            RaisePropertyChanged(GetPropertyName(() => LeftZeroCount));
            RaisePropertyChanged(GetPropertyName(() => ThousandSeparator));
            RaisePropertyChanged(GetPropertyName(() => IsAddFormatPanelOpen));
            RaisePropertyChanged(GetPropertyName(() => IsAnySelection));
            RaisePropertyChanged(GetPropertyName(() => IsSingleSelection));
            RaisePropertyChanged(GetPropertyName(() => SelectionMode));
            RaisePropertyChanged(GetPropertyName(() => FormatItemsView));



            //RaisePropertyChanged(GetPropertyName(() => FormatTextBoxToolTip));

        }

    public string CurrentFormat
        {
            get => _currentFormat;
            set
            {
                if (SetProperty(ref _currentFormat, value))
                {
                    Load(value);
                    UpdateUI();
                }
            }
        }

        public string CurrentFormatUser
        {
            get
            {
                
                string format = _currentFormat;
                format = NumericFormatHelper.ConvertToUserFormat(format);

                //string str = string.Format("{0}: {1}", LocalizationProvider.GetString("FormatoWindows"), format);
                return format;

            }

        }
        List<NumericFormatItemView> _selectedNumberFormats = new List<NumericFormatItemView>();
        public List<NumericFormatItemView> SelectedNumberFormats
        {
            get => _selectedNumberFormats;
            set
            {
                _selectedNumberFormats = value;
                OnSelectionChanged(new EventArgs());
            }
        }

        bool _isSingleSelection = false;
        public bool IsSingleSelection { get => _isSingleSelection;
            set
            {
                if (SetProperty(ref _isSingleSelection, value))
                {
                    if (_isSingleSelection)
                        SelectionMode = "Single";
                    else
                        SelectionMode = "Extended";
                };
            }
        }

        object _selectionMode = "Extended";//Extended, Multiple, Single
        public object SelectionMode
        {
            get { return _selectionMode; }
            set { SetProperty(ref _selectionMode, value); }
        }


        protected void OnSelectionChanged(EventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }

        public bool IsCurrency { get; set; }


        public int CurrentFormatIndex { get; set; } = -1;





        private ObservableCollection<string> _userformati = new ObservableCollection<string>();
        public ObservableCollection<string> UserFormatItems
        {
            get { return _userformati; }
            set { _userformati = value; }
        }

        public ICommand AddFormatCommand { get { return new CommandHandler(() => this.AddFormat()); } }
        private void AddFormat()
        {
            if (CurrentFormat == null || !CurrentFormat.Any())
                return;

            //if (CurrentFormatIndex >= 0 && CurrentFormatIndex < UserFormatItems.Count - 1)
            //    UserFormatItems.Insert(CurrentFormatIndex + 1, CurrentFormatUser);
            //else
            //    UserFormatItems.Add(CurrentFormatUser);

            if (_numericFormats.FirstOrDefault(item => item.Format == CurrentFormat) == null)
            {

                if (CurrentFormatIndex >= 0 && CurrentFormatIndex < _numericFormats.Count - 1)
                    _numericFormats.Insert(CurrentFormatIndex + 1, new NumericFormat() { Format = CurrentFormat });
                else
                    _numericFormats.Add(new NumericFormat() { Format = CurrentFormat });

                Load();

            }
            IsAddFormatPanelOpen = false;
            SelectedNumberFormats = new List<NumericFormatItemView>() { FormatItemsView.FirstOrDefault(item => item.Format == CurrentFormat) };

            Commit();            

            
        }
        public ICommand CancelAddFormatCommand { get { return new CommandHandler(() => this.CancelAddFormat()); } }
        void CancelAddFormat()
        {
            IsAddFormatPanelOpen = false;
        }

        public ICommand RemoveFormatCommand { get { return new CommandHandler(() => this.RemoveFormat()); } }
        private void RemoveFormat()
        {
            foreach (var item1 in SelectedNumberFormats)
            {
                _numericFormats.RemoveAll(item => item.Format == item1.Format);
            }

            //if (CurrentFormatIndex >= 0 && CurrentFormatIndex < _numericFormats.Count)
            //    _numericFormats.RemoveAt(CurrentFormatIndex);

            Load();
            Commit();
        }

        bool _isAddFormatPanelOpen = false;
        public bool IsAddFormatPanelOpen
        {
            get => _isAddFormatPanelOpen;
            set
            {
                if (SetProperty(ref _isAddFormatPanelOpen, value))
                {
                    if (_isAddFormatPanelOpen)
                    {
                        SelectedNumberFormats = new List<NumericFormatItemView>();
                        SetDefaultFormat();
                    }
                }
            }
        }

        private void SetDefaultFormat()
        {
            SymbolText = string.Empty;
            IsSymbolAtLeft = false;
            DecimalDigitCount = 2;
            LeftZeroCount = 1;
            UseThousandSeparator = true;
        }

        public bool IsAnySelection { get => SelectedNumberFormats.Any(); }

        public string FormattedExample
        {
            get
            {
                if (_currentFormat == null || !_currentFormat.Any())
                    return string.Empty;

                string fullFormat = string.Format("{0}0:{1}{2}", "{", _currentFormat, "}");
                string formattedExampleNum = string.Format(fullFormat, ExampleNumber);

                string str = string.Format("{0}: {1}", LocalizationProvider.GetString("Esempio"), formattedExampleNum);
                return str;
            }
        }

        bool _isSymbolAtLeft = false;
        public bool IsSymbolAtLeft
        {
            get => _isSymbolAtLeft;
            set
            {
                if (SetProperty(ref _isSymbolAtLeft, value))
                    CurrentFormat = ComposeFormat();
            }
        }

        bool _isSymbolSeparated = false;
        public bool IsSymbolSeparated
        {
            get => _isSymbolSeparated;
            set
            {
                if (SetProperty(ref _isSymbolSeparated, value))
                    CurrentFormat = ComposeFormat();
            }
        }

        bool _useThousandSeparator = false;
        public bool UseThousandSeparator
        {
            get => _useThousandSeparator;
            set
            {
                if (SetProperty(ref _useThousandSeparator, value))
                    CurrentFormat = ComposeFormat();
            }
        }

        
        public string ThousandSeparator
        {
            get => string.Format(" ({0})", NumericFormatHelper.CurrentGroupSeparator);
        }


        string _symbolText = string.Empty;
        public string SymbolText
        {
            get => _symbolText;
            set
            {
                string str = Regex.Replace(value, _formatSpecialChars, string.Empty);

                if (SetProperty(ref _symbolText, str))
                    CurrentFormat = ComposeFormat();
            }
        }

        int _leftZeroCount = 0;
        public int LeftZeroCount
        {
            get => _leftZeroCount;
            set
            {
                if (SetProperty(ref _leftZeroCount, Math.Abs(value)))
                    CurrentFormat = ComposeFormat();
            }
        }

        int _decimalDigitCount = 0;
        public int DecimalDigitCount
        {
            get => _decimalDigitCount;
            set
            {
                if (SetProperty(ref _decimalDigitCount, Math.Abs(value)))
                    CurrentFormat = ComposeFormat();
            }
        }

        

        string ComposeFormat()
        {
            NumberFormat numberFormat = new NumberFormat()
            {
                IsSymbolAtLeft = _isSymbolAtLeft,
                IsSymbolSeparated = _isSymbolSeparated,
                UseThousandSeparator = _useThousandSeparator,
                SymbolText = _symbolText,
                LeftZeroCount = _leftZeroCount,
                DecimalDigitCount = _decimalDigitCount,
            };

            return NumericFormatHelper.ComposeFormat(numberFormat);
        }

        void Load(string format)
        {
            if (format == null )
                return;

            NumberFormat numberFormat = NumericFormatHelper.DecomposeFormat(format);

            if (numberFormat == null)
                return;

            _isSymbolAtLeft = numberFormat.IsSymbolAtLeft;
            _useThousandSeparator = numberFormat.UseThousandSeparator;
            _symbolText = numberFormat.SymbolText;
            _leftZeroCount = numberFormat.LeftZeroCount;
            _decimalDigitCount = numberFormat.DecimalDigitCount;
        }

        public List<string> GetSelectedNumericFormats()
        {
            return SelectedNumberFormats.Select(item => item.Format).ToList();
        }

        public ICommand PasteClipboardCommand { get { return new CommandHandler(() => this.PasteClipboard()); } }
        void PasteClipboard()
        {
            IDataObject dataObject = Clipboard.GetDataObject();


            if (dataObject.GetDataPresent(DataFormats.Text))
            {

                string text = dataObject.GetData(DataFormats.Text) as string;
                string[] lines = Regex.Split(text, "\r\n|\r|\n");
                

                foreach (string line in lines)
                {
                    if (!line.Trim().Any())
                        continue;

                    bool isFormatValid = false;

                    NumericFormatHelper.IsCurrency = NumericFormatHelper.IsFormatCurrency(line);

                    string format;
                    if (NumericFormatHelper.ConvertFromUserFormat(line, out format, out _))
                    {
                        NumberFormat nf = NumericFormatHelper.DecomposeFormat(format);
                        if (nf != null)
                        {
                            isFormatValid = true;
                            if (_numericFormats.FirstOrDefault(item => item.Format == format) == null)
                                _numericFormats.Add(new NumericFormat() { Format = format });
                        }
                    }

                    if (!isFormatValid)
                    {
                        MessageBox.Show(string.Format("{0}: {1}", LocalizationProvider.GetString("FormatoNonValido"), line));
                        break;
                    }
                }

                Load();

                Commit();

            }
        }

        public ICommand CopyClipboardCommand { get { return new CommandHandler(() => this.CopyClipboard()); } }
        void CopyClipboard()
        {
            Clipboard.Clear();
            DataObject dataObject = new DataObject();

            string text = string.Empty;

            foreach (NumericFormatItemView item in SelectedNumberFormats)
            {
                text += item.UserFormat;
                text += "\r\n";
            }

            dataObject.SetData(DataFormats.Text, text);
            Clipboard.SetDataObject(dataObject);
        }
    }

    public class NumericFormatItemView
    {
        public string Format { get; set; }
        public string UserFormat { get; set; }
    }
}
