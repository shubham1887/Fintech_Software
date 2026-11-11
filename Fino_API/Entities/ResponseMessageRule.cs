using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One rewrite rule for a user-facing message.
    ///
    /// Upstream providers answer in their own words, and those words are written for US, not for
    /// the person standing at the counter: "Low balance" from JaiJui means the ADMIN's float with
    /// the provider has run out, but the retailer who reads it believes their own wallet is empty
    /// and calls support. Worse, it tells an outsider something about the business that is nobody
    /// else's concern. A rule maps that text to what the user should actually be told
    /// ("Server down — please try again shortly"), without touching the provider log, which keeps
    /// the raw reply for whoever has to debug it.
    ///
    /// Scope narrows a rule: ProviderID / ServiceID 0 mean "any", and Channel decides whether it
    /// applies to the panel and app, to the B2B API, or to both.
    /// </summary>
    public class ResponseMessageRule : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ResponseMessageRuleID { get; set; }

        /// <summary>0 = every provider. Otherwise only messages known to come from this provider.</summary>
        public int ProviderID { get; set; }

        /// <summary>0 = every service. Otherwise only this service's messages.</summary>
        public int ServiceID { get; set; }

        /// <summary>CONTAINS | EQUALS | STARTSWITH | REGEX — see MessageMatchType.</summary>
        [Column(TypeName = "varchar(20)")]
        public string MatchType { get; set; } = string.Empty;

        /// <summary>What to look for in the provider's message.</summary>
        [Column(TypeName = "varchar(500)")]
        public string Pattern { get; set; } = string.Empty;

        /// <summary>What the user is shown instead. Empty means "say nothing".</summary>
        [Column(TypeName = "varchar(500)")]
        public string Replacement { get; set; } = string.Empty;

        /// <summary>ALL | PANEL | API — see MessageChannel.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Channel { get; set; } = string.Empty;

        /// <summary>Lowest first. The first matching rule wins, so order is the priority.</summary>
        public int Sequence { get; set; }

        /// <summary>Why this rule exists — for whoever reads the list a year from now.</summary>
        [Column(TypeName = "varchar(250)")]
        public string Note { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
