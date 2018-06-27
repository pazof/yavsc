FROM microsoft/aspnet:1.0.0-rc1-update2

RUN printf "deb http://ftp2.fr.debian.org/debian/ stable main contrib non-free\n" >> /etc/apt/sources.list
# RUN apt-get -qq update && apt-get install -qqy sqlite3 libsqlite3-dev && rm -rf /var/lib/apt/lists/*
RUN apt-get -qq update && apt-get install -qqy postgresql-9.5 postgresql-client-9.5 && rm -rf /var/lib/apt/lists/*
COPY . /app
WORKDIR /app
RUN ["dnu", "restore"]

EXPOSE 5000/tcp
ENTRYPOINT ["dnx", "-p", "project.json", "web"]
