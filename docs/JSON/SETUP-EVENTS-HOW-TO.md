# Posting seed-data to Events

- Seeddata is located in docs/JSON/CourseWorkflow.json
- Prerequisities: 
- Server instance running
- Three events running
- For each Event, post seeddata for ONE Event (copy from CourseWorkflow.json into "raw") using "POST" in PostMan AT THE INTENDED EVENT's ADDRESS
- Example: POST on htttp://localhost:13754/event/

PASTE THIS INTO "raw" and set type to "JSON (application/json)":


{
    "EventId": "test1",
    "WorkflowId": "test1",
    "Name": "Test 1",
    "Pending": false,
    "Executed": false,
    "Included": true,
    "Conditions": [],
    "Exclusions": [],
    "Responses": [],
    "Inclusions": []
}


- Do the same for the next two events ( 1) Copy the next content from file 2) Make sure you target the right address ) 