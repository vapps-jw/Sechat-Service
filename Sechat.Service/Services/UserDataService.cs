using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Sechat.Data.Repositories;
using Sechat.Service.Configuration;
using Sechat.Service.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sechat.Service.Services;

public class UserDataService
{
    private readonly CalendarRepository _calendarRepository;
    private readonly CryptographyService _cryptographyService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IMapper _mapper;
    private readonly UserRepository _userRepository;

    public UserDataService(
        CalendarRepository calendarRepository,
        CryptographyService cryptographyService,
        UserManager<IdentityUser> userManager,
        IMapper mapper,
        UserRepository userRepository)
    {
        _calendarRepository = calendarRepository;
        _cryptographyService = cryptographyService;
        _userManager = userManager;
        _mapper = mapper;
        _userRepository = userRepository;
    }

    public async Task<UserProfileProjection> GetProfile(string userId, string userName)
    {
        _userRepository.UpdateUserActivity(userId);
        _ = await _userRepository.SaveChanges();
        if (!_userRepository.ProfileExists(userId))
        {
            _userRepository.CreateUserProfile(userId, userName);
            if (await _userRepository.SaveChanges() == 0)
            {
                throw new Exception("Profile creation failed");
            }
        }
        if (!_calendarRepository.CalendarExists(userId))
        {
            _calendarRepository.CreateCalendar(userId);
            if (await _userRepository.SaveChanges() == 0)
            {
                throw new Exception("Calendar creation failed");
            }
        }
        if (!_userRepository.KeyExists(userId, Data.KeyType.DefaultEncryption))
        {
            var newKey = _cryptographyService.GenerateKey();
            _userRepository.UpdatKey(userId, Data.KeyType.DefaultEncryption, newKey);
            if (await _userRepository.SaveChanges() == 0)
            {
                throw new Exception("Defauly Key creation failed");
            }
        }
        if (!_userRepository.KeyExists(userId, Data.KeyType.AuthToken))
        {
            var newKey = _cryptographyService.GenerateKey();
            _userRepository.UpdatKey(userId, Data.KeyType.AuthToken, newKey);
            if (await _userRepository.SaveChanges() == 0)
            {
                throw new Exception("Auth Key creation failed");
            }
        }

        var user = await _userManager.FindByNameAsync(userName);
        var profileProjection = _mapper.Map<UserProfileProjection>(_userRepository.GetUserProfile(userId));
        profileProjection.UserId = userId;
        profileProjection.UserName = userName;
        profileProjection.Email = user.Email;
        profileProjection.EmailConfirmed = user.EmailConfirmed;

        var claims = await _userManager.GetClaimsAsync(user) as List<Claim>;
        profileProjection.Claims = claims
            .Where(c => c.Type.Equals(AppConstants.ClaimType.ServiceClaim))
            .Select(c => c.Value)
            .ToList(); 

        return profileProjection;
    }
}
