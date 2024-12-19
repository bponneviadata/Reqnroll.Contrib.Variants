﻿using Gherkin.Ast;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Reqnroll.Configuration;
using Reqnroll.Generator.CodeDom;

namespace Reqnroll.Contrib.Variants.ReqnrollPlugin.Generator.ClassGenerator
{
    internal static class ClassGeneratorExtensions
    {
        public static void AddLinePragmaInitial(this CodeDomHelper codeDomHelper, CodeTypeDeclaration testType, string sourceFile, ReqnrollConfiguration reqnrollConfiguration)
        {
            if (reqnrollConfiguration.AllowDebugGeneratedFiles) return;
            codeDomHelper.BindTypeToSourceFile(testType, Path.GetFileName(sourceFile));
        }

        public static void AddLineDirectiveHidden(this CodeDomHelper codeDomHelper, CodeStatementCollection statements, ReqnrollConfiguration reqnrollConfiguration)
        {
            if (reqnrollConfiguration.AllowDebugGeneratedFiles) return;
            //codeDomHelper.AddDisableSourceLinePragmaStatement(statements);
        }

        public static void AddLineDirective(this CodeDomHelper codeDomHelper, Background background, CodeStatementCollection statements, ReqnrollConfiguration reqnrollConfiguration)
        {
            AddLineDirective(statements, background.Location, reqnrollConfiguration, codeDomHelper);
        }

        public static void AddLineDirective(this CodeDomHelper codeDomHelper, StepsContainer scenarioDefinition, CodeStatementCollection statements, ReqnrollConfiguration reqnrollConfiguration)
        {
            AddLineDirective(statements, scenarioDefinition.Location, reqnrollConfiguration, codeDomHelper);
        }

        public static void AddLineDirective(this CodeDomHelper codeDomHelper, Step step, CodeStatementCollection statements, ReqnrollConfiguration reqnrollConfiguration)
        {
            AddLineDirective(statements, step.Location, reqnrollConfiguration, codeDomHelper);
        }

        private static void AddLineDirective(CodeStatementCollection statements, Location location, ReqnrollConfiguration reqnrollConfiguration, CodeDomHelper codeDomHelper)
        {
            if (location == null || reqnrollConfiguration.AllowDebugGeneratedFiles) return;
            //codeDomHelper.AddSourceLinePragmaStatement(statements, location.Line, location.Column);
        }

        public static CodeExpression GetSubstitutedString(this ParameterSubstitution paramToIdentifier, string text)
        {
            if (text == null)
                return new CodeCastExpression(typeof(string), new CodePrimitiveExpression(null));
            if (paramToIdentifier == null)
                return new CodePrimitiveExpression(text);
            var regex = new Regex("\\<(?<param>[^\\>]+)\\>");
            var input = text.Replace("{", "{{").Replace("}", "}}");
            var arguments = new List<string>();
            MatchEvaluator evaluator = match =>
            {
                if (!paramToIdentifier.TryGetIdentifier(match.Groups["param"].Value, out string id))
                    return match.Value;
                int num = arguments.IndexOf(id);
                if (num < 0)
                {
                    num = arguments.Count;
                    arguments.Add(id);
                }
                return "{" + num + "}";
            };
            string str2 = regex.Replace(input, evaluator);
            if (arguments.Count == 0)
                return new CodePrimitiveExpression(text);
            var codeExpressionList = new List<CodeExpression> { new CodePrimitiveExpression(str2) };
            codeExpressionList.AddRange(arguments.Select(id => new CodeVariableReferenceExpression(id)).Cast<CodeExpression>());
            return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(string)), "Format", codeExpressionList.ToArray());
        }

        public static CodeExpression GetStringArrayExpression(this IEnumerable<string> items, ParameterSubstitution paramToIdentifier)
        {
            return new CodeArrayCreateExpression(typeof(string[]), items.Select(item => paramToIdentifier.GetSubstitutedString(item)).ToArray());
        }

        public static CodeMemberField DeclareTestRunnerMember<T>(this CodeTypeDeclaration type, string name)
        {
            var codeMemberField = new CodeMemberField(typeof(T), name);
            type.Members.Add(codeMemberField);
            return codeMemberField;
        }

        public static CodeMemberMethod CreateMethod(this CodeTypeDeclaration type)
        {
            var codeMemberMethod = new CodeMemberMethod();
            type.Members.Add(codeMemberMethod);
            return codeMemberMethod;
        }
    }
}
