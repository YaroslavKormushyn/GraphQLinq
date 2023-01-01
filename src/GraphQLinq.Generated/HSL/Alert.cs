namespace HSL
{
    using System;
    using System.Collections.Generic;
    using GraphQLinq.Attributes;
    using System.Reflection;

    [GraphQL(Name = "Alert")]
    public partial class Alert : Node
    {
        [GraphQL(Name = "id")]
        public string Id { get; set; }
        [GraphQL(Name = "alertHash")]
        public int? AlertHash { get; set; }
        [GraphQL(Name = "feed")]
        public string Feed { get; set; }
        [GraphQL(Name = "agency")]
        public Agency Agency { get; set; }
        [GraphQL(Name = "route")]
        public Route Route { get; set; }
        [GraphQL(Name = "trip")]
        public Trip Trip { get; set; }
        [GraphQL(Name = "stop")]
        public Stop Stop { get; set; }
        [GraphQL(Name = "patterns")]
        public List<Pattern> Patterns { get; set; }
        [GraphQL(Name = "alertHeaderText")]
        public string AlertHeaderText { get; set; }
        [GraphQL(Name = "alertHeaderTextTranslations")]
        public List<TranslatedString> AlertHeaderTextTranslations { get; set; }
        [GraphQL(Name = "alertDescriptionText")]
        public string AlertDescriptionText { get; set; }
        [GraphQL(Name = "alertDescriptionTextTranslations")]
        public List<TranslatedString> AlertDescriptionTextTranslations { get; set; }
        [GraphQL(Name = "alertUrl")]
        public string AlertUrl { get; set; }
        [GraphQL(Name = "alertUrlTranslations")]
        public List<TranslatedString> AlertUrlTranslations { get; set; }
        [GraphQL(Name = "alertEffect")]
        public AlertEffectType AlertEffect { get; set; }
        [GraphQL(Name = "alertCause")]
        public AlertCauseType AlertCause { get; set; }
        [GraphQL(Name = "alertSeverityLevel")]
        public AlertSeverityLevelType AlertSeverityLevel { get; set; }
        [GraphQL(Name = "effectiveStartDate")]
        public long? EffectiveStartDate { get; set; }
        [GraphQL(Name = "effectiveEndDate")]
        public long? EffectiveEndDate { get; set; }
    }
}