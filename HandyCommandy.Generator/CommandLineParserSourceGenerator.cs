using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace HandyCommandy.Generator
{
    [Generator]
    public class CommandLineParserSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);

            var body = mainMethod.DeclaringSyntaxReferences.Single();
            var statement = (LocalDeclarationStatementSyntax)body.SyntaxTree.GetRoot().ChildNodes().OfType<GlobalStatementSyntax>().First().Statement; // TODO improve search
            var runExpression = (InvocationExpressionSyntax)statement.Declaration.Variables.Single().Initializer.Value;
            var optionExpression = ((MemberAccessExpressionSyntax)runExpression.Expression).Expression;
            List<Option> options = new List<Option>();
            while (optionExpression is InvocationExpressionSyntax s)
            {
                var key = (string)((LiteralExpressionSyntax)s.ArgumentList.Arguments[0].Expression).Token.Value;
                var description = (string)((LiteralExpressionSyntax)s.ArgumentList.Arguments[1].Expression).Token.Value;
                options.Add(Option.FromKey(key, description));

                optionExpression = ((MemberAccessExpressionSyntax)s.Expression).Expression;
            }
            string source = $@"// Auto-generated code
#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace HandyCommandy;

public class ArgBuilder
{{
    public ArgBuilder Option(string key, string description)
    {{
        return this;
    }}

    public Args Run(string[] args)
    {{
        if (Args.TryParse(args, out var result))
        {{
            return result;
        }}
        throw new Exception(""Error while parsing args."");
    }}
}}

public class Args
{{
    public Args({options.Select(v => $"{v.TypeName} {v.Name.FirstToLower()}").Join(", ")})
    {{
{options.Select(v => $"        this.{v.Name.FirstToUpper()} = {v.Name};").Join("\r\n")}
    }}

{options.Select(v => $"    public {v.TypeName} {v.Name.FirstToUpper()} {{ get; }}").Join("\r\n")}

    public static bool TryParse(string[] args, [NotNullWhen(true)]out Args? result)
    {{
{options
    .SelectMany(v =>
    {
        if (v.TypeName == "string")
        {
            return new[]
            {
                $@"var {v.Name.FirstToLower()} = args.SkipWhile(v => !v.Equals(""--{v.Name}"", StringComparison.InvariantCultureIgnoreCase)).Skip(1).FirstOrDefault();",
                $@"if ({v.Name.FirstToLower()} == null)",
                "{",
                "    result = default;",
                "    return false;",
                "}"
            };
        }
        else if (v.TypeName == "string?")
        {
            return new[]
            {
                $@"var {v.Name.FirstToLower()} = args.SkipWhile(v => !v.Equals(""--{v.Name}"", StringComparison.InvariantCultureIgnoreCase)).Skip(1).FirstOrDefault();"
            };
        }
        else if (v.TypeName == "bool")
        {
            return new[]
            {
                $@"var {v.Name.FirstToLower()} = args.Any(v => v.Equals(""--{v.Name}"", StringComparison.InvariantCultureIgnoreCase));"
            };
        }
        else
        {
            return new[] { "throw new NotSupportedException();" };
        }
    })
    .Select(v => $"        {v}")
    .Join("\r\n")}

        result = new Args({options.Select(v => v.Name.FirstToLower()).Join(", ")});
        return true;
    }}
}}
";
            var typeName = mainMethod.ContainingType.Name;

            context.AddSource($"{typeName}.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }

    class Option
    {
        public Option(string name, string typeName, string description)
        {
            Name = name;
            TypeName = typeName;
            Description = description;
        }

        public string Name { get; }
        public string TypeName { get; }
        public string Description { get; }

        public static Option FromKey(string key, string description)
        {
            var indexOfFirstSpace = key.IndexOf(' ');
            if (indexOfFirstSpace == -1)
            {
                var name = key.TrimStart('-');
                return new Option(name, "bool", description);
            }
            else
            {
                var name = key.Substring(0, indexOfFirstSpace).TrimStart('-');
                var value = key.Substring(indexOfFirstSpace + 1);
                if (value.StartsWith("["))
                {
                    return new Option(name, "string?", description);
                }
                else
                {
                    return new Option(name, "string", description);
                }
            }
        }
    }

    public static class StringExtensions
    {
        public static string FirstToUpper(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            return text.Substring(0, 1).ToUpper() + text.Substring(1);
        }

        public static string FirstToLower(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            return text.Substring(0, 1).ToLower() + text.Substring(1);
        }

        public static string Join(this IEnumerable<string> values, string separator)
        {
            return string.Join(separator, values);
        }
    }
}
