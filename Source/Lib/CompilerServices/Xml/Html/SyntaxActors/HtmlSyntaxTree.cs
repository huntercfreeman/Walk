using System.Text;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib;
using Walk.CompilerServices.Xml.Html.SyntaxObjects;
using Walk.CompilerServices.Xml.Html.InjectedLanguage;
using Walk.CompilerServices.Xml.Html.ExtensionMethods;
using Walk.CompilerServices.Xml.Html.Decoration;
using Walk.CompilerServices.Xml.Html.SyntaxEnums;
using Walk.CompilerServices.Xml.Html.Facts;
using Walk.CompilerServices.Xml.Html.SyntaxObjects.Builders;

namespace Walk.CompilerServices.Xml.Html.SyntaxActors;

public static class HtmlSyntaxTree
{
	/// <summary>
	/// Very confusing: `textEditorService` is passed in
	/// in order to permit use of TextEditorService.EditContext_GetText(...)
	/// 
	/// This method is only safe to invoke from a TextEditorEditContext however.
	/// So, if this HtmlSyntaxTree is used from a non-TextEditorEditContext
	/// then the textEditorService will be passed as null to avoid unsafe invocation of the method.
	///
	/// This needs to be rewritten, but the short term benefit of adding this hack
	/// far outweights the cost of then having to rewrite this.
	/// </summary>
    public static HtmlSyntaxUnit ParseText(
    	TextEditorService? textEditorService,
    	StringWalker stringWalker,
        ResourceUri resourceUri,
        string content,
        InjectedLanguageDefinition? injectedLanguageDefinition = null)
    {
    	stringWalker.Initialize(resourceUri, content);

        var rootTagSyntaxBuilder = new TagNodeBuilder
        {
            OpenTagNameSyntax = new TagNameNode(
                new TextEditorTextSpan(
                    0,
                    8,
                    (byte)HtmlDecorationKind.None)),
        };

        var textEditorHtmlDiagnosticBag = new List<TextEditorDiagnostic>();

        rootTagSyntaxBuilder.Children = HtmlSyntaxTreeStateMachine
            .ParseTagChildContent(
            	textEditorService,
                stringWalker,
                textEditorHtmlDiagnosticBag,
                injectedLanguageDefinition);

        var htmlSyntaxUnitBuilder = new HtmlSyntaxUnit.HtmlSyntaxUnitBuilder(
            rootTagSyntaxBuilder.Build(),
            textEditorHtmlDiagnosticBag);

        return htmlSyntaxUnitBuilder.Build();
    }

