namespace Sechat.Service.Dtos.CryptoDtos;

public record DMKeyRequest(string Receipient, string KeyHolder, long Id);
public record DMSharedKey(string Receipient, string Key, long Id);

public record RoomKeyRequest(string Receipient, string Id);
public record RoomSharedKey(string Receipient, string Key, string Id);

public record MasterSharedKey(string Key, string Id);