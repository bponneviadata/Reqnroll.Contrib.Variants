﻿using Gherkin.Ast;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.Parser;

namespace Reqnroll.Contrib.Variants.UnitTests
{
    internal static class Helpers
    {
        public static T GetScenario<T>(this ReqnrollDocument document, string scenarioName) where T : Scenario
        {
            return (T)document.ReqnrollFeature.ScenarioDefinitions.FirstOrDefault(a => a.Name == scenarioName);
        }

        public static IList<Tag> GetTagsByNameStart(this Scenario scenario, string tagName)
        {
            return scenario.GetTags().Where(a => a.GetNameWithoutAt().StartsWith(tagName)).ToList();
        }

        public static IList<Tag> GetTagsByNameStart(this Feature feature, string tagName)
        {
            return feature.Tags.Where(a => a.GetNameWithoutAt().StartsWith(tagName)).ToList();
        }

        public static Tag GetTagsByNameExact(this Feature feature, string tagName)
        {
            return feature.Tags.Where(a => a.GetNameWithoutAt() == tagName).FirstOrDefault();
        }

        public static IList<Tag> GetTagsExceptNameStart(this Feature feature, string tagName)
        {
            return feature.Tags.Where(a => !a.GetNameWithoutAt().StartsWith(tagName)).ToList();
        }

        public static Tag GetTagsByNameExact(this Scenario scenario, string tagName)
        {
            return scenario.GetTags().Where(a => a.GetNameWithoutAt() == tagName).FirstOrDefault();
        }

        public static IList<Tag> GetTagsExceptNameStart(this Scenario scenario, string tagName)
        {
            return scenario.GetTags().Where(a => !a.GetNameWithoutAt().StartsWith(tagName)).ToList();
        }

        public static IList<TableRow> GetExamplesTableBody(this ScenarioOutline scenario)
        {
            return scenario.Examples.First().TableBody.ToList();
        }

        public static IList<TableCell> GetExamplesTableHeaders(this ScenarioOutline scenario)
        {
            return scenario.Examples.First().TableHeader.Cells.ToList();
        }

        public static IList<CodeTypeMember> GetTestMethods(this CodeNamespace generatedCode, Scenario scenario)
        {
            return generatedCode.Types[0].Members.Cast<CodeTypeMember>().Where(a => a.Name.StartsWith(scenario.Name.Replace(" ", "")
                .Replace(",", ""), StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        public static IList<CodeTypeMember> GetRowTestMethods(this CodeNamespace generatedCode, Scenario scenario)
        {
            return generatedCode.GetTestMethods(scenario).Where(a => a.CustomAttributes.Count > 0).ToList();
        }

        public static CodeTypeMember GetRowTestBaseMethod(this CodeNamespace generatedCode, Scenario scenario)
        {
            return generatedCode.GetTestMethods(scenario).FirstOrDefault(a => a.CustomAttributes.Count == 0);
        }

        public static IList<CodeAttributeDeclaration> GetMethodAttributes(this CodeTypeMember member, string attributeName)
        {
            return member.CustomAttributes.Cast<CodeAttributeDeclaration>().Where(a => a.Name == attributeName).ToList();
        }

        public static IList<CodeParameterDeclarationExpression> GetMethodParameters(this CodeTypeMember member)
        {
            return ((CodeMemberMethod)member).Parameters.Cast<CodeParameterDeclarationExpression>().ToList();
        }

        public static IList<CodeAttributeArgument> GetAttributeArguments(this CodeAttributeArgumentCollection args)
        {
            return args.Cast<CodeAttributeArgument>().ToList();
        }

        public static string GetArgumentValue(this CodeAttributeArgument codeExpression)
        {
            return ((CodePrimitiveExpression)codeExpression.Value).Value.ToString();
        }

        public static IList<CodeStatement> GetMethodStatements(this CodeTypeMember member)
        {
            return ((CodeMemberMethod)member).Statements.Cast<CodeStatement>().ToList();
        }

        public static IList<string> GetStepTableHeaderArgs(this CodeStatement statement)
        {
            return ((CodeArrayCreateExpression)((CodeObjectCreateExpression)((CodeVariableDeclarationStatement)statement).InitExpression)
                .Parameters[0]).Initializers.Cast<CodeExpression>().Select(a => a as CodePrimitiveExpression).Select(b => b.Value.ToString()).ToList();
        }

        public static IList<string> GetStepTableCellArgs(this CodeStatement statement)
        {
            return ((CodeArrayCreateExpression)((CodeMethodInvokeExpression)((CodeExpressionStatement)statement).Expression)
                .Parameters[0]).Initializers.Cast<CodeExpression>().Select(a => a as CodePrimitiveExpression).Select(b => b.Value.ToString()).ToList();
        }

        public static IList<CodeStatement> GetTableStatements(this IEnumerable<CodeStatement> statements, int rowCount)
        {
            return statements.SkipWhile(a =>
            {
                var istr = a as CodeVariableDeclarationStatement;
                return istr == null || istr.Type.BaseType != "Reqnroll.Table";
            }).Take(rowCount).ToList();
        }
    }
}
