FROM mcr.microsoft.com/dotnet/core/sdk:2.2.105 AS build
WORKDIR /app

ARG NUGETVERSIONV2=99.99.99
ARG ASSEMBLYSEMVER=99.99.99.99

COPY ./*.sln ./nuget.config ./
COPY ./*/*.props ./

# COPY nuget.config.build ./nuget.config
# COPY *.sln *.props ./

# Copy the main source project files
COPY src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done

# Copy the test project files
COPY test/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p test/${file%.*}/ && mv $file test/${file%.*}/; done

# Copy the samples project files
COPY samples/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p samples/${file%.*}/ && mv $file samples/${file%.*}/; done

RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet build -c Release /p:NuGetVersionV2=$NUGETVERSIONV2 /p:AssemblySemVer=$ASSEMBLYSEMVER

# testrunner

FROM build as testrunner
ENTRYPOINT dotnet test --results-directory ./app/artifacts --logger:trx

# pack

FROM build AS pack
ENTRYPOINT dotnet pack -c Release /p:NuGetVersionV2=$NUGETVERSIONV2 /p:AssemblySemVer=$ASSEMBLYSEMVER -o /app/artifacts

# publish

FROM pack AS publish
ENTRYPOINT [ "dotnet", "nuget", "push", "/app/artifacts/*.nupkg" ]

# docker build --target build -t sopifx:build .
# docker build --target testrunner -t sopifx:testrunner .

# docker build --target pack -t sopifx:pack .
# docker run -it -v "$(pwd)/artifacts:/app/artifacts" sopifx:pack

# docker build --target publish -t sopifx:publish .

# FROM src AS buildsln
# ARG CONFIGURATION=Release
# ARG NUGETVERSIONV2=99.99.99
# ARG ASSEMBLYSEMVER=99.99.99.99
# WORKDIR /proj/src/
# RUN dotnet build /proj/SoftwarePioniere.Fx.sln -c $CONFIGURATION --no-restore /p:NuGetVersionV2=$NUGETVERSIONV2 /p:AssemblySemVer=$ASSEMBLYSEMVER

# FROM buildsln as testrunner
# ARG CONFIGURATION=Release
# ARG PROJECT=SoftwarePioniere.Messaging.Tests
# WORKDIR /proj/test/$PROJECT
# # ENTRYPOINT ["dotnet", "test", "--logger:trx"]
# # RUN dotnet test -c $CONFIGURATION --no-restore --no-build /p:NuGetVersionV2=$NUGETVERSIONV2 /p:AssemblySemVer=$ASSEMBLYSEMVER -r /testresults

# FROM buildsln as pack
# ARG CONFIGURATION=Release
# ARG NUGETVERSIONV2=99.99.99
# ARG ASSEMBLYSEMVER=99.99.99.99
# RUN dotnet pack /proj/SoftwarePioniere.Fx.sln -c $CONFIGURATION --no-restore --no-build /p:NuGetVersionV2=$NUGETVERSIONV2 /p:AssemblySemVer=$ASSEMBLYSEMVER -o /proj/packages
# WORKDIR /proj/packages/