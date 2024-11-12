namespace SyncMapProject
{
    public class Settings
    {
        public Section_SQLConnection SQLConnection { get; set; } = new Section_SQLConnection();
        public class Section_SQLConnection
        {
            public string Host { get; set; } = ".\\VSRO_TESTIN";
            public string Username { get; set; } = "admin";
            public string Password { get; set; } = "123123123";
            public string Database { get; set; } = "SRO_VT_SHARD";
        }
    }
}
