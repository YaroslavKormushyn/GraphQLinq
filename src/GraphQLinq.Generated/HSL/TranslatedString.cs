namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "TranslatedString")]
    public partial class TranslatedString
    {
        [GraphQL(Name = "text")]
        public string Text { get; set; }
        [GraphQL(Name = "language")]
        public string Language { get; set; }
    }
}