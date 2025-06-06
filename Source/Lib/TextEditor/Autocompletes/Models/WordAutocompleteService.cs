namespace Walk.TextEditor.RazorLib.Autocompletes.Models;

public class WordAutocompleteService : IAutocompleteService
{
    private readonly IAutocompleteIndexer _autocompleteIndexer;
    private readonly List<string> _emptyResult = new();

    public WordAutocompleteService(IAutocompleteIndexer autocompleteIndexer)
    {
        _autocompleteIndexer = autocompleteIndexer;
    }

    public List<string> GetAutocompleteOptions(string word)
    {
    	// (2025-01-21)
    	// ==============================================
    	// Value does not fall within the expected range.
		// 	at System.Collections.Immutable.ImmutableExtensions.ToArray[T](IEnumerable`1 sequence, Int32 count)
		// 	at System.Collections.Immutable.ImmutableArray.CreateRange[T](IEnumerable`1 items)
		// 	at Walk.TextEditor.RazorLib.Autocompletes.Models.WordAutocompleteService.GetAutocompleteOptions(String word) in C:\Users\hunte\Repos\Walk.Ide_Fork\Source\Lib\TextEditor\Autocompletes\Models\WordAutocompleteService.cs:line 14
		// 	at Walk.TextEditor.RazorLib.TextEditors.Displays.Internals.AutocompleteMenu.GetMenuRecord()
		// 	at Walk.TextEditor.RazorLib.TextEditors.Displays.Internals.AutocompleteMenu.<>c__DisplayClass33_0.<BuildRenderTree>b__1(RenderTreeBuilder __builder2)
		// 	at Microsoft.AspNetCore.Components.CascadingValue`1.Render(RenderTreeBuilder builder)
		// 	at Microsoft.AspNetCore.Components.Rendering.ComponentState.RenderIntoBatch(RenderBatchBuilder batchBuilder, RenderFragment renderFragment, Exception& renderFragmentException)
    	try
    	{
	        var indexedStrings = _autocompleteIndexer.IndexedStringsList;
	        return new List<string>(indexedStrings.Where(x => x.StartsWith(word)).Take(5));
        }
        catch (Exception e)
        {
        	Console.WriteLine(e);
        	return _emptyResult;
        }
    }
}
