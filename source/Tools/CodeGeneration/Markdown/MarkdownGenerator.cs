﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Pihrtsoft.Markdown;
using Pihrtsoft.Markdown.Linq;
using Roslynator.Metadata;
using static Pihrtsoft.Markdown.Linq.MFactory;

namespace Roslynator.CodeGeneration.Markdown
{
    public static class MarkdownGenerator
    {
        public static string CreateReadMe(IEnumerable<AnalyzerDescriptor> analyzers, IEnumerable<RefactoringDescriptor> refactorings, IComparer<string> comparer)
        {
            MDocument document = Document(
                Heading3("List of Analyzers"),
                analyzers
                    .OrderBy(f => f.Id, comparer)
                    .Select(analyzer => BulletItem(analyzer.Id, " - ", Link(analyzer.Title.TrimEnd('.'), $"docs/analyzers/{analyzer.Id}.md"))),
                Heading3("List of Refactorings"),
                refactorings
                    .OrderBy(f => f.Title, comparer)
                    .Select(refactoring => BulletItem(Link(refactoring.Title.TrimEnd('.'), $"docs/refactorings/{refactoring.Id}.md"))));

            return File.ReadAllText(@"..\text\ReadMe.txt", Encoding.UTF8)
                + Environment.NewLine
                + document;
        }

        public static string CreateRefactoringsMarkdown(IEnumerable<RefactoringDescriptor> refactorings, IComparer<string> comparer)
        {
            MDocument document = Document(
                Heading2("Roslynator Refactorings"),
                GetRefactorings());

            return document.ToString();

            IEnumerable<object> GetRefactorings()
            {
                foreach (RefactoringDescriptor refactoring in refactorings.OrderBy(f => f.Title, comparer))
                {
                    yield return Heading4($"{refactoring.Title} ({refactoring.Id})");
                    yield return BulletItem(Bold("Syntax"), ": ", string.Join(", ", refactoring.Syntaxes.Select(f => f.Name)));

                    if (!string.IsNullOrEmpty(refactoring.Span))
                        yield return BulletItem(Bold("Span"), ": ", refactoring.Span);

                    foreach (object item in GetRefactoringSamples(refactoring))
                        yield return item;
                }
            }
        }

        private static IEnumerable<object> GetRefactoringSamples(RefactoringDescriptor refactoring)
        {
            if (refactoring.Samples.Count > 0)
            {
                foreach (MElement element in GetSamples(refactoring.Samples, Heading4("Before"), Heading4("After")))
                    yield return element;
            }
            else if (refactoring.Images.Count > 0)
            {
                bool isFirst = true;

                foreach (ImageDescriptor image in refactoring.Images)
                {
                    if (!isFirst)
                        yield return Environment.NewLine;

                    yield return RefactoringImage(refactoring, image.Name);
                    yield return Environment.NewLine;

                    isFirst = false;
                }

                yield return Environment.NewLine;
            }
            else
            {
                yield return RefactoringImage(refactoring, refactoring.Identifier);
                yield return Environment.NewLine;
                yield return Environment.NewLine;
            }
        }

        private static IEnumerable<MElement> GetSamples(IEnumerable<SampleDescriptor> samples, MHeading beforeHeader, MHeading afterHeader)
        {
            bool isFirst = true;

            foreach (SampleDescriptor sample in samples)
            {
                if (!isFirst)
                {
                    yield return HorizontalRule();
                }
                else
                {
                    isFirst = false;
                }

                yield return beforeHeader;
                yield return FencedCodeBlock(sample.Before, LanguageIdentifiers.CSharp);

                if (!string.IsNullOrEmpty(sample.After))
                {
                    yield return afterHeader;
                    yield return FencedCodeBlock(sample.After, LanguageIdentifiers.CSharp);
                }
            }
        }

