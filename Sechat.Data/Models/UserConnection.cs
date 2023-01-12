﻿using System.ComponentModel.DataAnnotations;

namespace Sechat.Data.Models;
public record UserConnection : BaseModel<long>
{
    [Required]
    public bool Approved { get; set; }
    [Required]
    public string Inviter { get; set; }
    [Required]
    public string Invited { get; set; }
}
