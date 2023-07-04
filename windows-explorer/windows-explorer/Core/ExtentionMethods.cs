﻿namespace windows_explorer.Core
{
    public static class ExtentionMethods
    {
        /// <summary>
        /// Generates a deterministic hash code that do not change by a program re-run
        /// <para>Source code from: https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/</para>
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int GetFixedHashCode(this string str)
        {
            
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}
