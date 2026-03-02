using Xunit;

namespace ASD.NES.Tests;

/// <summary> CPU tests share singleton CPU address space; run sequentially. </summary>
[CollectionDefinition("CPU", DisableParallelization = true)]
public sealed class CpuCollection { }
