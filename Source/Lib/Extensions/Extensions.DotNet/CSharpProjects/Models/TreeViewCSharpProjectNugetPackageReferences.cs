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
        
        List<LightWeightNugetPackageRecord> lightWeightNugetPackageRecords = new();
        
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
            
                if (tagNameOpenString == "PackageReference")
                {
                    var includeValue = string.Empty;
                    var versionValue = string.Empty;
                
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
                                    else if (attributeNameString == "Version")
                                    {
                                        if (versionValue == string.Empty)
                                            versionValue = stringBuilder.ToString();
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
                    
                    if (includeValue != string.Empty && versionValue != string.Empty)
                        lightWeightNugetPackageRecords.Add(new(includeValue, includeValue, versionValue));
                }
            }
        }

        var cSharpProjectAbsolutePathString = Item.CSharpProjectAbsolutePath.Value;

        var newChildList = lightWeightNugetPackageRecords.Select(
            npr => (TreeViewNoType)new TreeViewCSharpProjectNugetPackageReference(
                new(cSharpProjectAbsolutePathString, npr),
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
