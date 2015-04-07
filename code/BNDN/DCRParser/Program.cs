using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Common;

namespace DCRParser
{
    class Program
    {
        private Dictionary<string, EventDto> _map; 
        public Program()
        {
            _map = new Dictionary<string, EventDto>();
            var xDoc = XDocument.Load("graph.xml");
            var events = xDoc.Descendants("events").Descendants("event");
            foreach(var e in events)
            {
                //ID
                var s = e.Attribute("id").Value;
                var b = _map.ContainsKey(s);
                EventDto eventDto;
                if (b)
                {
                   _map.TryGetValue(s, out eventDto); 
                }
                else
                {
                    eventDto = new EventDto() {Responses = new HashSet<EventAddressDto>(), Conditions = new HashSet<EventAddressDto>(), Role = new HashSet<string>(), Inclusions = new HashSet<EventAddressDto>(), Exclusions = new HashSet<EventAddressDto>()};
                }
                if (eventDto == null)
                {
                    throw new NullReferenceException();
                }

                //ROLES
                var role = e.Descendants("roles"); // GIVER KUN FØRSTE ROLE. 
                foreach (var r in role)
                {
                    var roleString = r.Element("role").Value;
                    //eventDto.Role.Add(roleString);
                }


            }
        }

        static void Main(string[] args)
        {
            new Program();
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
