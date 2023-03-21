#!/bin/sh

dotnet restore
dotnet watch run --project MainDemo.Blazor.Server
