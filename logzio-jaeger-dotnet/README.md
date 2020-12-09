# Distributed tracing with Logz.io & Jaeger

## Design

[logz.io](https://logz.io/solutions/fully-managed-elk/) provides a fully managed Elastic Stack on the Cloud (SaaS).

[Jaeger](https://www.jaegertracing.io/) is an open source, end-to-end distributed tracing, and will be used to feed data to logz.io.

### Components

<img src="https://dytvr9ot2sszz.cloudfront.net/logz-docs/distributed-tracing/tracing_architecture.png" style="width:50%">

- .NET Core Web Application (Jaeger client library): Kubernetes Pod
- Jaeger Agent: Kubernetes Deployment/Service
- Jaeger Collector: Kubernetes DaemonSet
- Logz.io Platform: SaaS

### Documentation

- [**OpenTelemetry**](https://opentelemetry.io/): [specification](https://github.com/open-telemetry/opentelemetry-specification), [.NET](https://github.com/open-telemetry/opentelemetry-dotnet) (_Dec 8th, 2020: 1.0.0-rc1.1 available_)
  - [Propagators API](https://github.com/open-telemetry/opentelemetry-specification/blob/master/specification/context/api-propagators.md)
- [**OpenTracing**](https://opentracing.io/): [specification](https://github.com/opentracing/specification), [.NET](https://github.com/opentracing/opentracing-csharp)
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
    - [OpenTracing Tutorial - C#](https://github.com/yurishkuro/opentracing-tutorial/tree/master/csharp)

## Getting started

### Setup a Logz.io account

- Create a free account on [logz.io](https://logz.io/freetrial/) (you may have issues with popup/privacy blocker such as DuckDuckGo)
- Make sure you are on the right Logz.io instance then retrieve your [region code](https://docs.logz.io/user-guide/accounts/account-region.html#available-regions) and token from [your account page](https://app-eu.logz.io/#/dashboard/settings/general)

### Use demo .NET web app

- Use the existing solution or create a new project
  - You need to add the NuGet package reference to `Jaeger`
  - Minimalist code to be added in a Controller to have a quick check

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

### Run locally with Docker

- Start new containers

```bash
# Jaeger network
docker network create net-logzio
docker network ls

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

- Run a quick test with the .NET web app
  - Add environment variables in `launchSettings.json` (`host.docker.internal` will work on Windows, to be replaced in Linux)

  ```json
    "DataApi": {
      "commandName": "Project",
      "dotnetRunMessages": "true",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "JAEGER_SERVICE_NAME": "MY_DEMO",
        "JAEGER_AGENT_HOST": "<AGENT-IP-OR-HOSTNAME>",
        "JAEGER_AGENT_PORT": "6831"
      }
    }
  ```

### Run locally with Docker Compose

- Create or review `local.env` file

```env
LOGZIO_ACCOUNT_TOKEN=xxxxxxx
LOGZIO_REGION_CODE=xx
```

- Use Docker compose to run all containers

```bash
# create
docker-compose --env-file ./local.env up

# delete
docker-compose down
```

### Run in Kubernetes

- Edit `kubernetes/manifest.yml` file and set the region code and token
- Apply Kubernetes configuration (see [Minikube cheatsheet](https://github.com/devpro/everyday-cheatsheets/blob/master/docs/minikube.md))

```bash
# create the resources
kubectl apply -f kubernetes/manifest.yml

# make sure everything is ok
kubectl get services,daemonset.apps,deployment.apps -A
kubectl get pods

# do clean-up
kubectl delete -f kubernetes/manifest.yml
```

- Run the .NET application directly

```bash
dotnet run -p src/DataApi
```

- Run the .NET application in Docker (for .NET 5.0 see [Update Docker images](https://docs.microsoft.com/en-us/aspnet/core/migration/31-to-50?view=aspnetcore-5.0&tabs=visual-studio#update-docker-images))

```bash
# build a new image
docker build . -t devprofr/jaegerdataapidemo -f src/DataApi/Dockerfile --no-cache

# run a container as a daemon on the new images with only HTTP
docker run -d -p 8000:80 --name jaegerdataapidemo devprofr/jaegerdataapidemo:latest

# make sure the container is running fine (and open http://localhost:8000/WeatherForecast)
docker ps

# run a container interactively on the new image with HTTPS activated (tested on Windows with Linux containers)
docker run --rm -it -p 8000:80 -p 8001:443 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_ENVIRONMENT=Development -e ASPNETCORE_HTTPS_PORT=8001 -e ASPNETCORE_Kestrel__Certificates__Default__Password="password" -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx -v %USERPROFILE%\.aspnet\https:/https/ --name jaegerdataapidemo devprofr/jaegerdataapidemo

# if there is an issue (direct crash), replace the ENTRYPOINT line by CMD "/bin/bash" and run
docker run -i -t -p 8080:80 devprofr/jaegerdataapidemo

# push the image on a container registry
docker push devprofr/jaegerdataapidemo:latest

# clean up
docker system prune -f
```

- Run the application in Kubernetes

```bash
# create a deployment in the Kubernetes cluster
kubectl create deployment jaegerdataapidemo --image=devprofr/jaegerdataapidemo:latest

# make sure everything is created ok
kubectl get deploy,pod

# expose the deployment and (optional) access it if you are using Minikube
kubectl expose deployment jaegerdataapidemo --type=LoadBalancer --port 8000 --target-port 80 --name jaegerdataapidemo
minikube service jaegerdataapidemo

# clean-up
kubectl delete service jaegerdataapidemo
kubectl delete deployment jaegerdataapidemo
```

## References

- ASP.NET Core: [docs](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-5.0), [code](https://github.com/dotnet/aspnetcore)
  - [Write custom ASP.NET Core middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write?view=aspnetcore-5.0)
- Dev Mentors: [youtube](https://www.youtube.com/watch?v=toXFRBtv4fg) (source code: [Pacco](https://github.com/devmentors/Pacco), [Convey](https://github.com/snatch-dev/Convey), [Convey.Tracing.Jaeger](https://github.com/convey-stack/Convey.Tracing.Jaeger))
