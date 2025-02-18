namespace Workleap.Extensions.OpenAPI.Analyzers.Internals;

internal static class RuleIdentifiers
{
    public const string HelpUri = "https://github.com/workleap/wl-extensions-openapi";

    // DO NOT change the identifier of existing rules.
    // Projects can customize the severity level of analysis rules using a .editorconfig file.
    public const string MismatchResponseTypeWithAnnotation = "WLOAS001";
    public const string HasTypedResultsUsage = "WLOAS002";
}