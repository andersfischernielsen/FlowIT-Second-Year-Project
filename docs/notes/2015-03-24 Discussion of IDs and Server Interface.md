# Why string ids?
* WorkflowID:
    * Because organizations will typically have a Server each, it should not be possible to have two workflows with the same name.
* EventID: There should not be identical event-ids inside a workflow.

# Server REST interface
* WorkflowsController:
    * GET /workflows/
        * Returns WorkflowDto[]
    * POST /workflows/workflowId { WorkflowDto }
        * workflowId : string
    * PUT /workflows/workflowId { WorkflowDto }
        * workflowId : string
        * optional
    * DELETE /workflows/workflowId
        * workflowId : string
* EventsController:
    * GET /workflows/workflowId
        * workflowId : string
        * Returns EventAddressDto[]
    * GET /workflows/workflowId/eventId
        * Returns EventAddressDto
    * POST /workflows/workflowId/eventId { EventAddressDto }
        * workflowId : string
        * eventId : string
        * Event does this.
    * PUT /workflows/workflowId/eventId {EventAddressDto}
        * Optional
    * DELETE /workflows/workflowId/eventId
        * workflowId : string
        * eventId : string

Husk at ToLowerCase alle string-ids.

# Event
* Remember own Uri when POST /event/ is called.
