namespace VAutomationCore.Core.Data
{
    /// <summary>
    /// Runtime auth role stored on a user entity for ECS-driven checks and tooling.
    /// </summary>
    public struct ConsoleRoleComponent
    {
        /// <summary>
        /// 0=None, 1=Admin, 2=Developer.
        /// </summary>
        public byte Role;

        /// <summary>
        /// Session expiration in UTC Unix seconds. 0 means not set.
        /// </summary>
        public long ExpiresAtUnixSeconds;
    }
}
