# ─────────────────────────────────────────────────────────────────────────────
# Stage 1 — build
# ─────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first so NuGet restore is cached independently
COPY LIOSCare.DoctorDashboard.sln ./
COPY src/LIOSCare.DoctorDashboard.Domain/LIOSCare.DoctorDashboard.Domain.csproj \
     src/LIOSCare.DoctorDashboard.Domain/
COPY src/LIOSCare.DoctorDashboard.Application/LIOSCare.DoctorDashboard.Application.csproj \
     src/LIOSCare.DoctorDashboard.Application/
COPY src/LIOSCare.DoctorDashboard.Infrastructure/LIOSCare.DoctorDashboard.Infrastructure.csproj \
     src/LIOSCare.DoctorDashboard.Infrastructure/
COPY src/LIOSCare.DoctorDashboard.Web/LIOSCare.DoctorDashboard.Web.csproj \
     src/LIOSCare.DoctorDashboard.Web/

RUN dotnet restore

# Copy all remaining source
COPY . .

# Publish the web entry-point project in Release mode
RUN dotnet publish src/LIOSCare.DoctorDashboard.Web/LIOSCare.DoctorDashboard.Web.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# ─────────────────────────────────────────────────────────────────────────────
# Stage 2 — runtime
# ─────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Pre-create the photo-upload directory so volume mounts land with correct path
RUN mkdir -p /app/wwwroot/uploads/doctors

# Copy published output from build stage
COPY --from=build /app/publish .

# ASP.NET Core 10 defaults to port 8080 inside containers
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "LIOSCare.DoctorDashboard.Web.dll"]
