using System;
using System.Collections.Generic;
using System.Linq;

namespace Calc1
{
    // 数値と演算子を別々のスタックに保持し、同じ優先順位は左から計算する。
    internal sealed class ExpressionEvaluator
    {
        private readonly Stack<decimal> values = new();
        private readonly Stack<string> operators = new();

        private static int Precedence(string op) => op switch
        {
            "＋" or "−" => 1,
            "×" or "÷" => 2,
            _ => throw new ArgumentException("不明な演算子", nameof(op))
        };

        public ExpressionEvaluator Copy()
        {
            var copy = new ExpressionEvaluator();
            foreach (var value in values.Reverse()) copy.values.Push(value);
            foreach (var op in operators.Reverse()) copy.operators.Push(op);
            return copy;
        }

        public void PushValue(decimal value) => values.Push(value);

        public void PushOperator(string op)
        {
            while (operators.Count > 0 && Precedence(operators.Peek()) >= Precedence(op))
                Reduce();
            operators.Push(op);
        }

        public decimal Finish()
        {
            while (operators.Count > 0) Reduce();
            if (values.Count != 1) throw new InvalidOperationException("式が不完全です");
            return values.Pop();
        }

        private void Reduce()
        {
            decimal right = values.Pop();
            decimal left = values.Pop();
            string op = operators.Pop();
            values.Push(op switch
            {
                "＋" => left + right,
                "−" => left - right,
                "×" => left * right,
                "÷" => left / right,
                _ => throw new InvalidOperationException("不明な演算子")
            });
        }
    }
}
