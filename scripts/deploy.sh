#!/bin/bash
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/.."

docker compose pull web
docker compose up -d --no-build web

for attempt in {1..30}; do
    if curl --fail --silent --output /dev/null --max-time 5 http://127.0.0.1:8580/; then
        echo "Deployment complete: application is responding."
        exit 0
    fi
    sleep 2
done

echo "Deployment started, but the application did not respond. Check docker compose logs web." >&2
exit 1
