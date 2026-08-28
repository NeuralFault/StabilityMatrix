using System.Net.Http;
using System.Text;
using NSubstitute;
using StabilityMatrix.Core.Helper;
using StabilityMatrix.Core.Helper.Cache;
using StabilityMatrix.Core.Services;

namespace StabilityMatrix.Tests.Core;

[TestClass]
public class PipWheelServiceTorchDiscoveryTests
{
    private const string StableTorchWheelIndexUrl = "https://download.pytorch.org/whl/torch/";
    private const string NightlyTorchWheelIndexUrl = "https://download.pytorch.org/whl/nightly/torch/";

    // Mirrors the real download.pytorch.org/whl/torch/ listing: multiple torch versions,
    // multiple cu/rocm/cpu variants for the latest version.
    private const string StableListing = """
        <!DOCTYPE html>
        <html>
        <body>
        <a href="torch-2.11.0%2Bcu118-cp310-cp310-manylinux_2_28_x86_64.whl">torch-2.11.0+cu118</a>
        <a href="torch-2.12.0%2Bcu124-cp310-cp310-manylinux_2_28_x86_64.whl">torch-2.12.0+cu124</a>
        <a href="torch-2.13.0%2Bcu126-cp310-cp310-manylinux_2_28_x86_64.whl">torch-2.13.0+cu126</a>
        <a href="torch-2.13.0%2Bcu129-cp312-cp312-manylinux_2_28_x86_64.whl">torch-2.13.0+cu129</a>
        <a href="torch-2.13.0%2Bcu130-cp312-cp312-manylinux_2_28_x86_64.whl">torch-2.13.0+cu130</a>
        <a href="torch-2.13.0%2Bcu132-cp312-cp312-manylinux_2_28_x86_64.whl">torch-2.13.0+cu132</a>
        <a href="torch-2.13.0%2Brocm7.1-cp312-cp312-manylinux_2_28_x86_64.whl">torch-2.13.0+rocm7.1</a>
        <a href="torch-2.13.0%2Brocm7.2-cp312-cp312-manylinux_2_28_x86_64.whl">torch-2.13.0+rocm7.2</a>
        <a href="torch-2.13.0%2Bcpu-cp312-cp312-manylinux_2_28_x86_64.whl">torch-2.13.0+cpu</a>
        </body>
        </html>
        """;

    // Mirrors the real download.pytorch.org/whl/nightly/torch/ listing: a single dev version
    // with the current cu/rocm variants.
    private const string NightlyListing = """
        <!DOCTYPE html>
        <html>
        <body>
        <a href="torch-2.15.0.dev20260828%2Bcu126-cp312-cp312-linux_x86_64.whl">torch-2.15.0.dev20260828+cu126</a>
        <a href="torch-2.15.0.dev20260828%2Bcu130-cp312-cp312-linux_x86_64.whl">torch-2.15.0.dev20260828+cu130</a>
        <a href="torch-2.15.0.dev20260828%2Bcu132-cp312-cp312-linux_x86_64.whl">torch-2.15.0.dev20260828+cu132</a>
        <a href="torch-2.15.0.dev20260828%2Bcu134-cp312-cp312-linux_x86_64.whl">torch-2.15.0.dev20260828+cu134</a>
        <a href="torch-2.15.0.dev20260828%2Brocm7.14-cp312-cp312-linux_x86_64.whl">torch-2.15.0.dev20260828+rocm7.14</a>
        </body>
        </html>
        """;

    private const string NoCudaListing = """
        <a href="torch-2.13.0%2Brocm7.2-cp312-cp312-manylinux_2_28_x86_64.whl">torch-2.13.0+rocm7.2</a>
        <a href="torch-2.13.0%2Bcpu-cp312-cp312-manylinux_2_28_x86_64.whl">torch-2.13.0+cpu</a>
        """;

    private const string NoRocmListing = """
        <a href="torch-2.13.0%2Bcu126-cp312-cp312-manylinux_2_28_x86_64.whl">torch-2.13.0+cu126</a>
        <a href="torch-2.13.0%2Bcu132-cp312-cp312-manylinux_2_28_x86_64.whl">torch-2.13.0+cu132</a>
        """;

    [TestMethod]
    public async Task GetAvailableCudaIndexesAsync_Stable_ReturnsOnlyLatestVersionCudaIndexes()
    {
        var service = CreateService(CreateDownloadService(StableListing));

        var result = await service.GetAvailableCudaIndexesAsync(nightly: false);

        CollectionAssert.AreEqual(new[] { "cu132", "cu130", "cu129", "cu126" }, result.ToList());
    }

