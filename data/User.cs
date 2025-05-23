using System;
using System.Collections.Generic;

namespace ShoppingApp.Data;

public partial class User
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? BusinessName { get; set; }
}
