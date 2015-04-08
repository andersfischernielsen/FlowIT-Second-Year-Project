using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Common;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace DCRParserGraphic
{
    class DcrParser
    {
        private readonly Dictionary<string, EventDto> _map;
        private readonly XDocument _xDoc;
        private string _path;

        public DcrParser(string path)
        {
            _path = path;
            _map = new Dictionary<string, EventDto>();
            _xDoc = XDocument.Load(path);
            //ORDER OF METHOD CALL IS IMPORTANT, MUST be THIS!
            InitiateAllEventAddressDtoWithRolesAndNames();
            MapDCRIdToRealId();
            Constraints();
            States();
            CreateXmlFile();
        }

        private void InitiateAllEventAddressDtoWithRolesAndNames()
        {
            var events = _xDoc.Descendants("events").Descendants("event");
            foreach (var e in events)
            {
                EventDto eventDto;
                //DCR-ID
                var s = e.Attribute("id").Value;
                var b = _map.ContainsKey(s);
                if (b)
                {
                    _map.TryGetValue(s, out eventDto);
                }
                else
                {
                    eventDto = new EventDto()
                    {
                        Responses = new HashSet<EventAddressDto>(),
                        Conditions = new HashSet<EventAddressDto>(),
                        Roles = new HashSet<string>(),
                        Inclusions = new HashSet<EventAddressDto>(),
                        Exclusions = new HashSet<EventAddressDto>(),
                        Executed = false,
                        Included = false,
                        Pending = false,
                        WorkflowId = "SOMEWORKFLOWIDPLEASESETMECORRECT",
                    };
                }
                if (eventDto == null)
                {
                    throw new NullReferenceException();
                }

                //ROLES
                var role = e.Descendants("roles").Descendants("role");
                foreach (var r in role)
                {
                    var roleString = r.Value;
                    ((HashSet<string>)eventDto.Roles).Add(roleString);
                }

                //Name / description
                var desc = e.Descendants("eventDescription");
                foreach (var d in desc)
                {
                    eventDto.Name = d.Value;
                }

                _map[s] = eventDto;
            }
        }

        private void MapDCRIdToRealId()
        {
            var eventIds = _xDoc.Descendants("labelMappings").Descendants("labelMapping");
            foreach (var i in eventIds)
            {
                var id = i.Attribute("eventId").Value;
                var eventId = i.Attribute("labelId").Value;
                var eventDto = _map[id];
                eventDto.EventId = eventId;
                _map[id] = eventDto;
            }
        }

        private void Constraints()
        {
            //Constraints general tag into variable
            var constraints = _xDoc.Descendants("constraints");
            //Conditions 
            var conditions = constraints.Descendants("conditions").Descendants("condition");
            foreach (var c in conditions)
            {
                var source = c.Attribute("sourceId").Value;
                var target = c.Attribute("targetId").Value;
                var eventDto = _map[source];
                ((HashSet<EventAddressDto>)eventDto.Conditions).Add(new EventAddressDto()
                {
                    Id = _map[target].EventId,
                    Roles = _map[target].Roles,
                    Uri = new Uri("http://jegvedikke.dk/" + _map[target].EventId)
                });
                _map[source] = eventDto;
            }

            //responses
            var responses = constraints.Descendants("responses").Descendants("response");
            foreach (var c in responses)
            {
                var source = c.Attribute("sourceId").Value;
                var target = c.Attribute("targetId").Value;
                var eventDto = _map[source];
                ((HashSet<EventAddressDto>)eventDto.Responses).Add(new EventAddressDto()
                {
                    Id = _map[target].EventId,
                    Roles = _map[target].Roles,
                    Uri = new Uri("http://jegvedikke.dk/" + _map[target].EventId)
                });
                _map[source] = eventDto;
            }

            //excludes
            var excludes = constraints.Descendants("excludes").Descendants("exclude");
            foreach (var c in excludes)
            {
                var source = c.Attribute("sourceId").Value;
                var target = c.Attribute("targetId").Value;
                var eventDto = _map[source];
                ((HashSet<EventAddressDto>)eventDto.Exclusions).Add(new EventAddressDto()
                {
                    Id = _map[target].EventId,
                    Roles = _map[target].Roles,
                    Uri = new Uri("http://jegvedikke.dk/" + _map[target].EventId)
                });
                _map[source] = eventDto;
            }

            //includes
            var includes = constraints.Descendants("includes").Descendants("include");
            foreach (var c in includes)
            {
                var source = c.Attribute("sourceId").Value;
                var target = c.Attribute("targetId").Value;
                var eventDto = _map[source];
                ((HashSet<EventAddressDto>)eventDto.Inclusions).Add(new EventAddressDto()
                {
                    Id = _map[target].EventId,
                    Roles = _map[target].Roles,
                    Uri = new Uri("http://jegvedikke.dk/" + _map[target].EventId)
                });
                _map[source] = eventDto;
            }
        }

        private void States()
        {
            //State stuff
            var state = _xDoc.Descendants("marking");
        }

        private void CreateXmlFile()
        {
            using (var sw = new StreamWriter("graph.json", false))
            {
                foreach (var v in _map.Values)
                {
                    var json = JsonConvert.SerializeObject(v, Formatting.Indented);
                    sw.WriteLine(json);
                    sw.WriteLine("");
                    sw.WriteLine("");
                }
            }
        }
    }
}