        public static string CreateRefactoringMarkdown(RefactoringDescriptor refactoring)
        {
            var format = new MarkdownFormat(tableOptions: MarkdownFormat.Default.TableOptions | TableOptions.FormatContent);

            MDocument document = Document(
                Heading2(refactoring.Title),
                Table(TableRow("Property", "Value"),
                    TableRow("Id", refactoring.Id),
                    TableRow("Title", refactoring.Title),
                    TableRow("Syntax", string.Join(", ", refactoring.Syntaxes.Select(f => f.Name))),
                    (!string.IsNullOrEmpty(refactoring.Span)) ? TableRow("Span", refactoring.Span) : null,
                    TableRow("Enabled by Default", CheckboxOrHyphen(refactoring.IsEnabledByDefault))),
                Heading3("Usage"),
                GetRefactoringSamples(refactoring),
                Link("full list of refactorings", "Refactorings.md"));

            return document.ToString(format);
        }

        public static string CreateAnalyzerMarkdown(AnalyzerDescriptor analyzer)
        {
            var format = new MarkdownFormat(tableOptions: MarkdownFormat.Default.TableOptions | TableOptions.FormatContent);

            MDocument document = Document(
                Heading1($"{((analyzer.IsObsolete) ? "[deprecated] " : "")}{analyzer.Id}: {analyzer.Title.TrimEnd('.')}"),
                Table(
                    TableRow("Property", "Value"),
                    TableRow("Id", analyzer.Id),
                    TableRow("Category", analyzer.Category),
                    TableRow("Default Severity", analyzer.DefaultSeverity),
                    TableRow("Enabled by Default", CheckboxOrHyphen(analyzer.IsEnabledByDefault)),
                    TableRow("Supports Fade-Out", CheckboxOrHyphen(analyzer.SupportsFadeOut)),
                    TableRow("Supports Fade-Out Analyzer", CheckboxOrHyphen(analyzer.SupportsFadeOutAnalyzer))),
                Samples(),
                Heading2("How to Suppress"),
                Heading3("SuppressMessageAttribute"),
                FencedCodeBlock($"[assembly: SuppressMessage(\"{analyzer.Category}\", \"{analyzer.Id}:{analyzer.Title}\", Justification = \"<Pending>\")]", LanguageIdentifiers.CSharp),
                Heading3("#pragma"),
                FencedCodeBlock($"#pragma warning disable {analyzer.Id} // {analyzer.Title}\r\n#pragma warning restore {analyzer.Id} // {analyzer.Title}", LanguageIdentifiers.CSharp),
                Heading3("Ruleset"),
                BulletItem(Link("How to configure rule set", "../HowToConfigureAnalyzers.md")));

            return document.ToString(format);

            IEnumerable<MElement> Samples()
            {
                ReadOnlyCollection<SampleDescriptor> samples = analyzer.Samples;

                if (samples.Count > 0)
                {
                    yield return Heading2((samples.Count == 1) ? "Example" : "Examples");

                    foreach (MElement item in GetSamples(samples, Heading3("Code with Diagnostic"), Heading3("Code with Fix")))
                        yield return item;
                }
            }
        }

        public static string CreateAnalyzersReadMe(IEnumerable<AnalyzerDescriptor> analyzers, IComparer<string> comparer)
        {
            MDocument document = Document(
                Heading2("Roslynator Analyzers"),
                Table(
                    TableRow("Id", "Title", "Category", TableColumn(Alignment.Center, "Enabled by Default")),
                    analyzers.OrderBy(f => f.Id, comparer).Select(f =>
                    {
                        return TableRow(
                            f.Id,
                            Link(f.Title.TrimEnd('.'), $"../../docs/analyzers/{f.Id}.md"),
                            f.Category,
                            CheckboxOrHyphen(f.IsEnabledByDefault));
                    })));

            return document.ToString();
        }

        public static string CreateRefactoringsReadMe(IEnumerable<RefactoringDescriptor> refactorings, IComparer<string> comparer)
        {
            MDocument document = Document(
                Heading2("Roslynator Refactorings"),
                Table(
                    TableRow("Id", "Title", TableColumn(Alignment.Center, "Enabled by Default")),
                    refactorings.OrderBy(f => f.Title, comparer).Select(f =>
                    {
                        return TableRow(
                        f.Id,
                        Link(f.Title.TrimEnd('.'), $"../../docs/refactorings/{f.Id}.md"),
                        CheckboxOrHyphen(f.IsEnabledByDefault));
                    })));

            return document.ToString();
        }

