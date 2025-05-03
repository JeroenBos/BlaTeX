FROM node:22-bookworm-slim

SHELL ["/bin/sh", "-excu"]

WORKDIR /app
RUN <<EOF
    echo 'Dpkg::Use-Pty "0";' > /etc/apt/apt.conf.d/99no-pty
    apt-get update
    apt-get install --no-install-recommends -y \
      curl \
      libssl3 \
      openssl \
      ca-certificates
    apt-get -y upgrade 
    apt-get -y autoremove

    update-ca-certificates

    rm -rf /tmp/* /var/tmp/*

    echo '<!DOCTYPE html><html><body>No docker volume attached to "/app"</body></html>' > /app/index.html
    npm install -g http-server
EOF

ARG CONFIGURATION=Release
COPY src/bin/${CONFIGURATION}/net9.0/wwwroot/_framework/blazor.webassembly.js /app/_framework/blazor.webassembly.js
COPY src/bin/${CONFIGURATION}/net9.0/wwwroot/_framework/dotnet.js             /app/_framework/dotnet.js

EXPOSE 8080

CMD [ "http-server", "/app" ]
