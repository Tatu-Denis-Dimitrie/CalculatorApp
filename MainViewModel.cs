using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using static System.Windows.Clipboard;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace CalculatorApp
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private const string CONFIG_FILE = "calculator_config.json";
        private bool _isDigitGroupingEnabled = true;
        private bool _respectOperationOrder = false;
        private List<string> _operationsList = new List<string>();
        private List<double> _valuesList = new List<double>();

        //Standard
        private double _memory = 0;
        private string _display = "0";
        private double _lastValue = 0;
        private string _operation = "";
        private bool _isNewEntry = true;
        private string _currentNumberSystem = "Dec";

        //Programmer mode variables
        private double _lastValueProg = 0;
        private string _operationProg = "";
        private bool _isNewEntryProg = true;

        private bool _isMemoryPanelVisible = false;
        private bool _isMenuVisible=false;

        private bool _isProgrammerModeVisible;
        private bool _isStandardModeVisible;
        private string _currentMode;

        private ObservableCollection<double> _memoryStack = new ObservableCollection<double>();
        private double? _selectedMemoryValue;
        private int _currentMemoryIndex = -1;

        //Standard
        public ICommand NumberCommand { get; }
        public ICommand ToggleDigitGroupingCommand { get; }
        public ICommand OperationCommand { get; }
        public ICommand EqualsCommand { get; }
        public ICommand CE { get; }
        public ICommand C { get; }
        public ICommand Mod { get; }
        public ICommand DeleteLastChar { get; }
        public ICommand SquareRootCommand { get; }
        public ICommand SquareCommand { get; }
        public ICommand ReciprocalCommand { get; }
        public ICommand Negation { get; }
        public ICommand MemoryClear { get; }
        public ICommand MemoryR { get; }
        public ICommand MemoryAdd { get; }
        public ICommand MemoryRemove { get; }
        public ICommand MemorySave { get; }
        public ICommand ShowMemoryStack { get; }
        public ICommand ShowMenu { get; }
        public ICommand SelectMemoryValue { get; }
        public ICommand CloseMemoryPanelCommand { get; }
        public ICommand CloseMenuCommand { get; }
        public ICommand AboutMenuItem_Click { get; }
        public ICommand DecimalCommand { get; }

        public ICommand SwitchToStandardCommand { get; }
        public ICommand SwitchToProgrammerCommand { get; }

        // Programmer
        public ICommand CharacterCommand { get; }
        public ICommand ShiftLeftCommand { get; }
        public ICommand ShiftRightCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand OpenParenthesisCommand { get; }
        public ICommand CloseParenthesisCommand { get; }
        public ICommand ModuloCommand { get; }
        public ICommand DivideCommand { get; }
        public ICommand MultiplyCommand { get; }
        public ICommand SubtractCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand ToggleSignCommand { get; }
        public ICommand DecimalCommandProg { get; }
        public ICommand EqualsCommandProg { get; }
        public ICommand NumberCommandProg { get; private set; }
        public ICommand CommandC { get; private set; }
        public ICommand SelectHexCommand { get; private set; }
        public ICommand SelectDecCommand { get; private set; }
        public ICommand SelectOctCommand { get; private set; }
        public ICommand SelectBinCommand { get; private set; }

        public ICommand CutCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand PasteCommand { get; }

        public ICommand ToggleOperationOrderCommand { get; }

        public bool RespectOperationOrder
        {
            get => _respectOperationOrder;
            set
            {
                if (_respectOperationOrder != value)
                {
                    _respectOperationOrder = value;
                    OnPropertyChanged(nameof(RespectOperationOrder));
                    SaveSettings();
                }
            }
        }

        public string Display
        {
            get => _display;
            set
            {
                if (_display != value)
                {
                    _display = value.ToUpper();
                    OnPropertyChanged(nameof(Display));
                    OnPropertyChanged(nameof(HexDisplay));
                    OnPropertyChanged(nameof(DecDisplay));
                    OnPropertyChanged(nameof(OctDisplay));
                    OnPropertyChanged(nameof(BinDisplay));
                }
            }
        }
        public string HexDisplay => ConvertToBase(16);
        public string DecDisplay => ConvertToBase(10);
        public string OctDisplay => ConvertToBase(8);
        public string BinDisplay => ConvertToBase(2);
        public string CurrentNumberSystem
        {
            get { return _currentNumberSystem; }
            set
            {
                _currentNumberSystem = value;
                OnPropertyChanged(nameof(CurrentNumberSystem));
            }
        }
        private string ConvertToBase(int numBase)
        {
            try
            {
                int decimalValue = 0;

                if (CurrentNumberSystem == "Bin")
                {
                    decimalValue = Convert.ToInt32(Display, 2); 
                }
                else if (CurrentNumberSystem == "Dec")
                {
                    decimalValue = int.Parse(Display); 
                }
                else if (CurrentNumberSystem == "Hex")
                {
                    decimalValue = Convert.ToInt32(Display, 16); 
                }
                else if (CurrentNumberSystem == "Oct")
                {
                    decimalValue = Convert.ToInt32(Display, 8); 
                }

                return Convert.ToString(decimalValue, numBase).ToUpper();
            }
            catch
            {
                return "ERR";
            }
        }

        public bool IsMemoryPanelVisible
        {
            get => _isMemoryPanelVisible;
            set
            {
                _isMemoryPanelVisible = value;
                OnPropertyChanged(nameof(IsMemoryPanelVisible));
            }
        }
        public bool IsMenuVisible
        {
            get => _isMenuVisible;
            set
            {
                _isMenuVisible = value;
                OnPropertyChanged(nameof(IsMenuVisible));
            }
        }

        public ObservableCollection<double> MemoryStack
        {
            get { return _memoryStack; }
            set
            {
                if (_memoryStack != value)
                {
                    _memoryStack = value;
                    OnPropertyChanged(nameof(MemoryStack));
                }
            }
        }

        public double? SelectedMemoryValue
        {
            get => _selectedMemoryValue;
            set
            {
                if (value.HasValue)
                {
                    Display = FormatNumber(value.Value);
                    _memory = value.Value;
                    
                    _currentMemoryIndex = _memoryStack.IndexOf(value.Value);
                    
                    _isNewEntry = true;

                    var timer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(50)
                    };

                    timer.Tick += (sender, e) =>
                    {
                        _selectedMemoryValue = null;
                        OnPropertyChanged(nameof(SelectedMemoryValue));
                        timer.Stop();
                    };
                    timer.Start();
                }
                IsMemoryPanelVisible = false;   
                _selectedMemoryValue = value;
                OnPropertyChanged(nameof(SelectedMemoryValue));
            }
        }
        public bool IsStandardModeVisible
        {
            get { return _isStandardModeVisible; }
            set
            {
                if (_isStandardModeVisible != value)
                {
                    _isStandardModeVisible = value;
                    OnPropertyChanged(nameof(IsStandardModeVisible));
                }
            }
        }
        public bool IsProgrammerModeVisible
        {
            get { return _isProgrammerModeVisible; }
            set
            {
                if (_isProgrammerModeVisible != value)
                {
                    _isProgrammerModeVisible = value;
                    OnPropertyChanged(nameof(IsProgrammerModeVisible));
                }
            }
        }
        public string CurrentMode
        {
            get { return _currentMode; }
            set
            {
                if (_currentMode != value)
                {
                    _currentMode = value;
                    OnPropertyChanged(nameof(CurrentMode));
                }
            }
        }

        public MainViewModel()
        {
            LoadSettings();
            
            // Commands initialization
            NumberCommand = new RelayCommand(param => AddNumber(param.ToString()));
            ToggleDigitGroupingCommand = new RelayCommand(_ => ToggleDigitGrouping());
            OperationCommand = new RelayCommand(param => SetOperation(param.ToString()));
            EqualsCommand = new RelayCommand(_ => CalculateResult());
            CE = new RelayCommand(_ => { Display = "0"; _isNewEntry = true; });
            C = new RelayCommand(_ => { Display = "0"; _lastValue = 0; _operation = ""; _isNewEntry = true; });
            Mod = new RelayCommand(_ => Display = FormatNumber(double.Parse(Display) / 100));
            DeleteLastChar = new RelayCommand(_ => DeleteLastCharacter());
            SquareRootCommand = new RelayCommand(_ => Display = FormatNumber(Math.Sqrt(double.Parse(Display))));
            SquareCommand = new RelayCommand(_ => Display = FormatNumber(Math.Pow(double.Parse(Display), 2)));
            ReciprocalCommand = new RelayCommand(_ => Display = FormatNumber(1 / double.Parse(Display)));
            Negation = new RelayCommand(_ => Display = FormatNumber(-double.Parse(Display)));
            DecimalCommand = new RelayCommand(_ => AddNumber(","));

            // Memory commands
            MemoryClear = new RelayCommand(_ => ClearMemory());
            MemoryAdd = new RelayCommand(_ => AddToMemory());
            MemoryRemove = new RelayCommand(_ => RemoveFromMemory());
            MemorySave = new RelayCommand(_ => SaveToMemory());
            MemoryR = new RelayCommand(_ => RecallMemory());
            ShowMemoryStack = new RelayCommand(_ => ToggleMemoryPanel());
            SelectMemoryValue = new RelayCommand(_ => SelectMemory());
            CloseMemoryPanelCommand = new RelayCommand(_ => IsMemoryPanelVisible = false);
            //Menu stuff
            CloseMenuCommand = new RelayCommand(_ => IsMenuVisible = false);
            ShowMenu = new RelayCommand(_ => ToggleMenuPanel());

            // About command
            AboutMenuItem_Click = new RelayCommand(ShowAbout);
            //Comutare prog stand
            SwitchToStandardCommand = new RelayCommand(_ => SwitchToStandardMode());
            SwitchToProgrammerCommand = new RelayCommand(_ => SwitchToProgrammerMode());
            
            //Programmer commands
            CharacterCommand = new RelayCommand(param => AddCharacter(param.ToString()));
            ShiftLeftCommand = new RelayCommand(_ =>ShiftLeft());
            ShiftRightCommand = new RelayCommand(_ => ShiftRight());
            DeleteCommand = new RelayCommand(_ => DeleteCharacter());
            OpenParenthesisCommand = new RelayCommand(_ => AddCharacter("("));
            CloseParenthesisCommand = new RelayCommand(_ => AddCharacter(")"));
            ModuloCommand = new RelayCommand(_ => SetOperationProg("%"));
            DivideCommand = new RelayCommand(_ => SetOperationProg("/"));
            MultiplyCommand = new RelayCommand(_ => SetOperationProg("*"));
            SubtractCommand = new RelayCommand(_ => SetOperationProg("-"));
            AddCommand = new RelayCommand(_ => SetOperationProg("+"));
            ToggleSignCommand = new RelayCommand(_ => ToggleSign());
            DecimalCommandProg = new RelayCommand(_ => AddCharacter("."));
            EqualsCommandProg = new RelayCommand(_ => CalculateResultProg());
            NumberCommandProg = new RelayCommand(param => AddNumberProg(param.ToString()));
            CommandC = new RelayCommand(_ => { Display = "0"; _lastValueProg = 0; _operationProg = ""; _isNewEntryProg = true; });
            SelectHexCommand = new RelayCommand(_ => { CurrentNumberSystem = "Hex"; SaveSettings(); });
            SelectDecCommand = new RelayCommand(_ => { CurrentNumberSystem = "Dec"; SaveSettings(); });
            SelectOctCommand = new RelayCommand(_ => { CurrentNumberSystem = "Oct"; SaveSettings(); });
            SelectBinCommand = new RelayCommand(_ => { CurrentNumberSystem = "Bin"; SaveSettings(); });

            CutCommand = new RelayCommand(_ => CutToClipboard());
            CopyCommand = new RelayCommand(_ => CopyToClipboard());
            PasteCommand = new RelayCommand(_ => PasteFromClipboard());

            ToggleOperationOrderCommand = new RelayCommand(_ => RespectOperationOrder = !RespectOperationOrder);
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(CONFIG_FILE))
                {
                    string jsonString = File.ReadAllText(CONFIG_FILE);
                    var config = JsonSerializer.Deserialize<CalculatorConfig>(jsonString);
                    _isDigitGroupingEnabled = config.IsDigitGroupingEnabled;
                    
                    if (!string.IsNullOrEmpty(config.LastMode))
                    {
                        if (config.LastMode == "Programmer")
                        {
                            IsStandardModeVisible = false;
                            IsProgrammerModeVisible = true;
                            CurrentMode = "Programmer";
                            
                            if (!string.IsNullOrEmpty(config.LastNumberSystem))
                            {
                                CurrentNumberSystem = config.LastNumberSystem;
                            }
                        }
                        else
                        {
                            IsStandardModeVisible = true;
                            IsProgrammerModeVisible = false;
                            CurrentMode = "Standard";
                        }
                    }
                    else
                    {
                        IsStandardModeVisible = true;
                        IsProgrammerModeVisible = false;
                        CurrentMode = "Standard";
                    }
                }
                else
                {
                    IsStandardModeVisible = true;
                    IsProgrammerModeVisible = false;
                    CurrentMode = "Standard";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la incarcarea setarilor: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                
                IsStandardModeVisible = true;
                IsProgrammerModeVisible = false;
                CurrentMode = "Standard";
            }
        }

        private void SaveSettings()
        {
            try
            {
                var config = new CalculatorConfig 
                { 
                    IsDigitGroupingEnabled = _isDigitGroupingEnabled,
                    LastMode = CurrentMode,
                    LastNumberSystem = CurrentNumberSystem
                };
                
                string jsonString = JsonSerializer.Serialize(config);
                File.WriteAllText(CONFIG_FILE, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la salvarea setarilor: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleDigitGrouping()
        {
            _isDigitGroupingEnabled = !_isDigitGroupingEnabled;
            SaveSettings();

            string numericDisplay = Display.Replace(".", "");

            if (double.TryParse(numericDisplay, NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
            {
                Display = FormatNumber(number);
            }
        }

        private void AddNumber(string number)
        {
            if (_isNewEntry)
            {
                Display = number == "," ? "0," : number;
                _isNewEntry = false;
            }
            else
            {
                if (number == "," && !Display.Contains(","))
                {
                    Display += ",";
                }
                else if (number != ",")
                {
                    string cleanDisplay = Display.Replace(".", "");
                    cleanDisplay += number;
                    Display = cleanDisplay;
                }
            }
            
            if (!Display.EndsWith(",") && _isDigitGroupingEnabled)
            {
                if (Display.Contains(","))
                {
                    var parts = Display.Split(',');
                    if (double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double integerPart))
                    {
                        string formattedInteger = integerPart.ToString("#,0", new CultureInfo("ro-RO"));
                        Display = formattedInteger + "," + parts[1];
                    }
                }
                else
                {
                    if (double.TryParse(Display.Replace(".", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                    {
                        Display = value.ToString("#,0", new CultureInfo("ro-RO"));
                    }
                }
            }
        }
        private void SetOperation(string operation)
        {
            try
            {
                double parsedValue;
                string cleanDisplay = Display.Replace(".", "");
                
                if (cleanDisplay.Contains(","))
                {
                    parsedValue = double.Parse(cleanDisplay.Replace(",", "."), CultureInfo.InvariantCulture);
                }
                else
                {
                    parsedValue = double.Parse(cleanDisplay, CultureInfo.InvariantCulture);
                }

                if (_respectOperationOrder)
                {
                    if (_valuesList.Count == 0 || _isNewEntry)
                    {
                        _valuesList.Add(parsedValue);
                    }
                    else if (_operationsList.Count == _valuesList.Count)
                    {
                        _valuesList[_valuesList.Count - 1] = parsedValue;
                    }
                    else
                    {
                        _valuesList.Add(parsedValue);
                    }
                    
                    _operationsList.Add(operation);
                }
                else
                {
                    if (_operation != "" && !_isNewEntry)
                    {
                        double result = _lastValue;
                        switch (_operation)
                        {
                            case "+": result += parsedValue; break;
                            case "-": result -= parsedValue; break;
                            case "*": result *= parsedValue; break;
                            case "/": 
                                if (parsedValue == 0)
                                {
                                    MessageBox.Show("Nu se poate imparti la zero!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                                result /= parsedValue; 
                                break;
                        }
                        
                        Display = FormatNumber(result);
                        _lastValue = result;
                    }
                    else
                    {
                        _lastValue = parsedValue;
                    }
                    
                    _operation = operation;
                }
                
                _isNewEntry = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"A aparut o eroare: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                Display = "0";
                _lastValue = 0;
                _operation = "";
                _isNewEntry = true;
                _valuesList.Clear();
                _operationsList.Clear();
            }
        }

        private void CalculateResult()
        {
            if (_respectOperationOrder)
            {
                CalculateWithOperationOrder();
                return;
            }

            if (_operation == "") return;

            try
            {
                double currentValue;
                string cleanDisplay = Display.Replace(".", "");
                
                if (cleanDisplay.Contains(","))
                {
                    currentValue = double.Parse(cleanDisplay.Replace(",", "."), CultureInfo.InvariantCulture);
                }
                else
                {
                    currentValue = double.Parse(cleanDisplay, CultureInfo.InvariantCulture);
                }

                double result = _lastValue;

                switch (_operation)
                {
                    case "+": result += currentValue; break;
                    case "-": result -= currentValue; break;
                    case "*": result *= currentValue; break;
                    case "/": 
                        if (currentValue == 0)
                        {
                            MessageBox.Show("Nu se poate imparti la zero!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        result /= currentValue; 
                        break;
                }

                Display = FormatNumber(result);
                _lastValue = result;
                _operation = "";
                _isNewEntry = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"A aparut o eroare: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                Display = "0";
                _lastValue = 0;
                _operation = "";
                _isNewEntry = true;
            }
        }

        private void CalculateWithOperationOrder()
        {
            try
            {
                double lastValue;
                string cleanDisplay = Display.Replace(".", "");
                
                if (cleanDisplay.Contains(","))
                {
                    lastValue = double.Parse(cleanDisplay.Replace(",", "."), CultureInfo.InvariantCulture);
                }
                else
                {
                    lastValue = double.Parse(cleanDisplay, CultureInfo.InvariantCulture);
                }

                if (_valuesList.Count == 0)
                {
                    Display = FormatNumber(lastValue);
                    _isNewEntry = true;
                    return;
                }

                if (_valuesList.Count == 1 && _operationsList.Count == 0)
                {
                    _valuesList[0] = lastValue;
                    Display = FormatNumber(lastValue);
                    _isNewEntry = true;
                    return;
                }

                if (_valuesList.Count <= _operationsList.Count)
                {
                    _valuesList.Add(lastValue);
                }
                else
                {
                    _valuesList[_valuesList.Count - 1] = lastValue;
                }

                List<double> tempValues = new List<double>(_valuesList);
                List<string> tempOperations = new List<string>(_operationsList);

                for (int i = 0; i < tempOperations.Count;)
                {
                    if (tempOperations[i] == "*" || tempOperations[i] == "/")
                    {
                        double result;
                        
                        if (tempOperations[i] == "*")
                        {
                            result = tempValues[i] * tempValues[i + 1];
                        }
                        else // "/"
                        {
                            if (tempValues[i + 1] == 0)
                            {
                                MessageBox.Show("Nu se poate imparti la zero!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            result = tempValues[i] / tempValues[i + 1];
                        }

                        tempValues[i] = result;
                        tempValues.RemoveAt(i + 1);
                        tempOperations.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }

                double finalResult = tempValues[0];
                for (int i = 0; i < tempOperations.Count; i++)
                {
                    if (tempOperations[i] == "+")
                    {
                        finalResult += tempValues[i + 1];
                    }
                    else if (tempOperations[i] == "-")
                    {
                        finalResult -= tempValues[i + 1];
                    }
                }

                Display = FormatNumber(finalResult);
                
                _valuesList.Clear();
                _valuesList.Add(finalResult);
                _operationsList.Clear();
                
                _lastValue = finalResult;
                _isNewEntry = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"A aparut o eroare la calcularea rezultatului: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                Display = "0";
                _lastValue = 0;
                _valuesList.Clear();
                _operationsList.Clear();
                _isNewEntry = true;
            }
        }

        public void DeleteLastCharacter()
        {
            if (Display.Length > 1)
                Display = Display.Remove(Display.Length - 1);
            else
                Display = "0";
            FormatDisplay();
        }

        private void FormatDisplay()
        {
            if (CurrentNumberSystem == "Dec")
            {
                if (Display.Contains(","))
                {
                    var parts = Display.Split(',');
                    Display = FormatNumber(double.Parse(parts[0])) + "," + parts[1];
                }
                else
                {
                    Display = FormatNumber(double.Parse(Display));
                }
            }
            else
            {
                Display = Display.ToUpper();
            }
        }


        private void AddToMemory()
        {
            try
            {
                double currentValue;
                string cleanDisplay = Display;
                
                if (cleanDisplay.Contains(","))
                {
                    currentValue = double.Parse(cleanDisplay.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                }
                else
                {
                    currentValue = double.Parse(cleanDisplay.Replace(".", ""), CultureInfo.InvariantCulture);
                }
                
                double newValue = _memory + currentValue;
                _memory = newValue;
                Display = FormatNumber(newValue);
                
                if (_currentMemoryIndex >= 0 && _currentMemoryIndex < _memoryStack.Count)
                {
                    _memoryStack[_currentMemoryIndex] = newValue;
                    OnPropertyChanged(nameof(MemoryStack));
                }
                else 
                {
                    _memoryStack.Add(newValue);
                    _currentMemoryIndex = _memoryStack.Count - 1;
                    OnPropertyChanged(nameof(MemoryStack));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"A aparut o eroare: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveToMemory()
        {
            try
            {
                double currentValue;
                string cleanDisplay = Display;
                
                if (cleanDisplay.Contains(","))
                {
                    currentValue = double.Parse(cleanDisplay.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                }
                else
                {
                    currentValue = double.Parse(cleanDisplay.Replace(".", ""), CultureInfo.InvariantCulture);
                }
                
                _memory = currentValue;
                
                _memoryStack.Add(currentValue);
                _currentMemoryIndex = _memoryStack.Count - 1;
                OnPropertyChanged(nameof(MemoryStack));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"A aparut o eroare: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectMemory()
        {
            if (SelectedMemoryValue.HasValue)
            {
                Display = FormatNumber(SelectedMemoryValue.Value);
                IsMemoryPanelVisible = false;
            }
        }

        private void ClearMemory()
        {
            _memoryStack.Clear();
            Display = "0";
        }

        private void RecallMemory()
        {
            if (_memoryStack.Count > 0)
            {
                if (_currentMemoryIndex >= 0 && _currentMemoryIndex < _memoryStack.Count)
                {
                    Display = FormatNumber(_memoryStack[_currentMemoryIndex]);
                    _memory = _memoryStack[_currentMemoryIndex];
                }
                else
                {
                    Display = FormatNumber(_memoryStack.Last());
                    _memory = _memoryStack.Last();
                    _currentMemoryIndex = _memoryStack.Count - 1;
                }
                _isNewEntry = true;
            }
            else
            {
                Display = "0";
            }
        }

        private void RemoveFromMemory()
        {
            try
            {
                double currentValue;
                string cleanDisplay = Display;
                
                if (cleanDisplay.Contains(","))
                {
                    currentValue = double.Parse(cleanDisplay.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                }
                else
                {
                    currentValue = double.Parse(cleanDisplay.Replace(".", ""), CultureInfo.InvariantCulture);
                }
                
                double newValue = _memory - currentValue;
                _memory = newValue;
                Display = FormatNumber(newValue);
                
                if (_currentMemoryIndex >= 0 && _currentMemoryIndex < _memoryStack.Count)
                {
                    _memoryStack[_currentMemoryIndex] = newValue;
                    OnPropertyChanged(nameof(MemoryStack));
                }
                else 
                {
                    _memoryStack.Add(newValue);
                    _currentMemoryIndex = _memoryStack.Count - 1;
                    OnPropertyChanged(nameof(MemoryStack));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"A aparut o eroare: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleMemoryPanel()
        {
            IsMemoryPanelVisible = !IsMemoryPanelVisible;
        }
        private void ToggleMenuPanel()
        {
            IsMenuVisible = !IsMenuVisible;
        }

        private string FormatNumber(double number)
        {
            CultureInfo culture = new CultureInfo("ro-RO");

            if (_isDigitGroupingEnabled)
            {
                return number.ToString("#,0.############", culture);
            }
            else
            {
                string formatted = number.ToString("0.############", culture);
                return formatted.Replace(".", ",");
            }
        }

        private void ShowAbout(object obj)
        {
            MessageBox.Show("Made by Gimishoor (TDD)", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public void SwitchToStandardMode()
        {
            IsStandardModeVisible = true;
            IsProgrammerModeVisible = false;
            CurrentMode = "Standard";
            IsMenuVisible = false;
            SaveSettings();
        }

        public void SwitchToProgrammerMode()
        {
            IsStandardModeVisible = false;
            IsProgrammerModeVisible = true;
            CurrentMode = "Programmer";
            IsMenuVisible = false;
            SaveSettings();
        }
        //Programmer
        private bool IsValidNumberForCurrentBase(string number)
        {
            switch (CurrentNumberSystem)
            {
                case "Hex":
                    return "0123456789ABCDEF".Contains(number.ToUpper());
                case "Dec":
                    return "0123456789".Contains(number);
                case "Oct":
                    return "01234567".Contains(number);
                case "Bin":
                    return "01".Contains(number);
                default:
                    return false;
            }
        }
        private void AddCharacter(string character)
        {
            if (!IsValidNumberForCurrentBase(character))
            {
                return;
            }

            try
            {
                if (_isNewEntryProg)
                {
                    Display = character;
                    _isNewEntryProg = false;
                }
                else
                {
                    Display += character;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void ShiftLeft()
        {
           
        }

        private void ShiftRight()
        {
            
        }

        private void DeleteCharacter()
        {
            if (Display.Length > 1)
                Display = Display.Remove(Display.Length - 1);
            else
                Display = "0";
            FormatDisplay();
        }

        private void ToggleSign()
        {
            if (Display.StartsWith("-"))
                Display = Display.Substring(1);
            else
                Display = "-" + Display;
        }

        private void CalculateResultProg()
        {
            if (_operationProg == "") return;
            try
            {
                double currentValue;
                string cleanDisplay = Display.Replace(".", "");
                
                if (cleanDisplay.Contains(","))
                {
                    currentValue = double.Parse(cleanDisplay.Replace(",", "."), CultureInfo.InvariantCulture);
                }
                else
                {
                    switch (CurrentNumberSystem)
                    {
                        case "Bin":
                            currentValue = Convert.ToInt32(cleanDisplay, 2);
                            break;
                        case "Hex":
                            currentValue = Convert.ToInt32(cleanDisplay, 16);
                            break;
                        case "Oct":
                            currentValue = Convert.ToInt32(cleanDisplay, 8);
                            break;
                        default:
                            currentValue = double.Parse(cleanDisplay, CultureInfo.InvariantCulture);
                            break;
                    }
                }

                double result = _lastValueProg;
                switch (_operationProg)
                {
                    case "+": result += currentValue; break;
                    case "-": result -= currentValue; break;
                    case "*": result *= currentValue; break;
                    case "/": 
                        if (currentValue == 0)
                        {
                            MessageBox.Show("Nu se poate imparti la zero!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        result /= currentValue; 
                        break;
                    case "%": 
                        if (currentValue == 0)
                        {
                            MessageBox.Show("Nu se poate calcula modulo cu zero!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        result %= currentValue; 
                        break;
                }

                switch (CurrentNumberSystem)
                {
                    case "Hex":
                        Display = Convert.ToString((int)result, 16).ToUpper();
                        break;
                    case "Dec":
                        Display = FormatNumber(result);
                        break;
                    case "Oct":
                        Display = Convert.ToString((int)result, 8);
                        break;
                    case "Bin":
                        Display = Convert.ToString((int)result, 2);
                        break;
                }

                _lastValueProg = result;
                _operationProg = "";
                _isNewEntryProg = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"A aparut o eroare: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetOperationProg(string operation)
        {
            try
            {
                double parsedValue;
                string cleanDisplay = Display.Replace(".", "");
                
                if (cleanDisplay.Contains(","))
                {
                    parsedValue = double.Parse(cleanDisplay.Replace(",", "."), CultureInfo.InvariantCulture);
                }
                else
                {
                    switch (CurrentNumberSystem)
                    {
                        case "Bin":
                            parsedValue = Convert.ToInt32(cleanDisplay, 2);
                            break;
                        case "Hex":
                            parsedValue = Convert.ToInt32(cleanDisplay, 16);
                            break;
                        case "Oct":
                            parsedValue = Convert.ToInt32(cleanDisplay, 8);
                            break;
                        default:
                            parsedValue = double.Parse(cleanDisplay, CultureInfo.InvariantCulture);
                            break;
                    }
                }

                _lastValueProg = parsedValue;
                _operationProg = operation;
                _isNewEntryProg = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"A aparut o eroare: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddNumberProg(string number)
        {
            if (!IsValidNumberForCurrentBase(number)) return;

            if (number == ".") number = ",";
            if (_isNewEntryProg)
            {
                if (CurrentNumberSystem == "Bin")
                {
                    Display = number;
                }
                else
                {
                    Display = number == "," ? "0," : number;
                }
                _isNewEntryProg = false;
            }
            else
            {
                if (CurrentNumberSystem == "Dec")
                {
                    string cleanDisplay = Display.Replace(".", "");
                    
                    if (number == "," && !cleanDisplay.Contains(","))
                    {
                        cleanDisplay += ",";
                    }
                    else if (number != ",")
                    {
                        cleanDisplay += number;
                    }
                    
                    Display = cleanDisplay;
                }
                else
                {
                    if (number == "," && Display.Contains(",")) return;
                    Display += number;
                }
            }
            
            if (CurrentNumberSystem == "Dec" && !Display.EndsWith(",") && _isDigitGroupingEnabled)
            {
                if (Display.Contains(","))
                {
                    var parts = Display.Split(',');
                    if (double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double integerPart))
                    {
                        string formattedInteger = integerPart.ToString("#,0", new CultureInfo("ro-RO"));
                        Display = formattedInteger + "," + parts[1];
                    }
                }
                else
                {
                    if (double.TryParse(Display.Replace(".", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                    {
                        Display = value.ToString("#,0", new CultureInfo("ro-RO"));
                    }
                }
            }
            else if (!Display.EndsWith(","))
            {
                Display = Display.ToUpper();
            }
        }

        private void CutToClipboard()
        {
            if (Display != "0")
            {
                string cleanDisplay = Display.Replace(".", "");
                Clipboard.SetText(cleanDisplay);
                Display = "0";
                _isNewEntry = true;
            }
        }

        private void CopyToClipboard()
        {
            if (Display != "0")
            {
                string cleanDisplay = Display.Replace(".", "");
                Clipboard.SetText(cleanDisplay);
            }
        }

        private void PasteFromClipboard()
        {
            try
            {
                string clipboardText = Clipboard.GetText();
                if (!string.IsNullOrEmpty(clipboardText))
                {
                    clipboardText = new string(clipboardText.Where(c => char.IsDigit(c) || c == ',' || c == '.').ToArray());
                    
                    if (double.TryParse(clipboardText.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
                    {
                        Display = FormatNumber(number);
                        _isNewEntry = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"A aparut o eroare la lipire: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class CalculatorConfig
    {
        public bool IsDigitGroupingEnabled { get; set; } = true;
        public string LastMode { get; set; } = "Standard";
        public string LastNumberSystem { get; set; } = "Dec";
    }
}
