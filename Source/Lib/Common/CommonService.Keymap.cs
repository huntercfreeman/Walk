using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keymaps.Models;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    private KeymapState _keymapState = new();
    
    public KeymapState GetKeymapState() => _keymapState;
    
    public void RegisterKeymapLayer(KeymapLayer keymapLayer)
    {
        lock (_stateModificationLock)
        {
            var inState = GetKeymapState();

            if (!inState.KeymapLayerList.Any(x => x.Key == keymapLayer.Key))
            {
                var outKeymapLayerList = new List<KeymapLayer>(inState.KeymapLayerList);
                outKeymapLayerList.Add(keymapLayer);
    
                _keymapState = inState with
                {
                    KeymapLayerList = outKeymapLayerList
                };
            }
        }

        KeymapStateChanged?.Invoke();
    }
    
    public void DisposeKeymapLayer(int keymapLayerKey)
    {
        lock (_stateModificationLock)
        {
            var inState = GetKeymapState();

            var indexExisting = inState.KeymapLayerList.FindIndex(x => x.Key == keymapLayerKey);

            if (indexExisting != -1)
            {
                var outKeymapLayerList = new List<KeymapLayer>(inState.KeymapLayerList);
                outKeymapLayerList.RemoveAt(indexExisting);
    
                _keymapState = inState with
                {
                    KeymapLayerList = outKeymapLayerList
                };
            }
        }

        KeymapStateChanged?.Invoke();
    }
}
