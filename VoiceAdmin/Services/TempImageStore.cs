using System.Collections.Concurrent;

namespace VoiceAdmin.Services
{
    public static class TempImageStore
    {
        private static ConcurrentDictionary<string, (byte[] Data, string ContentType)> _store = new();

        public static void Set(string token, byte[] data, string contentType)
        {
            _store[token] = (data, contentType);
        }

        public static bool TryGet(string token, out byte[] data, out string contentType)
        {
            if (_store.TryRemove(token, out var v))
            {
                data = v.Data; contentType = v.ContentType; return true;
            }
            data = null!; contentType = null!; return false;
        }
    }
}