    [TestMethod]
    public async Task GetAvailableCudaIndexesAsync_Nightly_ReturnsDistinctCudaIndexesSortedDescending()
    {
        var service = CreateService(CreateDownloadService(NightlyListing));

        var result = await service.GetAvailableCudaIndexesAsync(nightly: true);

        CollectionAssert.AreEqual(new[] { "cu134", "cu132", "cu130", "cu126" }, result.ToList());
    }

    [TestMethod]
    public async Task GetAvailableCudaIndexesAsync_ReturnsEmpty_WhenListingHasNoCudaWheels()
    {
        var service = CreateService(CreateDownloadService(NoCudaListing));

        var result = await service.GetAvailableCudaIndexesAsync(nightly: false);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetAvailableCudaIndexesAsync_ReturnsEmpty_WhenDownloadFails()
    {
        var downloadService = CreateFailingDownloadService();
        var service = CreateService(downloadService);

        var result = await service.GetAvailableCudaIndexesAsync(nightly: false);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetLatestUpstreamRocmIndexAsync_Stable_ReturnsHighestRocmForLatestTorchVersion()
    {
        var service = CreateService(CreateDownloadService(StableListing));

        var result = await service.GetLatestUpstreamRocmIndexAsync(nightly: false);

        Assert.AreEqual("rocm7.2", result);
    }

    [TestMethod]
    public async Task GetLatestUpstreamRocmIndexAsync_Nightly_ReturnsRocmVersion()
    {
        var service = CreateService(CreateDownloadService(NightlyListing));

        var result = await service.GetLatestUpstreamRocmIndexAsync(nightly: true);

        Assert.AreEqual("rocm7.14", result);
    }

    [TestMethod]
    public async Task GetLatestUpstreamRocmIndexAsync_ReturnsNull_WhenListingHasNoRocmWheels()
    {
        var service = CreateService(CreateDownloadService(NoRocmListing));

        var result = await service.GetLatestUpstreamRocmIndexAsync(nightly: false);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetLatestUpstreamRocmIndexAsync_ReturnsNull_WhenDownloadFails()
    {
        var downloadService = CreateFailingDownloadService();
        var service = CreateService(downloadService);

        var result = await service.GetLatestUpstreamRocmIndexAsync(nightly: false);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task TorchListing_CachedPerChannel_FetchesEachChannelOnce()
    {
        var downloadService = CreateDownloadService(StableListing);
        downloadService
            .GetContentAsync(NightlyTorchWheelIndexUrl, Arg.Any<CancellationToken>())
            .Returns(StreamOf(NightlyListing));
        var service = CreateService(downloadService);

        await service.GetAvailableCudaIndexesAsync(nightly: false);
        await service.GetAvailableCudaIndexesAsync(nightly: false);
        await service.GetLatestUpstreamRocmIndexAsync(nightly: true);
        await service.GetLatestUpstreamRocmIndexAsync(nightly: true);

        downloadService.Received(1).GetContentAsync(StableTorchWheelIndexUrl, Arg.Any<CancellationToken>());
        downloadService.Received(1).GetContentAsync(NightlyTorchWheelIndexUrl, Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task TorchListing_FailedFetch_IsNotCached_RefetchesOnNextCall()
    {
        var callCount = 0;
        var downloadService = Substitute.For<IDownloadService>();
        downloadService
            .GetContentAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callCount++;
                return callCount == 1
                    ? Task.FromException<Stream>(new HttpRequestException("Simulated network failure"))
                    : Task.FromResult<Stream>(StreamOf(StableListing));
            });
        var service = CreateService(downloadService);

        var first = await service.GetAvailableCudaIndexesAsync(nightly: false);
        var second = await service.GetAvailableCudaIndexesAsync(nightly: false);

        Assert.AreEqual(0, first.Count);
        CollectionAssert.AreEqual(new[] { "cu132", "cu130", "cu129", "cu126" }, second.ToList());
        downloadService.Received(2).GetContentAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private static PipWheelService CreateService(IDownloadService downloadService) =>
        new(Substitute.For<IGithubApiCache>(), downloadService, Substitute.For<IPrerequisiteHelper>());

    private static IDownloadService CreateDownloadService(string listing)
    {
        var downloadService = Substitute.For<IDownloadService>();
        downloadService
            .GetContentAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(StreamOf(listing));
        return downloadService;
    }

    private static IDownloadService CreateFailingDownloadService()
    {
        var downloadService = Substitute.For<IDownloadService>();
        downloadService
            .GetContentAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Stream>(new HttpRequestException("Simulated network failure")));
        return downloadService;
    }

    private static MemoryStream StreamOf(string text) => new(Encoding.UTF8.GetBytes(text));
}
