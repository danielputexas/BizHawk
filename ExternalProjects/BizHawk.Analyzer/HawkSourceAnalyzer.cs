﻿namespace BizHawk.Analyzers;

using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HawkSourceAnalyzer : DiagnosticAnalyzer
{
	private static readonly DiagnosticDescriptor DiagInterpStringIsDollarAt = new(
		id: "BHI1004",
		title: "Verbatim interpolated strings should begin $@, not @$",
		messageFormat: "Swap @ and $ on interpolated string",
		category: "Usage",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	private static readonly DiagnosticDescriptor DiagNoAnonClasses = new(
		id: "BHI1002",
		title: "Do not use anonymous types (classes)",
		messageFormat: "Replace anonymous class with tuple or explicit type",
		category: "Usage",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	private static readonly DiagnosticDescriptor DiagNoAnonDelegates = new(
		id: "BHI1001",
		title: "Do not use anonymous delegates",
		messageFormat: "Replace anonymous delegate with lambda or local method",
		category: "Usage",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	private static readonly DiagnosticDescriptor DiagNoQueryExpression = new(
		id: "BHI1003",
		title: "Do not use query expression syntax",
		messageFormat: "Use method chain for LINQ instead of query expression syntax",
		category: "Usage",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
		DiagInterpStringIsDollarAt,
		DiagNoAnonClasses,
		DiagNoAnonDelegates,
		DiagNoQueryExpression);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(
			static snac =>
			{
				switch (snac.Node)
				{
					case AnonymousMethodExpressionSyntax:
						snac.ReportDiagnostic(Diagnostic.Create(DiagNoAnonDelegates, snac.Node.GetLocation()));
						break;
					case AnonymousObjectCreationExpressionSyntax:
						snac.ReportDiagnostic(Diagnostic.Create(DiagNoAnonClasses, snac.Node.GetLocation()));
						break;
					case InterpolatedStringExpressionSyntax ises:
						if (ises.StringStartToken.Text[0] is '@') snac.ReportDiagnostic(Diagnostic.Create(DiagInterpStringIsDollarAt, ises.GetLocation()));
						break;
					case QueryExpressionSyntax:
						snac.ReportDiagnostic(Diagnostic.Create(DiagNoQueryExpression, snac.Node.GetLocation()));
						break;
				}
			},
			SyntaxKind.AnonymousObjectCreationExpression,
			SyntaxKind.AnonymousMethodExpression,
			SyntaxKind.InterpolatedStringExpression,
			SyntaxKind.QueryExpression);
	}
}
