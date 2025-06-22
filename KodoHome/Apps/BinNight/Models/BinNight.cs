using System;
using System.Collections.Generic;

namespace KodoHome.BinNight.Models;

public class BinNightCollections
{
    public string AddressId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public Dictionary<string, CollectionService> Services { get; set; } = new();
    public Dictionary<string, CollectionNight> ServiceDates { get; set; } = new();
}

public class CollectionService
{
    public string ServiceUid { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}

public class CollectionNight
{
    public string Date { get; set; } = string.Empty;
    public Dictionary<string, string> Services { get; set; } = new();
}
