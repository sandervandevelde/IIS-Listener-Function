# IIS Listener Function 

Ingest messages from the IIS Now API and forwards the JSON to an Azure EventHub.

## Message format

```
{
    "message": "success", 
    "timestamp": 1734273015, 
    "iss_position": {
        "longitude": "19.7957", 
        "latitude": "-6.9518"
    }
}
```

Note, timstamp is in Epoch seconds.

The interval of the function is once every 5 seconds.

## Environment variables

- EventHubEndpoint : the endpoint of the eventhub where messages are dropped of.
- IISNowUrl : likely 'http://api.open-notify.org/iss-now.json'

