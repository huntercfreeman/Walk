namespace Walk.Extensions.CompilerServices.Syntax.Enums;

public enum ArgumentModifierKind
{
    None,
    Out,
    In,
    Ref,
    Params,
    This,
    Readonly,
    RefReadonly,
    // Is ReadonlyRef valid C#?
    ReadonlyRef,
    ThisRef,
    RefThis,
}
