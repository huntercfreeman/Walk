using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Walk.Ide.RazorLib.FileSystems.Models;

namespace Walk.Ide.RazorLib;

public partial class IdeService
{
	/// <summary>
	/// The order of the entries in <see cref="_fileTemplatesList"/> is important
	/// as the .FirstOrDefault(x => ...true...) is used.
	/// </summary>
	private List<IFileTemplate> _fileTemplatesList = new()
	{
		FileTemplateFacts.RazorCodebehind,
		FileTemplateFacts.RazorMarkup,
		FileTemplateFacts.CSharpClass
	};

	public List<IFileTemplate> FileTemplatesList => _fileTemplatesList;
}
