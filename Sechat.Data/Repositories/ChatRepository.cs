﻿using Microsoft.EntityFrameworkCore;
using Sechat.Data.Models.ChatModels;
using Sechat.Data.Models.VideoCalls;
using Sechat.Data.QueryModels;

namespace Sechat.Data.Repositories;

public class ChatRepository : RepositoryBase<SechatContext>
{
    public ChatRepository(SechatContext context) : base(context) { }

    // Access

    public bool IsRoomAllowed(string userId, string roomId)
    {
        var members = _context.Rooms.Where(r => r.Id.Equals(roomId)).SelectMany(r => r.Members.Select(rm => rm.Id)).ToList();
        return members.Any(m => m.Equals(userId));
    }

    // Call Log

    public CallLog CreateNewCallLog(string caleeId, string phonerId)
    {
        var calee = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(caleeId));
        var phoner = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(phonerId));

        if (calee is null || phoner is null) return null;

        var newLog = new CallLog()
        {
            CalleeId = caleeId,
            CalleeName = calee.UserName,
            VideoCallResult = VideoCallResult.Unanswered,
            VideoCallType = VideoCallType.Outgoing
        };
        phoner.CallLogs.Add(newLog);
        return newLog;
    }

    public void LogCallAnswered(string caleeId, string phonerId)
    {
        var lastLog = _context.CallLogs
            .Where(cl => cl.UserProfileId.Equals(phonerId) && cl.CalleeId.Equals(caleeId))
            .OrderByDescending(cl => cl.Id)
            .FirstOrDefault();

        if (lastLog is null) return;
        lastLog.VideoCallResult = VideoCallResult.Answered;
        lastLog.WasViewed = true;
    }

    public void LogCallRejected(string caleeId, string phonerId)
    {
        var lastLog = _context.CallLogs
            .Where(cl => cl.UserProfileId.Equals(phonerId) && cl.CalleeId.Equals(caleeId))
            .OrderByDescending(cl => cl.Id)
            .FirstOrDefault();

        if (lastLog is null) return;
        lastLog.VideoCallResult = VideoCallResult.Rejected;
        lastLog.WasViewed = true;
    }

    public void MarkCallLogsAsViewed(string userId)
    {
        var logsToMark = _context.CallLogs
            .Where(cl => cl.CalleeId.Equals(userId) && !cl.WasViewed)
            .ToList();
        logsToMark.ForEach(cl => cl.WasViewed = true);
    }

    public List<CallLog> GetAllCallLogs(string userId) => _context.CallLogs
        .Where(cl => cl.UserProfileId.Equals(userId) || cl.CalleeId.Equals(userId))
        .Include(cl => cl.UserProfile)
        .ToList();

    public CallLog GetCallLog(long logId, string userId) => _context.CallLogs
        .Where(cl => cl.Id == logId && (cl.UserProfileId.Equals(userId) || cl.CalleeId.Equals(userId)))
        .Include(cl => cl.UserProfile)
        .FirstOrDefault();

    public List<CallLog> GetCallLogUpdates(string userId, long lastLog) => _context.CallLogs
        .Where(cl => cl.Id > lastLog && (cl.UserProfileId.Equals(userId) || cl.CalleeId.Equals(userId)))
        .Include(cl => cl.UserProfile)
        .ToList();

    // Messages

    public bool MessageExists(long messageId) => _context.Messages.Any(m => m.Id == messageId);

    public bool DirectMessageExists(long messageId) => _context.DirectMessages.Any(m => m.Id == messageId);

    public bool IsMessageAuthor(long messageId, string userId)
    {
        var msg = _context.Messages.Where(m => m.Id == messageId).FirstOrDefault();
        return msg is not null && msg.IdSentBy.Equals(userId);
    }

    public bool IsDirectMessageAuthor(long messageId, string userId)
    {
        var msg = _context.DirectMessages.Where(m => m.Id == messageId).FirstOrDefault();
        return msg is not null && msg.IdSentBy.Equals(userId);
    }

    public Message CreateMessage(string userId, string messageText, string roomId)
    {
        var profile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(userId));
        if (profile is null) return null;

        var room = _context.Rooms.FirstOrDefault(r => r.Id.Equals(roomId));
        if (room is null) return null;

        room.LastActivity = DateTime.UtcNow;
        profile.LastActivity = DateTime.UtcNow;

        var newMessage = new Message()
        {
            Text = messageText,
            IdSentBy = profile.Id,
            NameSentBy = profile.UserName,
            RoomId = room.Id,
            MessageViewers = new List<MessageViewer>() { new MessageViewer(userId) }
        };
        room.Messages.Add(newMessage);
        return newMessage;
    }

    public DirectMessage CreateDirectMessage(string userId, string messageText, long contactId)
    {
        var profile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(userId));
        if (profile is null) return null;

        var contact = _context.Contacts.FirstOrDefault(p => p.Id == contactId);
        if (contact is null) return null;

        var newMessage = new DirectMessage()
        {
            Text = messageText,
            IdSentBy = profile.Id,
            NameSentBy = profile.UserName,
        };
        contact.DirectMessages.Add(newMessage);
        return newMessage;
    }

    public void MarkMessagesAsViewed(string userId, string roomId)
    {
        var messages = _context.Messages
            .Where(m => m.RoomId.Equals(roomId) && !m.MessageViewers.Any(mv => mv.UserId.Equals(userId)))
            .ToList();

        messages.ForEach(m => m.MessageViewers.Add(new MessageViewer(userId)));
    }

    public void MarkMessageAsViewed(string userId, long messageId)
    {
        var message = _context.Messages
            .FirstOrDefault(m => m.Id == messageId && !m.MessageViewers.Any(mv => mv.UserId.Equals(userId)));

        message?.MessageViewers.Add(new MessageViewer(userId));
    }

    public void MarkDirectMessageAsViewed(string userId, long contactId, long messageId)
    {
        var message = _context.DirectMessages
            .FirstOrDefault(m => m.Id == messageId && m.ContactId == contactId && !m.IdSentBy.Equals(userId));
        message.WasViewed = true;
    }

    public void MarkDirectMessagesAsViewed(string userId, long contactId)
    {
        var messages = _context.DirectMessages
            .Where(m => m.ContactId == contactId && !m.IdSentBy.Equals(userId))
            .ExecuteUpdate(setters => setters
                .SetProperty(m => m.WasViewed, true));
    }

    public Task<int> DeleteMessage(string roomId, long messageId) => _context.Messages
        .Where(m => m.Id == messageId && m.RoomId.Equals(roomId))
        .ExecuteDeleteAsync();

    public Task<int> DeleteDirectMessage(long contactId, long messageId) => _context.DirectMessages
        .Where(m => m.Id == messageId && m.ContactId == contactId)
        .ExecuteDeleteAsync();

    // Rooms

    public Room CreateRoom(string roomName, string creatorUserId, string creatorName)
    {
        var profile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(creatorUserId));
        if (profile == null) return null;

        var newRoom = new Room() { Id = Guid.NewGuid().ToString(), CreatorId = creatorUserId, Name = roomName, CreatorName = creatorName };
        newRoom.Members.Add(profile);

        _ = _context.Rooms.Add(newRoom);
        return newRoom;
    }

    public string GetRoomName(string roomId) => _context.Rooms.Where(r => r.Id.Equals(roomId)).Select(r => r.Name).FirstOrDefault();

    public List<string> GetRoomMembersIds(string roomId) => _context.Rooms.Include(r => r.Members).FirstOrDefault(r => r.Id.Equals(roomId))?.Members.Select(m => m.Id).ToList();

    public async Task<List<Room>> GetRoomsWithMessages(string memberUserId)
    {
        var res = await _context.Rooms
        .Include(r => r.Messages)
            .ThenInclude(m => m.MessageViewers)
        .Where(r => r.Members.Any(m => m.Id.Equals(memberUserId))).Select(r => new Room()
        {
            LastActivity = r.LastActivity,
            Created = r.Created,
            CreatorName = r.CreatorName,
            Id = r.Id,
            Members = r.Members,
            Messages = r.Messages.OrderBy(m => m.Id).ToList(),
            Name = r.Name
        }).ToListAsync();
        return res;
    }

    public async Task<Room> GetRoomWithMessages(string roomId)
    {
        var res = await _context.Rooms
        .Where(r => r.Id.Equals(roomId))
        .Include(r => r.Messages)
            .ThenInclude(m => m.MessageViewers)
        .Select(r => new Room()
        {
            LastActivity = r.LastActivity,
            Created = r.Created,
            CreatorName = r.CreatorName,
            Id = r.Id,
            Members = r.Members,
            Messages = r.Messages.OrderBy(m => m.Id).ToList(),
            Name = r.Name
        }).FirstOrDefaultAsync();
        return res;
    }

    public async Task<List<Room>> GetRoomsWithMessages(string memberUserId, List<GetRoomUpdate> getRoomUpdates)
    {
        var rooms = await _context.Rooms
            .Where(r => r.Members.Any(m => m.Id.Equals(memberUserId)))
            .Select(r => new Room()
            {
                Name = r.Name,
                LastActivity = r.LastActivity,
                Created = r.Created,
                CreatorName = r.CreatorName,
                Id = r.Id,
                Members = r.Members
            }).ToListAsync();

        foreach (var room in rooms)
        {
            var updateData = getRoomUpdates.FirstOrDefault(ru => ru.RoomId.Equals(room.Id));
            if (updateData is not null)
            {
                room.Messages = _context.Messages
                    .Where(m => m.RoomId.Equals(room.Id))
                    .Include(m => m.MessageViewers)
                    .Where(m => m.Id > updateData.LastMessage)
                    .OrderBy(m => m.Id)
                    .ToList();
                continue;
            }

            room.Messages = _context.Messages
                .Where(m => m.RoomId.Equals(room.Id))
                .Include(m => m.MessageViewers)
                .OrderBy(m => m.Id)
                .ToList();
        }
        return rooms;
    }

    public Room AddToRoom(string roomId, string userId)
    {
        var room = _context.Rooms
            .Where(r => r.Id.Equals(roomId))
            .Include(r => r.Members)
            .FirstOrDefault();

        if (room is null) return null;
        var newMemberProfile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(userId));

        room.Members.Add(newMemberProfile);
        room.LastActivity = DateTime.UtcNow;
        return room;
    }

    public Room RemoveFromRoom(string roomId, string userId)
    {
        var room = _context.Rooms
            .Where(r => r.Id.Equals(roomId))
            .Include(r => r.Members)
            .FirstOrDefault();

        if (room is null) return null;
        if (room.CreatorId.Equals(userId)) return null;

        var memberProfile = _context.UserProfiles.FirstOrDefault(p => p.Id.Equals(userId));

        _ = room.Members.Remove(memberProfile);
        return room;
    }

    public bool IsRoomMember(string userId, string roomId) => _context.Rooms
        .Where(r => r.Id.Equals(roomId))
        .SelectMany(r => r.Members)
        .Any(m => m.Id.Equals(userId));

    public bool IsRoomsMember(string userId, List<string> roomId) =>
        _context.Rooms.Where(r => roomId.Contains(r.Id)).All(r => r.Members.Any(m => m.Id.Equals(userId)));

    public Task<Room> GetRoomWithNewMessages(string roomId, DateTime lastMessage) => _context.Rooms
        .Where(r => roomId.Contains(r.Id))
        .Include(r => r.Messages.Where(m => m.Created > lastMessage))
        .FirstOrDefaultAsync();

    public Task<int> DeleteRoom(string roomId, string creatorUserId) => _context.Rooms
        .Where(r => r.Id.Equals(roomId) && r.CreatorId.Equals(creatorUserId))
        .ExecuteDeleteAsync();
}

