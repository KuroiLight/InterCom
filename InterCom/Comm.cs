using System.Collections.Concurrent;

namespace InterCom
{
    public class Comm <T>
    {
        private readonly object _lock = new();
        private readonly Dictionary<int, ConcurrentQueue<T>> SubscriberChannels;
        private readonly int _maxSubs;

        public Comm(int MaxSubscribers = 2)
        {
            _maxSubs = MaxSubscribers;
            SubscriberChannels = new Dictionary<int, ConcurrentQueue<T>>(MaxSubscribers);
        }


        public bool Subscribe(int ThreadID = 0)
        {
            if(ThreadID == 0)
                ThreadID = Environment.CurrentManagedThreadId;

            lock (_lock)
            {
                if (SubscriberChannels.ContainsKey(ThreadID) || SubscriberChannels.Count >= _maxSubs)
                    return false;

                SubscriberChannels.Add(ThreadID, new ConcurrentQueue<T>());
            }
            return true;
        }

        public bool UnSubscribe(int ThreadID = 0)
        {
            if (ThreadID == 0)
                ThreadID = Environment.CurrentManagedThreadId;

            lock (_lock)
            {
                return SubscriberChannels.Remove(ThreadID);
            }
        }

        public void Publish(T data, int ThreadID = 0)
        {
            if (ThreadID == 0)
                ThreadID = Environment.CurrentManagedThreadId;

            lock (_lock)
            {
                foreach (var sub in SubscriberChannels)
                {
                    if(sub.Key != ThreadID)
                        sub.Value.Enqueue(data);
                }
            }
        }

        public T? Read(int ThreadID = 0)
        {
            if (ThreadID == 0)
                ThreadID = Environment.CurrentManagedThreadId;

            T? data = default;
            bool result;

            lock (_lock)
            {
                result = (SubscriberChannels[ThreadID] != null) && SubscriberChannels[ThreadID].TryDequeue(out data);
            }

            if (result && data != null)
                return data;
            return default;
        }

    }
}