# Jaeger (OpenTelemetry) .NET demo web application chart

## Quick guide

```bash
# lint the chart
helm lint .

# create Kubernetes template from chart
helm template . -f values.yaml  --set aspnetcore.environment=Development > temp.yaml

# install with Helm
helm install demo-jaeger-dotnet-app .

# list deployed releases
helm ls

# uninstall
helm install demo-jaeger-dotnet-app
```
