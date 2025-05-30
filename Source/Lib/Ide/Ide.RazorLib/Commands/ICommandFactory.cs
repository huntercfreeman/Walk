using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Ide.RazorLib.Commands;

public interface ICommandFactory
{
	public IDialog? CodeSearchDialog { get; set; }

    public void Initialize();
    public ValueTask OpenCodeSearchDialog();
}