    public static class HtmlSyntaxTreeStateMachine
    {
        /// <summary>Invocation of this method requires the stringWalker to have <see cref="StringWalker.PeekCharacter" /> of 0 be equal to <see cref="HtmlFacts.OPEN_TAG_BEGINNING" /></summary>
        public static IHtmlSyntaxNode ParseTag(
        	TextEditorService? textEditorService,
            StringWalker stringWalker,
            List<TextEditorDiagnostic> diagnosticList,
            InjectedLanguageDefinition? injectedLanguageDefinition)
        {
            if (stringWalker.PeekForSubstring(
                    HtmlFacts.COMMENT_TAG_BEGINNING))
            {
                return ParseComment(
                	textEditorService,
                    stringWalker,
                    diagnosticList,
                    injectedLanguageDefinition);
            }

            var startingPositionIndex = stringWalker.PositionIndex;

            var tagBuilder = new TagNodeBuilder();

            // HtmlFacts.TAG_OPENING_CHARACTER
            _ = stringWalker.ReadCharacter();

            // Example: <!DOCTYPE html>
            if (stringWalker.PeekCharacter(0) == HtmlFacts.SPECIAL_HTML_TAG)
            {
                // HtmlFacts.SPECIAL_HTML_TAG_CHARACTER
                stringWalker.ReadCharacter();

                tagBuilder.HasSpecialHtmlCharacter = true;
            }

            tagBuilder.OpenTagNameSyntax = ParseTagName(
            	textEditorService,
                stringWalker,
                diagnosticList,
                injectedLanguageDefinition);

            // Get all html attributes break when see End Of File or closing of the tag
            while (true)
            {
                // Skip Whitespace
                while (!stringWalker.IsEof)
                {
                    if (WhitespaceFacts.ALL_LIST.Contains(stringWalker.CurrentCharacter))
                        _ = stringWalker.ReadCharacter();
                    else
                        break;
                }

                // End Of File is unexpected at this point so report a diagnostic.
                if (stringWalker.CurrentCharacter == ParserFacts.END_OF_FILE)
                {
                    /*diagnosticBag.ReportEndOfFileUnexpected(
                        new TextEditorTextSpan(
                            startingPositionIndex,
                            stringWalker.PositionIndex,
                            (byte)HtmlDecorationKind.Error,
                            stringWalker.ResourceUri,
                            stringWalker.SourceText));*/

                    return tagBuilder.Build();
                }

                if (stringWalker.PeekForSubstring(HtmlFacts.OPEN_TAG_WITH_CHILD_CONTENT_ENDING))
                {
                    // Ending of opening tag
                    tagBuilder.HtmlSyntaxKind = HtmlSyntaxKind.TagOpeningNode;

                    // Skip the '>' character to set stringWalker at the first character of the child content
                    _ = stringWalker.ReadCharacter();

                    tagBuilder.Children = ParseTagChildContent(
                		textEditorService,
                        stringWalker,
                        diagnosticList,
                        injectedLanguageDefinition);

                    // TODO: check that the closing tag name matches the opening tag
                }
                else if (stringWalker.PeekForSubstring(HtmlFacts.OPEN_TAG_SELF_CLOSING_ENDING))
                {
                    stringWalker.SkipRange(
                            HtmlFacts.OPEN_TAG_SELF_CLOSING_ENDING
                                .Length);

                    // Ending of self-closing tag
                    tagBuilder.HtmlSyntaxKind = HtmlSyntaxKind.TagSelfClosingNode;

                    return tagBuilder.Build();
                }
                else if (stringWalker.PeekForSubstring(HtmlFacts.CLOSE_TAG_WITH_CHILD_CONTENT_BEGINNING))
                {
                    tagBuilder.HtmlSyntaxKind = HtmlSyntaxKind.TagClosingNode;

                    stringWalker.SkipRange(
                        HtmlFacts.CLOSE_TAG_WITH_CHILD_CONTENT_BEGINNING
                            .Length);

                    var closeTagNameStartingPositionIndex = stringWalker.PositionIndex;

                    var closeTagNameBuilder = new StringBuilder();

                    while (!stringWalker.IsEof)
                    {
                        if (stringWalker.PeekForSubstring(HtmlFacts.CLOSE_TAG_WITH_CHILD_CONTENT_ENDING))
                        {
                            tagBuilder.CloseTagNameSyntax = new TagNameNode(
                                new(closeTagNameStartingPositionIndex, stringWalker.PositionIndex, (byte)HtmlDecorationKind.TagName));

                            stringWalker.SkipRange(HtmlFacts.CLOSE_TAG_WITH_CHILD_CONTENT_ENDING.Length);

                            break;
                        }

                        closeTagNameBuilder.Append(stringWalker.CurrentCharacter);

                        _ = stringWalker.ReadCharacter();
                    }

                    if (tagBuilder.CloseTagNameSyntax is null)
                    {
                        // TODO: Not sure if this can happen but I am getting a warning about this and aim to get to this when I find time.
                        throw new NotImplementedException();
                    }

                    if (tagBuilder.OpenTagNameSyntax.TextEditorTextSpan.GetText(stringWalker.SourceText, textEditorService) != tagBuilder.CloseTagNameSyntax.TextEditorTextSpan.GetText(stringWalker.SourceText, textEditorService))
                    {
                        /*diagnosticBag.ReportOpenTagWithUnMatchedCloseTag(
                            tagBuilder.OpenTagNameSyntax.TextEditorTextSpan.GetText(),
                            tagBuilder.CloseTagNameSyntax.TextEditorTextSpan.GetText(),
                            new TextEditorTextSpan(
                                closeTagNameStartingPositionIndex,
                                stringWalker.PositionIndex,
                                (byte)HtmlDecorationKind.Error,
                                stringWalker.ResourceUri,
                                stringWalker.SourceText));*/
                    }

                    return tagBuilder.Build();
                }
                else
                {
                    var attributeSyntax = ParseAttribute(textEditorService, stringWalker, diagnosticList, injectedLanguageDefinition);
                    tagBuilder.AttributeSyntaxes.Add(attributeSyntax);
                }
            }
        }

