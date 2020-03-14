namespace Edelstein.Core.Gameplay.Social.Guild.Events
{
    public class GuildSetMemberGradeEvent : IGuildMemberEvent
    {
        public int GuildID { get; }
        public int GuildMemberID { get; }

        public byte Grade { get; }

        public GuildSetMemberGradeEvent(int guildID, int guildMemberID, byte grade)
        {
            GuildID = guildID;
            GuildMemberID = guildMemberID;
            Grade = grade;
        }
    }
}