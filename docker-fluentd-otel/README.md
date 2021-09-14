# Log collection from Docker with Fluentd & OpenTelemetry

This is a basic example on how to send one container output to an observable system thanks to OpenTelemetry Collector.

## Usecase

* Can be interesting if you integrate a component "as-is", for example when you only have the container image with limited configuration options

## Design

### Components (containers)

* **Busy Box**
  * Display the date every 10 seconds
* **OpenTelemetry collector**
  * Receiver: [Fluent Forward Receiver](https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/receiver/fluentforwardreceiver)
  * Exporter: [Logging Exporter](https://github.com/open-telemetry/opentelemetry-collector/tree/main/exporter/loggingexporter)

### Container runtime

* **Docker Compose**
  * Logging: [Docker Fluentd logging driver](https://docs.docker.com/config/containers/logging/fluentd/)

## Minimal path to awesome

* Run Docker Compose from a terminal

```bash
docker compose up
```

* Wait for several iterations and see the output is correctly processed by the OpenTelemetry Collector

* Gently stop the containers from a terminal with `Ctrl+C`
