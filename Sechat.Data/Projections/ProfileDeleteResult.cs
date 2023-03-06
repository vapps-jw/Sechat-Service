using Sechat.Data.Models;

namespace Sechat.Data.Projections;
public record ProfileDeleteResult(List<string> OwnedRooms, List<string> MemberRooms, List<UserConnection> Connections);
