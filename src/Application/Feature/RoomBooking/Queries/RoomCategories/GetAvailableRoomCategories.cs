using AutoMapper;
using DHAFacilitationAPIs.Application.Common.Interfaces;
using DHAFacilitationAPIs.Application.Feature.RoomBooking.Queries.RoomCategories.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DHAFacilitationAPIs.Application.Feature.RoomBooking.Queries.RoomCategories;

public record GetAvailableRoomCategoriesQuery(Guid ClubId, DateTime CheckInDate, DateTime CheckOutDate) : IRequest<List<AvailableRoomCategoryDto>>;

public class GetAvailableRoomCategoriesQueryHandler : IRequestHandler<GetAvailableRoomCategoriesQuery, List<AvailableRoomCategoryDto>>
{
    private readonly IOLMRSApplicationDbContext _context;

    public GetAvailableRoomCategoriesQueryHandler(IOLMRSApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AvailableRoomCategoryDto>> Handle(GetAvailableRoomCategoriesQuery request, CancellationToken cancellationToken)
    {
        var bookedRoomIds = await _context.RoomAvailabilities
            .Where(rb =>
                rb.IsActive == true &&
                !rb.IsDeleted == false &&
                // Overlap logic: (start < checkout) AND (end > checkin)
                rb.FromDate < request.CheckOutDate &&
                rb.ToDate > request.CheckInDate)
            .Select(rb => rb.RoomId)
            .ToListAsync(cancellationToken);

        var categories = await (
            from room in _context.Rooms
            join category in _context.RoomCategories on room.RoomCategoryId equals category.Id
            where room.ClubId == request.ClubId
                && room.IsActive == true
                && !room.IsDeleted == false
                && !bookedRoomIds.Contains(room.Id)
            select new { category.Name })
            .Distinct() // works because EF can compare these primitives
            .Select(x => new AvailableRoomCategoryDto{
                Name = x.Name
                }).ToListAsync(cancellationToken);

        return categories;
    }
}
