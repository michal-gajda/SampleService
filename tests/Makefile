all: clean
	dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover --logger trx
	dotnet sonarscanner begin /k:"SampleService" /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" /d:sonar.coverage.exclusions="**Tests*.cs" /d:sonar.cs.vstest.reportsPaths="**/*.trx"
	dotnet build
	dotnet sonarscanner end
clean:
	dotnet clean
test: clean
	dotnet test