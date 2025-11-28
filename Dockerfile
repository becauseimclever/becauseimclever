# Build stage
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src

# Copy solution and project files
COPY BecauseImClever.sln .
COPY Directory.Build.props .
COPY stylecop.json .
COPY src/BecauseImClever.Domain/BecauseImClever.Domain.csproj src/BecauseImClever.Domain/
COPY src/BecauseImClever.Application/BecauseImClever.Application.csproj src/BecauseImClever.Application/
COPY src/BecauseImClever.Infrastructure/BecauseImClever.Infrastructure.csproj src/BecauseImClever.Infrastructure/
COPY src/BecauseImClever.Shared/BecauseImClever.Shared.csproj src/BecauseImClever.Shared/
COPY src/BecauseImClever.Client/BecauseImClever.Client.csproj src/BecauseImClever.Client/
COPY src/BecauseImClever.Server/BecauseImClever.Server.csproj src/BecauseImClever.Server/

# Restore dependencies
RUN dotnet restore src/BecauseImClever.Server/BecauseImClever.Server.csproj -a $TARGETARCH

# Copy source code
COPY src/ src/

# Build and publish
RUN dotnet publish src/BecauseImClever.Server/BecauseImClever.Server.csproj \
    -a $TARGETARCH \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Use the built-in non-root user ($APP_UID is set to 1654 by default in .NET images)
USER $APP_UID

# Expose port
EXPOSE 8580

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8580
ENV ASPNETCORE_ENVIRONMENT=Production

# Email settings - pass these at runtime via docker run -e or docker-compose
# ENV EmailSettings__SmtpHost=smtp.office365.com
# ENV EmailSettings__SmtpPort=587
# ENV EmailSettings__SmtpUsername=your-email@yourdomain.com
# ENV EmailSettings__SmtpPassword=your-password
# ENV EmailSettings__EnableSsl=true
# ENV EmailSettings__FromAddress=your-email@yourdomain.com
# ENV EmailSettings__FromName=BecauseImClever Contact Form
# ENV EmailSettings__ToAddress=your-email@yourdomain.com

# Start the application
ENTRYPOINT ["dotnet", "BecauseImClever.Server.dll"]
