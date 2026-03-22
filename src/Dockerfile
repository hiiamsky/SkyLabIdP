# 使用 ASP.NET Core 10.0 運行時作為基礎映像
FROM mcr.microsoft.com/dotnet/aspnet:10.0-jammy AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 443

# 安裝安全更新和基本工具
RUN apt-get update && apt-get upgrade -y && \
    apt-get install -y --no-install-recommends \
    curl \
    ca-certificates && \
    apt-get autoremove -y && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*


# 安裝 SkiaSharp 依賴項
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    libfontconfig1 \
    libfreetype6 \
    libgif7 \
    libgl1-mesa-glx \
    libjpeg-turbo8 \
    libpng16-16 && \
    apt-get autoremove -y && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# 創建非root用戶
RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup --home /app --shell /bin/false appuser


# 使用 .NET SDK 10.0 進行構建
FROM mcr.microsoft.com/dotnet/sdk:10.0-jammy AS build
WORKDIR /src

# 設置環境變數以避免 NuGet 問題
ENV NUGET_XMLDOC_MODE=skip
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true

# 安裝安全更新
RUN apt-get update && apt-get upgrade -y && \
    apt-get autoremove -y && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*


# 複製 csproj 檔案並還原依賴項 (優化快取層)    
COPY ["presentation/SkyLabIdP.WebApi/SkyLabIdP.WebApi.csproj", "presentation/SkyLabIdP.WebApi/"]
COPY ["core/SkyLabIdP.Application/SkyLabIdP.Application.csproj", "core/SkyLabIdP.Application/"]
COPY ["core/SkyLabIdP.Domain/SkyLabIdP.Domain.csproj", "core/SkyLabIdP.Domain/"]
COPY ["infrastructure/SkyLabIdP.Data/SkyLabIdP.Data.csproj", "infrastructure/SkyLabIdP.Data/"]
COPY ["infrastructure/SkyLabIdP.Identity/SkyLabIdP.Identity.csproj", "infrastructure/SkyLabIdP.Identity/"]
COPY ["infrastructure/SkyLabIdP.Shared/SkyLabIdP.Shared.csproj", "infrastructure/SkyLabIdP.Shared/"]

# 清理 NuGet 快取並還原依賴項
RUN dotnet nuget locals all --clear && \
    dotnet restore "presentation/SkyLabIdP.WebApi/SkyLabIdP.WebApi.csproj" --verbosity normal

# 複製其餘源碼
COPY . .
WORKDIR "/src/presentation/SkyLabIdP.WebApi"

# 建置應用程式 (建置所有依賴項目)
RUN dotnet restore "SkyLabIdP.WebApi.csproj" --verbosity normal && \
    dotnet build "SkyLabIdP.WebApi.csproj" -c Release -o /app/build --no-restore

# 發布應用程式
FROM build AS publish
RUN dotnet publish "SkyLabIdP.WebApi.csproj" -c Release -o /app/publish --no-restore

# 最終階段，基於非 root 用戶運行
FROM base AS final
WORKDIR /app

# 複製發布文件
COPY --from=publish /app/publish .

# 複製 audio 資料夾及其所有 wav 文件
COPY presentation/SkyLabIdP.WebApi/audio /app/audio

# 創建需要的目錄並設置適當的權限
RUN mkdir -p /app/Logs \
    /app/Uploads/ConclusionDocumentFiles \
    /app/Uploads/SkyLabDocUserDetailFiles \
    /app/Uploads/KMLFiles \
    /app/Uploads/ReviewDataDocumentFiles \
    /app/Uploads/FinalDataDocumentFiles \
    /app/Uploads/RevocationConclusionDocumentFiles && \
    chown -R appuser:appgroup /app && \
    chmod -R 755 /app

# 設定環境變數 (這些可以被 Docker Compose 覆蓋)
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV DOTNET_DATA_PROTECTION__KEYS_PATH=/app/.aspnet/DataProtection-Keys

# 創建 DataProtection 金鑰目錄
RUN mkdir -p /app/.aspnet/DataProtection-Keys && \
    chown -R appuser:appgroup /app/.aspnet

# 健康檢查 (配合新的端口)
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# 切換到非 root 用戶 
USER appuser

# 啟動應用程式
ENTRYPOINT ["dotnet", "SkyLabIdP.WebApi.dll"]