        /// <summary>Invocation of this method requires the stringWalker to have <see cref="StringWalker.PeekCharacter" /> of 0 be equal to the first character that is part of the tag's name</summary>
        public static TagNameNode ParseTagName(
        	TextEditorService? textEditorService,
            StringWalker stringWalker,
            List<TextEditorDiagnostic> diagnosticList,
            InjectedLanguageDefinition? injectedLanguageDefinition)
        {
            var startingPositionIndex = stringWalker.PositionIndex;

            var tagNameBuilder = new StringBuilder();

            while (!stringWalker.IsEof)
            {
                if (stringWalker.PeekForSubstringRange(HtmlFacts.TAG_NAME_STOP_DELIMITERS, out var matchedOn))
                    break;

                tagNameBuilder.Append(stringWalker.CurrentCharacter);

                _ = stringWalker.ReadCharacter();
            }

            var tagName = tagNameBuilder.ToString();

            if (tagNameBuilder.Length == 0)
            {
                if (stringWalker.CurrentCharacter == ParserFacts.END_OF_FILE)
                {
                    /*diagnosticBag.ReportEndOfFileUnexpected(
                        new TextEditorTextSpan(
                            startingPositionIndex,
                            stringWalker.PositionIndex,
                            (byte)HtmlDecorationKind.Error,
                            stringWalker.ResourceUri,
                            stringWalker.SourceText));*/
                }
                else
                {
                    // Report a diagnostic for the missing 'tag name'
                    /*diagnosticBag.ReportTagNameMissing(
                        new TextEditorTextSpan(
                            startingPositionIndex,
                            stringWalker.PositionIndex,
                            (byte)HtmlDecorationKind.Error,
                            stringWalker.ResourceUri,
                            stringWalker.SourceText));*/

                    // Fabricate a value for the string variable: 'tagName' so the rest of the file can still be parsed.
                    tagName =
                        $"__ReportTagNameMissing__";;//$"__{nameof(diagnosticBag.ReportTagNameMissing)}__";
                }
            }

			var tagNameTextSpan = new TextEditorTextSpan(
	            startingPositionIndex,
	            stringWalker.PositionIndex,
	            (byte)HtmlDecorationKind.TagName);

            injectedLanguageDefinition?.ParseTagName?.Invoke(
                stringWalker,
                diagnosticList,
                injectedLanguageDefinition,
                tagNameTextSpan);

            return new TagNameNode(tagNameTextSpan);
        }

        public static List<IHtmlSyntax> ParseTagChildContent(
        	TextEditorService? textEditorService,
            StringWalker stringWalker,
            List<TextEditorDiagnostic> diagnosticList,
            InjectedLanguageDefinition? injectedLanguageDefinition)
        {
            var startingPositionIndex = stringWalker.PositionIndex;

            List<IHtmlSyntax> htmlSyntaxes = new();

            int? textNodeStartingPositionIndex = null;

            // Make a TagTextSyntax - HTML TextNode if there was anything in the current builder
            void AddTextNode()
            {
                if (textNodeStartingPositionIndex is null)
                    return;

				var isWhiteSpace = true;

				for (int i = textNodeStartingPositionIndex.Value; i < stringWalker.PositionIndex; i++)
				{
					if (!char.IsWhiteSpace(stringWalker.SourceText[i]))
					{
						isWhiteSpace = false;
						break;
					}
				}
				
				// TODO: Allow an option to include whitespace.
				if (!isWhiteSpace)
				{
	                var tagTextSyntax = new TextNode(
	                    new TextEditorTextSpan(
	                        textNodeStartingPositionIndex.Value,
	                        stringWalker.PositionIndex,
	                        (byte)GenericDecorationKind.None));
	
	                htmlSyntaxes.Add(tagTextSyntax);
                }
                textNodeStartingPositionIndex = null;
            }

            while (!stringWalker.IsEof)
            {
                if (stringWalker.PeekForSubstring(HtmlFacts.CLOSE_TAG_WITH_CHILD_CONTENT_BEGINNING))
                    break;

                if (stringWalker.CurrentCharacter == HtmlFacts.OPEN_TAG_BEGINNING)
                {
                    // If there is text in textNodeBuilder add a new TextNode to the List of TagSyntax
                    AddTextNode();

                    if (stringWalker.PeekForSubstring(HtmlFacts.COMMENT_TAG_BEGINNING))
                    {
                        var node = ParseComment(textEditorService, stringWalker, diagnosticList, injectedLanguageDefinition);
                        htmlSyntaxes.Add(node);
                    }
                    else
                    {
                        var node = ParseTag(textEditorService, stringWalker, diagnosticList, injectedLanguageDefinition);
                        htmlSyntaxes.Add(node);
                    }

                    continue;
                }

                if (injectedLanguageDefinition is not null && stringWalker.AtInjectedLanguageCodeBlockTag(injectedLanguageDefinition))
                {
                    // If there is text in textNodeBuilder add a new TextNode to the List of TagSyntax
                    AddTextNode();

                    var nodeBag = ParseInjectedLanguageCodeBlock(textEditorService, stringWalker, diagnosticList, injectedLanguageDefinition);
                    htmlSyntaxes.AddRange(nodeBag);

                    continue;
                }

                textNodeStartingPositionIndex ??= stringWalker.PositionIndex;

                _ = stringWalker.ReadCharacter();
            }

            /*if (stringWalker.CurrentCharacter == ParserFacts.END_OF_FILE)
                diagnosticBag.ReportEndOfFileUnexpected(new(startingPositionIndex, stringWalker, (byte)HtmlDecorationKind.Error));*/

            // If there is text in textNodeBuilder add a new TextNode to the List of TagSyntax
            AddTextNode();

            return htmlSyntaxes;
        }

