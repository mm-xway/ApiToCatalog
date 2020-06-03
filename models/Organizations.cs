namespace apitocatalog.models
{
    using System;

    public partial class Organization
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public object Description { get; set; }
        public object Email { get; set; }
        public object Image { get; set; }
        public bool Restricted { get; set; }
        public object VirtualHost { get; set; }
        public object Phone { get; set; }
        public bool Enabled { get; set; }
        public bool Development { get; set; }
        public string Dn { get; set; }
        public long CreatedOn { get; set; }
        public object StartTrialDate { get; set; }
        public object EndTrialDate { get; set; }
        public object TrialDuration { get; set; }
        public object IsTrial { get; set; }
    }
}
