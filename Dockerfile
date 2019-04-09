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

#COPY src/SoftwarePioniere.Clients/*.csproj ./src/SoftwarePioniere.Clients/
#
#COPY src/SoftwarePioniere.DomainModel/*.csproj ./src/SoftwarePioniere.DomainModel/
#COPY src/SoftwarePioniere.DomainModel.Abstractions/*.csproj ./src/SoftwarePioniere.DomainModel.Abstractions/
#COPY src/SoftwarePioniere.DomainModel.Services/*.csproj ./src/SoftwarePioniere.DomainModel.Services/
#COPY src/SoftwarePioniere.DomainModel.TestHarness/*.csproj ./src/SoftwarePioniere.DomainModel.TestHarness/
#
#COPY src/SoftwarePioniere.Foundatio/*.csproj ./src/SoftwarePioniere.Foundatio/
#COPY src/SoftwarePioniere.Foundatio.Redis/*.csproj ./src/SoftwarePioniere.Foundatio.Redis/
#
#COPY src/SoftwarePioniere.Messaging/*.csproj ./src/SoftwarePioniere.Messaging/
#COPY src/SoftwarePioniere.Messaging.TestHarness/*.csproj ./src/SoftwarePioniere.Messaging.TestHarness/
#
#COPY src/SoftwarePioniere.Projections/*.csproj ./src/SoftwarePioniere.Projections/
#COPY src/SoftwarePioniere.Projections.Abstractions/*.csproj ./src/SoftwarePioniere.Projections.Abstractions/
#COPY src/SoftwarePioniere.Projections.Services/*.csproj ./src/SoftwarePioniere.Projections.Services/
#
#COPY src/SoftwarePioniere.ReadModel/*.csproj ./src/SoftwarePioniere.ReadModel/
#COPY src/SoftwarePioniere.ReadModel.Services/*.csproj ./src/SoftwarePioniere.ReadModel.Services/
#COPY src/SoftwarePioniere.ReadModel.TestHarness/*.csproj ./src/SoftwarePioniere.ReadModel.TestHarness/
#
#COPY src/SoftwarePioniere.TestHarness/*.csproj ./src/SoftwarePioniere.TestHarness/
#
#COPY test/SoftwarePioniere.DomainModel.Services.Tests/*.csproj ./test/SoftwarePioniere.DomainModel.Services.Tests/
#COPY test/SoftwarePioniere.DomainModel.Tests/*.csproj ./test/SoftwarePioniere.DomainModel.Tests/
#
#COPY test/SoftwarePioniere.Messaging.Tests/*.csproj ./test/SoftwarePioniere.Messaging.Tests/
#
#COPY test/SoftwarePioniere.ReadModel.Services.Tests/*.csproj ./test/SoftwarePioniere.ReadModel.Services.Tests/
#COPY test/SoftwarePioniere.ReadModel.Tests/*.csproj ./test/SoftwarePioniere.ReadModel.Tests/
#
RUN dotnet restore SoftwarePioniere.Fx.sln

FROM restore as src
COPY . .

FROM src AS buildsln
ARG CONFIGURATION=Release
ARG NUGETVERSIONV2=99.99.99
ARG ASSEMBLYSEMVER=99.99.99.99
WORKDIR /proj/src/
RUN dotnet build /proj/SoftwarePioniere.Fx.sln -c $CONFIGURATION --no-restore /p:NuGetVersionV2=$NUGETVERSIONV2 /p:AssemblySemVer=$ASSEMBLYSEMVER

FROM buildsln as testrunner
ARG PROJECT=SoftwarePioniere.Messaging.Tests
# ARG CONFIGURATION=Release
# ARG NUGETVERSIONV2=99.99.99
# ARG ASSEMBLYSEMVER=99.99.99.99
WORKDIR /proj/test/$PROJECT
# ENTRYPOINT ["dotnet", "test", "--logger:trx"]
# RUN dotnet test -c $CONFIGURATION --no-restore --no-build /p:NuGetVersionV2=$NUGETVERSIONV2 /p:AssemblySemVer=$ASSEMBLYSEMVER -r /testresults

FROM buildsln as pack
ARG CONFIGURATION=Release
ARG NUGETVERSIONV2=99.99.99
ARG ASSEMBLYSEMVER=99.99.99.99
RUN dotnet pack /proj/SoftwarePioniere.Fx.sln -c $CONFIGURATION --no-restore --no-build /p:NuGetVersionV2=$NUGETVERSIONV2 /p:AssemblySemVer=$ASSEMBLYSEMVER -o /proj/packages
WORKDIR /proj/packages/