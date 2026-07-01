using HotelSearch.Application.Hotels;
using HotelSearch.Application.Hotels.Commands;
using HotelSearch.Domain.Hotels;
using Moq;

namespace HotelSearch.Tests.Application;

public class HotelServiceTests
{
    private readonly Mock<IHotelRepository> _repository = new();
    private readonly HotelService _sut;

    public HotelServiceTests()
    {
        _sut = new HotelService(_repository.Object);
    }

    [Fact]
    public async Task CreateAsync_persists_and_returns_hotel()
    {
        Hotel? persisted = null;
        _repository.Setup(r => r.AddAsync(It.IsAny<Hotel>(), It.IsAny<CancellationToken>()))
            .Callback<Hotel, CancellationToken>((hotel, _) => persisted = hotel)
            .Returns(Task.CompletedTask);

        var response = await _sut.CreateAsync(
            new CreateHotelCommand("Sea View", 99m, 43.5, 16.4));

        Assert.NotNull(persisted);
        Assert.Equal("Sea View", response.Name);
        Assert.Equal(99m, response.Price);
        Assert.Equal(persisted!.Id, response.Id);
        _repository.Verify(r => r.AddAsync(It.IsAny<Hotel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_hotel_not_found()
    {
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Hotel?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_returns_mapped_hotel()
    {
        var id = Guid.NewGuid();
        var hotel = new Hotel(id, "Lake View", new Money(80m), new GeoLocation(45.1, 15.1));
        _repository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);

        var result = await _sut.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.Equal("Lake View", result.Name);
        Assert.Equal(80m, result.Price);
    }

    [Fact]
    public async Task ListAsync_returns_all_hotels_mapped()
    {
        var hotels = new List<Hotel>
        {
            new(Guid.NewGuid(), "Alpha", new Money(100m), new GeoLocation(45.0, 15.0)),
            new(Guid.NewGuid(), "Beta", new Money(200m), new GeoLocation(46.0, 16.0)),
        };
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotels);

        var result = await _sut.ListAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Alpha", result[0].Name);
        Assert.Equal("Beta", result[1].Name);
    }

    [Fact]
    public async Task UpdateAsync_returns_null_when_hotel_not_found()
    {
        var id = Guid.NewGuid();
        _repository.Setup(r => r.GetByIdForUpdateAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Hotel?)null);

        var result = await _sut.UpdateAsync(
            id,
            new UpdateHotelCommand("Updated", 100m, 45.0, 15.0));

        Assert.Null(result);
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_persists_changes_and_returns_hotel()
    {
        var id = Guid.NewGuid();
        var hotel = new Hotel(id, "Before", new Money(90m), new GeoLocation(45.0, 15.0));
        _repository.Setup(r => r.GetByIdForUpdateAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotel);
        _repository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.UpdateAsync(
            id,
            new UpdateHotelCommand("After", 120m, 45.5, 15.5));

        Assert.NotNull(result);
        Assert.Equal("After", result!.Name);
        Assert.Equal(120m, result.Price);
        Assert.Equal(45.5, result.Latitude);
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_returns_false_when_hotel_not_found()
    {
        _repository.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var deleted = await _sut.DeleteAsync(Guid.NewGuid());

        Assert.False(deleted);
        _repository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_returns_true_when_hotel_deleted()
    {
        var id = Guid.NewGuid();
        _repository.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var deleted = await _sut.DeleteAsync(id);

        Assert.True(deleted);
    }
}
