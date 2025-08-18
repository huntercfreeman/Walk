using System.Text;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Icons.Displays;
using Walk.Common.RazorLib.Icons.Displays.Codicon;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.DotNetSolution.Models.Project;
using Walk.CompilerServices.Xml;
using Walk.Extensions.DotNet.Nugets.Models;

namespace Walk.Extensions.DotNet.CSharpProjects.Models;

public class TreeViewCSharpProjectNugetPackageReferences : TreeViewWithType<CSharpProjectNugetPackageReferences>
{
    public TreeViewCSharpProjectNugetPackageReferences(
            CSharpProjectNugetPackageReferences cSharpProjectNugetPackageReferences,
            CommonService commonService,
            bool isExpandable,
            bool isExpanded)
        : base(cSharpProjectNugetPackageReferences, isExpandable, isExpanded)
    {
        CommonService = commonService;
    }

    public CommonService CommonService { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not TreeViewCSharpProjectNugetPackageReferences otherTreeView)
            return false;

        return otherTreeView.GetHashCode() == GetHashCode();
    }

    public override int GetHashCode() => Item.CSharpProjectAbsolutePath.Value.GetHashCode();

    public override string GetDisplayText() => "NuGet Packages";
    
    public override Microsoft.AspNetCore.Components.RenderFragment<IconDriver> GetIcon => IconNuGetPackagesFragment.Render;

    /*public override TreeViewRenderer GetTreeViewRenderer()
    {
    
        using Microsoft.AspNetCore.Components;
        using Walk.Common.RazorLib.Options.Models;
        
        namespace Walk.Extensions.DotNet.CSharpProjects.Displays;
        
        public partial class TreeViewCSharpProjectNugetPackageReferencesDisplay : ComponentBase
        {
            [Inject]
            private IAppOptionsService AppOptionsService { get; set; } = null!;
        }
    
    
        
        <div>

            @{
                var appOptionsState = AppOptionsService.GetAppOptionsState();
            
                var iconDriver = new IconDriver(
                    appOptionsState.Options.IconSizeInPixels,
                    appOptionsState.Options.IconSizeInPixels);
            }
        
            @IconNuGetPackagesFragment.Render(iconDriver)
            NuGet Packages
        </div>
        
        
        
        
        return new TreeViewRenderer(
            DotNetComponentRenderers.CompilerServicesTreeViews.TreeViewCSharpProjectNugetPackageReferencesRendererType,
            null);
    }*/

    public override async Task LoadChildListAsync()
    {
        var previousChildren = new List<TreeViewNoType>(ChildList);

        using StreamReader sr = new StreamReader(Item.CSharpProjectAbsolutePath.Value);
        var lexerOutput = XmlLexer.Lex(new StreamReaderWrap(sr));
        
        var stringBuilder = new StringBuilder();
        var getTextBuffer = new char[1];
        
        List<(string ValueAttributeOne, string ValueAttributeTwo)> lightWeightNugetPackageRecords = new();
        
        var outputReader = new XmlOutputReader(lexerOutput.TextSpanList);
        
        outputReader.ConsoleWriteFormatted(
            sr,
            stringBuilder,
            getTextBuffer);
        
        outputReader.FindTagGetEitherOrBothAttributeValue(
            targetTagName: "PackageReference",
            targetAttributeOne: "Include",
            targetAttributeTwo: "Version",
            shouldIncludeFullMissLines: false,
            sr,
            stringBuilder,
            getTextBuffer,
            lightWeightNugetPackageRecords);

        var cSharpProjectAbsolutePathString = Item.CSharpProjectAbsolutePath.Value;

        var newChildList = lightWeightNugetPackageRecords.Select(
            npr => (TreeViewNoType)new TreeViewCSharpProjectNugetPackageReference(
                new(cSharpProjectAbsolutePathString, new LightWeightNugetPackageRecord(npr.ValueAttributeOne, npr.ValueAttributeOne, npr.ValueAttributeTwo)),
                CommonService,
                false,
                false)
            {
                TreeViewChangedKey = Key<TreeViewChanged>.NewKey()
            })
            .ToList();

        ChildList = newChildList;
        LinkChildren(previousChildren, ChildList);
        TreeViewChangedKey = Key<TreeViewChanged>.NewKey();
    }

    public override void RemoveRelatedFilesFromParent(List<TreeViewNoType> siblingsAndSelfTreeViews)
    {
        return;
    }
}
