namespace StabilityMatrix.Core.Models;

/// <summary>
/// Torch install channel. Values with an Amd prefix use the AMD multi-arch repo; the rest use the upstream PyTorch index.
/// </summary>
public enum TorchChannel
{
    Stable,
    Nightly,
    AmdStable,
    AmdNightly,
    AmdPrerelease,
}
