using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.Groups.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.TextEditor.RazorLib;

public partial class TextEditorService
{
    private readonly object Group_stateModificationLock = new();
    
    // TextEditorGroupService.cs
    private TextEditorGroupState Group_textEditorGroupState = new();

    public TextEditorGroupState Group_GetTextEditorGroupState() => Group_textEditorGroupState;

    public void Group_SetActiveViewModel(Key<TextEditorGroup> textEditorGroupKey, Key<TextEditorViewModel> textEditorViewModelKey)
    {
        Group_SetActiveViewModelOfGroup(
            textEditorGroupKey,
            textEditorViewModelKey);
    }

    public void Group_RemoveViewModel(Key<TextEditorGroup> textEditorGroupKey, Key<TextEditorViewModel> textEditorViewModelKey)
    {
        Group_RemoveViewModelFromGroup(
            textEditorGroupKey,
            textEditorViewModelKey);
    }

    public void Group_Register(Key<TextEditorGroup> textEditorGroupKey, Category? category = null)
    {
        category ??= new Category("main");

        var textEditorGroup = new TextEditorGroup(
            textEditorGroupKey,
            Key<TextEditorViewModel>.Empty,
            new List<Key<TextEditorViewModel>>(),
            category.Value,
            this,
            CommonService);

        Group_Register(textEditorGroup);
    }

    public TextEditorGroup? Group_GetOrDefault(Key<TextEditorGroup> textEditorGroupKey)
    {
        return Group_GetTextEditorGroupState().GroupList.FirstOrDefault(
            x => x.GroupKey == textEditorGroupKey);
    }

    public void Group_AddViewModel(Key<TextEditorGroup> textEditorGroupKey, Key<TextEditorViewModel> textEditorViewModelKey)
    {
        Group_AddViewModelToGroup(
            textEditorGroupKey,
            textEditorViewModelKey);
    }

    public List<TextEditorGroup> Group_GetGroups()
    {
        return Group_GetTextEditorGroupState().GroupList;
    }

    public void Group_Register(TextEditorGroup group)
    {
        lock (Group_stateModificationLock)
        {
            var inState = Group_GetTextEditorGroupState();

            var inGroup = inState.GroupList.FirstOrDefault(
                x => x.GroupKey == group.GroupKey);

            if (inGroup is not null)
                goto finalize;

            var outGroupList = new List<TextEditorGroup>(inState.GroupList);
            outGroupList.Add(group);

            Group_textEditorGroupState = new TextEditorGroupState
            {
                GroupList = outGroupList
            };

            goto finalize;
        }

        finalize:
        Group_TextEditorGroupStateChanged?.Invoke();
    }

    public void Group_AddViewModelToGroup(
        Key<TextEditorGroup> groupKey,
        Key<TextEditorViewModel> viewModelKey)
    {
        lock (Group_stateModificationLock)
        {
            var inState = Group_GetTextEditorGroupState();

            var inGroupIndex = inState.GroupList.FindIndex(
                x => x.GroupKey == groupKey);

            if (inGroupIndex == -1)
                goto finalize;

            var inGroup = inState.GroupList[inGroupIndex];

            if (inGroup is null)
                goto finalize;

            if (inGroup.ViewModelKeyList.Contains(viewModelKey))
                goto finalize;

            var outViewModelKeyList = new List<Key<TextEditorViewModel>>(inGroup.ViewModelKeyList);
            outViewModelKeyList.Add(viewModelKey);

            var outGroup = inGroup with
            {
                ViewModelKeyList = outViewModelKeyList
            };

            if (outGroup.ViewModelKeyList.Count == 1)
            {
                outGroup = outGroup with
                {
                    ActiveViewModelKey = viewModelKey
                };
            }

            var outGroupList = new List<TextEditorGroup>(inState.GroupList);
            outGroupList[inGroupIndex] = outGroup;

            Group_textEditorGroupState = new TextEditorGroupState
            {
                GroupList = outGroupList
            };

            goto finalize;
        }

        finalize:
        Group_TextEditorGroupStateChanged?.Invoke();
        Group_PostScroll(groupKey, viewModelKey);
    }

