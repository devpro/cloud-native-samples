# Data collection with Splunk & OpenTelemetry from a .NET application

## Design

### Terminology

* [Splunk](https://github.com/devpro/everyday-cheatsheets/blob/main/docs/splunk.md) is the data platform where we want to send traces from our application. We'll be using:
  * the HTTP Event Collector (HEC): [examples](https://docs.splunk.com/Documentation/Splunk/8.1.3/Data/HECExamples)
* [OpenTelemetry](https://opentelemetry.io/), aka "Otel", is an "observability framework for cloud-native software (a collection of tools, APIs, and SDKs)". It is:
  * the new standard for tracing and observability and one of the most active projects of the [Cloud Native Computing Foundation (CNCF)](https://github.com/devpro/everyday-cheatsheets/blob/main/docs/cncf.md) (as of May 2021)
  * the library that we will use to collect data from the application code: [opentelemetry-dotnet](https://github.com/open-telemetry/opentelemetry-dotnet)
  * the collector that will receive data from the application and send it to Splunk: [opentelemetry-collector-contrib](https://github.com/open-telemetry/opentelemetry-collector-contrib)
* [ASP.NET](https://dotnet.microsoft.com/apps/aspnet) is a "free, cross-platform, open source framework for building web apps and services with .NET and C#".
* [NuGet](https://www.nuget.org/) is the "package manager for .NET".

### Reason

* Decoupled architecture:
  * the application must not have a strong dependency with Splunk
  * trace sending should be managed by a tracing library whose behavior is completely driven by configuration
* Splunk is a major contributor to OpenTelemetry project and progessively deprecates previous libraries for OpenTelemetry
  * [Announcing Native OpenTelemetry Support in Splunk APM](https://www.splunk.com/en_us/blog/conf-splunklive/announcing-native-opentelemetry-support-in-splunk-apm.html) - October 20, 2020
  * [OpenTelemetry, Open Collaboration](https://www.splunk.com/en_us/blog/devops/opentelemetry-open-collaboration.html) - July 13, 2020
  * [Data Insider > What Is OpenTelemetry?](https://www.splunk.com/en_us/data-insider/what-is-opentelemetry.html)

### Additional resources

* CNCF Webinars by [Steve Flanders](https://twitter.com/smflanders) (Director of Engineering at Splunk)
  * [OpenTelemetry Agent and Collector: Telemetry Built-in Into All Software](https://www.youtube.com/watch?v=cHiFSprUqa0) - September 4, 2020
  * [Webinar: How OpenTelemetry is Eating the World](https://www.youtube.com/watch?v=DbaO0Xxv34c) - May 8, 2020

### Data flow

```txt
ASP.NET 5 web API with Otel .NET library
  -> Otel collector
    -> Splunk HEC
```

## Setup

### Run locally

_Important_: run this commands in a Linux shell (on Windows you can use WSL)

* Start Splunk as a container ([Docker repository](https://github.com/Splunk/docker-Splunk), [documentation](https://splunk.github.io/docker-splunk/))

```bash
docker run -d -p 8000:8000 --name splunk -e SPLUNK_START_ARGS='--accept-license' -e SPLUNK_PASSWORD='<password>' splunk/splunk:latest
```

* Open Splunk web UI at [localhost:8000](http://localhost:8000) and login with admin and the password used in the command line

* Create OpenTelemetry Collector configuration: `collector.yaml`

```yaml
#TODO!
```

* Start OpenTelemetry Collector ([releases](https://github.com/open-telemetry/opentelemetry-collector-contrib/releases), [documentation]((https://docs.signalfx.com/en/latest/apm/apm-getting-started/apm-opentelemetry-collector.html)))

```bash
docker run -p 13133:13133 -p 14250:14250 -p 14268:14268 -p 4317:4317 \
  -p 6060:6060 -p 7276:7276 -p 8888:8888 -p 9411:9411 -p 9943:9943 \
  -e SPLUNK_CONFIG=/etc/collector.yaml \
  -v "${PWD}/collector.yaml":/etc/collector.yaml:ro \
  --name otelcontribcol otel/opentelemetry-collector-contrib:0.25.0
  --mem-ballast-size-mib=683
```

## Code sample

### Build locally

```bash
# build the .NET solution
dotnet build

# start the web API (available at https://localhost:5001/swagger)
dotnet run -p src/WebApi
```
