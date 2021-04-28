# Distributed tracing with Logz.io & Jaeger

## Design

[logz.io](https://logz.io/solutions/fully-managed-elk/) provides a fully managed Elastic Stack on the Cloud (SaaS).

[Jaeger](https://www.jaegertracing.io/) is an open source, end-to-end distributed tracing, and will be used to feed data to logz.io.

### Components

<img src="https://dytvr9ot2sszz.cloudfront.net/logz-docs/distributed-tracing/tracing_architecture.png" style="width:50%">

Component | Kubernetes resource | Kubernetes Service | Dependencies
--------- | ------------------- | ------------------ | ------------
Logz.io Platform (SaaS) | - | -
Jaeger Collector | Deployment | ClusterIP | Logz.io Platform
Jaeger Agent | DaemonSet | ClusterIP | Jaeger Collector
ASP.NET Data API | Deployment | LoadBalancer | Jaeger Agent
ASP.NET Business API | Deployment | LoadBalancer | ASP.NET Data API, Jaeger Agent

### OpenTelemetry terminology

Class name | Namespace | OpenTelemetry representation | Comment
---------- | --------- | ---------------------------- | -------
ActivitySource | System.Diagnostics | [Tracer](https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/api.md#tracer) | The tracer is responsible for creating Spans
Activity | System.Diagnostics | [Span](https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/trace/api.md#span) | A Span represents a single operation within a trace. Spans can be nested to form a trace tree. Each trace contains a root span, which typically describes the entire operation and, optionally, one or more sub-spans for its sub-operations

Know more by looking at the CNCF webinar: [Fundamentals of OpenTelemetry](https://www.cncf.io/webinars/fundamentals-of-opentelemetry/).

### Documentation

- [**OpenTelemetry**](https://opentelemetry.io/): [specification](https://github.com/open-telemetry/opentelemetry-specification), [.NET](https://github.com/open-telemetry/opentelemetry-dotnet) (_Dec 8th, 2020: 1.0.0-rc1.1 available_)
  - [Propagators API](https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/context/api-propagators.md)
- [**Jaeger**](https://www.jaegertracing.io): [code](https://github.com/jaegertracing/jaeger)
  - [Deployment](https://www.jaegertracing.io/docs/1.21/deployment/)
    - Operator for Kubernetes: [docs](https://www.jaegertracing.io/docs/1.21/operator/), [code](https://github.com/jaegertracing/jaeger-operator)
    - [Helm Charts](https://github.com/jaegertracing/helm-charts)
  - [Client Libraries](https://www.jaegertracing.io/docs/1.21/client-libraries/#supported-libraries)
    - [C# client (tracer) for Jaeger](https://github.com/jaegertracing/jaeger-client-csharp)
- [**Logz.io**](https://logz.io)
  - [Deploying components in your system](https://docs.logz.io/user-guide/distributed-tracing/deploying-components)
    - [logzio/jaeger-logzio](https://github.com/logzio/jaeger-logzio)
    - [Kubernetes deployment reference](https://docs.logz.io/user-guide/distributed-tracing/k8s-deployment)
    - [Jaeger Essentials: Best Practices for Deploying Jaeger on Kubernetes in Production](https://logz.io/blog/jaeger-kubernetes-best-practices/) - Aug 7th, 2020
  - [Setting up instrumentation and ingesting traces](https://docs.logz.io/user-guide/distributed-tracing/tracing-instrumentation.html)

## Getting started

### Setup a Logz.io account

- Create a free account on [logz.io](https://logz.io/freetrial/) (you may have issues with popup/privacy blocker such as DuckDuckGo)
- Make sure you are on the right Logz.io instance then retrieve your [region code](https://docs.logz.io/user-guide/accounts/account-region.html#available-regions) and token from [your account page](https://app-eu.logz.io/#/dashboard/settings/general)

### Run locally the backing services (Logz.io Jaeger collector & Jaeger agent) with Docker

- Start new containers

```bash
# Jaeger collector
docker run -e ACCOUNT_TOKEN=<ACCOUNT-TOKEN> -e REGION=<REGION> \
  --name=jaeger-logzio-collector \
  -p 14268:14268 \
  -p 9411:9411 \
  -p 14267:14267 \
  -p 14269:14269 \
  -p 14250:14250 \
  logzio/jaeger-logzio-collector:latest

# Grab collector IP
# on Linux: docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' <CONTAINER-ID>
# on Windows: look at C:\Windows\System32\drivers\etc\hosts for the host.docker.internal entry

# Jaeger agent
docker run --rm -p6831:6831/udp -p6832:6832/udp -p5778:5778/tcp -p5775:5775/udp jaegertracing/jaeger-agent:1.21 --reporter.grpc.host-port=<COLLECTOR-IP-OR-HOSTNAME>:14250
```

### Quick Jaeger .NET test

- Minimalist code to be added in a Controller to have a quick check (by adding the NuGet package reference to `Jaeger`), make sure environment variables "JAEGER_SERVICE_NAME", "JAEGER_AGENT_HOST" and "JAEGER_AGENT_PORT" are set (in `appsettings.json` file for example)

```csharp
  private readonly ILoggerFactory _loggerFactory;

  public WeatherForecastController(ILoggerFactory loggerFactory)
  {
      _loggerFactory = loggerFactory;
  }

  [HttpGet]
  public IEnumerable<WeatherForecast> Get()
  {
      Jaeger.Configuration.SenderConfiguration.DefaultSenderResolver = new Jaeger.Senders.SenderResolver(_loggerFactory)
          .RegisterSenderFactory<Jaeger.Senders.Thrift.ThriftSenderFactory>();

      Jaeger.Configuration config = Jaeger.Configuration.FromEnv(_loggerFactory);

      var samplerConfiguration = new Jaeger.Configuration.SamplerConfiguration(_loggerFactory)
          .WithType(Jaeger.Samplers.ConstSampler.Type)
          .WithParam(1);

      var reporterConfiguration = new Jaeger.Configuration.ReporterConfiguration(_loggerFactory)
          .WithLogSpans(true);

      OpenTracing.ITracer tracer = config
          .WithSampler(samplerConfiguration)
          .WithReporter(reporterConfiguration)
          .GetTracer();

      OpenTracing.ISpanBuilder builder = tracer.BuildSpan("myop");

      OpenTracing.ISpan span = builder.Start();

      span.Log("My message to the world");

      span.Finish();
  }
```

### Run locally the applications directly

```bash
# Data API
dotnet run -p src/DataApi

# Data API
dotnet run -p src/BusinessApi
```

### Run everythings locally with Docker Compose

- Create a file `local.env` with the content (this file containing secrets will be ignored by git)

```env
LOGZIO_ACCOUNT_TOKEN=xxxxxxx
LOGZIO_REGION_CODE=xx
```

- Use Docker compose to run all containers

```bash
# Jaeger network
docker network create net-logzio
docker network ls

# build
docker-compose build

# start
docker-compose --env-file ./local.env up

# stop
docker-compose down

# clean-up
docker rm logzio-jaeger-dotnet_businessapi_1 logzio-jaeger-dotnet_dataapi_1 logzio-jaeger-dotnet_jaegeragent_1 logzio-jaeger-dotnet_logziojaegercollector_1
```

### Create application images

- Create images (Docker)

```bash
# Data API
docker build . -t devprofr/jaegerdataapidemo -f src/DataApi/Dockerfile --no-cache

# Business API
docker build . -t devprofr/jaegerbusinessapidemo -f src/BusinessApi/Dockerfile --no-cache
```

- Push newly created images to a container image registry (Docker Hub)

```bash
# Data API
docker push devprofr/jaegerdataapidemo:latest

# Business API
docker push devprofr/jaegerbusinessapidemo:latest
```

### Run applications in Docker

```bash

# run a container as a daemon on the new images with only HTTP
docker run -d -p 8000:80 --name jaegerdataapidemo devprofr/jaegerdataapidemo:latest -e DistributedTracing__IsEnabled=true -e DistributedTracing__ServiceName=My_DEMO_POC -e DistributedTracing__Framework=OpenTelemetry -e DistributedTracing__Reporter=Jaeger -e DistributedTracing__Jaeger__AgentHost=host.docker.internal -e DistributedTracing__Jaeger__AgentPort=6831

# make sure the container is running fine (and open http://localhost:8000/WeatherForecast)
docker ps

# run a container interactively on the new image with HTTPS activated (tested on Windows with Linux containers)
docker run --rm -it -p 8000:80 -p 8001:443 -e DistributedTracing__IsEnabled=true -e DistributedTracing__ServiceName=My_DEMO_POC -e DistributedTracing__Framework=OpenTelemetry -e DistributedTracing__Reporter=Jaeger -e DistributedTracing__Jaeger__AgentHost=host.docker.internal -e DistributedTracing__Jaeger__AgentPort=6831 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_ENVIRONMENT=Development -e ASPNETCORE_HTTPS_PORT=8001 -e ASPNETCORE_Kestrel__Certificates__Default__Password="password" -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx -v %USERPROFILE%\.aspnet\https:/https/ --name jaegerdataapidemo devprofr/jaegerdataapidemo

# if there is an issue (direct crash), replace the ENTRYPOINT line by CMD "/bin/bash" and run
docker run -i -t -p 8080:80 devprofr/jaegerdataapidemo

# clean up
docker system prune -f
```

### Run everything in Kubernetes

- Apply Kubernetes configuration (see [Minikube cheatsheet](https://github.com/devpro/everyday-cheatsheets/blob/master/docs/minikube.md))

```bash
# create secrets (Logz.io)
kubectl create secret generic logzio \
  # Replace with the Tracing account token from Logz.io in Manage accounts > Distributed Tracing
  --from-literal=accounttoken=xxxxxxxxxx \
  # Replace with the 2-letter code for your region from the Logz.io Regions and Listener hosts table or from your Account settings page (for example: eu)
  --from-literal=region2lettercode=xxxxxxx

# create the resources
kubectl apply -f kubernetes/manifest.yml

# make sure everything is ok
kubectl get services,daemonset.apps,deployment.apps -A
kubectl get pods

# apply a change (for example if a new image has been pushed)
kubectl rollout restart deployment/demo-dotnet-dataapi
kubectl rollout restart deployment/demo-dotnet-businessapi

# Minikube: access the applications
minikube service demo-dotnet-dataapi
minikube service demo-dotnet-businessapi

# do clean-up
kubectl delete -f kubernetes/manifest.yml
```

### Run with Helm

- Apply Helm configuration (see [Helm](https://github.com/devpro/everyday-cheatsheets/blob/master/docs/helm.md))

```bash
# make sure secret has been generated (see previous step)

# install with Helm
helm install demo-logzio-jaeger helm/charts/logzio-jaeger
helm install demo-jaeger-dotnet-app helm/charts/dotnet-app

# list deployed releases
helm ls

# uninstall
helm uninstall demo-jaeger-dotnet-app
helm uninstall demo-logzio-jaeger
```

## Additional references

- CNCF webinars
  - [The what and why of distributed tracing](https://www.cncf.io/webinars/the-whats-and-whys-of-distributed-tracing/)

- ASP.NET Core: [docs](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-5.0), [code](https://github.com/dotnet/aspnetcore)
  - [Write custom ASP.NET Core middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write?view=aspnetcore-5.0)
  - [Health checks in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-5.0)
  - [Update Docker images for ASP.NET Core 5.0](https://docs.microsoft.com/en-us/aspnet/core/migration/31-to-50?view=aspnetcore-5.0&tabs=visual-studio#update-docker-images)

- [**OpenTracing**](https://opentracing.io/): [specification](https://github.com/opentracing/specification), [.NET](https://github.com/opentracing/opentracing-csharp)
  - [OpenTracing Tutorial - C#](https://github.com/yurishkuro/opentracing-tutorial/tree/master/csharp)

- Dev Mentors: [youtube](https://www.youtube.com/watch?v=toXFRBtv4fg) (source code: [Pacco](https://github.com/devmentors/Pacco), [Convey](https://github.com/snatch-dev/Convey), [Convey.Tracing.Jaeger](https://github.com/convey-stack/Convey.Tracing.Jaeger))
