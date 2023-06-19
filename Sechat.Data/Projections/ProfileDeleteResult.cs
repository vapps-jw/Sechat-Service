using Sechat.Data.Models.UserDetails;

namespace Sechat.Data.Projections;
public record ProfileDeleteResult(List<string> OwnedRooms, List<string> MemberRooms, List<UserConnection> Connections);
