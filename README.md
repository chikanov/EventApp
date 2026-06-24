# REST API example application

Event management Service.


## Run the app

    dotnet build
    dotnet run

## To start test

    dotnet test

# REST API

The REST API to the example app is described below.

## Event
** Description: **
Class 'Event' is a template for storing Event information
** Attributes **
'Id' - unique int
'Title' - event title string
'Description' - event description string
'StartAt' - Start event date DateTime
'EndAt' - End event date DateTime
'TotalSeats' - total number of seats at the event int
'AvailableSeats' - current number of available seats int
## Booking
** Description: **
    Class 'Booking' is a template for storing booking information
** Attributes **
    'Id' - unique guid
    'EventId' - unique int id
    'Status' - status of booking
    'CreatedAt' - Creation Date
    'ProcessedAt' - Processing Date

## BookingStatus
** Description: **
    The class 'BookingStatus' is used to store constant values of the booking status.
** Status options: **
    'Pending' - Status when booking is created
    'Confirmed' - Status confermed booking
    'Rejected' - Status rejected booking

## BookingBackgroundService
** Description: **
    This is a service that runs the program all the time and tries to process booking and change the status to Confirmed and add the processing date.
** Example: **
    Every 10 seconds, the service polls the database for new pending bookings, processes all bookings in parallel, changes the status to cofirmed, and fills in ProcessedAt with the current date time.
** Used synchronization primitives and why they are needed **
Critical section protection in BookingService.
The competitive problem here is classic: two threads read AvailableSeats > 0 at the same time, both decide to create a reservation, and the number of reservations exceeds the number of seats. A private lock field has been added to BookingService. This ensures that only one thread passes through the critical section at any given time.
** Parallel processing in BookingBackgroundService **
Previously, the background service processed requests sequentially. Now it processes multiple requests simultaneously while protecting the storage update.
** An example of an overbooking scenario. **
Given: an event with 5 seats and 20 competing requests.
Expected: exactly 5 successful bookings, 15 - NoAvailableSeatsException, AvailableSeats = 0.

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

## Create Booking on Event

### Request
`POST /events/{id}/book`

curl -X 'POST' \
  'https://localhost:7124/api/Events/16/book' \
  -H 'accept: text/plain' \
  -d ''
### Response

    HTTP/1.1 202 Accepted
    Date: Thu, 24 Feb 2011 12:36:30 GMT
    Status: 202 Accepted
    Connection: close
    Content-Type: application/json
    
    {
  "id": "01eeba11-fa52-4857-a060-803c1670b78b",
  "eventId": 16,
  "status": "Pending",
  "createdAt": "2026-06-02T18:52:52.8095186+03:00",
  "processedAt": "0001-01-01T00:00:00"
    }
    
    content-type: application/json; charset=utf-8 
    date: Tue,02 Jun 2026 15:52:52 GMT 
    location: /bookings/01eeba11-fa52-4857-a060-803c1670b78b 
    server: Kestrel

## Create Booking on Event when Available Seats = 0
### Request
`POST /events/{id}/book`

curl -X 'POST' \
  'https://localhost:7124/api/Events/16/book' \
  -H 'accept: */*' \
  -d ''
Error: response status is 409 Conflict

{
  "status": 409,
  "detail": "No available seats for this event."
}
  

## Get booking by Id.

### Request
`GET /bookings/{id}`

curl -X 'GET' \
  'https://localhost:7124/api/Bookings/01eeba11-fa52-4857-a060-803c1670b78b' \
  -H 'accept: text/plain'

### Response

    HTTP/1.1 200 OK
    Date: Thu, 24 Feb 2011 12:36:30 GMT
    Status: 200 OK
    Connection: close
    Content-Type: application/json
    
    {
  "id": "01eeba11-fa52-4857-a060-803c1670b78b",
  "eventId": 16,
  "status": "Confirmed",
  "createdAt": "2026-06-02T18:52:52.8095186+03:00",
  "processedAt": "2026-06-02T18:52:53.9904206+03:00"
}

     content-type: application/json; charset=utf-8 
     date: Tue,02 Jun 2026 15:53:03 GMT 
     server: Kestrel 
    
