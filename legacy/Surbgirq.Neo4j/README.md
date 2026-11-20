# Swagger endpoint
http://192.0.2.10:33010/swagger/index.html

# C# in emacs
https://github.com/OmniSharp/omnisharp-emacs
B5https://joshwolfe.ca/post/emacs-for-csharp/
http://codinggorilla.com/?p=2352
https://bbbscarter.wordpress.com/2013/08/17/c-autocompletion-in-emacs/


# install dotnet sdk
sudo apt update
sudo apt install -y dotnet-sdk-<version>


# nuget from command line
dotnet list package

dotnet add package <PackageName> --version <Version>
dotnet remove package <PackageName>

dotnet restore

# dotnet-outdated Tool 
dotnet tool install --global dotnet-outdated-tool
dotnet outdated
dotnet outdated --upgrade

You can also use flags like:
--include-auto-references
--pre-release
--prompt (asks before upgrading)




