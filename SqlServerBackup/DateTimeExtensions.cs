using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerBackup
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts the specified <see cref="DateTime"/> object to a Unix timestamp.
        /// </summary>
        public static Double ToUnixTimestamp(this DateTime dateTime, Boolean isLocalTime = true)
        {
            if (isLocalTime)
            {
                return Math.Floor((dateTime - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds);
            }
            else
            {
                return Math.Floor((dateTime - new DateTime(1970, 1, 1)).TotalSeconds);
            }
        }

        /// <summary>
        /// Creates a <see cref="DateTime"/> object that represents the specified Unix timestamp.
        /// </summary>
        public static DateTime FromUnixTimestamp(double timestamp, Boolean isLocalTime = true)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            if (isLocalTime)
            {
                dt = dt.ToLocalTime();
            }
            dt = dt.AddSeconds(timestamp);
            return dt;
        }
    }
}
