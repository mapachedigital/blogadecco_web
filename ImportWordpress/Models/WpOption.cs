// Copyright (c) 2021, Mapache Digital
// Version: 1.0.0
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

namespace ImportWordpress.Models;

public partial class WpOption
{
    public ulong OptionId { get; set; }

    public string OptionName { get; set; } = null!;

    public string OptionValue { get; set; } = null!;

    public string Autoload { get; set; } = null!;
}
