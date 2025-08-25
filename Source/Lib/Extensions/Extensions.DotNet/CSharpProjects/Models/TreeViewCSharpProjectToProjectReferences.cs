using System.Text;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Icons.Displays;
using Walk.Common.RazorLib.Icons.Displays.Codicon;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.DotNetSolution.Models.Project;
using Walk.CompilerServices.Xml;

namespace Walk.Extensions.DotNet.CSharpProjects.Models;

public class TreeViewCSharpProjectToProjectReferences : TreeViewWithType<CSharpProjectToProjectReferences>
{
    public TreeViewCSharpProjectToProjectReferences(
            CSharpProjectToProjectReferences cSharpProjectToProjectReferences,
            CommonService commonService,
            bool isExpandable,
            bool isExpanded)
        : base(cSharpProjectToProjectReferences, isExpandable, isExpanded)
    {
        CommonService = commonService;
    }

    public CommonService CommonService { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not TreeViewCSharpProjectToProjectReferences otherTreeView)
            return false;

        return otherTreeView.GetHashCode() == GetHashCode();
    }

    public override int GetHashCode() => Item.CSharpProjectAbsolutePath.Value.GetHashCode();

    public override string GetDisplayText() => "Project References";
    
    public override Microsoft.AspNetCore.Components.RenderFragment<IconDriver> GetIcon => IconReferencesFragment.Render;

    /*public override TreeViewRenderer GetTreeViewRenderer()
    {
    
        using Microsoft.AspNetCore.Components;
        using Walk.Common.RazorLib.Options.Models;
        
        namespace Walk.Extensions.DotNet.CSharpProjects.Displays;
        
        public partial class TreeViewCSharpProjectToProjectReferencesDisplay : ComponentBase
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
        
            @IconReferencesFragment.Render(iconDriver)
            Project References
        </div>
        
        
        
        return new TreeViewRenderer(
            DotNetComponentRenderers.CompilerServicesTreeViews.TreeViewCSharpProjectToProjectReferencesRendererType,
            null);
    }*/

    public override async Task LoadChildListAsync()
    {
        var previousChildren = new List<TreeViewNoType>(ChildList);

        using StreamReader sr = new StreamReader(Item.CSharpProjectAbsolutePath.Value);
        var lexerOutput = XmlLexer.Lex(new StreamReaderWrap(sr), textSpanList: new());
        
        var stringBuilder = new StringBuilder();
        var getTextBuffer = new char[1];
        
        List<string> relativePathReferenceList = new();
        
        var outputReader = new XmlOutputReader(lexerOutput.TextSpanList);
        
        outputReader.FindTagGetAttributeValue(
            targetTagName: "ProjectReference",
            targetAttributeOne: "Include",
            shouldIncludeFullMissLines: false,
            sr,
            stringBuilder,
            getTextBuffer,
            relativePathReferenceList);
        
        List<CSharpProjectToProjectReference> cSharpProjectToProjectReferences = new();
        
        var tokenBuilder = new StringBuilder();
        var formattedBuilder = new StringBuilder();
        
        var moveUpDirectoryToken = $"..{CommonService.EnvironmentProvider.DirectorySeparatorChar}";
        // "./" is being called the 'sameDirectoryToken'
        var sameDirectoryToken = $".{CommonService.EnvironmentProvider.DirectorySeparatorChar}";

        var projectAncestorDirectoryList = Item.CSharpProjectAbsolutePath.GetAncestorDirectoryList(
            CommonService.EnvironmentProvider,
            tokenBuilder,
            formattedBuilder,
            AbsolutePathNameKind.NameWithExtension);
        
        foreach (var projectReference in relativePathReferenceList)
        {
            var referenceProjectAbsolutePathString = CommonFacts.GetAbsoluteFromAbsoluteAndRelative(
                Item.CSharpProjectAbsolutePath,
                projectReference,
                (IEnvironmentProvider)CommonService.EnvironmentProvider,
                tokenBuilder,
                formattedBuilder,
                moveUpDirectoryToken: moveUpDirectoryToken,
                sameDirectoryToken: sameDirectoryToken,
                projectAncestorDirectoryList);

            var referenceProjectAbsolutePath = CommonService.EnvironmentProvider.AbsolutePathFactory(
                referenceProjectAbsolutePathString,
                false,
                tokenBuilder,
                formattedBuilder,
                AbsolutePathNameKind.NameWithExtension);

            var cSharpProjectToProjectReference = new CSharpProjectToProjectReference(
                Item.CSharpProjectAbsolutePath,
                referenceProjectAbsolutePath);

            cSharpProjectToProjectReferences.Add(cSharpProjectToProjectReference);
        }

        var newChildList = cSharpProjectToProjectReferences
            .Select(x => (TreeViewNoType)new TreeViewCSharpProjectToProjectReference(
                x,
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
