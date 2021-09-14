# Capture Docker container logs thanks to OpenTelemetry Collector

This is a working minimalist example on how to send a container output to an observable system thanks to OpenTelemetry Collector.

## Usecase

* You are integrating a component "as-is", in particular you only have the container image with limited configuration options

## Design

### Specifications

* [Fluentd Forward Protocol Specification](https://github.com/fluent/fluentd/wiki/Forward-Protocol-Specification-v1) is the only "language" available in Docker and OpenTelemetry

### Components (containers)

* **Busy Box**
  * Display the date every 10 seconds
* **OpenTelemetry collector**
  * Receivers: [Fluent Forward Receiver](https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/receiver/fluentforwardreceiver)
  * Exporter: [Logging Exporter](https://github.com/open-telemetry/opentelemetry-collector/tree/main/exporter/loggingexporter)

### Log processing logic

```txt
busybox container console output
  -(docker fluentd logging)-> fluent forward specified message on opentelemetry fluent port
    -(opentelemetry collector fluent forward receiver)-> opentelemetry specified message
      -(opentelemetry collector logging exporter)-> console output
```

### Container runtime

* **Docker Compose**
  * Logging: [Docker Fluentd logging driver](https://docs.docker.com/config/containers/logging/fluentd/)

## Minimal path to awesome

* Run Docker Compose from a terminal

```bash
docker compose up
```

* Wait for several iterations and see the output is correctly processed by the OpenTelemetry Collector and displayed in the console (with OpenTelemetry specification)

* Gently stop the containers from a terminal with `Ctrl+C`
