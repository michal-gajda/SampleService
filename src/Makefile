all: clean
	dotnet sonarscanner begin /k:"SampleService"
	dotnet build
	dotnet sonarscanner end
clean:
	dotnet clean
config:
	docker compose config
docker:
	docker compose build
run: docker
	docker compose up -d
shutdown:
	docker compose down
watch:
	dotnet watch run --project .\CompanyName.SampleService.WebApi\CompanyName.SampleService.WebApi.csproj