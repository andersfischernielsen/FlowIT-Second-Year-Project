# GET /Workflows
* When 0 Workflows exists
* When 1 Workflow exists
* When many (10+?) Workflows exists.
* Exceptional cases?

# GET /Workflows/workflowID
* When the workflow exists, it shall return the list of EventAddressDtos of the current workflow.
* When no such workflow exists. 404 Not found (?)
* Test with weird characters/symbols/spaces to see that WebAPI doesn't break.

# POST /Workflows/workflowId WorkflowDto
* Good case: Post a workflowId that does not exists with a good copy of a WorkflowDto.
* Bad cases:
    * POST a workflow which already exists (all of the following cases should return BadRequest of some kind) (check [this stackoverflow](http://stackoverflow.com/questions/3825990/http-response-code-for-post-when-resource-already-exists) to see other people views on which status code should be returned!)
        * These are probably a logic/storage test:
            * With the current WorkflowDto
            * With another WorkflowDto
            * With an empty WorkflowDto
        * Without any Dto.
        * With nonsense data (something which is not JSON) (This is integration testing/testing of WebAPI ModelState (bad?))
        * With a wrong object type. (Cannot be done without running the WebApi)
    * POST a workflow which does not exist (all of the following cases should return BadRequest of some kind)
        * With an empty WorkflowDto
        * Without any Dto.
        * With nonsense data (something which is not JSON) (This is integration testing/testing of WebAPI (bad?))
        * With a wrong object type.

# DELETE /Workflows/workflowId
* Good case: WorkflowId existed and was empty.
* Bad cases:
    * WorkflowId did not exist
    * Workflow with WorkflowId is not empty (it has events which is part of the workflow).

# PUT /Workflows/workflowId EventAddressDto (Shouldn't this be something else?)
* I think this test will be interesting because the Route say one thing (Update workflow) while the method does something else (Update an event in a workflow.
* A PUT method seems to be missing on the workflow which should be at the route of this method.

## Test for PUT /Workflows/workflowId WorkflowDto
* Good case: WorkflowId exists and the updated WorkflowDto matches the Id but has a new name.
* Bad cases:
    * WorkflowId exists:
        * But the given WorkflowDto does not have a matching workflowId.
        * Body is empty
        * Body not JSON
        * Body is wrong JSON object type
        * Body matches the Id but has a null value name (Is this an exceptional case?)
    * WorkflowId does not exist
        * Should probably fail with a 404 on every call, because otherwise we spend resources on checking som data which is never used for anything.

## Test for PUT /Workflows/workflowId/eventId EventAddressDto
* Good case: The dto-id matches the url, and the Uri of the dto is valid. The event is currently part of the given workflowId. (Not sure on how to use this method though)
* Bad cases:
    * The workflow with workflowId does not exist.
    * An event with eventId has not been POSTed onto the workflow.
    * An event with eventId belongs to another workflow (same case as above?)
    * The body is empty although the url-id exists.
    * The body is not JSON
    * The body is valid JSON but wrong object type.

# POST /Workflows/workflowId/eventId EventAddressDto
* Good case: Workflow has already been created, eventId has not been created yet, and Dto matches the id of the Url.
* Good case: Workflow exists and eventId has not been created on it yet, but another workflow has an event with the same id as this one.
* Bad cases:
    * Workflow has not been created yet.
    * Workflow exists but the eventId is already taken.
    * Dto id does not match url-id.
    * Body is empty
    * Body is not JSON
    * Body is JSON but wrong type.

# DELETE /Workflows/workflowId/eventId
* Good case: eventId is part of workflowId which both exists.
* Bad cases:
    * eventId is not part of workflowId.
    * workflowId does not exist.
