namespace PremiumFinance.Application.Common;

// Role names used in [Authorize(Roles = ...)] and when creating users.
// Constants, so a typo is a compile error instead of a silent "access denied".
public static class Roles
{
    public const string Admin = "Admin";
    public const string Broker = "Broker";
    public const string Agent = "Agent";
    public const string TeamLeader = "TeamLeader";
    public const string Underwriter = "Underwriter";
    public const string Operations = "Operations";
    public const string Compliance = "Compliance";

    public static readonly string[] All =
        [Admin, Broker, Agent, TeamLeader, Underwriter, Operations, Compliance];
}