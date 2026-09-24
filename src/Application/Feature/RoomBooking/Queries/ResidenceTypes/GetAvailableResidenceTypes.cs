using DHAFacilitationAPIs.Application.Common.Interfaces;
using DHAFacilitationAPIs.Application.Feature.RoomBooking.Queries.ResidenceTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DHAFacilitationAPIs.Application.Feature.RoomBooking.Queries.ResidenceTypes;

public record GetAvailableResidenceTypesQuery(Guid ClubId, Guid RoomCategoryId, DateTime CheckIn, DateTime CheckOut)
    : IRequest<List<AvailableResidenceTypeDto>>;

public class GetAvailableResidenceTypesQueryHandler
    : IRequestHandler<GetAvailableResidenceTypesQuery, List<AvailableResidenceTypeDto>>
{
    private readonly IOLMRSApplicationDbContext _context;

    public GetAvailableResidenceTypesQueryHandler(IOLMRSApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AvailableResidenceTypeDto>> Handle(GetAvailableResidenceTypesQuery request, CancellationToken cancellationToken)
    {
        // Find rooms already booked in given date range
        var bookedRoomIds = await _context.RoomAvailabilities
            .Where(b => request.CheckIn < b.FromDate && request.CheckOut > b.ToDate)
            .Select(b => b.RoomId)
            .ToListAsync(cancellationToken);

        // Get available residence types for given club & category
        var residenceTypes = await (
            from room in _context.Rooms
            join resType in _context.ResidenceTypes on room.ResidenceTypeId equals resType.Id
            where room.ClubId == request.ClubId
                  && room.RoomCategoryId == request.RoomCategoryId
                  && room.IsActive == true
                  && !room.IsDeleted == false
                  && !bookedRoomIds.Contains(room.Id)
            select new { resType.Name }
        )
        .Distinct()
        .Select(x => new AvailableResidenceTypeDto
        {
            Name = x.Name
        })
        .ToListAsync(cancellationToken);

        return residenceTypes;
    }
}
