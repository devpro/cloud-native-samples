# Distributed tracing with Logz.io & Jaeger

## Design

[logz.io](https://logz.io/solutions/fully-managed-elk/) provides a fully managed Elastic Stack on the Cloud (SaaS).

[Jaeger](https://www.jaegertracing.io/) is an open source, end-to-end distributed tracing, and will be used to feed data to logz.io.

<img src="https://dytvr9ot2sszz.cloudfront.net/logz-docs/distributed-tracing/tracing_architecture.png" style="width:50%">

Components:

- .NET Core Web Application (Jaeger client library): Kubernetes Pod
- Jaeger Agent: Kubernetes Deployment/Service
- Jaeger Collector: Kubernetes DaemonSet
- Logz.io Platform: SaaS

References:

- OpenTracing: [specification](https://github.com/opentracing/specification), [C#](https://github.com/opentracing/opentracing-csharp)
- Jaeger: [code](https://github.com/jaegertracing/jaeger)
  - [Deployment](https://www.jaegertracing.io/docs/1.21/deployment/)
    - Operator for Kubernetes: [docs](https://www.jaegertracing.io/docs/1.21/operator/), [code](https://github.com/jaegertracing/jaeger-operator)
    - [Helm Charts](https://github.com/jaegertracing/helm-charts)
  - [Client Libraries](https://www.jaegertracing.io/docs/1.21/client-libraries/#supported-libraries)
    - [C# client (tracer) for Jaeger](https://github.com/jaegertracing/jaeger-client-csharp)
- [Deploying components in your system](https://docs.logz.io/user-guide/distributed-tracing/deploying-components)
  - [logzio/jaeger-logzio](https://github.com/logzio/jaeger-logzio)
  - [Kubernetes deployment reference](https://docs.logz.io/user-guide/distributed-tracing/k8s-deployment)
- [Setting up instrumentation and ingesting traces](https://docs.logz.io/user-guide/distributed-tracing/tracing-instrumentation.html)
  - [OpenTracing Tutorial - C#](https://github.com/yurishkuro/opentracing-tutorial/tree/master/csharp)

## Getting started

### Setup Logz.io account

- Create a free account on [logz.io](https://logz.io/freetrial/) (you may have issues with popup/privacy blocker such as DuckDuckGo)
- Make sure you are on the right Logz.io instance then retrieve your [region code](https://docs.logz.io/user-guide/accounts/account-region.html#available-regions) and token from [your account page](https://app-eu.logz.io/#/dashboard/settings/general)

### Demo .NET web app

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
docker run -e ACCOUNT_TOKEN=<SHIPPING-TOKEN> -e REGION=<REGION> \
  --network=net-logzio \
  --name=jaeger-logzio-collector \
  -p 14268:14268 \
  -p 9411:9411 \
  -p 14267:14267 \
  -p 14269:14269 \
  -p 14250:14250 \
  logzio/jaeger-logzio-collector:latest

# Grab collector IP
docker ps
# on Linux
docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' <CONTAINER-ID>
# on Windows look at C:\Windows\System32\drivers\etc\hosts for the host.docker.internal entry

# Jaeger agent
docker run --rm -p6831:6831/udp -p6832:6832/udp -p5778:5778/tcp -p5775:5775/udp jaegertracing/jaeger-agent:1.21 --reporter.grpc.host-port=<COLLECTOR-IP>:14250
```

- Run a quick test with the .NET web app
  - Add environment variables in `launchSettings.json` (host.docker.internal will work on Windows, to be replaced in Linux)

  ```json
    "WebApi": {
      "commandName": "Project",
      "dotnetRunMessages": "true",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "JAEGER_SERVICE_NAME": "MY_DEMO",
        "JAEGER_AGENT_HOST": "host.docker.internal",
        "JAEGER_AGENT_PORT": "6831"
      }
    }
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
dotnet run -p src/WebApi
```

- Run the .NET application in Docker (for .NET 5.0 see [Update Docker images](https://docs.microsoft.com/en-us/aspnet/core/migration/31-to-50?view=aspnetcore-5.0&tabs=visual-studio#update-docker-images))

```bash
# build a new image
docker build . -t devprofr/jaegerwebapidemo -f src/WebApi/Dockerfile --no-cache

# run a container as a daemon on the new images with only HTTP
docker run -d -p 8000:80 --name jaegerwebapidemo devprofr/jaegerwebapidemo:latest

# make sure the container is running fine (and open http://localhost:8000/WeatherForecast)
docker ps

# run a container interactively on the new image with HTTPS activated (tested on Windows with Linux containers)
docker run --rm -it -p 8000:80 -p 8001:443 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_ENVIRONMENT=Development -e ASPNETCORE_HTTPS_PORT=8001 -e ASPNETCORE_Kestrel__Certificates__Default__Password="password" -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx -v %USERPROFILE%\.aspnet\https:/https/ --name jaegerwebapidemo devprofr/jaegerwebapidemo

# if there is an issue (direct crash), replace the ENTRYPOINT line by CMD "/bin/bash" and run
docker run -i -t -p 8080:80 devprofr/jaegerwebapidemo

# push the image on a container registry
docker push devprofr/jaegerwebapidemo:latest

# clean up
docker system prune -f
```

- Run the application in Kubernetes

```bash
# create a deployment in the Kubernetes cluster
kubectl create deployment jaegerwebapidemo --image=devprofr/jaegerwebapidemo:latest

# make sure everything is created ok
kubectl get deploy,pod

# expose the deployment and (optional) access it if you are using Minikube
kubectl expose deployment jaegerwebapidemo --type=LoadBalancer --port 8000 --target-port 80 --name jaegerwebapidemo
minikube service jaegerwebapidemo

# clean-up
kubectl delete service jaegerwebapidemo
kubectl delete deployment jaegerwebapidemo
```

## References

- Dev Mentors: [youtube](https://www.youtube.com/watch?v=toXFRBtv4fg) (source code: [Pacco](https://github.com/devmentors/Pacco), [Convey.Tracing.Jaeger](https://github.com/convey-stack/Convey.Tracing.Jaeger))
