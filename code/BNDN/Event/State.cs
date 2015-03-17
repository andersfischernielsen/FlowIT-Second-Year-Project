using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;

namespace Event
{
    // Super hack! TODO: Database or equivalent.
    public static class State
    {
        public static bool Executed { get; set; }
        public static bool Included { get; set; }
        public static bool Pending { get; set; }

        public static List<EventAddressDto> Conditions { get; set; }
        public static List<EventAddressDto> Responses { get; set; }
        public static List<EventAddressDto> Exclusions { get; set; }
        public static List<EventAddressDto> Inclusions { get; set; }

        private static readonly Dictionary<string, Tuple<bool, bool>> Preconditions = new Dictionary<string, Tuple<bool, bool>>();

        public static Tuple<bool, bool> GetPrecondition(string id)
        {
            lock (Preconditions) { return Preconditions[id]; }
        }

        public static IEnumerable<String> GetPreconditions()
        {
            lock (Preconditions)
            {
                return Preconditions.Keys;
            }
        }

        public static void AddPrecondition(string key, Tuple<bool, bool> state)
        {
            lock (Preconditions)
            {
                Preconditions[key] = state;
            }
        }

        public static Task<EventStateDto> EventStateDto
        {
            get
            {
                lock (Preconditions)
                {
                    return Task.Run(() => new EventStateDto
                    {
                        Executed = Executed,
                        Included = Included,
                        Pending = Pending,
                        Executable = Preconditions.Values.All(state => state.Item1 || state.Item2)
                    });
                }
            }
        }

    }
}