        public static string CreateCodeFixesReadMe(IEnumerable<CodeFixDescriptor> codeFixes, IEnumerable<CompilerDiagnosticDescriptor> diagnostics, IComparer<string> comparer)
        {
            MDocument document = Document(
                Heading2("Roslynator Code Fixes"),
                Table(
                    TableRow("Id", "Title", "Fixable Diagnostics", TableColumn(Alignment.Center, "Enabled by Default")),
                    codeFixes.OrderBy(f => f.Title, comparer).Select(f =>
                    {
                        return TableRow(
                            f.Id,
                            f.Title.TrimEnd('.'),
                            Join(new MText(", "), f.FixableDiagnosticIds.Join(diagnostics, x => x, y => y.Id, (x, y) => LinkOrText(x, y.HelpUrl))),
                            CheckboxOrHyphen(f.IsEnabledByDefault));
                    })));

            return document.ToString();
        }

        public static string CreateCodeFixesByDiagnosticId(IEnumerable<CodeFixDescriptor> codeFixes, IEnumerable<CompilerDiagnosticDescriptor> diagnostics)
        {
            MDocument document = Document(
                Heading2("Roslynator Code Fixes by Diagnostic Id"),
                Table(
                    TableRow("Diagnostic", "Code Fixes"),
                    GetRows()));

            return document.ToString();

            IEnumerable<MTableRow> GetRows()
            {
                foreach (var grouping in codeFixes
                    .SelectMany(codeFix => codeFix.FixableDiagnosticIds.Select(diagnosticId => new { DiagnosticId = diagnosticId, CodeFixDescriptor = codeFix }))
                    .OrderBy(f => f.DiagnosticId)
                    .ThenBy(f => f.CodeFixDescriptor.Id)
                    .GroupBy(f => f.DiagnosticId))
                {
                    CompilerDiagnosticDescriptor diagnostic = diagnostics.FirstOrDefault(f => f.Id == grouping.Key);

                    yield return TableRow(
                        LinkOrText(diagnostic?.Id ?? grouping.Key, diagnostic?.HelpUrl),
                        string.Join(", ", grouping.Select(f => f.CodeFixDescriptor.Id)));
                }
            }
        }

        public static string CreateAnalyzersByCategoryMarkdown(IEnumerable<AnalyzerDescriptor> analyzers, IComparer<string> comparer)
        {
            MDocument document = Document(
                Heading2("Roslynator Analyzers by Category"),
                Table(
                    TableRow("Category", "Title", "Id", TableColumn(Alignment.Center, "Enabled by Default")),
                    GetRows()));

            return document.ToString();

            IEnumerable<MTableRow> GetRows()
            {
                foreach (IGrouping<string, AnalyzerDescriptor> grouping in analyzers
                    .GroupBy(f => MarkdownEscaper.Escape(f.Category))
                    .OrderBy(f => f.Key, comparer))
                {
                    foreach (AnalyzerDescriptor analyzer in grouping.OrderBy(f => f.Title, comparer))
                    {
                        yield return TableRow(
                            grouping.Key,
                            Link(analyzer.Title.TrimEnd('.'), $"../../docs/analyzers/{analyzer.Id}.md"),
                            analyzer.Id,
                            CheckboxOrHyphen(analyzer.IsEnabledByDefault));
                    }
                }
            }
        }

        private static MImage RefactoringImage(RefactoringDescriptor refactoring, string fileName)
        {
            return Image(refactoring.Title, $"../../images/refactorings/{fileName}.png");
        }

        private static MElement CheckboxOrHyphen(bool value)
        {
            if (value)
            {
                return CharEntity((char)0x2713);
            }
            else
            {
                return new MText("-");
            }
        }
    }
}
