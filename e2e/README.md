```bash
# don't be in e2e/ dir
docker build . -f e2e/http-server.Dockerfile -t http-server
docker run -itd -p 8080:8080 \
  --volume "/$(pwd)/e2e:/app" \
  --volume "/$(pwd)/src/bin/Release/net9.0/wwwroot/_framework:/app/_framework" \
  --volume "/$(pwd)/js/node_modules:/app/node_modules" \
  http-server
cd src; dotnet watch build -c Release
```

Navigate to http://localhost:8080