    public void Group_RemoveViewModelFromGroup(
        Key<TextEditorGroup> groupKey,
        Key<TextEditorViewModel> viewModelKey)
    {
        lock (Group_stateModificationLock)
        {
            var inState = Group_GetTextEditorGroupState();

            var inGroupIndex = inState.GroupList.FindIndex(
                x => x.GroupKey == groupKey);

            if (inGroupIndex == -1)
                goto finalize;

            var inGroup = inState.GroupList[inGroupIndex];

            if (inGroup is null)
                goto finalize;

            var indexOfViewModelKeyToRemove = inGroup.ViewModelKeyList.FindIndex(
                x => x == viewModelKey);

            if (indexOfViewModelKeyToRemove == -1)
                goto finalize;

            var viewModelKeyToRemove = inGroup.ViewModelKeyList[indexOfViewModelKeyToRemove];

            var nextViewModelKeyList = new List<Key<TextEditorViewModel>>(inGroup.ViewModelKeyList);
            nextViewModelKeyList.RemoveAt(indexOfViewModelKeyToRemove);

            Key<TextEditorViewModel> nextActiveTextEditorModelKey;

            if (inGroup.ActiveViewModelKey != Key<TextEditorViewModel>.Empty &&
                inGroup.ActiveViewModelKey != viewModelKeyToRemove)
            {
                // Because the active tab was not removed, do not bother setting a different
                // active tab.
                nextActiveTextEditorModelKey = inGroup.ActiveViewModelKey;
            }
            else
            {
                // The active tab was removed, therefore a new active tab must be chosen.

                // This variable is done for renaming
                var activeViewModelKeyIndex = indexOfViewModelKeyToRemove;

                // If last item in list
                if (activeViewModelKeyIndex >= inGroup.ViewModelKeyList.Count - 1)
                {
                    activeViewModelKeyIndex--;
                }
                else
                {
                    // ++ operation because this calculation is using the immutable list where
                    // the view model was not removed.
                    activeViewModelKeyIndex++;
                }

                // If removing the active will result in empty list set the active as an Empty TextEditorViewModelKey
                if (inGroup.ViewModelKeyList.Count - 1 == 0)
                    nextActiveTextEditorModelKey = Key<TextEditorViewModel>.Empty;
                else
                    nextActiveTextEditorModelKey = inGroup.ViewModelKeyList[activeViewModelKeyIndex];
            }

            var outGroupList = new List<TextEditorGroup>(inState.GroupList);

            outGroupList[inGroupIndex] = inGroup with
            {
                ViewModelKeyList = nextViewModelKeyList,
                ActiveViewModelKey = nextActiveTextEditorModelKey
            };

            Group_textEditorGroupState = new TextEditorGroupState
            {
                GroupList = outGroupList
            };

            goto finalize;
        }

        finalize:
        Group_TextEditorGroupStateChanged?.Invoke();
        Group_PostScroll(groupKey, Group_GetOrDefault(groupKey).ActiveViewModelKey);
    }

    public void Group_SetActiveViewModelOfGroup(
        Key<TextEditorGroup> groupKey,
        Key<TextEditorViewModel> viewModelKey)
    {
        lock (Group_stateModificationLock)
        {
            var inState = Group_GetTextEditorGroupState();

            var inGroupIndex = inState.GroupList.FindIndex(
                x => x.GroupKey == groupKey);

            if (inGroupIndex == -1)
                goto finalize;

            var inGroup = inState.GroupList[inGroupIndex];

            if (inGroup is null)
                goto finalize;

            var outGroupList = new List<TextEditorGroup>(inState.GroupList);

            outGroupList[inGroupIndex] = inGroup with
            {
                ActiveViewModelKey = viewModelKey
            };

            Group_textEditorGroupState = new TextEditorGroupState
            {
                GroupList = outGroupList
            };

            goto finalize;
        }

        finalize:
        Group_PostScroll(groupKey, viewModelKey);
        Group_TextEditorGroupStateChanged?.Invoke();
    }

    public void Group_Dispose(Key<TextEditorGroup> groupKey)
    {
        lock (Group_stateModificationLock)
        {
            var inState = Group_GetTextEditorGroupState();

            var inGroup = inState.GroupList.FirstOrDefault(
                x => x.GroupKey == groupKey);

            if (inGroup is null)
                goto finalize;

            var outGroupList = new List<TextEditorGroup>(inState.GroupList);
            outGroupList.Remove(inGroup);

            Group_textEditorGroupState = new TextEditorGroupState
            {
                GroupList = outGroupList
            };

            goto finalize;
        }

        finalize:
        Group_TextEditorGroupStateChanged?.Invoke();
    }

    private void Group_PostScroll(
        Key<TextEditorGroup> groupKey,
        Key<TextEditorViewModel> viewModelKey)
    {
        WorkerArbitrary.PostUnique(editContext =>
        {
            var viewModelModifier = editContext.GetViewModelModifier(viewModelKey);
            if (viewModelModifier is null)
                return ValueTask.CompletedTask;

            viewModelModifier.ScrollWasModified = true;
            return ValueTask.CompletedTask;
        });
    }
}
