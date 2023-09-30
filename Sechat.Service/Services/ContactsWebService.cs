using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using System;
using System.Collections.Generic;

namespace Sechat.Service.Services;

// todo: finish cs service
public class ContactsWebService
{
    private readonly IDbContextFactory<SechatContext> _contextFactory;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IMapper _mapper;

    public ContactsWebService(
        IDbContextFactory<SechatContext> contextFactory,
        UserManager<IdentityUser> userManager,
        IMapper mapper)
    {
        _contextFactory = contextFactory;
        _userManager = userManager;
        _mapper = mapper;
    }

    public List<ContactSuggestion> CreateContactSuggections(int level) => throw new NotImplementedException();
}

public class ContactSuggestion
{
    public int Level { get; set; }
    public string UserName { get; set; }
    public string ProfilePicture { get; set; }
}
