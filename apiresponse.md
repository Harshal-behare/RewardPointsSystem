Request URL
https://localhost:1352/api/v1/Products
Server response
Code	Details
401
Undocumented
Error: response status is 401

Response headers
 content-length: 0 
 date: Sat,22 Nov 2025 11:20:45 GMT 
 server: Kestrel 
 www-authenticate: Bearer 
Responses
Code	Description	Links
201	
Created

Media type

application/json
Controls Accept header.
Example Value
Schema
{
  "success": true,
  "message": "string",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "string",
    "description": "string",
    "category": "string",
    "imageUrl": "string",
    "currentPointsCost": 0,
    "isActive": true,
    "isInStock": true,
    "stockQuantity": 0,
    "createdAt": "2025-11-22T11:26:15.241Z"
  },
  "timestamp": "2025-11-22T11:26:15.241Z"
}
No links
422	
Client Error

Media type

application/json
Example Value
Schema
{
  "success": true,
  "message": "string",
  "statusCode": 0,
  "timestamp": "2025-11-22T11:26:15.243Z",
  "path": "string",
  "traceId": "string",
  "errors": {
    "additionalProp1": [
      "string"
    ],
    "additionalProp2": [
      "string"
    ],
    "additionalProp3": [
      "string"
    ]
  }
}

Curl

curl -X 'PUT' \
  'https://localhost:1352/api/v1/Roles/94ACA835-0442-4868-98F3-16B039DDC263' \
  -H 'accept: application/json' \
  -H 'Authorization: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6Ijk0YWNhODM1LTA0NDItNDg2OC05OGYzLTE2YjAzOWRkYzI2MyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImhhcnNoYWwuYmVoYXJlQGFnZGF0YS5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiSGFyc2hhbCBCZWhhcmUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9naXZlbm5hbWUiOiJIYXJzaGFsIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvc3VybmFtZSI6IkJlaGFyZSIsImp0aSI6ImFkMzM0OTYxLTliMzctNDg3NS1iYTFiLWRmMTQxOGI0ODEzYiIsImlhdCI6MTc2MzgwOTkzMiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiRW1wbG95ZWUiLCJuYmYiOjE3NjM4MDk5MzIsImV4cCI6MTc2MzgxMDgzMiwiaXNzIjoiUmV3YXJkUG9pbnRzQVBJIiwiYXVkIjoiUmV3YXJkUG9pbnRzQ2xpZW50In0._AIEGKLty4sM4lC-R9J6Evfk-p_HeBcdNXY66xQOQko' \
  -H 'Content-Type: application/json' \
  -d '{
  "name": "Admin",
  "description": "System Administrator
}'
Request URL
https://localhost:1352/api/v1/Roles/94ACA835-0442-4868-98F3-16B039DDC263
Server response
Code	Details
Undocumented
Failed to fetch.
Possible Reasons:

CORS
Network Failure
URL scheme must be "http" or "https" for CORS request.
Responses
Code	Description	Links
200	
Role updated successfully

Media type

application/json
Controls Accept header.
Example Value
Schema
{
  "success": true,
  "message": "string",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "string",
    "description": "string",
    "createdAt": "2025-11-22T11:26:15.255Z"
  },
  "timestamp": "2025-11-22T11:26:15.255Z"
}
No links
404	
Role not found

Media type

application/json
Example Value
Schema
{
  "success": true,
  "message": "string",
  "statusCode": 0,
  "timestamp": "2025-11-22T11:26:15.257Z",
  "path": "string",
  "traceId": "string"
}