        public static List<IHtmlSyntaxNode> ParseInjectedLanguageCodeBlock(
        	TextEditorService? textEditorService,
            StringWalker stringWalker,
            List<TextEditorDiagnostic> diagnosticList,
            InjectedLanguageDefinition injectedLanguageDefinition)
        {
            var injectedLanguageFragmentSyntaxes = new List<IHtmlSyntaxNode>();

            var injectedLanguageFragmentSyntaxStartingPositionIndex = stringWalker.PositionIndex;

            // Track text span of the "@" sign (example in .razor files)
            injectedLanguageFragmentSyntaxes.Add(
                new InjectedLanguageFragmentNode(
                    Array.Empty<IHtmlSyntax>(),
                    new TextEditorTextSpan(
                        injectedLanguageFragmentSyntaxStartingPositionIndex,
                        stringWalker.PositionIndex + 1,
                        (byte)HtmlDecorationKind.InjectedLanguageFragment)));

            injectedLanguageFragmentSyntaxes.AddRange(
                injectedLanguageDefinition.ParseInjectedLanguageFunc
                    .Invoke(
                        stringWalker,
                        diagnosticList,
                        injectedLanguageDefinition));

            return injectedLanguageFragmentSyntaxes;
        }

        public static AttributeNode ParseAttribute(
        	TextEditorService? textEditorService,
            StringWalker stringWalker,
            List<TextEditorDiagnostic> diagnosticList,
            InjectedLanguageDefinition? injectedLanguageDefinition)
        {
            var attributeNameSyntax = ParseAttributeName(
            	textEditorService,
                stringWalker,
                diagnosticList,
                injectedLanguageDefinition);

            _ = TryReadAttributeValue(
            		textEditorService,
                    stringWalker,
                    diagnosticList,
                    injectedLanguageDefinition,
                    out var attributeValueSyntax);

            return new AttributeNode(
                attributeNameSyntax,
                attributeValueSyntax,
                new(
			        attributeNameSyntax.TextEditorTextSpan.StartInclusiveIndex,
			        attributeValueSyntax.TextEditorTextSpan.EndExclusiveIndex,
			        (byte)GenericDecorationKind.None));
        }

