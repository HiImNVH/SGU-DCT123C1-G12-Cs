using Microsoft.Extensions.Logging.Abstractions;
using TravelGuide.API.Repositories;
using TravelGuide.API.Services;
using TravelGuide.Core.Models;

namespace TravelGuide.Tests;

public class POIServiceTests
{
    [Fact]
    public async Task GetByIdAsync_WhenPoiDoesNotExist_ReturnsNull()
    {
        var service = new POIService(new FakePoiRepository(), NullLogger<POIService>.Instance);

        var result = await service.GetByIdAsync(Guid.NewGuid(), "en");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRequestedLanguageIsMissing_FallsBackToVietnamese()
    {
        var poi = new POI
        {
            Name = "Bưu điện Thành phố",
            Category = "Kiến trúc",
            Contents =
            [
                new POIContent
                {
                    LanguageCode = "vi",
                    NarrationText = "Nội dung tiếng Việt"
                }
            ]
        };
        var service = new POIService(new FakePoiRepository(poi), NullLogger<POIService>.Instance);

        var result = await service.GetByIdAsync(poi.Id, "ja");

        Assert.NotNull(result);
        Assert.Equal("vi", result.Content?.LanguageCode);
        Assert.Equal("Nội dung tiếng Việt", result.Content?.NarrationText);
    }

    [Fact]
    public async Task GetAllAsync_WithActiveFilter_ReturnsOnlyActivePois()
    {
        var repository = new FakePoiRepository(
            new POI { Name = "Active", Category = "Test", IsActive = true },
            new POI { Name = "Inactive", Category = "Test", IsActive = false });
        var service = new POIService(repository, NullLogger<POIService>.Instance);

        var result = await service.GetAllAsync(active: true);

        var poi = Assert.Single(result);
        Assert.True(poi.IsActive);
        Assert.Equal("Active", poi.Name);
    }

    private sealed class FakePoiRepository(params POI[] pois) : IPOIRepository
    {
        private readonly List<POI> _pois = [.. pois];

        public Task<POI?> GetByIdWithContentAsync(Guid id, string lang) =>
            Task.FromResult(_pois.FirstOrDefault(poi => poi.Id == id && poi.IsActive));

        public Task<List<POI>> GetAllActiveAsync() =>
            Task.FromResult(_pois.Where(poi => poi.IsActive).ToList());

        public Task<POI?> GetByIdAsync(Guid id) =>
            Task.FromResult(_pois.FirstOrDefault(poi => poi.Id == id));

        public Task<Guid> AddAsync(POI poi)
        {
            _pois.Add(poi);
            return Task.FromResult(poi.Id);
        }

        public Task UpdateAsync(POI poi) => Task.CompletedTask;

        public Task SoftDeleteAsync(Guid id)
        {
            var poi = _pois.FirstOrDefault(item => item.Id == id);
            if (poi is not null) poi.IsActive = false;
            return Task.CompletedTask;
        }

        public Task UpsertContentAsync(POIContent content) => Task.CompletedTask;
        public Task<List<POI>> GetAllAsync() => Task.FromResult(_pois.ToList());
        public Task<List<POI>> GetAllByActiveStatusAsync(bool isActive) =>
            Task.FromResult(_pois.Where(poi => poi.IsActive == isActive).ToList());
    }
}
