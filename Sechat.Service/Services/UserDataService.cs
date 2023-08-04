using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Sechat.Data.Repositories;
using Sechat.Service.Dtos;
using System;
using System.Threading.Tasks;

namespace Sechat.Service.Services;

public class UserDataService
{
    private readonly CryptographyService _cryptographyService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IMapper _mapper;
    private readonly UserRepository _userRepository;

    public UserDataService(
         CryptographyService cryptographyService,
        UserManager<IdentityUser> userManager,
        IMapper mapper,
        UserRepository userRepository)
    {
        _cryptographyService = cryptographyService;
        _userManager = userManager;
        _mapper = mapper;
        _userRepository = userRepository;
    }

    public async Task<UserProfileProjection> GetProfile(string userId, string userName)
    {
        _userRepository.UpdateUserActivity(userId);
        if (!_userRepository.ProfileExists(userId))
        {
            _userRepository.CreateUserProfile(userId, userName);
            if (await _userRepository.SaveChanges() == 0)
            {
                throw new Exception("Profile creation failed");
            }
        }
        if (!_userRepository.KeyExists(userId, Data.KeyType.DefaultEncryption))
        {
            var newKey = _cryptographyService.GenerateKey();
            _userRepository.UpdatKey(userId, Data.KeyType.DefaultEncryption, newKey);
            if (await _userRepository.SaveChanges() == 0)
            {
                throw new Exception("Defauly Key creating failed");
            }
        }

        var user = await _userManager.FindByNameAsync(userName);
        var profileProjection = _mapper.Map<UserProfileProjection>(_userRepository.GetUserProfile(userId));
        profileProjection.UserId = userId;
        profileProjection.UserName = userName;
        profileProjection.Email = user.Email;
        profileProjection.EmailConfirmed = user.EmailConfirmed;

        return profileProjection;
    }
}
