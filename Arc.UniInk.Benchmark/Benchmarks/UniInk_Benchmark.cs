namespace Arc.UniInk.Benchmark
{

    // ReSharper disable UnusedVariable
    using System;
    using BenchmarkDotNet.Attributes;
    using CodingSeb.ExpressionEvaluator;
    using ParsecSharp.Examples;
    using Sprache.Calc;


    [MemoryDiagnoser]
    public class UniInkBenchmark
    {
        private static readonly UniInk Ink = new();

        private static int EvaluateInt(string expression)
        {
            var value = Ink.Evaluate(expression);

            var result = value!.ValueInt;

            InkValue.Release(value);

            return result;
        }


        private const string TEST_INPUT_ARITHMETIC_INT_01 = "12345678+87654321-1*2*3*4*5*6*7*8*9+9*8*7*6*5*4*3*2*1+1*2*3*4*5*6*7*8*9-87654321-12345678";

        private const string TEST_INPUT_ARITHMETIC_INT_02 =
            "1+2+3+4+5+6+7+8+9+10+11+12+13+14+15+16+17+18+19+20+21+22+23+24+25+26+27+28+29+30+31+32+33+"
          + "34+35+36+37+38+39+40+41+42+43+44+45+46+47+48+49+50+51+52+53+54+55+56+57+58+59+60+61+62+63+"
          + "64+65+66+67+68+69+70+71+72+73+74+75+76+77+78+79+80+81+82+83+84+85+86+87+88+89+90+91+92+93+"
          + "94+95+96+97+98+99+100";

        private const string TEST_INPUT_ARITHMETIC_INT_03 = "1*2*3*4*5*6*7*8*9*10*11*12/12/11/10/9/8/7/6/5/4/3/2/1";

        private const string TEST_INPUT_ARITHMETIC_INT_04 =
            "0-1-2-3-4-5-6-7-8-9-10-11-12-13-14-15-16-17-18-19-20-21-22-23-24-25-26-27-28-29-30-31-32-33-"
          + "34-35-36-37-38-39-40-41-42-43-44-45-46-47-48-49-50-51-52-53-54-55-56-57-58-59-60-61-62-63-64-"
          + "65-66-67-68-69-70-71-72-73-74-75-76-77-78-79-80-81-82-83-84-85-86-87-88-89-90-91-92-93-94-95-"
          + "96-97-98-99-100";

        private const string TEST_INPUT_IF_STATEMENT = @" if ( 1 > 2 )      
                                                      {                 
                                                         123            
                                                      }                 
                                                      else if ( 3 > 6 ) 
                                                      {                 
                                                         456            
                                                      }                 
                                                      else if ( 3 < 6 ) 
                                                      {                 
                                                        return 456      
                                                      }                 
                                                      else              
                                                      {                 
                                                        return 666      
                                                      }                 ";

        private static readonly InkSyntaxList compiledTestInputArithmeticInt01 = Ink.CompileLexerAndFill(TEST_INPUT_ARITHMETIC_INT_01, 0, TEST_INPUT_ARITHMETIC_INT_01.Length - 1);

        private static readonly InkSyntaxList compiledTestInputArithmeticInt02 = Ink.CompileLexerAndFill(TEST_INPUT_ARITHMETIC_INT_02, 0, TEST_INPUT_ARITHMETIC_INT_02.Length - 1);

        private static readonly InkSyntaxList compiledTestInputArithmeticInt03 = Ink.CompileLexerAndFill(TEST_INPUT_ARITHMETIC_INT_03, 0, TEST_INPUT_ARITHMETIC_INT_03.Length - 1);

        private static readonly InkSyntaxList compiledTestInputArithmeticInt04 = Ink.CompileLexerAndFill(TEST_INPUT_ARITHMETIC_INT_04, 0, TEST_INPUT_ARITHMETIC_INT_04.Length - 1);




        [Benchmark(Baseline = true)]
        public void TEST_Arithmetic__UniInk()
        {
            var result1 = EvaluateInt(TEST_INPUT_ARITHMETIC_INT_01);
            var result2 = EvaluateInt(TEST_INPUT_ARITHMETIC_INT_02);
            var result3 = EvaluateInt(TEST_INPUT_ARITHMETIC_INT_03);
            var result4 = EvaluateInt(TEST_INPUT_ARITHMETIC_INT_04);

#if DEBUG
            Console.WriteLine(result1 + "|" + result2 + "|" + result3 + "|" + result4);
#endif
        }


        [Benchmark]
        public void TEST_Arithmetic__UniInk_Compiled()
        {
            // ExecuteProcess hands out an InkValue that the caller owns : read it with GetResult_Int ,
            // which releases the instance back to the pool ( otherwise the benchmark allocates 4 x 392 B per op )
            var result1 = ((InkValue)UniInk.ExecuteProcess(compiledTestInputArithmeticInt01)).GetResult_Int();
            var result2 = ((InkValue)UniInk.ExecuteProcess(compiledTestInputArithmeticInt02)).GetResult_Int();
            var result3 = ((InkValue)UniInk.ExecuteProcess(compiledTestInputArithmeticInt03)).GetResult_Int();
            var result4 = ((InkValue)UniInk.ExecuteProcess(compiledTestInputArithmeticInt04)).GetResult_Int();

            InkSyntaxList.Recover(compiledTestInputArithmeticInt01);
            InkSyntaxList.Recover(compiledTestInputArithmeticInt02);
            InkSyntaxList.Recover(compiledTestInputArithmeticInt03);
            InkSyntaxList.Recover(compiledTestInputArithmeticInt04);

#if DEBUG
            Console.WriteLine(result1 + "|" + result2 + "|" + result3 + "|" + result4);
#endif
        }


        private static readonly ExpressionEvaluator expressionEvaluator = new ExpressionEvaluator();

        [Benchmark]
        public void TEST_Arithmetic__ExpressionEvaluator()
        {
            var result1 = expressionEvaluator.Evaluate<int>(TEST_INPUT_ARITHMETIC_INT_01);
            var result2 = expressionEvaluator.Evaluate<int>(TEST_INPUT_ARITHMETIC_INT_02);
            var result3 = expressionEvaluator.Evaluate<int>(TEST_INPUT_ARITHMETIC_INT_03);
            var result4 = expressionEvaluator.Evaluate<int>(TEST_INPUT_ARITHMETIC_INT_04);

#if DEBUG
            Console.WriteLine(result1 + "|" + result2 + "|" + result3 + "|" + result4);
#endif
        }

        private static SimpleCalculator Calculator { get; } = new SimpleCalculator();

        [Benchmark]
        public void TEST_Arithmetic__Sprache()
        {
            var result1 = Calculator.ParseExpression(TEST_INPUT_ARITHMETIC_INT_01).Compile().Invoke();
            var result2 = Calculator.ParseExpression(TEST_INPUT_ARITHMETIC_INT_02).Compile().Invoke();
            var result3 = Calculator.ParseExpression(TEST_INPUT_ARITHMETIC_INT_03).Compile().Invoke();
            var result4 = Calculator.ParseExpression(TEST_INPUT_ARITHMETIC_INT_04).Compile().Invoke();
#if DEBUG
            Console.WriteLine(result1 + "|" + result2 + "|" + result3 + "|" + result4);
#endif
        }

        [Benchmark]
        public void TEST_Arithmetic__ParsecSharp()
        {
            var result1 = Integer.Parser.Parse(TEST_INPUT_ARITHMETIC_INT_01).Value.Value;
            var result2 = Integer.Parser.Parse(TEST_INPUT_ARITHMETIC_INT_02).Value.Value;
            var result3 = Integer.Parser.Parse(TEST_INPUT_ARITHMETIC_INT_03).Value.Value;
            var result4 = Integer.Parser.Parse(TEST_INPUT_ARITHMETIC_INT_04).Value.Value;

#if DEBUG
            Console.WriteLine(result1 + "|" + result2 + "|" + result3 + "|" + result4);
#endif
        }



        [Benchmark]
        public void TEST_IfStatement__UniInk()
        {
            var ink  = Ink;
            var test = ink.Evaluate_IfStatement(TEST_INPUT_IF_STATEMENT);
            if (test is InkValue value)
            {
                InkValue.Release(value);
            }
        }
    }

}