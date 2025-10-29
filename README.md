# Zykmtrixo
*"You don't know what it is, and frankly, neither do I."*

Zykmtrixo is a service for enabling the ability to shut down Roblox servers.
This is *only* intended for custom server migrations that can't use
[Server Management](https://devforum.roblox.com/t/introducing-server-management/3941907).
Since this does shutdowns, it should be paired with a
[soft shutdown implementation](https://devforum.roblox.com/t/soft-shutdown-script/47588).

This project is """temporary""" and will be deprecated once
[the "Delay server restart" window in Server Management is increased](https://devforum.roblox.com/t/increase-delay-server-restart-limit-in-server-management/4015329).
It was only created for when you need more than 60 minutes to migrate servers.
If you always need less than 60 minutes, you should use Server Management and
manually teleport players out of the servers within the 60-minute window (which
will send players to newer servers since the older servers will be out of
matchmaking).

(The name was hallucinated by an over-quantized machine learning model and
has no meaning.)

## Limitations
To shut down servers, the user-facing endpoint `/matchmaking-api/v1/game-instances/shutdown`
is used. This API is meant to be used by a user (not Open Cloud, and not meant
to be automated).

### Public Servers Only
The endpoint does not work on reserved servers or private servers, so it should
not be used for them.
- For reserved servers: create new reserved servers and teleport them.
- For private servers: no real option since you can't teleport into private servers.

### No Matchmaking Removal with Staggered Shutdowns
Unlike Server Management which takes the servers out of matchmaking, the shutdown
endpoint can only remove servers from matchmaking at shutdown. When the shutdowns
are staggered, it is probable for players to get removed from a server being
shut down to one about to shut down again.

### Potentially Brittle
Since this uses an undocumented API, this might break long term. Always test
the functionality before running a migration.

## Security
### Session Cookies
The endpoint used requires a Roblox account session cookie, not an Open Cloud
API key. It is strongly recommended to use a service/alt account for only this purpose
(NEVER your main account).

## Disable "Standard Protections"
You also need to [disable "Standard Protections" for the account](https://create.roblox.com/settings/advanced)
to allow the session cookie to be used on other hosts. Again, this should be on
a service/alt account.

### Limit Uptime
Zykmtrixo is intended to shut down servers. If exploited by the API key being
guessed, this can cause a denial-of-service attack on the place it controls.
Since the server is only meant for migrations, consider only running the service
when a server migration is active and shut it down when complete.

### Signatures vs API Keys
Requests to Zykmtrixo can use either an API key or an HMAC SHA256 signature of the
request body. API keys exist for being used directly by a Roblox server, but
for custom servers, HMAC SHA256 is strongly preferred.

## Configuration
The configuration is managed in a single file named `configuration.json`.
- When using the provided Docker Compose: it will be at `configuration/configuration.json`.
- Otherwise: it will be in the working directory of the application.

When started without a configuration, the default will be created:
```json
{
  "Logging": {
    "MinimumLogLevel": "Information",
    "AspNetLoggingEnabled": false
  },
  "Roblox": {
    "Places": [
      {
        "ApiKey": "default",
        "SecretKey": "default",
        "SessionCookie": "ROBLOSECURITY=default",
        "PlaceIds": [
          12345
        ]
      }
    ]
  },
  "Server": {
    "Host": "localhost",
    "Port": 8000
  }
}
```

Some configuration options can be changed without restarting the application.
Those that can't will be marked as "*(Requires Restart)*".

### Logging
Two options are provided:
- `MinimumLogLevel` - Minimum log level to display in the logs. Must be one
  of `None`, `Trace`, `Debug`, `Information`, `Warning`, or `Error`.
- `AspNetLoggingEnabled` *(Requires Restart)* - If `true`, additional logging
  for the ASP.NET server will be enabled.

### Places
The only option provided is a list of objects called `Places`. For most
deployments, there will be only 1 entry, but multitenancy is supported
(but keep in mind - these are raw session cookies; don't share the hosting
with other teams).

Each entry can contain the following:
- `ApiKey`: If provided, `Authorization` headers starting with `Bearer` or
  `ApiKey` with the exact string will be accepted.
- `SecretKey`: If provided, `Authorization` headers starting with `Signature`
  with an HMAC SHA256 of the request body using this secret key will be
  accepted.
- `SessionCookie`: Session cookie (including the `ROBLOSECURITY=`, which
  can be copied from the browser directly) to use.
- `PlaceIds`: List of *place* (not game/experience) ids that can be shut
  down.

### Server
Two options are provided:
- `Host` *(Requires Restart)* - Host to accept connections from. `*` allows
  for external connections.
- `Port` *(Requires Restart)* - Port to use for the server.

## Running
Zykmtrixo is intended to be ran using Docker. Using the built-in Docker
Compose setup, it can be started and updated with the following:

```bash
docker compose up -d --build
```

Stopping the server is done with the following:
```bash
docker compose down
```

### Expose Service
Changes to the Docker Compose can be used with a `docker-compose.override.yml`
file. You can directly expose the port (change the left port number
to change the exposed port):

```yml
services:
  zykmtrixo:
    ports:
      - 8000:8000
```

Alternatively, to use with another Docker container and not expose the port,
you can create a network with a command like `docker network create zykmtrixo_internal`
(the name is arbitrary, but must match) and use the following:

```yml
services:
  zykmtrixo:
    networks:
      - zykmtrixo_internal

networks:
  zykmtrixo_internal:
    external: true
```

## Requests
[Example HTTP requests can be found here](./Zykmtrixo/TestRequests.http)

For the `POST /shutdown` endpoint, they take the place id (`game.PlaceId`)
and job id (`game.JobId`).

### API Keys vs Signatures
The `Authorization` headers support both API keys (starting with `ApiKey` or
`Bearer`) and HMAC SHA256 signatures (starting with `Signature`). Outside
of Roblox servers, using HMAC SHA256 is recommended to make it hard to reuse
headers for different server Job Ids.

## License
Zykmtrixo is available under the terms of the GNU Lesser General Public
License. See [LICENSE](LICENSE) for details.