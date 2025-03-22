using System.Windows;
using System.Windows.Input;

namespace CalculatorApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                bool isShiftPressed = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
                bool isCtrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                if (viewModel.IsStandardModeVisible)
                {
                    switch (e.Key)
                    {
                        case Key.D0: case Key.NumPad0: viewModel.NumberCommand.Execute("0"); break;
                        case Key.D1: case Key.NumPad1: viewModel.NumberCommand.Execute("1"); break;
                        case Key.D2: case Key.NumPad2: viewModel.NumberCommand.Execute("2"); break;
                        case Key.D3: case Key.NumPad3: viewModel.NumberCommand.Execute("3"); break;
                        case Key.D4: case Key.NumPad4: viewModel.NumberCommand.Execute("4"); break;
                        case Key.D5: case Key.NumPad5: viewModel.NumberCommand.Execute("5"); break;
                        case Key.D6: case Key.NumPad6: viewModel.NumberCommand.Execute("6"); break;
                        case Key.D7: case Key.NumPad7: viewModel.NumberCommand.Execute("7"); break;

                        case Key.D8:
                            if (isShiftPressed)
                                viewModel.OperationCommand.Execute("*");
                            else
                                viewModel.NumberCommand.Execute("8");
                            break;

                        case Key.D9: case Key.NumPad9: viewModel.NumberCommand.Execute("9"); break;

                        case Key.OemComma: case Key.OemPeriod: case Key.Decimal: viewModel.NumberCommand.Execute(","); break;

                        case Key.Add: viewModel.OperationCommand.Execute("+"); break;
                        case Key.Subtract: case Key.OemMinus: viewModel.OperationCommand.Execute("-"); break;
                        case Key.Multiply: viewModel.OperationCommand.Execute("*"); break;
                        case Key.Divide: case Key.Oem2: viewModel.OperationCommand.Execute("/"); break;

                        case Key.Enter: viewModel.EqualsCommand.Execute(null); break;
                        case Key.Back: viewModel.DeleteLastChar.Execute(null); break;
                        case Key.Escape: viewModel.C.Execute(null); break;

                        case Key.C:
                            if (isCtrlPressed)
                                viewModel.CopyCommand.Execute(null);
                            break;
                        case Key.V:
                            if (isCtrlPressed)
                                viewModel.PasteCommand.Execute(null);
                            break;
                        case Key.X:
                            if (isCtrlPressed)
                                viewModel.CutCommand.Execute(null);
                            break;

                        case Key.OemPlus:
                            if (!isShiftPressed)
                                viewModel.EqualsCommand.Execute(null);
                            else
                                viewModel.OperationCommand.Execute("+");
                            break;
                                           
                    }
                }
                else
                {
                    switch (e.Key)
                    {
                        case Key.D0: case Key.NumPad0: viewModel.NumberCommandProg.Execute("0"); break;
                        case Key.D1: case Key.NumPad1: viewModel.NumberCommandProg.Execute("1"); break;
                        case Key.D2: case Key.NumPad2: viewModel.NumberCommandProg.Execute("2"); break;
                        case Key.D3: case Key.NumPad3: viewModel.NumberCommandProg.Execute("3"); break;
                        case Key.D4: case Key.NumPad4: viewModel.NumberCommandProg.Execute("4"); break;
                        case Key.D5: case Key.NumPad5: viewModel.NumberCommandProg.Execute("5"); break;
                        case Key.D6: case Key.NumPad6: viewModel.NumberCommandProg.Execute("6"); break;
                        case Key.D7: case Key.NumPad7: viewModel.NumberCommandProg.Execute("7"); break;
                        case Key.D8:
                            if (isShiftPressed)
                                viewModel.MultiplyCommand.Execute("*");
                            else
                                viewModel.NumberCommandProg.Execute("8");
                            break;
                        case Key.D9: case Key.NumPad9: viewModel.NumberCommandProg.Execute("9"); break;

                        case Key.OemComma: case Key.OemPeriod: case Key.Decimal: viewModel.NumberCommandProg.Execute(","); break;

                        case Key.Subtract: case Key.OemMinus: viewModel.SubtractCommand.Execute("-"); break;
                        case Key.OemPlus:
                            if (!isShiftPressed)
                                viewModel.EqualsCommandProg.Execute(null);
                            else
                                viewModel.AddCommand.Execute("+");
                            break;
                        case Key.Divide: case Key.Oem2: viewModel.DivideCommand.Execute("/"); break;

                        case Key.C:
                            if (isCtrlPressed)
                                viewModel.CopyCommand.Execute(null);
                            else
                                viewModel.CharacterCommand.Execute("C"); break;
                            break;
                        case Key.V:
                            if (isCtrlPressed)
                                viewModel.PasteCommand.Execute(null);
                            break;
                        case Key.X:
                            if (isCtrlPressed)
                                viewModel.CutCommand.Execute(null);
                            break;
                        case Key.Enter: viewModel.EqualsCommandProg.Execute(null); break;
                        case Key.Back: viewModel.DeleteCommand.Execute(null); break;
                        case Key.Escape: viewModel.C.Execute(null); break;
                        case Key.A: viewModel.CharacterCommand.Execute("A"); break;
                        case Key.B: viewModel.CharacterCommand.Execute("B"); break;
                        case Key.D: viewModel.CharacterCommand.Execute("D"); break;
                        case Key.E: viewModel.CharacterCommand.Execute("E"); break;
                        case Key.F: viewModel.CharacterCommand.Execute("F"); break;

                    }
                }
           
            }
            
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
