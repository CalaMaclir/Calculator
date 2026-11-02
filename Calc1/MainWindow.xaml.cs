using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Calc1
{
    public partial class MainWindow : Window
    {
        private ExpressionEvaluator evaluator = new();
        private ExpressionEvaluator? beforeLastOperator;
        private string expression = "";
        private string operandText = "0";
        private bool waitingForOperand;
        private bool hasOperator;
        private bool replaceEntry;
        private bool finished;
        private bool hasError;

        public MainWindow() => InitializeComponent();

        private decimal CurrentValue => decimal.Parse(Display.Text, CultureInfo.InvariantCulture);
        private static string Format(decimal value) => value.ToString("0.############################", CultureInfo.InvariantCulture);

        private void BeginEntry()
        {
            if (hasError || finished) Reset();
            if (waitingForOperand || replaceEntry)
            {
                Display.Text = "0";
                operandText = "0";
                waitingForOperand = false;
                replaceEntry = false;
            }
        }

        private void Digit_Click(object sender, RoutedEventArgs e)
        {
            BeginEntry();
            string digit = (string)((Button)sender).Content;
            string candidate = Display.Text == "0" ? digit : Display.Text + digit;
            if (decimal.TryParse(candidate, NumberStyles.Number, CultureInfo.InvariantCulture, out _) && candidate.Length <= 30)
                Display.Text = candidate;
            operandText = Display.Text;
        }

        private void Decimal_Click(object sender, RoutedEventArgs e)
        {
            BeginEntry();
            if (!Display.Text.Contains('.')) Display.Text += ".";
            operandText = Display.Text;
        }

        private void Operator_Click(object sender, RoutedEventArgs e)
        {
            if (hasError) return;
            string op = (string)((Button)sender).Content;
            try
            {
                if (finished)
                {
                    evaluator = new ExpressionEvaluator();
                    expression = "";
                    operandText = Display.Text;
                    finished = false;
                }
                if (waitingForOperand && beforeLastOperator != null)
                {
                    // 演算子の連続押下は、直前の優先順位処理も戻して置き換える。
                    evaluator = beforeLastOperator.Copy();
                    expression = expression[..^2];
                }
                else
                {
                    evaluator.PushValue(CurrentValue);
                    expression += operandText + " ";
                    beforeLastOperator = evaluator.Copy();
                }
                evaluator.PushOperator(op);
                expression += op + " ";
                ExpressionValue.Text = expression.TrimEnd();
                ExpressionOperator.Text = "";
                waitingForOperand = true;
                replaceEntry = false;
                hasOperator = true;
            }
            catch (Exception ex) when (ex is DivideByZeroException || ex is OverflowException)
            {
                ShowError();
            }
        }

        private void Equals_Click(object sender, RoutedEventArgs e)
        {
            if (hasError || !hasOperator || waitingForOperand) return;
            try
            {
                evaluator.PushValue(CurrentValue);
                string completedExpression = expression + operandText;
                Display.Text = Format(evaluator.Finish());
                ExpressionValue.Text = completedExpression;
                ExpressionOperator.Text = "＝";
                evaluator = new ExpressionEvaluator();
                beforeLastOperator = null;
                expression = "";
                operandText = Display.Text;
                hasOperator = false;
                finished = true;
                replaceEntry = true;
            }
            catch (Exception ex) when (ex is DivideByZeroException || ex is OverflowException)
            {
                ShowError();
            }
        }

        private void Function_Click(object sender, RoutedEventArgs e)
        {
            if (hasError) return;
            if (finished) { evaluator = new ExpressionEvaluator(); expression = ""; finished = false; }
            // 演算子の直後に関数を押した場合は、表示中の値を次の項として使う。
            if (waitingForOperand) operandText = Display.Text;
            waitingForOperand = false;
            decimal value = CurrentValue;
            string op = (string)((Button)sender).Content;
            try
            {
                decimal result = op switch
                {
                    "√x" => value < 0 ? throw new ArithmeticException() : (decimal)Math.Sqrt((double)value),
                    "1/x" => 1m / value,
                    "x²" => value * value,
                    "n!" => Factorial(value),
                    _ => throw new InvalidOperationException()
                };
                operandText = op switch
                {
                    "√x" => $"√({operandText})",
                    "1/x" => $"1/({operandText})",
                    "x²" => $"({operandText})²",
                    _ => $"({operandText})!"
                };
                Display.Text = Format(result);
                replaceEntry = true;
                if (!hasOperator) { ExpressionValue.Text = operandText; ExpressionOperator.Text = ""; }
            }
            catch (ArithmeticException) { ShowError(); }
        }

        private static decimal Factorial(decimal value)
        {
            if (value < 0 || value != decimal.Truncate(value) || value > 27)
                throw new ArithmeticException();
            decimal result = 1;
            for (int i = 2; i <= (int)value; i++) result *= i;
            return result;
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (hasError) { Reset(); return; }
            if (waitingForOperand || replaceEntry || finished) return;
            string text = Display.Text;
            text = text.Length > 1 ? text[..^1] : "0";
            Display.Text = text == "-" ? "0" : text;
            operandText = Display.Text;
        }

        private void Sign_Click(object sender, RoutedEventArgs e)
        {
            if (hasError) return;
            if (finished) { Reset(); }
            if (waitingForOperand) { Display.Text = "0"; waitingForOperand = false; }
            if (CurrentValue != 0) Display.Text = Format(-CurrentValue);
            operandText = Display.Text;
            replaceEntry = false;
        }

        private void ShowError()
        {
            evaluator = new ExpressionEvaluator();
            beforeLastOperator = null;
            expression = "";
            hasOperator = false;
            waitingForOperand = false;
            replaceEntry = false;
            finished = false;
            hasError = true;
            Display.Text = "エラー";
            ExpressionValue.Text = "";
            ExpressionOperator.Text = "";
        }

        private void Clear_Click(object sender, RoutedEventArgs e) => Reset();
        private void Reset()
        {
            evaluator = new ExpressionEvaluator();
            beforeLastOperator = null;
            expression = "";
            operandText = "0";
            waitingForOperand = false;
            hasOperator = false;
            replaceEntry = false;
            finished = false;
            hasError = false;
            Display.Text = "0";
            ExpressionValue.Text = "";
            ExpressionOperator.Text = "";
        }
    }
}
