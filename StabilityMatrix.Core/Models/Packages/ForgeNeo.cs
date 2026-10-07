using System.Collections.Immutable;
using Injectio.Attributes;
using StabilityMatrix.Core.Helper;
using StabilityMatrix.Core.Helper.Cache;
using StabilityMatrix.Core.Helper.HardwareInfo;
using StabilityMatrix.Core.Models;
using StabilityMatrix.Core.Models.Progress;
using StabilityMatrix.Core.Models.Rocm;
using StabilityMatrix.Core.Processes;
using StabilityMatrix.Core.Python;
using StabilityMatrix.Core.Services;
using StabilityMatrix.Core.Services.Rocm;

namespace StabilityMatrix.Core.Models.Packages;

[RegisterSingleton<BasePackage, ForgeNeo>(Duplicate = DuplicateStrategy.Append)]
public class ForgeNeo(
    IGithubApiCache githubApi,
    ISettingsManager settingsManager,
    IDownloadService downloadService,
    IPrerequisiteHelper prerequisiteHelper,
    IPyInstallationManager pyInstallationManager,
    IPipWheelService pipWheelService,
    IRocmPackageHelper rocmPackageHelper
)
    : ForgeClassic(
        githubApi,
        settingsManager,
        downloadService,
        prerequisiteHelper,
        pyInstallationManager,
        pipWheelService
    )
{
    /// <summary>
    /// Uses the shared ROCm helper for Windows ROCm eligibility checks so Forge Neo does not maintain
    /// its own support matrix.
    /// </summary>
    private bool HasWindowsRocmSupport()
    {
        return HasWindowsRocmSupport(rocmPackageHelper);
    }

    public override string Name => "forge-neo";
    public override string DisplayName { get; set; } = "Stable Diffusion WebUI Forge - Neo";
    public override string MainBranch => "neo";
    public override PackageType PackageType => PackageType.SdInference;

    // Windows-only ROCm comes from the shared helper; CUDA remains the only option everywhere else.
    public override IEnumerable<TorchIndex> AvailableTorchIndices =>
        Compat.IsWindows ? [TorchIndex.Cuda, TorchIndex.Rocm] : [TorchIndex.Cuda];

    public override bool IsCompatible => HardwareHelper.HasNvidiaGpu() || HasWindowsRocmSupport();

    public override TorchIndex GetRecommendedTorchVersion()
    {
        var preferNvidia = SettingsManager.Settings.PreferredGpu?.IsNvidia ?? HardwareHelper.HasNvidiaGpu();
        if (AvailableTorchIndices.Contains(TorchIndex.Cuda) && preferNvidia)
        {
            return TorchIndex.Cuda;
        }

        if (AvailableTorchIndices.Contains(TorchIndex.Rocm) && HasWindowsRocmSupport())
        {
            return TorchIndex.Rocm;
        }

        return base.GetRecommendedTorchVersion();
    }

    public override List<LaunchOptionDefinition> LaunchOptions
    {
        get
        {
            var options = new List<LaunchOptionDefinition>(base.LaunchOptions);
            var insertIndex = Math.Max(0, options.Count - 1);
            options.Insert(
                insertIndex,
                new LaunchOptionDefinition
                {
                    Name = "Bitsandbytes NF4",
                    Type = LaunchOptionType.Bool,
                    Description = "Install bitsandbytes for low-bits (NF4) inference",
                    Options = ["--bnb"],
                }
            );

            ForgeNeoWindowsRocmProfile.Default.ApplyWindowsRocmLaunchDefaults(options, rocmPackageHelper);

            return options;
        }
    }

    protected override async Task PrepareRocmInstallAsync(
        IPyVenvRunner venvRunner,
        InstalledPackage installedPackage,
        InstallPackageOptions options,
        Action<ProcessOutput>? onConsoleOutput,
        IProgress<ProgressReport>? progress,
        CancellationToken cancellationToken
    )
    {
        var torchIndex = options.PythonOptions.TorchIndex ?? GetRecommendedTorchVersion();
        if (!rocmPackageHelper.ShouldApplyWindowsLaunchEnvironment(torchIndex))
        {
            return;
        }

        var profile = ForgeNeoWindowsRocmProfile.Default;

        progress?.Report(
            new ProgressReport(-1f, "Preparing Windows ROCm environment...", isIndeterminate: true)
        );

        // Apply the ROCm launch environment (including the TORCH_COMMAND/TORCH_INDEX_URL override) so both
        // the helper-managed torch install and the subsequent upstream launch.py see it.
        venvRunner.UpdateEnvironmentVariables(env =>
            env.SetItems(rocmPackageHelper.BuildLaunchEnvironment(profile))
        );

        // Install the AMD multi-arch torch build before launch.py runs, so its is_installed("torch")
        // gate skips the default NVIDIA CUDA torch install.
        await rocmPackageHelper
            .InstallWindowsNativeTorchAsync(
                venvRunner,
                installedPackage,
                profile,
                progress,
                onConsoleOutput,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    protected override ImmutableDictionary<string, string> GetEnvVars(
        ImmutableDictionary<string, string> env,
        InstalledPackage installedPackage
    )
    {
        env = base.GetEnvVars(env, installedPackage);

        var selectedTorchIndex = installedPackage.PreferredTorchIndex ?? GetRecommendedTorchVersion();
        if (!rocmPackageHelper.ShouldApplyWindowsLaunchEnvironment(selectedTorchIndex))
        {
            return env;
        }

        return env.SetItems(rocmPackageHelper.BuildLaunchEnvironment(ForgeNeoWindowsRocmProfile.Default));
    }

    protected override IReadOnlyList<string> GetLaunchNoticeLines(InstalledPackage installedPackage)
    {
        var selectedTorchIndex = installedPackage.PreferredTorchIndex ?? GetRecommendedTorchVersion();
        return rocmPackageHelper.GetWindowsLaunchNoticeLines(selectedTorchIndex);
    }

    public override string Blurb =>
        "Neo mainly serves as an continuation for the \"latest\" version of Forge. Additionally, this fork is focused on optimization and usability, with the main goal of being the lightest WebUI without any bloatwares.";
}
