#!/usr/bin/env pwsh

$Output = Join-Path $PSScriptRoot Build

dotnet publish src/TSMapEditor/TSMapEditor.csproj --configuration=Release --runtime win-x64 --output=$Output --self-contained false