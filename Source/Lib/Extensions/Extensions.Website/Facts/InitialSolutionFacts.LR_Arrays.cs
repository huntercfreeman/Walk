namespace Walk.Ide.Wasm.Facts;

public partial class InitialSolutionFacts
{
    public const string PERSON_CS_ABSOLUTE_FILE_PATH = @"/BlazorCrudApp/ConsoleApp/Persons/Person.cs";
    public const string PERSON_CS_CONTENTS =
"""""""""
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Menus.Models;

namespace Walk.Common.RazorLib.Contexts.Models;

public class ContextSwitchGroup
{
	public ContextSwitchGroup(
		Key<ContextSwitchGroup> key,
		string title,
		Func<Task<MenuRecord>> getMenuFunc)
	{
		Key = key;
		Title = title;
		GetMenuFunc = getMenuFunc;
	}

	public Key<ContextSwitchGroup> Key { get; }
	public string Title { get; }
	public Func<Task<MenuRecord>> GetMenuFunc { get; }
}

""""""""";
}
