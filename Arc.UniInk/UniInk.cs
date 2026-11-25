namespace Arc.UniInk
{

    // ReSharper disable SpecifyACultureInStringConversionExplicitly
    // ReSharper disable PartialTypeWithSinglePart
    using System;
    using System.Collections.Generic;

    /* ================================================== SUMMARY  ================================================== */
    /* Title    :  UniInk (https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator)                       */
    /* Author   :  Arc (https://github.com/Arc-huangjingtong)                                                         */
    /* Version  :  1.3.5 (AOT expression evaluator)                                                                    */
    /* Licence  :  MIT (https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator/blob/main/LICENSE)        */
    /* Support  :  [netstandard2.0 / 2.1] [C# 9.0+] [IL2CPP Support]                                                   */
    /* Feature  :  [Easy-use] [High performance] [zero box & unbox] [zero GC!] [zero reflection runtime]              */
    /* ============================================================================================================== */

    /*===================================================  GUIDE  ====================================================*/
    /* 1. Quickly Start :                                                                                             */
    /*    // 1.1 create a new instance                                                                                */
    /*    var ink   = new UniInk();                                                                                   */
    /*                                                                                                                */
    /*    // 1.2 use Evaluate and input an expression string                                                          */
    /*    var res01 = ink.Evaluate("3 + 5 * 2").GetResult_Int();                                                      */
    /* 2. Operate Features :                                                                                          */
    /*    // 2.1 Auto ignore WhiteSpace                                                                               */
    /*    // 2.2 Auto ignore case(Ff,Dd,(T)true,(F)false)                                                             */
    /*    var res02 = ink.Evaluate("3333.333f-3.3f+3.3f+  3.3f - 3.3F").GetResult_Float()                             */
    /*    var res03 = ink.Evaluate("true && false || True && 1+1==1+1").GetResult_Bool();                             */
    /*    // 2.3 Arithmetic overflow (because the compute is runtime)                                                 */
    /*    var res04 = ink.Evaluate("999 * 123123 * 321321 / 999 / 666").GetResult_Int();                              */
    /*    var ans04 = unchecked(999 * 123123 * 321321) / 999 / 666);                                                  */
    /*    // res04 == ans04 == 232                                                                                    */
    /*    // 2.4 Don't support two operators together                                                                 */
    /*    var worry01 = "9 * -1"                                                                                      */
    /*    var suggest01 = "9 * (-1)"; // you should use parenthesis to avoid the worry01                              */
    /*    var suggest02 = "-1*9"; // just those two operators are not together to avoid the worry01                   */
    /*                                                                                                                */
    /*    // 2.5 String add is zero GC too(the inner logic is List<char>                                              */
    /*    var res05 = ink.Evaluate("\"Hello\"+\"World\"+\"!\"").GetResult_String();                                   */
    /* 3. Call C# Layer                                                                                               */
    /*    // 3.1 Register a local function(param: IList<object> return: object                                        */
    /*    object SUM(IList<object> list)                                                                              */
    /*    {                                                                                                           */
    /*       // the param and return value must be InkValue,and its will be auto released                             */
    /*       // If you are not in the above situations to generate InkValue, you will produce GC                      */
    /*       // Such as :                                                                                             */
    /*       // var temp = inkValue + inkValue                                                                        */
    /*       // and temp is not return value, you must use InkValue.Release(temp) to release it                       */
    /*       var param1 = (InkValue)list[0];                                                                          */
    /*       var param2 = (InkValue)list[1];                                                                          */
    /*       var param3 = (InkValue)list[2];                                                                          */
    /*       var sum    = InkValue.GetIntValue(param1.Value_int + param2.Value_int + param3.Value_int);               */
    /*       return sum;                                                                                              */
    /*     }                                                                                                          */
    /*     // the register function API                                                                               */
    /*     Ink.RegisterFunction("SUM", new InkFunction(SUM));                                                         */
    /*     var res06 = Ink.Evaluate("SUM(SUM(1,2,-3),SUM(1,2,3),1) + SUM(1,2,3)")                                     */
    /*                                                                                                                */
    /*     // 3.2 Register Variable                                                                                   */
    /*     Ink.RegisterVariable("Age", InkValue.GetIntValue(25));                                                     */
    /*     Ink.RegisterVariable("Ages", InkValue.GetObjectValue(new List<int>(){11,12,13,14,15}));                    */
    /*     Ink.RegisterVariable("IsBoy", InkValue.GetBoolValue(true));                                                */
    /*                                                                                                                */
    /*  4. Scripts Features                                                                                           */
    /*     // 4.1 Evaluate Scripts(use Semicolon[;] to split the scripts)                                             */
    /*     // 4.2 The last expression will be the result of the script, so you can use it to return a value           */
    /*     var res07 = Ink.Evaluate("var a = 1; var b = 2; a + b;").GetResult_Int();                                  */
    /*     // 4.3 Can use registered functions and variables in scripts                                               */
    /*     var res08 = Ink.Evaluate("var a = 1; var b = 2; SUM(a,b,Age);").GetResult_Int();                           */
    /*                                                                                                                */
    /*  5. Other Features                                                                                             */
    /*                                                                                                                */
    /*     // 5.1 Custom Lambda Predicate<T> Expression                                                               */
    /*     List<int> AgeSearch(IList<int> ages, Predicate<int> func)                                                  */
    /*     {                                                                                                          */
    /*         var list = new List<int>();                                                                            */
    /*                                                                                                                */
    /*         foreach (var card in ages)                                                                             */
    /*         {                                                                                                      */
    /*             if (func(card))                                                                                    */
    /*             {                                                                                                  */
    /*                list.Add(card);                                                                                 */
    /*             }                                                                                                  */
    /*         }                                                                                                      */
    /*                                                                                                                */
    /*         return list;                                                                                           */
    /*     }                                                                                                          */
    /*     var res09 = Ink.Evaluate("AgeSearch(Ages,var b => GET(b, Rarity) == 2)").GetResult_Object();               */
    /*                                                                                                                */
    /*     // 5.2 Custom Getter (when you evaluate the variable, it will be auto called)                              */
    /*     Ink.RegisterVariable("grower", InkValue.SetGetter(InkValue.GetIntValue(0), value => value.Value_int++));   */
    /*                                                                                                                */
    /*                                                                                                                */
    /*                                                                                                                */
    /*================================================================================================================*/

    /// <summary> The C# Evaluator easy to use : you can execute simple expression or scripts with a string </summary>
    /// <remarks> If you want to custom your own rules , you should read the code easily and modify it ! </remarks>
    public partial class UniInk
    {
        /***************************************************  Cfgs  ***************************************************/

        internal const float  EPSILON_FLOAT     = 0.000001f; // The epsilon value for float
        internal const double EPSILON_DOUBLE    = 0.000001d; // The epsilon value for double
        internal const int    FUNC_NAME_MAX_LEN = 10;        // Max length of function name
        internal const int    VARI_NAME_MAX_LEN = 10;        // Max length of variable name

        internal const int INK_VALUE_POOL_CAPACITY  = 128;  // The capacity of InkValue.Pool
        internal const int INK_SYNTAX_POOL_CAPACITY = 64;   // The capacity of InkSyntaxList.Pool and Lambda.Pool
        internal const int STRING_MAX_LEN           = 128;  // The max length of string that will be parsed
        internal const int EXPRESS_ELEMENT_MAX_LEN  = 1024; // The max length of expression's element that will be parsed
        internal const int FUNCTION_CAPACITY        = 32;   // The capacity of UniInk.dic_Functions
        internal const int VARIABLE_CAPACITY        = 32;   // The capacity of UniInk.dic_Variables
        internal const int VARIABLE_TEMP_CAPACITY   = 32;   // The capacity of UniInk.dic_Variables_Temp
        internal const int OPERATOR_FUNC_CAPACITY   = 32;   // The capacity of InkOperator.Dic_OperatorsFunc
        internal const int INK_OPERATOR_CAPACITY    = 64;   // The capacity of InkOperator.Dic_Values

        /***************************************************  Ctor  ***************************************************/

        /// <summary> Default Constructor : Initialize variables and parsing Methods </summary>
        /// <remarks> the variables are saved the object’s reference, not the value </remarks>
        /// <param name="variables"> Set variables can replace a key string with value object </param>
        public UniInk(IDictionary<string, InkValue> variables = null)
        {
            const int PARSING_METHODS_CAPACITY = 7;
            ParsingMethods = new List<ParsingMethodDelegate>(PARSING_METHODS_CAPACITY)
            {
                EvaluateOperators
              , EvaluateFunction //
              , EvaluateNumber   //
              , EvaluateChar     //
              , EvaluateString   //
              , EvaluateBool     //
              , EvaluateVariable //
            };

            if (variables == null) return;

            foreach (var variable in variables)
            {
                DicVariables.Add(GetStringSliceHashCode(variable.Key, 0, variable.Key.Length - 1), variable.Value);
            }
        }

        /***************************************************  APIs  ***************************************************/

        /// <summary> Evaluate an expression or simple scripts </summary>
        /// <returns> return the result object , when valueType , will be replaced to a InkValue </returns>
        public InkValue Evaluate(string expression) => Evaluate(expression, 0, expression.Length - 1);

        /// <summary> Evaluate an expression or simple scripts in string slice </summary>
        /// <returns> return the result object , when valueType , will be replaced to a InkValue </returns>
        /// <param name="expression"> the expression to Evaluate </param>
        /// <param name="startIndex"> The start index of the expression(contain the start index) </param>
        /// <param name="endIndex"> The end index of the expression(contain the end index) </param>
        public InkValue Evaluate(string expression, int startIndex, int endIndex)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return null;
            }

            var context = InkEvalContext.Current;

            context.PushFrame(expression);

            InkSyntaxList keys = null;

            try
            {
                keys = CompileLexerAndFill(expression, startIndex, endIndex);

                return ExecuteProcess(keys) as InkValue;
            }
            catch (InkSyntaxException exception) when (exception.Expression is null)
            {
                throw exception.WithLocation(-1, expression); // attach the expression when no frame knew it
            }
            finally
            {
                // release even when the parsing throws , otherwise the pools silently shrink on every syntax error
                if (keys is not null)
                {
                    InkSyntaxList.ReleaseAll(keys);
                }

                context.PopFrame(); // releases the temp lists and temp variables of this frame
            }
        }

        /// <summary> Register a local function </summary>
        public void RegisterFunction(string fucName, InkFunction inkFunc)
        {
            var hash = GetStringSliceHashCode(fucName, 0, fucName.Length - 1);
            if (!DicFunctions.ContainsKey(hash))
            {
                DicFunctions.Add(hash, inkFunc);
            }
        }

        /// <summary> Register a local variable </summary>
        public void RegisterVariable(string varName, InkValue inkValue)
        {
            var hash = GetStringSliceHashCode(varName, 0, varName.Length - 1);
#if DEBUG
            InkValue.GetTime--; // this value is kept by the table and will never be released
#endif
            if (!DicVariables.ContainsKey(hash))
            {
                inkValue.DontRelease = true;
                DicVariables.Add(hash, inkValue);
            }
        }

        /// <summary> UniInk Lexer  :   Fill the SyntaxList       </summary>
        public InkSyntaxList CompileLexerAndFill(string expression, int startIndex, int endIndex)
        {
            var keys = InkSyntaxList.Get();

            for (var i = startIndex ; i <= endIndex ; i++)
            {
                if (char.IsWhiteSpace(expression[i])) continue;

                var any        = false;
                var tokenIndex = i;

                foreach (var parsingMethod in ParsingMethods)
                {
                    if (parsingMethod(expression, keys, ref i))
                    {
                        any = true;
                        break;
                    }
                }

                if (!any)
                {
                    throw UnknownToken(expression, tokenIndex);
                }

                keys.PositionList.Add(tokenIndex);
            }

            if (keys.Count > 0 && Equals(keys[keys.Count - 1], InkOperator.Semicolon))
            {
                keys.ObjectList.RemoveAt(keys.Count - 1);

                if (keys.PositionList.Count > 0)
                {
                    keys.PositionList.RemoveAt(keys.PositionList.Count - 1);
                }
            }

            return keys;
        }

        /// <summary> Build the error for a character that no lexer rule accepts </summary>
        private static InkSyntaxException UnknownToken(string expression, int index)
        {
            var c = expression[index];

            if (!char.IsLetter(c) && c != '_')
            {
                return new InkSyntaxException($"unexpected character [{c}]", index, expression);
            }

            var end = index + 1;

            while (end < expression.Length && (char.IsLetterOrDigit(expression[end]) || expression[end] == '_'))
            {
                end++;
            }

            var name = expression.Substring(index, end - index);

            return new InkSyntaxException($"unknown identifier [{name}] : register the variable or function first", index, expression);
        }

        /// <summary> UniInk process the compiled SyntaxList </summary>
        /// <remarks> The returned object is an InkValue owned by the caller : release it ( or use GetResult_* ) </remarks>
        public static object ExecuteProcess(InkSyntaxList keys)
        {
            var res = InputIsScript(keys) ? ProcessList_Scripts(keys) : ProcessList(keys, 0, keys.Count - 1);

            return res;
        }

        /// <summary> Release the resources in UniInk </summary>
        public void RecoverResources(InkSyntaxList keys)
        {
            InkSyntaxList.ReleaseAll(keys);
            InkSyntaxList.ReleaseTemp();

            ReleaseTempVariables();
        }

        /// <summary> Clear the cache in UniInk anytime , Internal cache pool will be clear </summary>
        /// <remarks> Most of the time you don't need to call </remarks>
        public static void ClearCache()
        {
            InkValue.ReleasePool();
            InkSyntaxList.ReleasePool();
        }

        /*************************************************  Process  **************************************************/

        /// <summary> UniInk SyntaxList : Process the SyntaxList span </summary>
        protected static object ProcessList(InkSyntaxList syntaxList, int start, int end)
        {
            if (start > end) return null;

            ProcessList_Lambda(syntaxList, start, end);
            ProcessList_Parentheses(syntaxList, start, end);
            ProcessList_Operators(syntaxList, start, end);

            var resultCache = syntaxList.CastOther[start];

            if (resultCache is InkValue inkValue)
            {
                var copy = InkValue.Get();
                inkValue.CopyTo(copy);
                resultCache = copy;
            }

            return resultCache;
        }

        /// <summary> UniInk SyntaxList : Process the Scripts in SyntaxList </summary>
        protected static object ProcessList_Scripts(InkSyntaxList keys)
        {
            var    start = 0;
            object res;

            while (true)
            {
                var (success, index) = FindOperator(keys, InkOperator.Semicolon, start, keys.Count - 1);

                if (success)
                {
                    ProcessList(keys, start, index - 1);
                    keys.SetDirty(index);
                    start = index + 1;
                    continue;
                }

                res = ProcessList(keys, start, index); //index is the last index

                break;
            }

            return res;
        }

        /// <summary> UniInk SyntaxList : Process the Parentheses in SyntaxList </summary>
        protected static void ProcessList_Parentheses(InkSyntaxList keys, int start, int end)
        {
            var hasParentheses = true;

            while (hasParentheses)
            {
                int startIndex, endIndex;

                (hasParentheses, startIndex, endIndex) = FindSection(keys, InkOperator.ParenthesisLeft, InkOperator.ParenthesisRight, start, end);

                if (!hasParentheses) continue;

                object result = null;

                if (startIndex > 0 && keys[startIndex - 1] is InkFunction)
                {
                    ProcessList_Functions(keys, startIndex, endIndex);
                }
                else
                {
                    ProcessList_Operators(keys, startIndex + 1, endIndex - 1);

                    // move the section result into the [ ( ] slot , that's where ProcessList reads the result
                    if (startIndex + 1 <= endIndex - 1)
                    {
                        result = keys.CastOther[startIndex + 1];

                        keys.CastOther[startIndex + 1] = null; // the [ ( ] slot owns the value from now on
                    }
                }

                keys.SetDirty(result, startIndex, endIndex);
            }
        }

        /// <summary> UniInk SyntaxList : Process the Operators in SyntaxList </summary>
        protected static void ProcessList_Operators(InkSyntaxList keys, int start, int end)
        {
            var hasOperators = true;

            while (hasOperators)
            {
                var (curOperator, index) = GetHighestPriorityOperator(keys, start, end);

                if (Equals(curOperator, InkOperator.Empty) || Equals(curOperator, InkOperator.Semicolon))
                {
                    hasOperators = false;
                    continue;
                }

                object left  = null;
                object right = null;

                var lNeedRelease = false;
                var rNeedRelease = false;

                var startIndex = index;
                var endIndex   = index;

                for (var i = index - 1 ; i >= start ; i--) // Left
                {
                    if (keys.IndexDirty[i])
                    {
                        if (keys.CastOther[i] == null) continue;

                        left              = keys.CastOther[i];
                        keys.CastOther[i] = null;

                        lNeedRelease = true;

                        startIndex = i;

                        break;
                    }

                    left = keys[i];

                    startIndex = i;

                    break;
                }

                if (left is InkOperator)
                {
                    left       = null;
                    startIndex = index;
                }

                for (var i = index + 1 ; i <= end ; i++) // Right
                {
                    if (keys.IndexDirty[i])
                    {
                        if (keys.CastOther[i] == null) continue;

                        right             = keys.CastOther[i];
                        keys.CastOther[i] = null;

                        rNeedRelease = true;

                        endIndex = i;

                        break;
                    }

                    right = keys[i];

                    endIndex = i;

                    break;
                }

                if (DicOperatorsFunc.TryGetValue(curOperator, out var func))
                {
                    object    result = null;
                    Exception error  = null;

                    try
                    {
                        result = func(left, right);
                    }
                    catch (InkSyntaxException exception)
                    {
                        error = exception.WithLocation(keys.PositionAt(index), InkEvalContext.Current.Source);
                    }
                    catch (DivideByZeroException)
                    {
                        error = new InkSyntaxException($"division by zero with [{curOperator.Name}]", keys.PositionAt(index), InkEvalContext.Current.Source);
                    }

                    // return the operands to the pool on both paths , otherwise every error drains it
                    if (lNeedRelease && left is InkValue inkValueL)
                    {
                        InkValue.Release(inkValueL);
                    }

                    if (rNeedRelease && right is InkValue inkValueR)
                    {
                        InkValue.Release(inkValueR);
                    }

                    if (error is not null)
                    {
                        throw error;
                    }

                    keys.SetDirty(result, startIndex, endIndex);
                }
                else
                {
                    throw new InkSyntaxException($"unknown operator [{curOperator.Name}]", keys.PositionAt(index), InkEvalContext.Current.Source);
                }
            }

            for (var i = start ; i <= end ; i++)
            {
                if (keys.IndexDirty[i]) continue;

                if (keys.ObjectList[i] is InkValue inkValue)
                {
                    if (keys.CastOther[i] == null)
                    {
                        keys.CastOther[i] = inkValue.Clone();
                    }
                    else if (keys.CastOther[i] is InkValue inkValue2)
                    {
                        inkValue.CopyTo(inkValue2);
                    }
                }
                else if (keys.ObjectList[i] != null)
                {
                    keys.CastOther[i] = keys.ObjectList[i];
                }

                keys.SetDirty(i);
            }
        }

        /// <summary> UniInk SyntaxList : Process the Functions in SyntaxList </summary>
        protected static void ProcessList_Functions(InkSyntaxList keys, int paramStart, int paramEnd)
        {
            var func         = keys[paramStart - 1] as InkFunction;
            var paramList    = InkSyntaxList.GetTemp();
            var sectionStart = 0;

            for (var i = paramStart + 1 ; i <= paramEnd - 1 ; i++)
            {
                object current;

                if (keys.IndexDirty[i])
                {
                    if (keys.CastOther[i] == null) continue;

                    current = keys.CastOther[i];

                    keys.CastOther[i] = null;
                }
                else
                {
                    current = keys[i] is InkValue inkValue ? inkValue.Clone() : keys[i];
                }

                if (current == null) continue;

                if (Equals(current, InkOperator.Comma))
                {
                    ProcessList_Operators(paramList, sectionStart, paramList.Count - 1);
                    sectionStart = paramList.Count;
                }
                else
                {
                    paramList.Add(current);
                }
            }

            ProcessList_Operators(paramList, sectionStart, paramList.Count - 1);

            ObjectRemoveNull(paramList.CastOther);
            var result = func?.FuncDelegate.Invoke(paramList.CastOther);

            // the function may return one of its own arguments : that instance is owned by the temporary
            // parameter list and is released when the frame ends , so the caller must get a copy instead
            if (result is InkValue returned)
            {
                if (IsOwnedBy(returned, paramList.CastOther))
                {
                    result = returned.Clone();
                }
            }
            else if (result is not null)
            {
                result = InkValue.GetObjectValue(result);
            }

            keys.SetDirty(result, paramStart - 1, paramEnd);
        }

        /// <summary> UniInk SyntaxList : Process the Lambda in SyntaxList </summary>
        protected static void ProcessList_Lambda(InkSyntaxList keys, int paramStart, int paramEnd)
        {
            while (true)
            {
                var (hasArrow, arrowIndex) = FindOperator(keys, InkOperator.Lambda, paramStart, paramEnd);

                if (!hasArrow) break;

                ProcessList_LambdaExpression(keys, arrowIndex, paramEnd);
            }
        }

        /// <summary> UniInk SyntaxList : Process one lambda expression </summary>
        /// <remarks> The closure lives here , so it is only allocated when an arrow was really found </remarks>
        protected static void ProcessList_LambdaExpression(InkSyntaxList keys, int arrowIndex, int paramEnd)
        {
            var (hasBalance, endIndex) = FindNoBalanceOperator(keys, InkOperator.ParenthesisLeft, InkOperator.ParenthesisRight, arrowIndex, paramEnd);

            if (!hasBalance)
            {
                throw new InkSyntaxException("the parenthesis is not balanced", keys.PositionAt(arrowIndex), InkEvalContext.Current.Source);
            }

            //var c => GET(c, Rarity) == 2)
            var startIndex = arrowIndex - 1;

            var lambdaData = InkSyntaxList.GetTemp();

            for (var i = arrowIndex + 1 ; i <= endIndex - 1 ; i++)
            {
                object current;

                if (keys.IndexDirty[i])
                {
                    if (keys.CastOther[i] == null) continue;

                    current = keys.CastOther[i];

                    keys.CastOther[i] = null;
                }
                else
                {
                    if (keys[i] is InkValue { ValueType: not (TypeCode.Object or TypeCode.DBNull) } inkValue)
                    {
                        current = inkValue.Clone();
                    }
                    else
                    {
                        current = keys[i];
                    }
                }

                lambdaData.Add(current);
            }

            var variable = keys[startIndex] as InkValue;

            var lambda = new Predicate<object>(o =>
            {
                // this section has GC 🙂
                InkOperator.InkOperator_Assign(variable, o);
                var result = (bool)(InkValue)ProcessList(lambdaData, 0, lambdaData.Count - 1);
                InkSyntaxList.Recover(lambdaData);

                return result;
            });

            var inkObject = InkValue.GetObjectValue(lambda);

            keys.SetDirty(inkObject, startIndex, endIndex - 1);
        }

        /// <summary> Release the temp variables of the running evaluation frame </summary>
        protected void ReleaseTempVariables()
        {
            var variables = InkEvalContext.Current.TempVariables;

            foreach (var variable in variables)
            {
                variable.Value.DontRelease = false;
                InkValue.Release(variable.Value);
            }

            variables.Clear();
        }

        /*************************************************  Mapping  **************************************************/

        /// <summary> Some UnaryPostfix operators func mapping </summary>
        protected static readonly Dictionary<InkOperator, Func<object, object, object>> DicOperatorsFunc = new(OPERATOR_FUNC_CAPACITY)
        {
            { InkOperator.Plus, InkOperator.InkOperator_Plus }                     //
          , { InkOperator.Minus, InkOperator.InkOperator_Minus }                   //
          , { InkOperator.Multiply, InkOperator.InkOperator_Multiply }             //
          , { InkOperator.Divide, InkOperator.InkOperator_Divide }                 //
          , { InkOperator.Modulo, InkOperator.InkOperator_Modulo }                 //
          , { InkOperator.Lower, InkOperator.InkOperator_Lower }                   //
          , { InkOperator.Greater, InkOperator.InkOperator_Greater }               //
          , { InkOperator.Equal, InkOperator.InkOperator_Equal }                   //
          , { InkOperator.LowerOrEqual, InkOperator.InkOperator_LowerOrEqual }     //
          , { InkOperator.GreaterOrEqual, InkOperator.InkOperator_GreaterOrEqual } //
          , { InkOperator.NotEqual, InkOperator.InkOperator_NotEqual }             //
          , { InkOperator.LogicalNot, InkOperator.InkOperator_LogicalNOT }         //
          , { InkOperator.ConditionalAnd, InkOperator.InkOperator_ConditionalAnd } //
          , { InkOperator.ConditionalOr, InkOperator.InkOperator_ConditionalOr }   //
          , { InkOperator.Assign, InkOperator.InkOperator_Assign }                 //
          , { InkOperator.KeyReturn, InkOperator.InkOperator_Return }              //
        };

        /// <summary> Some Escaped Char mapping </summary>
        protected static char GetEscapedChar(char c)
        {
            return c switch
            {
                '\\' => '\\' //
              , '\'' => '\'' //
              , '0'  => '\0' //
              , 'a'  => '\a' //
              , 'b'  => '\b' //
              , 'f'  => '\f' //
              , 'n'  => '\n' //
              , 'r'  => '\r' //
              , 't'  => '\t' //
              , 'v'  => '\v' //
              , _    => throw new InkSyntaxException($"Unknown escape character[{c}]")
            };
        }

        /// <summary> Some local functions mapping </summary>
        protected readonly Dictionary<int, InkFunction> DicFunctions = new(FUNCTION_CAPACITY);

        /// <summary> Some local variables mapping </summary>
        protected readonly Dictionary<int, InkValue> DicVariables = new(VARIABLE_CAPACITY);

        /**************************************************  Lexer  ***************************************************/

        /// <summary> The delegate of Parsing Methods for <see cref="ParsingMethods"/> </summary>
        /// <param name="expression"> the expression to Evaluate </param>
        /// <param name="stack"> the object stack to push or pop </param>
        /// <param name="i"> the <paramref name="expression"/> start index </param>
        protected delegate bool ParsingMethodDelegate(string expression, InkSyntaxList stack, ref int i);

        /// <summary> The Parsing Methods for <see cref="CompileLexerAndFill"/> </summary>
        protected readonly List<ParsingMethodDelegate> ParsingMethods;

        /// <summary> Evaluate Operators in<see cref="InkOperator"/> </summary>
        /// <param name="expression"> the expression to Evaluate </param>
        /// <param name="keys"> the object stack to push or pop </param>
        /// <param name="i"> the <paramref name="expression"/> start index </param>
        /// <returns> the evaluate is success or not </returns>
        protected static bool EvaluateOperators(string expression, InkSyntaxList keys, ref int i)
        {
            for (var operatorLen = InkOperator.MaxOperatorLen - 1 ; operatorLen >= 0 ; operatorLen--)
            {
                if (i + operatorLen >= expression.Length) continue; // long=>short, || first than |, so we need to check the length

                var operatorHash = GetStringSliceHashCode(expression, i, i + operatorLen);
                if (InkOperator.DicValues.TryGetValue(operatorHash, out var @operator))
                {
                    keys.Add(@operator);
                    i += operatorLen;
                    return true;
                }
            }

            return false;
        }

        /// <summary> Evaluate Number _eg: -3.64f </summary>
        /// <param name="expression"> the expression to Evaluate </param>
        /// <param name="keys"> the object stack to push or pop </param>
        /// <param name="i"> the <paramref name="expression"/> start index  </param>
        /// <returns> the evaluate is success or not </returns>
        protected static bool EvaluateNumber(string expression, InkSyntaxList keys, ref int i)
        {
            if (StartsWithNumbersFromIndex(expression, i, out var numberMatch, out var len))
            {
                numberMatch.Calculate();
                keys.Add(numberMatch);
                i += len - 1;
                return true;
            }

            return false;
        }

        /// <summary> Evaluate Bool _eg: true false </summary>
        /// <param name="expression"> the expression to Evaluate </param>
        /// <param name="keys"> the object stack to push or pop </param>
        /// <param name="i"> the <paramref name="expression"/> start index  </param>
        /// <returns> the evaluate is success or not </returns>
        protected static bool EvaluateBool(string expression, InkSyntaxList keys, ref int i)
        {
            if (StartsWithBoolFromIndex(expression, i, out var boolMatch, out var len))
            {
                keys.Add(boolMatch);
                i += len - 1;
                return true;
            }

            return false;
        }

        /// <summary> Evaluate Char or Escaped Char  _eg: 'a' '\d'          </summary>
        /// <param name="expression"> the expression to Evaluate            </param>
        /// <param name="keys"> the object stack to push or pop </param>
        /// <param name="i">the <paramref name="expression"/> start index   </param>
        /// <returns> the evaluate is success or not                        </returns>
        protected static bool EvaluateChar(string expression, InkSyntaxList keys, ref int i)
        {
            if (StartsWithCharFormIndex(expression, i, out var value, out var len))
            {
                keys.Add(value);
                i += len - 1; // len contains the closing quote , the parsing loop adds the last step
                return true;
            }

            return false;
        }

        /// <summary> Evaluate String _eg:"string" </summary>
        /// <param name="expression"> the expression to Evaluate </param>
        /// <param name="keys"> the object stack to push or pop </param>
        /// <param name="i"> the <paramref name="expression"/> start index </param>
        /// <returns> the evaluate is success or not </returns>
        protected static bool EvaluateString(string expression, InkSyntaxList keys, ref int i)
        {
            if (StartsWithStringFormIndex(expression, i, out var value, out var len))
            {
                keys.Add(value);
                i += len;
                return true;
            }

            return false;
        }

        /// <summary> Evaluate Function _eg: LOG("Hello World")             </summary>
        /// <param name="expression"> the expression to Evaluate            </param>
        /// <param name="keys"> the object stack to push or pop             </param>
        /// <param name="i"> the <paramref name="expression"/> start index  </param>
        /// <returns> the evaluate is success or not                        </returns>
        protected bool EvaluateFunction(string expression, InkSyntaxList keys, ref int i)
        {
            for (var len = FUNC_NAME_MAX_LEN ; len >= 0 ; len--)
            {
                if (i + len >= expression.Length) continue; // long=>short, || first than |, so we need to check the length

                var varHash = GetStringSliceHashCode(expression, i, i + len);
                if (DicFunctions.TryGetValue(varHash, out var variable))
                {
                    keys.Add(variable);
                    i += len;
                    return true;
                }
            }

            return false;
        }

        /// <summary> Evaluate Variable _eg: var a = 3; </summary>
        /// <param name="expression"> the expression to Evaluate </param>
        /// <param name="keys"> the object stack to push or pop </param>
        /// <param name="i"> the <paramref name="expression"/> start index </param>
        /// <returns> the evaluate is success or not </returns>
        protected bool EvaluateVariable(string expression, InkSyntaxList keys, ref int i)
        {
            var tempVariables = InkEvalContext.Current.TempVariables;

            if (keys.Count > 0 && keys[keys.Count - 1] is InkOperator operatorValue && Equals(operatorValue, InkOperator.KeyVar))
            {
                var startIndex = i;

                while (i < expression.Length && expression[i] != ' ' && expression[i] != '=')
                {
                    i++;
                }

                i--;

                var keyHash = GetStringSliceHashCode(expression, startIndex, i);
                var name    = expression.Substring(startIndex, i - startIndex + 1);

                if (DicVariables.TryGetValue(keyHash, out _))
                {
                    throw new InkSyntaxException($"the variable [{name}] is already registered", startIndex, expression);
                }

                if (tempVariables.ContainsKey(keyHash))
                {
                    throw new InkSyntaxException($"the variable [{name}] is already declared in this expression", startIndex, expression);
                }

                var inkValue = InkValue.Get();
                inkValue.ValueType   = TypeCode.DBNull;
                inkValue.DontRelease = true;
                tempVariables.Add(keyHash, inkValue);

                keys.SetDirty(keys.Count - 1); // the keyVar

                keys.Add(tempVariables[keyHash]);

                return true;
            }

            for (var len = VARI_NAME_MAX_LEN ; len >= 0 ; len--)
            {
                if (i + len >= expression.Length) continue; // long=>short, || first than |, so we need to check the length

                var varHash = GetStringSliceHashCode(expression, i, i + len);
                if (tempVariables.TryGetValue(varHash, out var variable))
                {
                    keys.Add(variable);
                    i += len;
                    return true;
                }

                if (DicVariables.TryGetValue(varHash, out var variable2))
                {
                    keys.Add(variable2);
                    if (variable2.IsGetter)
                    {
                        variable2.Getter(variable2);
                    }

                    i += len;
                    return true;
                }
            }

            return false;
        }

        // ---- lexer character helpers ----

        /// <summary> Match <paramref name="value"/> from <paramref name="input"/> 's <paramref name="startIndex"/> </summary>
        protected static bool StartsWithInputStrFromIndex(string input, string value, int startIndex, bool ignoreFirstCase = false)
        {
            if (input.Length - startIndex < value.Length)
            {
                return false;
            }

            if (ignoreFirstCase)
            {
                if (char.ToLower(input[startIndex]) == char.ToLower(value[0]))
                {
                    return false;
                }
            }
            else
            {
                if (input[startIndex] != value[0])
                {
                    return false;
                }
            }

            for (var i = 1 ; i < value.Length ; i++)
            {
                if (input[i + startIndex] != value[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary> Find <paramref name="input"/> is whether start with numbers from <paramref name="startIndex"/> </summary>
        protected static bool StartsWithNumbersFromIndex(string input, int startIndex, out InkValue value, out int len)
        {
            if (!char.IsDigit(input[startIndex]))
            {
                value = null;
                len   = 0;
                return false;
            }

            value           = InkValue.Get();
            value.ValueType = TypeCode.Int32;
            len             = 0;

            var pointNum = 0;

            for (var i = startIndex ; i < input.Length ; i++)
            {
                if (char.IsDigit(input[i]))
                {
                    len++;
                    value.ValueMeta.Add(input[i]);
                }
                else if (input[i] == '.')
                {
                    len++;
                    pointNum++;

                    if (pointNum > 1)
                    {
                        throw new InkSyntaxException(
                                                     "[NotSupport]:Too many decimal points, can't calling method with float or double number.");
                    }

                    value.ValueType = TypeCode.Double;
                    value.ValueMeta.Add(input[i]);
                }
                else
                {
                    if (len == 0)
                    {
                        InkValue.Release(value);
                        return false;
                    }

                    switch (input[i])
                    {
                        case 'f' or 'F' :
                            value.ValueType = TypeCode.Single;
                            len++;
                            break;
                        case 'd' or 'D' :
                            value.ValueType = TypeCode.Double;
                            len++;
                            break;
                    }

                    return true;
                }
            }

            return true;
        }

        /// <summary> Find <paramref name="input"/> is whether start with char from <paramref name="startIndex"/> </summary>
        protected static bool StartsWithCharFormIndex(string input, int startIndex, out InkValue value, out int len)
        {
            var i = startIndex;
            if (input[i].Equals('\''))
            {
                i++;
                if (input[i].Equals('\\'))
                {
                    i++;

                    value = InkValue.GetCharValue(GetEscapedChar(input[i]));
                    i++;
                }
                else if (input[i].Equals('\''))
                {
                    throw new InkSyntaxException($"Illegal character[{i}] : ['']");
                }
                else
                {
                    value = InkValue.GetCharValue(input[i]);
                    i++;
                }

                if (input[i].Equals('\''))
                {
                    len = i - startIndex + 1;
                    return true;
                }

                throw new InkSyntaxException($"Illegal character[{i}] : too many characters in a character literal");
            }

            len   = 0;
            value = null;
            return false;
        }

        /// <summary> Find <paramref name="input"/> is whether start with string from <paramref name="startIndex"/> </summary>
        protected static bool StartsWithStringFormIndex(string input, int startIndex, out InkValue value, out int len)
        {
            var i = startIndex;

            if (input[i].Equals('\"'))
            {
                i++;
                value           = InkValue.Get();
                value.ValueType = TypeCode.String;

                while (i < input.Length)
                {
                    if (input[i].Equals('\\'))
                    {
                        i++;

                        value.ValueMeta.Add(GetEscapedChar(input[i]));
                        i++;
                    }
                    else if (input[i].Equals('\"'))
                    {
                        len = i - startIndex;

                        return true;
                    }
                    else
                    {
                        value.ValueMeta.Add(input[i]);
                        i++;
                    }
                }

                throw new InkSyntaxException($"Illegal character[{i}] : too many characters in a character literal");
            }

            len   = 0;
            value = null;
            return false;
        }

        /// <summary> Find <paramref name="input"/> is whether start with bool from <paramref name="startIndex"/> </summary>
        protected static bool StartsWithBoolFromIndex(string input, int startIndex, out InkValue value, out int len)
        {
            const string TRUE_STR01  = "true";
            const string TRUE_STR02  = "True";
            const string FALSE_STR01 = "false";
            const string FALSE_STR02 = "False";

            if (StartsWithInputStrFromIndex(input, FALSE_STR01, startIndex))
            {
                len   = FALSE_STR01.Length;
                value = InkValue.GetBoolValue(false);
                return true;
            }

            if (StartsWithInputStrFromIndex(input, FALSE_STR02, startIndex))
            {
                len   = FALSE_STR02.Length;
                value = InkValue.GetBoolValue(false);
                return true;
            }

            if (StartsWithInputStrFromIndex(input, TRUE_STR01, startIndex))
            {
                len   = TRUE_STR01.Length;
                value = InkValue.GetBoolValue(true);
                return true;
            }

            if (StartsWithInputStrFromIndex(input, TRUE_STR02, startIndex))
            {
                len   = TRUE_STR02.Length;
                value = InkValue.GetBoolValue(true);
                return true;
            }

            len   = 0;
            value = null;
            return false;
        }

        /**************************************************  Helper  **************************************************/

        /// <summary> Find the innermost section between <paramref name="sctStart"/> and <paramref name="sctEnd"/> </summary>
        /// <param name="keys"> the keys to find section in </param>
        /// <param name="sctStart"> the start section key : the last find before <paramref name="sctEnd"/> </param>
        /// <param name="sctEnd"> the end section key : the first find after <paramref name="sctStart"/> </param>
        /// <param name="start"> the start index to search from </param>
        /// <param name="end"> the end index to search to </param>
        /// <returns> the result is success or not , the start index and end index of the section </returns>
        protected static (bool result, int startIndex, int endIndex) FindSection(InkSyntaxList keys, InkOperator sctStart, InkOperator sctEnd, int start, int end)
        {
            var startIndex = -1;
            var endIndex   = -1;

            for (var i = start ; i <= end ; i++)
            {
                if (keys.IndexDirty[i])
                {
                    continue;
                }

                if (Equals(keys[i], sctStart))
                {
                    startIndex = i;
                }
                else if (Equals(keys[i], sctEnd))
                {
                    endIndex = i;
                    break;
                }
            }

            if (startIndex > endIndex)
            {
                throw new InkSyntaxException($"missing the matching [{sctEnd.Name}]", keys.PositionAt(start), InkEvalContext.Current.Source);
            }

            return (startIndex != -1 && endIndex != -1, startIndex, endIndex);
        }

        /// <summary> Find the outermost operator range in the specified left and right </summary>
        protected static (int StartIndex, int EndIndex) GetMatchOperator(InkSyntaxList keys, InkOperator opLeft, InkOperator opRight, int start, int end)
        {
            int startIndex = -1, balance = -1;

            for (var i = start ; i <= end ; i++)
            {
                if (keys[i] is InkOperator op && Equals(op, opLeft))
                {
                    startIndex = i;
                    balance    = 1;
                    break;
                }
            }

            for (var i = startIndex + 1 ; i <= end ; i++)
            {
                if (keys[i] is InkOperator opL && Equals(opL, opLeft))
                {
                    balance++;
                }
                else if (keys[i] is InkOperator opR && Equals(opR, opRight))
                {
                    balance--;

                    if (balance == 0)
                    {
                        return (startIndex, i);
                    }
                }
            }

            return (-1, -1);
        }

        /// <summary> Find the specified <paramref name="operator"/> in the <paramref name="keys"/> </summary>
        /// <param name="keys"> the keys to find the operator in </param>
        /// <param name="operator"> the operator to find </param>
        /// <param name="start"> the start index to search from </param>
        /// <param name="end"> the end index to search to </param>
        /// <returns> the result is success or not , the index of the operator </returns>
        protected static (bool result, int index) FindOperator(InkSyntaxList keys, InkOperator @operator, int start, int end)
        {
            for (var i = start ; i <= end ; i++)
            {
                if (keys.IndexDirty[i])
                {
                    continue;
                }

                if (Equals(keys[i], @operator))
                {
                    return (true, i);
                }
            }

            return (false, end);
        }

        /// <summary> Find the specified left and right operator in the <paramref name="keys"/> without balance </summary>
        /// <param name="keys"> the keys to find the operator in </param>
        /// <param name="operatorLeft"> the left operator to find </param>
        /// <param name="operatorRight"> the right operator to find </param>
        /// <param name="start"> the start index to search from </param>
        /// <param name="end"> the end index to search to </param>
        /// <returns> the result is success or not , the index of the right operator </returns>
        protected static (bool result, int index) FindNoBalanceOperator(InkSyntaxList keys, InkOperator operatorLeft, InkOperator operatorRight, int start, int end)
        {
            var balance = 0;

            for (var i = start ; i <= end ; i++)
            {
                var current = keys[i];

                if (current is InkOperator operatorValue)
                {
                    if (Equals(operatorValue, operatorLeft))
                    {
                        balance++;
                    }
                    else if (Equals(operatorValue, operatorRight))
                    {
                        balance--;
                    }
                }

                if (balance == -1)
                {
                    return (true, i);
                }
            }

            return (false, -1);
        }

        /// <summary>Get the highest priority operator in the <paramref name="keys"/> </summary>
        /// <param name="keys"> the keys to find the highest priority operator in </param>
        /// <param name="startIndex"> the start index to search from </param>
        /// <param name="endIndex"> the end index to search to </param>
        /// <returns> the highest priority operator and its index </returns>
        protected static (InkOperator @operator, int index) GetHighestPriorityOperator(InkSyntaxList keys, int startIndex, int endIndex)
        {
            var index            = -1;
            var priorityOperator = InkOperator.Empty;

            for (var i = startIndex ; i <= endIndex ; i++)
            {
                if (keys.IndexDirty[i])
                {
                    continue;
                }

                if (keys[i] is InkOperator @operator
                 && @operator.PriorityIndex < priorityOperator.PriorityIndex)
                {
                    index            = i;
                    priorityOperator = @operator;
                }
            }

            return (priorityOperator, index);
        }

        /// <summary> Judge the input String is a Script or not (depend on the operator:[;] [{]) </summary>
        /// <param name="keys"> the keys to judge </param>
        /// <returns> the result is script or not </returns>
        protected static bool InputIsScript(InkSyntaxList keys)
        {
            foreach (var obj in keys.ObjectList)
            {
                //match [;]
                if (obj is InkOperator @operator && Equals(@operator, InkOperator.Semicolon))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary> Remove null object from the <paramref name="objectList"/> </summary>
        /// <param name="objectList"> the object list to remove null </param>
        protected static void ObjectRemoveNull(List<object> objectList)
        {
            for (var i = 0 ; i < objectList.Count ; i++)
            {
                if (objectList[i] == null)
                {
                    objectList.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary> Whether <paramref name="value"/> is one of the <paramref name="objects"/> ( identity , no Equals ) </summary>
        /// <param name="value"> the value to look for </param>
        /// <param name="objects"> the objects that may own the value </param>
        protected static bool IsOwnedBy(InkValue value, List<object> objects)
        {
            foreach (var other in objects)
            {
                if (ReferenceEquals(other, value))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary> Get the hash code of the string from <paramref name="startIndex"/> to <paramref name="endIndex"/> </summary>
        /// <param name="str"> the string to get hash code </param>
        /// <param name="startIndex"> the start index </param>
        /// <param name="endIndex"> the end index </param>
        /// <returns> the hash code of the string slice </returns>
        public static int GetStringSliceHashCode(string str, int startIndex, int endIndex)
        {
            var hash = 0;
            for (var i = startIndex ; i <= endIndex ; i++)
            {
                hash = hash * 31 + str[i];
            }

            return hash;
        }

        /// <summary> Get the hash code of the whole string </summary>
        /// <param name="str"> the string to get hash code </param>
        /// <returns> the hash code of the string </returns>
        public static int GetStringSliceHashCode(string str) => GetStringSliceHashCode(str, 0, str.Length - 1);
    }

    /*************************************************  Helper Class  *************************************************/

    /// <summary> UniInk Operator : Custom your own Operator! </summary>
    public partial class InkOperator
    {
        /// <summary> All Operators Dictionary </summary>
        public static readonly Dictionary<int, InkOperator> DicValues = new(UniInk.INK_OPERATOR_CAPACITY);

        //priority refer to : https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/operators/
        //keyword  refer to : https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/keywords/
        /// <summary> Left Parenthesis  "(" </summary>
        public static readonly InkOperator ParenthesisLeft = new("(", 1);

        /// <summary> Right Parenthesis ")" </summary>
        public static readonly InkOperator ParenthesisRight = new(")", 1);

        /// <summary> Dot Operator "." (don't support , it's reserved) </summary>
        public static readonly InkOperator Dot = new(".", 2);

        /// <summary> Array Bracket Start  "[" (don't support , it's reserved) </summary>
        public static readonly InkOperator BracketStart = new("[", 2);

        /// <summary> Array Bracket End    "]" (don't support , it's reserved) </summary>
        public static readonly InkOperator BracketEnd = new("]", 2);

        /// <summary> Unary Plus  "++" (don't support , it's reserved) </summary>
        public static readonly InkOperator Increment = new("++", 2);

        /// <summary> Unary Minus "--" (don't support , it's reserved) </summary>
        public static readonly InkOperator Decrement = new("--", 2);

        /// <summary> Logical NOT "!" </summary>
        public static readonly InkOperator LogicalNot = new("!", 3);

        /// <summary> Bitwise NOT "~" (don't support , it's reserved) </summary>
        public static readonly InkOperator BitNot = new("~", 3);

        /// <summary> Unary Plus  "+" (don't support , it's reserved) </summary>
        public static readonly InkOperator Cast = new("😊()", 4);

        /// <summary> Multiply "*" </summary>
        public static readonly InkOperator Multiply = new("*", 5);

        /// <summary> Divide "/" </summary>
        public static readonly InkOperator Divide = new("/", 5);

        /// <summary> Modulo "%" </summary>
        public static readonly InkOperator Modulo = new("%", 5);

        /// <summary> Plus "+" </summary>
        public static readonly InkOperator Plus = new("+", 6);

        /// <summary> Minus "-" </summary>
        public static readonly InkOperator Minus = new("-", 6);

        /// <summary> Left Shift "&lt;&lt;" (don't support , it's reserved) </summary>
        public static readonly InkOperator LeftShift = new("<<", 7);

        /// <summary> Right Shift "&gt;&gt;" (don't support , it's reserved) </summary>
        public static readonly InkOperator RightShift = new(">>", 7);

        /// <summary> Lower "&lt;" </summary>
        public static readonly InkOperator Lower = new("<", 8);

        /// <summary> Greater "&gt;" </summary>
        public static readonly InkOperator Greater = new(">", 8);

        /// <summary> Lower Or Equal "&lt;=" </summary>
        public static readonly InkOperator LowerOrEqual = new("<=", 8);

        /// <summary> Greater Or Equal "&gt;=" </summary>
        public static readonly InkOperator GreaterOrEqual = new(">=", 8);

        /// <summary> Equal "==" </summary>
        public static readonly InkOperator Equal = new("==", 9);

        /// <summary> Not Equal "!=" </summary>
        public static readonly InkOperator NotEqual = new("!=", 9);

        /// <summary> Bitwise AND "&amp;" (don't support , it's reserved) </summary>
        public static readonly InkOperator BitwiseAnd = new("&", 10); // don't support , its reserved

        /// <summary> Bitwise XOR "^" (don't support , it's reserved) </summary>
        public static readonly InkOperator BitwiseXor = new("^", 11); // don't support , its reserved

        /// <summary> Bitwise OR "|" (don't support , it's reserved) </summary>
        public static readonly InkOperator BitwiseOr = new("|", 12); // don't support , its reserved

        /// <summary> Conditional AND "&amp;&amp;" </summary>
        public static readonly InkOperator ConditionalAnd = new("&&", 13);

        /// <summary> Conditional OR "||" </summary>
        public static readonly InkOperator ConditionalOr = new("||", 14);

        /// <summary> Conditional Ternary Operator "?:" (don't support , it's reserved) </summary>
        public static readonly InkOperator Conditional = new("?:", 15); // don't support , its reserved

        /// <summary> Assignment Operator "=" </summary>
        public static readonly InkOperator Assign = new("=", 16);

        /// <summary> Comma Operator "," </summary>
        public static readonly InkOperator Comma = new(",", 16);

        /// <summary> Lambda Expression Operator "=&gt;" </summary>
        public static readonly InkOperator Lambda = new("=>", 17);

        /// <summary> Left Brace "{" </summary>
        public static readonly InkOperator BraceLeft = new("{", 20);

        /// <summary> Right Brace "}" </summary>
        public static readonly InkOperator BraceRight = new("}", 20);

        /// <summary> Semicolon ";" </summary>
        public static readonly InkOperator Semicolon = new(";", 20);

        /// <summary> Colon ":" (don't support , it's reserved) </summary>
        public static readonly InkOperator Colon = new(":", -1);

        /// <summary> Question Mark "?" (don't support , it's reserved) </summary>
        public static readonly InkOperator QuestionMark = new("?", -1);

        /// <summary> Verbatim String Prefix "@\"" (don't support , it's reserved) </summary>
        public static readonly InkOperator At = new("@\"", -1);

        /// <summary> Interpolated String Prefix "$\"" (don't support , it's reserved) </summary>
        public static readonly InkOperator Dollar = new("$\"", -1);

        /// <summary> Preprocessor Directive "#" (don't support , it's reserved) </summary>
        public static readonly InkOperator Hash = new("#", -1);

        /// <summary> Keyword "if" - Conditional Statement </summary>
        public static readonly InkOperator KeyIf = new("if", 20);

        /// <summary> Keyword "var" - Variable Declaration </summary>
        public static readonly InkOperator KeyVar = new("var", 20);

        /// <summary> Keyword "else" - Alternative Conditional Branch </summary>
        public static readonly InkOperator KeyElse = new("else", 20);

        /// <summary> Keyword "return" - Return Statement </summary>
        public static readonly InkOperator KeyReturn = new("return", 20);

        /// <summary> Keyword "switch" - Switch Statement (don't support , it's reserved) </summary>
        public static readonly InkOperator KeySwitch = new("switch", 20);

        /// <summary> Keyword "while" - While Loop (don't support , it's reserved) </summary>
        public static readonly InkOperator KeyWhile = new("while", 20);

        /// <summary> Keyword "for" - For Loop (don't support , it's reserved) </summary>
        public static readonly InkOperator KeyFor = new("for", 20);

        /// <summary> Keyword "foreach" - Foreach Loop (don't support , it's reserved) </summary>
        public static readonly InkOperator KeyForeach = new("foreach", 20);

        /// <summary> Keyword "in" - Collection Iterator (don't support , it's reserved) </summary>
        public static readonly InkOperator KeyIn = new("in", 20);

        /// <summary> Keyword "break" - Break Statement (don't support , it's reserved) </summary>
        public static readonly InkOperator KeyBreak = new("break", 20);

        /// <summary> Keyword "continue" - Continue Statement (don't support , it's reserved) </summary>
        public static readonly InkOperator KeyContinue = new("continue", 20);

        /// <summary> Empty Placeholder Operator with Maximum Priority </summary>
        public static readonly InkOperator Empty = new("😊", short.MaxValue);

        /// <summary> the max length of all operators , filled by the constructors below </summary>
        internal static short MaxOperatorLen;

        /// <summary> Constructor of <see cref="InkOperator"/> </summary>
        /// <param name="name"> the name of the operator </param>
        /// <param name="priorityIndex"> the priority index of the operator </param>
        protected InkOperator(string name, short priorityIndex)
        {
            PriorityIndex  = priorityIndex;
            MaxOperatorLen = Math.Max(MaxOperatorLen, (short)name.Length);
            Name           = name;
            var hash = UniInk.GetStringSliceHashCode(name, 0, name.Length - 1);
            DicValues.Add(hash, this);
        }

        /// <summary> the lower the value, the higher the priority </summary>
        public readonly short PriorityIndex;

        /// <summary> the name of the operator      </summary>
        public readonly string Name;

        /// <summary> the indexer of the operator    </summary>
        protected static short Indexer;

        /// <summary> the only value of the operator </summary>
        protected readonly short OperatorValue = Indexer++;

        /// <summary> Override Equals Method </summary>
        /// <param name="otherOperator"> the other operator to compare </param>
        /// <returns> the result is equal or not </returns>
        public override bool Equals(object otherOperator) => otherOperator is InkOperator @operator && OperatorValue == @operator.OperatorValue;

        /// <summary> Override GetHashCode Method </summary>
        /// <returns> the hash code of the operator </returns>
        public override int GetHashCode() => OperatorValue;

        /// <summary> Override ToString Method </summary>
        public override string ToString() => $"Operator : {Name}  Priority : {PriorityIndex}";

        /// <summary> Plus Operator Implementation </summary>
        public static object InkOperator_Plus(object left, object right) => InkValue.Apply(Plus, left, right);

        /// <summary> Minus Operator Implementation </summary>
        public static object InkOperator_Minus(object left, object right) => InkValue.Apply(Minus, left, right);

        /// <summary> Multiplication operator implementation </summary>
        public static object InkOperator_Multiply(object left, object right) => InkValue.Apply(Multiply, left, right);

        /// <summary> Division operator implementation </summary>
        public static object InkOperator_Divide(object left, object right) => InkValue.Apply(Divide, left, right);

        /// <summary> Modulo operator implementation </summary>
        public static object InkOperator_Modulo(object left, object right) => InkValue.Apply(Modulo, left, right);

        /// <summary> Less than operator implementation </summary>
        public static object InkOperator_Lower(object left, object right) => InkValue.Apply(Lower, left, right);

        /// <summary> Greater than operator implementation </summary>
        public static object InkOperator_Greater(object left, object right) => InkValue.Apply(Greater, left, right);

        /// <summary> Equality operator implementation </summary>
        public static object InkOperator_Equal(object left, object right) => InkValue.Apply(Equal, left, right);

        /// <summary> Less than or equal operator implementation </summary>
        public static object InkOperator_LowerOrEqual(object left, object right) => InkValue.Apply(LowerOrEqual, left, right);

        /// <summary> Greater than or equal operator implementation </summary>
        public static object InkOperator_GreaterOrEqual(object left, object right) => InkValue.Apply(GreaterOrEqual, left, right);

        /// <summary> Not equal operator implementation </summary>
        public static object InkOperator_NotEqual(object left, object right) => InkValue.Apply(NotEqual, left, right);

        /// <summary> Logical NOT operator implementation </summary>
        public static object InkOperator_LogicalNOT(object left, object right) => InkValue.Apply(LogicalNot, left, right);

        /// <summary> Conditional AND operator implementation </summary>
        public static object InkOperator_ConditionalAnd(object left, object right) => InkValue.Apply(ConditionalAnd, left, right);

        /// <summary> Conditional OR operator implementation </summary>
        public static object InkOperator_ConditionalOr(object left, object right) => InkValue.Apply(ConditionalOr, left, right);

        /// <summary> Assignment operator implementation </summary>
        public static object InkOperator_Assign(object left, object right)
        {
            if (left is InkValue leftValue)
            {
                switch (right)
                {
                    case InkValue rightValue :
                    {
                        leftValue.ValueType = rightValue.ValueType;
                        rightValue.Calculate();
                        rightValue.CopyTo(leftValue);

                        return null;
                    }
                    case not null :
                    {
                        leftValue.ValueType   = TypeCode.Object;
                        leftValue.ValueObject = right;
                        leftValue.IsCalculate = true;

                        return null;
                    }

                    default : throw new InkSyntaxException("the right side of the assignment is not supported");
                }
            }

            throw new InkSyntaxException("the left side of the assignment must be a variable");
        }

        /// <summary> Return statement implementation </summary>
        public static object InkOperator_Return(object left, object right)
        {
            switch (right)
            {
                case InkValue rightValue :
                {
                    rightValue.Calculate();
                    var result = rightValue.Clone();
                    result.IsReturner = true;
                    return result;
                }
                default : throw new InkSyntaxException("the return statement needs a value");
            }
        }
    }

    /// <summary> UniInk Function : Custom your own Function! </summary>
    public partial class InkFunction
    {
        /// <summary> Constructor for creating a new function with a delegate </summary>
        public InkFunction(Func<List<object>, object> func)
        {
            FuncDelegate = func;
        }

        /// <summary> The function delegate that will be invoked </summary>
        public readonly Func<List<object>, object> FuncDelegate;
    }

    /// <summary> UniInk evaluation context : the pools of the calling thread and the state of the running evaluation </summary>
    /// <remarks>
    /// One context exists per thread ( created lazily ) , so several UniInk instances can be evaluated on different
    /// threads . A context keeps a stack of frames , so a registered function may call Evaluate again ( reentrancy )
    /// without releasing the temp objects of the outer call .
    /// An evaluation never mutates the UniInk instance itself , so one instance can also be shared by several threads ,
    /// as long as no function / variable is registered while an evaluation is running .
    /// </remarks>
    public sealed class InkEvalContext
    {
        [ThreadStatic]
        private static InkEvalContext current;

        /// <summary> The context of the calling thread </summary>
        public static InkEvalContext Current => current ??= new InkEvalContext();

        /// <summary> The InkValue instances available for reuse on this thread </summary>
        public readonly Queue<InkValue> ValuePool = new(UniInk.INK_VALUE_POOL_CAPACITY);

        /// <summary> The InkSyntaxList instances available for reuse on this thread </summary>
        public readonly Queue<InkSyntaxList> SyntaxPool = new(UniInk.INK_SYNTAX_POOL_CAPACITY);

        /// <summary> The temp syntax lists created by the running evaluation ( the lambda cache ) </summary>
        public readonly List<InkSyntaxList> TempLists = new(UniInk.INK_SYNTAX_POOL_CAPACITY);

        private sealed class Frame
        {
            public readonly Dictionary<int, InkValue> TempVariables = new(UniInk.VARIABLE_TEMP_CAPACITY);
            public          int                       TempListMark;
            public          string                    Source;
        }

        private readonly List<Frame>  frames     = new(4) { new Frame() }; // [0] is the root frame
        private readonly Stack<Frame> freeFrames = new(4);

        /// <summary> The temp variables of the running evaluation frame </summary>
        public Dictionary<int, InkValue> TempVariables => frames[frames.Count - 1].TempVariables;

        /// <summary> The expression of the running evaluation frame , used by the error messages </summary>
        public string Source => frames[frames.Count - 1].Source;

        /// <summary> The number of temp syntax lists owned by the outer frames </summary>
        internal int TempListMark => frames[frames.Count - 1].TempListMark;

        /// <summary> Begin an evaluation frame </summary>
        /// <param name="source"> the expression that is being evaluated , used by the error messages </param>
        public void PushFrame(string source = null)
        {
            var frame = freeFrames.Count > 0 ? freeFrames.Pop() : new Frame();

            frame.TempListMark = TempLists.Count;
            frame.Source       = source;

            frames.Add(frame);
        }

        /// <summary> End the running evaluation frame and return its temp objects to the pools </summary>
        public void PopFrame()
        {
            if (frames.Count <= 1) return; // the root frame serves the manual API ( CompileLexerAndFill / ExecuteProcess )

            var frame = frames[frames.Count - 1];

            frames.RemoveAt(frames.Count - 1);

            // the temp lists first : they may reference a temp variable , which must be released exactly once
            for (var i = TempLists.Count - 1 ; i >= frame.TempListMark ; i--)
            {
                InkSyntaxList.ReleaseAll(TempLists[i]);

                TempLists.RemoveAt(i);
            }

            foreach (var variable in frame.TempVariables)
            {
                variable.Value.DontRelease = false;
                InkValue.Release(variable.Value);
            }

            frame.TempVariables.Clear();

            frame.Source = null;

            freeFrames.Push(frame);
        }
    }

    /// <summary> In UniInk , every valueType is Object , No Boxing! </summary>
    public partial class InkValue
    {
        /// <summary> The object pool of the calling thread , see <see cref="InkEvalContext"/> </summary>
        private static Queue<InkValue> Pool => InkEvalContext.Current.ValuePool;

        /// <summary> Gets an InkValue instance from the pool or creates a new one </summary>
        /// <returns>An InkValue instance ready for use</returns>
        public static InkValue Get()
        {
#if DEBUG
            GetTime++;
#endif

            var value = Pool.Count > 0 ? Pool.Dequeue() : new();

#if DEBUG
            value.isReleased = false;
#endif

            return value;
        }

        /// <summary> Clears the InkValue pool, releasing all cached instances </summary>
        public static void ReleasePool() => Pool.Clear();

#if DEBUG
        /// <summary> DEBUG only : how many times Get() has been called </summary>
        public static int GetTime;
        /// <summary> DEBUG only : how many times Release() has been called </summary>
        public static int ReleaseTime;
#endif

        /// <summary> Creates an InkValue containing a char value </summary>
        /// <param name="c">The char value to store</param>
        /// <returns>An InkValue instance containing the char</returns>
        public static InkValue GetCharValue(char c)
        {
            var value = Get();
            value.ValueType   = TypeCode.Char;
            value.ValueChar   = c;
            value.IsCalculate = true;
            return value;
        }

        /// <summary> Creates an InkValue containing a boolean value </summary>
        /// <param name="b">The boolean value to store</param>
        /// <returns>An InkValue instance containing the boolean</returns>
        public static InkValue GetBoolValue(bool b)
        {
            var value = Get();
            value.ValueType   = TypeCode.Boolean;
            value.ValueBool   = b;
            value.IsCalculate = true;
            return value;
        }

        /// <summary> Creates an InkValue containing an integer value </summary>
        /// <param name="i">The integer value to store</param>
        /// <returns>An InkValue instance containing the integer</returns>
        public static InkValue GetIntValue(int i)
        {
            var value = Get();
            value.ValueType   = TypeCode.Int32;
            value.ValueInt    = i;
            value.IsCalculate = true;
            return value;
        }

        /// <summary> Creates an InkValue containing a float value </summary>
        /// <param name="f">The float value to store</param>
        /// <returns>An InkValue instance containing the float</returns>
        public static InkValue GetFloatValue(float f)
        {
            var value = Get();
            value.ValueType   = TypeCode.Single;
            value.ValueFloat  = f;
            value.IsCalculate = true;
            return value;
        }

        /// <summary> Creates an InkValue containing a double value </summary>
        /// <param name="d">The double value to store</param>
        /// <returns>An InkValue instance containing the double</returns>
        public static InkValue GetDoubleValue(double d)
        {
            var value = Get();
            value.ValueType   = TypeCode.Double;
            value.ValueDouble = d;
            value.IsCalculate = true;
            return value;
        }

        /// <summary> Creates an InkValue containing a string value </summary>
        /// <param name="str">The string to store</param>
        /// <returns>An InkValue instance containing the string</returns>
        public static InkValue GetString(string str)
        {
            var value = Get();
            value.ValueType = TypeCode.String;

            foreach (var c in str)
            {
                value.ValueMeta.Add(c);
            }

            value.IsCalculate = true;
            return value;
        }

        /// <summary> Creates an InkValue containing an object reference </summary>
        /// <param name="obj">The object to store</param>
        /// <returns>An InkValue instance containing the object</returns>
        public static InkValue GetObjectValue(object obj)
        {
            var value = Get();
            value.ValueType   = TypeCode.Object;
            value.ValueObject = obj;
            value.IsCalculate = true;
            return value;
        }

        /// <summary> Sets a getter action for the InkValue </summary>
        /// <param name="value">The InkValue to modify</param>
        /// <param name="getter">The getter action to set</param>
        /// <returns>The modified InkValue</returns>
        public static InkValue SetGetter(InkValue value, Action<InkValue> getter)
        {
            value.IsGetter = true;
            value.Getter   = getter;
            return value;
        }

        /// <summary> Releases an InkValue back to the pool for reuse </summary>
        /// <remarks>
        /// Releasing the same instance twice is a bug : it throws in DEBUG builds , because the second call would
        /// put the same instance into the pool twice and it would then be handed out to two different computations .
        /// RELEASE builds pay nothing for the check , so a misuse there stays silent .
        /// Releasing <c>null</c> is a no-op , so cleanup code does not need its own null check .
        /// </remarks>
        /// <param name="value">The InkValue to release</param>
        public static void Release(InkValue value)
        {
            if (value is null) return;

            if (value.DontRelease) return;

#if DEBUG
            if (value.isReleased)
            {
                throw new InvalidOperationException(
                    $"InkValue.Release : the value has already been released ( last type : {value.ValueType} ) ! " +
                    "Enqueuing it twice would hand the same instance to two different computations .");
            }

            value.isReleased = true;

            ReleaseTime++;
#endif

            value.ValueMeta.Clear();
            value.IsCalculate = false;
            value.IsIsSetter  = false;
            value.IsGetter    = false;
            value.IsReturner  = false;
            value.ValueInt    = 0;
            value.ValueBool   = false;
            value.ValueChar   = '\0';
            value.ValueFloat  = 0;
            value.ValueDouble = 0;
            value.ValueObject = null;

            Pool.Enqueue(value);
        }

        /// <summary> Integer value storage </summary>
        public int ValueInt;

        /// <summary> Boolean value storage </summary>
        public bool ValueBool;

        /// <summary> Char value storage </summary>
        public char ValueChar;

        /// <summary> Float value storage </summary>
        public float ValueFloat;

        /// <summary> Double value storage </summary>
        public double ValueDouble;

        /// <summary> Object reference storage </summary>
        public object ValueObject;

        /// <summary> String value computed from Value_Meta </summary>
        public string ValueString => string.Concat(ValueMeta);

        /// <summary> The type code of the stored value </summary>
        public TypeCode ValueType;

        /// <summary> Metadata storage for string and complex values </summary>
        internal readonly List<char> ValueMeta = new(UniInk.STRING_MAX_LEN);

        /// <summary> Indicates if the value has been calculated </summary>
        public bool IsCalculate { get; internal set; }

        /// <summary> Prevents the value from being released to the pool </summary>
        public bool DontRelease { get; internal set; }

#if DEBUG
        /// <summary> DEBUG only : the value is owned by the pool , releasing it again is a bug </summary>
        private bool isReleased;
#endif

        /// <summary> Indicates if this value is a setter </summary>
        public bool IsIsSetter { get; internal set; }

        /// <summary> Indicates if this value is a getter </summary>
        public bool IsGetter { get; internal set; }

        /// <summary> Indicates if this value is a return value </summary>
        public bool IsReturner { get; internal set; }

        /// <summary> Setter action for this value </summary>
        public Action<InkValue> Setter;

        /// <summary> Getter action for this value </summary>
        public Action<InkValue> Getter;

        /// <summary> Calculate the value </summary>
        public void Calculate()
        {
            if (IsCalculate) return;

            switch (ValueType)
            {
                case TypeCode.Int32 :
                {
                    ValueInt = 0;

                    foreach (var t in ValueMeta)
                    {
                        ValueInt = ValueInt * 10 + (t - '0');
                    }

                    break;
                }
                case TypeCode.Single :
                {
                    ValueFloat = 0;
                    var floatPart = 0.0f;
                    var numCount  = 0;

                    foreach (var c in ValueMeta) //123
                    {
                        if (c == '.') break;

                        numCount++;
                        ValueFloat = ValueFloat * 10 + (c - '0');
                    }

                    for (var i = ValueMeta.Count - 1 ; i >= numCount ; i--)
                    {
                        if (ValueMeta[i] == '.') break;

                        floatPart = floatPart / 10 + (ValueMeta[i] - '0') * 0.1f;
                    }

                    ValueFloat += floatPart;

                    break;
                }
                case TypeCode.Double :
                {
                    ValueDouble = 0;
                    var floatPart = 0.0d;
                    var numCount  = 0;

                    foreach (var c in ValueMeta)
                    {
                        if (c == '.') break;

                        ValueDouble = ValueDouble * 10 + (c - '0');
                        numCount++;
                    }

                    for (var i = ValueMeta.Count - 1 ; i >= numCount ; i--)
                    {
                        if (ValueMeta[i] == '.') break;

                        floatPart = floatPart / 10 + (ValueMeta[i] - '0') * 0.1d;
                    }

                    ValueDouble += floatPart;

                    break;
                }
                case TypeCode.Object : break;
                case TypeCode.String : break;
                default :              throw new InkSyntaxException("Unknown ValueType");
            }

            IsCalculate = true;
        }

        /// <summary> Copy the value to another InkValue instance </summary>
        /// <param name="value">The target InkValue to copy to</param>
        public void CopyTo(InkValue value)
        {
            value.ValueType   = ValueType;
            value.IsCalculate = IsCalculate;

            value.ValueInt    = ValueInt;
            value.ValueBool   = ValueBool;
            value.ValueChar   = ValueChar;
            value.ValueFloat  = ValueFloat;
            value.ValueDouble = ValueDouble;
            value.ValueObject = ValueObject;
            value.ValueMeta.Clear();

            foreach (var meta in ValueMeta)
            {
                value.ValueMeta.Add(meta);
            }
        }

        /// <summary> Clone the current InkValue instance </summary>
        /// <returns>A new InkValue instance that is a copy of the current instance</returns>
        public InkValue Clone()
        {
            var value = Get();
            CopyTo(value);
            return value;
        }

        /// <summary> Override ToString Method to provide a string representation of the InkValue </summary>
        /// <returns>A string representation of the InkValue</returns>
        public override string ToString()
        {
            if (!IsCalculate) return ValueString;

            switch (ValueType)
            {
                case TypeCode.Int32 :   return ValueInt.ToString();
                case TypeCode.Boolean : return ValueBool.ToString();
                case TypeCode.Char :    return ValueChar.ToString();
                case TypeCode.Single :  return ValueFloat.ToString();
                case TypeCode.Double :  return ValueDouble.ToString();
                case TypeCode.String :  return ValueString;
                case TypeCode.Object :  return ValueObject.ToString();
                default :               throw new InkSyntaxException("Unknown ValueType");
            }
        }

        /// <summary> Override GetHashCode Method to provide the hash code of the ValueMeta instance </summary>
        /// <remarks> The hash code is based on identity , not on the value ; use the == operator to compare values </remarks>
        /// <returns>A hash code representing this InkValue instance </returns>
        public override int GetHashCode() => ValueMeta.GetHashCode();

        /// <summary> Override Equals Method : an InkValue is only equal to itself </summary>
        /// <param name="obj">The other object to compare</param>
        /// <returns>True only when both references point at the same InkValue instance , otherwise false</returns>
        public override bool Equals(object obj) => obj is InkValue value && ValueMeta == value.ValueMeta;

        /// <summary> Get the result as an int and auto release the value </summary>
        public int GetResult_Int() => Take(TypeCode.Int32, static value => value.ValueInt);

        /// <summary> Get the result as a bool and auto release the value </summary>
        public bool GetResult_Bool() => Take(TypeCode.Boolean, static value => value.ValueBool);

        /// <summary> Get the result as a char and auto release the value </summary>
        public char GetResult_Char() => Take(TypeCode.Char, static value => value.ValueChar);

        /// <summary> Get the result as a float and auto release the value </summary>
        public float GetResult_Float() => Take(TypeCode.Single, static value => value.ValueFloat);

        /// <summary> Get the result as a double and auto release the value </summary>
        public double GetResult_Double() => Take(TypeCode.Double, static value => value.ValueDouble);

        /// <summary> Get the result as a string and auto release the value </summary>
        public string GetResult_String() => Take(TypeCode.String, static value => value.ValueString);

        /// <summary> Get the result as an object and auto release the value </summary>
        public object GetResult_Object() => Take(TypeCode.Object, static value => value.ValueObject);

        /// <summary> Read the result with the expected type , then release the value </summary>
        private T Take<T>(TypeCode expected, Func<InkValue, T> read)
        {
            if (!IsCalculate) Calculate();

            if (ValueType != expected)
            {
                throw new InkSyntaxException($"the value type is not [{expected}] but [{ValueType}]");
            }

            var result = read(this);

            Release(this);

            return result;
        }

        /************************************************  Promotion  *************************************************/

        /// <summary> Numeric rank used by the implicit promotion , -1 when the type is not numeric </summary>
        private static int NumericRank(TypeCode type) => type switch
        {
            TypeCode.Char   => 0,
            TypeCode.Int32  => 1,
            TypeCode.Single => 2,
            TypeCode.Double => 3,
            _               => -1
        };

        /// <summary> Get the common type of two numeric operands , false when no promotion applies </summary>
        /// <remarks> C# style : int -> single -> double , and char promotes to int </remarks>
        private static bool TryGetPromotedType(InkValue left, InkValue right, out TypeCode promoted)
        {
            var leftRank  = NumericRank(left.ValueType);
            var rightRank = NumericRank(right.ValueType);

            if (leftRank < 0 || rightRank < 0)
            {
                promoted = TypeCode.Empty;

                return false;
            }

            promoted = leftRank >= rightRank ? left.ValueType : right.ValueType;

            if (promoted == TypeCode.Char)
            {
                promoted = TypeCode.Int32; // char combined with char is an int in C#
            }

            return promoted != left.ValueType || promoted != right.ValueType;
        }

        /// <summary> Read the value as an int </summary>
        private static int ToInt(InkValue value) => value.ValueType switch
        {
            TypeCode.Char   => value.ValueChar,
            TypeCode.Int32  => value.ValueInt,
            TypeCode.Single => (int)value.ValueFloat,
            TypeCode.Double => (int)value.ValueDouble,
            _               => 0
        };

        /// <summary> Read the value as a single </summary>
        private static float ToSingle(InkValue value) => value.ValueType switch
        {
            TypeCode.Char   => value.ValueChar,
            TypeCode.Int32  => value.ValueInt,
            TypeCode.Single => value.ValueFloat,
            TypeCode.Double => (float)value.ValueDouble,
            _               => 0
        };

        /// <summary> Read the value as a double </summary>
        private static double ToDouble(InkValue value) => value.ValueType switch
        {
            TypeCode.Char   => value.ValueChar,
            TypeCode.Int32  => value.ValueInt,
            TypeCode.Single => value.ValueFloat,
            TypeCode.Double => value.ValueDouble,
            _               => 0
        };

        /***********************************************  Dispatch  ***************************************************/

        /// <summary> A short operand description for the error messages </summary>
        private static string Describe(object operand) => operand switch
        {
            null           => "null",
            InkValue value => value.ValueType.ToString(),
            InkOperator op => $"operator {op.Name}",
            _              => operand.GetType().Name
        };

        /// <summary> Apply a built-in operator to the raw operands of the syntax list </summary>
        /// <remarks> left is null for the unary form ( +x , -x , !x ) </remarks>
        internal static object Apply(InkOperator op, object left, object right)
        {
            if (left is null) return ApplyUnary(op, right);

            if (left is InkValue leftValue && right is InkValue rightValue) return Binary(op, leftValue, rightValue);

            throw new InkSyntaxException($"operator [{op.Name}] is not defined for [{Describe(left)}] and [{Describe(right)}]");
        }

        /// <summary> Apply the unary form of a built-in operator </summary>
        private static object ApplyUnary(InkOperator op, object right)
        {
            if (right is not InkValue rightValue) throw new InkSyntaxException($"operator [{op.Name}] is not defined for [{Describe(right)}]");

            switch (op.Name)
            {
                case "+" :
                {
                    rightValue.Calculate();

                    var copy = Get();

                    rightValue.CopyTo(copy);

                    return copy;
                }
                case "-" :
                {
                    var zero   = GetIntValue(0);
                    var result = Binary(InkOperator.Minus, zero, rightValue);

                    Release(zero);

                    return result;
                }
                case "!" : return !rightValue;

                // a binary-only operator has no unary meaning : returning the operand here would hand back an
                // instance that the syntax list still owns , so releasing the list would release it twice
                // ( once through ObjectList , once through CastOther ) and the pool would hand the same
                // instance to two different computations
                default : throw new InkSyntaxException($"operator [{op.Name}] is missing its left operand");
            }
        }

        /// <summary> Apply a binary operator to two values </summary>
        private static InkValue Binary(InkOperator op, InkValue left, InkValue right)
        {
            switch (op.Name)
            {
                case "+" :
                case "-" :
                case "*" :
                case "/" :
                case "%" :  return Arithmetic(op, left, right);
                case "==" : return Equality(left, right);
                case "!=" : return Equality(left, right).Negate();
                case "<" :  return Relational(InkOperator.Lower, left, right);
                case ">" :  return Relational(InkOperator.Greater, left, right);
                case "<=" : return Relational(InkOperator.Greater, left, right).Negate();
                case ">=" : return Relational(InkOperator.Lower, left, right).Negate();
                case "&&" :
                case "||" : return Logical(op, left, right);
                case "!" :  return !right;
                default :   throw new InkSyntaxException($"unsupported operator : {op.Name}");
            }
        }

        /// <summary> Arithmetic operators ( + - * / % ) , with the numeric promotion and the string add </summary>
        private static InkValue Arithmetic(InkOperator op, InkValue left, InkValue right)
        {
            left.Calculate();
            right.Calculate();

            var answer = Get();

            answer.ValueType = left.ValueType;

            switch (left.ValueType, right.ValueType)
            {
                case (TypeCode.String, TypeCode.String) when op.Name == "+" :
                    foreach (var c in left.ValueMeta) answer.ValueMeta.Add(c);
                    foreach (var c in right.ValueMeta) answer.ValueMeta.Add(c);
                    break;
                case (TypeCode.Int32, TypeCode.Int32) :   answer.ValueInt    = IntMath(op, left.ValueInt, right.ValueInt); break;
                case (TypeCode.Single, TypeCode.Single) : answer.ValueFloat  = SingleMath(op, left.ValueFloat, right.ValueFloat); break;
                case (TypeCode.Double, TypeCode.Double) : answer.ValueDouble = DoubleMath(op, left.ValueDouble, right.ValueDouble); break;
                default :
                {
                    if (!TryGetPromotedType(left, right, out var promoted))
                    {
                        throw new InkSyntaxException($"operator [{op.Name}] is not defined for [{left.ValueType}] and [{right.ValueType}]");
                    }

                    answer.ValueType = promoted;

                    switch (promoted)
                    {
                        case TypeCode.Int32 :  answer.ValueInt    = IntMath(op, ToInt(left), ToInt(right)); break;
                        case TypeCode.Single : answer.ValueFloat  = SingleMath(op, ToSingle(left), ToSingle(right)); break;
                        case TypeCode.Double : answer.ValueDouble = DoubleMath(op, ToDouble(left), ToDouble(right)); break;
                    }

                    break;
                }
            }

            answer.IsCalculate = true;

            return answer;
        }

        /// <summary> Equality operators ( == != ) </summary>
        private static InkValue Equality(InkValue left, InkValue right)
        {
            left.Calculate();
            right.Calculate();

            var answer = GetBoolValue(false);

            switch (left.ValueType, right.ValueType)
            {
                case (TypeCode.Int32, TypeCode.Int32) :     answer.ValueBool = left.ValueInt == right.ValueInt; break;
                case (TypeCode.Boolean, TypeCode.Boolean) : answer.ValueBool = left.ValueBool == right.ValueBool; break;
                case (TypeCode.Char, TypeCode.Char) :       answer.ValueBool = left.ValueChar == right.ValueChar; break;
                case (TypeCode.Single, TypeCode.Single) :   answer.ValueBool = Math.Abs(left.ValueFloat - right.ValueFloat) < UniInk.EPSILON_FLOAT; break;
                case (TypeCode.Double, TypeCode.Double) :   answer.ValueBool = Math.Abs(left.ValueDouble - right.ValueDouble) < UniInk.EPSILON_DOUBLE; break;
                case (TypeCode.String, TypeCode.String) :   answer.ValueBool = ValueMetaEquals(left, right); break;
                default :
                {
                    if (TryGetPromotedType(left, right, out var promoted))
                    {
                        answer.ValueBool = promoted switch
                        {
                            TypeCode.Int32  => ToInt(left) == ToInt(right),
                            TypeCode.Single => Math.Abs(ToSingle(left) - ToSingle(right)) < UniInk.EPSILON_FLOAT,
                            TypeCode.Double => Math.Abs(ToDouble(left) - ToDouble(right)) < UniInk.EPSILON_DOUBLE,
                            _               => false
                        };
                    }

                    break;
                }
            }

            return answer;
        }

        /// <summary> Relational operators ( &lt; &gt; ) , &lt;= and &gt;= negate the opposite comparison </summary>
        private static InkValue Relational(InkOperator op, InkValue left, InkValue right)
        {
            left.Calculate();
            right.Calculate();

            var answer = GetBoolValue(false);

            switch (left.ValueType, right.ValueType)
            {
                case (TypeCode.Int32, TypeCode.Int32) :   answer.ValueBool = IntCompare(op, left.ValueInt, right.ValueInt); break;
                case (TypeCode.Single, TypeCode.Single) : answer.ValueBool = SingleCompare(op, left.ValueFloat, right.ValueFloat); break;
                case (TypeCode.Double, TypeCode.Double) : answer.ValueBool = DoubleCompare(op, left.ValueDouble, right.ValueDouble); break;
                case (TypeCode.Char, TypeCode.Char) :     answer.ValueBool = IntCompare(op, left.ValueChar, right.ValueChar); break;
                default :
                {
                    if (!TryGetPromotedType(left, right, out _))
                    {
                        throw new InkSyntaxException($"operator [{op.Name}] cannot compare [{left.ValueType}] and [{right.ValueType}]");
                    }

                    answer.ValueBool = op.Name == ">" ? ToDouble(left) > ToDouble(right) : ToDouble(left) < ToDouble(right);

                    break;
                }
            }

            return answer;
        }

        /// <summary> Conditional operators ( &amp;&amp; || ) </summary>
        private static InkValue Logical(InkOperator op, InkValue left, InkValue right)
        {
            if (left.ValueType == TypeCode.Boolean && right.ValueType == TypeCode.Boolean)
            {
                return GetBoolValue(op.Name == "&&" ? left.ValueBool && right.ValueBool : left.ValueBool || right.ValueBool);
            }

            throw new InkSyntaxException($"operator [{op.Name}] needs two bool operands, got [{left.ValueType}] and [{right.ValueType}]");
        }

        private static int IntMath(InkOperator op, int left, int right) => op.Name[0] switch
        {
            '+' => left + right,
            '-' => left - right,
            '*' => left * right,
            '/' => left / right,
            _   => left % right
        };

        private static float SingleMath(InkOperator op, float left, float right) => op.Name[0] switch
        {
            '+' => left + right,
            '-' => left - right,
            '*' => left * right,
            '/' => left / right,
            _   => left % right
        };

        private static double DoubleMath(InkOperator op, double left, double right) => op.Name[0] switch
        {
            '+' => left + right,
            '-' => left - right,
            '*' => left * right,
            '/' => left / right,
            _   => left % right
        };

        private static bool IntCompare(InkOperator op, int left, int right)         => op.Name[0] == '>' ? left > right : left < right;
        private static bool SingleCompare(InkOperator op, float left, float right) => op.Name[0] == '>' ? left > right : left < right;
        private static bool DoubleCompare(InkOperator op, double left, double right) => op.Name[0] == '>' ? left > right : left < right;

        /// <summary> Compare two string values held by <see cref="ValueMeta"/> without allocating </summary>
        private static bool ValueMetaEquals(InkValue left, InkValue right)
        {
            if (left.ValueMeta.Count != right.ValueMeta.Count) return false;

            for (var i = 0 ; i < left.ValueMeta.Count ; i++)
            {
                if (left.ValueMeta[i] != right.ValueMeta[i]) return false;
            }

            return true;
        }

        /***********************************************  Operators  **************************************************/

        /// <summary> Addition operator overload for InkValue </summary>
        public static InkValue operator +(InkValue left, InkValue right) => Binary(InkOperator.Plus, left, right);

        /// <summary> Subtraction operator overload for InkValue </summary>
        public static InkValue operator -(InkValue left, InkValue right) => Binary(InkOperator.Minus, left, right);

        /// <summary> Multiplication operator overload for InkValue </summary>
        public static InkValue operator *(InkValue left, InkValue right) => Binary(InkOperator.Multiply, left, right);

        /// <summary> Division operator overload for InkValue </summary>
        public static InkValue operator /(InkValue left, InkValue right) => Binary(InkOperator.Divide, left, right);

        /// <summary> Modulo operator overload for InkValue </summary>
        public static InkValue operator %(InkValue left, InkValue right) => Binary(InkOperator.Modulo, left, right);

        /// <summary> Greater than operator overload for InkValue </summary>
        public static InkValue operator >(InkValue left, InkValue right) => Binary(InkOperator.Greater, left, right);

        /// <summary> Less than operator overload for InkValue </summary>
        public static InkValue operator <(InkValue left, InkValue right) => Binary(InkOperator.Lower, left, right);

        /// <summary> Equality operator overload for InkValue , null safe </summary>
        public static InkValue operator ==(InkValue left, InkValue right)
        {
            if (left is null || right is null)
            {
                return GetBoolValue(left is null && right is null);
            }

            return Binary(InkOperator.Equal, left, right);
        }

        /// <summary> Inequality operator overload for InkValue </summary>
        public static InkValue operator !=(InkValue left, InkValue right) => (left == right).Negate();

        /// <summary> Greater than or equal operator overload for InkValue </summary>
        public static InkValue operator >=(InkValue left, InkValue right) => Binary(InkOperator.GreaterOrEqual, left, right);

        /// <summary> Less than or equal operator overload for InkValue </summary>
        public static InkValue operator <=(InkValue left, InkValue right) => Binary(InkOperator.LowerOrEqual, left, right);

        /// <summary> Logical NOT operator overload for InkValue </summary>
        public static InkValue operator !(InkValue left) => left.Clone().Negate();

        /// <summary> Implicit conversion from InkValue to int </summary>
        /// <param name="st">The InkValue to convert</param>
        /// <returns>The integer value</returns>
        public static implicit operator int(InkValue st) => st.ValueInt;

        /// <summary> Implicit conversion from InkValue to float </summary>
        /// <param name="st">The InkValue to convert</param>
        /// <returns>The float value</returns>
        public static implicit operator float(InkValue st) => st.ValueFloat;

        /// <summary> Implicit conversion from InkValue to double </summary>
        /// <param name="st">The InkValue to convert</param>
        /// <returns>The double value</returns>
        public static implicit operator double(InkValue st) => st.ValueDouble;

        /// <summary> Implicit conversion from InkValue to bool </summary>
        /// <param name="st">The InkValue to convert</param>
        /// <returns>The boolean value</returns>
        public static implicit operator bool(InkValue st) => st.ValueBool;

        /// <summary> Implicit conversion from InkValue to char </summary>
        /// <param name="st">The InkValue to convert</param>
        /// <returns>The character value</returns>
        public static implicit operator char(InkValue st) => st.ValueChar;

        /// <summary> Implicit conversion from InkValue to string </summary>
        /// <param name="st">The InkValue to convert</param>
        /// <returns>The string value</returns>
        public static implicit operator string(InkValue st) => st.ValueString;

        /// <summary> Negates the boolean value of this InkValue </summary>
        /// <returns>This InkValue with negated boolean value</returns>
        protected InkValue Negate()
        {
            if (ValueType == TypeCode.Boolean)
            {
                ValueBool = !ValueBool;
            }

            return this;
        }
    }

    /// <summary> InkSyntaxList is a list of object, it can be used to store the syntax object </summary>
    public partial class InkSyntaxList
    {
        /// <summary> Object pool for InkSyntaxList instances of the calling thread , see <see cref="InkEvalContext"/> </summary>
        public static Queue<InkSyntaxList> Pool => InkEvalContext.Current.SyntaxPool;

        /// <summary> Cache for the temp syntax lists of the running evaluation </summary>
        public static List<InkSyntaxList> LambdaCache => InkEvalContext.Current.TempLists;

        /// <summary> Gets an InkSyntaxList instance from the pool or creates a new one </summary>
        /// <returns>An InkSyntaxList instance ready for use</returns>
        public static InkSyntaxList Get() => Pool.Count > 0 ? Pool.Dequeue() : new();

        /// <summary> Gets a temporary InkSyntaxList and adds it to the lambda cache </summary>
        /// <returns>A temporary InkSyntaxList instance</returns>
        public static InkSyntaxList GetTemp()
        {
            var get = Get();
            LambdaCache.Add(get);
            return get;
        }

        /// <summary> Clears the InkSyntaxList pool, releasing all cached instances </summary>
        public static void ReleasePool() => Pool.Clear();

        /// <summary> Releases an InkSyntaxList and all its contained InkValues back to the pool </summary>
        /// <param name="value">The InkSyntaxList to release</param>
        public static void ReleaseAll(InkSyntaxList value)
        {
            foreach (var obj in value.ObjectList)
            {
                if (obj is InkValue inkValue)
                {
                    InkValue.Release(inkValue);
                }
            }

            foreach (var obj in value.CastOther)
            {
                if (obj is InkValue inkValue)
                {
                    InkValue.Release(inkValue);
                }
            }

            value.ObjectList.Clear();
            value.CastOther.Clear();
            value.IndexDirty.Clear();
            value.PositionList.Clear();

            Pool.Enqueue(value);
        }

        /// <summary> Releases the temporary InkSyntaxList instances created by the running evaluation frame </summary>
        public static void ReleaseTemp()
        {
            var context = InkEvalContext.Current;

            for (var i = context.TempLists.Count - 1 ; i >= context.TempListMark ; i--)
            {
                ReleaseAll(context.TempLists[i]);

                context.TempLists.RemoveAt(i);
            }
        }

        /// <summary> Recover the InkSyntaxList without releasing the objects </summary>
        /// <param name="value">The InkSyntaxList to recover</param>
        public static void Recover(InkSyntaxList value)
        {
            for (var index = 0 ; index < value.CastOther.Count ; index++)
            {
                var obj = value.CastOther[index];
                if (obj is InkValue inkValue)
                {
                    InkValue.Release(inkValue);
                }

                value.CastOther[index] = null;
            }

            for (var index = 0 ; index < value.IndexDirty.Count ; index++)
            {
                value.IndexDirty[index] = false;
            }
        }

        /// <summary> The list of objects </summary>
        public readonly List<object> ObjectList = new(UniInk.EXPRESS_ELEMENT_MAX_LEN);

        /// <summary> The cast other objects </summary>
        public readonly List<object> CastOther = new(UniInk.EXPRESS_ELEMENT_MAX_LEN);

        /// <summary> The index dirty flags </summary>
        public readonly List<bool> IndexDirty = new(UniInk.EXPRESS_ELEMENT_MAX_LEN);

        /// <summary> The character index in the source expression of every token , filled by the lexer </summary>
        public readonly List<int> PositionList = new();

        /// <summary> The character index in the source expression of the token at <paramref name="index"/> , -1 when unknown </summary>
        public int PositionAt(int index) => (uint)index < (uint)PositionList.Count ? PositionList[index] : -1;

        /// <summary> Add an object to the list </summary>
        public void Add(object value)
        {
            ObjectList.Add(value);
            CastOther.Add(null);
            IndexDirty.Add(false);
        }

        /// <summary> Get the count of the list </summary>
        public int Count => ObjectList.Count;

        /// <summary> Get the object at index </summary>
        /// <param name="index"> the index </param>
        public object this[int index] => ObjectList[index];

        /// <summary> Set the index dirty from start to end </summary>
        /// <param name="other"> the cast object </param>
        /// <param name="start"> the start index </param>
        /// <param name="end"> the end index </param>
        public void SetDirty(object other, int start, int end)
        {
            for (var i = start ; i <= end ; i++)
            {
                IndexDirty[i] = true;
            }

            CastOther[start] = other;
        }

        /// <summary> Set the index dirty </summary>
        public void SetDirty(int index) => IndexDirty[index] = true;
    }

    /// <summary> InkSyntaxException throw when the syntax is wrong  </summary>
    public class InkSyntaxException : Exception
    {
        private readonly string rawMessage;

        /// <summary> The zero based character index in the expression , -1 when unknown </summary>
        public int Position { get; } = -1;

        /// <summary> The expression that failed , null when unknown </summary>
        public string Expression { get; }

        /// <summary> Constructor </summary>
        /// <param name="message"> the exception message </param>
        public InkSyntaxException(string message) : base(message)
        {
            rawMessage = message;
        }

        /// <summary> Constructor with the location of the failure </summary>
        /// <param name="message"> the reason </param>
        /// <param name="position"> the zero based character index , -1 when unknown </param>
        /// <param name="expression"> the expression that failed </param>
        public InkSyntaxException(string message, int position, string expression) : base(Format(message, position, expression))
        {
            rawMessage = message;
            Position   = position;
            Expression = expression;
        }

        /// <summary> Return this exception , or a copy with the location filled in </summary>
        public InkSyntaxException WithLocation(int position, string expression)
        {
            if (Expression is not null) return this;

            return new InkSyntaxException(rawMessage, position, expression);
        }

        /// <summary> The message plus the expression and a caret under the failing character </summary>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Expression)) return base.ToString();

            if (Position < 0) return $"{base.ToString()}{Environment.NewLine}{Expression}";

            var index     = Math.Min(Position, Expression.Length - 1);
            var lineStart = Expression.LastIndexOf('\n', index) + 1;
            var lineEnd   = Expression.IndexOf('\n', lineStart);
            var line      = lineEnd < 0 ? Expression.Substring(lineStart) : Expression.Substring(lineStart, lineEnd - lineStart);

            return $"{base.ToString()}{Environment.NewLine}{line}{Environment.NewLine}{new string(' ', index - lineStart)}^";
        }

        private static string Format(string message, int position, string expression)
        {
            if (string.IsNullOrEmpty(expression)) return message;

            return position < 0 ? $"{message} in \"{expression}\"" : $"{message} at {position} in \"{expression}\"";
        }
    }

}
//3000 lines of code [MAX]