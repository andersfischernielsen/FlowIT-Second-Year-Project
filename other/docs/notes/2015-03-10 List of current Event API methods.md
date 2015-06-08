HttpMethod | Address | Argument type | Return type | Comment
-----------|---------|---------------|-------------|--------
GET | /event/ | | EventDto |
GET | /event/executable/ | | bool |
GET | /event/executed/ | | bool |
PUT | /event/executed/ | | | When client wants to execute
GET | /event/included/ | | bool |
GET | /event/pending/ | | bool |
GET | /event/state/ | | EventStateDto |
PUT | /event/ | EventDto | | Start Event
PUT | /event/notify/ | IEnumerable< NotifyDto> | | Should possibly be post instead? Or put on the three states in the top?
POST | /event/rules/eventId/ | EventRuleDto | | Still relevant?
PUT | /event/rules/eventId/ | EventRuleDto | | Still relevant?
DELETE | /event/rules/eventId/ | | | Still relevant?
