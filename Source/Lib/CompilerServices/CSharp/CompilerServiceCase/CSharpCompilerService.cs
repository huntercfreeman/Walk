using System.Text;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Autocompletes.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.ComponentRenderers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.TextEditors.Displays.Internals;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Events.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models.Defaults;
using Walk.Extensions.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Displays;
using Walk.CompilerServices.CSharp.BinderCase;
using Walk.CompilerServices.CSharp.LexerCase;
using Walk.CompilerServices.CSharp.ParserCase;

namespace Walk.CompilerServices.CSharp.CompilerServiceCase;

public sealed class CSharpCompilerService : IExtendedCompilerService
{
	// <summary>Public because the RazorCompilerService uses it.</summary>
    public readonly CSharpBinder __CSharpBinder;
    
    private readonly Dictionary<ResourceUri, CSharpResource> _resourceMap = new();
    private readonly object _resourceMapLock = new();
    private readonly HashSet<string> _collapsePointUsedIdentifierHashSet = new();
    private readonly StringBuilder _getAutocompleteMenuStringBuilder = new();
    
    // Service dependencies
    private readonly TextEditorService _textEditorService;
    private readonly CommonUtilityService _commonUtilityService;
    
    public CSharpCompilerService(TextEditorService textEditorService, CommonUtilityService commonUtilityService)
    {
    	_textEditorService = textEditorService;
    	_commonUtilityService = commonUtilityService;
    	
    	__CSharpBinder = new(_textEditorService);
    }

    public event Action? ResourceRegistered;
    public event Action? ResourceParsed;
    public event Action? ResourceDisposed;

    public IReadOnlyList<ICompilerServiceResource> CompilerServiceResources { get; }
    
    public IReadOnlyDictionary<string, TypeDefinitionNode> AllTypeDefinitions { get; }
    
    /// <summary>
    /// This overrides the default Blazor component: <see cref="Walk.TextEditor.RazorLib.TextEditors.Displays.Internals.SymbolDisplay"/>.
    /// It is shown when hovering with the cursor over a <see cref="Walk.TextEditor.RazorLib.CompilerServices.Syntax.Symbols.ISymbol"/>
    /// (as well other actions will show it).
    ///
    /// If only a small change is necessary, It is recommended to replicate <see cref="Walk.TextEditor.RazorLib.TextEditors.Displays.Internals.SymbolDisplay"/>
    /// but with a component of your own name.
    ///
    /// There is a switch statement that renders content based on the symbol's SyntaxKind.
    ///
    /// So, if the small change is for a particular SyntaxKind, copy over the entire switch statement,
    /// and change that case in particular.
    ///
    /// There are optimizations in the SymbolDisplay's codebehind to stop it from re-rendering
    /// unnecessarily. So check the codebehind and copy over the code from there too if desired (this is recommended).
    ///
    /// The "all in" approach to overriding the default 'SymbolRenderer' was decided on over
    /// a more fine tuned override of each individual case in the UI's switch statement.
    ///
    /// This was because it is firstly believed that the properties necessary to customize
    /// the SymbolRenderer would massively increase.
    /// 
    /// And secondly because it is believed that the Nodes shouldn't even be shared
    /// amongst the TextEditor and the ICompilerService.
    ///
    /// That is to say, it feels quite odd that a Node and SyntaxKind enum member needs
    /// to be defined by the text editor, rather than the ICompilerService doing it.
    ///
    /// The solution to this isn't yet known but it is always in the back of the mind
    /// while working on the text editor.
    /// </summary>
    public Type? SymbolRendererType { get; }
    public Type? DiagnosticRendererType { get; }

    public void RegisterResource(ResourceUri resourceUri, bool shouldTriggerResourceWasModified)
    {
    	lock (_resourceMapLock)
        {
            if (_resourceMap.ContainsKey(resourceUri))
                return;

            _resourceMap.Add(resourceUri, new CSharpResource(resourceUri, this));
        }

		if (shouldTriggerResourceWasModified)
	        ResourceWasModified(resourceUri, Array.Empty<TextEditorTextSpan>());
	        
        ResourceRegistered?.Invoke();
    }
    
    public void DisposeResource(ResourceUri resourceUri)
    {
    	lock (_resourceMapLock)
        {
            _resourceMap.Remove(resourceUri);
        }

        ResourceDisposed?.Invoke();
    }

