all: clean
	dotnet sonarscanner begin /k:"SampleService" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="admin" /d:sonar.password="P@ssw0rd" /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" /d:sonar.cs.vstest.reportsPaths="**/*.trx"
	dotnet restore
	dotnet build --no-restore --configuration Release
	dotnet test --no-build --configuration Release /p:CollectCoverage=true /p:CoverletOutputFormat=opencover --logger trx
	dotnet sonarscanner end /d:sonar.login="admin" /d:sonar.password="P@ssw0rd"
clean:
	dotnet clean
test: clean
	dotnet test