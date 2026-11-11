namespace FintechApp.Responses.Menu
{
    /// <summary>One row of the Menu Visibility screen: an entry, and whether it is shown.</summary>
    public class MenuVisibilityItem
    {
        /// <summary>The leaf's URL lower-cased, or <c>group:&lt;name&gt;</c> for a whole group.</summary>
        public string MenuKey { get; set; } = string.Empty;

        /// <summary>The entry's label.</summary>
        public string MenuName { get; set; } = string.Empty;

        /// <summary>The group it sits under, so the screen can draw the menu rather than a flat list.
        /// Empty for a top-level entry.</summary>
        public string GroupName { get; set; } = string.Empty;

        /// <summary>The route behind it, for the admin to recognise the screen. Empty for a group.</summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>True for a group header rather than a single screen. Hiding a group hides
        /// everything inside it.</summary>
        public bool IsGroup { get; set; }

        /// <summary>Which role the decision applies to; 0 = every role.</summary>
        public int UserTypeID { get; set; }

        /// <summary>Whether it is currently drawn for that scope.</summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>True when an admin has made a decision about this entry. False means it is
        /// simply behaving as shipped — the screen says so rather than implying somebody chose it.</summary>
        public bool HasOverride { get; set; }

        /// <summary>Which roles the shipped definition offers this entry to, so the admin can see
        /// at a glance that hiding it for retailers changes nothing if retailers never had it.</summary>
        public List<int> DefinedForRoles { get; set; } = new();
    }
}