    public void ResourceWasModified(ResourceUri resourceUri, IReadOnlyList<TextEditorTextSpan> editTextSpansList)
    {
    	_textEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
			var modelModifier = editContext.GetModelModifier(resourceUri);

			if (modelModifier is null)
				return ValueTask.CompletedTask;

			return ParseAsync(editContext, modelModifier, shouldApplySyntaxHighlighting: true);
        });
    }

    public ICompilerServiceResource? GetResource(ResourceUri resourceUri)
    {
    	var model = _textEditorService.ModelApi.GetOrDefault(resourceUri);

        lock (_resourceMapLock)
        {
            if (!_resourceMap.ContainsKey(resourceUri))
                return null;

            return _resourceMap[resourceUri];
        }
    }
    
    public MenuRecord GetContextMenu(TextEditorVirtualizationResult virtualizationResult, ContextMenu contextMenu)
	{
		return contextMenu.GetDefaultMenuRecord();
	}
	
	private MenuRecord? GetAutocompleteMenuPart(TextEditorVirtualizationResult virtualizationResult, AutocompleteMenu autocompleteMenu, int positionIndex)
	{
		var character = '\0';
		
		var foundMemberAccessToken = false;
		var memberAccessTokenPositionIndex = -1;
		
		var isParsingIdentifier = false;
		var isParsingNumber = false;
		
		// banana.Price
		//
		// 'banana.' is  the context
		// 'banana' is the operating word
		var operatingWordEndExclusiveIndex = -1;
		
		var filteringWordEndExclusiveIndex = -1;
		var filteringWordStartInclusiveIndex = -1;
		
		// '|' indicates cursor position:
		//
		// "apple banana.Pri|ce"
		// "apple.banana Pri|ce"
		var notParsingButTouchingletterOrDigit = false;
		var letterOrDigitIntoNonMatchingCharacterKindOccurred = false;
		
		var i = positionIndex - 1;
		
		for (; i >= 0; i--)
		{
		    character = virtualizationResult.Model.GetCharacter(i);
		    
		    switch (character)
		    {
		        /* Lowercase Letters */
		        case 'a':
		        case 'b':
		        case 'c':
		        case 'd':
		        case 'e':
		        case 'f':
		        case 'g':
		        case 'h':
		        case 'i':
		        case 'j':
		        case 'k':
		        case 'l':
		        case 'm':
		        case 'n':
		        case 'o':
		        case 'p':
		        case 'q':
		        case 'r':
		        case 's':
		        case 't':
		        case 'u':
		        case 'v':
		        case 'w':
		        case 'x':
		        case 'y':
		        case 'z':
		        /* Uppercase Letters */
		        case 'A':
		        case 'B':
		        case 'C':
		        case 'D':
		        case 'E':
		        case 'F':
		        case 'G':
		        case 'H':
		        case 'I':
		        case 'J':
		        case 'K':
		        case 'L':
		        case 'M':
		        case 'N':
		        case 'O':
		        case 'P':
		        case 'Q':
		        case 'R':
		        case 'S':
		        case 'T':
		        case 'U':
		        case 'V':
		        case 'W':
		        case 'X':
		        case 'Y':
		        case 'Z':
		        /* Underscore */
		        case '_':
		            if (foundMemberAccessToken)
		            {
		                isParsingIdentifier = true;
		                
		                if (operatingWordEndExclusiveIndex == -1)
		                	operatingWordEndExclusiveIndex = i;
		            }
		            else
		            {
		            	if (!notParsingButTouchingletterOrDigit)
		            	{
		            		notParsingButTouchingletterOrDigit = true;
		            		
		            		if (filteringWordEndExclusiveIndex == -1)
		                		filteringWordEndExclusiveIndex = i + 1;
		            	}
		            }
		            break;
		        case '0':
		        case '1':
		        case '2':
		        case '3':
		        case '4':
		        case '5':
		        case '6':
		        case '7':
		        case '8':
		        case '9':
		            if (foundMemberAccessToken)
		            {
		                if (!isParsingIdentifier)
		                {
		                    isParsingNumber = true;
		                    
		                    if (operatingWordEndExclusiveIndex == -1)
			                	operatingWordEndExclusiveIndex = i;
		                }
		            }
		            else
		            {
		                if (!notParsingButTouchingletterOrDigit)
		            	{
		            		notParsingButTouchingletterOrDigit = true;
		            		
		            		if (filteringWordEndExclusiveIndex == -1)
		                		filteringWordEndExclusiveIndex = i + 1;
		            	}
		            }
		            break;
		        case '\r':
		        case '\n':
		        case '\t':
		        case ' ':
		            if (isParsingIdentifier || isParsingNumber)
		                goto exitOuterForLoop;
		
		            if (notParsingButTouchingletterOrDigit)
		            {
		                if (letterOrDigitIntoNonMatchingCharacterKindOccurred)
		                {
		                    goto exitOuterForLoop;
		                }
		                else
		                {
		                    letterOrDigitIntoNonMatchingCharacterKindOccurred = true;
		                }
		            }
		            break;
		        case '.':
		            if (!foundMemberAccessToken)
		            {
		            	if (notParsingButTouchingletterOrDigit && filteringWordStartInclusiveIndex == -1)
	                    	filteringWordStartInclusiveIndex = i + 1;
		            
		                foundMemberAccessToken = true;
		                notParsingButTouchingletterOrDigit = false;
		                letterOrDigitIntoNonMatchingCharacterKindOccurred = false;
		                
		                if (i > 0)
		                {
		                	var innerCharacter = virtualizationResult.Model.GetCharacter(i - 1);
		                	
		                	if (innerCharacter == '?' || innerCharacter == '!')
		                		i--;
		                }
		            }
		            else
		            {
		            	goto exitOuterForLoop;
		            }
		            break;
		        default:
		            goto exitOuterForLoop;
		    }
		}
		
		exitOuterForLoop:
		
		// Invalidate the parsed identifier if it starts with a number.
		if (isParsingIdentifier)
		{
		    switch (character)
		    {
		        case '0':
		        case '1':
		        case '2':
		        case '3':
		        case '4':
		        case '5':
		        case '6':
		        case '7':
		        case '8':
		        case '9':
		            isParsingIdentifier = false;
		            break;
		    }
		}
		
		var filteringWord = string.Empty;
		
		if (filteringWordStartInclusiveIndex != -1 && filteringWordEndExclusiveIndex != -1)
		{
			var textSpan = new TextEditorTextSpan(
				filteringWordStartInclusiveIndex,
				filteringWordEndExclusiveIndex,
				DecorationByte: 0,
				virtualizationResult.Model.PersistentState.ResourceUri,
				virtualizationResult.Model.GetAllText());
				
			filteringWord = textSpan.Text;
		}
			
		if (foundMemberAccessToken && operatingWordEndExclusiveIndex != -1)
		{
			var autocompleteEntryList = new List<AutocompleteEntry>();
			
			var operatingWordAmongPositionIndex = operatingWordEndExclusiveIndex - 1;
       	
			if (operatingWordAmongPositionIndex < 0)
				operatingWordAmongPositionIndex = 0;
       
	        var foundMatch = false;
	        
	        var resource = GetResource(virtualizationResult.Model.PersistentState.ResourceUri);
	        var compilationUnitLocal = (CSharpCompilationUnit)resource.CompilationUnit;
	        
	        var symbols = compilationUnitLocal.SymbolList;
	        var diagnostics = compilationUnitLocal.DiagnosticList;
	        
	        Symbol foundSymbol = default;
	
	        if (!foundMatch && symbols.Count != 0)
	        {
	            foreach (var symbol in symbols)
	            {
	                if (operatingWordAmongPositionIndex >= symbol.TextSpan.StartInclusiveIndex &&
	                    operatingWordAmongPositionIndex < symbol.TextSpan.EndExclusiveIndex)
	                {
	                    foundMatch = true;
	                    foundSymbol = symbol;
	                }
	            }
	        }
	        
	        if (foundMatch)
	        {
	        	var textEditorModel = _textEditorService.ModelApi.GetOrDefault(foundSymbol.TextSpan.ResourceUri);
		    	var extendedCompilerService = (IExtendedCompilerService)textEditorModel.PersistentState.CompilerService;
		    	var compilerServiceResource = extendedCompilerService.GetResource(textEditorModel.PersistentState.ResourceUri);
		
		    	var definitionNode = extendedCompilerService.GetDefinitionNode(foundSymbol.TextSpan, compilerServiceResource, foundSymbol);
		    	
		    	if (definitionNode is not null)
		    	{
		    	    if (definitionNode.SyntaxKind == SyntaxKind.NamespaceClauseNode)
		    	    {
		    	        var namespaceClauseNode = (NamespaceClauseNode)definitionNode;
		    	    
		    	        NamespacePrefixNode? namespacePrefixNode;
		    	        
		    	        if (namespaceClauseNode.NamespacePrefixNode is not null)
		    	        {
		    	            namespacePrefixNode = namespaceClauseNode.NamespacePrefixNode;
		    	        }
		    	        else
		    	        {
		    	            _ = __CSharpBinder.NamespacePrefixTree.__Root.Children.TryGetValue(
                    		    foundSymbol.TextSpan.Text, // This is the same value as the definition's TextSpan.
                    		    out namespacePrefixNode);
		    	        }

                        if (namespaceClauseNode is not null)
                		{
                		    foreach (var kvp in namespacePrefixNode.Children.Where(kvp => kvp.Key.Contains(filteringWord)).Take(5))
                		    {
        						autocompleteEntryList.Add(new AutocompleteEntry(
    								kvp.Key,
    				                AutocompleteEntryKind.Namespace,
    				                () => MemberAutocomplete(kvp.Key, virtualizationResult.Model.PersistentState.ResourceUri, virtualizationResult.ViewModel.PersistentState.ViewModelKey)));
                		    }
                		    
                		    if (__CSharpBinder.NamespaceGroupMap.TryGetValue(foundSymbol.TextSpan.Text, out var namespaceGroup))
                		    {
                		        foreach (var typeDefinitionNode in namespaceGroup.GetTopLevelTypeDefinitionNodes().Where(x => x.TypeIdentifierToken.TextSpan.Text.Contains(filteringWord)).Take(5))
                		        {
	        						autocompleteEntryList.Add(new AutocompleteEntry(
										typeDefinitionNode.TypeIdentifierToken.TextSpan.Text,
						                AutocompleteEntryKind.Type,
						                () => MemberAutocomplete(typeDefinitionNode.TypeIdentifierToken.TextSpan.Text, virtualizationResult.Model.PersistentState.ResourceUri, virtualizationResult.ViewModel.PersistentState.ViewModelKey)));
                		        }
                		    }
                		    
                		    return new MenuRecord(
                				autocompleteEntryList.Select(entry => new MenuOptionRecord(
                					    entry.DisplayName,
                					    MenuOptionKind.Other,
                					    () => entry.SideEffectFunc?.Invoke() ?? Task.CompletedTask,
                					    widgetParameterMap: new Dictionary<string, object?>
                					    {
                					        {
                					            nameof(AutocompleteEntry),
                					            entry
                					        }
                					    }))
                					.ToList());
                		}
		    	        
		    	        return null;
		    	    }
		    	
		    		TypeReference typeReference = default;
		    		
					if (definitionNode.SyntaxKind == SyntaxKind.VariableReferenceNode)
					{
						var variableReferenceNode = (VariableReferenceNode)definitionNode;
						if (variableReferenceNode.VariableDeclarationNode is not null)
							typeReference = variableReferenceNode.VariableDeclarationNode.TypeReference;
					}
					else if (definitionNode.SyntaxKind == SyntaxKind.VariableDeclarationNode)
					{
						var variableDeclarationNode = (VariableDeclarationNode)definitionNode;
						typeReference = variableDeclarationNode.TypeReference;
					}
					else if (definitionNode.SyntaxKind == SyntaxKind.FunctionInvocationNode)
					{
						typeReference = ((FunctionInvocationNode)definitionNode).ResultTypeReference;
					}
					else if (definitionNode.SyntaxKind == SyntaxKind.TypeClauseNode)
					{
						typeReference = new TypeReference((TypeClauseNode)definitionNode);
					}
					else if (definitionNode.SyntaxKind == SyntaxKind.TypeDefinitionNode)
					{
						var typeDefinitionNode = (TypeDefinitionNode)definitionNode;
						typeReference = typeDefinitionNode.ToTypeReference();
					}
						
					if (typeReference != default)
					{
						Symbol innerFoundSymbol = default;

						if (typeReference.TypeIdentifierToken.TextSpan.ResourceUri != textEditorModel.PersistentState.ResourceUri)
						{
							var innerCompilerServiceResource = extendedCompilerService.GetResource(typeReference.TypeIdentifierToken.TextSpan.ResourceUri);
							if (innerCompilerServiceResource is not null)
								symbols = ((CSharpCompilationUnit)innerCompilerServiceResource.CompilationUnit).SymbolList;
						}
						
				        if (symbols.Count != 0)
				        {
				            foreach (var symbol in symbols)
				            {
				                if (typeReference.TypeIdentifierToken.TextSpan.StartInclusiveIndex >= symbol.TextSpan.StartInclusiveIndex &&
				                    typeReference.TypeIdentifierToken.TextSpan.StartInclusiveIndex < symbol.TextSpan.EndExclusiveIndex)
				                {
				                    innerFoundSymbol = symbol;
				                }
				            }
				        }
				        
				        if (innerFoundSymbol != default)
				        {
				        	var maybeTypeDefinitionNode = GetDefinitionNode(innerFoundSymbol.TextSpan, compilerServiceResource, innerFoundSymbol);
							
							if (maybeTypeDefinitionNode is not null && maybeTypeDefinitionNode.SyntaxKind == SyntaxKind.TypeDefinitionNode)
							{
								var typeDefinitionNode = (TypeDefinitionNode)maybeTypeDefinitionNode;
								var memberList = typeDefinitionNode.GetMemberList();
								ISyntaxNode? foundDefinitionNode = null;
					    		
					    		foreach (var member in memberList.Where(x => __CSharpBinder.GetName(x).Contains(filteringWord)).Take(25))
			        			{
			        				switch (member.SyntaxKind)
			        				{
			        					case SyntaxKind.VariableDeclarationNode:
			        						var variableDeclarationNode = (VariableDeclarationNode)member;
			        						autocompleteEntryList.Add(new AutocompleteEntry(
												variableDeclarationNode.IdentifierToken.TextSpan.Text,
								                AutocompleteEntryKind.Variable,
								                () => MemberAutocomplete(variableDeclarationNode.IdentifierToken.TextSpan.Text, virtualizationResult.Model.PersistentState.ResourceUri, virtualizationResult.ViewModel.PersistentState.ViewModelKey)));
			        						break;
			    						case SyntaxKind.FunctionDefinitionNode:
			        						var functionDefinitionNode = (FunctionDefinitionNode)member;
			        						autocompleteEntryList.Add(new AutocompleteEntry(
												functionDefinitionNode.FunctionIdentifierToken.TextSpan.Text,
								                AutocompleteEntryKind.Function,
								                () => MemberAutocomplete(functionDefinitionNode.FunctionIdentifierToken.TextSpan.Text, virtualizationResult.Model.PersistentState.ResourceUri, virtualizationResult.ViewModel.PersistentState.ViewModelKey)));
			        						break;
		        						case SyntaxKind.TypeDefinitionNode:
			        						var innerTypeDefinitionNode = (TypeDefinitionNode)member;
			        						autocompleteEntryList.Add(new AutocompleteEntry(
												innerTypeDefinitionNode.TypeIdentifierToken.TextSpan.Text,
								                AutocompleteEntryKind.Type,
								                () => MemberAutocomplete(innerTypeDefinitionNode.TypeIdentifierToken.TextSpan.Text, virtualizationResult.Model.PersistentState.ResourceUri, virtualizationResult.ViewModel.PersistentState.ViewModelKey)));
			        						break;
			        				}
			        			}
							}
				        }
					}
		    	}
	        }
		
			return new MenuRecord(
				autocompleteEntryList.Select(entry => new MenuOptionRecord(
					    entry.DisplayName,
					    MenuOptionKind.Other,
					    () => entry.SideEffectFunc?.Invoke() ?? Task.CompletedTask,
					    widgetParameterMap: new Dictionary<string, object?>
					    {
					        {
					            nameof(AutocompleteEntry),
					            entry
					        }
					    }))
					.ToList());
		}
		
		return null;
	}

	public MenuRecord GetAutocompleteMenu(TextEditorVirtualizationResult virtualizationResult, AutocompleteMenu autocompleteMenu)
	{
		var positionIndex = virtualizationResult.Model.GetPositionIndex(virtualizationResult.ViewModel);
		
		var autocompleteMenuPart = GetAutocompleteMenuPart(virtualizationResult, autocompleteMenu, positionIndex);
		if (autocompleteMenuPart is not null)
			return autocompleteMenuPart;
        
        var word = virtualizationResult.Model.ReadPreviousWordOrDefault(
	        virtualizationResult.ViewModel.LineIndex,
	        virtualizationResult.ViewModel.ColumnIndex);
	
		// The cursor is 1 character ahead.
        var textSpan = new TextEditorTextSpan(
            positionIndex - 1,
            positionIndex,
            0,
            virtualizationResult.Model.PersistentState.ResourceUri,
            virtualizationResult.Model.GetAllText());
	
		var compilerServiceAutocompleteEntryList = OBSOLETE_GetAutocompleteEntries(
			word,
            textSpan,
            virtualizationResult);
	
		return autocompleteMenu.GetDefaultMenuRecord(compilerServiceAutocompleteEntryList);
	}
	
	private Task MemberAutocomplete(string text, ResourceUri resourceUri, Key<TextEditorViewModel> viewModelKey)
	{
		_textEditorService.WorkerArbitrary.PostUnique(async editContext =>
		{
			var model = editContext.GetModelModifier(resourceUri);
			var viewModel = editContext.GetViewModelModifier(viewModelKey);
			
			model.Insert(text, viewModel);
			await viewModel.FocusAsync();
			
			await ParseAsync(editContext, model, shouldApplySyntaxHighlighting: true);
		});
		
		return Task.CompletedTask;
	}
    
    public ValueTask<MenuRecord> GetQuickActionsSlashRefactorMenu(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModelModifier)
    {
		var compilerService = modelModifier.PersistentState.CompilerService;
	
		var compilerServiceResource = viewModelModifier is null
			? null
			: compilerService.GetResource(modelModifier.PersistentState.ResourceUri);

		int? primaryCursorPositionIndex = modelModifier is null || viewModelModifier is null
			? null
			: modelModifier.GetPositionIndex(viewModelModifier);

		var syntaxNode = primaryCursorPositionIndex is null || __CSharpBinder is null || compilerServiceResource?.CompilationUnit is null
			? null
			: __CSharpBinder.GetSyntaxNode(null, primaryCursorPositionIndex.Value, compilerServiceResource.ResourceUri, (CSharpResource)compilerServiceResource);
			
		var menuOptionList = new List<MenuOptionRecord>();
			
		menuOptionList.Add(new MenuOptionRecord(
			"QuickActionsSlashRefactorMenu",
			MenuOptionKind.Other));
			
		if (syntaxNode is null)
		{
			menuOptionList.Add(new MenuOptionRecord(
				"syntaxNode was null",
				MenuOptionKind.Other,
				onClickFunc: async () => {}));
		}
		else
		{
			if (syntaxNode.SyntaxKind == SyntaxKind.TypeClauseNode)
			{
				var allTypeDefinitions = __CSharpBinder.AllTypeDefinitions;
				
				var typeClauseNode = (TypeClauseNode)syntaxNode;
				
				if (allTypeDefinitions.TryGetValue(typeClauseNode.TypeIdentifierToken.TextSpan.Text, out var typeDefinitionNode))
				{
					var usingStatementText = $"using {typeDefinitionNode.NamespaceName};";
						
					menuOptionList.Add(new MenuOptionRecord(
						$"Copy: {usingStatementText}",
						MenuOptionKind.Other,
						onClickFunc: async () =>
						{
							await _commonUtilityService.SetClipboard(usingStatementText).ConfigureAwait(false);
						}));
				}
				else
				{
					menuOptionList.Add(new MenuOptionRecord(
						"type not found",
						MenuOptionKind.Other,
						onClickFunc: async () => {}));
				}
			}
			else
			{
				menuOptionList.Add(new MenuOptionRecord(
					syntaxNode.SyntaxKind.ToString(),
					MenuOptionKind.Other,
					onClickFunc: async () => {}));
			}
		}
		
		MenuRecord menu;
		
		if (menuOptionList.Count == 0)
			menu = new MenuRecord(MenuRecord.NoMenuOptionsExistList);
		else
			menu = new MenuRecord(menuOptionList);
    
    	return ValueTask.FromResult(menu);
    }
	
	public async ValueTask OnInspect(
		TextEditorEditContext editContext,
		TextEditorModel modelModifier,
		TextEditorViewModel viewModelModifier,
		double clientX,
		double clientY,
		bool shiftKey,
        bool ctrlKey,
        bool altKey,
		TextEditorComponentData componentData,
		IWalkTextEditorComponentRenderers textEditorComponentRenderers,
        ResourceUri resourceUri)
    {
    	// Lazily calculate row and column index a second time. Otherwise one has to calculate it every mouse moved event.
        var lineAndColumnIndex = await EventUtils.CalculateLineAndColumnIndex(
				modelModifier,
				viewModelModifier,
				clientX,
				clientY,
				componentData,
				editContext)
			.ConfigureAwait(false);
	
        var cursorPositionIndex = modelModifier.GetPositionIndex(
        	lineAndColumnIndex.LineIndex,
            lineAndColumnIndex.ColumnIndex);

        var foundMatch = false;
        
        var resource = GetResource(modelModifier.PersistentState.ResourceUri);
        var compilationUnitLocal = (CSharpCompilationUnit)resource.CompilationUnit;
        
        var symbols = compilationUnitLocal.SymbolList;
        var diagnostics = compilationUnitLocal.DiagnosticList;

        if (diagnostics.Count != 0)
        {
            foreach (var diagnostic in diagnostics)
            {
                if (cursorPositionIndex >= diagnostic.TextSpan.StartInclusiveIndex &&
                    cursorPositionIndex < diagnostic.TextSpan.EndExclusiveIndex)
                {
                    // Prefer showing a diagnostic over a symbol when both exist at the mouse location.
                    foundMatch = true;

                    var parameterMap = new Dictionary<string, object?>
                    {
                        {
                            nameof(ITextEditorDiagnosticRenderer.Diagnostic),
                            diagnostic
                        }
                    };

                    viewModelModifier.PersistentState.TooltipModel = new Walk.Common.RazorLib.Tooltips.Models.TooltipModel<(TextEditorService TextEditorService, Key<TextEditorViewModel> ViewModelKey)>(
	                    modelModifier.PersistentState.CompilerService.DiagnosticRendererType ?? textEditorComponentRenderers.DiagnosticRendererType,
	                    parameterMap,
	                    clientX,
	                    clientY,
	                    cssClassString: null,
                        componentData.ContinueRenderingTooltipAsync,
                        Walk.TextEditor.RazorLib.Commands.Models.Defaults.TextEditorCommandDefaultFunctions.OnWheel,
                        (_textEditorService, viewModelModifier.PersistentState.ViewModelKey));
                    componentData.TextEditorViewModelSlimDisplay.CommonUtilityService.SetTooltipModel(viewModelModifier.PersistentState.TooltipModel);
                }
            }
        }

        if (!foundMatch && symbols.Count != 0)
        {
            foreach (var symbol in symbols)
            {
                if (cursorPositionIndex >= symbol.TextSpan.StartInclusiveIndex &&
                    cursorPositionIndex < symbol.TextSpan.EndExclusiveIndex)
                {
                    foundMatch = true;

                    var parameters = new Dictionary<string, object?>
                    {
                        {
                            "Symbol",
                            symbol
                        }
                    };

                    viewModelModifier.PersistentState.TooltipModel = new Walk.Common.RazorLib.Tooltips.Models.TooltipModel<(TextEditorService TextEditorService, Key<TextEditorViewModel> ViewModelKey)>(
                        typeof(Walk.Extensions.CompilerServices.Displays.SymbolDisplay),
                        parameters,
                        clientX,
                        clientY,
                        cssClassString: null,
                        componentData.ContinueRenderingTooltipAsync,
                        Walk.TextEditor.RazorLib.Commands.Models.Defaults.TextEditorCommandDefaultFunctions.OnWheel,
                        (_textEditorService, viewModelModifier.PersistentState.ViewModelKey));
                    componentData.TextEditorViewModelSlimDisplay.CommonUtilityService.SetTooltipModel(viewModelModifier.PersistentState.TooltipModel);
                }
            }
        }

        if (!foundMatch)
        {
			viewModelModifier.PersistentState.TooltipModel = null;
			componentData.TextEditorViewModelSlimDisplay.CommonUtilityService.SetTooltipModel(viewModelModifier.PersistentState.TooltipModel);
        }

        // TODO: Measure the tooltip, and reposition if it would go offscreen.
    }
    
    public async ValueTask ShowCallingSignature(
		TextEditorEditContext editContext,
		TextEditorModel modelModifier,
		TextEditorViewModel viewModelModifier,
		int positionIndex,
		TextEditorComponentData componentData,
		IWalkTextEditorComponentRenderers textEditorComponentRenderers,
        ResourceUri resourceUri)
    {
    	return;
    	/*var success = __CSharpBinder.TryGetCompilationUnit(
    		cSharpCompilationUnit: null,
    		resourceUri,
    		out CSharpCompilationUnit compilationUnit);
    		
    	if (!success)
    		return;
    	
    	var scope = __CSharpBinder.GetScopeByPositionIndex(compilationUnit, resourceUri, positionIndex);
    	
    	if (!scope.ConstructorWasInvoked)
			return;
		
		if (scope.CodeBlockOwner is null)
			return;
		
		if (!scope.CodeBlockOwner.CodeBlock.ConstructorWasInvoked)
			return;
    	
    	FunctionInvocationNode? functionInvocationNode = null;
    	
    	foreach (var childSyntax in scope.CodeBlockOwner.CodeBlock.ChildList)
    	{
    		if (childSyntax.SyntaxKind == SyntaxKind.ReturnStatementNode)
    		{
    			var returnStatementNode = (ReturnStatementNode)childSyntax;
    			
    			if (returnStatementNode.ExpressionNode.SyntaxKind == SyntaxKind.FunctionInvocationNode)
	    		{
	    			functionInvocationNode = (FunctionInvocationNode)returnStatementNode.ExpressionNode;
	    			break;
	    		}
    		}
    	
    		if (functionInvocationNode is not null)
    			break;
    	
    		if (childSyntax.SyntaxKind == SyntaxKind.FunctionInvocationNode)
    		{
    			functionInvocationNode = (FunctionInvocationNode)childSyntax;
    			break;
    		}
    	}
    	
    	if (functionInvocationNode is null)
    		return;
    	
    	var foundMatch = false;
        
        var resource = modelModifier.PersistentState.ResourceUri;
        var compilationUnitLocal = compilationUnit;
        
        var symbols = compilationUnitLocal.SymbolList;
        
        var cursorPositionIndex = functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan.StartInclusiveIndex;
        
        var lineAndColumnIndices = modelModifier.GetLineAndColumnIndicesFromPositionIndex(cursorPositionIndex);
        
        var elementPositionInPixels = await _textEditorService.JsRuntimeTextEditorApi
            .GetBoundingClientRect(componentData.PrimaryCursorContentId)
            .ConfigureAwait(false);

        elementPositionInPixels = elementPositionInPixels with
        {
            Top = elementPositionInPixels.Top +
                (.9 * viewModelModifier.CharAndLineMeasurements.LineHeight)
        };
        
        var mouseEventArgs = new MouseEventArgs
        {
            ClientX = elementPositionInPixels.Left,
            ClientY = elementPositionInPixels.Top
        };
		    
		var relativeCoordinatesOnClick = new RelativeCoordinates(
		    mouseEventArgs.ClientX - viewModelModifier.TextEditorDimensions.BoundingClientRectLeft,
		    mouseEventArgs.ClientY - viewModelModifier.TextEditorDimensions.BoundingClientRectTop,
		    viewModelModifier.ScrollLeft,
		    viewModelModifier.ScrollTop);

        if (!foundMatch && symbols.Count != 0)
        {
            foreach (var symbol in symbols)
            {
                if (cursorPositionIndex >= symbol.TextSpan.StartInclusiveIndex &&
                    cursorPositionIndex < symbol.TextSpan.EndExclusiveIndex &&
                    symbol.SyntaxKind == SyntaxKind.FunctionSymbol)
                {
                    foundMatch = true;

                    var parameters = new Dictionary<string, object?>
                    {
                        {
                            "Symbol",
                            symbol
                        }
                    };

                    viewModelModifier.PersistentState.TooltipViewModel = new(
                        typeof(Walk.Extensions.CompilerServices.Displays.SymbolDisplay),
                        parameters,
                        relativeCoordinatesOnClick,
                        null,
                        componentData.ContinueRenderingTooltipAsync);
                        
                    break;
                }
            }
        }

        if (!foundMatch)
        {
			viewModelModifier.PersistentState.TooltipViewModel = null;
        }
        */
    }
    
    public async ValueTask GoToDefinition(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModelModifier,
        Category category)
    {
        var cursorPositionIndex = modelModifier.GetPositionIndex(viewModelModifier);

        var foundMatch = false;
        
        var resource = GetResource(modelModifier.PersistentState.ResourceUri);
        var compilationUnitLocal = (CSharpCompilationUnit)resource.CompilationUnit;
        
        var symbolList = compilationUnitLocal.SymbolList;
        var foundSymbol = default(Symbol);
        
        foreach (var symbol in symbolList)
        {
            if (cursorPositionIndex >= symbol.TextSpan.StartInclusiveIndex &&
                cursorPositionIndex < symbol.TextSpan.EndExclusiveIndex)
            {
                foundMatch = true;
				foundSymbol = symbol;
            }
        }
        
        if (!foundMatch)
        	return;
    
    	var symbolLocal = foundSymbol;
		var targetNode = SymbolDisplay.GetTargetNode(_textEditorService, symbolLocal);
		var definitionNode = SymbolDisplay.GetDefinitionNode(_textEditorService, symbolLocal, targetNode);
		
		if (definitionNode is null)
			return;
			
		// TODO: Do not duplicate this code from SyntaxViewModel.HandleOnClick(...)
		
		string? resourceUriValue = null;
		var indexInclusiveStart = -1;
		
		if (definitionNode.SyntaxKind == SyntaxKind.TypeDefinitionNode)
		{
			var typeDefinitionNode = (TypeDefinitionNode)definitionNode;
			resourceUriValue = typeDefinitionNode.TypeIdentifierToken.TextSpan.ResourceUri.Value;
			indexInclusiveStart = typeDefinitionNode.TypeIdentifierToken.TextSpan.StartInclusiveIndex;
		}
		else if (definitionNode.SyntaxKind == SyntaxKind.VariableDeclarationNode)
		{
			var variableDeclarationNode = (VariableDeclarationNode)definitionNode;
			resourceUriValue = variableDeclarationNode.IdentifierToken.TextSpan.ResourceUri.Value;
			indexInclusiveStart = variableDeclarationNode.IdentifierToken.TextSpan.StartInclusiveIndex;
		}
		else if (definitionNode.SyntaxKind == SyntaxKind.NamespaceStatementNode)
		{
			var namespaceStatementNode = (NamespaceStatementNode)definitionNode;
			resourceUriValue = namespaceStatementNode.IdentifierToken.TextSpan.ResourceUri.Value;
			indexInclusiveStart = namespaceStatementNode.IdentifierToken.TextSpan.StartInclusiveIndex;
		}
		else if (definitionNode.SyntaxKind == SyntaxKind.FunctionDefinitionNode)
		{
			var functionDefinitionNode = (FunctionDefinitionNode)definitionNode;
			resourceUriValue = functionDefinitionNode.FunctionIdentifierToken.TextSpan.ResourceUri.Value;
			indexInclusiveStart = functionDefinitionNode.FunctionIdentifierToken.TextSpan.StartInclusiveIndex;
		}
		else if (definitionNode.SyntaxKind == SyntaxKind.ConstructorDefinitionNode)
		{
			var constructorDefinitionNode = (ConstructorDefinitionNode)definitionNode;
			resourceUriValue = constructorDefinitionNode.FunctionIdentifier.TextSpan.ResourceUri.Value;
			indexInclusiveStart = constructorDefinitionNode.FunctionIdentifier.TextSpan.StartInclusiveIndex;
		}
		
		if (resourceUriValue is null || indexInclusiveStart == -1)
			return;
		
		_textEditorService.WorkerArbitrary.PostUnique(async editContext =>
		{
			if (category.Value == "CodeSearchService")
			{
				await ((TextEditorKeymapDefault)TextEditorKeymapFacts.DefaultKeymap).AltF12Func.Invoke(
					editContext,
					resourceUriValue,
					indexInclusiveStart);
			}
			else
			{
				await _textEditorService.OpenInEditorAsync(
						editContext,
						resourceUriValue,
						true,
						indexInclusiveStart,
						category,
						Key<TextEditorViewModel>.NewKey())
					.ContinueWith(_ => _textEditorService.ViewModelApi.StopCursorBlinking());
			}
		});
    }
    
    /// <summary>
    /// This implementation is NOT thread safe.
    /// </summary>
    public ValueTask ParseAsync(TextEditorEditContext editContext, TextEditorModel modelModifier, bool shouldApplySyntaxHighlighting)
	{
		var resourceUri = modelModifier.PersistentState.ResourceUri;
	
		if (!_resourceMap.ContainsKey(resourceUri))
			return ValueTask.CompletedTask;
	
		_textEditorService.ModelApi.StartPendingCalculatePresentationModel(
			editContext,
	        modelModifier,
	        CompilerServiceDiagnosticPresentationFacts.PresentationKey,
			CompilerServiceDiagnosticPresentationFacts.EmptyPresentationModel);

		var presentationModel = modelModifier.PresentationModelList.First(
			x => x.TextEditorPresentationKey == CompilerServiceDiagnosticPresentationFacts.PresentationKey);
		
		var cSharpCompilationUnit = new CSharpCompilationUnit(
			resourceUri,
			presentationModel.PendingCalculation.ContentAtRequest,
			CompilationUnitKind.IndividualFile_AllData);
		
		var lexerOutput = CSharpLexer.Lex(__CSharpBinder, resourceUri, presentationModel.PendingCalculation.ContentAtRequest, shouldUseSharedStringWalker: true);
		cSharpCompilationUnit.TokenList = lexerOutput.SyntaxTokenList;
		cSharpCompilationUnit.MiscTextSpanList = lexerOutput.MiscTextSpanList;

		// Even if the parser throws an exception, be sure to
		// make use of the Lexer to do whatever syntax highlighting is possible.
		try
		{
			__CSharpBinder.StartCompilationUnit(resourceUri);
			CSharpParser.Parse(cSharpCompilationUnit, __CSharpBinder, ref lexerOutput);
		}
		finally
		{
			lock (_resourceMapLock)
			{
				if (_resourceMap.ContainsKey(resourceUri))
				{
					var resource = (CSharpResource)_resourceMap[resourceUri];
					resource.CompilationUnit = cSharpCompilationUnit;
				}
			}
			
			var diagnosticTextSpans = cSharpCompilationUnit.DiagnosticList
				.Select(x => x.TextSpan)
				.ToList();

			modelModifier.CompletePendingCalculatePresentationModel(
				CompilerServiceDiagnosticPresentationFacts.PresentationKey,
				CompilerServiceDiagnosticPresentationFacts.EmptyPresentationModel,
				diagnosticTextSpans);
			
			if (shouldApplySyntaxHighlighting)
			{
				editContext.TextEditorService.ModelApi.ApplySyntaxHighlighting(
					editContext,
					modelModifier);
					
				CreateCollapsePoints(
					editContext,
					modelModifier);
			}

			ResourceParsed?.Invoke();
        }
		
        return ValueTask.CompletedTask;
	}
    
    public async ValueTask FastParseAsync(TextEditorEditContext editContext, ResourceUri resourceUri, IFileSystemProvider fileSystemProvider, CompilationUnitKind compilationUnitKind)
	{
		var content = await fileSystemProvider.File
            .ReadAllTextAsync(resourceUri.Value)
            .ConfigureAwait(false);
	
		if (!_resourceMap.ContainsKey(resourceUri))
			return;

		var cSharpCompilationUnit = new CSharpCompilationUnit(
			resourceUri,
			content,
			compilationUnitKind);
		
		var lexerOutput = CSharpLexer.Lex(__CSharpBinder, resourceUri, content, shouldUseSharedStringWalker: true);
		cSharpCompilationUnit.TokenList = lexerOutput.SyntaxTokenList;
		cSharpCompilationUnit.MiscTextSpanList = lexerOutput.MiscTextSpanList;

		// Even if the parser throws an exception, be sure to
		// make use of the Lexer to do whatever syntax highlighting is possible.
		try
		{
			__CSharpBinder.StartCompilationUnit(resourceUri);
			CSharpParser.Parse(cSharpCompilationUnit, __CSharpBinder, ref lexerOutput);
		}
		finally
		{
			lock (_resourceMapLock)
			{
				if (_resourceMap.ContainsKey(resourceUri))
				{
					var resource = (CSharpResource)_resourceMap[resourceUri];
					resource.CompilationUnit = cSharpCompilationUnit;
				}
			}
			
			ResourceParsed?.Invoke();
        }
	}
	
	public void FastParse(TextEditorEditContext editContext, ResourceUri resourceUri, IFileSystemProvider fileSystemProvider, CompilationUnitKind compilationUnitKind)
	{
	    if (!_resourceMap.ContainsKey(resourceUri))
			return;
	
		var content = fileSystemProvider.File.ReadAllText(resourceUri.Value);

		var cSharpCompilationUnit = new CSharpCompilationUnit(
			resourceUri,
			content,
			compilationUnitKind);
		
		var lexerOutput = CSharpLexer.Lex(__CSharpBinder, resourceUri, content, shouldUseSharedStringWalker: true);
		cSharpCompilationUnit.TokenList = lexerOutput.SyntaxTokenList;
		cSharpCompilationUnit.MiscTextSpanList = lexerOutput.MiscTextSpanList;

		// Even if the parser throws an exception, be sure to
		// make use of the Lexer to do whatever syntax highlighting is possible.
		try
		{
			__CSharpBinder.StartCompilationUnit(resourceUri);
			CSharpParser.Parse(cSharpCompilationUnit, __CSharpBinder, ref lexerOutput);
		}
		finally
		{
			lock (_resourceMapLock)
			{
				if (_resourceMap.ContainsKey(resourceUri))
				{
					var resource = (CSharpResource)_resourceMap[resourceUri];
					resource.CompilationUnit = cSharpCompilationUnit;
				}
			}
			
			// Do not invoke ResourceParsed for the fast parse
			// TODO: Consider making this change to the async version too.
        }
	}
    
    /// <summary>
    /// Looks up the <see cref="IScope"/> that encompasses the provided positionIndex.
    ///
    /// Then, checks the <see cref="IScope"/>.<see cref="IScope.CodeBlockOwner"/>'s children
    /// to determine which node exists at the positionIndex.
    ///
    /// If the <see cref="IScope"/> cannot be found, then as a fallback the provided compilationUnit's
    /// <see cref="CompilationUnit.RootCodeBlockNode"/> will be treated
    /// the same as if it were the <see cref="IScope"/>.<see cref="IScope.CodeBlockOwner"/>.
    ///
    /// If the provided compilerServiceResource?.CompilationUnit is null, then the fallback step will not occur.
    /// The fallback step is expected to occur due to the global scope being implemented with a null
    /// <see cref="IScope"/>.<see cref="IScope.CodeBlockOwner"/> at the time of this comment.
    /// </summary>
    public ISyntaxNode? GetSyntaxNode(int positionIndex, ResourceUri resourceUri, ICompilerServiceResource? compilerServiceResource)
    {
    	return __CSharpBinder.GetSyntaxNode(cSharpCompilationUnit: null, positionIndex, resourceUri, (CSharpResource)compilerServiceResource);
    }
    
    /// <summary>
    /// Returns the <see cref="ISyntaxNode"/> that represents the definition in the <see cref="CompilationUnit"/>.
    ///
    /// The option argument 'symbol' can be provided if available. It might provide additional information to the method's implementation
    /// that is necessary to find certain nodes (ones that are in a separate file are most common to need a symbol to find).
    /// </summary>
    public ISyntaxNode? GetDefinitionNode(TextEditorTextSpan textSpan, ICompilerServiceResource compilerServiceResource, Symbol? symbol = null)
    {
    	if (symbol is null)
    		return null;
    		
    	return __CSharpBinder.GetDefinitionNode(cSharpCompilationUnit: null, textSpan, symbol.Value.SyntaxKind, symbol);
    }

	/// <summary>
	/// Returns the text span at which the definition exists in the source code.
	/// </summary>
    public TextEditorTextSpan? GetDefinitionTextSpan(TextEditorTextSpan textSpan, ICompilerServiceResource compilerServiceResource)
    {
    	return __CSharpBinder.GetDefinitionTextSpan(textSpan, (CSharpResource)compilerServiceResource);
    }

	public Scope GetScopeByPositionIndex(ResourceUri resourceUri, int positionIndex)
    {
    	return __CSharpBinder.GetScopeByPositionIndex(null, resourceUri, positionIndex);
    }
    
    public List<AutocompleteEntry>? OBSOLETE_GetAutocompleteEntries(string word, TextEditorTextSpan textSpan, TextEditorVirtualizationResult virtualizationResult)
    {
    	if (word is null)
			return null;
			
    	var boundScope = __CSharpBinder.GetScope(null, textSpan);

        if (!boundScope.ConstructorWasInvoked)
            return null;
        
        var autocompleteEntryList = new List<AutocompleteEntry>();

        var targetScope = boundScope;
        
        if (textSpan.Text == ".")
        {
        	var textEditorModel = _textEditorService.ModelApi.GetOrDefault(textSpan.ResourceUri);
	    	if (textEditorModel is null)
	    		return autocompleteEntryList.DistinctBy(x => x.DisplayName).ToList();
	    	
	    	var compilerService = textEditorModel.PersistentState.CompilerService;
	    	
	    	var compilerServiceResource = compilerService.GetResource(textEditorModel.PersistentState.ResourceUri);
	    	if (compilerServiceResource is null)
	    		return autocompleteEntryList.DistinctBy(x => x.DisplayName).ToList();
	
	    	var targetNode = __CSharpBinder.GetSyntaxNode(
	    		(CSharpCompilationUnit)compilerServiceResource.CompilationUnit,
	    		textSpan.StartInclusiveIndex - 1,
	    		textSpan.ResourceUri,
	    		(CSharpResource)compilerServiceResource);
	    		
	    	if (targetNode is null)
	    		return autocompleteEntryList.DistinctBy(x => x.DisplayName).ToList();
        
        	TypeReference typeReference = default;
	
			if (targetNode.SyntaxKind == SyntaxKind.VariableReferenceNode)
			{
				var variableReferenceNode = (VariableReferenceNode)targetNode;
			
				if (variableReferenceNode.VariableDeclarationNode is not null)
				{
					typeReference = variableReferenceNode.VariableDeclarationNode.TypeReference;
				}
			}
			else if (targetNode.SyntaxKind == SyntaxKind.VariableDeclarationNode)
			{
				typeReference = ((VariableDeclarationNode)targetNode).TypeReference;
			}
			else if (targetNode.SyntaxKind == SyntaxKind.TypeClauseNode)
			{
				typeReference = new TypeReference((TypeClauseNode)targetNode);
			}
			else if (targetNode.SyntaxKind == SyntaxKind.TypeDefinitionNode)
			{
				typeReference = ((TypeDefinitionNode)targetNode).ToTypeReference();
			}
			else if (targetNode.SyntaxKind == SyntaxKind.ConstructorDefinitionNode)
			{
				typeReference = ((ConstructorDefinitionNode)targetNode).ReturnTypeReference;
			}
			
			if (typeReference == default)
				return autocompleteEntryList.DistinctBy(x => x.DisplayName).ToList();
			
			var maybeTypeDefinitionNode = __CSharpBinder.GetDefinitionNode((CSharpCompilationUnit)compilerServiceResource.CompilationUnit, typeReference.TypeIdentifierToken.TextSpan, SyntaxKind.TypeClauseNode);
			if (maybeTypeDefinitionNode is null || maybeTypeDefinitionNode.SyntaxKind != SyntaxKind.TypeDefinitionNode)
				return autocompleteEntryList.DistinctBy(x => x.DisplayName).ToList();
			
			var typeDefinitionNode = (TypeDefinitionNode)maybeTypeDefinitionNode;
			var memberList = typeDefinitionNode.GetMemberList();
			
			autocompleteEntryList.AddRange(
	        	memberList
	        	.Select(node => 
	        	{
	        		if (node.SyntaxKind == SyntaxKind.VariableDeclarationNode)
	        		{
	        			var variableDeclarationNode = (VariableDeclarationNode)node;
	        			return variableDeclarationNode.IdentifierToken.TextSpan.Text;
	        		}
	        		else if (node.SyntaxKind == SyntaxKind.TypeDefinitionNode)
	        		{
	        			var typeDefinitionNode = (TypeDefinitionNode)node;
	        			return typeDefinitionNode.TypeIdentifierToken.TextSpan.Text;
	        		}
	        		else if (node.SyntaxKind == SyntaxKind.FunctionDefinitionNode)
	        		{
	        			var functionDefinitionNode = (FunctionDefinitionNode)node;
	        			return functionDefinitionNode.FunctionIdentifierToken.TextSpan.Text;
	        		}
	        		else
	        		{
	        			return string.Empty;
	        		}
	        	})
	            .ToArray()
	            //.Where(x => x.Contains(word, StringComparison.InvariantCulture))
	            .Distinct()
	            .Take(5)
	            .Select(x =>
	            {
	                return new AutocompleteEntry(
	                    x,
	                    AutocompleteEntryKind.Variable,
	                    null);
	            }));
        }
		else
		{
			while (targetScope.ConstructorWasInvoked)
	        {
	            autocompleteEntryList.AddRange(
	            	__CSharpBinder.GetVariableDeclarationNodesByScope(cSharpCompilationUnit: null, textSpan.ResourceUri, targetScope.IndexKey)
	            	.Select(x => x.IdentifierToken.TextSpan.Text)
	                .ToArray()
	                .Where(x => x.Contains(word, StringComparison.InvariantCulture))
	                .Distinct()
	                .Take(5)
	                .Select(x =>
	                {
	                    return new AutocompleteEntry(
	                        x,
	                        AutocompleteEntryKind.Variable,
	                        null);
	                }));
	
	            autocompleteEntryList.AddRange(
	                __CSharpBinder.GetFunctionDefinitionNodesByScope(cSharpCompilationUnit: null, textSpan.ResourceUri, targetScope.IndexKey)
	            	.Select(x => x.FunctionIdentifierToken.TextSpan.Text)
	                .ToArray()
	                .Where(x => x.Contains(word, StringComparison.InvariantCulture))
	                .Distinct()
	                .Take(5)
	                .Select(x =>
	                {
	                    return new AutocompleteEntry(
	                        x,
	                        AutocompleteEntryKind.Function,
	                        null);
	                }));
	
				if (targetScope.ParentIndexKey == -1)
					targetScope = default;
				else
	            	targetScope = __CSharpBinder.GetScopeByScopeIndexKey(compilationUnit: null, textSpan.ResourceUri, targetScope.ParentIndexKey);
	        }
        
	        var allTypeDefinitions = __CSharpBinder.AllTypeDefinitions;
	
	        autocompleteEntryList.AddRange(
	            allTypeDefinitions
	            .Where(x => x.Key.Contains(word, StringComparison.InvariantCulture))
	            .Distinct()
	            .Take(5)
	            .Select(x =>
	            {
	                return new AutocompleteEntry(
	                    x.Key,
	                    AutocompleteEntryKind.Type,
	                    () =>
	                    {
	                    	// TODO: The namespace code is buggy at the moment.
	                    	//       It is annoying how this keeps adding the wrong namespace.
	                    	//       Just have it do nothing for now. (2024-08-24)
	                    	// ===============================================================
	                        /*if (boundScope.EncompassingNamespaceStatementNode.IdentifierToken.TextSpan.GetText() == x.Key.NamespaceIdentifier ||
	                            boundScope.CurrentUsingStatementNodeList.Any(usn => usn.NamespaceIdentifier.TextSpan.GetText() == x.Key.NamespaceIdentifier))
	                        {
	                            return Task.CompletedTask;
	                        }
	
	                        _textEditorService.PostUnique(
	                            "Add using statement",
	                            editContext =>
	                            {
	                                var modelModifier = editContext.GetModelModifier(textSpan.ResourceUri);
	
	                                if (modelModifier is null)
	                                    return Task.CompletedTask;
	
	                                var viewModelList = _textEditorService.ModelApi.GetViewModelsOrEmpty(textSpan.ResourceUri);
	
	                                var cursor = new TextEditorCursor(0, 0, true);
	                                var cursorModifierBag = new CursorModifierBagTextEditor(
	                                    Key<TextEditorViewModel>.Empty,
	                                    new List<TextEditorCursorModifier> { new(cursor) });
	
	                                var textToInsert = $"using {x.Key.NamespaceIdentifier};\n";
	
	                                modelModifier.Insert(
	                                    textToInsert,
	                                    cursorModifierBag,
	                                    cancellationToken: CancellationToken.None);
	
	                                foreach (var unsafeViewModel in viewModelList)
	                                {
	                                    var viewModelModifier = editContext.GetViewModelModifier(unsafeViewModel.ViewModelKey);
	                                    var viewModelCursorModifierBag = editContext.GetCursorModifierBag(viewModelModifier);
	
	                                    if (viewModelModifier is null || viewModelCursorModifierBag is null)
	                                        continue;
	
	                                    foreach (var cursorModifier in viewModelCursorModifierBag.List)
	                                    {
	                                        for (int i = 0; i < textToInsert.Length; i++)
	                                        {
	                                            _textEditorService.ViewModelApi.MoveCursor(
	                                            	new KeyboardEventArgs
	                                                {
	                                                    Key = KeyboardKeyFacts.MovementKeys.ARROW_RIGHT,
	                                                },
											        editContext,
											        modelModifier,
											        viewModelModifier,
											        viewModelCursorModifierBag);
	                                        }
	                                    }
	
	                                    editContext.TextEditorService.ModelApi.ApplySyntaxHighlighting(
	                                        editContext,
	                                        modelModifier);
	                                }
	
	                                return Task.CompletedTask;
	                            });*/
							return Task.CompletedTask;
	                    });
	            }));
	    }
	    
	    foreach (var namespaceGroupKvp in __CSharpBinder.NamespacePrefixTree.__Root.Children.Where(x => x.Key.Contains(word)).Take(5))
		{
			autocompleteEntryList.Add(new AutocompleteEntry(
				namespaceGroupKvp.Key,
		        AutocompleteEntryKind.Namespace,
		        () => Task.CompletedTask));
		}
            
        AddSnippets(autocompleteEntryList, word, textSpan);

        return autocompleteEntryList.DistinctBy(x => x.DisplayName).ToList();
    }
    
    private void AddSnippets(List<AutocompleteEntry> autocompleteEntryList, string word, TextEditorTextSpan textSpan)
    {
    	if ("prop".Contains(word))
    	{
	    	autocompleteEntryList.Add(new AutocompleteEntry(
	        	"prop",
		        AutocompleteEntryKind.Snippet,
		        () => PropSnippet(word, textSpan, "public TYPE NAME { get; set; }")));
		}
		
		if ("propnn".Contains(word))
    	{
	    	autocompleteEntryList.Add(new AutocompleteEntry(
	        	"propnn",
		        AutocompleteEntryKind.Snippet,
		        () => PropSnippet(word, textSpan, "public TYPE NAME { get; set; } = null!;")));
		}
    }
    
    private Task PropSnippet(string word, TextEditorTextSpan textSpan, string textToInsert)
    {
        _textEditorService.WorkerArbitrary.PostUnique((Func<TextEditorEditContext, ValueTask>)(editContext =>
        {
            var modelModifier = editContext.GetModelModifier(textSpan.ResourceUri);

            if (modelModifier is null)
                return ValueTask.CompletedTask;

            var viewModelList = _textEditorService.ModelApi.GetViewModelsOrEmpty(textSpan.ResourceUri);
            
            var viewModel = viewModelList.FirstOrDefault(x => x.PersistentState.Category.Value == "main")
            	?? viewModelList.FirstOrDefault();
            
            if (viewModel is null)
            	return ValueTask.CompletedTask;
            	
            var viewModelModifier = editContext.GetViewModelModifier(viewModel.PersistentState.ViewModelKey);
            
            if (viewModelModifier is null)
            	return ValueTask.CompletedTask;

            var cursorPositionIndex = modelModifier.GetPositionIndex(viewModelModifier);
            var behindPositionIndex = cursorPositionIndex - 1;
            		
            modelModifier.Insert(
                textToInsert,
                viewModelModifier);
                
            /*if (behindPositionIndex > 0 && modelModifier.GetCharacter(behindPositionIndex) == 'p')
            {
            	modelModifier.Delete(
			        viewModelModifier,
			        1,
			        expandWord: false,
                    TextEditorModel.DeleteKind.Delete);
            }*/

            modelModifier.PersistentState.CompilerService.ResourceWasModified(
            	(ResourceUri)modelModifier.PersistentState.ResourceUri,
            	(IReadOnlyList<TextEditorTextSpan>)Array.Empty<TextEditorTextSpan>());
            	
            return ValueTask.CompletedTask;
        }));
	        
	    return Task.CompletedTask;
    }
    
    private void CreateCollapsePoints(TextEditorEditContext editContext, TextEditorModel modelModifier)
    {
    	_collapsePointUsedIdentifierHashSet.Clear();
    
    	var resource = GetResource(modelModifier.PersistentState.ResourceUri);
			
		var collapsePointList = new List<CollapsePoint>();
		
		if (resource.CompilationUnit is IExtendedCompilationUnit extendedCompilationUnit)
		{
			if (extendedCompilationUnit.ScopeTypeDefinitionMap is not null)
			{
				foreach (var entry in extendedCompilationUnit.ScopeTypeDefinitionMap.Values)
				{
					if (entry.TypeIdentifierToken.TextSpan.ResourceUri != modelModifier.PersistentState.ResourceUri)
		    			continue;
			    		
			    	if (!_collapsePointUsedIdentifierHashSet.Add(entry.TypeIdentifierToken.TextSpan.Text))
		    			continue;
					
					collapsePointList.Add(new CollapsePoint(
						modelModifier.GetLineAndColumnIndicesFromPositionIndex(entry.TypeIdentifierToken.TextSpan.StartInclusiveIndex).lineIndex,
						false,
						entry.TypeIdentifierToken.TextSpan.Text,
						modelModifier.GetLineAndColumnIndicesFromPositionIndex(entry.CloseCodeBlockTextSpan.StartInclusiveIndex).lineIndex + 1));
				}
			}
			
			if (extendedCompilationUnit.ScopeFunctionDefinitionMap is not null)
			{
				foreach (var entry in extendedCompilationUnit.ScopeFunctionDefinitionMap.Values)
				{
					if (entry.FunctionIdentifierToken.TextSpan.ResourceUri != modelModifier.PersistentState.ResourceUri)
			    		continue;
			    		
					if (!_collapsePointUsedIdentifierHashSet.Add(entry.FunctionIdentifierToken.TextSpan.Text))
		    			continue;
					
					collapsePointList.Add(new CollapsePoint(
						modelModifier.GetLineAndColumnIndicesFromPositionIndex(entry.FunctionIdentifierToken.TextSpan.StartInclusiveIndex).lineIndex,
						false,
						entry.FunctionIdentifierToken.TextSpan.Text,
						modelModifier.GetLineAndColumnIndicesFromPositionIndex(entry.CloseCodeBlockTextSpan.StartInclusiveIndex).lineIndex + 1));
				}
			}
			
			foreach (var viewModelKey in modelModifier.PersistentState.ViewModelKeyList)
			{
				if (modelModifier.PersistentState.ViewModelKeyList.Count > 1)
					collapsePointList = new(collapsePointList);
				
				var viewModel = editContext.GetViewModelModifier(viewModelKey);
			
				for (int i = 0; i < collapsePointList.Count; i++)
				{
					var collapsePoint = collapsePointList[i];
					
					var indexPreviousCollapsePoint = viewModel.PersistentState.AllCollapsePointList.FindIndex(
						x => x.Identifier == collapsePoint.Identifier);
						
					bool isCollapsed;
						
					if (indexPreviousCollapsePoint != -1)
					{
						if (viewModel.PersistentState.AllCollapsePointList[indexPreviousCollapsePoint].IsCollapsed)
						{
							collapsePoint.IsCollapsed = true;
							collapsePointList[i] = collapsePoint;
						}
					}
				}
				
				viewModel.PersistentState.AllCollapsePointList = collapsePointList;
				
				viewModel.ApplyCollapsePointState(editContext);
			}
		}
    }
    
    public string GetTextFromToken(SyntaxToken token)
    {
        if (token.TextSpan.Text == string.Empty)
        {
            var resource = GetResource(token.TextSpan.ResourceUri);
            var cSharpCompilationUnit = (CSharpCompilationUnit)resource.CompilationUnit;
            
            if (token.TextSpan.StartInclusiveIndex < cSharpCompilationUnit.SourceText.Length &&
                token.TextSpan.EndExclusiveIndex <= cSharpCompilationUnit.SourceText.Length &&
                token.TextSpan.EndExclusiveIndex >= token.TextSpan.StartInclusiveIndex)
            {
                return cSharpCompilationUnit.SourceText.Substring(
                    token.TextSpan.StartInclusiveIndex,
                    token.TextSpan.EndExclusiveIndex - token.TextSpan.StartInclusiveIndex);
            }
        }
        
        return token.TextSpan.Text;
    }
}