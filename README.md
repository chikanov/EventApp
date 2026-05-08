# REST API example application

Event management Service.


## Run the app

    dotnet build
    dotnet run

# REST API

The REST API to the example app is described below.

## Get list of Events

### Request

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