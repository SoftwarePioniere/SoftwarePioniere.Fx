FROM microsoft/dotnet:2-sdk AS restore
ARG CONFIGURATION=Release
WORKDIR /proj
COPY nuget.config.build.tmp ./nuget.config
COPY *.sln *.props ./

# Copy the main source project files
COPY src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done

# Copy the test project files
COPY test/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p test/${file%.*}/ && mv $file test/${file%.*}/; done

RUN dotnet restore SoftwarePioniere.Fx.sln

FROM restore as src
ARG CONFIGURATION=Release
ARG NUGETVERSIONV2=99.99.99
ARG ASSEMBLYSEMVER=99.99.99.99
COPY . .

FROM src AS buildsln
ARG CONFIGURATION=Release
ARG NUGETVERSIONV2=99.99.99
ARG ASSEMBLYSEMVER=99.99.99.99
WORKDIR /proj/src/
RUN dotnet build /proj/SoftwarePioniere.Fx.sln -c $CONFIGURATION --no-restore /p:NuGetVersionV2=$NUGETVERSIONV2 /p:AssemblySemVer=$ASSEMBLYSEMVER

FROM buildsln as testrunner
ARG CONFIGURATION=Release
ARG PROJECT=SoftwarePioniere.Messaging.Tests
WORKDIR /proj/test/$PROJECT
# ENTRYPOINT ["dotnet", "test", "--logger:trx"]
# RUN dotnet test -c $CONFIGURATION --no-restore --no-build /p:NuGetVersionV2=$NUGETVERSIONV2 /p:AssemblySemVer=$ASSEMBLYSEMVER -r /testresults

FROM buildsln as pack
ARG CONFIGURATION=Release
ARG NUGETVERSIONV2=99.99.99
ARG ASSEMBLYSEMVER=99.99.99.99
RUN dotnet pack /proj/SoftwarePioniere.Fx.sln -c $CONFIGURATION --no-restore --no-build /p:NuGetVersionV2=$NUGETVERSIONV2 /p:AssemblySemVer=$ASSEMBLYSEMVER -o /proj/packages
WORKDIR /proj/packages/