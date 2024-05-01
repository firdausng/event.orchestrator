# Sample Events Processing using .NET and CloudEvent


## Run locally
```bash
docker-compose -f .\docker-compose.infra.yml up -d   
```

## Run in docker
```bash
docker-compose build
docker-compose up -d   
```

## Run in local k8s
start local registry
```bash
docker run -d -p 5000:5000 --restart=always --name registry registry:2
```

push the image to local registry
```bash
docker tag local/subscriber:latest localhost:5000/subscriber:dev
docker push localhost:5000/subscriber:dev
docker tag local/publisher:latest localhost:5000/publisher:dev
docker push localhost:5000/publisher:dev
```

Create package of the helm chart
```bash
cd k8s
helm package subscriber     
helm package publisher  
```

Install a package
```bash
cd k8s
helm install subscriber subscriber-0.1.0.tgz
helm install publisher publisher-0.1.0.tgz
```

Uninstall package
```bash
cd k8s
helm uninstall subscriber
helm uninstall publisher
```