# IIS Listener Function 

Ingest messages from the IIS Now APIs and forwards the JSON to an Azure EventHub.

## Message format

IIS Now:

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

Astronauts:

```
{
    "people": [
        {
            "craft": "ISS",
            "name": "Oleg Kononenko"
        },
        {
            "craft": "ISS",
            "name": "Nikolai Chub"
        },
        {
            "craft": "ISS",
            "name": "Tracy Caldwell Dyson"
        },
        {
            "craft": "ISS",
            "name": "Matthew Dominick"
        },
        {
            "craft": "ISS",
            "name": "Michael Barratt"
        },
        {
            "craft": "ISS",
            "name": "Jeanette Epps"
        },
        {
            "craft": "ISS",
            "name": "Alexander Grebenkin"
        },
        {
            "craft": "ISS",
            "name": "Butch Wilmore"
        },
        {
            "craft": "ISS",
            "name": "Sunita Williams"
        },
        {
            "craft": "Tiangong",
            "name": "Li Guangsu"
        },
        {
            "craft": "Tiangong",
            "name": "Li Cong"
        },
        {
            "craft": "Tiangong",
            "name": "Ye Guangfu"
        }
    ],
    "number": 12,
    "message": "success"
}
```

Only astronauts on the IIS are forwarded.


## Azure Function

The interval of the function is once every 5 seconds.

## Environment variables

- producerIisPositionClient : the endpoint of the (Microsoft Fabric eventstream custom source) eventhub where messages are dropped of.
- IISNowUrl : likely 'http://api.open-notify.org/iss-now.json'
- AstrosUrl: likely '   '

