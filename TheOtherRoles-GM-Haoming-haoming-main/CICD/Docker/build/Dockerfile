FROM --platform=amd64 mcr.microsoft.com/dotnet/sdk:6.0 AS build

RUN apt update \
    && apt install -y --no-install-recommends \
    python3 python3-pip\
    && apt clean \
    && rm -rf /var/lib/apt/lists/*
RUN pip3 install --no-cache --upgrade pip setuptools
RUN pip3 install openpyxl