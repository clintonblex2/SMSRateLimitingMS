using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSRateLimitingMS.Domain.Entities
{
    public class MessageRateAggregate
    {
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public string PhoneNumber { get; private set; }
        public DateTime TimeWindow { get; private set; }
        public TimeSpan WindowSize { get; private set; }
        public int TotalRequests { get; private set; }
        public int RejectedRequests { get; private set; }

        private MessageRateAggregate() { }

        public MessageRateAggregate(
            string phoneNumber, 
            DateTime timeWindow,
            TimeSpan windowSize, 
            int totalRequests, 
            int rejectedRequests)
        {
            PhoneNumber = phoneNumber;
            TimeWindow = timeWindow;
            WindowSize = windowSize;
            TotalRequests = totalRequests;
            RejectedRequests = rejectedRequests;
        }
    }
}
