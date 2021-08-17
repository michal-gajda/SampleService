all: clean
	dotnet sonarscanner begin /k:"SampleService" /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" /d:sonar.cs.vstest.reportsPaths="**/*.trx"
	dotnet restore
	dotnet build --no-restore --configuration Release
	dotnet test --no-build --configuration Release /p:CollectCoverage=true /p:CoverletOutputFormat=opencover --logger trx
	dotnet sonarscanner end
clean:
	dotnet clean
test: clean
	dotnet test