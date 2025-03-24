namespace TAKSuite.Data.ServicesTak
{
    public class CachedDataService
    {
        private readonly Dictionary<String, CacheDetail> _cache = new Dictionary<String, CacheDetail>();
        public CachedDataService()
        {
              
        }
        public void Add(String uid, String message, TimeSpan timeout)
        {
            _cache[uid] = new CacheDetail(uid, message, timeout);
        }
        public String? Get(String uid)
        {
            if (_cache.ContainsKey(uid))
            {
                if(DateTime.Now - _cache[uid].TimeStamp > _cache[uid].Timeout)
                {
                    _cache.Remove(uid);
                    return null;
                }
                return _cache[uid].Message;
            }
            return null;
        }


        class CacheDetail
        {
            public String Uid { get; set; }
            public String Message { get; set; }
            public DateTime TimeStamp { get; set; }
            public TimeSpan Timeout { get; set; }
            public CacheDetail(string uid, string message, TimeSpan timeout)
            {
                Uid = uid;
                Message = message;
                TimeStamp = DateTime.Now;
                Timeout = timeout;
            }
        }
    }
}
