FROM mcr.microsoft.com/dotnet/sdk:8.0.408-noble AS build-env
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
RUN sed -i 's/net8.0;net6.0;netstandard2.1/net8.0/' ./NCoreUtils.Videos.WebService.Shared/NCoreUtils.Videos.WebService.Shared.csproj && \
    sed -i 's/net8.0;net6.0/net8.0/' ./NCoreUtils.Videos.WebService.Core/NCoreUtils.Videos.WebService.Core.csproj && \
    sed -i 's/net8.0;net6.0/net8.0/' ./NCoreUtils.Videos.WebService.Core.Generic/NCoreUtils.Videos.WebService.Core.Generic.csproj && \
    sed -i 's/net8.0;net6.0;netstandard2.1/net8.0/' ./NCoreUtils.Videos/NCoreUtils.Videos.csproj && \
    sed -i 's/net8.0;net6.0;netstandard2.1/net8.0/' ./NCoreUtils.Videos.Abstractions/NCoreUtils.Videos.Abstractions.csproj
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
RUN rm /app/out/*.pdb /app/out/*.dbg
# RUN ls -lh /app/out && exit -1

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0.15-noble-chiseled
WORKDIR /app
ENV DOTNET_ENVIRONMENT=Production \
    ASPNETCORE_ENVIRONMENT=Production
COPY --from=build-env /app/out ./
ENTRYPOINT ["./NCoreUtils.Videos.WebService"]
