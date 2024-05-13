cd nugets/
for /F "delims=" %%i in ('dir /b') do (rmdir "%%i" /s/q || del "%%i" /s/q)
cd ..

dotnet build -c Release

dotnet pack src/MdEmail/ -c Release -o nugets/
dotnet pack src/MdEmail.Contracts/ -c Release -o nugets/
dotnet pack src/MdEmail.Templates/ -c Release -o nugets/
dotnet pack src/MdEmail.Templates.Contracts/ -c Release -o nugets/
dotnet pack src/MdEmail.Templates.Data.Postgres/ -c Release -o nugets/
dotnet pack src/MdEmail.Templates.Data.Sqlite/ -c Release -o nugets/
dotnet pack src/MdEmail.Templates.Rendering.Razor/ -c Release -o nugets/
