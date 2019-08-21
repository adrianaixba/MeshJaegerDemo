# MeshJaegerDemo
Jaeger Docker Container:

run jaeger docker all-in-one container using 'docker run -d -p6831:6831/udp -p16686:16686 jaegertracing/all-in-one:latest'

access jaeger ui in http://localhost:16686

Services:

run serviceB (server) using 'dotnet run'

run serviceA (client) using 'dotnet run {value}'
