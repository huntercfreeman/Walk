namespace Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

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
