FROM mcr.microsoft.com/dotnet/sdk:7.0.203-bullseye-slim AS build-env
RUN apt update && apt install -y clang zlib1g-dev && apt clean
WORKDIR /app
# RESTORE
COPY ./NuGet.Config ./
COPY ./Directory.Build.props ./
COPY ./NCoreUtils.Videos.Abstractions/*.csproj ./NCoreUtils.Videos.Abstractions/
COPY ./NCoreUtils.Videos/*.csproj ./NCoreUtils.Videos/
COPY ./NCoreUtils.Videos.Providers.FFMpeg/*.csproj ./NCoreUtils.Videos.Providers.FFMpeg/
COPY ./NCoreUtils.Videos.FFMpeg/*.csproj ./NCoreUtils.Videos.FFMpeg/
COPY ./NCoreUtils.Videos.WebService.Shared/*.csproj ./NCoreUtils.Videos.WebService.Shared/
COPY ./NCoreUtils.Videos.WebService.Core/*.csproj ./NCoreUtils.Videos.WebService.Core/
COPY ./NCoreUtils.Videos.WebService.Core.Generic/*.csproj ./NCoreUtils.Videos.WebService.Core.Generic/
COPY ./NCoreUtils.Videos.WebService/*.csproj ./NCoreUtils.Videos.WebService/
RUN sed -i 's/net7.0;net6.0;netstandard2.1/net7.0/' ./NCoreUtils.Videos.WebService.Shared/NCoreUtils.Videos.WebService.Shared.csproj && \
    sed -i 's/net7.0;net6.0/net7.0/' ./NCoreUtils.Videos.WebService.Core/NCoreUtils.Videos.WebService.Core.csproj && \
    sed -i 's/net7.0;net6.0/net7.0/' ./NCoreUtils.Videos.WebService.Core.Generic/NCoreUtils.Videos.WebService.Core.Generic.csproj && \
    sed -i 's/net7.0;net6.0;netstandard2.1/net7.0/' ./NCoreUtils.Videos/NCoreUtils.Videos.csproj && \
    sed -i 's/net7.0;net6.0;netstandard2.1/net7.0/' ./NCoreUtils.Videos.Abstractions/NCoreUtils.Videos.Abstractions.csproj
RUN dotnet restore ./NCoreUtils.Videos.WebService/NCoreUtils.Videos.WebService.csproj -r linux-x64 -v n -p EnableAzureBlobStorage=false -p EnableGoogleFluentdLogging=true
# PUBLISH
COPY ./NCoreUtils.Videos.Abstractions/*.cs ./NCoreUtils.Videos.Abstractions/
COPY ./NCoreUtils.Videos.Abstractions/Logging ./NCoreUtils.Videos.Abstractions/Logging/
COPY ./NCoreUtils.Videos.Abstractions/Internal ./NCoreUtils.Videos.Abstractions/Internal/
COPY ./NCoreUtils.Videos/*.cs ./NCoreUtils.Videos/
COPY ./NCoreUtils.Videos.Providers.FFMpeg/*.cs ./NCoreUtils.Videos.Providers.FFMpeg/
COPY ./NCoreUtils.Videos.FFMpeg/*.cs ./NCoreUtils.Videos.FFMpeg/
COPY ./NCoreUtils.Videos.WebService.Shared/*.cs ./NCoreUtils.Videos.WebService.Shared/
COPY ./NCoreUtils.Videos.WebService.Core/*.cs ./NCoreUtils.Videos.WebService.Core/
COPY ./NCoreUtils.Videos.WebService.Core/Internal/ ./NCoreUtils.Videos.WebService.Core/Internal/
COPY ./NCoreUtils.Videos.WebService.Core.Generic/Generic/ ./NCoreUtils.Videos.WebService.Core.Generic/Generic/
COPY ./NCoreUtils.Videos.WebService/*.cs ./NCoreUtils.Videos.WebService/
COPY ./NCoreUtils.Videos.WebService/*.trim.xml ./NCoreUtils.Videos.WebService/
RUN dotnet publish ./NCoreUtils.Videos.WebService/NCoreUtils.Videos.WebService.csproj -r linux-x64 -c Release --self-contained -p PublishAot=true -p EnableAzureBlobStorage=false -p EnableGoogleFluentdLogging=true -o /app/out

FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.5-bullseye-slim
WORKDIR /app
ENV DOTNET_ENVIRONMENT=Production \
    ASPNETCORE_ENVIRONMENT=Production \
    LISTEN=0.0.0.0:80
COPY --from=build-env /app/out ./
ENTRYPOINT ["./NCoreUtils.Videos.WebService"]
