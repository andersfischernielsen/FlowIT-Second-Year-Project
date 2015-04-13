using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Common;
using Newtonsoft.Json;

namespace DCRParserGraphic
{
    public class DcrParser
    {
        private readonly Dictionary<string, EventDto> _map;
        private readonly XDocument _xDoc;
        private readonly string _workflowId;
        private readonly string[] _ips;
        private readonly HashSet<string> _rolesSet;
        public Dictionary<string, string> IdToAddress { get; set; }

        public static DcrParser Parse(string filePath, string workflowId, string[] eventIps)
        {
            var parser = new DcrParser(filePath, workflowId, eventIps);
            //ORDER OF METHOD CALL IS IMPORTANT, MUST be THIS!
            parser.InitiateAllEventAddressDtoWithRolesAndNames();
            parser.MapDcrIdToRealId();
            parser.DelegateIps();
            parser.Constraints();
            parser.States();

            return parser;
        }

        private DcrParser(string filePath, string workflowId, string[] eventIps)
        {
            IdToAddress = new Dictionary<string, string>();
            _rolesSet = new HashSet<string>();
            _ips = eventIps;
            _workflowId = workflowId;
            _map = new Dictionary<string, EventDto>();
            _xDoc = XDocument.Load(filePath);
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
                        WorkflowId = _workflowId,
                    };
                }
                if (eventDto == null)
                {
                    throw new NullReferenceException();
                }

                //ROLES
                var role = e.Descendants("roles").Descendants("role");
                foreach (var r in role.Select(r => r.Value))
                {
                    _rolesSet.Add(r);
                    ((HashSet<string>)eventDto.Roles).Add(r);
                }

                //Name / description
                var desc = e.Descendants("eventDescription");
                foreach (var d in desc.Select(d => d.Value))
                {
                    eventDto.Name = d;
                }

                _map[s] = eventDto;
            }
        }

        private void MapDcrIdToRealId()
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

        private void DelegateIps()
        {
            var random = new Random();
            foreach (var v in _map.Values)
            {
                IdToAddress.Add(v.EventId, _ips[random.Next(_ips.Length)]);
            }
        }

        private void Constraints()
        {
            //Constraints general tag into variable
            var constraints = _xDoc.Descendants("constraints").ToList();
            //Conditions 
            var conditions = constraints.Descendants("conditions").Descendants("condition");
            foreach (var c in conditions)
            {
                var source = c.Attribute("sourceId").Value;
                var target = c.Attribute("targetId").Value;
                var eventDto = _map[target];
                ((HashSet<EventAddressDto>)eventDto.Conditions).Add(new EventAddressDto()
                {
                    Id = _map[source].EventId,
                    Roles = _map[source].Roles,
                    Uri = new Uri(IdToAddress[_map[source].EventId])
                });
                _map[target] = eventDto;
            }

            //responses
            var responses = constraints.Descendants("responses").Descendants("response");
            foreach (var c in responses)
            {
                var source = c.Attribute("sourceId").Value;
                var target = c.Attribute("targetId").Value;
                var eventDto = _map[source];
                ((HashSet<EventAddressDto>)eventDto.Responses).Add(new EventAddressDto
                {
                    Id = _map[target].EventId,
                    Roles = _map[target].Roles,
                    Uri = new Uri(IdToAddress[_map[target].EventId])
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
                ((HashSet<EventAddressDto>)eventDto.Exclusions).Add(new EventAddressDto
                {
                    Id = _map[target].EventId,
                    Roles = _map[target].Roles,
                    Uri = new Uri(IdToAddress[_map[target].EventId])
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
                ((HashSet<EventAddressDto>)eventDto.Inclusions).Add(new EventAddressDto
                {
                    Id = _map[target].EventId,
                    Roles = _map[target].Roles,
                    Uri = new Uri(IdToAddress[_map[target].EventId])
                });
                _map[source] = eventDto;
            }
        }

        private void States()
        {
            //State stuff
            var state = _xDoc.Descendants("marking").ToList();

            //Executed
            var executed = state.Descendants("executed").Descendants("event");
            foreach (var e in executed)
            {
                var eventId = e.Attribute("id").Value;
                var eventDto = _map[eventId];
                eventDto.Executed = true;
                _map[eventId] = eventDto;
            }

            //Included
            var included = state.Descendants("included").Descendants("event");
            foreach (var i in included)
            {
                var eventId = i.Attribute("id").Value;
                var eventDto = _map[eventId];
                eventDto.Included = true;
                _map[eventId] = eventDto;
            }

            //Pending
            var pending = state.Descendants("pendingResponses").Descendants("event");
            foreach (var p in pending)
            {
                var eventId = p.Attribute("id").Value;
                var eventDto = _map[eventId];
                eventDto.Pending = true;
                _map[eventId] = eventDto;
            }
        
        }

        public async Task CreateJsonFile()
        {
            using (var sw = new StreamWriter(@"graph.json", false))
            {
                foreach (var v in _map.Values)
                {
                    var json = JsonConvert.SerializeObject(v, Formatting.Indented);
                    await sw.WriteLineAsync(json);
                    await sw.WriteLineAsync("");
                    await sw.WriteLineAsync("");
                }
            }
        }

        public IEnumerable<string> GetRoles()
        {
            return _rolesSet;
        }

        public Dictionary<string, EventDto> GetMap()
        {
            return _map;
        }
    }
}
