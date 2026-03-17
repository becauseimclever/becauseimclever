# 036 - Trusted Time Source & Client Offset Display

## Feature Description

Set up the host Raspberry Pi as a trusted time source using chrony, expose the server's authoritative time via an API endpoint, and display the offset between the client's local clock and the server's clock on the Clock page. This gives users visibility into how accurate their device's clock is relative to a reliable NTP-synced source.

## Goals

- Configure chrony on the host Pi for accurate NTP synchronization
- Expose a lightweight server time endpoint in the API
- Calculate and display the client-to-server time offset on the Clock page
- Account for network latency when computing the offset

## Technical Approach

### 1. Host Pi: Chrony Setup

Install and configure chrony on the Raspberry Pi to synchronize with upstream NTP servers and serve as a local stratum 1/2 time source.

```bash
sudo apt install chrony
```

#### /etc/chrony/chrony.conf (key additions)

```conf
# NTP Pool Project (global volunteer pool)
pool pool.ntp.org iburst maxsources 4
pool 0.pool.ntp.org iburst maxsources 2
pool 1.pool.ntp.org iburst maxsources 2

# Google Public NTP
server time.google.com iburst
server time1.google.com iburst
server time2.google.com iburst
server time3.google.com iburst
server time4.google.com iburst

# Microsoft Azure NTP (Stratum 1, GPS/atomic-backed)
server time.windows.com iburst

# Cloudflare NTP (anycast, low latency)
server time.cloudflare.com iburst

# NIST (National Institute of Standards and Technology)
server time.nist.gov iburst
server time-a-g.nist.gov iburst
server time-b-g.nist.gov iburst
server time-a-wwv.nist.gov iburst
server time-b-wwv.nist.gov iburst

# US Naval Observatory (USNO)
server tick.usno.navy.mil iburst
server tock.usno.navy.mil iburst

# Ubuntu / Canonical
pool ntp.ubuntu.com iburst maxsources 4

# Apple NTP
server time.apple.com iburst

# Meta / Facebook
server time.facebook.com iburst
server time1.facebook.com iburst

# Allow local network clients to query this server
allow 192.168.0.0/16

# Serve time even when not fully synced (stratum 10 fallback)
local stratum 10

# Log tracking data for diagnostics
log tracking measurements statistics
```

After configuration:

```bash
sudo systemctl enable chrony
sudo systemctl restart chrony
chronyc tracking   # Verify synchronization
chronyc sources    # Check upstream sources
```

### 2. Server: Time API Endpoint

Add a minimal API endpoint that returns the server's current UTC time. The response includes the time in a precise ISO 8601 format.

#### Endpoint

```
GET /api/v1/time
```

#### Response

```json
{
  "utc": "2026-03-16T14:30:00.1234567Z"
}
```

#### Implementation

| Layer | Component | Description |
|-------|-----------|-------------|
| Server | `TimeController.cs` | Returns `DateTime.UtcNow` as JSON |

The endpoint should be lightweight with no authentication, no database access, and minimal middleware to keep latency as low as possible.

### 3. Client: Offset Calculation

Use a round-trip approach to estimate the offset, accounting for network latency:

1. Record client time **before** the request (`t1`)
2. Fetch the server time endpoint
3. Record client time **after** the response (`t2`)
4. Estimate one-way latency as `(t2 - t1) / 2`
5. Calculate offset as `serverTime - (t1 + latency)`

This is the standard NTP-style offset estimation for a single round-trip.

#### Implementation

| Layer | Component | Description |
|-------|-----------|-------------|
| Client | `ITimeService.cs` | Interface for fetching server time |
| Client | `TimeService.cs` | `HttpClient`-based implementation |
| Client | `Clock.razor` | Display offset below the digital time readout |

### 4. Clock Page UI Changes

Add an offset display beneath the existing digital time readout on the Clock page:

- Show the offset in milliseconds (e.g., "+42ms", "-15ms")
- Use a subtle/muted style so it doesn't dominate the clock face
- Refresh the offset periodically (e.g., every 30 seconds) rather than every tick
- Show a brief loading indicator on first fetch

## Affected Components/Layers

| Layer | Component | Changes |
|-------|-----------|---------|
| Infrastructure | Host Pi | Install and configure chrony |
| Server | `Controllers/TimeController.cs` | New endpoint returning server UTC time |
| Application | `ServerTimeResponse.cs` | DTO for the time response |
| Client | `Services/ITimeService.cs` | Interface for time fetching |
| Client | `Services/TimeService.cs` | Implementation with offset calculation |
| Client | `Pages/Clock.razor` | Display offset from server time |
| Client | `Program.cs` | Register `ITimeService` / `TimeService` in DI |

## Implementation Steps

1. Install and configure chrony on the host Pi
2. Verify NTP sync with `chronyc tracking`
3. Create `ServerTimeResponse` DTO
4. Create `TimeController` with `GET /api/v1/time` endpoint
5. Write unit tests for the controller
6. Create `ITimeService` interface and `TimeService` implementation
7. Register the service in the client DI container
8. Update `Clock.razor` to fetch and display the offset
9. Write unit tests for the offset calculation logic
10. Write unit tests for Clock page offset display

## Design Decisions

- **Chrony over ntpd**: Chrony is the modern default on Debian/Ubuntu, handles intermittent connectivity better, and syncs faster after startup — ideal for a Pi that may be rebooted
- **Round-trip offset estimation**: Simple and effective without requiring WebSockets or a more complex protocol; good enough for millisecond-level accuracy over a LAN
- **Periodic refresh (30s)**: Avoids flooding the server with requests on every timer tick while keeping the offset reasonably current
- **UTC only**: Server returns UTC to avoid timezone ambiguity; the offset is timezone-independent
- **No authentication**: The time endpoint is read-only, non-sensitive, and should be as fast as possible

## Testing Considerations

- Unit test the `TimeController` returns a valid UTC timestamp
- Unit test the offset calculation logic with mocked request/response times
- Unit test the Clock page renders the offset display
- Manual verification that chrony is syncing on the Pi (`chronyc tracking`)
- Manual verification of displayed offset accuracy (compare against known NTP client)

## Future Enhancements (Out of Scope)

- Displaying chrony sync status or stratum info on the Clock page
- Historical offset tracking/graphing
- WebSocket-based continuous time sync
- PPS (pulse-per-second) GPS disciplined clock on the Pi
