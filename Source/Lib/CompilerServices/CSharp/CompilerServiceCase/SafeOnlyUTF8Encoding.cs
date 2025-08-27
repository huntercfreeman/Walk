namespace Walk.CompilerServices.CSharp.CompilerServiceCase;

using System.Text;

public class SafeOnlyUTF8Encoding : UTF8Encoding
{
    private Decoder? _decoder;

    public override Decoder GetDecoder()
    {
        _decoder ??= base.GetDecoder();
        _decoder.Reset();
        return _decoder;
    }
}

