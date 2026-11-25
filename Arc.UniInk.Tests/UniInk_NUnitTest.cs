namespace Arc.UniInk.Tests
{

    /*******************************************************************************************************************
    *  📰 Title    :  UniInk (https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator)                    *
    *  🔖 Version  :  1.3.5                                                                                           *
    *  😀 Author   :  Arc (https://github.com/Arc-huangjingtong)                                                      *
    *  🔑 License  :  MIT (https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator/blob/main/LICENSE)     *
    *  🤝 Support  :  [.NET Standard 2.1] [C# 9.0+] [IL2CPP Support]                                                  *
    *  📝 Desc     :  [High performance] [zero box & unbox] [zero GC!] [zero reflection runtime] [Easy-use]           *
    *  📦 State    :  [Developing] [0GC]                                                                              *
    /*******************************************************************************************************************/

    // ReSharper disable EqualExpressionComparison
    // ReSharper disable RedundantLogicalConditionalExpressionOperand
    // ReSharper disable PartialTypeWithSinglePart
    using Arc.UniInk;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;


    [TestFixture]
    public sealed partial class UniInkNUnitTest
    {
        public static readonly UniInk Ink = new();

        public bool isInit;

        object SUM(IList<object> list)
        {
            // the param and return value must be InkValue,and its will be auto released
            // If you are not in the above situations to generate InkValue, you will produce GC
            // Such as :
            // var temp = inkValue + inkValue
            // and temp is not return value, you must use InkValue.Release(temp) to release it

            var param1 = (InkValue)list[0];
            var param2 = (InkValue)list[1];
            var param3 = (InkValue)list[2];

            var sum = InkValue.GetIntValue(param1.ValueInt + param2.ValueInt + param3.ValueInt);

            return sum;
        }

        [OneTimeSetUp]
        public void Test_Initiation()
        {
            if (isInit) return;

            isInit = true;

            Ink.RegisterFunction("SUM", new InkFunction(SUM));
            Ink.RegisterFunction("LOG", new(prms =>
            {
                if (prms[0] is InkValue value)
                {
                    Console.WriteLine(value.ToString());
                }

                return null;
            }));

            Ink.RegisterFunction("PAY", new(prms =>
            {
                var param1 = (MyEnum)((int)(InkValue)prms[0]);

                var param2 = (int)((InkValue)(prms[1]));

                PAY(param1, param2);
                return null;
            }));


            Ink.RegisterFunction("GET", new(prms =>
            {
                var param1 = (Card)((InkValue)prms[0]).ValueObject;
                var param2 = (int)(InkValue)prms[1];

                return param2 switch
                {
                    0 => InkValue.GetIntValue(param1.ID)     //
                  , 5 => InkValue.GetIntValue(param1.Rarity) //
                  , _ => InkValue.GetIntValue(0)
                };
            }));


            Ink.RegisterFunction("CRE", new(prms =>
            {
                var param1 = (InkValue)prms[0];
                var param2 = (InkValue)prms[1];

                return new Card() { ID = param1, Rarity = param2 };
            }));

            Ink.RegisterFunction("FLT", new(prms =>
            {
                var param1 = (List<Card>)((InkValue)prms[0]).ValueObject;
                var param2 = (Predicate<object>)((InkValue)prms[1]).ValueObject;

                return FLT(param1, param2);
            }));


            List<Card> FLT(IList<Card> cards, Predicate<Card> func)
            {
                var list = new System.Collections.Generic.List<Card>();

                foreach (var card in cards)
                {
                    if (func(card))
                    {
                        list.Add(card);
                    }
                }

                return list;
            }


            Ink.RegisterVariable("grower", InkValue.SetGetter(InkValue.GetIntValue(0), value => value.ValueInt++));

            Ink.RegisterVariable("Target", InkValue.SetGetter(InkValue.GetObjectValue(Target), _ => { Target.ID++; }));


            Ink.RegisterVariable("Food",   InkValue.GetIntValue(0));
            Ink.RegisterVariable("Rarity", InkValue.GetIntValue(5));
            Ink.RegisterVariable("ID",     InkValue.GetIntValue(0));
            Ink.RegisterVariable("Config", InkValue.GetObjectValue(Config));
        }


        #region SP : Features Test


        [Repeat(10000)]
        [TestCase("999*123123*321321/999/666 ", ExpectedResult = unchecked(999 * 123123 * 321321) / 999 / 666)] // arithmetic overflow
        public static object Test_Expression_Extreme(string input)
        {
            var result = Ink.Evaluate(input).GetResult_Int(); // result is 232
            return result;
        }


        #endregion


        #region 1.Basic: Arithmetic Test , return result InkValue and Release it


        [Repeat(10000)]
        [TestCase("+123456789             ", ExpectedResult = +123456789)]
        [TestCase("+123456789+987654321   ", ExpectedResult = +123456789 + 987654321)]
        [TestCase("111 * 111 * 3 /3*3/3   ", ExpectedResult = 111 * 111 * 3 / 3 * 3 / 3)]
        [TestCase("3333333-3+3+  3 - 3    ", ExpectedResult = 3333333 - 3 + 3 + 3 - 3)]
        [TestCase("9*(1+1 + 1 + 1 + 1+1+1)", ExpectedResult = 9 * (1  + 1 + 1 + 1 + 1 + 1 + 1))]
        [TestCase("   999999 + 999999     ", ExpectedResult = 999999 + 999999)]
        [TestCase("9*((1+(1+1)+(1+1))+1+1)", ExpectedResult = 9  * ((1 + (1 + 1) + (1 + 1)) + 1 + 1))]
        [TestCase("9*((1+(2+3)*(4+5))+6+7)", ExpectedResult = 9  * ((1 + (2 + 3) * (4 + 5)) + 6 + 7))]
        [TestCase("9 * ( ( 1 + 2 * 3 ) /2)", ExpectedResult = 9  * ((1 + 2       * 3) / 2))]
        //[TestCase("9 *+5 ",                  ExpectedResult = 9  * +5)] // not support
        [TestCase("+9*5 ",                   ExpectedResult = +9 * 5)]  // support
        [TestCase("9 * (-5) ",               ExpectedResult = 9  * -5)] // support
        public static int Test_Arithmetic_Int(string input)
        {
            var res    = Ink.Evaluate(input);
            var result = res!.ValueInt;
            InkValue.Release(res);

            return result;
        }

        [Repeat(10000)]
        [TestCase("   999999.9999f + 999999.9999f     ",             ExpectedResult = 999999.9999f + 999999.9999f)]
        [TestCase("9.9f*((1.1f+(1.1f+1.1f)+(1.1f+1.1f))+1.1f+1.1f)", ExpectedResult = 9.9f * ((1.1f + (1.1f + 1.1f) + (1.1f + 1.1f)) + 1.1f + 1.1f))]
        [TestCase("+123456789.987654321f  ",                         ExpectedResult = 123456789.987654321f)]
        [TestCase("+123456789.987654321f + 987654321.123456789f",    ExpectedResult = 123456789.987654321f + 987654321.123456789f)]
        [TestCase("111.111f * 111.111f * 3.3f /3.3f*3.3f/3.3f",      ExpectedResult = 111.111f * 111.111f * 3.3f / 3.3f * 3.3f / 3.3f)]
        [TestCase("3333333.3333333f-3.3f+3.3f+  3.3f - 3.3f",        ExpectedResult = 3333333.3333333f - 3.3f + 3.3f + 3.3f - 3.3f)]
        public static float Test_Arithmetic_Float(string input)
        {
            var res    = Ink.Evaluate(input);
            var result = res!.ValueFloat;
            InkValue.Release(res);

            return result;
        }


        [Repeat(10000)]
        [TestCase("   999999.999d + 999999.999d     ",            ExpectedResult = 999999.999 + 999999.999)]
        [TestCase("9.9*((1.1+(1.1+1.1)+(1.1+1.1))+1.1+1.1)",      ExpectedResult = 9.9 * ((1.1 + (1.1 + 1.1) + (1.1 + 1.1)) + 1.1 + 1.1))]
        [TestCase("+123456789.987654321d  ",                      ExpectedResult = 123456789.987654321)]
        [TestCase("+123456789.987654321d + 987654321.123456789d", ExpectedResult = 123456789.987654321 + 987654321.123456789)]
        [TestCase("111.111 * 111.111 * 3.3 /3.3*3.3/3.3",         ExpectedResult = 111.111 * 111.111 * 3.3 / 3.3 * 3.3 / 3.3)]
        [TestCase("3333333.3333333-3.3+3.3+  3.3 - 3.3",          ExpectedResult = 3333333.3333333 - 3.3 + 3.3 + 3.3 - 3.3)]
        public static double Test_Arithmetic_Double(string input)
        {
            var res    = Ink.Evaluate(input);
            var result = res!.ValueDouble;
            InkValue.Release(res);

            return result;
        }


        [Repeat(10000)]
        [TestCase("!true && false || true && false ",   ExpectedResult = (!true && false) || (true && false))]
        [TestCase("1 > 2 || 2 > 1 || 2==1          ",   ExpectedResult = 1 > 2            || 2 > 1  || 2 == 1)]
        [TestCase("1 < 2 || 2 ==1 || 2 < 1         ",   ExpectedResult = 1 < 2            || 2 == 1 || 2 < 1)]
        [TestCase("1 >= 2 && 2 >= 1                ",   ExpectedResult = 1 >= 2 && 2           >= 1)]
        [TestCase("1 <= 2 || 2 <= 1                ",   ExpectedResult = 1 <= 2 || 2           <= 1)]
        [TestCase("1 == 2 && 2 == 1                ",   ExpectedResult = 1 == 2 && 2           == 1)]
        [TestCase("1 != 2 || 2 != 1                ",   ExpectedResult = 1 != 2        || 2    != 1)]
        [TestCase("1 != 1+1 || 2 != 1                ", ExpectedResult = 1 != 1 + 1    || 2    != 1)]
        [TestCase("true && false || True && 1+1==1+1",  ExpectedResult = true && false || true && 1 + 1 == 1 + 1)]
        public static bool Test_Arithmetic_Bool(string input)
        {
            var res    = Ink.Evaluate(input);
            var result = res!.ValueBool;
            InkValue.Release(res);
            return result;
        }


        #endregion


        #region 1.1 Parentheses: an expression may start with [ ( ]


        [Repeat(1000)]
        [TestCase("(1)",                ExpectedResult = 1)]
        [TestCase("(1+2)",              ExpectedResult = 1 + 2)]
        [TestCase("((1))",              ExpectedResult = 1)]
        [TestCase("(((1)))",            ExpectedResult = 1)]
        [TestCase("(1+2)*3",            ExpectedResult = (1 + 2) * 3)]
        [TestCase("(2+3)*(4+5)",        ExpectedResult = (2 + 3) * (4 + 5))]
        [TestCase("(1+2)+(3+4)",        ExpectedResult = (1 + 2) + (3 + 4))]
        [TestCase("((1+2)*3)",          ExpectedResult = (1 + 2) * 3)]
        [TestCase("(-5)",               ExpectedResult = -5)]
        [TestCase("(0-5)",              ExpectedResult = 0 - 5)]
        [TestCase("(8/2)/2",            ExpectedResult = (8     / 2) / 2)]
        [TestCase("(1+(2*3))",          ExpectedResult = 1 + (2 * 3))]
        [TestCase("(1+1)*((2+2))",      ExpectedResult = (1 + 1) * (2 + 2))]
        [TestCase("(1+2)-3",            ExpectedResult = (1 + 2) - 3)]
        [TestCase("var a = (1+2); a",   ExpectedResult = 1       + 2)]
        [TestCase("4; (1+2)",           ExpectedResult = 1       + 2)]
        [TestCase("(1+2); 4",           ExpectedResult = 4)]
        [TestCase("(1+2)*(3)",          ExpectedResult = (1  + 2) * 3)]
        [TestCase("-(1+2)",             ExpectedResult = -(1 + 2))]
        [TestCase("(1+2) + SUM(1,2,3)", ExpectedResult = (1  + 2) + (1 + 2 + 3))]
        public static int Test_Expression_Parentheses(string input)
        {
            var res = Ink.Evaluate(input);

            Assert.That(res is null, Is.False, $"the expression [{input}] should not be evaluated to null");

            var result = res!.ValueInt;

            InkValue.Release(res);

            return result;
        }


        [Repeat(1000)]
        [TestCase("(1+2) > 2",   ExpectedResult = (1 + 2) > 2)]
        [TestCase("(1+2) == 3",  ExpectedResult = (1 + 2) == 3)]
        [TestCase("((1) < (2))", ExpectedResult = 1       < 2)]
        public static bool Test_Expression_Parentheses_Bool(string input)
        {
            var res = Ink.Evaluate(input);

            Assert.That(res is null, Is.False, $"the expression [{input}] should not be evaluated to null");

            var result = res!.ValueBool;

            InkValue.Release(res);

            return result;
        }


        #endregion


        #region 1.2 Comparison: int / float / double / bool / char / string


        [Repeat(1000)]
        [TestCase("1 < 2",            ExpectedResult = 1     < 2)]
        [TestCase("2 < 1",            ExpectedResult = 2     < 1)]
        [TestCase("1 <= 2",           ExpectedResult = 1     <= 2)]
        [TestCase("2 <= 1",           ExpectedResult = 2     <= 1)]
        [TestCase("1 <= 1",           ExpectedResult = 1     <= 1)]
        [TestCase("1 >= 2",           ExpectedResult = 1     >= 2)]
        [TestCase("2 >= 1",           ExpectedResult = 2     >= 1)]
        [TestCase("1 >= 1",           ExpectedResult = 1     >= 1)]
        [TestCase("1 == 1",           ExpectedResult = 1     == 1)]
        [TestCase("1 == 2",           ExpectedResult = 1     == 2)]
        [TestCase("1 != 1",           ExpectedResult = 1     != 1)]
        [TestCase("1 != 2",           ExpectedResult = 1     != 2)]
        [TestCase("1.5 < 2.5",        ExpectedResult = 1.5   < 2.5)]
        [TestCase("2.5 < 1.5",        ExpectedResult = 2.5   < 1.5)]
        [TestCase("1.5 <= 2.5",       ExpectedResult = 1.5   <= 2.5)]
        [TestCase("2.5 <= 1.5",       ExpectedResult = 2.5   <= 1.5)]
        [TestCase("1.5 >= 2.5",       ExpectedResult = 1.5   >= 2.5)]
        [TestCase("2.5 >= 2.5",       ExpectedResult = 2.5   >= 2.5)]
        [TestCase("1.5 == 1.5",       ExpectedResult = 1.5   == 1.5)]
        [TestCase("1.5 == 2.5",       ExpectedResult = 1.5   == 2.5)]
        [TestCase("1.5 != 2.5",       ExpectedResult = 1.5   != 2.5)]
        [TestCase("1.5f < 2.5f",      ExpectedResult = 1.5f  < 2.5f)]
        [TestCase("2.5f < 1.5f",      ExpectedResult = 2.5f  < 1.5f)]
        [TestCase("1.5f <= 2.5f",     ExpectedResult = 1.5f  <= 2.5f)]
        [TestCase("2.5f <= 1.5f",     ExpectedResult = 2.5f  <= 1.5f)]
        [TestCase("1.5f >= 1.5f",     ExpectedResult = 1.5f  >= 1.5f)]
        [TestCase("1.5f >= 2.5f",     ExpectedResult = 1.5f  >= 2.5f)]
        [TestCase("1.5f == 1.5f",     ExpectedResult = 1.5f  == 1.5f)]
        [TestCase("1.5f == 2.5f",     ExpectedResult = 1.5f  == 2.5f)]
        [TestCase("1.5f != 1.5f",     ExpectedResult = 1.5f  != 1.5f)]
        [TestCase("true == true",     ExpectedResult = true  == true)]
        [TestCase("true == false",    ExpectedResult = true  == false)]
        [TestCase("false == false",   ExpectedResult = false == false)]
        [TestCase("true != false",    ExpectedResult = true  != false)]
        [TestCase("true != true",     ExpectedResult = true  != true)]
        [TestCase("'a' == 'a'",       ExpectedResult = 'a'   == 'a')]
        [TestCase("'a' == 'b'",       ExpectedResult = 'a'   == 'b')]
        [TestCase("'a' != 'b'",       ExpectedResult = 'a'   != 'b')]
        [TestCase("'a' < 'b'",        ExpectedResult = 'a'   < 'b')]
        [TestCase("'b' > 'a'",        ExpectedResult = 'b'   > 'a')]
        [TestCase("'a'=='a'",         ExpectedResult = 'a'   == 'a')]
        [TestCase("'a'!='a'",         ExpectedResult = 'a'   != 'a')]
        [TestCase("'a'=='b'",         ExpectedResult = 'a'   == 'b')]
        [TestCase("\"ab\" == \"ab\"", ExpectedResult = true)] // string == is not a C# constant expression
        [TestCase("\"ab\" == \"ac\"", ExpectedResult = false)]
        [TestCase("\"ab\" != \"ac\"", ExpectedResult = true)]
        public static bool Test_Comparison_Bool(string input)
        {
            var res = Ink.Evaluate(input);

            Assert.That(res is null, Is.False, $"the expression [{input}] should not be evaluated to null");

            var result = res!.ValueBool;

            InkValue.Release(res);

            return result;
        }


        [Test]
        public static void Test_Comparison_Unsupported_Ordering_Throws()
        {
            // ordering values is only defined for numbers and chars , bool must be reported instead of silently comparing 0.0f
            Assert.Throws<InkSyntaxException>(() => Ink.Evaluate("true > false"));
        }


        #endregion


        #region 2.Complex: variable declaration and assignment


        [Repeat(10000)]
        [TestCase("var a = 123 ;;  var b = a + 1 ; a + b    ",        ExpectedResult = 123 + 123 + 1)]
        [TestCase("var aaa= 123 ;  var bbb =aaa + 1 ; aaa + bbb    ", ExpectedResult = 123 + 123 + 1)]
        public static int Test_Expression_Variable(string input)
        {
            var res = Ink.Evaluate(input);

            var result = 0;

            if (res is InkValue inkValue)
            {
                result = inkValue.ValueInt;
                InkValue.Release(inkValue);
            }

            return result;
        }

        [Repeat(10000)]
        [TestCase("var a = 123f ;  var b = a + 1f ; a + b    ", ExpectedResult = 123f + 123f + 1f)]
        public static float Test_Expression_Variable2(string input)
        {
            var res = Ink.Evaluate(input);

            var result = 0f;

            if (res is InkValue inkValue)
            {
                result = inkValue.ValueFloat;
                InkValue.Release(inkValue);
            }

            return result;
        }


        [Repeat(10000)]
        [TestCase("var a = 123d ;  var b = a + 1d ; a + b    ", ExpectedResult = 123d + 123d + 1d)]
        public static double Test_Expression_Variable3(string input)
        {
            var res = Ink.Evaluate(input);

            var result = 0d;

            if (res is InkValue inkValue)
            {
                result = inkValue.ValueDouble;
                InkValue.Release(inkValue);
            }

            return result;
        }


        [Repeat(10000)]
        [TestCase("var a = true ;  var b = !a ; a && b         ", ExpectedResult = true && !true)]
        [TestCase("var a = CRE(1,3) ;   GET(a, Rarity) == 3    ", ExpectedResult = true)]
        public static bool Test_Expression_Variable4(string input)
        {
            var res = Ink.Evaluate(input);

            var result = false;

            if (res is InkValue inkValue)
            {
                result = inkValue.ValueBool;
                InkValue.Release(inkValue);
            }

            return result;
        }


        #endregion


        #region 3.Function: Function call


        //[Repeat(10000)]
        [TestCase("SUM(SUM(1,2,3),SUM(1,2,3),1) + 123456789 ",  ExpectedResult = 1         + 2 + 3 + 1 + 2 + 3 + 1 + 123456789)]
        [TestCase("SUM(SUM(1,2,-3),SUM(1,2,3),1) + SUM(1,2,3)", ExpectedResult = 1 + 2 - 3 + 1 + 2 + 3 + 1 + 1 + 2 + 3)]
        [TestCase("SUM(1,1-2,3)",                               ExpectedResult = 3)]
        [TestCase("SUM(1,-2,3)",                                ExpectedResult = 2)]
        [TestCase("SUM(1,2,-3)",                                ExpectedResult = 1 + 2 - 3)]
        [TestCase("SUM(1,2,3);SUM(1,2,3);",                     ExpectedResult = 6)]
        [TestCase("SUM(1,2,3);",                                ExpectedResult = 6)]
        public static int Test_Expression_Function(string input)
        {
            var res = Ink.Evaluate(input);

            var result = 0;

            if (res is InkValue inkValue)
            {
                result = inkValue.ValueInt;
                InkValue.Release(inkValue);
            }

#if DEBUG
            Console.WriteLine(InkValue.GetTime);
            Console.WriteLine(InkValue.ReleaseTime);
#endif

            return result;
        }

        [TestCase("PAY(Food,100);PAY(Food,100)")]
        [TestCase("LOG(\"Hello World ! \"+\"Hello World ! \" ) ")]
        public static void Test_Expression_Function2(string input)
        {
            var res = Ink.Evaluate(input);

            if (res is InkValue inkValue)
            {
                InkValue.Release(inkValue);
            }

#if DEBUG
            Console.WriteLine(InkValue.GetTime);
            Console.WriteLine(InkValue.ReleaseTime);
#endif
        }


        [Test]
        public static void Test_Expression_Function_IsolatedPerInstance()
        {
            // functions belong to the instance that registered them : there is no global function table
            var owner = new UniInk();

            owner.RegisterFunction("ONLYA", new InkFunction(_ => InkValue.GetIntValue(7)));

            Assert.That(owner.Evaluate("ONLYA()").GetResult_Int(), Is.EqualTo(7));

            var other = new UniInk();

            Assert.Throws<InkSyntaxException>(() => other.Evaluate("ONLYA()"));
        }


        #endregion


        #region 4.Best Practice: Lambda Function


        [Repeat(10000)]
        [TestCase("FLT(Config,var b => GET(b, Rarity) == 2)")]
        [TestCase("var cards = FLT(Config,var c => GET(c, Rarity) == 2  && GET(c, ID) == 1)")]
        public static void Test_Expression_Lambda(string input)
        {
            var res = Ink.Evaluate(input);

            // "res != null" would call InkValue.op_Inequality and throw , so check the reference instead
            if (res is not null)
            {
                if (res.ValueObject is List<Card> cards)
                {
                    Console.WriteLine(cards.Count);
                    foreach (var card in cards)
                    {
                        Console.WriteLine(card.ID);
                        Console.WriteLine(card.Rarity);
                    }
                }

                InkValue.Release(res);
            }

            // Console.WriteLine(InkValue.GetTime);
            // Console.WriteLine(InkValue.ReleaseTime);
        }


        #endregion


        #region 5.Best Practice: Property Getter (setter will be supported in the future...)


        [Repeat(10000)]
        [TestCase("grower + 100000")]
        [TestCase("GET(Target, 1001)")]
        public static void Test_Expression_Getter(string input)
        {
            var test = Ink.Evaluate(input);
            if (test is InkValue value)
            {
                //Console.WriteLine(value.Value_int); // each time , the result will be different

                InkValue.Release(value);
            }
        }


        #endregion


        #region SP : Special Test


        [Repeat(10000)]
        [TestCase(" ",   ExpectedResult = null)]
        [TestCase("",    ExpectedResult = null)] // string.Empty
        [TestCase(";;",  ExpectedResult = null)]
        [TestCase(";",   ExpectedResult = null)]
        [TestCase("()",  ExpectedResult = null)]
        [TestCase("( )", ExpectedResult = null)]
        //[TestCase(null, ExpectedResult = null)] // null will throw exception
        public static object Test_Expression_SP(string input)
        {
            return Ink.Evaluate(input);
        }


        #endregion


        [Test]
        public static void Test_Temp()
        {
            const string TestInput_Arithmetic_01 =
                "12345678+87654321-1*2*3*4*5*6*7*8*9+9*8*7*6*5*4*3*2*1+1*2*3*4*5*6*7*8*9-87654321-12345678"; //

            var res = Ink.Evaluate(TestInput_Arithmetic_01);

            if (res is InkValue inkValue)
            {
                Console.WriteLine(inkValue.ValueInt);
                InkValue.Release(inkValue);
            }
        }

        ///////////////////////////////////////////////  Pool Contract Test ///////////////////////////////////////////

        #region SP : Pool contract : a double release must be reported


        [Test]
        public static void Test_Pool_DoubleRelease_Is_Reported()
        {
            var value = InkValue.Get();

            InkValue.Release(value);

#if DEBUG
            // a second release is a bug : the instance would be handed out twice afterwards
            Assert.Throws<InvalidOperationException>(() => InkValue.Release(value));
#endif

            // cleanup code should not need its own null check
            InkValue.Release(null);

            // a released instance must never be handed out twice
            var leased = new List<InkValue>();

            for (var i = 0 ; i < 64 ; i++)
            {
                var one = InkValue.Get();

                Assert.That(leased.Contains(one), Is.False, "the pool must not hand out the same instance twice");

                leased.Add(one);
            }

            foreach (var one in leased)
            {
                InkValue.Release(one);
            }
        }


        #endregion

        #region 6.Threading : reentrancy and parallel evaluation


        [Test]
        public static void Test_Reentrancy_NestedEvaluate()
        {
            // a registered function may evaluate again on the same instance : the inner call must not release the
            // temp objects of the outer call
            var ink = new UniInk();

            ink.RegisterFunction("NEST", new InkFunction(_ => ink.Evaluate("1 + 1")));

            Assert.That(ink.Evaluate("NEST(1) + 3").GetResult_Int(), Is.EqualTo(5));

            // the temp variable of the outer call must survive the nested evaluation ( it used to be released and reset )
            Assert.That(ink.Evaluate("var a = 10; NEST(1) + a").GetResult_Int(), Is.EqualTo(12));
        }


        [Test]
        public static void Test_ThreadSafety_PerThreadInstances()
        {
            const int THREADS = 8;
            const int ROUNDS  = 500;

            var failures = 0;

            Parallel.For(0, THREADS, thread =>
            {
                var ink = new UniInk();
                var rnd = new Random(thread);

                for (var i = 0 ; i < ROUNDS ; i++)
                {
                    var a = rnd.Next(100);
                    var b = rnd.Next(100);

                    try
                    {
                        if (ink.Evaluate($"{a} * {b} + {a}").GetResult_Int() != a * b + a)
                        {
                            Interlocked.Increment(ref failures);
                        }
                    }
                    catch (Exception)
                    {
                        Interlocked.Increment(ref failures);
                    }
                }
            });

            Assert.That(failures, Is.Zero, "every thread must be able to evaluate with its own instance");
        }


        [Test]
        public static void Test_ThreadSafety_SharedInstance()
        {
            const int THREADS = 8;
            const int ROUNDS  = 500;

            var shared   = new UniInk();
            var failures = 0;

            var cards = new List<Card> { new() { ID = 1, Rarity = 2 }, new() { ID = 2, Rarity = 5 }, new() { ID = 3, Rarity = 2 } };

            shared.RegisterVariable("Cards",  InkValue.GetObjectValue(cards));
            shared.RegisterVariable("Rarity", InkValue.GetIntValue(5));
            shared.RegisterFunction("SUM", new InkFunction(prms => InkValue.GetIntValue(
                                                                                        (int)(InkValue)prms[0] + (int)(InkValue)prms[1] + (int)(InkValue)prms[2])));
            shared.RegisterFunction("GET", new InkFunction(prms => InkValue.GetIntValue(((Card)((InkValue)prms[0]).ValueObject).Rarity)));
            shared.RegisterFunction("FLT", new InkFunction(prms =>
            {
                var source = (List<Card>)((InkValue)prms[0]).ValueObject;
                var filter = (Predicate<object>)((InkValue)prms[1]).ValueObject;
                var list   = new List<Card>();

                foreach (var card in source)
                {
                    if (filter(card))
                    {
                        list.Add(card);
                    }
                }

                return list;
            }));

            Parallel.For(0, THREADS, thread =>
            {
                var rnd = new Random(thread);

                for (var i = 0 ; i < ROUNDS ; i++)
                {
                    var a = rnd.Next(100);
                    var b = rnd.Next(100);

                    try
                    {
                        var sum  = shared.Evaluate($"SUM({a}, {b}, 1) + {a}").GetResult_Int();
                        var temp = shared.Evaluate("var x = 5; x * 2").GetResult_Int();
                        var flt  = shared.Evaluate("FLT(Cards, var c => GET(c, Rarity) == 2)");
                        var hits = ((List<Card>)flt.ValueObject).Count;

                        InkValue.Release(flt);

                        if (sum != a + b + 1 + a || temp != 10 || hits != 2)
                        {
                            Interlocked.Increment(ref failures);
                        }
                    }
                    catch (Exception)
                    {
                        Interlocked.Increment(ref failures);
                    }
                }
            });

            Assert.That(failures, Is.Zero, "the instances must not share mutable evaluation state");
        }


        #endregion


        #region 7.Value : null safety and identity semantics


        [Test]
        public static void Test_Value_NullComparison_DoesNotThrow()
        {
            // "value == null" is a legal C# pattern , it must not call Calculate on a null instance
            var value = InkValue.GetIntValue(5);

            Assert.That((bool)(value == null), Is.False);
            Assert.That((bool)(value != null), Is.True);

            InkValue.Release(value);

            InkValue left  = null;
            InkValue right = null;

            Assert.That((bool)(left == right), Is.True);
            Assert.That((bool)(left != right), Is.False);
        }
        
        #endregion


        #region 8.Promotion : implicit numeric conversion ( int -> single -> double , char -> int )


        private static double ToNumber(InkValue value)
        {
            return value.ValueType switch
            {
                TypeCode.Int32  => value.ValueInt,
                TypeCode.Single => value.ValueFloat,
                TypeCode.Double => value.ValueDouble,
                TypeCode.Char   => value.ValueChar,
                _               => throw new AssertionException($"not a number : {value.ValueType}")
            };
        }


        [TestCase("1 + 1.5",       ExpectedResult = 2.5d)]
        [TestCase("1.5 + 1",       ExpectedResult = 2.5d)]
        [TestCase("1 + 1.5f",      ExpectedResult = 2.5d)]
        [TestCase("1.5f + 1",      ExpectedResult = 2.5d)]
        [TestCase("1.5f + 2.5",    ExpectedResult = 4.0d)]
        [TestCase("2.5 + 1.5f",    ExpectedResult = 4.0d)]
        [TestCase("1 - 0.5",       ExpectedResult = 0.5d)]
        [TestCase("0.5 - 1",       ExpectedResult = -0.5d)]
        [TestCase("2 * 1.5",       ExpectedResult = 3.0d)]
        [TestCase("1.5 * 2",       ExpectedResult = 3.0d)]
        [TestCase("2 * 1.5f",      ExpectedResult = 3.0d)]
        [TestCase("1.5f * 2",      ExpectedResult = 3.0d)]
        [TestCase("7 / 2.0",       ExpectedResult = 3.5d)]
        [TestCase("7.0 / 2",       ExpectedResult = 3.5d)]
        [TestCase("7 / 2",         ExpectedResult = 3.0d)]   // int division stays int
        [TestCase("7.5 % 2",       ExpectedResult = 1.5d)]
        [TestCase("5 % 2.5",       ExpectedResult = 0.0d)]
        [TestCase("-1.5",          ExpectedResult = -1.5d)]
        [TestCase("-2.5f",         ExpectedResult = -2.5d)]
        [TestCase("'a' + 1",       ExpectedResult = 98.0d)]
        [TestCase("'a' + 'b'",     ExpectedResult = 195.0d)]
        public static double Test_Promotion_Arithmetic(string input)
        {
            var res = Ink.Evaluate(input);

            Assert.That(res is null, Is.False, $"the expression [{input}] should not be evaluated to null");

            var result = ToNumber(res!);

            InkValue.Release(res);

            return result;
        }


        [TestCase("1 < 1.5",        ExpectedResult = true)]
        [TestCase("1.5 > 1",        ExpectedResult = true)]
        [TestCase("1.5f < 2",       ExpectedResult = true)]
        [TestCase("2 > 1.5f",       ExpectedResult = true)]
        [TestCase("1.5f <= 1.5",    ExpectedResult = true)]
        [TestCase("2 <= 1.5",       ExpectedResult = false)]
        [TestCase("1 >= 1.5f",      ExpectedResult = false)]
        [TestCase("2 >= 1.5f",      ExpectedResult = true)]
        [TestCase("1 == 1.0",       ExpectedResult = true)]
        [TestCase("1 != 1.0",       ExpectedResult = false)]
        [TestCase("1.5 != 2",       ExpectedResult = true)]
        [TestCase("1.5f == 1.5",    ExpectedResult = true)]
        [TestCase("2 == 2.0f",      ExpectedResult = true)]
        [TestCase("'a' == 97",      ExpectedResult = true)]
        [TestCase("'a' < 98",       ExpectedResult = true)]
        [TestCase("'a' + 1 == 98",  ExpectedResult = true)]
        public static bool Test_Promotion_Comparison(string input)
        {
            var res = Ink.Evaluate(input);

            Assert.That(res is null, Is.False, $"the expression [{input}] should not be evaluated to null");

            var result = res!.ValueBool;

            InkValue.Release(res);

            return result;
        }


        [TestCase("1 + 1.5",    TypeCode.Double)]
        [TestCase("1.5 + 1",    TypeCode.Double)]
        [TestCase("1 + 1.5f",   TypeCode.Single)]
        [TestCase("1.5f + 1",   TypeCode.Single)]
        [TestCase("1.5f + 2.5", TypeCode.Double)]
        [TestCase("1 + 2",      TypeCode.Int32)]
        [TestCase("7 / 2",      TypeCode.Int32)]
        [TestCase("'a' + 1",    TypeCode.Int32)]
        [TestCase("'a' + 'b'",  TypeCode.Int32)]
        [TestCase("1 < 1.5",    TypeCode.Boolean)]
        [TestCase("1 == 1.0",   TypeCode.Boolean)]
        public static void Test_Promotion_ResultType(string input, TypeCode expected)
        {
            var res = Ink.Evaluate(input);

            Assert.That(res is null, Is.False, $"the expression [{input}] should not be evaluated to null");
            Assert.That(res!.ValueType, Is.EqualTo(expected));

            InkValue.Release(res);
        }


        [Test]
        public static void Test_Promotion_UnsupportedStaysUnsupported()
        {
            // no numeric promotion applies to bool / string / object operands
            Assert.Throws<InkSyntaxException>(() => Ink.Evaluate("1 + true"));
            Assert.Throws<InkSyntaxException>(() => Ink.Evaluate("\"x\" + 1"));
            Assert.Throws<InkSyntaxException>(() => Ink.Evaluate("true > false"));
        }


        #endregion


        #region 9.Errors : one exception type , with the expression and a caret


        [Test]
        public static void Test_Error_DivideByZero_IsInkSyntaxException_WithPosition()
        {
            var exception = Assert.Throws<InkSyntaxException>(() => Ink.Evaluate("1 / 0"));

            Assert.That(exception.Message, Does.Contain("division by zero"));
            Assert.That(exception.Position, Is.EqualTo(2), "the position must point at the / operator");
            Assert.That(exception.Expression, Is.EqualTo("1 / 0"));
            Assert.That(exception.ToString(), Does.Contain("^"));
        }


        [Test]
        public static void Test_Error_ModuloByZero_IsInkSyntaxException()
        {
            var exception = Assert.Throws<InkSyntaxException>(() => Ink.Evaluate("1 % 0"));

            Assert.That(exception.Message, Does.Contain("division by zero"));
            Assert.That(exception.Position, Is.EqualTo(2));
        }


        [Test]
        public static void Test_Error_DuplicateVariable_IsInkSyntaxException()
        {
            var exception = Assert.Throws<InkSyntaxException>(() => Ink.Evaluate("var a = 1; var a = 2; a"));

            Assert.That(exception.Message, Does.Contain("already declared"));
            Assert.That(exception.Position, Is.GreaterThanOrEqualTo(0));
            Assert.That(exception.Expression, Is.EqualTo("var a = 1; var a = 2; a"));
        }


        [Test]
        public static void Test_Error_UnknownIdentifier_IsInkSyntaxException()
        {
            var exception = Assert.Throws<InkSyntaxException>(() => Ink.Evaluate("1 + nope"));

            Assert.That(exception.Message, Does.Contain("unknown identifier [nope]"));
            Assert.That(exception.Position, Is.EqualTo(4));
        }


        [Test]
        public static void Test_Error_UnsupportedOperand_ReportsTheOperatorPosition()
        {
            var exception = Assert.Throws<InkSyntaxException>(() => Ink.Evaluate("1 + true"));

            Assert.That(exception.Position, Is.EqualTo(2), "the position must point at the + operator");
            Assert.That(exception.Expression, Is.EqualTo("1 + true"));
            Assert.That(exception.ToString(), Does.Contain("^"));
        }


        [Test]
        public static void Test_Error_PositionInsideNestedExpression()
        {
            var exception = Assert.Throws<InkSyntaxException>(() => Ink.Evaluate("1 + (2 / 0)"));

            Assert.That(exception.Position, Is.EqualTo(7), "the / inside the parenthesis");
            Assert.That(exception.ToString(), Does.Contain("^"));
        }


        [Test]
        public static void Test_Error_IfStatement_IsInkSyntaxException()
        {
            var exception = Assert.Throws<InkSyntaxException>(() => Ink.Evaluate_IfStatement("if ( 1 > 2 ) { return 1 } else"));

            Assert.That(exception.Message, Does.Contain("else branch is missing"));
            Assert.That(exception.Position, Is.EqualTo(26), "the position of the dangling else");
        }


        #endregion


        ///////////////////////////////////////////////  Extension Test ////////////////////////////////////////////////


        [Test]
        public static void Test_Expression_IfStatement_WithCharCondition()
        {
            // the char literals used to swallow the following token , so "if('a'<'b')" could not be parsed at all
            var result = Ink.Evaluate_IfStatement("if('a'<'b'){return 1}else{return 2}") as InkValue;

            Assert.That(result is null,   Is.False, "the if statement should return a value");
            Assert.That(result!.ValueInt, Is.EqualTo(1));

            InkValue.Release(result);
        }


        //[Repeat(1)]
        ///////////////////////////////////////
        [TestCase("  if ( 1 > 2 )       "
                + "  {                  "
                + "    LOG(Target);     "
                + "  }                  "
                + "  else               "
                + "  {                  "
                + "    LOG(Target);     "
                + "  }                  ")]
        ///////////////////////////////////////
        [TestCase("  if ( 1 > 2 )       "
                + "  {                  "
                + "     123             "
                + "  }                  "
                + "  else if ( 3 > 6 )  "
                + "  {                  "
                + "     456             "
                + "  }                  "
                + "  else               "
                + "  {                  "
                + "    return 666       "
                + "  }                  ")]
        ///////////////////////////////////////
        [TestCase("  if ( 1 > 2 )       "
                + "  {                  "
                + "     123             "
                + "  }                  "
                + "  else if ( 3 > 6 )  "
                + "  {                  "
                + "     456;            "
                + "  }                  "
                + "  else if ( 3 < 6 )  "
                + "  {                  "
                + "    return 456;      "
                + "  }                  "
                + "  else               "
                + "  {                  "
                + "    return 666       "
                + "  }                  ")]
        public static void Test_Expression_IfStatements(string input)
        {
            var test = Ink.Evaluate_IfStatement(input);
            if (test is InkValue value)
            {
#if DEBUG
                Console.WriteLine(value.ValueInt); // each time , the result will be different
#endif

                InkValue.Release(value);
            }
#if DEBUG
            Console.WriteLine(InkValue.GetTime);
            Console.WriteLine(InkValue.ReleaseTime);
#endif
        }


        ///////////////////////////////////////////////  Helper Object  ////////////////////////////////////////////////

        public enum MyEnum { Food }

        public int grower = 100;

        public static Card Target => new() { ID = 1, Rarity = 2 };


        public       int FoodNum;
        public const int Food = (int)MyEnum.Food;

        public static void PAY(MyEnum @enum, int num)
        {
            if (@enum != MyEnum.Food)
            {
                return;
            }
            else
            {
                num = num > 0 ? num : 0;
                //  @this.FoodNum -= num;
                //Console.WriteLine(" paid " + num);
            }

            //  @this.FoodNum -= num;
            //Console.WriteLine(" paid " + num);
        }

        public static void GET(Card card, int id)
        {
            if (card.ID == id)
            {
                Console.WriteLine("ID:" + card.ID);
            }
            else
            {
                Console.WriteLine("ID:" + card.Rarity);
            }
        }


        public class Card
        {
            public int ID;
            public int Rarity;

            /// <inheritdoc />
            public override string ToString() => "TEST";
        }


        public Card CRE(int id, int rarity) => new() { ID = id, Rarity = rarity };


        #region 10.Lexer : numbers , strings , escapes and comments


        [TestCase("42",           ExpectedResult = 42)]
        [TestCase("-42",          ExpectedResult = -42)]
        [TestCase("1 + 2 * 3",    ExpectedResult = 7)]
        [TestCase("2 * 3 + 1",    ExpectedResult = 7)]
        [TestCase("10 - 2 - 3",   ExpectedResult = 5)]
        [TestCase("100 / 10 / 5", ExpectedResult = 2)]
        public static int Test_Lexer_Integer(string input)
        {
            var value = Ink.Evaluate(input);

            var result = value!.ValueInt;

            InkValue.Release(value);

            return result;
        }


        [TestCase("1.5f + 1.5f",  ExpectedResult = 3f)]
        [TestCase("2.5f * 2",     ExpectedResult = 5f)]
        public static float Test_Lexer_Single(string input)
        {
            var value = Ink.Evaluate(input);

            var result = value!.ValueFloat;

            InkValue.Release(value);

            return result;
        }


        [TestCase("'a'",      ExpectedResult = 'a')]
        [TestCase("'\\n'",   ExpectedResult = '\n')]
        [TestCase("'\\''",   ExpectedResult = '\'')]
        public static char Test_Lexer_Char(string input)
        {
            var value = Ink.Evaluate(input);

            var result = value!.ValueChar;

            InkValue.Release(value);

            return result;
        }


        [TestCase(@"""hello""",        ExpectedResult = "hello")]
        [TestCase(@"""a"" + ""b""",     ExpectedResult = "ab")]
        public static string Test_Lexer_String(string input)
        {
            var value = Ink.Evaluate(input);

            var result = value!.ValueString;

            InkValue.Release(value);

            return result;
        }


        [Test]
        public static void Test_Lexer_RemoveComments()
        {
            const string script = @"var a = 1; // the first comment
                                    /* and a block
                                       comment */
                                    a";

            Assert.That(UniInkExtensions.RemoveComments(script), Does.Not.Contain("comment"));
        }


        [Test]
        public static void Test_Lexer_RemoveComments_KeepsStringLiterals()
        {
            Assert.That(UniInkExtensions.RemoveComments(@"""// not a comment"""), Is.EqualTo(@"""// not a comment"""));
        }


        [Test]
        public static void Test_Script_VariableAndSemicolon()
        {
            var value = Ink.Evaluate("var a = 1; var b = 2; a + b");

            Assert.That(value!.ValueInt, Is.EqualTo(3));

            InkValue.Release(value);
        }


        [TestCase("if ( 1 < 2 ) { return 1 } else { return 2 }", ExpectedResult = 1)]
        [TestCase("if ( 1 > 2 ) { return 1 } else { return 2 }", ExpectedResult = 2)]
        [TestCase("if ( 1 > 2 ) { return 1 } else if ( 3 < 4 ) { return 3 } else { return 4 }", ExpectedResult = 3)]
        [TestCase("if ( 1 > 2 ) { return 1 } else if ( 3 > 4 ) { return 3 } else { return 4 }", ExpectedResult = 4)]
        public static int Test_Script_IfStatement(string input)
        {
            var result = Ink.Evaluate_IfStatement(input);

            if (result is InkValue value)
            {
                var answer = value.ValueInt;

                InkValue.Release(value);

                return answer;
            }

            return -1;
        }


        [Test]
        public static void Test_Extension_EvaluateInt_ReleasesTheValue()
        {
            Assert.That(Ink.EvaluateInt("1 + 2"), Is.EqualTo(3));
        }


        [Test]
        public static void Test_Extension_EvaluateBool()
        {
            Assert.That(Ink.EvaluateBool("1 < 2"), Is.True);
            Assert.That(Ink.EvaluateBool("1 > 2"), Is.False);
        }


        #endregion


        #region 11.Pool contract : a malformed unary operator must never release its operand twice


        [TestCase("*1")]
        [TestCase("/1")]
        [TestCase("%1")]
        [TestCase("==1")]
        [TestCase("!=1")]
        [TestCase("<1")]
        [TestCase(">1")]
        [TestCase("<=1")]
        [TestCase(">=1")]
        [TestCase("||1")]
        [TestCase("&&1")]
        public static void Test_Unary_MissingLeftOperand_IsReported(string input)
        {
            // these forms used to hand the operand back . The syntax list owns that instance , so it was
            // released twice and the pool handed the same instance to two different computations
            Assert.Throws<InkSyntaxException>(() => Ink.Evaluate(input));
        }


        [TestCase("+1",          ExpectedResult = 1)]
        [TestCase("-1",          ExpectedResult = -1)]
        [TestCase("-(-1)",       ExpectedResult = 1)]
        [TestCase("+123456789",  ExpectedResult = 123456789)]
        [TestCase("-1 * 2",      ExpectedResult = -2)]
        [TestCase("(-1) * (-2)", ExpectedResult = 2)]
        public static int Test_Unary_StillSupported(string input)
        {
            return Ink.Evaluate(input).GetResult_Int();
        }


        [TestCase("!true",    ExpectedResult = false)]
        [TestCase("!false",   ExpectedResult = true)]
        [TestCase("!(1 < 2)", ExpectedResult = false)]
        public static bool Test_Unary_LogicalNot_StillSupported(string input)
        {
            return Ink.Evaluate(input).GetResult_Bool();
        }


        [Test]
        public static void Test_Unary_MissingLeftOperand_DoesNotCorruptThePool()
        {
            var malformed = new[] { "*1", "/1", "==1", "<1", ">1", "<=1", ">=1", "!=1", "||1" };

            for (var i = 0 ; i < 64 ; i++)
            {
                Assert.That(Ink.Evaluate("1 + 2 * 3").GetResult_Int(), Is.EqualTo(7));
            }

            for (var round = 0 ; round < 50 ; round++)
            {
                foreach (var input in malformed)
                {
                    Assert.Throws<InkSyntaxException>(() => Ink.Evaluate(input));

                    // a failed evaluation must leave the pool usable
                    Assert.That(Ink.Evaluate("1 + 2 * 3").GetResult_Int(), Is.EqualTo(7));
                }
            }

            AssertPoolHasNoDuplicateInstances();
        }


        [Test]
        public static void Test_Function_ReturningItsOwnArgument_DoesNotCorruptThePool()
        {
            var ink = new UniInk();

            // a function that returns its argument unchanged : the result used to be released twice
            ink.RegisterFunction("ID", new InkFunction(prms => prms[0]));

            for (var i = 0 ; i < 100 ; i++)
            {
                Assert.That(ink.Evaluate("ID(7)").GetResult_Int(),                Is.EqualTo(7));
                Assert.That(ink.Evaluate("ID(1) + 1").GetResult_Int(),            Is.EqualTo(2));
                Assert.That(ink.Evaluate("ID(ID(3)) * 2").GetResult_Int(),        Is.EqualTo(6));
                Assert.That(ink.Evaluate("var a = ID(5); a + 1").GetResult_Int(), Is.EqualTo(6));
            }

            AssertPoolHasNoDuplicateInstances();
        }


        /// <summary> Lease every pooled instance : the pool must never contain the same instance twice </summary>
        private static void AssertPoolHasNoDuplicateInstances()
        {
            var pool   = InkEvalContext.Current.ValuePool;
            var leased = new List<InkValue>(pool.Count);

            while (pool.Count > 0)
            {
                var value = InkValue.Get();

                foreach (var other in leased)
                {
                    Assert.That(ReferenceEquals(other, value), Is.False,
                                "the pool must not hand out the same InkValue instance twice");
                }

                leased.Add(value);
            }

            foreach (var value in leased)
            {
                InkValue.Release(value);
            }
        }


        #endregion


        public List<Card> Config = new()
        {
            new Card() { ID = 1, Rarity = 2 } //
          , new Card() { ID = 2, Rarity = 2 } //
          , new Card() { ID = 3, Rarity = 3 } //
          , new Card() { ID = 4, Rarity = 4 } //
          , new Card() { ID = 5, Rarity = 5 } //
          , new Card() { ID = 6, Rarity = 6 }
        };
    }

}