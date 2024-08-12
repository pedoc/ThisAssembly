﻿// <autogenerated />
#nullable enable
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using static Devlooped.Sponsors.SponsorLink;

namespace Devlooped.Sponsors;

/// <summary>
/// Links the sponsor status for the current compilation.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public class SponsorLinkAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = DiagnosticsManager.KnownDescriptors.Values.ToImmutableArray();

#pragma warning disable RS1026 // Enable concurrent execution
    public override void Initialize(AnalysisContext context)
#pragma warning restore RS1026 // Enable concurrent execution
    {
#if !DEBUG
        // Only enable concurrent execution in release builds, otherwise debugging is quite annoying.
        context.EnableConcurrentExecution();
#endif
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

#pragma warning disable RS1013 // Start action has no registered non-end actions
        // We do this so that the status is set at compilation start so we can use it 
        // across all other analyzers. We report only on finish because multiple 
        // analyzers can report the same diagnostic and we want to avoid duplicates. 
        context.RegisterCompilationStartAction(ctx =>
        {
            // Setting the status early allows other analyzers to potentially check for it.
            var status = Diagnostics.GetOrSetStatus(() => ctx.Options);

            // Never report any diagnostic unless we're in an editor.
            if (IsEditor)
            {
                // NOTE: for multiple projects with the same product name, we only report one diagnostic, 
                // so it's expected to NOT get a diagnostic back. Also, we don't want to report 
                // multiple diagnostics for each project in a solution that uses the same product.
                ctx.RegisterCompilationEndAction(ctx =>
                {
                    // We'd never report Info/hero link if users opted out of it.
                    if (status.IsSponsor() &&
                        ctx.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.SponsorLinkHero", out var slHero) &&
                        bool.TryParse(slHero, out var isHero) && isHero)
                        return;

                    // Only report if the package is directly referenced in the project for
                    // any of the funding packages we monitor (i.e. we could have one or more
                    // metapackages we also consider "direct references).
                    // See SL_CollectDependencies in buildTransitive\Devlooped.Sponsors.targets
                    foreach (var prop in Funding.PackageIds.Select(id => id.Replace('.', '_')))
                    {
                        if (ctx.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property." + prop, out var package) &&
                            package?.Length > 0 &&
                            Diagnostics.Pop() is { } diagnostic)
                        {
                            ctx.ReportDiagnostic(diagnostic);
                            break;
                        }
                    }
                });
            }
        });
#pragma warning restore RS1013 // Start action has no registered non-end actions
    }
}
