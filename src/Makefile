all: clean
	dotnet sonarscanner begin /k:"SampleService"
	dotnet build
	dotnet sonarscanner end
clean:
	dotnet clean
docker:
	docker compose build
run: docker
	docker compose up -d
shutdown:
	docker compose down
watch:
	dotnet watch run --project .\SampleService.WebApi\SampleService.WebApi.csproj