        /// <summary>currentCharacterIn:<br/> -Any character that can start an attribute name<br/> currentCharacterOut:<br/> -<see cref="WhitespaceFacts.ALL_LIST"/> (whitespace)<br/> -<see cref="HtmlFacts.SEPARATOR_FOR_ATTRIBUTE_NAME_AND_ATTRIBUTE_VALUE"/><br/> -<see cref="HtmlFacts.OPEN_TAG_ENDING_OPTIONS"/></summary>
        public static AttributeNameNode ParseAttributeName(
        	TextEditorService? textEditorService,
            StringWalker stringWalker,
            List<TextEditorDiagnostic> diagnosticList,
            InjectedLanguageDefinition? injectedLanguageDefinition)
        {
            // When ParseAttributeName is invoked the PositionIndex is always 1 character too far
            _ = stringWalker.BacktrackCharacter();

            var startingPositionIndex = stringWalker.PositionIndex;

            bool firstLoop = true;

            while (!stringWalker.IsEof)
            {
                _ = stringWalker.ReadCharacter();

                // Try to read for injected language
                if (firstLoop)
                {
                    firstLoop = false;

                    if (injectedLanguageDefinition?.ParseAttributeName is not null &&
                        stringWalker.AtInjectedLanguageCodeBlockTag(injectedLanguageDefinition))
                    {
                        return injectedLanguageDefinition.ParseAttributeName
                            .Invoke(
                                stringWalker,
                                diagnosticList,
                                injectedLanguageDefinition);
                    }
                }

                if (WhitespaceFacts.ALL_LIST.Contains(stringWalker.CurrentCharacter) ||
                    HtmlFacts.SEPARATOR_FOR_ATTRIBUTE_NAME_AND_ATTRIBUTE_VALUE == stringWalker.CurrentCharacter ||
                    stringWalker.PeekForSubstringRange(HtmlFacts.OPEN_TAG_ENDING_OPTIONS, out var matchedOn))
                {
                    break;
                }
            }
            
            var attributeNameTextSpan = new TextEditorTextSpan(
                startingPositionIndex,
                stringWalker.PositionIndex,
                (byte)HtmlDecorationKind.AttributeName);
            
            return new AttributeNameNode(attributeNameTextSpan);
        }

        /// <summary>Returns placeholder match attribute value if fails to read an attribute value<br/> <br/> currentCharacterIn:<br/> -<see cref="WhitespaceFacts.ALL_LIST"/> (whitespace)<br/> -<see cref="HtmlFacts.SEPARATOR_FOR_ATTRIBUTE_NAME_AND_ATTRIBUTE_VALUE"/><br/> -<see cref="HtmlFacts.OPEN_TAG_ENDING_OPTIONS"/><br/> currentCharacterOut:<br/> -<see cref="HtmlFacts.ATTRIBUTE_VALUE_ENDING"/><br/> -<see cref="HtmlFacts.OPEN_TAG_ENDING_OPTIONS"/></summary>
        private static bool TryReadAttributeValue(
        	TextEditorService? textEditorService,
            StringWalker stringWalker,
            List<TextEditorDiagnostic> diagnosticList,
            InjectedLanguageDefinition? injectedLanguageDefinition,
            out AttributeValueNode attributeValueSyntax)
        {
            if (WhitespaceFacts.ALL_LIST.Contains(stringWalker.CurrentCharacter))
            {
                // Move to the first non-whitespace
                while (!stringWalker.IsEof)
                {
                    _ = stringWalker.ReadCharacter();

                    if (!WhitespaceFacts.ALL_LIST.Contains(stringWalker.CurrentCharacter))
                        break;
                }
            }

            if (HtmlFacts.SEPARATOR_FOR_ATTRIBUTE_NAME_AND_ATTRIBUTE_VALUE == stringWalker.CurrentCharacter)
            {
                attributeValueSyntax = ParseAttributeValue(
                	textEditorService,
                    stringWalker,
                    diagnosticList,
                    injectedLanguageDefinition);

                return true;
            }

			// Set out variable as a 'matched attribute value' so there aren't any cascading error diagnostics due to having expected an attribute value.
			var attributeValueTextSpan = new TextEditorTextSpan(
	            0,
	            0,
	            (byte)HtmlDecorationKind.AttributeValue);
            attributeValueSyntax = new AttributeValueNode(attributeValueTextSpan);

            return false;
        }

