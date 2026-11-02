using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Calc1
{
    public partial class MainWindow : Window
    {
        private decimal storedValue;
        private string? pendingOperator;
        private bool startNewEntry = true;
        private bool hasError;

        public MainWindow()
        {
            InitializeComponent();
        }

        private decimal CurrentValue => decimal.Parse(Display.Text, CultureInfo.InvariantCulture);

        private void Digit_Click(object sender, RoutedEventArgs e)
        {
            if (hasError) Reset();
            string digit = (string)((Button)sender).Content;
            if (startNewEntry || Display.Text == "0") Display.Text = digit;
            else if (Display.Text.Length < 28) Display.Text += digit;
            startNewEntry = false;
        }

        private void Decimal_Click(object sender, RoutedEventArgs e)
        {
            if (hasError) Reset();
            if (startNewEntry) Display.Text = "0";
            if (!Display.Text.Contains('.')) Display.Text += ".";
            startNewEntry = false;
        }

        private void Operator_Click(object sender, RoutedEventArgs e)
        {
            if (hasError) return;
            if (pendingOperator != null && !startNewEntry)
            {
                if (!Calculate()) return;
            }
            else if (pendingOperator == null)
            {
                storedValue = CurrentValue;
            }
            pendingOperator = (string)((Button)sender).Content;
            startNewEntry = true;
        }

        private void Equals_Click(object sender, RoutedEventArgs e)
        {
            if (hasError || pendingOperator == null || startNewEntry) return;
            if (Calculate())
            {
                pendingOperator = null;
                startNewEntry = true;
            }
        }

        private bool Calculate()
        {
            try
            {
                decimal right = CurrentValue;
                storedValue = pendingOperator switch
                {
                    "＋" => storedValue + right,
                    "−" => storedValue - right,
                    "×" => storedValue * right,
                    "÷" => storedValue / right,
                    _ => storedValue
                };
                Display.Text = storedValue.ToString("0.############################", CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception ex) when (ex is DivideByZeroException || ex is OverflowException)
            {
                Display.Text = "エラー";
                pendingOperator = null;
                startNewEntry = true;
                hasError = true;
                return false;
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e) => Reset();

        private void Reset()
        {
            Display.Text = "0";
            storedValue = 0;
            pendingOperator = null;
            startNewEntry = true;
            hasError = false;
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (hasError) { Reset(); return; }
            if (startNewEntry) return;
            string text = Display.Text;
            text = text.Length > 1 ? text[..^1] : "0";
            Display.Text = text == "-" ? "0" : text;
        }

        private void Sign_Click(object sender, RoutedEventArgs e)
        {
            if (hasError) return;
            if (startNewEntry && pendingOperator != null) Display.Text = "0";
            if (CurrentValue != 0)
                Display.Text = Display.Text.StartsWith('-') ? Display.Text[1..] : "-" + Display.Text;
            startNewEntry = false;
        }
    }
}
