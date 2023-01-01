namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    public interface PlaceInterface
    {
        string Id { get; set; }
        float? Lat { get; set; }
        float? Lon { get; set; }
    }
}