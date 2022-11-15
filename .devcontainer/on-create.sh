cd "$1"
dotnet restore
# cd Beam.Server
dotnet tool restore
dotnet dev-certs https