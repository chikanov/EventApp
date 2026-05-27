# REST API example application

Event management Service.


## Run the app

    dotnet build
    dotnet run

## To start test

    dotnet test

# REST API

The REST API to the example app is described below.

## Get list of Events

### Request
<param name="tittle">Event title</param>
<param name="from">Date when event start</param>
<param name="to">Date when event finished</param>
<param name="page">Number of page</param>
<param name="pageSize">Page size</param>
`GET /events/`

    curl -X 'GET' \  'https://localhost:7124/api/Events' \  -H 'accept: text/plain'

### Response

    HTTP/1.1 200 OK
    Date: Thu, 24 Feb 2011 12:36:30 GMT
    Status: 200 OK
    Connection: close
    Content-Type: application/json
    Content-Length: 2

    []
### Response with error
    Error: response status is 404
    Response body
    {
        "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
        "title": "Not Found",
        "status": 404,
        "traceId": "00-1ec2d2b9aad6b86a546c67aecdccbb82-8b0a7bb24db31e32-00"
    }

    Response headers
    content-type: application/problem+json; charset=utf-8 
    date: Sun,24 May 2026 15:11:10 GMT 
    server: Kestrel

## Create a new Event

### Request

`POST /events/{id}`

    curl -i -H 'Accept: application/json' -d 'name=Foo&status=new' https://localhost:7124/events

### Response

    HTTP/1.1 201 Created
    Date: Thu, 24 Feb 2011 12:36:30 GMT
    Status: 201 Created
    Connection: close
    Content-Type: application/json
    Location: /events/1
    Content-Length: 36

     {
        "id": 1,
        "title": "Tittle1",
        "description": "Description1",
        "startAt": "2026-05-07T20:25:42.6807737+03:00",
        "endAt": "2026-05-08T20:25:42.6808257+03:00"
    }

## Get Event by id

### Request

`GET /events/{id}`

    curl -i -H 'Accept: application/json' https://localhost:7124/events/1

### Response

    HTTP/1.1 200 OK
    Date: Thu, 24 Feb 2011 12:36:30 GMT
    Status: 200 OK
    Connection: close
    Content-Type: application/json
    Content-Length: 36

    {
        "id": 1,
        "title": "Tittle1",
        "description": "Description1",
        "startAt": "2026-05-07T20:25:42.6807737+03:00",
        "endAt": "2026-05-08T20:25:42.6808257+03:00"
    }

## Get a non-existent Event

### Request

`GET /events/{id}`

    curl -i -H 'Accept: application/json' https://localhost:7124/events/9999

### Response

    HTTP/1.1 404 Not Found
    Date: Thu, 24 Feb 2011 12:36:30 GMT
    Status: 404 Not Found
    Connection: close
    Content-Type: application/json
    Content-Length: 35

    {"status":404,"reason":"Not found"}

## Change a Events state

### Request

`PUT /events/:id/status/changed`

    curl -i -H 'Accept: application/json' -X PUT https://localhost:7124/events/1/status/changed

### Response

    HTTP/1.1 200 OK
    Date: Thu, 24 Feb 2011 12:36:31 GMT
    Status: 200 OK
    Connection: close
    Content-Type: application/json
    Content-Length: 40

    {"id":1,"name":"Foo","status":"changed"}

## Delete Event

### Request

`DELETE /events/{id}`

    curl -i -H 'Accept: application/json' -X DELETE https://localhost:7124/events/1/

### Response

    HTTP/1.1 204 No Content
    Date: Thu, 24 Feb 2011 12:36:32 GMT
    Status: 204 No Content
    Connection: close