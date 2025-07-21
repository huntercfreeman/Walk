namespace Walk.TextEditor.RazorLib.CompilerServices;

/// <summary>
/// 3 stages of parsing:
/// The first 2 stages are solution wide, and occur immediately upon selecting a solution.
/// The final stage only occurs for files that you've opened.
/// 
/// Solution Wide upon selecting a solution:
/// - The initial solution wide parse should only parse the definitions / declarations that are non-local.
/// - Then the second time around you include the locals. BUT, you don't actually store the data about the locals, you just make Symbols for the locals.
/// 
/// Upon opening a file for the first time / making edits that cause a re-parse:
/// - Then when you open a file a third parse will occur, this parse WILL store the data about the locals.
/// </summary>
public enum CompilationUnitKind
{
    SolutionWide_DefinitionsOnly,
    SolutionWide_MinimumLocalsData,
    IndividualFile_AllData,
}
