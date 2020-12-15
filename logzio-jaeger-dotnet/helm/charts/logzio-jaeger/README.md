# Helm charts

## Quick guide

```bash
# lint the chart
helm lint .

# create Kubernetes template from chart
helm template . -f values.yaml > temp.yaml

# install with Helm
helm install demo-logzio-jaeger .

# list deployed releases
helm ls

# uninstall
helm install demo-logzio-jaeger
```
