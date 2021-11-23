using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LabCalculator
{
    class LabCalculatorVisitor : LabCalculatorBaseVisitor<double>
    {
        //таблиця ідентифікаторів (тут для прикладу)
        //в лабораторній роботі заміните на свою!!!!
        Dictionary<string, double> tableIdentifier = new Dictionary<string, double>();

        public override double VisitCompileUnit(LabCalculatorParser.CompileUnitContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitNumberExpr(LabCalculatorParser.NumberExprContext context)
        {
            var result = double.Parse(context.GetText());
            Debug.WriteLine(result);

            return result;
        }

        //IdentifierExpr
        public override double VisitIdentifierExpr(LabCalculatorParser.IdentifierExprContext context)
        {
            var result = context.GetText();
            double value;
            //видобути значення змінної з таблиці
            if (tableIdentifier.TryGetValue(result.ToString(), out value))
            {
                return value;
            }
            else
            {
                return 0.0;
            }
        }

        public override double VisitParenthesizedExpr(LabCalculatorParser.ParenthesizedExprContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitExponentialExpr(LabCalculatorParser.ExponentialExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (right == double.PositiveInfinity)
            {
                return double.PositiveInfinity;
            }
            Debug.WriteLine("{0} ^ {1}", left, right);
            return System.Math.Pow(left, right);
        }

        public override double VisitAdditiveExpr(LabCalculatorParser.AdditiveExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (context.operatorToken.Type == LabCalculatorLexer.ADD)
            {
                Debug.WriteLine("{0} + {1}", left, right);
                return left + right;
            }
            else //LabCalculatorLexer.SUBTRACT
            {
                Debug.WriteLine("{0} - {1}", left, right);
                return left - right;
            }
        }


        public override double VisitMultiplicativeExpr(LabCalculatorParser.MultiplicativeExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (context.operatorToken.Type == LabCalculatorLexer.MULTIPLY)
            {
                Debug.WriteLine("{0} * {1}", left, right);
                return left * right;
            }
            else //LabCalculatorLexer.DIVIDE
            {
                if(right == 0)
                {
                    MessageBox.Show("ПОМИЛКА, ДІЛЕННЯ НА НУЛЬ");
/*                        return 0;*/
                }
                Debug.WriteLine("{0} / {1}", left, right);
                return left / right;
            }
        }

        public override double VisitModDivExpr(LabCalculatorParser.ModDivExprContext context)
        {
            int left = Convert.ToInt32(WalkLeft(context));
            int right = Convert.ToInt32(WalkRight(context));
            if (right == 0 || right == double.PositiveInfinity)
            {
                MessageBox.Show("ПОМИЛКА, ДІЛЕННЯ НА НУЛЬ!");
                return double.PositiveInfinity;
            }
            if (context.operatorToken.Type == LabCalculatorLexer.MOD)
            {
                Debug.WriteLine("{0} mod {1}", left, right);
                return left % right;
            }
            else
            {
                Debug.WriteLine("{0} div {1}", left, right);
                return left / right;
            }
        }

        public override double VisitIncExpr(LabCalculatorParser.IncExprContext context)
        {
            var left = WalkLeft(context);
            Debug.WriteLine("inc({0})", left);
            return ++left;
        }

        public override double VisitDecExpr(LabCalculatorParser.DecExprContext context)
        {
            var left = WalkLeft(context);
            Debug.WriteLine("dec({0})", left);
            return --left;
        }

        private double WalkLeft(LabCalculatorParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<LabCalculatorParser.ExpressionContext>(0));
        }

        private double WalkRight(LabCalculatorParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<LabCalculatorParser.ExpressionContext>(1));
        }


    }
}