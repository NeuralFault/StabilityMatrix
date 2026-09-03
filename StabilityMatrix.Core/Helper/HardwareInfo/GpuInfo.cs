using StabilityMatrix.Core.Models.Rocm;

namespace StabilityMatrix.Core.Helper.HardwareInfo;

public record GpuInfo
{
    public int Index { get; set; }
    public string? Name { get; init; } = string.Empty;
    public ulong MemoryBytes { get; init; }
    public string? ComputeCapability { get; init; }
    public string? VendorId { get; init; }
    public string? DeviceId { get; init; }

    /// <summary>
    /// Gets the compute capability as a comparable decimal value (e.g. "8.6" becomes 8.6m)
    /// Returns null if compute capability is not available
    /// </summary>
    public decimal? ComputeCapabilityValue =>
        ComputeCapability != null && decimal.TryParse(ComputeCapability, out var value) ? value : null;

    public MemoryLevel? MemoryLevel =>
        MemoryBytes switch
        {
            <= 0 => HardwareInfo.MemoryLevel.Unknown,
            < 4 * Size.GiB => HardwareInfo.MemoryLevel.Low,
            < 8 * Size.GiB => HardwareInfo.MemoryLevel.Medium,
            _ => HardwareInfo.MemoryLevel.High,
        };

    public bool IsNvidia
    {
        get
        {
            var name = Name?.ToLowerInvariant();

            if (string.IsNullOrEmpty(name))
                return false;

            return name.Contains("nvidia") || name.Contains("tesla");
        }
    }

    public bool IsBlackwellGpu()
    {
        if (ComputeCapability is null)
            return false;

        return ComputeCapabilityValue >= 12.0m;
    }

    public bool IsAmpereOrNewerGpu()
    {
        if (ComputeCapability is null)
            return false;

        return ComputeCapabilityValue >= 8.6m;
    }

    public bool IsLegacyNvidiaGpu()
    {
        if (ComputeCapability is null)
            return false;

        return ComputeCapabilityValue < 7.5m;
    }

    public bool IsWindowsRocmSupportedGpu()
    {
        return RocmSupport.IsSupportedGpu(this);
    }

    public bool IsAmd =>
        (Name?.Contains("amd", StringComparison.OrdinalIgnoreCase) ?? false) || VendorId == "1002";
    public bool IsIntel => Name?.Contains("arc", StringComparison.OrdinalIgnoreCase) ?? false;

    public string? GetAmdGfxArch()
    {
        // PCI device IDs are authoritative when present (Linux lspci -nn enumeration).
        if (!string.IsNullOrWhiteSpace(DeviceId))
        {
            var deviceGfxArch = RadeonGfxArchMap.GetGfxArchFromDeviceId(DeviceId);
            if (deviceGfxArch is not null)
                return deviceGfxArch;
        }

        if (!IsAmd || string.IsNullOrWhiteSpace(Name))
            return null;

        return RadeonGfxArchMap.GetGfxArchFromName(Name);
    }

    public virtual bool Equals(GpuInfo? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return Name == other.Name && MemoryBytes == other.MemoryBytes;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Index, Name, MemoryBytes);
    }
}
