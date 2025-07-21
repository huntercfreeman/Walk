using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.Extensions.CompilerServices;

public record struct ScopeKeyAndIdentifierTextAndSyntaxKind(
    int ScopeIndexKey,
    string IdentifierText,
    SyntaxKind SyntaxKind);
