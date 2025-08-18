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
        var lexerOutput = XmlLexer.Lex(new StreamReaderWrap(sr));
        
        var stringBuilder = new StringBuilder();
        var getTextBuffer = new char[1];
        
        List<string> relativePathReferenceList = new();
        
        for (int indexTextSpan = 0; indexTextSpan < lexerOutput.TextSpanList.Count; indexTextSpan++)
        {
            var textSpan = lexerOutput.TextSpanList[indexTextSpan];
            var decorationKind = (XmlDecorationKind)textSpan.DecorationByte;
            
            if (decorationKind == XmlDecorationKind.TagNameOpen)
            {
                sr.BaseStream.Seek(textSpan.ByteIndex, SeekOrigin.Begin);
                sr.DiscardBufferedData();
                stringBuilder.Clear();
                for (int i = 0; i < textSpan.Length; i++)
                {
                    sr.Read(getTextBuffer, 0, 1);
                    stringBuilder.Append(getTextBuffer[0]);
                }
                var tagNameOpenString = stringBuilder.ToString();
            
                if (tagNameOpenString == "ProjectReference")
                {
                    var includeValue = string.Empty;
                
                    while (indexTextSpan < lexerOutput.TextSpanList.Count - 1)
                    {
                        if ((XmlDecorationKind)lexerOutput.TextSpanList[indexTextSpan + 1].DecorationByte == XmlDecorationKind.AttributeName)
                        {
                            var attributeNameTextSpan = lexerOutput.TextSpanList[indexTextSpan + 1];
                            ++indexTextSpan;
                            
                            sr.BaseStream.Seek(attributeNameTextSpan.ByteIndex, SeekOrigin.Begin);
                            sr.DiscardBufferedData();
                            stringBuilder.Clear();
                            for (int i = 0; i < attributeNameTextSpan.Length; i++)
                            {
                                sr.Read(getTextBuffer, 0, 1);
                                stringBuilder.Append(getTextBuffer[0]);
                            }
                            var attributeNameString = stringBuilder.ToString();
                            
                            while (indexTextSpan < lexerOutput.TextSpanList.Count - 1)
                            {
                                var nextDecorationKind = (XmlDecorationKind)lexerOutput.TextSpanList[indexTextSpan + 1].DecorationByte;
                                
                                if (nextDecorationKind == XmlDecorationKind.AttributeOperator)
                                {
                                    ++indexTextSpan;
                                }
                                else if (nextDecorationKind == XmlDecorationKind.AttributeDelimiter)
                                {
                                    ++indexTextSpan;
                                }
                                else if (nextDecorationKind == XmlDecorationKind.AttributeValue)
                                {
                                    var attributeValueTextSpan = lexerOutput.TextSpanList[indexTextSpan + 1];
                                    
                                    sr.BaseStream.Seek(attributeValueTextSpan.ByteIndex, SeekOrigin.Begin);
                                    sr.DiscardBufferedData();
                                    stringBuilder.Clear();
                                    for (int i = 0; i < attributeValueTextSpan.Length; i++)
                                    {
                                        sr.Read(getTextBuffer, 0, 1);
                                        stringBuilder.Append(getTextBuffer[0]);
                                    }
                                    
                                    if (attributeNameString == "Include")
                                    {
                                        if (includeValue == string.Empty)
                                            includeValue = stringBuilder.ToString();
                                    }
                                    
                                    ++indexTextSpan;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if ((XmlDecorationKind)lexerOutput.TextSpanList[indexTextSpan + 1].DecorationByte == XmlDecorationKind.AttributeDelimiter)
                            {
                                ++indexTextSpan;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    
                    if (includeValue != string.Empty)
                        relativePathReferenceList.Add(includeValue);
                }
            }
        }
        
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
            var referenceProjectAbsolutePathString = PathHelper.GetAbsoluteFromAbsoluteAndRelative(
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
