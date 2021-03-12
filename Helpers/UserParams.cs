namespace API.Helpers
{
    public class UserParams
    {
        private const int MaxPageSize=50;
        public int PageNumber { get; set; }=1;
        private int pageSize=10;

        public int PageSize
        {
            get=>pageSize;
            set=>pageSize=(value>MaxPageSize) ? MaxPageSize : value;
        }

        public string currentUsername { get; set; }
        public string gender { get; set; }
        public int minAge { get; set; }=18;
        public int maxAge { get; set; }=150;

        public string OrderBy { get; set; }="lastActive";
    }
}