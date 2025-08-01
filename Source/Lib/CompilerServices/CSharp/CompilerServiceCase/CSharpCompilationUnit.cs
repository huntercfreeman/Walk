using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.CompilerServices.CSharp.CompilerServiceCase;

/// <summary>
/// I think I want to get rid of the 'IBinderSession'.
/// Some of its properties were moved to the 'CSharpCompilationUnit'
/// and some to the 'CSharpParserModel'.
///
/// This was done based on whether the data should continue to exist
/// after the parse finished, or if the data should be cleared immediately after the parse finishes
/// (respectively).
/// </summary>
public struct CSharpCompilationUnit : IExtendedCompilationUnit, ICompilerServiceResource
{
    public CSharpCompilationUnit(CompilationUnitKind compilationUnitKind)
    {
        CompilationUnitKind = compilationUnitKind;
    }
    
    ICompilationUnit ICompilerServiceResource.CompilationUnit { get => this; set => _ = value; }
    
    public CompilationUnitKind CompilationUnitKind { get; }

    /// <summary>Set this index in the CSharpParserModel constructor</summary>
    public int IndexDiagnosticList { get; set; }
    public int CountDiagnosticList { get; set; }
    
    /// <summary>Set this index in the CSharpParserModel constructor</summary>
    public int IndexSymbolList { get; set; }
    public int CountSymbolList { get; set; }
    
    /// <summary>This needs to initialize to -1 to signify there are no entries in the pooled list.</summary>
    public int IndexFunctionInvocationParameterMetadataList { get; set; } = -1;
    public int CountFunctionInvocationParameterMetadataList { get; set; }
    
    /// <summary>Set this index in the CSharpParserModel constructor</summary>
    public int IndexCodeBlockOwnerList { get; set; }
    public int CountCodeBlockOwnerList { get; set; }
    
    /// <summary>Set this index in the CSharpParserModel constructor</summary>
    public int IndexNodeList { get; set; }
    public int CountNodeList { get; set; }
    
    public bool IsDefault() => CompilationUnitKind == CompilationUnitKind.None;
}
