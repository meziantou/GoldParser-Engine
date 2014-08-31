using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using GoldParserEngine;

namespace Samples.BooleanExpression
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] expressions =
            {
                "true",
                "true || false",
                "[true] || false || true",
                "true || false && true",
                "true && false && true",
                "true || !false",
                "true || || false",
                "A && (!B || [C ]) ^ D /*|| (true && false)*/",
            };

            BooleanLanguage language = new BooleanLanguage();
            foreach (var expression in expressions)
            {
                using (TextReader textReader = new StringReader(expression))
                {
                    if (!language.Parse(textReader))
                    {
                        Console.WriteLine(expression);
                        Console.WriteLine(language.ErrorString);
                        Console.WriteLine(new string('-', 40));
                        continue;
                    }

                    if (language.Expression != null)
                    {
                        Console.WriteLine(expression);
                        Console.WriteLine(language.Expression.DisplayName);
                        Console.WriteLine(new string('-', 40));
                    }
                }

            }
        }
    }

    public class BooleanLanguage
    {
        private readonly Parser _parser = new Parser();
        public Expression Expression { get; private set; }

        private string _errorString;

        public int LineNumber
        {
            get
            {
                return _parser.CurrentPosition.Line;
            }
        }

        public int LinePosition
        {
            get
            {
                return _parser.CurrentPosition.Column;
            }
        }

        public string ErrorString
        {
            get
            {
                return _errorString;
            }
        }

        public void LoadGrammar(BinaryReader reader)
        {
            _parser.LoadTables(reader);
        }

        public bool Parse(TextReader reader)
        {
            if (!_parser.AreTablesLoaded)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string resourceName = assembly.GetName().Name + ".Resources." + this.GetType().Name + ".egt";
                using (Stream s = assembly.GetManifestResourceStream(resourceName))
                {
                    if (s == null)
                    {
                        throw new Exception("Grammar not found.");
                    }

                    using (BinaryReader binaryReader = new BinaryReader(s))
                    {
                        LoadGrammar(binaryReader);
                    }
                }
            }

            _parser.Open(reader);
            _parser.TrimReductions = true;

            while (true)
            {
                ParseMessage response = _parser.Parse();

                switch (response)
                {
                    case ParseMessage.LexicalError:
                        _errorString = string.Format("Lexical Error. Line {0}, Column {1}. Token {2} was not expected.", LineNumber, LinePosition, _parser.CurrentToken.Data);
                        return false;

                    case ParseMessage.SyntaxError:
                        StringBuilder text = new StringBuilder();
                        foreach (Symbol tokenSymbol in _parser.ExpectedSymbols)
                        {
                            text.Append(' ');
                            text.Append(tokenSymbol);
                        }
                        _errorString = string.Format("Syntax Error. Line {0}, Column {1}. Expecting: {2}.", LineNumber, LinePosition, text);
                        return false;

                    case ParseMessage.Reduction:
                        _parser.CurrentReduction = CreateNewObject(_parser.CurrentReduction as Reduction);
                        break;

                    case ParseMessage.Accept:
                        //Accepted!
                        Expression = _parser.CurrentReduction as Expression;
                        return true;

                    case ParseMessage.TokenRead:
                        Trace.WriteLine("Token: " + _parser.CurrentToken.Parent.Name);
                        break;

                    case ParseMessage.InternalError:
                        //INTERNAL ERROR! Something is horribly wrong.
                        return false;

                    case ParseMessage.NotLoadedError:
                        //This error occurs if the CGT was not loaded.                   
                        return false;

                    case ParseMessage.GroupError:
                        //GROUP ERROR! Unexpected end of file
                        return false;
                }
            }
        }

        private object CreateNewObject(Reduction r)
        {
            string ruleName = r.Parent.Head.Name;
            Trace.WriteLine("Reduce: " + ruleName);

            if (ruleName == "orExpression")
            {
                var left = r.GetData(0) as Expression;
                var right = r.GetData(2) as Expression;
                return new OrExpression(left, right);
            }
            else if (ruleName == "andExpression")
            {
                var left = r.GetData(0) as Expression;
                var right = r.GetData(2) as Expression;
                return new AndExpression(left, right);
            }
            else if (ruleName == "xorExpression")
            {
                var left = r.GetData(0) as Expression;
                var right = r.GetData(2) as Expression;
                return new XorExpression(left, right);
            }
            else if (ruleName == "notExpression")
            {
                var left = r.GetData(1) as Expression;
                return new NotExpression(left);
            }
            else if (ruleName == "parentheseExpression")
            {
                return r.GetData(1) as Expression;
            }
            else if (ruleName == "value")
            {
                var value = r.GetData(0) as string;
                if (value != null)
                {
                    value = value.Trim();
                    bool boolean;
                    if (bool.TryParse(value, out boolean))
                    {
                        return new BooleanValueExpression(boolean);
                    }

                    return new RoleNameExpression(value);
                }
            }

            return null;
        }
    };

    [DebuggerDisplay("{DisplayName}")]
    public abstract class Expression
    {
        public abstract bool Eval(IPrincipal principal);

        public abstract void GetDisplayName(TextWriter writer);

        public virtual string DisplayName
        {
            get
            {
                using (StringWriter writer = new StringWriter())
                {
                    GetDisplayName(writer);
                    return writer.ToString();
                }
            }
        }
    }

    public abstract class UnaryExpression : Expression
    {
        private readonly Expression _expression;

        public Expression Expression
        {
            get { return _expression; }
        }

        protected UnaryExpression(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            _expression = expression;
        }
    }

    public abstract class BinaryExpression : Expression
    {
        private readonly Expression _leftExpression;
        private readonly Expression _rightExpression;

        public Expression LeftExpression
        {
            get { return _leftExpression; }
        }

        public Expression RightExpression
        {
            get { return _rightExpression; }
        }

        protected BinaryExpression(Expression leftExpression, Expression rightExpression)
        {
            if (leftExpression == null) throw new ArgumentNullException("leftExpression");
            if (rightExpression == null) throw new ArgumentNullException("rightExpression");

            _leftExpression = leftExpression;
            _rightExpression = rightExpression;
        }
    }

    public class AndExpression : BinaryExpression
    {
        public AndExpression(Expression leftExpression, Expression rightExpression)
            : base(leftExpression, rightExpression)
        {
        }

        public override bool Eval(IPrincipal principal)
        {
            return LeftExpression.Eval(principal) && RightExpression.Eval(principal);
        }

        public override void GetDisplayName(TextWriter writer)
        {
            writer.Write("(");
            LeftExpression.GetDisplayName(writer);
            writer.Write(" && ");
            RightExpression.GetDisplayName(writer);
            writer.Write(")");
        }
    }

    public class OrExpression : BinaryExpression
    {
        public OrExpression(Expression leftExpression, Expression rightExpression)
            : base(leftExpression, rightExpression)
        {
        }

        public override bool Eval(IPrincipal principal)
        {
            return LeftExpression.Eval(principal) || RightExpression.Eval(principal);
        }

        public override void GetDisplayName(TextWriter writer)
        {
            writer.Write("(");
            LeftExpression.GetDisplayName(writer);
            writer.Write(" || ");
            RightExpression.GetDisplayName(writer);
            writer.Write(")");
        }
    }

    public class XorExpression : BinaryExpression
    {
        public XorExpression(Expression leftExpression, Expression rightExpression)
            : base(leftExpression, rightExpression)
        {
        }

        public override bool Eval(IPrincipal principal)
        {
            return LeftExpression.Eval(principal) ^ RightExpression.Eval(principal);
        }

        public override void GetDisplayName(TextWriter writer)
        {
            writer.Write("(");
            LeftExpression.GetDisplayName(writer);
            writer.Write(" ^ ");
            RightExpression.GetDisplayName(writer);
            writer.Write(")");
        }
    }

    public class NotExpression : UnaryExpression
    {
        public NotExpression(Expression expression)
            : base(expression)
        {
        }

        public override bool Eval(IPrincipal principal)
        {
            return !Expression.Eval(principal);
        }

        public override void GetDisplayName(TextWriter writer)
        {
            writer.Write("(");
            writer.Write("!");
            Expression.GetDisplayName(writer);
            writer.Write(")");
        }
    }

    public class BooleanValueExpression : Expression
    {
        private readonly bool _value;

        public bool Value
        {
            get { return _value; }
        }

        public BooleanValueExpression(bool value)
        {
            _value = value;
        }

        public override bool Eval(IPrincipal principal)
        {
            return Value;
        }

        public override void GetDisplayName(TextWriter writer)
        {
            writer.Write(Value);
        }
    }

    public class RoleNameExpression : Expression
    {
        private readonly string _roleName;

        public string RoleName
        {
            get { return _roleName; }
        }

        public RoleNameExpression(string roleName)
        {
            if (roleName == null) throw new ArgumentNullException("roleName");

            roleName = roleName.Trim();
            if (roleName.StartsWith("[") && roleName.EndsWith("]"))
            {
                roleName = roleName.Substring(1, roleName.Length - 2);
            }

            _roleName = roleName;
        }

        public override bool Eval(IPrincipal principal)
        {
            return principal.IsInRole(RoleName);
        }

        public override void GetDisplayName(TextWriter writer)
        {
            writer.Write("[");
            writer.Write(RoleName);
            writer.Write("]");
        }
    }
}
