# Log collection with Splunk & OpenTelemetry from NGINX

This is an example on how to retrieve NGINX log files into Splunk thanks to OpenTelemetry Collector.

## Run locally

* 2 options

  * With Docker Compose

  ```bash
  docker compose up
  ```

  * With Docker

  ```bash
  # start NGINX
  docker run -d -p 8180:80 \
    -v /tmp/log/nginx:/var/log/nginx nginx

  # start Splunk
  docker run -d -p 8000:8000 -p 8088:8088 \
    -e SPLUNK_START_ARGS='--accept-license' -e SPLUNK_PASSWORD='opentelemetry' \
    -v "$(pwd)/config/splunk.yml:/tmp/defaults/default.yml" \
    --name splunk splunk/splunk:latest

  # start OpenTelemetry Collector
  docker run -d -p 13133:13133 -p 14250:14250 -p 14268:14268 -p 4317:4317 -p 6060:6060 -p 8888:8888 -p 7276:7276 -p 9943:9943 \
    -v "$(pwd)/config/otel-collector.yaml:/etc/otel-collector-config.yml" \
    -v "/tmp/log/nginx:/tmp/log/nginx" \
    --name otelcol otel/opentelemetry-collector-contrib:latest \
    --config=/etc/otel-collector-config.yml
  ```

* Open [NGINX](http://localhost:8180/)

* Open [Splunk](http://localhost:8000), login with admin/opentelemetry

  * Search with `source="otel"` and make sure you see entries
