using System;

namespace janzi.Logging;

internal sealed class NullScope : IDisposable
{
    public static NullScope Instance { get; } = new NullScope();
    private NullScope() { }
    public void Dispose() { GC.SuppressFinalize(this);}
}