        /// <summary> currentCharacterIn:<br/> -<see cref="HtmlFacts.SEPARATOR_FOR_ATTRIBUTE_NAME_AND_ATTRIBUTE_VALUE"/><br/> currentCharacterOut:<br/> -<see cref="HtmlFacts.ATTRIBUTE_VALUE_ENDING"/></summary>
        public static AttributeValueNode ParseAttributeValue(
        	TextEditorService? textEditorService,
            StringWalker stringWalker,
            List<TextEditorDiagnostic> diagnosticList,
            InjectedLanguageDefinition? injectedLanguageDefinition)
        {
            // Suppress these unused parameters because all 'Parse...()' methods should take them for consistency.
            _ = diagnosticList;
            _ = injectedLanguageDefinition;

            var startingPositionIndex = stringWalker.PositionIndex;

            // Move to the first non-whitespace which follows the HtmlFacts.SEPARATOR_FOR_ATTRIBUTE_NAME_AND_ATTRIBUTE_VALUE
            while (!stringWalker.IsEof)
            {
                _ = stringWalker.ReadCharacter();

                if (!WhitespaceFacts.ALL_LIST.Contains(stringWalker.CurrentCharacter))
                    break;
            }

            var foundOpenTagEnding = stringWalker.PeekForSubstringRange(
                HtmlFacts.OPEN_TAG_ENDING_OPTIONS,
                out _);

            if (!foundOpenTagEnding)
            {
                var beganWithAttributeValueStarting =
                    HtmlFacts.ATTRIBUTE_VALUE_STARTING == stringWalker.CurrentCharacter;

                while (!stringWalker.IsEof)
                {
                    // TODO: (2023-05-31) This is logic for syntax highlighting a blazor event handler such as @onclick for example. In specific it would be the method group provided that this syntax highlights.
                    //// Try to read for injected language
                    //if (firstLoop)
                    //{
                    //    firstLoop = false;

                    //    if (injectedLanguageDefinition?.ParseAttributeValue is not null &&
                    //        stringWalker.CheckForInjectedLanguageCodeBlockTag(injectedLanguageDefinition))
                    //    {
                    //        return injectedLanguageDefinition.ParseAttributeValue
                    //            .Invoke(
                    //                stringWalker,
                    //                textEditorHtmlDiagnosticBag,
                    //                injectedLanguageDefinition);
                    //    }
                    //}

                    _ = stringWalker.ReadCharacter();

                    if (!beganWithAttributeValueStarting &&
                        WhitespaceFacts.ALL_LIST.Contains(stringWalker.CurrentCharacter))
                    {
                        break;
                    }

                    if (stringWalker.PeekForSubstringRange(
                            HtmlFacts.OPEN_TAG_ENDING_OPTIONS,
                            out _))
                    {
                        foundOpenTagEnding = true;
                        break;
                    }

                    if (HtmlFacts.ATTRIBUTE_VALUE_ENDING == stringWalker.CurrentCharacter)
                        break;
                }
            }

            var endingIndexExclusive = stringWalker.PositionIndex;

            if (!foundOpenTagEnding)
                endingIndexExclusive++;

			/*TextEditorTextSpan attributeValueTextSpan;

			if (textEditorService is null)
			{
				attributeValueTextSpan = new TextEditorTextSpan(
	                startingPositionIndex,
	                endingIndexExclusive,
	                (byte)HtmlDecorationKind.AttributeValue,
	                stringWalker.ResourceUri,
	                stringWalker.SourceText);
			}
			else
			{
				// TODO: This is very questionable.
				// Attribute values might be too unique to be a good idea to use EditContext_GetText with.
				
				attributeValueTextSpan = new TextEditorTextSpan(
	                startingPositionIndex,
	                endingIndexExclusive,
	                (byte)HtmlDecorationKind.AttributeValue,
	                stringWalker.ResourceUri,
	                stringWalker.SourceText,
	                textEditorService.EditContext_GetText(
	                	stringWalker.SourceText.AsSpan(startingPositionIndex, stringWalker.PositionIndex - startingPositionIndex)));
			}*/
			
			// Not currently worth it using `EditContext_GetText` for an attribute value, too distinct.
			var attributeValueTextSpan = new TextEditorTextSpan(
	            startingPositionIndex,
	            endingIndexExclusive,
	            (byte)HtmlDecorationKind.AttributeValue);

            return new AttributeValueNode(attributeValueTextSpan);
        }

        public static CommentNode ParseComment(
        	TextEditorService? textEditorService,
            StringWalker stringWalker,
            List<TextEditorDiagnostic> diagnosticList,
            InjectedLanguageDefinition? injectedLanguageDefinition)
        {
            // Suppress these unused parameters because all 'Parse...()' methods should take them for consistency.
            _ = diagnosticList;
            _ = injectedLanguageDefinition;

            var startingPositionIndex = stringWalker.PositionIndex;

            while (!stringWalker.IsEof)
            {
                _ = stringWalker.ReadCharacter();

                if (stringWalker.PeekForSubstring(HtmlFacts.COMMENT_TAG_ENDING))
                    break;
            }

            // Skip the remaining characters in the comment tag ending string
            stringWalker.SkipRange(HtmlFacts.COMMENT_TAG_ENDING.Length - 1);

            var commentTagTextSpan = new TextEditorTextSpan(
                startingPositionIndex,
                stringWalker.PositionIndex + 1,
                (byte)HtmlDecorationKind.Comment);

            return new CommentNode(commentTagTextSpan);
        }
    }
}