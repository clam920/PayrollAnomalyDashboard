# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PayrollAnomalyDashboard.sln .
COPY Payroll.Domain/Payroll.Domain.csproj Payroll.Domain/
COPY Payroll.Application/Payroll.Application.csproj Payroll.Application/
COPY Payroll.Infrastructure/Payroll.Infrastructure.csproj Payroll.Infrastructure/
COPY Payroll.API/Payroll.API.csproj Payroll.API/
COPY Payroll.Domain.Tests/Payroll.Domain.Tests.csproj Payroll.Domain.Tests/
COPY Payroll.Application.Tests/Payroll.Application.Tests.csproj Payroll.Application.Tests/

RUN dotnet restore Payroll.API/Payroll.API.csproj

COPY . .
RUN dotnet publish Payroll.API/Payroll.API.csproj -c Release -o /app/publish --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Payroll.API.dll"]