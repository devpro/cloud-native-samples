# Data collection with Splunk & OpenTelemetry from a .NET application

## Design

### Terminology

* [Splunk](https://github.com/devpro/everyday-cheatsheets/blob/main/docs/splunk.md) is the data platform where we want to send traces from our application. We'll be using:
  * the HTTP Event Collector (HEC): [examples](https://docs.splunk.com/Documentation/Splunk/8.1.3/Data/HECExamples), [splunkhecexporter](https://github.com/open-telemetry/opentelemetry-collector-contrib/blob/main/exporter/splunkhecexporter/README.md)
* [OpenTelemetry](https://opentelemetry.io/), aka "Otel", is an "observability framework for cloud-native software (a collection of tools, APIs, and SDKs)". It is:
  * the new standard for tracing and observability and one of the most active projects of the [Cloud Native Computing Foundation (CNCF)](https://github.com/devpro/everyday-cheatsheets/blob/main/docs/cncf.md) (as of May 2021)
  * the library that we will use to collect data from the application code: [opentelemetry-dotnet](https://github.com/open-telemetry/opentelemetry-dotnet)
  * the collector that will receive data from the application and send it to Splunk: [collector](https://opentelemetry.io/docs/collector/), [opentelemetry-collector-contrib](https://github.com/open-telemetry/opentelemetry-collector-contrib)
* [ASP.NET](https://dotnet.microsoft.com/apps/aspnet) is a "free, cross-platform, open source framework for building web apps and services with .NET and C#"
* [NuGet](https://www.nuget.org/) is the "package manager for .NET"
* [SignalFx](https://www.splunk.com/en_us/investor-relations/acquisitions/signalfx.html) has been acquired by Splunk in 2019

### Reason

* Decoupled architecture
  * the application must not have a strong dependency with Splunk
  * trace sending should be managed by a tracing library whose behavior is completely driven by configuration
  * request internal call stack should be easily correlated
* OpenTelemetry .NET is stable
  * [OpenTelemetry .NET reaches v1.0](https://devblogs.microsoft.com/dotnet/opentelemetry-net-reaches-v1-0/) - March 18, 2021
* Splunk is a major contributor to OpenTelemetry project and progessively deprecates previous libraries for OpenTelemetry
  * Blog articles
    * [Getting Started with OpenTelemetry .NET and OpenTelemetry Java v1.0.0](https://www.splunk.com/en_us/blog/devops/getting-started-with-opentelemetry-net-and-opentelemetry-java-v1-0-0.html) - March 16, 2021
    * [Announcing Native OpenTelemetry Support in Splunk APM](https://www.splunk.com/en_us/blog/conf-splunklive/announcing-native-opentelemetry-support-in-splunk-apm.html) - October 20, 2020
    * [OpenTelemetry, Open Collaboration](https://www.splunk.com/en_us/blog/devops/opentelemetry-open-collaboration.html) - July 13, 2020
    * [Data Insider > What Is OpenTelemetry?](https://www.splunk.com/en_us/data-insider/what-is-opentelemetry.html)
  * CNCF Webinars by [Steve Flanders](https://twitter.com/smflanders), Director of Engineering at Splunk
    * [OpenTelemetry Agent and Collector: Telemetry Built-in Into All Software](https://www.youtube.com/watch?v=cHiFSprUqa0) - September 4, 2020
    * [How OpenTelemetry is Eating the World](https://www.youtube.com/watch?v=DbaO0Xxv34c) - May 8, 2020

### Containers

Name | Image (Docker) | Repository
---- | ----- | ----------
Splunk | `splunk/splunk` | [github.com/Splunk/docker-Splunk](https://github.com/Splunk/docker-Splunk)
OpenTelemetry Contrib Collector | `otel/opentelemetry-collector-contrib` | [github.com/open-telemetry/opentelemetry-collector-contrib](https://github.com/open-telemetry/opentelemetry-collector-contrib)

### OpenTelemetry Collector components

* Receivers
  * [otlpreceiver](https://github.com/open-telemetry/opentelemetry-collector/tree/main/receiver/otlpreceiver)
* Exporters
  * [splunkhecexporter](https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/exporter/splunkhecexporter)

### Data flow

* Data feed

```txt
ASP.NET 5 web API with Otel .NET library
  -> Otel collector
    -> Splunk HEC
```

* Data read

```txt
Human
  -> Splunk Web
```

### HTTP streams

* Ports

Port | Reason
---- | ------
4317 | OpenTelemetry Collector gRPC
8000 | Splunk web application
8088 | Splunk HEC
8888 | Prometheus metrics

## Setup

### Run locally

_Important_: run this commands in a Linux shell (on Windows you can use WSL)

* (Optional) Create default Splunk docker configuration file ([documentation](https://splunk.github.io/docker-splunk/ADVANCED.html#usage))

```bash
docker run --rm -it splunk/splunk:latest create-defaults > docker/splunk.yml
```

* Make sure `docker/splunk.yml` file has the following lines (you can replace the token value), it is mandatory to enable HEC and retrieve the token

```yaml
splunk:
  hec:
    enable: true
    port: 8088
    ssl: false
    token: <default_hec_token>
```

* Start Splunk as a container ([documentation](https://splunk.github.io/docker-splunk/))

```bash
docker run -d -p 8000:8000 -p 8088:8088 \
  -e SPLUNK_START_ARGS='--accept-license' -e SPLUNK_PASSWORD='<password>' \
  -v "$(pwd)/docker/splunk.yml:/tmp/defaults/default.yml" \
  --name splunk splunk/splunk:latest
```

* Make sure HEC is opened (should return `{"text":"Success","code":0}`)

```bash
curl -k "http://localhost:8088/services/collector" \
  -H "Authorization: Splunk <default_hec_token>" \
  -d '{"event": "Hello, world!", "sourcetype": "manual"}'
```

* Open Splunk web UI at [localhost:8000](http://localhost:8000) and login with admin and the password used in the command line
  * Make a new search `sourcetype="manual"` and double check you can see our "Hello, world!" message

* Create OpenTelemetry Collector configuration: `docker/otel-collector/collector.yaml`

```yaml
# example: https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/exporter/splunkhecexporter/example
receivers:
  otlp:
    protocols:
      grpc:

#processors:
  # https://github.com/open-telemetry/opentelemetry-collector/blob/main/processor/memorylimiter/README.md
  #memory_limiter:

exporters:
  splunk_hec:
    token: "<default_hec_token>"
    endpoint: "http://host.docker.internal:8088/services/collector"
    # https://docs.splunk.com/Splexicon:Source
    source: "otel"
    # https://docs.splunk.com/Splexicon:Sourcetype
    sourcetype: "otel"
    #index: "traces"
    max_connections: 20
    disable_compression: false
    timeout: 10s
    insecure_skip_verify: true
    insecure: true

service:
  pipelines:
    traces:
      receivers: [otlp]
      #processors: [memory_limiter]
      exporters: [splunk_hec]
```

* Start OpenTelemetry Collector ([releases](https://github.com/open-telemetry/opentelemetry-collector-contrib/releases), [getting started](https://opentelemetry.io/docs/collector/getting-started/))

```bash
docker run -p 13133:13133 -p 14250:14250 -p 14268:14268 -p 4317:4317 -p 6060:6060 -p 8888:8888 -p 7276:7276 -p 9943:9943 \
  -v "$(pwd)/docker/otel-collector.yaml:/otel-collector-config.yaml" \
  --name otelcol otel/opentelemetry-collector-contrib:0.26.0 \
  --config otel-collector-config.yaml
```

## Code sample (.NET 5)

## Librairies (NuGet packages)

Name | Reason | Links
---- | ------ | -----
`OpenTelemetry.Exporter.OpenTelemetryProtocol` | The OTLP (OpenTelemetry Protocol) exporter communicates to an OpenTelemetry Collector through a gRPC protocol | [GitHub](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.OpenTelemetryProtocol/README.md), [NuGet](https://www.nuget.org/packages/OpenTelemetry.Exporter.OpenTelemetryProtocol/)
`OpenTelemetry.Extensions.Hosting` | | [NuGet](https://www.nuget.org/packages/OpenTelemetry.Extensions.Hosting)

### Build locally

```bash
# build the .NET solution
dotnet build

# start the web API (available at https://localhost:5001/swagger)
dotnet run -p src/WebApi
```

### Run with Docker

```bash
# build a new image
docker build . -t splunkoteldotnetsamplewebapi -f src/WebApi/Dockerfile --no-cache

# run the image
docker run -d -p 8001:443 -e ASPNETCORE_ENVIRONMENT=Development -e OpenTelemetryTracing__Enabled="true" -e OpenTelemetryTracing__OtlpExporter__Endpoint="http://host.docker.internal:4317" -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=8001 -e ASPNETCORE_Kestrel__Certificates__Default__Password="password" -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx -v %USERPROFILE%\.aspnet\https:/https/ --name splunkoteldotnetsamplewebapi splunkoteldotnetsamplewebapi:latest

# check service is up
curl --insecure https://localhost:8001/health
```

### Run with Docker Compose

Example: https://github.com/open-telemetry/opentelemetry-collector-contrib/blob/main/exporter/splunkhecexporter/example/docker-compose.yml

```bash
docker-compose up

docker-compose down
```

## Additional resources

* Technical articles
  * [TekStream - Containerization and Splunk: How Docker and Splunk Work Together](https://www.tekstream.com/containerization-and-splunk-how-docker-and-splunk-work-together/) - May 4, 2017
  * [Logz.io - Installing the OpenTelemetry Collector for Distributed Tracing](https://docs.logz.io/shipping/tracing-sources/opentelemetry.html) - April 26, 2021
* Other OpenTelemetry Collectors
  * [open-telemetry/opentelemetry-collector](https://github.com/open-telemetry/opentelemetry-collector)
  * [signalfx/splunk-otel-collector](https://github.com/signalfx/splunk-otel-collector)
* Articles in French
  * [Silicon > Splunk propulse Observability Cloud](https://www.silicon.fr/splunk-propulse-observability-cloud-407218.html) - May 11, 2021
  * [LeMagIT > APM : Splunk propulse son Observability Cloud](https://www.lemagit.fr/actualites/252500558/APM-Splunk-propulse-son-Observability-Cloud)- May 11, 2021

## Troubleshooting

* Issues with volume

```bash
docker run -it -v $(pwd)/test.txt:/somedata --name tbshoot ubuntu /bin/bash
```
